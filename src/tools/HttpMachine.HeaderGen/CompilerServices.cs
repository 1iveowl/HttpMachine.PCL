// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Records use init-only setters, which the compiler lowers to a modreq on IsExternalInit. That
// type does not exist before net5.0, so a source generator - which must target netstandard2.0 to
// be loadable by the compiler - has to supply it. Mirrors the copy in HttpMachine.netstandard and
// IHttpMachine; no #if here because this project only ever targets netstandard2.0.

using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}
