using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace QuanLyBanHang.Services
{
    public static class QueryService
    {
        public static async Task<List<T>> Load<T>(Controller controller, CouchDbService db, string type)
        {
            var request=controller.Request;
            var selector=new JObject { ["type"]=type };
            string q=request.QueryString["q"], status=request.QueryString["status"], category=request.QueryString["category"], customer=request.QueryString["customer"];
            if(q!=null && q.Length>100) throw new CouchDbException("Từ khóa tối đa 100 ký tự.");
            if(!string.IsNullOrWhiteSpace(q)) selector[type=="customer"||type=="product"?"name":"_id"]=new JObject { ["$regex"]=Regex.Escape(q) };
            if(!string.IsNullOrWhiteSpace(status)) selector["status"]=status;
            if(type=="product" && !string.IsNullOrWhiteSpace(category)) selector["category"]=category;
            if(type=="order" && !string.IsNullOrWhiteSpace(customer)) selector["customerId"]=customer;
            foreach(var range in new[]{new[]{"min","$gte"},new[]{"max","$lte"}}) {
                var raw=request.QueryString[range[0]]; if(string.IsNullOrWhiteSpace(raw)) continue;
                decimal value; if(!decimal.TryParse(raw,NumberStyles.Number,CultureInfo.InvariantCulture,out value)||value<0) throw new CouchDbException("Khoảng tiền không hợp lệ.");
                if(type=="product"||type=="order") { string field=type=="product"?"price":"total"; if(selector[field]==null) selector[field]=new JObject(); selector[field][range[1]]=value; }
            }
            if(type=="order") foreach(var range in new[]{new[]{"from","$gte"},new[]{"to","$lte"}}) {
                var raw=request.QueryString[range[0]]; if(string.IsNullOrWhiteSpace(raw)) continue;
                DateTime date; if(!DateTime.TryParseExact(raw,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date)) throw new CouchDbException("Khoảng ngày không hợp lệ.");
                if(selector["orderDate"]==null) selector["orderDate"]=new JObject(); selector["orderDate"][range[1]]=raw;
            }
            var actor=AppSecurity.Current(controller.HttpContext);
            if(type=="delivery" && actor!=null && actor.role=="Shipper") selector["shipperId"]=actor.username;
            var page=await db.FindPage<T>(selector,request.QueryString["bookmark"],20);
            controller.ViewBag.NextBookmark=page.docs.Count==20?page.bookmark:null;
            return page.docs;
        }
    }
}
