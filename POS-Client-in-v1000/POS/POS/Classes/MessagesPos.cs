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
    public  class MessagesPos
    {
        #region properties
        public int msgPosId { get; set; }
        public Nullable<int> msgId { get; set; }
        public Nullable<int> posId { get; set; }
        public bool isReaded { get; set; }
        public string notes { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public bool canDelete { get; set; }
        public string posName { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<int> userReadId { get; set; }
        public string userRead { get; set; }
        public string toUserFullName { get; set; }
        //message
        public string title { get; set; }
        public string msgContent { get; set; }
        public bool isActive { get; set; }
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }//الفرع الذي انشا الرسالة
        public string msgCreatorName { get; set; }//المستخد الذي انشأ الرسالة
        public string msgCreatorLast { get; set; }
        public Nullable<int> mainMsgId { get; set; }
        //user
        public Nullable<int> toUserId { get; set; }
        #endregion
        #region Methods
        public async Task<List<MessagesPos>> GetAll()
        {
            List<MessagesPos> items = new List<MessagesPos>();
            IEnumerable<Claim> claims = await APIResult.getList("messagesPos/GetAll");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<MessagesPos>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<MessagesPos> GetById(int itemId)
        {
            MessagesPos item = new MessagesPos();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("messagesPos/GetById", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<MessagesPos>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
 
        public async Task<decimal> Save(MessagesPos item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "messagesPos/Save";
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
            string method = "messagesPos/Delete";
            return await APIResult.post(method, parameters);
        }

        public async Task<List<MessagesPos>> GetBymsgId(int msgId)
        {
            List<MessagesPos> items = new List<MessagesPos>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", msgId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("messagesPos/GetBymsgId", parameters);
  
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<MessagesPos>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<decimal> SendMessage(AdminMessages message, bool all = false, int branchId = 0, List<int> posIdList = null)
        {
            if (posIdList is null)
                posIdList = new List<int>();

            //1- all 2- branchId 3-posIdList

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "adminMessages/SendMessage";
            var myContent = JsonConvert.SerializeObject(message);
            parameters.Add("message", myContent);

            var myContent2 = JsonConvert.SerializeObject(posIdList);
            parameters.Add("posIdList", myContent2);

            parameters.Add("all", all.ToString());
            parameters.Add("branchId", branchId.ToString());
            return await APIResult.post(method, parameters);
        }
        public async Task<decimal> SendMessageToUsers(AdminMessages message, bool all = false, List<int> userIdList = null)
        {
            if (userIdList is null)
                userIdList = new List<int>();

            //1- all 2-userIdList

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "adminMessages/SendMessageToUsers";
            var myContent = JsonConvert.SerializeObject(message);
            parameters.Add("message", myContent);

            var myContent2 = JsonConvert.SerializeObject(userIdList);
            parameters.Add("userIdList", myContent2);

            parameters.Add("all", all.ToString());
          
            return await APIResult.post(method, parameters);
        }

        //void send()
        //{
        //    // message object
        //    AdminMessages adminMessages = new AdminMessages();
        //    // specified pos 
        //    List<int> _posIdList = new List<int>();

        //    // send To all pos in all branch
        //    var result1 =  SendMessage(adminMessages, true);
        //    // send To all pos in one branch
        //    var result2 = SendMessage(adminMessages,  branchId: 1);
        //    // send To specified pos 
        //    var result3 =  SendMessage(adminMessages, posIdList: _posIdList);
        //}
        public async Task<List<MessagesPos>> GetByPosId(int posId)
        {
            List<MessagesPos> items = new List<MessagesPos>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", posId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("messagesPos/GetByPosId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<MessagesPos>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        //pos+user
        public async Task<List<MessagesPos>> GetByPosIdUserId(int posId, int userId)
        {
            List<MessagesPos> items = new List<MessagesPos>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", posId.ToString());
            parameters.Add("userId", userId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("messagesPos/GetByPosIdUserId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<MessagesPos>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<decimal> updateIsReaded(  List<int> msgPosIdList, int userId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "messagesPos/updateIsReaded";
            var myContent = JsonConvert.SerializeObject(msgPosIdList);
            parameters.Add("msgPosIdList", myContent);
            parameters.Add("userId", userId.ToString());
            return await APIResult.post(method, parameters);
        }

        #endregion
    }
}
