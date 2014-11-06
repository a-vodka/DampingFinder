using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace DampingFinder
{
    class WavFile
    {
        // Поля
        private alglib.complex[] _fftComplex;
        private alglib.complex[] _inverseFFT;

        // Свойства
        public int BitsPerSample { get; private set; }
        public int SampleRate { get; private set; }
        public string PathToFile { get; private set; }
        public int Channels { get; private set; }
        public int Bitrate { get; private set; }
        public int Length { get; private set; }
        public float[] Data { get; private set; }

        // Конструктор
        public WavFile(string path)
        {
            // Запоминаем путь к файлу.
            PathToFile = path;

            // Открываем стрим для чтения файла и парсим его.
            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    //Read the wave file header from the buffer. 
                    int chunkID = reader.ReadInt32();
                    int fileLength = reader.ReadInt32();
                    int riffType = reader.ReadInt32();
                    int fmtID = reader.ReadInt32();
                    int fmtSize = reader.ReadInt32();
                    int fmtCode = reader.ReadInt16();
                    Channels = reader.ReadInt16();
                    SampleRate = reader.ReadInt32();
                    Bitrate = reader.ReadInt32();
                    int fmtBlockAlign = reader.ReadInt16();
                    BitsPerSample = reader.ReadInt16();

                    if (fmtSize == 18)
                    {
                        // Read any extra values
                        int fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(fmtExtraSize);
                    }

                    int dataID = reader.ReadInt32();
                    Length = reader.ReadInt32();

                    // Записываем в массив непосредственно данные WAV файла.
                    byte[] bData = reader.ReadBytes(Length);

                    // Указатель на позицию в массиве байтов. Показывает от куда начинать читать след. значение.
                    int bDataPosition = 0;

                    // Глубина звука в байтах.
                    int soundDepth = BitsPerSample / 8;

                    // Инициализация массива с числовыми значениями сигнала.
                    Data = new float[Length / soundDepth];

                    // Читаем из массива по soundDepth байт и конвертируем их во float.
                    for (int i = 0; i < Length / soundDepth; i++)
                    {
                        Data[i] = BitConverter.ToSingle(bData, bDataPosition);
                        bDataPosition += soundDepth;
                    }
                }
            }
        }


        // Отрисовка звука. Возвращает канвас с рисунком.
        public Canvas getWaveform()
        {
            // Создаем холст, на котором будем рисовать.
            Canvas result = new Canvas();

            // Если моно - пока ничего не делаем. По-умолчанию у нас 2 канала.
            if (Channels == 1)
                return null;

            // Масштаб
            double mx = 0;
            for (int t = 0; t < Data.Length; t++)
                mx = Math.Max(Data[t], mx);
            double scale = 120 / mx;


            // разделить на два массива. левый и правый канал.
            float[] leftChanel = new float[Data.Length / 2];
            float[] rightChanel = new float[Data.Length / 2];
            int k = 0;
            for (int i = 0; i < Data.Length; i+= 2)
            {
                leftChanel[k] = Data[i];
                rightChanel[k] = Data[i + 1];
                k++;
            }

            // Считаем количество сэмплов в файле.
            int samples = Length / (Channels * BitsPerSample / 8);
            int xPos = 0;
            int arrayPosition = 0;
            float maxLeft = 0;
            float maxRight = 0;

            int batch = 100;
            float[] arrLeft = new float[batch];
            float[] arrRight = new float[batch];

            do
            {
                // Обработка левого канала
                arrLeft = getBatchArray(arrayPosition, batch);
                maxLeft = 0;
                for (int i = 0; i < batch; i++)
                    maxLeft = Math.Max(Math.Abs(arrLeft[i]), maxLeft);

                // Обработка правого канала
                arrRight = getBatchArray(arrayPosition, batch);
                maxRight = 0;
                for (int i = 0; i < batch; i++)
                    maxRight = Math.Max(Math.Abs(arrRight[i]), maxRight);


                Line line = new Line();
                line.X1 = xPos;
                line.Y1 = 130 - Math.Abs(maxLeft * scale);
                line.X2 = xPos;
                line.Y2 = 130 + Math.Abs(maxRight * scale);

                line.StrokeThickness = 1;
                line.Stroke = Brushes.Red;
                result.Children.Add(line);

                xPos++;
                arrayPosition += batch;
            }
            while (arrayPosition <= samples);

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = 260;

            return result;
        }


        // Метод возвращает выборку значений заданного размера.
        private float[] getBatchArray(int startPos, int length)
        {
            float[] result = new float[length];
            int position = startPos;

            if (startPos + length > Data.Length)
                length = Data.Length - (startPos + length);

            for (int i = 0; i < length; i++)
            {
                result[i] = Data[position];
                position++;
            }
            return result;
        }


        public Canvas FFT()
        {
            // проверка массива данных, он должен иметь размер 2 в степени икс.
            double dataLog = Math.Log(Data.Length, 2);
            int newDataLength = Data.Length;
            bool isDataLengthPowerOfTwo = dataLog % 1 == 0;
            List<float> dataList = new List<float>(Data);

            // проверяем, если размер массива не является результатом 2 в степени n, то добиваем его нулями до необходимого размера.
            if (!isDataLengthPowerOfTwo)
            {
                int powerOfTwo = (int)Math.Ceiling(dataLog);
                newDataLength = (int)Math.Pow(2, powerOfTwo);
                for (int k = dataList.Count; k < newDataLength; k++)
                    dataList.Add(0);
            }

            // создаем массив комплексных чисел, для подачи на БПФ.
            _fftComplex = new alglib.complex[newDataLength];
            for (int i = 0; i < newDataLength; i++)
                _fftComplex[i] = new alglib.complex(dataList[i]);

            // делаем БПФ.
            alglib.fftc1d(ref _fftComplex);

            // берем модуль от результата.
            List<float> fftArray = new List<float>();
            for (int i = 0; i < _fftComplex.Length; i++)
                fftArray.Add((float)Math.Sqrt(_fftComplex[i].x * _fftComplex[i].x + _fftComplex[i].y * _fftComplex[i].y));


            // Рисуем полученный результат.
            int xPos = 0;
            int arrayPosition = 0;
            float max = 0;
            int batch = 20;
            int canvasHeight = 300;
            List<float> arr = new List<float>();
            Canvas result = new Canvas();
            
            double mx = 0;
            for (int t = 0; t < fftArray.Count; t++)
                mx = Math.Max(fftArray[t], mx);
            double scale = canvasHeight / mx;

            
            do
            {
                // 
                if ((arrayPosition + batch) > fftArray.Count)
                    batch = fftArray.Count - arrayPosition;
                arr = fftArray.GetRange(arrayPosition, batch);
                max = 0;
                for (int i = 0; i < batch; i++)
                    max = Math.Max(Math.Abs(arr[i]), max);

                Line line = new Line();
                line.X1 = xPos;
                line.Y1 = canvasHeight;
                line.X2 = xPos;
                line.Y2 = canvasHeight - max * scale;

                line.StrokeThickness = 1;
                line.Stroke = Brushes.Red;
                result.Children.Add(line);

                
                xPos++;
                arrayPosition += batch;
            }
            while (arrayPosition < 22050);

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = canvasHeight;

            return result;
        }


        public Canvas inverseFFT(int startPos, int length)
        {
            startPos *= 20;
            length *= 20;
            // создаем массив комплексных чисел, для подачи на БПФ.
            _inverseFFT = new alglib.complex[_fftComplex.Length];
            for (int i = 0; i < _fftComplex.Length; i++)
            {
                if (i > startPos && i < startPos + length)
                    _inverseFFT[i] = _fftComplex[i];
                else
                    _inverseFFT[i] = new alglib.complex(0, 0);
            }

            // делаем обратное БПФ.
            alglib.fftc1dinv(ref _inverseFFT);

            // Создаем для удобства список из массива, который прошел обратное БПФ.
            List<float> fftArray = new List<float>();
            for (int i = 0; i < _inverseFFT.Length; i++)
            {
                fftArray.Add((float)_inverseFFT[i].x);
                //fftArray.Add((float)Math.Sqrt(_inverseFFT[i].x*_inverseFFT[i].x + _inverseFFT[i].y*_inverseFFT[i].y));
            }

           
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


        public Canvas envelopeGraph(int startPosition = 0)
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
                    max = Math.Max(waves[i][k],max);
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

                xPos+=2;

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
