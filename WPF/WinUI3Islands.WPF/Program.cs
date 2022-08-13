using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider(),
                new CSCustomComponents.CSCustomComponents_XamlTypeInfo.XamlMetaDataProvider()
            }))
            {
                xamlApp.Resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
        }
    }
}
