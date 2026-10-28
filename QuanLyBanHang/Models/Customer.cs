using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyBanHang.Models
{
    public class Customer
    {
        [Required]
        public string _id { get; set; }

        public string _rev { get; set; }

        public string type { get; set; }

        [Required]
        public string name { get; set; }

        public string phone { get; set; }

        [EmailAddress]
        public string email { get; set; }

        public string address { get; set; }
    }
}