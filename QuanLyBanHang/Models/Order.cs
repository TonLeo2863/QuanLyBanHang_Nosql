using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyBanHang.Models
{
    public class Order
    {
        [Required]
        public string _id { get; set; }

        public string _rev { get; set; }

        public string type { get; set; }

        [Required]
        public string customerId { get; set; }

        [Required]
        public string orderDate { get; set; }

        [RegularExpression("^(processing|shipping|delivered|cancelled)$"), Required]
        public string status { get; set; }

        public List<OrderItem> items { get; set; }

        public decimal total { get; set; }

        public string paymentMethod { get; set; }

        public string address { get; set; }
    }
}