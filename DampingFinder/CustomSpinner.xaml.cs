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
    /// Логика взаимодействия для CustomSpinner.xaml
    /// </summary>
    public partial class CustomSpinner : UserControl
    {        
        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value.ToString());
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(string),
                typeof(CustomSpinner),
                new PropertyMetadata(string.Empty));


        public CustomSpinner()
        {
            InitializeComponent();
        }

        private void numericText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void btnInc_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(numericText.Text))
                return;

            int value = Convert.ToInt32(numericText.Text);
            value++;
            numericText.Text = value.ToString();
        }

        private void btnDec_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(numericText.Text))
                return;

            int value = Convert.ToInt32(numericText.Text);
            if (value <= 1)
                return;

            value--;
            numericText.Text = value.ToString();
        }
    }
}
