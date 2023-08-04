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
    public class TaxTypes
    {
        #region properties
        public int taxTypeId { get; set; }
        public string name { get; set; }
        public string nameTr { get; set; }
        public string translate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(nameTr))
                    return (MainWindow.resourcemanager.GetString(nameTr));
                else return "";
            }
        }
        public Nullable<bool> isActive { get; set; }
        public string notes { get; set; }
        #endregion
        #region Methods
        public async Task<List<TaxTypes>> GetAll()
        {
            List<TaxTypes> items = new List<TaxTypes>();
            IEnumerable<Claim> claims = await APIResult.getList("posPrinters/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<TaxTypes>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
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
