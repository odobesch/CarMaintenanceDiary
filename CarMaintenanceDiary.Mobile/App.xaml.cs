namespace CarMaintenanceDiary.Mobile
{
    public partial class App : IApplication
    {
        public static IServiceProvider Services { get; private set; } = default!;

        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                // Log or display a message
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                // Log or display a message
                e.SetObserved();
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}