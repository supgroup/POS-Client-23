using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Group = POS.Classes.Group;
using Object = POS.Classes.Object;

namespace POS.Classes
{
    public class FillCombo
    {
        static public string deliveryPermission = "setUserSetting_delivery";
        static public string administrativeMessagesPermission = "setUserSetting_administrativeMessages";
        static public string administrativePosTransfersPermission = "setUserSetting_administrativePosTransfers";

        public enum UserControls
        {
            root,
            // purchases
            payInvoice,
            // sales
            receiptInvoice,
            // storage
            receiptOfPurchaseInvoice,
            itemsExport,
        }

        #region agent
        static public Agent agent = new Agent();
        static public List<Agent> agentsList;
        static public async Task<IEnumerable<Agent>> RefreshAgents()
        {
            agentsList = await agent.GetAll();

            customersList = agentsList.Where(x => x.type == "c" && x.isActive == 1).ToList();
            vendorsList = agentsList.Where(x => x.type == "v" && x.isActive == 1).ToList();

            agent = new Agent();
            agent.agentId = 0;
            agent.name = "-";

            customersList.Insert(0, agent);
            vendorsList.Insert(0, agent);

            return agentsList;
        }
        #region Vendors
        static public List<Agent> vendorsList;
        static public List<Agent> vendorsActiveForAccountListType_d;
        static public List<Agent> vendorsActiveForAccountListType_p;
        static public List<Agent> vendorsListReport;
        static public async Task<IEnumerable<Agent>> RefreshVendorAllReport()
        {
            vendorsListReport = await agent.Get("v");
            return vendorsListReport;
        }
        static public async Task<IEnumerable<Agent>> RefreshVendors()
        {
            vendorsList = await agent.GetAgentsActive("v");
            agent = new Agent();
            agent.agentId = 0;
            agent.name = "-";
            vendorsList.Insert(0, agent);
            return vendorsList;
        }
        static public async Task FillComboVendors(ComboBox cmb)
        {
            if (vendorsList is null)
                await RefreshVendors();
            cmb.ItemsSource = vendorsList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }
        static public async Task<IEnumerable<Agent>> RefreshVendorsActiveForAccountType_d()
        {
            vendorsActiveForAccountListType_d = await agent.GetActiveForAccount("v", "d");
            return vendorsActiveForAccountListType_d;
        }
        static public async Task FillComboVendorsActiveForAccountType_d(ComboBox cmb)
        {
            if (vendorsActiveForAccountListType_d is null)
                await RefreshVendorsActiveForAccountType_d();
            cmb.ItemsSource = vendorsActiveForAccountListType_d;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }
        static public async Task<IEnumerable<Agent>> RefreshVendorsActiveForAccountType_p()
        {
            vendorsActiveForAccountListType_p = await agent.GetActiveForAccount("v", "p");
            return vendorsActiveForAccountListType_p;
        }
        static public async Task FillComboVendorsActiveForAccountType_p(ComboBox cmb)
        {
            if (vendorsActiveForAccountListType_p is null)
                await RefreshVendorsActiveForAccountType_p();
            cmb.ItemsSource = vendorsActiveForAccountListType_p;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region Customers
        static public List<Agent> customersList;
        static public List<Agent> customersActiveForAccountListType_d;
        static public List<Agent> customersActiveForAccountListType_p;
        static public List<Agent> customersListReport;
        static public async Task<IEnumerable<Agent>> RefreshCustomerAllReport()
        {
            customersListReport = await agent.Get("c");
            return customersListReport;
        }
        static public async Task<IEnumerable<Agent>> RefreshCustomers()
        {
            customersList = await agent.GetAgentsActive("c");
            return customersList;
        }
       
        static public async Task FillComboCustomers(ComboBox cmb)
        {
            if (customersList is null)
                await RefreshCustomers();
            cmb.ItemsSource = customersList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }
        static public async Task FillComboCustomers_withDefault(ComboBox cmb)
        {
            if (customersList is null)
                await RefreshCustomers();

            var ships = shippingCompaniesList.ToList();
            agent = new Agent();
            agent.agentId = 0;
            agent.name = "-";
            customersList.Insert(0, agent);
            cmb.ItemsSource = ships;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }


        static public async Task<IEnumerable<Agent>> RefreshCustomersActiveForAccountType_d()
        {
            customersActiveForAccountListType_d = await agent.GetActiveForAccount("c", "d");
            return customersActiveForAccountListType_d;
        }
        static public async Task FillComboCustomersActiveForAccountType_d(ComboBox cmb)
        {
            if (customersActiveForAccountListType_d is null)
                await RefreshCustomersActiveForAccountType_d();
            cmb.ItemsSource = customersActiveForAccountListType_d;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }
        static public async Task<IEnumerable<Agent>> RefreshCustomersActiveForAccountType_p()
        {
            customersActiveForAccountListType_p = await agent.GetActiveForAccount("c", "p");
            return customersActiveForAccountListType_p;
        }
        static public async Task FillComboCustomersActiveForAccountType_p(ComboBox cmb)
        {
            if (customersActiveForAccountListType_p is null)
                await RefreshCustomersActiveForAccountType_p();
            cmb.ItemsSource = customersActiveForAccountListType_p;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "agentId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #endregion
        #region User
        static public User user = new User();
        static public List<User> usersList;
        static public List<User> usersAllList;
        static public List<User> usersActiveList;
        static public List<User> driversList;
        static public List<User> usersActiveForAccountListType_d;
        static public List<User> usersActiveForAccountListType_p;
        static public async Task<IEnumerable<User>> RefreshUsers()
        {
            usersList = await user.Get();
            return usersList;
        }
        static public async Task<IEnumerable<User>> RefreshAllUsers()
        {
            usersAllList = await user.GetAll();
            return usersAllList;
        }
        static public async Task<IEnumerable<User>> RefreshUsersActive()
        {
            usersActiveList = await user.GetUsersActive();
            return usersActiveList;
        }
        static public async Task<IEnumerable<User>> RefreshDrivers()
        {
            driversList = await user.getBranchSalesMan(MainWindow.branchID.Value, deliveryPermission);
            return driversList;
        }
        static public async Task FillComboUsers(ComboBox cmb)
        {
            if (usersList is null)
                await RefreshUsers();
            var users = usersList.Where(x => x.isActive == 1).ToList();
            user = new User();
            user.userId = 0;
            user.name = "-";
            users.Insert(0, user);
            cmb.ItemsSource = users;
            cmb.DisplayMemberPath = "fullName";
            cmb.SelectedValuePath = "userId";
            cmb.SelectedIndex = -1;
        }
        static public async Task FillComboUsersWithoutAdmin(ComboBox cmb)
        {
            if (usersList is null)
                await RefreshUsers();
            var users = usersList.Where(x => x.isActive == 1 && x.isAdmin != true).ToList();
            user = new User();
            user.userId = 0;
            user.name = "-";
            users.Insert(0, user);
            cmb.ItemsSource = users;
            cmb.DisplayMemberPath = "fullName";
            cmb.SelectedValuePath = "userId";
            cmb.SelectedIndex = -1;
        }

        static public async Task FillComboDrivers(ComboBox cmb)
        {
            if (driversList is null)
                await RefreshDrivers();
            cmb.ItemsSource = driversList;
            cmb.DisplayMemberPath = "fullName";
            cmb.SelectedValuePath = "userId";
            cmb.SelectedIndex = -1;
        }
        static public async Task FillComboDrivers_withDefault(ComboBox cmb)
        {
            if (driversList is null)
                await RefreshDrivers();
            List<User> _driversList = driversList.ToList();
            var dr = new User();
            dr.userId = 0;
            dr.name = "-";
            _driversList.Insert(0, dr);

            cmb.ItemsSource = _driversList;
            cmb.DisplayMemberPath = "fullName";
            cmb.SelectedValuePath = "userId";
            cmb.SelectedIndex = -1;
        }




        static public async Task<IEnumerable<User>> RefreshUsersActiveForAccountType_d()
        {
            usersActiveForAccountListType_d = await user.GetActiveForAccount("d");
            return usersActiveForAccountListType_d;
        }
        static public async Task FillComboUsersActiveForAccountType_d(ComboBox cmb)
        {
            if (usersActiveForAccountListType_d is null)
                await RefreshUsersActiveForAccountType_d();
            cmb.ItemsSource = usersActiveForAccountListType_d;
            cmb.DisplayMemberPath = "fullName";
            cmb.SelectedValuePath = "userId";
            cmb.SelectedIndex = -1;
        }
        static public async Task<IEnumerable<User>> RefreshUsersActiveForAccountType_p()
        {
            usersActiveForAccountListType_p = await user.GetActiveForAccount("p");
            return usersActiveForAccountListType_p;
        }
        static public async Task FillComboUsersActiveForAccountType_p(ComboBox cmb)
        {
            if (usersActiveForAccountListType_p is null)
                await RefreshUsersActiveForAccountType_p();
            cmb.ItemsSource = usersActiveForAccountListType_p;
            cmb.DisplayMemberPath = "fullName";
            cmb.SelectedValuePath = "userId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region ShippingCompanies
        static public ShippingCompanies shippingCompanie = new ShippingCompanies();
        static public List<ShippingCompanies> shippingCompaniesList;
        static public List<ShippingCompanies> shippingCompaniesAllList;
        static public List<ShippingCompanies> shippingCompaniesActiveForAccountListType_d;
        static public List<ShippingCompanies> shippingCompaniesActiveForAccountListNotLocalType_d;
        static public List<ShippingCompanies> shippingCompaniesActiveForAccountListNotLocalType_p;
        static public async Task<IEnumerable<ShippingCompanies>> RefreshShippingCompanies()
        {
            shippingCompaniesList = await shippingCompanie.Get();
            shippingCompaniesList = shippingCompaniesList.Where(X => X.isActive == 1).ToList();
            return shippingCompaniesList;
        }
        static public async Task<IEnumerable<ShippingCompanies>> RefreshShippingCompaniesAll()
        {
            shippingCompaniesAllList = await shippingCompanie.Get();
            return shippingCompaniesAllList;
        }
        static public async Task FillComboShippingCompaniesWithDefault(ComboBox cmb)
        {
            if (shippingCompaniesList is null)
                await RefreshShippingCompanies();

            var shippingCompanies = shippingCompaniesList.ToList();
            shippingCompanie = new ShippingCompanies();
            shippingCompanie.shippingCompanyId = 0;
            shippingCompanie.name = "-";
            shippingCompanie.deliveryType = "";
            shippingCompanies.Insert(0, shippingCompanie);
            cmb.ItemsSource = shippingCompanies;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "shippingCompanyId";
            cmb.SelectedIndex = -1;
        }




        static public async Task<IEnumerable<ShippingCompanies>> RefreshShippingCompaniesActiveForAccountType_d()
        {
            shippingCompaniesActiveForAccountListType_d = await shippingCompanie.GetForAccount("d");
            return shippingCompaniesActiveForAccountListType_d;
        }
        static public async Task FillComboShippingCompaniesActiveForAccountType_d(ComboBox cmb)
        {
            if (shippingCompaniesActiveForAccountListType_d is null)
                await RefreshShippingCompaniesActiveForAccountType_d();
            cmb.ItemsSource = shippingCompaniesActiveForAccountListType_d;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "shippingCompanyId";
            cmb.SelectedIndex = -1;
        }
        static public async Task<IEnumerable<ShippingCompanies>> RefreshShippingCompaniesActiveForAccountNotLocalType_d()
        {
            shippingCompaniesActiveForAccountListNotLocalType_d = await shippingCompanie.GetForAccount("d");
            shippingCompaniesActiveForAccountListNotLocalType_d = shippingCompaniesActiveForAccountListNotLocalType_d.Where(sh => sh.deliveryType != "local").ToList();
            return shippingCompaniesActiveForAccountListNotLocalType_d;
        }
        static public async Task FillComboShippingCompaniesActiveForAccountNotLocalType_d(ComboBox cmb)
        {
            if (shippingCompaniesActiveForAccountListNotLocalType_d is null)
                await RefreshShippingCompaniesActiveForAccountNotLocalType_d();
            cmb.ItemsSource = shippingCompaniesActiveForAccountListNotLocalType_d;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "shippingCompanyId";
            cmb.SelectedIndex = -1;
        }
        static public async Task<IEnumerable<ShippingCompanies>> RefreshShippingCompaniesActiveForAccountNotLocalType_p()
        {
            shippingCompaniesActiveForAccountListNotLocalType_p = await shippingCompanie.GetForAccount("p");
            shippingCompaniesActiveForAccountListNotLocalType_p = shippingCompaniesActiveForAccountListNotLocalType_p.Where(sh => sh.deliveryType != "local").ToList();
            return shippingCompaniesActiveForAccountListNotLocalType_p;
        }
        static public async Task FillComboShippingCompaniesActiveForAccountNotLocalType_p(ComboBox cmb)
        {
            if (shippingCompaniesActiveForAccountListNotLocalType_p is null)
                await RefreshShippingCompaniesActiveForAccountNotLocalType_p();
            cmb.ItemsSource = shippingCompaniesActiveForAccountListNotLocalType_p;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "shippingCompanyId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region fill cards
        static public Card card = new Card();
        static public List<Card> cardsList;
        static public async Task<IEnumerable<Card>> RefreshCards()
        {
            cardsList = await card.GetAll();
            cardsList = cardsList.Where(c => c.isActive == 1).ToList();
            return cardsList;
        }
        #endregion
        #region Category
        static public Category category = new Category();
        static public List<Category> categoriesList;
        static public async Task<IEnumerable<Category>> RefreshCategories()
        {
            categoriesList = await category.GetAllCategories();
            return categoriesList;
        }

        #endregion
        #region Unit
        static public Unit unit = new Unit();
        static public List<Unit> allUnitsList;
        static public List<Unit> specialUnitsList;
        static public async Task<IEnumerable<Unit>> RefreshAllUnits()
        {
            allUnitsList = await unit.GetU();
            return allUnitsList;
        }
        static public async Task<IEnumerable<Unit>> RefreshSpecialUnits()
        {
            specialUnitsList = await unit.Get();
            specialUnitsList = specialUnitsList.Where(u => u.name != "package" && u.name != "service").ToList();
            return specialUnitsList;
        }
        static public async Task FillComboSpecialUnits(ComboBox cmb)
        {
            if (specialUnitsList is null)
                await RefreshSpecialUnits();
            cmb.ItemsSource = specialUnitsList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "unitId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region Property
        static public Property property = new Property();
        static public List<Property> propertysList;
        static public async Task<IEnumerable<Property>> RefreshPropertys()
        {
            propertysList = await property.Get();
            propertysList = propertysList.Where(x => x.isActive == 1).ToList();
            return propertysList;
        }
        static public async Task FillComboPropertys(ComboBox cmb)
        {
            if (propertysList is null)
                await RefreshPropertys();
            cmb.ItemsSource = propertysList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "propertyId";
            cmb.SelectedIndex = -1;
        }
        static public async Task FillComboPropertysWithDefault(ComboBox cmb)
        {
            if (propertysList is null)
                await RefreshPropertys();

            var list = propertysList.ToList();
            var property = new Property();
            property.propertyId = 0;
            property.name = "-";
            list.Insert(0, property);

            cmb.ItemsSource = list;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "propertyId";
            cmb.SelectedIndex = 0;
        }
        #endregion
        #region StorageCost
        static public StorageCost storageCost = new StorageCost();
        static public List<StorageCost> storageCostsList;
        static public async Task<IEnumerable<StorageCost>> RefreshStorageCosts()
        {
            storageCostsList = await storageCost.Get();
            storageCostsList = storageCostsList.Where(x => x.isActive == 1).ToList();
            return storageCostsList;
        }
        static public async Task FillComboStorageCosts(ComboBox cmb)
        {
            if (storageCostsList is null)
                await RefreshStorageCosts();
            cmb.ItemsSource = storageCostsList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "storageCostId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region Branch
        static public Branch branch = new Branch();
        static public List<Branch> branchsAllList;
        static public List<Branch> branchsActiveList_b;
        static public List<Branch> branchesAllWithoutMainReport;

        static public async Task<IEnumerable<Branch>> RefreshBranchsWithoutMainReport()
        {
            branchesAllWithoutMainReport = await branch.GetAllWithoutMain("b");
            return branchesAllWithoutMainReport;
        }
        static public async Task<IEnumerable<Branch>> RefreshBranchsAll ()
        {
            branchsAllList = await branch.GetAll();
            return branchsAllList;
        }
        static public async Task<IEnumerable<Branch>> RefreshBranchsActive_b()
        {
            branchsActiveList_b = await branch.GetBranchesActive("b");
            return branchsActiveList_b;
        }
        static public async Task FillComboBranchsActive_b(ComboBox cmb)
        {
            if (branchsActiveList_b is null)
                await RefreshBranchsActive_b();
            cmb.ItemsSource = branchsActiveList_b;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "branchId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region Bank
        static public Bank bank = new Bank();
        static public List<Bank> banksList;
        static public async Task<IEnumerable<Bank>> RefreshBanks()
        {
            banksList = await bank.Get();
            banksList = banksList.Where(x => x.isActive == 1).ToList();
            return banksList;
        }
        static public async Task FillComboBanks(ComboBox cmb)
        {
            if (banksList is null)
                await RefreshBanks();
            cmb.ItemsSource = banksList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "bankId";
            cmb.SelectedIndex = -1;
        }
        #endregion
        #region  CountryCode
        static public CountryCode countryCode = new CountryCode();
        static public List<CountryCode> countryCodesList;
        static public List<CountryCode> regionsList = new List<CountryCode>();

        static public async Task<IEnumerable<CountryCode>> RefreshCountryCodes()
        {
            countryCodesList = await countryCode.GetAllCountries();
            return countryCodesList;
        }

        static public async Task<IEnumerable<CountryCode>> RefreshRegions()
        {
            regionsList = await countryCode.GetAllRegion();
            return regionsList;
        }
        #endregion
        #region  City
        static public City city = new City();
        static public List<City> citysList;
        static public async Task<IEnumerable<City>> RefreshCitys()
        {
            citysList = await city.Get();
            return citysList;
        }
        #endregion
        #region pos
        static public Pos pos = new Pos();
        static public List<Pos> posAllReport;
        static public async Task<IEnumerable<Pos>> RefreshPosAllReport()
        {
            posAllReport = await pos.Get();
            return posAllReport;
        }
        #endregion
        #region Section
        static public Section section = new Section();
        static public List<Section> branchSectionsList;
        static public async Task<IEnumerable<Section>> RefreshBranchSections()
        {
            branchSectionsList = await section.getBranchSections(MainWindow.branchID.Value);
            return branchSectionsList;
        }
        static public async Task FillComboBranchSections(ComboBox cmb)
        {
            if (branchSectionsList is null)
                await RefreshBranchSections();
            cmb.ItemsSource = branchSectionsList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "sectionId";
            cmb.SelectedIndex = -1;
        }
        #endregion

        #region invoice
        public static List<Invoice> invoices = new List<Invoice>();
        #endregion

        #region settings
        static SettingCls setModel = new SettingCls();
        static SetValues valueModel = new SetValues();
        static UserSetValues uSetValueModel = new UserSetValues();

        public static List<SettingCls> settingsCls;
        public static List<SetValues> settingsValues;
        public static List<UserSetValues> userSetValuesLst ;
        static public async Task<IEnumerable<SettingCls>> RefreshSettings()
        {
            settingsCls = await setModel.GetAll();
            return settingsCls;
        }

        static public async Task<IEnumerable<SetValues>> RefreshSettingsValues()
        {
            settingsValues = await valueModel.GetAll();
            return settingsValues;
        }

        static public async Task<IEnumerable<UserSetValues>> RefreshUserSetValues()
        {
            userSetValuesLst = await uSetValueModel.GetAll();
            return userSetValuesLst;
        }

        static public async Task<string> getSetValue(string setName)
        {
            if (settingsCls.Count == 0)
                await RefreshSettings();
            var set = settingsCls.Where(s => s.name == setName).FirstOrDefault<SettingCls>();
            var setId = set.settingId;
            if (settingsValues.Count == 0)
                await RefreshSettingsValues();
             var setValue = settingsValues.Where(i => i.settingId == setId).FirstOrDefault().value;

            return setValue;
        }
        #endregion

        #region objects
        public static Object _object = new Object();
        public static IEnumerable<Object> objectsList;
        static public async Task<IEnumerable<Object>> RefreshObjectsList()
        {
            objectsList = await _object.GetAll();
            objectsList = objectsList.Where(x => x.objectType != "-");
            return objectsList;
        }
        #endregion
        #region GroupObject
        public static GroupObject groupObject = new GroupObject();
        public static IEnumerable<GroupObject> groupObjectsList;
        static public async Task<IEnumerable<GroupObject>> RefreshGroupObjectList()
        {
            groupObjectsList = await groupObject.GetAll();
            return groupObjectsList;
        }
        #endregion

        #region Group
        public static Group group = new Group();
        public static IEnumerable<Group> groupsList;
        static public async Task<IEnumerable<Group>> RefreshGroupList()
        {
            groupsList = await group.GetAll();
            return groupsList;
        }

        static public async Task FillComboGroup(ComboBox cmb)
        {
            if (groupsList is null)
                await RefreshGroupList();
            cmb.ItemsSource = groupsList;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "groupId";
        }
        #endregion

        #region warranty
        static public Warranty warranty = new Warranty();
        static public List<Warranty> warrantiesList;
        
        static public async Task<IEnumerable<Warranty>> RefreshWarranties()
        {
            warrantiesList = await warranty.GetAll();
            return warrantiesList;
        }

        static public async Task FillComboWarranty(ComboBox cmb)
        {
            if (warrantiesList is null)
                await RefreshWarranties();
            List<Warranty> wLst = new List<Warranty>();
            wLst = warrantiesList.Where(w => w.isActive == true).ToList();
            cmb.ItemsSource = wLst;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "warrantyId";
            cmb.SelectedIndex = -1;
        }
        #endregion

        #region slice
        static public Slice slice = new Slice();
        static public List<Slice> slicesList;
        static public List<Slice> slicesUserList;

        static public async Task<IEnumerable<Slice>> RefreshSlices()
        {
            slicesList = await slice.GetAll();
            return slicesList;
        }
        static public async Task FillComboSlices(ComboBox cmb)
        {
            if (slicesList is null)
                await RefreshSlices();
            List<Slice> sLst = new List<Slice>();
            sLst = slicesList.Where(w => w.isActive == true).ToList();
            cmb.ItemsSource = sLst;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "sliceId";
        }

        static public async Task<IEnumerable<Slice>> RefreshSlicesUser()
        {
            slicesUserList = await slice.GetAllowedSlicesByUserId(MainWindow.userLogin.userId);
            return slicesUserList;
        }
        static public async Task FillComboSlicesUser(ComboBox cmb)
        {
            if (slicesUserList is null)
                await RefreshSlicesUser();
            List<Slice> sLst = new List<Slice>();
            sLst = slicesUserList.Where(w => w.isActive == true).ToList();
            cmb.ItemsSource = sLst;
            cmb.DisplayMemberPath = "name";
            cmb.SelectedValuePath = "sliceId";
        }
        #endregion

        #region items && units
        static public Item item = new Item();
        static public List<Item> itemsList;
        static public ItemUnit itemUnitModel = new ItemUnit();
        static public List<ItemUnit> itemUnitsList;
        static public async Task<IEnumerable<Item>> RefreshItems()
        {
            itemsList = await item.GetItemsWichHasUnits();
            return itemsList;
        }
        static public async Task fillItemCombo(ComboBox cmb)
        {
            if (itemsList is null)
                await RefreshItems();


            cmb.ItemsSource = itemsList.ToList();
            cmb.SelectedValuePath = "itemId";
            cmb.DisplayMemberPath = "name";
        }
        static public async Task<IEnumerable<ItemUnit>> RefreshUnitsByItem(int itemId)
        {
            itemUnitsList = await itemUnitModel.GetItemUnits(itemId);
            return itemUnitsList;
        }
        static public async Task fillItemUnitCombo(ComboBox cmb , int itemId)
        {
            //if (itemUnitsList is null)
                await RefreshUnitsByItem(itemId);

            cmb.ItemsSource = itemUnitsList.ToList();
            cmb.SelectedValuePath = "itemUnitId";
            cmb.DisplayMemberPath = "mainUnit";
        }
        #endregion

        #region ItemUnit
        static public ItemUnit itemunit = new ItemUnit();
        static public List<ItemUnit> itemunitsList;
        static public async Task<IEnumerable<ItemUnit>> RefreshItemUnits()
        { 
            itemunitsList = await itemunit.Getall();
            return itemunitsList;
        }
        #endregion
        #region printerList
        static public InvoiceTypesPrinters invTypeModel = new InvoiceTypesPrinters();
        static public List<InvoiceTypesPrinters> printersList;
        static public async Task<IEnumerable<InvoiceTypesPrinters>> RefreshPrintersList()
        {
            printersList = await invTypeModel.GetByPosForPrint(MainWindow.posLogIn.posId);
            return printersList;
        }

        #endregion


        #region InvoiceTypes
        static public InvoiceTypes invoiceTypes = new InvoiceTypes();
        static public List<InvoiceTypes> invoiceTypessList;
        static public async Task<IEnumerable<InvoiceTypes>> RefreshInvoiceTypess()
        {
            invoiceTypessList = await invoiceTypes.GetAll();
            return invoiceTypessList;
        }
        static public async Task FillComboInvoiceTypess(ComboBox cmb)
        {
            if (invoiceTypessList is null)
                await RefreshInvoiceTypess();

            cmb.ItemsSource = invoiceTypessList;
            cmb.DisplayMemberPath = "translate";
            cmb.SelectedValuePath = "invoiceTypeId";
        }
        #endregion
        #region Papersize
        static public Papersize papersize = new Papersize();
        static public List<Papersize> papersizesList;
        static public async Task<IEnumerable<Papersize>> RefreshPapersizes()
        {
            papersizesList = await papersize.GetAll();
            return papersizesList;
        }
        static public async Task FillComboPapersizes(ComboBox cmb)
        {
            if (papersizesList is null)
                await RefreshPapersizes();

            cmb.ItemsSource = papersizesList;
            cmb.DisplayMemberPath = "paperSize1";
            cmb.SelectedValuePath = "sizeId";
        }
        static public async Task FillComboSmallPapersizes(ComboBox cmb)
        {
            if (papersizesList is null)
                await RefreshPapersizes();

            cmb.ItemsSource = papersizesList.Where(x => x.sizeValue == "5.7cm").ToList();
            cmb.DisplayMemberPath = "paperSize1";
            cmb.SelectedValuePath = "sizeId";
        }
        static public async Task FillComboSalePapersizes(ComboBox cmb)
        {
            if (papersizesList is null)
                await RefreshPapersizes();

            cmb.ItemsSource = papersizesList.Where(x => x.printfor.Contains("sal")).ToList();
            cmb.DisplayMemberPath = "paperSize1";
            cmb.SelectedValuePath = "sizeId";
        }
        #endregion
        #region taxes
        static public Taxes taxes = new Taxes();
        static public List<Taxes> taxessList;

        static public async Task<IEnumerable<Taxes>> RefreshTaxess()
        {
            taxessList = await taxes.GetAll();
            return taxessList;
        }
       
        #endregion
        #region taxTypes
        static public TaxTypes taxTypes = new TaxTypes();
        static public List<keyValueString> taxTypessList;

        static public List<keyValueString> RefreshTaxTypess()
        {
            taxTypessList = new List<keyValueString> {
                new keyValueString { key = "sales" ,  value = MainWindow.resourcemanager.GetString("SalesTax")},
                //new keyValueString { key = "income" ,  value = MainWindow.resourcemanager.GetString("IncomeTax")},

            };
            //taxTypessList = await taxTypes.GetAll();
            return taxTypessList;
        }
        static public void FillComboTaxTypess(ComboBox cmb)
        {
            if (taxTypessList is null)
                 RefreshTaxTypess();
            cmb.ItemsSource = taxTypessList;
            cmb.DisplayMemberPath = "value";
            cmb.SelectedValuePath = "key";
        }

        #endregion

        static public InvoiceTypesPrinters invoiceTypesPrinters = new InvoiceTypesPrinters();

    }
}
