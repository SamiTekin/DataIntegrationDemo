using DataIntegrationDemo.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.Design.AxImporter;

namespace DataIntegrationDemo
{
    public partial class BrandAddDatabaseForm : Form
    {
        private string connectionString = @"Data Source=DESKTOP-2AFIS8M\SQLEXPRESS;Initial Catalog=IntegrationDemoDb;Integrated Security=True;";
        public BrandAddDatabaseForm()
        {
            InitializeComponent();
        }
        private void InsertBrand(SqlConnection connection, string brandName)
        {
            string checkQuery = "SELECT COUNT(*) FROM Brands WHERE Name=@Name";
            using (SqlCommand command = new SqlCommand(checkQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", brandName);
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    string insertQuery = "INSERT INTO Brands (Name) VALUES (@Name)";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Name", brandName);
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string jsonFilePath = @"C:\Users\Sami\Desktop\spor-outdoor.json";
            string jsonString = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsonString, options);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Her bir ürünün markasını tabloya ekleyin
                foreach (var product in products)
                {
                    InsertBrand(connection, product.brand);
                }
            }
            MessageBox.Show("Markalar eklendi");

        }
    }
}
