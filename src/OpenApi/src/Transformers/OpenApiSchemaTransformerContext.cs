// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.OpenApi;

/// <summary>
/// Represents the context in which an OpenAPI schema transformer is executed.
/// </summary>
public sealed class OpenApiSchemaTransformerContext
{
    /// <summary>
    /// Gets the name of the associated OpenAPI document.
    /// </summary>
    public required string DocumentName { get; init; }

    /// <summary>
    /// Gets the <see cref="Type"/> associated with the target schema.
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    /// Gets the <see cref="ApiParameterDescription" /> associated with the schema, if applicable.
    /// </summary>
    public ApiParameterDescription? ParameterDescription { get; init; }

    /// <summary>
    /// Gets the application services associated with the current document the target operation is in.
    /// </summary>
    public required IServiceProvider ApplicationServices { get; init; }
}
