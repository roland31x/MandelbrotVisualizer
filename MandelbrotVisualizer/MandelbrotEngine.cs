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
        public bool isLoading
        {
            get
            {
                return _ld;
            }
            private set
            {              
                _ld = value;
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
        int DHeight
        {
            get
            {
                return _DH;
            }
            set
            {
                DrawingCanvas.Height = value;
                _DH = value;
            }
        }
        int _DW = 800;
        int DWidth
        {
            get
            {
                return _DW;
            }
            set
            {
                DrawingCanvas.Width = value;
                _DW = value;
            }
        }
        int RenderMultiplier = 1;
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
        protected MandelbrotEngine()
        {
            _hov = new Border() { Width = ZoomVal, Height = ZoomVal, BorderThickness = new Thickness(2), BorderBrush = Brushes.White, Visibility = Visibility.Collapsed };           
        }
        public async Task InitializeOn(Canvas canvas, int XSize, int YSize)
        {
            DrawingCanvas = canvas;         
            DrawingCanvas.MouseDown += DrawingCanvas_MouseDown;
            DrawingCanvas.MouseEnter += DrawingCanvas_MouseEnter;
            DrawingCanvas.MouseLeave += DrawingCanvas_MouseLeave;
            DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            DrawingCanvas.MouseWheel += DrawingCanvas_MouseWheel;
            DWidth = XSize;
            DHeight = YSize;
            DrawingCanvas.Children.Add(_hov);

            await ReDraw();
        }
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isLoading)
                return;
            Point current = e.GetPosition(DrawingCanvas);
            Canvas.SetTop(_hov, current.Y - _hov.Height / 2);
            Canvas.SetLeft(_hov, current.X - _hov.Width / 2);
        }
        private async void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isLoading)
                return;
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
        async Task ReDraw()
        {
            if (isLoading)
                return;
            isLoading = true;
            ImageBrush awaited = await Task.Run(() => GetNewImage(DrawingCanvas));

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

            isLoading = false;
        }
        Task<ImageBrush> GetNewImage(Canvas OnCanvas)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            DrawingContext drawingContext2 = drawingVisual.RenderOpen();
            //for (int i = 0; i < DHeight; i++)
            //{
            //    for (int j = 0; j < DWidth; j++)
            //    {
            //        CurrentProgress = (double)(DHeight * i + j) / (double)(DHeight * DWidth);
            //        double ConvertedX = (((double)i / DWidth) * (XEnd - XStart)) + XStart;
            //        double ConvertedY = (((double)j / DHeight) * (YEnd - YStart)) + YStart;
            //        await SetPixel(i, j, new Complex(ConvertedX, ConvertedY), drawingContext);
            //    }
            //}

            Color[,] pixels = new Color[DHeight * RenderMultiplier, DWidth * RenderMultiplier];
            Parallel.For(0, DHeight * RenderMultiplier, i =>
            {
                for (int j = 0; j < DWidth * RenderMultiplier; j++)
                {
                    double convertedX = (((double)i / (DWidth * RenderMultiplier)) * (XEnd - XStart)) + XStart;
                    double convertedY = (((double)j / (DHeight * RenderMultiplier)) * (YEnd - YStart)) + YStart;

                    pixels[i, j] = GetColorForComplexNumber(convertedX, convertedY).Result;
                }
            });

            CurrentProgress = RenderMultiplier * DHeight * RenderMultiplier * DWidth;

            for(int i = 0; i < DHeight; i++)
            {
                for (int j = 0; j < DWidth; j ++)
                {
                    drawingContext.DrawRectangle(new SolidColorBrush(pixels[RenderMultiplier * i, RenderMultiplier * j]), null, new Rect(i, j, 1, 1));
                }
            }


            drawingContext.Close();
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)DWidth, (int)DHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.Freeze();
            ImageBrush toReturn = new ImageBrush(bmp);
            toReturn.Freeze();


            return Task.FromResult(toReturn);
        }
        Task SetPixel(int x, int y, Complex Converted, DrawingContext drawingContext)
        {

            int n = Complex.NumberOfIterations(Converted);
            Color result;
            if (n == Complex.MaxIterations)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)Complex.MaxIterations);
            }

            drawingContext.DrawRectangle(new SolidColorBrush(result), null, new Rect(x, y, 1, 1));

            return Task.CompletedTask;
        }
        Task<Color> GetColorForPixel(Complex Converted)
        {

            int n = Complex.NumberOfIterations(Converted);
            Color result;
            if (n == Complex.MaxIterations)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)Complex.MaxIterations);
            }

            CurrentProgress += 1;
            return Task.FromResult(result);
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
            } while (Math.Sqrt(x * x + y * y) < 4 && n < 1000);

            Color result;
            if (n == 1000)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)500);
            }

            CurrentProgress += 1;
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

        public async Task SaveImageToPath(string filename, int Resolution)
        {
            BitmapSource bitmap = BitmapSource.Create(Resolution, Resolution, 300, 300, PixelFormats.Bgra32, null, await GetStreamForImage(Resolution,Resolution),Resolution * 4);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        Task<byte[]> GetStreamForImage(int h, int w)
        {
            byte[] result = new byte[h * w * 4];

            for (int y = 0; y < h; y++)
            {
                for (int x = y % 2; x < w; x+= 2)
                {
                    int index = (y * w + x) * 4;
                    double convertedX = (((double)x / (w)) * (XEnd - XStart)) + XStart;
                    double convertedY = (((double)y / (h)) * (YEnd - YStart)) + YStart;
                    Color computed = GetColorForComplexNumber(convertedX, convertedY).Result;
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
    }
}
