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
        void BindCoordonates()
        {
            BindingOperations.SetBinding(XMINBOX, ContentControl.ContentProperty, new Binding("XStart") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(XMAXBOX, ContentControl.ContentProperty, new Binding("XEnd") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(YMINBOX, ContentControl.ContentProperty, new Binding("MirroredYEnd") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(YMAXBOX, ContentControl.ContentProperty, new Binding("MirroredYStart") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(ProgressLabel, ContentControl.ContentProperty, new Binding("ProgressString") { Source = Engine, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(this, FrameworkElement.CursorProperty, new Binding("CurrentCursor") { Source = Engine, Mode = BindingMode.OneWay });
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Engine.InitializeOn(DrawingCanvas, (int)DrawingCanvas.Width, (int)DrawingCanvas.Height);
            BindCoordonates();
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
            SaveButton.Content = "Saving...";
            string path = @"E:\Stuff\SavedImage1.png";
            await Task.Run(() => Engine.SaveImageToPath(path,8000));

            isSaving = false;
            SaveButton.Content = "SAVE IMAGE";
            
        }
    }
}
