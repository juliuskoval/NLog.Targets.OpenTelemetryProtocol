// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NLog.Targets.OpenTelemetryProtocol" + AssemblyInfo.PublicKey)]

file static class AssemblyInfo
{
    public const string PublicKey = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100f9c317cd45baf2f2907d054c5f19780569252fa65e761bdafbdea5ac19f0767a8fe11bc9ac3564cd02d2d988cee825057cb3349b73915336818c021a2174c7ea1404544fe665713b1d5ccd8b94e345a403c8b488a11afbc140d6f0d50c2cfbe19be9371fe61849d49c4b9730d3c348b3adeaf325ddcba25bd70cf5b5b10b90cb";
}