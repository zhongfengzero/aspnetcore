// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

public class SchemaTransformerTests : OpenApiDocumentServiceTestBase
{
    [Fact]
    public async Task SchemaTransformer_RunsInRegisteredOrder()
    {
        var builder = CreateBuilder();

        builder.MapPost("/todo", (Todo todo) => { });

        var options = new OpenApiOptions();
        options.UseSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.Type == typeof(Todo))
            {
                schema.Description = "Represents a todo item";
            }
            return Task.CompletedTask;
        });
        options.UseSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.Type == typeof(Todo) && schema.Description != null)
            {
                schema.Example = new OpenApiString("{ \"id\": 1, \"title\": \"Todo item\", \"completed\": false, \"createdAt\": \"2021-01-01T00:00:00Z\" }");
            }
            return Task.CompletedTask;
        });

        await VerifyOpenApiDocument(builder, options, document =>
        {
            var todoSchema = document.Components.Schemas["Todo"];
            Assert.Equal("Represents a todo item", todoSchema.Description);
            var example = Assert.IsType<OpenApiString>(todoSchema.Example);
            Assert.Equal("{ \"id\": 1, \"title\": \"Todo item\", \"completed\": false, \"createdAt\": \"2021-01-01T00:00:00Z\" }", example.Value);
        });
    }

    [Fact]
    public async Task SchemaTransformer_CanUseApiParameterDescription()
    {
        var builder = CreateBuilder();

        builder.MapGet("/{name}", (string name) => { });

        var options = new OpenApiOptions();
        options.UseSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (context.ParameterDescription is { Name: "name" })
            {
                schema.Description = "Represents a name";
            }
            return Task.CompletedTask;
        });

        await VerifyOpenApiDocument(builder, options, document =>
        {
            var operation = document.Paths["/{name}"].Operations[OperationType.Get];
            var parameter = operation.Parameters[0];
            Assert.Equal("Represents a name", parameter.Schema.Description);
        });
    }
}
