using LiveCharts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Security.Claims;


namespace POS.Classes
{
    public class InvoiceCount
    {
        public string invType { get; set; }
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public int purCount { get; set; }
        public int saleCount { get; set; }
        public int purBackCount { get; set; }
        public int saleBackCount { get; set; }

    }
    public class AgentsCount
    {


        public int vendorCount { get; set; }
        public int customerCount { get; set; }


    }
  

    public class AgentsCountbyBranch
    {
        public int vendorsCount { get; set; }
        public int customersCount { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
    }
    public class UserOnlineCount
    {

        public int branchId { get; set; }
        public string branchName { get; set; }
        public int userOnlineCount { get; set; }
        public int allPos { get; set; }
        // public int allUsers { get; set; }
        public int offlineUsers { get; set; }

    }
    public class userOnlineInfo
    { 
        public Nullable<int> branchId { get; set; }
        public string branchName { get; set; }
        public Nullable<byte> branchisActive { get; set; }
        public Nullable<int> posId { get; set; }
        public string posName { get; set; }
        public Nullable<byte> posisActive { get; set; }
        public Nullable<int> userId { get; set; }
        public string usernameAccount { get; set; }
        public string userName { get; set; }
        public string lastname { get; set; }
        public string job { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public Nullable<byte> userisActive { get; set; }
        public Nullable<byte> isOnline { get; set; }
        public string image { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
    }
    public class BranchOnlineCount
    {

        public int branchOnline { get; set; }
        public int branchAll { get; set; }
        public int branchOffline { get; set; }


    }
    public class BestSeller
    {
        public string itemName { get; set; }
        public string unitName { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public Nullable<long> quantity { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public Nullable<decimal> subTotal { get; set; }
    }
    // storage
    public class IUStorage
    {

        public string itemName { get; set; }
        public string unitName { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }

        public string branchName { get; set; }
        public Nullable<int> branchId { get; set; }
        public Nullable<long> quantity { get; set; }


    }
    public class TotalPurSale
    {
        public Nullable<int> branchCreatorId { get; set; }
        public string branchCreatorName { get; set; }
        public Nullable<decimal> totalPur { get; set; }
        public Nullable<decimal> totalSale { get; set; }
        public int countPur { get; set; }
        public int countSale { get; set; }
        public int day { get; set; }

    }
    public class PosOnlineCount
    {
       
        public int branchId { get; set; }
        public string branchName { get; set; }
        public int posOnlineCount { get; set; }
        public int allPos { get; set; }
        public int offlinePos { get; set; }

        
    }
    public class Dash
    {
        public List<InvoiceCount> ListSalPur { get; set; }
        public string countAllPurchase { get; set; }
        public string countAllSalesValue { get; set; }

        public List<AgentsCountbyBranch> ListAgentCount { get; set; }
        public string customerCount { get; set; }
        public string vendorCount { get; set; }


        public List<UserOnlineCount> UsersList { get; set; }
        public string userOnline { get; set; }
        public string userOffline { get; set; }

       public List<BranchOnlineCount> ListBranchOnline { get; set; }
        public string branchOnline { get; set; }
        public string branchOffline { get; set; }

        public List<InvoiceCount> DailySalPur { get; set; }
        public string countDailyPurchase { get; set; }
        public string countDailySales { get; set; }

        public List<TotalPurSale> MonthlySalPur { get; set; }
        public string countMonthlyPurchase { get; set; }
        public string countMonthlySales { get; set; }

        public List<BestSeller> ListBestSeller { get; set; }
        public List<IUStorage> ListIUStorage { get; set; }
        public List<TotalPurSale> ListAllBestSeller { get; set; }
        public List<userOnlineInfo> listUserOnline;


        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        //
        public List<Pos> branchBalance { get; set; }
        public string cashBalance { get; set; }

        public List<Pos>  PosOnlineInfo { get; set; }
        public List<PosOnlineCount> posOnlineCount { get; set; }
        public string posOnline { get; set; }
        public string posOffline { get; set; }

        public List<CardsSts> paymentsToday { get; set; }


        // عدد الموردين والزبائن الكلي
        public async Task<List<AgentsCount>> GetAgentCount()
        {
            List<AgentsCount> list = new List<AgentsCount>();

            IEnumerable<Claim> claims = await APIResult.getList("dash/GetAgentCount");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<AgentsCount>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

 
        }
        // عدد فواتير المبيعات ومرتجع المبيعات والمشتريات ومرتجع المشتريات حسب الفرع
        public async Task<List<InvoiceCount>> Getdashsalpur(int mainBranchId, int userId)
        {

            List<InvoiceCount> list = new List<InvoiceCount>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());

            IEnumerable<Claim> claims = await APIResult.getList("dash/Getdashsalpur", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<InvoiceCount>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

        }
        public async Task<Dash> GetDashInfo(int mainBranchId, int userId, List<ItemUnit> IUList)
        {

            Dash list = new Dash();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            var myContent = JsonConvert.SerializeObject(IUList);
            parameters.Add("IUList", myContent);

            IEnumerable<Claim> claims = await APIResult.getList("dash/GetDashInfo", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list = JsonConvert.DeserializeObject<Dash>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                }
            }
            return list;

        }

        public async Task<MainWindowNot> GetMainNotification( int userId, string alertType,int posId,string invType,string status,
                                                        string cashTransferType,string cashSide)
        {

            MainWindowNot list = new MainWindowNot();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("userId", userId.ToString());
            parameters.Add("posId", posId.ToString());
            parameters.Add("alertType", alertType);
            parameters.Add("invType", invType);
            parameters.Add("status", status);
            parameters.Add("cashTransferType", cashTransferType);
            parameters.Add("cashSide", cashSide);


            IEnumerable<Claim> claims = await APIResult.getList("dash/GetMainNotification", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list = JsonConvert.DeserializeObject<MainWindowNot>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                }
            }
            return list;

        }

        //عدد المستخدمين المتصلين والغير متصلين  حاليا في كل فرع 
        public async Task<List<UserOnlineCount>> Getuseronline(int mainBranchId, int userId)
        {

            List<UserOnlineCount> list = new List<UserOnlineCount>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("dash/Getuseronline", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<UserOnlineCount>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

       
        }
        //بيانات المستخدمين المتصلين
        public async Task<List<userOnlineInfo>> GetuseronlineInfo(int mainBranchId, int userId)
        {
            List<userOnlineInfo> list = new List<userOnlineInfo>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("dash/GetuseronlineInfo", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<userOnlineInfo>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

 
        }
        // عدد الفروع المتصلة وغير المتصلة
        public async Task<List<BranchOnlineCount>> GetBrachonline(int mainBranchId, int userId)
        {
            List<BranchOnlineCount> list = new List<BranchOnlineCount>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("dash/GetBrachonline", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<BranchOnlineCount>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;


      
        }
        // عدد فواتير المبيعات ومرتجع المبيعات والمشتريات ومرتجع المشتريات حسب الفرع في اليوم الحالي
        public async Task<List<InvoiceCount>> GetdashsalpurDay(int mainBranchId, int userId)
        {
            List<InvoiceCount> list = new List<InvoiceCount>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("dash/GetdashsalpurDay", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<InvoiceCount>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

      
        }
        // اكثر 10 اصناف مبيعا
        public async Task<List<BestSeller>> Getbestseller(int mainBranchId, int userId)
        {
            List<BestSeller> list = new List<BestSeller>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("dash/Getbestseller", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<BestSeller>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
           
        }
        //كمية قائمة من 10 اصناف في كل فرع 
        public async Task<List<IUStorage>> GetIUStorage(List<ItemUnit> IUList, int mainBranchId, int userId)
        {

            List<IUStorage> list = new List<IUStorage>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            var myContent = JsonConvert.SerializeObject(IUList);
            parameters.Add("IUList", myContent);
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("dash/GetIUStorage", parameters);



            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<IUStorage>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;

   
        }
        // مجموع مبالغ المشتريات والمبيعات اليومي خلال الشهر الحالي لكل فرع
        public async Task<List<TotalPurSale>> GetTotalPurSale(int mainBranchId, int userId)
        {
            List<TotalPurSale> list = new List<TotalPurSale>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("mainBranchId", mainBranchId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("dash/GetTotalPurSale", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<TotalPurSale>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
 
        }


    }
}
