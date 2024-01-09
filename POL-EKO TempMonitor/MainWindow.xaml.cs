using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Windows.Media.Animation;
using System.Net.Http;
using System.Text.Json;
using SQLite;
using System.Diagnostics.Eventing.Reader;

namespace POL_EKO_TempMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ipAddress = Properties.Settings.Default.ipAddress;
        private string port = Properties.Settings.Default.port;
        private double time2 = Properties.Settings.Default.interwal;
        private FridgeData fridgeData;
        private DateTime appStartTime;
        private System.Timers.Timer timer1;
        private System.Timers.Timer timer2;


        private void ChangeIpAddress_Click(object sender, RoutedEventArgs e)
        {
            // Wyświetlamy okno dialogowe z prośbą o wprowadzenie nowego adresu IP
            string newIpAddress = Microsoft.VisualBasic.Interaction.InputBox("Wprowadź nowy adres IP:");
            string newPort = ":" + Microsoft.VisualBasic.Interaction.InputBox("Wprowadź nowy port:");
            if (IPAddress.TryParse(newIpAddress, out IPAddress ip))
            {
                // Adres IP jest poprawny - zapisujemy go do zmiennej ipAddress
                ipAddress = newIpAddress;
                if (int.TryParse(newPort.TrimStart(':'), out int portNumber) && portNumber >= 0 && portNumber <= 65535)
                {
                    // Port jest poprawny - zapisujemy go do zmiennej port i wykonujemy połączenie z serwerem
                    port = newPort;
                    Properties.Settings.Default.ipAddress = ipAddress;
                    Properties.Settings.Default.port = port;
                    Properties.Settings.Default.Save();
                    GetFridgeData(ipAddress, port);
                }
                else
                {
                    // Port jest niepoprawny - wyświetlamy komunikat o błędzie
                    MessageBox.Show("Błędny Port");
                }
            }
            else
            {
                // Adres IP jest niepoprawny - wyświetlamy komunikat o błędzie
                MessageBox.Show("Błędny adres IP");
            }
        }
        private void option1_Click(object sender, RoutedEventArgs e)
        {
            time2 = 300000;
            Properties.Settings.Default.interwal = time2;
            Properties.Settings.Default.Save();
            ReloadTimer();
        }
        private void option2_Click(object sender, RoutedEventArgs e)
        {
            time2 = 600000;
            Properties.Settings.Default.interwal = time2;
            Properties.Settings.Default.Save();
            ReloadTimer();
        }
        private void option3_Click(object sender, RoutedEventArgs e)
        {
            time2 = 900000;
            Properties.Settings.Default.interwal = time2;
            Properties.Settings.Default.Save();
            ReloadTimer();
        }
        public class FridgeData
        {
            public bool IsRunning { get; set; }
            public double Temperature { get; set; }
            public bool TemperatureError { get; set; }
        }
        public FridgeData GetFridgeData(string ipAddress, string port)
        {
            string url = $"http://{ipAddress + port}/api/v1/school/status";
            try
            {
                // utwórz klienta HTTP
                HttpClient client = new HttpClient();

                // pobierz dane z lodówki
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                // przetwórz odpowiedź
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JsonDocument doc = JsonDocument.Parse(responseBody);

                bool isRunning = doc.RootElement.GetProperty("IS_RUNNING").GetBoolean();
                double temperature = doc.RootElement.GetProperty("TEMPERATURE_MAIN").GetProperty("value").GetDouble();
                bool temperatureError = doc.RootElement.GetProperty("TEMPERATURE_MAIN").GetProperty("error").GetBoolean();

                return new FridgeData { IsRunning = isRunning, Temperature = temperature, TemperatureError = temperatureError };
            }
            catch (HttpRequestException ex)
            {                
                DateTime now = DateTime.Now;
                Dispatcher.Invoke(() =>
                {
                    temperatureTextBlock.FontSize = 20;
                    temperatureTextBlock.Foreground = Brushes.Red;
                    temperatureTextBlock.Text = $"Wystąpił błąd. \nTrwa ponowne łączenie z serwerem.";
                    measurementDateTextBlock.Text = $"";
                    isWorkingTextBlock.Text = $"";
                    status.Text = $"";
                    temperatureTextBlockInfo.Text = $"";
                    measurementTimeTextBlock.Text = $"{"📅 " + now.ToString("dd-MM-yyyy \n" + "🕛 " + "HH:mm")}";
                    TimeSpan measurementTimeSpan = DateTime.Now - appStartTime;
                    DateTime measurementTime = DateTime.Today.Add(measurementTimeSpan);
                    string measurementTimeStr = measurementTime.ToString("HH:mm:ss");
                    DB temp = new DB("Error", measurementTimeStr, now.ToString("dd-MM-yyyy HH:mm:ss"), "Wystąpił błąd sieci!");
                    var db = new SQLiteConnection("C:\\Pol-Eko\\raport.db");

                    db.Insert(temp);

                    db.Close();
                });
                string message = $"Błąd zapytania HTTP: {ex.Message}";
                MessageBox.Show(message);
                return null;
            }
            catch (UriFormatException ex)
            {                
                DateTime now = DateTime.Now;
                Dispatcher.Invoke(() =>
                {
                    temperatureTextBlock.FontSize = 20;
                    temperatureTextBlock.Foreground = Brushes.Red;
                    temperatureTextBlock.Text = $"Wystąpił błąd. \nTrwa ponowne łączenie z serwerem.";
                    measurementDateTextBlock.Text = $"";
                    isWorkingTextBlock.Text = $"";
                    status.Text = $"";
                    temperatureTextBlockInfo.Text = $"";
                    measurementTimeTextBlock.Text = $"{"📅 " + now.ToString("dd-MM-yyyy \n" + "🕛 " + "HH:mm")}";
                    TimeSpan measurementTimeSpan = DateTime.Now - appStartTime;
                    DateTime measurementTime = DateTime.Today.Add(measurementTimeSpan);
                    string measurementTimeStr = measurementTime.ToString("HH:mm:ss");
                    DB temp = new DB("Error", measurementTimeStr, now.ToString("dd-MM-yyyy HH:mm:ss"), "Wystąpił błąd sieci!");
                    var db = new SQLiteConnection("C:\\Pol-Eko\\raport.db");

                    db.Insert(temp);

                    db.Close();

                });
                string message = $"Niepoprawny format adresu URL: {ex.Message}";
                MessageBox.Show(message);
                return null;
            }
            catch (Exception ex)
            {
                DateTime now = DateTime.Now;
                Dispatcher.Invoke(() =>
                {
                    temperatureTextBlock.FontSize = 20;
                    temperatureTextBlock.Foreground = Brushes.Red;
                    temperatureTextBlock.Text = $"Wystąpił błąd. \nTrwa ponowne łączenie z serwerem.";
                    measurementDateTextBlock.Text = $"";
                    isWorkingTextBlock.Text = $"";
                    status.Text = $"";
                    temperatureTextBlockInfo.Text = $"";
                    measurementTimeTextBlock.Text = $"{"📅 " + now.ToString("dd-MM-yyyy \n" + "🕛 " + "HH:mm")}";
                    TimeSpan measurementTimeSpan = DateTime.Now - appStartTime;
                    DateTime measurementTime = DateTime.Today.Add(measurementTimeSpan);
                    string measurementTimeStr = measurementTime.ToString("HH:mm:ss");                    
                    DB temp = new DB("Error", measurementTimeStr, now.ToString("dd-MM-yyyy HH:mm:ss"), "Wystąpił błąd sieci!");
                    var db = new SQLiteConnection("C:\\Pol-Eko\\raport.db");

                    db.Insert(temp);

                    db.Close();
                });
                string message = $"Nieznany błąd: {ex.Message}";
                MessageBox.Show(message);
                return null;
            }
    }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
           
            timer1.Stop();
            fridgeData = GetFridgeData(ipAddress, port);
            timer1.Start();

            if (fridgeData != null)
            {
                Dispatcher.Invoke(() =>
                {
                    
                    // oblicz czas i datę pomiaru
                    TimeSpan measurementTimeSpan = DateTime.Now - appStartTime;
                    DateTime measurementTime = DateTime.Today.Add(measurementTimeSpan);
                    string measurementTimeStr = measurementTime.ToString("HH:mm:ss");
                    DateTime now = DateTime.Now;
                    temperatureTextBlock.FontSize = 72;
                    temperatureTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#017CC4"));
                    temperatureTextBlockInfo.Text = $"{"Aktualna temperatura: "}";
                    temperatureTextBlock.Text = $"{(fridgeData.Temperature / 100.0).ToString("F1") + "℃"}";
                    measurementDateTextBlock.Text = $"{"Czas pomiaru: "+measurementTimeStr}";
                    status.Text = $"Status pracy: ";
                    measurementTimeTextBlock.Text = $"{"📅 " + now.ToString("dd-MM-yyyy \n" + "🕛 " + "HH:mm")}";
                    if (fridgeData.IsRunning == true) 
                    {
                        isWorkingTextBlock.Foreground = Brushes.Green;
                        isWorkingTextBlock.Text = $"Działa";
                    }
                    else
                    {
                        isWorkingTextBlock.Foreground = Brushes.Red;
                        isWorkingTextBlock.Text = $"Nie działa";
                    }
                });
            }
        }
        private void Timer_Elapsed2(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (temperatureTextBlock.Text != "Wystąpił błąd. \nTrwa ponowne łączenie z serwerem.")
                {   
                    timer2.Stop();
                    InsertIntoDb();
                    timer2.Start();
                }                
            });

        }
        public void ReloadTimer()
        {
            time2 = Properties.Settings.Default.interwal;
            timer2.Interval = time2;
        }
        public MainWindow()
        {
            InitializeComponent();
            if (System.IO.Directory.Exists("C:\\Pol-Eko") && System.IO.File.Exists("C:\\Pol-Eko\\raport.db"))
            {

            }
            else
            {
                System.IO.Directory.CreateDirectory("C:\\Pol-Eko");
                var db = new SQLiteConnection("C:\\Pol-Eko\\raport.db");
                db.CreateTable<DB>();
                db.Close();
            }
            if (Properties.Settings.Default.interwal == 300000)
            {
                option1.IsChecked = true;
            }
            else if (Properties.Settings.Default.interwal == 600000)
            {
                option2.IsChecked = true;
            }
            else
            {
                option3.IsChecked = false;
            }
            fridgeData = GetFridgeData(ipAddress, port);
            // zapisz czas uruchomienia aplikacji
            appStartTime = DateTime.Now;

            // Ustawienie interwału timera
            timer1 = new System.Timers.Timer(1000);
            timer1.Elapsed += Timer_Elapsed;
            timer1.Start();
            timer2 = new System.Timers.Timer(Properties.Settings.Default.interwal);
            timer2.Elapsed += Timer_Elapsed2;
            timer2.Start();
            ReloadTimer();
        }
        public void InsertIntoDb()
        {
            TimeSpan measurementTimeSpan = DateTime.Now - appStartTime;
            DateTime measurementTime = DateTime.Today.Add(measurementTimeSpan);
            string measurementTimeStr = measurementTime.ToString("HH:mm:ss");
            DateTime now = DateTime.Now;
            DB temp = new DB((fridgeData.Temperature / 100.0).ToString("F1"), measurementTimeStr, now.ToString("dd-MM-yyyy HH:mm:ss"), isWorkingTextBlock.Text);
            var db = new SQLiteConnection("C:\\Pol-Eko\\raport.db");

            db.Insert(temp);

            db.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (temperatureTextBlock.Text != "Wystąpił błąd. \nTrwa ponowne łączenie z serwerem.")
            {
                InsertIntoDb();
                MessageBox.Show("Sukces! Zapisano w bazie danych!");
            }
            else
            {
                string message = $"Nie można zapisać w bazie danych. Błąd połączenia!";
                MessageBox.Show(message);
            }
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Raport newWindow = new Raport();
            newWindow.Show();
        }
        private void Show2_Click(object sender, RoutedEventArgs e)
        {
            Chart newWindow = new Chart();
            newWindow.Show();
        }
    }
}
