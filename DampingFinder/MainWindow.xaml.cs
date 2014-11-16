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
using System.Windows.Media.Animation;
using System.IO;
using System.ComponentModel;

namespace DampingFinder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SOLVE_FFT = 1;
        const int SOLVE_INVERSE_FFT = 2;
        const int SOLVE_DAMPING_GRAPH = 3;

        WavFile currentFile;            // Текущий открытый файл.        
        Grid currentStepGrid;           // Текущая открытая форма, соответствующая нажатой кнопке.
        int currentStep = 1;            // Текущий открытый шаг.
        Button currentStepButton;       // Текущая нажатая кнопка.
        BackgroundWorker worker;

        // Визард.
        int wizardCurrentStep = 1;      // Последний открытый шаг.
        bool[] wizardOpenedSteps = new bool[5] { true, false, false, false, false }; 

        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
        }
        
        // Обработчик кнопки "открыть". Загружаем звуковой файл и достаем из него всю необходимую о нем инфу.
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Звуковые файлы WAVE (*.wav)|*.wav";
            dialog.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            dialog.Multiselect = false;
            dialog.Title = "Выберите файл";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Создаем объект файла и цепляем ссылку в currentFile.
                ObjectManager.CurrentFile = new WavFile(dialog.FileName);

                // Отрисовываем waveform и выводим на экран
                scrollWaveform.Content = ObjectManager.CurrentFile.getWaveform();

                // Отображаем информацию о файле.
                showFileInfo(ObjectManager.CurrentFile);

                // Закрываем начальный "экран" с выбором файла.
                gridIntro.Visibility = System.Windows.Visibility.Collapsed;
                
                // Указываем текущий открытый "экран".
                currentStepGrid = gridWaveform;

                wizardOpenNextStep();
                
                // Отрисовываем БПФ.
                ObjectManager.CurrentFile.FFT();
                gridGraphFFT.Children.Add(new GraphFFT(ObjectManager.CurrentFile.modulFFT));
            }       
        }

        // Показывает всю инфу о файле.
        private void showFileInfo(WavFile file)
        {
            labelFileName.Content = System.IO.Path.GetFileName(file.PathToFile);
            labelFilePath.Content = file.PathToFile;
            labelFileSize.Content = file.Length;
            labelFileDate.Content = System.IO.File.GetCreationTime(file.PathToFile);

            labelFileChanels.Content = file.Channels == 2 ? "2 (stereo)" : "1 (mono)";
            labelFileFreq.Content = file.SampleRate;
            labelFileDepth.Content = file.BitsPerSample;
            labelFileBitrate.Content = file.Bitrate;
        }


        // Делает доступным следующий шаг.
        private void wizardOpenNextStep()
        {
            if (wizardCurrentStep >= 5)
                return;

            wizardCurrentStep++;
            wizardOpenedSteps[wizardCurrentStep - 1] = true;

            switch (wizardCurrentStep)
            {
                case 1:
                    {
                        openStep(1);
                        break;
                    }
                case 2:
                    {
                        rectR01.Fill = Colors.Blue;
                        rectL02.Fill = Colors.Blue;
                        el02.Fill = Colors.Blue;
                        stepLabel02.Foreground = Brushes.White;
                        openStep(2);
                        break;
                    }
                case 3:
                    {
                        //runAsync(SOLVE_FFT);
                        //gridCanvasFFT.Children.Add(currentFile.FFT());
                        rectR02.Fill = Colors.Blue;
                        rectL03.Fill = Colors.Blue;
                        el03.Fill = Colors.Blue;
                        stepLabel03.Foreground = Brushes.White;
                        openStep(3);
                        break;
                    }
                case 4:
                    {
                        //runAsync(SOLVE_INVERSE_FFT);
                        // временное решение. вместо одного окна будет массив.
                        //int t = (int)gridChooserFFT.ColumnDefinitions[0].ActualWidth;
                        //int t2 = (int)gridChooserFFT.ColumnDefinitions[1].ActualWidth;
                        //if (currentFile != null)
                        //    scrollInverseFFT.Content = currentFile.inverseFFT(t, t2);                        
                        rectR03.Fill = Colors.Blue;
                        rectL04.Fill = Colors.Blue;
                        el04.Fill = Colors.Blue;
                        stepLabel04.Foreground = Brushes.White;
                        openStep(4);
                        break;
                    }
                case 5:
                    {
                        //scrollGraph.Content = currentFile.envelopeGraph();
                        rectR04.Fill = Colors.Blue;
                        rectL05.Fill = Colors.Blue;
                        el05.Fill = Colors.Blue;
                        stepLabel05.Foreground = Brushes.White;
                        openStep(5);
                        break;
                    }
            }
        }



        // Открывает указанный шаг.
        private void openStep(int step)
        {
            // Если кнопка еще залочена - ничего не делаем.
            if (!wizardOpenedSteps[step - 1])
                return;

            // Говорим, какой теперь шаг открыт.
            currentStep = step;

            // Передвигает указатель под шагами.
            Grid.SetColumn(stepPointer, step);

            // ТУ ДУ: открытие самого шага, экрана.
            currentStepGrid.Visibility = System.Windows.Visibility.Collapsed;
            switch (step)
            {
                case 1:
                    {
                        MessageBox.Show("Пока не будем открывать интро. В идеале подымаем алерт о том, что все очиститься.");
                        break;
                    }
                case 2:
                    {
                        labelHeader.Content = "WAVEFORM";
                        gridWaveform.Visibility = System.Windows.Visibility.Visible;
                        currentStepGrid = gridWaveform;
                        break;
                    }
                case 3:
                    {
                        labelHeader.Content = "FFT";
                        gridFFT.Visibility = System.Windows.Visibility.Visible;
                        currentStepGrid = gridFFT;
                        break;
                    }
                case 4:
                    {
                        labelHeader.Content = "INVERSE FFT";
                        gridInverseFFT.Visibility = System.Windows.Visibility.Visible;
                        currentStepGrid = gridInverseFFT;
                        break;
                    }
                case 5:
                    {
                        labelHeader.Content = "RESULTS";
                        gridResults.Visibility = System.Windows.Visibility.Visible;
                        currentStepGrid = gridResults;
                        break;
                    }
            }
        }

        
        // Нажатие на круглые кнопки внизу.
        private void menuNavigation(object sender, RoutedEventArgs e)
        {
            currentStepButton = (Button)sender;
            switch(currentStepButton.Tag.ToString())
            {
                case "intro":
                    {
                        openStep(1);
                        break;
                    }
                case "waveform":
                    {
                        openStep(2);
                        break;
                    }
                case "FFT":
                    {
                        openStep(3);
                        break;
                    }
                case "inverseFFT":
                    {
                        openStep(4);
                        break;
                    }
                case "results":
                    {
                        openStep(5);
                        break;
                    }
            }
        }


        // Нажатие на кнопки "К след шагу" или "К пред шагу"
        private void nextPrevNavigation(object sender, RoutedEventArgs e)
        {
            switch(((Button)sender).Tag.ToString())
            {
                case "prev":
                    {
                        if(currentStep > 1)
                            openStep(currentStep - 1);
                        break;
                    }
                case "next":
                    {
                        // Если след. шаг еще залоченый, то открываем его визардом.
                        if(currentStep == wizardCurrentStep)
                            wizardOpenNextStep();
                        break;
                    }
            }
        }


        private void startProcessing()
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromMilliseconds(1400),
                EasingFunction = qa,
                RepeatBehavior = RepeatBehavior.Forever
            };

            RotateTransform rotate = new RotateTransform();
            rotate.BeginAnimation(RotateTransform.AngleProperty, da);
            rectLoader.RenderTransform = rotate;
            loader.Visibility = System.Windows.Visibility.Visible;
        }

        private void stopProcessing()
        {
            loader.Visibility = System.Windows.Visibility.Collapsed;
        }



        //private void animateMenuContent(Grid showThisGrid)
        //{
        //    currentStepGrid.Visibility = System.Windows.Visibility.Collapsed;
        //    showThisGrid.Visibility = System.Windows.Visibility.Visible;

        //    DoubleAnimation da = new DoubleAnimation()
        //    {
        //        From = 80,
        //        To = 0,
        //        Duration = TimeSpan.FromMilliseconds(800),
        //        EasingFunction = qa
        //    };
        //    DoubleAnimation oa = new DoubleAnimation()
        //    {
        //        From = 0,
        //        To = 1,
        //        Duration = TimeSpan.FromMilliseconds(600),
        //        EasingFunction = qa
        //    };

        //    for (int i = 0; i < showThisGrid.Children.Count; i++)
        //    {
        //        TranslateTransform transform = new TranslateTransform();
        //        da.BeginTime = TimeSpan.FromMilliseconds(i * 50);

        //        transform.BeginAnimation(TranslateTransform.YProperty, da);
        //        showThisGrid.Children[i].BeginAnimation(OpacityProperty, oa);
        //        showThisGrid.Children[i].RenderTransform = transform;
        //    }

        //    currentStepGrid = showThisGrid;
        //}

        QuarticEase qa = new QuarticEase()
        {
            EasingMode = EasingMode.EaseInOut
        };

    }
}
