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
    public class Warranty
    {
        public int warrantyId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string notes { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Boolean canDelete { get; set; }

        // Methods
        public async Task<List<Warranty>> GetAll()
        {
            List<Warranty> items = new List<Warranty>();
            IEnumerable<Claim> claims = await APIResult.getList("warranty/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Warranty>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<Warranty> getById(int itemId)
        {
            Warranty item = new Warranty();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Id", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("warranty/GetbyId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<Warranty>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
        public async Task<decimal> Save(Warranty item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "warranty/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
            return await APIResult.post(method, parameters);
        }

        public async Task<decimal> Delete(int locationId, int userId, Boolean final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", locationId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "warranty/Delete";
            return await APIResult.post(method, parameters);
        }
    }
}
