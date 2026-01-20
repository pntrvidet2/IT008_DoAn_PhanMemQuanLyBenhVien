using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Models
{
    public class Drug
    {
        public string Drug_ID { get; set; } = "";
        public string Drug_Name { get; set; } = "";
        public string Drug_Unit { get; set; } = "";
        public decimal Drug_Price { get; set; }
        public int Stock_Quantity { get; set; }
    }
}
