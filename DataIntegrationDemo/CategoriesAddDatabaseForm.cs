using DataIntegrationDemo.Entities;
using System.Data.SqlClient;
using System.Text.Json;

namespace DataIntegrationDemo
{
    public partial class CategoriesAddDatabaseForm : Form
    {
        private string connectionString = @"Data Source=DESKTOP-2AFIS8M\SQLEXPRESS;Initial Catalog=IntegrationDemoDb;Integrated Security=True;";
        public CategoriesAddDatabaseForm()
        {
            InitializeComponent();
        }
        private int InsertCategory(SqlConnection connection, string name, int? parentId)
        {
            // Önce kategori adýnýn veritabanýnda olup olmadýðýný kontrol et
            string checkQuery = @"SELECT Id FROM Categories WHERE Name = @Name";
            using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@Name", name);
                object existingCategoryId = checkCommand.ExecuteScalar();

                // Eðer kategori zaten varsa, ID'sini döndür
                if (existingCategoryId != null)
                {
                    return Convert.ToInt32(existingCategoryId);
                }
            }

            // Eðer kategori yoksa, yeni bir kategori ekle
            string insertQuery = @"INSERT INTO Categories (Name, ParentId) VALUES(@Name, @ParentId); SELECT SCOPE_IDENTITY();";
            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@ParentId", parentId.HasValue ? parentId.Value : DBNull.Value);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        private void InsertSubcategoriesToDatabase(SqlConnection connection, List<Subcategory> subcategories, int? parentId)
        {
            if (subcategories == null) return;

            foreach (var subcategory in subcategories)
            {
                int subcategoryId = InsertCategory(connection, subcategory.name, parentId);

                // Eðer alt kategorinin de alt kategorileri varsa, özyinelemeli olarak ekle
                InsertSubcategoriesToDatabase(connection, subcategory.subcategories, subcategoryId);
            }
        }
        private void InsertCategoriesToDatabase(List<Product> products) 
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var product in products)
                {
                    // Ana kategorileri ekle
                    int rootCategoryId = InsertCategory(connection, product.categories.name, null);

                    // Alt kategorileri ekle
                    InsertSubcategoriesToDatabase(connection, product.categories.subcategories, rootCategoryId);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string jsonFilePath = @"C:\Users\Sami\Desktop\spor-outdoor.json";
            string jsonString=File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsonString);
            InsertCategoriesToDatabase(products);
            MessageBox.Show("Kategoriler Baþarýyla Eklendi");
        }
    }
}






