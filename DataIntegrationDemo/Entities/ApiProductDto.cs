using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataIntegrationDemo.Entities
{
    public class ApiProductDto
    {
        public string name { get; set; }
        public List<string> product_images { get; set; }
        public string description { get; set; }
        public List<int> category_ids { get; set; }
        public int brand_id { get; set; }
    }
}
