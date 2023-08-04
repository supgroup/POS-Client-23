using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims; 
using System.Web;

namespace POS.Classes
{
    public class Taxes
    {
        #region properties
        public int taxId { get; set; }
        public string name { get; set; }
        public string nameAr { get; set; }
        public Nullable<decimal> rate { get; set; }
        public Nullable<int> taxTypeId { get; set; }
        public string taxType { get; set; }
        public string taxTypeName { get; set; }
        public Nullable<bool> isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public bool canDelete { get; set; }
        #endregion
        #region Methods
        public async Task<List<Taxes>> GetAll()
        {
            List<Taxes> items = new List<Taxes>();
            IEnumerable<Claim> claims = await APIResult.getList("Taxes/Get");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Taxes>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<Taxes> GetById(long itemId)
        {
            Taxes item = new Taxes();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Taxes/GetById", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<Taxes>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }      
        public async Task<decimal> Save(Taxes item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Taxes/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
            return await APIResult.post(method, parameters);
        }
        public async Task<decimal> Delete(long itemId, long userId, bool final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "Taxes/Delete";
            return await APIResult.post(method, parameters);
        }

 
        #endregion
    }
}
