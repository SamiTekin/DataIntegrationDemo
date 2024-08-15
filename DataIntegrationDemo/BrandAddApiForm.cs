using DataIntegrationDemo.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataIntegrationDemo
{
    public partial class BrandAddApiForm : Form
    {
        private const string ApiUrl = "https://dehapi.com/api/seller/brand/add";
        private const string ApiKey = "0eaf70d3def343b9e7fa2572ac8f40fcc56141ae8b30bd2a6a75e6";
        private const string ApiSecret = "c590a94236d96eded95c2ae1efbbbb4e6479ef1d";
        private const string connectionString = @"Data Source=DESKTOP-2AFIS8M\SQLEXPRESS;Initial Catalog=IntegrationDemoDb;Integrated Security=True;";
        private Dictionary<int, int> apiBrandIds = new Dictionary<int, int>();
        public BrandAddApiForm()
        {
            InitializeComponent();
        }
        private async Task<List<BrandDto>> GetBrandFromDatabase(bool isAll= true)
        {
            List<BrandDto> brands = new List<BrandDto>();
            using (var connection=new SqlConnection(connectionString))
            {
                string getAllSqlCommand = "SELECT Id, Name FROM Brands";
                string getNewSqlCommand = "SELECT Id, Name FROM Brand WHERE ResponseId=0";
                await connection.OpenAsync();
                using (var command = new SqlCommand(isAll ? getAllSqlCommand : getNewSqlCommand, connection))
                {
                    using (var reader=await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id =reader.GetInt32(0);
                            string name =reader.GetString(1);
                            {
                                brands.Add(new BrandDto
                                {
                                    Id = id,
                                    Name = name
                                });
                            }
                        }
                    }
                }
            }
            return brands;
        } 
        private async Task<int> SendBrandToApi(ApiBrandDto apiBrand)
        {
            using (var client=new HttpClient())
            {
                var request =new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Add("Dehasoft-Api-Key", ApiKey);
                request.Headers.Add("Dehasoft-Api-Secret", ApiSecret);

                var json= Newtonsoft.Json.JsonConvert.SerializeObject(apiBrand);
                var content= new StringContent(json,Encoding.UTF8,MediaTypeHeaderValue.Parse("application/json"));
                request.Content=content;

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Marka {apiBrand.name} başarıyla gönderildi.");
                    var respCon = await response.Content.ReadAsStringAsync();
                    dynamic responseContent= JsonConvert.DeserializeObject(respCon);
                    Console.WriteLine($"API Yanıtı: {responseContent}");
                    return responseContent.brand.id;

                }
                else
                {
                    Console.WriteLine($"Marka {apiBrand.name} gönderilirken hata oluştu: {response.ReasonPhrase}"); 
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Hata Mesajı: {errorMessage}");
                }
            }
            return 0;
        }

        private async Task SendBrandsToApi(BrandDto brand)
        {
            var apiBrand = new ApiBrandDto()
            {
                name = brand.Name,
                description = null
            };
            var responseId = await SendBrandToApi(apiBrand);
            apiBrandIds[brand.Id]=responseId;
            await UptadeApiResponseId(brand.Id, responseId);

        }
        private async Task UptadeApiResponseId(int brandId, int responseId)
        {
            using(var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using(var command =new SqlCommand("UPDATE Brands SET ResponseId = @responseId WHERE Id = @brandId", connection))
                {
                    command.Parameters.AddWithValue("@responseId", responseId);
                    command.Parameters.AddWithValue("@brandId", brandId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            List<BrandDto> brands = await GetBrandFromDatabase();
            foreach(var brand in brands)
            {
                await SendBrandsToApi(brand);
            }
            MessageBox.Show("Markalar Apiye gönderildi");
        }
    }
}
