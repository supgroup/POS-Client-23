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
    public class InvoiceTypesPrinters
    {
        #region properties
        public int invTypePrinterId { get; set; }
        public Nullable<int> printerId { get; set; }
        public Nullable<int> invoiceTypeId { get; set; }
        public string invoiceTypeName { get; set; }
        public string invoiceTypeNameTranslate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(invoiceTypeName))
                    return (MainWindow.resourcemanager.GetString(invoiceTypeName));
                else return "";
            }
        }
        public Nullable<int> sizeId { get; set; }
        public string sizeName { get; set; }
        public string notes { get; set; }
        public int copyCount { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public string printerName { get; set; }
        public string printerSysName { get; set; }
        public string invoiceType { get; set; }
        public string sizeValue { get; set; }
        #endregion
        #region Methods
        public async Task<List<InvoiceTypesPrinters>> GetAll()
        {
            List<InvoiceTypesPrinters> items = new List<InvoiceTypesPrinters>();
            IEnumerable<Claim> claims = await APIResult.getList("invoiceTypesPrinters/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<InvoiceTypesPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<InvoiceTypesPrinters> GetById(int itemId)
        {
            InvoiceTypesPrinters item = new InvoiceTypesPrinters();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("invoiceTypesPrinters/GetById", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<InvoiceTypesPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }

        public async Task<decimal> Save(InvoiceTypesPrinters item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "invoiceTypesPrinters/Save";
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
            string method = "invoiceTypesPrinters/Delete";
            return await APIResult.post(method, parameters);
        }
        public async Task<List<InvoiceTypesPrinters>> GetByPrinterId(int printerId)
        {
            List<InvoiceTypesPrinters> list = new List<InvoiceTypesPrinters>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", printerId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("invoiceTypesPrinters/GetByPrinterId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<InvoiceTypesPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

        }
        public async Task<decimal> updateListByPrinterId(int printerId, List<int> newlist, int userId)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "invoiceTypesPrinters/updateListByPrinterId";
            var myContent = JsonConvert.SerializeObject(newlist);
            parameters.Add("Object", myContent);
            parameters.Add("printerId", printerId.ToString());
            parameters.Add("userId", userId.ToString());
            return await APIResult.post(method, parameters);
            #endregion
        }
        public async Task<List<InvoiceTypesPrinters>> GetByPosForPrint(int posId)
        {
            List<InvoiceTypesPrinters> list = new List<InvoiceTypesPrinters>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", posId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("invoiceTypesPrinters/GetByPosForPrint", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<InvoiceTypesPrinters>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            foreach (InvoiceTypesPrinters row in list)
            {
                row.printerSysName = SectionData.convertToPrinterName(row.printerSysName);
            }

            return list;

        }
        public async Task refreshPrinters()
        {
            List<InvoiceTypesPrinters> list = new List<InvoiceTypesPrinters>();
            list = await GetByPosForPrint(MainWindow.posLogIn.posId);
            FillCombo.printersList = list;

        }
    }
}
