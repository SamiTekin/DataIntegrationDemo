using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataIntegrationDemo.Entities
{
    public class ProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> ProductImages { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
    }
}
