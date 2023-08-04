using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;
using static POS.View.uc_categorie;

namespace POS.View.sales
{
    /// <summary>
    /// Interaction logic for uc_orders.xaml
    /// </summary>
    public partial class uc_orders : UserControl
    {
        string createPermission = "salesOrders_create";
        string deliveryPermission = "setUserSetting_delivery";
        string reportsPermission = "salesOrders_reports";
        string deletePermission = "salesOrders_delete";
        private static uc_orders _instance;
        public static uc_orders Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_orders();
                return _instance;
            }
        }
        public uc_orders()
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

        #region variables
        List<Coupon> couponsLst = new List<Coupon>();
        ObservableCollection<BillDetails> billDetails = new ObservableCollection<BillDetails>();
        public static bool isFromReport = false;
        public static bool archived = false;
        static private int _invoiceId;
        Item itemModel = new Item();
        Item item = new Item();
        IEnumerable<Item> items;
        List<Invoice> invoices;

        Branch branchModel = new Branch();
        //Branch branch;
        Agent agentModel = new Agent();
        IEnumerable<Agent> customers;
        ItemUnit itemUnitModel = new ItemUnit();
        List<ItemUnit> barcodesList;
        List<Item> itemUnits;
        Invoice invoiceModel = new Invoice();
        public Invoice invoice = new Invoice();
        ItemLocation itemLocationModel = new ItemLocation();
        Coupon couponModel = new Coupon();
        IEnumerable<Coupon> coupons;
        List<CouponInvoice> selectedCoupons = new List<CouponInvoice>();
       // Pos posModel = new Pos();
       Pos pos;
        List<ItemTransfer> invoiceItems;
        ShippingCompanies companyModel = new ShippingCompanies();
       // List<ShippingCompanies> companies;
        //User userModel = new User();
        //List<User> users;
        public List<Control> controls;
        Notification notification = new Notification();
        #region for notifications
        private static DispatcherTimer timer;
        int _DraftCount = 0;
        int _OrdersCount = 0;
        int _OrdersWaitCount = 0;
        int _DocCount = 0;
        #endregion
        #region//to handle barcode characters
        static private int _SelectedCustomer = -1;
        static private int _SelectedCompany = -1;
        static private int _SelectedUser = -1;
        static private int _SelectedDiscountType = -1;

        // for barcode
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private object _Sender;
        bool _IsFocused = false;
        #endregion
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;
        //for bill details
        static private int _SequenceNum = 0;
        static private decimal _Sum = 0;
        static private decimal _Tax = 0;
        static private decimal _Discount = 0;
        static private decimal _DeliveryCost = 0;
        static private decimal _RealDeliveryCost = 0;
        static public string _InvoiceType = "ord"; // order draft

        // for report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        Invoice prInvoice = new Invoice();
        //bool isClose = false;
        #endregion

        #region bill
        public class BillDetails
        {
            public int ID { get; set; }
            public int itemId { get; set; }
            public int itemUnitId { get; set; }
            public int? offerId { get; set; }

            public string Product { get; set; }
            public string Unit { get; set; }
            public int Count { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
            public decimal VATRatio { get; set; }
            public bool isTaxExempt { get; set; }
            public decimal Tax { get; set; }
            public decimal basicPrice { get; set; }
            public decimal OfferValue { get; set; }
            public string OfferType { get; set; }
            public string OfferName { get; set; }
        }

        #endregion
        private void translate()
        {
            dg_billDetails.Columns[1].Header = MainWindow.resourcemanager.GetString("trNo.");
            dg_billDetails.Columns[2].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_billDetails.Columns[3].Header = MainWindow.resourcemanager.GetString("trUnit");
            dg_billDetails.Columns[4].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_billDetails.Columns[5].Header = MainWindow.resourcemanager.GetString("trPrice");
            dg_billDetails.Columns[6].Header = MainWindow.resourcemanager.GetString("trTotal");

            txt_tax.Text = MainWindow.resourcemanager.GetString("trTax");
            txt_sum.Text = MainWindow.resourcemanager.GetString("trSum");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrder");
            txt_store.Text = MainWindow.resourcemanager.GetString("trBranch");
            txt_coupon.Text = MainWindow.resourcemanager.GetString("trCoupon");
            txt_customer.Text = MainWindow.resourcemanager.GetString("trCustomer");
            txt_delivery.Text = MainWindow.resourcemanager.GetString("trDelivery");
            txt_discount.Text = MainWindow.resourcemanager.GetString("trDiscount");
            txt_isApproved.Text = MainWindow.resourcemanager.GetString("trReady");

            txt_waitConfirmUser.Text = MainWindow.resourcemanager.GetString("trWaiting");
            txt_printInvoice.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_invoiceImages.Text = MainWindow.resourcemanager.GetString("trImages");
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_orders.Text = MainWindow.resourcemanager.GetString("trReady");
            txt_drafts.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_newDraft.Text = MainWindow.resourcemanager.GetString("trNew");
            txt_submitOrder.Text = MainWindow.resourcemanager.GetString("trReserve");

            tt_error_previous.Content = MainWindow.resourcemanager.GetString("trPrevious");
            tt_error_next.Content = MainWindow.resourcemanager.GetString("trNext");
            chk_onDelivery.Content = MainWindow.resourcemanager.GetString("OnDelivery");
            chk_isFreeDelivery.Content = MainWindow.resourcemanager.GetString("freeDelivery");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_coupon, MainWindow.resourcemanager.GetString("trCoponHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_customer, MainWindow.resourcemanager.GetString("trCustomerHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trCompanyHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUserHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_discount, MainWindow.resourcemanager.GetString("trDiscountHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_typeDiscount, MainWindow.resourcemanager.GetString("trDiscountTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sliceId, MainWindow.resourcemanager.GetString("invoiceClass"));

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        #region loading
        List<keyValueBool> loadingList;
        List<string> catchError = new List<string>();
        int catchErrorCount = 0;
        async void loading_RefrishItems()
        {
            try
            {
                await RefrishItems();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_RefrishItems"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_RefrishCustomers()
        {
            try
            {
                if (FillCombo.customersList is null)
                    await FillCombo.RefreshCustomers();
                RefrishCustomers();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_RefrishCustomers"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_fillBarcodeList()
        {
            try
            {
                await fillBarcodeList();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillBarcodeList"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_fillCouponsList()
        {
            try
            {
                await fillCouponsList();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillCouponsList"))
                {
                    item.value = true;
                    break;
                }
            }
        }


        async void loading_fillBranches()
        {
            try
            {

                await SectionData.fillBranchesWithType(cb_branch, "b");

                cb_branch.SelectedValue = MainWindow.branchID;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillBranches"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_fillShippingCompanies()
        {
            try
            {
                await fillShippingCompanies();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillShippingCompanies"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_fillUsers()
        {
            try
            {
                await fillUsers();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillUsers"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        bool loadingSuccess_globalSaleUnits = false;
        async void loading_globalSaleUnits()
        {
            try
            {
                #region global Sale Units
                MainWindow.InvoiceGlobalSaleUnitsList = await itemUnitModel.GetForSale();

                if (MainWindow.InvoiceGlobalSaleUnitsList is null)
                    loading_globalSaleUnits();
                else
                    loadingSuccess_globalSaleUnits = true;
                #endregion

            }
            catch (Exception ex)
            {
                catchError.Add("loading_globalSaleUnits");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_globalSaleUnits = true;
                }
                else
                    loading_globalSaleUnits();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_globalSaleUnits)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_globalSaleUnits"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }


        bool loadingSuccess_FillComboSlices = false;
        async void loading_FillComboSlices()
        {
            try
            {
                await FillCombo.FillComboSlicesUser(cb_sliceId);

                ///default slice
                if (AppSettings.DefaultInvoiceSlice == 0 || FillCombo.slicesUserList.Where(w => w.isActive == true && w.sliceId == AppSettings.DefaultInvoiceSlice).Count() == 0)
                    cb_sliceId.SelectedIndex = 0;
                else
                    cb_sliceId.SelectedValue = AppSettings.DefaultInvoiceSlice;

                if (FillCombo.slicesUserList is null)
                    loading_FillComboSlices();
                else
                    loadingSuccess_FillComboSlices = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_FillComboSlices");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_FillComboSlices = true;
                }
                else
                    loading_FillComboSlices();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_FillComboSlices)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_FillComboSlices"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_FillTaxes = false;
        async void loading_FillTaxes()
        {
            try
            {
                await fillTaxes();

                if (taxes is null)
                    loading_FillTaxes();
                else
                    loadingSuccess_FillTaxes = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_FillTaxes");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_FillTaxes = true;
                }
                else
                    loading_FillTaxes();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_FillTaxes)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_FillTaxes"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        List<Taxes> taxes = new List<Taxes>();
        private async Task fillTaxes()
        {
            await FillCombo.RefreshTaxess();

            taxes = FillCombo.taxessList.Where(x => x.isActive == true && x.taxType == "sales").ToList();
        }
        #endregion
        public async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                MainWindow.mainWindow.Closing += ParentWin_Closing;
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                MainWindow.mainWindow.KeyDown += HandleKeyPress;
                tb_moneyIcon.Text = AppSettings.Currency;
                tb_moneyIconTotal.Text = AppSettings.Currency;
                tb_moneyIconDis.Text = AppSettings.Currency;
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
                tb_moneyIcon.Text = AppSettings.Currency;
                tb_moneyIcon1.Text = AppSettings.Currency;

                translate();

                lst_coupons.ItemsSource = couponsLst;
                lst_coupons.DisplayMemberPath = "notes";
                lst_coupons.SelectedValuePath = "cId";

                configureDiscountType();


                #region loading
                loadingList = new List<keyValueBool>();
                bool isDone = true;
                loadingList.Add(new keyValueBool { key = "loading_RefrishItems", value = false });
                loadingList.Add(new keyValueBool { key = "loading_RefrishCustomers", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillBarcodeList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillCouponsList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillBranches", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillShippingCompanies", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillUsers", value = false });
                loadingList.Add(new keyValueBool { key = "loading_globalSaleUnits", value = false });
                loadingList.Add(new keyValueBool { key = "loading_FillComboSlices", value = false });
                loadingList.Add(new keyValueBool { key = "loading_FillTaxes", value = false });

                loading_RefrishItems();
                loading_RefrishCustomers();
                loading_fillBarcodeList();
                loading_fillCouponsList();
                loading_fillBranches();
                loading_fillShippingCompanies();
                loading_fillUsers();
                loading_globalSaleUnits();
                loading_FillComboSlices();
                loading_FillTaxes();
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
                        await Task.Delay(0500);
                    }
                }
                while (!isDone);
                #endregion

                pos = MainWindow.posLogIn;

                //List all the UIElement in the VisualTree
                controls = new List<Control>();
                FindControl(this.grid_main, controls);

                if (AppSettings.invoiceTax_bool == false)
                    sp_tax.Visibility = Visibility.Collapsed;
                else
                {
                    tb_taxValue.Text = SectionData.DecTostring(AppSettings.invoiceTax_decimal);
                    sp_tax.Visibility = Visibility.Visible;
                }
                setNotifications();
                setTimer();
                #region datagridChange
                //CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(dg_billDetails.Items);
                //((INotifyCollectionChanged)myCollectionView).CollectionChanged += new NotifyCollectionChangedEventHandler(DataGrid_CollectionChanged);
                #endregion

                #region key up
                cb_branch.IsTextSearchEnabled = false;
                cb_branch.IsEditable = true;
                cb_branch.StaysOpenOnEdit = true;
                cb_branch.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branch.Text = "";

                cb_company.IsTextSearchEnabled = false;
                cb_company.IsEditable = true;
                cb_company.StaysOpenOnEdit = true;
                cb_company.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_company.Text = "";

                cb_customer.IsTextSearchEnabled = false;
                cb_customer.IsEditable = true;
                cb_customer.StaysOpenOnEdit = true;
                cb_customer.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_customer.Text = "";

                cb_user.IsTextSearchEnabled = false;
                cb_user.IsEditable = true;
                cb_user.StaysOpenOnEdit = true;
                cb_user.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_user.Text = "";

                cb_sliceId.IsTextSearchEnabled = false;
                cb_sliceId.IsEditable = true;
                cb_sliceId.StaysOpenOnEdit = true;
                cb_sliceId.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_sliceId.Text = "";
                #endregion

                #region Permision
                //if (MainWindow.groupObject.HasPermissionAction(deliveryPermission, MainWindow.groupObjects, "one"))
                //    md_ordersWait.Visibility = Visibility.Visible;
                //else
                //    md_ordersWait.Visibility = Visibility.Collapsed;
                #endregion

                #region print - pdf - send email
                if (!isFromReport)
                {
                    btn_printInvoice.Visibility = Visibility.Collapsed;
                    btn_pdf.Visibility = Visibility.Collapsed;
                    sp_Approved.Visibility = Visibility.Collapsed;
                }
                #endregion
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                tb_barcode.Focus();
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void ParentWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //isClose = true;
            //UserControl_Unloaded(this, null);
        }
        public void FindControl(DependencyObject root, List<Control> controls)
        {
            controls.Clear();
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var control = current as Control;
                if (control != null && control.IsTabStop)
                {
                    controls.Add(control);
                }
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child != null)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }
        #region timer to refresh notifications
        private void setTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(180); // 3 minutes
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private async void timer_Tick(object sendert, EventArgs et)
        {
            try
            {
                //refreshOrdersNotification();

                if (invoice.invoiceId != 0)
                {
                    refreshDocCount(invoice.invoiceId);
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region notifications
        private async void setNotifications()
        {
            try
            {
                refreshDraftNotification();
                refreshOrdersNotification();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async Task refreshDraftNotification()
        {
            try
            {
                string invoiceType = "ord, ors";
                int duration = 2;
                int draftCount = 0;
                if (AppSettings.SalesOrdersDraftCount <= 0)
                {
                    draftCount = (int)await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                    draftCount = draftCount < 0 ? 0 : draftCount;
                    AppSettings.SalesOrdersDraftCount = draftCount;
                }
                setDraftNotification(AppSettings.SalesOrdersDraftCount);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void setDraftNotification(int draftCount)
        {
            try
            {
                if (draftCount > 0 && invoice != null && (_InvoiceType == "ord" || _InvoiceType == "ors") && invoice.invoiceId != 0 && !isFromReport)
                    draftCount--;
                SectionData.refreshNotification(md_draft, ref _DraftCount, draftCount);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async Task refreshOrdersNotification()
        {
            try
            {
                string invoiceType = "or";
                int duration = 0;
                if (AppSettings.SalesWaitingOrdersCount == 0)
                {
                    AppSettings.SalesWaitingOrdersCount = (int)await invoice.GetCountUnHandeledOrders(invoiceType, MainWindow.branchID.Value, 0, MainWindow.userID.Value, duration);
                }
                setOrdersNotification(AppSettings.SalesWaitingOrdersCount);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void setOrdersNotification(int orderCount)
        {
            try
            {
                if (invoice != null && _InvoiceType == "or" && invoice.invoiceId != 0 && !isFromReport)
                    orderCount--;

                if (orderCount != _OrdersCount)
                {
                    if (orderCount > 9)
                    {
                        md_order.Badge = "+9";
                    }
                    else if (orderCount == 0) md_order.Badge = "";
                    else
                        md_order.Badge = orderCount.ToString();
                }
                _OrdersCount = orderCount;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async Task refreshDocCount(int invoiceId)
        {
            try
            {
                DocImage doc = new DocImage();
                int docCount = (int)await doc.GetDocCount("Invoices", invoiceId);

                if (docCount != _DocCount)
                {
                    if (docCount > 9)
                    {
                        md_docImage.Badge = "+9";
                    }
                    else if (docCount == 0) md_docImage.Badge = "";
                    else
                        md_docImage.Badge = docCount.ToString();
                }
                _DocCount = docCount;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        #endregion
        private void configureDiscountType()
        {
            var dislist = new[] {
            new { Text = "", Value = -1 },
            new { Text = MainWindow.resourcemanager.GetString("trValueDiscount"), Value = 1 },
            new { Text = MainWindow.resourcemanager.GetString("trPercentageDiscount"), Value = 2 },
             };

            cb_typeDiscount.DisplayMemberPath = "Text";
            cb_typeDiscount.SelectedValuePath = "Value";
            cb_typeDiscount.ItemsSource = dislist;
            cb_typeDiscount.SelectedIndex = 0;
        }
        void RefrishCustomers()
        {
            customers = FillCombo.customersList.Where(x => x.agentId != 0).ToList();
            cb_customer.ItemsSource = customers;
            cb_customer.DisplayMemberPath = "name";
            cb_customer.SelectedValuePath = "agentId";
        }
        async Task RefrishItems()
        {
            items = await itemModel.GetSaleOrPurItems(0, 0, 0, MainWindow.branchID.Value);
        }

        async Task fillBarcodeList()
        {
            barcodesList = await itemUnitModel.Getall();
        }
        async Task fillCouponsList()
        {
            coupons = await couponModel.GetEffictiveCoupons();

            foreach (Coupon c in coupons)
                c.notes = c.name + "   #" + c.code + System.Environment.NewLine + c.details;
        }
        private async Task fillShippingCompanies()
        {
            await FillCombo.FillComboShippingCompaniesWithDefault(cb_company);

        }
        private async Task fillUsers()
        {
            await FillCombo.FillComboDrivers(cb_user);

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
        #region Button In DataGrid
        void deleteRowFromInvoiceItems(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);

                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        BillDetails row = (BillDetails)dg_billDetails.SelectedItems[0];
                        int index = dg_billDetails.SelectedIndex;
                        // calculate new sum
                        _Sum -= row.Total;

                        // remove item from bill
                        billDetails.RemoveAt(index);

                        ObservableCollection<BillDetails> data = (ObservableCollection<BillDetails>)dg_billDetails.ItemsSource;
                        data.Remove(row);

                        // calculate new total
                        refreshTotalValue();
                    }
                _SequenceNum = 0;
                _Sum = 0;

                for (int i = 0; i < billDetails.Count; i++)
                {
                    _SequenceNum++;
                    _Sum += billDetails[i].Total;
                    billDetails[i].ID = _SequenceNum;
                }
                refrishBillDetails();

                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region coupon

        private async Task dealWithBarcode(string barcode)
        {
            int codeindex = barcode.IndexOf("-");
            string prefix = "";
            if (codeindex >= 0)
                prefix = barcode.Substring(0, codeindex);
            prefix = prefix.ToLower();
            barcode = barcode.ToLower();
            switch (prefix)
            {
                case "or":// this barcode for invoice

                    Btn_newDraft_Click(null, null);
                    invoice = await invoiceModel.GetInvoicesByNum(barcode);
                    _InvoiceType = invoice.invType;
                    if (_InvoiceType.Equals("ord") || _InvoiceType.Equals("or"))
                    {
                        // set title to bill
                        if (_InvoiceType == "ord")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrderDraft");
                        }
                        else if (_InvoiceType == "or")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrder");
                        }
                        await fillInvoiceInputs(invoice);
                    }
                    break;
                case "cop":// this barcode for coupon
                    {
                        couponModel = coupons.ToList().Find(c => c.barcode.ToLower() == barcode);
                        var exist = selectedCoupons.Find(c => c.couponId == couponModel.cId);
                        if (couponModel != null && exist == null)
                        {
                            if ((couponModel.invMin != 0 && couponModel.invMax != 0 && _Sum >= couponModel.invMin && _Sum <= couponModel.invMax)
                                 || (couponModel.invMax == 0 && _Sum >= couponModel.invMin)
                                  || (couponModel.invMax != 0 && couponModel.invMin == 0 && _Sum <= couponModel.invMax))
                            {
                                CouponInvoice ci = new CouponInvoice();
                                ci.couponId = couponModel.cId;
                                ci.discountType = couponModel.discountType;
                                ci.discountValue = couponModel.discountValue;

                                couponsLst.Add(couponModel);
                                lst_coupons.ItemsSource = null;
                                lst_coupons.ItemsSource = couponsLst;

                                selectedCoupons.Add(ci);
                                refreshTotalValue();
                            }
                            else if (couponModel.invMax != 0 || couponModel.invMin != 0)
                            {
                                if (_Sum < couponModel.invMin)
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMinInvToolTip"), animation: ToasterAnimation.FadeIn);
                                else if (_Sum > couponModel.invMax)
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMaxInvToolTip"), animation: ToasterAnimation.FadeIn);
                            }
                            else if (couponModel.invMax == 0)
                            {
                                if (_Sum < couponModel.invMin)
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMinInvToolTip"), animation: ToasterAnimation.FadeIn);
                            }
                        }
                        else
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorCouponExist"), animation: ToasterAnimation.FadeIn);
                        }
                        tb_coupon.Clear();
                    }
                    break;
                default: // if barcode for item
                    tb_barcode.Text = barcode;
                    // get item matches barcode
                    if (barcodesList != null)
                    {
                        ItemUnit unit1 = barcodesList.ToList().Find(c => c.barcode == tb_barcode.Text.Trim());

                        // get item matches the barcode
                        if (unit1 != null)
                        {
                            int itemId = (int)unit1.itemId;
                            if (unit1.itemId != 0)
                            {
                                int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == unit1.itemUnitId).FirstOrDefault());

                                if (index == -1)//item doesn't exist in bill
                                {
                                    decimal price = 0;
                                    decimal basicPrice = 0;
                                    decimal tax = 0;
                                    if (unit1.SalesPrices == null || (unit1.SalesPrices != null && unit1.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                                    {
                                        //basicPrice = (decimal)unit1.basicPrice;
                                        //if (AppSettings.itemsTax_bool == true)
                                        //{
                                        //    price = (decimal)unit1.priceTax;
                                        //}
                                        //else
                                        price = (decimal)unit1.price;
                                        basicPrice = (decimal)unit1.price;
                                    }
                                    else
                                    {
                                        var slice = unit1.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                                        //basicPrice = (decimal)slice.basicPrice;
                                        //if (AppSettings.itemsTax_bool == true)
                                        //{
                                        //    price = (decimal)slice.priceTax;
                                        //}
                                        //else
                                            price = (decimal)slice.price;
                                        basicPrice = (decimal)slice.price;
                                    }

                                    // get item units
                                    itemUnits = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemId == item.itemId).ToList();
                                    //get item from list
                                    item = items.ToList().Find(i => i.itemId == itemId);

                                    int count = 1;
                                    if (AppSettings.itemsTax_bool == true)
                                        tax = (decimal)(count * item.taxes);

                                    decimal total = count * price;

                                    int offerId = 0;
                                    string discountType = "1";
                                    decimal discountValue = 0;
                                    if (item.offerId != null)
                                    {
                                        offerId = (int)item.offerId;
                                        discountType = item.discountType;
                                        discountValue = (decimal)item.discountValue;
                                    }
                                    addRowToBill(item.name, item.itemId, unit1.mainUnit, unit1.itemUnitId, count, price, total, tax, item.offerId, discountType, discountValue, basicPrice);
                                }
                                else // item exist prevoiusly in list
                                {
                                    decimal itemTax = 0;
                                    if (item.taxes != null)
                                        itemTax = (decimal)item.taxes;
                                    billDetails[index].Count++;
                                    billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;
                                   // if (AppSettings.itemsTax_bool == true)
                                      //  billDetails[index].Tax = (decimal)(billDetails[index].Count * itemTax);
                                   // else
                                    //    billDetails[index].Tax = 0;

                                    _Sum += billDetails[index].Price;

                                }
                                refreshTotalValue();
                                refrishBillDetails();
                            }
                        }
                        else
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorItemNotFoundToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }

                    break;
            }

            tb_barcode.Clear();
        }
        private void Btn_clearCoupon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                _Discount = 0;
                selectedCoupons.Clear();

                couponsLst.Clear();
                lst_coupons.ItemsSource = null;


                tb_coupon.Clear();
                refreshTotalValue();

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
        #endregion
        #region Categor and Item 

        #region Get Id By Click  Y
        public void ChangeCategorieIdEvent(int categoryId)
        {
        }
        public void testChangeCategorieItemsIdEvent()
        {
        }
        #endregion

        #endregion

        private bool validateInvoiceValues()
        {
            bool valid = true;
            if (!SectionData.validateEmptyComboBox(cb_customer, p_errorCustomer, tt_errorCustomer, "trEmptyCustomerToolTip"))
                exp_customer.IsExpanded = true;
            if (!SectionData.validateEmptyComboBox(cb_branch, p_errorBranch, tt_errorBranch, "trEmptyBranchToolTip"))
                exp_store.IsExpanded = true;
            if (billDetails.Count == 0)
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trAddInvoiceWithoutItems"), animation: ToasterAnimation.FadeIn);

            if ((cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() == "0") || billDetails.Count == 0
                || (cb_branch.SelectedValue != null && cb_branch.SelectedValue.ToString() == "0"))
                valid = false;
            if (valid)
                valid = validateItemUnits();
            return valid;
        }
        private bool validateItemUnits()
        {
            bool valid = true;
            for (int i = 0; i < billDetails.Count; i++)
            {
                if (billDetails[i].itemUnitId == 0)
                {
                    valid = false;
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trItemWithNoUnit"), animation: ToasterAnimation.FadeIn);

                    return valid;
                }
            }
            return valid;
        }
        private async void Btn_newDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Boolean available = true;
                    bool valid = validateItemUnits();

                    if (billDetails.Count > 0 && available && valid && _InvoiceType == "ord")
                    {
                        #region Accept
                        MainWindow.mainWindow.Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trSaveOrderNotification");

                        w.ShowDialog();
                        MainWindow.mainWindow.Opacity = 1;
                        #endregion
                        if (w.isOk)
                            await addInvoice(_InvoiceType);

                        clearInvoice();
                        setNotifications();
                    }
                    else
                    {
                        clearInvoice();
                        setNotifications();
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
        private async Task clearInvoice()
        {
            txt_invNumber.Text = "";
            ///default slice
            if (AppSettings.DefaultInvoiceSlice == 0 || FillCombo.slicesUserList.Where(w => w.isActive == true && w.sliceId == AppSettings.DefaultInvoiceSlice).Count() == 0)
                cb_sliceId.SelectedIndex = 0;
            else
                cb_sliceId.SelectedValue = AppSettings.DefaultInvoiceSlice; _Sum = 0;
            _Discount = 0;
            _SequenceNum = 0;
            _DeliveryCost = 0;
            _RealDeliveryCost = 0;
            _SelectedCustomer = -1;
            _SelectedDiscountType = 0;
            _InvoiceType = "ord";
            invoice = new Invoice();
            selectedCoupons.Clear();
            tb_barcode.Clear();
            cb_customer.SelectedIndex = -1;
            cb_customer.SelectedItem = "";
            btn_updateCustomer.IsEnabled = false;
            tgl_ActiveOffer.IsChecked = false;
            chk_onDelivery.IsChecked = false;
            chk_isFreeDelivery.IsChecked = false;
            cb_branch.SelectedValue = MainWindow.branchID;

            cb_branch.SelectedItem = "";
            tb_note.Clear();
            billDetails.Clear();
            tb_total.Text = "0";
            tb_sum.Text = "0";
            tb_totalDescount.Text = "0";
            cb_company.SelectedIndex = -1;
            cb_company.SelectedItem = "";
            cb_user.SelectedIndex = -1;
            cb_user.SelectedItem = "";
            tb_discount.Clear();
            cb_typeDiscount.SelectedIndex = 0;
            tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
            md_docImage.Badge = "";

            couponsLst = new List<Coupon>();
            lst_coupons.ItemsSource = null;

            isFromReport = false;
            archived = false;
            btn_deleteInvoice.Visibility = Visibility.Collapsed;
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrder");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
            SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
            SectionData.clearComboBoxValidate(cb_branch, p_errorBranch);
            refrishBillDetails();
            tb_barcode.Focus();
            inputEditable();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
            await fillCouponsList();
        }
        private void inputEditable()
        {
            if (archived)
                _InvoiceType = "or";
            if (archived || (isFromReport && invoice.performed == true))
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                cb_branch.IsEnabled = false;
                cb_sliceId.IsEnabled = false;
                cb_customer.IsEnabled = false;
                cb_company.IsEnabled = false;
                cb_user.IsEnabled = false;
                tb_note.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                tgl_ActiveOffer.IsEnabled = false;
                btn_save.IsEnabled = false;
                tb_coupon.IsEnabled = false;
                btn_clearCoupon.IsEnabled = false;
                btn_clearCustomer.IsEnabled = false;
                chk_onDelivery.IsEnabled = false;
                chk_isFreeDelivery.IsEnabled = false;
                btn_items.IsEnabled = false;
                btn_deleteInvoice.Visibility = Visibility.Collapsed;
                btn_submitOrder.Visibility = Visibility.Collapsed;
            }
            else
                switch (_InvoiceType)
                {
                    case "ord": // sales order draft invoice
                        dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                        dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                        dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                        cb_branch.IsEnabled = true;
                        cb_sliceId.IsEnabled = true;
                        cb_customer.IsEnabled = true;
                        cb_company.IsEnabled = true;
                        cb_user.IsEnabled = true;
                        tb_note.IsEnabled = true;
                        tb_barcode.IsEnabled = true;
                        tgl_ActiveOffer.IsEnabled = false;
                        btn_save.IsEnabled = true;
                        tb_coupon.IsEnabled = true;
                        btn_clearCoupon.IsEnabled = true;
                        btn_clearCustomer.IsEnabled = true;
                        chk_onDelivery.IsEnabled = true;
                        chk_isFreeDelivery.IsEnabled = true;
                        btn_items.IsEnabled = true;

                        btn_deleteInvoice.Visibility = Visibility.Collapsed;
                        btn_submitOrder.Visibility = Visibility.Collapsed;
                        break;
                    case "ors": // sales order saved  invoice
                        dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                        dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                        dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                        cb_branch.IsEnabled = true;
                        cb_sliceId.IsEnabled = true;
                        cb_customer.IsEnabled = true;
                        cb_company.IsEnabled = true;
                        cb_user.IsEnabled = true;
                        tb_note.IsEnabled = true;
                        tb_barcode.IsEnabled = true;
                        tgl_ActiveOffer.IsEnabled = true;
                        btn_save.IsEnabled = true;
                        tb_coupon.IsEnabled = true;
                        btn_clearCoupon.IsEnabled = true;
                        btn_clearCustomer.IsEnabled = true;
                        chk_onDelivery.IsEnabled = true;
                        chk_isFreeDelivery.IsEnabled = true;
                        btn_items.IsEnabled = true;
                        btn_deleteInvoice.Visibility = Visibility.Visible;
                        btn_submitOrder.Visibility = Visibility.Visible;
                        break;
                    case "or": //quotation invoice
                        dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                        dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                        dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                        cb_branch.IsEnabled = false;
                        cb_sliceId.IsEnabled = false;
                        cb_customer.IsEnabled = false;
                        cb_company.IsEnabled = false;
                        cb_user.IsEnabled = false;
                        tb_note.IsEnabled = false;
                        tb_barcode.IsEnabled = false;
                        tgl_ActiveOffer.IsEnabled = true;
                        btn_save.IsEnabled = true;
                        tb_coupon.IsEnabled = false;
                        btn_clearCoupon.IsEnabled = false;
                        btn_clearCustomer.IsEnabled = false;
                        chk_onDelivery.IsEnabled = false;
                        chk_isFreeDelivery.IsEnabled = false;
                        btn_items.IsEnabled = false;
                        btn_deleteInvoice.Visibility = Visibility.Collapsed;
                        btn_submitOrder.Visibility = Visibility.Collapsed;
                        break;
                }
            if (_InvoiceType.Equals("or") || _InvoiceType.Equals("ors") || _InvoiceType.Equals("s"))
            {
                #region print - pdf - send email
                btn_printInvoice.Visibility = Visibility.Visible;
                btn_pdf.Visibility = Visibility.Visible;
                sp_Approved.Visibility = Visibility.Visible;
                #endregion
            }
            else
            {
                #region print - pdf - send email
                btn_printInvoice.Visibility = Visibility.Collapsed;
                btn_pdf.Visibility = Visibility.Collapsed;
                sp_Approved.Visibility = Visibility.Collapsed;
                #endregion
            }
            if (!isFromReport)
            {
                btn_next.Visibility = Visibility.Visible;
                btn_previous.Visibility = Visibility.Visible;
            }
        }
        #region navigation buttons
        private void navigateBtnActivate()
        {
            int index = invoices.IndexOf(invoices.Where(x => x.invoiceId == _invoiceId).FirstOrDefault());
            if (index == invoices.Count - 1)
                btn_next.IsEnabled = false;
            else
                btn_next.IsEnabled = true;

            if (index == 0)
                btn_previous.IsEnabled = false;
            else
                btn_previous.IsEnabled = true;
        }
        private async Task clearNavigation()
        {
            txt_invNumber.Text = "";
            _Sum = 0;
            _Discount = 0;
            _SequenceNum = 0;
            _DeliveryCost = 0;
            _RealDeliveryCost = 0;
            _SelectedCustomer = -1;
            _SelectedDiscountType = 0;
            invoice = new Invoice();
            selectedCoupons.Clear();
            tb_barcode.Clear();
            cb_customer.SelectedIndex = -1;
            cb_customer.SelectedItem = "";
            cb_branch.SelectedIndex = -1;
            cb_branch.SelectedItem = "";
            tb_note.Clear();
            billDetails.Clear();
            tb_total.Text = "0";
            tb_sum.Text = "0";
            tb_totalDescount.Text = "0";
            cb_company.SelectedIndex = -1;
            cb_company.SelectedItem = "";
            cb_user.SelectedIndex = -1;
            cb_user.SelectedItem = "";
            tb_discount.Clear();
            cb_typeDiscount.SelectedIndex = 0;
            tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
            md_docImage.Badge = "";

            couponsLst.Clear();
            lst_coupons.ItemsSource = couponsLst;

            isFromReport = false;
            archived = false;
            btn_deleteInvoice.Visibility = Visibility.Collapsed;
            SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
            SectionData.clearComboBoxValidate(cb_branch, p_errorBranch);
            refrishBillDetails();
            tb_barcode.Focus();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
            await fillCouponsList();
        }
        private async Task navigateInvoice(int index)
        {
            try
            {
                await clearNavigation();
                invoice = invoices[index];
                _invoiceId = invoice.invoiceId;
                _InvoiceType = invoice.invType;

                if (invoice.invType == "ord")
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrderDraft");
                else if (invoice.invType == "ors")
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrderSaved");

                navigateBtnActivate();
                await fillInvoiceInputs(invoice);
            }
            catch (Exception ex)
            {
            }
        }
        private async void Btn_next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                int index = invoices.IndexOf(invoices.Where(x => x.invoiceId == _invoiceId).FirstOrDefault());
                index++;
                await navigateInvoice(index);
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
        private async void Btn_previous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                int index = invoices.IndexOf(invoices.Where(x => x.invoiceId == _invoiceId).FirstOrDefault());
                index--;
                await navigateInvoice(index);
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
        #endregion
        private async Task addInvoice(string invType)
        {
            int previouseBranch = 0;

            #region invoice object
            if (invoice.branchCreatorId == 0 || invoice.branchCreatorId == null)
            {
                invoice.branchCreatorId = MainWindow.branchID.Value;
                invoice.posId = MainWindow.posID.Value;
            }
            if ((invType == "or" || invType == "ors") && (invoice.invType == "ord" || invoice.invoiceId == 0))
                invoice.invNumber = "or";
            else if (invType == "ord" && invoice.invoiceId == 0)
                invoice.invNumber = "ord";


            if (invoice.invoiceId != 0)
            {
                if (invoice.branchId != null)
                    previouseBranch = (int)invoice.branchId;
            }
            invoice.invType = invType;
            invoice.discountValue = _Discount;
            invoice.discountType = "1";

            invoice.total = _Sum;
            invoice.totalNet = decimal.Parse(tb_total.Text);

            if (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0")
                invoice.agentId = (int)cb_customer.SelectedValue;

            invoice.notes = tb_note.Text;
            invoice.sales_invoice_note = await FillCombo.getSetValue("sales_invoice_note");
            invoice.itemtax_note = await FillCombo.getSetValue("itemtax_note");
            invoice.shippingCost = _DeliveryCost;
            invoice.realShippingCost = _RealDeliveryCost;
            if (cb_branch.SelectedValue != null && cb_branch.SelectedValue.ToString() != "0")
                invoice.branchId = (int)cb_branch.SelectedValue;

            if (cb_sliceId.SelectedValue != null)
            {
                invoice.sliceId = (int)cb_sliceId.SelectedValue;
                invoice.sliceName = cb_sliceId.Text;
            }

            if (tb_taxValue.Text != "" && AppSettings.invoiceTax_bool == true)
            {
                invoice.taxValue = _TaxValue;
                invoice.invoiceTaxes = invoiceTaxex;
              invoice.tax = _TaxValue;
            }
            else
            {
                invoice.taxValue = 0;
                invoice.tax = 0;
            }

            if (cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() != "0")
                invoice.shippingCompanyId = (int)cb_company.SelectedValue;
            else
                invoice.shippingCompanyId = null;

            if (cb_user.SelectedValue != null && cb_user.SelectedValue.ToString() != "0")
                invoice.shipUserId = (int)cb_user.SelectedValue;
            if (cb_typeDiscount.SelectedIndex != -1)
                invoice.manualDiscountType = cb_typeDiscount.SelectedValue.ToString();
            if (tb_discount.Text != "")
                invoice.manualDiscountValue = decimal.Parse(tb_discount.Text);
            invoice.createUserId = MainWindow.userID;
            invoice.updateUserId = MainWindow.userID;

            if (chk_onDelivery.IsChecked.Equals(true))
                invoice.isPrePaid = 0;
            else
                invoice.isPrePaid = 1;

            if (chk_isFreeDelivery.IsChecked == true)
                invoice.isFreeShip = 1;

            if (invType == "or" || invType == "ors" || invType == "ord")
            {
                byte isApproved = 0;
                if (tgl_ActiveOffer.IsChecked == true)
                    isApproved = 1;
                else
                    isApproved = 0;
                invoice.isApproved = isApproved;
            }
            #endregion

            #region invoice items
            invoiceItems = new List<ItemTransfer>();
            ItemTransfer itemT;
            decimal VATValue = 0;
            for (int i = 0; i < billDetails.Count; i++)
            {
                itemT = new ItemTransfer();

                itemT.quantity = billDetails[i].Count;
                itemT.price = billDetails[i].Price;
                itemT.itemUnitId = billDetails[i].itemUnitId;
                itemT.createUserId = MainWindow.userID;
                itemT.offerId = billDetails[i].offerId;
                itemT.offerType = decimal.Parse(billDetails[i].OfferType);
                itemT.offerValue = billDetails[i].OfferValue;
                itemT.offerName = billDetails[i].OfferName;
                //itemT.itemTax = billDetails[i].Tax;
                itemT.itemUnitPrice = billDetails[i].basicPrice;
                itemT.itemName = billDetails[i].Product;
                itemT.unitName = billDetails[i].Unit;
                itemT.isTaxExempt = billDetails[i].isTaxExempt;
                itemT.VATRatio = billDetails[i].VATRatio;
                

                //calculate VATValue
                if (AppSettings.itemsTax_bool == true)
                {
                    if (!itemT.isTaxExempt)
                    {
                        var tmp = SectionData.calcPercentage(1, (decimal)AppSettings.itemsTax_decimal);
                        decimal priceWithoutTax = (decimal)itemT.price / (1 + SectionData.calcPercentage(1, (decimal)AppSettings.itemsTax_decimal));
                        var embededTax = (decimal)itemT.price - priceWithoutTax;
                        itemT.itemTax = embededTax * (decimal)itemT.quantity;
                        VATValue += embededTax * (decimal)itemT.quantity;
                    }
                }
                invoiceItems.Add(itemT);
            }
            #endregion

            invoice.VATValue = VATValue;

            #region invoice coupons
            foreach (CouponInvoice ci in selectedCoupons)
            {
                ci.createUserId = MainWindow.userID;
            }
            #endregion
            #region status object
            invoiceStatus st = new invoiceStatus();
            st.status = "UnderProcessing";
            st.createUserId = MainWindow.userLogin.userId;
            #endregion

            #region notification Object
            int branchId = 0;
            if (cb_branch.SelectedValue != null)
                branchId = (int)cb_branch.SelectedValue;
            Notification not = new Notification()
            {
                title = "trSalesOrderAlertTilte",
                ncontent = "trSalesOrderAlertContent",
                msgType = "alert",
                objectName = "saleAlerts_executeOrder",
                prefix = MainWindow.userLogin.name,
                branchId = branchId,
                createUserId = MainWindow.userID.Value,
                updateUserId = MainWindow.userID.Value,
            };
            #endregion
            // save invoice in DB
            InvoiceResult invoiceResult = await invoiceModel.preparingOrder(invoice, invoiceItems, selectedCoupons, st, not, MainWindow.branchID.Value, previouseBranch, MainWindow.userID.Value);
            if (invoiceResult.Result > 0)
            {
                invoice.invoiceId = invoiceResult.Result;
                invoice.invNumber = invoiceResult.Message;
                invoice.updateDate = invoiceResult.UpdateDate;
                TimeSpan ts;
                TimeSpan.TryParse(invoiceResult.InvTime, out ts);
                invoice.invTime = ts;

                AppSettings.SalesOrdersDraftCount = invoiceResult.SalesDraftCount;
                AppSettings.SalesWaitingOrdersCount = invoiceResult.SalesWaitingOrdersCount;

                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
            }
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
        }

        #region Get Id By Click  Y
        public async Task ChangeItemIdEvent(int itemId)
        {
            try
            {
                if (items != null) item = items.ToList().Find(c => c.itemId == itemId);

                if (item != null)
                {
                    // get item units
                    itemUnits = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemId == item.itemId).ToList();
                    decimal itemTax = 0;
                    if (AppSettings.itemsTax_bool == true)
                    {
                        if (item.taxes != null)
                            itemTax = (decimal)(item.taxes);
                    }
                    else
                        itemTax = 0;

                    // search for default unit for sales
                    var defaultsaleUnit = itemUnits.ToList().Find(c => c.defaultSale == 1);
                    if (defaultsaleUnit != null)
                    {
                        decimal price = 0;
                        decimal basicPrice = 0;

                        if (defaultsaleUnit.SalesPrices == null || (defaultsaleUnit.SalesPrices != null && defaultsaleUnit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                        {
                            //basicPrice = (decimal)defaultsaleUnit.basicPrice;
                            //if (AppSettings.itemsTax_bool == true)
                            //    price = (decimal)defaultsaleUnit.priceTax;
                            //else
                                price = (decimal)defaultsaleUnit.price;
                            basicPrice = (decimal)defaultsaleUnit.price;
                        }
                        else
                        {
                            var slice = defaultsaleUnit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                            //basicPrice = (decimal)slice.basicPrice;
                            //if (AppSettings.itemsTax_bool == true)
                            //    price = (decimal)slice.priceTax;
                            //else
                                price = (decimal)slice.price;
                            basicPrice = (decimal)slice.price;
                        }
                        int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == defaultsaleUnit.itemUnitId).FirstOrDefault());
                        // create new row in bill details data grid
                        if (index == -1)//item doesn't exist in bill
                        {

                            int? offerId;
                            string discountType = "1";
                            decimal discountValue = 0;
                            if (item.offerId != null)
                            {
                                offerId = (int)item.offerId;
                                discountType = item.discountType;
                                discountValue = (decimal)item.discountValue;
                            }
                            else
                                offerId = null;
                            addRowToBill(item.name, itemId, defaultsaleUnit.unitName, (int)defaultsaleUnit.itemUnitId, 1, price, price, itemTax, item.offerId, discountType, discountValue, basicPrice);
                        }
                        else // item exist prevoiusly in list
                        {
                            billDetails[index].Count++;
                            billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;

                            _Sum += billDetails[index].Price;
                        }


                    }
                    else
                    {
                        addRowToBill(item.name, itemId, null, 0, 1, 0, 0, itemTax, null, "1", 0, 0);

                    }
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_branch")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorBranch, tt_errorBranch, "trEmptyBranchToolTip");
                    if ((sender as ComboBox).Name == "cb_customer")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCustomer, tt_errorCustomer, "trEmptyCustomerToolTip");
                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        decimal _TaxValue = 0;
        List<InvoiceTaxes> invoiceTaxex;
        private void refreshTotalValue()
        {

            _Discount = 0;
            _TaxValue = 0;
            decimal manualDiscount = 0;
            decimal totalDiscount = 0;
            invoiceTaxex = new List<InvoiceTaxes>();
            if (_Sum > 0)
            {
                #region calculate discount value
                foreach (CouponInvoice coupon in selectedCoupons)
                {
                    string discountType = coupon.discountType.ToString();
                    decimal discountValue = (decimal)coupon.discountValue;
                    if (discountType == "2")
                        discountValue = SectionData.calcPercentage(_Sum, discountValue);
                    _Discount += discountValue;
                }
                #endregion

                #region manaula discount           
                if (cb_typeDiscount.SelectedIndex != -1 && cb_typeDiscount.SelectedIndex != 0 && tb_discount.Text != "")
                {
                    int manualDisType = cb_typeDiscount.SelectedIndex;
                    manualDiscount = decimal.Parse(tb_discount.Text);
                    if (manualDisType == 2)
                        manualDiscount = SectionData.calcPercentage(_Sum, manualDiscount);
                }
                #endregion
                totalDiscount = _Discount + manualDiscount;
            }

            decimal total = _Sum - totalDiscount;

            #region invoice tax

            if (invoice != null && invoice.invType != null && invoice.invType.Equals("or"))
            {
                if (invoice.taxValue != null)
                    _TaxValue = (decimal)invoice.taxValue;
            }
            else if (AppSettings.invoiceTax_bool == true)
            {
                try
                {
                    foreach (var row in taxes)
                    {
                        decimal taxV = SectionData.calcPercentage(total, (decimal)row.rate);
                        //add tax to invoice tax
                        invoiceTaxex.Add(new InvoiceTaxes()
                        {
                            rate = row.rate,
                            taxType = row.taxType,
                            taxId = row.taxId,
                            taxValue = taxV,
                            name = row.name,
                            nameAr = row.nameAr,
                        });

                        // calculate entier tax value
                        _TaxValue += taxV;
                    }
                    //_TaxValue = SectionData.calcPercentage(total, decimal.Parse(tb_taxValue.Text));
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
            }
            if (_TaxValue != 0)
                tb_taxValue.Text = SectionData.DecTostring(_TaxValue);
            else
                tb_taxValue.Text = "0";
            //if (AppSettings.invoiceTax_bool == true)
            //{
            //    try
            //    {
            //        _TaxValue = SectionData.calcPercentage(total, decimal.Parse(tb_taxValue.Text));
            //    }
            //    catch (Exception ex)
            //    {
            //        SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            //    }
            //}

            #endregion

            total += _TaxValue;

            if (_Sum > 0 && chk_isFreeDelivery.IsChecked == false)
                total += _DeliveryCost;

            if (_Sum != 0)
                tb_sum.Text = SectionData.DecTostring(_Sum);
            else
                tb_sum.Text = "0";


            if (total < 0)
                total = 0;
            if (total != 0)
                tb_total.Text = SectionData.DecTostring(total);
            else
                tb_total.Text = "0";

            if (totalDiscount != 0)
                tb_totalDescount.Text = SectionData.PercentageDecTostring(totalDiscount);
            else
                tb_totalDescount.Text = "0";
        }
        #endregion
        #region billdetails

        bool firstTimeForDatagrid = true;
        async void refrishBillDetails()
        {
            dg_billDetails.ItemsSource = null;
            if (billDetails.Count == 1)
            {
                BillDetails bd = new BillDetails();
                billDetails.Add(bd);
                dg_billDetails.ItemsSource = billDetails;
                billDetails.Remove(bd);
            }
            else
                dg_billDetails.ItemsSource = billDetails;

            if (firstTimeForDatagrid)
            {
                SectionData.StartAwait(grid_main);
                dg_billDetails.IsEnabled = false;
                await Task.Delay(1000);
                dg_billDetails.Items.Refresh();
                firstTimeForDatagrid = false;
                dg_billDetails.IsEnabled = true;
                SectionData.EndAwait(grid_main);
            }
            DataGrid_CollectionChanged(dg_billDetails, null);
            //tb_sum.Text = _Sum.ToString();
            if (_Sum != 0)
                tb_sum.Text = SectionData.DecTostring(_Sum);
            else
                tb_sum.Text = "0";

            if (MainWindow.isInvTax == 0)
            {
                if (_Tax != 0)
                    tb_taxValue.Text = SectionData.PercentageDecTostring(_Tax);
                else
                    tb_taxValue.Text = "0";
            }

        }
        void refrishDataGridItems()
        {
            dg_billDetails.ItemsSource = null;
            dg_billDetails.ItemsSource = billDetails;
            dg_billDetails.Items.Refresh();
            DataGrid_CollectionChanged(dg_billDetails, null);
        }

        // read item from barcode
        private async void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_IsFocused)
                {
                    Control c = CheckActiveControl();
                    if (c == null)
                        tb_barcode.Focus();
                    _IsFocused = true;
                }
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 80)
                {
                    _BarcodeStr = "";
                }

                string digit = "";
                // record keystroke & timestamp 
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                    //digit pressed!         
                    digit = e.Key.ToString().Substring(1);
                    // = "1" when D1 is pressed
                }
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    digit = e.Key.ToString().Substring(6); // = "1" when NumPad1 is pressed
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                    digit = e.Key.ToString();
                else if (e.Key == Key.OemMinus)
                {
                    digit = "-";
                }

                _BarcodeStr += digit;
                _lastKeystroke = DateTime.Now;

                if (e.Key.ToString() == "Return" && _BarcodeStr != "" && _InvoiceType == "ord")
                {
                    await dealWithBarcode(_BarcodeStr);
                    if (_Sender != null) //clear barcode from inputs
                    {
                        DatePicker dt = _Sender as DatePicker;
                        TextBox tb = _Sender as TextBox;
                        if (dt != null)
                        {
                            if (dt.Name == "dp_desrvedDate")
                                _BarcodeStr = _BarcodeStr.Substring(1);
                        }
                        else if (tb != null)
                        {
                            if (tb.Name == "tb_note" || tb.Name == "tb_discount")// remove barcode from text box
                            {
                                string tbString = tb.Text;
                                string newStr = "";
                                int startIndex = tbString.IndexOf(_BarcodeStr);
                                if (startIndex != -1)
                                    newStr = tbString.Remove(startIndex, _BarcodeStr.Length);

                                tb.Text = newStr;
                            }
                        }
                    }
                    //tb_barcode.Text = _BarcodeStr;
                    _BarcodeStr = "";
                    e.Handled = true;
                    _IsFocused = false;
                }
                _Sender = null;

                if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                {
                    switch (e.Key)
                    {
                        case Key.P:
                            //handle P key
                            Btn_printInvoice_Click(null, null);
                            break;
                        case Key.S:
                            //handle S key
                            Btn_save_Click(btn_save, null);
                            break;
                        case Key.I:
                            //handle S key
                            Btn_items_Click(null, null);
                            break;
                    }
                }
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
        public Control CheckActiveControl()
        {
            for (int i = 0; i < controls.Count; i++)
            {
                Control c = controls[i];
                if (c.IsFocused)
                {
                    return c;
                }
            }
            return null;
        }
        private async void Tb_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                if (e.Key == Key.Return)
                {
                    string barcode = "";
                    if (tb_barcode.Text.Length <= 13)
                    {
                        barcode = tb_barcode.Text;
                        await dealWithBarcode(barcode);
                    }
                }
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

        private void addRowToBill(string itemName, int itemId, string unitName, int itemUnitId, int count, decimal price, decimal total, decimal tax, int? offerId,
                                     string offerType, decimal offerValue, decimal basicPrice)
        {
            // increase sequence for each read
            _SequenceNum++;

            decimal vatRatio = 0;
            if (AppSettings.itemsTax_bool == true)
            {
                if (!item.isTaxExempt)
                    vatRatio = (decimal)AppSettings.itemsTax_decimal;
            }

            billDetails.Add(new BillDetails()
            {
                ID = _SequenceNum,
                Product = item.name,
                itemId = item.itemId,
                Unit = unitName,
                itemUnitId = itemUnitId,
                Count = 1,
                Price = price,
                Total = total,
                isTaxExempt = item.isTaxExempt,
                //Tax = tax,
                offerId = offerId,
                OfferType = offerType,
                OfferValue = offerValue,
                OfferName = item.offerName,
                basicPrice = basicPrice,
                VATRatio = vatRatio,
            });
            _Sum += total;
        }
        #endregion billdetails
        private async void Btn_draft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    string invoiceType = "ord, ors";
                    int duration = 2;
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_invoice w = new wd_invoice();

                    w.invoiceType = invoiceType; //draft order
                    w.userId = MainWindow.userLogin.userId;
                    w.duration = duration; // view drafts which updated during 2 last days 
                    w.title = MainWindow.resourcemanager.GetString("trDrafts");

                    if (w.ShowDialog() == true)
                    {
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _InvoiceType = invoice.invType;
                            _invoiceId = invoice.invoiceId;
                            isFromReport = false;
                            archived = false;
                            setNotifications();
                            refreshDocCount(invoice.invoiceId);
                            invoices = FillCombo.invoices;
                            //invoices = await invoice.GetInvoicesByCreator(invoiceType, MainWindow.userID.Value, duration);

                            // set title to bill
                            if (_InvoiceType == "ord")
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrderDraft");
                            else
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrderSaved");
                            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
                            await fillInvoiceInputs(invoice);
                            navigateBtnActivate();
                        }
                    }
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

        public async Task fillInvoiceInputs(Invoice invoice)
        {
            txt_invNumber.Text = invoice.invNumber;

            _Sum = (decimal)invoice.total;
            if (invoice.tax != null)
                _Tax = (decimal)invoice.tax;
            cb_branch.SelectedValue = invoice.branchId;
            cb_customer.SelectedValue = invoice.agentId;
            _DeliveryCost = invoice.shippingCost;
            _RealDeliveryCost = invoice.realShippingCost;
            #region slice
            if (invoice.sliceId != null)
                cb_sliceId.SelectedValue = invoice.sliceId;
            #endregion
            if (invoice.totalNet != null)
            {
                if ((decimal)invoice.totalNet != 0)
                    tb_total.Text = SectionData.DecTostring((decimal)invoice.totalNet);
                else
                    tb_total.Text = "0";
            }
            if (invoice.tax != 0)
                tb_taxValue.Text = SectionData.PercentageDecTostring(invoice.tax);
            else
                tb_taxValue.Text = "0";
            tb_note.Text = invoice.notes;
            if (invoice.total != 0)
                tb_sum.Text = SectionData.DecTostring(invoice.total);
            else
                tb_sum.Text = "0";

            cb_company.SelectedValue = invoice.shippingCompanyId;
            cb_user.SelectedValue = invoice.shipUserId;

            if (invoice.manualDiscountValue != 0)
                tb_discount.Text = SectionData.PercentageDecTostring(invoice.manualDiscountValue);
            else
                tb_discount.Text = "0";

            if (invoice.manualDiscountType == "1")
                cb_typeDiscount.SelectedIndex = 1;
            else if (invoice.manualDiscountType == "2")
                cb_typeDiscount.SelectedIndex = 2;

            tgl_ActiveOffer.IsChecked = invoice.isApproved == 1 ? true : false;
            chk_onDelivery.IsChecked = invoice.isPrePaid.Equals(0) ? true : false;
            chk_isFreeDelivery.IsChecked = invoice.isFreeShip.Equals(1) ? true : false;

            tb_barcode.Clear();
            tb_barcode.Focus();
            await getInvoiceCoupons(invoice.invoiceId);
            // build invoice details grid
            await buildInvoiceDetails();
            _InvoiceType = invoice.invType;
            inputEditable();
            refreshTotalValue();
        }
        private async Task getInvoiceCoupons(int invoiceId)
        {
            couponsLst = new List<Coupon>();
            lst_coupons.ItemsSource = null;

            if (_InvoiceType != "ord")
                selectedCoupons = await invoiceModel.GetInvoiceCoupons(invoiceId);
            else
                selectedCoupons = await invoiceModel.getOriginalCoupons(invoiceId);
            foreach (CouponInvoice invCoupon in selectedCoupons)
            {
                var cop = coupons.Where(c => c.cId == invCoupon.couponId).FirstOrDefault();
                couponsLst.Add(cop);
                lst_coupons.ItemsSource = couponsLst;
                //lst_coupons.Items.Add(invCoupon);
            }
        }
        private async Task buildInvoiceDetails()
        {
            //get invoice items
            if (invoice.invoiceItems == null)
                invoiceItems = await invoiceModel.GetInvoicesItems(invoice.invoiceId);
            else
                invoiceItems = invoice.invoiceItems;
            // build invoice details grid
            _SequenceNum = 0;
            billDetails.Clear();
            foreach (ItemTransfer itemT in invoiceItems)
            {
                _SequenceNum++;
                decimal total = (decimal)(itemT.price * itemT.quantity);
                billDetails.Add(new BillDetails()
                {
                    ID = _SequenceNum,
                    Product = itemT.itemName,
                    itemId = (int)itemT.itemId,
                    Unit = itemT.itemUnitId.ToString(),
                    itemUnitId = (int)itemT.itemUnitId,
                    Count = (int)itemT.quantity,
                    Price = (decimal)itemT.price,
                    Total = total,
                    offerId = itemT.offerId,
                    OfferType = itemT.offerType.ToString(),
                    OfferValue = (decimal)itemT.offerValue,
                    OfferName = itemT.offerName,
                    basicPrice = (decimal)itemT.itemUnitPrice,
                    isTaxExempt = itemT.isTaxExempt,
                    VATRatio = itemT.VATRatio,
                    Tax = (decimal)itemT.itemTax,
                });
            }

            tb_barcode.Focus();

            refrishBillDetails();
        }


        private async void Btn_orders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") ||
                    MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") ||
                    SectionData.isAdminPermision())
                {
                    string invoiceType = "or";
                    int duration = 0;
                    Window.GetWindow(this).Opacity = 0.2;
                    saveBeforeExit();
                    wd_invoice w = new wd_invoice();

                    // quontations invoices
                    w.invoiceType = invoiceType;
                    w.userId = MainWindow.userLogin.userId;
                    w.branchCreatorId = MainWindow.loginBranch.branchId;
                    w.duration = duration; // view orders which updated during 1 last days 
                    w.condition = "orders";
                    w.fromOrder = true;
                    w.title = MainWindow.resourcemanager.GetString("trOrders");

                    if (w.ShowDialog() == true)
                    {
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _InvoiceType = invoice.invType;
                            _invoiceId = invoice.invoiceId;
                            isFromReport = false;
                            archived = false;
                            await fillInvoiceInputs(invoice);

                            setNotifications();
                            refreshDocCount(invoice.invoiceId);
                            invoices = FillCombo.invoices;
                            // invoices = await invoice.GetInvoicesByCreator(invoiceType,MainWindow.userID.Value,duration);
                            navigateBtnActivate();
                            // set title to bill
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrder");
                            btn_save.Content = MainWindow.resourcemanager.GetString("trSubmit");

                        }
                    }
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

        private async void Btn_waitConfirmUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(deliveryPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    string invoiceType = "s";
                    //string invoiceStatus = "ex";
                    string invoiceStatus = "Ready";
                    saveBeforeExit();
                    wd_invoice w = new wd_invoice();

                    // quontations invoices
                    w.invoiceType = invoiceType;
                    w.invoiceStatus = invoiceStatus;
                    w.userId = MainWindow.userLogin.userId;

                    w.title = MainWindow.resourcemanager.GetString("trWaitConfirmUser");

                    if (w.ShowDialog() == true)
                    {
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _InvoiceType = invoice.invType;
                            _invoiceId = invoice.invoiceId;
                            isFromReport = false;
                            archived = false;
                            setNotifications();
                            refreshDocCount(invoice.invoiceId);
                            await fillInvoiceInputs(invoice);
                            //invoices = await invoice.getDeliverOrders(invoiceType, invoiceStatus, MainWindow.userLogin.userId);
                            navigateBtnActivate();
                            // set title to bill
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrder");
                            await fillInvoiceInputs(invoice);
                        }
                    }
                    //  (((((((this.Parent as Grid).Parent as Grid).Parent as UserControl)).Parent as Grid).Parent as Grid).Parent as Window).Opacity = 1;
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

        private async void Cbm_unitItemDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cmb = sender as ComboBox;
                TextBlock tb;

                if (dg_billDetails.SelectedIndex != -1 && cmb != null)
                {
                    billDetails[dg_billDetails.SelectedIndex].itemUnitId = (int)cmb.SelectedValue;
                    if (tgl_ActiveOffer.IsChecked == true)
                        cmb.IsEnabled = false;
                    else
                        cmb.IsEnabled = true;

                    int _datagridSelectedIndex = dg_billDetails.SelectedIndex;
                    var iu = (ItemUnit)cmb.SelectedItem;

                    int itemUnitId = (int)cmb.SelectedValue;
                    billDetails[_datagridSelectedIndex].itemUnitId = (int)cmb.SelectedValue;

                    var unit = MainWindow.InvoiceGlobalSaleUnitsList.Find(x => x.itemUnitId == (int)cmb.SelectedValue && x.itemId == billDetails[_datagridSelectedIndex].itemId);

                    billDetails[_datagridSelectedIndex].OfferType = "1";
                    billDetails[_datagridSelectedIndex].OfferValue = 0;
                    billDetails[_datagridSelectedIndex].offerId = null;
                    billDetails[_datagridSelectedIndex].OfferName = "";
                    billDetails[_datagridSelectedIndex].Unit = iu.mainUnit;


                    if (unit.offerId != null && (int)unit.offerId != 0)
                    {
                        billDetails[_datagridSelectedIndex].OfferType = unit.discountType;
                        billDetails[_datagridSelectedIndex].OfferValue = (decimal)unit.discountValue;
                        billDetails[_datagridSelectedIndex].offerId = unit.offerId;
                        billDetails[_datagridSelectedIndex].OfferName = unit.offerName;
                    }

                    int oldCount = 0;
                    long newCount = 0;
                    decimal oldPrice = 0;
                    decimal itemTax = 0;
                    decimal price = 0;
                    #region change unit price
                    if (unit.SalesPrices == null || (unit.SalesPrices != null && unit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                    {
                        //if (AppSettings.itemsTax_bool == true)
                        //    price = (decimal)unit.priceTax;
                        //else
                            price = (decimal)unit.price;
                    }
                    else
                    {
                        var slice = unit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                        if (slice != null)
                        {
                            //if (AppSettings.itemsTax_bool == true)
                            //    price = (decimal)slice.priceTax;
                            //else
                                price = (decimal)slice.price;
                        }

                    }
                    #endregion
                    if (item.taxes != null)
                        itemTax = (decimal)item.taxes;
                    decimal newPrice = price;


                    oldCount = billDetails[_datagridSelectedIndex].Count;
                    oldPrice = billDetails[_datagridSelectedIndex].Price;

                    newCount = oldCount;

                    decimal total = oldPrice * oldCount;
                    _Sum -= total;


                    // new total for changed item
                    total = newCount * newPrice;
                    _Sum += total;

                    //refresh Price cell
                    tb = dg_billDetails.Columns[5].GetCellContent(dg_billDetails.Items[_datagridSelectedIndex]) as TextBlock;
                    tb.Text = newPrice.ToString();
                    //refresh total cell
                    tb = dg_billDetails.Columns[6].GetCellContent(dg_billDetails.Items[_datagridSelectedIndex]) as TextBlock;
                    tb.Text = total.ToString();

                    //  refresh sum and total text box
                    refreshTotalValue();

                    // update item in billdetails           
                    billDetails[_datagridSelectedIndex].Count = (int)newCount;
                    billDetails[_datagridSelectedIndex].Price = newPrice;
                    billDetails[_datagridSelectedIndex].Total = total;
                    //refrishBillDetails();
                }
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cbm_unitItemDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //billDetails
                if (billDetails.Count == 1)
                {
                    var cmb = sender as ComboBox;
                    cmb.SelectedValue = (int)billDetails[0].itemUnitId;
                    if (tgl_ActiveOffer.IsChecked == true)
                        cmb.IsEnabled = false;
                    else
                        cmb.IsEnabled = true;

                }
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
        private void DataGrid_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //billDetails
                int count = 0;
                foreach (var item in billDetails)
                {
                    if (dg_billDetails.Items.Count != 0)
                    {
                        if (dg_billDetails.Items.Count > 1)
                        {
                            DataGridCell cell = null;
                            try
                            {
                                cell = DataGridHelper.GetCell(dg_billDetails, count, 3);
                            }
                            catch
                            { }
                            if (cell != null)
                            {
                                var cp = (ContentPresenter)cell.Content;
                                var combo = (ComboBox)cp.ContentTemplate.FindName("cbm_unitItemDetails", cp);
                                //var combo = (combo)cell.Content;
                                combo.SelectedValue = (int)item.itemUnitId;
                                if (tgl_ActiveOffer.IsChecked == true)
                                    combo.IsEnabled = false;
                                else
                                    combo.IsEnabled = true;
                            }
                        }
                    }
                    count++;
                }
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
        private void Dg_billDetails_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

            if (dg_billDetails.SelectedIndex != -1)
                if (tgl_ActiveOffer.IsChecked == true)
                    e.Cancel = true;
        }
        private void Dg_billDetails_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);
                TextBlock tb;
                TextBox t = e.EditingElement as TextBox;  // Assumes columns are all TextBoxes
                var columnName = e.Column.Header.ToString();

                BillDetails row = e.Row.Item as BillDetails;
                int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == row.itemUnitId).FirstOrDefault());

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds < 100)
                {
                    if (columnName == MainWindow.resourcemanager.GetString("trQTR"))
                        t.Text = billDetails[index].Count.ToString();
                    else if (columnName == MainWindow.resourcemanager.GetString("trPrice"))
                        t.Text = billDetails[index].Price.ToString();
                }
                else
                {
                    int oldCount = 0;
                    long newCount = 0;
                    decimal oldPrice = 0;
                    decimal newPrice = 0;

                    //"tb_amont"
                    if (columnName == MainWindow.resourcemanager.GetString("trQTR"))
                        newCount = int.Parse(t.Text);
                    else
                        newCount = row.Count;
                    if (newCount < 0)
                    {
                        newCount = 0;
                        t.Text = "0";
                    }

                    oldCount = row.Count;


                    if (columnName == MainWindow.resourcemanager.GetString("trPrice"))
                        newPrice = decimal.Parse(t.Text);
                    else
                        newPrice = row.Price;
                    if (newPrice < 0)
                    {
                        newPrice = 0;
                        t.Text = "0";
                    }

                    oldPrice = row.Price;

                    // old total for changed item
                    decimal total = oldPrice * oldCount;
                    _Sum -= total;


                    // new total for changed item
                    total = newCount * newPrice;
                    _Sum += total;

                    //decimal itemTax = 0;
                    //if (item.taxes != null)
                        //itemTax = (decimal)item.taxes;

                    //refresh total cell
                    tb = dg_billDetails.Columns[6].GetCellContent(dg_billDetails.Items[index]) as TextBlock;
                    tb.Text = total.ToString();

                    //  refresh sum and total text box
                    refreshTotalValue();

                    // update item in billdetails           
                    billDetails[index].Count = (int)newCount;
                    billDetails[index].Price = newPrice;
                    billDetails[index].Total = total;
                }
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                //refrishDataGridItems();
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_invoiceImages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") ||
                    MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") ||
                    SectionData.isAdminPermision())
                {
                    if (invoice != null && invoice.invoiceId != 0)
                    {
                        //  (((((((this.Parent as Grid).Parent as Grid).Parent as UserControl)).Parent as Grid).Parent as Grid).Parent as Window).Opacity = 0.2;

                        wd_uploadImage w = new wd_uploadImage();

                        w.tableName = "invoices";
                        w.tableId = invoice.invoiceId;
                        w.docNum = invoice.invNumber;
                        w.ShowDialog();
                        // (((((((this.Parent as Grid).Parent as Grid).Parent as UserControl)).Parent as Grid).Parent as Grid).Parent as Window).Opacity =1;
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trChooseInvoiceToolTip"), animation: ToasterAnimation.FadeIn);
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

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") ||
                        SectionData.isAdminPermision())
                {
                    //check mandatory inputs
                    bool valid = validateInvoiceValues();
                    if (valid)
                    {
                        if (tgl_ActiveOffer.IsChecked == true)
                            _InvoiceType = "or";
                        else
                            _InvoiceType = "ors";

                        await addInvoice(_InvoiceType);

                        if (_InvoiceType == "or")
                            await clearInvoice();
                        else
                        {
                            txt_invNumber.Text = invoice.invNumber;
                            inputEditable();
                        }

                        setNotifications();

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

        private void Cb_customer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0"))
                {
                    _SelectedCustomer = (int)cb_customer.SelectedValue;
                    if (_InvoiceType == "ord")
                        btn_updateCustomer.IsEnabled = true;
                }
                else
                {
                    cb_customer.SelectedValue = _SelectedCustomer;
                }
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



        private void Dp_desrvedDate_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                _Sender = sender;

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
        private async void Btn_items_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    //items
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_items w = new wd_items();
                    w.CardType = "order";
                    w.branchId = (int)cb_branch.SelectedValue;
                    w.items = items;
                    w.ShowDialog();
                    if (w.isActive)
                    {
                        for (int i = 0; i < w.selectedItems.Count; i++)
                        {
                            int itemId = w.selectedItems[i];
                            await ChangeItemIdEvent(itemId);
                        }
                        refreshTotalValue();
                        refrishBillDetails();
                        items = w.items;
                    }

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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.KeyDown -= HandleKeyPress;

                lst_coupons.ItemsSource = couponsLst;
                lst_coupons.DisplayMemberPath = "notes";
                lst_coupons.SelectedValuePath = "cId";

                saveBeforeExit();
                timer.Stop();

                try
                {
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        private async void saveBeforeExit()
        {
            if (billDetails.Count > 0 && _InvoiceType == "ord")
            {
                //#region Accept
                //MainWindow.mainWindow.Opacity = 0.2;
                //wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                //w.contentText = MainWindow.resourcemanager.GetString("trSaveOrderNotification");

                //w.ShowDialog();
                //MainWindow.mainWindow.Opacity = 1;
                //#endregion
                //if (w.isOk  )
                Btn_newDraft_Click(null, null);
                //else
                //{
                //    await clearInvoice();
                //    setNotifications();
                //}
            }
            else
            {
                await clearInvoice();
                setNotifications();
            }
        }
        private void Cb_company_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && (cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() != "0"))
                {
                    sp_PrePaid.IsEnabled = true;
                    #region free delivery according to settings
                    if (AppSettings.freeDelivery)
                        sp_isFreeDelivery.IsEnabled = true;
                    #endregion
                    _SelectedCompany = (int)cb_company.SelectedValue;
                    companyModel = FillCombo.shippingCompaniesList.Find(c => c.shippingCompanyId == (int)cb_company.SelectedValue);
                    _DeliveryCost = (decimal)companyModel.deliveryCost;
                    _RealDeliveryCost = (decimal)companyModel.RealDeliveryCost;
                    refreshTotalValue();

                    if (companyModel.deliveryType == "local")
                    {
                        cb_user.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        cb_user.SelectedIndex = -1;
                        cb_user.Visibility = Visibility.Collapsed;
                    }
                }
                else if (cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() == "0")
                {
                    companyModel = new ShippingCompanies();
                    cb_company.SelectedItem = "";
                    cb_user.SelectedIndex = -1;
                    _DeliveryCost = 0;
                    _RealDeliveryCost = 0;
                    sp_PrePaid.IsEnabled = false;
                    sp_isFreeDelivery.IsEnabled = false;
                    chk_onDelivery.IsChecked = false;
                    chk_isFreeDelivery.IsChecked = false;
                }
                else
                    cb_company.SelectedValue = _SelectedCompany;

                tb_deliveryCost.Text = SectionData.PercentageDecTostring(_DeliveryCost);
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




        private void Tb_note_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _Sender = sender;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0"))
                {
                    _SelectedUser = (int)cb_user.SelectedValue;
                }
                else if (cb_customer.SelectedIndex == -1)
                    cb_user.SelectedItem = "";
                else
                {
                    cb_user.SelectedValue = _SelectedUser;
                }
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




        #region print
        private async void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////
                Thread t1 = new Thread(async () =>
                {
                    string msg = "";

                    msg = await printInvoice(invoice, invoiceItems);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (msg == "")
                        {

                        }
                        else
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString(msg), animation: ToasterAnimation.FadeIn);

                        }
                    });
                });
                t1.Start();
                //////////////////////////
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


        public async Task<string> printInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            string msg = "";
            // prInvoice = new Invoice();
            //  prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);

            if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
            {
                //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                msg = "trPrintDraftInvoice";
            }
            else
            {

                List<ReportParameter> paramarr = new List<ReportParameter>();


                if (prInvoice.invoiceId > 0)
                {
                    //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);


                    //user
                    User employ = new User();
                    //  employ = await userModel.getUserById((int)prInvoice.updateUserId);
                    if (FillCombo.usersAllList is null)
                    { await FillCombo.RefreshAllUsers(); }
                    employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                    prInvoice.uuserName = employ.name;
                    prInvoice.uuserLast = employ.lastname;
                    //agent

                    if (prInvoice.agentId != null)
                    {
                        Agent agentinv = new Agent();

                        //  agentinv = customers.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                        if (FillCombo.agentsList is null)
                        { await FillCombo.RefreshAgents(); }
                        agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                        prInvoice.agentCode = agentinv.code;
                        //new lines
                        prInvoice.agentName = agentinv.name;
                        prInvoice.agentCompany = agentinv.company;

                    }
                    else
                    {
                        prInvoice.agentCode = "-";
                        prInvoice.agentName = "-";
                        prInvoice.agentCompany = "-";
                    }
                    //branch
                    Branch branch = new Branch();
                    //   branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                    if (FillCombo.branchsAllList is null)
                    { await FillCombo.RefreshBranchsAll(); }
                    branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                    if (branch.branchId > 0)
                    {
                        prInvoice.branchName = branch.name;
                    }
                    //shipping
                    ShippingCompanies shippingcom = new ShippingCompanies();
                    if (prInvoice.shippingCompanyId > 0)
                    {
                        //  shippingcom = await shippingcom.GetByID((int)prInvoice.shippingCompanyId);
                        if (FillCombo.shippingCompaniesAllList is null)
                        { await FillCombo.RefreshShippingCompaniesAll(); }
                        shippingcom = FillCombo.shippingCompaniesAllList.Where(s => s.shippingCompanyId == (int)prInvoice.shippingCompanyId).FirstOrDefault();

                    }
                    User shipuser = new User();
                    if (prInvoice.shipUserId > 0)
                    {
                        //shipuser = await userModel.getUserById((int)prInvoice.shipUserId);
                        if (FillCombo.usersAllList is null)
                        { await FillCombo.RefreshAllUsers(); }
                        shipuser = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.shipUserId).FirstOrDefault();

                    }
                    prInvoice.shipUserName = shipuser.name + " " + shipuser.lastname;
                    //end shipping
                    ReportCls.checkInvLang();

                    foreach (var i in invoiceItems)
                    {
                        i.price = decimal.Parse(SectionData.DecTostring(i.price));
                        i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                    }
                    reportSize repsize = new reportSize();
                    int itemscount = 0;

                    clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                    itemscount = invoiceItems.Count();
                    ///printer
                    clsReports clsrep = new clsReports();
                    reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                    repsize.paperSize = repsset.paperSize;
                    repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, 0, itemscount, repsize.paperSize);
                    repsize.printerName = repsset.printerName;
                    //end
                    //repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, 0, itemscount,);
                    string reppath = repsize.reppath;
                    rep.ReportPath = reppath;

                    // clsReports.purchaseInvoiceReport(newl, rep, reppath);
                    clsReports.setInvoiceLanguage(paramarr);
                    clsReports.InvoiceHeader(paramarr);
                    paramarr = reportclass.fillSaleInvReport(prInvoice, paramarr, shippingcom);
                    rep = reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                    if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
|| prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
|| prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                    {

                        paramarr.Add(new ReportParameter("isSaved", "n"));
                    }
                    else
                    {

                        paramarr.Add(new ReportParameter("isSaved", "y"));
                    }
                    List<PayedInvclass> payedList = new List<PayedInvclass>();
                    rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));
                    rep.SetParameters(paramarr);
                    rep.Refresh();
                    //this.Dispatcher.Invoke(() =>
                    //{

                    // LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
                    if (repsize.printerName == "")
                    {
                        if (AppSettings.sale_printer_name == "")
                        {
                            repsize.printerName = SectionData.getdefaultPrinters();
                        }
                        else
                        {
                            repsize.printerName = AppSettings.sale_printer_name;
                        }

                    }

                    if (repsize.paperSize == "A4")
                    {

                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, 1);

                    }
                    else
                    {
                        LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, 1, repsize.width, repsize.height);

                    }

                    //});
                }
                else
                {

                    //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintEmptyInvoice"), animation: ToasterAnimation.FadeIn);
                    msg = "trPrintEmptyInvoice";
                }
            }
            return msg;
        }
        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    prInvoice = invoice;
                    if (prInvoice.invoiceId > 0)
                    {
                        // prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                        Window.GetWindow(this).Opacity = 0.2;

                        List<ReportParameter> paramarr = new List<ReportParameter>();
                        string pdfpath;

                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);


                        if (prInvoice.invoiceId > 0)
                        {
                            //user
                            User employ = new User();
                            // employ = await userModel.getUserById((int)prInvoice.updateUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                            prInvoice.uuserName = employ.name;
                            prInvoice.uuserLast = employ.lastname;

                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            //agent
                            if (prInvoice.agentId != null)
                            {
                                Agent agentinv = new Agent();
                                //   agentinv = customers.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                                if (FillCombo.agentsList is null)
                                { await FillCombo.RefreshAgents(); }
                                agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                prInvoice.agentCode = agentinv.code;
                                //new lines
                                prInvoice.agentName = agentinv.name;
                                prInvoice.agentCompany = agentinv.company;
                            }
                            else
                            {
                                prInvoice.agentCode = "-";
                                prInvoice.agentName = "-";
                                prInvoice.agentCompany = "-";
                            }
                            //branch name
                            Branch branch = new Branch();
                            //   branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                            if (FillCombo.branchsAllList is null)
                            { await FillCombo.RefreshBranchsAll(); }
                            branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                            if (branch.branchId > 0)
                            {
                                prInvoice.branchName = branch.name;
                            }
                            //shipping
                            ShippingCompanies shippingcom = new ShippingCompanies();
                            if (prInvoice.shippingCompanyId > 0)
                            {
                                //shippingcom = await shippingcom.GetByID((int)prInvoice.shippingCompanyId);
                                if (FillCombo.shippingCompaniesAllList is null)
                                { await FillCombo.RefreshShippingCompaniesAll(); }
                                shippingcom = FillCombo.shippingCompaniesAllList.Where(s => s.shippingCompanyId == (int)prInvoice.shippingCompanyId).FirstOrDefault();

                            }
                            //ship user
                            User shipuser = new User();
                            if (prInvoice.shipUserId > 0)
                            {
                                //   shipuser = await userModel.getUserById((int)prInvoice.shipUserId);
                                if (FillCombo.usersAllList is null)
                                { await FillCombo.RefreshAllUsers(); }
                                shipuser = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.shipUserId).FirstOrDefault();

                            }
                            prInvoice.shipUserName = shipuser.name + " " + shipuser.lastname;
                            //end shipping
                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            ReportCls.checkInvLang();
                            foreach (var i in invoiceItems)
                            {
                                i.price = decimal.Parse(SectionData.DecTostring(i.price));
                                i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                            }
                            reportSize repsize = new reportSize();
                            int itemscount = 0;
                            clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            itemscount = invoiceItems.Count();
                            //repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, itemscount);
                            ///printer
                               clsReports clsrep = new clsReports();
                            reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                            // rs = reportclass.GetpayInvoiceRdlcpath(prInvoice, AppSettings.salePaperSize, itemscount, rep);
                            repsize.paperSize = repsset.paperSize;
                            repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, 0, itemscount, repsize.paperSize);
                            repsize.printerName = repsset.printerName;
                            //end     

                            string reppath = repsize.reppath;
                            rep.ReportPath = reppath;
                            clsReports.setInvoiceLanguage(paramarr);
                            clsReports.InvoiceHeader(paramarr);
                            paramarr = reportclass.fillSaleInvReport(prInvoice, paramarr, shippingcom);
                            rep = reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                            if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
|| prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
|| prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                            {

                                paramarr.Add(new ReportParameter("isSaved", "n"));
                            }
                            else
                            {

                                paramarr.Add(new ReportParameter("isSaved", "y"));
                            }
                            List<Serial> srl = new List<Serial>();
                            List<PayedInvclass> repPayedList = new List<PayedInvclass>();
                          rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));
                        //    rep.DataSources.Add(new ReportDataSource("DataSetSerial", srl));
                            
                            rep.SetParameters(paramarr);
                            rep.Refresh();

                            if (repsize.paperSize != "A4")
                            {
                                LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                            }
                            else
                            {
                                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                            }

                            // LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        }

                        wd_previewPdf w = new wd_previewPdf();
                        w.pdfPath = pdfpath;
                        if (!string.IsNullOrEmpty(w.pdfPath))
                        {
                            w.ShowDialog();

                            w.wb_pdfWebViewer.Dispose();
                        }
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: "", animation: ToasterAnimation.FadeIn);
                        Window.GetWindow(this).Opacity = 1;

                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSaveInvoiceToPreview"), animation: ToasterAnimation.FadeIn);
                    }
                    #endregion
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

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                ///////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    pdfPurInvoice(invoice, invoiceItems);
                });
                t1.Start();
                //////////////////////////////
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

        public async void pdfPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            if (prInvoice.invoiceId > 0)
            {
                // prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                             || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                             || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                }
                else
                {

                    List<ReportParameter> paramarr = new List<ReportParameter>();

                    if (prInvoice.invoiceId > 0)
                    {
                        // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        //user
                        User employ = new User();
                        //   employ = await userModel.getUserById((int)prInvoice.updateUserId);
                        if (FillCombo.usersAllList is null)
                        { await FillCombo.RefreshAllUsers(); }
                        employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                        prInvoice.uuserName = employ.name;
                        prInvoice.uuserLast = employ.lastname;
                        //  agentinv = customers.Where(X => X.agentId == invoice.agentId).FirstOrDefault();

                        //  invoice.agentCode = agentinv.code;
                        //agent
                        if (prInvoice.agentId != null)
                        {
                            Agent agentinv = new Agent();
                            //agentinv = customers.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                            if (FillCombo.agentsList is null)
                            { await FillCombo.RefreshAgents(); }
                            agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();


                            prInvoice.agentCode = agentinv.code;
                            //new lines
                            prInvoice.agentName = agentinv.name;
                            prInvoice.agentCompany = agentinv.company;
                        }
                        else
                        {
                            prInvoice.agentCode = "-";
                            prInvoice.agentName = "-";
                            prInvoice.agentCompany = "-";
                        }

                        ReportCls.checkInvLang();
                        //branch
                        Branch branch = new Branch();
                        // branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                        if (FillCombo.branchsAllList is null)
                        { await FillCombo.RefreshBranchsAll(); }
                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                        if (branch.branchId > 0)
                        {
                            prInvoice.branchName = branch.name;
                        }
                        //shipping
                        ShippingCompanies shippingcom = new ShippingCompanies();
                        if (prInvoice.shippingCompanyId > 0)
                        {
                            //   shippingcom = await shippingcom.GetByID((int)prInvoice.shippingCompanyId);
                            if (FillCombo.shippingCompaniesAllList is null)
                            { await FillCombo.RefreshShippingCompaniesAll(); }
                            shippingcom = FillCombo.shippingCompaniesAllList.Where(s => s.shippingCompanyId == (int)prInvoice.shippingCompanyId).FirstOrDefault();


                        }
                        User shipuser = new User();
                        if (prInvoice.shipUserId > 0)
                        {
                            //shipuser = await userModel.getUserById((int)prInvoice.shipUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            shipuser = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.shipUserId).FirstOrDefault();

                        }
                        prInvoice.shipUserName = shipuser.name + " " + shipuser.lastname;
                        //end shipping
                        foreach (var i in invoiceItems)
                        {
                            i.price = decimal.Parse(SectionData.DecTostring(i.price));
                            i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                        }
                        reportSize repsize = new reportSize();
                        int itemscount = 0;

                        clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                        itemscount = invoiceItems.Count();
                        //repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, itemscount);
                        ///printer
                        clsReports clsrep = new clsReports();
                        reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                        // rs = reportclass.GetpayInvoiceRdlcpath(prInvoice, AppSettings.salePaperSize, itemscount, rep);
                        repsize.paperSize = repsset.paperSize;
                        repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, 0, itemscount, repsize.paperSize);
                        repsize.printerName = repsset.printerName;
                        //end     

                        string reppath = repsize.reppath;
                        rep.ReportPath = reppath;
                        clsReports.setInvoiceLanguage(paramarr);
                        clsReports.InvoiceHeader(paramarr);
                        paramarr = reportclass.fillSaleInvReport(prInvoice, paramarr, shippingcom);
                        rep = reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                        if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
|| prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
|| prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                        {

                            paramarr.Add(new ReportParameter("isSaved", "n"));
                        }
                        else
                        {

                            paramarr.Add(new ReportParameter("isSaved", "y"));
                        }
                        List<PayedInvclass> payedList = new List<PayedInvclass>();
                        rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));

                        rep.SetParameters(paramarr);
                        rep.Refresh();
                        this.Dispatcher.Invoke(() =>
                        {
                            saveFileDialog.Filter = "PDF|*.pdf;";

                            if (saveFileDialog.ShowDialog() == true)
                            {

                                string filepath = saveFileDialog.FileName;
                                
                                if (repsize.paperSize != "A4")
                                {
                                    LocalReportExtensions.customExportToPDF(rep, filepath, repsize.width, repsize.height);
                                }
                                else
                                {
                                    LocalReportExtensions.ExportToPDF(rep, filepath);
                                }
                            }
                        });
                    }

                }
            }
        }


        #endregion
        private async void Btn_invoiceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (invoice != null && invoice.invoiceId != 0)
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    wd_uploadImage w = new wd_uploadImage();

                    w.tableName = "invoices";
                    w.tableId = invoice.invoiceId;
                    w.docNum = invoice.invNumber;
                    w.ShowDialog();
                    await refreshDocCount(invoice.invoiceId);
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trChooseInvoiceToolTip"), animation: ToasterAnimation.FadeIn);

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

        private void tb_discount_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                _Sender = sender;
                refreshTotalValue();
                e.Handled = true;
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

        private void Cb_typeDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && cb_typeDiscount.SelectedIndex != -1)
                {
                    _SelectedDiscountType = (int)cb_typeDiscount.SelectedValue;
                    refreshTotalValue();
                }
                else
                {
                    cb_typeDiscount.SelectedValue = _SelectedDiscountType;
                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Cb_coupon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string s = _BarcodeStr;
                //if (cb_coupon.SelectedIndex != -1)
                //{
                //    couponModel = coupons.ToList().Find(c => c.cId == (int)cb_coupon.SelectedValue);
                //    if (couponModel != null)
                //    {                       
                //        s = couponModel.barcode;
                //        await dealWithBarcode(s);       
                //    }
                //    cb_coupon.SelectedIndex = -1;
                //    cb_coupon.SelectedItem = "";
                //}
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
        private async void Btn_addCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                wd_updateVendor w = new wd_updateVendor();
                //// pass agent id to update windows
                w.agent.agentId = 0;
                w.type = "c";
                w.ShowDialog();
                if (w.isOk == true)
                {
                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    await FillCombo.RefreshCustomers();
                    RefrishCustomers();
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
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_updateCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0")
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_updateVendor w = new wd_updateVendor();
                    //// pass agent id to update windows
                    w.agent.agentId = (int)cb_customer.SelectedValue;
                    w.ShowDialog();
                    await FillCombo.RefreshCustomers();
                    RefrishCustomers();
                    Window.GetWindow(this).Opacity = 1;
                }
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

        private async void Btn_deleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(deletePermission, MainWindow.groupObjects, "one"))
                {
                    if (invoice.invoiceId != 0)
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion
                        if (w.isOk)
                        {
                            int savedBranch = (int)invoice.branchId;
                            //int res = await itemLocationModel.unlockItems(invoiceItems, savedBranch);
                            InvoiceResult invoiceResult = await invoice.deleteOrder(invoice.invoiceId, MainWindow.userID.Value);
                            if (invoiceResult.Result > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                                AppSettings.SalesOrdersDraftCount = invoiceResult.OrdersCount;
                                clearInvoice();
                                setDraftNotification(AppSettings.SalesOrdersDraftCount);
                                //refreshOrdersNotification();
                            }
                            else
                                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                var Sender = sender as Expander;

                foreach (var control in FindControls.FindVisualChildren<Expander>(this))
                {

                    var expander = control as Expander;
                    if (expander.Tag != null && Sender.Tag != null)
                        if (expander.Tag.ToString() != Sender.Tag.ToString())
                            expander.IsExpanded = false;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_clearCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                _SelectedCustomer = -1;
                cb_customer.SelectedIndex = -1;
                //dp_desrvedDate.SelectedDate = null;
                tb_note.Clear();

                btn_updateCustomer.IsEnabled = false;
                SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
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
        private void Cb_customer_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var tb = cb_customer.Template.FindName("PART_EditableTextBox", cb_customer) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                cb_customer.ItemsSource = customers.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                //cb_customer.ItemsSource = customers.Where(x => x.name.Contains(cb_customer.Text));
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
                _Sender = sender;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
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

        private void Dg_billDetails_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _IsFocused = true;
        }

        private void Btn_submitOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") ||
                    MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") ||
                    SectionData.isAdminPermision())
                {

                    wd_submitOrder w = new wd_submitOrder();
                    w.invoice = invoice;
                    w.ShowDialog();

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

        private async void Tgl_ActiveOffer_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (tgl_ActiveOffer.IsFocused)
                {
                    #region Accept
                    if ((cb_branch.SelectedValue != null && cb_branch.SelectedValue.ToString() != "0")
                        && (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0")
                        && billDetails.Count > 0)
                    {
                        bool ready = await checkOrderReady(invoice.invoiceId);
                        if (ready)
                        {
                            MainWindow.mainWindow.Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trApproveOrderNotification");

                            w.ShowDialog();
                            if (!w.isOk)
                            {
                                tgl_ActiveOffer.IsChecked = false;
                                _InvoiceType = "ord";
                            }
                            else
                            {
                                _InvoiceType = "or";
                                ///lock grid
                                btn_save.Content = MainWindow.resourcemanager.GetString("trSubmit");
                            }
                            MainWindow.mainWindow.Opacity = 1;

                        }
                        else
                        {
                            tgl_ActiveOffer.IsChecked = false;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                    #endregion
                    else if (billDetails.Count == 0)
                    {
                        tgl_ActiveOffer.IsChecked = false;
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trAddInvoiceWithoutItems"), animation: ToasterAnimation.FadeIn);
                    }
                    else if (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() == "0")
                    {
                        tgl_ActiveOffer.IsChecked = false;
                        exp_customer.IsExpanded = true;
                        SectionData.validateEmptyComboBox(cb_customer, p_errorCustomer, tt_errorCustomer, "trEmptyCustomerToolTip");
                    }
                    else if (cb_branch.SelectedValue != null && cb_branch.SelectedValue.ToString() == "0")
                    {
                        tgl_ActiveOffer.IsChecked = false;
                        exp_store.IsExpanded = true;
                        SectionData.validateEmptyComboBox(cb_branch, p_errorBranch, tt_errorBranch, "trEmptyBranchToolTip");
                    }
                    inputEditable();
                }

                if (tgl_ActiveOffer.IsChecked == true)
                {
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                }
                else
                {
                    dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                }
                refrishDataGridItems();
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

        private void Tgl_ActiveOffer_Unchecked(object sender, RoutedEventArgs e)
        {
            //_InvoiceType = "ord";
            if (tgl_ActiveOffer.IsFocused)
            {
                _InvoiceType = invoice.invType;
                btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

                if (tgl_ActiveOffer.IsChecked == true)
                {
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                }
                else
                {
                    dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                }
                inputEditable();
                refrishDataGridItems();
            }
        }
        private async Task<bool> checkOrderReady(int invoiceId)
        {
            bool ready = false;
            int res = (int)await invoice.checkOrderRedeaniss(invoiceId);
            if (res == 1)
                ready = true;
            //foreach(BillDetails item in billDetails)
            //{
            //    int availableAmount = await getAvailableAmount(item.itemId, item.itemUnitId, (int)cb_branch.SelectedValue, item.ID);
            //    if (availableAmount < item.Count)
            //        return false;
            //}
            return ready;
        }
        private async Task<decimal> getAvailableAmount(int itemId, int itemUnitId, int branchId, int ID)
        {
            // var itemUnits = await itemUnitModel.GetItemUnits(itemId);
            var itemUnits = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemId == item.itemId).ToList();
            int availableAmount = (int)await itemLocationModel.getAmountInBranch(itemUnitId, branchId);
            var smallUnits = await itemUnitModel.getSmallItemUnits(itemId, itemUnitId);
            foreach (Item u in itemUnits)
            {
                var isInBill = billDetails.ToList().Find(x => x.itemUnitId == (int)u.itemUnitId && x.ID != ID); // unit exist in invoice
                if (isInBill != null)
                {
                    var isSmall = smallUnits.Find(x => x.itemUnitId == (int)u.itemUnitId);
                    int unitValue = 0;

                    int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == u.itemUnitId).FirstOrDefault());
                    int quantity = billDetails[index].Count;
                    if (itemUnitId == u.itemUnitId)
                    { }
                    else if (isSmall != null) // from-unit is bigger than to-unit
                    {
                        unitValue = (int)await itemUnitModel.largeToSmallUnitQuan(itemUnitId, (int)u.itemUnitId);
                        quantity = quantity / unitValue;
                    }
                    else
                    {
                        unitValue = (int)await itemUnitModel.smallToLargeUnit(itemUnitId, (int)u.itemUnitId);

                        if (unitValue != 0)
                        {
                            quantity = quantity * unitValue;
                        }
                    }
                    availableAmount -= quantity;
                }
            }
            return availableAmount;
        }

        private void Lst_coupons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {//double click
            try
            {
                if (lst_coupons.SelectedItem != null)
                {
                    //couponModel = coupons.ToList().Find(c => c.name == lst_coupons.SelectedItem.ToString());
                    couponModel = coupons.ToList().Find(c => c.cId == (int)lst_coupons.SelectedValue);
                    var cop = selectedCoupons.Where(x => x.couponId == couponModel.cId).FirstOrDefault();
                    couponsLst = couponsLst.Where(c => c.cId != couponModel.cId).ToList();
                    lst_coupons.ItemsSource = null;
                    lst_coupons.ItemsSource = couponsLst;
                    lst_coupons.Items.Refresh();

                    selectedCoupons.Remove(cop);
                }
            }
            catch { }
        }

        private async void Btn_enter_Click(object sender, RoutedEventArgs e)
        {//enter
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string s = _BarcodeStr;
                if (!tb_coupon.Text.Equals(""))
                {
                    couponModel = coupons.ToList().Find(c => c.code == tb_coupon.Text || c.name == tb_coupon.Text);
                    if (couponModel != null)
                    {
                        s = couponModel.barcode;
                        await dealWithBarcode(s);
                    }
                    tb_coupon.Text = "";
                }
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

        private async void Tb_coupon_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (e.Key == Key.Return)
                {
                    string s = _BarcodeStr;
                    couponModel = coupons.ToList().Find(c => c.code == tb_coupon.Text || c.barcode == tb_coupon.Text);
                    if (couponModel != null)
                    {
                        s = couponModel.barcode;
                        await dealWithBarcode(s);
                    }
                    tb_coupon.Text = "";

                }
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

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Btn_enter_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_sliceId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                AppSettings.DefaultInvoiceSlice = (int)cb_sliceId.SelectedValue;

                decimal basicPrice = 0;
                decimal price = 0;
                _Sum = 0;
                foreach (var b in billDetails)
                {
                    var it = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemId == b.itemId && a.itemUnitId == b.itemUnitId).FirstOrDefault();
                    if (it.SalesPrices == null || (it.SalesPrices != null && it.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                    {
                        //basicPrice = (decimal)it.basicPrice;
                        //if (AppSettings.itemsTax_bool == true)
                        //    price = (decimal)it.priceTax;
                        //else
                            price = (decimal)it.price;
                        basicPrice = (decimal)it.price;
                    }
                    else
                    {
                        var slice = it.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                        //basicPrice = (decimal)slice.basicPrice;
                        //if (AppSettings.itemsTax_bool == true)
                        //    price = (decimal)slice.priceTax;
                        //else
                            price = (decimal)slice.price;
                        basicPrice = (decimal)slice.price;
                    }
                    b.basicPrice = basicPrice;
                    b.Price = price;
                    b.Total = b.Price * b.Count;
                    _Sum += b.Total;
                }
                refrishDataGridItems();
                refreshTotalValue();


            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Chk_isFreeDelivery_Checked(object sender, RoutedEventArgs e)
        {
            if (chk_isFreeDelivery.IsFocused)
                refreshTotalValue();
        }
        private void Chk_isFreeDelivery_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chk_isFreeDelivery.IsFocused)
                refreshTotalValue();
        }

        private void Chk_onDelivery_Checked(object sender, RoutedEventArgs e)
        {
            //btn_save.Content = MainWindow.resourcemanager.GetString("trSubmit");

        }
        private void Chk_onDelivery_Unchecked(object sender, RoutedEventArgs e)
        {
            //btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

        }

        private void Cb_user_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = FillCombo.driversList.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_company_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;

                var shippingCompanies = FillCombo.shippingCompaniesList.ToList();
                ShippingCompanies shippingCompanie = new ShippingCompanies();
                shippingCompanie.shippingCompanyId = 0;
                shippingCompanie.name = "-";
                shippingCompanie.deliveryType = "";
                shippingCompanies.Insert(0, shippingCompanie);

                combo.ItemsSource = shippingCompanies.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_branch_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                List<Branch> branches = new List<Branch>();
                if (SectionData.isAdminPermision())
                    branches = SectionData.branchsAllList.ToList();
                else
                    branches = SectionData.BranchesByBranchandUserList.ToList();

                branches = branches.Where(b => b.type == "b" && b.branchId != 1).ToList();

                combo.ItemsSource = branches.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_sliceId_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                List<Slice> sLst = new List<Slice>();
                sLst = FillCombo.slicesList.Where(s => s.isActive == true).ToList();
                combo.ItemsSource = sLst.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
