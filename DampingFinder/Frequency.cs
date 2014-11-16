using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace DampingFinder
{
    class Frequency
    {
        private alglib.complex[] _inverseFFT;

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
        public int FrequencyN { get; set; }



        /// <summary>
        /// Ширина "окна".
        /// </summary>
        public int WindowWidth { get; set; }


        public Frequency(int freq) 
        {
            FrequencyN = freq;
        }

        public Canvas getInverseFFT()
        {
            int startPos = FrequencyN - (WindowWidth / 2);
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

            // Создаем для удобства список из массива, который прошел обратное БПФ.
            List<float> fftArray = new List<float>();
            for (int i = 0; i < _inverseFFT.Length; i++)            
                fftArray.Add((float)_inverseFFT[i].x);

            // Рисуем полученный результат.
            int xPos = 0;
            int arrayPosition = 0;
            float max = 0;
            int batch = 100;
            int canvasHeight = 300;
            List<float> arr = new List<float>();
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

            List<List<float>> waves = new List<List<float>>();
            List<float> wave = new List<float>();

            // Достаем положительные волны.
            for (int i = 0; i < _inverseFFT.Length; i++)
            {
                if (_inverseFFT[i].x > 0)
                    wave.Add((float)_inverseFFT[i].x);
                else
                    if (wave.Count > 0)
                    {
                        waves.Add(new List<float>(wave));
                        wave.Clear();
                    }
            }


            // Ищем максимумы волн.
            List<float> graph = new List<float>();
            for (int i = 0; i < waves.Count; i++)
            {
                float max = 0;
                for (int k = 0; k < waves[i].Count; k++)
                    max = Math.Max(waves[i][k], max);
                graph.Add(max);
            }

            // Масштаб.
            float m = 0;
            for (int i = 0; i < graph.Count; i++)
                m = Math.Max(graph[i], m);
            float scale = 280 / m;

            // Рисуем график.
            for (int j = 0; j < graph.Count; j++)
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
