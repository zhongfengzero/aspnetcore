// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// HTTP metrics extension methods for <see cref="IEndpointConventionBuilder"/>.
/// </summary>
public static class HttpMetricsEndpointConventionBuilderExtensions
{
    public static IEndpointConventionBuilder DisableHttpMetrics(this IEndpointConventionBuilder builder)
    {
        builder.Add(b => b.Metadata.Add(new DisableHttpMetricsAttribute()));
        return builder;
    }
}
