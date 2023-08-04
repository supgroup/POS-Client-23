using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    public class User
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string fullName { get; set; }
        public string job { get; set; }
        public string workHours { get; set; }
        public string details { get; set; }
        public float balance { get; set; }
        public int balanceType { get; set; }
        public DateTime? createDate { get; set; }
        public DateTime? updateDate { get; set; }
        public int? createUserId { get; set; }
        public int? updateUserId { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public short? isActive { get; set; }
        public string notes { get; set; }
        public byte? isOnline { get; set; }
        public bool? isAdmin { get; set; }
        public string role { get; set; }
        public Boolean canDelete { get; set; }
        public string image { get; set; }
        public Nullable<int> groupId { get; set; }
        public string groupName { get; set; }
        public string userGroup
        {
            get
            {
                return ( string.IsNullOrWhiteSpace(groupName) ? $"{name} {lastname}" : $"{name} {lastname} - {groupName}");
            }
            set
            {
                userGroup = value;
            }
        }
        public int? canLogin { get; set; }
        public int? branchId { get; set; }
        public int? userLogInID { get; set; }

        public bool hasCommission { get; set; }
        public Nullable<decimal> commissionValue { get; set; }
        public Nullable<decimal> commissionRatio { get; set; }

        public async Task<List<User>> Get()
        {
            List<User> users = new List<User>();

            IEnumerable<Claim> claims = await APIResult.getList("Users/Get");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    users.Add(JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return users;
        }
        public async Task<List<User>> GetAll()
        {
            List<User> users = new List<User>();

            IEnumerable<Claim> claims = await APIResult.getList("Users/GetAll");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    users.Add(JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return users;
        }

        public async Task<List<User>> getBranchSalesMan(int branchId, string objectName)
        {
            List<User> items = new List<User>();

            //########### to pass parameters (optional)
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("branchId", branchId.ToString());
            parameters.Add("deliveryPermission", objectName);
            IEnumerable<Claim> claims = await APIResult.getList("Users/GetSalesMan", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<User>> GetUsersActive()
        {
            List<User> items = new List<User>();
            IEnumerable<Claim> claims = await APIResult.getList("Users/GetActive");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<decimal> CanLogIn(int userId,int posId)
        {
            int items =0;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("posId", posId.ToString());
            parameters.Add("userId", userId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("Users/CanLogIn", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items=int.Parse(c.Value);
                }
            }
            return items;
        }
        public async Task<List<User>> GetActiveForAccount(string payType)
        {
            List<User> items = new List<User>();
            //  to pass parameters (optional)
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("payType", payType.ToString());

            IEnumerable<Claim> claims = await APIResult.getList("Users/GetActiveForAccount", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<User> Getloginuser(string userName, string password,int posId)
        {
            User user = new User();

            //########### to pass parameters (optional)
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("userName", userName);
            parameters.Add("password", password);
            parameters.Add("posId", posId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("Users/Getloginuser", parameters);
            //#################

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    user = JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return user;
        }
        public async Task<User> getUserById(int userId)
        {
            User user = new User();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", userId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Users/GetUserByID", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    user = JsonConvert.DeserializeObject<User>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return user;
        }
        public async Task<UserSettings> getUserSettings(int userId,int posId)
        {
            UserSettings userSet = new UserSettings();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("userId", userId.ToString());
            parameters.Add("posId", posId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("Users/GetUserSettings", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    userSet = JsonConvert.DeserializeObject<UserSettings>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                }
            }
            //user specialist
            AppSettings.lang = userSet.userLang;
            AppSettings.menuIsOpen = userSet.UserMenu;
            AppSettings.firstPath = userSet.firstPath;
            AppSettings.secondPath = userSet.secondPath;
            AppSettings.firstPathId = userSet.firstPathId;
            AppSettings.secondPathId = userSet.secondPathId;
            AppSettings.messageContent = userSet.messageContent;
            AppSettings.messageTitle = userSet.messageTitle;
            AppSettings.invoiceSlice = userSet.invoiceSlice;
            AppSettings.DefaultInvoiceSlice = userSet.invoiceSlice;


            #region general info
            AppSettings.dateFormat = userSet.dateFormat;
            AppSettings.accuracy = userSet.accuracy;
            AppSettings.StorageCost = userSet.StorageCost;
            AppSettings.Currency = userSet.Currency;
            AppSettings.CurrencyId = userSet.CurrencyId;
            AppSettings.countryId = userSet.countryId;

            #endregion

            #region //default system info
            AppSettings.companyName = userSet.companyName;
            AppSettings.Address = userSet.Address;
            AppSettings.Email = userSet.Email;//company email
            AppSettings.Mobile = userSet.Mobile;
            AppSettings.Phone = userSet.Phone;
            AppSettings.Fax = userSet.Fax;
            AppSettings.logoImage = userSet.logoImage;

            AppSettings.com_name_ar = userSet.com_name_ar;
            AppSettings.com_address_ar = userSet.com_address_ar;        
            #endregion
            #region social
            AppSettings.com_website = userSet.com_website;
            AppSettings.com_social = userSet.com_social;
            AppSettings.com_social_icon = userSet.com_social_icon;
            #endregion
            #region //report - printing
            AppSettings.sale_copy_count = userSet.sale_copy_count;
            AppSettings.pur_copy_count = userSet.pur_copy_count;
            AppSettings.print_on_save_sale = userSet.print_on_save_sale;
            AppSettings.print_on_save_pur = userSet.print_on_save_pur;
            AppSettings.email_on_save_sale = userSet.email_on_save_sale;
            AppSettings.email_on_save_pur = userSet.email_on_save_pur;
            AppSettings.rep_printer_name = userSet.rep_printer_name;
            AppSettings.sale_printer_name = userSet.sale_printer_name;
            AppSettings.salePaperSize = userSet.salePaperSize;
            AppSettings.rep_print_count = userSet.rep_print_count;
            AppSettings.docPapersize = userSet.docPapersize;
            AppSettings.Allow_print_inv_count = userSet.Allow_print_inv_count;
            AppSettings.show_header = userSet.show_header;
            AppSettings.print_on_save_directentry = userSet.print_on_save_directentry;
            AppSettings.directentry_copy_count = userSet.directentry_copy_count;
            AppSettings.itemtax_note = userSet.itemtax_note;
            AppSettings.sales_invoice_note = userSet.sales_invoice_note;
            AppSettings.Reportlang = userSet.Reportlang;
            AppSettings.invoice_lang = userSet.invoice_lang;
            #endregion

            #region tax
            AppSettings.invoiceTax_bool = userSet.invoiceTax_bool;
            AppSettings.invoiceTax_decimal = userSet.invoiceTax_decimal;
            AppSettings.itemsTax_bool = userSet.itemsTax_bool;
            AppSettings.itemsTax_decimal = userSet.itemsTax_decimal;
            #endregion
            //support
            AppSettings.activationSite = userSet.activationSite;

            //property
            #region property
            AppSettings.canSkipProperties = userSet.canSkipProperties;
            AppSettings.canSkipSerialsNum = userSet.canSkipSerialsNum;
            #endregion

            #region related to invoice
            AppSettings.returnPeriod = userSet.returnPeriod;
            AppSettings.freeDelivery = userSet.freeDelivery;
            #endregion
            return userSet;
        }
         
        public async Task<decimal> save(User user)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Users/Save";

            var myContent = JsonConvert.SerializeObject(user);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> updateOnline(int userId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Users/updateOnline";       
            parameters.Add("userId", userId.ToString());
            return await APIResult.post(method, parameters);
        }
        public async Task<decimal> checkLoginAvalability(int posId, string userName, string password)
        {
            string motherCode = setupConfiguration.GetMotherBoardID();
            string hardCode = setupConfiguration.GetHDDSerialNo();
            string deviceCode = motherCode + "-" + hardCode;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Users/checkLoginAvalability";

            parameters.Add("deviceCode", deviceCode);
            parameters.Add("posId", posId.ToString());
            parameters.Add("userName", userName);
            parameters.Add("password", password);
           return await APIResult.post(method, parameters);
        }

        public async Task<decimal> editUserBalance(decimal amount , int userId)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Users/editUserBalance";

            parameters.Add("amount", amount.ToString());
            parameters.Add("userId", userId.ToString());

           return await APIResult.post(method, parameters);
        }

        public async Task<decimal> delete(int delUserId, int userId, bool final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("delUserId", delUserId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());

            string method = "Users/Delete";
           return await APIResult.post(method, parameters);
        }
        public async Task<decimal> updateImage(User user)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var myContent = JsonConvert.SerializeObject(user);
            parameters.Add("itemObject", myContent);

            string method = "Users/UpdateImage";
           return await APIResult.post(method, parameters);
        }
        public async Task<string> uploadImage(string imagePath, string imageName, int userId)
        {
            if (imagePath != "")
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                // get file extension
                var ext = imagePath.Substring(imagePath.LastIndexOf('.'));
                var extension = ext.ToLower();
                string fileName = imageName + extension;
                try
                {
                    // configure trmporery path
                    string dir = Directory.GetCurrentDirectory();
                    string tmpPath = Path.Combine(dir, Global.TMPUsersFolder);
                    string[] files = System.IO.Directory.GetFiles(tmpPath, imageName + ".*");
                    foreach (string f in files)
                    {
                        System.IO.File.Delete(f);
                    }

                    tmpPath = Path.Combine(tmpPath, imageName + extension);

                    if (imagePath != tmpPath) // edit mode
                    {
                        // resize image
                        ImageProcess imageP = new ImageProcess(150, imagePath);
                        imageP.ScaleImage(tmpPath);

                        // read image file
                        var stream = new FileStream(tmpPath, FileMode.Open, FileAccess.Read);

                        // create http client request
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(Global.APIUri);
                            client.Timeout = System.TimeSpan.FromSeconds(3600);
                            string boundary = string.Format("----WebKitFormBoundary{0}", DateTime.Now.Ticks.ToString("x"));
                            HttpContent content = new StreamContent(stream);
                            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            content.Headers.Add("client", "true");

                            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = imageName,
                                FileName = fileName
                            };
                            form.Add(content, "fileToUpload");

                            var response = await client.PostAsync(@"users/PostUserImage", form);

                        }
                        stream.Dispose();
                    }
                    // save image name in DB
                    User user = new User();
                    user.userId = userId;
                    user.image = fileName;
                    await updateImage(user);
                    return fileName;
                }
                catch
                { return ""; }
            }
            return "";
        }
      
        public async Task<byte[]> downloadImage(string imageName)
        {
            byte[] byteImg = null;
            if (imageName != "")
            {
                byteImg = await APIResult.getImage("Users/GetImage", imageName);

                string dir = Directory.GetCurrentDirectory();
                string tmpPath = Path.Combine(dir, Global.TMPUsersFolder);
                if (!Directory.Exists(tmpPath))
                    Directory.CreateDirectory(tmpPath);
                tmpPath = Path.Combine(tmpPath, imageName);
                if (System.IO.File.Exists(tmpPath))
                {
                    System.IO.File.Delete(tmpPath);
                }
                if (byteImg != null)
                {
                    using (FileStream fs = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fs.Write(byteImg, 0, byteImg.Length);
                    }
                }

            }
            return byteImg;
        }
            public async Task<daysremain> getRemainDayes()
        {
            daysremain items = new daysremain();

            IEnumerable<Claim> claims = await APIResult.getList("ProgramDetails/getRemainDayes");
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {

                    items = JsonConvert.DeserializeObject<daysremain>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                }
            }
            return items;
        }

    }
}
