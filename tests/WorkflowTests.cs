using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuanLyBanHang.Services;
using QuanLyBanHang.Models;

class MemoryCouch : HttpMessageHandler
{
    public Dictionary<string,JObject> Docs=new Dictionary<string,JObject>();
    public bool FailProjection, FailAck;
    public int ProjectionWrites;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req,CancellationToken token)
    {
        var id=Uri.UnescapeDataString(req.RequestUri.AbsolutePath.Substring("/unit/".Length));
        JObject doc; Docs.TryGetValue(id,out doc);
        if(req.Method==HttpMethod.Get) return Reply(doc==null?404:200,doc==null?"{}":doc.ToString());
        var data=JObject.Parse(req.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        if((string)data["type"]=="delivery" && FailProjection) { FailProjection=false; return Reply(500,"{}"); }
        if((string)data["type"]=="order" && data["pendingDelivery"].Type==JTokenType.Null && FailAck) { FailAck=false;return Reply(500,"{}"); }
        if(doc!=null && (string)data["_rev"]!=(string)doc["_rev"]) return Reply(409,"{}");
        if((string)data["type"]=="delivery") ProjectionWrites++;
        data["_rev"]=Guid.NewGuid().ToString("N"); Docs[id]=data;
        return Reply(201,"{\"ok\":true}");
    }
    static Task<HttpResponseMessage> Reply(int status,string json) { return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status){Content=new StringContent(json)}); }
}
class WorkflowTests
{
    static int checks;
    static void Check(bool value,string label) { if(!value)throw new Exception(label);checks++; }
    static void Error(Action action) { try{action();throw new Exception("Expected failure");}catch(CouchDbException){checks++;} }
    static JObject Pending(string id,string target) { return new JObject { ["_id"]=id,["_rev"]="1-unit",["type"]="order",["status"]="preparing",
        ["pendingDelivery"]=new JObject{["token"]=Guid.NewGuid().ToString("N"),["delete"]=false,["expectedRev"]=null,
            ["target"]=new JObject{["_id"]=target,["type"]="delivery",["orderId"]=id,["status"]="pending"}} }; }
    static int Main()
    {
        try {
            Check(WorkflowService.OrderTransition("processing","confirmed"),"confirm");
            Check(WorkflowService.OrderTransition("confirmed","preparing"),"prepare");
            Check(!WorkflowService.OrderTransition("processing","delivered"),"no skip");
            Check(!WorkflowService.OrderTransition("delivered","processing"),"terminal order");
            Check(WorkflowService.DeliveryTransition("shipping","failed")&&WorkflowService.DeliveryTransition("failed","assigned"),"retry");
            Check(!WorkflowService.DeliveryTransition("delivered","shipping"),"terminal delivery");
            var user=AppSecurity.NewUser("unit",Guid.NewGuid().ToString("N"),"Administrator");
            Check(user.passwordHash!=null&&Convert.FromBase64String(user.salt).Length==16,"salt/hash");
            Check(!AppSecurity.Verify(user,"incorrect-password"),"wrong password");
            Check(!AppSecurity.Strong("short"),"minimum password");
            Environment.SetEnvironmentVariable("COUCHDB_URL","http://127.0.0.1:5984");
            Environment.SetEnvironmentVariable("COUCHDB_DATABASE","unit");
            Environment.SetEnvironmentVariable("COUCHDB_USERNAME","unit");
            Environment.SetEnvironmentVariable("COUCHDB_PASSWORD","unit-fake");
            var handler=new MemoryCouch();
            using(var client=new HttpClient(handler)) {
                var workflow=new WorkflowService(new CouchDbService(client));
                handler.Docs["o"]=Pending("o","d");handler.FailProjection=true;
                Error(()=>workflow.Repair("o").GetAwaiter().GetResult());
                Check(handler.Docs["o"]["pendingDelivery"] is JObject&&!handler.Docs.ContainsKey("d"),"intent survives projection failure");
                Error(()=>workflow.SaveOrder(new Order{_id="o",_rev="1-unit"},true,"unit").GetAwaiter().GetResult());
                workflow.Repair("o").GetAwaiter().GetResult();
                Check(handler.Docs["o"]["pendingDelivery"].Type==JTokenType.Null&&handler.Docs.ContainsKey("d"),"recovered");
                workflow.Repair("o").GetAwaiter().GetResult();Check(handler.ProjectionWrites==1,"idempotent retry");
                handler.Docs["o2"]=Pending("o2","d2");handler.FailAck=true;
                Error(()=>workflow.Repair("o2").GetAwaiter().GetResult());
                Check(handler.Docs["o2"]["pendingDelivery"] is JObject&&handler.Docs.ContainsKey("d2"),"ack failure retains intent");
                var rev=(string)handler.Docs["d2"]["_rev"];
                workflow.Repair("o2").GetAwaiter().GetResult();
                Check((string)handler.Docs["d2"]["_rev"]==rev&&handler.Docs["o2"]["pendingDelivery"].Type==JTokenType.Null,"ack retry does not duplicate write");
                handler.Docs["o3"]=Pending("o3","d2");
                Error(()=>workflow.Repair("o3").GetAwaiter().GetResult());
                Check((string)handler.Docs["d2"]["_rev"]==rev&&handler.Docs["o3"]["pendingDelivery"] is JObject,"external conflict preserved");
            }
            Console.WriteLine("PASS workflow/security "+checks+" assertions (unit, injected HTTP failures)");return 0;
        } catch(Exception ex) { Console.Error.WriteLine("FAIL "+ex.GetType().Name+" "+ex.Message);return 1; }
    }
}
