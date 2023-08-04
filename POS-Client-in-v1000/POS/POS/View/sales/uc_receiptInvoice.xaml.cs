using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static POS.View.uc_categorie;
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Resources;
using System.ComponentModel;
using static POS.MainWindow;
//using POS.Classes;
namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_receiptInvoice.xaml
    /// </summary>
    public partial class uc_receiptInvoice : UserControl
    {
        string invoicePermission = "reciptInvoice_invoice";
        string returnPermission = "reciptInvoice_return";
        string paymentsPermission = "reciptInvoice_payments";
        string executeOrderPermission = "reciptInvoice_executeOrder";
        string quotationPermission = "reciptInvoice_quotation";
        string sendEmailPermission = "reciptInvoice_sendEmail";
        string printCountPermission = "reciptInvoice_printCount";

        private static uc_receiptInvoice _instance;
        public static uc_receiptInvoice Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_receiptInvoice();
                return _instance;
            }
        }
        public uc_receiptInvoice()
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

        ObservableCollection<BillDetails> billDetails = new ObservableCollection<BillDetails>();

        public static bool isFromReport = false;
        public static bool archived = false;
        Item itemModel = new Item();
        Item item = new Item();
        IEnumerable<Item> items;
        Card cardModel = new Card();
        //IEnumerable<Card> cards;
        Agent agentModel = new Agent();
        IEnumerable<Agent> customers;
        ItemUnit itemUnitModel = new ItemUnit();
        List<ItemUnit> barcodesList;
        List<Item> itemUnits;
        Invoice invoiceModel = new Invoice();
        List<Invoice> invoices;
        public Invoice invoice = new Invoice();
        Coupon couponModel = new Coupon();
        IEnumerable<Coupon> coupons;
        List<Coupon> couponsLst = new List<Coupon>();
        List<CouponInvoice> selectedCoupons = new List<CouponInvoice>();
        Branch branchModel = new Branch();
        Pos posModel = new Pos();
        Pos pos;
        List<ItemTransfer> invoiceItems;
        List<ItemTransfer> mainInvoiceItems;
        CashTransfer cashTransfer = new CashTransfer();
        ItemLocation itemLocationModel = new ItemLocation();
        ShippingCompanies companyModel = new ShippingCompanies();
        List<ShippingCompanies> companies;
        User userModel = new User();
        List<User> users;
        private static DispatcherTimer timer;
        #region//to handle barcode characters
        static private int _SelectedCustomer = -1;
        static private int _SelectedDiscountType = -1;

        // for barcode
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private object _Sender;
        bool _IsFocused = false;
        #endregion

        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        public List<Control> controls;
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;

        //for bill details
        static private int _SequenceNum = 0;
        static private int _invoiceId;
        static private decimal _Sum = 0;
        static private decimal _Tax = 0;
        static private decimal _Discount = 0;
        static private decimal _DeliveryCost = 0;
        static private decimal _RealDeliveryCost = 0;
        static public string _InvoiceType = "sd"; // sale draft

        // for report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        ItemUnitOffer offer = new ItemUnitOffer();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
      //  public static int width;
        public static int itemscount;
      //  public static int height;
        Invoice prInvoice = new Invoice();
        int prinvoiceId = 0;
        List<PayedInvclass> mailpayedList = new List<PayedInvclass>();
        //shipping
        ShippingCompanies shippingcomp = new ShippingCompanies();
        User shipinguser = new User();

        //bool isClose = false;

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
            public decimal OfferValue { get; set; }
            public string OfferType { get; set; }
            public string OfferName { get; set; }
            public List<Serial> itemSerials { get; set; }
            public List<Serial> returnedSerials { get; set; }
            public List<StoreProperty> ItemProperties { get; set; }
            public List<StoreProperty> StoreProperties { get; set; }
            public List<StoreProperty> ReturnedProperties { get; set; }
            public bool valid { get; set; }
            public string type { get; set; }
           // public decimal basicPrice { get; set; }
            public List<Item> packageItems { get; set; }

            public Nullable<int> warrantyId { get; set; }
            public string warrantyName { get; set; }
            public string warrantyDescription { get; set; }
            public int basicItemUnitId { get; set; }

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
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoice");
            txt_coupon.Text = MainWindow.resourcemanager.GetString("trCoupon");
            txt_customer.Text = MainWindow.resourcemanager.GetString("trCustomer");
            txt_delivery.Text = MainWindow.resourcemanager.GetString("trDelivery");
            txt_discount.Text = MainWindow.resourcemanager.GetString("trDiscount");
            txt_totalDescount.Text = MainWindow.resourcemanager.GetString("trDiscount");

            txt_pdf.Text = MainWindow.resourcemanager.GetString("trPdf");
            txt_printCount.Text = MainWindow.resourcemanager.GetString("trAdditional");
            txt_printInvoice.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_invoiceImages.Text = MainWindow.resourcemanager.GetString("trImages");
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_drafts.Text = MainWindow.resourcemanager.GetString("trDrafts");
            txt_newDraft.Text = MainWindow.resourcemanager.GetString("trNew");
            txt_emailMessage.Text = MainWindow.resourcemanager.GetString("trSendEmail");
            txt_payments.Text = MainWindow.resourcemanager.GetString("trPayments");
            txt_returnInvoice.Text = MainWindow.resourcemanager.GetString("trReturn");
            txt_quotations.Text = MainWindow.resourcemanager.GetString("trQuotations");
            txt_ordersWait.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_invoices.Text = MainWindow.resourcemanager.GetString("trInvoices");
            txt_deliveryCostTitle.Text = MainWindow.resourcemanager.GetString("trDeliveryCost");
            chk_onDelivery.Content = MainWindow.resourcemanager.GetString("OnDelivery");
            chk_isFreeDelivery.Content = MainWindow.resourcemanager.GetString("freeDelivery");
            txt_payTypeTitle.Text = MainWindow.resourcemanager.GetString("defaultPayment") + ":";
            txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("creditLimit") + ":";
            txt_upperLimit.Text = "-";

            tt_error_previous.Content = MainWindow.resourcemanager.GetString("trPrevious");
            tt_error_next.Content = MainWindow.resourcemanager.GetString("trNext");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_coupon, MainWindow.resourcemanager.GetString("trCoponHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_customer, MainWindow.resourcemanager.GetString("trCustomerHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_desrvedDate, MainWindow.resourcemanager.GetString("trDeservedDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trCompanyHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUserHint"));          
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_discount, MainWindow.resourcemanager.GetString("trDiscountHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_typeDiscount, MainWindow.resourcemanager.GetString("trDiscountTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sliceId, MainWindow.resourcemanager.GetString("invoiceClass"));
           
            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");
        }

        private async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.KeyDown -= HandleKeyPress;
                await newDraft();
                timer.Stop();

                GC.Collect();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
        }

        private async Task saveBeforeExit()
        {
            if (billDetails.Count > 0 && (_InvoiceType == "sd" || _InvoiceType == "sbd"))
            {
                #region Accept
                MainWindow.mainWindow.Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trSaveInvoiceNotification");

                w.ShowDialog();
                MainWindow.mainWindow.Opacity = 1;
                #endregion
                if (w.isOk)
                    Btn_newDraft_Click(null, null);
                else
                {
                    await clearInvoice();
                }
            }
            else
            {
                await clearInvoice();
            }
        }
        #region loading
        List<keyValueBool> loadingList;
        List<string> catchError = new List<string>();
        int catchErrorCount = 0;

        bool loadingSuccess_RefrishItems = false;
        async void loading_RefrishItems()
        {
            try
            {
                await RefrishItems();
                if (items is null)
                    loading_RefrishItems();
                else
                    loadingSuccess_RefrishItems = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_RefrishItems");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_RefrishItems = true;
                }
                else
                    loading_RefrishItems();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_RefrishItems)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_RefrishItems"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_RefrishCustomers = false;
        async void loading_RefrishCustomers()
        {
            try
            {
                await RefrishCustomers();
                if (customers is null)
                    loading_RefrishCustomers();
                else
                    loadingSuccess_RefrishCustomers = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_RefrishCustomers");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_RefrishCustomers = true;
                }
                else
                    loading_RefrishCustomers();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_RefrishCustomers)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_RefrishCustomers"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_fillBarcodeList = false;
        async void loading_fillBarcodeList()
        {
            try
            {
                await fillBarcodeList();
                if (barcodesList is null)
                    loading_fillBarcodeList();
                else
                    loadingSuccess_fillBarcodeList = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_fillBarcodeList");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_fillBarcodeList = true;
                }
                else
                    loading_fillBarcodeList();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_fillBarcodeList)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_fillBarcodeList"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_fillCouponsList = false;
        async void loading_fillCouponsList()
        {
            try
            {
                await fillCouponsList();
                if (coupons is null)
                    loading_fillCouponsList();
                else
                    loadingSuccess_fillCouponsList = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_fillCouponsList");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_fillCouponsList = true;
                }
                else
                    loading_fillCouponsList();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_fillCouponsList)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_fillCouponsList"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_fillShippingCompanies = false;
        async void loading_fillShippingCompanies()
        {
            try
            {
                await fillShippingCompanies();

                if (FillCombo.shippingCompaniesList is null)
                    loading_fillShippingCompanies();
                else
                    loadingSuccess_fillShippingCompanies = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_fillShippingCompanies");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_fillShippingCompanies = true;
                }
                else
                    loading_fillShippingCompanies();

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_fillShippingCompanies)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_fillShippingCompanies"))
                    {
                        item.value = true;
                        break;
                    }
                }
        }
        bool loadingSuccess_fillUsers = false;
        async void loading_fillUsers()
        {
            try
            {
                await fillUsers();

                if (users is null)
                    loading_fillUsers();
                else
                    loadingSuccess_fillUsers = true;
            }
            catch (Exception ex)
            {
                catchError.Add("loading_fillUsers");
                catchErrorCount++;
                if (catchErrorCount > 50)
                {
                    loadingSuccess_fillUsers = true;
                }
                else
                    loading_fillUsers();
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            if (loadingSuccess_fillUsers)
                foreach (var item in loadingList)
                {
                    if (item.key.Equals("loading_fillUsers"))
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
                if (isFromReport)
                {
                    await FillCombo.FillComboSlices(cb_sliceId);

                    if (FillCombo.slicesList is null)
                        loading_FillComboSlices();
                    else
                        loadingSuccess_FillComboSlices = true;
                }
                else
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
        #endregion

        SetValues sliceValue = new SetValues();
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
                tb_moneyIcon2.Text = AppSettings.Currency;
                tb_moneyIcon3.Text = AppSettings.Currency;
                tb_moneyIconTotal.Text = AppSettings.Currency;
                tb_moneyIconDeliveryCost.Text = AppSettings.Currency;

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
                tb_moneyIconDis.Text = AppSettings.Currency;
                translate();
               
                ///coupon
                lst_coupons.ItemsSource = couponsLst;
                lst_coupons.DisplayMemberPath = "notes";
                lst_coupons.SelectedValuePath = "cId";

                catigoriesAndItemsView.ucReceiptInvoice = this;
                pos = MainWindow.posLogIn;
                configureDiscountType();
                setNotifications();
                setTimer();

                #region loading
                loadingList = new List<keyValueBool>();
                bool isDone = true;
                loadingList.Add(new keyValueBool { key = "loading_RefrishItems", value = false });
                loadingList.Add(new keyValueBool { key = "loading_RefrishCustomers", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillBarcodeList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillCouponsList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillShippingCompanies", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillUsers", value = false });
                loadingList.Add(new keyValueBool { key = "loading_globalSaleUnits", value = false });
                loadingList.Add(new keyValueBool { key = "loading_FillComboSlices", value = false });
                loadingList.Add(new keyValueBool { key = "loading_FillTaxes", value = false });

                loading_RefrishItems();
                loading_RefrishCustomers();
                loading_fillBarcodeList();
                loading_fillCouponsList();
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
                        //string s = "";
                        //foreach (var item in loadingList)
                        //{
                        //    s += item.name + " - " + item.value + "\n";
                        //}
                        //MessageBox.Show(s);
                        await Task.Delay(0500);
                    }
                }
                while (!isDone);
                #endregion

                #region Style Date
                SectionData.defaultDatePickerStyle(dp_desrvedDate);
                #endregion

                if (AppSettings.invoiceTax_bool == false)
                    sp_tax.Visibility = Visibility.Collapsed;
                else
                {
                    if(taxes.Count == 0)
                    {
                        //go to taxes page
                        if (MainWindow.groupObject.HasPermission("taxes", MainWindow.groupObjects) || SectionData.isAdminPermision())
                        {
                            
                            await MainWindow.loadingDefaultPath("sectionData", "taxes");
                            return;
                        }
                        else
                        {
                            // show message to add tax
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("youDontHaveAnyTaxYet"), animation: ToasterAnimation.FadeIn);

                        }
                    }
                   // tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
                    sp_tax.Visibility = Visibility.Visible;
                }

                #region key up
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

                #region datagridChange
                //CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(dg_billDetails.Items);
                //((INotifyCollectionChanged)myCollectionView).CollectionChanged += new NotifyCollectionChangedEventHandler(DataGrid_CollectionChanged);
                #endregion

                #region Permision

                if (MainWindow.groupObject.HasPermissionAction(returnPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    //bdr_returnInvoice.Visibility = Visibility.Visible;
                    brd_returnInvoice.Visibility = Visibility.Visible;
                }
                else
                {
                    //bdr_returnInvoice.Visibility = Visibility.Collapsed;
                    brd_returnInvoice.Visibility = Visibility.Collapsed;
                }

                //if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one"))
                //{
                //    md_payments.Visibility = Visibility.Visible;
                //    bdr_payments.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    md_payments.Visibility = Visibility.Collapsed;
                //    bdr_payments.Visibility = Visibility.Collapsed;
                //}



                if (MainWindow.groupObject.HasPermissionAction(executeOrderPermission, MainWindow.groupObjects, "one"))
                    md_ordersWait.Visibility = Visibility.Visible;
                else
                    md_ordersWait.Visibility = Visibility.Collapsed;

                if (MainWindow.groupObject.HasPermissionAction(quotationPermission, MainWindow.groupObjects, "one"))
                    md_quotations.Visibility = Visibility.Visible;
                else
                    md_quotations.Visibility = Visibility.Collapsed;

                //if (MainWindow.groupObject.HasPermissionAction(sendEmailPermission, MainWindow.groupObjects, "one"))
                //{
                //    btn_emailMessage.Visibility = Visibility.Visible;
                //    bdr_emailMessage.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    btn_emailMessage.Visibility = Visibility.Collapsed;
                //    bdr_emailMessage.Visibility = Visibility.Collapsed;
                //}


                //if (MainWindow.groupObject.HasPermissionAction(printCountPermission, MainWindow.groupObjects, "one"))
                //{
                //    btn_printCount.Visibility = Visibility.Visible;
                //    bdr_printCount.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    btn_printCount.Visibility = Visibility.Collapsed;
                //    bdr_printCount.Visibility = Visibility.Collapsed;
                //}

                #endregion
                #region print - pdf - send email
                if(!isFromReport)
                {
                    btn_printInvoice.Visibility = Visibility.Collapsed;
                    btn_pdf.Visibility = Visibility.Collapsed;
                    btn_printCount.Visibility = Visibility.Collapsed;
                    btn_emailMessage.Visibility = Visibility.Collapsed;
                    bdr_emailMessage.Visibility = Visibility.Collapsed;
                }
                #endregion
                #region tb_total textChange
                /*
                var dp = DependencyPropertyDescriptor.FromProperty(
                TextBlock.TextProperty,
                 typeof(TextBlock));
                dp.AddValueChanged(tb_total, (Null, args) =>
                {
                    Tb_cashPaid_TextChanged(null, null);
                });
                */
                #endregion               

                //List all the UIElement in the VisualTree
                controls = new List<Control>();
                FindControl(this.grid_main, controls);


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
        async private void ParentWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //try
            //{
            //    if (sender != null)
            //        SectionData.StartAwait(grid_main);
            //    isClose = true;
            //    //UserControl_Unloaded(this, null);
            //    await saveBeforeExit();
            //    if (sender != null)
            //        SectionData.EndAwait(grid_main);
            //}
            //catch (Exception ex)
            //{
            //    if (sender != null)
            //        SectionData.EndAwait(grid_main);
            //   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            //}
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
        async void timer_Tick(object sendert, EventArgs et)
        {
            try
            {

                //if (SectionData.isAdminPermision())
                //    setInvoiceNotification();
                //setOrdersWaitNotification();
                //setQuotationNotification();
                refreshSalesNotification();
                if (invoice.invoiceId != 0)
                {
                    await refreshDocCount(invoice.invoiceId);
                    if (_InvoiceType == "s" || _InvoiceType == "sb")
                        refreshInvoiceNot(invoice.invoiceId);
                }
                //if (sendert != null)
                //    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sendert != null)
                    //SectionData.EndAwait(grid_main);
                   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region notifications
        private void setNotifications()
        {
            refreshDraftNotification();
            refreshSalesNotification();
        }

        private async Task refreshDraftNotification()
        {
            try
            {
                string invoiceType = "sd ,sbd";
                int duration = 2;
                int draftCount = 0;
                if (AppSettings.SalesDraftCount <= 0)
                {
                    draftCount = (int)await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);

                    draftCount = draftCount < 0 ? 0 : draftCount;
                    AppSettings.SalesDraftCount = draftCount;
                }
                setDraftNotification(AppSettings.SalesDraftCount);
            }
            catch (Exception ex) { }
        }

        private void setDraftNotification(int draftCount)
        {
            try
            {
                if (draftCount > 0 && (_InvoiceType == "sd" || _InvoiceType == "sbd") && invoice != null && invoice.invoiceId != 0 && !isFromReport)
                    draftCount--;

                int previouseCount = 0;
                if (md_draft.Badge != null && md_draft.Badge.ToString() != "") previouseCount = int.Parse(md_draft.Badge.ToString());

                if (draftCount != previouseCount)
                {
                    if (draftCount > 9)
                    {
                        draftCount = 9;
                        md_draft.Badge = "+" + draftCount.ToString();
                    }
                    else if (draftCount == 0) md_draft.Badge = "";
                    else
                        md_draft.Badge = draftCount.ToString();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        private async Task refreshSalesNotification()
        {
            int duration = 1;
            InvoiceResult notificationRes = await invoice.getSalesNot(MainWindow.userID.Value, duration, MainWindow.branchID.Value);
            setInvoiceNotification(notificationRes.InvoiceCount);
            setOrdersWaitNotification(notificationRes.SalesWaitingOrdersCount);
            setQuotationNotification(notificationRes.SalesQuotationCount);
        }
        private void setInvoiceNotification(int invoicesCount)
        {
            try
            {
                //string invoiceType = "s ,sb";
                //int duration = 1;
                //int invoicesCount = 0;
                //if (SectionData.isAdminPermision())
                //    invoicesCount = await invoice.GetCountForAdmin(invoiceType, duration);
                //else
                //    invoicesCount = await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                if ((_InvoiceType == "s" || _InvoiceType == "sb") && invoice != null && invoice.isArchived == false && invoice.invoiceId != 0 && !isFromReport)
                    invoicesCount--;

                int previouseCount = 0;
                if (md_invoice.Badge != null && md_invoice.Badge.ToString() != "") previouseCount = int.Parse(md_invoice.Badge.ToString());

                if (invoicesCount != previouseCount)
                {
                    if (invoicesCount > 9)
                    {
                        invoicesCount = 9;
                        md_invoice.Badge = "+" + invoicesCount.ToString();
                    }
                    else if (invoicesCount == 0) md_invoice.Badge = "";
                    else
                        md_invoice.Badge = invoicesCount.ToString();
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void setOrdersWaitNotification(int ordersCount)
        {
            try
            {
                if (_InvoiceType == "or" && invoice.invoiceId != 0 && !isFromReport && invoice != null)
                    ordersCount--;

                int previouseCount = 0;
                if (md_ordersWait.Badge != null && md_ordersWait.Badge.ToString() != "") previouseCount = int.Parse(md_ordersWait.Badge.ToString());

                if (ordersCount != previouseCount)
                {
                    if (ordersCount > 9)
                    {
                        ordersCount = 9;
                        md_ordersWait.Badge = "+" + ordersCount.ToString();
                    }
                    else if (ordersCount == 0) md_ordersWait.Badge = "";
                    else
                        md_ordersWait.Badge = ordersCount.ToString();
                }
            }
            catch (Exception ex){ }
        }
        private void setQuotationNotification(int ordersCount)
        {
            try
            {
                if (_InvoiceType == "q" && invoice.invoiceId != 0 && !isFromReport && invoice != null)
                    ordersCount--;

                int previouseCount = 0;
                if (md_quotations.Badge != null && md_quotations.Badge.ToString() != "") previouseCount = int.Parse(md_quotations.Badge.ToString());

                if (ordersCount != previouseCount)
                {
                    if (ordersCount > 9)
                    {
                        ordersCount = 9;
                        md_quotations.Badge = "+" + ordersCount.ToString();
                    }
                    else if (ordersCount == 0) md_quotations.Badge = "";
                    else
                        md_quotations.Badge = ordersCount.ToString();
                }
            }
            catch (Exception ex){ }
        }
        int _DocCount = 0;
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
                        docCount = 9;
                        md_docImage.Badge = "+" + docCount.ToString();
                    }
                    else if (docCount == 0) md_docImage.Badge = "";
                    else
                        md_docImage.Badge = docCount.ToString();
                }
                _DocCount = docCount;

            }
            catch (Exception ex){ }
        }

        private async void refreshInvoiceNot(int invoiceId)
        {
            InvoiceResult notificationRes = await invoice.getInvoicePaymentArchiveCount(invoiceId);
            setPaymentsNotification(notificationRes.PaymentsCount);
            setArchiveNotification(notificationRes.InvoiceCount);
        }

        int _PaymentCount = 0;
        private void setPaymentsNotification(int paymentsCount)
        {
            try
            {
                //if (paymentsCount == 0 || !MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one"))
                if (!MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one"))
                {
                    bdr_payments.Visibility = Visibility.Collapsed;
                    md_payments.Visibility = Visibility.Collapsed;
                }
                else if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one"))
                {
                    bdr_payments.Visibility = Visibility.Visible;
                    md_payments.Visibility = Visibility.Visible;
                    
                    if (paymentsCount != _PaymentCount)
                    {
                        if (paymentsCount > 9)
                        {
                            paymentsCount = 9;
                            md_payments.Badge = "+" + paymentsCount.ToString();
                        }
                        else if (paymentsCount == 0) md_payments.Badge = "";
                        else
                            md_payments.Badge = paymentsCount.ToString();
                    }
                    _PaymentCount = paymentsCount;

                }
            }
            catch (Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }

        int _ArchiveCount = 0;
        private void setArchiveNotification(int invCount)
        {
            try
            {
                //if (invoice != null && invoice.isArchived == true)
                    //invCount--;

                if (invCount != _ArchiveCount)
                {
                    if (invCount > 9)
                    {
                        md_invoiceArchive.Badge = "+9";
                    }
                    else if (invCount == 0) md_invoiceArchive.Badge = "";
                    else
                        md_invoiceArchive.Badge = invCount.ToString();
                }
                _ArchiveCount = invCount;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        #endregion
        async Task RefrishCustomers()
        {
            //customers = await agentModel.GetAgentsActive("c");
            if (FillCombo.customersList is null)
                await FillCombo.RefreshCustomers();
            customers = FillCombo.customersList;
            cb_customer.ItemsSource = FillCombo.customersList;
            cb_customer.DisplayMemberPath = "name";
            cb_customer.SelectedValuePath = "agentId";
        }
        async Task RefrishItems()
        {
            items = await itemModel.GetSaleOrPurItems(0, 1, 0, MainWindow.branchID.Value);
        }
        async Task fillBarcodeList()
        {
            barcodesList = await itemUnitModel.GetUnitsForSales(MainWindow.branchID.Value);
        }
        async Task fillCouponsList()
        {
            couponModel = new Coupon();
            coupons = await couponModel.GetEffictiveCoupons();

            foreach (Coupon c in coupons)
                c.notes = c.name + "   #" + c.code + System.Environment.NewLine +c.details ;

        }
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
        private async Task fillShippingCompanies()
        {

            await FillCombo.FillComboShippingCompaniesWithDefault(cb_company);
        }
        private async Task fillUsers()
        {
            if (FillCombo.driversList is null)
                await FillCombo.RefreshDrivers();

            users = FillCombo.driversList;
            cb_user.ItemsSource = users;
            cb_user.DisplayMemberPath = "fullName";
            cb_user.SelectedValuePath = "userId";
        }

        List<Taxes> taxes = new List<Taxes>();
        private async Task fillTaxes()
        {
            await FillCombo.RefreshTaxess();

            taxes = FillCombo.taxessList.Where(x => x.isActive == true && x.taxType == "sales").ToList();
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
                        // _Tax -= row.Tax;

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
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
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
                    await RefrishCustomers();
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
                    w.type = "c";
                    w.ShowDialog();
                    if (w.isOk == true)
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        await FillCombo.RefreshCustomers();
                        await RefrishCustomers();
                    }
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
                    //decimal itemTax = 0;
                    //if (AppSettings.itemsTax_bool == true)
                    //{
                    //    if (item.taxes != null)
                    //        itemTax = (decimal)(item.taxes);
                    //}
                    //else
                    //    itemTax = 0;
                    // search for default unit for purchase
                    var defaultsaleUnit = itemUnits.ToList().Find(c => c.defaultSale == 1);

                    if (defaultsaleUnit != null)
                    {

                        decimal price = 0;

                        if (defaultsaleUnit.SalesPrices == null || (defaultsaleUnit.SalesPrices != null && defaultsaleUnit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                        {
                            //basicPrice = (decimal)defaultsaleUnit.basicPrice;
                            //if (AppSettings.itemsTax_bool == true)
                            //    price = (decimal)defaultsaleUnit.priceTax;
                            //else
                            price = (decimal)defaultsaleUnit.price;
                        }
                        else
                        {
                            var slice = defaultsaleUnit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                            //basicPrice = (decimal)slice.basicPrice;
                            //if (AppSettings.itemsTax_bool == true)
                            //    price = (decimal)slice.priceTax;
                            //else
                                price = (decimal)slice.price;
                        }
                        int? warrantyId = null;
                        string warrantName = "";
                        string warrantyDesc = "";
                        if(defaultsaleUnit.hasWarranty)
                        {
                            warrantyId = defaultsaleUnit.warrantyId;
                            warrantyDesc = defaultsaleUnit.warrantyDescription;
                            warrantName = defaultsaleUnit.warrantyName;
                        }
                       await  addItemToBill(itemId, (int)defaultsaleUnit.itemUnitId, defaultsaleUnit.unitName,price, warrantyId,warrantName, warrantyDesc);

                    }
                    else
                    {
                        bool valid = true;
                        if (item.type == "sn" ||
                                (item.type.Equals("p") && item.packageItems != null && item.packageItems.Where(x => x.type == "sn").FirstOrDefault() != null))
                            valid = false;

                        int? offerId = null;
                        addRowToBill(item.name, itemId, null, 0, 1, 0, item.type, valid, offerId, "1", 0,item.packageItems,null,"","");
                    }
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion


        #region validation
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

        DateTime? _DeservedDate;
        private bool validateInvoiceValues()
        {
            bool valid = true;
            bool available = true;
            if (decimal.Parse(tb_total.Text) == 0)
            {
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorTotalIsZeroToolTip"), animation: ToasterAnimation.FadeIn);
                return false;
            }

            //after remove payments
            if (cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() != "0")
            {
                if (!SectionData.validateEmptyComboBox(cb_customer, p_errorCustomer, tt_errorCustomer, "trEmptyCustomerToolTip"))
                {
                    exp_customer.IsExpanded = true;
                    return false;
                    //valid = false;
                }
            }
           // #region validate deserved date
           //if(!_InvoiceType.Equals("sbd"))
           // foreach(var pay in listPayments)
           // {
           //     if(pay.processType.Equals("balance") )
           //     {
           //         if (!SectionData.validateEmptyDatePicker(dp_desrvedDate, p_errorDesrvedDate, tt_errorDesrvedDate, "trErrorEmptyDeservedDate"))
           //         {
           //             exp_customer.IsExpanded = true;
           //             return false;
           //         }
           //         else
           //             _DeservedDate = dp_desrvedDate.SelectedDate;
           //     }
           // }
           // #endregion
            // validate empty values
            //if (cb_paymentProcessType.SelectedIndex != -1)
            //{
            //    switch (cb_paymentProcessType.SelectedValue.ToString())
            //    {
            //        case "cash":
            //            SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
            //            SectionData.clearValidate(tb_cashPaid, p_errorCashPaid);

            //            if(!(string.IsNullOrWhiteSpace(tb_cashPaid.Text) || tb_cashPaid.Text == "0"))
            //            {
            //                try
            //                {
            //                    if (decimal.Parse(tb_cashPaid.Text) < decimal.Parse(tb_total.Text))
            //                    {
            //                        SectionData.SetError(tb_cashPaid, p_errorCashPaid, tt_errorCashPaid, "amountPaidMustEqualOrBiggerInvoiceValue");
            //                        valid = false;
            //                    }
            //                }
            //                catch
            //                {}
            //            } 
            //            break;
            //        case "balance":
            //            if (!SectionData.validateEmptyComboBox(cb_customer, p_errorCustomer, tt_errorCustomer, "trEmptyCustomerToolTip"))
            //            {
            //                exp_customer.IsExpanded = true;
            //                valid = false;
            //            }
            //            break;
            //        case "card":
            //            if (txt_card.Text.Equals(""))
            //                valid = false;
            //            SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
            //            SectionData.validateEmptyTextBlock(txt_card, p_errorCard, tt_errorCard, "trSelectCreditCard");
            //            if (_SelectedCard != -1)
            //            {
            //                var card = cards.Where(x => x.cardId == _SelectedCard).FirstOrDefault();
            //                if (card.hasProcessNum.Value && tb_processNum.Text.Equals(""))
            //                {
            //                    SectionData.validateEmptyTextBox(tb_processNum, p_errorProcessNum, tt_errorProcessNum, "trEmptyProcessNumToolTip");
            //                    valid = false;
            //                }
            //            }
            //            break;
            //    }
            //}
            //else
            //    valid = false;


            // validate items serials
            foreach (BillDetails item in billDetails)
            {
                if (item.valid == false)
                {
                    valid = false;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorNoSerialToolTip"), animation: ToasterAnimation.FadeIn);

                    return valid;
                }
            }
            #region validate sales man
            //com
            SectionData.clearComboBoxValidate(cb_user, p_errorUser);
            if (companyModel.deliveryType == "local" && (cb_user.SelectedValue != null && cb_user.SelectedValue.ToString() == "0"))
            {
                valid = false;
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectTheDeliveryMan"), animation: ToasterAnimation.FadeIn);
                SectionData.validateEmptyComboBox(cb_user, p_errorUser, tt_errorUser, "trSelectTheDeliveryMan");
                exp_delivery.IsExpanded = true;
                return valid;
            }
            #endregion

            if (billDetails.Count > 0 && available && valid)
                valid = true;
            else
                valid = false;
            if (valid == true)
                valid = validateItemUnits();

            //if (valid)
            //{
            //    if (cb_paymentProcessType.SelectedIndex == 1 && (companyModel == null || companyModel.deliveryType != "com") && cb_customer.SelectedIndex != -1)
            //    {
            //        int agentId = (int)cb_customer.SelectedValue;
            //        decimal remain = 0;
            //        float totalNet = float.Parse(tb_total.Text);
            //        Agent customer = customers.ToList().Find(b => b.agentId == agentId && b.isLimited == true);
            //        //Agent customer = customers.ToList()
            //        //    .Find(b => b.agentId == agentId 
            //        //    && (b.isLimited == true ||(b.type == "0" && b.balance > totalNet )));
            //        if (customer != null)
            //        {
            //            remain = getCusAvailableBlnc(customer);
            //            float customerBalance = customer.balance;
            //            if (remain > customer.maxDeserve && customer.maxDeserve > 0)
            //            {
            //                valid = false;
            //                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMaxDeservedExceeded"), animation: ToasterAnimation.FadeIn);
            //            }
            //        }
            //        else
            //        {
            //            Agent customerNotHaveLimited = customers.ToList().Find(b => b.agentId == agentId);
            //            if (!(customerNotHaveLimited.balanceType == 0 && customerNotHaveLimited.balance > totalNet))
            //            {
            //                valid = false;
            //                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMaxDeservedExceeded"), animation: ToasterAnimation.FadeIn);
            //            }
            //        }
            //    }
            //}
            return valid;
        }
      
       void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_customer")
                    {
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCustomer, tt_errorCustomer, "trEmptyCustomerToolTip");
                        exp_customer.IsExpanded = true;

                    }
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region save invoice

        private async Task addInvoice(string invType)
        {
            InvoiceResult invoiceResult = new InvoiceResult();

           
            #region Exceed Min Limit of items alert
            Notification amountNot = new Notification()
            {
                title = "trExceedMinLimitAlertTilte",
                ncontent = "trExceedMinLimitAlertContent",
                msgType = "alert",
                objectName = "storageAlerts_minMaxItem",
                branchId = MainWindow.branchID.Value,
                createDate = DateTime.Now,
                updateDate = DateTime.Now,
                createUserId = MainWindow.userID.Value,
                updateUserId = MainWindow.userID.Value,
            };

            #endregion

            #region invoice Object

            #region invNumber
            if ((invoice.invType == "s" && (invType == "sb" || invType == "sbd")) || _InvoiceType == "or" || _InvoiceType == "q") // invoice is sale and will be bounce sale  or sale bounce draft  , save another invoice in db
            {
                invoice.invoiceMainId = invoice.invoiceId;
                invoice.invoiceId = 0;
                if (invType == "sb")
                {
                    invoice.invNumber = "sb";
                }
                else if (invType == "sbd")
                {
                    invoice.invNumber = "sbd";
                }
                else if (_InvoiceType == "or" || _InvoiceType == "q")
                {
                    invoice.invNumber = "si";
                }
            }
            // build invoice NUM 
            else if ((invoice.invNumber == null && invType == "s") || (invoice.invType == "sd" && invType == "s"))
            {
                invoice.invNumber = "si";
                invoice.branchId = MainWindow.branchID.Value;
            }
            else if (invoice.invType == "sbd" && invType == "sb") // convert invoicce from draft bounce to bounce
            {
                invoice.invNumber = "sb";
            }
            else if (invType == "sd" && invoice.invoiceId == 0)
            {
                invoice.branchId = MainWindow.branchID.Value;
                invoice.invNumber = "sd";
            }
            #endregion
            if (invoice.branchCreatorId == 0 || invoice.branchCreatorId == null)
            {
                invoice.branchCreatorId = MainWindow.branchID.Value;
            }

            invoice.posId = MainWindow.posID;
            invoice.discountValue = _Discount;
            invoice.discountType = "1";
            invoice.printedcount = 0;
            invoice.isOrginal = true;
            invoice.isActive = true;
            invoice.invCase = "";

            invoice.deservedDate = dp_desrvedDate.SelectedDate;
            invoice.notes = tb_note.Text;
            invoice.sales_invoice_note = await FillCombo.getSetValue("sales_invoice_note");
            invoice.itemtax_note = await FillCombo.getSetValue( "itemtax_note");

            invoice.shippingCost = _DeliveryCost;
            invoice.realShippingCost = _RealDeliveryCost;

            if (chk_isFreeDelivery.IsChecked == true)
                invoice.isFreeShip = 1;
            
            invoice.createUserId = MainWindow.userID;
            invoice.updateUserId = MainWindow.userID;
            invoice.invType = invType;

            if(cb_sliceId.SelectedValue != null)
            {
                invoice.sliceId = (int)cb_sliceId.SelectedValue;
                invoice.sliceName = cb_sliceId.Text;
            }
            if (cb_typeDiscount.SelectedIndex != -1)
                invoice.manualDiscountType = cb_typeDiscount.SelectedValue.ToString();
            if (tb_discount.Text != "")
                invoice.manualDiscountValue = decimal.Parse(tb_discount.Text);
            else
                invoice.manualDiscountValue = 0;

            invoice.total = _Sum;
            invoice.totalNet = decimal.Parse(tb_total.Text);

            if (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0")
                invoice.agentId = (int)cb_customer.SelectedValue;

            if (tb_taxValue.Text != "" && AppSettings.invoiceTax_bool == true)
            {
                invoice.tax = AppSettings.invoiceTax_decimal;
                invoice.taxValue = _TaxValue;
                invoice.invoiceTaxes = invoiceTaxex;
            }
            else
            {
               invoice.tax = 0;
                invoice.taxValue = 0;
            }


            if (cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() != "0")
                invoice.shippingCompanyId = (int)cb_company.SelectedValue;
            else
                invoice.shippingCompanyId = null;

            if (cb_user.SelectedValue != null  && cb_user.SelectedValue.ToString() != "0")
                invoice.shipUserId = (int)cb_user.SelectedValue;

            if (chk_onDelivery.IsChecked.Equals(true))
                invoice.isPrePaid = 0;
            else
                invoice.isPrePaid = 1;

            invoice.paid = 0;
            invoice.deserved = invoice.totalNet;
            invoice.cashReturn = theRemine;

            #endregion

            #region invoice items
            invoiceItems = new List<ItemTransfer>();
            ItemTransfer itemT;
            decimal VATValue = 0;
            for (int i = 0; i < billDetails.Count; i++)
            {
                itemT = new ItemTransfer();
                itemT.invoiceId = 0;
                itemT.quantity = billDetails[i].Count;
                itemT.price = billDetails[i].Price;
                itemT.itemUnitId = billDetails[i].itemUnitId;
                itemT.offerId = billDetails[i].offerId == null ? 0 : billDetails[i].offerId;
                itemT.offerType = billDetails[i].offerId == null ?0 :decimal.Parse(billDetails[i].OfferType);
                itemT.offerValue = billDetails[i].OfferValue;
                itemT.offerName = billDetails[i].OfferName;
                itemT.itemUnitPrice = billDetails[i].Price;
                itemT.itemType = billDetails[i].type;
                itemT.itemName = billDetails[i].Product;
                itemT.unitName = billDetails[i].Unit;
                if (billDetails[i].returnedSerials == null)
                    itemT.itemSerials = billDetails[i].itemSerials;
                else
                    itemT.itemSerials = billDetails[i].itemSerials.Where(x => !billDetails[i].returnedSerials.Any(e =>  e.serialNum == x.serialNum)).ToList();

                itemT.returnedSerials = billDetails[i].returnedSerials;

                itemT.ItemStoreProperties = billDetails[i].StoreProperties;
                itemT.ReturnedProperties = billDetails[i].ReturnedProperties;

                itemT.packageItems = billDetails[i].packageItems;
                itemT.warrantyId = billDetails[i].warrantyId;
                itemT.warrantyName = billDetails[i].warrantyName;
                itemT.warrantyDescription = billDetails[i].warrantyDescription;
                itemT.itemId = billDetails[i].itemId;
                itemT.isTaxExempt = billDetails[i].isTaxExempt;
                itemT.VATRatio = billDetails[i].VATRatio;
                itemT.createUserId = MainWindow.userID;

                invoiceItems.Add(itemT);

                //calculate VATValue
                if(AppSettings.itemsTax_bool == true)
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
            }
            #endregion

            invoice.VATValue = VATValue;
            #region Invoice Coupons

            foreach (CouponInvoice ci in selectedCoupons)
            {
                ci.createUserId = MainWindow.userID;
            }
            #endregion

            int invoiceId = 0;
            invoiceStatus st;
            CashTransfer cashT = new CashTransfer();
            Notification not;
            if (_InvoiceType.Equals("or"))
                invType = "or";

            switch (invType)
            {
                case "sb":

                    #region notification Object
                    not = new Notification()
                    {
                        title = "trExceedMaxLimitAlertTilte",
                        ncontent = "trExceedMaxLimitAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_minMaxItem",
                        branchId = MainWindow.branchID.Value,
                        createDate = DateTime.Now,
                        updateDate = DateTime.Now,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };
                    #endregion

                    #region posCash
                    cashT = invoice.posCashTransfer(invoice, "sb");
                    #endregion

                    invoiceResult = await invoiceModel.saveSalesBounce(invoice, invoiceItems, listPayments, not, cashT, selectedCoupons, MainWindow.branchID.Value, MainWindow.posID.Value);

                    break;
                case "or":
                    #region notification Object
                    int shipUserId = 0;
                    try { shipUserId = (int)invoice.shipUserId; }
                    catch (Exception ex)
                    {
                        SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                    }
                    not = new Notification()
                    {
                        title = "trDeliverOrderAlertTilte",
                        ncontent = "trDeliverOrderAlertContent",
                        msgType = "alert",
                        objectName = "saleAlerts_shippingUser",
                        prefix = MainWindow.loginBranch.name,
                        recieveId = shipUserId,
                        branchId = MainWindow.branchID.Value,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };

                    #endregion

                    #region order status
                    st = new invoiceStatus();
                    st.status = "Ready";
                    st.createUserId = MainWindow.userLogin.userId;
                    #endregion

                    #region posCash with type inv
                    cashT = invoice.posCashTransfer(invoice, "si");
                    #endregion

                    invoiceResult = await invoiceModel.saveSalesOrder(invoice, invoiceItems, not, amountNot, st, cashT, selectedCoupons, listPayments, MainWindow.branchID.Value, MainWindow.posID.Value);

                    break;
                case "s":
                    #region order status
                    st = new invoiceStatus();
                    // st.status = status; //UnderProcessing - Ready - Done
                    st.createUserId = MainWindow.userLogin.userId;
                    #endregion

                    #region posCash posCash with type inv
                    cashT = invoice.posCashTransfer(invoice, "si");
                    #endregion

                    invoiceResult = await invoiceModel.saveSalesInvoice(invoice, invoiceItems, st, amountNot, cashT, selectedCoupons, listPayments, MainWindow.branchID.Value, MainWindow.posID.Value);
                    break;

                default:
                    invoiceResult = await invoiceModel.saveSalesWithItems(invoice, invoiceItems, selectedCoupons);

                    break;

            }

            if (invoiceResult.Result > 0) //success
            {
                invoiceId = invoiceResult.Result;
                invoice.invoiceId = invoiceId;
                invoice.invNumber = invoiceResult.Message;
                invoice.updateDate = invoiceResult.UpdateDate;
                TimeSpan ts;
                TimeSpan.TryParse(invoiceResult.InvTime, out ts);
                invoice.invTime = ts;
                prinvoiceId = invoiceId;

                AppSettings.PosBalance = invoiceResult.PosBalance;
                MainWindow.setBalance();
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                prInvoice = invoice;
                if (prInvoice.invType == "s")
                {

                    if (AppSettings.print_on_save_sale == "1")
                    {
                        // printInvoice();
                        Thread tp = new Thread(async () =>
                        {
                            string msg = "";
                            List<PayedInvclass> payedlist = new List<PayedInvclass>();
                            payedlist = await cashTransfer.PayedBycashlist(listPayments);
                            msg = await printInvoice(prInvoice, invoiceItems, payedlist.ToList());
                            if (msg == "")
                            {

                            }
                            else
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString(msg), animation: ToasterAnimation.FadeIn);

                                });
                            }
                        });
                        tp.Start();
                    }
                    if (AppSettings.email_on_save_sale == "1")
                    {
                        //sendsaleEmail();
                        ////
                        Thread t1 = new Thread(async () =>
                        {
                            string msg = "";
                            List<PayedInvclass> payedlist = new List<PayedInvclass>();
                            payedlist = await cashTransfer.PayedBycashlist(listPayments);
                            msg = await sendsaleEmail(prInvoice, invoiceItems, payedlist.ToList());
                            this.Dispatcher.Invoke(() =>
                            {

                                if (msg == "")
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailNotSent"), animation: ToasterAnimation.FadeIn);
                                }
                                else if (msg == "trTheCustomerHasNoEmail")
                                {

                                }
                                else if (msg == "trMailSent")
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailSent"), animation: ToasterAnimation.FadeIn);

                                }
                                else
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString(msg), animation: ToasterAnimation.FadeIn);

                                }
                            });

                        });
                        t1.Start();
                        ////

                    }
                }

                await clearInvoice();

                switch (invType)
                {
                    case "or":
                        break;
                    case "sb":
                    case "s":
                        AppSettings.SalesDraftCount = invoiceResult.SalesDraftCount;
                        break;
                    default:
                        AppSettings.SalesDraftCount = invoiceResult.SalesDraftCount;

                        break;
                };
            }
            else if (invoiceResult.Result == -3) // كمية العنصر غير كافية
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -10) // كمية الخصائص المطلوبة غير كافية
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesNotAvailable") , animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -7) // الكمية المتبقية من الكوبون 0
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("RemainIsZero") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -8) // الكوبون غير فعال
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorCouponNotActive") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -5) // الحد الأدنى للكوبون أكبر من قيمة الفاتورة
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMinInvToolTip") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -6) // الحد الأعلى للكوبون إصغر من قيمة الفاتورة
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMaxInvToolTip") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -4) // رصيد الزبون غير كاف
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMaxDeservedExceeded"), animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -2) // رصيد pos غير كاف
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == -1)// إظهار رسالة الترقية
                Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpgrade"), animation: ToasterAnimation.FadeIn);
             else if (invoiceResult.Result == -9)// الكمية المرجعة أكبر من الكمية المباعة
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
            else if (invoiceResult.Result == 0) // an error occure
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
            #region old
            //else
            // {

            //await invoiceModel.saveInvoiceItems(invoiceItems, invoiceId);

            //if (invType == "s")
            //{
            //    #region notification Object
            //    Notification not = new Notification()
            //    {
            //        title = "trExceedMinLimitAlertTilte",
            //        ncontent = "trExceedMinLimitAlertContent",
            //        msgType = "alert",
            //        createDate = DateTime.Now,
            //        updateDate = DateTime.Now,
            //        createUserId = MainWindow.userID.Value,
            //        updateUserId = MainWindow.userID.Value,
            //    };
            //    #endregion
            //    await itemLocationModel.decreaseAmounts(invoiceItems, MainWindow.branchID.Value, MainWindow.userID.Value, "storageAlerts_minMaxItem", not, (int)invoice.invoiceMainId); // update item quantity in DB
            //    await invoice.recordPosCashTransfer(invoice, "si");                                                                                                         //if (paid > 0)
            //                                                                                                                                                                //                                                                                                                                                             //}
            //}
            //else if (invType == "sb")
            //{
            //    #region notification Object
            //    Notification not = new Notification()
            //    {
            //        title = "trExceedMaxLimitAlertTilte",
            //        ncontent = "trExceedMaxLimitAlertContent",
            //        msgType = "alert",
            //        createDate = DateTime.Now,
            //        updateDate = DateTime.Now,
            //        createUserId = MainWindow.userID.Value,
            //        updateUserId = MainWindow.userID.Value,
            //    };
            //    #endregion
            //    await itemLocationModel.recieptInvoice(invoiceItems, MainWindow.branchID.Value, MainWindow.userID.Value, "storageAlerts_minMaxItem", not); // update item quantity in DB
            //    await invoice.recordPosCashTransfer(invoice, "sb");

            //}
            // #region save coupns on invoice

            //foreach (CouponInvoice ci in selectedCoupons)
            //{
            //    ci.InvoiceId = invoiceId;
            //    ci.createUserId = MainWindow.userID;
            //}
            //await invoiceModel.saveInvoiceCoupons(selectedCoupons, invoiceId, invoice.invType);
            // #endregion


            //}
            #endregion

        }

        //bool logInProcessing = true;
        //void awaitSaveBtn(bool isAwait)
        //{
        //    if (isAwait == true)
        //    {
        //        btn_save.IsEnabled = false;
        //        wait_saveBtn.Visibility = Visibility.Visible;
        //        wait_saveBtn.IsIndeterminate = true;
        //    }
        //    else
        //    {
        //        btn_save.IsEnabled = true;
        //        wait_saveBtn.Visibility = Visibility.Collapsed;
        //        wait_saveBtn.IsIndeterminate = false;
        //    }
        //}
        List<CashTransfer> listPayments;
        public decimal theRemine = 0;
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (((MainWindow.groupObject.HasPermissionAction(invoicePermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    &&
                    (invoice.invType == "sd" || invoice.invType == "s"))
                    || (invoice.invType != "sd" && invoice.invType != "s"))
                {

                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        if (sender != null)
                            SectionData.StartAwait(grid_main);


                        refreshTotalValue();
                        //check mandatory inputs
                        bool valid = validateInvoiceValues();
                            if (valid)
                            {
                        bool multipleValid = true;
                        listPayments = new List<CashTransfer>();

                        if ((!((cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() != "0" )
                                && chk_onDelivery.IsChecked == true) && !_InvoiceType.Equals("sbd") )
                                || (_InvoiceType.Equals("sbd") && onCredit == false) )
                        {
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_multiplePayment w = new wd_multiplePayment();
                            if (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0")
                                w.hasCredit = true;
                            else
                                w.hasCredit = false;

                                if (cb_company.SelectedValue != null && (int)cb_company.SelectedValue > 0 && cb_user.SelectedValue == null)
                                    w.hasDeliveryCompany = true;
                                else
                                    w.hasDeliveryCompany = false;
                                

                            w.isPurchase = false;
                            if (cb_customer.SelectedValue != null)
                            {
                                Agent customer = await agentModel.getAgentById((int)cb_customer.SelectedValue);
                                w.agent = customer;
                            }
                            w.invoice.invType = _InvoiceType;
                            w.invoice.totalNet = decimal.Parse(tb_total.Text);
                            w.checkMaxCredit = true;
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            multipleValid = w.isOk;
                            listPayments = w.listPayments;
                            theRemine = w.theRemine;
                        }
                        else if ((_InvoiceType.Equals("sbd") && onCredit == true) || ((int)cb_company.SelectedValue > 0 && cb_user.SelectedValue == null) || chk_onDelivery.IsChecked == true)//return on credit sales or delivery with company - payments on delivery
                            oneCashTransfer();


                        if (multipleValid)
                        {
                            #region Save
                            if (_InvoiceType == "sbd") //sbd means sale bounse draft
                            {
                                await addInvoice("sb"); // sb means sale bounce
                                    setDraftNotification(AppSettings.SalesDraftCount);
                                    refreshSalesNotification();

                            }
                            else if (_InvoiceType == "or")
                            {
                                await addInvoice("s");

                                refreshSalesNotification();

                            }
                            else//s  sale invoice
                            {
                                await addInvoice("s");
                            
                                setDraftNotification(AppSettings.SalesDraftCount);
                                refreshSalesNotification();

                            }
                        }

                        #endregion

                        }
                        if (sender != null)
                            SectionData.EndAwait(grid_main);
                    }
                    else //box is closed
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void oneCashTransfer()
        {
            CashTransfer cashTrasnfer = new CashTransfer();
            cashTrasnfer.cash = decimal.Parse(tb_total.Text);
            cashTrasnfer.processType = "balance";

            if (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0")
                cashTrasnfer.agentId = (int)cb_customer.SelectedValue;
            listPayments.Add(cashTrasnfer);
        }
        
        private bool validateItemUnits()
        {
            for (int i = 0; i < billDetails.Count; i++)
            {
                if (billDetails[i].itemUnitId == 0)
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trItemWithNoUnit"), animation: ToasterAnimation.FadeIn);
                    return false;
                }
                if (billDetails[i].Count == 0)
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trItemWithNoCount"), animation: ToasterAnimation.FadeIn);
                    return false;
                }
            }
            return true;
        }
        async Task newDraft()
        {
            if (billDetails.Count > 0 && (_InvoiceType == "sd" || _InvoiceType == "sbd"))
            {
                bool valid = validateItemUnits();

                if (billDetails.Count > 0 && valid)
                {
                    #region Accept
                    MainWindow.mainWindow.Opacity = 0.2;
                    wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                    w.contentText = MainWindow.resourcemanager.GetString("trSaveInvoiceNotification");
                    // w.contentText = "Do you want save pay invoice in drafts?";
                    w.ShowDialog();
                    MainWindow.mainWindow.Opacity = 1;
                    #endregion
                    if (w.isOk)
                    {
                        await addInvoice(_InvoiceType);
                        await clearInvoice();

                    }
                    else
                    {
                        await clearInvoice();
                    }
                }
                else if (billDetails.Count == 0)
                {
                    _InvoiceType = "sd";
                    await clearInvoice();
                }

            }
            else
            {
                await clearInvoice();
            }
            setNotifications();
        }
        private async void Btn_newDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await newDraft();
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
        private async Task clearInvoice()
        {
            _Sum = 0;
            #region slice
            ///default slice
            ///
            await FillCombo.FillComboSlicesUser(cb_sliceId);
            if (AppSettings.DefaultInvoiceSlice == 0 || FillCombo.slicesUserList.Where(w => w.isActive == true && w.sliceId == AppSettings.DefaultInvoiceSlice).Count() == 0)
                cb_sliceId.SelectedIndex = 0;
            else
                cb_sliceId.SelectedValue = AppSettings.DefaultInvoiceSlice;
            #endregion



            companyModel = new ShippingCompanies();
            _Tax = 0;
            _Discount = 0;
            _DeliveryCost = 0;
            _RealDeliveryCost = 0;
            _SequenceNum = 0;
            txt_invNumber.Text = "";
            _SelectedCustomer = -1;
            _SelectedDiscountType = 0;
            _InvoiceType = "sd";
            invoice = new Invoice();
            selectedCoupons.Clear();
            tb_barcode.Clear();
            cb_customer.SelectedIndex = -1;
            cb_customer.SelectedItem = "";
            txt_payType.Text = "-";
            txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("creditLimit") + ":";
            txt_upperLimit.Text = "-";
            tb_moneyIcon2.Visibility = Visibility.Collapsed;
            dp_desrvedDate.Text = "";
            _DeservedDate = null;
            tb_note.Clear();
            billDetails.Clear();
            tb_total.Text = "0";
            tb_sum.Text = "0";
            tb_deliveryCost.Text = "0";
            tb_discount.Clear();
            tb_totalDescount.Text = "0";
            cb_typeDiscount.SelectedIndex = 0;
            cb_company.SelectedIndex = -1;
            cb_user.SelectedIndex = -1;
            chk_onDelivery.IsChecked = false;
            chk_isFreeDelivery.IsChecked = false;
            couponsLst = new List<Coupon>();
            lst_coupons.ItemsSource = null;
            #region clear notification
            isFromReport = false;
            archived = false;

            md_docImage.Badge = "";
            md_payments.Badge = "";
            md_invoiceArchive.Badge = "";
            bdr_invoiceArchive.Visibility = Visibility.Collapsed;
            md_invoiceArchive.Visibility = Visibility.Collapsed;
            _PaymentCount = 0;
            _DocCount = 0;
            _ArchiveCount = 0;
            #endregion
            btn_updateCustomer.IsEnabled = false;
            btn_items.IsEnabled = true;
            //if (AppSettings.invoiceTax_decimal != 0)
            //    tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
            //else
            //    tb_taxValue.Text = "0";
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoice");
            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

            SectionData.clearComboBoxValidate(cb_user, p_errorUser);
            SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);

            refrishBillDetails();
            tb_barcode.Focus();
            inputEditable();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
            await fillCouponsList();
        }
        #endregion

        private async void Btn_draft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();
                string invoiceType = "sd ,sbd";
                int duration = 2;
                w.invoiceType = invoiceType; //sales draft invoices , sales bounce drafts
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
                        // notifications
                        md_payments.Badge = "";
                        setNotifications();
                        await refreshDocCount(invoice.invoiceId);

                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                        navigateBtnActivate();

                        // set title to bill
                        if (_InvoiceType == "sd")
                        {
                            mainInvoiceItems = invoiceItems;
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesDraft");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                        }
                        if (_InvoiceType == "sbd")
                        {
                            mainInvoiceItems = await invoiceModel.GetInvoicesItems(invoice.invoiceMainId.Value);
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftBounceBill");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");
                        }
                        // orange #FFA926 red #D22A17
                        txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                        btn_save.Content = MainWindow.resourcemanager.GetString("trPay");


                    }
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
        private async Task getInvoiceCoupons(int invoiceId)
        {
            couponsLst = new List<Coupon>();
            lst_coupons.ItemsSource = null;
            
            if (_InvoiceType != "sd")
                selectedCoupons = await invoiceModel.GetInvoiceCoupons(invoiceId);
            else
                selectedCoupons = await invoiceModel.getOriginalCoupons(invoiceId);
            foreach (CouponInvoice invCoupon in selectedCoupons)
            {
                var cop = coupons.Where(c => c.cId == invCoupon.couponId).FirstOrDefault();

                couponsLst.Add(cop);
                lst_coupons.ItemsSource = couponsLst;
            }
        }
        private async void Btn_invoices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await newDraft();
                Window.GetWindow(this).Opacity = 0.2;

                wd_invoice w = new wd_invoice();

                // sale invoices
                string invoiceType = "s , sb";
                int duration = 1;
                w.invoiceType = invoiceType;
                w.userId = MainWindow.userLogin.userId;
                w.duration = duration; // view invoices which updated during 1 last days 
                w.condition = "salesInv";

                w.title = MainWindow.resourcemanager.GetString("trInvoices");

                if (w.ShowDialog() == true)
                {
                    if (w.invoice != null)
                    {
                        invoice = w.invoice;

                        _InvoiceType = invoice.invType;
                        _invoiceId = invoice.invoiceId;
                        isFromReport = false;
                        archived = false;
                        #region  set title and color
                        if (_InvoiceType == "s")
                        {
                            if (invoice.ChildInvoice != null)
                            {
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoiceUpdated");
                                bdr_invoiceArchive.Visibility = Visibility.Visible;
                                md_invoiceArchive.Visibility = Visibility.Visible;
                                txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trReturns");

                            }
                            else
                            {
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoice");
                            }
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                        }
                        else
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                            md_invoiceArchive.Visibility = Visibility.Visible;
                            bdr_invoiceArchive.Visibility = Visibility.Visible;
                            txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trInvoice");
                        }
                        #endregion
                        btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                       
                        navigateBtnActivate();

                        mainInvoiceItems = invoiceItems;
                        setNotifications();
                        refreshInvoiceNot(invoice.invoiceId);
                        await refreshDocCount(invoice.invoiceId);

                    }
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
        private async void Btn_ordersWait_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(executeOrderPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    await newDraft();
                    Window.GetWindow(this).Opacity = 0.2;

                    wd_invoice w = new wd_invoice();

                    // sale invoices
                    string invoiceType = "or";
                    w.invoiceType = invoiceType;
                    w.condition = "salesOrders";
                    w.branchId = MainWindow.branchID.Value;
                    w.title = MainWindow.resourcemanager.GetString("trOrders");

                    if (w.ShowDialog() == true)
                    {
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _invoiceId = invoice.invoiceId;
                            _InvoiceType = invoice.invType;
                            isFromReport = false;
                            archived = false;
                            //notifications
                            setNotifications();
                            await refreshDocCount(invoice.invoiceId);
                            md_payments.Badge = "";

                            // set title to bill
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSaleOrder");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

                            await fillInvoiceInputs(invoice);
                            invoices = FillCombo.invoices;
                            navigateBtnActivate();

                            mainInvoiceItems = invoiceItems;
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
        private async void Btn_invoiceArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Window.GetWindow(this).Opacity = 0.2;

                wd_invoice w = new wd_invoice();

                // sale invoices
                string invoiceType = "s";
                w.invoiceType = invoiceType;
                w.condition = "invoiceArchive";
                w.invoiceId = invoice.invoiceId;
                w.title = txt_invoiceArchive.Text;

                if (w.ShowDialog() == true)
                {
                    if (w.invoice != null)
                    {
                        invoice = w.invoice;
                        _invoiceId = invoice.invoiceId;
                        _InvoiceType = invoice.invType;
                        isFromReport = false;
                        archived = false;
                        //notifications
                        setNotifications();
                        refreshInvoiceNot(invoice.invoiceId);
                        refreshDocCount(invoice.invoiceId);


                        #region  set title and color
                        if (_InvoiceType == "s")
                        {

                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoiceUpdated");
                            bdr_invoiceArchive.Visibility = Visibility.Visible;
                            md_invoiceArchive.Visibility = Visibility.Visible;
                            txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trReturns");

                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                        }
                        else
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                            md_invoiceArchive.Visibility = Visibility.Visible;
                            bdr_invoiceArchive.Visibility = Visibility.Visible;
                            txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trInvoice");
                        }
                        #endregion
                        btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                        navigateBtnActivate();

                        mainInvoiceItems = invoiceItems;
                    }
                }
                Window.GetWindow(this).Opacity = 1;

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch(Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);

            }
        }
        public async Task fillInvoiceInputs(Invoice invoice)
        {
            if(isFromReport)
            {
                invoice = await invoice.GetByInvoiceId(invoice.invoiceId);
                refreshInvoiceNot(invoice.invoiceId);
            }
            if (invoice.total != null)
                _Sum = (decimal)invoice.total;
            else
                _Sum = 0;
            txt_invNumber.Text = invoice.invNumber.ToString();
            #region tax
            if (_InvoiceType == "sbd")
            {
                _Tax = 0;
                tb_taxValue.Text = "0";
            }
            else
            {
                if (invoice.taxValue != null )
                {
                    _Tax = (decimal)invoice.tax;
                    tb_taxValue.Text = SectionData.DecTostring(invoice.taxValue);
                }
                else
                {
                    _Tax = 0;
                    tb_taxValue.Text = "0";
                }
   

            }
            #endregion
            #region slice
            if (invoice.sliceId != null)
                cb_sliceId.SelectedValue = invoice.sliceId;
            #endregion
            if (invoice.agentId != null)
                cb_customer.SelectedValue = invoice.agentId;
            dp_desrvedDate.Text = invoice.deservedDate.ToString();
            tb_deliveryCost.Text = invoice.shippingCost.ToString();
            _DeliveryCost = invoice.shippingCost;
            _RealDeliveryCost = invoice.realShippingCost;

            if (invoice.totalNet != null)
            {
                if ((decimal)invoice.totalNet != 0 && invoice.totalNet != null)
                    tb_total.Text = SectionData.DecTostring((decimal)invoice.totalNet);
                else
                    tb_total.Text = "0";
            }

            cb_company.SelectedValue = invoice.shippingCompanyId;
            cb_user.SelectedValue = invoice.shipUserId;
            tb_note.Text = invoice.notes;
            if (invoice.total != 0 && invoice.total != null)
                tb_sum.Text = SectionData.DecTostring(invoice.total);
            else tb_sum.Text = "0";

            tb_discount.Text = invoice.manualDiscountValue.Equals(0) ? "0" : SectionData.PercentageDecTostring(invoice.manualDiscountValue);

            if(invoice.isPrePaid.Equals(1))
            {
                btn_save.Content = MainWindow.resourcemanager.GetString("trPay");
                chk_onDelivery.IsChecked = false;
            }
            else
            {
                btn_save.Content = MainWindow.resourcemanager.GetString("trSubmit");
                chk_onDelivery.IsChecked = true;
            }

            if (invoice.isFreeShip.Equals(1))
                chk_isFreeDelivery.IsChecked = true;

            if (invoice.manualDiscountType == "1")
                cb_typeDiscount.SelectedIndex = 1;
            else if (invoice.manualDiscountType == "2")
                cb_typeDiscount.SelectedIndex = 2;
            else
                cb_typeDiscount.SelectedIndex = 0;

            tb_barcode.Clear();
            tb_barcode.Focus();

            
            await getInvoiceCoupons(invoice.invoiceId);
            // build invoice details grid
            await buildInvoiceDetails(invoice);
            inputEditable();
            refreshTotalValue();
        }

        bool onCredit = false;
        private async void Btn_returnInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(returnPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (_InvoiceType == "s"  )
                    {
                        if (invoice.canReturn == true)
                        {
                            _InvoiceType = "sbd";
                            isFromReport = true;
                            archived = false;

                            onCredit = false;
                            if (invoice.cachTrans.Count() == 0)
                                onCredit = true;

                            if (invoice.ChildInvoice != null)
                                await fillInvoiceInputs(invoice.ChildInvoice);                              
                            else
                                await fillInvoiceInputs(invoice);

                            refreshTotalValue();
                            #region display
                            bdr_payments.Visibility = Visibility.Collapsed;
                            md_payments.Visibility = Visibility.Collapsed;
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                            //if(onCredit)
                            //    btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
                            //else
                           btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                            #endregion

                            refreshSalesNotification();
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("ReturnPeriodExceeded"), animation: ToasterAnimation.FadeIn);


                    }
                    else
                    {
                        await newDraft();
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_returnInvoice w = new wd_returnInvoice();
                        w.userId = MainWindow.userID.Value;
                        w.invoiceType = "s";

                        w.ShowDialog();
                        if (w.DialogResult == true)
                        {
                            if (w.invoice.canReturn == false)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("ReturnPeriodExceeded"), animation: ToasterAnimation.FadeIn);

                            }
                            else
                            {
                                _InvoiceType = "sbd";
                                invoice = w.invoice;
                                isFromReport = true;
                                archived = false;

                                onCredit = false;
                                if (invoice.cachTrans.Count() == 0)
                                    onCredit = true;

                                await fillInvoiceInputs(invoice);
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                                txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                                //if (onCredit)
                                //    btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
                                //else
                                    btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                            }
                        }
                        
                        Window.GetWindow(this).Opacity = 1;
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
        private async Task buildInvoiceDetails(Invoice invoice)
        {
            _Sum = 0;
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
                List<string> serialNumLst = new List<string>();

                #region valid icon
                bool isValid = true;
                long serialNum = 0;

                if (itemT.itemType == "sn")
                    serialNum = (long)itemT.quantity;
                else if (itemT.itemType.Equals("p"))
                {
                    long packageCount = (long)itemT.quantity;
                    foreach (var p in itemT.packageItems)
                    {
                        if (p.type.Equals("sn"))
                            serialNum += (long)p.itemCount * packageCount;
                    }

                }
                if ( (_InvoiceType.Equals("sbd") && invoice.invType.Equals("s") && serialNum > 0)||(invoice.invType.Equals("or") && serialNum > 0)||(invoice.invType.Equals("q") && serialNum > 0) || (_InvoiceType.Equals("sbd") && !invoice.invType.Equals("s") && itemT.itemSerials.Count() < serialNum))
                    isValid = false;
                else if((_InvoiceType == "sd" || _InvoiceType == "or" || _InvoiceType == "q") &&  serialNum > 0 && (itemT.itemSerials == null || serialNum < itemT.itemSerials.Count))
                    isValid = false;

                #endregion

                decimal itemTax = 0;
                if (itemT.itemTax != null)
                    itemTax = (decimal)itemT.itemTax;

                var unit = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemUnitId == itemT.itemUnitId).FirstOrDefault();
                billDetails.Add(new BillDetails()
                {
                    ID = _SequenceNum,
                    Product = itemT.itemName,
                    itemId = (int)itemT.itemId,
                    Unit = itemT.unitName,
                    itemUnitId = (int)itemT.itemUnitId,
                    Count = (int)itemT.quantity,
                    Price = (decimal)itemT.price,
                    Total = total,
                    itemSerials = itemT.itemSerials,
                    ItemProperties = unit.ItemProperties,
                    StoreProperties = itemT.ItemStoreProperties,
                    packageItems = itemT.packageItems,
                    type = itemT.itemType,
                    valid = isValid,
                    offerId = itemT.offerId,
                    OfferType = itemT.offerType.ToString(),
                    OfferValue = (decimal)itemT.offerValue,
                    OfferName = itemT.offerName,
                   // basicPrice = (decimal)itemT.itemUnitPrice,
                    Tax =itemTax,
                    warrantyId=itemT.warrantyId,
                    warrantyDescription=itemT.warrantyDescription,
                    warrantyName=itemT.warrantyName,
                    basicItemUnitId = (int)itemT.itemUnitId,
                    isTaxExempt = itemT.isTaxExempt,
                    VATRatio = itemT.VATRatio,
                });

                _Sum += total;
            }

            tb_barcode.Focus();

            refrishBillDetails();
        }
        private void inputEditable()
        {
            if (archived && !invoice.invType.Equals("sd"))
                _InvoiceType = "s";
            switch (_InvoiceType)
            {
                case "sbd": // sales bounce draft invoice
                    dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                    dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                    dg_billDetails.Columns[5].IsReadOnly = false; //make price writable
                    cb_sliceId.IsEnabled = false;
                    cb_customer.IsEnabled = false;
                    btn_addCustomer.IsEnabled = false;
                    btn_clearCustomer.IsEnabled = false;
                    dp_desrvedDate.IsEnabled = false;
                    tb_note.IsEnabled = false;
                    tb_barcode.IsEnabled = false;
                    btn_save.IsEnabled = true;
                    cb_company.IsEnabled = false;
                    cb_user.IsEnabled = false;
                    tb_coupon.IsEnabled = false;
                    btn_clearCoupon.IsEnabled = false;
                    tb_discount.IsEnabled = false;
                    cb_typeDiscount.IsEnabled = false;
                    btn_items.IsEnabled = false;
                    chk_onDelivery.IsEnabled = false;
                    sp_isFreeDelivery.IsEnabled = false;
                    //sp_tax.Visibility = Visibility.Collapsed;
                    bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                    md_invoiceArchive.Visibility = Visibility.Collapsed;
                    break;
                case "sd": // sales draft invoice
                    dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                    dg_billDetails.Columns[3].IsReadOnly = false;
                    dg_billDetails.Columns[4].IsReadOnly = false;
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price readonly
                    cb_sliceId.IsEnabled = true;
                    cb_customer.IsEnabled = true;
                    btn_addCustomer.IsEnabled = true;
                    btn_clearCustomer.IsEnabled = true;
                    dp_desrvedDate.IsEnabled = true;
                    tb_note.IsEnabled = true;
                    tb_barcode.IsEnabled = true;
                    btn_save.IsEnabled = true;
                    cb_company.IsEnabled = true;
                    cb_user.IsEnabled = true;
                    tb_coupon.IsEnabled = true;
                    btn_clearCoupon.IsEnabled = true;
                    btn_items.IsEnabled = true;                  
                    tb_discount.IsEnabled = true;
                    cb_typeDiscount.IsEnabled = true;
                    chk_onDelivery.IsEnabled = true;
                    chk_isFreeDelivery.IsEnabled = true;

                    bdr_payments.Visibility = Visibility.Collapsed;
                    md_payments.Visibility = Visibility.Collapsed;

                    if (AppSettings.invoiceTax_bool == false)
                        sp_tax.Visibility = Visibility.Collapsed;
                    else
                    {
                        //tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
                        sp_tax.Visibility = Visibility.Visible;
                    }
                    bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                    md_invoiceArchive.Visibility = Visibility.Collapsed;
                    break;
                case "or": //sales order
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[3].IsReadOnly = true;//make unit readonly
                    dg_billDetails.Columns[4].IsReadOnly = true; //make qty readonly
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price readonly
                    cb_sliceId.IsEnabled = false;
                    cb_customer.IsEnabled = false;
                    btn_addCustomer.IsEnabled = false;
                    btn_clearCustomer.IsEnabled = false;
                    dp_desrvedDate.IsEnabled = true;
                    tb_note.IsEnabled = true;
                    tb_barcode.IsEnabled = true;
                    cb_company.IsEnabled = true;
                    cb_user.IsEnabled = true;
                    tb_coupon.IsEnabled = true;
                    btn_clearCoupon.IsEnabled = true;
                    btn_items.IsEnabled = false;
                   
                    tb_discount.IsEnabled = true;
                    cb_typeDiscount.IsEnabled = true;
                    chk_onDelivery.IsEnabled = true;
                    sp_isFreeDelivery.IsEnabled = true;
                    //sp_tax.Visibility = Visibility.Visible;

                    bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                    md_invoiceArchive.Visibility = Visibility.Collapsed;
                    break;
                case "s": //sales invoice
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price read only
                    cb_sliceId.IsEnabled = false;
                    cb_customer.IsEnabled = false;
                    btn_clearCustomer.IsEnabled = false;
                    btn_addCustomer.IsEnabled = false;
                    dp_desrvedDate.IsEnabled = false;
                    tb_note.IsEnabled = false;
                    tb_barcode.IsEnabled = false;
                    btn_save.IsEnabled = false;

                    cb_company.IsEnabled = false;
                    cb_user.IsEnabled = false;
                    tb_coupon.IsEnabled = false;
                    btn_clearCoupon.IsEnabled = false;
                    btn_items.IsEnabled = false;
                    tb_discount.IsEnabled = false;
                    cb_typeDiscount.IsEnabled = false;
                    chk_onDelivery.IsEnabled = false;
                    sp_isFreeDelivery.IsEnabled = false;
                    //sp_tax.Visibility = Visibility.Visible;

                    if(invoice.ChildInvoice != null)
                    {
                        bdr_invoiceArchive.Visibility = Visibility.Visible;
                        md_invoiceArchive.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                        md_invoiceArchive.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "sb":
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price read only
                    cb_sliceId.IsEnabled = false;
                    cb_customer.IsEnabled = false;
                    btn_clearCustomer.IsEnabled = false;
                    btn_addCustomer.IsEnabled = false;
                    dp_desrvedDate.IsEnabled = false;
                    tb_note.IsEnabled = false;
                    tb_barcode.IsEnabled = false;
                    btn_save.IsEnabled = false;

                    cb_company.IsEnabled = false;
                    cb_user.IsEnabled = false;
                    tb_coupon.IsEnabled = false;
                    btn_clearCoupon.IsEnabled = false;
                    btn_items.IsEnabled = false;
                    tb_discount.IsEnabled = false;
                    cb_typeDiscount.IsEnabled = false;
                    chk_onDelivery.IsEnabled = false;
                    sp_isFreeDelivery.IsEnabled = false;
                    //sp_tax.Visibility = Visibility.Collapsed;

                    bdr_invoiceArchive.Visibility = Visibility.Visible;
                    md_invoiceArchive.Visibility = Visibility.Visible;
                    break;
                case "q": //qoutation invoice
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price read only
                    cb_sliceId.IsEnabled = false;
                    cb_customer.IsEnabled = false;
                    btn_clearCustomer.IsEnabled = false;
                    btn_addCustomer.IsEnabled = false;
                    dp_desrvedDate.IsEnabled = true;
                    tb_note.IsEnabled = false;
                    tb_barcode.IsEnabled = false;
                    btn_save.IsEnabled = true;

                    cb_company.IsEnabled = false;
                    cb_user.IsEnabled = false;

                    tb_coupon.IsEnabled = false;
                    btn_clearCoupon.IsEnabled = false;
                    btn_items.IsEnabled = false;
                   
                    tb_discount.IsEnabled = false;
                    cb_typeDiscount.IsEnabled = false;
                    chk_onDelivery.IsEnabled = true;
                    sp_isFreeDelivery.IsEnabled = true;
                   // sp_tax.Visibility = Visibility.Visible;

                    bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                    md_invoiceArchive.Visibility = Visibility.Collapsed;
                    break;
            }
            if (_InvoiceType.Equals("s"))
            {
                #region print - pdf - send email
                btn_printInvoice.Visibility = Visibility.Visible;
                btn_pdf.Visibility = Visibility.Visible;
                if (MainWindow.groupObject.HasPermissionAction(printCountPermission, MainWindow.groupObjects, "one"))
                {
                    btn_printCount.Visibility = Visibility.Visible;
                    bdr_printCount.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_printCount.Visibility = Visibility.Collapsed;
                    bdr_printCount.Visibility = Visibility.Collapsed;
                }
                if (MainWindow.groupObject.HasPermissionAction(sendEmailPermission, MainWindow.groupObjects, "one"))
                {
                    btn_emailMessage.Visibility = Visibility.Visible;
                    bdr_emailMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_emailMessage.Visibility = Visibility.Collapsed;
                    bdr_emailMessage.Visibility = Visibility.Collapsed;
                }

                #endregion
            }
            else
            {
                #region print - pdf - send email 
                btn_printInvoice.Visibility = Visibility.Collapsed;
                btn_pdf.Visibility = Visibility.Collapsed;
                btn_printCount.Visibility = Visibility.Collapsed;
                btn_emailMessage.Visibility = Visibility.Collapsed;
                bdr_emailMessage.Visibility = Visibility.Collapsed;

                #endregion
            }

      
            if (!isFromReport)
            {
                btn_next.Visibility = Visibility.Visible;
                btn_previous.Visibility = Visibility.Visible;
            }

            if ((_InvoiceType != "sd" && invoice.taxValue == 0) || _InvoiceType == "sbd")
            {
                sp_tax.Visibility = Visibility.Collapsed;
                tb_taxValue.Text = "0";
            }
            else if (AppSettings.invoiceTax_bool == true || invoice.taxValue > 0)
                sp_tax.Visibility = Visibility.Visible;
        }
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

        private void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
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
                        Count = (int)itemT.quantity,
                        Price = (decimal)itemT.price,
                        Total = total,
                    });
                }

                refrishBillDetails();
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

        private void Cb_customer_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var tb = cb_customer.Template.FindName("PART_EditableTextBox", cb_customer) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                cb_customer.ItemsSource = customers.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void Cb_customer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_customer.IsFocused || _InvoiceType == "sd")
                {
                    TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                    if (elapsed.TotalMilliseconds > 100 && (cb_customer.SelectedValue != null && cb_customer.SelectedValue.ToString() != "0"))
                    {
                        _SelectedCustomer = (int)cb_customer.SelectedValue;

                        if (_InvoiceType == "sd")
                            btn_updateCustomer.IsEnabled = true;

                        var c = customers.Where(x => x.agentId == _SelectedCustomer).FirstOrDefault();
                        try
                        {
                            if (SectionData.defaultPayTypeList is null)
                                SectionData.RefreshDefaultPayType();
                            txt_payType.Text = SectionData.defaultPayTypeList.Where(x => x.key == c.payType).FirstOrDefault().value;
                        }
                        catch
                        {
                            txt_payType.Text = "-";
                        }

                        try
                        {
                            if (c.agentId != 0)
                            {

                                if (c.isLimited)
                                {
                                    if (c.maxDeserve != 0)
                                    {
                                        txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("remainingCreditLimit") + ":";
                                        txt_upperLimit.Text = getCusAvailableBlnc(c).ToString();
                                        tb_moneyIcon2.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("creditLimit") + ":";
                                        txt_upperLimit.Text = MainWindow.resourcemanager.GetString("trUnlimited");
                                        tb_moneyIcon2.Visibility = Visibility.Collapsed;
                                    }
                                }
                                else
                                {
                                    txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("deptNotAvailable");
                                    txt_upperLimit.Text = "";
                                    tb_moneyIcon2.Visibility = Visibility.Collapsed;
                                }
                            }
                            else
                            {
                                txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("creditLimit") + ":";
                                txt_upperLimit.Text = "-";
                                tb_moneyIcon2.Visibility = Visibility.Collapsed;
                            }
                        }
                        catch
                        {
                            txt_upperLimitTitle.Text = MainWindow.resourcemanager.GetString("creditLimit") + ":";
                            txt_upperLimit.Text = "-";
                            tb_moneyIcon2.Visibility = Visibility.Collapsed;
                        }

                    }
                    else
                    {
                        cb_customer.SelectedValue = _SelectedCustomer;
                    }
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private decimal getCusAvailableBlnc(Agent agent)
        {
            decimal maxCredit = 0;

            float customerBalance = agent.balance;

            if (agent.balanceType == 0)
                maxCredit = agent.maxDeserve + (decimal)customerBalance;
            else
            {
                maxCredit = agent.maxDeserve - (decimal)customerBalance;
                if (maxCredit < 0)
                    maxCredit = 0;
            }
            return maxCredit;
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
        decimal _TaxValue = 0;
        List<InvoiceTaxes> invoiceTaxex;
        private void refreshTotalValue()
        {
            _Discount = 0;
            _TaxValue = 0;
            decimal totalDiscount = 0;
            decimal manualDiscount = 0;
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

            #region invoice tax value 
           
            if (_InvoiceType != "sbd" && _InvoiceType != "sb")
            {
                if (invoice != null && invoice.invType != null 
                    && (invoice.invType.Equals("s") || invoice.invType.Equals("or") || invoice.invType.Equals("q")))
                {
                    //if (invoice.tax != null)
                    //    _TaxValue = SectionData.calcPercentage(total, (decimal)invoice.tax);
                    if(invoice.taxValue != null)
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
                            invoiceTaxex.Add(new InvoiceTaxes() {
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
            }
            if (_TaxValue != 0)
                tb_taxValue.Text = SectionData.DecTostring(_TaxValue);
            else
                tb_taxValue.Text = "0";
                #endregion
            total += _TaxValue;
            if (_Sum != 0)
                tb_sum.Text = SectionData.DecTostring(_Sum);
            else
                tb_sum.Text = "0";

            if (invoice.invoiceId != 0 && invoice.invType.Equals("s"))
                _DeliveryCost = invoice.shippingCost;
            else if (_Sum > 0 && chk_isFreeDelivery.IsChecked == false)
                total += _DeliveryCost;

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
        #region billdetails

        bool firstTimeForDatagrid = true;
        async void refrishBillDetails()
        {
            dg_billDetails.ItemsSource = null;
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

        }
       async void refrishDataGridItems()
        {
            dg_billDetails.ItemsSource = null;
            dg_billDetails.ItemsSource = billDetails;
            //dg_billDetails.Items.Refresh();
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
                // process barcode

                if (e.Key.ToString() == "Return" && _BarcodeStr != "" && (_InvoiceType == "sd" || _InvoiceType == "or" || _InvoiceType == "q"))
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
                            if (tb.Name == "tb_processNum" || tb.Name == "tb_note" || tb.Name == "tb_discount")// remove barcode from text box
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
                case "si":// this barcode for invoice

                    Btn_newDraft_Click(null, null);
                    invoice = await invoiceModel.GetInvoicesByNum(barcode);
                    _InvoiceType = invoice.invType;
                    if (_InvoiceType.Equals("sd") || _InvoiceType.Equals("s") || _InvoiceType.Equals("sbd") || _InvoiceType.Equals("sb"))
                    {
                        // set title to bill
                        if (_InvoiceType == "sd")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesDraft");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");
                        }
                        else if (_InvoiceType == "s")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");
                        }
                        else if (_InvoiceType == "sbd")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnDraft");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                        }
                        else if (_InvoiceType == "sb")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
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
                        else if (exist != null)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorCouponExist"), animation: ToasterAnimation.FadeIn);
                        }
                        tb_coupon.Text = "";
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
                            if (unit1.itemCount == 0)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);
                            }
                            else
                            {

                                int itemId = (int)unit1.itemId;
                                if (unit1.itemId != 0)
                                {
                                    //decimal price = 0;
                                    decimal basicPrice = 0;
                                    if (unit1.SalesPrices == null || (unit1.SalesPrices != null && unit1.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                                    {
                                        basicPrice = (decimal)unit1.basicPrice;
                                        //if (AppSettings.itemsTax_bool == true)
                                        //    price = (decimal)unit1.priceTax;
                                        //else
                                        //    price = (decimal)unit1.price;
                                    }
                                    else
                                    {
                                        var slice = unit1.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                                        basicPrice = (decimal)slice.basicPrice;
                                        //if (AppSettings.itemsTax_bool == true)
                                        //    price = (decimal)slice.priceTax;
                                        //else
                                        //    price = (decimal)slice.price;


                                    }
                                    int? warrantyId = null;
                                    string warrantName = "";
                                    string warrantyDesc = "";
                                    if (unit1.hasWarranty)
                                    {
                                        warrantyId = unit1.warrantyId;
                                        warrantyDesc = unit1.warrantyDescription;
                                        warrantName = unit1.warrantyName;
                                    }
                                    await addItemToBill(itemId, unit1.itemUnitId, unit1.mainUnit, basicPrice, warrantyId, warrantName, warrantyDesc);
                                    refreshTotalValue();
                                    refrishBillDetails();
                                }
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
        private async Task addItemToBill(int itemId, int itemUnitId, string unitName, decimal price, int? warrantyId,string warrantyName,string warrantyDesc)
        {
            item = items.ToList().Find(i => i.itemId == itemId);
            bool isValid = true;


            int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == itemUnitId).FirstOrDefault());
            if (item.type == "sn" ||
                ( item.type.Equals("p") && item.packageItems != null && item.packageItems.Where(x => x.type =="sn").FirstOrDefault() != null))
            {
                isValid = false;
            }

            int count = 0;
            int? offerId = null;

            #region availavble quantity
            int availableAmount = 0;
            if (item.type != "sr")
                availableAmount = (int)await getAvailableAmount(item.itemId, (int)item.itemUnitId, MainWindow.branchID.Value, index+1);
           
            #endregion
            if (index == -1)//item doesn't exist in bill
            {
                // get item units
                itemUnits = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemId == item.itemId).ToList();

                count = 1;
                //decimal tax = 0;
                ////decimal total = 0;
                //if (AppSettings.itemsTax_bool == true)
                //    tax = (decimal)(count * item.taxes);

                
                string discountType = "1";
                decimal discountValue = 0;
                if (item.offerId != null)
                {
                    offerId = (int)item.offerId;
                    discountType = item.discountType;
                    discountValue = (decimal)item.discountValue;
                }

                #region check available amount
                if (availableAmount < count && item.type != "sr")
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                    if (offerId != null && offerId != 0)
                    {
                        offer = new ItemUnitOffer();
                        int remainAmount = (int)await offer.getRemain((int)offerId,(int) item.itemUnitId);
                        if (remainAmount < availableAmount)
                            availableAmount = remainAmount;
                    }
                    count = availableAmount;
                }
                else if (offerId != null &&  offerId != 0)
                {
                    offer = new ItemUnitOffer();
                    int remainAmount = (int)await offer.getRemain((int)offerId,(int) item.itemUnitId);
                    if (remainAmount < count)
                    {
                        availableAmount = remainAmount;
                        count = availableAmount;
                    }
                }
                #endregion
                addRowToBill(item.name, item.itemId, unitName, itemUnitId, count, price, item.type, isValid, offerId, discountType, discountValue,item.packageItems,warrantyId,warrantyName,warrantyDesc);
            }
            else // item exist prevoiusly in list
            {
                count = billDetails[index].Count + 1;
                offerId = billDetails[index].offerId;
                #region check available amount
                if (availableAmount < count && item.type != "sr")
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                    if (offerId != null && offerId != 0)
                    {
                        offer = new ItemUnitOffer();
                        int remainAmount = (int)await offer.getRemain((int)offerId, (int)item.itemUnitId);
                        if (remainAmount < availableAmount)
                            availableAmount = remainAmount;
                    }
                    count = availableAmount;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorNoEnoughQuantityToolTip") , animation: ToasterAnimation.FadeIn);
                }
                else if (offerId != null && offerId != 0)
                {
                    offer = new ItemUnitOffer();
                    int remainAmount = (int)await offer.getRemain((int)offerId, (int)item.itemUnitId);
                    if (remainAmount < count)
                    {
                        availableAmount = remainAmount;
                        count = availableAmount;
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorNoEnoughQuantityToolTip"), animation: ToasterAnimation.FadeIn);
                    }
                }
                #endregion

                decimal itemTax = 0;
                if (item.taxes != null)
                    itemTax = (decimal)item.taxes;

                _Sum -= billDetails[index].Count * billDetails[index].Price;
                billDetails[index].Count = count;
                billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;
                billDetails[index].valid = isValid;

                _Sum += billDetails[index].Count * billDetails[index].Price;

            }
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
                    //if (_BarcodeStr != "" && _BarcodeStr.Length != 1)
                    //    barcode = _BarcodeStr;
                    //else
                    if (_BarcodeStr.Length < 13)
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

        private void addRowToBill(string itemName, int itemId, string unitName, int itemUnitId, int count, decimal price,
                                     string type, bool valid, int? offerId, string offerType, decimal offerValue,
                                    List<Item> packageItems,int? warrantyId,string warrantyName,string warrantyDesc)
        {
            // increase sequence for each read
            _SequenceNum++;
            decimal vatRatio = 0;
            if(AppSettings.itemsTax_bool == true)
            {
                if (!item.isTaxExempt)
                    vatRatio = (decimal)AppSettings.itemsTax_decimal;
            }
            billDetails.Add(new BillDetails()
            {
                ID = _SequenceNum,
                Product = item.name,
                itemId = item.itemId,
                isTaxExempt = item.isTaxExempt,
                Unit = unitName,
                itemUnitId = itemUnitId,
                Count = count,
                Price = price,
                Total = count * price,
                type = type,
                valid = valid,
                offerId = offerId,
                OfferType = offerType,
                OfferName = item.offerName,
                OfferValue = offerValue,
               // basicPrice = basicPrice,
                packageItems = packageItems,
                warrantyId = warrantyId,
                warrantyDescription= warrantyDesc,
                warrantyName = warrantyName,
                ItemProperties = item.ItemProperties,
                VATRatio = vatRatio,
            });
            _Sum += count * price;

        }
        #endregion billdetails
        private async void Cbm_unitItemDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
               
                var cmb = sender as ComboBox;
                TextBlock tb;
                if (dg_billDetails.SelectedIndex != -1 && cmb != null)
                {
                    int _datagridSelectedIndex = dg_billDetails.SelectedIndex;
                    int itemUnitId = (int)cmb.SelectedValue;
                    var iu = (ItemUnit)cmb.SelectedItem;
                    billDetails[_datagridSelectedIndex].Unit = iu.mainUnit;

                    billDetails[_datagridSelectedIndex].itemUnitId = (int)cmb.SelectedValue;
                    #region Dina

                    Item unit = new Item();

                    unit = MainWindow.InvoiceGlobalSaleUnitsList.Find(x => x.itemUnitId == (int)cmb.SelectedValue && x.itemId == billDetails[_datagridSelectedIndex].itemId);

                    int oldCount = 0;
                    long newCount = 0;
                    decimal oldPrice = 0;
                    decimal itemTax = 0;
                    decimal newPrice = 0;

                    oldCount = billDetails[_datagridSelectedIndex].Count;
                    oldPrice = billDetails[_datagridSelectedIndex].Price;
                    newCount = oldCount;
                    tb = dg_billDetails.Columns[4].GetCellContent(dg_billDetails.Items[_datagridSelectedIndex]) as TextBlock;
                    billDetails[_datagridSelectedIndex].OfferType = "1";
                    billDetails[_datagridSelectedIndex].OfferValue = 0;
                    billDetails[_datagridSelectedIndex].offerId = null;
                    billDetails[_datagridSelectedIndex].OfferName = "";


                    #region invoice is sales bounce draft
                    if (_InvoiceType == "sbd")
                    {
                        billDetails[_datagridSelectedIndex].Count = (int)newCount;

                        var selectedItemUnitId = itemUnitId;

                        var itemUnitsIds = barcodesList.Where(x => x.itemId == billDetails[_datagridSelectedIndex].itemId).Select(x => x.itemUnitId).Distinct().ToList();

                        #region caculate available amount in this bill
                        int amountInBill = (int)await getAmountInBill(billDetails[_datagridSelectedIndex].itemId, itemUnitId, MainWindow.branchID.Value, billDetails[_datagridSelectedIndex].ID);
                        #endregion

                        #region calculate amount in sales invoice
                        var items = mainInvoiceItems.ToList().Where(i => itemUnitsIds.Contains((int)i.itemUnitId));
                        int buyedAmount = 0;
                        foreach (ItemTransfer it in items)
                        {
                            if (selectedItemUnitId == (int)it.itemUnitId)
                                buyedAmount += (int)it.quantity;
                            else
                                buyedAmount += (int)await itemUnitModel.fromUnitToUnitQuantity((int)it.quantity, billDetails[_datagridSelectedIndex].itemId, (int)it.itemUnitId, selectedItemUnitId);
                        }
                        #endregion

                        if (newCount > (buyedAmount - amountInBill))
                        {
                            // return old value 
                            tb.Text = (buyedAmount - amountInBill).ToString();

                            newCount = (buyedAmount - amountInBill);
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                    #endregion
                    else
                    {
                        int availableAmount = (int)await getAvailableAmount(billDetails[_datagridSelectedIndex].itemId, (int)unit.itemUnitId, MainWindow.branchID.Value, billDetails[_datagridSelectedIndex].ID);
                        if (availableAmount < newCount && billDetails[_datagridSelectedIndex].type != "sr")
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                            if (unit.offerId != null && (int)unit.offerId != 0)
                            {
                                offer = new ItemUnitOffer();
                                int remainAmount = (int)await offer.getRemain((int)unit.offerId, (int)unit.itemUnitId);
                                if (remainAmount < availableAmount)
                                    availableAmount = remainAmount;
                            }
                            newCount = availableAmount;
                            tb.Text = newCount.ToString();
                            billDetails[_datagridSelectedIndex].Count = (int)newCount;
                        }
                        else if (unit.offerId != null && (int)unit.offerId != 0)
                        {
                            offer = new ItemUnitOffer();
                            int remainAmount = (int)await offer.getRemain((int)unit.offerId, (int)unit.itemUnitId);
                            if (remainAmount < newCount)
                            {
                                availableAmount = remainAmount;
                                newCount = availableAmount;
                                tb.Text = newCount.ToString();
                                billDetails[_datagridSelectedIndex].Count = (int)newCount;
                                billDetails[_datagridSelectedIndex].OfferType = unit.discountType;
                                billDetails[_datagridSelectedIndex].OfferValue = (int)unit.discountValue;
                                billDetails[_datagridSelectedIndex].offerId = unit.offerId;
                                billDetails[_datagridSelectedIndex].OfferName = unit.offerName;

                            }

                        }
                        
                        if(unit.offerId == null)
                        {
                            billDetails[_datagridSelectedIndex].OfferType = "";
                            billDetails[_datagridSelectedIndex].OfferValue = 0;
                            billDetails[_datagridSelectedIndex].offerId = null;
                        }

                        int? warrantyId = null;
                        string warrantName = "";
                        string warrantyDesc = "";
                        if (unit.hasWarranty)
                        {
                            warrantyId = unit.warrantyId;
                            warrantyDesc = unit.warrantyDescription;
                            warrantName = unit.warrantyName;
                        }
                        billDetails[_datagridSelectedIndex].warrantyId = warrantyId;
                        billDetails[_datagridSelectedIndex].warrantyName = warrantName;
                        billDetails[_datagridSelectedIndex].warrantyDescription = warrantyDesc;
                    }

                    #region change unit price
                    if (unit.SalesPrices == null || (unit.SalesPrices != null && unit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault() == null))
                    {
                        //if (AppSettings.itemsTax_bool == true)
                        //    newPrice = (decimal)unit.priceTax;
                        //else
                            newPrice = (decimal)unit.price;
                    }
                    else
                    {
                        var slice = unit.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                        if (slice != null)
                        {
                            //if (AppSettings.itemsTax_bool == true)
                            //    newPrice = (decimal)slice.priceTax;
                            //else
                                newPrice = (decimal)slice.price;
                        }

                    }
                    #endregion
                    tb = dg_billDetails.Columns[5].GetCellContent(dg_billDetails.Items[_datagridSelectedIndex]) as TextBlock;
                    tb.Text = newPrice.ToString();

                    // old total for changed item
                    decimal total = oldPrice * oldCount;
                    _Sum -= total;


                    // new total for changed item
                    total = newCount * newPrice;
                    _Sum += total;

                    #region items tax
                    if (item.taxes != null)
                        itemTax = (decimal)item.taxes;

                    #endregion

                    refreshTotalValue();

                    // update item in billdetails           
                    billDetails[_datagridSelectedIndex].Count = (int)newCount;
                    billDetails[_datagridSelectedIndex].Price = newPrice;
                    billDetails[_datagridSelectedIndex].Total = total;
                    #region update unit properties
                    billDetails[dg_billDetails.SelectedIndex].ItemProperties = unit.ItemProperties;
                    #endregion
                    #region update row valid
                    if (_InvoiceType.Equals("sbd") && !billDetails[_datagridSelectedIndex].itemUnitId.Equals(billDetails[_datagridSelectedIndex].basicItemUnitId))
                    {
                        billDetails[_datagridSelectedIndex].valid = true;
                        billDetails[_datagridSelectedIndex].returnedSerials = null ;
                        billDetails[_datagridSelectedIndex].ReturnedProperties = null;
                    }
                    else 
                    {
                        var hasSerial = false;
                        if (billDetails[_datagridSelectedIndex].type == "sn")
                            hasSerial = true;
                        else if (billDetails[_datagridSelectedIndex].type.Equals("p"))
                        {
                            long packageCount = (long)billDetails[_datagridSelectedIndex].Count;

                            foreach (var p in billDetails[_datagridSelectedIndex].packageItems)
                            {
                                if (p.type.Equals("sn"))
                                {
                                    hasSerial = true;
                                    break;
                                }
                            }

                        }

                        billDetails[_datagridSelectedIndex].valid = !hasSerial;
                        if (_InvoiceType.Equals("sbd"))
                        {
                            billDetails[_datagridSelectedIndex].returnedSerials = null;
                            billDetails[_datagridSelectedIndex].ReturnedProperties = null;
                        }
                        else if (_InvoiceType.Equals("sd"))
                        {
                            billDetails[_datagridSelectedIndex].itemSerials = new List<Serial>();
                            billDetails[_datagridSelectedIndex].StoreProperties = new List<StoreProperty>();
                        }
                    }
                    
                    #endregion
                    refrishBillDetails();
                    #endregion
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cbm_unitItemDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                //billDetails
                if (billDetails.Count == 1)
                {
                    var cmb = sender as ComboBox;
                    cmb.SelectedValue = (int)billDetails[0].itemUnitId;

                    if (_InvoiceType == "s" || _InvoiceType == "sb" || _InvoiceType == "or")
                        cmb.IsEnabled = false;
                    else
                        cmb.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void DataGrid_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
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

                                combo.SelectedValue = (int)item.itemUnitId;

                                if (_InvoiceType == "s" || _InvoiceType == "sb" || _InvoiceType == "or")
                                    combo.IsEnabled = false;
                                else
                                    combo.IsEnabled = true;
                            }
                        }
                    }

                    count++;
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Dg_billDetails_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            int column = dg_billDetails.CurrentCell.Column.DisplayIndex;
            if (_InvoiceType == "s" || _InvoiceType == "sb")
                e.Cancel = true;
        }

        #region
        public DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
                return (DataGridCell)cellContent.Parent;

            return null;
        }
        #endregion
        #region
        static DataGridCell TryToFindGridCell(DataGrid grid, DataGridCellInfo cellInfo)
        {
            DataGridCell result = null;
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(cellInfo.Item);
            if (row != null)
            {
                int columnIndex = grid.Columns.IndexOf(cellInfo.Column);
                if (columnIndex > -1)
                {
                    DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);
                    result = presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
                }
            }
            return result;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        #endregion


        private async void Dg_billDetails_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                TextBlock tb = new TextBlock();
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
                    {
                        newCount = int.Parse(t.Text);
                        if (row.type == "sn" || (row.type.Equals("p") && row.packageItems != null && row.packageItems.Where(x => x.type == "sn").FirstOrDefault() != null))
                            billDetails[index].valid = false;
                        if (newCount < 0)
                        {
                            newCount = 0;
                            t.Text = "0";
                        }

                    }
                    else
                        newCount = row.Count;

                    oldCount = row.Count;
                    oldPrice = row.Price;
                    #region invoice is sales bounce draft
                    if (_InvoiceType == "sbd")
                    {

                        billDetails[index].itemUnitId = row.itemUnitId;

                        var selectedItemUnitId = row.itemUnitId;

                        var itemUnitsIds = barcodesList.Where(x => x.itemId == row.itemId).Select(x => x.itemUnitId).ToList();

                        #region caculate available amount in this bill
                        int amountInBill = (int)await getAmountInBill(row.itemId, row.itemUnitId, MainWindow.branchID.Value, row.ID);
                        #endregion
                        #region calculate amount in sales invoice
                        var items = mainInvoiceItems.ToList().Where(i => itemUnitsIds.Contains((int)i.itemUnitId));
                        int buyedAmount = 0;
                        foreach (ItemTransfer it in items)
                        {
                            if (selectedItemUnitId == (int)it.itemUnitId)
                                buyedAmount += (int)it.quantity;
                            else
                                buyedAmount += (int)await itemUnitModel.fromUnitToUnitQuantity((int)it.quantity, row.itemId, (int)it.itemUnitId, selectedItemUnitId);
                        }
                        #endregion

                        if (newCount > (buyedAmount - amountInBill))
                        {
                            // return old value 
                            t.Text = (buyedAmount - amountInBill).ToString();

                            newCount = (buyedAmount - amountInBill);
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                    #endregion
                    else
                    {
                        int availableAmount = 0;
                        if (row.type != "sr")
                            availableAmount = (int)await getAvailableAmount(row.itemId, row.itemUnitId, MainWindow.branchID.Value, row.ID);

                        if (availableAmount < newCount && row.type != "sr")
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                            if (row.offerId != null && (int)row.offerId != 0)
                            {
                                offer = new ItemUnitOffer();
                                int remainAmount = (int)await offer.getRemain((int)row.offerId, row.itemUnitId);
                                if (remainAmount < availableAmount)
                                    availableAmount = remainAmount;
                            }
                            newCount = availableAmount;
                            tb = dg_billDetails.Columns[4].GetCellContent(dg_billDetails.Items[index]) as TextBlock;
                            tb.Text = newCount.ToString();
                            row.Count = (int)newCount;
                        }
                        else if (row.offerId != null && (int)row.offerId != 0)
                        {
                            offer = new ItemUnitOffer();
                            int remainAmount = (int)await offer.getRemain((int)row.offerId, row.itemUnitId);
                            if (remainAmount < newCount)
                            {
                                availableAmount = remainAmount;
                                newCount = availableAmount;
                                tb = dg_billDetails.Columns[4].GetCellContent(dg_billDetails.Items[index]) as TextBlock;
                                tb.Text = newCount.ToString();
                                row.Count = (int)newCount;
                            }
                        }
                    }

                    if (columnName == MainWindow.resourcemanager.GetString("trPrice"))
                        newPrice = decimal.Parse(t.Text);
                    else
                        newPrice = row.Price;



                    // old total for changed item
                    decimal total = oldPrice * oldCount;
                    _Sum -= total;


                    // new total for changed item
                    total = newCount * newPrice;
                    _Sum += total;

                    decimal itemTax = 0;
                    if (item.taxes != null)
                        itemTax = (decimal)item.taxes;

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
                refrishDataGridItems();

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async Task<decimal> getAmountInBill(int itemId, int itemUnitId, int branchId, int ID)
        {
            int quantity = 0;
            var itemUnits = barcodesList.Where(a => a.itemId == itemId).Select(x => x.itemUnitId).Distinct().ToList();
            var smallUnits = await itemUnitModel.getSmallItemUnits(itemId, itemUnitId);
            foreach (int itemUnit in itemUnits)
            {
                var isInBill = billDetails.ToList().Find(x => x.itemUnitId == (int)itemUnit && x.ID != ID); // unit exist in invoice
                if (isInBill != null)
                {
                    var isSmall = smallUnits.Find(x => x.itemUnitId == (int)itemUnit);
                    int unitValue = 0;

                    int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == itemUnit && p.ID != ID).FirstOrDefault());
                    int count = billDetails[index].Count;
                    if (itemUnitId == itemUnit)
                    {
                        quantity += count;
                    }
                    else if (isSmall != null) // from-unit is bigger than to-unit
                    {
                        unitValue = (int)await itemUnitModel.largeToSmallUnitQuan(itemUnitId, (int)itemUnit);
                        quantity += count / unitValue;
                    }
                    else
                    {
                        unitValue = (int)await itemUnitModel.smallToLargeUnit(itemUnitId, (int)itemUnit);

                        if (unitValue != 0)
                        {
                            quantity += count * unitValue;
                        }
                    }

                }
            }
            return quantity;
        }
        private async Task<decimal> getAvailableAmount(int itemId, int itemUnitId, int branchId, int ID)
        {
            var itemUnits = MainWindow.InvoiceGlobalSaleUnitsList.Where(a => a.itemId == itemId).ToList();
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
        private void Dp_date_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                _Sender = sender;
                moveControlToBarcode(sender, e);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void moveControlToBarcode(object sender, KeyEventArgs e)
        {
            try
            {
                DatePicker dt = sender as DatePicker;
                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds < 100)
                {
                    tb_barcode.Focus();
                    HandleKeyPress(sender, e);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        //
        #region print-email
        public async Task<string> SaveSalepdf(Invoice prInvoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> payedList)
        {
            string msg = "";
            //for email
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string pdfpath = "";

            // prInvoice;

            //
            if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                                       || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                                       || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
            {
                //  Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                msg = "trPrintDraftInvoice";
            }
            else
            {


                if (prInvoice.invoiceId > 0)
                {
                    ////////////////////////
                    string folderpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath) + @"\Thumb\report\";
                    ReportCls.clearFolder(folderpath);

                    pdfpath = @"\Thumb\report\File" + DateTime.Now.ToFileTime().ToString() + ".pdf";
                    pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                    // get user
                    User employ = new User();
                    //employ = await userModel.getUserById((int)prInvoice.updateUserId);
                    if (FillCombo.usersAllList is null)
                    { await FillCombo.RefreshAllUsers(); }
                    employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                    prInvoice.uuserName = employ.name;
                    prInvoice.uuserLast = employ.lastname;
                    //end
                    //get user
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
                        prInvoice.agentAddress = agentinv.address;
                        prInvoice.agentMobile = agentinv.mobile;
                       

                    }
                    else
                    {
                        prInvoice.agentCode = "-";
                        prInvoice.agentName = "-";
                        prInvoice.agentCompany = "-";
                        prInvoice.agentAddress ="-";
                        prInvoice.agentMobile = "-";
                    }
                    //end

                   // ReportCls.checkInvLang();
                    ReportCls.checkInvLang();
                    //get branch
                    Branch branch = new Branch();
                    if (FillCombo.branchsAllList is null)
                    { await FillCombo.RefreshBranchsAll(); }
                    branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                    //  branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                    if (branch.branchId > 0)
                    {
                        prInvoice.branchName = branch.name;
                    }
                    //end branch
                    decimal totaltax = 0;
                    foreach (var i in invoiceItems)
                    {
                        i.price = decimal.Parse(SectionData.DecTostring(i.price));
                        if (i.itemTax != null)
                        {
                            totaltax += (decimal)i.itemTax;

                        }

                        i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                    }
                    if (totaltax > 0 && prInvoice.invType != "sbd" && prInvoice.invType != "sb")
                    {
                        paramarr.Add(new ReportParameter("itemtax_note", (prInvoice.itemtax_note == null || prInvoice.itemtax_note == "") ? "" : prInvoice.itemtax_note.Trim()));
                       // paramarr.Add(new ReportParameter("itemtax_note", prInvoice.itemtax_note.Trim()));
                        paramarr.Add(new ReportParameter("hasItemTax", "1"));
                      
                    }
                    else
                    {
                        // paramarr.Add(new ReportParameter("itemtax_note", MainWindow.itemtax_note.Trim()));
                        paramarr.Add(new ReportParameter("hasItemTax", "0"));
                    }
                    reportSize repsize = new reportSize();
                    clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                   itemscount= invoiceItems.Count();

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
                    paramarr.Add(new ReportParameter("isSaved", "y"));
                    paramarr = reportclass.fillSaleInvReport(prInvoice, paramarr, shippingcomp);
                    rep = reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                    // multiplePaytable(paramarr);
                    //   if ((prInvoice.invType == "s" || prInvoice.invType == "sd" || prInvoice.invType == "sbd" || prInvoice.invType == "sb"))
                    {
                        CashTransfer cachModel = new CashTransfer();
                        //   List<PayedInvclass> payedList = new List<PayedInvclass>();
                        // payedList = prInvoice.cachTrans;
                        //  payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);

                        if (prInvoice.cachTrans == null)
                        {
                            payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                        }
                        else
                        {
                            payedList = clsrep.payedConvert(prInvoice.cachTrans);
                            // payedList = prInvoice.cachTrans;
                        }
                        //  clsReports clsrep = new clsReports();
                        List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                        foreach (var p in payedList)
                        {

                            if (p.processType == "cash")
                            {
                                p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                            }
                            else
                            {
                                p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                            }

                        }
                        mailpayedList = payedList;
                        decimal sump = payedList.Sum(x => x.cash).Value;
                        decimal deservd = (decimal)prInvoice.totalNet - sump + prInvoice.cashReturn;
                        paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                        paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                        paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                        rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


                    }
                    //if (invoice.invType == "s" )
                    //{
                    //    CashTransfer cachModel = new CashTransfer();
                    //    List<PayedInvclass> payedList = new List<PayedInvclass>();
                    //    payedList = await cachModel.GetPayedByInvId(invoice.invoiceId);
                    //    rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));
                    //}


                    rep.SetParameters(paramarr);
                    rep.Refresh();

                    //if (MainWindow.salePaperSize != "A4")
                        if (repsize.paperSize != "A4")
                    {
                        LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                    }
                    else
                    {
                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                    }
                }

            }
            return pdfpath;
        }

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                //Thread t1 = new Thread(() =>
                //{
                pdfPurInvoice();
                //});
                //t1.Start();
                //////////////////////////////////////
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

        public async void pdfPurInvoice()
        {
            try
            {

                prInvoice = invoice;
                // prInvoice = new Invoice();//
                //if (prinvoiceId != 0)
                //    prInvoice = await invoiceModel.GetByInvoiceId(prinvoiceId);
                //else
                // prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);//

                if (int.Parse(AppSettings.Allow_print_inv_count) <= prInvoice.printedcount)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);

                    });
                }
                else
                {

                    if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                             || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                             || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                        });
                    }
                    else
                    {
                        clsReports clsrep = new clsReports();
                        List<ReportParameter> paramarr = new List<ReportParameter>();
                        if (prInvoice.invoiceId > 0)
                        {
                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            //get user
                            User employ = new User();
                            //  employ = await userModel.getUserById((int)prInvoice.updateUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();
                            prInvoice.uuserName = employ.name;
                            prInvoice.uuserLast = employ.lastname;
                            //end
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
                                prInvoice.agentAddress = agentinv.address;
                                prInvoice.agentMobile = agentinv.mobile;
                            }
                            else
                            {
                                prInvoice.agentCode = "-";
                                prInvoice.agentName = "-";
                                prInvoice.agentCompany = "-";
                                prInvoice.agentAddress ="-";
                                prInvoice.agentMobile = "-";
                            }
                            ReportCls.checkInvLang();
                            //get branch
                            Branch branch = new Branch();
                            //  branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                            if (FillCombo.branchsAllList is null)
                            { await FillCombo.RefreshBranchsAll(); }
                            branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                            if (branch.branchId > 0)
                            {
                                prInvoice.branchName = branch.name;
                            }
                            //end branch
                            //shipping
                            ShippingCompanies shippingcom = new ShippingCompanies();
                            if (prInvoice.shippingCompanyId > 0)
                            {
                                // shippingcom = await shippingcom.GetByID((int)prInvoice.shippingCompanyId);
                                if (FillCombo.shippingCompaniesAllList is null)
                                { await FillCombo.RefreshShippingCompaniesAll(); }
                                shippingcom = FillCombo.shippingCompaniesAllList.Where(s => s.shippingCompanyId == (int)prInvoice.shippingCompanyId).FirstOrDefault();

                            }
                            User shipuser = new User();
                            if (prInvoice.shipUserId > 0)
                            {
                                // shipuser = await userModel.getUserById((int)prInvoice.shipUserId);
                                if (FillCombo.usersAllList is null)
                                { await FillCombo.RefreshAllUsers(); }
                                shipuser = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.shipUserId).FirstOrDefault();

                            }
                            prInvoice.shipUserName = shipuser.name + " " + shipuser.lastname;
                            //end shipping

                            decimal totaltax = 0;
                            foreach (var i in invoiceItems)
                            {
                                i.price = decimal.Parse(SectionData.DecTostring(i.price));
                                if (i.itemTax != null)
                                {
                                    totaltax += (decimal)i.itemTax;

                                }

                                i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                            }
                            if (totaltax > 0 && prInvoice.invType != "sbd" && prInvoice.invType != "sb")
                            {
                                paramarr.Add(new ReportParameter("itemtax_note", (prInvoice.itemtax_note == null || prInvoice.itemtax_note == "") ? "" : prInvoice.itemtax_note.Trim()));
                              //  paramarr.Add(new ReportParameter("itemtax_note", prInvoice.itemtax_note.Trim()));
                                paramarr.Add(new ReportParameter("hasItemTax", "1"));

                            }
                            else
                            {
                                // paramarr.Add(new ReportParameter("itemtax_note", MainWindow.itemtax_note.Trim()));
                                paramarr.Add(new ReportParameter("hasItemTax", "0"));
                            }
                            reportSize repsize = new reportSize();
                            clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            itemscount = invoiceItems.Count();
                            ///printer
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
                            paramarr.Add(new ReportParameter("isSaved", "y"));
                            paramarr = reportclass.fillSaleInvReport(prInvoice, paramarr, shippingcom);
                            rep= reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                            //  multiplePaytable(paramarr);
                            //     if ((prInvoice.invType == "s" || prInvoice.invType == "sd" || prInvoice.invType == "sbd" || prInvoice.invType == "sb"))
                            {
                                CashTransfer cachModel = new CashTransfer();
                                List<PayedInvclass> payedList = new List<PayedInvclass>();



                                if (prInvoice.cachTrans == null)
                                {
                                    payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                                }
                                else
                                {
                                    payedList = clsrep.payedConvert(prInvoice.cachTrans);
                                    // payedList = prInvoice.cachTrans;
                                }
                              List<PayedInvclass> repPayedList= clsrep.cashPayedinvoice(payedList, prInvoice);
                                // payedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                                foreach (var p in payedList)
                                {

                                    if (p.processType == "cash")
                                    {
                                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                                    }
                                    else
                                    {
                                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                    }

                                }

                                decimal sump = payedList.Sum(x => x.cash).Value;
                                decimal deservd = (decimal)prInvoice.totalNet - sump + prInvoice.cashReturn;
                                paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                                paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                                paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


                            }
                            rep.SetParameters(paramarr);


                            rep.Refresh();



                            saveFileDialog.Filter = "PDF|*.pdf;";
                            bool? savdialog = false;
                            this.Dispatcher.Invoke(() =>
                            {
                                savdialog = saveFileDialog.ShowDialog();

                            });


                            if (savdialog == true)
                            {

                                string filepath = saveFileDialog.FileName;

                                //copy count
                                if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb")
                                {

                                    paramarr.Add(new ReportParameter("isOrginal", false.ToString()));


                                    //if (i > 1)
                                    //{
                                    //    // update paramarr->isOrginal
                                    //    foreach (var item in paramarr.Where(x => x.Name == "isOrginal").ToList())
                                    //    {
                                    //        StringCollection myCol = new StringCollection();
                                    //        myCol.Add(prInvoice.isOrginal.ToString());
                                    //        item.Values = myCol;


                                    //    }
                                    //    //end update

                                    //}
                                    rep.SetParameters(paramarr);

                                    rep.Refresh();

                                    if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
                                    {

                                        this.Dispatcher.Invoke(() =>
                                        {
                                            //if (MainWindow.salePaperSize != "A4")
                                                if (repsize.paperSize != "A4")
                                            {
                                                LocalReportExtensions.customExportToPDF(rep, filepath, repsize.width, repsize.height);
                                            }
                                            else
                                            {
                                                LocalReportExtensions.ExportToPDF(rep, filepath);
                                            }

                                        });


                                        decimal res = await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);



                                        prInvoice.printedcount = prInvoice.printedcount + 1;

                                        prInvoice.isOrginal = false;


                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);

                                        });

                                    }


                                }
                                else
                                {

                                    this.Dispatcher.Invoke(() =>
                                    {

                                        
                                        
                                         //if (MainWindow.salePaperSize != "A4")
                                            if (repsize.paperSize != "A4")
                                        {
                                            LocalReportExtensions.customExportToPDF(rep, filepath, repsize.width, repsize.height);
                                        }
                                        else
                                        {
                                            LocalReportExtensions.ExportToPDF(rep, filepath);
                                        }

                                    });

                                }
                                // end copy count



                            }


                        }
                    }
                }

            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: "Not completed", animation: ToasterAnimation.FadeIn);

                });
            }


        }
        public async void multiplePaytable(List<ReportParameter> paramarr)
        {
        //    if ((prInvoice.invType == "s" || prInvoice.invType == "sd" || prInvoice.invType == "sbd" || prInvoice.invType == "sb"))
            {
                CashTransfer cachModel = new CashTransfer();
                List<PayedInvclass> payedList = new List<PayedInvclass>();
                payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                clsReports clsrep = new clsReports();
                List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                foreach (var p in payedList)
                {

                    if (p.processType == "cash")
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                    }
                    else
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    }

                }
                mailpayedList = payedList;
                decimal sump = payedList.Sum(x => x.cash).Value;
                decimal deservd = (decimal)prInvoice.totalNet - sump + prInvoice.cashReturn;
                paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


            }
        }
        public async Task<string> printInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> payedList)
        {
            string msg = "";
            try
            {
                clsReports clsrep = new clsReports();
                payedList = clsrep.payedConvert(payedList);

                //   prInvoice = new Invoice();//
                //if (isdirect)
                //{
                //     prInvoice = await invoiceModel.GetByInvoiceId(invoiceId);//
                //}
                //else
                //{
                //    prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                //}

                ////if (prinvoiceId != 0)
                ////    prInvoice = await invoiceModel.GetByInvoiceId(prinvoiceId);
                //else


                //  int resu=await  invoiceModel.updateprintstat(prInvoice.invoiceId, 1, true, false);
                if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                    || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                    || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                {
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                    //});
                    msg = "trPrintDraftInvoice";
                }
                else
                {

                    List<ReportParameter> paramarr = new List<ReportParameter>();

                    if (prInvoice.invoiceId > 0)
                    {
                        //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);


                        User employ = new User();
                        // employ = await userModel.getUserById((int)prInvoice.updateUserId);
                        //get user 
                        if (FillCombo.usersAllList is null)
                        { await FillCombo.RefreshAllUsers(); }
                        employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                        prInvoice.uuserName = employ.name;
                        prInvoice.uuserLast = employ.lastname;
                        // end user
                        // get customer
                        if (prInvoice.agentId != null)
                        {
                            Agent agentinv = new Agent();
                            if (FillCombo.agentsList is null)
                            { await FillCombo.RefreshAgents(); }
                            agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                            //    agentinv = customers.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                            prInvoice.agentCode = agentinv.code;
                            //new lines
                            prInvoice.agentName = agentinv.name;
                            prInvoice.agentCompany = agentinv.company;
                            prInvoice.agentAddress = agentinv.address;
                            prInvoice.agentMobile = agentinv.mobile;
                        

                        }
                        else
                        {
                            prInvoice.agentCode = "-";
                            prInvoice.agentName = "-";
                            prInvoice.agentCompany = "-";
                            prInvoice.agentAddress = "-";
                         
                            prInvoice.agentMobile = "-";
                        }
                        //end customer
                        //get branch
                        Branch branch = new Branch();
                        //  branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                        if (FillCombo.branchsAllList is null)
                        { await FillCombo.RefreshBranchsAll(); }
                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();
                        if (branch.branchId > 0)
                        {
                            prInvoice.branchName = branch.name;
                        }
                        //end branch
                        ReportCls.checkInvLang();
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
                            // shipuser = await userModel.getUserById((int)prInvoice.shipUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            shipuser = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.shipUserId).FirstOrDefault();


                        }
                        prInvoice.shipUserName = shipuser.name + " " + shipuser.lastname;
                        //end shipping
                        decimal totaltax = 0;
                        foreach (var i in invoiceItems)
                        {
                            i.price = decimal.Parse(SectionData.DecTostring(i.price));
                            if (i.itemTax != null)
                            {
                                totaltax += (decimal)i.itemTax;

                            }
                            i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));

                        }
                        if (totaltax > 0 && prInvoice.invType != "sbd" && prInvoice.invType != "sb")
                        {
                            paramarr.Add(new ReportParameter("itemtax_note", (prInvoice.itemtax_note == null || prInvoice.itemtax_note == "") ? "" : prInvoice.itemtax_note.Trim()));
                          //  paramarr.Add(new ReportParameter("itemtax_note", prInvoice.itemtax_note.Trim()));
                            paramarr.Add(new ReportParameter("hasItemTax", "1"));

                        }
                        else
                        {
                            // paramarr.Add(new ReportParameter("itemtax_note", MainWindow.itemtax_note.Trim()));
                            paramarr.Add(new ReportParameter("hasItemTax", "0"));
                        }

                        reportSize repsize = new reportSize();
                        clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                        itemscount = invoiceItems.Count();
                        ///printer
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
                        paramarr.Add(new ReportParameter("isSaved", "y"));
                        paramarr = reportclass.fillSaleInvReport(prInvoice, paramarr, shippingcom);
                        rep = reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                        //   multiplePaytable(paramarr);


                        //  if ((prInvoice.invType == "s" || prInvoice.invType == "sd" || prInvoice.invType == "sbd" || prInvoice.invType == "sb"))
                        {
                            CashTransfer cachModel = new CashTransfer();
                            //  List<PayedInvclass> payedList = new List<PayedInvclass>();
                            //   payedList= prInvoice.cachTrans;
                            // payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                         //   clsReports clsrep = new clsReports();
                            List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                            foreach (var p in payedList)
                            {

                                if (p.processType == "cash")
                                {
                                    p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                                }
                                else
                                {
                                    p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                }

                            }

                            decimal sump = payedList.Sum(x => x.cash).Value;
                            decimal deservd = (decimal)prInvoice.totalNet - sump + prInvoice.cashReturn;
                            paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                            paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                            paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                            rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


                        }


                        rep.SetParameters(paramarr);

                        rep.Refresh();
                        //copy count

                        if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb")
                        {
                            //  if(prInvoice.printedcount)
                            paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));

                            for (int i = 1; i <= short.Parse(AppSettings.sale_copy_count); i++)
                            {
                                if (i > 1)
                                {
                                    // update paramarr->isOrginal
                                    foreach (var item in paramarr.Where(x => x.Name == "isOrginal").ToList())
                                    {
                                        StringCollection myCol = new StringCollection();
                                        myCol.Add(prInvoice.isOrginal.ToString());
                                        item.Values = myCol;


                                    }
                                    //end update

                                }
                                rep.SetParameters(paramarr);

                                rep.Refresh();
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

                                if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
                                {

                                     
                                    
                                          //if (MainWindow.salePaperSize == "A4")
                                        if (repsize.paperSize == "A4")
                                    {

                                        //LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.sale_printer_name, 1);
                                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, 1);
                                    }
                                    else
                                    {
                                        //LocalReportExtensions.customPrintToPrinter(rep, MainWindow.sale_printer_name, 1, repsize.width, repsize.height);
                                        LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, 1, repsize.width, repsize.height);

                                    }

                                    //});


                                    decimal res = await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);
                                    prInvoice.printedcount = prInvoice.printedcount + 1;

                                    prInvoice.isOrginal = false;


                                }
                                else
                                {
                                    //this.Dispatcher.Invoke(() =>
                                    //{
                                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);
                                    //});
                                    msg = "trYouExceedLimit";
                                }

                            }
                        }
                        else
                        {

                            
                            //if (MainWindow.salePaperSize == "A4")
                                if (repsize.paperSize == "A4")
                            {

                                //LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.sale_printer_name, short.Parse(AppSettings.sale_copy_count));
                                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, short.Parse(AppSettings.sale_copy_count));
                            }
                            else
                            {
                                //LocalReportExtensions.customPrintToPrinter(rep, MainWindow.sale_printer_name, short.Parse(AppSettings.sale_copy_count), repsize.width, repsize.height);
                                LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, short.Parse(AppSettings.sale_copy_count), repsize.width, repsize.height);

                            }


                            //});

                        }
                        // end copy count

                    }
                    else
                    {
                        //this.Dispatcher.Invoke(() =>
                        //{
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintEmptyInvoice"), animation: ToasterAnimation.FadeIn);
                        //});
                        msg = "trPrintEmptyInvoice";
                    }
                }
            }
            catch (Exception ex)
            {
                //this.Dispatcher.Invoke(() =>
                //{
                //    Toaster.ShowWarning(Window.GetWindow(this), message: "Not completed", animation: ToasterAnimation.FadeIn);

                //});
                msg = "trNotCompleted";
            }
            return msg;

        }
        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                ////////////////
                Thread t1 = new Thread(async () =>
                {
                    string msg = "";
                    msg = await printInvoice(invoice, invoiceItems, invoice.cachTrans.ToList());
                    if (msg == "")
                    {

                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString(msg), animation: ToasterAnimation.FadeIn);
                        });
                    }
                });
                t1.Start();
                /////////////////

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

        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                prInvoice = invoice;
                if (prInvoice.invoiceId > 0)
                {
                    //   prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                    if (int.Parse(AppSettings.Allow_print_inv_count) <= prInvoice.printedcount)
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);

                    }
                    else
                    {

                        Window.GetWindow(this).Opacity = 0.2;

                        List<ReportParameter> paramarr = new List<ReportParameter>();
                        clsReports clsrep = new clsReports();
                        string pdfpath = "";

                        ////////////////////////
                        string folderpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath) + @"\Thumb\report\";
                        ReportCls.clearFolder(folderpath);

                        pdfpath = @"\Thumb\report\Temp" + DateTime.Now.ToFileTime().ToString() + ".pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                        //////////////////////////////////


                        //   invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);

                        //get user
                        User employ = new User();
                        if (FillCombo.usersAllList is null)
                        { await FillCombo.RefreshAllUsers(); }
                        employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                        // employ = await userModel.getUserById((int)prInvoice.updateUserId);
                        prInvoice.uuserName = employ.name;
                        prInvoice.uuserLast = employ.lastname;
                        //end
                        //   invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        //get user
                        if (prInvoice.agentId != null)
                        {
                            Agent agentinv = new Agent();
                            // agentinv = customers.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                            if (FillCombo.agentsList is null)
                            { await FillCombo.RefreshAgents(); }
                            agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                            prInvoice.agentCode = agentinv.code;
                            //new lines
                            prInvoice.agentName = agentinv.name;
                            prInvoice.agentCompany = agentinv.company;
                            prInvoice.agentAddress = agentinv.address;
                            prInvoice.agentMobile = agentinv.mobile;
                        }
                        else
                        {
                            prInvoice.agentCode = "-";
                            prInvoice.agentName = "-";
                            prInvoice.agentCompany = "-";
                            prInvoice.agentAddress = "-";
                            prInvoice.agentMobile = "-";
                        }
                        //end
                        //branch name
                        Branch branch = new Branch();
                        //  branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                        if (FillCombo.branchsAllList is null)
                        { await FillCombo.RefreshBranchsAll(); }
                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                        if (branch.branchId > 0)
                        {
                            prInvoice.branchName = branch.name;
                        }
                        //end branch
                        //shipping
                        ShippingCompanies shippingcom = new ShippingCompanies();
                        if (prInvoice.shippingCompanyId > 0)
                        {
                            if (FillCombo.shippingCompaniesAllList is null)
                            { await FillCombo.RefreshShippingCompaniesAll(); }
                            shippingcom = FillCombo.shippingCompaniesAllList.Where(s => s.shippingCompanyId == (int)prInvoice.shippingCompanyId).FirstOrDefault();

                            //  shippingcom = await shippingcom.GetByID((int)prInvoice.shippingCompanyId);
                        }
                        User shipuser = new User();
                        if (prInvoice.shipUserId > 0)
                        {
                            //  shipuser = await userModel.getUserById((int)prInvoice.shipUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            shipuser = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.shipUserId).FirstOrDefault();

                        }
                        prInvoice.shipUserName = shipuser.name + " " + shipuser.lastname;
                        //end shipping
                        //    invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        ReportCls.checkInvLang();
                        decimal totaltax = 0;
                        foreach (var i in invoiceItems)
                        {
                            i.price = decimal.Parse(SectionData.DecTostring(i.price));
                            if (i.itemTax != null)
                            {

                                totaltax += (decimal)i.itemTax;

                            }
                            i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));

                        }
                        if (totaltax > 0 && prInvoice.invType != "sbd" && prInvoice.invType != "sb")
                        {
                            paramarr.Add(new ReportParameter("itemtax_note", (prInvoice.itemtax_note == null || prInvoice.itemtax_note == "") ? "" : prInvoice.itemtax_note.Trim()));
                            paramarr.Add(new ReportParameter("hasItemTax", "1"));

                        }
                        else
                        {
                            // paramarr.Add(new ReportParameter("itemtax_note", MainWindow.itemtax_note.Trim()));
                            paramarr.Add(new ReportParameter("hasItemTax", "0"));
                        }
                        reportSize repsize = new reportSize();
                        clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                        itemscount = invoiceItems.Count();
                        ///printer
                        reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                        // rs = reportclass.GetpayInvoiceRdlcpath(prInvoice, AppSettings.salePaperSize, itemscount, rep);
                        repsize.paperSize = repsset.paperSize;

                        repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, 1, itemscount, repsize.paperSize);
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
                            paramarr.Add(new ReportParameter("isOrginal", true.ToString()));
                            paramarr.Add(new ReportParameter("isSaved", "n"));
                        }
                        else
                        {
                            paramarr.Add(new ReportParameter("isOrginal", false.ToString()));
                            paramarr.Add(new ReportParameter("isSaved", "y"));
                        }
                   
                     //   if ((prInvoice.invType == "s" || prInvoice.invType == "sd" || prInvoice.invType == "sbd" || prInvoice.invType == "sb"))
                        {

                            CashTransfer cachModel = new CashTransfer();
                            List<PayedInvclass> payedList = new List<PayedInvclass>();
                            if (prInvoice.cachTrans == null)
                            {
                                payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                            }
                            else
                            {
                                payedList = clsrep.payedConvert(prInvoice.cachTrans);
                                // payedList = prInvoice.cachTrans;
                            }


                            //                payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                            //   payedList= cachModel.PayedByInvId(listPayments,prInvoice.invoiceId);
                          //  clsReports clsrep = new clsReports();
                            List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                            foreach (var p in payedList)
                            {

                                if (p.processType == "cash")
                                {
                                    p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                                }
                                else
                                {
                                    p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                }

                            }

                            decimal sump = payedList.Sum(x => x.cash).Value;
                            decimal deservd = (decimal)prInvoice.totalNet - sump + prInvoice.cashReturn;
                            paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                            paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                            paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                            rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));

                      }


                        rep.SetParameters(paramarr);
                        rep.Refresh();



                        //copy count
                        if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb")
                        {

                            //   paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));
                            // update paramarr->isOrginal
                            foreach (var item in paramarr.Where(x => x.Name == "isOrginal").ToList())
                            {
                                StringCollection myCol = new StringCollection();
                                myCol.Add(prInvoice.isOrginal.ToString());
                                item.Values = myCol;


                            }
                            //end update
                            paramarr.Add(new ReportParameter("isOrginal", false.ToString()));

                            rep.SetParameters(paramarr);

                            rep.Refresh();

                            if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
                            {
                                //if (prInvoice.invType == "s" && MainWindow.salePaperSize != "A4")
                                    if (MainWindow.salePaperSize != "A4")
                                {
                                    LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                                }
                                else
                                {
                                    LocalReportExtensions.ExportToPDF(rep, pdfpath);
                                }


                                decimal res = await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);



                                prInvoice.printedcount = prInvoice.printedcount + 1;

                                prInvoice.isOrginal = false;


                            }
                            else
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);
                            }


                        }
                        else
                        {

                            //if (prInvoice.invType == "s" && MainWindow.salePaperSize != "A4")
                            if ( MainWindow.salePaperSize != "A4")
                            {
                                LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                            }
                            else
                            {
                                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                            }

                        }
                        // end copy count






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
                }
                else
                {

                    if (billDetails.Count > 0)
                    {
                        //     Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSaveInvoiceToPreview"), animation: ToasterAnimation.FadeIn);
                        ////
                        Invoice tmpinvoice = new Invoice();
                        tmpinvoice.invType = "s";
                        if (cb_customer.SelectedValue != null)
                        {
                            tmpinvoice.agentId = (int)cb_customer.SelectedValue;
                        }


                        tmpinvoice.branchCreatorId = MainWindow.branchID;
                        tmpinvoice.branchId = MainWindow.branchID;
                        tmpinvoice.totalNet = decimal.Parse(tb_total.Text);
                        tmpinvoice.deserved = tmpinvoice.totalNet;
                        tmpinvoice.discountValue = decimal.Parse(tb_totalDescount.Text);
                        tmpinvoice.tax = tb_taxValue.Text == "" ? 0 : decimal.Parse(tb_taxValue.Text);
                        tmpinvoice.total = tb_sum.Text == "" ? 0 : decimal.Parse(tb_sum.Text);
                        tmpinvoice.invDate = DateTime.Now;

                        tmpinvoice.deservedDate = dp_desrvedDate.SelectedDate;
                        tmpinvoice.updateDate = DateTime.Now;
                        tmpinvoice.invTime = new TimeSpan();
                        //                         tmpinvoice.vendorInvDate=

                        // prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                        //if (int.Parse(MainWindow.Allow_print_inv_count) <= prInvoice.printedcount)
                        //{
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);

                        //}
                        //else
                        {

                            Window.GetWindow(this).Opacity = 0.2;

                            List<ReportParameter> paramarr = new List<ReportParameter>();
                            string pdfpath = "";

                            ////////////////////////
                            string folderpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath) + @"\Thumb\report\";
                            ReportCls.clearFolder(folderpath);

                            pdfpath = @"\Thumb\report\Temp" + DateTime.Now.ToFileTime().ToString() + ".pdf";
                            pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                            //////////////////////////////////
                            List<ItemTransfer> invoiceItems = new List<ItemTransfer>();
                            ItemTransfer itemtemp = new ItemTransfer();
                            decimal totaltax = 0;
                            //foreach (var billrow in billDetails)
                            //{
                            //    itemtemp = new ItemTransfer();
                            //    itemtemp.itemsTransId = billrow.ID;
                            //    itemtemp.itemName = billrow.Product;
                            //    itemtemp.unitName = billrow.Unit;
                            //    itemtemp.quantity = billrow.Count;

                            //    itemtemp.price = decimal.Parse(SectionData.DecTostring(billrow.Price));

                            //    totaltax += billrow.Tax;


                            //    // itemtemp.t = billrow.Total;
                            //    invoiceItems.Add(itemtemp);



                            //}
                            ItemUnit tempiu = new ItemUnit();
                            Item tempI = new Item();

                            for (int i = 0; i < billDetails.Count; i++)
                            {
                                itemtemp = new ItemTransfer();
                                //itemtemp.invoiceId = 0;
                                itemtemp.quantity = billDetails[i].Count;
                                itemtemp.price = decimal.Parse(SectionData.DecTostring(billDetails[i].Price));
                                itemtemp.subTotal = decimal.Parse(SectionData.DecTostring(itemtemp.price * itemtemp.quantity));
                                itemtemp.itemUnitId = billDetails[i].itemUnitId;
                                //  tempiu = InvoiceGlobalSaleUnitsList.Where(x => x.itemUnitId == billDetails[i].itemUnitId).FirstOrDefault();
                                tempI = InvoiceGlobalSaleUnitsList.Where(X => X.itemUnitId == billDetails[i].itemUnitId).FirstOrDefault();


                                itemtemp.unitName = tempI.unitName;


                                //itemtemp.itemTax = decimal.Parse(SectionData.DecTostring(billDetails[i].Tax));
                               
                                itemtemp.itemSerials = billDetails[i].itemSerials;
                                itemtemp.createUserId = MainWindow.userID;
                                itemtemp.itemName = billDetails[i].Product;

                                //totaltax += billDetails[i].Tax;
                                invoiceItems.Add(itemtemp);
                            }

                            // MainWindow.userLogin
                            User employ = new User();
                            employ = MainWindow.userLogin;
                            tmpinvoice.uuserName = employ.name;
                            tmpinvoice.uuserLast = employ.lastname;

                            if (tmpinvoice.agentId != null && tmpinvoice.agentId > 0)
                            {
                                Agent agentinv = new Agent();
                                agentinv = customers.Where(X => X.agentId == tmpinvoice.agentId).FirstOrDefault();

                                tmpinvoice.agentCode = agentinv.code;
                                //new lines
                                tmpinvoice.agentName = agentinv.name;
                                tmpinvoice.agentCompany = agentinv.company;
                                tmpinvoice.agentAddress = agentinv.address;
                                tmpinvoice.agentMobile = agentinv.mobile;
                               

                            }
                            else
                            {
                                tmpinvoice.agentCode = "-";
                                tmpinvoice.agentName = "-";
                                tmpinvoice.agentCompany = "-";
                                tmpinvoice.agentAddress = "-";
                                tmpinvoice.agentMobile = "-";
                            }
                            //branch name
                            Branch branch = new Branch();
                            branch = MainWindow.loginBranch;
                            if (branch.branchId > 0)
                            {
                                tmpinvoice.branchName = branch.name;
                            }

                            ReportCls.checkInvLang();


                            if (totaltax > 0)
                            {
                               
                                paramarr.Add(new ReportParameter("itemtax_note", (tmpinvoice.itemtax_note == null || tmpinvoice.itemtax_note == "") ? "" : tmpinvoice.itemtax_note.Trim()));
                                paramarr.Add(new ReportParameter("hasItemTax", "1"));

                            }
                            else
                            {
                                // paramarr.Add(new ReportParameter("itemtax_note", MainWindow.itemtax_note.Trim()));
                                paramarr.Add(new ReportParameter("hasItemTax", "0"));
                            }
                            reportSize repsize = new reportSize();
                            clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            itemscount = invoiceItems.Count();
                            ///printer
                            clsReports clsrep = new clsReports();
                            reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                            // rs = reportclass.GetpayInvoiceRdlcpath(prInvoice, AppSettings.salePaperSize, itemscount, rep);
                            repsize.paperSize = repsset.paperSize;

                            repsize = reportclass.GetreceiptInvoiceRdlcpath(prInvoice, 1, itemscount, repsize.paperSize);
                            repsize.printerName = repsset.printerName;

                            //end
                            string reppath = repsize.reppath;                                
                            rep.ReportPath = reppath;
                            clsReports.setInvoiceLanguage(paramarr);
                            clsReports.InvoiceHeader(paramarr);
                            paramarr.Add(new ReportParameter("isSaved", "n"));

                            paramarr = reportclass.fillSaleInvReport(tmpinvoice, paramarr, shippingcomp);
                            rep = reportclass.AddDataset(rep, prInvoice.invoiceTaxes);
                            if (tmpinvoice.invType == "pd" || tmpinvoice.invType == "sd" || tmpinvoice.invType == "qd"
         || tmpinvoice.invType == "sbd" || tmpinvoice.invType == "pbd"
         || tmpinvoice.invType == "ord" || tmpinvoice.invType == "imd" || tmpinvoice.invType == "exd")
                            {
                                paramarr.Add(new ReportParameter("isOrginal", true.ToString()));
                            }
                            else
                            {
                                paramarr.Add(new ReportParameter("isOrginal", false.ToString()));
                            }
                      //      if ((tmpinvoice.invType == "s" || tmpinvoice.invType == "sd" || tmpinvoice.invType == "sbd" || tmpinvoice.invType == "sb"))
                            {
                                CashTransfer cachModel = new CashTransfer();
                                PayedInvclass PayedInvtemp = new PayedInvclass();

                                List<PayedInvclass> payedList = new List<PayedInvclass>();
                                decimal sump = 0;

                                //
                                //if (cb_paymentProcessType.SelectedIndex != -1)
                                //{
                                //    switch (cb_paymentProcessType.SelectedValue.ToString())
                                //    {
                                //        case "cash":
                                //            {
                                //                PayedInvtemp.processType = "cash";

                                //                //  sump = tb_cashPaid.Text==""?0: decimal.Parse(tb_cashPaid.Text);


                                //            }
                                //            break;
                                //        case "balance":
                                //            {
                                //                PayedInvtemp.processType = "balance";
                                //                //   sump = tb_cashPaid.Text == "" ? 0 : decimal.Parse(tb_cashPaid.Text);
                                //            }
                                //            break;
                                //        case "card":
                                //            {
                                //                PayedInvtemp.processType = "card";
                                //                //   sump = tb_cashPaid.Text == "" ? 0 : decimal.Parse(tb_cashPaid.Text);
                                //            }
                                //            break;
                                //        case "multiple":
                                //            {
                                //                PayedInvtemp.processType = "multiple";
                                //            }
                                //            break;

                                //    }
                                //}
                                //
                                // payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                                sump = 0;
                                payedList.Add(PayedInvtemp);

                                //  sump = payedList.Sum(x => x.cash).Value;
                                //  decimal deservd = (decimal)tmpinvoice.totalNet - sump;
                                paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                                paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));

                                paramarr.Add(new ReportParameter("trDraftInv", MainWindow.resourcemanagerreport.GetString("trDraft")));
                                //  paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));


                            }


                            rep.SetParameters(paramarr);
                            rep.Refresh();



                            ////copy count
                            //if (tmpinvoice.invType == "s" || tmpinvoice.invType == "sb" || tmpinvoice.invType == "p" || tmpinvoice.invType == "pb")
                            //{

                            //    //   paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));
                            //    // update paramarr->isOrginal
                            //    foreach (var item in paramarr.Where(x => x.Name == "isOrginal").ToList())
                            //    {
                            //        StringCollection myCol = new StringCollection();
                            //        myCol.Add(tmpinvoice.isOrginal.ToString());
                            //        item.Values = myCol;


                            //    }
                            //end update
                            //paramarr.Add(new ReportParameter("isOrginal", false.ToString()));

                            rep.SetParameters(paramarr);

                            rep.Refresh();

                            //if (int.Parse(MainWindow.Allow_print_inv_count) > tmpinvoice.printedcount)
                            //{
                          //  if (tmpinvoice.invType == "s" && MainWindow.salePaperSize != "A4")
                                if (  MainWindow.salePaperSize != "A4")
                            {
                                LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                            }
                            else
                            {
                                LocalReportExtensions.ExportToPDF(rep, pdfpath);
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



                        //////


                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintEmptyInvoice"), animation: ToasterAnimation.FadeIn);
                        });
                    }

                }
                #endregion



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

        //public void BuildReport()
        //{
        //    List<ReportParameter> paramarr = new List<ReportParameter>();

        //    string addpath;
        //    bool isArabic = ReportCls.checkInvLang();
        //    if (isArabic)
        //    {
        //        addpath = @"\Reports\Sale\Ar\PackageReport.rdlc";
        //    }
        //    else
        //        addpath = @"\Reports\Sale\En\PackageReport.rdlc";
        //    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

        //    ReportCls.checkInvLang();

        //    clsReports.purchaseInvoiceReport(invoiceItems, rep, reppath);
        //    clsReports.setInvoiceLanguage(paramarr);
        //    clsReports.InvoiceHeader(paramarr);

        //    rep.SetParameters(paramarr);

        //    rep.Refresh();
        //}
        public async Task<string> sendsaleEmail(Invoice prInvoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> payedList)
        {
            string msg = "";
            try
            {
                clsReports clsrep = new clsReports();
                payedList = clsrep.payedConvert(payedList);

                //
                //  prInvoice = new Invoice();//
                //if (isdirect)
                //{
                // prInvoice = await invoiceModel.GetByInvoiceId(invoiceId);//
                //}
                //else
                //{
                //    prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                //}
                if (prInvoice.invoiceId > 0)
                {
                    //  prInvoice = new Invoice();
                    //  Invoice tomailInvoice = new Invoice();
                    //if (prinvoiceId != 0)
                    //    prInvoice = await invoiceModel.GetByInvoiceId(prinvoiceId);
                    //else
                    //  prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                    //   tomailInvoice = prInvoice;
                    decimal? discountval = 0;
                    string discounttype = "";
                    discountval = prInvoice.discountValue;
                    discounttype = prInvoice.discountType;
                    if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                    || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                    || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                    {
                        //this.Dispatcher.Invoke(() =>
                        //{
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCanNotSendDraftInvoice"), animation: ToasterAnimation.FadeIn);
                        //});
                    }
                    else
                    {
                        // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        SysEmails email = new SysEmails();
                        EmailClass mailtosend = new EmailClass();
                        email = await email.GetByBranchIdandSide((int)MainWindow.branchID, "sales");

                        Agent toAgent = new Agent();
                        if (FillCombo.agentsList is null)
                        { await FillCombo.RefreshAgents(); }
                        toAgent = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                        //  toAgent = customers.Where(x => x.agentId == prInvoice.agentId).FirstOrDefault();
                        if (toAgent == null || toAgent.agentId == 0)
                        {
                            msg = "trTheCustomerHasNoEmail";
                            //edit warning message to customer
                            //this.Dispatcher.Invoke(() =>
                            //{
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheCustomerHasNoEmail"), animation: ToasterAnimation.FadeIn);
                            //});

                        }
                        else
                        {
                            //  int? itemcount = invoiceItems.Count();
                            if (email.emailId == 0)
                            {
                                msg = "trNoEmailForThisDept";
                                //this.Dispatcher.Invoke(() =>
                                //{
                                //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoEmailForThisDept"), animation: ToasterAnimation.FadeIn);
                                //});
                            }
                            else
                            {
                                if (prInvoice.invoiceId == 0)
                                {
                                    msg = "trThereIsNoOrderToSen";

                                    //       this.Dispatcher.Invoke(() =>
                                    //{
                                    //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoOrderToSen"), animation: ToasterAnimation.FadeIn);
                                    //           });
                                    //   }
                                }
                                else
                                {
                                    if (invoiceItems == null || invoiceItems.Count() == 0)
                                    {
                                        //   this.Dispatcher.Invoke(() =>
                                        //{
                                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoItemsToSend"), animation: ToasterAnimation.FadeIn);
                                        //});
                                        msg = "trThereIsNoItemsToSend";
                                    }

                                    else
                                    {

                                        if (toAgent.email.Trim() == "")
                                        {
                                            //this.Dispatcher.Invoke(() =>
                                            //     {
                                            //         Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheCustomerHasNoEmail"), animation: ToasterAnimation.FadeIn);
                                            //     });
                                            msg = "trTheCustomerHasNoEmail";
                                        }

                                        else
                                        {
                                            SetValues setvmodel = new SetValues();

                                            List<SetValues> setvlist = new List<SetValues>();
                                            if (prInvoice.invType == "s")
                                            {
                                                setvlist = await setvmodel.GetBySetName("sale_email_temp");
                                            }
                                            else if (prInvoice.invType == "or" || prInvoice.invType == "ors")
                                            {
                                                setvlist = await setvmodel.GetBySetName("sale_order_email_temp");
                                            }
                                            else if (prInvoice.invType == "q" || prInvoice.invType == "qs")
                                            {
                                                setvlist = await setvmodel.GetBySetName("quotation_email_temp");
                                            }
                                            else
                                            {
                                                setvlist = await setvmodel.GetBySetName("sale_email_temp");
                                            }

                                            //shipping

                                            if (prInvoice.shippingCompanyId > 0)
                                            {
                                                if (FillCombo.shippingCompaniesAllList is null)
                                                { await FillCombo.RefreshShippingCompaniesAll(); }
                                                shippingcomp = FillCombo.shippingCompaniesAllList.Where(s => s.shippingCompanyId == (int)prInvoice.shippingCompanyId).FirstOrDefault();

                                                //  shippingcomp = await shippingcomp.GetByID((int)prInvoice.shippingCompanyId);
                                            }

                                            if (prInvoice.shipUserId > 0)
                                            {
                                                shipinguser = await userModel.getUserById((int)prInvoice.shipUserId);
                                            }
                                            prInvoice.shipUserName = shipinguser.name + " " + shipinguser.lastname;
                                            //end shipping

                                            string pdfpath = await SaveSalepdf(prInvoice, invoiceItems, payedList);

                                            prInvoice.discountValue = discountval;
                                            prInvoice.discountType = discounttype;
                                            mailtosend = mailtosend.fillSaleTempData(prInvoice, invoiceItems, mailpayedList, email, toAgent, setvlist);


                                            mailtosend.AddAttachTolist(pdfpath);


                                            //
                                            //Thread t2 = new Thread(() =>
                                            //{
                                            string resmsg = "";
                                            resmsg = mailtosend.Sendmail();
                                            //this.Dispatcher.Invoke(() =>
                                            //{
                                            // msg = mailtosend.Sendmail();// temp comment
                                            if (resmsg == "Failure sending mail.")
                                            {
                                                // msg = "No Internet connection";
                                                msg = "trNoConnection";
                                                //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoConnection"), animation: ToasterAnimation.FadeIn);

                                            }
                                            else if (resmsg == "mailsent")
                                            {
                                                //  Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailSent"), animation: ToasterAnimation.FadeIn);
                                                msg = "trMailSent";
                                            }
                                            else
                                            {
                                                //  Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailNotSent"), animation: ToasterAnimation.FadeIn);
                                                msg = "trMailNotSent";
                                            }
                                            //});
                                            //});
                                            //  t2.Start();


                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoItemsToSend"), animation: ToasterAnimation.FadeIn);
                    //});
                    msg = "trThereIsNoItemsToSend";
                }


                //

            }
            catch (Exception ex)
            {
                //this.Dispatcher.Invoke(() =>
                //{
                //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCannotSendEmail"), animation: ToasterAnimation.FadeIn);
                //});
                msg = "trCannotSendEmail";
            }
            return msg;
        }
        private void Btn_emailMessage_Click(object sender, RoutedEventArgs e)
        {//email
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(sendEmailPermission, MainWindow.groupObjects, "one"))
                {

                    //await sendsaleEmail();
                    ////
                    Thread t1 = new Thread(async () =>
                    {
                        string msg = "";

                        msg = await sendsaleEmail(invoice, invoiceItems, invoice.cachTrans.ToList());
                        this.Dispatcher.Invoke(() =>
                        {

                            if (msg == "")
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailNotSent"), animation: ToasterAnimation.FadeIn);
                            }
                            //else if (msg == "trTheCustomerHasNoEmail")
                            //{

                            //}
                            else if (msg == "trMailSent")
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailSent"), animation: ToasterAnimation.FadeIn);

                            }
                            else
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString(msg), animation: ToasterAnimation.FadeIn);

                            }
                        });

                    });
                    t1.Start();
                    ////
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

        private async void Btn_printCount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                decimal result = 0;

                if (invoice.invoiceId > 0)
                {
                    result = await invoiceModel.updateprintstat(invoice.invoiceId, -1, true, true);
                    prInvoice.isOrginal = true;
                    prInvoice.printedcount = prInvoice.printedcount - 1;
                    invoice.isOrginal = true;
                    invoice.printedcount = prInvoice.printedcount - 1;


                    if (result > 0)
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }

                }
                else
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trChooseInvoiceToolTip"), animation: ToasterAnimation.FadeIn);
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

        #endregion
        //
        private async void Btn_items_Click(object sender, RoutedEventArgs e)
        {
            try
            { //items

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                wd_items w = new wd_items();
                w.CardType = "sales";
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

        private async void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await clearInvoice();
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

        private async void Btn_quotations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(quotationPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    await newDraft();
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_invoice w = new wd_invoice();

                    // sale invoices
                    string invoiceType = "q";
                    w.invoiceType = invoiceType;
                    w.condition = "orders";
                    w.branchCreatorId = MainWindow.branchID.Value;
                    w.title = MainWindow.resourcemanager.GetString("trQuotations");

                    if (w.ShowDialog() == true)
                    {
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _InvoiceType = invoice.invType;
                            isFromReport = false;
                            archived = false;
                            //notifications
                            setNotifications();
                            md_payments.Badge = "";
                            await refreshDocCount(invoice.invoiceId);

                            _invoiceId = invoice.invoiceId;
                            // set title to bill
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trQuotations");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

                            await fillInvoiceInputs(invoice);
                            invoices = FillCombo.invoices;
                            navigateBtnActivate();

                            mainInvoiceItems = invoiceItems;


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


        private void PreventSpaces(object sender, KeyEventArgs e)
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

        private async void Btn_clearCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await RefrishCustomers();
                _SelectedCustomer = -1;
                cb_customer.SelectedIndex = -1;
                txt_payType.Text = "-";
                txt_upperLimit.Text = "-";
                tb_moneyIcon2.Visibility = Visibility.Collapsed;

                dp_desrvedDate.SelectedDate = null;
                tb_note.Clear();
                invoice.agentId = 0;

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

        //private void Cb_card_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
        //        if (elapsed.TotalMilliseconds > 100 && cb_card.SelectedIndex != -1)
        //        {
        //            _SelectedCard = (int)cb_card.SelectedValue;
        //        }
        //        else
        //        {
        //            cb_card.SelectedValue = _SelectedCard;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}

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
                
                tb_coupon.Text = "";
               
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



        private void Cb_company_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_company.SelectedValue != null && cb_company.SelectedValue.ToString() != "0")
                {
                    sp_PrePaid.IsEnabled = true;
                    #region free delivery according to settings
                    if (AppSettings.freeDelivery)
                        sp_isFreeDelivery.IsEnabled = true;
                    #endregion

                    companyModel = FillCombo.shippingCompaniesList.Find(c => c.shippingCompanyId == (int)cb_company.SelectedValue);
                    _DeliveryCost = (decimal)companyModel.deliveryCost;
                    _RealDeliveryCost = (decimal)companyModel.RealDeliveryCost;                  

                    if (companyModel.deliveryType == "local")
                    {
                        cb_user.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        cb_user.SelectedIndex = -1;
                        cb_user.Visibility = Visibility.Collapsed;
                        p_errorUser.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    companyModel = new ShippingCompanies();
                    cb_user.SelectedIndex = -1;
                    _DeliveryCost = 0;
                    _RealDeliveryCost = 0;
                    cb_user.Visibility = Visibility.Collapsed;
                    p_errorUser.Visibility = Visibility.Collapsed;

                    #region
                    sp_PrePaid.IsEnabled = false;
                    sp_isFreeDelivery.IsEnabled = false;
                    chk_onDelivery.IsChecked = false;
                    chk_isFreeDelivery.IsChecked = false;
                    #endregion

                }
                tb_deliveryCost.Text = SectionData.PercentageDecTostring(_DeliveryCost);
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




        private void Btn_payments_Click(object sender, RoutedEventArgs e)
        {//payments
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (invoice != null && invoice.invoiceId != 0)
                {
                    if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_cashTransfer w = new wd_cashTransfer();

                        w.invId = invoice.invoiceId;
                        //w.childInvoice = invoice.ChildInvoice;
                        //w.mainInvId = invoice.invoiceMainId;
                        w.invType = invoice.invType;
                        w.invPaid = invoice.paid.Value;
                        w.invTotal = invoice.totalNet.Value;
                        w.title = MainWindow.resourcemanager.GetString("trReceived");
                        w.sourceUserControls = FillCombo.UserControls.receiptInvoice;

                        w.ShowDialog();

                        Window.GetWindow(this).Opacity = 1;
                    }
                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private void serialItemsRow(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                BillDetails row = (BillDetails)dg_billDetails.SelectedItems[0];
                int itemId = row.itemId;
                item =(Item) items.ToList().Where(i => i.itemId == itemId).FirstOrDefault().Clone();
                item.itemUnitId = row.itemUnitId;

                Window.GetWindow(this).Opacity = 0.2;
                wd_serialNum w = new wd_serialNum();
                w.sourceUserControls = FillCombo.UserControls.receiptInvoice;
                w.item = item;
                w.item.unitName = row.Unit;
                w.itemCount = row.Count;
                if (_InvoiceType.Equals("or") || _InvoiceType.Equals("q"))
                    w.invType = "sd";
                else
                    w.invType = _InvoiceType;
                if(invoice.invoiceItems != null)
                w.itemTransfer = invoice.invoiceItems.Where(x => x.itemId == itemId).FirstOrDefault();
               
                //if (invoice.invoiceMainId != null)
                    w.mainInvoiceId = (int)invoice.invoiceId;

                if (!_InvoiceType.Equals("sbd") || (_InvoiceType.Equals("sbd") && row.itemUnitId.Equals(row.basicItemUnitId)))
                {
                    w.ItemProperties = row.ItemProperties;
                    w.itemsSerials = row.itemSerials;
                    w.returnedSerials = row.returnedSerials;
                    w.StoreProperties = row.StoreProperties;
                    w.ReturnedProperties = row.ReturnedProperties;
                }

                w.valid = row.valid;
                w.ShowDialog();
                if (w.isOk == true)
                {

                    row.itemSerials = w.itemsSerials;
                    row.returnedSerials = w.returnedSerials;
                    row.StoreProperties = w.StoreProperties;
                    row.ReturnedProperties = w.ReturnedProperties;

                   
                    row.valid = true;
                    refrishBillDetails();

                }
                else 
                {
                    row.itemSerials = w.itemsSerials;
                    row.returnedSerials = w.returnedSerials;
                    row.StoreProperties = w.StoreProperties;
                    row.ReturnedProperties = w.ReturnedProperties;

                    #region valid icon
                    bool isValid = true;
                    long serialNum = 0;

                    if (item.type == "sn")
                        serialNum = (long)row.Count;
                    else if (item.type.Equals("p"))
                    {
                        long packageCount = (long)row.Count;
                        foreach (var p in item.packageItems)
                        {
                            if (p.type.Equals("sn"))
                                serialNum += (long)p.itemCount * packageCount;
                        }

                    }
                    if ((_InvoiceType.Equals("sbd") && serialNum > row.returnedSerials.Count) 
                        || (invoice.invType != null && invoice.invType.Equals("or") && serialNum > row.itemSerials.Count)
                        || (invoice.invType != null && invoice.invType.Equals("q") && serialNum > row.itemSerials.Count)
                        || (_InvoiceType.Equals("sd")  &&  serialNum > row.itemSerials.Count))
                        isValid = false;
                    else if ((_InvoiceType == "sd" || _InvoiceType == "or" || _InvoiceType == "q") && serialNum < (long)row.Count)
                        isValid = false;

                    #endregion

                    row.valid = isValid;
                    refrishBillDetails();
                }
                //else if(w.serialsSkip || w.serialsSave)
                //{
                //    row.itemSerials = w.itemsSerials;
                //    row.returnedSerials = w.returnedSerials;
                //    row.valid =true;
                //    refrishBillDetails();

                //}
                //else if(w.propertiesSkip || w.propertiesSave)
                //{
                //    row.StoreProperties = w.StoreProperties;
                //    row.ReturnedProperties = w.ReturnedProperties;
                //}

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
        private void Cb_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                #region
                //com
                SectionData.clearComboBoxValidate(cb_user, p_errorUser);
                if (companyModel.deliveryType == "local" && (cb_user.SelectedValue != null && cb_user.SelectedValue.ToString() == "0"))
                {
                    //valid = false;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectTheDeliveryMan"), animation: ToasterAnimation.FadeIn);
                    SectionData.validateEmptyComboBox(cb_user, p_errorUser, tt_errorUser, "trSelectTheDeliveryMan");

                    //return valid;
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

        private void Cb_typeDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && (cb_typeDiscount.SelectedValue != null && cb_typeDiscount.SelectedValue.ToString() != "0"))
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
        private void clearNavigation()
        {
            _Sum = 0;
            companyModel = new ShippingCompanies();
            isFromReport = false;
            archived = false;
            _Tax = 0;
            _Discount = 0;
            _DeliveryCost = 0;
            _RealDeliveryCost = 0;
            _SequenceNum = 0;
            txt_invNumber.Text = "";
            _SelectedCustomer = -1;
            _SelectedDiscountType = 0;
            invoice = new Invoice();
            selectedCoupons.Clear();
            tb_barcode.Clear();
            cb_customer.SelectedIndex = -1;
            cb_customer.SelectedItem = "";
            dp_desrvedDate.Text = "";
            tb_note.Clear();
            billDetails.Clear();
            tb_total.Text = "0";
            tb_sum.Text = "0";
            tb_deliveryCost.Text = "0";
            tb_discount.Clear();
            tb_totalDescount.Text = "0";
            cb_typeDiscount.SelectedIndex = 0;
            cb_company.SelectedIndex = -1;
            cb_user.SelectedIndex = -1;

            couponsLst.Clear();
            lst_coupons.ItemsSource = couponsLst;
           
            btn_items.IsEnabled = true;
            md_docImage.Badge = "";
            md_payments.Badge = "";
            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
            SectionData.clearComboBoxValidate(cb_user, p_errorUser);
            refrishBillDetails();
            tb_barcode.Focus();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;

        }
        private async Task navigateInvoice(int index)
        {
            try
            {
                clearNavigation();
                invoice = invoices[index];
                _InvoiceType = invoice.invType;
                _invoiceId = invoice.invoiceId;
                navigateBtnActivate();
                await fillInvoiceInputs(invoice);

                if (_InvoiceType == "s")
                {
                    if (invoice.isArchived)
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoiceArchived");
                    if (invoice.ChildInvoice != null)
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoiceUpdated");
                    else
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesInvoice");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                }
                else if (_InvoiceType == "sb")
                {
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesReturnInvoice");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                }
                else if (_InvoiceType == "sd")
                {
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trSalesDraft");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                }
                else if (_InvoiceType == "sbd")
                {
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftBounceBill");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                }

                if (_InvoiceType == "s" || _InvoiceType == "sb")
                    refreshInvoiceNot(invoice.invoiceId);
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



 

        private void Exp_payment_Expanded(object sender, RoutedEventArgs e)
        {

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

        private void Tb_EnglishDigit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only english and digits
            Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
            if (!regex.IsMatch(e.Text))
                e.Handled = true;
        }
        /*
        private void Tb_cashPaid_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(tb_total.Text) && !string.IsNullOrEmpty(tb_cashPaid.Text))
                {
                    if (tb_cashPaid.Text.Equals("0"))
                    {
                        txt_theRest.Text = "0";
                        txt_theRest.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                    }
                    else
                    {
                        decimal total = decimal.Parse(tb_total.Text);
                        decimal cashPaid = decimal.Parse(tb_cashPaid.Text);
                        decimal theRest = (cashPaid - total);

                        txt_theRest.Text = theRest.ToString();

                        if (theRest > 0)
                            txt_theRest.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush;
                        else if (theRest < 0)
                            txt_theRest.Foreground = Application.Current.Resources["mediumRed"] as SolidColorBrush;
                        else
                            txt_theRest.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                    }


                }
                else
                {
                    txt_theRest.Text = "0";
                    txt_theRest.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        private void Dg_billDetails_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _IsFocused = true;
        }

        private void Chk_onDelivery_Checked(object sender, RoutedEventArgs e)
        {
            btn_save.Content = MainWindow.resourcemanager.GetString("trSubmit");

        }

        private void Chk_onDelivery_Unchecked(object sender, RoutedEventArgs e)
        {
            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

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
       
        private void Lst_coupons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {//double click
            try
            {
                if (lst_coupons.SelectedItem != null)
                {
                    //couponModel = coupons.ToList().Find(c => c.name == lst_coupons.SelectedItem.ToString() );
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

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Btn_enter_Click(null , null);
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
                if (cb_sliceId.IsMouseCaptured)
                {
                    AppSettings.DefaultInvoiceSlice = (int)cb_sliceId.SelectedValue;

                   // decimal basicPrice = 0;
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
                            //    
                            price = (decimal)it.price;
                        }
                        else
                        {
                            var slice = it.SalesPrices.Where(x => x.sliceId == AppSettings.DefaultInvoiceSlice).FirstOrDefault();
                            //basicPrice = (decimal)slice.basicPrice;
                            //if (AppSettings.itemsTax_bool == true)
                            //    price = (decimal)slice.priceTax;
                            //else
                                price = (decimal)slice.price;
                        }
                       // b.basicPrice = basicPrice;
                        b.Price = price;
                        b.Total = b.Price * b.Count;
                        _Sum += b.Total;
                    }
                    refrishDataGridItems();
                    refreshTotalValue();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Chk_isFreeDelivery_Checked(object sender, RoutedEventArgs e)
        {
            refreshTotalValue();
        }

        private void Chk_isFreeDelivery_Unchecked(object sender, RoutedEventArgs e)
        {
            refreshTotalValue();
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

        private void Cb_user_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = FillCombo.driversList.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
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
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
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
