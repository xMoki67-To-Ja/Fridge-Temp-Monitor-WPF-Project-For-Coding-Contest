using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SQLite;

namespace POL_EKO_TempMonitor
{
    /// <summary>
    /// Logika interakcji dla klasy Chart.xaml
    /// </summary>
    public partial class Chart : Window
    {
        public Chart()
        {
            InitializeComponent();
            // Pobierz dane z bazy danych
            SQLiteConnection connection = new SQLiteConnection("C:\\Pol-Eko\\raport.db");
            string query = "SELECT dTime, temp FROM DB";
            List<DB> data = connection.Query<DB>(query);

            // Stwórz listy z danymi do wykresu
            List<DateTime> dates = new List<DateTime>();
            List<double> temperatures = new List<double>();
            foreach (DB item in data)
            {
                DateTime date = DateTime.ParseExact(item.dTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                double temperature = 0;
                
                dates.Add(date);
                if (item.temp != "Error")
                {
                    temperature = float.Parse(item.temp, CultureInfo.InvariantCulture)/10;
                }
                temperatures.Add(temperature);
                
            }

            // Stwórz wykres
            var plotModel = new PlotModel { Title = "Temperatura w czasie" };
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left,
                Title = "Temperatura w ℃",
                Minimum = -10, // minimalna wartość osi Y
                Maximum = 30, // maksymalna wartość osi Y
                MajorStep = 5, // krok główny
                MinorStep = 1, });
            plotModel.Axes.Add(new DateTimeAxis { Title = "Data i godzina pomiaru", Position = AxisPosition.Bottom, StringFormat = "dd:MM:yyyy\nHH:mm:ss", IntervalType = DateTimeIntervalType.Seconds,
                MinorStep = 1});
                var series = new LineSeries();
            for (int i = 0; i < dates.Count; i++)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dates[i]), temperatures[i]));
            }
            plotModel.Series.Add(series);

            // Wyświetl wykres w kontrolce PlotView
            plotView.Model = plotModel;
        }
    }
}
