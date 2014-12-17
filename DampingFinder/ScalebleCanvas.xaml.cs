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
    /// Логика взаимодействия для ScalebleCanvas.xaml
    /// </summary>
    public partial class ScalebleCanvas : UserControl
    {
        private int _scaleXPos = 8;
        private double[] SCALE_ARRAY = new double[14] { 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.5, 2, 3, 4, 5 };

        public ScalebleCanvas(Canvas canv)
        {
            InitializeComponent();
            if (gridCanvas != null)
                gridCanvas.Children.Add(canv);
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
    }
}
