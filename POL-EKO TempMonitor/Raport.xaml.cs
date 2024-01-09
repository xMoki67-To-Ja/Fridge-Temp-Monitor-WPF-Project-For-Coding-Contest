using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
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
using SQLite;
using OfficeOpenXml;

namespace POL_EKO_TempMonitor
{
    /// <summary>
    /// Logika interakcji dla klasy Raport.xaml
    /// </summary>
    public partial class Raport : Window
    {
        public Raport()
        {
            InitializeComponent();
            SQLiteConnection connection = new SQLiteConnection("C:\\Pol-Eko\\raport.db");

            // Tworzenie zapytania SQL
            string query = "SELECT temp, mTime, dTime, isWorking FROM DB";

            // Wykonanie zapytania SQL
            List<DB> data = connection.Query<DB>(query);

            // Przypisanie wyników do źródła danych datagrid
            raportDataGrid.ItemsSource = data;
        }
        public class DB
        {
            public string temp { get; set; }
            public string mTime { get; set; }
            public string dTime { get; set; }
            public string isWorking { get; set; }

            public DB() { }

            public DB(string temp, string mTime, string dTime, string isWorking)
            {
                this.temp = temp;
                this.mTime = mTime;
                this.dTime = dTime;
                this.isWorking = isWorking;
            }
        }
        public List<DB> GetDataFromSQLiteDatabase()
        {
            var dbPath = "C:\\Pol-Eko\\raport.db";
            var conn = new SQLiteConnection(dbPath);

            // Pobranie danych z bazy danych
            var query = conn.Table<DB>().ToList();

            return query;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var package = new ExcelPackage())
            {
                {
                    // Dodanie arkusza do pliku Excel
                    var worksheet = package.Workbook.Worksheets.Add("Dane");

                    // Utworzenie nagłówków kolumn
                    worksheet.Cells[1, 1].Value = "Temperatura";
                    worksheet.Cells[1, 2].Value = "Długość pomiaru";
                    worksheet.Cells[1, 3].Value = "Data i godzina pomiaru";
                    worksheet.Cells[1, 4].Value = "Czy działała";

                    // Pobranie danych z bazy danych SQLite
                    List<DB> data = GetDataFromSQLiteDatabase();

                    // Wpisanie danych do pliku Excel
                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = float.Parse(data[i].temp, CultureInfo.InvariantCulture) / 10;
                        worksheet.Cells[i + 2, 2].Value = data[i].mTime;
                        worksheet.Cells[i + 2, 3].Value = data[i].dTime;
                        worksheet.Cells[i + 2, 4].Value = data[i].isWorking;
                    }

                    // Zapisanie pliku Excel
                    var fileStream = new FileStream("C:\\Pol-Eko\\dane.xlsx", FileMode.Create);
                    package.SaveAs(fileStream);
                }
            }
            MessageBox.Show("Wyeksportowano pomyślnie!");
        }
    }
}
