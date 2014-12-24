using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DampingFinder
{
    /// <summary>
    /// Логика взаимодействия для GraphView.xaml
    /// </summary>
    public partial class GraphView : UserControl
    {
        private Canvas canvas = null;
        private bool isZero;
        private double horStep;
        private double verStep;
        Canvas bg;

        public GraphView(Canvas canv, double horizontalStep, double verticalStep = 0, bool isZeroInCenter = true)
        {
            InitializeComponent();
            canvas = canv;
            isZero = isZeroInCenter;
            horStep = horizontalStep;
            verStep = verticalStep;

            prepareToVisualisation();
        }

        private void prepareToVisualisation()
        {
            if (canvas == null)
                return;

            Color col = new Color();
            col.A = 0;
            canvas.Background = new SolidColorBrush(col);

            bg = new Canvas();
            bg.Width = canvas.Width + 50;
            bg.Height = canvas.Height + 50;

            // horizontal lines
            for (int i = 1; i <= 6; i++)
            {
                Line l = new Line();

                l.X1 = 50;
                l.X2 = canvas.Width + 50;
                l.Y1 = (canvas.Height / 6) * i;
                l.Y2 = (canvas.Height / 6) * i;

                l.StrokeThickness = 1;
                l.Stroke = Brushes.Gray;
                bg.Children.Add(l);

                //value
                Label lb = new Label();
                if (isZero && i == 3)
                    lb.Content = 0;
                else
                    lb.Content = Math.Round(horStep * i, 2);
                lb.Margin = new Thickness(20, (canvas.Height / 6) * i - 10, 0, 0);
                lb.Padding = new Thickness(0);
                bg.Children.Add(lb);
            }


            // vertical lines
            for (int i = 1; i <= 10; i++)
            {
                Line l = new Line();

                l.X1 = (canvas.Width / 10) * i + 50;
                l.X2 = (canvas.Width / 10) * i + 50;
                l.Y1 = 0;
                l.Y2 = canvas.Height;

                l.StrokeThickness = 1;
                l.Stroke = Brushes.Gray;
                bg.Children.Add(l);
            }



            canvas.Margin = new Thickness(50, 0, 0, 50);
            bg.Children.Add(canvas);

            scrollView.Content = bg;
        }

        public void SaveAs()
        {
            //bg.Measure()

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bg.Width, (int)bg.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(bg);

            //var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = System.IO.File.OpenWrite("d:/logo.png"))
            {
                pngEncoder.Save(fs);
            }
        }
    }
}
