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
    /// Логика взаимодействия для SliceControl.xaml
    /// </summary>
    public partial class SliceControl : UserControl
    {
        private int _scaleXPos = 8;
        private double[] SCALE_ARRAY = new double[14] { 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.5, 2, 3, 4, 5 };
        private WavFile _file;

        public SliceControl(WavFile file)
        {
            InitializeComponent();
            comboScale.SelectedIndex = _scaleXPos;
            _file = file;
            draw();
        }


        // Отрисовываем waveform.
        private void draw()
        {
            gridCanvas.Children.Clear();
            if (_file.Channels == 1)
            {
                gridCanvas.Children.Add(_file.getWaveformMono());
                return;
            }

            if (_file.Channels == 2)            
                gridCanvas.Children.Add(_file.getWaveformStereo());
        }


        // Выбираем масштаб.
        private void comboScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            scale(comboScale.SelectedIndex);
        }
        // Скалируем.
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
        // Ресайз по вращению колеса мыши.
        private void gridCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
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


        // Воспроизводим файл.
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if(_file != null)
            {
                media.Stop();
                media.Source = new Uri(_file.PathToFile);
                try 
                { 
                    media.Play();
                    btnPlay.Visibility = System.Windows.Visibility.Collapsed;
                    btnPause.Visibility = System.Windows.Visibility.Visible;
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }


        // Останавливаем.
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            try 
            { 
                media.Stop();
                btnPause.Visibility = System.Windows.Visibility.Collapsed;
                btnPlay.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }


        // Скриваем или показываем "резалку".
        private void btnVis_Click(object sender, RoutedEventArgs e)
        {
            gridSlicer.Visibility = gridSlicer.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }


        // ОБрезаем!
        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            double leftPos = gridSlicer.ColumnDefinitions[0].Width.Value;
            double rightPos = gridSlicer.ColumnDefinitions[1].Width.Value;

            int length = (int)rightPos * 100;
            int index = (int)leftPos * 100;
            int[] data = _file.Data;
            int[] result = new int[length];
            Array.Copy(data, index, result, 0, length);
            _file.Data = result;

            gridSlicer.Visibility = System.Windows.Visibility.Collapsed;
            draw();
        }


        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            btnPause.Visibility = System.Windows.Visibility.Collapsed;
            btnPlay.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
