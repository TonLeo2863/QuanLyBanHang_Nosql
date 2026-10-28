using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyBanHang.Models
{
    public class Product
    {
        [Required]
        public string _id { get; set; }

        public string _rev { get; set; }

        public string type { get; set; }

        [Required]
        public string name { get; set; }

        public string category { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal price { get; set; }

        [Range(0, int.MaxValue)]
        public int quantity { get; set; }

        public string status { get; set; }
    }
}