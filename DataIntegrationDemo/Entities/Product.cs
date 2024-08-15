using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataIntegrationDemo.Entities
{
    public class Product
    {
        public string name { get; set; }
        public string price { get; set; }
        public string brand { get; set; }
        public string description { get; set; }
        public Categories categories { get; set; }
        public List<string> images { get; set; }
    }
}
