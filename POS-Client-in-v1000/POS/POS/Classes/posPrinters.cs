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
    public class PosPrinters
    {
        #region properties
        public int printerId { get; set; }
        public string name { get; set; }
        public string printerName { get; set; }
        public Nullable<int> sizeId { get; set; }
        public string sizeValue { get; set; }
        public int copiesCount { get; set; }
        public string notes { get; set; }
        public Nullable<long> posId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<long> createUserId { get; set; }
        public Nullable<long> updateUserId { get; set; }
        public string purpose { get; set; }
        public byte isActive { get; set; }
        public bool canDelete { get; set; }
        public List<InvoiceTypesPrinters> invoiceTypesPrintersList { get; set; }
        #endregion
        #region Methods
        public async Task<List<PosPrinters>> GetAll()
        {
            List<PosPrinters> items = new List<PosPrinters>();
            IEnumerable<Claim> claims = await APIResult.getList("posPrinters/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<PosPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<PosPrinters> GetById(long itemId)
        {
            PosPrinters item = new PosPrinters();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("posPrinters/GetById", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<PosPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }      
        public async Task<decimal> Save(PosPrinters item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "posPrinters/Save";
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
            string method = "posPrinters/Delete";
            return await APIResult.post(method, parameters);
        }

        public async Task<List<PosPrinters>> GetByPosId(long posId)
        {
            List<PosPrinters> items = new List<PosPrinters>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", posId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("posPrinters/GetByPosId", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<PosPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        #endregion
    }
}
