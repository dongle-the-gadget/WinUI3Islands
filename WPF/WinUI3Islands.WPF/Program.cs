using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using Toolkit;

namespace WinUI3Islands.WPF
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (XamlApplication xamlApp = new(new List<IXamlMetadataProvider>()
            {
                new CSCustomComponents.CSCustomComponents_XamlTypeInfo.XamlMetaDataProvider()
            }))
            {
                xamlApp.Resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
