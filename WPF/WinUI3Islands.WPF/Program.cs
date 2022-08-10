using System;
using System.Collections.Generic;

namespace WinUI3Islands.WPF
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var xamlApp = new Toolkit.XamlApplication(new List<Microsoft.UI.Xaml.Markup.IXamlMetadataProvider>() { new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider() }))
            {
                xamlApp.Resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
