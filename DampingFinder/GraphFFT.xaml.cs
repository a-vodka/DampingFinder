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
using System.Collections.ObjectModel;

namespace DampingFinder
{
    /// <summary>
    /// Логика взаимодействия для GraphFFT.xaml
    /// </summary>
    public partial class GraphFFT : UserControl
    {
        private List<double> _listPoints = new List<double>();
        //private List<int> _frequencysList = new List<int>();
        private ObservableCollection<Frequency> _frequencysList = new ObservableCollection<Frequency>();
        private int _scaleXPos = 8;
        private double[] SCALE_ARRAY = new double[14] { 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.5, 2, 3, 4, 5 };


        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="listPoints">Массив с модулем FFT</param>
        public GraphFFT(List<double> listPoints)
        {
            InitializeComponent();
            this._listPoints = listPoints;                           // Запоминаем массив со значениями.
            transformFFT();
            Draw();           
            comboScale.SelectedIndex = _scaleXPos;
            ObjectManager.currentFftControl = this;
            autoDetectFrequencys();
        }


        /// <summary>
        /// Приводит массив ФФТ к виду: 1 элемент массива = 1 Гц.
        /// </summary>
        private void transformFFT()
        {
            // Показывает сколько Гц в одном элементе массива.
            int t1 = ObjectManager.CurrentFile.SampleRate;
            int t2 = _listPoints.Count;
            int t3 = ObjectManager.CurrentFile.SamplesCount;
            double elementWeight = (double)ObjectManager.CurrentFile.SampleRate / (double)_listPoints.Count;
            int newSize;
            List<double> newArray = new List<double>();

            if (elementWeight < 1)
            {
                newSize = (int)Math.Round(1 / elementWeight);

                for (int i = 0; i < _listPoints.Count; i += newSize)
                {
                    if (_listPoints.Count - i < newSize)
                        newSize = _listPoints.Count - i;

                    newArray.Add(_listPoints.GetRange(i, newSize).Max());
                }

                _listPoints.Clear();
                _listPoints = newArray.GetRange(0, 22050);
            }
        }


        /// <summary>
        /// Отрисовка графика.
        /// </summary>
        private void Draw()
        {
            // Рисуем полученный результат.
            int xPos = 0;                               // Позиция по оси Х.
            int arrayPosition = 0;                      // Позиция в массиве.
            double max = 0;                              // Максимальное значение в выборке из массива.
            int batch = 1;                              // Размер выборки.
            int canvasHeight = 300;                     // Высота графика.
            List<double> arr = new List<double>();      // Выборка.
            Canvas result = new Canvas();               // Холст, на котором рисуем.

            double mx = 0;                              // Глобальный максимум в массиве.
            for (int t = 0; t < _listPoints.Count; t++)
                mx = Math.Max(_listPoints[t], mx);
            double scale = canvasHeight / mx;     // Масштаб по оси У.

            do
            {
                if ((arrayPosition + batch) > _listPoints.Count)        // Вынимаем из массива выборку.
                    batch = _listPoints.Count - arrayPosition;
                arr = _listPoints.GetRange(arrayPosition, batch);
                max = 0;
                for (int i = 0; i < batch; i++)                         // Находим локальный максимум (в выборке).
                {
                    if (Math.Abs(arr[i]) > Math.Abs(max))
                        max = arr[i];
                }

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
            while (arrayPosition < _listPoints.Count);

            result.Background = Brushes.Beige;
            result.Width = xPos;
            result.Height = canvasHeight;

            gridCanvas.Children.Clear();
            gridCanvas.Children.Add(result);
        }


        /// <summary>
        /// Автоматическое определение частот.
        /// </summary>
        private void autoDetectFrequencys()
        {
            // Ищем глобальный максимум.
            double max = 0;
            for (int i = 0; i < _listPoints.Count; i++)            
                max = Math.Max(_listPoints[i], max);
            

            // Порог для частоты. (70%)
            double freqTreshold = max * .7;

            // Ищем частоты, преодолевающие порог и записываем их в массив.
            for (int i = 10; i < _listPoints.Count; i++)            
                if (_listPoints[i] > freqTreshold)
                    _frequencysList.Add(new Frequency(i, 6));

            // Нормализуем частоты 2 раза, для точности.
            autoDetectedFreqNormalization();
            

            // Строим окна для каждой найденой частоты.
            refreshWindows();

            //ObjectManager.FrequencysSelectedList = getSelectedFrequencys();
        }


        private void autoDetectedFreqNormalization()
        {
            Dictionary<double, int> freqNormalization = new Dictionary<double, int>();
            List<Frequency> freqTempMax = new List<Frequency>();

            if (_frequencysList.Count > 1)
            {
                for (int i = 0; i < _frequencysList.Count; i++)
                {
                    if (i < _frequencysList.Count - 1 && _frequencysList[i + 1].FrequencyValue - _frequencysList[i].FrequencyValue < 10)
                    {
                        int k = i + 1;
                        while (k < _frequencysList.Count && _frequencysList[k].FrequencyValue - _frequencysList[i].FrequencyValue - freqNormalization.Count == 1)
                        {
                            freqNormalization.Add(_listPoints[_frequencysList[k].FrequencyValue], _frequencysList[k].FrequencyValue);
                            k++;
                        }

                        freqNormalization.Add(_listPoints[_frequencysList[i].FrequencyValue], _frequencysList[i].FrequencyValue);

                        freqTempMax.Add(new Frequency(freqNormalization[freqNormalization.Keys.Max()], 6));
                        freqNormalization.Clear();
                        i = k;
                        k = 0;
                    }
                    else
                        freqTempMax.Add(new Frequency(_frequencysList[i].FrequencyValue, 6));
                }

                _frequencysList.Clear();
                _frequencysList = new ObservableCollection<Frequency>(freqTempMax);
            }
        }


        /// <summary>
        /// Добавление окна
        /// </summary>
        /// <param name="freq">Частота</param>
        /// <param name="winWidth">Ширина окна</param>
        private void addWindow(int freq)
        {
            //_frequencysList.Add(freq);
            //refreshWindows();
        }


        /// <summary>
        /// Рефрешит (отрисовывает) окна.
        /// </summary>
        private void refreshWindows()
        {
            // Чистим все.
            gridPicker.ColumnDefinitions.Clear();
            gridPicker.Children.Clear();

            // странный костыль... не помню почему так написал)
            if (_frequencysList.Count < 1)
            {
                MessageBox.Show("Действие не удалось.");
                return;
            }



            // Строим разделители. У каждого окна 2 разделителя, 1 слева, 1 справа.
            for (int i = 0; i < _frequencysList.Count; i++)
            {
                // 1) не имеет смысла отображать меньше 10, 2) ломается))
                if (_frequencysList[i].FrequencyValue > 10)
                {
                    double length1 = 0;
                    double windowsSizes = 0;
                    for (int j = 0; j < gridPicker.ColumnDefinitions.Count; j++)
                        windowsSizes += gridPicker.ColumnDefinitions[j].Width.Value;

                    length1 = gridPicker.ColumnDefinitions.Count > 0 ? _frequencysList[i].FrequencyValue - windowsSizes - _frequencysList[i].WindowWidth / 2 : _frequencysList[i].FrequencyValue - _frequencysList[i].WindowWidth / 2;
                    gridPicker.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(length1, GridUnitType.Pixel) });
                    gridPicker.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(_frequencysList[i].WindowWidth, GridUnitType.Pixel) });
                }
            }

            // Считаем количество затемненных частей графика. (то что снаружи окон)
            int rectCount = gridPicker.ColumnDefinitions.Count > 2 ? (gridPicker.ColumnDefinitions.Count / 2 + 1) : 2;

            // Без этого не работет!
            double windowsSizes2 = 0;
            for (int j = 0; j < gridPicker.ColumnDefinitions.Count; j++)
                windowsSizes2 += gridPicker.ColumnDefinitions[j].Width.Value;
            gridPicker.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(_listPoints.Count - windowsSizes2, GridUnitType.Pixel) });


            // Вставляем затемняшки в контейнер.
            for (int i = 0; i < rectCount; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.Black;
                rect.Opacity = .8;
                gridPicker.Children.Add(rect);
                int posSet = i * 2;
                Grid.SetColumn(rect, posSet);
            }


            // Вставляем палки, за которые будем двигать и менять ширину окон.
            // Для лучшей работы необходимо ColumnDefinitions перевести в относительный формат.
            for (int i = 0; i < rectCount - 1; i++)
            {
                GridSplitter gsLeft = new GridSplitter();
                gsLeft.Width = 2;
                gsLeft.Background = Brushes.Orange;
                gsLeft.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                gridPicker.Children.Add(gsLeft);
                int posLeftSet = i * 2 + 1;
                Grid.SetColumn(gsLeft, posLeftSet);

                GridSplitter gsRight = new GridSplitter();
                gsRight.Width = 2;
                gsRight.Background = Brushes.Orange;
                gsRight.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                gridPicker.Children.Add(gsRight);
                int posRightSet = i * 2 + 1;
                Grid.SetColumn(gsRight, posRightSet);
                
            }
        }


        // Ресайз по вращению колеса мыши.
        private void gridCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            gridPicker.Visibility = System.Windows.Visibility.Collapsed;

            if (e.Delta > 0 && _scaleXPos < SCALE_ARRAY.Length - 1)
            {
                _scaleXPos++;
                scale(_scaleXPos);
                comboScale.SelectedIndex = _scaleXPos;
                return;
            }

            if (e.Delta < 0 && _scaleXPos > 0)
            {
                _scaleXPos--;
                scale(_scaleXPos);
                comboScale.SelectedIndex = _scaleXPos;
                return;
            }
        }


        // Масштабирует график.
        private void scale(int scaleArrayIndex)
        {
            ScaleTransform st = new ScaleTransform();

            // Меняем масштаб.
            if (scaleArrayIndex < SCALE_ARRAY.Length && scaleArrayIndex >= 0)
            {
                st.ScaleX = SCALE_ARRAY[scaleArrayIndex];
                gridContainer.LayoutTransform = st;
            }
        }


        // Видимость "пикера".
        private void pickerVisibility()
        {
            gridPicker.Visibility = gridPicker.IsVisible ? Visibility.Collapsed : Visibility.Visible;
        }


        

        // ОБработчик комбобокса.
        private void comboScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            scale(comboScale.SelectedIndex);
        }

        // Кнопка видимости пикера.
        private void btnVis_Click(object sender, RoutedEventArgs e)
        {
            pickerVisibility();
        }


        // Кнопка автоматического выделения частот.
        private void btnAutoDetect_Click(object sender, RoutedEventArgs e)
        {
            autoDetectFrequencys();
        }



        /// <summary>
        /// Возвращает список выделенных частот.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Frequency> getSelectedFrequencys()
        {
            return _frequencysList;
        }

    }
}
