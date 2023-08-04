using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.Reporting.WinForms;
using System.IO;
namespace POS.View.Settings
{
    /// <summary>
    /// Interaction logic for uc_general.xaml
    /// </summary>
    public partial class uc_general : UserControl
    {
        #region variables
        static SetValues valueModel = new SetValues();
        AvtivateServer ac = new AvtivateServer();
        static UserSetValues usValueModel = new UserSetValues();
        static CountryCode countryModel = new CountryCode();
        static SettingCls set = new SettingCls();
        
        SetValues activationSite = new SetValues();
        static SetValues itemCost = new SetValues();
        static SetValues backupTime = new SetValues();
        static SetValues backupDailyEnabled = new SetValues();
        static SetValues accuracy = new SetValues();
        static SetValues dateForm = new SetValues();
        static SetValues returnPeriod = new SetValues();
        static SetValues freeDelivery = new SetValues();
        static UserSetValues slice = new UserSetValues();
        static public UserSetValues usLanguage = new UserSetValues();
        static CountryCode region = new CountryCode();
        static List<SetValues> languages = new List<SetValues>();
        string usersSettingsPermission = "general_usersSettings";
        string companySettingsPermission = "general_companySettings";

        static ProgramDetails progDetailsModel = new ProgramDetails();
        static ProgramDetails progDetails = new ProgramDetails();

        OpenFileDialog openFileDialog = new OpenFileDialog();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        ReportCls reportclass = new ReportCls();

        LocalReport rep = new LocalReport();
        private static uc_general _instance;
        TextBox tb;
        BrushConverter bc = new BrushConverter();
        #endregion

        public static uc_general Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_general();
                return _instance;
            }
        }
        public uc_general()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region loading
        static public bool firstLoading = true;
        List<keyValueBool> loadingList;
        async void loading_fillRegions()
        {
            try
            {
                await fillRegions();
                #region get default region
                await getDefaultRegion();
                if (region != null)
                {
                    cb_region.SelectedValue = region.countryId;
                    cb_region.Text = region.name;
                }
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillRegions"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async Task loading_fillLanguages()
        {
            try
            {
                await fillLanguages();
                #region get default language
                await getDefaultLanguage();
                if (usLanguage != null)
                    cb_language.SelectedValue = usLanguage.valId;
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillLanguages"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        /*
        async void loading_fillCurrencies()
        {
            try
            {
                await fillCurrencies();
                #region get default currency
                if (region != null)
                {
                    tb_currency.Text = region.currency;
                }
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillCurrencies"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        */
        async void loading_getDefaultItemCost()
        {
            try
            {
                #region get default item cost
                await getDefaultItemCost();
                if (itemCost != null)
                    tb_itemsCost.Text = itemCost.value;
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getDefaultItemCost"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_getDefaultReturnPeriod()
        {
            try
            {
                #region get default return period
                await getDefaultReturnPeriod();
                if (returnPeriod != null)
                    tb_returnPeriod.Text = returnPeriod.value;
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getDefaultReturnPeriod"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_getDefaultFreeDelivery()
        {
            try
            {
                #region get default free delivery
                await getDefaultFreeDelivery();
                if (freeDelivery != null)
                    tgl_freeDelivery.IsChecked = bool.Parse(freeDelivery.value);
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getDefaultFreeDelivery"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_getDefaultActivationSite()
        {
            try
            {
                //activationSite = await ac.getactivesite();
                if (AppSettings.activationSite != null)
                    tb_activationSite.Text = AppSettings.activationSite;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getDefaultActivationSite"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_getDefaultDateForm()
        {
            try
            {
                #region fill dateform
                DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
                var date = DateTime.Now;
                var typelist = new[] {
                    new { Text = date.ToString(dtfi.ShortDatePattern), Value = "ShortDatePattern" },
                    new { Text = date.ToString(dtfi.LongDatePattern) , Value = "LongDatePattern" },
                    new { Text =  date.ToString(dtfi.MonthDayPattern), Value = "MonthDayPattern" },
                    new { Text =  date.ToString(dtfi.YearMonthPattern), Value = "YearMonthPattern" },
                     };
                cb_dateForm.DisplayMemberPath = "Text";
                cb_dateForm.SelectedValuePath = "Value";
                cb_dateForm.ItemsSource = typelist;
                #endregion

                #region get default date form
                await getDefaultDateForm();
                if (dateForm != null)
                    cb_dateForm.SelectedValue = dateForm.value;
                else
                    cb_dateForm.SelectedIndex = -1;
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getDefaultDateForm"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async Task loading_getDefaultServerStatus()
        {
            try
            {
                fillTypeOnline();

                #region get default server status
                await getDefaultServerStatus();
                if (AppSettings.progDetails != null)
                {
                    if (progDetails.isOnlineServer.Value) cb_serverStatus.SelectedIndex = 0;
                    else cb_serverStatus.SelectedIndex = 1;
                }
                else
                    cb_serverStatus.SelectedIndex = -1;
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getDefaultServerStatus"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_fillAccuracy()
        {
            try
            {
                fillAccuracy();
                #region get default accracy
                await getDefaultAccuracy();
                if (accuracy != null)
                {
                    cb_accuracy.SelectedValue = accuracy.value;
                }
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillAccuracy"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async Task loading_getBackupTime()
        {
            try
            {
                #region get default backup time
                await getDefaultBackupTime();
                if (backupTime != null)
                    tp_backupTime.Text = DateTime.Parse(backupTime.value).ToString("T", CultureInfo.CurrentCulture);
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getBackupTime"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        bool isFromLoading = false;
        async Task loading_getBackupDailyEnabled()
        {
            try
            {
                #region get default backup daily enabled
                await getDefaultBackupDailyEnabled();
                if (backupDailyEnabled != null)
                {
                    isFromLoading = true;

                    if (backupDailyEnabled.value.Equals("1"))
                    {
                        tgl_backupTime.IsChecked = true;
                        Tgl_backupTime_Checked(tgl_backupTime , null);
                    }
                    else if (backupDailyEnabled.value.Equals("0"))
                    {
                        tgl_backupTime.IsChecked = false;
                        Tgl_backupTime_Unchecked(tgl_backupTime , null);
                    }

                    isFromLoading = false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_getBackupDailyEnabled"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_fillSlices()
        {
            try
            {
                //await fillSlices();
                await  FillCombo.FillComboSlicesUser(cb_defaultInvoiceSlice);

                #region get default slice
                await getDefaultSlice();
                if (slice != null)
                {
                    //cb_defaultInvoiceSlice.SelectedValue = slice.Value;
                    int sliceValue = 0;
                    try
                    {
                        sliceValue = int.Parse(slice.Value);
                    }
                    catch
                    {
                        sliceValue = 0;
                    }

                    ///default slice
                    if (sliceValue == 0 || FillCombo.slicesUserList.Where(w => w.isActive == true && w.sliceId == sliceValue).Count() == 0)
                        cb_defaultInvoiceSlice.SelectedIndex = 0;
                    else
                        cb_defaultInvoiceSlice.SelectedValue = sliceValue;
                }
                #endregion
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillSlices"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        #endregion

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
                tb = (TextBox)tp_backupTime.Template.FindName("PART_TextBox", tp_backupTime);
                tb.IsReadOnly = true;

                if (SectionData.isSupportPermision())
                {
                    brd_activationSite.Visibility = Visibility.Visible;
                    brd_serverStatus.Visibility = Visibility.Visible;
                    brd_backupTime.Visibility = Visibility.Visible;
                    brd_itemsCost.Visibility = Visibility.Visible;
                }
                else
                {
                    brd_activationSite.Visibility = Visibility.Collapsed;
                    brd_serverStatus.Visibility = Visibility.Collapsed;
                    brd_backupTime.Visibility = Visibility.Collapsed;
                    brd_itemsCost.Visibility = Visibility.Collapsed;
                }

                if (FillCombo.settingsCls == null)
                    await FillCombo.RefreshSettings();

                if (FillCombo.settingsValues == null)
                    await FillCombo.RefreshSettingsValues();

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                #region loading
                if (firstLoading)
                {
                    loadingList = new List<keyValueBool>();
                    bool isDone = true;
                    loadingList.Add(new keyValueBool { key = "loading_fillRegions", value = false });
                    //loadingList.Add(new keyValueBool { key = "loading_fillCurrencies", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getDefaultItemCost", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getDefaultActivationSite", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getDefaultDateForm", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_fillAccuracy", value = false });
                    //loadingList.Add(new keyValueBool { key = "loading_getDefaultServerStatus", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getBackupTime", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getBackupDailyEnabled", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_fillSlices", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getDefaultReturnPeriod", value = false });
                    loadingList.Add(new keyValueBool { key = "loading_getDefaultFreeDelivery", value = false });

                    loading_fillRegions();
                    //loading_fillCurrencies();
                    loading_getDefaultItemCost();
                    loading_getDefaultActivationSite();
                    loading_getDefaultDateForm();
                    loading_fillAccuracy();
                    //loading_getDefaultServerStatus();
                    loading_getBackupTime();
                    loading_getBackupDailyEnabled();
                    loading_fillSlices();
                    loading_getDefaultReturnPeriod();
                    loading_getDefaultFreeDelivery();
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
                    await loading_fillLanguages();
                    await loading_getDefaultServerStatus();
                    fillBackup();
                    firstLoading = false;
                }

                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region get default
        public static async Task<SetValues> getDefaultAccuracy()
        {
            accuracy = await SectionData.getSetValueBySetName("accuracy");
            return accuracy;
        }
        public static async Task<ProgramDetails> getDefaultServerStatus()
        {
            progDetails = AppSettings.progDetails;
            return AppSettings.progDetails;
        }
        public static async Task<SetValues> getDefaultBackupTime()
        {
            backupTime = await SectionData.getSetValueBySetName("backup_time");
            return backupTime;
        }
        public static async Task<SetValues> getDefaultReturnPeriod()
        {
            returnPeriod = await SectionData.getSetValueBySetName("returnPeriod");
            return returnPeriod;
        }
        public static async Task<SetValues> getDefaultFreeDelivery()
        {
            freeDelivery = await SectionData.getSetValueBySetName("freeDelivery");
            return freeDelivery;
        }
        public static async Task<SetValues> getDefaultBackupDailyEnabled()
        {
            backupDailyEnabled = await SectionData.getSetValueBySetName("backup_daily_enabled");
            return backupDailyEnabled;
        }
        public  async Task<CountryCode> getDefaultRegion()
        {
            if (FillCombo.regionsList.Count == 0)
                await FillCombo.RefreshRegions();

            region = FillCombo.regionsList.Where(r => r.isDefault == 1).FirstOrDefault<CountryCode>();
            return region;
        }
        public static async Task<SetValues> getDefaultItemCost()
        {
            itemCost = await SectionData.getSetValueBySetName("item_cost");
            return itemCost;
        }
        public static async Task<SetValues> getDefaultDateForm()
        {
            dateForm = await SectionData.getSetValueBySetName("dateForm");
            return dateForm;
        }
        public static async Task<UserSetValues> getDefaultLanguage()
        {
            if (FillCombo.settingsCls.Count == 0)
                await FillCombo.RefreshSettings();
            set = FillCombo.settingsCls.Where(l => l.name == "language").FirstOrDefault<SettingCls>();
            if (FillCombo.settingsValues.Count == 0)
                await FillCombo.RefreshSettingsValues();
            languages = FillCombo.settingsValues.Where(vl => vl.settingId == set.settingId).ToList<SetValues>();
            if (FillCombo.userSetValuesLst.Count() == 0)
                await FillCombo.RefreshUserSetValues();
            var curUserValues = FillCombo.userSetValuesLst.Where(c => c.userId == MainWindow.userID);
            foreach (var l in curUserValues)
            if (languages.Any(c => c.valId == l.valId))
            {
                usLanguage = l;
            }
            return usLanguage;
        }
        public static async Task<UserSetValues> getDefaultSlice()
        {
            if (FillCombo.userSetValuesLst.Count() == 0)
                await FillCombo.RefreshUserSetValues();
           slice = FillCombo.userSetValuesLst.Where(c => c.userId == MainWindow.userLogin.userId && c.note == "invoice_slice").FirstOrDefault();


          

            return slice;
        }
        #endregion

        #region save
        private void Btn_companyInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_companyInfo w = new wd_companyInfo();
                    w.isFirstTime = false;
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_saveRegion_Click(object sender, RoutedEventArgs e)
        {//save region
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    // string s = "";
                    int s = 0;
                    SectionData.validateEmptyComboBox(cb_region, p_errorRegion, tt_errorRegion, "trEmptyRegion");
                    if (!cb_region.Text.Equals(""))
                    {
                        int regionId = Convert.ToInt32(cb_region.SelectedValue);
                        if (regionId != 0)
                        {
                            s = (int)await countryModel.UpdateIsdefault(regionId);
                            if (!s.Equals(0))
                            {
                                if (FillCombo.settingsCls.Count == 0)
                                    await FillCombo.RefreshSettings();
                                var Region = FillCombo.regionsList.Where(r => r.countryId == s).FirstOrDefault<CountryCode>();
                                AppSettings.Currency = Region.currency;
                                AppSettings.CurrencyId = Region.currencyId;
                                AppSettings.countryId = s;
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        int lanID = 0;
        private async void Btn_saveLanguage_Click(object sender, RoutedEventArgs e)
        {//save language
            try
            {
                if (MainWindow.groupObject.HasPermissionAction(usersSettingsPermission, MainWindow.groupObjects, "one") ||
                    MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyComboBox(cb_language, p_errorLanguage, tt_errorLanguage, "trEmptyLanguage");
                    if (!cb_language.Text.Equals(""))
                    {
                        string lang = "";
                        if (cb_language.Text.Equals(MainWindow.resourcemanager.GetString("trEnglish")))
                        {
                            lang = "en";
                        }
                        else
                            lang = "ar";

                        if (usLanguage.id == 0)
                        {
                            if (lanID == 0)
                                usLanguage = new UserSetValues();
                            else
                                usLanguage.id = lanID;
                        }
                        if (Convert.ToInt32(cb_language.SelectedValue) != 0)
                        {
                            if (sender != null)
                                SectionData.StartAwait(grid_main);

                            usLanguage.userId = MainWindow.userID;
                            usLanguage.valId = Convert.ToInt32(cb_language.SelectedValue);
                            usLanguage.createUserId = MainWindow.userID;

                            int s = (int)await usValueModel.Save(usLanguage);
                            if (!s.Equals(0))
                            {
                                lanID = s;
                                //update language in main window
                                AppSettings.lang = lang;
                                //save to user settings
                                Properties.Settings.Default.Lang = lang;
                                Properties.Settings.Default.Save();

                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            if (sender != null)
                                SectionData.EndAwait(grid_main);
                            uc_settings objUC1 = new uc_settings();

                            //update languge in main window
                            MainWindow parentWindow = Window.GetWindow(this) as MainWindow;

                            if (parentWindow != null)
                            {
                                //access property of the MainWindow class that exposes the access rights...
                                if (AppSettings.lang.Equals("en"))
                                {
                                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                                    parentWindow.grid_mainWindow.FlowDirection = FlowDirection.LeftToRight;

                                }
                                else
                                {
                                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                                    parentWindow.grid_mainWindow.FlowDirection = FlowDirection.RightToLeft;
                                }

                                parentWindow.translate();

                                MainWindow.loadingDefaultPath("settings", "general");

                                #region translate
                                if (AppSettings.lang.Equals("en"))
                                {
                                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                                }
                                else
                                {
                                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                                }

                                translate();
                                #endregion
                            }
                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_saveTax_Click(object sender, RoutedEventArgs e)
        {//save Tax
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_taxSetting w = new wd_taxSetting();
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_saveInvoiceItemsDetails_Click(object sender, RoutedEventArgs e)
        {//save InvoiceItemsDetails
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_invoiceItemsDetails w = new wd_invoiceItemsDetails();
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_saveReturnPeriod_Click(object sender, RoutedEventArgs e)
        {//save return period
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyTextBox(tb_returnPeriod, p_errorReturnPeriod, tt_errorReturnPeriod, "trIsRequired");
                    
                    if (!tb_returnPeriod.Text.Equals(""))
                    {
                        if (returnPeriod == null)
                            returnPeriod = new SetValues();

                        returnPeriod.value = tb_returnPeriod.Text;
                        returnPeriod.isSystem = 1;
                        returnPeriod.settingId = returnPeriod.settingId;

                        int s = (int)await valueModel.Save(returnPeriod);
                        if (!s.Equals(0))
                        {
                            await FillCombo.RefreshSettingsValues();
                            //update returnPeriod in main window
                            AppSettings.returnPeriod = int.Parse(returnPeriod.value);

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_saveCurrency_Click(object sender, RoutedEventArgs e)
        {//save currency
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_savedDateForm_Click(object sender, RoutedEventArgs e)
        {//save date form
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyComboBox(cb_dateForm, p_errorDateForm, tt_errorDateForm, "trEmptyDateFormat");
                    if (!cb_dateForm.Text.Equals(""))
                    {
                        if (dateForm == null)
                            dateForm = new SetValues();

                        dateForm.value = cb_dateForm.SelectedValue.ToString();
                        dateForm.isSystem = 1;
                        dateForm.settingId = dateForm.settingId;

                        int s = (int)await valueModel.Save(dateForm);
                        if (!s.Equals(0))
                        {
                            await FillCombo.RefreshSettingsValues();
                            //update dateForm in main window
                            AppSettings.dateFormat = dateForm.value;

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_saveBackupTime_Click(object sender, RoutedEventArgs e)
        {//save backup time
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //TextBox tb = (TextBox)tp_backupTime.Template.FindName("PART_TextBox", tp_backupTime);
                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    if (tb.Text.Equals(""))
                        SectionData.showTimePickerValidate(tp_backupTime, p_errorBackupTime, tt_errorBackupTime, "trEmptyBackupTime");
                    if (tp_backupTime.SelectedTime != null)
                    {
                        if (backupTime == null)
                            backupTime = new SetValues();

                        backupTime.value = SectionData.DateTimeTodbString(tp_backupTime.SelectedTime);
                        backupTime.isSystem = 1;
                        backupTime.settingId = backupTime.settingId;

                        int s = (int)await valueModel.Save(backupTime);
                        if (!s.Equals(0))
                        {
                            await FillCombo.RefreshSettingsValues();
                            //update backup time in main window
                            AppSettings.backupTime = backupTime.value;
                            
                            tb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                            p_errorBackupTime.Visibility = Visibility.Collapsed;
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_userPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.groupObject.HasPermissionAction(usersSettingsPermission, MainWindow.groupObjects, "one") ||
                    MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_userPath w = new wd_userPath();
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_saveBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);
                    if (cb_backup.SelectedValue.ToString() == "backup")
                    {
                        BackupCls back = new BackupCls();
                        saveFileDialog.Filter = "INC|*.inc;";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            string message = await back.GetFile(filepath);
                            if (message == "1")
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBackupDoneSuccessfuly"), animation: ToasterAnimation.FadeIn);
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBackupNotComplete"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                    else if(MainWindow.userID==1 && MainWindow.userLogin.username== "Support@Increase")
                    {
                        // restore
                        string filepath = "";
                        openFileDialog.Filter = "INC|*.inc; ";
                        BackupCls back = new BackupCls();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trContinueProcess?");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                // here start restore if user click yes button Mr. Yasin //////////////////////////////////////////////////////
                                filepath = openFileDialog.FileName;
                                string message = await back.uploadFile(filepath);
                                if (message == "1")
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trRestoreDoneSuccessfuly"), animation: ToasterAnimation.FadeIn);
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trRestoreNotComplete"), animation: ToasterAnimation.FadeIn);


                            }
                            else
                            {
                                // here if user click no button

                            }
                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_book_Click(object sender, RoutedEventArgs e)
        {//book
            grid_main.Children.Clear();
            grid_main.Children.Add(uc_packageBookSetting.Instance);
            Button button = sender as Button;
        }
        private async void Btn_saveActivationSite_Click(object sender, RoutedEventArgs e)
        {//activation Site
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyTextBox(tb_activationSite, p_errorActivationSite, tt_errorActivationSite, "trEmptyActivationSite");
                    if (!tb_activationSite.Text.Equals(""))
                    {

                        activationSite = await ac.getactivesite();

                        // save
                        activationSite.value = @tb_activationSite.Text;
                        int res = (int)await valueModel.Save(activationSite);

                        if (!res.Equals(0))
                        {
                            AppSettings.activationSite = activationSite.value;
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);

                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Btn_savesSrverStatus_Click(object sender, RoutedEventArgs e)
        {//server status
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyComboBox(cb_serverStatus, p_errorServerStatus, tt_errorServerStatus, "trEmptyServerStatus");
                    if (!cb_serverStatus.Text.Equals(""))
                    {
                        if (progDetails == null)
                            progDetails = new ProgramDetails();

                        bool isOnline = bool.Parse(cb_serverStatus.SelectedValue.ToString());
                        int res = (int)await progDetailsModel.updateIsonline(isOnline);

                        if (res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                            MainWindow.loadingDefaultPath("settings", "general");
                            AppSettings.progDetails = progDetails;
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Btn_saveItemsCost_Click(object sender, RoutedEventArgs e)
        {//save purchase cost
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyTextBox(tb_itemsCost, p_errorItemsCost, tt_errorItemsCost, "trEmptyItemCost");
                    if (!tb_itemsCost.Text.Equals(""))
                    {
                        if (int.Parse(tb_itemsCost.Text) >= 1)
                        {
                            if (itemCost == null)
                                itemCost = new SetValues();
                            itemCost.value = tb_itemsCost.Text;
                            itemCost.isSystem = 1;
                            itemCost.isDefault = 1;
                            itemCost.settingId = itemCost.settingId;

                            int s = (int)await valueModel.Save(itemCost);
                            if (!s.Equals(0))
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            SectionData.SetError(tb_itemsCost, p_errorItemsCost, tt_errorItemsCost, "itMustBeGreaterThanZero");
                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Btn_saveErrorsExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    saveFileDialog.Filter = "File|*.er;";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string DestPath = saveFileDialog.FileName;
                        ReportCls rc = new ReportCls();

                        List<ReportParameter> paramarr = new List<ReportParameter>();

                        string addpath;
                        bool isArabic = ReportCls.checkLang();
                        string pdfpath = "";
                        pdfpath = @"\Thumb\report\temp1.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        addpath = @"\Reports\image\error.rdlc";
                        string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                        List<ErrorClass> eList = new List<ErrorClass>();
                        ErrorClass errorModel = new ErrorClass();
                        eList = await errorModel.Get();

                        clsReports.ErrorsReport(eList, rep, reppath);
                        //  clsReports.setReportLanguage(paramarr);
                        clsReports.HeaderNoLogo(paramarr);

                        rep.SetParameters(paramarr);

                        rep.Refresh();
                        bool res = false;

                        LocalReportExtensions.ExportToExcel(rep, pdfpath);
                        res = rc.encodefile(pdfpath, DestPath);
                        rc.DelFile(pdfpath);
                        //  rc.decodefile(DestPath,@"D:\error.xls");
                        if (res)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }

                        //saveFileDialog.Filter = "File|*.er;";
                        //if (saveFileDialog.ShowDialog() == true)
                        //{
                        //    string filepath = saveFileDialog.FileName;

                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_saveAccuracy_Click(object sender, RoutedEventArgs e)
        {//save accuracy
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyComboBox(cb_accuracy, p_errorAccuracy, tt_errorAccuracy, "trEmptyAccuracy");
                    if (!cb_accuracy.Text.Equals(""))
                    {
                        if (accuracy == null)
                            accuracy = new SetValues();
                        accuracy.value = cb_accuracy.SelectedValue.ToString();
                        accuracy.isSystem = 1;
                        accuracy.settingId = accuracy.settingId;
                        //  string s = await valueModel.Save(accuracy);
                        int s = (int)await valueModel.Save(accuracy);
                        if (!s.Equals(0))
                        {
                            //update accuracy in main window
                            AppSettings.accuracy = accuracy.value;

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                wd_setupServer w = new wd_setupServer();
                w.ShowDialog();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_changePassword_Click(object sender, RoutedEventArgs e)
        {//change password
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_adminChangePassword w = new wd_adminChangePassword();
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;

                    //update user in main window
                    //user = await userModel.getUserById(w.userID);
                    //MainWindow.userLogin = user;

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tgl_backupTime_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                tp_backupTime.IsEnabled = true;
                btn_saveBackupTime.IsEnabled = true;

                if (!isFromLoading)
                {
                    if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                    {

                        if (sender != null)
                            SectionData.StartAwait(grid_main);

                        if (backupDailyEnabled == null)
                            backupDailyEnabled = new SetValues();

                        backupDailyEnabled.value = "1";
                        backupDailyEnabled.isSystem = 1;
                        backupDailyEnabled.isDefault = 1;
                        backupDailyEnabled.settingId = backupDailyEnabled.settingId;

                        int s = (int)await valueModel.Save(backupDailyEnabled);
                        if (!s.Equals(0))
                        {
                            await FillCombo.RefreshSettingsValues();
                            //update backup daily enabled in main window
                            AppSettings.backupDailyEnabled = backupDailyEnabled.value;

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        if (sender != null)
                            SectionData.EndAwait(grid_main);

                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                }

            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tgl_backupTime_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                tp_backupTime.IsEnabled = false;
                btn_saveBackupTime.IsEnabled = false;

                if (!isFromLoading)
                {
                    if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                    {

                        if (sender != null)
                            SectionData.StartAwait(grid_main);

                        if (backupDailyEnabled == null)
                            backupDailyEnabled = new SetValues();

                        backupDailyEnabled.value = "0";
                        backupDailyEnabled.isSystem = 1;
                        backupDailyEnabled.isDefault = 1;
                        backupDailyEnabled.settingId = backupDailyEnabled.settingId;

                        int s = (int)await valueModel.Save(backupDailyEnabled);
                        if (!s.Equals(0))
                        {
                            await FillCombo.RefreshSettingsValues();
                            //update backup daily enabled in main window
                            AppSettings.backupDailyEnabled = backupDailyEnabled.value;

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        if (sender != null)
                            SectionData.EndAwait(grid_main);
                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                }

            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_defaultInvoiceSlice_Click(object sender, RoutedEventArgs e)
        {//save default slice
            try
            {
                if (MainWindow.groupObject.HasPermissionAction(usersSettingsPermission, MainWindow.groupObjects, "one") ||
                    MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    SectionData.validateEmptyComboBox(cb_defaultInvoiceSlice, p_errorDefaultInvoiceSlice, tt_errorDefaultInvoiceSlice, "trIsRequired");
                   
                    if (cb_defaultInvoiceSlice.SelectedIndex != -1)
                    {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);

                    if (slice == null)
                            slice = new UserSetValues();

                        slice.userId = MainWindow.userLogin.userId;
                        slice.Value = Convert.ToInt32(cb_defaultInvoiceSlice.SelectedValue).ToString();
                        slice.createUserId = MainWindow.userID;
                        slice.note = "invoice_slice";
                        slice.settingId = 47;

                        int s = (int)await usValueModel.Save(slice);
                        if (!s.Equals(0))
                        {
                            //update slice in main window
                            AppSettings.invoiceSlice = int.Parse(slice.Value);
                            AppSettings.DefaultInvoiceSlice = AppSettings.invoiceSlice;
                               
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                    if (sender != null)
                        SectionData.EndAwait(grid_main);
                }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_freeDelivery_Click(object sender, RoutedEventArgs e)
        {//save free delivery
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(companySettingsPermission, MainWindow.groupObjects, "one"))
                {
                    if (freeDelivery == null)
                        freeDelivery = new SetValues();

                    freeDelivery.value = tgl_freeDelivery.IsChecked.Value.ToString();
                    freeDelivery.isSystem = 1;
                    freeDelivery.settingId = freeDelivery.settingId;

                    int s = (int)await valueModel.Save(freeDelivery);
                    if (!s.Equals(0))
                    {
                        await FillCombo.RefreshSettingsValues();
                        //update freeDelivery in main window
                        AppSettings.freeDelivery = bool.Parse(freeDelivery.value);

                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region methods
        #region fill
        private void fillAccuracy()
        {
            var list = new[] {
                new { Text = "0"       , Value = "0" },
                new { Text = "0.0"     , Value = "1" },
                new { Text = "0.00"    , Value = "2" },
                new { Text = "0.000"   , Value = "3" },
                 };
            cb_accuracy.DisplayMemberPath = "Text";
            cb_accuracy.SelectedValuePath = "Value";
            cb_accuracy.ItemsSource = list;
        }
        private void fillBackup()
        {
            cb_backup.DisplayMemberPath = "Text";
            cb_backup.SelectedValuePath = "Value";
            var typelist = new[] {
                 new { Text = MainWindow.resourcemanager.GetString("trBackup")       , Value = "backup" },
                 new { Text = MainWindow.resourcemanager.GetString("trRestore")       , Value = "restore" },
                };
            cb_backup.ItemsSource = typelist;
            cb_backup.SelectedIndex = 0;

            if (SectionData.isSupportPermision())
            {
                grid_comboBackup.Visibility = Visibility.Visible;
                grid_textBackup.Visibility = Visibility.Collapsed;
            }
            else
            {
                grid_comboBackup.Visibility = Visibility.Collapsed;
                grid_textBackup.Visibility = Visibility.Visible;
            }
        }
        private void fillTypeOnline()
        {
            cb_serverStatus.DisplayMemberPath = "Text";
            cb_serverStatus.SelectedValuePath = "Value";
            var typelist = new[] {
                 new { Text = MainWindow.resourcemanager.GetString("trOnlineType")       , Value = "True" },
                 new { Text = MainWindow.resourcemanager.GetString("trOfflineType")       , Value = "False" },
                };
            cb_serverStatus.ItemsSource = typelist;
        }
        //private async Task fillCurrencies()
        //{
        //    cb_currency.ItemsSource = FillCombo.regionsList;
        //    cb_currency.DisplayMemberPath = "currency";
        //    cb_currency.SelectedValuePath = "countryId";
        //}
        private async Task fillLanguages()
        {
            if (FillCombo.settingsCls.Count() == 0)
                await FillCombo.RefreshSettings();

            set = FillCombo.settingsCls.Where(l => l.name == "language").FirstOrDefault<SettingCls>();
            
            if (FillCombo.settingsValues.Count() == 0)
                await FillCombo.RefreshSettingsValues();
            languages = FillCombo.settingsValues.Where(vl => vl.settingId == set.settingId).ToList<SetValues>();
            foreach (var v in languages)
            {
                if (v.value.ToString().Equals("en")) v.value = MainWindow.resourcemanager.GetString("trEnglish");
                else if (v.value.ToString().Equals("ar")) v.value = MainWindow.resourcemanager.GetString("trArabic");
            }

            cb_language.ItemsSource = languages;
            cb_language.DisplayMemberPath = "value";
            cb_language.SelectedValuePath = "valId";

        }
        private async Task fillRegions()
        {
            if (FillCombo.regionsList.Count == 0)
                await FillCombo.RefreshRegions();
            cb_region.ItemsSource = FillCombo.regionsList;
            cb_region.DisplayMemberPath = "name";
            cb_region.SelectedValuePath = "countryId";
        }
        //private async Task fillSlices()
        //{
        //    if (FillCombo.slicesList is null)
        //        await FillCombo.RefreshSlices();

        //    cb_defaultInvoiceSlice.ItemsSource = FillCombo.slicesList;
        //    cb_defaultInvoiceSlice.DisplayMemberPath = "name";
        //    cb_defaultInvoiceSlice.SelectedValuePath = "sliceId";
        //}
        #endregion
        private void translate()
        {
            txt_comInfo.Text = MainWindow.resourcemanager.GetString("trComInfo");
            txt_comHint.Text = MainWindow.resourcemanager.GetString("trSettingHint");
            txt_region.Text = MainWindow.resourcemanager.GetString("trRegion");
            txt_language.Text = MainWindow.resourcemanager.GetString("trLanguage");
            txt_currencyTitle.Text = MainWindow.resourcemanager.GetString("trCurrency") + ":";
            txt_tax.Text = MainWindow.resourcemanager.GetString("trTax");
            txt_taxHint.Text = MainWindow.resourcemanager.GetString("trInvoice") + ", " + MainWindow.resourcemanager.GetString("trItem") + "...";
            txt_itemsCost.Text = MainWindow.resourcemanager.GetString("trItemCost");
            txt_dateForm.Text = MainWindow.resourcemanager.GetString("trDateForm");
            txt_accuracy.Text = MainWindow.resourcemanager.GetString("trAccuracy");
            txt_adminChangePassword.Text = MainWindow.resourcemanager.GetString("trChangePassword");
            txt_adminChangePasswordHint.Text = MainWindow.resourcemanager.GetString("trChangePasswordHint");
            txt_userPath.Text = MainWindow.resourcemanager.GetString("trUserPath");
            txt_userPathHint.Text = MainWindow.resourcemanager.GetString("trUserPath") + "...";
            txt_errorsExport.Text = MainWindow.resourcemanager.GetString("trErrorsFile");
            txt_errorsExportHint.Text = MainWindow.resourcemanager.GetString("trErrorFileDownload") + "...";
            txt_itemsCost.Text = MainWindow.resourcemanager.GetString("trPurchaseCost");
            brd_itemsCost.ToolTip = MainWindow.resourcemanager.GetString("trItemCostHint");
            txt_returnPeriod.Text = MainWindow.resourcemanager.GetString("returnPeriod");
            txt_day.Text = MainWindow.resourcemanager.GetString("trDay");
            if (SectionData.isSupportPermision())
                txt_backup.Text = MainWindow.resourcemanager.GetString("trBackUp/Restore");
            else
                txt_backup.Text = MainWindow.resourcemanager.GetString("trBackup");
            txt_backupHint.Text = MainWindow.resourcemanager.GetString("backupData") + "...";
            txt_activationSite.Text = MainWindow.resourcemanager.GetString("trActivationSite");
            txt_serverStatus.Text = MainWindow.resourcemanager.GetString("trServerType");
            txt_defaultInvoiceSlice.Text = MainWindow.resourcemanager.GetString("priceClass");
            txt_defaultInvoiceSliceHint.Text = MainWindow.resourcemanager.GetString("defaultClass")+"...";
            txt_backupTime.Text = MainWindow.resourcemanager.GetString("backupTime");
            txt_freeDelivery.Text = MainWindow.resourcemanager.GetString("freeDelivery");
            txt_invoiceItemsDetails.Text = MainWindow.resourcemanager.GetString("skipFeatures");
            txt_invoiceItemsDetailsHint.Text = MainWindow.resourcemanager.GetString("trSerials") + ", " + MainWindow.resourcemanager.GetString("trProperties") + "...";

            tt_region.Content = MainWindow.resourcemanager.GetString("trRegion");
            tt_language.Content = MainWindow.resourcemanager.GetString("trLanguage");
            //tt_currency.Content = MainWindow.resourcemanager.GetString("trCurrency");
            tt_dateForm.Content = MainWindow.resourcemanager.GetString("trDateForm");
            tt_accuracy.Content = MainWindow.resourcemanager.GetString("trAccuracy");
            tt_activationSite.Content = MainWindow.resourcemanager.GetString("trActivationSite");
           
            //TextBox tbBackupTime = (TextBox)tp_backupTime.Template.FindName("PART_TextBox", tp_backupTime);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb, MainWindow.resourcemanager.GetString("trTime"));
            
        }
      
        #endregion

        #region events
        private void Cb_region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                region = cb_region.SelectedItem as CountryCode;
                if (region != null)
                    txt_currencyValue.Text = region.currency;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tp_backupTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (tb.Text.Equals(""))
                    SectionData.showTimePickerValidate(tp_backupTime, p_errorBackupTime, tt_errorBackupTime, "trEmptyBackupTime");
                else
                {
                    tb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    p_errorBackupTime.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_PreventSpaces(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_count_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        { //decimal
            try
            {
                var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
                if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                    e.Handled = false;
                else
                    e.Handled = true;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //TextBox tb = (TextBox)tp_backupTime.Template.FindName("PART_TextBox", tp_backupTime);
                
                if (tb.Text.Equals(""))
                    SectionData.showTimePickerValidate(tp_backupTime, p_errorBackupTime, tt_errorBackupTime, "trEmptyBackupTime");
                else
                {
                    tb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    p_errorBackupTime.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void validateEmpty(string name, object sender)
        {//validate
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_activationSite")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorActivationSite, tt_errorActivationSite, "trEmptyActivationSite");
                    if ((sender as TextBox).Name == "tb_itemsCost")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorItemsCost, tt_errorItemsCost, "trEmptyItemCost");

                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_region")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorRegion, tt_errorRegion, "trEmptyRegion");
                    if ((sender as ComboBox).Name == "cb_language")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorLanguage, tt_errorLanguage, "trEmptyLanguage");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                SectionData.InputJustNumber(ref textBox);
                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var txb = sender as TextBox;
                SectionData.InputJustNumber(ref txb);
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion



    }
}
