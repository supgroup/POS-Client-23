using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using static POS.MainWindow;

namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_home.xaml
    /// </summary>
    public partial class uc_home : UserControl
    {
        int x;
        int y;
        int z;
        int value_items;
        int value_purchase;
        int value_sales;
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer threadtimer10 = new DispatcherTimer();
        DispatcherTimer threadtimer30 = new DispatcherTimer();
        private static uc_home _instance;
        public static uc_home Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_home();
                return _instance;
            }
        }
        public uc_home()
        {
            try
            {
                InitializeComponent();
                //timerAnimation();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        int NumberDaysInMonth { get; set; }
        public static int secondTimer10 = 10;
        public static int MinuteTimer = 60;
        int SkipBestSeller = 0;
        int SkipIUStorage = 0;
        int SkipCardBalance = 0;
        bool firstLoad = true;
        string branchesPermission = "dashboard_branches";
        Dash dash = new Dash();
        User user = new User();
         ImageBrush brush = new ImageBrush();

        public List<BestSeller> listBestSeller = new List<BestSeller>();
        public List<CardsSts> listCardBalance = new List<CardsSts>();
        public List<TotalPurSale> listMonthlyInvoice = new List<TotalPurSale>();
        public List<IUStorage> listIUStorage = new List<IUStorage>();
        public List<ItemUnit> IUList = new List<ItemUnit>();
        bool firstLoading = true;
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                {
                    threadtimer10.Start();
                    threadtimer10.IsEnabled = true;
                }
                {
                    threadtimer30.Start();
                    threadtimer30.IsEnabled = true;
                }


                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft; }
                translate();
                #endregion
                refrishIUList(MainWindow.itemUnitsUsers);

               

                SkipBestSeller = 0;
                SkipIUStorage = 0;
                firstLoad = true;



                CalculateNumberDaysInMonth calculate = new CalculateNumberDaysInMonth();
                NumberDaysInMonth = calculate.getdays(DateTime.Now);

 
                if (firstLoading)
                {
                    await SectionData.fillBranchesWithAll(cb_branch,  "bs");
                     cb_branch.SelectedValue = MainWindow.branchID;
                }
                if (MainWindow.groupObject.HasPermissionAction(branchesPermission, MainWindow.groupObjects, "one"))
                    cb_branch.IsEnabled = true;
                else cb_branch.IsEnabled = false;

                try
                {
                    tb_version.Text = AppSettings.CurrentVersion;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                txt_rightReserved.Text =DateTime.Now.Date.Year + " © All Right Reserved for SupClouds";

                if (firstLoading)
                {
                    starTimerAfter10();
                    starTimerAfterMinute();
                }
                firstLoading = false;
                await refreshViewTask();

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
        async void starTimerAfter10()
        {
            await Task.Delay(10000);
            if (threadtimer10.IsEnabled)
            {
            // thread 10
            //threadtimer = new DispatcherTimer();
            threadtimer10.Interval = TimeSpan.FromSeconds(secondTimer10);
            threadtimer10.Tick += timer_Thread10;
            threadtimer10.Start();
            ////////////////////
            }
        }
        async void starTimerAfterMinute()
        {
            await Task.Delay(60000);
            if (threadtimer30.IsEnabled)
            {
                //thread 30
                //threadtimer = new DispatcherTimer();
            threadtimer30.Interval = TimeSpan.FromSeconds(MinuteTimer);
            threadtimer30.Tick += timer_ThreadMinute;
            threadtimer30.Start();
            ////////////////////
            }
        }
        private void translate()
        {
            txt_countAllPurchase.Text = MainWindow.resourcemanager.GetString("trPurchases");
            txt_countAllSales.Text = MainWindow.resourcemanager.GetString("trSales");
            txt_countAllVendor.Text = MainWindow.resourcemanager.GetString("trSuppliers");
            txt_countAllCustomer.Text = MainWindow.resourcemanager.GetString("trCustomers");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trStore/BranchHint"));
            bestSellerTitle.Text = MainWindow.resourcemanager.GetString("trBestSeller");
            storageTitle.Text = MainWindow.resourcemanager.GetString("trStorage");
            txt_userOnlineTitle.Text = MainWindow.resourcemanager.GetString("trUsers");
            purchaseAndSalesTitle.Text = MainWindow.resourcemanager.GetString("trPurchase&Sales");
            txt_branchOnlineTitle.Text = MainWindow.resourcemanager.GetString("trBranch&Store");
            txt_dailyPurchaseTitle.Text = MainWindow.resourcemanager.GetString("trDailyPurchase");
            txt_dailySalesTitle.Text = MainWindow.resourcemanager.GetString("trDailySales");

            txt_userOnline.Text = txt_branchOnline.Text = txt_posOnline.Text = MainWindow.resourcemanager.GetString("trOnline") + ":";
            txt_countDailyPurchase.Text = txt_countDailySales.Text = MainWindow.resourcemanager.GetString("trCount") + ":";

            tb_versionTitle.Text = resourcemanager.GetString("Version") + ": ";
            txt_moneyIcon.Text = AppSettings.Currency;

            txt_posOnlineTitle.Text = resourcemanager.GetString("trPOSs");
            txt_cashBalance.Text = resourcemanager.GetString("trTotal");
            txt_cashBalanceTitle.Text = resourcemanager.GetString("trCashBalance");
            txt_cardBalanceTitle.Text = resourcemanager.GetString("trPaymentMethods");
            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        void timer_ThreadMinute(object sendert, EventArgs et)
        {
            try
            {
                refreshView();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void timer_Thread10(object sendert, EventArgs et)
        {
            try
            {

                #region BestSeller
                if (!firstLoad)
                {
                    if ((listBestSeller.Count < 4 || SkipBestSeller == 2) || (listBestSeller.Count == 3)
                     || (listBestSeller.Count >= 4 && listBestSeller.Count <= 6 && SkipBestSeller == 1)
                     || (listBestSeller.Count >= 7 && listBestSeller.Count <= 9 && SkipBestSeller == 2))
                        SkipBestSeller = 0;
                    else if (listBestSeller.Count < 7 && SkipBestSeller < 1)
                        SkipBestSeller++;
                    else if (listBestSeller.Count < 10 && SkipBestSeller < 2)
                        SkipBestSeller++;
                }
                paginationBestSeller(listBestSeller, SkipBestSeller);
                #endregion
                #region IUStorage
                if (!firstLoad)
                {
                    if ((listIUStorage.Count < 4 || SkipIUStorage == 2) || (listIUStorage.Count == 3)
                      || (listIUStorage.Count >= 4 && listIUStorage.Count <= 6 && SkipIUStorage == 1)
                      || (listIUStorage.Count >= 7 && listIUStorage.Count <= 9 && SkipIUStorage == 2))
                        SkipIUStorage = 0;
                    else if (listIUStorage.Count < 7 && SkipIUStorage < 1)
                        SkipIUStorage++;
                    else if (listIUStorage.Count < 10 && SkipIUStorage < 2)
                        SkipIUStorage++;
                }
                paginationIUStorage(listIUStorage, SkipIUStorage);
                #endregion
                #region cardBalance
                if (!firstLoad)
                {
                    if ((listCardBalance.Count < 4 || SkipCardBalance == 2) || (listCardBalance.Count == 3)
                     || (listCardBalance.Count >= 4 && listCardBalance.Count <= 6 && SkipCardBalance == 1)
                     || (listCardBalance.Count >= 7 && listCardBalance.Count <= 9 && SkipCardBalance == 2))
                        SkipCardBalance = 0;
                    else if (listCardBalance.Count < 7 && SkipCardBalance < 1)
                        SkipCardBalance++;
                    else if (listCardBalance.Count < 10 && SkipCardBalance < 2)
                        SkipCardBalance++;
                }
                paginationCardBalance(listCardBalance, SkipCardBalance);
                #endregion
                firstLoad = false;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        List<keyValueBool> loadingList;
       
        async Task refreshViewTask()
        {
            try
            {
                await getDashInfo();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        void refreshView()
        {
            try
            {
                getDashInfo();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        async Task getDashInfo()
        {
            dash = await dash.GetDashInfo(MainWindow.loginBranch.branchId, MainWindow.userLogin.userId,IUList);

            AllSalPur();
            CountMonthlySalPur();
            DailySalPur();
            AgentCount();
            UserOnline();
            BranchOnline();
            PosOnline();
            branchBalance();
            #region BestSeller
            BestSeller();
            paginationBestSeller(listBestSeller, SkipBestSeller);
            #endregion

            #region IUStorage
            IUStorage();
            paginationIUStorage(listIUStorage, SkipIUStorage);
            #endregion
            #region cardBalance
            CardBalance();
            paginationCardBalance(listCardBalance, SkipCardBalance);
            #endregion
            AmountMonthlySalPur();


            UserOnlinePic();


            this.DataContext = new Dash();
            this.DataContext = dash;
        }
        void AllSalPur()
        {
            try
            {
                List<InvoiceCount> listSalPur = dash.ListSalPur;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        dash.countAllPurchase = listSalPur.Sum(x => x.purCount).ToString();
                        dash.countAllSalesValue = listSalPur.Sum(x => x.saleCount).ToString();
                    }
                    else
                    {
                        var newSalPur = listSalPur.Where(s => s.branchCreatorId == (int)cb_branch.SelectedValue).FirstOrDefault();
                        if (newSalPur != null)
                        {
                            dash.countAllPurchase = newSalPur.purCount.ToString();
                            dash.countAllSalesValue = newSalPur.saleCount.ToString();
                        }
                        else
                            dash.countAllPurchase = dash.countAllSalesValue = "0";
                    }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
          
        }
        void CountMonthlySalPur()
        {
            try
            {
                List<TotalPurSale> listSalPur = dash.MonthlySalPur;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        dash.countMonthlyPurchase = listSalPur.Sum(x => x.countPur).ToString();
                        dash.countMonthlySales = listSalPur.Sum(x => x.countSale).ToString();
                    }
                    else
                    {
                        var newSalPur = listSalPur.Where(s => s.branchCreatorId == (int)cb_branch.SelectedValue);
                        if (newSalPur != null)
                        {
                            dash.countMonthlyPurchase = newSalPur.Sum(x => x.countPur).ToString();
                            dash.countMonthlySales = newSalPur.Sum(x => x.countSale).ToString();
                        }
                        else
                            dash.countMonthlyPurchase = dash.countMonthlySales = "0";
                    }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void DailySalPur()
        {
            try
            {
                List<InvoiceCount> listSalPur = dash.DailySalPur;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        dash.countDailyPurchase = listSalPur.Sum(x => x.purCount).ToString();
                        dash.countDailySales = listSalPur.Sum(x => x.saleCount).ToString();
                    }
                    else
                    {
                        var newSalPur = listSalPur.Where(s => s.branchCreatorId == (int)cb_branch.SelectedValue).FirstOrDefault();
                        if (newSalPur != null)
                        {
                            dash.countDailyPurchase = newSalPur.purCount.ToString();
                            dash.countDailySales = newSalPur.saleCount.ToString();
                        }
                        else
                            dash.countDailyPurchase = dash.countDailySales = "0";
                    }
                InitializePieChart(pch_dailyPurchaseInvoice,
                    int.Parse(dash.countDailyPurchase),
                    int.Parse(dash.countMonthlyPurchase));

                InitializePieChart(pch_dailySalesInvoice,
                    int.Parse(dash.countDailySales),
                    int.Parse(dash.countMonthlySales));
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        /*
       void AgentCount()
        {
            try
            {
                List<AgentsCount> listAgentCount = dash.ListAgentCount;
                var newAgentCount = dash.ListAgentCount.FirstOrDefault();
                if (newAgentCount != null)
                {
                    dash.customerCount = newAgentCount.customerCount.ToString();
                    dash.vendorCount = newAgentCount.vendorCount.ToString();
                }
                else
                {
                    dash.customerCount = dash.vendorCount = "0";
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
          
        }
        */

        void AgentCount()
        {
            try
            {
                List<AgentsCountbyBranch> listAgentCount = dash.ListAgentCount;
                

                 if (cb_branch.SelectedValue != null)
                    //if ((int)cb_branch.SelectedValue == 0)
                    //{
                    //    dash.customerCount = listAgentCount.Sum(x => x.customersCount).ToString();
                    //    dash.vendorCount = listAgentCount.Sum(x => x.vendorsCount).ToString();
                    //}
                    //else
                    {

                        var newAgentCount = dash.ListAgentCount.Where(s => s.branchId == (int)cb_branch.SelectedValue).FirstOrDefault();
                        if (newAgentCount != null)
                        {
                            dash.customerCount = newAgentCount.customersCount.ToString();
                            dash.vendorCount = newAgentCount.vendorsCount.ToString();
                        }
                        else
                            dash.customerCount = dash.vendorCount = "0";
                    }

                
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }

        }

        void UserOnline()
        {
            try
            {
                List<UserOnlineCount> listUserOnline = dash.UsersList;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        dash.userOnline = listUserOnline.Sum(x => x.userOnlineCount).ToString();
                        dash.userOffline = listUserOnline.Sum(x => x.offlineUsers).ToString();
                    }
                    else
                    {
                        var newUserOnline = listUserOnline.Where(s => s.branchId == (int)cb_branch.SelectedValue).FirstOrDefault();
                        if (newUserOnline != null)
                        {
                            dash.userOnline = newUserOnline.userOnlineCount.ToString();
                            dash.userOffline = newUserOnline.offlineUsers.ToString();
                        }
                        else
                            dash.userOnline = dash.userOffline = "0";
                    }
                InitializePieChart(pch_userOnline,
                int.Parse(dash.userOnline),
                (int.Parse(dash.userOnline) + int.Parse(dash.userOffline)));
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void BranchOnline()
        {
            try
            {
                var newBranchOnline = dash.ListBranchOnline.FirstOrDefault();
                if (newBranchOnline != null)
                {
                    dash.branchOnline = newBranchOnline.branchOnline.ToString();
                    dash.branchOffline = newBranchOnline.branchOffline.ToString();
                }
                else
                    dash.branchOnline = dash.branchOffline = "0";
                InitializePieChart(pch_branch,
                    int.Parse(dash.branchOnline),
                    (int.Parse(dash.branchOnline) + int.Parse(dash.branchOffline)));
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void PosOnline()
        {
            try
            {
                List<PosOnlineCount> listPosOnline = dash.posOnlineCount;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        dash.posOnline = listPosOnline.Sum(x => x.posOnlineCount).ToString();
                        dash.posOffline = listPosOnline.Sum(x => x.offlinePos).ToString();
                    }
                    else
                    {
                        var newPosOnline = listPosOnline.Where(s => s.branchId == (int)cb_branch.SelectedValue).FirstOrDefault();
                        if (newPosOnline != null)
                        {
                            dash.posOnline = newPosOnline.posOnlineCount.ToString();
                            dash.posOffline = newPosOnline.offlinePos.ToString();
                        }
                        else
                            dash.posOnline = dash.posOffline = "0";
                    }
                InitializePieChart(pch_posOnline,
                int.Parse(dash.posOnline),
                (int.Parse(dash.posOnline) + int.Parse(dash.posOffline)));
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
         void branchBalance()
        {
            try
            {
                List<Pos> listBranchBalance = dash.branchBalance;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        dash.cashBalance = SectionData.DecTostring(listBranchBalance.Sum(x => x.balance));
                    }
                    else
                    {
                        var newBranchBalance = listBranchBalance.Where(s => s.branchId == (int)cb_branch.SelectedValue).FirstOrDefault();
                        if (newBranchBalance != null)
                        {
                            dash.cashBalance = SectionData.DecTostring(newBranchBalance.balance);
                        }
                        else
                            dash.cashBalance =  "0";
                    }
                
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void BestSeller()
        {
            try
            {
                List<BestSeller> listAllBestSeller = dash.ListBestSeller;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        listBestSeller = listAllBestSeller.GroupBy(x => new { x.itemName, x.unitName }).Select(s => new BestSeller
                        {
                            itemName = s.FirstOrDefault().itemName,
                            unitName = s.FirstOrDefault().unitName,
                            quantity = s.Sum(g => g.quantity),
                        }).ToList();
                    }
                    else
                    {
                        listBestSeller = listAllBestSeller.Where(s => s.branchCreatorId == (int)cb_branch.SelectedValue).ToList();
                        if (listBestSeller != null)
                        {
                            listBestSeller = listBestSeller.GroupBy(x => new { x.itemName, x.unitName }).Select(s => new BestSeller
                            {
                                itemName = s.FirstOrDefault().itemName,
                                unitName = s.FirstOrDefault().unitName,
                                quantity = s.Sum(g => g.quantity),
                            }).ToList();
                        }
                        else
                            listBestSeller = new List<BestSeller>();
                    }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void CardBalance()
        {
            try
            {
                List<CardsSts> listAllCardBalance = dash.paymentsToday.Where(x => x.total != 0).ToList();
                foreach (var item in listAllCardBalance)
                {
                    if (item.name == "obox")
                        item.name =  MainWindow.resourcemanager.GetString("trOpenCash");
                    else if (item.name == "cash")
                        item.name =  MainWindow.resourcemanager.GetString("trCash");
                }
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        listCardBalance = listAllCardBalance.GroupBy(x => new { x.cardId}).Select(s => new CardsSts
                        {
                            name = s.FirstOrDefault().name,
                            total = s.Sum(g => g.total),
                        }).ToList();
                    }
                    else
                    {
                        listCardBalance = listAllCardBalance.Where(s => s.branchId == (int)cb_branch.SelectedValue).ToList();
                        if (listCardBalance != null)
                        {
                            listCardBalance = listCardBalance.GroupBy(x => new { x.cardId}).Select(s => new CardsSts
                            {
                                name = s.FirstOrDefault().name,
                                total = s.Sum(g => g.total),
                            }).ToList();
                        }
                        else
                            listCardBalance = new List<CardsSts>();
                    }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void UserOnlinePic()
        {
            try
            {
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        InitializeUserOnlinePic(dash.listUserOnline);
                    }
                    else
                    {
                        List<userOnlineInfo> newUserOnline = dash.listUserOnline.Where(s => s.branchId == (int)cb_branch.SelectedValue).ToList();
                        if (newUserOnline != null && newUserOnline.Count != 0)
                        {
                            InitializeUserOnlinePic(newUserOnline);
                        }
                        else
                            InitializeNoUserOnlinePic();
                    }
                InitializePieChart(pch_userOnline,
                int.Parse(dash.userOnline),
                (int.Parse(dash.userOnline) + int.Parse(dash.userOffline)));
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void InitializeUserOnlinePic(List<userOnlineInfo> users)
        {
            #region userImageLoad
            grid_userImages.Children.Clear();
            int userCount = 0;
            foreach (var item in users)
            {
                if (userCount > 4)
                {
                    #region Button
                    Button button = new Button();
                    button.Padding = new Thickness(0, 0, 0, 0);
                    button.Margin = new Thickness(-5, 0, -5, 0);
                    button.Background = null;
                    button.BorderBrush = null;
                    button.Height = 40;
                    button.Width = 40;
                    button.Click += userOnlineListWindow;
                    Grid.SetColumn(button, 4);
                    #region grid
                    Grid grid = new Grid();
                  
                    #region rectangle
                    Rectangle rectangle = new Rectangle();
                    rectangle.Fill = Application.Current.Resources["Orange"] as SolidColorBrush;
                    rectangle.RadiusX = 90;
                    rectangle.RadiusY = 90;
                    rectangle.Height = 40;
                    rectangle.Width = 40;
                    rectangle.StrokeThickness = 1;
                    rectangle.Stroke = Application.Current.Resources["White"] as SolidColorBrush; ;
                    grid.Children.Add(rectangle);
                    #endregion
                    #region Text
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "+" + (users.Count() - 4).ToString();
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.Foreground = Application.Current.Resources["White"] as SolidColorBrush;
                    grid.Children.Add(textBlock);
                    #endregion
                    #endregion
                    button.Content = grid;
                    #endregion


                    grid_userImages.Children.Add(button);
                    break;
                }
                else
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Margin = new Thickness(-5, 0, -5, 0);
                    ellipse.StrokeThickness = 1;
                    ellipse.Stroke = Application.Current.Resources["White"] as SolidColorBrush;
                    ellipse.Height = 40;
                    ellipse.Width = 40;
                    ellipse.FlowDirection = FlowDirection.LeftToRight;
                    ellipse.ToolTip = item.userName;
                    userImageLoad(ellipse, item.image,(DateTime)item.updateDate);
                    Grid.SetColumn(ellipse, userCount);
                    grid_userImages.Children.Add(ellipse);
                    userCount++;
                }
            }
            #endregion
        }
        void InitializeNoUserOnlinePic()
        {
            grid_userImages.Children.Clear();
            Grid grid = new Grid();
            grid.Margin = new Thickness(-5, 0, -5, 0);
            Grid.SetColumn(grid, 4);
            #region rectangle
            Rectangle rectangle = new Rectangle();
            rectangle.Fill = Application.Current.Resources["Orange"] as SolidColorBrush;
            rectangle.RadiusX = 90;
            rectangle.RadiusY = 90;
            rectangle.Height = 40;
            rectangle.Width = 40;
            rectangle.StrokeThickness = 1;
            rectangle.Stroke = Application.Current.Resources["White"] as SolidColorBrush; ;
            grid.Children.Add(rectangle);
            #endregion
            #region rectangle
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "0";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = Application.Current.Resources["White"] as SolidColorBrush;
            grid.Children.Add(textBlock);
            #endregion
            grid_userImages.Children.Add(grid);
        }
        void userOnlineListWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this).Opacity = 0.2;
                wd_usersOnline w = new wd_usersOnline();
                w.usersOnline = dash.listUserOnline;
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        #region BestSeller
     
        void paginationBestSeller(List<BestSeller> listBestSeller, int skip)
        {
            grid_bestSeller.Children.Clear();
            int order = (skip * 3) + 1;
            int row = 1;
            listBestSeller = listBestSeller.Skip(skip * 3).Take(3).ToList();
            foreach (var item in listBestSeller)
            {
                InitializeBestSellerRow(order.ToString(), item.itemName + "-" + item.unitName,
                  item.quantity.ToString(), row);
                order++;
                row += 2;
            }

            #region Border
            #region   firstBorder 
            var firstBorder = new Border();
            firstBorder.Margin = new Thickness(10, 2.5, 10, 2.5);
            firstBorder.Height = 1;
            firstBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            firstBorder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#C2F3E8"));
            Grid.SetRow(firstBorder, 2);
            Grid.SetColumnSpan(firstBorder, 7);
            #endregion
            #region   firstBorder 
            var secondBorder = new Border();
            secondBorder.Margin = new Thickness(10, 2.5, 10, 2.5);
            secondBorder.Height = 1;
            secondBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            secondBorder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#C2F3E8"));
            Grid.SetRow(secondBorder, 4);
            Grid.SetColumnSpan(secondBorder, 7);
            #endregion
            grid_bestSeller.Children.Add(firstBorder);
            grid_bestSeller.Children.Add(secondBorder);
            #endregion


        }
        
        void InitializeBestSellerRow(string No, string item, string count, int row)
        {
            #region   itemNo
            var itemNo = new TextBlock();
            itemNo.Text = "#" + No;
            itemNo.Tag = "itemTitle";
            itemNo.VerticalAlignment = VerticalAlignment.Center;
            itemNo.HorizontalAlignment = HorizontalAlignment.Center;
            itemNo.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1D75B8"));
            itemNo.FontSize = 16;
            itemNo.Margin = new Thickness(10, 1, 2.5, 1);
            Grid.SetRow(itemNo, row);
            Grid.SetColumn(itemNo, 0);
            #endregion
            #region   itemTitle
            var itemTitle = new TextBlock();
            itemTitle.Text = MainWindow.resourcemanager.GetString("trItem") + ":";
            itemTitle.Tag = "itemTitle";
            itemTitle.VerticalAlignment = VerticalAlignment.Center;
            itemTitle.HorizontalAlignment = HorizontalAlignment.Center;
            itemTitle.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            itemTitle.FontSize = 16;
            itemTitle.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(itemTitle, row);
            Grid.SetColumn(itemTitle, 1);
            #endregion
            #region   itemName
            var itemName = new TextBlock();
            itemName.Text = item;
            itemName.Tag = "itemName";
            itemName.VerticalAlignment = VerticalAlignment.Center;
            itemName.HorizontalAlignment = HorizontalAlignment.Left;
            itemName.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            itemName.FontSize = 16;
            itemName.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(itemName, row);
            Grid.SetColumn(itemName, 2);
            #endregion
            #region   countTitle
            var countTitle = new TextBlock();
            countTitle.Text = MainWindow.resourcemanager.GetString("trCount") + ":";
            countTitle.Tag = "countTitle";
            countTitle.VerticalAlignment = VerticalAlignment.Center;
            countTitle.HorizontalAlignment = HorizontalAlignment.Center;
            countTitle.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            countTitle.FontSize = 16;
            countTitle.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(countTitle, row);
            Grid.SetColumn(countTitle, 3);
            #endregion
            #region   countItem
            var countItem = new TextBlock();
            countItem.Text = count;
            countItem.Tag = "itemName";
            countItem.VerticalAlignment = VerticalAlignment.Center;
            countItem.HorizontalAlignment = HorizontalAlignment.Left;
            countItem.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1D75B8"));
            countItem.FontSize = 16;
            countItem.Margin = new Thickness(5, 1, 10, 1);
            Grid.SetRow(countItem, row);
            Grid.SetColumn(countItem, 4);
            #endregion
            grid_bestSeller.Children.Add(itemNo);
            grid_bestSeller.Children.Add(itemTitle);
            grid_bestSeller.Children.Add(itemName);
            grid_bestSeller.Children.Add(countTitle);
            grid_bestSeller.Children.Add(countItem);
        }
        void paginationCardBalance(List<CardsSts> listCardBalance, int skip)
        {
            grid_cardBalance.Children.Clear();
            int order = (skip * 3) + 1;
            int row = 1;
            listCardBalance = listCardBalance.Skip(skip * 3).Take(3).ToList();
            foreach (var item in listCardBalance)
            {
                InitializeCardBalanceRow(order.ToString(), item.name ,
                  item.total.Value, row);
                order++;
                row += 2;
            }

            #region Border
            #region   firstBorder 
            var firstBorder = new Border();
            firstBorder.Margin = new Thickness(10, 2.5, 10, 2.5);
            firstBorder.Height = 1;
            firstBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            firstBorder.Background = brd_cardBalance.Background;
            Grid.SetRow(firstBorder, 2);
            Grid.SetColumnSpan(firstBorder, 7);
            #endregion
            #region   firstBorder 
            var secondBorder = new Border();
            secondBorder.Margin = new Thickness(10, 2.5, 10, 2.5);
            secondBorder.Height = 1;
            secondBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            secondBorder.Background = brd_cardBalance.Background;
            Grid.SetRow(secondBorder, 4);
            Grid.SetColumnSpan(secondBorder, 7);
            #endregion
            grid_cardBalance.Children.Add(firstBorder);
            grid_cardBalance.Children.Add(secondBorder);
            #endregion


        }
        void InitializeCardBalanceRow(string No, string name, decimal value, int row)
        {
            #region   itemNo
            var itemNo = new TextBlock();
            itemNo.Text = "#" + No;
            itemNo.Tag = "itemTitle";
            itemNo.VerticalAlignment = VerticalAlignment.Center;
            itemNo.HorizontalAlignment = HorizontalAlignment.Center;
            itemNo.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1D75B8"));
            itemNo.FontSize = 16;
            itemNo.Margin = new Thickness(10, 1, 2.5, 1);
            Grid.SetRow(itemNo, row);
            Grid.SetColumn(itemNo, 0);
            #endregion
            #region   itemName
            var itemName = new TextBlock();
            itemName.Text = name;
            itemName.Tag = "cardName";
            itemName.VerticalAlignment = VerticalAlignment.Center;
            itemName.HorizontalAlignment = HorizontalAlignment.Left;
            itemName.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            itemName.FontSize = 16;
            itemName.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(itemName, row);
            Grid.SetColumn(itemName, 1);
            
            #endregion
            
            #region   countItem
            var valueItem = new TextBlock();
            valueItem.Text = SectionData.DecTostring(value);
            valueItem.Tag = "cardValue";
            valueItem.VerticalAlignment = VerticalAlignment.Center;
            valueItem.HorizontalAlignment = HorizontalAlignment.Left;
            valueItem.FontSize = 16;
            valueItem.Margin = new Thickness(5, 1, 5, 1);
            if(value > 0)
                valueItem.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush;
            else
                valueItem.Foreground = Application.Current.Resources["mediumRed"] as SolidColorBrush;
            Grid.SetRow(valueItem, row);
            Grid.SetColumn(valueItem, 2);
            #endregion
            #region   currencyItem
            var currencyItem = new TextBlock();
            currencyItem.Text = AppSettings.Currency;
            currencyItem.Tag = "cardValue";
            currencyItem.VerticalAlignment = VerticalAlignment.Center;
            currencyItem.HorizontalAlignment = HorizontalAlignment.Left;
            currencyItem.FontSize = 16;
            currencyItem.Margin = new Thickness(5, 1, 10, 1);
            currencyItem.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            Grid.SetRow(currencyItem, row);
            Grid.SetColumn(currencyItem, 3);
            #endregion

            grid_cardBalance.Children.Add(itemNo);
            grid_cardBalance.Children.Add(itemName);
            grid_cardBalance.Children.Add(valueItem);
            grid_cardBalance.Children.Add(currencyItem);
        }
        #endregion
        #region IUStorage
        private async void Btn_storageSetting_Click(object sender, RoutedEventArgs e)
        {//settings
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;

                wd_itemsUnitList w = new wd_itemsUnitList();
                w.itemId = 0;
                w.itemUnitId = 0;
                w.CallerName = "IUList";
                w.ShowDialog();
                if (w.isActive)
                {
                    MainWindow.itemUnitsUsers = w.selectedItemUnits;

                    refrishIUList(MainWindow.itemUnitsUsers);
                    await getDashInfo();

                }
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }

        }
        void refrishIUList(List<ItemUnitUser> itemUnitsUsers)
        {
            try
            {
                IUList.Clear();
                foreach (var item in itemUnitsUsers)
                {
                    var itemUnit = new ItemUnit();
                    itemUnit.itemUnitId = item.itemUnitId.Value;
                    itemUnit.itemId = item.itemId;
                    itemUnit.unitId = item.unitId;
                    IUList.Add(itemUnit);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void IUStorage()
        {
            try
            {
               List<IUStorage> listAllIUStorage = dash.ListIUStorage;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        listIUStorage = listAllIUStorage.GroupBy(x => new { x.itemName, x.unitName }).Select(s => new IUStorage
                        {
                            itemName = s.FirstOrDefault().itemName,
                            unitName = s.FirstOrDefault().unitName,
                            quantity = s.Sum(g => g.quantity),
                        }).ToList();
                    }
                    else
                    {
                        listIUStorage = listAllIUStorage.Where(s => s.branchId == (int)cb_branch.SelectedValue).ToList();
                        if (listIUStorage != null)
                        {
                            listIUStorage = listIUStorage.GroupBy(x => new { x.itemName, x.unitName }).Select(s => new IUStorage
                            {
                                itemName = s.FirstOrDefault().itemName,
                                unitName = s.FirstOrDefault().unitName,
                                quantity = s.Sum(g => g.quantity),
                            }).ToList();
                        }
                        else
                            listIUStorage = new List<IUStorage>();

                        if (listIUStorage.Count >9)
                        {
                            listIUStorage = listIUStorage.Take(9).ToList();
                        }
                    }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        void paginationIUStorage(List<IUStorage> listIUStorage, int skip = 0)
        {
            grid_IUStorage.Children.Clear();
            int order = (skip * 3) + 1;
            int row = 1;
            listIUStorage = listIUStorage.Skip(skip * 3).Take(3).ToList();
            foreach (var item in listIUStorage)
            {
                InitializeIUStorageRow(order.ToString(), item.itemName + "-" + item.unitName,
                  item.quantity.ToString(), row);
                order++;
                row += 2;
            }

            #region Border
            #region   firstBorder 
            var firstBorder = new Border();
            firstBorder.Margin = new Thickness(10, 2.5, 10, 2.5);
            firstBorder.Height = 1;
            firstBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            firstBorder.Background = brd_IUStorage.Background;
            Grid.SetRow(firstBorder, 2);
            Grid.SetColumnSpan(firstBorder, 7);
            #endregion
            #region   firstBorder 
            var secondBorder = new Border();
            secondBorder.Margin = new Thickness(10, 2.5, 10, 2.5);
            secondBorder.Height = 1;
            secondBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            secondBorder.Background = brd_IUStorage.Background;
            Grid.SetRow(secondBorder, 4);
            Grid.SetColumnSpan(secondBorder, 7);
            #endregion
            grid_IUStorage.Children.Add(firstBorder);
            grid_IUStorage.Children.Add(secondBorder);
            #endregion


        }
        void InitializeIUStorageRow(string No, string item, string count, int row)
        {
            #region   itemNo
            var itemNo = new TextBlock();
            itemNo.Text = "#" + No;
            itemNo.Tag = "itemTitle";
            itemNo.VerticalAlignment = VerticalAlignment.Center;
            itemNo.HorizontalAlignment = HorizontalAlignment.Center;
            itemNo.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1D75B8"));
            itemNo.FontSize = 16;
            itemNo.Margin = new Thickness(10, 1, 2.5, 1);
            Grid.SetRow(itemNo, row);
            Grid.SetColumn(itemNo, 0);
            #endregion
            #region   itemTitle
            var itemTitle = new TextBlock();
            itemTitle.Text = MainWindow.resourcemanager.GetString("trItem") +":";
            itemTitle.Tag = "itemTitle";
            itemTitle.VerticalAlignment = VerticalAlignment.Center;
            itemTitle.HorizontalAlignment = HorizontalAlignment.Center;
            itemTitle.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            itemTitle.FontSize = 16;
            itemTitle.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(itemTitle, row);
            Grid.SetColumn(itemTitle, 1);
            #endregion
            #region   itemName
            var itemName = new TextBlock();
            itemName.Text = item;
            itemName.Tag = "itemName";
            itemName.VerticalAlignment = VerticalAlignment.Center;
            itemName.HorizontalAlignment = HorizontalAlignment.Left;
            itemName.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            itemName.FontSize = 16;
            itemName.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(itemName, row);
            Grid.SetColumn(itemName, 2);
            #endregion
            #region   countTitle
            var countTitle = new TextBlock();
            countTitle.Text = MainWindow.resourcemanager.GetString("trCount") + ":";  
            countTitle.Tag = "countTitle";
            countTitle.VerticalAlignment = VerticalAlignment.Center;
            countTitle.HorizontalAlignment = HorizontalAlignment.Center;
            countTitle.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            countTitle.FontSize = 16;
            countTitle.Margin = new Thickness(5, 1, 5, 1);
            Grid.SetRow(countTitle, row);
            Grid.SetColumn(countTitle, 3);
            #endregion
            #region   countItem
            var countItem = new TextBlock();
            countItem.Text = count;
            countItem.Tag = "itemName";
            countItem.VerticalAlignment = VerticalAlignment.Center;
            countItem.HorizontalAlignment = HorizontalAlignment.Left;
            countItem.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1D75B8"));
            countItem.FontSize = 16;
            countItem.Margin = new Thickness(5, 1, 10, 1);
            Grid.SetRow(countItem, row);
            Grid.SetColumn(countItem, 4);
            #endregion
            grid_IUStorage.Children.Add(itemNo);
            grid_IUStorage.Children.Add(itemTitle);
            grid_IUStorage.Children.Add(itemName);
            grid_IUStorage.Children.Add(countTitle);
            grid_IUStorage.Children.Add(countItem);
        }
        #endregion

        void AmountMonthlySalPur()
        {
            try
            {
                double[] ArrayS = new double[NumberDaysInMonth];
                double[] ArrayP = new double[NumberDaysInMonth];
                string[] ArrayCount = new string[NumberDaysInMonth];
               List<TotalPurSale> listAllBestSeller = dash.ListAllBestSeller;
                if (cb_branch.SelectedValue != null)
                    if ((int)cb_branch.SelectedValue == 0)
                    {
                        listMonthlyInvoice = listAllBestSeller.GroupBy(x => new { x.day }).Select(s => new TotalPurSale
                        {
                            branchCreatorId = s.FirstOrDefault().branchCreatorId,
                            branchCreatorName = s.FirstOrDefault().branchCreatorName,
                            day = s.FirstOrDefault().day,
                            totalPur = s.Sum(g => g.totalPur),
                            totalSale = s.Sum(g => g.totalSale),
                            countPur = s.Sum(g => g.countPur),
                            countSale = s.Sum(g => g.countSale)
                        }).ToList();
                    }
                    else
                    {
                        listMonthlyInvoice = listAllBestSeller.Where(s => s.branchCreatorId == (int)cb_branch.SelectedValue).ToList();
                        if (listMonthlyInvoice != null)
                        {
                            listMonthlyInvoice = listMonthlyInvoice.GroupBy(x => new { x.day }).Select(s => new TotalPurSale
                            {
                                branchCreatorId = s.FirstOrDefault().branchCreatorId,
                                branchCreatorName = s.FirstOrDefault().branchCreatorName,
                                day = s.FirstOrDefault().day,
                                totalPur = s.Sum(g => g.totalPur),
                                totalSale = s.Sum(g => g.totalSale),
                                countPur = s.Sum(g => g.countPur),
                                countSale = s.Sum(g => g.countSale)
                            }).ToList();
                        }
                        else
                            listMonthlyInvoice = new List<TotalPurSale>();
                    }

                int count = 0;
                foreach (var item in listMonthlyInvoice)
                {
                    ArrayS[count] = double.Parse(item.totalSale.ToString()); 
                    ArrayP[count] = double.Parse(item.totalPur.ToString());
                    ArrayCount[count] = (count+1).ToString();
                    count++;
                }
                dash.SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = MainWindow.resourcemanager.GetString("trSales") ,
                    Values = ArrayS.AsChartValues()
                },
                 new LineSeries
                {
                    Title = MainWindow.resourcemanager.GetString("trPurchases"),
                    Values = ArrayP.AsChartValues()
                }
            };

                axs_AxisY.Title = MainWindow.resourcemanager.GetString("trTotal");
                axs_AxisX.Title = MainWindow.resourcemanager.GetString("trDays") ;
                dash.Labels = ArrayCount;
                dash.YFormatter = value => value.ToString("C");
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
          
        }
        void InitializePieChart(PieChart pieChart, int partial = 0, int all = 0)
        {
            SeriesCollection series = new SeriesCollection();
            series.Add(
                 new PieSeries
                 {
                     Values = new ChartValues<int> { partial },
                     Title = "",
                     DataLabels = false,
                     Fill = Application.Current.Resources["mediumGreen"] as SolidColorBrush
                 }
             );
            series.Add(
                 new PieSeries
                 {
                     Values = new ChartValues<int> { all - partial },
                     Title = "",
                     DataLabels = false,
                     Fill = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush
                 }
             );
            pieChart.Series = series;
        }
        async void userImageLoad(Ellipse ellipse, string image,DateTime updateDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(image))
                {
                    clearImg(ellipse);
                    var bitmapImage = new BitmapImage();
                    bool isModified = SectionData.chkImgChng(image, (DateTime)updateDate, Global.TMPUsersFolder);
                    if (!isModified)
                        bitmapImage = SectionData.getOnlineUserImg(image);
                    else
                    {
                        byte[] imageBuffer = await user.downloadImage(image); // read this as BLOB from your DB
                        using (var memoryStream = new System.IO.MemoryStream(imageBuffer))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();
                        }
                        
                    }
                    ellipse.Fill = new ImageBrush(bitmapImage);
                }
                else
                {
                    clearImg(ellipse);
                }
            }
            catch
            {
                clearImg(ellipse);
            }
        }
        private void clearImg(Ellipse ellipse)
        {
            Uri resourceUri = new Uri("pic/no-image-icon-90x90.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            brush.ImageSource = temp;
            ellipse.Fill = brush;
        }
        public void timerAnimation()
        {
            x = 1000;
            y = 2000;
            z = 3000;
            //value_items = Convert.ToInt32(tb_items.Text);
            //value_purchase = Convert.ToInt32(tb_purchase.Text);
            //value_sales = Convert.ToInt32(tb_sales.Text);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            try
            {

                x += 86;
            y += 74;
            z += 65;

            //tb_items.Text = x.ToString();
            //tb_purchase.Text = x.ToString();
            //tb_sales.Text = x.ToString();
            if (x >= 5000)
            {
                //tb_items.Text = Convert.ToString(value_items);
                //tb_purchase.Text = Convert.ToString(value_purchase);
                //tb_sales.Text = Convert.ToString(value_sales);
                timer.Stop();
            }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void UserControl_TouchLeave(object sender, TouchEventArgs e)
        {
            MessageBox.Show("hey i'm here in TouchLeave");
        }
        private async void Cb_branch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_branch.IsMouseCaptured)
                {
                    if (sender != null)
                        SectionData.StartAwait(grid_main);

                    await refreshViewTask();
                    if (sender != null)
                        SectionData.EndAwait(grid_main);
                }
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (threadtimer10 != null)
            {
                threadtimer10.Stop();
                threadtimer10.IsEnabled = false;
            }
            if (threadtimer30 != null)
            {
                threadtimer30.Stop();
                threadtimer30.IsEnabled = false;
            }
        }

        private void Btn_posOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;

                wd_posOnline w = new wd_posOnline();
                w.posList = dash.PosOnlineInfo.ToList();
                w.ShowDialog();


                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        private void Btn_printChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Button button = sender as Button;
                string buttonTag = button.Tag.ToString();
                Window.GetWindow(this).Opacity = 0.2;

                wd_chart w = new wd_chart();
                    w.type = "cartesianChartHome";
                    w.cartesianChartHome.Series = cartesianChartHome.Series;
                    w.axs_AxisX.Labels = axs_AxisX.Labels;
                    w.axs_AxisY.Labels = axs_AxisY.Labels;

                //if (buttonTag.Equals("cartesianChart"))
                //{
                //    w.type = "cartesianChart";
                //    w.cartesianChart.Series = cartesianChart.Series;
                //    w.axcolumn.Labels = axcolumn.Labels;

                //}
                //else if (buttonTag.Equals("chart1"))
                //{
                //    w.type = "chart1";
                //    w.pieChart.Series = chart1.Series;

                //}
                //else if (buttonTag.Equals("rowChart"))
                //{
                //    w.type = "cartesianChart";
                //    w.cartesianChart.Series = rowChart.Series;
                //    w.axcolumn.Labels = MyAxis.Labels;
                //}
                w.ShowDialog();

                Window.GetWindow(this).Opacity = 1;
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
    }
}
