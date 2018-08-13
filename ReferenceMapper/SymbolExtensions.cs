using Microsoft.CodeAnalysis;

namespace ReferenceMapper
{
    public static class SymbolExtensions
    {
        public static string GetFullName(this IMethodSymbol symbol) =>
            $"{(symbol.IsStatic ? "static " : "")}{symbol.ReturnType} {symbol.ToDisplayString()}";

    }
}
