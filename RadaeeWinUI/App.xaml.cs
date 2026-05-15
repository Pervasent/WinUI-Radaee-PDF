using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using RadaeeWinUI.Services;
using RadaeeWinUI.ViewModels;

namespace RadaeeWinUI
{
    public partial class App : Application
    {
        private Window? _window;
        private static IServiceProvider? _serviceProvider;

        public static Window MainWindow { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDocumentManager, DocumentManager>();
            services.AddSingleton<IPageRenderService, PageRenderService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILayoutManager, LayoutManager>();
            services.AddSingleton<IAnnotationManager, AnnotationManager>();
            services.AddSingleton<IDOCXRenderService, DOCXRenderService>();

            services.AddSingleton<PDFViewModel>();
            services.AddSingleton<DOCXViewModel>();
            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : class
        {
            return _serviceProvider?.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            MainWindow = _window;
            _window.Activate();
        }
    }
}
