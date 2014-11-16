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
        private alglib.complex[] _fftComplex;                       // массив с БПФ.
        List<float> fftArray = new List<float>();                   // список с модулем БПФ.
        List<Frequency> Frequensys = new List<Frequency>();         // список выделенных частот.


        // Свойства
        public int BitsPerSample { get; private set; }
        public int SampleRate { get; private set; }
        public string PathToFile { get; private set; }
        public int Channels { get; private set; }
        public int Bitrate { get; private set; }
        public int Length { get; private set; }
        public float[] Data { get; private set; }

        public alglib.complex[] ComplexFFT 
        {
            get { return this._fftComplex; }
        }

        public List<float> modulFFT
        {
            get { return this.fftArray; }
        }

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

                    if (BitsPerSample != 32)
                    {
                        System.Windows.Forms.MessageBox.Show(BitsPerSample + "BPS doesn't support! Sorry");
                        return;
                    }

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
            //if (Channels == 1)
            //    return null;

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


        
        public void FFT()
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
            for (int i = 0; i < _fftComplex.Length; i++)
                fftArray.Add((float)Math.Sqrt(_fftComplex[i].x * _fftComplex[i].x + _fftComplex[i].y * _fftComplex[i].y));
        }

        
    }
}
