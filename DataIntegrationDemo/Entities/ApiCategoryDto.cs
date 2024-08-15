using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataIntegrationDemo.Entities
{
    public class ApiCategoryDto
    {
        public string name { get; set; }
        public int parent_id { get; set; }
        public string description { get; set; }
    }
}
