using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace DampingFinder
{
    public class Frequency
    {
        private alglib.complex[] _inverseFFT = new alglib.complex[ObjectManager.CurrentFile.ComplexFFT.Length];

        /// <summary>
        /// Обратное преобразрвание Фурье.
        /// </summary>
        public alglib.complex[] InverseFFT { get { return this._inverseFFT; } }



        /// <summary>
        /// Огибающая к обратному преобразованию Фурье.
        /// </summary>
        public int Graph { get; set; }



        /// <summary>
        /// График демпфирования.
        /// </summary>
        public int DampingGraph { get; set; }



        /// <summary>
        /// Коэффициент демпфирования.
        /// </summary>
        public int DampingCoefficient { get; set; }



        /// <summary>
        /// Нужно ли считать ОПФ и строить график.
        /// </summary>
        public bool NeedToShow { get; set; }



        /// <summary>
        /// Возможно выбирать ширину вручную.
        /// </summary>
        public bool isManual { get; set; }



        /// <summary>
        /// Частота.
        /// </summary>
        public int FrequencyNumber { get; set; }



        /// <summary>
        /// Ширина "окна".
        /// </summary>
        public int WindowWidth { get; set; }


        public Frequency(int freq, int width) 
        {
            FrequencyNumber = freq;
            WindowWidth = width;
        }


        // Создаем для удобства список из массива, который прошел обратное БПФ.
        List<double> fftArray = new List<double>();
        public Canvas getInverseFFT()
        {            
            int startPos = FrequencyNumber - (WindowWidth / 2);
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

                xPos++;
                arrayPosition += batch;
            }
            while (arrayPosition < fftArray.Count);

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = canvasHeight;

            return result;
        }


        public Canvas getGraph()
        {
            Canvas result = new Canvas();

            int xPos = 0;

            List<List<double>> waves = new List<List<double>>();
            List<double> wave = new List<double>();

            // Достаем положительные волны.
            for (int i = 0; i < this.fftArray.Count; i++)
            {
                if (this.fftArray[i] > 0)
                    wave.Add(this.fftArray[i]);
                else
                    if (wave.Count > 0)
                    {
                        waves.Add(new List<double>(wave));
                        wave.Clear();
                    }
            }


            // Ищем максимумы волн.
            List<double> graph = new List<double>();
            for (int i = 0; i < waves.Count; i++)
            {
                double max = 0;
                for (int k = 0; k < waves[i].Count; k++)
                    max = Math.Max(waves[i][k], max);
                graph.Add(max);
            }

            // Масштаб.
            double m = 0;
            for (int i = 0; i < graph.Count; i++)
                m = Math.Max(graph[i], m);
            double scale = 280 / m;

            // Рисуем график.
            for (int j = 0; j < graph.Count; j+=2)
            {
                Line line = new Line();
                line.X1 = xPos;
                line.Y1 = 300 - graph[j] * scale;
                line.X2 = xPos + 1;
                line.Y2 = 300 - graph[j + 1] * scale;

                line.StrokeThickness = 1;
                line.Stroke = Brushes.Red;
                result.Children.Add(line);

                xPos += 2;

                if (j > 3000)
                    break;
            }

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = 300;

            return result;
        }
    }
}
