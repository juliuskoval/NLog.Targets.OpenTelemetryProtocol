// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using OpenTelemetry.Logs.Custom;

namespace OpenTelemetry.Custom
{
    internal static class Sdk
    {
        public static LoggerProviderBuilderBase CreateLoggerProviderBuilder()
        {
            return new LoggerProviderBuilderBase();
        }
    }
}
