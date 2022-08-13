// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using windows = Windows;

namespace Toolkit.WPF
{
    /// <summary>
    /// Extensions for use with UWP UIElement objects wrapped by the WindowsXamlHostBaseExt
    /// </summary>
    public static class UwpUIElementExtensions
    {
        private static bool IsDesktopWindowsXamlSourcePresent() => windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Microsoft.UI.Xaml.Hosting.HostingContract", 3);

        private static Microsoft.UI.Xaml.DependencyProperty WrapperProperty
        {
            get
            {
                if (IsDesktopWindowsXamlSourcePresent())
                {
                    var result = Microsoft.UI.Xaml.DependencyProperty.RegisterAttached("Wrapper", typeof(Microsoft.UI.Xaml.UIElement), typeof(UwpUIElementExtensions), new Microsoft.UI.Xaml.PropertyMetadata(null));
                    return result;
                }

                throw new NotImplementedException();
            }
        }

        public static WindowsXamlHostBase GetWrapper(this Microsoft.UI.Xaml.UIElement element)
        {
            if (IsDesktopWindowsXamlSourcePresent())
            {
                return (WindowsXamlHostBase)element.GetValue(WrapperProperty);
            }

            return null;
        }

        public static void SetWrapper(this Microsoft.UI.Xaml.UIElement element, WindowsXamlHostBase wrapper)
        {
            if (IsDesktopWindowsXamlSourcePresent())
            {
                element.SetValue(WrapperProperty, wrapper);
            }
        }
    }
}
