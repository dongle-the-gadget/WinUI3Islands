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
        /// Gets XAML Content by type name
        /// </summary>
        public static DependencyProperty InitialTypeNameProperty { get; } = DependencyProperty.Register("InitialTypeName", typeof(string), typeof(WindowsXamlHost));

        /// <summary>
        /// Gets or sets XAML Content by type name
        /// </summary>
        /// <example><code>XamlClassLibrary.MyUserControl</code></example>
        /// <remarks>
        /// Content creation is deferred until after the parent hwnd has been created.
        /// </remarks>
        [Browsable(true)]
        [Category("XAML")]
        public string InitialTypeName
        {
            get => (string)GetValue(InitialTypeNameProperty);

            set => SetValue(InitialTypeNameProperty, value);
        }

        private WUX.UIElement CreateXamlContent()
        {
            WUX.UIElement content = null;
            try
            {
                content = UWPTypeFactory.CreateXamlContentByType(InitialTypeName);
            }
            catch
            {
                content = new WUX.Controls.TextBlock()
                {
                    Text = $"Cannot create control of type {InitialTypeName}",
                };
            }

            return content;
        }

        /// <summary>
        /// Creates <see cref="WUX.Application" /> object, wrapped <see cref="WUX.Hosting.DesktopWindowXamlSource" /> instance; creates and
        /// sets root UWP XAML element on DesktopWindowXamlSource.
        /// </summary>
        /// <param name="hwndParent">Parent window handle</param>
        /// <returns>Handle to XAML window</returns>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create and set initial root UWP XAML content
            if (!string.IsNullOrEmpty(InitialTypeName) && Child == null)
            {
                Child = CreateXamlContent();
                var frameworkElement = Child as WUX.FrameworkElement;

                // Default to stretch : UWP XAML content will conform to the size of WindowsXamlHost
                if (frameworkElement != null)
                {
                    frameworkElement.HorizontalAlignment = WUX.HorizontalAlignment.Stretch;
                    frameworkElement.VerticalAlignment = WUX.VerticalAlignment.Stretch;
                }
            }

            return base.BuildWindowCore(hwndParent);
        }

        /// <summary>
        /// Set data context on <seealso cref="Child"/> when it has changed.
        /// </summary>
        protected override void OnChildChanged()
        {
            base.OnChildChanged();
            var frameworkElement = ChildInternal as WUX.FrameworkElement;
            if (frameworkElement != null)
            {
                // WindowsXamlHost DataContext should flow through to UWP XAML content
                frameworkElement.DataContext = DataContext;
            }
        }
    }
}
