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
    /// Логика взаимодействия для ToolTip.xaml
    /// </summary>
    public partial class ToolTipWizardControl : UserControl
    {
        public string Title
        {            
            set { TitleControl.Content = value; }
        }

        public string Message 
        { 
            set { MessageControl.Text = value; }
        }

        public ToolTipWizardControl()
        {
            InitializeComponent();
        }

    }
}
