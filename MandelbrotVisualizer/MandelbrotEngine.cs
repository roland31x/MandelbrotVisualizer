using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MandelbrotVisualizer
{
    public class MandelbrotEngine : INotifyPropertyChanged
    {
        static MandelbrotEngine _singleton = new MandelbrotEngine();
        public static MandelbrotEngine Instance
        {
            get
            {
                return _singleton;
            }
        }

        bool _ld = false;
        public bool isNotLoading { get { return !_ld; } }
        public bool isLoading
        {
            get
            {
                return _ld;
            }
            private set
            {              
                _ld = value;
                OnPropertyChanged();
                OnPropertyChanged("isNotLoading");
                OnPropertyChanged("ProgressString");
                if (_ld)
                {
                    CurrentProgress = 0;
                    CurrentCursor = Cursors.Wait;
                }
                else
                {
                    CurrentProgress = DHeight * DWidth;
                    if (DrawingCanvas.IsMouseDirectlyOver)
                    {
                        CurrentCursor = Cursors.None;
                    }
                    else
                    {
                        CurrentCursor = Cursors.Arrow;
                    }                 
                }
            }
        }


        Border _hov;      

        Cursor _current = Cursors.Arrow;
        public Cursor CurrentCursor
        {
            get
            {
                return _current;
            }
            set
            {                
                _current = value;
                OnPropertyChanged();
                if (isLoading)
                {
                    if (DrawingCanvas.IsMouseDirectlyOver)
                    {
                        _current = Cursors.Wait;
                        OnPropertyChanged();
                    }
                    return;
                }
                if (value != Cursors.None)
                {
                    _hov.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _hov.Visibility = Visibility.Visible;
                }
            }
        }
        Canvas DrawingCanvas { get; set; }

        double _xs = -2;
        public double XStart { get { return _xs; } private set { _xs = value; OnPropertyChanged(); } }

        double _ys = -2;
        public double YStart { get { return _ys; } private set { _ys = value; OnPropertyChanged(); OnPropertyChanged("MirroredYStart"); } }
        public double MirroredYStart { get { return -1 * _ys; } }

        double _xe = 2;
        public double XEnd { get { return _xe; } private set { _xe = value; OnPropertyChanged(); } }

        double _ye = 2;
        public double YEnd { get { return _ye; } private set { _ye = value; OnPropertyChanged(); OnPropertyChanged("MirroredYEnd"); } }
        public double MirroredYEnd { get { return -1 * _ye; } }
        int _maxiter = 500;
        public int MaxIterations { get { return _maxiter; } set { _maxiter = value; OnPropertyChanged(); } }

        int _zoom = 100;
        int ZoomVal
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (value <= 1)
                {
                    _zoom = 1;
                }
                else
                {
                    _zoom = value;
                }
                if (_hov != null)
                {
                    
                    Canvas.SetTop(_hov, Canvas.GetTop(_hov) - (_zoom - _hov.Height) / 2);
                    Canvas.SetLeft(_hov, Canvas.GetLeft(_hov) - (_zoom -_hov.Width) / 2);
                    _hov.Height = _zoom;
                    _hov.Width = _zoom;
                }
            }
        }
        
        int _DH = 800;
        public int DHeight
        {
            get
            {
                return _DH;
            }
            private set
            {
                DrawingCanvas.Height = value;
                _DH = value;
            }
        }
        int _DW = 800;
        public int DWidth
        {
            get
            {
                return _DW;
            }
            private set
            {
                DrawingCanvas.Width = value;
                _DW = value;
            }
        }
        double _rm = 1;
        public double RenderMultiplier { get { return _rm; } set { _rm = value; OnPropertyChanged(); } } 

        int RenderWidth { get { return (int)(RenderMultiplier * _DW); } }
        int RenderHeight { get { return (int)(RenderMultiplier * _DH); } }

        public string ResolutionString { get { return RenderWidth + " x " + RenderHeight; } }

        int _progress = 0;
        int CurrentProgress { get { return _progress; } set { _progress = value; OnPropertyChanged("ProgressString"); } }
        double ConvertedProgress { get { return (double)_progress / (RenderMultiplier * DHeight * RenderMultiplier * DWidth); } }
        public string ProgressString
        {
            get
            {
                if (!isLoading)
                {
                    return string.Empty;
                }
                if (ConvertedProgress <= 0.99)
                    return $"Calculating... {Math.Round(ConvertedProgress * 100, 1)}%";
                else
                    return "Rendering image...";
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        protected MandelbrotEngine()
        {
            _hov = new Border() { Width = ZoomVal, Height = ZoomVal, BorderThickness = new Thickness(2), BorderBrush = Brushes.White, Visibility = Visibility.Collapsed };
            DrawingCanvas = new Canvas();
            DrawingCanvas.Children.Add(_hov);
            DrawingCanvas.MouseDown += DrawingCanvas_MouseDown;
            DrawingCanvas.MouseEnter += DrawingCanvas_MouseEnter;
            DrawingCanvas.MouseLeave += DrawingCanvas_MouseLeave;
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseWheel += DrawingCanvas_MouseWheel;
        }
        public async Task InitializeOn(Canvas canvas, int XSize, int YSize)
        {
            canvas.Children.Add(DrawingCanvas);            
            DWidth = XSize;
            DHeight = YSize;
            
            await ReDraw();
        }
        public async Task ResetToDefaults()
        {
            RenderMultiplier = 1;
            DWidth = 800;
            DHeight = 800;
            XStart = -2;
            XEnd = 2;
            YStart = -2;
            YEnd = 2;
            MaxIterations = 500;

            await ReDraw();
        }
        async Task ReDraw()
        {
            if (isLoading)
                return;
            isLoading = true;
            ImageBrush awaited = await Task.Run(() => GetNewImage());

            await Task.Delay(100);

            DrawingCanvas.Background = awaited;

            if (DrawingCanvas.IsMouseDirectlyOver)
            {
                CurrentCursor = Cursors.None;
            }
            else
            {
                CurrentCursor = Cursors.Arrow;
            }

            OnPropertyChanged("ResolutionString");
            isLoading = false;
        }
        async Task<ImageBrush> GetNewImage()
        {
            byte[] result = await GetStream(RenderHeight,RenderWidth);

            ImageBrush toReturn = new ImageBrush(BitmapSource.Create(RenderWidth, RenderHeight, 300, 300, PixelFormats.Bgra32, null, result, RenderHeight * 4));
            toReturn.Freeze();

            return toReturn;
        }
        Task<Color> GetColorForComplexNumber(double a, double b)
        {
            double x = a;
            double y = b;
            int n = 0;
            do
            {
                double temp = x * x - y * y + a;
                y = 2 * x * y + b;
                x = temp;
                n++;
            } while (x * x + y * y < 16 && n < MaxIterations);

            Color result;
            if (n == MaxIterations)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)MaxIterations);
            }

            return Task.FromResult(result);
        }
        Task<Color> GetColorForComplexNumber(double a, double b, int MaxIter)
        {
            double x = a;
            double y = b;
            int n = 0;
            do
            {
                double temp = x * x - y * y + a;
                y = 2 * x * y + b;
                x = temp;
                n++;
            } while (x * x + y * y < 16 && n < MaxIter);

            Color result;
            if (n == MaxIter)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)MaxIter);
            }

            return Task.FromResult(result);
        }
        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, (byte)ascending, 0);
                case 1:
                    return Color.FromArgb(255, (byte)descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, (byte)ascending);
                case 3:
                    return Color.FromArgb(255, 0, (byte)descending, 255);
                case 4:
                    return Color.FromArgb(255, (byte)ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, (byte)descending);
            }
        }

        public async Task SaveImageToPath(string filename, int Resolution, int Iterations)
        {
            double xs = XStart;
            double xe = XEnd;
            double ys = YStart;
            double ye = YEnd;

            BitmapSource bitmap = BitmapSource.Create(Resolution, Resolution, 300, 300, PixelFormats.Bgra32, null, await GetStreamForImage(Resolution,Resolution,Iterations,xs,xe,ys,ye),Resolution * 4);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
        Task<byte[]> GetStream(int h, int w)
        {
            byte[] result = new byte[h * w * 4];

            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    int index = (y * w + x) * 4;
                    double convertedX = (((double)x / (w)) * (XEnd - XStart)) + XStart;
                    double convertedY = (((double)y / (h)) * (YEnd - YStart)) + YStart;
                    Color computed = GetColorForComplexNumber(convertedX, convertedY).Result;
                    result[index] = computed.B;
                    result[index + 1] = computed.G;
                    result[index + 2] = computed.R;
                    result[index + 3] = computed.A;
                    CurrentProgress += 1;
                }
            });

            return Task.FromResult(result);

        }
        Task<byte[]> GetStreamForImage(int h, int w, int Iterations, double XStart, double XEnd, double YStart, double YEnd) // so saving can happen in the background while using the app
        {
            byte[] result = new byte[h * w * 4];

            for (int y = 0; y < h; y++)
            {
                for (int x = y % 2; x < w; x+= 2)
                {
                    int index = (y * w + x) * 4;
                    double convertedX = (((double)x / (w)) * (XEnd - XStart)) + XStart;
                    double convertedY = (((double)y / (h)) * (YEnd - YStart)) + YStart;
                    Color computed = GetColorForComplexNumber(convertedX, convertedY, Iterations).Result;
                    result[index] = computed.B;
                    result[index + 1] = computed.G;
                    result[index + 2] = computed.R;
                    result[index + 3] = computed.A;
                }               
            }

            // checkerboard upscale
            for (int y = 0; y < h; y++)
            {
                for (int x = 1 - (y % 2); x < w; x += 2)
                {
                    int index = (y * w + x) * 4;
                    List<int> neighborstoaverage = new List<int>();
                    int leftindex = (y * w + x - 1) * 4;
                    if (x != 0)
                    {
                        neighborstoaverage.Add(leftindex);
                    }
                    int rightindex = (y * w + x + 1) * 4;
                    if (x != w - 1)
                    {
                        neighborstoaverage.Add(rightindex);
                    }
                    
                    int bottomindex = ((y + 1) * h + x) * 4;
                    if (y != h - 1)
                    {
                        neighborstoaverage.Add(bottomindex);
                    }
                    int topindex = ((y - 1) * h + x) * 4;
                    if(y != 0)
                    {
                        neighborstoaverage.Add(topindex);
                    }
                    
                    int newB = 0;
                    foreach(int ind  in neighborstoaverage)
                    {
                        newB += result[ind];
                    }
                    newB /= neighborstoaverage.Count;

                    int newG = 0;
                    foreach (int ind in neighborstoaverage)
                    {
                        newG += result[ind + 1];
                    }
                    newG /= neighborstoaverage.Count;

                    int newR = 0;
                    foreach (int ind in neighborstoaverage)
                    {
                        newR += result[ind + 2];
                    }
                    newR /= neighborstoaverage.Count;

                    int newA = 0;
                    foreach (int ind in neighborstoaverage)
                    {
                        newA += result[ind + 3];
                    }
                    newA /= neighborstoaverage.Count;

                    Color computed = Color.FromArgb((byte)newA, (byte)newR, (byte)newG, (byte)newB);
                    result[index] = computed.B;
                    result[index + 1] = computed.G;
                    result[index + 2] = computed.R;
                    result[index + 3] = computed.A;

                                   
                }
            }

            return Task.FromResult(result);
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isLoading)
                return;
            Point current = e.GetPosition(DrawingCanvas);
            Canvas.SetTop(_hov, current.Y - _hov.Height / 2);
            Canvas.SetLeft(_hov, current.X - _hov.Width / 2);
        }
        private void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomVal += 3;
            }
            else if (e.Delta < 0)
            {
                ZoomVal -= 3;
            }
        }
        private async void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isLoading)
                return;
            if (RenderHeight >= 8000 || RenderWidth >= 8000)
            {
                MessageBoxResult warning = MessageBox.Show("You are about to begin calculating a massive resultion image, this might take a while, are you sure you want to do it?", "Warning!", MessageBoxButton.YesNo);
                if (warning == MessageBoxResult.No)
                {
                    return;
                }
            }
            Canvas clicked = (Canvas)sender;

            Point p = e.GetPosition(clicked);
            double newXstart = (((double)p.X - (double)ZoomVal / 2) / (double)clicked.Width) * (XEnd - XStart) + XStart;
            double newXend = (((double)p.X + (double)ZoomVal / 2) / (double)clicked.Width) * (XEnd - XStart) + XStart;
            double newYstart = (((double)p.Y - (double)ZoomVal / 2) / (double)clicked.Height) * (YEnd - YStart) + YStart;
            double newYend = (((double)p.Y + (double)ZoomVal / 2) / (double)clicked.Height) * (YEnd - YStart) + YStart;

            XStart = newXstart;
            XEnd = newXend;
            YStart = newYstart;
            YEnd = newYend;

            await ReDraw();
        }

        private void DrawingCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            CurrentCursor = Cursors.None;
        }

        private void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            CurrentCursor = Cursors.Arrow;
        }
    }
}
