using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MandelbrotVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {    
        
        MandelbrotEngine Engine = MandelbrotEngine.Instance;
        bool isSaving = false;
        int SaveResolution = 800;
        public MainWindow()
        {
            InitializeComponent();                     
        }
        void BindUIStuff()
        {
            BindingOperations.SetBinding(XMINBOX, ContentControl.ContentProperty, new Binding("XStart") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(XMAXBOX, ContentControl.ContentProperty, new Binding("XEnd") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(YMINBOX, ContentControl.ContentProperty, new Binding("MirroredYEnd") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(YMAXBOX, ContentControl.ContentProperty, new Binding("MirroredYStart") { Source = Engine, Mode = BindingMode.OneWay });
            //BindingOperations.SetBinding(ProgressLabel, ContentControl.ContentProperty, new Binding("ProgressString") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(this, FrameworkElement.CursorProperty, new Binding("CurrentCursor") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(ImageResLabel, ContentControl.ContentProperty, new Binding("ResolutionString") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(RenderMultiplierTextbox, TextBox.TextProperty, new Binding("RenderMultiplier") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(IterationTextbox, TextBox.TextProperty, new Binding("MaxIterations") { Source = Engine, Mode = BindingMode.OneWay });

            BindingOperations.SetBinding(RenderMultiplierTextbox, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(IterationTextbox, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(ResetButton, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(PrecisionComboBox, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(PrecisionTextBox, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindingOperations.SetBinding(ProgressLabel, ContentControl.ContentProperty, new Binding("ProgressString") { Source = Engine, Mode = BindingMode.OneWay });
            await Engine.InitializeOn(MyCanvas, (int)MyCanvas.Width, (int)MyCanvas.Height);
            BindUIStuff();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSaving)
            {
                return;
            }
            
            string path = Directory.GetCurrentDirectory();
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                }
                else
                {
                    MessageBoxResult check = MessageBox.Show($"Bad folder selected, would you like to continue saving the image to:{Environment.NewLine}{path}", "Error", MessageBoxButton.YesNo);
                    if(check == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }
            Stopwatch SaveTime = new Stopwatch();
            SaveTime.Start();
            isSaving = true;
            SaveButton.IsEnabled = false;
            SaveResComboBox.IsEnabled = false;
            SaveButton.Content = "Saving...";

            path += @"\SavedImage1.png";

            int i = 1;
            while (File.Exists(path))
            {
                path = path.Replace($@"\SavedImage{i}.png",$@"\SavedImage{i + 1}.png");
                i++;
            }
            int currentIterations = Engine.MaxIterations;
            await Task.Run(() => Engine.SaveImageToPath(path, SaveResolution, currentIterations));
            isSaving = false;
            SaveButton.Content = "Save Image";
            SaveButton.IsEnabled = true;
            SaveResComboBox.IsEnabled = true;
            SaveTime.Stop();
            MessageBox.Show("Image saved to:" + Environment.NewLine + path + Environment.NewLine + $"The operation took {SaveTime.Elapsed.ToString(@"mm\:ss")}","Save complete.");
        }

        private async void RenderMultiplierTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(RenderMultiplierTextbox.Text, out double newMultiplier) && newMultiplier >= 0.1 && newMultiplier <= 28.9)
            {
                if(newMultiplier == Engine.RenderMultiplier)
                {
                    return;
                }
                Engine.RenderMultiplier = newMultiplier;
                RenderInfoLabel.Content = $"New RenderMultiplier: {newMultiplier}x";
                await Task.Delay(1000);
                RenderInfoLabel.Content = string.Empty;

            }
            else
            {
                RenderMultiplierTextbox.Text = Engine.RenderMultiplier.ToString();
                RenderInfoLabel.Content = $"New multiplier wasn't saved";
                await Task.Delay(1000);
                RenderInfoLabel.Content = string.Empty;
            }
        }

        private void IterationTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(IterationTextbox.Text, out int NewMaxIterations) && NewMaxIterations >= 100 && NewMaxIterations <= 10000)
            {
                if(NewMaxIterations == Engine.MaxIterations)
                {
                    return;
                }
                Engine.MaxIterations = NewMaxIterations;
            }
        }

        private async void ResetBUtton_Click(object sender, RoutedEventArgs e)
        {
            await Engine.ResetToDefaults();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int SaveResolution = Convert.ToInt32((cb.SelectedItem as ComboBoxItem)!.Tag);
            if(SaveResolution < 400)
            {
                SaveResolution = 800;
                cb.SelectedIndex = 0;
            }
            if(SaveResolution > 8000)
            {
                MessageBoxResult mbr = MessageBox.Show($"{SaveResolution} x {SaveResolution} is going to result in a massive image and will take a long time to save, are you sure you want to use this resolution?","Warning",MessageBoxButton.YesNo );
                if(mbr == MessageBoxResult.No)
                {
                    cb.SelectedIndex = 0;
                    return;
                }
            }
            this.SaveResolution = SaveResolution;
        }

        private void PrecisionChanged_Event(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int precision = Convert.ToInt32((cb.SelectedItem as ComboBoxItem)!.Tag);
            switch(precision)
            {
                case 0:
                    Engine.CurrentPrecision = Precision.DOUBLE; break;
                case 1:
                    Engine.CurrentPrecision = Precision.DECIMAL; break;
                case 2:
                    Engine.CurrentPrecision = Precision.HIGHPRECISION; break;
            }
        }

        private void Precision_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(PrecisionTextBox.Text, out int NewPrecision) && NewPrecision >= 10 && NewPrecision <= 100)
            {
                if (NewPrecision == HighPrecisionDecimal.CurrentMaxPrecision)
                {
                    return;
                }
                HighPrecisionDecimal.CurrentMaxPrecision = NewPrecision;
            }
        }
    }
}
