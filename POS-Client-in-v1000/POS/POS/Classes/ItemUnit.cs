using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    public class ItemUnit
    {
        public int itemUnitId { get; set; }
        public Nullable<int> itemId { get; set; }
        public Nullable<int> unitId { get; set; }
        public Nullable<int> storageCostId { get; set; }
        public Nullable<int> unitValue { get; set; }
        public Nullable<short> defaultSale { get; set; }
        public Nullable<short> defaultPurchase { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> packCost { get; set; }
        public Nullable<decimal> basicPrice { get; set; }

        public Nullable<decimal> cost { get; set; }
        public Nullable<decimal> priceTax { get; set; }
        public string barcode { get; set; }
        public string mainUnit { get; set; }
        public string smallUnit { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> updateDate { get; set; }
        public Nullable<int> createUserId { get; set; }
        public Nullable<int> updateUserId { get; set; }
        public Nullable<int> subUnitId { get; set; }
        public string itemName { get; set; }
        public string itemCode { get; set; }
        public string unitName { get; set; }
        public Boolean canDelete { get; set; }
        public Nullable<byte> isActive { get; set; }
        public Nullable<decimal> purchasePrice { get; set; }
        public Nullable<int> warrantyId { get; set; }
        public string warrantyName { get; set; }
        public string warrantyDescription { get; set; }
        public bool hasWarranty { get; set; }
        public bool skipProperties { get; set; }
        public bool skipSerialsNum { get; set; }

        #region extra info
        public List<StoreProperty> ItemProperties { get; set; }
        public List<Price> SalesPrices { get; set; }
        public Nullable<long> quantity { get; set; }
        public Nullable<long> serialsCount { get; set; }
        public Nullable<long> PropertiesCount { get; set; }
        public string itemType { get; set; }
        public int itemCount { get; set; }
        #endregion


        //**************************************************
        //*************** item unit methods *********************
        public async Task<List<ItemUnit>> GetAllItemUnits(int itemId)
        {


            List<ItemUnit> list = new List<ItemUnit>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetAll", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }


        public async Task<ItemUnit> GetById(int itemUnitId)
        {
            ItemUnit item = new ItemUnit();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemUnitId", itemUnitId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetById", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item =JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                }
            }
            return item;

        }
        public async Task<List<ItemUnit>> GetItemUnits(int itemId)
        {

            List<ItemUnit> list = new List<ItemUnit>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/Get",parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }

        public List<ItemUnit> GetIUbyItem(int itemId, List<ItemUnit>AllIU,List<Unit>AllUnits)
        {

            List<ItemUnit> itemUnitsList = new List<ItemUnit>();
            try
            {
                itemUnitsList = ( from IU in AllIU
                            where (IU.itemId == itemId && IU.isActive == 1)
                            join U in AllUnits on IU.unitId equals U.unitId into lj

                            from v in lj.DefaultIfEmpty()
                            join u1 in AllUnits on IU.subUnitId equals u1.unitId into tj
                            from v1 in tj.DefaultIfEmpty()
                            select new ItemUnit()
                            {
                                itemUnitId = IU.itemUnitId,
                                unitId = IU.unitId,
                                mainUnit = v.name,
                                createDate = IU.createDate,
                                createUserId = IU.createUserId,
                                defaultPurchase = IU.defaultPurchase,
                                defaultSale = IU.defaultSale,
                                price = IU.price,
                                subUnitId = IU.subUnitId,
                                smallUnit = v1.name,
                                unitValue = IU.unitValue,
                                barcode = IU.barcode,
                                updateDate = IU.updateDate,
                                updateUserId = IU.updateUserId,
                                storageCostId = IU.storageCostId,

                            }).ToList();

                    return itemUnitsList;
              
            }
            catch
            {
                return itemUnitsList;
            }
        }


        //***************************************
        // get all barcodes from DB , return list of string represent barcodes
        //***************************************
        public async Task<List<ItemUnit>> getAllBarcodes()
        {
            List<ItemUnit> list = new List<ItemUnit>();

            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetAllBarcodes");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }
        //***************************************
        // add or update item unit
        //***************************************
        public async Task<decimal> saveItemUnit(ItemUnit itemUnit)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = "ItemsUnits/Save";

            var myContent = JsonConvert.SerializeObject(itemUnit);
            parameters.Add("Object", myContent);
           return await APIResult.post(method, parameters);
        
        }
        //***************************************
        // delete item unit (barcode)
        //***************************************
        public async Task<decimal> Delete(int ItemUnitId, int userId, bool final)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ItemUnitId", ItemUnitId.ToString());
            parameters.Add("userId", userId.ToString());
            parameters.Add("final", final.ToString());
            string method = "ItemsUnits/Delete";
           return await APIResult.post(method, parameters);

        
        }

        public async Task<List<ItemUnit>> Getall()
        {
            List<ItemUnit> list = new List<ItemUnit>();
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetallItemsUnits");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;         
        }
       
        public async Task<List<Item>> GetForSale()
        {
            List<Item> list = new List<Item>();
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetForSale");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<Item>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list; 
        }

        public async Task<List<ItemUnit>> GetIU()
        {
            List<ItemUnit> list = new List<ItemUnit>();
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetIU");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }

        public async Task<List<ItemUnit>> GetActiveItemsUnits()
        {
            List<ItemUnit> list = new List<ItemUnit>();
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetActiveItemsUnits");

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;   
        }
        public async Task<List<ItemUnit>> GetUnitsForSales(int branchId)
        {
            List<ItemUnit> list = new List<ItemUnit>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("branchId", branchId.ToString());
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetUnitsForSales",parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;
        }
            //***************************************

            
            public async Task<List<ItemUnit>> getSmallItemUnits(int itemId, int itemUnitId)
        {

            List<ItemUnit> list = new List<ItemUnit>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("itemId", itemId.ToString());
            parameters.Add("itemUnitId", itemUnitId.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("itemsUnits/getSmallItemUnits", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    list.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return list;        

        }
        public async Task<decimal> largeToSmallUnitQuan(int fromItemUnit, int toItemUnit)
        {
            int AvailableAmount = 0;

           
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("fromItemUnit", fromItemUnit.ToString());
            parameters.Add("toItemUnit", toItemUnit.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("itemsUnits/largeToSmallUnitQuan", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    AvailableAmount = int.Parse(c.Value);
                }
            }
            return AvailableAmount;

        }
        //local of largeToSmallUnitQuan
        public int getUnitConversionQuan(int fromItemUnit, int toItemUnit,List<ItemUnit> itemUnits)
        {
            int amount = 0;

            var unit = itemUnits.Where(x => x.itemUnitId == toItemUnit).FirstOrDefault();

            var upperUnit = itemUnits.Where(x => x.subUnitId == unit.unitId  && x.subUnitId != x.unitId )
                .Select(x => new ItemUnit()
                {
                    unitValue = x.unitValue,
                    itemUnitId = x.itemUnitId
                }).FirstOrDefault();
            if (upperUnit != null)
            {
                try
                {
                    amount = (int)upperUnit.unitValue;
                }
                catch { }

                if (fromItemUnit == upperUnit.itemUnitId)
                    return amount;

                amount += (int)upperUnit.unitValue * getUnitConversionQuan(fromItemUnit, upperUnit.itemUnitId,itemUnits);
            }
            return amount;

        }
        public async Task<decimal> smallToLargeUnit(int fromItemUnit, int toItemUnit)
        {
            int AvailableAmount = 0;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("fromItemUnit", fromItemUnit.ToString());
            parameters.Add("toItemUnit", toItemUnit.ToString());

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("itemsUnits/smallToLargeUnitQuan",parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    AvailableAmount = int.Parse(c.Value);
                }
            }
            return AvailableAmount;

        }
        // same of smallToLargeUnit
        public int getLargeUnitConversionQuan(int fromItemUnit, int toItemUnit,List<ItemUnit> itemUnits)
        {
            int amount = 0;

            var unit = itemUnits.Where(x => x.itemUnitId == toItemUnit).Select(x => new { x.unitId, x.subUnitId, x.unitValue }).FirstOrDefault();
            var smallUnit = itemUnits.Where(x => x.unitId == unit.subUnitId ).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

            if (toItemUnit == smallUnit.itemUnitId)
            {
                amount = 1;
                return amount;
            }

            if (smallUnit != null)
                amount += (int)unit.unitValue * getLargeUnitConversionQuan(fromItemUnit, smallUnit.itemUnitId,itemUnits);

            return amount;
        }
        public async Task<decimal> fromUnitToUnitQuantity(int quantity, int itemId, int fromItemUnitId, int toItemUnitId)
        {
            int remain = 0;
            int _ConversionQuantity;
            int _ToQuantity = 0;

            if (quantity != 0)
            {
                List<ItemUnit> smallUnits = await getSmallItemUnits(itemId, fromItemUnitId);

                var isSmall = smallUnits.Find(x => x.itemUnitId == toItemUnitId);
                if (isSmall != null) // from-unit is bigger than to-unit
                {
                    _ConversionQuantity = (int)await largeToSmallUnitQuan(fromItemUnitId, toItemUnitId);
                    _ToQuantity = quantity * _ConversionQuantity;

                }
                else
                {
                    _ConversionQuantity = (int)await smallToLargeUnit(fromItemUnitId, toItemUnitId);

                    if (_ConversionQuantity != 0)
                    {
                        _ToQuantity = quantity / _ConversionQuantity;
                        remain = quantity - (_ToQuantity * _ConversionQuantity); // get remain quantity which cannot be changeed
                    }
                }
            }

            return _ToQuantity;
        }

        public async Task<List<ItemUnit>> GetIUBranchWithCount(int branchId)
        {
            List<ItemUnit> items = new List<ItemUnit>();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("branchId", branchId.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("ItemsUnits/GetIUBranchWithCount", parameters);
            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    items.Add(JsonConvert.DeserializeObject<ItemUnit>(c.Value, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" }));
                }
            }
            return items;
        }
    }
}
