using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.IO;

namespace ToDoWin
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string dynamicText;
        private int currentTextBlock = 0;
        private List<TextBox> textBoxes;
        private string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string DynamicText
        {
            get => dynamicText;
            set
            {
                if (dynamicText != value)
                {
                    dynamicText = value;
                    OnPropertyChanged(nameof(DynamicText));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            DynamicText = "Исходный текст";
            DataContext = this;
            textBoxes = new List<TextBox> { DynamicTextBox1, DynamicTextBox2, DynamicTextBox3, DynamicTextBox4 };
            LoadText();

            // Window configuration for taskbar-like appearance
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;

            SetStartup();
        }

        private void LoadText()
        {
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(docPath, "savedTexts.txt"));
                for (int i = 0; i < lines.Length && i < textBoxes.Count; i++)
                {
                    textBoxes[i].Text = lines[i];
                }
            }
            catch (Exception)
            {
                // Handle exceptions or do nothing if file doesn't exist
            }
        }

        private void SaveText()
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "savedTexts.txt")))
            {
                foreach (var textBox in textBoxes)
                {
                    outputFile.WriteLine(textBox.Text);
                }
            }
        }

        private void SetStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue("MyApp", System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting startup: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textBoxes[currentTextBlock].Text = InputTextBox.Text;
                currentTextBlock = (currentTextBlock + 1) % textBoxes.Count;
                InputTextBox.Clear();
                SaveText();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 0;
            this.Top = (SystemParameters.WorkArea.Height - this.Height) / 2;
            this.Width = 5;
            this.MouseEnter += (s, args) => ExpandWindow();
            this.MouseLeave += (s, args) => CollapseWindow();
        }

        private void ExpandWindow()
        {
            AnimateWidth(300, 250);
        }

        private void CollapseWindow()
        {
            AnimateWidth(5, 250);
        }

        private void AnimateWidth(double toWidth, int durationMilliseconds)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = toWidth,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds)
            };
            this.BeginAnimation(Window.WidthProperty, animation);
        }
    }
}
