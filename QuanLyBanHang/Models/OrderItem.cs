using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyBanHang.Models
{
    public class OrderItem
    {
        public string productId { get; set; }

        public int quantity { get; set; }

        public decimal price { get; set; }
    }
}