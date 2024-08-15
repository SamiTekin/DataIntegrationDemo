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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataIntegrationDemo
{
    public partial class CategoriesAddApiForm : Form
    {
        private const string ApiUrl = "https://dehapi.com/api/seller/product/category/add";
        private const string ApiKey = "0eaf70d3def343b9e7fa2572ac8f40fcc56141ae8b30bd2a6a75e6";
        private const string ApiSecret = "c590a94236d96eded95c2ae1efbbbb4e6479ef1d";
        private const string connectionString = @"Data Source=DESKTOP-2AFIS8M\SQLEXPRESS;Initial Catalog=IntegrationDemoDb;Integrated Security=True;";
        private Dictionary<int, int> apiCategoryIds = new Dictionary<int, int>();
        public CategoriesAddApiForm()
        {
            InitializeComponent();
        }
        /*veritabanından kategorileri getir */
        private async Task<List<CategoryDto>> GetCategoryFromDatabase(bool isAll = true)
        {
            List<CategoryDto> categories = new List<CategoryDto>();
            using (var connection = new SqlConnection(connectionString))
            {
                string getAllSqlCommand = "SELECT Id, Name ,  ParentId FROM Categories";
                string getNewSqlCommand = "SELECT Id, Name ,  ParentId FROM Categories WHERE ResponseId=0";
                await connection.OpenAsync();
                using (var command = new SqlCommand(isAll ? getAllSqlCommand : getNewSqlCommand, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            int parentId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            categories.Add(new CategoryDto
                            {
                                Id = id,
                                Name = name,
                                ParentId = parentId

                            });
                        }
                    }
                }
            }
            return categories;
        }
        /*Kategorileri Apiye gönder*/
        private async Task<int> SendCategoryToApi(ApiCategoryDto apiCategory)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Add("Dehasoft-Api-Key", ApiKey);
                request.Headers.Add("Dehasoft-Api-Secret", ApiSecret);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(apiCategory);
                var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
                request.Content = content;

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Kategori {apiCategory.name} başarıyla gönderildi.");

                    // API'den gelen yanıtı inceleyin
                    var respCon = await response.Content.ReadAsStringAsync();
                    dynamic responseContent = JsonConvert.DeserializeObject(respCon);
                    Console.WriteLine($"API Yanıtı: {responseContent}");
                    return responseContent.category.id; 
                }
                else
                {
                    Console.WriteLine($"Kategori {apiCategory.name} gönderilirken hata oluştu: {response.ReasonPhrase}");

                    // Hata mesajını inceleyin
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Hata Mesajı: {errorMessage}");
                }
            }
            return 0; // Hata durumunda 0 döndür

        }
        private async Task SendCategoriesToApi()
        {
            try
            {
                // Veritabanından kategorileri al
                List<CategoryDto> categories = await GetCategoryFromDatabase(false);

                // Üst kategorileri (ParentId = 0) işleyin
                foreach (var category in categories.Where(c => c.ParentId == 0))
                {
                    await SendCategoriesToApiRecursive(category);
                }

                MessageBox.Show("Kategoriler API'ye gönderildi başarılı");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategoriler API'ye gönderilirken hata oluştu: {ex.Message}", "Hata");
            }
        }
        private async Task UpdateApiResponseId(int categoryId, int responseId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("UPDATE Categories SET ResponseId = @responseId WHERE Id = @categoryId", connection))
                {
                    command.Parameters.AddWithValue("@responseId", responseId);
                    command.Parameters.AddWithValue("@categoryId", categoryId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task SendCategoriesToApiRecursive(CategoryDto category)
        {
            var apiCategory = new ApiCategoryDto()
            {
                name = category.Name,
                parent_id = apiCategoryIds.GetValueOrDefault(category.ParentId, 0),
                description = null
            };
            var responseId = await SendCategoryToApi(apiCategory);

            // API responseId'sini dictionary'ye ekle
            apiCategoryIds[category.Id] = responseId;

            // Veritabanına API ID'sini güncelle
            await UpdateApiResponseId(category.Id, responseId);

            // Alt kategorileri yinelemeli olarak gönder
            var childCategories = (await GetCategoryFromDatabase(false)).Where(c => c.ParentId == category.Id);
            foreach (var childCategory in childCategories)
            {
                // Alt kategoriler için ebeveyn ID'sini doğru şekilde al
                // Yeni bir apiCategory nesnesi oluştur ve parent_id'yi ayarla
                apiCategory = new ApiCategoryDto
                {
                    name = childCategory.Name,
                    parent_id = apiCategoryIds.GetValueOrDefault(childCategory.ParentId, 0), // Doğru ebeveyn ID'si
                    description = null
                };
                await SendCategoriesToApiRecursive(childCategory);
            }
        }

        private async void btnAddCategory_Click(object sender, EventArgs e)
        {
            await SendCategoriesToApi();
        }
    }
}
