using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using netoaster;
using POS.Classes;
using POS.View;
using POS.View.accounts;
using POS.View.delivery;
using POS.View.reports;
using POS.View.Settings;
using POS.View.windows;
using WPFTabTip;

namespace POS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static ResourceManager resourcemanager;
        public static ResourceManager resourcemanagerreport;
        public static ResourceManager resourcemanagerAr;
        public static ResourceManager resourcemanagerEn;
        bool menuState = false;
        public static string first = "";
        public static string second = "";
        internal static int? userID;
        internal static User userLogin;
        internal static int? userLogInID;
        internal static Pos posLogIn = new Pos();
        internal static int? posID;
        internal static int? branchID;
        public static Branch loginBranch;
        bool isHome = false;
        internal static int? isInvTax;
        internal static decimal? tax;
        
        public static int Idletime = 14;
        public static int threadtime = 5;
        public static List<ItemUnitUser> itemUnitsUsers = new List<ItemUnitUser>();
        public static ItemUnitUser itemUnitsUser = new ItemUnitUser();

        static public GroupObject groupObject = new GroupObject();
        static public List<GroupObject> groupObjects = new List<GroupObject>();
        static SettingCls setModel = new SettingCls();
        static SetValues valueModel = new SetValues();
        static int nameId, addressId, emailId, mobileId, phoneId, faxId, logoId, taxId;


        ImageBrush myBrush = new ImageBrush();
        NotificationUser notificationUser = new NotificationUser();

        public static DispatcherTimer timer;
        DispatcherTimer idletimer;//  logout timer
        DispatcherTimer threadtimer;//  repeat timer for check other login
        DispatcherTimer notTimer;//  repeat timer for notifications
                                
        public static string rep_printer_name;
        public static string sale_printer_name;
        public static string salePaperSize;
        public static string docPapersize;
        public static Boolean go_out = false;
        public static Boolean go_out_didNotAnyProcess = false;
        static public PosSetting posSetting = new PosSetting();
        internal static List<Pos> posList = new List<Pos>();

        static public List<Item> InvoiceGlobalItemsList = new List<Item>();
        static public List<ItemUnit> InvoiceGlobalItemUnitsList = new List<ItemUnit>();
        static public List<Item> InvoiceGlobalSaleUnitsList = new List<Item>();


        public ItemUnit GlobalItemUnit = new ItemUnit();
        public List<ItemUnit> GlobalItemUnitsList = new List<ItemUnit>();
        public Unit GlobalUnit = new Unit();
        //public List<Unit> GlobalUnitsList = new List<Unit>();

        Dash dashModel = new Dash();
        public static int _CachTransfersCount = 0;

        string deliveryPermission = "setUserSetting_delivery";

        public static async Task Getprintparameter()
        {
        }
        public static async Task GetReportlang()
        {
            List<SetValues> replangList = new List<SetValues>();
            replangList = await valueModel.GetBySetName("report_lang");
            AppSettings.Reportlang = replangList.Where(r => r.isDefault == 1).FirstOrDefault().value;

        }
        public static async Task getPrintersNames()
        {

            posSetting = new PosSetting();

            posSetting = await posSetting.GetByposId((int)MainWindow.posID);
            posSetting = posSetting.MaindefaultPrinterSetting(posSetting);

            if (posSetting.repname is null || posSetting.repname == "")
            {
                rep_printer_name = "";
            }
            else
            {
                rep_printer_name = Encoding.UTF8.GetString(Convert.FromBase64String(posSetting.repname));
            }
            if (posSetting.salname is null || posSetting.salname == "")
            {
                posSetting.salname = "";
            }
            else
            {
                sale_printer_name = Encoding.UTF8.GetString(Convert.FromBase64String(posSetting.salname));
            }

            salePaperSize = posSetting.saleSizeValue;
            docPapersize = posSetting.docPapersize;

        }
        public static async Task getprintSitting()
        {
            await getPrintersNames();
        }
        static public MainWindow mainWindow;
        public MainWindow()
        {
            try
            {
                InitializeComponent();

                mainWindow = this;
                windowFlowDirection();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        void windowFlowDirection()
        {
            #region translate
            if (AppSettings.lang.Equals("en"))
            {
                resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                grid_mainWindow.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                grid_mainWindow.FlowDirection = FlowDirection.RightToLeft;
            }
            #endregion
        }

        #region loading
        static List<keyValueBool> loadingList;
        List<string> catchError = new List<string>();
        int catchErrorCount = 0;
        

        bool loadingSuccess_getBalance = false;
        async void loading_getBalance()
        {
            //get print count
            try
            {
                await refreshBalance();
                loadingSuccess_getBalance = true;
            }
            catch (Exception ex)
            {
                //if(reloadingCount_getBalance < 2)
                //reloadingCount_getBalance++;
                catchError.Add("loading_getBalance");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_getBalance = true;
                }
                else
                    loading_getBalance();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getBalance)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getBalance"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        
        int reloadingCount_getUserPersonalInfo = 0;
        async void loading_getUserPersonalInfo()
        {
            #region user personal info
            txt_userName.Text = userLogin.name;
            txt_userJob.Text = userLogin.job;
            try
            {
                if (!string.IsNullOrEmpty(userLogin.image))
                {
                    byte[] imageBuffer = await userModel.downloadImage(userLogin.image); // read this as BLOB from your DB

                    var bitmapImage = new BitmapImage();

                    using (var memoryStream = new System.IO.MemoryStream(imageBuffer))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                    }

                    img_userLogin.Fill = new ImageBrush(bitmapImage);
                }
                else
                {
                    clearImg();
                }
            }
            catch (Exception ex)
            {
                //clearImg();
                reloadingCount_getUserPersonalInfo++;
                catchError.Add("loading_getUserPersonalInfo");
                catchErrorCount++;
                if (reloadingCount_getUserPersonalInfo < 3)
                    loading_getUserPersonalInfo();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getUserPersonalInfo"))
                {
                    item.value = true;
                    break;
                }
            }
            #endregion
        }

        bool loadingSuccess_getItemUnitsUsers = false;
        //int reloadingCount_getItemUnitsUsers = 0;
        async void loading_getItemUnitsUsers()
        {
            try
            {
                itemUnitsUsers = await itemUnitsUser.GetByUserId(userLogin.userId);
                loadingSuccess_getItemUnitsUsers = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getItemUnitsUsers");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_getItemUnitsUsers = true;
                }
                else
                    //if(reloadingCount_getItemUnitsUsers < 2)
                    loading_getItemUnitsUsers();
                //reloadingCount_getItemUnitsUsers++;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getItemUnitsUsers)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getItemUnitsUsers"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }

        bool loadingSuccess_getGroupObjects = false;
        //int reloadingCount_getGroupObjects = 0;
        async void loading_getGroupObjects()
        {
            try
            {
                groupObjects = await groupObject.GetUserpermission(userLogin.userId);

               

                if (groupObjects.Count > 150 || userLogin.groupId is null || SectionData.isAdminPermision())
                    loadingSuccess_getGroupObjects = true;
                else
                    loading_getGroupObjects();
            }
            catch (Exception ex)
            {
                //if (reloadingCount_getGroupObjects < 2)
                //reloadingCount_getGroupObjects++;
                catchError.Add("loading_getGroupObjects");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_getGroupObjects = true;
                }
                else
                    loading_getGroupObjects();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getGroupObjects)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getGroupObjects"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }

        int reloadingCount_getImage = 0;
        async void loading_getImage()
        {
            try
            {
                SetValues setV = new SetValues();
                await setV.getImg(AppSettings.logoImage);
            }
            catch (Exception ex)
            {
                reloadingCount_getImage++;
                catchError.Add("loading_getImage");
                catchErrorCount++;
                if (reloadingCount_getImage < 2)
                    loading_getImage();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getImage"))
                {
                    item.value = true;
                    break;
                }
            }
        }

        

        bool loadingSuccess_getprintSitting = false;
        //int reloadingCount_getprintSitting = 0;
        async void loading_getprintSitting()
        {
            try
            {
                await getprintSitting();
                loadingSuccess_getprintSitting = true;
            }
            catch (Exception ex)
            {
                //if(reloadingCount_getprintSitting < 2)
                //reloadingCount_getprintSitting++;
                catchError.Add("loading_getprintSitting");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getprintSitting = true;
                }
                else
                    loading_getprintSitting();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getprintSitting)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getprintSitting"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }

        bool loadingSuccess_GlobalItemUnitsList = false;
        //int reloadingCount_GlobalItemUnitsList = 0;
        async void loading_GlobalItemUnitsList()
        {
            try
            {
                GlobalItemUnitsList = await GlobalItemUnit.GetIU();
                loadingSuccess_GlobalItemUnitsList = true;
            }
            catch (Exception ex)
            {
                //if(reloadingCount_GlobalItemUnitsList < 2)
                //reloadingCount_GlobalItemUnitsList++;
                catchError.Add("loading_GlobalItemUnitsList");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_GlobalItemUnitsList = true;
                }
                else
                    loading_GlobalItemUnitsList();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_GlobalItemUnitsList)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_GlobalItemUnitsList"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }

        bool loadingSuccess_RefreshAllUnits = false;
        //int reloadingCount_RefreshAllUnits = 0;
        async void loading_RefreshAllUnits()
        {
            try
            {
                //GlobalUnitsList = await GlobalUnit.GetU();
                await FillCombo.RefreshAllUnits();
                loadingSuccess_RefreshAllUnits = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_RefreshAllUnits");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_RefreshAllUnits = true;
                }
                else
                    loading_RefreshAllUnits();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_RefreshAllUnits)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_RefreshAllUnits"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }

        bool loadingSuccess_POSList = false;
        //int reloadingCount_POSList = 0;
        async void loading_POSList()
        {
            try
            {
                posList = await posLogIn.Get();
                loadingSuccess_POSList = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_POSList");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_POSList = true;
                }
                else
                    loading_POSList();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_POSList)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_POSList"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        

        bool loadingSuccess_RefreshAgents = false;
        //int reloadingCount_RefreshAgents = 0;
        async void loading_RefreshAgents()
        {
            try
            {
                await FillCombo.RefreshAgents();

                loadingSuccess_RefreshAgents = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_RefreshAgents");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_RefreshAgents = true;
                }
                else
                    loading_RefreshAgents();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_RefreshAgents)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_RefreshAgents"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }

        bool loadingSuccess_getObjectsAll = false;
        async void loading_getObjectsAll()
        {
            try
            {
                await FillCombo.RefreshObjectsList();
                loadingSuccess_getObjectsAll = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getObjectsAll");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getObjectsAll = true;
                }
                else
                    loading_getObjectsAll();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getObjectsAll)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getObjectsAll"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_getSetValues = false;
        async void loading_getSetValues()
        {
            try
            {
                await FillCombo.RefreshSettings();
                await FillCombo.RefreshSettingsValues();
                await FillCombo.RefreshUserSetValues();

                loadingSuccess_getSetValues = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_getSetValues");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getSetValues = true;
                }
                else
                    loading_getSetValues();

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getSetValues)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getSetValues"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_getCountries = false;
        async void loading_getCountries()
        {
            try
            {
                await FillCombo.RefreshCountryCodes();

                loadingSuccess_getCountries = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getCountries");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getCountries = true;
                }
                else
                    loading_getCountries();

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getCountries)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getCountries"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_getCities = false;
        async void loading_getCities()
        {
            try
            {
                await FillCombo.RefreshCitys();

                loadingSuccess_getCities = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getCities");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getCities = true;
                }
                else
                    loading_getCities();

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getCities)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getCities"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_getRegions = false;
        async void loading_getRegions()
        {
            try
            {
                await FillCombo.RefreshRegions();

                loadingSuccess_getRegions = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getRegions");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getRegions = true;
                }
                else
                    loading_getRegions();

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getRegions)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getRegions"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_getActiveUsers = false;
        async void loading_getActiveUsers()
        {
            try
            {
                await FillCombo.RefreshUsersActive();

                loadingSuccess_getActiveUsers = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getActiveUsers");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_getActiveUsers = true;
                }
                else
                    loading_getActiveUsers();

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_getActiveUsers)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getActiveUsers"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        AvtivateServer ac = new AvtivateServer();
        bool loadingSuccess_activationSite = false;
        async void loading_activationSite()
        {
            try
            {
                SetValues setmod = new SetValues();
                setmod = await ac.getactivesite();
                AppSettings.activationSite = setmod.value;

                loadingSuccess_activationSite = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_activationSite");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_activationSite = true;
                }
                else
                    loading_activationSite();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);

            }
            if (loadingSuccess_activationSite)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_activationSite"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        ProgramDetails progDetailsModel = new ProgramDetails();
        bool loadingSuccess_defaultServerStatus = false;
        async void loading_getDefaultServerStatus()
        {
            try
            {
                AppSettings.progDetails = await progDetailsModel.getCurrentInfo();

                loadingSuccess_defaultServerStatus = true;
            }
            catch (Exception ex)
            {

                catchError.Add("loading_getDefaultServerStatus");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                loadingSuccess_defaultServerStatus = true;
                }
                else
                    loading_activationSite();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);

            }
            if (loadingSuccess_defaultServerStatus)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_getDefaultServerStatus"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        #endregion

        public async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainWindow, "mainWindow_load");


                #region bonni
                #pragma warning disable CS0436 // Type conflicts with imported type
                TabTipAutomation.IgnoreHardwareKeyboard = HardwareKeyboardIgnoreOptions.IgnoreAll;
                #pragma warning restore CS0436 // Type conflicts with imported type
                #pragma warning disable CS0436 // Type conflicts with imported type
                #pragma warning restore CS0436 // Type conflicts with imported type
                #pragma warning disable CS0436 // Type conflicts with imported type
                TabTipAutomation.ExceptionCatched += TabTipAutomationOnTest;
                #pragma warning restore CS0436 // Type conflicts with imported type
                this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
                //this.Width = SystemParameters.MaximizedPrimaryScreenHeight;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();

                // idle timer
                idletimer = new DispatcherTimer();
                idletimer.Interval = TimeSpan.FromMinutes(1);
                idletimer.Tick += timer_Idle;
                idletimer.Start();


                //thread
                threadtimer = new DispatcherTimer();
                threadtimer.Interval = TimeSpan.FromSeconds(threadtime);
                threadtimer.Tick += timer_Thread;
                threadtimer.Start();




                #endregion

                translate();
                try
                {
                    tb_version.Text = AppSettings.CurrentVersion;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #region export error file
                ErrorClass errorcls = new ErrorClass();
                await errorcls.saveTodayErrors();
                #endregion
                #region loading
                loadingList = new List<keyValueBool>();
                bool isDone = true;
                loadingList.Add(new keyValueBool { key = "loading_getUserPersonalInfo", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getItemUnitsUsers", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getGroupObjects", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getprintSitting", value = false });
                loadingList.Add(new keyValueBool { key = "loading_GlobalItemUnitsList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_RefreshAllUnits", value = false });
                loadingList.Add(new keyValueBool { key = "loading_POSList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getBalance", value = false });
                loadingList.Add(new keyValueBool { key = "loading_RefreshAgents", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getImage", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getObjectsAll", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getSetValues", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getRegions", value = false });
                loadingList.Add(new keyValueBool { key = "loading_activationSite", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getDefaultServerStatus", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getCountries", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getCities", value = false });
                loadingList.Add(new keyValueBool { key = "loading_getActiveUsers", value = false });

                loading_getItemUnitsUsers();
                loading_getUserPersonalInfo();
                loading_getGroupObjects();
                loading_getprintSitting();
                loading_GlobalItemUnitsList();
                loading_RefreshAllUnits();
                loading_POSList();
                loading_getBalance();
                loading_RefreshAgents();
                loading_getImage();
                loading_getObjectsAll();
                loading_getSetValues();
                loading_getRegions();
                loading_activationSite();
                loading_getDefaultServerStatus();
                loading_getCountries();
                loading_getCities();
                loading_getActiveUsers();
                do
                {
                    isDone = true;
                    foreach (var item in loadingList)
                    {
                        if (item.value == false)
                        {
                            isDone = false;
                            break;
                        }
                    }
                    if (!isDone)
                    {
                        //MessageBox.Show("not done");
                        //string s = "";
                        //foreach (var item in loadingList)
                        //{
                        //    s += item.name + " - " + item.value + "\n";
                        //}
                        //MessageBox.Show(s);
                        await Task.Delay(0500);
                        //MessageBox.Show("do");
                    }
                }
                while (!isDone);
                //string s = "";
                //foreach (var item in catchError)
                //{
                //    s += item + "- \n";
                //}
                //MessageBox.Show(s + "count: " + catchErrorCount);
                //MessageBox.Show("MainWindow.groupObjects.Count = " + MainWindow.groupObjects.Count);
                #endregion
                #region check if Password has been change
                var updateUser = FillCombo.usersActiveList.Where(x => x.userId == userLogin.userId).FirstOrDefault();
                if (userLogin.password != updateUser.password)
                {
                    wd_messageBoxWithIcon w2 = new wd_messageBoxWithIcon();
                    w2.contentText1 = MainWindow.resourcemanager.GetString("yourPasswordHasBeenChange");
                    w2.Show();

                }
                #endregion
                #region check if have a new message
                if (!string.IsNullOrWhiteSpace(AppSettings.messageTitle) && !string.IsNullOrWhiteSpace(AppSettings.messageContent))
                {
                    #region
                    wd_messageBox w = new wd_messageBox();
                    w.titleText2 = AppSettings.messageTitle;
                    w.contentText2 = AppSettings.messageContent;
                    w.ShowDialog();
                    #endregion
                }
                #endregion
                #region notifications 
                setNotifications();
                setTimer();
                #endregion
                #region Permision
                permission();


                if (MainWindow.groupObject.HasPermissionAction(deliveryPermission, MainWindow.groupObjects, "one"))
                    md_deliveryWaitConfirmUser.Visibility = Visibility.Visible;
                else
                    md_deliveryWaitConfirmUser.Visibility = Visibility.Collapsed;
                #endregion

                //SelectAllText
                EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox), System.Windows.Controls.TextBox.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText));


                SetNotificationsLocation();

                #region expire date
                daysremain daysr = await userModel.getRemainDayes();
                if (daysr.expirestate == "e" && daysr.days <= 10)
                {

                    wd_messageBoxWithIcon w = new wd_messageBoxWithIcon();
                    if (daysr.days >= 1)
                    {
                        w.contentText1 = resourcemanager.GetString("trExpireNote1") + " " + daysr.days.ToString() + " " + resourcemanager.GetString("trExpireDays");
                        w.Show();
                    }
                    else if (daysr.days == 0)
                    {


                        if (daysr.hours >= 0 && daysr.hours <= 24)
                        {
                            if (daysr.hours == 0)
                            {
                                if (daysr.minute >= 0)
                                {
                                    w.contentText1 = resourcemanager.GetString("trExpireNote2");
                                    w.Show();
                                }


                            }
                            else
                            {
                                w.contentText1 = resourcemanager.GetString("trExpireNote2");
                                w.Show();
                            }



                        }
                        //else 
                        //{
                        //    w.contentText1 = resourcemanager.GetString("trExpireNote3")+" " + (daysr.days* -1 ).ToString() + " " + resourcemanager.GetString("trAgo");
                        //}
                    }

                }

                #endregion


                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow, "mainWindow_load");
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow, "mainWindow_load");
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void SetNotificationsLocation()
        {
            #region notifications location
            var thickness = bdrMain.Margin;
            bdrMain.Margin = new Thickness(0, 70, thickness.Right + stp_userName.ActualWidth, 0);
            #endregion
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                if (textBox != null)
                    if (!textBox.IsReadOnly)
                        textBox.SelectAll();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public async static Task<bool> loadingDefaultPath(string first, string second)
        {
            bool load = false;
            if (!string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(second))
            {
                foreach (Button button in FindControls.FindVisualChildren<Button>(MainWindow.mainWindow))
                {
                    if (button.Tag != null)
                        if (button.Tag.ToString() == first && (MainWindow.groupObject.HasPermission(first, MainWindow.groupObjects) || SectionData.isAdminPermision()))
                        {
                            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            load = true;
                            break;
                        }
                }
                await Task.Delay(0500);
                if (first == "home")
                    loadingSecondLevel(second, uc_home.Instance);
                if (first == "catalog")
                    loadingSecondLevel(second, UC_catalog.Instance);
                if (first == "storage")
                    loadingSecondLevel(second, POS.View.uc_storage.Instance);
                if (first == "purchase")
                    loadingSecondLevel(second, uc_purchases.Instance);
                if (first == "sales")
                    loadingSecondLevel(second, uc_sales.Instance);
                if (first == "delivery")
                    loadingSecondLevel(second, uc_delivery.Instance);
                if (first == "accounts")
                    loadingSecondLevel(second, uc_accounts.Instance);
                if (first == "reports")
                    loadingSecondLevel(second, uc_reports.Instance);
                if (first == "sectionData")
                    loadingSecondLevel(second, UC_SectionData.Instance);
                if (first == "settings")
                    loadingSecondLevel(second, uc_settings.Instance);

            }
            return load;
        }

        static void loadingSecondLevel(string second, UserControl userControl)
        {
            userControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            var button = userControl.FindName("btn_" + second) as Button;
            if (button != null)
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        async void permission()
        {
            bool loadWindow = false;
            loadWindow = await loadingDefaultPath(AppSettings.firstPath, AppSettings.secondPath);
            if (!SectionData.isAdminPermision())
                foreach (Button button in FindControls.FindVisualChildren<Button>(this))
                {
                    if (button.Tag != null)
                        if (MainWindow.groupObject.HasPermission(button.Tag.ToString(), MainWindow.groupObjects))
                        {
                            button.Visibility = Visibility.Visible;
                            if (!loadWindow)
                            {
                                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                loadWindow = true;
                            }
                        }
                        else button.Visibility = Visibility.Collapsed;
                }
            else
            {
                if (!loadWindow)
                    BTN_Home_Click(BTN_home, null);
            }
        }
        #region notifications
        Invoice invoice = new Invoice();
        int _OrdersWaitCount = 0;
        int _NotCount = 0;
        int _MessageCount = 0;
        private void setTimer()
        {
            notTimer = new DispatcherTimer();
            notTimer.Interval = TimeSpan.FromSeconds(60); //1 minute
            notTimer.Tick += notTimer_Tick;
            notTimer.Start();
        }
        private void notTimer_Tick(object sendert, EventArgs et)
        {
            try
            {
                setNotifications();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }



        }
        private async void setNotifications()
        {
            try
            {
                string invoiceType = "s";
                MainWindowNot mainWindowNot = new MainWindowNot();
                mainWindowNot = await dashModel.GetMainNotification((int)userID, "alert", (int)posID, invoiceType, "Ready", "all", "p");
                AppSettings.PosBalance = mainWindowNot.PosBalance;
                setNotificationCount(mainWindowNot.UserNotCount);
                setMessageCount(mainWindowNot.UserMessageCount);
                setCashTransferNotification(mainWindowNot.CashTransferCount);
                setOrdersWaitNotification(mainWindowNot.UseraitingOrderCount);
                setBoxState(mainWindowNot.BoxState);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void setNotificationCount(int notCount)
        {

            if (notCount != _NotCount)
            {
                if (notCount > 9)
                {
                    notCount = 9;
                    md_notificationCount.Badge = "+" + notCount.ToString();
                }
                else if (notCount == 0) md_notificationCount.Badge = "";
                else
                    md_notificationCount.Badge = notCount.ToString();
            }
            _NotCount = notCount;
        }

         private void setMessageCount(int msgCount)
        {

            if (msgCount != _MessageCount)
            {
                if (msgCount > 9)
                {
                    msgCount = 9;
                    md_messageCount.Badge = "+" + msgCount.ToString();
                }
                else if (msgCount == 0) md_messageCount.Badge = "";
                else
                    md_messageCount.Badge = msgCount.ToString();
            }
            _MessageCount = msgCount;
        }
        private void setOrdersWaitNotification(int ordersCount)
        {
            try
            {
                if (ordersCount != _OrdersWaitCount)
                {
                    if (ordersCount > 9)
                    {
                        md_deliveryWaitConfirmUser.Badge = "+9";
                    }
                    else if (ordersCount == 0) md_deliveryWaitConfirmUser.Badge = "";
                    else
                        md_deliveryWaitConfirmUser.Badge = ordersCount.ToString();
                }
                _OrdersWaitCount = ordersCount;
            }
            catch { }
        }
        private void setCashTransferNotification(int posCachTransfers)
        {
            try
            {
                SectionData.refreshNotification(md_transfers, ref _CachTransfersCount, posCachTransfers);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        #endregion
        void timer_Idle(object sender, EventArgs e)
        {

            try
            {
                if (IdleClass.IdleTime.Minutes >= Idletime)
                {
                    go_out_didNotAnyProcess = true;
                    BTN_Close_Click(null, null);
                    idletimer.Stop();
                    //popup here
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        void timer_Thread(object sendert, EventArgs et)
        {
            try
            {
                if (go_out)
                {
                    BTN_Close_Click(null, null);
                    threadtimer.Stop();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static async Task refreshBalance()
        {
            try
            {
                posLogIn = await posLogIn.getById(posID.Value);
                AppSettings.PosBalance = (decimal)posLogIn.balance;
                mainWindow.txt_cashValue.Text = SectionData.DecTostring(posLogIn.balance);
                mainWindow.txt_cashSympol.Text = AppSettings.Currency;

                if (posLogIn.boxState == "o")
                    mainWindow.txt_cashTitle.Foreground = Application.Current.Resources["MainColor"] as SolidColorBrush;
                else
                    mainWindow.txt_cashTitle.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, mainWindow , mainWindow.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        public void setBoxState(string BoxState)
        {
            try
            {
                mainWindow.txt_cashValue.Text = SectionData.DecTostring(AppSettings.PosBalance);

                if (BoxState == "o")
                    mainWindow.txt_cashTitle.Foreground = Application.Current.Resources["MainColor"] as SolidColorBrush;
                else
                    mainWindow.txt_cashTitle.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        public static void setBalance()
        {
            mainWindow.txt_cashValue.Text = SectionData.DecTostring(AppSettings.PosBalance);

        }
        void timer_Tick(object sender, EventArgs e)
        {
            try
            {

                txtTime.Text = DateTime.Now.ToShortTimeString();
                txtDate.Text = DateTime.Now.ToShortDateString();


            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void TabTipAutomationOnTest(Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
        private static List<string> QueryWmiKeyboards()
        {
            using (var searcher = new ManagementObjectSearcher(new SelectQuery("Win32_Keyboard")))
            using (var result = searcher.Get())
            {
                return result
                    .Cast<ManagementBaseObject>()
                    .SelectMany(keyboard =>
                        keyboard.Properties
                            .Cast<PropertyData>()
                            .Where(k => k.Name == "Description")
                            .Select(k => k.Value as string))
                    .ToList();
            }
        }
        void FN_tooltipVisibility(Button btn)
        {
            ToolTip T = btn.ToolTip as ToolTip;
            if (T.Visibility == Visibility.Visible)
                T.Visibility = Visibility.Hidden;
            else T.Visibility = Visibility.Visible;
        }
        
        async Task close()
        {
            //log out
            //update lognin record
            if (!go_out)
            {
                await updateLogninRecord();
            }
            timer.Stop();
            idletimer.Stop();
            threadtimer.Stop();
        }
        private async void BTN_Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainWindow);
                if (go_out)
                {
                    await close();
                    this.Visibility = Visibility.Hidden;
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_messageBox w = new wd_messageBox();
                    w.contentText2 = MainWindow.resourcemanager.GetString("trUserLoginFromOtherPos");
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                    #endregion

                    SectionData.deleteDirectoryFiles(Global.TMPFolder);
                    Application.Current.Shutdown();
                }
                else if (go_out_didNotAnyProcess)
                {
                    await close();
                    this.Visibility = Visibility.Hidden;
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_messageBoxWithIcon w = new wd_messageBoxWithIcon();
                    w.contentText1 = MainWindow.resourcemanager.GetString("trLoggedOutBecauseDidNotDoneAnyProcess");
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                    #endregion

                    SectionData.deleteDirectoryFiles(Global.TMPFolder);

                    Application.Current.Shutdown();
                }
                else
                {
                    await close();
                    SectionData.deleteDirectoryFiles(Global.TMPFolder);

                    Application.Current.Shutdown();
                }

                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void BTN_Minimize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        void colorTextRefreash(TextBlock txt)
        {
            txt_home.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_catalog.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_storage.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_purchases.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_sales.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_sales.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_delivery.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_delivery.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_accounting.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_reports.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_sectiondata.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            txt_settings.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));

            txt.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E8E8E8"));
        }
        void fn_ColorIconRefreash(Path p)
        {
            path_iconSettings.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconSectionData.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconReports.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconAccounts.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconSales.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconDelivery.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconPurchases.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconStorage.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconCatalog.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));
            path_iconHome.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#9FD7F8"));

            p.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E8E8E8"));
        }
        public void translate()
        {
            tt_menu.Content = resourcemanager.GetString("trMenu");
            tt_home.Content = resourcemanager.GetString("trHome");
            txt_home.Text = resourcemanager.GetString("trHome");
            tt_catalog.Content = resourcemanager.GetString("trCatalog");
            txt_catalog.Text = resourcemanager.GetString("trCatalog");
            tt_storage.Content = resourcemanager.GetString("trStore");
            txt_storage.Text = resourcemanager.GetString("trStore");
            tt_purchase.Content = resourcemanager.GetString("trPurchases");
            txt_purchases.Text = resourcemanager.GetString("trPurchases");
            tt_sales.Content = resourcemanager.GetString("trSales");
            txt_sales.Text = resourcemanager.GetString("trSales");
            tt_delivery.Content = resourcemanager.GetString("trDelivery");
            txt_delivery.Text = resourcemanager.GetString("trDelivery");
            tt_accounts.Content = resourcemanager.GetString("trAccounting");
            txt_accounting.Text = resourcemanager.GetString("trAccounting");
            tt_reports.Content = resourcemanager.GetString("trReports");
            txt_reports.Text = resourcemanager.GetString("trReports");
            tt_sectionData.Content = resourcemanager.GetString("trSectionData");
            txt_sectiondata.Text = resourcemanager.GetString("trSectionData");
            tt_settings.Content = resourcemanager.GetString("trSettings");
            txt_settings.Text = resourcemanager.GetString("trSettings");
            txt_cashTitle.Text = resourcemanager.GetString("trCashBalance");

            mi_refreshLoading.Header = resourcemanager.GetString("trRefresh");
            mi_changePassword.Header = resourcemanager.GetString("trChangePassword");
            BTN_logOut.Header = resourcemanager.GetString("trLogOut");

            txt_notifications.Text = resourcemanager.GetString("trNotifications");
            txt_noNoti.Text = resourcemanager.GetString("trNoNotifications");
            btn_showAll.Content = resourcemanager.GetString("trShowAll");

            tb_versionTitle.Text = resourcemanager.GetString("Version");

        }

        //فتح
        private void BTN_Menu_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!menuState)
                {
                    Storyboard sb = this.FindResource("Storyboard1") as Storyboard;
                    sb.Begin();
                    menuState = true;
                }
                else
                {
                    Storyboard sb = this.FindResource("Storyboard2") as Storyboard;
                    sb.Begin();
                    menuState = false;
                }


                #region tooltipVisibility
                FN_tooltipVisibility(BTN_menu);
                FN_tooltipVisibility(BTN_home);
                FN_tooltipVisibility(btn_catalog);
                FN_tooltipVisibility(btn_storage);
                FN_tooltipVisibility(btn_purchase);
                FN_tooltipVisibility(btn_sales);
                FN_tooltipVisibility(btn_delivery);
                FN_tooltipVisibility(btn_reports);
                FN_tooltipVisibility(btn_accounts);
                FN_tooltipVisibility(btn_sectionData);
                FN_tooltipVisibility(btn_settings);
                #endregion


            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        void fn_pathOpenCollapsed()
        {
            path_openCatalog.Visibility = Visibility.Collapsed;
            path_openStorage.Visibility = Visibility.Collapsed;
            path_openPurchases.Visibility = Visibility.Collapsed;
            path_openSales.Visibility = Visibility.Collapsed;
            path_openDelivery.Visibility = Visibility.Collapsed;
            path_openReports.Visibility = Visibility.Collapsed;
            path_openSectionData.Visibility = Visibility.Collapsed;
            path_openSettings.Visibility = Visibility.Collapsed;
            path_openHome.Visibility = Visibility.Collapsed;
            path_openAccount.Visibility = Visibility.Collapsed;


        }
        void FN_pathVisible(Path p)
        {
            fn_pathOpenCollapsed();
            p.Visibility = Visibility.Visible;
        }




        private void btn_Keyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (TabTip.Close())
                {
                #pragma warning disable CS0436 // Type conflicts with imported type
                    TabTip.OpenUndockedAndStartPoolingForClosedEvent();
                    #pragma warning restore CS0436 // Type conflicts with imported type
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void BTN_message_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Opacity = 0.2;
                wd_administrativeMessages w = new wd_administrativeMessages();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                setNotifications();
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        User userModel = new User();
        UsersLogs userLogsModel = new UsersLogs();

        async Task<bool> updateLogninRecord()
        {
            UsersLogs userLog = new UsersLogs();
            userLog = await userLogsModel.GetByID(userLogInID.Value);
            //update user record
            userLogin.isOnline = 0;
            int s = (int)await userModel.updateOnline(userLogin.userId);
            //update lognin record
            s = (int)await userLogsModel.Save(userLog);
            return true;
        }

        private void BTN_SectionData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_sectiondata);
                FN_pathVisible(path_openSectionData);
                fn_ColorIconRefreash(path_iconSectionData);
                grid_main.Children.Clear();
                grid_main.Children.Add(UC_SectionData.Instance);

                isHome = true;
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        public void initializationMainTrack(string tag, int level)
        {
            if (level == 0)
            {
                txt_secondLevelTrack.Visibility = Visibility.Collapsed;
                txt_thirdLevelTrack.Visibility = Visibility.Collapsed;
                #region  mainWindow
                if (tag == "home")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trHome");
                else if (tag == "catalog")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trCatalog");
                else if (tag == "storage")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trStore");
                else if (tag == "purchase")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trPurchases");
                else if (tag == "sales")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trSales");
                else if (tag == "delivery")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trDelivery");
                else if (tag == "accounts")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trAccounting");
                else if (tag == "reports")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trReports");
                else if (tag == "sectionData")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trSectionData");
                else if (tag == "settings")
                    txt_firstLevelTrack.Text = "> " + resourcemanager.GetString("trSettings");
                #endregion
            }
            else if (level == 1)
            {
                txt_secondLevelTrack.Visibility = Visibility.Visible;
                txt_thirdLevelTrack.Visibility = Visibility.Collapsed;
                #region  storage
                if (tag == "locations")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trLocation");
                else if (tag == "section")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trSection");
                else if (tag == "reciptOfInvoice")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trInvoice");
                else if (tag == "itemsStorage")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStorage");
                else if (tag == "importExport")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trMovements");
                else if (tag == "itemsDestroy")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trDestructive");
                else if (tag == "shortage")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trShortage");
                else if (tag == "inventory")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStocktaking");
                else if (tag == "storageStatistic")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStatistic");
                else if (tag == "serials")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trSerials");
                #endregion
                #region  Account
                else if (tag == "posAccounting")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPOS");
                else if (tag == "payments")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPayments");
                else if (tag == "received")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trReceived");
                else if (tag == "bonds")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trBonds");
                else if (tag == "banksAccounting")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trBanks");
                else if (tag == "ordersAccounting")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trOrders");
                else if (tag == "subscriptions")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trSubscriptions");
                else if (tag == "accountsStatistic")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStatistic");
                else if (tag == "dailyClosing")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trDailyClosing");
                #endregion
                #region  catalog
                else if (tag == "categories")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trCategories");
                else if (tag == "item")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trItems");
                else if (tag == "service")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trService");
                else if (tag == "package")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPackage");
                else if (tag == "properties")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trProperties");
                else if (tag == "units")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trUnits");
                else if (tag == "storageCost")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStorageCost");
                #endregion
                #region  purchase
                else if (tag == "payInvoice")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trInvoice");
                else if (tag == "purchaseOrder")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trOrders");
                else if (tag == "purchaseStatistic")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStatistic");
                #endregion
                #region  sales
                else if (tag == "reciptInvoice")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trInvoice");
                else if (tag == "coupon")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trCoupon");
                else if (tag == "offer")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trOffer");
                else if (tag == "quotation")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trQuotations");
                else if (tag == "salesOrders")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trOrders");
                else if (tag == "medals")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trMedals");
                else if (tag == "membership")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trMembership");
                else if (tag == "salesStatistic")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trDaily");
                else if (tag == "slice")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("prices");
                #endregion
                #region  delivery
                else if (tag == "deliveryManagement")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("management");
                else if (tag == "driversManagement")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("deliveryList");
                #endregion
                #region  sectionData
                else if (tag == "suppliers")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trSuppliers");
                else if (tag == "customers")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trCustomers");
                else if (tag == "users")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trUsers");
                else if (tag == "branches")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trBranches");
                else if (tag == "stores")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStores");
                else if (tag == "pos")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPOS");
                else if (tag == "banks")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trBanks");
                else if (tag == "cards")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPayment1");
                else if (tag == "shippingCompany")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trShippingCompanies");
                 else if (tag == "taxes")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trTax");
                #endregion
                #region  settings
                else if (tag == "general")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trGeneral");
                else if (tag == "reportsSettings")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trReports");
                else if (tag == "permissions")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPermission");
                else if (tag == "emailsSetting")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trEmail");
                else if (tag == "emailTemplates")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trEmailTemplates");
                else if (tag == "packageBookSetting")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("packageBookSettings");
                #endregion
                #region  report
                else if (tag == "reports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trReports");
                else if (tag == "storageReports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trStorage");
                else if (tag == "purchaseReports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trPurchases");
                else if (tag == "salesReports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trSales");
                else if (tag == "deliveryReports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trDelivery");
                else if (tag == "accountsReports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trAccounting");
                else if (tag == "usersReports")
                    txt_secondLevelTrack.Text = "> " + resourcemanager.GetString("trUsers");
                #endregion
            }
            else if (level == 2)
            {
                txt_thirdLevelTrack.Visibility = Visibility.Visible;

                #region  report

                if (tag == "invoice")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trInvoice");
                else if (tag == "order")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trOrders");
                else if (tag == "item")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trItems");
                else if (tag == "itemCost")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trItemsCost");

                #region  storageReports
                else if (tag == "stock")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trStock");
                else if (tag == "external")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trExternal");
                else if (tag == "internal")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trInternal");
                else if (tag == "stocktaking")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trStocktaking");
                else if (tag == "destroied")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trDestructives");
                else if (tag == "direct")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trDirectEntry");
                else if (tag == "serial")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trSerials");
                else if (tag == "properties")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trProperties");

                #endregion

                #region  salesReports
                else if (tag == "promotion")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trPromotion");
                else if (tag == "quotation")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trQuotations");
                else if (tag == "dailySales")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trDailySales");

                #endregion


                #region  accountsReports
                else if (tag == "payments")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trPayments");
                else if (tag == "recipient")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trRecipientTooltip");
                else if (tag == "received")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trReceived");
                else if (tag == "bank")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trBank");
                else if (tag == "pos")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trPOS");
                else if (tag == "statement")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trAccountStatement");
                else if (tag == "fund")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trCashBalance");
                else if (tag == "profit")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trProfits");
                else if (tag == "closing")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trDailyClosing");
                else if (tag == "tax")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("trTax");
                else if (tag == "commision")
                    txt_thirdLevelTrack.Text = "> " + resourcemanager.GetString("commission");

                #endregion

                #endregion
            }

        }
      
        private void BTN_Home_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                colorTextRefreash(txt_home);
                FN_pathVisible(path_openHome);
                fn_ColorIconRefreash(path_iconHome);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_home.Instance);
                if (isHome)
                {
                    //uc_home.Instance.timerAnimation();
                    isHome = false;
                }
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void BTN_catalog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_catalog);
                FN_pathVisible(path_openCatalog);
                fn_ColorIconRefreash(path_iconCatalog);
                grid_main.Children.Clear();
                grid_main.Children.Add(UC_catalog.Instance);
                isHome = true;
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainWindow);
                await close();
                Application.Current.Shutdown();

                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

       
        private async void Mi_refreshLoading_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window_Loaded(this, null);
                uc_general.firstLoading = false;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Mi_changePassword_Click(object sender, RoutedEventArgs e)
        {//change password
            try
            {

                Window.GetWindow(this).Opacity = 0.2;
                wd_changePassword w = new wd_changePassword();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;


            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        SetValues v = new SetValues();

        
        private void Mi_more_Click(object sender, RoutedEventArgs e)
        {

        }
        public static string GetUntilOrEmpty(string text, string stopAt)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }
        List<NotificationUser> notifications;
        private async void BTN_notifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bdrMain.Visibility == Visibility.Collapsed)
                {
                    bdrMain.Visibility = Visibility.Visible;
                    bdrMain.RenderTransform = Animations.borderAnimation(-25, bdrMain, true);
                    notifications = await notificationUser.GetByUserId(userID.Value, "alert", posID.Value);
                    IEnumerable<NotificationUser> orderdNotifications = notifications.OrderByDescending(x => x.createDate);
                    await notificationUser.setAsRead(userID.Value, posID.Value, "alert");
                    md_notificationCount.Badge = "";
                    if (notifications.Count == 0)
                    {
                        grd_notifications.Visibility = Visibility.Collapsed;
                        txt_noNoti.Visibility = Visibility.Visible;
                    }

                    else
                    {
                        grd_notifications.Visibility = Visibility.Visible;
                        txt_noNoti.Visibility = Visibility.Collapsed;

                        string secondPart = "";
                        if (orderdNotifications.Select(obj => obj.path).FirstOrDefault() != null)
                            secondPart = orderdNotifications.Select(obj => obj.path).FirstOrDefault() + " " + resourcemanager.GetString("Days");

                        txt_firstNotiTitle.Text = resourcemanager.GetString(orderdNotifications.Select(obj => obj.title).FirstOrDefault());

                        txt_firstNotiContent.Text = GetUntilOrEmpty(orderdNotifications.Select(obj => obj.ncontent).FirstOrDefault(), ":")
                          + " : " +
                          resourcemanager.GetString(orderdNotifications.Select(obj => obj.ncontent).FirstOrDefault().Substring(orderdNotifications.Select(obj => obj.ncontent).FirstOrDefault().LastIndexOf(':') + 1))
                          +" "+ secondPart;

                        txt_firstNotiDate.Text = orderdNotifications.Select(obj => obj.createDate).FirstOrDefault().ToString();

                        if (notifications.Count > 1)
                        {
                            if (orderdNotifications.Select(obj => obj.path).Skip(1).FirstOrDefault() != null)
                                secondPart = orderdNotifications.Select(obj => obj.path).Skip(1).FirstOrDefault() + " " + resourcemanager.GetString("Days");

                            txt_2NotiTitle.Text = resourcemanager.GetString(orderdNotifications.Select(obj => obj.title).Skip(1).FirstOrDefault());
                            txt_2NotiContent.Text = GetUntilOrEmpty(orderdNotifications.Select(obj => obj.ncontent).Skip(1).FirstOrDefault(), ":")
                          + " : " + resourcemanager.GetString(orderdNotifications.Select(obj => obj.ncontent).Skip(1).FirstOrDefault().Substring(orderdNotifications.Select(obj => obj.ncontent).Skip(1).FirstOrDefault().LastIndexOf(':') + 1))
                          + " " + secondPart;

                            txt_2NotiDate.Text = orderdNotifications.Select(obj => obj.createDate).Skip(1).FirstOrDefault().ToString();

                        }
                        if (notifications.Count > 2)
                        {
                            if (orderdNotifications.Select(obj => obj.path).Skip(2).FirstOrDefault() != null)
                                secondPart = orderdNotifications.Select(obj => obj.path).Skip(2).FirstOrDefault() + " " + resourcemanager.GetString("Days");

                            txt_3NotiTitle.Text = resourcemanager.GetString(orderdNotifications.Select(obj => obj.title).Skip(2).FirstOrDefault());
                            txt_3NotiContent.Text = GetUntilOrEmpty(orderdNotifications.Select(obj => obj.ncontent).Skip(2).FirstOrDefault(), ":")
                          + " : " + resourcemanager.GetString(orderdNotifications.Select(obj => obj.ncontent).Skip(2).FirstOrDefault().Substring(orderdNotifications.Select(obj => obj.ncontent).Skip(2).FirstOrDefault().LastIndexOf(':') + 1))
                           + " " + secondPart ;

                            txt_3NotiDate.Text = orderdNotifications.Select(obj => obj.createDate).Skip(2).FirstOrDefault().ToString();

                        }
                        if (notifications.Count > 3)
                        {
                            if (orderdNotifications.Select(obj => obj.path).Skip(3).FirstOrDefault() != null)
                                secondPart = orderdNotifications.Select(obj => obj.path).Skip(3).FirstOrDefault() + " " + resourcemanager.GetString("Days");

                            txt_4NotiTitle.Text = resourcemanager.GetString(orderdNotifications.Select(obj => obj.title).Skip(3).FirstOrDefault());
                            txt_4NotiContent.Text = GetUntilOrEmpty(orderdNotifications.Select(obj => obj.ncontent).Skip(3).FirstOrDefault(), ":")
                          + " : " + resourcemanager.GetString(orderdNotifications.Select(obj => obj.ncontent).Skip(3).FirstOrDefault().Substring(orderdNotifications.Select(obj => obj.ncontent).Skip(3).FirstOrDefault().LastIndexOf(':') + 1))
                           + " " + secondPart ;

                            txt_4NotiDate.Text = orderdNotifications.Select(obj => obj.createDate).Skip(3).FirstOrDefault().ToString();

                        }
                        if (notifications.Count > 4)
                        {
                            if (orderdNotifications.Select(obj => obj.path).Skip(4).FirstOrDefault() != null)
                                secondPart = orderdNotifications.Select(obj => obj.path).Skip(4).FirstOrDefault() + " " + resourcemanager.GetString("Days");

                            txt_5NotiTitle.Text = resourcemanager.GetString(orderdNotifications.Select(obj => obj.title).Skip(4).FirstOrDefault());
                            txt_5NotiContent.Text = GetUntilOrEmpty(orderdNotifications.Select(obj => obj.ncontent).Skip(4).FirstOrDefault(), ":")
                          + " : " + resourcemanager.GetString(orderdNotifications.Select(obj => obj.ncontent).Skip(4).FirstOrDefault().Substring(orderdNotifications.Select(obj => obj.ncontent).Skip(4).FirstOrDefault().LastIndexOf(':') + 1))
                           + " " + secondPart ;

                            txt_5NotiDate.Text = orderdNotifications.Select(obj => obj.createDate).Skip(4).FirstOrDefault().ToString();

                        }
                    }

                }
                else
                {
                    bdrMain.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_showAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Opacity = 0.2;
                wd_notifications w = new wd_notifications();
                w.notifications = notifications;
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {

                bdr_showAll.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {

                bdr_showAll.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bdrMain.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_info_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Opacity = 0.2;
                wd_info w = new wd_info();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_userImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Opacity = 0.2;
                wd_userInfo w = new wd_userInfo();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void clearImg()
        {
            Uri resourceUri = new Uri("pic/no-image-icon-90x90.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            myBrush.ImageSource = temp;
            img_userLogin.Fill = myBrush;

        }

        private void Btn_applicationStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainWindow);

                Window.GetWindow(this).Opacity = 0.2;
                wd_applicationStop w = new wd_applicationStop();
                w.ShowDialog();
                if (w.status == "o")
                    txt_cashTitle.Foreground = Application.Current.Resources["MainColor"] as SolidColorBrush;
                else
                    txt_cashTitle.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                Window.GetWindow(this).Opacity = 1;

                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_transfers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainWindow);

                Window.GetWindow(this).Opacity = 0.2;
                wd_transfers w = new wd_transfers();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                setNotifications();
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_deliveryWaitConfirmUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainWindow);

                Window.GetWindow(this).Opacity = 0.2;
                wd_deliveryWaitConfirmUser w = new wd_deliveryWaitConfirmUser();
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;

                setNotifications();
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_mainWindow);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }



        public void BTN_purchases_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_purchases);
                FN_pathVisible(path_openPurchases);
                fn_ColorIconRefreash(path_iconPurchases);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_purchases.Instance);
                isHome = true;
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

       



   
        public void BTN_sales_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_sales);
                FN_pathVisible(path_openSales);
                fn_ColorIconRefreash(path_iconSales);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_sales.Instance);
                isHome = true;
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void BTN_delivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_delivery);
                FN_pathVisible(path_openDelivery);
                fn_ColorIconRefreash(path_iconDelivery);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_delivery.Instance);
                isHome = true;
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void BTN_accounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_accounting);
                FN_pathVisible(path_openAccount);
                fn_ColorIconRefreash(path_iconAccounts);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_accounts.Instance);
                isHome = true;
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void BTN_reports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_reports);
                FN_pathVisible(path_openReports);
                fn_ColorIconRefreash(path_iconReports);
                isHome = true;
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_reports.Instance);
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void BTN_settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                colorTextRefreash(txt_settings);
                FN_pathVisible(path_openSettings);
                fn_ColorIconRefreash(path_iconSettings);
                isHome = true;
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_settings.Instance);
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void BTN_storage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                initializationMainTrack(button.Tag.ToString(), 0);
                colorTextRefreash(txt_storage);
                FN_pathVisible(path_openStorage);
                fn_ColorIconRefreash(path_iconStorage);
                grid_main.Children.Clear();
                grid_main.Children.Add(View.uc_storage.Instance);
                isHome = true;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

    }
}
