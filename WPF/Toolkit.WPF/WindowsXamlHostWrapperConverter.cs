// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.UI.Xaml;
using windows = Windows;

namespace Toolkit.WPF
{
    /// <summary>
    /// Dual interface (<see cref="IValueConverter"/> and <see cref="Microsoft.UI.Xaml.Data.IValueConverter"/>),
    /// assumes that the conversion is between a <see cref="WindowsXamlHostBase"/> and its wrapped <see cref="UIElement"/>
    /// and attempts to return the correct instance of each.
    /// </summary>
    public class WindowsXamlHostWrapperConverter : IValueConverter, Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Microsoft.UI.Xaml.UIElement)?.GetWrapper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as WindowsXamlHostBase)?.GetUwpInternalObject();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
