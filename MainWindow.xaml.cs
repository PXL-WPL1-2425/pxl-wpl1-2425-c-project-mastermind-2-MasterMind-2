using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        private readonly List<string> colors = new List<string> { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
        private List<string> secretCode;
        private int attempts = 0;
        private int maxAttempts = 10;
        private int score = 100;
        private Timer countdownTimer;
        private int timeLeft = 10;
        private bool isDebugMode = false; // Debug-modus status

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            foreach (var comboBox in new[] { ColorBox1, ColorBox2, ColorBox3, ColorBox4 })
            {
                comboBox.ItemsSource = colors;
                comboBox.SelectedIndex = 0;
            }

            secretCode = GenerateRandomCode();
            DebugTextBox.Text = string.Join(", ", secretCode); // Toon de geheime code in debug-veld
            HistoryPanel.Children.Clear();
            attempts = 0;
            score = 100;
            UpdateAttemptsLabel();
            UpdateScoreLabel();
            StartCountdown();
        }

        private List<string> GenerateRandomCode()
        {
            Random random = new Random();
            return Enumerable.Range(0, 4).Select(_ => colors[random.Next(colors.Count)]).ToList();
        }

        private void CheckCode_Click(object sender, RoutedEventArgs e)
        {
            StopCountdown();
            var guessedCode = new List<string>
            {
                ColorBox1.Text,
                ColorBox2.Text,
                ColorBox3.Text,
                ColorBox4.Text
            };

            if (guessedCode.SequenceEqual(secretCode))
            {
                EndGame("Gefeliciteerd! Je hebt de code gekraakt!");
                return;
            }

            ProvideFeedback(guessedCode);
            UpdateScore(guessedCode);

            attempts++;
            UpdateAttemptsLabel();

            if (attempts >= maxAttempts)
            {
                EndGame($"Helaas, je hebt geen pogingen meer. De code was: {string.Join(", ", secretCode)}");
            }
            else
            {
                StartCountdown();
            }
        }

        private void ProvideFeedback(List<string> guessedCode)
        {
            var feedbackRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            for (int i = 0; i < guessedCode.Count; i++)
            {
                var border = new Border
                {
                    Child = new TextBlock
                    {
                        Text = guessedCode[i],
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    BorderThickness = new Thickness(2),
                    Margin = new Thickness(5)
                };

                if (guessedCode[i] == secretCode[i])
                {
                    border.BorderBrush = Brushes.DarkRed;
                }
                else if (secretCode.Contains(guessedCode[i]))
                {
                    border.BorderBrush = Brushes.Wheat;
                }
                else
                {
                    border.BorderBrush = Brushes.Gray;
                }

                feedbackRow.Children.Add(border);
            }
            HistoryPanel.Children.Add(feedbackRow);
        }

        private void UpdateScore(List<string> guessedCode)
        {
            for (int i = 0; i < guessedCode.Count; i++)
            {
                if (guessedCode[i] == secretCode[i])
                {
                    // Geen strafpunten
                }
                else if (secretCode.Contains(guessedCode[i]))
                {
                    score -= 1; // 1 strafpunt
                }
                else
                {
                    score -= 2; // 2 strafpunten
                }
            }
            UpdateScoreLabel();
        }

        private void UpdateScoreLabel()
        {
            ScoreLabel.Content = $"Score: {score}";
        }

        private void UpdateAttemptsLabel()
        {
            AttemptsLabel.Content = $"Pogingen: {attempts} / {maxAttempts}";
        }

        private void StartCountdown()
        {
            timeLeft = 10;
            TimerLabel.Content = $"Tijd: {timeLeft}s";

            if (countdownTimer == null)
            {
                countdownTimer = new Timer(1000);
                countdownTimer.Elapsed += CountdownTimer_Elapsed;
            }

            countdownTimer.Start();
        }

        private void CountdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeLeft--;
            Dispatcher.Invoke(() =>
            {
                TimerLabel.Content = $"Tijd: {timeLeft}s";
                if (timeLeft <= 0)
                {
                    countdownTimer.Stop();
                    attempts++;
                    UpdateAttemptsLabel();

                    if (attempts >= maxAttempts)
                    {
                        EndGame($"Helaas, je hebt geen pogingen meer. De code was: {string.Join(", ", secretCode)}");
                    }
                    else
                    {
                        StartCountdown();
                    }
                }
            });
        }

        private void StopCountdown()
        {
            countdownTimer?.Stop();
        }

        private void EndGame(string message)
        {
            StopCountdown();
            var result = MessageBox.Show($"{message}\nWil je opnieuw spelen?", "Spel beëindigd", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                InitializeGame();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void ToggleDebug()
        {
            isDebugMode = !isDebugMode;
            DebugTextBox.Visibility = isDebugMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ToggleDebug();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Weet je zeker dat je wilt afsluiten?", "Afsluiten", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
