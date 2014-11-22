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
using NAudio.Wave;
using NAudio.CoreAudioApi;

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
       
        Grid currentStepGrid;           // Текущая открытая форма, соответствующая нажатой кнопке.
        int currentStep = 1;            // Текущий открытый шаг.
        Button currentStepButton;       // Текущая нажатая кнопка.
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        // Визард.
        int wizardCurrentStep = 1;      // Последний открытый шаг.
        bool[] wizardOpenedSteps = new bool[5] { true, false, false, false, false }; 

        public MainWindow()
        {
            InitializeComponent();
            LoadWasapiDevicesCombo();
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
                scrollWaveform.Children.Add(new SliceControl(ObjectManager.CurrentFile));

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


        List<Frequency> fq = new List<Frequency>();
        // Делает доступным следующий шаг.
        private void wizardOpenNextStep()
        {
            if (wizardCurrentStep >= 5)
                return;

            wizardCurrentStep++;
            wizardOpenedSteps[wizardCurrentStep - 1] = true;

            if(ObjectManager.currentFftControl != null)
                fq = ObjectManager.currentFftControl.getSelectedFrequencys();

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
                        scrollInverseFFT.Content = fq[0].getInverseFFT();
                        scrollGraph.Content = fq[0].getGraph();
                        rectR03.Fill = Colors.Blue;
                        rectL04.Fill = Colors.Blue;
                        el04.Fill = Colors.Blue;
                        stepLabel04.Foreground = Brushes.White;
                        openStep(4);
                        break;
                    }
                case 5:
                    {
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
            QuarticEase qa = new QuarticEase()
            {
                EasingMode = EasingMode.EaseInOut
            };

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



        // Закрывает диалог с записью звука.
        private bool isRecording = false;
        private string recordingFilePath = String.Empty;
        private void btnRecordCANCEL_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording)
                return;

            gridRecorder.Visibility = System.Windows.Visibility.Collapsed;
        }


        // Подгружает в комбобокс доступные записывающие устройства.
        private void LoadWasapiDevicesCombo()
        {
            MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
            MMDeviceCollection deviceCol = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            List<MMDevice> devices = new List<MMDevice>();

            foreach (MMDevice device in deviceCol)
            {
                devices.Add(device);
            }

            comboSoundSource.ItemsSource = devices;
            if(comboSoundSource.Items.Count != 0)
                comboSoundSource.SelectedIndex = 0;
        }


        // Открыть менеджер записи звука.
        private void btnRecordFile_Click(object sender, RoutedEventArgs e)
        {
            gridSliceControl.Visibility = System.Windows.Visibility.Collapsed;
            gridRecordDone.Visibility = System.Windows.Visibility.Collapsed;
            gridPopUpRecorder.Visibility = System.Windows.Visibility.Visible;
            gridRecorder.Visibility = System.Windows.Visibility.Visible;
            LoadWasapiDevicesCombo();
        }


        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;
        private List<byte> wavData = new List<byte>();
        private void btnRecordStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(tbRecordFile.Text))
            {
                MessageBox.Show("Choose directory first");
                return;
            }

            btnRecordStart.IsEnabled = false;
            btnRecordStop.IsEnabled = true;

            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveFile = new WaveFileWriter(recordingFilePath, waveSource.WaveFormat);

            waveSource.StartRecording();

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(timer_Tick);
            dt = DateTime.Now;
            timer.Start();
        }

        DateTime dt = new DateTime();
        private void timer_Tick(object sender, EventArgs e)
        {
            labelRecordTimer.Content = (DateTime.Now - dt).ToString();
        }

        private void btnRecordStop_Click(object sender, RoutedEventArgs e)
        {
            // если таймер не тикает, то запись не ведется, и нечего останавливать.
            if (!timer.IsEnabled)
                return;

            btnRecordStop.IsEnabled = false;

            waveSource.StopRecording();
            timer.Stop();

            gridRecordDone.Visibility = System.Windows.Visibility.Visible;
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
                wavData.AddRange(e.Buffer);
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
            
            btnRecordStart.IsEnabled = true;
        }

        private void btnRecordFileSavePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                recordingFilePath = dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "RecordedSound.wav";
                tbRecordFile.Text = dialog.SelectedPath;
            }
        }


        private void btnRecroderNext_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем начальный "экран" с выбором файла.
            gridIntro.Visibility = System.Windows.Visibility.Collapsed;
            gridRecorder.Visibility = System.Windows.Visibility.Collapsed;

            // Отображаем информацию о файле.
            showFileInfo(ObjectManager.CurrentFile);

            // Указываем текущий открытый "экран".
            currentStepGrid = gridWaveform;

            scrollWaveform.Children.Add(new SliceControl(ObjectManager.CurrentFile));

            wizardOpenNextStep();

            // Отрисовываем БПФ.
            ObjectManager.CurrentFile.FFT();
            gridGraphFFT.Children.Add(new GraphFFT(ObjectManager.CurrentFile.modulFFT));
        }

        private void btnRecroderDoneCancel_Click(object sender, RoutedEventArgs e)
        {
            gridRecordDone.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnRecroderDoneNext_Click(object sender, RoutedEventArgs e)
        {
            // Если файл существует, значит он записался, значит можно продолжать.
            if (File.Exists(recordingFilePath))
            {
                gridRecordDone.Visibility = System.Windows.Visibility.Collapsed;
                gridPopUpRecorder.Visibility = System.Windows.Visibility.Collapsed;

                if (ObjectManager.CurrentFile != null)
                {
                    // TODO: подымать алерт о том, что предыдущая сессия не сохранится. А лучше всего перенести эту проверку в само свойство!
                    ObjectManager.CurrentFile = null;
                }
                ObjectManager.CurrentFile = new WavFile(recordingFilePath);
                gridSliceControlData.Children.Add(new SliceControl(ObjectManager.CurrentFile));
                gridSliceControl.Visibility = System.Windows.Visibility.Visible;
            }
        }


    }
}
