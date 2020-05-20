using System;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;

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
                ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Stopped";
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

                            engine = (bool)jsonData["truck"]["engineOn"] ? "On" : "Off";

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

                            if (sourceCity.Length > 1)
                            {
                                state = altInfoFlag ? $"{speed} {speedUnit} {rpm} RPM Gear: {gear} Fuel: {fuelPercent}%" : $"{sourceCompany}, {sourceCity} to {destinationCompany}, {destinationCity}";
                                altInfoFlag = !altInfoFlag;
                            }
                            else
                            {
                                state = $"{speed} {speedUnit} {rpm} RPM Gear: {gear} Fuel: {fuelPercent}%";
                                altInfoFlag = !altInfoFlag;
                            }

                            Debug.WriteLine(sourceCity.Length);
                            details = $"Driving: {jsonData["truck"]["make"]} {jsonData["truck"]["model"]} Engine: {engine}";

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
                ((MainWindow)Application.Current.MainWindow).textBlock.Text = "Telemetry server is not running!\nPlease run it before hitting the 'Start' button.";
                ((MainWindow)Application.Current.MainWindow).processIndicator.Fill = new SolidColorBrush(Colors.Red);
                ((MainWindow)Application.Current.MainWindow).textBlockTwo.Text = "Stopped";
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
