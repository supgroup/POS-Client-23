using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS;
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
    public class InventoryItemLocation
    {
        public int sequence { get; set; }
        public int id { get; set; }
        public Nullable<bool> isDestroyed { get; set; }
        public Nullable<bool> isFalls { get; set; }
        public Nullable<int> amount { get; set; }
        public Nullable<int> amountDestroyed { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> itemLocationId { get; set; }
        public Nullable<int> inventoryId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Boolean canDelete { get; set; } 
        public string itemName { get; set; }
        public string location { get; set; }
        public string section { get; set; }
        public string unitName { get; set; }
        public int itemId { get; set; }
        public int itemUnitId { get; set; }
        public int unitId { get; set; }
        public string inventoryNum { get; set; }
        public Nullable<System.DateTime> inventoryDate { get; set; }
        public string itemType { get; set; }
        public string cause { get; set; }
        public string fallCause { get; set; }
        public Nullable<decimal> avgPurchasePrice { get; set; }
        public Nullable<decimal> total { get; set; }
        public async Task<List<InventoryItemLocation>> GetAll(int itemId)
        {
            List<InventoryItemLocation> items = new List<InventoryItemLocation>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("InventoryItemLocation/Get", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<InventoryItemLocation>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<InventoryItemLocation> getById(int itemId)
        {
            InventoryItemLocation item = new InventoryItemLocation();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("InventoryItemLocation/GetByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<InventoryItemLocation>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
        public async Task<List<InventoryItemLocation>> GetItemToDestroy(int branchId)
        {
            List<InventoryItemLocation> items = new List<InventoryItemLocation>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", branchId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("InventoryItemLocation/GetItemToDestroy", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<InventoryItemLocation>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<InventoryItemLocation>> GetShortageItem(int branchId)
        {
            List<InventoryItemLocation> items = new List<InventoryItemLocation>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", branchId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("InventoryItemLocation/GetShortageItem", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<InventoryItemLocation>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<decimal> save(List<InventoryItemLocation> newObject, int inventoryId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "InventoryItemLocation/Save";
            var myContent = JsonConvert.SerializeObject(newObject);
            parameters.Add("itemObject", myContent);
            parameters.Add("inventoryId", inventoryId.ToString());
           
           return await APIResult.post(method, parameters);
        }
        //public async Task<decimal> distroyItem(InventoryItemLocation item) 
        //{
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    string method = "InventoryItemLocation/distroyItem";
        //    var myContent = JsonConvert.SerializeObject(item);
        //    parameters.Add("itemObject", myContent);
        //   return await APIResult.post(method, parameters);
        //}
        public async Task<decimal> fallItem(InventoryItemLocation item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "InventoryItemLocation/fallItem";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> delete(int itemId, int userId, Boolean final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "InventoryItemLocation/Delete";
           return await APIResult.post(method, parameters);
        }

    }
}

