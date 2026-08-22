using System;
using System.Collections.Generic;
using System.Reflection;
using NLog.Common;
using NLog.Config;
using NLog.Targets.OpenTelemetryProtocol.Exceptions;
using OpenTelemetry.Resources;

namespace NLog.Targets.OpenTelemetryProtocol
{
    /// <summary>
    /// Registers a resource detector provided by an external package, ex. OpenTelemetry.Resources.AWS.
    /// </summary>
    /// <remarks>
    /// The detectors in the official OpenTelemetry.Resources.* packages are internal types, so they can only
    /// be registered through the public extension method that each package provides (ex. <c>AddAWSEC2Detector</c>).
    /// The extension method is resolved with reflection, which keeps the package that provides it an optional
    /// dependency of the application instead of a dependency of this library.
    /// </remarks>
    [NLogConfigurationItem]
    public class ResourceDetector
    {
        public ResourceDetector() { }

        public ResourceDetector(string assemblyName, string methodName)
        {
            Assembly = assemblyName;
            Method = methodName;
        }

        /// <summary>
        /// Simple name of the assembly that provides the detector, ex. <c>OpenTelemetry.Resources.AWS</c>.
        /// When empty, the already loaded assemblies are searched instead.
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// Full name of the type that provides the detector (optional).
        /// Either the static class declaring <see cref="Method"/>, ex. <c>OpenTelemetry.Resources.AWSResourceBuilderExtensions</c>,
        /// or - when <see cref="Method"/> is empty - a custom <see cref="IResourceDetector"/> implementation.
        /// </summary>
        /// <remarks>
        /// Named TypeName rather than Type, because NLog reserves the <c>type</c> attribute in XML configuration.
        /// </remarks>
        public string TypeName { get; set; }

        /// <summary>
        /// Name of the extension method that registers the detector, ex. <c>AddAWSEC2Detector</c>.
        /// Leave empty when <see cref="TypeName"/> is an <see cref="IResourceDetector"/> implementation.
        /// </summary>
        public string Method { get; set; }

        /// <exception cref="FailedToResolveResourceDetectorException">
        /// The detector could not be resolved or registered, ex. because the package providing it is not deployed
        /// in this environment. The caller is expected to skip the detector.
        /// </exception>
        internal ResourceBuilder ApplyTo(ResourceBuilder resourceBuilder)
        {
            if (string.IsNullOrEmpty(Method) && string.IsNullOrEmpty(TypeName))
                throw new FailedToResolveResourceDetectorException($"{this} - Method and/or TypeName must be specified.");

            var assembly = LoadAssembly();

            if (string.IsNullOrEmpty(Method))
                return AddDetectorInstance(resourceBuilder, assembly);

            return InvokeExtensionMethod(resourceBuilder, ResolveExtensionMethod(assembly));
        }

        private System.Reflection.Assembly LoadAssembly()
        {
            if (string.IsNullOrEmpty(Assembly))
                return null;

            try
            {
                return System.Reflection.Assembly.Load(new AssemblyName(Assembly));
            }
            catch (Exception ex)
            {
                throw new FailedToResolveResourceDetectorException($"{this} - Failed to load assembly '{Assembly}'. Make sure the application references the package that provides it.", ex);
            }
        }

        private Type ResolveType(System.Reflection.Assembly assembly)
        {
            var type = assembly is null ? Type.GetType(TypeName, throwOnError: false) : assembly.GetType(TypeName, throwOnError: false);
            if (type is null)
            {
                var location = assembly is null ? ". Specify Assembly, or use an assembly-qualified type name" : $" in assembly '{assembly.GetName().Name}'";
                throw new FailedToResolveResourceDetectorException($"{this} - Failed to resolve type '{TypeName}'{location}.");
            }

            return type;
        }

        private ResourceBuilder AddDetectorInstance(ResourceBuilder resourceBuilder, System.Reflection.Assembly assembly)
        {
            var detectorType = ResolveType(assembly);

            if (!typeof(IResourceDetector).IsAssignableFrom(detectorType))
                throw new FailedToResolveResourceDetectorException($"{this} - Type '{detectorType.FullName}' does not implement IResourceDetector. Specify Method when the detector is registered through an extension method, ex. AddAWSEC2Detector.");

            IResourceDetector detector;

            try
            {
                detector = (IResourceDetector)Activator.CreateInstance(detectorType);
            }
            catch (Exception ex)
            {
                throw new FailedToResolveResourceDetectorException($"{this} - Failed to create an instance of '{detectorType.FullName}'. It must have a public parameterless constructor.", ex);
            }

            InternalLogger.Debug("{0} - Adding resource detector '{1}'", this, detectorType.FullName);
            return resourceBuilder.AddDetector(detector);
        }

        private MethodInfo ResolveExtensionMethod(System.Reflection.Assembly assembly)
        {
            if (!string.IsNullOrEmpty(TypeName))
            {
                var declaringType = ResolveType(assembly);
                var declaredMethod = SelectMethod(declaringType);
                if (declaredMethod is null)
                    throw new FailedToResolveResourceDetectorException($"{this} - Type '{declaringType.FullName}' has no public static method '{Method}' accepting a ResourceBuilder.");

                return declaredMethod;
            }

            if (assembly != null)
            {
                var assemblyMethod = SearchAssembly(assembly);
                if (assemblyMethod is null)
                    throw new FailedToResolveResourceDetectorException($"{this} - Assembly '{assembly.GetName().Name}' has no public static method '{Method}' accepting a ResourceBuilder.");

                return assemblyMethod;
            }

            foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var method = SearchAssembly(loadedAssembly);
                if (method != null)
                    return method;
            }

            throw new FailedToResolveResourceDetectorException($"{this} - Found no public static method '{Method}' accepting a ResourceBuilder among the loaded assemblies. Specify Assembly, ex. OpenTelemetry.Resources.AWS.");
        }

        private MethodInfo SearchAssembly(System.Reflection.Assembly assembly)
        {
            foreach (var type in GetExportedTypes(assembly))
            {
                // Extension methods can only be declared by a static class
                if (!type.IsAbstract || !type.IsSealed)
                    continue;

                var method = SelectMethod(type);
                if (method != null)
                    return method;
            }

            return null;
        }

        private static IEnumerable<Type> GetExportedTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (Exception ex)
            {
                // A dependency of the assembly could not be loaded, so it cannot be the one providing the detector
                InternalLogger.Debug(ex, "Failed to inspect the types of assembly '{0}' when searching for a resource detector", assembly.FullName);
                return Array.Empty<Type>();
            }
        }

        /// <summary>
        /// Picks the overload with the fewest parameters, so that a package offering both
        /// <c>Add..Detector(builder)</c> and <c>Add..Detector(builder, configure)</c> resolves to the simple one.
        /// </summary>
        private MethodInfo SelectMethod(Type declaringType)
        {
            MethodInfo bestMatch = null;

            foreach (var method in declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!string.Equals(method.Name, Method, StringComparison.Ordinal) || method.IsGenericMethodDefinition)
                    continue;

                if (!IsResourceBuilderExtension(method))
                    continue;

                if (bestMatch is null || method.GetParameters().Length < bestMatch.GetParameters().Length)
                    bestMatch = method;
            }

            return bestMatch;
        }

        private static bool IsResourceBuilderExtension(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0 || !parameters[0].ParameterType.IsAssignableFrom(typeof(ResourceBuilder)))
                return false;

            // Anything beyond the ResourceBuilder must be optional, since there is no way to supply it from here
            for (int i = 1; i < parameters.Length; ++i)
            {
                if (!parameters[i].IsOptional)
                    return false;
            }

            return true;
        }

        private ResourceBuilder InvokeExtensionMethod(ResourceBuilder resourceBuilder, MethodInfo method)
        {
            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            arguments[0] = resourceBuilder;
            for (int i = 1; i < parameters.Length; ++i)
                arguments[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;

            try
            {
                InternalLogger.Debug("{0} - Adding resource detector using {1}.{2}", this, method.DeclaringType?.FullName, method.Name);
                return method.Invoke(null, arguments) as ResourceBuilder ?? resourceBuilder;
            }
            catch (TargetInvocationException ex)
            {
                throw new FailedToResolveResourceDetectorException($"{this} - {method.DeclaringType?.FullName}.{method.Name} threw an exception.", ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                throw new FailedToResolveResourceDetectorException($"{this} - Failed to invoke {method.DeclaringType?.FullName}.{method.Name}.", ex);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Method))
                return $"ResourceDetector(TypeName={TypeName})";

            return string.IsNullOrEmpty(Assembly)
                ? $"ResourceDetector(Method={Method})"
                : $"ResourceDetector(Method={Method}, Assembly={Assembly})";
        }
    }
}
