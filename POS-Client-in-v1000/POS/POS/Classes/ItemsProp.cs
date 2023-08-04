using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace POS.Classes
{   
   public class ItemsProp
    {
        public int itemPropId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public Nullable<int> propertyId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> itemUnitId { get; set; }

        public string propValue { get; set; }
        public string propName { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        public async Task<decimal> Save(ItemsProp itemsProp)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "ItemsProp/Save";

            var myContent = JsonConvert.SerializeObject(itemsProp);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

 
        }

        public async Task<decimal> Delete(int itemPropId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemPropId", itemPropId.ToString());
       
            string method = "itemsProp/Delete";
           return await APIResult.post(method, parameters);


 
        }

        public async Task<List<ItemsProp>> Get(int itemId)
        {


            List<ItemsProp> list = new List<ItemsProp>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("itemsProp/Get", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemsProp>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
 
        }

      
        public async Task<List<ItemsProp>> GetByItemUnitId(int itemUnitId)
        {


            List<ItemsProp> list = new List<ItemsProp>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemUnitId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("itemsProp/GetByItemUnitId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemsProp>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

        }
    }
}
