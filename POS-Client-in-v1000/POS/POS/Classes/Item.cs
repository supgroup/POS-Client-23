using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POS;
using POS.Classes;
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
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace POS.Classes
{
    public class Item :ICloneable
    {
        #region properties
        public int itemId { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string details { get; set; }
        public string type { get; set; }
        public string image { get; set; }
        public Nullable<decimal> taxes { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<int> min { get; set; }
        public Nullable<int> max { get; set; }
        public Nullable<int> categoryId { get; set; }
        public string categoryName { get; set; }
        public Nullable<int> parentId { get; set; }
        public bool isTaxExempt { get; set; }
        public bool isExpired { get; set; }
        public int alertDays { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> minUnitId { get; set; }
        public Nullable<int> maxUnitId { get; set; }
        public Boolean canDelete { get; set; }
        public string parentName { get; set; }
        public string minUnitName { get; set; }
        public string maxUnitName { get; set; }
        public Nullable<int> itemCount { get; set; }
        public Nullable<int> reservedCount { get; set; }

        public Nullable<decimal> avgPurchasePrice { get; set; }
        // new units and offers an is new
        //units
        public Nullable<int> unitId { get; set; }
        public string unitName { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> basicPrice { get; set; }
        //offer
        public Nullable<decimal> desPrice { get; set; }
        public Nullable<int> isNew { get; set; }
        public Nullable<int> isOffer { get; set; }
        public string offerName { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public byte? isActiveOffer { get; set; }
        public Nullable<int> itemUnitId { get; set; }
        public Nullable<int> offerId { get; set; }
        public Nullable<decimal> priceTax { get; set; }
        public Nullable<short> defaultSale { get; set; }
        public bool canUpdate { get; set; }
        public string discountType { get; set; }
        public Nullable<decimal> discountValue { get; set; }
        #region extra info
        public List<Item> packageItems { get; set; }
        public List<Price> SalesPrices { get; set; }
        public List<StoreProperty> ItemProperties { get; set; }
        #endregion
        public bool hasWarranty { get; set; }
        public Nullable<int> warrantyId { get; set; }
        public string warrantyName { get; set; }
        public string warrantyDescription { get; set; }
        public bool skipProperties { get; set; }
        public bool skipSerialsNum { get; set; }
        //for validate serial
        public bool valid { get; set; }
        public bool validProperty { get; set; }
        #endregion
        /// ////////////////////

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public async Task<decimal> saveItem(Item item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "Items/Save";

            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);
           return await APIResult.post(method, parameters);
        }
        public async Task<List<Item>> GetAllItems()
        {
            List<Item> items = new List<Item>();

            IEnumerable<Claim> claims = await APIResult.getList("items/GetAllItems");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        public async Task<List<Item>> GetSaleOrPurItems(int categoryId, short defaultSale, short defaultPurchase, int branchId)
        {
            List<Item> items = new List<Item>() ;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("categoryId", categoryId.ToString());
            parameters.Add("defaultSale", defaultSale.ToString());
            parameters.Add("defaultPurchase", defaultPurchase.ToString());
            parameters.Add("branchId", branchId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("items/GetSaleOrPurItems",parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;            
        }
        public async Task<decimal> deleteItem(int itemId, int userId,Boolean final)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());

            string method = "Items/Delete";
           return await APIResult.post(method, parameters);
            
        }

        // get codes of all items
        public async Task<List<string>> GetItemsCodes()
        {
            List<string> codes = new List<string>();

            IEnumerable<Claim> claims = await APIResult.getList("items/GetItemsCodes");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    codes.Add(c.Value.ToString());
                }
            }
            return codes;    
        }
        // get items of type
        public async Task<List<Item>> GetItemsByType(string type)
        {
            List<Item> items = new List<Item>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type);
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("items/GetItemsByType", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;  
        }
        public async Task<List<Item>> GetPackageItems(string type)
        {
            List<Item> items = new List<Item>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("type", type);
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("items/GetPackageItems", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;  
        }
        //public async Task<List<Item>> GetSubItems(int itemId)
        //{
        //    List<Item> items = new List<Item>();
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    parameters.Add("itemId", itemId.ToString());
        //    //#################
        //    IEnumerable<Claim> claims = await APIResult.getList("items/GetSubItems", parameters);

        //    foreach (Claim c in claims)
        //    {
        //        if (c.Type == "scopes")
        //        {
        //            items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
        //        }
        //    }
        //    return items;
        //}

        public async Task<Item> GetItemByID(int itemId)
        {
            Item item = new Item();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("items/GetItemByID", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                    break;
                }
            }
            return item;
        }
       
        public async Task<List<Item>> GetItemsWichHasUnits()
        {
            List<Item> items = new List<Item>();

            IEnumerable<Claim> claims = await APIResult.getList("items/GetItemsWichHasUnits");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;        
        }
       
        // get items in category and sub
        public async Task<List<Item>> GetItemsInCategoryAndSub(int categoryId)
        {
            List<Item> items = new List<Item>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("categoryId", categoryId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("items/GetItemsInCategoryAndSub", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }

        #region image
        // update image field in DB
        public async Task<decimal> updateImage(Item item)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var myContent = JsonConvert.SerializeObject(item);
            parameters.Add("itemObject", myContent);

            string method = "Items/UpdateImage";
           return await APIResult.post(method, parameters); 
        }
        public async Task<Boolean> uploadImage(string imagePath, string imageName, int itemId)
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
                    string tmpPath = Path.Combine(dir, Global.TMPItemsFolder);
                    
                    string[] files = System.IO.Directory.GetFiles(tmpPath, imageName + ".*");
                    foreach (string f in files)
                    {
                        System.IO.File.Delete(f);
                    }
                    tmpPath = Path.Combine(tmpPath, imageName + extension);

                    if (imagePath != tmpPath) // edit mode
                    {
                        // resize image
                        ImageProcess imageP = new ImageProcess(250, imagePath);
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

                            var response = await client.PostAsync(@"items/PostItemImage", form);
                        }
                        stream.Dispose();
                    }
                    // save image name in DB
                    Item item = new Item();
                    item.itemId = itemId;
                    item.image = fileName;
                    await updateImage(item);

                    return true;
                }
                catch
                { return false; }
            }
            return false;
        }
        public async Task<byte[]> downloadImage(string imageName)
        {
            byte[] byteImg = null;
            if (imageName != "")
            {
                byteImg = await APIResult.getImage("Items/GetImage", imageName);

                string dir = Directory.GetCurrentDirectory();
                string tmpPath = Path.Combine(dir, Global.TMPItemsFolder);
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


        #endregion


        //service

        public async Task<List<Item>> GetAllSrItems()
        {
            List<Item> items = new List<Item>();

            IEnumerable<Claim> claims = await APIResult.getList("items/GetAllSrItems");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }


        public async Task<List<Item>> GetSrItemsInCategoryAndSub(int categoryId)
        {
            List<Item> items = new List<Item>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("categoryId", categoryId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("items/GetSrItemsInCategoryAndSub", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
        List<int> categoriesId = new List<int>();
        public async Task< List<int>> getCategoryAndSubs(int categoryId)
        {
            categoriesId = new List<int>();
            if (categoryId != 0)
            {
                if (FillCombo.categoriesList is null)
                    await FillCombo.RefreshCategories();
                List<Category> categoriesList = FillCombo.categoriesList
                     .Select(p => new Category
                     {
                         categoryId = p.categoryId,
                         name = p.name,
                         parentId = p.parentId,
                     })
                    .ToList();

              
                categoriesId.Add(categoryId);

                // get child categories
                var result = Recursive(categoriesList, categoryId);
 
    
            }
            return categoriesId; ;
        }
        public IEnumerable<Category> Recursive(List<Category> categoriesList, int toplevelid)
        {
            List<Category> inner = new List<Category>();

            foreach (var t in categoriesList.Where(item => item.parentId == toplevelid))
            {
                categoriesId.Add(t.categoryId);
                inner.Add(t);
                inner = inner.Union(Recursive(categoriesList, t.categoryId)).ToList();
            }

            return inner;
        }
    }
}
