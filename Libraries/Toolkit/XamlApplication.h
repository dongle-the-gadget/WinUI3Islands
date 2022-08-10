#pragma once

#include "XamlApplication.g.h"
#include <winrt/Microsoft.UI.Xaml.Hosting.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Microsoft.UI.Xaml.Markup.h>
#include <Windows.h>

namespace winrt::Toolkit::implementation
{
    enum ExecutionMode
    {
        UWP = 0,
        Win32 = 1,
    };

    class XamlApplication : public XamlApplicationT<XamlApplication, winrt::Microsoft::UI::Xaml::Markup::IXamlMetadataProvider>
    {
    public:
        XamlApplication();
        XamlApplication(winrt::Windows::Foundation::Collections::IVector<winrt::Microsoft::UI::Xaml::Markup::IXamlMetadataProvider> providers);
        ~XamlApplication();

        void Initialize();
        void Close();

        winrt::Windows::Foundation::IClosable WindowsXamlManager() const;

        winrt::Microsoft::UI::Xaml::Markup::IXamlType GetXamlType(winrt::Windows::UI::Xaml::Interop::TypeName const& type);
        winrt::Microsoft::UI::Xaml::Markup::IXamlType GetXamlType(winrt::hstring const& fullName);
        winrt::com_array<winrt::Microsoft::UI::Xaml::Markup::XmlnsDefinition> GetXmlnsDefinitions();

        winrt::Windows::Foundation::Collections::IVector<winrt::Microsoft::UI::Xaml::Markup::IXamlMetadataProvider> MetadataProviders();

        bool IsDisposed() const
        {
            return m_bIsClosed;
        }

    private:
        ExecutionMode m_executionMode = ExecutionMode::Win32;
        winrt::Microsoft::UI::Xaml::Hosting::WindowsXamlManager m_windowsXamlManager = nullptr;
        winrt::Windows::Foundation::Collections::IVector<winrt::Microsoft::UI::Xaml::Markup::IXamlMetadataProvider> m_providers = winrt::single_threaded_vector<Microsoft::UI::Xaml::Markup::IXamlMetadataProvider>();
        bool m_bIsClosed = false;
    };
}

namespace winrt::Toolkit::factory_implementation
{
    class XamlApplication : public XamlApplicationT<XamlApplication, implementation::XamlApplication>
    {
    public:
        XamlApplication();
        ~XamlApplication();
    private:
        std::array<HMODULE, 2> m_preloadInstances;
    };
}
