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
    public class StoreProperty
    {
        public int storeProbId { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemsTransId { get; set; }
        public Nullable<int> serialId { get; set; }
        public long count { get; set; }
        public bool isSold { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        #region storePropertyValues attributes
        public int storeProbValueId { get; set; }
        public Nullable<int> propertyId { get; set; }
        public Nullable<int> propertyItemId { get; set; }
        public string propName { get; set; }
        public string propValue { get; set; }
        public int propertyIndex { get; set; }

        #endregion

        #region serial attributes
        public string serialNum { get; set; }

        #endregion
        public List<StorePropertyValue> StorePropertiesValueList { get; set; }
        public string StorePropertiesValueString { get; set; }
        public bool isManual { get; set; }


        #region Methods

        public async Task<List<StoreProperty>> GetPropertiesAvailable(int branchId, int itemUnitId)
        {
            List<StoreProperty> items = new List<StoreProperty>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("branchId", branchId.ToString());
            parameters.Add("itemUnitId", itemUnitId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("StoreProperty/GetPropertiesAvailable", parameters);


            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<StoreProperty>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
         public async Task<decimal> SoldProperties(int itemUnitId,string valIds,int count,int branchId,int userId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("valIds", valIds);
            parameters.Add("itemUnitId", itemUnitId.ToString());
            parameters.Add("branchId", branchId.ToString());
            parameters.Add("count", count.ToString());
            parameters.Add("userId", userId.ToString());
            //#################
            return  await APIResult.post("StoreProperty/SoldProperties", parameters);
        }
        public async Task<decimal> AddProperties(int itemUnitId,string valIds,int count,int branchId,int userId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("valIds", valIds);
            parameters.Add("itemUnitId", itemUnitId.ToString());
            parameters.Add("branchId", branchId.ToString());
            parameters.Add("count", count.ToString());
            parameters.Add("userId", userId.ToString());
            //#################
            return  await APIResult.post("StoreProperty/AddProperties", parameters);
        }
        public async Task<decimal> UpdateProperties(int itemUnitId,string valIds,int count,int branchId,int userId,int storPropId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("valIds", valIds);
            parameters.Add("itemUnitId", itemUnitId.ToString());
            parameters.Add("branchId", branchId.ToString());
            parameters.Add("count", count.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("storPropId", storPropId.ToString());
            //#################
            return  await APIResult.post("StoreProperty/UpdateProperties", parameters);
        }

        #endregion
    }
}
