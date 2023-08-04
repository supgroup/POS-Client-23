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
    public class Serial
    {
        #region attributes
        public int serialId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public string serialNum { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public bool isSold { get; set; }
        public Nullable<int> branchId { get; set; }

        public bool isManual { get; set; }
        public Boolean canDelete { get; set; }
        public string itemName { get; set; }
        public string unitName { get; set; }
        public string branchName { get; set; }

        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public string name { get; set; }
        public Nullable<int> count { get; set; }
        public Nullable<int> quantity { get; set; }
        public bool isSelected { get; set; }
        #endregion


        #region Methods
        public async Task<List<Serial>> GetSerialsByIsSold(bool isSold,int itemUnitId,int branchId)
        {
            List<Serial> items = new List<Serial>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("isSold", isSold.ToString());
            parameters.Add("itemUnitId", itemUnitId.ToString());
            parameters.Add("branchId", branchId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetSerialsByIsSold", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Serial>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<Serial>> GetMainInvoiceSerials(int mainInvoiceId, int itemUnitId)
        {
            List<Serial> items = new List<Serial>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemUnitId", itemUnitId.ToString());
            parameters.Add("mainInvoiceId", mainInvoiceId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetMainInvoiceSerials", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Serial>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<int> SerialCanAdded(List<Serial> selials)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Serials/SerialsCanAdded";
            var myContent = JsonConvert.SerializeObject(selials);
            parameters.Add("serials", myContent);

            return (int)await APIResult.post(method, parameters);
        }

        internal Task<IEnumerable<Serial>> Get()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Serial>> GetAll()
        {
            List<Serial> items = new List<Serial>();
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Serial>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<List<Serial>> GetIUbyBranch(int branchId)
        {
            List<Serial> items = new List<Serial>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
        
            parameters.Add("branchId", branchId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetIUbyBranch", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Serial>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        //GetIUBranchWithCount
        public async Task<List<ItemUnit>> GetIUBranchWithCount(int branchId)
        {
            List<ItemUnit> items = new List<ItemUnit>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("branchId", branchId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetIUBranchWithCount", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<Serial>> GetSerialsAvailable(int branchId,int itemUnitId)
        {
            List<Serial> items = new List<Serial>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("branchId", branchId.ToString());
            parameters.Add("itemUnitId", itemUnitId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetSerialsAvailable", parameters);


            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Serial>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<List<StoreProperty>> GetPropertiesAvailable(int branchId, int itemUnitId)
        {
            List<StoreProperty> items = new List<StoreProperty>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("branchId", branchId.ToString());
            parameters.Add("itemUnitId", itemUnitId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Serials/GetPropertiesAvailable", parameters);


            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<StoreProperty>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        public async Task<decimal> Save(Serial item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Serials/Save";
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
            string method = "Serials/Delete";
            return await APIResult.post(method, parameters);
        }
        public async Task<int> SaveSerialsList(List<string> selialsLst,Serial serialObj)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Serials/SaveSerialsList";
            var myContent = JsonConvert.SerializeObject(selialsLst);
            parameters.Add("selialsLst", myContent);
            var myContent1 = JsonConvert.SerializeObject(serialObj);
            parameters.Add("itemObject", myContent1);
            return (int)await APIResult.post(method, parameters);
        }

        public async Task<int> UpdateSerialsList(List<Serial> selialsLst)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Serials/UpdateSerialsList";
            var myContent = JsonConvert.SerializeObject(selialsLst);
            parameters.Add("selialsLst", myContent);
           
            return (int)await APIResult.post(method, parameters);
        }
        public async Task<int> SoldSerialsList(List<string> selialsLst,int userId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Serials/SoldSerialsList";
            var myContent = JsonConvert.SerializeObject(selialsLst);
            parameters.Add("selialsLst", myContent);
            parameters.Add("userId", userId.ToString());
            return (int)await APIResult.post(method, parameters);
        }
        #endregion

    }
}
