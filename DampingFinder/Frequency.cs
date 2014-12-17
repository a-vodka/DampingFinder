using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace DampingFinder
{
    public class Frequency
    {
        private alglib.complex[] _inverseFFT = new alglib.complex[ObjectManager.CurrentFile.ComplexFFT.Length];


        /// <summary>
        /// Нужно ли считать ОПФ и строить график.
        /// </summary>
        public bool NeedToShow { get; set; }

        /// <summary>
        /// Возможно выбирать ширину вручную.
        /// </summary>
        public bool isManual { get; set; }

        /// <summary>
        /// Частота (например 90 Гц).
        /// </summary>
        public int FrequencyValue { get; set; }

        /// <summary>
        /// Ширина "окна".
        /// </summary>
        public int WindowWidth { get; set; }


        public Frequency(int freq, int width, bool ismanual = true, bool needtoshow = true) 
        {
            FrequencyValue = freq;
            WindowWidth = width;
            NeedToShow = needtoshow;
            isManual = ismanual;
        }


        // Создаем для удобства список из массива, который прошел обратное БПФ.
        List<double> fftArray = new List<double>();
        List<double> fftArrayCompres = new List<double>();
        /// <summary>
        /// Строит график обратного БПФ.
        /// </summary>
        /// <returns></returns>
        public Canvas getInverseFFT()
        {            
            int startPos = FrequencyValue - (WindowWidth / 2);
            int endPos = startPos + WindowWidth;
            for (int i = 0; i < ObjectManager.CurrentFile.ComplexFFT.Length; i++)
            {
                if (i > startPos && i < endPos)
                    _inverseFFT[i] = ObjectManager.CurrentFile.ComplexFFT[i];
                else
                    _inverseFFT[i] = new alglib.complex(0, 0);
            }

            // делаем обратное БПФ.
            alglib.fftc1dinv(ref _inverseFFT);

            
            for (int i = 0; i < _inverseFFT.Length; i++)
                fftArray.Add((double)_inverseFFT[i].x);

            // Рисуем полученный результат.
            int xPos = 0;
            int arrayPosition = 0;
            double max = 0;
            int batch = 100;
            int canvasHeight = 300;
            List<double> arr = new List<double>();
            Canvas result = new Canvas();

            double mx = 0;
            for (int t = 0; t < fftArray.Count; t++)
                mx = Math.Max(fftArray[t], mx);
            double scale = (canvasHeight / 2) / mx;

            do
            {
                // 
                if ((arrayPosition + batch) > fftArray.Count)
                    batch = fftArray.Count - arrayPosition;
                arr = fftArray.GetRange(arrayPosition, batch);
                max = 0;
                for (int i = 0; i < batch; i++)
                {
                    if (Math.Abs(arr[i]) > Math.Abs(max))
                        max = arr[i];
                }

                Line line = new Line();
                line.X1 = xPos;
                line.Y1 = canvasHeight / 2;
                line.X2 = xPos;
                line.Y2 = canvasHeight / 2 - max * scale;

                line.StrokeThickness = 1;
                line.Stroke = Brushes.Red;
                result.Children.Add(line);

                fftArrayCompres.Add(max);

                xPos++;
                arrayPosition += batch;
            }
            while (arrayPosition < fftArray.Count);

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = canvasHeight;

            return result;
        }



        List<System.Windows.Point> pointsList = new List<System.Windows.Point>();
        /// <summary>
        /// Строит график огибающей обратного БПФ.
        /// </summary>
        /// <returns></returns>
        public Canvas getGraph()
        {
            Canvas result = new Canvas();
            
            List<Dictionary<double, int>> waves = new List<Dictionary<double, int>>();
            Dictionary<double, int> wave = new Dictionary<double, int>();

            // Достаем положительные волны.
            for (int i = 0; i < this.fftArrayCompres.Count; i++)
            {
                if (this.fftArrayCompres[i] > 0)
                    wave.Add(this.fftArrayCompres[i], i);
                else
                    if (wave.Count > 0)
                    {
                        waves.Add(new Dictionary<double, int>(wave));
                        wave.Clear();
                    }
            }

            // Ищем максимумы волн.
            Dictionary<double, int> graph = new Dictionary<double, int>();
            foreach (var item in waves)            
            {
                var max = item.Keys.Max();
                graph.Add(max, item[max]);
            }

            // Масштаб.
            double m = graph.Keys.Max();
            double scale = 280 / m;

            // Рисуем график.
            int xPos = 0;

            // Получаем список точек.
            pointsList.Clear();
            foreach (var item in graph)
            {
                System.Windows.Point point = new System.Windows.Point();
                point.X = item.Value;
                point.Y = 300 - item.Key * scale;
                pointsList.Add(point);

                xPos = item.Value;
            }         

            for (int i = 1; i < pointsList.Count; i++)
            {
                Line line = new Line();
                line.X1 = pointsList[i - 1].X;
                line.Y1 = pointsList[i - 1].Y;
                line.X2 = pointsList[i].X;
                line.Y2 = pointsList[i].Y;

                line.StrokeThickness = 1;
                line.Stroke = Brushes.Red;
                result.Children.Add(line);
            }

            // Рисуем каждую точку.
            foreach (var item in pointsList)
            {
                Ellipse el = new Ellipse();
                el.StrokeThickness = 0;
                el.Fill = Brushes.Red;
                el.Width = 6;
                el.Height = 6;
                el.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                el.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                el.Margin = new System.Windows.Thickness(item.X - 3, item.Y - 3, 0, 0);
                result.Children.Add(el);
            }

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = 300;

            return result;
        }


        /// <summary>
        /// Строит график декремента колебаний по точкам огибающей 
        /// (зависимость от времени)
        /// </summary>
        /// <returns></returns>
        public Canvas dampingGraph()
        {
            Canvas result = new Canvas();
            int xPos = 0;
            
            // Считаем декремент для каждой точки.
            List<Point> dampPoints = new List<Point>();
            int t = 1;
            double pointYInZero = pointsList[0].Y;
            foreach (Point point in pointsList)
            {
                Point p = new Point();
                p.X = point.X;
                
                double beta = (-1 / (double)t) * Math.Log(point.Y / pointYInZero);
                double decr = 2 * Math.PI * beta / (Math.Sqrt(1 - beta * beta));
                p.Y = decr;

                dampPoints.Add(p);

                t++;
                xPos = (int)point.X;
            }

            // Ищем максимальное и минимальное значение У, и в каких точках они лежат.
            List<Point> dampPointsScaled = new List<Point>();
            int maxIndex = 0;
            int minIndex = 0;
            double max = 0;
            double min = 0;
            for(int i = 0; i < dampPoints.Count; i++)
            {
                if(dampPoints[i].Y > max)
                {
                    max = dampPoints[i].Y;
                    maxIndex = i;
                }

                if (dampPoints[i].Y < min)
                {
                    min = dampPoints[i].Y;
                    minIndex = i;
                }
            }

            // Считаем коэфициент скалирования и скалируем все точки.
            double scale = 140 / (Math.Abs(dampPoints[maxIndex].Y) > Math.Abs(dampPoints[minIndex].Y) ? Math.Abs(dampPoints[maxIndex].Y) : Math.Abs(dampPoints[minIndex].Y));
            foreach (Point p in dampPoints)
            {
                dampPointsScaled.Add(new Point(p.X, p.Y * scale * (-1)));
            }

            // Рисуем линии графика.
            for (int i = 1; i < dampPointsScaled.Count; i++)
            {
                Line line = new Line();
                line.X1 = dampPointsScaled[i - 1].X;
                line.Y1 = 140 + dampPointsScaled[i - 1].Y;
                line.X2 = dampPointsScaled[i].X;
                line.Y2 = 140 + dampPointsScaled[i].Y;

                line.StrokeThickness = 1;
                line.Stroke = Brushes.Red;
                result.Children.Add(line);
            }

            // Рисуем каждую точку.
            for (int i = 0; i < dampPointsScaled.Count; i++ )
            {
                var item = dampPointsScaled[i];
            
                Ellipse el = new Ellipse();
                el.StrokeThickness = 0;
                el.Fill = Brushes.Red;
                el.Width = 6;
                el.Height = 6;
                el.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                el.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                el.Margin = new System.Windows.Thickness(item.X - 3, 140 + item.Y - 3, 0, 0);
                el.ToolTip = "Value = " + dampPoints[i].Y;
                result.Children.Add(el);
            }

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = 300;

            return result;
        }




        /// <summary>
        /// Строит график декремента колебаний по точкам огибающей
        /// (зависимость от амплитуды)
        /// </summary>
        /// <returns></returns>
        public Canvas dampingGraphAmp()
        {
            Canvas result = new Canvas();
            int xPos = 0;


            List<Point> pointsListSorted = new List<Point>();
            foreach (Point p in pointsList)            
                pointsListSorted.Add(new Point(p.Y, p.X));

            pointsListSorted = pointsListSorted.OrderBy(p => p.X).ToList();


            // Считаем декремент для каждой точки.
            List<Point> dampPoints = new List<Point>();
            int t = 1;
            double pointYInZero = pointsListSorted[1].Y;
            foreach (Point point in pointsListSorted)
            {
                Point p = new Point();
                p.X = point.X;

                double beta = (-1 / (double)t) * Math.Log(point.Y / pointYInZero);
                double decr = 2 * Math.PI * beta / (Math.Sqrt(1 - beta * beta));
                p.Y = decr;

                dampPoints.Add(p);

                t++;
                xPos = (int)point.X;
            }

            // Ищем максимальное и минимальное значение У, и в каких точках они лежат.
            List<Point> dampPointsScaled = new List<Point>();
            int maxIndex = 0;
            int minIndex = 0;
            double max = 0;
            double min = 0;
            for (int i = 0; i < dampPoints.Count; i++)
            {
                if (dampPoints[i].Y > max)
                {
                    max = dampPoints[i].Y;
                    maxIndex = i;
                }

                if (dampPoints[i].Y < min)
                {
                    min = dampPoints[i].Y;
                    minIndex = i;
                }
            }

            // Считаем коэфициент скалирования и скалируем все точки.
            double scale = 140 / (Math.Abs(dampPoints[maxIndex].Y) > Math.Abs(dampPoints[minIndex].Y) ? Math.Abs(dampPoints[maxIndex].Y) : Math.Abs(dampPoints[minIndex].Y));
            foreach (Point p in dampPoints)
            {
                dampPointsScaled.Add(new Point(p.X, p.Y * scale * (-1)));
            }

            // Рисуем линии графика.
            for (int i = 1; i < dampPointsScaled.Count; i++)
            {
                try
                {
                    Line line = new Line();
                    line.X1 = dampPointsScaled[i - 1].X;
                    line.Y1 = 140 + dampPointsScaled[i - 1].Y;
                    line.X2 = dampPointsScaled[i].X;
                    line.Y2 = 140 + dampPointsScaled[i].Y;

                    line.StrokeThickness = 1;
                    line.Stroke = Brushes.Red;
                    result.Children.Add(line);
                }
                catch { }
            }

            // Рисуем каждую точку.
            for (int i = 0; i < dampPointsScaled.Count; i++)
            {
                try
                {
                    var item = dampPointsScaled[i];

                    Ellipse el = new Ellipse();
                    el.StrokeThickness = 0;
                    el.Fill = Brushes.Red;
                    el.Width = 6;
                    el.Height = 6;
                    el.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    el.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    el.Margin = new System.Windows.Thickness(item.X - 3, 140 + item.Y - 3, 0, 0);
                    el.ToolTip = "Value = " + dampPoints[i].Y;
                    result.Children.Add(el);
                }
                catch { }
            }

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = 300;

            return result;
        }

    }
}
