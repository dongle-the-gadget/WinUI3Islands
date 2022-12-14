// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Toolkit.XamlHost;
using WUX = Microsoft.UI.Xaml;

namespace Toolkit.WPF
{
    /// <summary>
    /// WindowsXamlHost control hosts UWP XAML content inside the Windows Presentation Foundation
    /// </summary>
    partial class WindowsXamlHost : WindowsXamlHostBase
    {
        /// <summary>
        /// Gets or sets the root UWP XAML element displayed in the WPF control instance.
        /// </summary>
        /// <remarks>This UWP XAML element is the root element of the wrapped DesktopWindowXamlSource.</remarks>
        [Browsable(true)]
        public WUX.UIElement Child
        {
            get => ChildInternal;

            set => ChildInternal = value;
        }
    }
}
