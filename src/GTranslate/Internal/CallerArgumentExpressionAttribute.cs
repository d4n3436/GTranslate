#if !NETCOREAPP3_0_OR_GREATER
namespace System.Runtime.CompilerServices;

using Diagnostics;
using Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
#endif