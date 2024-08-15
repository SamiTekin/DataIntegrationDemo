using DataIntegrationDemo.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataIntegrationDemo
{
    public partial class ProductAddApiForm : Form
    {
        private const string ApiUrl = "https://dehapi.com/api/seller/product/add";
        private const string ApiKey = "0eaf70d3def343b9e7fa2572ac8f40fcc56141ae8b30bd2a6a75e6";
        private const string ApiSecret = "c590a94236d96eded95c2ae1efbbbb4e6479ef1d";
        private const string connectionString = @"Data Source=DESKTOP-2AFIS8M\SQLEXPRESS;Initial Catalog=IntegrationDemoDb;Integrated Security=True;";
        public ProductAddApiForm()
        {
            InitializeComponent();
        }
        private async Task<List<ProductDto>> GetProductsFromJson(string filePath)
        {
            List<ProductDto> products = new List<ProductDto>();
            string jsonString = File.ReadAllText(filePath);
            dynamic jsonData = JsonConvert.DeserializeObject(jsonString);
            foreach (var productData in jsonData)
            {
                string name = productData.name;
                if (string.IsNullOrEmpty(name)) 
                {
                    Console.WriteLine("Ürün adı boş veya eksik! Ürün eklenemiyor.");
                    continue; 
                }
                string description = productData.description;
                List<string> productImages = new List<string>();
                if (productData.product_images != null)
                {
                    
                    productImages = productData.product_images.ToObject<List<string>>();
                }
                else
                {
                    productImages.Add("https://cdn.cimri.io/image/1000x1000/kdr-oppo-reno-2z-cph1951-lcd-ekran_817434148.jpg");
                }
                
                int brandId = await GetBrandIdByName(productData.brand.ToString());
                int categoryId = await GetCategoryIdByName(productData.categories);
                products.Add(new ProductDto
                {
                    Name = name,
                    Description = description,
                    ProductImages = productImages,
                    CategoryId = categoryId,
                    BrandId = brandId
                });
            }
            return products;
        }
        private async Task<int> GetBrandIdByName(string brandName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SELECT ResponseId FROM Brands WHERE Name=@brandName", connection))
                {
                    command.Parameters.AddWithValue("@brandName", brandName);
                    using (var reader= await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }
            }
            return 0;
        }
        private async Task<int> GetCategoryIdByName(dynamic categoryData)
        {
            string categoryName = categoryData.name;
            List<dynamic> subcategories = categoryData.subcategories.ToObject<List<dynamic>>();

            while (subcategories.Count > 0)
            {
                if (subcategories.Count > 0)
                {
                    categoryName = subcategories[0].name;
                    subcategories = subcategories[0].subcategories.ToObject<List<dynamic>>(); 
                }
                else
                {
                    break;
                }
            }
            using (var connection= new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command= new SqlCommand("SELECT ResponseId FROM Categories WHERE Name= @categoryName", connection))
                {
                    command.Parameters.AddWithValue("@categoryName", categoryName);
                    using (var reader= await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }
            }
            return 0;
        }
        private async Task<bool> SendProductToApi(ProductDto product)
        {
            using (var cliend = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Add("Dehasoft-Api-Key", ApiKey);
                request.Headers.Add("Dehasoft-Api-Secret", ApiSecret);
                var apiProduct = new ApiProductDto
                {
                    name = product.Name,
                    description = product.Description,
                    product_images = product.ProductImages,
                    category_ids = new List<int> { product.CategoryId },
                    brand_id = product.BrandId,
                };
                var json = JsonConvert.SerializeObject(apiProduct);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
                request.Content = content;
                var response = await cliend.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Ürün {product.Name} başarıyla gönderildi.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Ürün{product.Name} gönderilirken hata oluştu:{response.ReasonPhrase}");
                    string errorMesage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(errorMesage);
                }
                return false;
            }
        }

        private async void btnAddProduct_Click(object sender, EventArgs e)
        {
            string jsonFilePath = @"C:\Users\Sami\Desktop\urunler.json";
            List<ProductDto> products= await GetProductsFromJson(jsonFilePath);
            foreach (var product in products)
            {
                bool success=await SendProductToApi(product);
                if (success)
                    Console.WriteLine("Ürünler Eklendi");
            }
            MessageBox.Show("Ürünler API'ye eklendi");
        }
    }
}
