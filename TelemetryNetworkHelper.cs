using System;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Windows.Documents;
using System.Collections.Generic;

namespace Ets2AtsCustomRichPresence
{
    class TelemetryNetworkHelper
    {
        static string data = null;
        MainWindow mainWindow;
        DiscordPresenceHelper discordPresenceHelper;
        Thread telemetryThread;
        bool isEtsBtnChecked;

        public TelemetryNetworkHelper() 
        {
            mainWindow = (MainWindow)Application.Current.MainWindow;
        }

        // Request data from telemetry server handle possible exceptions
        static async Task GetData() 
        {
            var httpClient = HttpClientFactory.Create();
            var url = "http://localhost:25555/api/ets2/telemetry";

            try
            {
                data = await httpClient.GetStringAsync(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ((MainWindow)Application.Current.MainWindow).textBlock.Text = "Telemetry server is not running!\nPlease run it before hitting the 'Start' button.";
                ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Red);
                ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Error! Telemetry server not running";
                Thread.Sleep(5000);
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void ParseData(JObject jsonData)
        {

            if (isEtsBtnChecked)
            {
                discordPresenceHelper = new DiscordPresenceHelper(clientId: "572835815609204768");
                discordPresenceHelper.InitClient();
                discordPresenceHelper.PresenceUpdate(details: "Starting up", state: "");
            }
            else
            {
                discordPresenceHelper = new DiscordPresenceHelper(clientId: "573464675631628289");
                discordPresenceHelper.InitClient();
                discordPresenceHelper.PresenceUpdate(details: "Starting up", state: "");
            }

            while (true)
            {
                Thread.Sleep(1500);
                try
                {
                    _ = mainWindow.textBlock.Dispatcher.Invoke(async () =>
                      {
                          await GetData();

                          if(((bool)jsonData["game"]["paused"]))
                          {
                              ((MainWindow)Application.Current.MainWindow).textBlock.Text = "Waiting for you to drive";
                              discordPresenceHelper.PresenceUpdate(details: "Resting", state: "");
                              
                          }
                          else
                          {
                              ((MainWindow)Application.Current.MainWindow).textBlock.Text = "Waiting for the game to load";
                              discordPresenceHelper.PresenceUpdate(details: "Starting up", state: "");
                          }
                          ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Processing";
                          ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Orange);

                          jsonData = JObject.Parse(data);
                      });

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }


                bool altInfoFlag = true;

                while (((bool)jsonData["game"]["connected"]) && (!(bool)jsonData["game"]["paused"]))
                {
                    Thread.Sleep(1000);

                    try
                    {
                        _ = mainWindow.textBlock.Dispatcher.Invoke(async () =>
                        {
                            ((MainWindow)Application.Current.MainWindow).textBlock.Text = "Everything is fine";
                            ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Running";
                            ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Green);

                            await GetData();
                            jsonData = JObject.Parse(data);

                            string details, state, engine;

                            engine = (bool)jsonData["truck"]["engineOn"] ? "" : "Engine: Off";

                            int speed = (int)jsonData["truck"]["speed"];
                            string speedUnit = isEtsBtnChecked ? "Km/h" : "Mph";

                            int rpm = (int)jsonData["truck"]["engineRpm"];

                            float truckFuel = (float)jsonData["truck"]["fuel"];
                            float truckFuelCapacity = (float)jsonData["truck"]["fuelCapacity"];
                            int fuelPercent = (int)((truckFuel / truckFuelCapacity) * 100);

                            string gear;

                            if ((int)jsonData["truck"]["displayedGear"] <= 0)
                            {
                                if((int)jsonData["truck"]["displayedGear"] < 0)
                                {
                                    gear = "R";
                                }
                                else
                                {
                                    gear = "N";
                                }
                            }
                            else
                            {
                                gear = jsonData["truck"]["displayedGear"].ToString();
                            }

                            string sourceCity, sourceCompany, destinationCity, destinationCompany;
                            sourceCity = jsonData["job"]["sourceCity"].ToString();
                            sourceCompany = jsonData["job"]["sourceCompany"].ToString();
                            destinationCity = jsonData["job"]["destinationCity"].ToString();
                            destinationCompany = jsonData["job"]["destinationCompany"].ToString();

                            string jobData = $"{sourceCompany}, {sourceCity} to {destinationCompany}, {destinationCity}";

                            string truckData = $"{jsonData["truck"]["make"]} {jsonData["truck"]["model"]} {engine} ";

                            string speedData = $" {speed} {speedUnit} ";

                            string rpmData = $" {rpm} RPM ";

                            string gearData = $" Gear: {gear} ";

                            string fuelData = $" Fuel: {fuelPercent}% ";

                            var mWin = ((MainWindow)Application.Current.MainWindow);

                            if ((mWin.job.IsChecked == true) && (mWin.truckModel.IsChecked == true) && (mWin.speed.IsChecked == true) && (mWin.rpm.IsChecked == true) && (mWin.gearPos.IsChecked == true) && (mWin.fuelPer.IsChecked == true))
                            {
                                details = $"{truckData}";

                                if (sourceCity.Length > 1)
                                {
                                    state = altInfoFlag ? $"{speedData}{rpmData}{gearData}{fuelData}" : $"{jobData}";
                                    altInfoFlag = !altInfoFlag;
                                }
                                else
                                {
                                    state = $"{speedData}{rpmData}{gearData}{fuelData}";
                                }
                            }
                            else if (mWin.jobModel.IsChecked == true)
                            {
                                details = $"{jobData}";
                                state = $"{truckData}";
                            }
                            else if (mWin.jobSpeed.IsChecked == true)
                            {
                                details = $"{jobData}";
                                state = $"{truckData}{speedData}";
                            }
                            else
                            {
                                List<string> infoToShow = new List<string>();

                                if (mWin.job.IsChecked == true)
                                {
                                    infoToShow.Add(jobData);
                                }
                                if (mWin.truckModel.IsChecked == true)
                                {
                                    infoToShow.Add(truckData);
                                }
                                if (mWin.speed.IsChecked == true)
                                {
                                    infoToShow.Add(speedData);
                                }
                                if (mWin.rpm.IsChecked == true)
                                {
                                    infoToShow.Add(rpmData);
                                }
                                if (mWin.gearPos.IsChecked == true)
                                {
                                    infoToShow.Add(gearData);
                                }
                                if (mWin.fuelPer.IsChecked == true)
                                {
                                    infoToShow.Add(fuelData);
                                }

                                string detailsData = "";
                                string stateData = "";

                                int count = 0;

                                if (infoToShow.Contains(jobData)) 
                                {
                                    infoToShow.Remove(jobData);
                                }

                                infoToShow.ForEach((string item) => {

                                    if (count < 3) {


                                        detailsData += item;
                                    }
                                    else
                                    {
                                        stateData += item;
                                    }

                                    count++;
                                });


                                if (sourceCity.Length > 1 && (mWin.job.IsChecked == true))
                                {
                                    state = altInfoFlag ? stateData : $"{jobData}";
                                    altInfoFlag = !altInfoFlag;
                                }
                                else if ((sourceCity.Length < 1) && (mWin.job.IsChecked == true))
                                {
                                    state = $"Freeroaming: No job taken";
                                }
                                else
                                {
                                    state = stateData;
                                }

                                details = detailsData;
                            }


                            discordPresenceHelper.PresenceUpdate(details: details, state: state);

                            // TODO: Add checkboxes in GUI so that user can select what to show and waht not to
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"down here {e.Message}");
                    }

                }

            }

        }

        public async void Start(bool isEtsBtnChecked)
        {
            this.isEtsBtnChecked = isEtsBtnChecked;

            await GetData();
            ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Processing";
            ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Orange);

            JObject jsonData;

            try
            {
                jsonData = JObject.Parse(data);
                telemetryThread = new Thread(() => { ParseData(jsonData); });
                telemetryThread.IsBackground = true;
                telemetryThread.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ((MainWindow)Application.Current.MainWindow).textBlock.Text = $"Error fetching data!\n{e.Message}";
                ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Red);
                ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Error";
            }
        }

        public void Stop()
        {
            try
            {
                discordPresenceHelper.DeInitClient();
                telemetryThread.Abort();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ((MainWindow)Application.Current.MainWindow).textBlock.Text = "Stopped by the user";
                ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Red);
                ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Stopped";
            }
        }

    }
}
