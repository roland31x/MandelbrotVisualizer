using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            BindingOperations.SetBinding(ProgressLabel, ContentControl.ContentProperty, new Binding("ProgressString") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(this, FrameworkElement.CursorProperty, new Binding("CurrentCursor") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(ImageResLabel, ContentControl.ContentProperty, new Binding("ResolutionString") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(RenderMultiplierTextbox, TextBox.TextProperty, new Binding("RenderMultiplier") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(IterationTextbox, TextBox.TextProperty, new Binding("MaxIterations") { Source = Engine, Mode = BindingMode.OneWay });

            BindingOperations.SetBinding(RenderMultiplierTextbox, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(IterationTextbox, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(ResetButton, ContentControl.IsEnabledProperty, new Binding("isNotLoading") { Source = Engine, Mode = BindingMode.OneWay });

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            isSaving = true;
            SaveButton.IsEnabled = false;
            SaveButton.Content = "Saving...";
            string path = @"E:\Stuff\SavedImage3.png";
            await Task.Run(() => Engine.SaveImageToPath(path,1600)); // careful with resolution

            isSaving = false;
            SaveButton.Content = "Save Image";
            SaveButton.IsEnabled = true;
            
        }

        private async void RenderMultiplierTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(RenderMultiplierTextbox.Text, out double newMultiplier) && newMultiplier >= 1 && newMultiplier <= 28.9)
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
    }
}
