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

namespace POS.Classes
{
    public class PropertiesItems
    {
        public int propertyItemId { get; set; }
        public string propertyItemName { get; set; }
        public string name { get; set; }
        public Nullable<int> propertyId { get; set; }
        public string propertyName { get; set; }
        public Nullable<short> isDefault { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Boolean canDelete { get; set; }

        public async Task<List<PropertiesItems>> Get()
        {
            List<PropertiesItems> items = new List<PropertiesItems>();
            IEnumerable<Claim> claims = await APIResult.getList("propertiesItems/Get");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<PropertiesItems>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        //********************************************************
        //****************** get values of property
        public async Task<PropertiesItems> getById(int itemId)
        {
            PropertiesItems item = new PropertiesItems();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("PropertiesItems/GetPropItemByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<PropertiesItems>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
        public async Task<List<PropertiesItems>> GetPropertyItems(int propertyId)
        {
            List<PropertiesItems> items = new List<PropertiesItems>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", propertyId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("propertiesItems/GetPropertyItems", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<PropertiesItems>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        // adding or editing  category by calling API metod "save"
        // if propertyItemId = 0 will call save else call edit
        public async Task<decimal> save(PropertiesItems item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "propertiesItems/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> delete(int propertyItemId, int userId, Boolean final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", propertyItemId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "propertiesItems/Delete";
           return await APIResult.post(method, parameters);
        }
    }
}
