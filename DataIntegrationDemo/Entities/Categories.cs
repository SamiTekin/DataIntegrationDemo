using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataIntegrationDemo.Entities
{
    public class Categories
    {
        public string name { get; set; }
        public List<Subcategory> subcategories { get; set; }
    }
}
