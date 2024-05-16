// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using JsonSchemaMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.OpenApi;

/// <summary>
/// Supports managing the JSON schemas associated with types
/// reference in a given OpenAPI document.
/// </summary>
internal sealed class OpenApiSchemaService([ServiceKey] string documentName, IOptions<JsonOptions> jsonOptions, IServiceProvider serviceProvider, IOptionsMonitor<OpenApiOptions> optionsMonitor)
{
    private readonly OpenApiOptions _options = optionsMonitor.Get(documentName);

    private readonly ConcurrentDictionary<OpenApiSchemaKey, OpenApiSchema> _schemas = new()
    {
        // Pre-populate OpenAPI schemas for well-defined types in ASP.NET Core.
        [new(typeof(IFormFile), nameof(IFormFile), null)] = new OpenApiSchema { Type = "string", Format = "binary" },
        [new(typeof(IFormFileCollection), nameof(IFormFileCollection), null)] = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema { Type = "string", Format = "binary" },
        },
        [new(typeof(Stream), nameof(Stream), null)] = new OpenApiSchema { Type = "string", Format = "binary" },
        [new(typeof(PipeReader), nameof(PipeReader), null)] = new OpenApiSchema { Type = "string", Format = "binary" },
    };

    private readonly JsonSerializerOptions _jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
    private readonly JsonSchemaMapperConfiguration _configuration = new()
    {
        OnSchemaGenerated = (context, schema) =>
        {
            var type = context.TypeInfo.Type;
            // Fix up schemas generated for IFormFile, IFormFileCollection, Stream, and PipeReader
            // that appear as properties within complex types.
            if (type == typeof(IFormFile) || type == typeof(Stream) || type == typeof(PipeReader))
            {
                schema.Clear();
                schema[OpenApiSchemaKeywords.TypeKeyword] = "string";
                schema[OpenApiSchemaKeywords.FormatKeyword] = "binary";
            }
            else if (type == typeof(IFormFileCollection))
            {
                schema.Clear();
                schema[OpenApiSchemaKeywords.TypeKeyword] = "array";
                schema[OpenApiSchemaKeywords.ItemsKeyword] = new JsonObject
                {
                    [OpenApiSchemaKeywords.TypeKeyword] = "string",
                    [OpenApiSchemaKeywords.FormatKeyword] = "binary"
                };
            }
            schema.ApplyPrimitiveTypesAndFormats(type);
            if (context.GetCustomAttributes(typeof(ValidationAttribute)) is { } validationAttributes)
            {
                schema.ApplyValidationAttributes(validationAttributes);
            }
            schema.ApplyPolymorphismOptions(context);
            if (context.TypeInfo.Kind == JsonTypeInfoKind.Object && context.TypeInfo.PolymorphismOptions == null)
            {
                schema[OpenApiConstants.SchemaId] = context.TypeInfo.Type.GetSchemaReferenceId();

            }
        }
    };

    internal async Task ApplySchemaTransformers()
    {
        var schemaTransformers = _options.SchemaTransformers;
        foreach (var schemaTransformer in schemaTransformers)
        {
            foreach (var schema in _schemas)
            {
                var context = new OpenApiSchemaTransformerContext
                {
                    DocumentName = OpenApiConstants.DefaultDocumentName,
                    Type = schema.Key.Type,
                    ParameterDescription = schema.Key.ParameterDescription,
                    ApplicationServices = serviceProvider
                };
                await schemaTransformer(schema.Value, context, CancellationToken.None);
            }
        }
    }

    internal async Task<OpenApiSchema> GetOrCreateSchema(Type type, ApiParameterDescription? parameterDescription = null)
    {
        var key = new OpenApiSchemaKey(type, type.GetSchemaReferenceId(), parameterDescription);
        return await _schemas.GetOrAddAsync(key, CreateSchema);
    }

    internal ConcurrentDictionary<OpenApiSchemaKey, OpenApiSchema> GetSchemas() => _schemas;

    private async ValueTask<OpenApiSchema> CreateSchema(OpenApiSchemaKey key)
    {
        JsonObject schemaAsJsonObject;
        if (key.ParameterDescription is { } parameterDescription &&
            parameterDescription.ParameterDescriptor is IParameterInfoParameterDescriptor { ParameterInfo: { } parameterInfo } &&
            parameterDescription.ModelMetadata.PropertyName is null)
        {
            schemaAsJsonObject = JsonSchemaMapper.JsonSchemaMapper.GetJsonSchema(_jsonSerializerOptions, parameterInfo, _configuration);
        }
        else
        {
            schemaAsJsonObject = JsonSchemaMapper.JsonSchemaMapper.GetJsonSchema(_jsonSerializerOptions, key.Type, _configuration);
        }
        if (key.ParameterDescription is { } && schemaAsJsonObject is not null)
        {
            schemaAsJsonObject.ApplyParameterInfo(key.ParameterDescription);
        }
        var deserializedSchema = JsonSerializer.Deserialize(schemaAsJsonObject, OpenApiJsonSchemaContext.Default.OpenApiJsonSchema);
        var resolvedSchema = deserializedSchema?.Schema ?? new OpenApiSchema();
        foreach (var property in resolvedSchema.Properties)
        {
            if (property.Value.Reference is { } reference && reference.Id == "#")
            {
                reference.Id = key.Type.GetSchemaReferenceId();
            }
        }
        await ApplySchemaTransformers(key, resolvedSchema);
        return resolvedSchema;
    }

    private async Task ApplySchemaTransformers(OpenApiSchemaKey key, OpenApiSchema schema)
    {
        var schemaTransformers = _options.SchemaTransformers;
        foreach (var schemaTransformer in schemaTransformers)
        {
            var context = new OpenApiSchemaTransformerContext
            {
                DocumentName = documentName,
                Type = key.Type,
                ParameterDescription = key.ParameterDescription,
                ApplicationServices = serviceProvider
            };
            await schemaTransformer(schema, context, CancellationToken.None);
        }
    }
}
