using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Security.Claims;
using System.Linq;

namespace POS.Classes
{

    public class PayedInvclass
    {
        public string processType { get; set; }
        public Nullable<decimal> cash { get; set; }
        public string cardName { get; set; }
        public int sequenc { get; set; }
        public Nullable<int> cardId { get; set; }
        public Nullable<decimal> commissionValue { get; set; }
        public Nullable<decimal> commissionRatio { get; set; }
        public string docNum { get; set; }
    }
    public class CashTransfer
    {
      
        public int cashTransId { get; set; }
        public string transType { get; set; }
        public Nullable<int> posId { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<int> agentId { get; set; }
        public Nullable<int> invId { get; set; }
        public string transNum { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<decimal> cash { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> createUserId { get; set; }
        public string notes { get; set; }
        public Nullable<int> posIdCreator { get; set; }
        public Nullable<byte> isConfirm { get; set; }
        public Nullable<int> cashTransIdSource { get; set; }
        public string side { get; set; }
        public string docName { get; set; }
        public string docNum { get; set; }
        public string docImage { get; set; }
        public Nullable<int> bankId { get; set; }
        public string bankName { get; set; }
        public string agentName { get; set; }
        public string usersName { get; set; }// side=u
        public string posName { get; set; }
        public string pos2Name { get; set; }
        public Nullable<int> pos2Id { get; set; }
        public string posCreatorName { get; set; }
        public int cashTrans2Id { get; set; }
        public Nullable<byte> isConfirm2 { get; set; }
        public string processType { get; set; }
        public Nullable<int> cardId { get; set; }
        public string createUserName { get; set; }
        public string createUserJob { get; set; }
        public string createUserLName { get; set; }
        public string usersLName { get; set; } // side=u
        public string cardName { get; set; }// processType=card
        public string reciveName { get; set; }
        public Nullable<int> bondId { get; set; }
        public Nullable<System.DateTime> bondDeserveDate { get; set; }
        public Nullable<byte> bondIsRecieved { get; set; }
        public Nullable<int> shippingCompanyId { get; set; }
        public string shippingCompanyName { get; set; }
        public string userAcc { get; set; }
        public int isCommissionPaid { get; set; }
        public Nullable<decimal> paid { get; set; }
        public Nullable<decimal> deserved { get; set; }

        //for reports
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorname { get; set; }
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public Nullable<int> branch2Id { get; set; }
        public string branch2Name { get; set; }
        public string updateUserAcc { get; set; }
        public Nullable<decimal> commissionValue { get; set; }
        public Nullable<decimal> commissionRatio { get; set; }
        public string invNumber { get; set; }
        public string invType { get; set; }
        public string purpose { get; set; }
        public bool isInvPurpose { get; set; }
        public string otherSide { get; set; }
        public async Task<List<CashTransfer>> GetCashTransferAsync(string type, string side)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());
          
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetBytypeandSide",parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
            //List<CashTransfer> cashtransfer = null;
            //// ... Use HttpClient.
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //using (var client = new HttpClient())
            //{
            //    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //    client.BaseAddress = new Uri(Global.APIUri);
            //    client.DefaultRequestHeaders.Clear();
            //    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            //    client.DefaultRequestHeaders.Add("Keep-Alive", "3600");
            //    HttpRequestMessage request = new HttpRequestMessage();
            //    request.RequestUri = new Uri(Global.APIUri + "Cashtransfer/GetBytypeandSide?type=" + type + "&side=" + side);
            //    request.Headers.Add("APIKey", Global.APIKey);
            //    /*
            //    request.Headers.Add("type", type);
            //    request.Headers.Add("side", side);
            //    */
            //    request.Method = HttpMethod.Get;
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    HttpResponseMessage response = await client.SendAsync(request);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var jsonString = await response.Content.ReadAsStringAsync();
            //        jsonString = jsonString.Replace("\\", string.Empty);
            //        jsonString = jsonString.Trim('"');
            //        // fix date format
            //        JsonSerializerSettings settings = new JsonSerializerSettings
            //        {
            //            Converters = new List<JsonConverter> { new BadDateFixingConverter() },
            //            DateParseHandling = DateParseHandling.None
            //        };
            //        cashtransfer = JsonConvert.DeserializeObject<List<CashTransfer>>(jsonString, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
            //        return cashtransfer;
            //    }
            //    else //web api sent error response 
            //    {
            //        cashtransfer = new List<CashTransfer>();
            //    }
            //    return cashtransfer;
            //}

        }
        public async Task<List<CashTransfer>> GetCashTransfer(string type, string side)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetCashTransfer", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }
            //
            public async Task<List<CashTransfer>> GetCashTransferForPosAsync(string type, string side)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetBytypeAndSideForPos", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

           
        }

        public async Task<List<CashTransfer>> GetCashTransferForPosById (string type, string side,int posId)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());
            parameters.Add("posId", posId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetCashTransferForPosById", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;


        }
        public async Task<List<CashTransfer>> GetNotConfirmdByPosId(string type, string side, int posId)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());
            parameters.Add("posId", posId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetNotConfirmdByPosId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;


        }

        //userId
        public async Task<List<CashTransfer>> GetCashTransferForPosByUserId(string type, string side, int userId)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());
            parameters.Add("userId", userId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetCashTransferForPosByUserId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;


        }
        public async Task<List<CashTransfer>> GetCashBond(string type, string side)
        {
            // string type, string side
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type.ToString());
            parameters.Add("side", side.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetCashBond", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

        }

        public async Task<CashTransfer> GetByInvId(int invId)
        {
            CashTransfer item = new CashTransfer();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("invId", invId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetByInvId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }

        //GetListByInvId
        public async Task<List<CashTransfer>> GetListByInvId(int invId)
        {
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("invId", invId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetListByInvId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;     

        } 


        //return cashes of invoice and return on its
        public async Task<List<CashTransfer>> GetInvAndReturn(int invId)
        {
            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("invId", invId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetInvAndReturn", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;     

        }

        /// ///////////////////////////////////////

        public async Task<decimal> Save(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/Save";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }
        public async Task<decimal> SaveCashWithCommission(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/SaveCashWithCommission";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }
        public async Task<decimal> transferPosBalance(CashTransfer cashTr1 , CashTransfer cashTr2)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/transferPosBalance";

            var myContent = JsonConvert.SerializeObject(cashTr1);
            parameters.Add("cash1", myContent);

            myContent = JsonConvert.SerializeObject(cashTr2);
            parameters.Add("cash2", myContent);
           return await APIResult.post(method, parameters);

        }
        public async Task<decimal> confirmBankTransfer(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/confirmBankTransfer";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }
        public async Task<decimal> SaveWithBalanceCheck(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/SaveWithBalanceCheck";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }
        public async Task<decimal> ConfirmCashTransfer(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/Confirm";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }
        //public async Task<decimal> ConfirmAllCashTransfer(CashTransfer cashTr)
        //{

        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    string method = "Cashtransfer/ConfirmAll";

        //    var myContent = JsonConvert.SerializeObject(cashTr);
        //    parameters.Add("Object", myContent);
        //    return await APIResult.post(method, parameters);

        //}
        public async Task<decimal> ConfirmAndTrans(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/ConfirmAndTrans";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
            return await APIResult.post(method, parameters);

        }
        public async Task<decimal> ConfirmAllAndTrans(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/ConfirmAllAndTrans";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
            return await APIResult.post(method, parameters);

        }

        public async Task<decimal> MakeDeposit(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/MakeDeposit";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }

        public async Task<decimal> MakePull(CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Cashtransfer/MakePull";

            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);

        }

    
        public async Task<List<PayedInvclass>> GetPayedByInvId(int invId)
        {

            List<PayedInvclass> list = new List<PayedInvclass>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
          
            parameters.Add("invId", invId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetPayedByInvId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<PayedInvclass>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }
        public async Task <List<PayedInvclass>> PayedBycashlist(List<CashTransfer>cashList)
        {
            List<PayedInvclass> payedlist = new List<PayedInvclass>();
            try
            {
                List<Card> cards = new List<Card>();

            //fill cards
            if (FillCombo.cardsList is null)
            { await FillCombo.RefreshCards(); }
            cards = FillCombo.cardsList.ToList();

                     
                cashList = cashList.Where(C =>(C.processType == "card" || C.processType == "cash")).ToList();
                    int i = 0;
                payedlist = cashList.GroupBy(x => x.cardId).Select(x => new PayedInvclass
                {
                    processType = x.FirstOrDefault().processType,

                    cash = x.Sum(c => c.cash),
                    cardId = x.FirstOrDefault().cardId,
                    cardName = x.FirstOrDefault().processType == "card" ? cards.Where(c => c.cardId == x.FirstOrDefault().cardId).FirstOrDefault().name : "cash",
                    sequenc = x.FirstOrDefault().processType == "cash" ? 0 : ++i,
                    commissionRatio = x.FirstOrDefault().commissionRatio,
                    commissionValue = x.FirstOrDefault().commissionValue,
                }).OrderBy(c => c.cardId).ToList();
                    return payedlist;                
            }
            catch
            {
                return payedlist;
            }
        }

        public async Task<List<CashTransfer>> GetbySourcId(string side, int sourceId)
        {

            List<CashTransfer> list = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("side", side);
            parameters.Add("sourceId", sourceId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetbySourcId", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;


            //List<CashTransfer> cashtransfer = null;
            //// ... Use HttpClient.
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //using (var client = new HttpClient())
            //{
            //    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //    client.BaseAddress = new Uri(Global.APIUri);
            //    client.DefaultRequestHeaders.Clear();
            //    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            //    client.DefaultRequestHeaders.Add("Keep-Alive", "3600");
            //    HttpRequestMessage request = new HttpRequestMessage();
            //    request.RequestUri = new Uri(Global.APIUri + "Cashtransfer/GetbySourcId?sourceId=" + sourceId + "&side=" + side);
            //    request.Headers.Add("APIKey", Global.APIKey);
            //    /*
            //    request.Headers.Add("type", type);
            //    request.Headers.Add("side", side);
            //    */
            //    request.Method = HttpMethod.Get;
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    HttpResponseMessage response = await client.SendAsync(request);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var jsonString = await response.Content.ReadAsStringAsync();
            //        jsonString = jsonString.Replace("\\", string.Empty);
            //        jsonString = jsonString.Trim('"');
            //        // fix date format
            //        JsonSerializerSettings settings = new JsonSerializerSettings
            //        {
            //            Converters = new List<JsonConverter> { new BadDateFixingConverter() },
            //            DateParseHandling = DateParseHandling.None
            //        };
            //        cashtransfer = JsonConvert.DeserializeObject<List<CashTransfer>>(jsonString, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
            //        return cashtransfer;
            //    }
            //    else //web api sent error response 
            //    {
            //        cashtransfer = new List<CashTransfer>();
            //    }
            //    return cashtransfer;
            //}

        }

        public async Task<decimal> deletePosTrans(int cashTransId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("cashTransId", cashTransId.ToString());
            
            string method = "Cashtransfer/Delete";
           return await APIResult.post(method, parameters);

        }
        public async Task<decimal> canclePosTrans(int cashTransId1 , int cashTransId2)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("cashTransId1", cashTransId1.ToString());
            parameters.Add("cashTransId2", cashTransId2.ToString());
            
            string method = "Cashtransfer/Cancle";
           return await APIResult.post(method, parameters);

        }

        public async Task<decimal> MovePosCash(int cashTransId,int userIdD)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("cashTransId", cashTransId.ToString());
            parameters.Add("userIdD", userIdD.ToString());
            string method = "Cashtransfer/MovePosCash";
           return await APIResult.post(method, parameters);



            //// ... Use HttpClient.
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            //using (var client = new HttpClient())
            //{
            //    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //    client.BaseAddress = new Uri(Global.APIUri);
            //    client.DefaultRequestHeaders.Clear();
            //    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            //    client.DefaultRequestHeaders.Add("Keep-Alive", "3600");
            //    HttpRequestMessage request = new HttpRequestMessage();
            //    request.RequestUri = new Uri(Global.APIUri + "Cashtransfer/MovePosCash?cashTransId=" + cashTransId+"&useridD="+ userIdD);
            //    request.Headers.Add("APIKey", Global.APIKey);
            //    request.Method = HttpMethod.Post;
            //    //set content type
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    var response = await client.SendAsync(request);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var message = await response.Content.ReadAsStringAsync();
            //        message = JsonConvert.DeserializeObject<string>(message);
            //        return message;
            //        /* message value:
            //          Ok = transdone 
            //          or
            //          nobalanceinpullpos  -pullposnotconfirmed -nopullidornotconfirmed-idnotfound      
            //         * */
            //    }
            //    return "error";
            //}
        }

        public async Task<decimal> PayByAmmount(int agentId ,decimal ammount , string payType , CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            
            parameters.Add("agentId", agentId.ToString());
            parameters.Add("amount", ammount.ToString());
            parameters.Add("payType", payType.ToString());
            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent);
          
         

            string method = "Cashtransfer/payByAmount";
           return await APIResult.post(method, parameters);

        }

        public async Task<decimal> PayUserByAmmount(int userId, decimal ammount, string payType, CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
         
            parameters.Add("userId", userId.ToString());
            parameters.Add("amount", ammount.ToString());
            parameters.Add("payType", payType.ToString());
            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent);



            string method = "Cashtransfer/payUserByAmount";
           return await APIResult.post(method, parameters);
        }

        public async Task<decimal> payShippingCompanyByAmount(int shippingCompanyId, decimal ammount, string payType, CashTransfer cashTr)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("shippingCompanyId", shippingCompanyId.ToString());
            parameters.Add("amount", ammount.ToString());
            parameters.Add("payType", payType.ToString());
            var myContent = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent);



            string method = "Cashtransfer/payShippingCompanyByAmount";
           return await APIResult.post(method, parameters);

            
        }

        public async Task<decimal> PayListOfInvoices(int agentId, List<Invoice> invoicelst , string payType, CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("agentId", agentId.ToString());
            
            parameters.Add("payType", payType);
            var myContent = JsonConvert.SerializeObject(invoicelst);
            parameters.Add("invoices", myContent);
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);



            string method = "Cashtransfer/payListOfInvoices";
           return await APIResult.post(method, parameters);


        }

        public async Task<decimal> PayUserListOfInvoices(int userId, List<Invoice> invoicelst, string payType, CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("userId", userId.ToString());

            parameters.Add("payType", payType);
            var myContent = JsonConvert.SerializeObject(invoicelst);
            parameters.Add("invoices", myContent);
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);



            string method = "Cashtransfer/payUserListOfInvoices";
           return await APIResult.post(method, parameters);
    
        }

        public async Task<decimal> PayShippingCompanyListOfInvoices(int shippingCompanyId, List<Invoice> invoicelst, string payType, CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("shippingCompanyId", shippingCompanyId.ToString());

            parameters.Add("payType", payType);
            var myContent = JsonConvert.SerializeObject(invoicelst);
            parameters.Add("invoices", myContent);
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);



            string method = "Cashtransfer/payShippingCompanyListOfInvoices";
           return await APIResult.post(method, parameters);
  
        }

        public async Task<decimal> payDeliveryCostOfInvoices(int shippingCompanyId, List<CashTransfer> invoicelst,  CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("shippingCompanyId", shippingCompanyId.ToString());

            var myContent = JsonConvert.SerializeObject(invoicelst);
            parameters.Add("invoices", myContent);
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);

            string method = "Cashtransfer/payDeliveryCostOfInvoices";
           return await APIResult.post(method, parameters);
  
        }
        public async Task<decimal> payDeliveryCostByAmount(int shippingCompanyId, decimal amount,  CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("shippingCompanyId", shippingCompanyId.ToString());

            parameters.Add("amount", amount.ToString());
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);

            string method = "Cashtransfer/payDeliveryCostByAmount";
           return await APIResult.post(method, parameters);
  
        }

        public async Task<decimal> payListShortageCashes(List<CashTransfer> cashesList,  CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            var myContent = JsonConvert.SerializeObject(cashesList);
            parameters.Add("cashesList", myContent);
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);

            string method = "Cashtransfer/payListShortageCashes";
           return await APIResult.post(method, parameters);
  
        }
        public async Task<decimal> payShortageCashesByAmount(int userId,decimal amount,  CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("userId", userId.ToString());
            parameters.Add("amount", amount.ToString());
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);

            string method = "Cashtransfer/payShortageCashesByAmount";
           return await APIResult.post(method, parameters);
  
        }

        public async Task<decimal> payListCommissionCashes(List<CashTransfer> cashesList, CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            var myContent = JsonConvert.SerializeObject(cashesList);
            parameters.Add("cashesList", myContent);
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);

            string method = "Cashtransfer/payListCommissionCashes";
            return await APIResult.post(method, parameters);

        }
        public async Task<decimal> payCommissionCashesByAmount(int userId, decimal amount, CashTransfer cashTr)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("userId", userId.ToString());
            parameters.Add("amount", amount.ToString());
            var myContent2 = JsonConvert.SerializeObject(cashTr);
            parameters.Add("cashTransfer", myContent2);

            string method = "Cashtransfer/payCommissionCashesByAmount";
            return await APIResult.post(method, parameters);

        }
        public async Task<string> generateCashNumber(string cashNum)
        {
            int sequence = (int) await GetLastNumOfCash(cashNum);
            sequence++;
            string strSeq = sequence.ToString();
            if (sequence <= 999999)
                strSeq = sequence.ToString().PadLeft(6, '0');
            string transNum = cashNum + "-" + strSeq;
            return transNum;
        }
       
        public async Task<decimal> GetLastNumOfCash(string cashNum)
        {
            int message = 0;
          
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("cashCode", cashNum);
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetLastNumOfCash", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                   message = int.Parse(c.Value); ;
                    break;
                }
            }
            return message;
        }
        public async Task<string> getLastOpenTransNum(int posId)
        {
            string message = "";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("posId", posId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/getLastOpenTransNum", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    message = c.Value ;
                    break;
                }
            }
            return message;
        }

        public async Task<string> generateDocNumber(string docNum)
        {
            int sequence = (int) await GetLastNumOfDocNum(docNum);
            sequence++;
            string strSeq = sequence.ToString();
            if (sequence <= 999999)
                strSeq = sequence.ToString().PadLeft(6, '0');
            string transNum = docNum + "-" + strSeq;
            return transNum;
        }

        public async Task<decimal> GetLastNumOfDocNum(string docNum)
        {
            int message = 0;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("docNum", docNum);
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetLastNumOfDocNum", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    message = int.Parse(c.Value); ;
                    break;
                }
            }
            return message;


            //// ... Use HttpClient.
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //using (var client = new HttpClient())
            //{
            //    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //    client.BaseAddress = new Uri(Global.APIUri);
            //    client.DefaultRequestHeaders.Clear();
            //    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            //    client.DefaultRequestHeaders.Add("Keep-Alive", "3600");
            //    HttpRequestMessage request = new HttpRequestMessage();
            //    request.RequestUri = new Uri(Global.APIUri + "Cashtransfer/GetLastNumOfDocNum?docNum=" + docNum);
            //    request.Headers.Add("APIKey", Global.APIKey);
            //    request.Method = HttpMethod.Get;
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    HttpResponseMessage response = await client.SendAsync(request);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        string message = await response.Content.ReadAsStringAsync();
            //        message = JsonConvert.DeserializeObject<string>(message);
            //        return int.Parse(message);
            //    }
            //    return 0;
            //}
        }

        public async Task<decimal> payOrderInvoice(int invoiceId, int invStatusId ,decimal ammount, string payType, CashTransfer cashTransfer)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("invoiceId", invoiceId.ToString());

            parameters.Add("invStatusId", invStatusId.ToString());
            parameters.Add("amount", ammount.ToString());
            parameters.Add("payType", payType);
            
          
            var myContent = JsonConvert.SerializeObject(cashTransfer);
            parameters.Add("cashTransfer", myContent);

            string method = "Cashtransfer/payOrderInvoice";
           return await APIResult.post(method, parameters);
 
        }
       
        public async Task<CashTransfer> GetByID(int cashTransferId)
        {
            CashTransfer item = new CashTransfer();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("cashTransferId", cashTransferId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/GetByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }

        public async Task<List<CashTransfer>> getNotPaidDeliverCashes(int shippingComId)
        {
            List<CashTransfer> items = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("shippingComId", shippingComId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/getNotPaidDeliverCashes", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<CashTransfer>> getNotPaidAgentCommission(int userId)
        {
            List<CashTransfer> items = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/getNotPaidAgentCommission", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<CashTransfer>> getNotPaidShortage(int userId)
        {
            List<CashTransfer> items = new List<CashTransfer>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("Cashtransfer/getNotPaidShortage", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<CashTransfer>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
    }

}

