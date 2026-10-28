using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models
{
    // A retail-money contract compatible with JSON number round trips in CouchDB.
    public class MoneyAttribute : ValidationAttribute
    {
        public const decimal Maximum = 999999999999.99m;
        public MoneyAttribute() : base("Số tiền phải từ 0 đến 999999999999.99 và tối đa 2 chữ số thập phân.") { }
        public static bool Valid(decimal amount) { return amount >= 0 && amount <= Maximum && decimal.Round(amount,2)==amount; }
        public override bool IsValid(object value) { return value is decimal && Valid((decimal)value); }
    }
}
