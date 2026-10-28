using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuanLyBanHang.Models;

namespace QuanLyBanHang.Services
{
    // Order is the aggregate authority. pendingDelivery is a durable outbox/lock.
    // A failed projection remains visible and blocks new writes until administrator retries.
    public class WorkflowService
    {
        private readonly CouchDbService db;
        public WorkflowService() : this(new CouchDbService()) { }
        public WorkflowService(CouchDbService service) { db=service; }
        private static void Require(bool condition, string message) { if (!condition) throw new CouchDbException(message); }
        public static bool OrderTransition(string from, string to)
        {
            return from==to || (from=="processing" && (to=="confirmed" || to=="cancelled")) ||
                (from=="confirmed" && (to=="preparing" || to=="cancelled")) || (from=="preparing" && to=="cancelled");
        }
        public static bool DeliveryTransition(string from, string to)
        {
            return from==to || (from=="pending" && (to=="assigned" || to=="cancelled")) ||
                (from=="assigned" && (to=="shipping" || to=="cancelled")) ||
                (from=="shipping" && (to=="delivered" || to=="failed")) ||
                ((from=="failed" || from=="cancelled") && to=="assigned");
        }
        private static void History(JObject doc, string status, string actor)
        {
            var history=doc["history"] as JArray ?? new JArray();
            history.Add(new JObject { ["status"]=status, ["at"]=DateTime.UtcNow.ToString("o"), ["actor"]=actor }); doc["history"]=history;
        }
        private static void Ready(JObject order) { Require(order["pendingDelivery"] == null || order["pendingDelivery"].Type==JTokenType.Null, "Đơn có đồng bộ đang chờ. Quản trị viên cần thử lại ở trang Đồng bộ."); }
        public async Task SaveOrder(Order input, bool editing, string actor)
        {
            JObject original=null;
            if (editing) {
                original=await db.ReadDocument<JObject>(input._id,"order"); Ready(original);
                Require((string)original["_rev"]==input._rev,"Dữ liệu đã thay đổi. Tải lại trang sửa.");
                Require(OrderTransition((string)original["status"],input.status),"Chuyển trạng thái đơn không đúng quy trình. Giao/hoàn tất qua phiếu giao.");
                Require(original["deliveryId"]==null || original["deliveryId"].Type==JTokenType.Null || string.IsNullOrEmpty((string)original["deliveryId"]),"Đơn đã có phiếu giao; cập nhật qua phiếu giao và giữ nguyên nội dung đơn.");
            } else Require(input.status=="processing","Đơn mới phải ở trạng thái processing.");
            DateTime date;
            Require(DateTime.TryParseExact(input.orderDate,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out date),"Ngày đặt hàng không hợp lệ.");
            await db.ReadDocument<Customer>(input.customerId,"customer");
            input.items=(input.items ?? new System.Collections.Generic.List<OrderItem>()).Where(x=>x!=null && !(string.IsNullOrWhiteSpace(x.productId)&&x.quantity==0&&x.price==0)).ToList();
            Require(input.items.Count>0,"Đơn cần ít nhất một sản phẩm.");
            decimal total=0;
            foreach(var item in input.items) {
                Require(!string.IsNullOrWhiteSpace(item.productId)&&item.quantity>0&&MoneyAttribute.Valid(item.price),"Sản phẩm/số lượng/giá không hợp lệ; giá tối đa 999999999999.99, 2 số thập phân.");
                await db.ReadDocument<Product>(item.productId,"product");
                try { total=checked(total+checked(item.quantity*item.price)); } catch(OverflowException) { throw new CouchDbException("Tổng tiền vượt giới hạn."); }
            }
            Require(MoneyAttribute.Valid(total),"Tổng tiền vượt giới hạn 999999999999.99 hoặc quá 2 số thập phân.");
            input.type="order"; input.total=total;
            var doc=JObject.FromObject(input);
            if(original!=null) doc["history"]=original["history"];
            if(original==null || (string)original["status"]!=input.status) History(doc,input.status,actor);
            if(editing) await db.UpdateDocument(input._id,doc); else await db.CreateDocument(input._id,doc);
        }
        public async Task SaveDelivery(Delivery input, bool editing, AppUser actor)
        {
            Require(!string.IsNullOrWhiteSpace(input._id) && !input._id.StartsWith("_") && input._id!="." && input._id!="..", "Mã phiếu giao không hợp lệ.");
            JObject previous=null;
            if(editing) {
                previous=await db.ReadDocument<JObject>(input._id,"delivery");
                Require((string)previous["_rev"]==input._rev,"Phiếu giao đã thay đổi. Tải lại.");
                Require((string)previous["orderId"]==input.orderId,"Không được đổi đơn hàng của phiếu giao.");
                Require(DeliveryTransition((string)previous["status"],input.status),"Chuyển trạng thái giao hàng không hợp lệ.");
            } else {
                Require(input.status=="pending","Phiếu mới phải ở trạng thái pending.");
                bool exists=true;
                try { await db.GetDocument(input._id); } catch(CouchDbException ex) { if(ex.StatusCode==404) exists=false; else throw; }
                Require(!exists,"Mã phiếu đã tồn tại; không thay đổi đơn hàng.");
            }
            if(actor.role=="Shipper") {
                Require(editing && (string)previous["shipperId"]==actor.username,"Chỉ được cập nhật phiếu được phân công.");
                Require(((string)previous["status"]=="assigned" && input.status=="shipping") || ((string)previous["status"]=="shipping" && (input.status=="delivered"||input.status=="failed")),"Nhân viên giao hàng chỉ được bắt đầu hoặc kết thúc giao hàng.");
                var status=input.status; input=previous.ToObject<Delivery>(); input.status=status;
            }
            var order=await db.ReadDocument<JObject>(input.orderId,"order"); Ready(order);
            if(!editing) {
                Require((string)order["status"]=="preparing","Chỉ tạo phiếu cho đơn preparing.");
                Require(!await db.HasReference("delivery","orderId",input.orderId),"Đơn đã có phiếu giao.");
            } else {
                if(string.IsNullOrEmpty((string)order["deliveryId"])) {
                    var linked=await db.FindPage<JObject>(new JObject { ["type"]="delivery",["orderId"]=input.orderId },null,2);
                    Require(linked.docs.Count==1 && (string)linked.docs[0]["_id"]==input._id,"Đơn cũ có nhiều phiếu; cần quản trị viên đối chiếu trước khi sửa.");
                    string previousStatus=(string)previous["status"];
                    string expectedOrder=previousStatus=="shipping"?"shipping":previousStatus=="delivered"?"delivered":"preparing";
                    Require((string)order["status"]==expectedOrder,"Đơn/phiếu cũ chưa khớp quy trình. Cần chuẩn bị đơn và đối chiếu trạng thái trước khi sửa phiếu.");
                } else Require((string)order["deliveryId"]==input._id,"Phiếu không thuộc đơn hàng đang được quản lý.");
            }
            if(input.status=="assigned"||input.status=="shipping"||input.status=="delivered"||input.status=="failed") {
                var shipper=await db.ReadDocument<AppUser>("app-user-"+input.shipperId,"app_user");
                Require(shipper.enabled&&shipper.role=="Shipper","Nhân viên giao phải là tài khoản Shipper đang hoạt động.");
                input.shipperName=shipper.username;
            }
            DateTime pickup, expected;
            if(!string.IsNullOrWhiteSpace(input.pickupDate)) Require(DateTime.TryParseExact(input.pickupDate,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out pickup),"Ngày lấy hàng không hợp lệ.");
            if(!string.IsNullOrWhiteSpace(input.expectedDate)) Require(DateTime.TryParseExact(input.expectedDate,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out expected),"Ngày dự kiến không hợp lệ.");
            if(DateTime.TryParse(input.pickupDate,out pickup)&&DateTime.TryParse(input.expectedDate,out expected)) Require(expected>=pickup,"Ngày dự kiến phải sau ngày lấy hàng.");
            var target=JObject.FromObject(input); target["type"]="delivery";
            target["history"]=previous==null?new JArray():previous["history"] ?? new JArray();
            if(previous==null || (string)previous["status"]!=input.status) History(target,input.status,actor.username);
            var orderStatus=input.status=="shipping"?"shipping":input.status=="delivered"?"delivered":"preparing";
            if((string)order["status"]!=orderStatus) History(order,orderStatus,actor.username);
            order["status"]=orderStatus; order["deliveryId"]=input._id;
            order["pendingDelivery"]=new JObject { ["target"]=target, ["expectedRev"]=previous==null?null:previous["_rev"], ["token"]=Guid.NewGuid().ToString("N"), ["delete"]=false };
            await db.UpdateDocument(input.orderId,order); // CAS serializes all transitions of this order.
            await Repair(input.orderId);
        }
        public async Task DeleteDelivery(string id, string rev, string actor)
        {
            var delivery=await db.ReadDocument<JObject>(id,"delivery");
            Require((string)delivery["_rev"]==rev && (string)delivery["status"]=="pending","Chỉ xóa phiếu pending với revision hiện tại.");
            var order=await db.ReadDocument<JObject>((string)delivery["orderId"],"order"); Ready(order);
            Require((string)order["deliveryId"]==id,"Phiếu cũ chưa được quản lý bởi quy trình mới.");
            order["pendingDelivery"]=new JObject { ["target"]=delivery, ["expectedRev"]=rev, ["token"]=Guid.NewGuid().ToString("N"), ["delete"]=true };
            await db.UpdateDocument((string)order["_id"],order); await Repair((string)order["_id"]);
        }
        public async Task Repair(string orderId)
        {
            var order=await db.ReadDocument<JObject>(orderId,"order"); var pending=order["pendingDelivery"] as JObject;
            if(pending==null) return;
            var target=(JObject)pending["target"].DeepClone(); var id=(string)target["_id"]; var token=(string)pending["token"];
            JObject current=null;
            try { current=await db.ReadDocument<JObject>(id,"delivery"); } catch(CouchDbException ex) { if(ex.StatusCode!=404) throw; }
            if((bool)pending["delete"]) {
                if(current!=null) {
                    Require((string)current["_rev"]==(string)pending["expectedRev"],"Xung đột đồng bộ xóa. Giữ yêu cầu chờ để quản trị viên kiểm tra.");
                    await db.DeleteDocument(id,(string)current["_rev"]);
                }
                order["deliveryId"]=null;
            } else if(current==null || (string)current["syncToken"]!=token) {
                target["syncToken"]=token;
                if(current==null) {
                    Require(pending["expectedRev"]==null || pending["expectedRev"].Type==JTokenType.Null,"Phiếu đã bị xóa bên ngoài; không tự tạo lại.");
                    await db.CreateDocument(id,target);
                } else {
                    Require((string)current["_rev"]==(string)pending["expectedRev"],"Xung đột đồng bộ. Không ghi đè thay đổi bên ngoài.");
                    target["_rev"]=current["_rev"]; await db.UpdateDocument(id,target);
                }
            }
            order["pendingDelivery"]=null;
            await db.UpdateDocument(orderId,order); // Failure here is safe: syncToken makes retry idempotent.
        }
    }
}
