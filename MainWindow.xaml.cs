using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace Ets2AtsCustomRichPresence
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TelemetryNetworkHelper telemetryNetworkHelper;
        Thread startingThread;

        public MainWindow()
        {
            InitializeComponent();
            telemetryNetworkHelper = new TelemetryNetworkHelper();

            stopButton.Visibility = Visibility.Hidden;
            textBlock.Text = "Please make sure that the telemetry server and discord are running before you hit the 'Start' button.";
            textBlockTwo.Text = "Stopped";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startingThread = new Thread(() =>
            {
                stopButton.Dispatcher.Invoke(() =>
                {
                    telemetryNetworkHelper.Start(isEtsBtnChecked: (etsBtn.IsChecked == true));
                });
            })
            {
                IsBackground = true
            };
            startingThread.Start();

            startButton.Visibility = Visibility.Hidden;
            stopButton.Visibility = Visibility.Visible;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            stopButton.Visibility = Visibility.Hidden;

            // stop current process
            telemetryNetworkHelper.Stop();

            // Cleanup and exit application
            Environment.Exit(Environment.ExitCode);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Deinitialize and dispose the connection upon closing the application window
            telemetryNetworkHelper.Stop();

            // Cleanup and exit application
            Environment.Exit(Environment.ExitCode);
        }

        private void CurrentJob_Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }
            
        }

        private void CurrentTruckModel_Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }
        }

        private void Speed_Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }
        }

        private void Rpm_Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }
        }

        private void GearPosition_Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }
        }

        private void FuelPercentage__Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }

        }

        private void OnlyCurrJobTruckMo_Checked(object sender, RoutedEventArgs e)
        {
            if (jobSpeed != null && jobSpeed.IsChecked == true)
            {
                jobSpeed.IsChecked = false;
            }


            if (job != null && job.IsChecked == true)
            {
                job.IsChecked = false;
            }

            if (truckModel != null && truckModel.IsChecked == true)
            {
                truckModel.IsChecked = false;
            }

            if (speed != null && speed.IsChecked == true)
            {
                speed.IsChecked = false;
            }

            if (rpm != null && rpm.IsChecked == true)
            {
                rpm.IsChecked = false;
            }

            if (gearPos != null && gearPos.IsChecked == true)
            {
                gearPos.IsChecked = false;
            }

            if (fuelPer != null && fuelPer.IsChecked == true)
            {
                fuelPer.IsChecked = false;
            }
        }

        private void OnlyCurrJobTruckSpeed_Checked(object sender, RoutedEventArgs e)
        {
            if (jobModel != null && jobModel.IsChecked == true)
            {
                jobModel.IsChecked = false;
            }


            if (job != null && job.IsChecked == true)
            {
                job.IsChecked = false;
            }

            if (truckModel != null && truckModel.IsChecked == true)
            {
                truckModel.IsChecked = false;
            }

            if (speed != null && speed.IsChecked == true)
            {
                speed.IsChecked = false;
            }

            if (rpm != null && rpm.IsChecked == true)
            {
                rpm.IsChecked = false;
            }

            if (gearPos != null && gearPos.IsChecked == true)
            {
                gearPos.IsChecked = false;
            }

            if (fuelPer != null && fuelPer.IsChecked == true)
            {
                fuelPer.IsChecked = false;
            }
        }
    }
}
