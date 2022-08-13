﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Code Analysis results, point to "Suppress Message", and click
// "In Suppression File".
// You do not need to add suppressions to this file manually.
#pragma warning disable SA1404 // Code analysis suppression must have justification
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1205:Partial elements must declare access", Scope = "type", Target = "~T:Toolkit.WPF.WindowsXamlHostBase")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements must be documented", Scope = "type", Target = "~T:Toolkit.WPF.WindowsXamlHostBase")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1205:Partial elements must declare access", Scope = "type", Target = "~T:Toolkit.WPF.WindowsXamlHost")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Scope = "member", Target = "~M:Toolkit.WPF.WindowsXamlHostBase.#cctor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Scope = "member", Target = "~M:Microsoft.Toolkit.Forms.UI.XamlHost.WindowsXamlHostBase.#cctor")]

#pragma warning restore SA1404 // Code analysis suppression must have justification
