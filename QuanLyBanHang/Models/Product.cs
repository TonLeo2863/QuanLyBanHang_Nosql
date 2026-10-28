using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyBanHang.Models
{
    public class Product
    {
        public string _id { get; set; }

        public string _rev { get; set; }

        public string type { get; set; }

        public string name { get; set; }

        public string category { get; set; }

        public decimal price { get; set; }

        public int quantity { get; set; }

        public string status { get; set; }
    }
}