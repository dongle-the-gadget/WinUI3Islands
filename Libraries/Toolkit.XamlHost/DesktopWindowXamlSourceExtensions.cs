// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using WUX = Microsoft.UI.Xaml;
using WinRT;

namespace Toolkit.XamlHost
{
    /// <summary>
    /// COM wrapper required to access native-only methods on <see cref="WUX.Hosting.DesktopWindowXamlSource" />
    /// </summary>
    public static class DesktopWindowXamlSourceExtensions
    {
        /// <summary>
        /// Gets the <see cref="IDesktopWindowXamlSourceNative" /> interface from a <see cref="WUX.Hosting.DesktopWindowXamlSource" /> instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface to cast to</typeparam>
        /// <param name="desktopWindowXamlSource">The DesktopWindowXamlSource instance to get the interface from</param>
        /// <returns><see cref="IDesktopWindowXamlSourceNative" /> interface pointer</returns>
        /// <remarks>
        /// This interface is the only way to set DesktopWindowXamlSource's target window for rendering.
        /// </remarks>
        public static TInterface GetInterop<TInterface>(this WUX.Hosting.DesktopWindowXamlSource desktopWindowXamlSource)
            where TInterface : class
            => desktopWindowXamlSource.As<TInterface>();
    }
}