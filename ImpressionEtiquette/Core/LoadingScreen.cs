using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;

namespace ImpressionEtiquetteDepot.Core
{
    public class LoadingScreen
    {
        public readonly Window _window;
        private readonly Label _loadingLabel;
        private readonly Label _loadingText;
        private readonly ProgressBar _progressBar;

        public LoadingScreen(string loadingText) 
        {

            _window = new Window
            {
                Width = 300,
                Height = 150,
                MaxWidth = 300,
                MinWidth = 300,
                MaxHeight = 130,
                MinHeight = 130,
                WindowStyle = WindowStyle.None,
                Background = Brushes.White,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,

            };


            _loadingLabel = new Label
            {
                Content = loadingText,
                FontFamily = new FontFamily("Arial"),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _loadingText = new Label
            {
                Content = "",
                FontFamily = new FontFamily("Arial"),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            _progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Height = 20,
            };

            _window.Content = new StackPanel
            {
                Children =
            {
                _loadingLabel,
                _loadingText,
                _progressBar,
            }
            };

        }
        public LoadingScreen(string loadingText, Brush backgroundColor, FontFamily fontFamily)
        {
            _window = new Window
            {
                Width = 300,
                Height = 150,
                MaxWidth = 300,
                MinWidth = 300,
                MaxHeight = 130,
                MinHeight = 130,
                WindowStyle = WindowStyle.None,
                Background = backgroundColor,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,

            };
            

            _loadingLabel = new Label
            {
                Content = loadingText,
                FontFamily = fontFamily,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _loadingText = new Label
            {
                Content = "",
                FontFamily = fontFamily,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            _progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Height = 20,
            };

            _window.Content = new StackPanel
            {
                Children =
            {
                _loadingLabel,
                _loadingText,
                _progressBar,
            }
            };           

            
        }

        public void SetMaxValue(int maxValue)
        {
            _progressBar.Dispatcher.Invoke(() =>
            {
                _progressBar.IsIndeterminate = false;
                _progressBar.Maximum = maxValue;
            });
        }

        public void SetIndeterminate()
        {
            _progressBar.Dispatcher.Invoke(() =>
            {
                _progressBar.IsIndeterminate = true;
            });
        }

        public void ResetValue()
        {
            _progressBar.Dispatcher.Invoke(() =>
            {
                _progressBar.Value = 0;
                _loadingText.Content = $"{_progressBar.Value}/{_progressBar.Maximum}";
            });
        }
        public void IncrementValue()
        {
            _progressBar.Dispatcher.Invoke(() =>
            {
                _progressBar.Value += 1;
                _loadingText.Content = $"{_progressBar.Value}/{_progressBar.Maximum}";
            });
        }

        public void Show()
        {
            _window.Show();
        }

        public void Close()
        {
            _window.Close();
        }
    }
}
