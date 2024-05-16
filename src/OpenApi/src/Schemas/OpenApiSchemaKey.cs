// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.OpenApi;

internal readonly struct OpenApiSchemaKey(Type type, string? referenceId, ApiParameterDescription? parameterDescription) : IEquatable<OpenApiSchemaKey>
{
    public Type Type { get; } = type;
    public string? ReferenceId { get; } = referenceId;
    public ApiParameterDescription? ParameterDescription { get; } = parameterDescription;

    public bool Equals(OpenApiSchemaKey other)
    {
        return Type == other.Type &&
            ReferenceId == other.ReferenceId &&
            ParameterDescription == other.ParameterDescription;
    }

    public bool ShouldUseRef()
    {
        return !Type.IsPrimitive;
    }
}
