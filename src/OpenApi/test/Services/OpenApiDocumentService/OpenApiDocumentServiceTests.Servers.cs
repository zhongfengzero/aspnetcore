// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Moq;

public partial class OpenApiDocumentServiceTests
{
    [Fact]
    public void GetOpenApiServers_HandlesServerAddressFeatureWithValues()
    {
        // Arrange
        var hostEnvironment = new HostingEnvironment
        {
            ApplicationName = "TestApplication"
        };
        var docService = new OpenApiDocumentService(
            "v1",
            new Mock<IApiDescriptionGroupCollectionProvider>().Object,
            hostEnvironment,
            new Mock<IOptionsMonitor<OpenApiOptions>>().Object,
            new Mock<IKeyedServiceProvider>().Object,
            new OpenApiTestServer(["http://localhost:5000"]));

        // Act
        var servers = docService.GetOpenApiServers();

        // Assert
        Assert.Contains("http://localhost:5000", servers.Select(s => s.Url));
    }

    [Fact]
    public void GetOpenApiServers_HandlesServerAddressFeatureWithNoValues()
    {
        // Arrange
        var hostEnvironment = new HostingEnvironment
        {
            ApplicationName = "TestApplication"
        };
        var docService = new OpenApiDocumentService(
            "v2",
            new Mock<IApiDescriptionGroupCollectionProvider>().Object,
            hostEnvironment,
            new Mock<IOptionsMonitor<OpenApiOptions>>().Object,
            new Mock<IKeyedServiceProvider>().Object,
            new OpenApiTestServer());

        // Act
        var servers = docService.GetOpenApiServers();

        // Assert
        Assert.Empty(servers);
    }
}
