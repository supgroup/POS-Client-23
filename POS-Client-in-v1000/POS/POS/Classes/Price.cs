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
    public class Price
    {

        #region properties
        public int priceId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> sliceId { get; set; }
        public string notes { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> basicPrice { get; set; }
        public Nullable<decimal> priceTax { get; set; }
        public string sliceName { get; set; }
        public string name { get; set; }
        public bool canDelete { get; set; }
        public string unitName { get; set; }
        public string itemName { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public bool isSelect { get; set; }
        public string itemType { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public Nullable<decimal> unitCost { get; set; }
        public Nullable<decimal> itemUnitPrice { get; set; }
        #endregion

        #region Methods
        public async Task<List<Price>> GetAll()
        {
            List<Price> items = new List<Price>();
            IEnumerable<Claim> claims = await APIResult.getList("Prices/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Price>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<Price> GetById(int itemId)
        {
            Price item = new Price();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Prices/GetById", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<Price>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }

        public async Task<decimal> Save(Price item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Prices/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
            return await APIResult.post(method, parameters);
        }
        public async Task<decimal> Delete(int itemId, int userId, bool final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "Prices/Delete";
            return await APIResult.post(method, parameters);
        }

        //internal Task<IEnumerable<Price>> getByitemUnitId(int itemUnitId)
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<List<Price>> getByitemUnitId(int itemUnitId)
        {
            List<Price> items = new List<Price>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemUnitId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Prices/getByitemUnitId", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Price>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<List<Price>> getBySliceId(int sliceId)
        {
            List<Price> items = new List<Price>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", sliceId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Prices/getBySliceId", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Price>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        #endregion

    }
}
