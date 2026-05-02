using PharmacyApp.Models.Dto;
using PharmacyApp.Services;
using PharmacyApp.ViewModels;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PharmacyApp
{
    public partial class MainWindow : Window
    {
        public MainWindow(SessionUser user)
        {
            InitializeComponent();
            var vm = new MainViewModel(user);
            DataContext = vm;
            vm.CurrentViewModelChanged += OnCurrentViewModelChanged;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var expiring = ReportService.GetExpiringCount(30);
            if (expiring > 0)
                MessageBox.Show($"Внимание! {expiring} партий истекают в течение 30 дней.",
                                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OnCurrentViewModelChanged()
        {
            var fadeIn = new DoubleAnimation(0, 1, new Duration(System.TimeSpan.FromMilliseconds(300)));
            TransitionControl.BeginAnimation(OpacityProperty, fadeIn);

            var slideIn = new DoubleAnimation
            {
                From = 10,
                To = 0,
                Duration = new Duration(System.TimeSpan.FromMilliseconds(200)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var transform = new TranslateTransform(10, 0);
            TransitionControl.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
        }
    }
}