// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

[Generator]
public sealed partial class XmlCommentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        while (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Console.WriteLine($"Waiting for debugger to attach on {System.Diagnostics.Process.GetCurrentProcess().Id}...");
            System.Threading.Thread.Sleep(1000);
        }
        var comments = context.CompilationProvider.Select(ParseComments);
        var groupedAddOpenApiInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(FilterInvocations, GetAddOpenApiOverloadVariant)
            .GroupWith((variantDetails) => variantDetails.Location, AddOpenApiInvocationComparer.Instance);

        var result = comments.Combine(groupedAddOpenApiInvocations.Collect());

        context.RegisterSourceOutput(result, (context, output) =>
        {
            var comments = output.Left;
            var groupedAddOpenApiInvocations = output.Right;
            EmitXmlCommentCache(context, comments, groupedAddOpenApiInvocations);
        });
    }
}