# WinUI3Islands

Experimental sample application demonstrating XAML Islands for WinUI 3.

## Prerequisites
- Visual Studio 2022 (with .NET Desktop Development, UWP app development and C++ app development workflow).
- [Windows App SDK 1.2 Runtime](https://aka.ms/windowsappsdk/1.2/1.2.220727.1-experimental1/windowsappruntimeinstall-x64.exe).

## Building the project
1. Clone this repository.
2. Open `WinUI3Islands.sln` in Visual Studio.
3. Select the correct project as the startup project:
   - For WPF, choose `WinUI3Islands.WPF.Package`.
   - For C++, choose `WinUI3Islands.CPP.Package`.
4. Start the project.
5. Wait for the project to compile and deploy on your machine.

## What currently works
### Status legends

**Note:** all emojis use the Emoji 1.0 standard (2015) or the Unicode 6.0 standard (2010).

✔️ Works as expected | ❗ Works with issues | ❌ Doesn't work | ❓ Untested
---------------------|-----------------------|-----------------|---------------

### Table
Item           | Status | Notes                                |
---------------|--------|--------------------------------------|
Styles support (via Community Toolkit port) | ✔️ |
WPF support | ❗ | Visual glitches are present, resizing is required on first launch. |
C++ support | ❗ | No theme detection for page background. |
WinForms support | ❓ |
Unpackaged support | ❓ | This sample doesn't provide unpackaged support. |
Custom C++ components support | ❓ |
Custom C# components support | ❓ |
Non-x64 architectures support | ❓ |

## Projects within solution
- **Libraries**
  - `Toolkit`: Port of Community Toolkit's `XamlApplication` to WinUI 3.
  - `Toolkit.Managed`: C#/WinRT projection for `Toolkit`.
- **WPF**
  - `Toolkit.WPF`: Port of Community Toolkit's `WindowsXamlHost` (WPF) for WinUI 3.
  - `WinUI3Islands.WPF`: The WPF sample app.
  - `WinUI3Islands.WPF.Package`: MSIX package for the WPF sample app.
- **CPP**
  - `WinUI3Islands.Cpp`: The C++ sample app.
  - `WinUI3Islands.CPP.Package`: MSIX package for the C++ sample app.
