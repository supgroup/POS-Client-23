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
    public class InvoiceTypes
    {
        #region properties
        public int invoiceTypeId { get; set; }
        public string invoiceType { get; set; }
        public string department { get; set; }
        public string notes { get; set; }
        public string translate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(notes))
                    return (MainWindow.resourcemanager.GetString(notes));
                else return "";
            }
        }
        public string allowPaperSize { get; set; }
        public bool isActive { get; set; }

        #endregion
        #region Methods
        public async Task<List<InvoiceTypes>> GetAll()
        {
            List<InvoiceTypes> items = new List<InvoiceTypes>();
            IEnumerable<Claim> claims = await APIResult.getList("InvoiceTypes/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<InvoiceTypes>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<InvoiceTypes> GetById(int itemId)
        {
            InvoiceTypes item = new InvoiceTypes();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("InvoiceTypes/GetById", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<InvoiceTypes>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }      
        public async Task<decimal> Save(InvoiceTypes item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "InvoiceTypes/Save";
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
            string method = "InvoiceTypes/Delete";
            return await APIResult.post(method, parameters);
        }

  
        public async Task<List<InvoiceTypes>> GetTypesOfPrinter(int printerId)
        {
            List<InvoiceTypes> list = new List<InvoiceTypes>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", printerId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("InvoiceTypes/GetTypesOfPrinter", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<InvoiceTypes>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

        }
        #endregion
    }
}
