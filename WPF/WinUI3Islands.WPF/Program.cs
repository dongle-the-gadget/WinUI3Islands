using System;
using System.Collections.Generic;
using Toolkit;

namespace WinUI3Islands.WPF
{
    static class Program
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static XamlApplication xamlApp;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [STAThread]
        static void Main()
        {
            xamlApp = new XamlApplication(new List<Microsoft.UI.Xaml.Markup.IXamlMetadataProvider>() { new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider() });
            xamlApp.Resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
