using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using WinRT;
using WF = Windows.Foundation;

namespace Toolkit.WPF
{
    static partial class UWPTypeFactory
    {
        /// <summary>
        /// Creates UWP XAML type instance from WinRT type name
        /// UWP XAML type name should be specified as: namespace.class
        /// ex: MyClassLibrary.MyCustomType
        /// ex: Windows.UI.Xaml.Shapes.Rectangle
        /// ex: Windows.UI.Xaml.Controls.Button
        /// </summary>
        /// <param name="xamlTypeName">UWP XAML type name</param>
        /// <exception cref="InvalidOperationException">Condition.</exception>
        /// <returns>Instance of UWP XAML type described by xamlTypeName string</returns>
        public static FrameworkElement CreateXamlContentByType(string xamlTypeName)
        {
            IXamlType xamlType = null;
            Type systemType = null;

            // If a root metadata provider has been defined on the application object,
            // use it to probe for custom UWP XAML type metadata.  If the root metadata
            // provider has not been implemented on the current application object, assume
            // the caller wants a built-in UWP XAML type, not a custom UWP XAML type.
            var xamlRootMetadataProvider = Application.Current as IXamlMetadataProvider;
            if (xamlRootMetadataProvider != null)
            {
                xamlType = xamlRootMetadataProvider.GetXamlType(xamlTypeName);
            }

            systemType = FindBuiltInType(xamlTypeName);

            if (systemType != null)
            {
                // Create built-in UWP XAML type
                return (FrameworkElement)Activator.CreateInstance(systemType);
            }

            if (xamlType != null)
            {
                // Create custom UWP XAML type
                return (FrameworkElement)xamlType.ActivateInstance();
            }

            throw new InvalidOperationException("Microsoft.Windows.Interop.UWPTypeFactory: Could not create type: " + xamlTypeName);
        }

        /// <summary>
        /// Searches for a built-in type by iterating through all types in
        /// all assemblies loaded in the current AppDomain
        /// </summary>
        /// <param name="typeName">Full type name, with namespace, without assembly</param>
        /// <returns>If found, <see cref="Type" />; otherwise, null..</returns>
        private static Type FindBuiltInType(string typeName)
        {
            var currentAppDomain = AppDomain.CurrentDomain;
            var appDomainLoadedAssemblies = currentAppDomain.GetAssemblies();

            foreach (var loadedAssembly in appDomainLoadedAssemblies)
            {
                var currentType = loadedAssembly.GetType(typeName);
                if (currentType != null)
                {
                    return currentType;
                }
            }

            return null;
        }
    }

    internal static class MetadataProviderDiscovery
    {
        private static readonly List<Type> FilteredTypes = new List<Type>
        {
            typeof(XamlApplication),
            typeof(IXamlMetadataProvider)
        };

        /// <summary>
        /// Probes working directory for all available metadata providers
        /// </summary>
        /// <returns>List of UWP XAML metadata providers</returns>
        internal static IEnumerable<IXamlMetadataProvider> DiscoverMetadataProviders()
        {
            // Get all assemblies loaded in app domain and placed side-by-side from all DLL and EXE
            var loadedAssemblies = GetAssemblies();
#if NET462
            var uniqueAssemblies = new HashSet<Assembly>(loadedAssemblies, EqualityComparerFactory<Assembly>.CreateComparer(
                a => a.GetName().FullName.GetHashCode(),
                (a, b) => a.GetName().FullName.Equals(b.GetName().FullName, StringComparison.OrdinalIgnoreCase)));
#else
            var uniqueAssemblies = new HashSet<Assembly>(loadedAssemblies, EqualityComparerFactory<Assembly>.CreateComparer(
                a => a.GetName().FullName.GetHashCode(StringComparison.InvariantCulture),
                (a, b) => a.GetName().FullName.Equals(b.GetName().FullName, StringComparison.OrdinalIgnoreCase)));
#endif

            // Load all types loadable from the assembly, ignoring any types that could not be resolved due to an issue in the dependency chain
            foreach (var assembly in uniqueAssemblies)
            {
                foreach (var provider in LoadTypesFromAssembly(assembly))
                {
                    yield return provider;

                    if (typeof(Application).IsAssignableFrom(provider.GetType()))
                    {
                        System.Diagnostics.Debug.WriteLine("Xaml application has been created");
                        yield break;
                    }
                }
            }
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return Assembly.GetExecutingAssembly();

            // Get assemblies already loaded in the current app domain
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                yield return a;
            }

            // Reflection-based runtime metadata probing
            var currentDirectory = new FileInfo(typeof(MetadataProviderDiscovery).Assembly.Location).Directory;

            foreach (var assembly in GetAssemblies(currentDirectory, "*.exe"))
            {
                yield return assembly;
            }

            foreach (var assembly in GetAssemblies(currentDirectory, "*.dll"))
            {
                yield return assembly;
            }
        }

        private static IEnumerable<Assembly> GetAssemblies(DirectoryInfo folder, string fileFilter)
        {
            foreach (var file in folder.EnumerateFiles(fileFilter))
            {
                Assembly a = null;

                try
                {
                    a = Assembly.LoadFrom(file.FullName);
                }
                catch (FileLoadException)
                {
                    // These exceptions are expected
                }
                catch (BadImageFormatException)
                {
                    // DLL is not loadable by CLR (e.g. Native)
                }

                if (a != null)
                {
                    yield return a;
                }
            }
        }

        /// <summary>
        /// Loads all types from the specified assembly and caches metadata providers
        /// </summary>
        /// <param name="assembly">Target assembly to load types from</param>
        /// <returns>The set of <seealso cref="WUX.Markup.IXamlMetadataProvider"/> found</returns>
        private static IEnumerable<IXamlMetadataProvider> LoadTypesFromAssembly(Assembly assembly)
        {
            // Load types inside the executing assembly
            foreach (var type in GetLoadableTypes(assembly))
            {
                // TODO: More type checking here
                // Not interface, not abstract, not generic, etc.
                if (typeof(IXamlMetadataProvider).IsAssignableFrom(type))
                {
                    var provider = (IXamlMetadataProvider)Activator.CreateInstance(type);
                    yield return provider;
                }
            }
        }

        // Algorithm from StackOverflow answer here:
        // http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                var asmTypes = assembly.DefinedTypes
                    .Select(t => t.AsType());
                var filteredTypes = asmTypes.Where(t => !FilteredTypes.Contains(t));
                return filteredTypes;
            }
            catch (ReflectionTypeLoadException)
            {
                return Enumerable.Empty<Type>();
            }
            catch (FileLoadException)
            {
                return Enumerable.Empty<Type>();
            }
        }

        private static class EqualityComparerFactory<T>
        {
            private class MyComparer : IEqualityComparer<T>
            {
                private readonly Func<T, int> _getHashCodeFunc;
                private readonly Func<T, T, bool> _equalsFunc;

                public MyComparer(Func<T, int> getHashCodeFunc, Func<T, T, bool> equalsFunc)
                {
                    _getHashCodeFunc = getHashCodeFunc;
                    _equalsFunc = equalsFunc;
                }

                public bool Equals(T x, T y) => _equalsFunc(x, y);

                public int GetHashCode(T obj) => _getHashCodeFunc(obj);
            }

            public static IEqualityComparer<T> CreateComparer(Func<T, int> getHashCodeFunc, Func<T, T, bool> equalsFunc)
            {
                if (getHashCodeFunc == null)
                {
                    throw new ArgumentNullException(nameof(getHashCodeFunc));
                }

                if (equalsFunc == null)
                {
                    throw new ArgumentNullException(nameof(equalsFunc));
                }

                return new MyComparer(getHashCodeFunc, equalsFunc);
            }
        }
    }

    public static partial class XamlApplicationExtensions
    {
        private static IXamlMetadataContainer _metadataContainer;
        private static bool _initialized = false;

        private static IXamlMetadataContainer GetCurrentProvider()
        {
            try
            {
                return Application.Current as IXamlMetadataContainer;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets and returns the current UWP XAML Application instance in a reference parameter.
        /// If the current XAML Application instance has not been created for the process (is null),
        /// a new <see cref="Microsoft.Toolkit.Win32.UI.XamlHost.XamlApplication" /> instance is created and returned.
        /// </summary>
        /// <returns>The instance of <seealso cref="XamlApplication"/></returns>
        public static IXamlMetadataContainer GetOrCreateXamlMetadataContainer()
        {
            // Instantiation of the application object must occur before creating the DesktopWindowXamlSource instance.
            // DesktopWindowXamlSource will create a generic Application object unable to load custom UWP XAML metadata.
            if (_metadataContainer == null && !_initialized)
            {
                _initialized = true;

                // Create a custom UWP XAML Application object that implements reflection-based XAML metadata probing.
                try
                {
                    _metadataContainer = GetCurrentProvider();
                    if (_metadataContainer == null)
                    {
                        var providers = MetadataProviderDiscovery.DiscoverMetadataProviders().ToList();
                        _metadataContainer = GetCurrentProvider();
                        if (_metadataContainer == null)
                        {
                            _metadataContainer = new XamlApplication(providers);
                            return _metadataContainer;
                        }
                    }
                    else
                    {
                        return _metadataContainer;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    _metadataContainer = GetCurrentProvider();
                }
            }

            var xamlApplication = _metadataContainer as XamlApplication;
            if (xamlApplication != null && xamlApplication.IsDisposed)
            {
                throw new ObjectDisposedException(typeof(XamlApplication).FullName);
            }

            return _metadataContainer;
        }
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0aea2f26-facf-4588-8cf4-34555124db32")]
    public interface IDesktopWindowXamlSourceNative
    {
        /// <summary>
        /// Attaches the <see cref="WUX.Hosting.DesktopWindowXamlSource" /> to a window using a window handle.
        /// </summary>
        /// <param name="parentWnd">pointer to parent Wnd</param>
        /// <remarks>
        /// The associated window will be used to parent UWP XAML visuals, appearing
        /// as UWP XAML's logical render target.
        /// </remarks>
        void AttachToWindow(IntPtr parentWnd);

        /// <summary>
        /// Gets the handle associated with the <see cref="WUX.Hosting.DesktopWindowXamlSource" /> instance.
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Sends the <paramref name="message"/> to the internal <see cref="DesktopWindowXamlSource" /> window handle.
        /// </summary>
        /// <returns>True if the <paramref name="message"/> was handled</returns>
        bool PreTranslateMessage(ref System.Windows.Interop.MSG message);
    }

    public static class UwpUIElementExtensions
    {
        private static bool IsDesktopWindowsXamlSourcePresent() => Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.UI.Xaml.Hosting.HostingContract", 3);

        private static DependencyProperty WrapperProperty
        {
            get
            {
                if (IsDesktopWindowsXamlSourcePresent())
                {
                    var result = DependencyProperty.RegisterAttached("Wrapper", typeof(System.Windows.UIElement), typeof(UwpUIElementExtensions), new PropertyMetadata(null));
                    return result;
                }

                throw new NotImplementedException();
            }
        }

        public static WindowsXamlHost GetWrapper(this UIElement element)
        {
            if (IsDesktopWindowsXamlSourcePresent())
            {
                return (WindowsXamlHost)element.GetValue(WrapperProperty);
            }

            return null;
        }

        public static void SetWrapper(this UIElement element, WindowsXamlHost wrapper)
        {
            if (IsDesktopWindowsXamlSourcePresent())
            {
                element.SetValue(WrapperProperty, wrapper);
            }
        }
    }

    public class WindowsXamlHost : HwndHost
    {
        /// <summary>
        /// Gets XAML Content by type name
        /// </summary>
        public static System.Windows.DependencyProperty InitialTypeNameProperty { get; } = System.Windows.DependencyProperty.Register("InitialTypeName", typeof(string), typeof(WindowsXamlHost));

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

        private UIElement CreateXamlContent()
        {
            UIElement content = null;
            try
            {
                content = UWPTypeFactory.CreateXamlContentByType(InitialTypeName);
            }
            catch
            {
                content = new Microsoft.UI.Xaml.Controls.TextBlock()
                {
                    Text = $"Cannot create control of type {InitialTypeName}",
                };
            }

            return content;
        }

        /// <summary>
        /// Gets or sets the root UWP XAML element displayed in the WPF control instance.
        /// </summary>
        /// <remarks>This UWP XAML element is the root element of the wrapped DesktopWindowXamlSource.</remarks>
        [Browsable(true)]
        public UIElement Child
        {
            get => ChildInternal;

            set => ChildInternal = value;
        }

        private static readonly IXamlMetadataContainer _metadataContainer;

        /// <summary>
        /// Dictionary that maps WPF (host framework) FocusNavigationDirection to UWP XAML XxamlSourceFocusNavigationReason
        /// </summary>
        private static readonly Dictionary<System.Windows.Input.FocusNavigationDirection, XamlSourceFocusNavigationReason>
            MapDirectionToReason =
                new Dictionary<System.Windows.Input.FocusNavigationDirection, XamlSourceFocusNavigationReason>
                {
                    { System.Windows.Input.FocusNavigationDirection.Next,     XamlSourceFocusNavigationReason.First },
                    { System.Windows.Input.FocusNavigationDirection.First,    XamlSourceFocusNavigationReason.First },
                    { System.Windows.Input.FocusNavigationDirection.Previous, XamlSourceFocusNavigationReason.Last },
                    { System.Windows.Input.FocusNavigationDirection.Last,     XamlSourceFocusNavigationReason.Last },
                    { System.Windows.Input.FocusNavigationDirection.Up,       XamlSourceFocusNavigationReason.Up },
                    { System.Windows.Input.FocusNavigationDirection.Down,     XamlSourceFocusNavigationReason.Down },
                    { System.Windows.Input.FocusNavigationDirection.Left,     XamlSourceFocusNavigationReason.Left },
                    { System.Windows.Input.FocusNavigationDirection.Right,    XamlSourceFocusNavigationReason.Right },
                };

        /// <summary>
        /// Dictionary that maps UWP XAML XamlSourceFocusNavigationReason to WPF (host framework) FocusNavigationDirection
        /// </summary>
        private static readonly Dictionary<XamlSourceFocusNavigationReason, System.Windows.Input.FocusNavigationDirection>
            MapReasonToDirection =
                new Dictionary<XamlSourceFocusNavigationReason, System.Windows.Input.FocusNavigationDirection>()
                {
                    { XamlSourceFocusNavigationReason.First, System.Windows.Input.FocusNavigationDirection.Next },
                    { XamlSourceFocusNavigationReason.Last,  System.Windows.Input.FocusNavigationDirection.Previous },
                    { XamlSourceFocusNavigationReason.Up,    System.Windows.Input.FocusNavigationDirection.Up },
                    { XamlSourceFocusNavigationReason.Down,  System.Windows.Input.FocusNavigationDirection.Down },
                    { XamlSourceFocusNavigationReason.Left,  System.Windows.Input.FocusNavigationDirection.Left },
                    { XamlSourceFocusNavigationReason.Right, System.Windows.Input.FocusNavigationDirection.Right },
                };

        /// <summary>
        /// Last Focus Request GUID to uniquely identify Focus operations, primarily used with error callbacks
        /// </summary>
        private Guid _lastFocusRequest = Guid.Empty;

        /// <summary>
        /// Override for OnGotFocus that passes NavigateFocus on to the DesktopXamlSource instance
        /// </summary>
        /// <param name="e">RoutedEventArgs</param>
        protected override void OnGotFocus(System.Windows.RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (!_xamlSource.HasFocus)
            {
                _xamlSource.NavigateFocus(
                    new XamlSourceFocusNavigationRequest(
                        XamlSourceFocusNavigationReason.Programmatic));
            }
        }

        /// <summary>
        /// Process Tab from host framework
        /// </summary>
        /// <param name="request"><see cref="System.Windows.Input.TraversalRequest"/> that contains requested navigation direction</param>
        /// <returns>Did handle tab</returns>
        protected override bool TabIntoCore(System.Windows.Input.TraversalRequest request)
        {
            if (_xamlSource.HasFocus && !_onTakeFocusRequested)
            {
                return false; // If we have focus already, then we dont need to NavigateFocus
            }

            // Bug 17544829: Focus is wrong if the previous element is in a different FocusScope than the WindowsXamlHost element.
            var focusedElement = System.Windows.Input.FocusManager.GetFocusedElement(
                System.Windows.Input.FocusManager.GetFocusScope(this)) as System.Windows.FrameworkElement;

            var origin = BoundsRelativeTo(focusedElement, this);
            var reason = MapDirectionToReason[request.FocusNavigationDirection];
            if (_lastFocusRequest == Guid.Empty)
            {
                _lastFocusRequest = Guid.NewGuid();
            }

            var sourceFocusNavigationRequest = new XamlSourceFocusNavigationRequest(reason, origin, _lastFocusRequest);
            try
            {
                var result = _xamlSource.NavigateFocus(sourceFocusNavigationRequest);

                // Returning true indicates that focus moved.  This will cause the HwndHost to
                // move focus to the source’s hwnd (call SetFocus Win32 API)
                return result.WasFocusMoved;
            }
            finally
            {
                _lastFocusRequest = Guid.Empty;
            }
        }

        /// <summary>
        /// Transform bounds relative to FrameworkElement
        /// </summary>
        /// <param name="sibling1">base rectangle</param>
        /// <param name="sibling2">second of pair to transform</param>
        /// <returns>result of transformed rectangle</returns>
        private static WF.Rect BoundsRelativeTo(System.Windows.FrameworkElement sibling1, System.Windows.Media.Visual sibling2)
        {
            WF.Rect origin = default(WF.Rect);

            if (sibling1 != null)
            {
                // TransformToVisual can throw an exception if two elements don't have a common ancestor
                try
                {
                    var transform = sibling1.TransformToVisual(sibling2);
                    var systemWindowsRect = transform.TransformBounds(
                        new System.Windows.Rect(0, 0, sibling1.ActualWidth, sibling1.ActualHeight));
                    origin.X = systemWindowsRect.X;
                    origin.Y = systemWindowsRect.Y;
                    origin.Width = systemWindowsRect.Width;
                    origin.Height = systemWindowsRect.Height;
                }
                catch (System.InvalidOperationException)
                {
                }
            }

            return origin;
        }

        private bool _onTakeFocusRequested = false;

        /// <summary>
        /// Handles the <see cref="DesktopWindowXamlSource.TakeFocusRequested" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DesktopWindowXamlSourceTakeFocusRequestedEventArgs"/> instance containing the event data.</param>
        private void OnTakeFocusRequested(object sender, DesktopWindowXamlSourceTakeFocusRequestedEventArgs e)
        {
            if (_lastFocusRequest == e.Request.CorrelationId)
            {
                // If we've arrived at this point, then focus is being move back to us
                // therefore, we should complete the operation to avoid an infinite recursion
                // by "Restoring" the focus back to us under a new correctationId
                var newRequest = new XamlSourceFocusNavigationRequest(
                    XamlSourceFocusNavigationReason.Restore);
                _xamlSource.NavigateFocus(newRequest);
            }
            else
            {
                _onTakeFocusRequested = true;
                try
                {
                    // Last focus request is not initiated by us, so continue
                    _lastFocusRequest = e.Request.CorrelationId;
                    var direction = MapReasonToDirection[e.Request.Reason];
                    var request = new System.Windows.Input.TraversalRequest(direction);
                    MoveFocus(request);
                }
                finally
                {
                    _onTakeFocusRequested = false;
                }
            }
        }

        private void OnThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled)
            {
                return;
            }

            var desktopWindowXamlSourceNative = _xamlSource.As<IDesktopWindowXamlSourceNative>();
            if (desktopWindowXamlSourceNative != null)
            {
                handled = desktopWindowXamlSourceNative.PreTranslateMessage(ref msg);
            }
        }

        protected override bool HasFocusWithinCore()
        {
            return _xamlSource.HasFocus;
        }

        static WindowsXamlHost()
        {
            _metadataContainer = XamlApplicationExtensions.GetOrCreateXamlMetadataContainer();
        }

        /// <summary>
        /// UWP XAML DesktopWindowXamlSource instance that hosts XAML content in a win32 application
        /// </summary>
        private readonly DesktopWindowXamlSource _xamlSource;

        /// <summary>
        /// Private field that backs ChildInternal property.
        /// </summary>
        private UIElement _childInternal;

        /// <summary>
        ///     Fired when WindowsXamlHost root UWP XAML content has been updated
        /// </summary>
        public event EventHandler ChildChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsXamlHostBase"/> class.
        /// </summary>
        /// <remarks>
        /// Default constructor is required for use in WPF markup. When the default constructor is called,
        /// object properties have not been set. Put WPF logic in OnInitialized.
        /// </remarks>
        public WindowsXamlHost()
        {
            // Create DesktopWindowXamlSource, host for UWP XAML content
            _xamlSource = new DesktopWindowXamlSource();

            // Hook DesktopWindowXamlSource OnTakeFocus event for Focus processing
            _xamlSource.TakeFocusRequested += OnTakeFocusRequested;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsXamlHostBase"/> class.
        /// </summary>
        /// <remarks>
        /// Constructor is required for use in WPF markup. When the default constructor is called,
        /// object properties have not been set. Put WPF logic in OnInitialized.
        /// </remarks>
        /// <param name="typeName">UWP XAML Type name</param>
        public WindowsXamlHost(string typeName)
            : this()
        {
            ChildInternal = UWPTypeFactory.CreateXamlContentByType(typeName);
            ChildInternal.SetWrapper(this);
        }

        /// <summary>
        /// Gets the current instance of <seealso cref="XamlApplication"/>
        /// </summary>
        protected static IXamlMetadataContainer MetadataContainer
        {
            get
            {
                return _metadataContainer;
            }
        }

        /// <summary>
        /// Binds this wrapper object's exposed WPF DependencyProperty with the wrapped UWP object's DependencyProperty
        /// for what effectively works as a regular one- or two-way binding.
        /// </summary>
        /// <param name="propertyName">the registered name of the dependency property</param>
        /// <param name="wpfProperty">the DependencyProperty of the wrapper</param>
        /// <param name="uwpProperty">the related DependencyProperty of the UWP control</param>
        /// <param name="converter">a converter, if one's needed</param>
        /// <param name="direction">indicates that the binding should be one or two directional.  If one way, the Uwp control is only updated from the wrapper.</param>
        public void Bind(string propertyName, System.Windows.DependencyProperty wpfProperty, DependencyProperty uwpProperty, object converter = null, System.ComponentModel.BindingDirection direction = System.ComponentModel.BindingDirection.TwoWay)
        {
            if (direction == System.ComponentModel.BindingDirection.TwoWay)
            {
                var binder = new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(propertyName),
                    Converter = (IValueConverter)converter
                };
                BindingOperations.SetBinding(ChildInternal, uwpProperty, binder);
            }

            var rebinder = new System.Windows.Data.Binding()
            {
                Source = ChildInternal,
                Path = new System.Windows.PropertyPath(propertyName),
                Converter = (System.Windows.Data.IValueConverter)converter
            };
            System.Windows.Data.BindingOperations.SetBinding(this, wpfProperty, rebinder);
        }

        /// <inheritdoc />
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (_childInternal != null)
            {
                SetContent();
            }
        }

        /// <summary>
        /// Gets or sets the root UWP XAML element displayed in the WPF control instance.
        /// </summary>
        /// <value>The <see cref="WUX.UIElement"/> child.</value>
        /// <remarks>This UWP XAML element is the root element of the wrapped <see cref="DesktopWindowXamlSource" />.</remarks>
        protected UIElement ChildInternal
        {
            get
            {
                return _childInternal;
            }

            set
            {
                if (value == ChildInternal)
                {
                    return;
                }

                var currentRoot = (FrameworkElement)ChildInternal;
                if (currentRoot != null)
                {
                    currentRoot.SizeChanged -= XamlContentSizeChanged;
                }

                _childInternal = value;
                SetContent();

                var frameworkElement = ChildInternal as FrameworkElement;
                if (frameworkElement != null)
                {
                    // If XAML content has changed, check XAML size
                    // to determine if WindowsXamlHost needs to re-run layout.
                    frameworkElement.SizeChanged += XamlContentSizeChanged;
                }

                OnChildChanged();

                // Fire updated event
                ChildChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Called when the property <seealso cref="ChildInternal"/> has changed.
        /// </summary>
        protected virtual void OnChildChanged()
        {
            var frameworkElement = ChildInternal as FrameworkElement;
            if (frameworkElement != null)
            {
                // WindowsXamlHost DataContext should flow through to UWP XAML content
                frameworkElement.DataContext = DataContext;
            }
        }

        /// <summary>
        /// Exposes ChildInternal without exposing its actual Type.
        /// </summary>
        /// <returns>the underlying UWP child object</returns>
        public object GetUwpInternalObject()
        {
            return ChildInternal;
        }

        /// <summary>
        /// Gets a value indicating whether this wrapper control instance been disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        private System.Windows.Window _parentWindow;

        /// <summary>
        /// Creates <see cref="WUX.Application" /> object, wrapped <see cref="DesktopWindowXamlSource" /> instance; creates and
        /// sets root UWP XAML element on <see cref="DesktopWindowXamlSource" />.
        /// </summary>
        /// <param name="hwndParent">Parent window handle</param>
        /// <returns>Handle to XAML window</returns>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create and set initial root UWP XAML content
            if (!string.IsNullOrEmpty(InitialTypeName) && Child == null)
            {
                Child = CreateXamlContent();
                var frameworkElement = Child as FrameworkElement;

                // Default to stretch : UWP XAML content will conform to the size of WindowsXamlHost
                if (frameworkElement != null)
                {
                    frameworkElement.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
                    frameworkElement.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
                }
            }

            this._parentWindow = System.Windows.Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.Closed += OnParentClosed;
            }

            ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;

            // 'EnableMouseInPointer' is called by the WindowsXamlManager during initialization. No need
            // to call it directly here.

            // Create DesktopWindowXamlSource instance
            var desktopWindowXamlSourceNative = _xamlSource.As<IDesktopWindowXamlSourceNative>();

            // Associate the window where UWP XAML will display content
            desktopWindowXamlSourceNative.AttachToWindow(hwndParent.Handle);

            var windowHandle = desktopWindowXamlSourceNative.WindowHandle;

            // Overridden function must return window handle of new target window (DesktopWindowXamlSource's Window)
            return new HandleRef(this, windowHandle);
        }

        /// <summary>
        /// The default implementation of SetContent applies ChildInternal to desktopWindowXamSource.Content.
        /// Override this method if that shouldn't be the case.
        /// For example, override if your control should be a child of another WindowsXamlHostBase-based control.
        /// </summary>
        protected virtual void SetContent()
        {
            if (_xamlSource != null)
            {
                _xamlSource.Content = _childInternal;
            }
        }

        /// <summary>
        /// Disposes the current instance in response to the parent window getting destroyed.
        /// </summary>
        /// <param name="sender">Paramter sender is ignored</param>
        /// <param name="e">Parameter args is ignored</param>
        private void OnParentClosed(object sender, EventArgs e)
        {
            this.Dispose(true);
        }

        /// <summary>
        /// WPF framework request to destroy control window.  Cleans up the HwndIslandSite created by DesktopWindowXamlSource
        /// </summary>
        /// <param name="hwnd">Handle of window to be destroyed</param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            Dispose(true);
        }

        /// <summary>
        /// WindowsXamlHost Dispose
        /// </summary>
        /// <param name="disposing">Is disposing?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.IsDisposed)
            {
                var currentRoot = (FrameworkElement)ChildInternal;
                if (currentRoot != null)
                {
                    currentRoot.SizeChanged -= XamlContentSizeChanged;
                }

                // Free any other managed objects here.
                ComponentDispatcher.ThreadFilterMessage -= this.OnThreadFilterMessage;
                ChildInternal = null;
                if (_xamlSource != null)
                {
                    _xamlSource.TakeFocusRequested -= OnTakeFocusRequested;
                }

                if (_parentWindow != null)
                {
                    _parentWindow.Closed -= OnParentClosed;
                    _parentWindow = null;
                }
            }

            // Free any unmanaged objects here.
            if (_xamlSource != null && !this.IsDisposed)
            {
                _xamlSource.Dispose();
            }

            // BUGBUG: CoreInputSink cleanup is failing when explicitly disposing
            // WindowsXamlManager.  Add dispose call back when that bug is fixed in 19h1.
            this.IsDisposed = true;

            // Call base class implementation.
            base.Dispose(disposing);
        }

        protected override IntPtr WndProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            const int WM_GETOBJECT = 0x003D;
            switch (msg)
            {
                // We don't want HwndHost to handle the WM_GETOBJECT.
                // Instead we want to let the HwndIslandSite's WndProc get it
                // So return handled = false and don't let the base class do
                // anything on that message.
                case WM_GETOBJECT:
                    handled = false;
                    return IntPtr.Zero;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        /// <summary>
        /// Measures wrapped UWP XAML content using passed in size constraint
        /// </summary>
        /// <param name="constraint">Available Size</param>
        /// <returns>XAML DesiredSize</returns>
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            var desiredSize = new System.Windows.Size(0, 0);

            if (IsXamlContentLoaded())
            {
                _xamlSource.Content.Measure(new WF.Size(constraint.Width, constraint.Height));
                desiredSize.Width = _xamlSource.Content.DesiredSize.Width;
                desiredSize.Height = _xamlSource.Content.DesiredSize.Height;
            }

            desiredSize.Width = Math.Min(desiredSize.Width, constraint.Width);
            desiredSize.Height = Math.Min(desiredSize.Height, constraint.Height);

            return desiredSize;
        }

        /// <summary>
        /// Arranges wrapped UWP XAML content using passed in size constraint
        /// </summary>
        /// <param name="finalSize">Final Size</param>
        /// <returns>Size</returns>
        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            if (IsXamlContentLoaded())
            {
                // Arrange is required to support HorizontalAlignment and VerticalAlignment properties
                // set to 'Stretch'.  The UWP XAML content will be 0 in the stretch alignment direction
                // until Arrange is called, and the UWP XAML content is expanded to fill the available space.
                var finalRect = new WF.Rect(0, 0, finalSize.Width, finalSize.Height);
                _xamlSource.Content.Arrange(finalRect);
            }

            return base.ArrangeOverride(finalSize);
        }

        /// <summary>
        /// Is the Xaml Content loaded and live?
        /// </summary>
        /// <returns>True if the Xaml content is properly loaded</returns>
        private bool IsXamlContentLoaded()
        {
            if (_xamlSource.Content == null)
            {
                return false;
            }

            if (VisualTreeHelper.GetParent(_xamlSource.Content) == null)
            {
                // If there's no parent to this content, it's not "live" or "loaded" in the tree yet.
                // Performing a measure or arrange in this state may cause unexpected results.
                return false;
            }

            return true;
        }

        /// <summary>
        /// UWP XAML content size changed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="windows.UI.Xaml.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void XamlContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
        }
    }
}
