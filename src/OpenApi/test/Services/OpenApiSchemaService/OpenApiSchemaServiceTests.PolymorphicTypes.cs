// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

public partial class OpenApiSchemaServiceTests : OpenApiDocumentServiceTestBase
{
    [Fact]
    public async Task GetOpenApiRequestBody_HandlesPolymorphicRequestWitDiscriminator()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.MapPost("/api", (Shape shape) => { });

        // Assert
        await VerifyOpenApiDocument(builder, document =>
        {
            var operation = document.Paths["/api"].Operations[OperationType.Post];
            Assert.NotNull(operation.RequestBody);
            var requestBody = operation.RequestBody.Content;
            Assert.True(requestBody.TryGetValue("application/json", out var mediaType));
            var schema = mediaType.Schema.GetReferencedSchema(document);
            Assert.NotNull(schema);
            Assert.Equal("object", schema.Type);
            // Assert that OpenAPI-specific discriminator mappings have
            // been set
            Assert.Equal("$type", schema.Discriminator.PropertyName);
            Assert.Collection(schema.Discriminator.Mapping, (mapping) =>
            {
                Assert.Equal("triangle", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedTriangle", mapping.Value);
            },
            (mapping) =>
            {
                Assert.Equal("square", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedSquare", mapping.Value);
            });
            Assert.Collection(schema.OneOf,
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$type", property.Key);
                            var discriminator = Assert.IsType<OpenApiString>(Assert.Single(property.Value.Enum));
                            Assert.Equal("triangle", discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("hypotenuse", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("color", property.Key);
                            Assert.Equal("string", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("sides", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                },
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$type", property.Key);
                            var discriminator = Assert.IsType<OpenApiString>(Assert.Single(property.Value.Enum));
                            Assert.Equal("square", discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("area", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("color", property.Key);
                            Assert.Equal("string", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("sides", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                });
        });
    }

    [Fact]
    public async Task GetOpenApiResponse_HandlesPolymorphicResponseWithDiscriminator()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.MapGet("/api", () => TypedResults.Ok<Shape>(new Triangle() { Color = "red", Sides = 3, Hypotenuse = 5 }));

        // Assert
        await VerifyOpenApiDocument(builder, document =>
        {
            var operation = document.Paths["/api"].Operations[OperationType.Get];
            Assert.NotNull(operation.Responses["200"]);
            var response = operation.Responses["200"].Content;
            Assert.True(response.TryGetValue("application/json", out var mediaType));
            var schema = mediaType.Schema.GetReferencedSchema(document);
            Assert.NotNull(schema);
            Assert.Equal("object", schema.Type);
            // Assert that OpenAPI-specific discriminator mappings have
            // been set
            Assert.Equal("$type", schema.Discriminator.PropertyName);
            Assert.Collection(schema.Discriminator.Mapping, (mapping) =>
            {
                Assert.Equal("triangle", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedTriangle", mapping.Value);
            },
            (mapping) =>
            {
                Assert.Equal("square", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedSquare", mapping.Value);
            });
            Assert.Collection(schema.OneOf,
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$type", property.Key);
                            var discriminator = Assert.IsType<OpenApiString>(Assert.Single(property.Value.Enum));
                            Assert.Equal("triangle", discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("hypotenuse", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("color", property.Key);
                            Assert.Equal("string", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("sides", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                },
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$type", property.Key);
                            var discriminator = Assert.IsType<OpenApiString>(Assert.Single(property.Value.Enum));
                            Assert.Equal("square", discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("area", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("color", property.Key);
                            Assert.Equal("string", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("sides", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                });
        });
    }

    [Fact]
    public async Task GetOpenApiResponse_HandlesPolymorphicResponseWithMixedDiscriminators()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.MapGet("/api", () => TypedResults.Ok<BasePoint>(new ThreeDimensionalPoint { X = 1, Y = 2, Z = 3 }));

        // Assert
        await VerifyOpenApiDocument(builder, document =>
        {
            var operation = document.Paths["/api"].Operations[OperationType.Get];
            Assert.NotNull(operation.Responses["200"]);
            var response = operation.Responses["200"].Content;
            Assert.True(response.TryGetValue("application/json", out var mediaType));
            var schema = mediaType.Schema.GetReferencedSchema(document);
            Assert.NotNull(schema);
            Assert.Equal("object", schema.Type);
            // Assert that OpenAPI-specific discriminator mappings have
            // been set
            Assert.Equal("$type", schema.Discriminator.PropertyName);
            Assert.Collection(schema.Discriminator.Mapping, (mapping) =>
            {
                Assert.Equal("3", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedThreeDimensionalPoint", mapping.Value);
            },
            (mapping) =>
            {
                Assert.Equal("4d", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedFourDimensionalPoint", mapping.Value);
            });
            Assert.Collection(schema.OneOf,
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$type", property.Key);
                            var discriminator = Assert.IsType<OpenApiInteger>(Assert.Single(property.Value.Enum));
                            Assert.Equal(3, discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("z", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("x", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("y", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                },
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$type", property.Key);
                            var discriminator = Assert.IsType<OpenApiString>(Assert.Single(property.Value.Enum));
                            Assert.Equal("4d", discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("w", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("z", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("x", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("y", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                });
        });
    }

    [JsonDerivedType(typeof(ThreeDimensionalPoint), typeDiscriminator: 3)]
    [JsonDerivedType(typeof(FourDimensionalPoint), typeDiscriminator: "4d")]
    public abstract class BasePoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ThreeDimensionalPoint : BasePoint
    {
        public int Z { get; set; }
    }

    public sealed class FourDimensionalPoint : ThreeDimensionalPoint
    {
        public int W { get; set; }
    }

    [Fact]
    public async Task GetOpenApiResponse_HandlesPolymorphicResponseWithCustomDiscrimantorKeyword()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.MapGet("/api", () => TypedResults.Ok<BasePointWithCustomName>(new ThreeDimensionalPointWithCustomName { X = 1, Y = 2, Z = 3 }));

        // Assert
        await VerifyOpenApiDocument(builder, document =>
        {
            var operation = document.Paths["/api"].Operations[OperationType.Get];
            Assert.NotNull(operation.Responses["200"]);
            var response = operation.Responses["200"].Content;
            Assert.True(response.TryGetValue("application/json", out var mediaType));
            var schema = mediaType.Schema.GetReferencedSchema(document);
            Assert.NotNull(schema);
            Assert.Equal("object", schema.Type);
            // Assert that OpenAPI-specific discriminator mappings have
            // been set
            Assert.Equal("$discriminator", schema.Discriminator.PropertyName);
            Assert.Collection(schema.Discriminator.Mapping, (mapping) =>
            {
                Assert.Equal("3d", mapping.Key);
                Assert.Equal("#/components/schemas/DiscriminatedThreeDimensionalPointWithCustomName", mapping.Value);
            });
            Assert.Collection(schema.OneOf,
                schema =>
                {
                    var referencedSchema = schema.GetReferencedSchema(document);
                    Assert.Collection(referencedSchema.Properties,
                        property =>
                        {
                            Assert.Equal("$discriminator", property.Key);
                            var discriminator = Assert.IsType<OpenApiString>(Assert.Single(property.Value.Enum));
                            Assert.Equal("3d", discriminator.Value);
                        },
                        property =>
                        {
                            Assert.Equal("z", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("x", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        },
                        property =>
                        {
                            Assert.Equal("y", property.Key);
                            Assert.Equal("integer", property.Value.Type);
                        });
                });
        });
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$discriminator")]
    [JsonDerivedType(typeof(ThreeDimensionalPointWithCustomName), typeDiscriminator: "3d")]
    public abstract class BasePointWithCustomName
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public sealed class ThreeDimensionalPointWithCustomName : BasePointWithCustomName
    {
        public int Z { get; set; }
    }
}
