using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyBanHang.Models
{
    public class Delivery
    {
        public string _id { get; set; }

        public string _rev { get; set; }

        public string type { get; set; }

        public string orderId { get; set; }

        public string shipperId { get; set; }

        public string shipperName { get; set; }

        public string shipperPhone { get; set; }

        public string status { get; set; }

        public string pickupDate { get; set; }

        public string expectedDate { get; set; }

        public string address { get; set; }
    }
}