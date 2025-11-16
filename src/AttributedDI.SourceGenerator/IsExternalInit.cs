// This file provides polyfill for C# 9+ record types in netstandard2.0

#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif

