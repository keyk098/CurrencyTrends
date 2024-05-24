using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ForexAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connection string for SQL Server
            string connectionString = "Server=KK505_Database;Database=ForexDB;Integrated Security=True;";
            
            // Load data from the database
            DataTable transactions = LoadTransactionData(connectionString);
            
            // Analyze data
            DataTable summary = AnalyzeData(transactions);
            
            // Display summary
            DisplaySummary(summary);
            
            // Generate recommendations
            GenerateRecommendations(summary);
        }

        static DataTable LoadTransactionData(string connectionString)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Transactions";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(dataTable);
            }

            return dataTable;
        }

        static DataTable AnalyzeData(DataTable transactions)
        {
            var summary = transactions.AsEnumerable()
                .GroupBy(row => new { Currency = row.Field<string>("Currency"), TransactionType = row.Field<string>("TransactionType") })
                .Select(g => new
                {
                    g.Key.Currency,
                    g.Key.TransactionType,
                    TotalAmount = g.Sum(row => row.Field<decimal>("Amount"))
                })
                .ToList();

            DataTable summaryTable = new DataTable();
            summaryTable.Columns.Add("Currency", typeof(string));
            summaryTable.Columns.Add("BUY", typeof(decimal));
            summaryTable.Columns.Add("SELL", typeof(decimal));
            summaryTable.Columns.Add("Total", typeof(decimal));
            summaryTable.Columns.Add("Net Demand", typeof(decimal));

            var currencies = summary.GroupBy(s => s.Currency).Select(g => g.Key).ToList();
            foreach (var currency in currencies)
            {
                var buy = summary.FirstOrDefault(s => s.Currency == currency && s.TransactionType == "BUY")?.TotalAmount ?? 0;
                var sell = summary.FirstOrDefault(s => s.Currency == currency && s.TransactionType == "SELL")?.TotalAmount ?? 0;
                var total = buy + sell;
                var netDemand = buy - sell;

                summaryTable.Rows.Add(currency, buy, sell, total, netDemand);
            }

            return summaryTable;
        }

        static void DisplaySummary(DataTable summary)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form form = new Form();
            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            form.Controls.Add(chart);

            ChartArea chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);

            Series buySeries = new Series("BUY")
            {
                ChartType = SeriesChartType.Bar
            };
            Series sellSeries = new Series("SELL")
            {
                ChartType = SeriesChartType.Bar
            };
            Series netDemandSeries = new Series("Net Demand")
            {
                ChartType = SeriesChartType.Bar,
                Color = System.Drawing.Color.SkyBlue
            };

            foreach (DataRow row in summary.Rows)
            {
                string currency = row["Currency"].ToString();
                buySeries.Points.AddXY(currency, row["BUY"]);
                sellSeries.Points.AddXY(currency, row["SELL"]);
                netDemandSeries.Points.AddXY(currency, row["Net Demand"]);
            }

            chart.Series.Add(buySeries);
            chart.Series.Add(sellSeries);
            chart.Series.Add(netDemandSeries);

            Application.Run(form);
        }

        static void GenerateRecommendations(DataTable summary)
        {
            Console.WriteLine("Recommendations for Next Week:");

            foreach (DataRow row in summary.Rows)
            {
                decimal netDemand = (decimal)row["Net Demand"];
                if (netDemand > 0)
                {
                    Console.WriteLine($"Order more of {row["Currency"]} based on net demand of {netDemand:F2}");
                }
            }
        }
    }
}
