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

  

    public class SliceUser
    {

        public int sliceUserId { get; set; }
        public Nullable<int> sliceId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<byte> isActive { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }

        // user

        public string username { get; set; }

        public string uname { get; set; }
        public string lastname { get; set; }
        public string job { get; set; }
        public string workHours { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }

        public string address { get; set; }
        public short? uisActive { get; set; }
        public byte? isOnline { get; set; }


        //slice
        public string name { get; set; }
        //
        public async Task<List<SliceUser>> GetAll()
        {
            List<SliceUser> items = new List<SliceUser>();
            IEnumerable<Claim> claims = await APIResult.getList("SliceUser/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<SliceUser>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<SliceUser> GetByID(int itemId)
        {
            SliceUser item = new SliceUser();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("SliceUser/GetByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<SliceUser>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
        
        public async Task<List<SliceUser>> GetSlicesByUserId(int userId)
        {
            List<SliceUser> items = new List<SliceUser>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", userId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("SliceUser/GetSlicesByUserId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add( JsonConvert.DeserializeObject<SliceUser>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                   
                }
            }
            return items;
        }
    
        public async Task<decimal> Save(SliceUser item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "SliceUser/Save";
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> UpdateSliceByUserId(List<SliceUser> newList, int userId, int updateUserId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "SliceUser/UpdateSliceByUserId";
            var newListParameter = JsonConvert.SerializeObject(newList);
            parameters.Add("newList", newListParameter);
            parameters.Add("userId", userId.ToString());
            parameters.Add("updateUserId", updateUserId.ToString());
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> delete(int sliceUserId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", sliceUserId.ToString());
            string method = "SliceUser/Delete";
           return await APIResult.post(method, parameters);
        }
    }
}
