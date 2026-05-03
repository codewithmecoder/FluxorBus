using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace FluxorBus.SourceGen;

/// <summary>
/// Generates source code to register message handler types for FluxorBus using incremental source generation.
/// </summary>
/// <remarks>This generator scans for classes annotated with the MessageHandlerAttribute that implement the
/// IMessageHandler<T/> interface, and produces extension methods to register these handlers with the dependency
/// injection container. The generated code simplifies the setup of message handlers in FluxorBus-based applications by
/// automating their registration. This generator is intended for use at compile time and does not affect runtime
/// behavior beyond code generation.</remarks>
[Generator]
public class FluxorBusGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Fast syntax filtering (no semantic model yet)
        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 },

                transform: static (ctx, _) =>
                {
                    var cls = (ClassDeclarationSyntax)ctx.Node;
                    return ctx.SemanticModel.GetDeclaredSymbol(cls) as INamedTypeSymbol;
                })
            .Where(static s => s is not null);

        // 2. Combine with compilation (for symbol resolution cache)
        var pipeline = context.CompilationProvider.Combine(candidates.Collect());

        // 3. Emit generated source
        context.RegisterSourceOutput(pipeline, GenerateSource!);
    }

    private static void GenerateSource(
        SourceProductionContext spc,
        (Compilation compilation, ImmutableArray<INamedTypeSymbol> handlers) source)
    {
        var (compilation, handlers) = source;

        // Resolve interface symbol ONCE per compilation
        var handlerInterface = compilation
            .GetTypeByMetadataName("FluxorBus.Abstractions.IMessageHandler`1");

        var handlerBatchInterface = compilation
            .GetTypeByMetadataName("FluxorBus.Abstractions.IMessageBatchHandler`1");

        if (handlerInterface is null && handlerBatchInterface is null)
            return;

        var sb = new StringBuilder();

        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using FluxorBus.Abstractions;");
        sb.AppendLine();
        sb.AppendLine("namespace FluxorBus.SourceGen;");
        sb.AppendLine();
        sb.AppendLine("public static class FluxorBusGeneratedExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddFluxorBusGenerated(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var symbol in handlers)
        {
            var messageHandlerInterface = FindMessageHandlerInterface(symbol, handlerInterface);

            if (messageHandlerInterface is not null)
            {
                var messageType = messageHandlerInterface.TypeArguments[0];
                var messageTypeName = messageType.ToDisplayString();
                var handlerTypeName = symbol.ToDisplayString();

                sb.AppendLine(
                    $"        services.AddScoped<IMessageHandler<{messageTypeName}>, {handlerTypeName}>();");
            }

            var messageBatchHandlerInterface = FindMessageHandlerInterface(symbol, handlerBatchInterface);
            if (messageBatchHandlerInterface is null) continue;
            var messageBatchType = messageBatchHandlerInterface.TypeArguments[0];
            var messageBatchTypeName = messageBatchType.ToDisplayString();
            var handlerBatchTypeName = symbol.ToDisplayString();

            sb.AppendLine(
                $"        services.AddScoped<IMessageBatchHandler<{messageBatchTypeName}>, {handlerBatchTypeName}>();");
        }

        sb.AppendLine();
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        spc.AddSource("FluxorBus.g.cs", sb.ToString());
    }

    private static INamedTypeSymbol? FindMessageHandlerInterface(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? handlerInterface)
    {
        // Avoid LINQ allocations
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var iface in symbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    iface.OriginalDefinition,
                    handlerInterface))
            {
                return iface;
            }
        }

        return null;
    }
}