using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace POS.Classes
{
    class Service
    {
        public int costId { get; set; }
        public string name { get; set; }
        public Nullable<decimal> costVal { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        //*****************************************************
        // get all item services
        public async Task<List<Service>> GetItemServices(int itemId)
        {
            List<Service> items = new List<Service>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("servicesCosts/Get", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Service>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        //*********************************************//
        // ******************add service to item
        public async Task<decimal> save(Service item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "servicesCosts/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        //**********************************************
        // call api method to delete item service
        public async Task<decimal> delete(int costId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", costId.ToString());
            string method = "servicesCosts/Delete";
           return await APIResult.post(method, parameters);
        }
    }
}
