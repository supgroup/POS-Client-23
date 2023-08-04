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
    public class Property
    {
        public int propertyId { get; set; } 
        public string name { get; set; }
        public string propertyValues { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Boolean canDelete { get; set; }

        public int propertyIndex { get; set; }

        public List<PropertiesItems> PropertiesItems { get; set; }
        // adding or editing  category by calling API metod "save"
        // if categoryId = 0 will call save else call edit
        public async Task<List<Property>> Get()
        {
            List<Property> items = new List<Property>();
            IEnumerable<Claim> claims = await APIResult.getList("Properties/get");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Property>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<PropertiesItems>> GetPropertyValues(int propertyId)
        {
            List<PropertiesItems> items = new List<PropertiesItems>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", propertyId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Properties/GetPropertyValues", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<PropertiesItems>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<Property> getById(int itemId)
        {
            Property item = new Property();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("property/GetpropertyByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<Property>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
        public async Task<decimal> save(Property item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Properties/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> delete(int propertyId, int userId, Boolean final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", propertyId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "Properties/Delete";
           return await APIResult.post(method, parameters);
        }
    }
}
