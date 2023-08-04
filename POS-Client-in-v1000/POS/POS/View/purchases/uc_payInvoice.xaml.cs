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


namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_payInvoice.xaml
    /// </summary>
    public partial class uc_payInvoice : UserControl
    {
        string invoicePermission = "payInvoice_invoice";
        string returnPermission = "payInvoice_return";
        string paymentsPermission = "payInvoice_payments";
        string sendEmailPermission = "payInvoice_sendEmail";
        string openOrderPermission = "payInvoice_openOrder";
        string initializeShortagePermission = "payInvoice_initializeShortage";
        string printCountPermission = "payInvoice_printCount";

        private static uc_payInvoice _instance;
        public static uc_payInvoice Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_payInvoice();
                return _instance;
            }
        }
        public uc_payInvoice()
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
        public List<Control> controls;

        public static bool isFromReport = false;
        public static bool archived = false;

        Item itemModel = new Item();
        Item item = new Item();
        IEnumerable<Item> items;
        ItemLocation itemLocationModel = new ItemLocation();

        Branch branchModel = new Branch();

        Agent agentModel = new Agent();
        //List<Agent> vendors;
        List<Agent> vendorsL;

        ItemUnit itemUnitModel = new ItemUnit();
        //List<ItemUnit> barcodesList;
        List<ItemUnit> itemUnits;

        Invoice invoiceModel = new Invoice();
        public Invoice invoice = new Invoice();
        List<Invoice> invoices;
        List<ItemTransfer> invoiceItems;
        List<ItemTransfer> mainInvoiceItems;
        List<CashTransfer> listPayments;
        Pos pos = new Pos();
        //Card cardModel = new Card();
       // IEnumerable<Card> cards;
        CashTransfer cashTransfer = new CashTransfer();
        #region //to handle barcode characters
        static private int _SelectedBranch = -1;
        static private int _SelectedVendor = -1;
        static private int _SelectedDiscountType = -1;
        static private string _SelectedPaymentType = "cash";
        static private int _SelectedCard = -1;
        // for barcode
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private object _Sender;
        bool _IsFocused = false;
        #endregion
        #region for notifications
        private static DispatcherTimer timer;
        int _DraftCount = 0;
        int _InvCount = 0;
        int _DocCount = 0;
        int _OrderCount = 0;
        int _PaymentCount = 0;

        #endregion
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        //  int? parentCategorieSelctedValue;
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;

        //for bill details
        static private int _SequenceNum = 0;
        static private int _invoiceId;
        static private decimal _Sum = 0;
        static public string _InvoiceType = "pd"; // purchase draft
        static public bool _isLack = false; // for lack invoice

        // for report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        int prInvoiceId;
        Invoice prInvoice = new Invoice();
        List<PayedInvclass> mailpayedList = new List<PayedInvclass>();
        //bool isClose = false;

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
        private void translate()
        {
            ////////////////////////////////----invoice----/////////////////////////////////
            dg_billDetails.Columns[1].Header = MainWindow.resourcemanager.GetString("trNo.");
            dg_billDetails.Columns[2].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_billDetails.Columns[3].Header = MainWindow.resourcemanager.GetString("trUnit");
            dg_billDetails.Columns[4].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_billDetails.Columns[5].Header = MainWindow.resourcemanager.GetString("trPrice");
            dg_billDetails.Columns[6].Header = MainWindow.resourcemanager.GetString("trTotal");
             
            txt_discount.Text = MainWindow.resourcemanager.GetString("trDiscount");
            txt_shippingCost.Text = MainWindow.resourcemanager.GetString("shippingAmount");
            txt_sum.Text = MainWindow.resourcemanager.GetString("trSum");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");
            txt_tax.Text = MainWindow.resourcemanager.GetString("trTax");
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
            txt_store.Text = MainWindow.resourcemanager.GetString("trStore/Branch");
            txt_vendor.Text = MainWindow.resourcemanager.GetString("trVendor");
            txt_vendorIvoiceDetails.Text = MainWindow.resourcemanager.GetString("vendorInvoiceDetails");

            txt_printCount.Text = MainWindow.resourcemanager.GetString("trAdditional");
            txt_shortageInvoice.Text = MainWindow.resourcemanager.GetString("trLack");
            txt_printInvoice.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_invoiceImages.Text = MainWindow.resourcemanager.GetString("trImages");
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_drafts.Text = MainWindow.resourcemanager.GetString("trDrafts");
            txt_newDraft.Text = MainWindow.resourcemanager.GetString("trNew");
            txt_payments.Text = MainWindow.resourcemanager.GetString("trPayments");
            txt_returnInvoice.Text = MainWindow.resourcemanager.GetString("trReturn");
            txt_invoices.Text = MainWindow.resourcemanager.GetString("trInvoices");
            txt_purchaseOrder.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_payTypeTitle.Text = MainWindow.resourcemanager.GetString("defaultPayment") + ":";

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_discount, MainWindow.resourcemanager.GetString("trDiscountHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_typeDiscount, MainWindow.resourcemanager.GetString("trDiscountTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trStore/BranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendor, MainWindow.resourcemanager.GetString("trVendorHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_desrvedDate, MainWindow.resourcemanager.GetString("trDeservedDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_invoiceNumber, MainWindow.resourcemanager.GetString("trInvoiceNumberHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_invoiceDate, MainWindow.resourcemanager.GetString("trInvoiceDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_shippingCost, MainWindow.resourcemanager.GetString("shippingAmount"));
           

            tt_error_previous.Content = MainWindow.resourcemanager.GetString("trPrevious");
            tt_error_next.Content = MainWindow.resourcemanager.GetString("trNext");

            btn_save.Content = MainWindow.resourcemanager.GetString("purchase");
        }
        async private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.KeyDown -= HandleKeyPress;
                saveBeforeExit();
                timer.Stop();
                GC.Collect();
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch //(Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        private async Task saveBeforeExit()
        {
            if (billDetails.Count > 0 && (_InvoiceType == "pd" || _InvoiceType == "pbd") && _isLack == false)
            {
                #region Accept
                MainWindow.mainWindow.Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                //w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                w.contentText = MainWindow.resourcemanager.GetString("trSaveInvoiceNotification");
                //w.contentText = "Do you want save pay invoice in drafts?";
                w.ShowDialog();
                MainWindow.mainWindow.Opacity = 1;
                #endregion
                if (w.isOk)
                {
                    await addInvoice(_InvoiceType);
                    clearInvoice();
                    _InvoiceType = "pd";
                }
            }
            clearInvoice();

        }
        private async Task saveBeforeTransfer()
        {
            if (billDetails.Count > 0 && _InvoiceType == "pd" && _isLack == false)
            {
                #region Accept
                MainWindow.mainWindow.Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                //w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                w.contentText = MainWindow.resourcemanager.GetString("trSaveInvoiceNotification");
                //w.contentText = "Do you want save pay invoice in drafts?";
                w.ShowDialog();
                MainWindow.mainWindow.Opacity = 1;
                #endregion
                if (w.isOk)
                {
                    await addInvoice(_InvoiceType);
                    clearInvoice();
                    _InvoiceType = "pd";
                    setNotifications();
                }
            }
            else
                clearInvoice();

        }
        #region loading

        List<keyValueBool> loadingList;


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
        async void loading_fillBranchesWithoutCurrent()
        {
            try
            {
                //await SectionData.fillBranchesWithoutCurrent(cb_branch, MainWindow.branchID.Value, "bs");
                await SectionData.fillBranches(cb_branch, "bs");
                cb_branch.SelectedValue = MainWindow.branchID;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_fillBranchesWithoutCurrent"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        async void loading_refrishVendors()
        {
            try
            {
                await RefrishVendors();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_refrishVendors"))
                {
                    item.value = true;
                    break;
                }
            }
        }
        //async void loading_fillBarcodeList()
        //{
        //    try
        //    {
        //        await fillBarcodeList();

        //    }
        //    catch (Exception ex)
        //    {
        //        SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
        //    }
        //    foreach (var item in loadingList)
        //    {
        //        if (item.key.Equals("loading_fillBarcodeList"))
        //        {
        //            item.value = true;
        //            break;
        //        }
        //    }
        //}
        async void loading_globalPurchaseUnits()
        {
            try
            {
                #region global purchase Units
                MainWindow.InvoiceGlobalItemUnitsList = await itemUnitModel.Getall();


                #endregion

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
            foreach (var item in loadingList)
            {
                if (item.key.Equals("loading_globalPurchaseUnits"))
                {
                    item.value = true;
                    break;
                }
            }
        }

        #endregion
        public async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.mainWindow.Closing += ParentWin_Closing;

                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                MainWindow.mainWindow.KeyDown += HandleKeyPress;
                tb_moneyIcon.Text = AppSettings.Currency;
                tb_moneyIcon1.Text = AppSettings.Currency;
                tb_moneyIconTotal.Text = AppSettings.Currency;

                dp_desrvedDate.SelectedDateChanged += this.dp_SelectedDateChanged;
                dp_invoiceDate.SelectedDateChanged += this.dp_SelectedDateChanged;

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
                catigoriesAndItemsView.ucPayInvoice = this;
                //List all the UIElement in the VisualTree
                controls = new List<Control>();

                #region loading
                loadingList = new List<keyValueBool>();
                bool isDone = true;
                loadingList.Add(new keyValueBool { key = "loading_RefrishItems", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillBranchesWithoutCurrent", value = false });
                loadingList.Add(new keyValueBool { key = "loading_refrishVendors", value = false });
                //loadingList.Add(new keyValueBool { key = "loading_fillBarcodeList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_globalPurchaseUnits", value = false });


                pos = MainWindow.posLogIn;
                loading_RefrishItems();
                loading_fillBranchesWithoutCurrent();
                loading_refrishVendors();
                //loading_fillBarcodeList();
                loading_globalPurchaseUnits();
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

                if (AppSettings.invoiceTax_bool == false)
                    sp_tax.Visibility = Visibility.Collapsed;
                else
                {
                    sp_tax.Visibility = Visibility.Visible;
                   // tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
                }
                setTimer();
                configureDiscountType();
                setNotifications();
                #region Style Date
                SectionData.defaultDatePickerStyle(dp_desrvedDate);
                SectionData.defaultDatePickerStyle(dp_invoiceDate);
                #endregion

                #region datagridChange
                //CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(dg_billDetails.Items);
                //((INotifyCollectionChanged)myCollectionView).CollectionChanged += new NotifyCollectionChangedEventHandler(DataGrid_CollectionChanged);
                #endregion
                //Walk through the VisualTree
                FindControl(this.grid_main, controls);

                #region Permision

                if (MainWindow.groupObject.HasPermissionAction(openOrderPermission, MainWindow.groupObjects, "one"))
                    md_orders.Visibility = Visibility.Visible;
                else
                    md_orders.Visibility = Visibility.Collapsed;

                if (MainWindow.groupObject.HasPermissionAction(returnPermission, MainWindow.groupObjects, "one"))
                {
                    brd_returnInvoice.Visibility = Visibility.Visible;
                }
                else
                {
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


                if (MainWindow.groupObject.HasPermissionAction(initializeShortagePermission, MainWindow.groupObjects, "one"))
                {
                    btn_shortageInvoice.Visibility = Visibility.Visible;
                    bdr_shortageInvoice.Visibility = Visibility.Visible;
                    md_shortage.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_shortageInvoice.Visibility = Visibility.Collapsed;
                    bdr_shortageInvoice.Visibility = Visibility.Collapsed;
                    md_shortage.Visibility = Visibility.Collapsed;
                }

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
                if (!isFromReport)
                {
                    btn_printInvoice.Visibility = Visibility.Collapsed;
                    btn_pdf.Visibility = Visibility.Collapsed;
                    btn_printCount.Visibility = Visibility.Collapsed;
                    btn_emailMessage.Visibility = Visibility.Collapsed;
                    bdr_emailMessage.Visibility = Visibility.Collapsed;
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

        #region bill
        public class Bill
        {
            public int Id { get; set; }
            public int Total { get; set; }

        }
        public class BillDetails
        {
            public int ID { get; set; }
            public int itemId { get; set; }
            public int itemUnitId { get; set; }
            public int basicItemUnitId { get; set; }
            public string Product { get; set; }
            public string Unit { get; set; }
            public int Count { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
            public int OrderId { get; set; }
            public string invType { get; set; }

            public List<Serial> itemSerials { get; set; }
            public List<Serial> returnedSerials { get; set; }
            public List<StoreProperty> ItemProperties { get; set; }
            public List<StoreProperty> StoreProperties { get; set; }
            public List<StoreProperty> ReturnedProperties { get; set; }

            public bool valid { get; set; }
            public string type { get; set; }
            public List<Item> packageItems { get; set; }
        }

        #endregion

        #region Button In DataGrid
        void deleteRowFromInvoiceItems(object sender, RoutedEventArgs e)
        {
            try
            {

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
 
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region timer to refresh notifications
        private void setTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(180); // 3minutes
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private async void timer_Tick(object sendert, EventArgs et)
        {
            try
            {
                setNotifications();
                //await setOrdersNotification();
                //setInvNotification();
                //setLackNotification();
                if (invoice.invoiceId != 0)
                {
                    refreshDocCount(invoice.invoiceId);
                    if (_InvoiceType == "pw" || _InvoiceType == "p" || _InvoiceType == "pb" || _InvoiceType == "pbw")
                        refreshInvoiceNot(invoice.invoiceId);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region notifications
        private void setNotifications()
        {
            try
            {
                refreshDraftNotification();
                refreshPurchaseNot();
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
                string invoiceType = "pd ,pbd";
                int duration = 2;
                if (AppSettings.PurchaseDraftCount <= 0)
                {
                    AppSettings.PurchaseDraftCount = (int)await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                    AppSettings.PurchaseDraftCount = AppSettings.PurchaseDraftCount < 0 ? 0 : AppSettings.PurchaseDraftCount;
                }
                setDraftNotification(AppSettings.PurchaseDraftCount);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }

        }
        private void setDraftNotification(int draftCount)
        {
            if (draftCount > 0 && invoice != null && (invoice.invType == "pd" || invoice.invType == "pbd") && invoice.invoiceId != 0 && !isFromReport)
                draftCount--;

            if (draftCount != _DraftCount)
            {
                if (draftCount > 9)
                    md_draft.Badge = "+9";
                else if (draftCount == 0) md_draft.Badge = "";
                else
                    md_draft.Badge = draftCount.ToString();
            }
            _DraftCount = draftCount;
        }
        private async Task refreshPurchaseNot()
        {
            try
            {
                int duration = 1;
                InvoiceResult notificationRes = await invoice.getPurNot(MainWindow.userID.Value, duration, MainWindow.branchID.Value);

                setInvNotification(notificationRes.InvoiceCount);
                setOrdersNotification(notificationRes.OrdersCount);
                setLackNotification(notificationRes.isThereLack);
            }
            catch { }
        }
        private void setInvNotification(int invCount)
        {
            try
            {
                // string invoiceType = "p ,pw ,pb ,pbw";
                //int duration = 1;
                // int invCount = await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                if (invoice != null && invoice.isArchived == false && (invoice.invType == "p" || invoice.invType == "pb" || invoice.invType == "pbw" || invoice.invType == "pw") && !isFromReport)
                    invCount--;

                if (invCount != _InvCount)
                {
                    if (invCount > 9)
                    {
                        md_invoices.Badge = "+9";
                    }
                    else if (invCount == 0) md_invoices.Badge = "";
                    else
                        md_invoices.Badge = invCount.ToString();
                }
                _InvCount = invCount;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void setOrdersNotification(int ordersCount)
        {
            try
            {
                //string invoiceType = "po";
                //int ordersCount = await invoice.GetCountUnHandeledOrders(invoiceType, 0, 0);
                if (invoice != null && _InvoiceType == "po" && invoice != null && invoice.invoiceId != 0)
                    ordersCount--;

                if (ordersCount != _OrderCount)
                {
                    if (ordersCount > 9)
                    {
                        md_orders.Badge = "+9";
                    }
                    else if (ordersCount == 0) md_orders.Badge = "";
                    else
                        md_orders.Badge = ordersCount.ToString();
                }
                _OrderCount = ordersCount;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void setLackNotification(string isThereLack)
        {
            try
            {
                if (isThereLack == "yes")
                    md_shortage.Badge = "!";
                else
                    md_shortage.Badge = "";
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async void refreshDocCount(int invoiceId)
        {
            try
            {
                DocImage doc = new DocImage();
                int docCount = (int)await doc.GetDocCount("Invoices", invoiceId);

                if (docCount != _DocCount)
                {
                    if (docCount > 9)
                        md_docImage.Badge = "+9";
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
    
        private async void refreshInvoiceNot(int invoiceId)
        {
            InvoiceResult notificationRes = await invoice.getInvoicePaymentArchiveCount(invoiceId);
            setPaymentsNotification(notificationRes.PaymentsCount);
            setArchiveNotification(notificationRes.InvoiceCount);
        }
        private async void setPaymentsNotification(int paymentsCount)
        {
            try
            {
                //if (paymentsCount == 0)
                //{
                //    bdr_payments.Visibility = Visibility.Collapsed;
                //    md_payments.Visibility = Visibility.Collapsed;
                //}
                //else if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one"))
                if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one"))
                {
                    bdr_payments.Visibility = Visibility.Visible;
                    md_payments.Visibility = Visibility.Visible;
                    if (paymentsCount != _PaymentCount)
                    {
                        if (paymentsCount > 9)
                            md_payments.Badge = "+9";
                        else if (paymentsCount == 0) md_payments.Badge = "";

                        else
                            md_payments.Badge = paymentsCount.ToString();
                    }
                    _PaymentCount = paymentsCount;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        int _ArchiveCount = 0;
        private void setArchiveNotification(int invCount)
        {
            try
            {
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

        private async void Btn_addVendor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                wd_updateVendor w = new wd_updateVendor();
                //// pass agent id to update windows
                w.agent.agentId = 0;
                w.type = "v";
                w.ShowDialog();
                Window.GetWindow(this).Opacity = 1;
                if (w.isOk == true)
                {
                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    await FillCombo.RefreshVendors();
                    await RefrishVendors();
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
        //private async void Cb_paymentProcessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (sender != null)
        //            SectionData.StartAwait(grid_main);
        //        TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
        //        if (elapsed.TotalMilliseconds > 100 && cb_paymentProcessType.SelectedIndex != -1)
        //        {
        //            _SelectedPaymentType = cb_paymentProcessType.SelectedValue.ToString();
        //        }
        //        else
        //        {
        //            cb_paymentProcessType.SelectedValue = _SelectedPaymentType;
        //        }

        //        switch (cb_paymentProcessType.SelectedIndex)
        //        {
        //            case 0://cash
        //                //gd_theRest.Visibility = Visibility.Visible;
        //                //tb_cashPaid.Text = txt_theRest.Text = "0";
        //                gd_card.Visibility = Visibility.Collapsed;
        //                tb_processNum.Clear();
        //                //cb_card.SelectedIndex = -1;
        //                _SelectedCard = -1;
        //                txt_card.Text = "";
        //                dp_desrvedDate.IsEnabled = false;
        //                // SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
        //                //SectionData.clearTextBlockValidate(txt_card, p_errorCard);
        //                //SectionData.clearValidate(tb_processNum, p_errorCard);
        //                break;
        //            case 1:// balance
        //                //gd_theRest.Visibility = Visibility.Collapsed;
        //                //tb_cashPaid.Text = txt_theRest.Text = "0";
        //                gd_card.Visibility = Visibility.Collapsed;
        //                dp_desrvedDate.IsEnabled = true;
        //                tb_processNum.Clear();
        //                //cb_card.SelectedIndex = -1;
        //                _SelectedCard = -1;
        //                //txt_card.Text = "";
        //                //SectionData.clearComboBoxValidate(cb_card, p_errorCard);
        //                //SectionData.clearTextBlockValidate(txt_card, p_errorCard);
        //                //SectionData.clearValidate(tb_processNum, p_errorCard);
        //                break;
        //            case 2://card
        //                //gd_theRest.Visibility = Visibility.Collapsed;
        //                //tb_cashPaid.Text = txt_theRest.Text = "0";
        //                dp_desrvedDate.IsEnabled = false;
        //                gd_card.Visibility = Visibility.Visible;
        //                // SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
        //                txt_card.Text = "";
        //                tb_processNum.Clear();
        //                tb_processNum.Visibility = Visibility.Collapsed;
        //                foreach (var el in cardEllipseList)
        //                {
        //                    el.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
        //                }
        //                CashTransfer cashTrasnfer = new CashTransfer();// cach transfer model
        //                if (invoice.invoiceId != 0 && invoice.invType == "p")//get payment information          
        //                {
        //                    cashTrasnfer = await cashTrasnfer.GetByInvId(invoice.invoiceId);
        //                }
        //                if (cashTrasnfer.cardId != null)
        //                {
        //                    Button btn = cardBtnList.Where(c => (int)c.Tag == cashTrasnfer.cardId.Value).FirstOrDefault();
        //                    card_Click(btn, null);
        //                }
        //                break;
        //            case 3://multiple
        //                //gd_theRest.Visibility = Visibility.Collapsed;
        //                //tb_cashPaid.Text = txt_theRest.Text = "0";
        //                gd_card.Visibility = Visibility.Collapsed;
        //                tb_processNum.Clear();
        //                _SelectedCard = -1;
        //                txt_card.Text = "";
        //                dp_desrvedDate.IsEnabled = true;
        //                //SectionData.clearComboBoxValidate(cb_customer, p_errorCustomer);
        //                //SectionData.clearTextBlockValidate(txt_card, p_errorCard);
        //                //SectionData.clearValidate(tb_processNum, p_errorCard);
        //                break;

        //        }
        //        if (sender != null)
        //            SectionData.EndAwait(grid_main);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (sender != null)
        //            SectionData.EndAwait(grid_main);
        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}
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
        private async void Btn_updateVendor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if ( cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                {

                    Window.GetWindow(this).Opacity = 0.2;
                    wd_updateVendor w = new wd_updateVendor();
                    //// pass agent id to update windows
                    w.agent.agentId = (int)cb_vendor.SelectedValue;
                    w.ShowDialog();
                    if (w.isOk == true)
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        await FillCombo.RefreshVendors();
                        await RefrishVendors();
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
        #region Categor and Item
        #region Refrish Y
        /// <summary>
        /// Category
        /// </summary>
        /// <returns></returns>

        async Task RefrishVendors()
        {
            await FillCombo.FillComboVendors(cb_vendor);
            if (FillCombo.vendorsList is null)
                await FillCombo.RefreshVendors();
            vendorsL = FillCombo.vendorsList;
        }

        async Task RefrishItems()
        {
            items = await itemModel.GetSaleOrPurItems(0, 0, 1, MainWindow.branchID.Value);
        }
        //async Task fillBarcodeList()
        //{
        //    barcodesList = await itemUnitModel.getAllBarcodes();
        //}
        #endregion
        #region Get Id By Click  Y

        public void ChangeCategoryIdEvent(int categoryId)
        {
        }


        public void ChangeItemIdEvent(int itemId)
        {
            try
            {
                item = items.ToList().Find(c => c.itemId == itemId);

                if (item != null)
                {
                    // get item units
                    itemUnits = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemId == item.itemId).ToList();
                    // search for default unit for purchase
                    var defaultPurUnit = itemUnits.ToList().Find(c => c.defaultPurchase == 1);
                    if (defaultPurUnit != null)
                    {
                        int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == defaultPurUnit.itemUnitId).FirstOrDefault());
                        if (index == -1)//item doesn't exist in bill
                        {
                            // create new row in bill details data grid
                            decimal price = 0;
                            try
                            {
                                price = (decimal)defaultPurUnit.cost; 
                            }
                            catch (Exception ex)
                            {
                                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                            }
                            decimal total = price;

                            bool valid = true;
                            if (item.type == "sn")
                                valid = false;

                            addRowToBill(defaultPurUnit.mainUnit, defaultPurUnit.itemUnitId, 1, 
                                            price, total,valid,item.type, item.packageItems);
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
                        bool valid = true;
                        if (item.type == "sn" )
                            valid = false;

                        addRowToBill( null, 0, 1, 0, 0,valid,item.type,item.packageItems);

                    }

                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion



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


        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_invoiceNumber")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorInvoiceNumber, tt_errorInvoiceNumber, "trErrorEmptyInvNumToolTip");
                    //else if ((sender as TextBox).Name == "tb_processNum")
                    //    SectionData.validateEmptyTextBox((TextBox)sender, p_errorProcessNum, tt_errorProcessNum, "trEmptyProcessNumToolTip");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_branch")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorBranch, tt_errorBranch, "trEmptyBranchToolTip");
                    //if ((sender as ComboBox).Name == "cb_vendor")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorVendor, tt_errorVendor, "trErrorEmptyVendorToolTip");
                }
                else
                {
                    if ((sender as DatePicker).Name == "dp_desrvedDate")
                        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorDesrvedDate, tt_errorDesrvedDate, "trErrorEmptyDeservedDate");
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

            #region invoice object
            if ((invoice.invType == "p" || invoice.invType == "pw") && (invType == "pbw" || invType == "pbd")) // invoice is purchase and will be bounce purchase  or purchase bounce draft , save another invoice in db
            {
                invoice.invoiceMainId = invoice.invoiceId;
                invoice.invoiceId = 0;
                invoice.invNumber = "pb";
                invoice.branchCreatorId = MainWindow.branchID.Value;
                invoice.posId = MainWindow.posID.Value;
            }
            else if (invoice.invType == "po")
            {
                invoice.invNumber = "pi";
            }
            else if (invType == "pd" && invoice.invoiceId == 0)
                invoice.invNumber = "pd";

            if (invoice.branchCreatorId == 0 || invoice.branchCreatorId == null)
            {
                invoice.branchCreatorId = MainWindow.branchID.Value;
                invoice.posId = MainWindow.posID.Value;
            }

            if (invoice.invType != "pw" || invoice.invoiceId == 0)
            {
                invoice.invType = invType;
                if (!tb_discount.Text.Equals(""))
                    invoice.discountValue = decimal.Parse(tb_discount.Text);

                 if (!tb_shippingCost.Text.Equals(""))
                    invoice.shippingCost = decimal.Parse(tb_shippingCost.Text);

                if (cb_typeDiscount.SelectedIndex != -1)
                    invoice.discountType = cb_typeDiscount.SelectedValue.ToString();

                
                invoice.total = _Sum;
                invoice.totalNet = decimal.Parse(tb_total.Text);
                invoice.paid = 0;
                invoice.deserved = invoice.totalNet;

                if (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                    invoice.agentId = (int)cb_vendor.SelectedValue;

                invoice.branchId = (int)cb_branch.SelectedValue;
                invoice.deservedDate = dp_desrvedDate.SelectedDate;
                invoice.vendorInvNum = tb_invoiceNumber.Text;
                invoice.vendorInvDate = dp_invoiceDate.SelectedDate;
                invoice.notes = tb_note.Text;
                invoice.taxtype = 1;
                if (tb_taxValue.Text != "" && AppSettings.invoiceTax_bool == true)
                {
                    invoice.tax =decimal.Parse(tb_taxValue.Text);
                    invoice.taxValue = _TaxValue;
                }
                else
                {
                    invoice.tax = 0;
                    invoice.taxValue = 0;
                }

                invoice.createUserId = MainWindow.userID;
                invoice.updateUserId = MainWindow.userID;
                invoice.isOrginal = true;
                if (invType == "pw" || invType == "p")
                    invoice.invNumber = "pi";
                #endregion

            #region invoice items
            invoiceItems = new List<ItemTransfer>();
            ItemTransfer itemT;
            for (int i = 0; i < billDetails.Count; i++)
            {
                itemT = new ItemTransfer();

                itemT.quantity = billDetails[i].Count;
                itemT.price = billDetails[i].Price;
                itemT.itemUnitId = billDetails[i].itemUnitId;
                itemT.createUserId = MainWindow.userID;
                itemT.unitName = billDetails[i].Unit;
                itemT.itemName = billDetails[i].Product;
                itemT.itemId = billDetails[i].itemId;
                itemT.invoiceId = 0;
                if (billDetails[i].returnedSerials == null)
                    itemT.itemSerials = billDetails[i].itemSerials;
                else
                    itemT.itemSerials = billDetails[i].itemSerials.Where(x => !billDetails[i].returnedSerials.Any(e => e.serialNum == x.serialNum)).ToList();

                itemT.returnedSerials = billDetails[i].returnedSerials;

                itemT.ItemStoreProperties = billDetails[i].StoreProperties;
                itemT.ReturnedProperties = billDetails[i].ReturnedProperties;
                invoiceItems.Add(itemT);
            }
            #endregion
            // save invoice in DB
            switch (invType)
            {
                case "pbw":
                    #region notification Object
                    Notification not = new Notification()
                    {
                        title = "trPurchaseReturnInvoiceAlertTilte",
                        ncontent = "trPurchaseReturnInvoiceAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_ctreatePurchaseReturnInvoice",
                        branchId = (int)invoice.branchCreatorId,
                        prefix = MainWindow.loginBranch.name,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };
                    #endregion
                    #region posCash posCash with type inv
                    var cashT = invoice.posCashTransfer(invoice, "pb");
                    #endregion
                    invoiceResult = await invoiceModel.savePurchaseBounce(invoice, invoiceItems, listPayments, cashT, not, MainWindow.posID.Value, MainWindow.branchID.Value);
                    break;
                case "p":
                case "pw":
                    #region notification Object
                    Notification amountNot = new Notification()
                    {
                        title = "trExceedMaxLimitAlertTilte",
                        ncontent = "trExceedMaxLimitAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_minMaxItem",
                        branchId = MainWindow.branchID.Value,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };
                    #endregion

                    #region purchase wait alert
                    int branchId = 0;
                     if ((int)cb_branch.SelectedValue != MainWindow.branchID)
                        branchId = (int)cb_branch.SelectedValue;
                    Notification waitNot = new Notification()
                    {
                        title = "trPurchaseInvoiceAlertTilte",
                        ncontent = "trPurchaseInvoiceAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_ctreatePurchaseInvoice",
                        prefix = MainWindow.loginBranch.name,
                        branchId = branchId,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };
                    #endregion

                    #region posCash
                    CashTransfer posCashTransfer = invoice.posCashTransfer(invoice, "pi");
                    #endregion
                    invoiceResult = await invoiceModel.savePurchaseInvoice(invoice, invoiceItems, amountNot, waitNot, posCashTransfer, listPayments, MainWindow.posID.Value);

                    break;

                default:
                    invoiceResult = await invoiceModel.savePurchaseDraft(invoice, invoiceItems, MainWindow.posID.Value);
                    break;
            };

                if (invoiceResult.Result > 0) // success
                {
                    prInvoiceId = invoiceResult.Result;
                    invoice.invoiceId = invoiceResult.Result;
                    invoice.invNumber = invoiceResult.Message;
                    invoice.updateDate = invoiceResult.UpdateDate;
                    TimeSpan ts;
                    TimeSpan.TryParse(invoiceResult.InvTime, out ts);
                    invoice.invTime = ts;

                    AppSettings.PurchaseDraftCount = invoiceResult.PurchaseDraftCount;
                    AppSettings.PosBalance = invoiceResult.PosBalance;
                    MainWindow.setBalance();

                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                    #region print
                    prInvoice = invoice;
                    ///////////////////////////////////////

                    if (prInvoice.invType == "pw" || prInvoice.invType == "p")
                    {
                        //Thread t = new Thread(async () =>
                        //{
                            if (AppSettings.print_on_save_pur == "1")
                            {
                                Thread t1 = new Thread(async () =>
                                {
                                    string msg = "";
                                    List<PayedInvclass> payedlist = new List<PayedInvclass>();
                                    payedlist = await cashTransfer.PayedBycashlist(listPayments);
                                 msg=await printPurInvoice(prInvoice, invoiceItems, payedlist);
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
                            }
                            if (AppSettings.email_on_save_pur == "1")
                            {
                                Thread t2 = new Thread(async () =>
                                {
                                    string msg = "";
                                    List<PayedInvclass> payedlist = new List<PayedInvclass>();
                                    payedlist = await cashTransfer.PayedBycashlist(listPayments);
                                    if (prInvoice.invDate==null) {
                                        prInvoice.invDate =DateTime.Now;
                                    }
                                    msg = await sendPurEmail(prInvoice, invoiceItems, payedlist);
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
                                t2.Start();
                            }
                        //});
                        //t.Start();

                    }
                    #endregion

                    MainWindow.InvoiceGlobalItemUnitsList = await itemUnitModel.Getall();

                    clearInvoice();
                    setNotifications();
                    #region old
                    //int s = await invoiceModel.saveInvoiceItems(invoiceItems, invoiceId);

                    //if (invType == "p")
                    //    invoiceModel.saveAvgPurchasePrice(invoiceItems);
                    #endregion
                }
                else if (invoiceResult.Result.Equals(-2))// رصيد pos غير كاف
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                else if (invoiceResult.Result == -10) // كمية الخصائص المرجعة غير كافية
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("PropertiesNotAvailable"), animation: ToasterAnimation.FadeIn);
                else if (invoiceResult.Result == -9)// الكمية المرجعة أكبر من الكمية المشتراة
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                else if (invoiceResult.Result.Equals(-3))// الكمية في المخزن غير كافية في حالة الإرجاع
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
                //else if (invoiceResult.Result == -4) // رصيد المورد غير كاف في حالة الإرجاع
                //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorMaxDeservedExceeded"), animation: ToasterAnimation.FadeIn);
                else
                    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            }

        }
        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DateTime desrveDate;
                if (dp_desrvedDate.SelectedDate != null)
                {
                    desrveDate = (DateTime)dp_desrvedDate.SelectedDate.Value.Date;
                    if (desrveDate.Date < DateTime.Now.Date && dp_desrvedDate.IsFocused)
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorInvDateAfterDeserveToolTip"), animation: ToasterAnimation.FadeIn);
                        dp_desrvedDate.Text = "";
                    }
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        DateTime? _DeservedDate;
        private bool validateInvoiceValues()
        {
            if (decimal.Parse(tb_total.Text) == 0)
            {
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorTotalIsZeroToolTip"), animation: ToasterAnimation.FadeIn);
                return false;
            }
            //if (cb_paymentProcessType.SelectedValue.ToString() == "cash" && !_InvoiceType.Equals("pbd") && (MainWindow.posLogIn.balance < decimal.Parse(tb_total.Text) || MainWindow.posLogIn.balance == null))
            //{
            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
            //    return false;
            //}
            //else if (cb_paymentProcessType.SelectedValue.ToString() == "balance")
            //{
            //    if (!SectionData.validateEmptyComboBox(cb_vendor, p_errorVendor, tt_errorVendor, "trErrorEmptyVendorToolTip"))
            //        exp_vendor.IsExpanded = true;
            //    if (cb_vendor.SelectedIndex < 1)
            //        return false;
            //}
            //else if (cb_paymentProcessType.SelectedValue.ToString() == "card")
            //{
            //    if (_SelectedCard == -1)
            //    {
            //        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectCreditCard"), animation: ToasterAnimation.FadeIn);
            //        return false;
            //    }
            //    else
            //    {
            //        var card = cards.Where(x => x.cardId == _SelectedCard).FirstOrDefault();
            //        if (card.hasProcessNum.Value && tb_processNum.Text.Equals(""))
            //        {
            //            SectionData.validateEmptyTextBox(tb_processNum, p_errorProcessNum, tt_errorProcessNum, "trEmptyProcessNumToolTip");
            //            return false;
            //        }
            //    }
            //}
           
            //if (cb_vendor.SelectedIndex > 0 && cb_paymentProcessType.SelectedValue.ToString() != "cash" && cb_paymentProcessType.SelectedValue.ToString() != "card")
            //{
            //    if (!SectionData.validateEmptyDatePicker(dp_desrvedDate, p_errorDesrvedDate, tt_errorDesrvedDate, "trErrorEmptyDeservedDate"))
            //        exp_vendor.IsExpanded = true;
            //    if (!SectionData.validateEmptyTextBox(tb_invoiceNumber, p_errorInvoiceNumber, tt_errorInvoiceNumber, "trErrorEmptyInvNumToolTip"))
            //        exp_vendor.IsExpanded = true;
            //    if (dp_desrvedDate.Text.Equals("") || tb_invoiceNumber.Text.Equals(""))
            //        return false;
            //}

            #region validate deserved date
                //foreach (var pay in listPayments)
                //{
                //    if (pay.processType.Equals("balance"))
                //    {
                //        if (!SectionData.validateEmptyDatePicker(dp_desrvedDate, p_errorDesrvedDate, tt_errorDesrvedDate, "trErrorEmptyDeservedDate"))
                //            exp_vendor.IsExpanded = true;
                //        if (!SectionData.validateEmptyTextBox(tb_invoiceNumber, p_errorInvoiceNumber, tt_errorInvoiceNumber, "trErrorEmptyInvNumToolTip"))
                //            exp_vendor.IsExpanded = true;
                //        if (dp_desrvedDate.Text.Equals("") || tb_invoiceNumber.Text.Equals(""))
                //            return false;
                //        else
                //            _DeservedDate = dp_desrvedDate.SelectedDate;
                //    }
                //}
            #endregion
            return true;
        }

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(invoicePermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {                      
                        bool valid = validateItemUnits();
                        //check mandatory inputs
                        bool validate = validateInvoiceValues();
                        bool multipleValid = true;
                        listPayments = new List<CashTransfer>();
                        TextBox tb = (TextBox)dp_desrvedDate.Template.FindName("PART_TextBox", dp_desrvedDate);

                        if (valid && validate)
                        {
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_multiplePayment w = new wd_multiplePayment();
                            if (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                                w.hasCredit = true;
                            else
                                w.hasCredit = false;
                            w.isPurchase = true;
                            w.invoice.invType = _InvoiceType;
                            w.invoice.totalNet = decimal.Parse(tb_total.Text);

                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            multipleValid = w.isOk;
                            listPayments = w.listPayments;

                            if (multipleValid)
                            {                              
                                switch (_InvoiceType)
                                {
                                    case "pbd":
                                        await addInvoice("pbw"); // pbw means waiting purchase bounce
                                        break;

                                    default:
                                        foreach (var item in listPayments)
                                        {
                                            item.transType = "p"; //pull
                                            item.posId = MainWindow.posID;
                                            if (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                                                item.agentId = (int)cb_vendor.SelectedValue;

                                            item.transNum = "pv";
                                            item.side = "v"; // vendor
                                            item.createUserId = MainWindow.userID;
                                        }
                                        if ((int)cb_branch.SelectedValue == MainWindow.branchID) // reciept invoice directly
                                        {
                                            await addInvoice("p");
                                        }
                                        else
                                        {
                                            await addInvoice("pw");
                                        }
                                        break;
                                };
                            }                         

                        }
                    }
                    else //box is closed
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
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
        //private void oneCashTransfer()
        //{
        //    CashTransfer cashTrasnfer = new CashTransfer();
        //    cashTrasnfer.cash = decimal.Parse(tb_total.Text);
        //    cashTrasnfer.processType = cb_paymentProcessType.SelectedValue.ToString();
        //    listPayments.Add(cashTrasnfer);
        //}
        //private async Task saveConfiguredCashTrans(CashTransfer cashTransfer)
        //{
        //    switch (cashTransfer.processType)
        //    {
        //        case "cash":// cash: update pos balance  
        //            pos = MainWindow.posLogIn;
        //            if (MainWindow.posLogIn.balance > 0)
        //            {
        //                if (MainWindow.posLogIn.balance >= cashTransfer.cash)
        //                {
        //                    MainWindow.posLogIn.balance -= cashTransfer.cash;
        //                    invoice.paid = cashTransfer.cash;
        //                    invoice.deserved -= cashTransfer.cash;
        //                }
        //                else
        //                {
        //                    invoice.paid = MainWindow.posLogIn.balance;
        //                    cashTransfer.cash = MainWindow.posLogIn.balance;
        //                    invoice.deserved -= MainWindow.posLogIn.balance;
        //                    MainWindow.posLogIn.balance = 0;
        //                }
        //                await pos.save(MainWindow.posLogIn);
        //                await cashTransfer.Save(cashTransfer); //add cash transfer  
        //                await invoice.saveInvoice(invoice);
        //            }
        //            break;
        //        case "balance":// balance: update customer balance
        //            await invoice.recordConfiguredAgentCash(invoice, "pi", cashTransfer);

        //            await invoice.saveInvoice(invoice);
        //            break;
        //        case "card": // card  
        //            cashTransfer.docNum = tb_processNum.Text;
        //            await cashTransfer.Save(cashTransfer); //add cash transfer 
        //            invoice.paid += cashTransfer.cash;
        //            invoice.deserved -= cashTransfer.cash;
        //            await invoice.saveInvoice(invoice);
        //            break;
        //    }
        //}
        //private async Task saveCashTransfer(CashTransfer cashTransfer)
        //{
        //    switch (cb_paymentProcessType.SelectedValue)
        //    {
        //        case "cash":// cash: update pos balance
        //            pos = MainWindow.posLogIn;
        //            if (MainWindow.posLogIn.balance > 0)
        //            {
        //                if (MainWindow.posLogIn.balance >= invoice.totalNet)
        //                {
        //                    MainWindow.posLogIn.balance -= invoice.totalNet;
        //                    invoice.paid = cashTransfer.cash = invoice.totalNet;
        //                    invoice.deserved = 0;
        //                }
        //                else
        //                {
        //                    invoice.paid = cashTransfer.cash = MainWindow.posLogIn.balance;
        //                    invoice.deserved -= MainWindow.posLogIn.balance;
        //                    MainWindow.posLogIn.balance = 0;
        //                }
        //                await pos.save(MainWindow.posLogIn);
        //                await cashTransfer.Save(cashTransfer); //add cash transfer  
        //                await invoice.saveInvoice(invoice);
        //            }
        //            break;
        //        case "balance":// balance: update vendor balance                
        //            await invoice.recordCashTransfer(invoice, "pi");
        //            break;
        //        case "card":
        //            cashTransfer.cash = invoice.totalNet;
        //            cashTransfer.cardId = _SelectedCard;
        //            cashTransfer.docNum = tb_processNum.Text;
        //            await cashTransfer.Save(cashTransfer); //add cash transfer  
        //            invoice.paid = invoice.totalNet;
        //            invoice.deserved = 0;
        //            await invoice.saveInvoice(invoice);
        //            break;
        //    }
        //}
        private void Tb_EnglishDigit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only english and digits
            Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
            if (!regex.IsMatch(e.Text))
                e.Handled = true;
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

                if (billDetails.Count > 0 && _isLack == false && (_InvoiceType == "pd" || _InvoiceType == "pbd"))
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
                        }
                        clearInvoice();
                        _InvoiceType = "pd";
                    }
                    else if (billDetails.Count == 0)
                    {
                        clearInvoice();
                        _InvoiceType = "pd";
                    }
                }
                else
                    clearInvoice();

                setNotifications();

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
        private void clearInvoice()
        {
            _Sum = 0;

            txt_invNumber.Text = "";
            _SequenceNum = 0;
            _SelectedBranch = -1;
            _SelectedVendor = -1;
            _SelectedCard = -1;
            invoice = new Invoice();
            tb_barcode.Clear();
            tb_shippingCost.Text = "";
            cb_branch.SelectedValue = MainWindow.branchID;
            cb_vendor.SelectedIndex = -1;
            txt_payType.Text = "-";
            cb_vendor.SelectedItem = "";
            cb_typeDiscount.SelectedIndex = 0;
            dp_desrvedDate.Text = "";
            _DeservedDate = null;
            tb_invoiceNumber.Clear();
            dp_invoiceDate.Text = "";
            tb_note.Clear();
            tb_discount.Clear();
            tb_taxValue.Clear();
            billDetails.Clear();
            tb_total.Text = "0";
            tb_sum.Text = "0";
  

            isFromReport = false;
            archived = false;
            #region clear notification
            md_docImage.Badge = "";
            md_invoiceArchive.Badge = "";
            md_payments.Badge = "";
            md_invoiceArchive.Visibility = Visibility.Collapsed;
            bdr_invoiceArchive.Visibility = Visibility.Collapsed;

            _PaymentCount = 0;
            _DocCount = 0;
            _ArchiveCount = 0;
            #endregion
            TextBox tbStartDate = (TextBox)dp_desrvedDate.Template.FindName("PART_TextBox", dp_desrvedDate);
            SectionData.clearValidate(tbStartDate, p_errorDesrvedDate);
            SectionData.clearComboBoxValidate(cb_vendor, p_errorVendor);
            SectionData.clearValidate(tb_invoiceNumber, p_errorInvoiceNumber);
            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseBill");
            btn_save.Content = MainWindow.resourcemanager.GetString("purchase");

            refrishBillDetails();
            _InvoiceType = "pd";
            _isLack = false;
            inputEditable();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
        }
        #endregion
        private void clearNavigation()
        {
            _Sum = 0;

            txt_invNumber.Text = "";
            _SequenceNum = 0;
            _SelectedBranch = -1;
            _SelectedVendor = -1;
            invoice = new Invoice();
            tb_barcode.Clear();
            cb_branch.SelectedIndex = -1;
            cb_vendor.SelectedIndex = -1;
            cb_vendor.SelectedItem = "";
            cb_typeDiscount.SelectedIndex = 0;
            dp_desrvedDate.Text = "";
            txt_vendorIvoiceDetails.Text = "";
            tb_invoiceNumber.Clear();
            dp_invoiceDate.Text = "";
            tb_note.Clear();
            tb_discount.Clear();
            tb_taxValue.Clear();
            billDetails.Clear();

            tb_total.Text = "0";
            tb_sum.Text = "0";

            isFromReport = false;
            archived = false;
            md_docImage.Badge = "";
            md_payments.Badge = "";

            TextBox tbStartDate = (TextBox)dp_desrvedDate.Template.FindName("PART_TextBox", dp_desrvedDate);
            SectionData.clearValidate(tbStartDate, p_errorDesrvedDate);
            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
            refrishBillDetails();
        }
        private async void Btn_draft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();
                string invoiceType = "pd ,pbd";
                int duration = 2;
                w.invoiceType = invoiceType;
                w.userId = MainWindow.userLogin.userId;
                w.duration = duration; // view drafts which created during 2 last days 

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
                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;

                        navigateBtnActivate();
                        md_payments.Badge = "";
                        if (_InvoiceType == "pd")// set title to bill
                        {
                            mainInvoiceItems = invoiceItems;
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftPurchaseBill");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                        }
                        if (_InvoiceType == "pbd")
                        {
                            mainInvoiceItems = await invoiceModel.GetInvoicesItems(invoice.invoiceMainId.Value);
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftBounceBill");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                        }
                        setNotifications();
                        refreshDocCount(invoice.invoiceId);
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
        private async void Btn_invoices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await saveBeforeTransfer();
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();

                // purchase invoices
                string invoiceType = "p , pw , pb, pbw";
                int duration = 1;
                w.invoiceType = invoiceType;
                w.userId = MainWindow.userLogin.userId;
                w.duration = duration; // view drafts which created during 1 last days 

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
                        setNotifications();
                        refreshInvoiceNot(_invoiceId);
                        refreshDocCount(invoice.invoiceId);
                        #region set title to bill - show archive btn
                        if (invoice.invType == "p" || invoice.invType == "pw")
                        {
                            if (invoice.ChildInvoice != null)
                            {
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurInvoiceUpdated");
                                bdr_invoiceArchive.Visibility = Visibility.Visible;
                                md_invoiceArchive.Visibility = Visibility.Visible;
                                txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trReturns");
                            }
                            else
                                txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                        }
                        else
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                            bdr_invoiceArchive.Visibility = Visibility.Visible;
                                md_invoiceArchive.Visibility = Visibility.Visible;
                            txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trInvoice");
                        }
                        #endregion
                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                        navigateBtnActivate();
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
        private async void Btn_purchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await saveBeforeTransfer();
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();

                // purchase orders
                string invoiceType = "po";
                w.invoiceType = invoiceType;
                w.condition = "orders";
                w.branchId = MainWindow.branchID.Value;
                //w.branchCreatorId = MainWindow.branchID.Value;
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
                        // notifications
                        md_payments.Badge = "";
                        setNotifications();
                        refreshDocCount(invoice.invoiceId);

                        // set title to bill
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseOrder");
                        txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                        //invoices = await invoice.getUnHandeldOrders(invoiceType, 0, MainWindow.branchID.Value);
                        navigateBtnActivate();
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
        public async Task fillInvoiceInputs(Invoice invoice)
        {
            if (isFromReport)
            {
                invoice = await invoice.GetByInvoiceId(invoice.invoiceId);
                refreshInvoiceNot(invoice.invoiceId);
            }
            #region tax
            if (_InvoiceType == "p" || _InvoiceType == "pw" || _InvoiceType == "pd")
            {
                if (invoice.tax != null)
                    tb_taxValue.Text = SectionData.DecTostring(invoice.taxValue);
                else
                    tb_taxValue.Text = "0";
            }
            else if (_InvoiceType == "pbw" || _InvoiceType == "pb" || _InvoiceType == "pbd")
            {
                tb_taxValue.Text = "0";
            }
            #endregion
            _Sum = (decimal)invoice.total;
            txt_invNumber.Text = invoice.invNumber.ToString();
            cb_branch.SelectedValue = invoice.branchId;
            cb_vendor.SelectedValue = invoice.agentId;
            dp_desrvedDate.Text = invoice.deservedDate.ToString();
            tb_invoiceNumber.Text = invoice.vendorInvNum;
            dp_invoiceDate.Text = invoice.vendorInvDate.ToString();

            if (invoice.shippingCost != 0)
                tb_shippingCost.Text = SectionData.DecTostring(invoice.shippingCost);
            else tb_shippingCost.Text = "0";

            if (invoice.totalNet != 0)
                tb_total.Text = SectionData.DecTostring(invoice.totalNet);
            else tb_total.Text = "0";


            tb_note.Text = invoice.notes;

            if (invoice.total != 0)
                tb_sum.Text = SectionData.DecTostring(invoice.total);
            else tb_sum.Text = "0";

            if ((invoice.DBDiscountValue != 0) && (invoice.DBDiscountValue != null))
                tb_discount.Text = SectionData.PercentageDecTostring(invoice.DBDiscountValue);
            else
                tb_discount.Text = "0";

            if (invoice.discountType == "1")
                cb_typeDiscount.SelectedIndex = 1;
            else if (invoice.discountType == "2")
                cb_typeDiscount.SelectedIndex = 2;
            else
                cb_typeDiscount.SelectedIndex = 0;

            // build invoice details grid
            await buildInvoiceDetails(invoice);
            refreshTotalValue();
            inputEditable();
        }
        private async void Btn_returnInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(returnPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (_InvoiceType == "p")
                    {
                        _InvoiceType = "pbd";
                        isFromReport = true;
                        archived = false;

                        if (invoice.ChildInvoice != null)
                            await fillInvoiceInputs(invoice.ChildInvoice);
                        else
                            await fillInvoiceInputs(invoice);

                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                        txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                        btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                        //setInvNotification();
                        setNotifications();
                    }
                    else
                    {
                        await saveBeforeExit();
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_returnInvoice w = new wd_returnInvoice();
                        w.fromPurchase = true;
                        w.userId = MainWindow.userID.Value;
                        w.invoiceType = "p";
                        w.ShowDialog();
                        if (w.DialogResult == true)
                        {
                            _InvoiceType = "pbd";
                            invoice = w.invoice;
                            isFromReport = true;
                            archived = false;
                            await fillInvoiceInputs(invoice);
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                            btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                            setNotifications();
                        }
                        //wd_invoice w = new wd_invoice();

                        //w.title = MainWindow.resourcemanager.GetString("trReturn");
                        //// purchase invoices
                        //string invoiceType = "p";
                        //w.invoiceType = invoiceType; // invoice type to view in grid
                        //if (SectionData.isAdminPermision())
                        //    w.condition = "admin";
                        //else
                        //    w.condition = "return";
                        //w.userId = MainWindow.userID.Value;
                        //if (w.ShowDialog() == true)
                        //{
                        //    if (w.invoice != null)
                        //    {
                        //        _InvoiceType = "pbd";
                        //        invoice = w.invoice;
                        //        _invoiceId = invoice.invoiceId;                           
                        //        #region refresh notification
                        //        isFromReport = false;
                        //        archived = false;
                        //        setNotifications();
                        //        refreshPaymentsNotification(_invoiceId);
                        //        refreshDocCount(invoice.invoiceId);
                        //        #endregion
                        //        md_payments.Badge = "";

                        //        await fillInvoiceInputs(invoice);
                        //        //invoices = await invoice.getBranchInvoices(invoiceType, MainWindow.branchID.Value, MainWindow.branchID.Value);
                        //        if (w.condition == "admin")
                        //            invoices = await invoice.GetInvoicesForAdmin(invoiceType, 0);
                        //        else
                        //            invoices = await invoice.getBranchInvoices(invoiceType, MainWindow.branchID.Value, MainWindow.branchID.Value);
                        //        navigateBtnActivate();
                        //        mainInvoiceItems = invoiceItems;

                        //        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                        //        txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                        //    }
                        //}
                        Window.GetWindow(this).Opacity = 1;
                    }
                    mainInvoiceItems = invoiceItems;
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
            # region get invoice items
            if (_InvoiceType == "po")
                invoiceItems = await invoiceModel.GetInvoicesItemsWithCost(invoice.invoiceId);
            else if(invoice.invoiceItems == null)
                invoiceItems = await invoiceModel.GetInvoicesItems(invoice.invoiceId);
            else
                invoiceItems = invoice.invoiceItems;
            #endregion
            // build invoice details grid
            _SequenceNum = 0;
            _Sum = 0;
            billDetails.Clear();
            foreach (ItemTransfer itemT in invoiceItems)
            {
                _SequenceNum++;
                decimal total = (decimal)(itemT.price * itemT.quantity);
                int orderId = 0;
                if (itemT.invoiceId != null)
                    orderId = (int)itemT.invoiceId;

                bool isValid = true;
                if (_InvoiceType == "pd" || _InvoiceType == "pbd" || _InvoiceType == "po")
                {
                    if(itemT.itemType == "sn" && _InvoiceType.Equals("pbd") && (invoice.invType.Equals("p") || itemT.itemSerials.Count() < itemT.quantity))
                        isValid = false;

                    else if (itemT.itemType == "sn" && itemT.itemSerials.Count() < itemT.quantity)
                        isValid = false;
                }

                var unit = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemUnitId == itemT.itemUnitId).FirstOrDefault();
                billDetails.Add(new BillDetails()
                {
                    ID = _SequenceNum,
                    Product = itemT.itemName,
                    itemId = (int)itemT.itemId,
                    Unit = itemT.unitName,
                    itemUnitId = (int)itemT.itemUnitId,
                    basicItemUnitId = (int)itemT.itemUnitId,
                    Count = (int)itemT.quantity,
                    Price = decimal.Parse(SectionData.DecTostring((decimal)itemT.price)),
                    Total = total,
                    OrderId = orderId,
                    invType = invoice.invType,
                    itemSerials = itemT.itemSerials,
                    packageItems = itemT.packageItems,
                    StoreProperties=itemT.ItemStoreProperties,
                    ItemProperties = unit.ItemProperties,
                    valid = isValid,
                });

                _Sum += total;
            }

            tb_barcode.Focus();

            refrishBillDetails();
        }
        private void inputEditable()
        {
            if (_InvoiceType == "pbw") // purchase invoice
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                dg_billDetails.Columns[5].IsReadOnly = false; //make price read only
                dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                cb_vendor.IsEnabled = false;
                dp_desrvedDate.IsEnabled = false;
                dp_invoiceDate.IsEnabled = false;
                tb_note.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                cb_branch.IsEnabled = false;
                tb_discount.IsEnabled = false;
                cb_typeDiscount.IsEnabled = false;
                btn_save.IsEnabled = false;
                tb_invoiceNumber.IsEnabled = false;
                tb_shippingCost.IsEnabled = false;
                tb_taxValue.IsEnabled = false;

                bdr_invoiceArchive.Visibility = Visibility.Visible;
                md_invoiceArchive.Visibility = Visibility.Visible;
              

            }
            else if (_InvoiceType == "pbd") // return invoice
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                dg_billDetails.Columns[5].IsReadOnly = false; //make price read only
                dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                cb_vendor.IsEnabled = false;
                dp_desrvedDate.IsEnabled = false;
                dp_invoiceDate.IsEnabled = false;
                tb_note.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                cb_branch.IsEnabled = true;
                tb_discount.IsEnabled = false;
                cb_typeDiscount.IsEnabled = false;
                btn_save.IsEnabled = true;
                tb_invoiceNumber.IsEnabled = false;
                tb_shippingCost.IsEnabled = false;

                btn_items.IsEnabled = false;
                btn_clear.IsEnabled = true;
                btn_addVendor.IsEnabled = false;
                tb_taxValue.IsEnabled = false;

                bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                md_invoiceArchive.Visibility = Visibility.Collapsed;
            }
            else if (_InvoiceType == "pd") // purchase draft 
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                dg_billDetails.Columns[5].IsReadOnly = false;
                dg_billDetails.Columns[3].IsReadOnly = false;
                dg_billDetails.Columns[4].IsReadOnly = false;
                cb_vendor.IsEnabled = true;
                dp_desrvedDate.IsEnabled = true;
                dp_invoiceDate.IsEnabled = true;
                tb_note.IsEnabled = true;
                tb_barcode.IsEnabled = true;
                cb_branch.IsEnabled = true;
                tb_discount.IsEnabled = true;
                cb_typeDiscount.IsEnabled = true;
                btn_save.IsEnabled = true;
                tb_invoiceNumber.IsEnabled = true;
                btn_items.IsEnabled = true;
                btn_clear.IsEnabled = true;
                btn_addVendor.IsEnabled = true;
                bdr_payments.Visibility = Visibility.Collapsed;
                md_payments.Visibility = Visibility.Collapsed;
                tb_taxValue.IsEnabled = true;

                if (AppSettings.invoiceTax_bool == false)
                {
                    sp_tax.Visibility = Visibility.Collapsed;
                    //tb_taxValue.Text = "0";
                }
                else
                {
                    sp_tax.Visibility = Visibility.Visible;
                    //tb_taxValue.Text = SectionData.PercentageDecTostring(AppSettings.invoiceTax_decimal);
                }
                tb_shippingCost.IsEnabled = true;

                bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                md_invoiceArchive.Visibility = Visibility.Collapsed;
            }
            else if (_InvoiceType == "po") //  purchase order
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                dg_billDetails.Columns[5].IsReadOnly = false;
                dg_billDetails.Columns[3].IsReadOnly = false;
                dg_billDetails.Columns[4].IsReadOnly = false;
                cb_vendor.IsEnabled = false;
                dp_desrvedDate.IsEnabled = true;
                dp_invoiceDate.IsEnabled = true;
                tb_note.IsEnabled = true;
                tb_barcode.IsEnabled = true;
                cb_branch.IsEnabled = true;
                tb_discount.IsEnabled = true;
                cb_typeDiscount.IsEnabled = true;
                btn_save.IsEnabled = true;
                tb_invoiceNumber.IsEnabled = true;
                btn_items.IsEnabled = true;
                btn_clear.IsEnabled = false;
                btn_addVendor.IsEnabled = false;
                tb_taxValue.IsEnabled = true;

                tb_shippingCost.IsEnabled = true;

                bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                md_invoiceArchive.Visibility = Visibility.Collapsed;
            }
            else if (_InvoiceType == "pw" || _InvoiceType == "p" || _InvoiceType == "pb" || archived)
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                dg_billDetails.Columns[5].IsReadOnly = true; //make price read only
                dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                cb_vendor.IsEnabled = false;
                dp_desrvedDate.IsEnabled = false;
                dp_invoiceDate.IsEnabled = false;
                tb_note.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                cb_branch.IsEnabled = false;
                tb_discount.IsEnabled = false;
                cb_typeDiscount.IsEnabled = false;
                btn_save.IsEnabled = false;
                tb_invoiceNumber.IsEnabled = false;
                tb_taxValue.IsEnabled = false;
                btn_items.IsEnabled = false;
                btn_clear.IsEnabled = false;
                btn_addVendor.IsEnabled = false;
                tb_shippingCost.IsEnabled = false;

                if (_InvoiceType.Equals("pb"))
                {
                    bdr_invoiceArchive.Visibility = Visibility.Visible;
                    md_invoiceArchive.Visibility = Visibility.Visible;
                }
                else
                {
                    if (invoice.ChildInvoice != null)
                    {
                        bdr_invoiceArchive.Visibility = Visibility.Visible;
                        md_invoiceArchive.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        bdr_invoiceArchive.Visibility = Visibility.Collapsed;
                        md_invoiceArchive.Visibility = Visibility.Collapsed;
                    }
                }


            }

            if (_InvoiceType.Equals("pb") || _InvoiceType.Equals("p"))
            {
                #region print - pdf - send email
                btn_printInvoice.Visibility = Visibility.Visible;
                btn_pdf.Visibility = Visibility.Visible;
                //if (MainWindow.groupObject.HasPermissionAction(printCountPermission, MainWindow.groupObjects, "one"))
                //{
                    btn_printCount.Visibility = Visibility.Visible;
                    bdr_printCount.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    btn_printCount.Visibility = Visibility.Collapsed;
                //    bdr_printCount.Visibility = Visibility.Collapsed;
                //}
                //if (MainWindow.groupObject.HasPermissionAction(sendEmailPermission, MainWindow.groupObjects, "one"))
                //{
                    btn_emailMessage.Visibility = Visibility.Visible;
                    bdr_emailMessage.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    btn_emailMessage.Visibility = Visibility.Collapsed;
                //    bdr_emailMessage.Visibility = Visibility.Collapsed;
                //}
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

            if ((_InvoiceType != "pd" && invoice.taxValue == 0) || _InvoiceType == "pbd")
            {
                sp_tax.Visibility = Visibility.Collapsed;
                tb_taxValue.Text = "0";
            }
            else if (AppSettings.invoiceTax_bool == true || invoice.taxValue > 0)
                sp_tax.Visibility = Visibility.Visible;
        }
        private void Btn_invoiceImage_Click(object sender, RoutedEventArgs e)
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
                    refreshDocCount(invoice.invoiceId);
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
                        invType = invoice.invType,
                    });
                }
                tb_barcode.Focus();

                refrishBillDetails();
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


        private void Cb_branch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && (cb_branch.SelectedValue != null && cb_branch.SelectedValue.ToString() != "0"))
                {
                    _SelectedBranch = (int)cb_branch.SelectedValue;
                }
                else
                {
                    cb_branch.SelectedValue = _SelectedBranch;
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_vendor_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                //cb_vendor.ItemsSource = vendorsL.Where(x => x.name.Contains(cb_vendor.Text));
                var tb = cb_vendor.Template.FindName("PART_EditableTextBox", cb_vendor) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                cb_vendor.ItemsSource = vendorsL.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_vendor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_vendor.IsFocused || _InvoiceType == "pd")
                {
                    TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                    if (elapsed.TotalMilliseconds > 100 && (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0"))
                    {
                        _SelectedVendor = (int)cb_vendor.SelectedValue;
                        if (_InvoiceType == "pd")
                            btn_updateVendor.IsEnabled = true;
                        //var v = vendorsL.Where(x => x.agentId == _SelectedVendor).FirstOrDefault();
                        //if (v.payType != null)
                        //{
                        //    //cb_paymentProcessType.SelectedValue = v.payType;
                        //    //Animations.shakingControl(cb_paymentProcessType);
                        //    Animations.shakingControl(txt_payment);
                        //}
                        ////else
                        ////    cb_paymentProcessType.SelectedIndex = 0;
                        var v = vendorsL.Where(x => x.agentId == _SelectedVendor).FirstOrDefault();
                        try
                        {
                            if (SectionData.defaultPayTypeList is null)
                                SectionData.RefreshDefaultPayType();
                            txt_payType.Text = SectionData.defaultPayTypeList.Where(x => x.key == v.payType).FirstOrDefault().value;
                        }
                        catch
                        {
                            txt_payType.Text = "-";
                        }
                    }
                    else
                    {
                        cb_vendor.SelectedValue = _SelectedVendor;
                    }
                }

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
        private void tb_discount_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                var txb = sender as TextBox;
                if (txb.IsFocused)
                {
                    if ((sender as TextBox).Name == "tb_discount")
                        SectionData.InputJustNumber(ref txb);
                    if ((sender as TextBox).Name == "tb_taxValue")
                        SectionData.InputJustNumber(ref txb);
                    _Sender = sender;
                    refreshTotalValue();
                    e.Handled = true;
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        decimal _TaxValue = 0;
        private void refreshTotalValue()
        {
            #region sum
            if (_Sum < 0)
                _Sum = 0;

            if (_Sum != 0)
                tb_sum.Text = SectionData.DecTostring(_Sum);
            else
                tb_sum.Text = "0";
            #endregion
            decimal discountValue = 0;

            #region calculate discount
            if (_Sum > 0)
            {
                if (tb_discount.Text != "." && !tb_discount.Text.Equals(""))
                    discountValue = decimal.Parse(tb_discount.Text);

                if (cb_typeDiscount.SelectedIndex != -1 && int.Parse(cb_typeDiscount.SelectedValue.ToString()) == 2) // discount type is rate
                {
                    discountValue = SectionData.calcPercentage(_Sum, discountValue);
                }

                tb_totalDescount.Text = SectionData.PercentageDecTostring(discountValue);
            }
            #endregion
            decimal total = _Sum - discountValue;
            #region tax
            _TaxValue = 0;
           // decimal taxInputVal = 0;
            if (_InvoiceType != "pbd" && _InvoiceType != "pbw" && _InvoiceType != "pb")
            { 
                if (invoice != null && invoice.invType != null && invoice.invType.Equals("p") )
                {
                    if (invoice.tax != null)
                        _TaxValue = (decimal)invoice.taxValue;
                }
                else
                {
                    //if (!tb_taxValue.Text.Equals(""))
                    //    taxInputVal = decimal.Parse(tb_taxValue.Text);
                    if(tb_taxValue.Text != "")
                        _TaxValue = decimal.Parse(tb_taxValue.Text);
                }
                //if (total != 0)
                   // _TaxValue = SectionData.calcPercentage(total, taxInputVal);

                if (_TaxValue != 0)
                    tb_taxValue.Text = SectionData.DecTostring(_TaxValue);
                else
                    tb_taxValue.Text = "0";
            }
            #endregion

            decimal shippingCost = 0;
            #region shipping cost
            if (tb_shippingCost.Text != "")
                shippingCost = decimal.Parse(tb_shippingCost.Text);
            #endregion
            total = total + _TaxValue +shippingCost;
            if (total != 0)
                tb_total.Text = SectionData.DecTostring(total);
            else tb_total.Text = "0";
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
            //tb_sum.Text = _Sum.ToString();
            if (_Sum != 0)
                tb_sum.Text = SectionData.DecTostring(_Sum);
            else
                tb_sum.Text = "0";

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
                if (elapsed.TotalMilliseconds > 150)
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

                if (e.Key.ToString() == "Return" && _BarcodeStr != "")
                {
                    if (_Sender != null)
                    {
                        DatePicker dt = _Sender as DatePicker;
                        TextBox tb = _Sender as TextBox;
                        if (dt != null)
                        {
                            if (dt.Name == "dp_desrvedDate" || dt.Name == "dp_invoiceDate")
                            {
                                string br = "";
                                for (int i = 0; i < _BarcodeStr.Length; i++)
                                {
                                    br += _BarcodeStr[i];
                                    i++;
                                }
                                _BarcodeStr = br;
                                //_BarcodeStr = _BarcodeStr.Substring(1);
                            }
                        }
                        else if (tb != null)
                        {
                            if (tb.Name == "tb_invoiceNumber" || tb.Name == "tb_note" || tb.Name == "tb_discount")// remove barcode from text box
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
                    await dealWithBarcode(_BarcodeStr);
                    tb_barcode.Text = _BarcodeStr;
                    _BarcodeStr = "";
                    _IsFocused = false;
                    e.Handled = true;
                    cb_branch.SelectedValue = _SelectedBranch;
                }
                _Sender = null;
                if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                {
                    switch (e.Key)
                    {
                        case Key.P:
                            //handle P key
                            btn_printInvoice_Click(null, null);
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
                case "pi":// this barcode for invoice               
                    Btn_newDraft_Click(null, null);
                    invoice = await invoiceModel.GetInvoicesByNum(barcode);
                    _InvoiceType = invoice.invType;
                    if (_InvoiceType.Equals("pd") || _InvoiceType.Equals("p") || _InvoiceType.Equals("pbd") || _InvoiceType.Equals("pb"))
                    {
                        // set title to bill
                        if (_InvoiceType == "pd")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftPurchaseBill");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                        }
                        else if (_InvoiceType == "p")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseBill");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                        }
                        else if (_InvoiceType == "pbd")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftBounceBill");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                        }
                        else if (_InvoiceType == "pb")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                        }

                        await fillInvoiceInputs(invoice);
                    }
                    break;

                default: // if barcode for item
                         // get item matches barcode
                    if (MainWindow.InvoiceGlobalItemUnitsList != null)
                    {
                        //ItemUnit unit1 = barcodesList.ToList().Find(c => c.barcode == barcode.Trim());
                        ItemUnit unit1 = MainWindow.InvoiceGlobalItemUnitsList.ToList().Find(c => c.barcode == barcode.Trim());

                        // get item matches the barcode
                        if (unit1 != null)
                        {
                            int itemId = (int)unit1.itemId;
                            if (unit1.itemId != 0)
                            {
                                int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == unit1.itemUnitId && p.OrderId == 0).FirstOrDefault());

                                if (index == -1)//item doesn't exist in bill
                                {
                                    // get item units
                                    itemUnits = await itemUnitModel.GetItemUnits(itemId);
                                    //get item from list
                                    item = items.ToList().Find(i => i.itemId == itemId);

                                    int count = 1;
                                    decimal price = (decimal)unit1.cost; //?????
                                    decimal total = count * price;

                                    bool valid = true;
                                    if (item.type == "sn")
                                        valid = false;

                                    addRowToBill( unit1.mainUnit, unit1.itemUnitId, count, price, total,valid,item.type,item.packageItems);
                                }
                                else // item exist prevoiusly in list
                                {
                                    billDetails[index].Count++;
                                    billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;

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
        private async void Tb_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (e.Key == Key.Return)
                {
                    string barcode = "";
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

        private void addRowToBill( string unitName, int itemUnitId, int count,
                                decimal price, decimal total,bool valid, string type, List<Item> packageItems)
        {
            // increase sequence for each read
            _SequenceNum++;

            billDetails.Add(new BillDetails()
            {
                ID = _SequenceNum,
                Product = item.name,
                itemId = item.itemId,
                Unit = unitName,
                itemUnitId = itemUnitId,
                basicItemUnitId = itemUnitId,
                Count = 1,
                Price = price,
                Total = total,
                invType = invoice.invType,
                valid = valid,
                type = type,
                packageItems = packageItems,
                ItemProperties = item.ItemProperties
            }); ;
            _Sum += total;
        }
        #endregion billdetails

        private void Cbm_unitItemDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                //billDetails
                var cmb = sender as ComboBox;
                cmb.SelectedValue = (int)billDetails[0].itemUnitId;
                #region enable - disable unit comboBox according to invoice type
                if (_InvoiceType == "p" || _InvoiceType == "pw" || _InvoiceType == "pb" || _InvoiceType == "pbw")
                    cmb.IsEnabled = false;
                else
                    cmb.IsEnabled = true;
                #endregion

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Cbm_unitItemDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cmb = sender as ComboBox;

                if (dg_billDetails.SelectedIndex != -1 && cmb.SelectedValue != null)
                {
                    billDetails[dg_billDetails.SelectedIndex].itemUnitId = (int)cmb.SelectedValue;

                    TextBlock tb;

                    int _datagridSelectedIndex = dg_billDetails.SelectedIndex;
                    int itemUnitId = (int)cmb.SelectedValue;
                    int itemId = billDetails[_datagridSelectedIndex].itemId;
                    var iu = (ItemUnit)cmb.SelectedItem;

                    billDetails[_datagridSelectedIndex].Unit = iu.mainUnit;
                    billDetails[_datagridSelectedIndex].itemUnitId = (int)cmb.SelectedValue;
                    #region Dina

                    dynamic unit;
                    //if (MainWindow.InvoiceGlobalItemUnitsList.Count == 0)
                    //{
                    //    unit = new Item();
                    //    unit = barcodesList.ToList().Find(x => x.itemUnitId == (int)cmb.SelectedValue && x.itemId == billDetails[_datagridSelectedIndex].itemId);
                    //}
                    //else
                    {
                        unit = new ItemUnit();
                        unit = MainWindow.InvoiceGlobalItemUnitsList.Find(x => x.itemUnitId == (int)cmb.SelectedValue && x.itemId == billDetails[_datagridSelectedIndex].itemId);
                    }

                    int oldCount = 0;
                    long newCount = 0;
                    decimal oldPrice = 0;
                    decimal itemTax = 0;

                    decimal newPrice = 0;
                    oldCount = billDetails[_datagridSelectedIndex].Count;
                    oldPrice = billDetails[_datagridSelectedIndex].Price;
                    newCount = oldCount;
                    #region if return invoice
                    if (_InvoiceType == "pbd" || _InvoiceType == "pbw")
                    {
                        tb = dg_billDetails.Columns[4].GetCellContent(dg_billDetails.Items[_datagridSelectedIndex]) as TextBlock;

                        //var itemUnitsIds = barcodesList.Where(x => x.itemId == itemId).Select(x => x.itemUnitId).ToList();
                        var itemUnitsIds = MainWindow.InvoiceGlobalItemUnitsList.Where(x => x.itemId == itemId).Select(x => x.itemUnitId).ToList();

                        #region caculate available amount in this bil
                        int availableAmountInBranch = (int)await itemLocationModel.getAmountInBranch(itemUnitId, MainWindow.branchID.Value);
                        int amountInBill = (int)await getAmountInBill(itemId, itemUnitId, MainWindow.branchID.Value, billDetails[_datagridSelectedIndex].ID);
                        int availableAmount = availableAmountInBranch - amountInBill;
                        #endregion
                        #region calculate amount in purchase invoice
                        var mainItems = mainInvoiceItems.ToList().Where(i => itemUnitsIds.Contains((int)i.itemUnitId));
                        int purchasedAmount = 0;
                        foreach (ItemTransfer it in mainItems)
                        {
                            if (itemUnitId == (int)it.itemUnitId)
                                purchasedAmount += (int)it.quantity;
                            else
                                purchasedAmount += (int)await itemUnitModel.fromUnitToUnitQuantity((int)it.quantity, itemId, (int)it.itemUnitId, itemUnitId);
                        }
                        #endregion
                        if (newCount > (purchasedAmount - amountInBill) || newCount > availableAmount)
                        {
                            // return old value 
                            tb.Text = (purchasedAmount - amountInBill) > availableAmount ? availableAmount.ToString() : (purchasedAmount - amountInBill).ToString();

                            newCount = (purchasedAmount - amountInBill) > availableAmount ? availableAmount : (purchasedAmount - amountInBill);
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                    #endregion


                    newPrice = unit.cost;

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
                    
                    
                    #region update row valid

                    if (_InvoiceType.Equals("pbd") && !billDetails[_datagridSelectedIndex].itemUnitId.Equals(billDetails[_datagridSelectedIndex].basicItemUnitId))
                    {
                        billDetails[_datagridSelectedIndex].valid = true;
                        billDetails[_datagridSelectedIndex].returnedSerials = null;
                        billDetails[_datagridSelectedIndex].ReturnedProperties = null;
                    }
                    else
                    {
                        var hasSerial = false;
                        if (billDetails[dg_billDetails.SelectedIndex].type == "sn")
                            hasSerial = true;
                        else if (billDetails[dg_billDetails.SelectedIndex].type.Equals("p"))
                        {
                            long packageCount = (long)billDetails[dg_billDetails.SelectedIndex].Count;

                            foreach (var p in billDetails[dg_billDetails.SelectedIndex].packageItems)
                            {
                                if (p.type.Equals("sn"))
                                {
                                    hasSerial = true;
                                    break;
                                }
                            }

                        }

                        billDetails[_datagridSelectedIndex].valid = !hasSerial;
                        if (_InvoiceType.Equals("pbd"))
                        {
                            billDetails[_datagridSelectedIndex].returnedSerials = null;
                            billDetails[_datagridSelectedIndex].ReturnedProperties = null;
                        }
                        else if (_InvoiceType.Equals("pd"))
                        {
                            billDetails[dg_billDetails.SelectedIndex].itemSerials = new List<Serial>();
                            billDetails[dg_billDetails.SelectedIndex].StoreProperties = new List<StoreProperty>();
                        }
                    }
                    #endregion

                    #region update unit properties
                    billDetails[dg_billDetails.SelectedIndex].ItemProperties = unit.ItemProperties;
                    #endregion
                    refrishBillDetails();
                    #endregion
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                //return;
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
                                //var combo = (combo)cell.Content;
                                combo.SelectedValue = (int)item.itemUnitId;

                                //if (item.invType == "p" || item.invType == "pw" || item.invType == "pb" || item.invType == "pbw")
                                //    combo.IsEnabled = false;
                                //else
                                //    combo.IsEnabled = true;
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
            //if (dg_billDetails.SelectedIndex != -1 )
            //    if (billDetails[dg_billDetails.SelectedIndex].invType == "p" || billDetails[dg_billDetails.SelectedIndex].invType == "pw"
            //       || billDetails[dg_billDetails.SelectedIndex].invType == "pb" || billDetails[dg_billDetails.SelectedIndex].invType == "pbw")
            //        e.Cancel = true;

            int column = dg_billDetails.CurrentCell.Column.DisplayIndex;
            if (_InvoiceType == "p" || _InvoiceType == "pw"
                || _InvoiceType == "pb" || _InvoiceType == "pbw"
                || (_InvoiceType == "pbd" && column == 3))
                e.Cancel = true;
        }


        private async void Dg_billDetails_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {

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
                        t.Text = SectionData.DecTostring(billDetails[index].Price);

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
                        if (row.type == "sn")
                            billDetails[index].valid = false;

                        if (!t.Text.Equals(""))
                            newCount = int.Parse(t.Text);
                        else
                            newCount = 0;
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
                    #region if return invoice
                    if (_InvoiceType == "pbd" || _InvoiceType == "pbw")
                    {
                        var selectedItemUnitId = row.itemUnitId;

                        //var itemUnitsIds = barcodesList.Where(x => x.itemId == row.itemId).Select(x => x.itemUnitId).ToList();
                        var itemUnitsIds = MainWindow.InvoiceGlobalItemUnitsList.Where(x => x.itemId == row.itemId).Select(x => x.itemUnitId).ToList();

                        #region caculate available amount in this bil
                        int availableAmountInBranch = (int)await itemLocationModel.getAmountInBranch(row.itemUnitId, MainWindow.branchID.Value);
                        int amountInBill = (int)await getAmountInBill(row.itemId, row.itemUnitId, MainWindow.branchID.Value, row.ID);
                        int availableAmount = availableAmountInBranch - amountInBill;
                        #endregion
                        #region calculate amount in purchase invoice
                        var mainItems = mainInvoiceItems.ToList().Where(i => itemUnitsIds.Contains((int)i.itemUnitId));
                        int purchasedAmount = 0;
                        foreach (ItemTransfer it in mainItems)
                        {
                            if (selectedItemUnitId == (int)it.itemUnitId)
                                purchasedAmount += (int)it.quantity;
                            else
                                purchasedAmount += (int)await itemUnitModel.fromUnitToUnitQuantity((int)it.quantity, row.itemId, (int)it.itemUnitId, selectedItemUnitId);
                        }
                        #endregion
                        if (newCount > (purchasedAmount - amountInBill) || newCount > availableAmount)
                        {
                            // return old value 
                            t.Text = (purchasedAmount - amountInBill) > availableAmount ? availableAmount.ToString() : (purchasedAmount - amountInBill).ToString();

                            newCount = (purchasedAmount - amountInBill) > availableAmount ? availableAmount : (purchasedAmount - amountInBill);
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                    #endregion

                    if (columnName == MainWindow.resourcemanager.GetString("trPrice") && !t.Text.Equals(""))
                        newPrice = decimal.Parse(t.Text);
                    else
                        newPrice = row.Price;

                    // old total for changed item
                    decimal total = oldPrice * oldCount;
                    _Sum -= total;

                    // new total for changed item
                    total = newCount * newPrice;
                    _Sum += total;

                    //refresh total cell
                    TextBlock tb = dg_billDetails.Columns[6].GetCellContent(dg_billDetails.Items[index]) as TextBlock;
                    tb.Text = SectionData.DecTostring(total);

                    //  refresh sum and total text box
                    refreshTotalValue();

                    // update item in billdetails           
                    billDetails[index].Count = (int)newCount;
                    billDetails[index].Price = newPrice;
                    billDetails[index].Total = total;
                }
                refrishBillDetails();

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async Task<decimal> getAmountInBill(int itemId, int itemUnitId, int branchId, int ID)
        {
            int quantity = 0;
            //var itemUnits = barcodesList.Where(a => a.itemId == itemId).ToList();
            var itemUnits = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemId == itemId).ToList();

            var smallUnits = await itemUnitModel.getSmallItemUnits(itemId, itemUnitId);
            foreach (ItemUnit u in itemUnits)
            {
                var isInBill = billDetails.ToList().Find(x => x.itemUnitId == (int)u.itemUnitId && x.ID != ID); // unit exist in invoice
                if (isInBill != null)
                {
                    var isSmall = smallUnits.Find(x => x.itemUnitId == (int)u.itemUnitId);
                    int unitValue = 0;

                    int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == u.itemUnitId).FirstOrDefault());
                    int count = billDetails[index].Count;
                    if (itemUnitId == u.itemUnitId)
                    {
                        quantity += count;
                    }
                    else if (isSmall != null) // from-unit is bigger than to-unit
                    {
                        unitValue = itemUnitModel.getUnitConversionQuan(itemUnitId,(int)u.itemUnitId,itemUnits);
                        //unitValue = (int)await itemUnitModel.largeToSmallUnitQuan(itemUnitId, (int)u.itemUnitId);
                        quantity += count / unitValue;
                    }
                    else
                    {
                        unitValue = itemUnitModel.getLargeUnitConversionQuan(itemUnitId, (int)u.itemUnitId, itemUnits);
                       // unitValue = (int)await itemUnitModel.smallToLargeUnit(itemUnitId, (int)u.itemUnitId);

                        if (unitValue != 0)
                        {
                            quantity += count * unitValue;
                        }
                    }

                }
            }
            return quantity;
        }

        private void Dp_date_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                _Sender = sender;
                moveControlToBarcode(sender, e);
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
        private void moveControlToBarcode(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                DatePicker dt = sender as DatePicker;
                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds < 100)
                {
                    tb_barcode.Focus();
                    HandleKeyPress(sender, e);
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


        //print
        #region print-email
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    ////////////////////////////
                    Thread t1 = new Thread(() =>
                    {
                        pdfPurInvoice(invoice, invoiceItems);
                    });
                    t1.Start();
                    ////////////////////////////
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

        public void multiplePaytable(List<ReportParameter> paramarr, Invoice prInvoice, List<PayedInvclass> payedList)
        {
           // if ((prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb"))
            {
                CashTransfer cachModel = new CashTransfer();
                // List<PayedInvclass> payedList = new List<PayedInvclass>();
                // payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                //   payedList = prInvoice.cachTrans;
                mailpayedList = payedList;

                decimal sump = payedList.Sum(x => x.cash).Value;
                decimal deservd = (decimal)prInvoice.totalNet - sump;
                prInvoice.deserved = deservd;
                clsReports clsrep = new clsReports();
                List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                //convertter
                foreach (var p in payedList)
                {
                   // p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    if (p.processType == "cash")
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                    }
                    else
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    }
                }
                paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


            }
        }
        public void multiplePaytable(List<ReportParameter> paramarr, Invoice prInvoice)
        {
           // if ((prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb"))
            {
                CashTransfer cachModel = new CashTransfer();
                List<PayedInvclass> payedList = new List<PayedInvclass>();
                // payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                payedList = prInvoice.cachTrans;
                 payedList = payedList == null ? new List<PayedInvclass>() : payedList;//
                mailpayedList = payedList;
                decimal sump = payedList.Sum(x => x.cash).Value;
                decimal deservd = (decimal)prInvoice.totalNet - sump;
                prInvoice.deserved = deservd;
                clsReports clsrep = new clsReports();
                List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                //convertter
                foreach (var p in payedList)
                {
                  //  p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    if (p.processType == "cash")
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                    }
                    else
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    }
                }
                paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


            }
        }


        public async void pdfPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            try
            {
                if (prInvoice.invoiceId > 0)
                {
                    //  prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);

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
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {

                            List<ReportParameter> paramarr = new List<ReportParameter>();

                            //string reppath = reportclass.GetpayInvoiceRdlcpath(prInvoice);
                            //rep.ReportPath = reppath;
                            if (prInvoice.invoiceId > 0)
                            {
                                //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                                if (prInvoice.agentId != null)
                                {
                                    Agent agentinv = new Agent();
                                    //  agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                                    // agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                                    if (FillCombo.agentsList is null)
                                    { await FillCombo.RefreshAgents(); }
                                    agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                    prInvoice.agentCode = agentinv.code;
                                    //new lines
                                    prInvoice.agentName = agentinv.name;
                                    prInvoice.agentCompany = agentinv.company;
                                    prInvoice.agentMobile = agentinv.mobile;
                                }
                                else
                                {

                                    prInvoice.agentCode = "-";
                                    //new lines
                                    prInvoice.agentName = "-";
                                    prInvoice.agentCompany = "-";
                                    prInvoice.agentMobile = "-";
                                }
                                //user
                                User employ = new User();
                                // employ = await employ.getUserById((int)prInvoice.updateUserId);
                                if (FillCombo.usersAllList is null)
                                { await FillCombo.RefreshAllUsers(); }
                                employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                                prInvoice.uuserName = employ.name;
                                prInvoice.uuserLast = employ.lastname;

                                //branch
                                Branch branch = new Branch();
                                //  branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                                if (FillCombo.branchsAllList is null)
                                { await FillCombo.RefreshBranchsAll(); }
                                branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                                if (branch.branchId > 0)
                                {
                                    prInvoice.branchCreatorName = branch.name;
                                }
                                //branch reciver
                                if (prInvoice.branchId != null)
                                {
                                    if (prInvoice.branchId > 0)
                                    {
                                        // branch = await branchModel.getBranchById((int)prInvoice.branchId);
                                        if (FillCombo.branchsAllList is null)
                                        { await FillCombo.RefreshBranchsAll(); }
                                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

                                        prInvoice.branchName = branch.name;
                                    }
                                    else
                                    {
                                        prInvoice.branchName = "-";
                                    }

                                }
                                else
                                {
                                    prInvoice.branchName = "-";
                                }
                                // end branch reciever


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
                                //printer
                                clsReports clsrep = new clsReports();
                                reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                                repsize.paperSize = repsset.paperSize;
                                repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount, repsize.paperSize);
                                repsize.printerName = repsset.printerName;
                                //end     


                                //repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount);
                                string reppath = repsize.reppath;
                                rep.ReportPath = reppath;
                              // clsReports.purchaseInvoiceReport(invoiceItems, rep, reppath);
                                clsReports.setInvoiceLanguage(paramarr);
                                clsReports.InvoiceHeader(paramarr);
                                multiplePaytable(paramarr, prInvoice);
                                paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);

                             

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
                                    if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb"|| prInvoice.invType == "pbw")
                                    {

                                        paramarr.Add(new ReportParameter("isOrginal", false.ToString()));



                                        rep.SetParameters(paramarr);

                                        rep.Refresh();

                                        if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
                                        {

                                            this.Dispatcher.Invoke(() =>
                                            {
                                             //   LocalReportExtensions.ExportToPDF(rep, filepath);
                                                if (repsize.paperSize != "A4")
                                                {
                                                    LocalReportExtensions.customExportToPDF(rep, filepath, repsize.width, repsize.height);
                                                }
                                                else
                                                {
                                                    LocalReportExtensions.ExportToPDF(rep, filepath);
                                                }

                                            });


                                            int res = 0;

                                            res = (int)await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);



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

                                           // LocalReportExtensions.ExportToPDF(rep, filepath);
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

                                    /*
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        saveFileDialog.Filter = "PDF|*.pdf;";

                                        if (saveFileDialog.ShowDialog() == true)
                                        {

                                            string filepath = saveFileDialog.FileName;
                                            LocalReportExtensions.ExportToPDF(rep, filepath);
                                        }
                                    });


                                    */
                                }
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
        public async void previewPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            try
            {
                if (prInvoice.invoiceId > 0)
                {

                    //  prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);

                    if (int.Parse(AppSettings.Allow_print_inv_count) <= prInvoice.printedcount)
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);

                    }
                    else
                    {
                        Window.GetWindow(this).Opacity = 0.2;

                        List<ReportParameter> paramarr = new List<ReportParameter>();
                        string pdfpath;

                        //
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        if (prInvoice.invoiceId > 0)
                        {
                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            if (prInvoice.agentId != null)
                            {
                                Agent agentinv = new Agent();
                                //   agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                                if (FillCombo.agentsList is null)
                                { await FillCombo.RefreshAgents(); }
                                agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                // agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                prInvoice.agentCode = agentinv.code;
                                //new lines
                                prInvoice.agentName = agentinv.name;
                                prInvoice.agentCompany = agentinv.company;
                                prInvoice.agentMobile = agentinv.mobile;
                            }
                            else
                            {

                                prInvoice.agentCode = "-";
                                //new lines
                                prInvoice.agentName = "-";
                                prInvoice.agentCompany = "-";
                                prInvoice.agentMobile = "-";
                            }

                            //invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            Branch branch = new Branch();
                            //   branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                            if (FillCombo.branchsAllList is null)
                            { await FillCombo.RefreshBranchsAll(); }
                            branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                            if (branch.branchId > 0)
                            {
                                prInvoice.branchCreatorName = branch.name;
                            }

                            //branch reciver
                            if (prInvoice.branchId != null)
                            {
                                if (prInvoice.branchId > 0)
                                {
                                    //  branch = await branchModel.getBranchById((int)prInvoice.branchId);
                                    if (FillCombo.branchsAllList is null)
                                    { await FillCombo.RefreshBranchsAll(); }
                                    branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

                                    prInvoice.branchName = branch.name;
                                }
                                else
                                {
                                    prInvoice.branchName = "-";
                                }

                            }
                            else
                            {
                                prInvoice.branchName = "-";
                            }
                            // end branch reciever

                            //user
                            User employ = new User();
                            //  employ = await employ.getUserById((int)prInvoice.updateUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                            prInvoice.uuserName = employ.name;
                            prInvoice.uuserLast = employ.lastname;

                            ReportCls.checkInvLang();
                            
                            foreach (var i in invoiceItems)
                            {
                                i.price = decimal.Parse(SectionData.DecTostring(i.price));
                                //r.deserveDate = Convert.ToDateTime(SectionData.DateToString(r.deserveDate));
                                i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                            }
                            int itemscount = 0;
                            reportSize repsize = new reportSize();
                            clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            itemscount = invoiceItems.Count();
                            //repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount);
                            //printer
                            clsReports clsrep = new clsReports();
                            reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                            repsize.paperSize = repsset.paperSize;
                            repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount, repsize.paperSize);
                            repsize.printerName = repsset.printerName;
                            //end 

                            string reppath = repsize.reppath;
                            rep.ReportPath = reppath;
                            clsReports.setInvoiceLanguage(paramarr);
                            clsReports.InvoiceHeader(paramarr);
                            paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);
                         //   if ((prInvoice.invType == "p"|| prInvoice.invType == "pbw" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb" || prInvoice.invType == "pd"))
                            {
                                CashTransfer cachModel = new CashTransfer();
                                List<PayedInvclass> payedList = new List<PayedInvclass>();
                                // payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                                payedList = prInvoice.cachTrans;
                                payedList = payedList == null ? new List<PayedInvclass>() : payedList;//
                                decimal sump =  payedList.Sum(x => x.cash).Value ;
                                decimal deservd = (decimal)prInvoice.totalNet - sump;
                               
                             
                                List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                                //convertter
                                foreach (var p in payedList)
                                {
                                   // p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                    if (p.processType == "cash")
                                    {
                                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                                    }
                                    else
                                    {
                                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                    }
                                }
                                paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                                paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                                paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                            
                                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));
                            }

                            if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
          || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
          || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                            {
                                paramarr.Add(new ReportParameter("isOrginal", true.ToString()));
                            }
                            else
                            {
                                paramarr.Add(new ReportParameter("isOrginal", false.ToString()));
                            }
                            rep.SetParameters(paramarr);
                            rep.Refresh();
                            //copy count
                            if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbw")
                            {

                                // paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));

                                // update paramarr->isOrginal
                                foreach (var item in paramarr.Where(x => x.Name == "isOrginal").ToList())
                                {
                                    StringCollection myCol = new StringCollection();
                                    myCol.Add(prInvoice.isOrginal.ToString());
                                    item.Values = myCol;


                                }
                                //end update


                                rep.SetParameters(paramarr);

                                rep.Refresh();

                                if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
                                {
                                   // LocalReportExtensions.ExportToPDF(rep, pdfpath);
                                    if (repsize.paperSize != "A4")
                                    {
                                        LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                                    }
                                    else
                                    {
                                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                                    }
                                    int res = 0;

                                    res = (int)await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);



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
                            // end copy count

                            //   LocalReportExtensions.ExportToPDF(rep, pdfpath);




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
                    //
                }
                else
                {
                    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSaveInvoiceToPreview"), animation: ToasterAnimation.FadeIn);

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
        public async Task<string> printPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> payedList)
        {
            string msg = "";
            try
            {
                if (prInvoiceId > 0)
                {
                    //prInvoice = new Invoice();
                    //prInvoice = await invoiceModel.GetByInvoiceId(prInvoiceId);
                    ////
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


                            // agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                            //user
                            User employ = new User();
                            //  employ = await employ.getUserById((int)prInvoice.updateUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                            prInvoice.uuserName = employ.name;
                            prInvoice.uuserLast = employ.lastname;
                            //agent
                            if (prInvoice.agentId != null)
                            {
                                Agent agentinv = new Agent();
                                //  agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                                // agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                                if (FillCombo.agentsList is null)
                                { await FillCombo.RefreshAgents(); }
                                agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                prInvoice.agentCode = agentinv.code;
                                //new lines
                                prInvoice.agentName = agentinv.name;
                                prInvoice.agentCompany = agentinv.company;
                                prInvoice.agentMobile = agentinv.mobile;
                            }
                            else
                            {

                                prInvoice.agentCode = "-";
                                //new lines
                                prInvoice.agentName = "-";
                                prInvoice.agentCompany = "-";
                                prInvoice.agentMobile = "-";
                            }

                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            //branch
                            Branch branch = new Branch();
                            //branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                            if (FillCombo.branchsAllList is null)
                            { await FillCombo.RefreshBranchsAll(); }
                            branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                            if (branch.branchId > 0)
                            {
                                prInvoice.branchCreatorName = branch.name;
                            }
                            //branch reciver
                            if (prInvoice.branchId != null)
                            {
                                if (prInvoice.branchId > 0)
                                {
                                    if (FillCombo.branchsAllList is null)
                                    { await FillCombo.RefreshBranchsAll(); }
                                    branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

                                    prInvoice.branchName = branch.name;
                                }
                                else
                                {
                                    prInvoice.branchName = "-";
                                }

                            }
                            else
                            {
                                prInvoice.branchName = "-";
                            }
                            // end branch reciever

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
                            //printer
                            clsReports clsrep = new clsReports();
                            reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                            repsize.paperSize = repsset.paperSize;
                            repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount, repsize.paperSize);
                            repsize.printerName = repsset.printerName;
                            //end 

                            //repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount);
                            string reppath = repsize.reppath;
                            rep.ReportPath = reppath;
                            clsReports.setInvoiceLanguage(paramarr);
                             
                            clsReports.InvoiceHeader(paramarr);
                            payedList = payedList == null ? new List<PayedInvclass>() : payedList;//
                            multiplePaytable(paramarr, prInvoice, payedList);
                            paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);
                         
                            rep.SetParameters(paramarr);
                            rep.Refresh();


                            //copy count
                            if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb" || prInvoice.invType == "pw" || prInvoice.invType == "pbw")
                            {

                                paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));

                                for (int i = 1; i <= short.Parse(AppSettings.pur_copy_count); i++)
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
                                        //this.Dispatcher.Invoke(() =>
                                        //{
                                        //    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.sale_printer_name, 1);
                                        if (repsize.paperSize == "A4")
                                        {
                                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, 1);
                                        }
                                        else
                                        {
                                            LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, 1, repsize.width, repsize.height);
                                        }
                                        //});
                                        int res = 0;
                                        res = (int)await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);
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
                            

                                if (repsize.paperSize == "A4")
                                {
                                    
                                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, short.Parse(AppSettings.pur_copy_count));

                                }
                                else
                                {
                                    LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, short.Parse(AppSettings.pur_copy_count), repsize.width, repsize.height);

                                }
                                //});

                            }
                            // end copy count

                            /*
                            this.Dispatcher.Invoke(() =>
                            {
                                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, 1);
                            });
                            */
                        }
                    }

                    //
                }
            }
            catch
            {
                //this.Dispatcher.Invoke(() =>
                //{
                //    Toaster.ShowWarning(Window.GetWindow(this), message: "Not completed", animation: ToasterAnimation.FadeIn);

                //});
                msg = "trNotCompleted";

            }
            return msg;
        }

        public async Task<string> sendPurEmail(Invoice prInvoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> payedList)
        {
            string msg = "";
            try
            {
                //
                if (prInvoiceId > 0)
                {
                    //prInvoice = new Invoice();
                    //prInvoice = await invoiceModel.GetByInvoiceId(prInvoiceId);
                    decimal? discountval = 0;
                    string discounttype = "";
                    discountval = prInvoice.discountValue;
                    discounttype = prInvoice.discountType;

                    if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                    || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                    || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                    {
                        //Dispatcher.Invoke(new Action(() =>
                        //{
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCanNotSendDraftInvoice"), animation: ToasterAnimation.FadeIn);
                        //}));
                        msg = "trCanNotSendDraftInvoice";
                    }
                    else
                    {
                        if (prInvoice.invType == "pw" || prInvoice.invType == "p")
                        {
                            invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            SysEmails email = new SysEmails();
                            EmailClass mailtosend = new EmailClass();
                            email = await email.GetByBranchIdandSide((int)MainWindow.branchID, "purchase");
                            if (email == null)
                            {
                                //this.Dispatcher.Invoke(new Action(() =>
                                //{
                                //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoEmailForThisDept"), animation: ToasterAnimation.FadeIn);
                                //}));
                                msg = "trNoEmailForThisDept";
                            }
                            else
                            {
                                //agent
                                Agent toAgent = new Agent();

                                if (prInvoice.agentId != null)
                                {

                                    //  agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                    //  toAgent = await toAgent.getAgentById((int)prInvoice.agentId);
                                    if (FillCombo.agentsList is null)
                                    { await FillCombo.RefreshAgents(); }
                                    toAgent = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                                    prInvoice.agentCode = toAgent.code;
                                    //new lines
                                    prInvoice.agentName = toAgent.name;
                                    prInvoice.agentCompany = toAgent.company;
                                    prInvoice.agentMobile = toAgent.mobile;
                                }
                                else
                                {

                                    prInvoice.agentCode = "-";
                                    //new lines
                                    prInvoice.agentName = "-";
                                    prInvoice.agentCompany = "-";
                                    prInvoice.agentMobile = "-";
                                }
                                if (toAgent == null || toAgent.agentId == 0)
                                {
                                    //edit warning message to customer
                                    //Dispatcher.Invoke(new Action(() =>
                                    //{
                                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheVendorHasNoEmail"), animation: ToasterAnimation.FadeIn);
                                    //}));
                                    msg = "trTheVendorHasNoEmail";
                                }
                                else
                                {
                                    //  int? itemcount = invoiceItems.Count();
                                    if (email.emailId == 0)
                                    {
                                        //Dispatcher.Invoke(new Action(() =>
                                        //{
                                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoEmailForThisDept"), animation: ToasterAnimation.FadeIn);
                                        //}));
                                        msg = "trNoEmailForThisDept";
                                    }
                                    else
                                    {
                                        if (prInvoice.invoiceId == 0)
                                        {
                                            //Dispatcher.Invoke(new Action(() =>
                                            //{
                                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoOrderToSen"), animation: ToasterAnimation.FadeIn);
                                            //}));
                                            msg = "trThereIsNoOrderToSen";
                                        }

                                        else
                                        {
                                            if (invoiceItems == null || invoiceItems.Count() == 0)
                                            {
                                                //Dispatcher.Invoke(new Action(() =>
                                                //{
                                                //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoItemsToSend"), animation: ToasterAnimation.FadeIn);
                                                //}));
                                                msg = "trThereIsNoItemsToSend";
                                            }
                                            else
                                            {

                                                if (toAgent.email.Trim() == "" || toAgent.email.Trim() == null)
                                                {
                                                    //Dispatcher.Invoke(new Action(() =>
                                                    //{
                                                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheVendorHasNoEmail"), animation: ToasterAnimation.FadeIn);
                                                    //}));
                                                    msg = "trTheVendorHasNoEmail";
                                                }

                                                else
                                                {
                                                    SetValues setvmodel = new SetValues();


                                                    List<SetValues> setvlist = new List<SetValues>();
                                                    if (prInvoice.invType == "pw" || prInvoice.invType == "p")
                                                    {
                                                        setvlist = await setvmodel.GetBySetName("pur_email_temp");
                                                    }
                                                    else if (prInvoice.invType == "or" || prInvoice.invType == "ors")
                                                    {
                                                        setvlist = await setvmodel.GetBySetName("sale_order_email_temp");
                                                    }
                                                    foreach (var i in invoiceItems)
                                                    {
                                                        i.price = decimal.Parse(SectionData.DecTostring(i.price));
                                                    }
                                                    string pdfpath = await SavePurpdf(prInvoice, invoiceItems, payedList);
                                                    prInvoice.discountValue = discountval;
                                                    prInvoice.discountType = discounttype;
                                                    mailtosend = mailtosend.fillSaleTempData(prInvoice, invoiceItems, mailpayedList, email, toAgent, setvlist);

                                                    //SavePurpdf();
                                                    //string pdfpath = emailpdfpath;
                                                    mailtosend.AddAttachTolist(pdfpath);

                                                    string resmsg = "";
                                                    //this.Dispatcher.Invoke(new Action(() =>
                                                    //{
                                                    resmsg = mailtosend.Sendmail();// temp comment
                                                    if (resmsg == "Failure sending mail.")
                                                    {
                                                        // msg = "No Internet connection";

                                                        //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoConnection"), animation: ToasterAnimation.FadeIn);
                                                        msg = "trNoConnection";
                                                    }
                                                    else if (resmsg == "mailsent")
                                                    {
                                                        //Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailSent"), animation: ToasterAnimation.FadeIn);
                                                        msg = "trMailSent";
                                                    }
                                                    else
                                                    {
                                                        //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trMailNotSent"), animation: ToasterAnimation.FadeIn);
                                                        msg = "trMailNotSent";
                                                    }

                                                    //}));

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //  MessageBox.Show("only purshase invoice");
                            msg = "trCanNotSendDraftInvoice";
                        }
                    }
                    //
                }
                else
                {
                    //this.Dispatcher.Invoke(new Action(() =>
                    //{
                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoItemsToSend"), animation: ToasterAnimation.FadeIn);
                    //}));
                    msg = "trThereIsNoItemsToSend";
                }


                //
            }
            catch(Exception ex)
            {
                //this.Dispatcher.Invoke(() =>
                //{
                //    Toaster.ShowWarning(Window.GetWindow(this), message: "Not completed", animation: ToasterAnimation.FadeIn);

                //});
                msg = "trCannotSendEmail";
            }
            return msg;
        }
        //public async void SavePurpdf()

        public async Task<string> SavePurpdf(Invoice prInvoice, List<ItemTransfer> invoiceItems, List<PayedInvclass> payedList)
        {
            //email
            string pdfpath;
            pdfpath = @"\Thumb\report\File.pdf";
            try
            {


                if (prInvoiceId > 0)
                {
                    //prInvoice = new Invoice();
                    //prInvoice = await invoiceModel.GetByInvoiceId(prInvoiceId);

                    List<ReportParameter> paramarr = new List<ReportParameter>();

                    //

                    pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                 
                    if (prInvoice.invoiceId > 0)
                    {
                        //agent
                        //invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        if (prInvoice.agentId != null)
                        {
                            Agent agentinv = new Agent();
                            //  agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                            // agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                            if (FillCombo.agentsList is null)
                            { await FillCombo.RefreshAgents(); }
                            agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                            prInvoice.agentCode = agentinv.code;
                            //new lines
                            prInvoice.agentName = agentinv.name;
                            prInvoice.agentCompany = agentinv.company;
                            prInvoice.agentMobile = agentinv.mobile;
                        }
                        else
                        {

                            prInvoice.agentCode = "-";
                            //new lines
                            prInvoice.agentName = "-";
                            prInvoice.agentCompany = "-";
                            prInvoice.agentMobile = "-";
                        }

                        // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        //branch
                        Branch branch = new Branch();
                        if (FillCombo.branchsAllList is null)
                        { await FillCombo.RefreshBranchsAll(); }
                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                        if (branch.branchId > 0)
                        {
                            prInvoice.branchCreatorName = branch.name;
                        }
                        //branch reciver
                        if (prInvoice.branchId != null)
                        {
                            if (prInvoice.branchId > 0)
                            {
                                // branch = await branchModel.getBranchById((int)prInvoice.branchId);
                                if (FillCombo.branchsAllList is null)
                                { await FillCombo.RefreshBranchsAll(); }
                                branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

                                prInvoice.branchName = branch.name;
                            }
                            else
                            {
                                prInvoice.branchName = "-";
                            }

                        }
                        else
                        {
                            prInvoice.branchName = "-";
                        }
                        // end branch reciever
                        //user
                        User employ = new User();
                        //  employ = await employ.getUserById((int)prInvoice.updateUserId);
                        if (FillCombo.usersAllList is null)
                        { await FillCombo.RefreshAllUsers(); }
                        employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                        prInvoice.uuserName = employ.name;
                        prInvoice.uuserLast = employ.lastname;

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
                        //printer
                        clsReports clsrep = new clsReports();
                        reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                        repsize.paperSize = repsset.paperSize;
                        repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount, repsize.paperSize);
                        repsize.printerName = repsset.printerName;
                        //end 

                        //repsize = reportclass.GetpayInvoiceRdlcpath(prInvoice, itemscount);
                        string reppath = repsize.reppath;
                        rep.ReportPath = reppath;
                      
                        clsReports.setInvoiceLanguage(paramarr);
                        clsReports.InvoiceHeader(paramarr);
                        paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);


                      //  if ((prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb"))
                        {
                            CashTransfer cachModel = new CashTransfer();
                            // List<PayedInvclass> payedList = new List<PayedInvclass>();
                            //  payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                            //payedList = prInvoice.cachTrans;
                            payedList = payedList == null ? new List<PayedInvclass>() : payedList;//
                            mailpayedList = payedList;
                            decimal sump = payedList.Sum(x => x.cash).Value;
                            decimal deservd = (decimal)prInvoice.totalNet - sump;
                         
                            List<PayedInvclass> repPayedList = clsrep.cashPayedinvoice(payedList, prInvoice);
                            //convertter
                            foreach (var p in payedList)
                            {
                              //  p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                if (p.processType == "cash")
                                {
                                    p.cash = decimal.Parse(reportclass.DecTostring(p.cash + prInvoice.cashReturn));
                                }
                                else
                                {
                                    p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                }
                            }
                            paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                            paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                            paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                            rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));


                        }
                        rep.SetParameters(paramarr);
                        rep.Refresh();
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (repsize.paperSize != "A4")
                            {
                                LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                            }
                            else
                            {
                                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                            }
                          //  LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        }));
                    }
                }
                return pdfpath;
            }
            catch
            {
                this.Dispatcher.Invoke(() =>
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: "Not completed", animation: ToasterAnimation.FadeIn);

                });
                return pdfpath;
            }
        }
        private void btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    prInvoiceId = invoice.invoiceId;

                    ////////////////////////////
                    Thread t1 = new Thread(async() =>
                    {
                        string msg = "";
                        msg= await  printPurInvoice(invoice, invoiceItems, invoice.cachTrans);
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
                    ////////////////////////////
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
        //

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    /////////////////////
                    previewPurInvoice(invoice, invoiceItems);
                    /////////////////////
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
        private void Btn_emailMessage_Click(object sender, RoutedEventArgs e)
        {//email
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(paymentsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    prInvoiceId = invoice.invoiceId;
                    ///////////////////////////////////
                    Thread t1 = new Thread(async () =>
                    {
                        string msg = "";
                        msg = await sendPurEmail(invoice, invoiceItems, invoice.cachTrans);
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
                    ////////////////////////////////////
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

                int result = 0;

                if (invoice.invoiceId > 0)
                {
                    result = (int)await invoiceModel.updateprintstat(invoice.invoiceId, -1, true, true);


                    if (result > 0)
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                        prInvoice.isOrginal = true;
                        prInvoice.printedcount = prInvoice.printedcount - 1;
                        invoice.isOrginal = true;
                        invoice.printedcount = prInvoice.printedcount - 1;
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

        private void Btn_items_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //items

                Window.GetWindow(this).Opacity = 0.2;
                wd_items w = new wd_items();
                w.CardType = "purchase";
                w.items = items;
                w.branchId = (int)cb_branch.SelectedValue;
                w.ShowDialog();
                if (w.isActive)
                {
                    for (int i = 0; i < w.selectedItems.Count; i++)
                    {
                        int itemId = w.selectedItems[i];
                        ChangeItemIdEvent(itemId);
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

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                clearVendor();
                btn_updateVendor.IsEnabled = false;
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
        private async void clearVendor()
        {
            await RefrishVendors();
            cb_vendor.SelectedIndex = -1;
            cb_vendor.Text = "";
            dp_desrvedDate.SelectedDate = null;
            dp_desrvedDate.Text = "";
            tb_invoiceNumber.Text = "";
            dp_invoiceDate.SelectedDate = null;
            dp_invoiceDate.Text = "";
            tb_note.Text = "";
            invoice.agentId = 0;

        }

        private void Tb_barcode_TextChanged(object sender, TextChangedEventArgs e)
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
                        w.invType = invoice.invType;
                        w.invPaid = invoice.paid.Value;
                        w.invTotal = invoice.totalNet.Value;
                        w.sourceUserControls = FillCombo.UserControls.payInvoice;

                        w.title = MainWindow.resourcemanager.GetString("trPayments");
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

                #region title text
                if (_InvoiceType == "pw" || _InvoiceType == "p")
                {
                    if(invoice.isArchived)
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoiceArchived");
                    if (invoice.ChildInvoice != null)
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trpurInvoiceUpdated");
                    else
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");

                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                }
                else if (_InvoiceType == "pb" || _InvoiceType == "pbw")
                {
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                }
                else if (_InvoiceType == "pd")
                {
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftPurchaseBill");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                }
                else if (_InvoiceType == "pbd")
                {
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trDraftBounceBill");
                    txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                }

                #endregion
                if (_InvoiceType == "pw" || _InvoiceType == "p" || _InvoiceType == "pb" || _InvoiceType == "pbw")
                    refreshInvoiceNot(invoice.invoiceId);
            }
            catch 
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
                //clearInvoice();
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

        private async void Btn_shortageInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await saveBeforeTransfer();
                if (invoice.invoiceId != 0)
                    clearInvoice();

                _isLack = true;
                await buildShortageInvoiceDetails();
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
        private async Task buildShortageInvoiceDetails()
        {

            //get invoice items
            invoiceItems = await invoice.getShortageNoPackageItems(MainWindow.branchID.Value);
            mainInvoiceItems = invoiceItems;
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
                    //OrderId = (int)itemT.invoiceId,
                    Price = decimal.Parse(SectionData.DecTostring((decimal)itemT.price)),
                    Total = total,
                    invType = invoice.invType,
                });

                _Sum += total;
            }

            tb_barcode.Focus();
            refreshTotalValue();
            refrishBillDetails();
            dg_billDetails.Columns[5].IsReadOnly = false; //make price read only
            dg_billDetails.Columns[4].IsReadOnly = false; //make quantity read only
            dg_billDetails.Columns[3].IsReadOnly = false; //make quantity read only
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

        private void Dg_billDetails_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _IsFocused = true;
        }

        private async void Btn_invoiceArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Window.GetWindow(this).Opacity = 0.2;

                wd_invoice w = new wd_invoice();

                // archived purchase invoices
                string invoiceType = "p";
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
                        refreshDocCount(invoice.invoiceId);
                        refreshInvoiceNot(invoice.invoiceId);

                        #region set title to bill - show archive btn
                        if (invoice.invType == "p" || invoice.invType == "pw")
                        {

                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurInvoiceUpdated");
                            bdr_invoiceArchive.Visibility = Visibility.Visible;
                            md_invoiceArchive.Visibility = Visibility.Visible;
                            txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trReturns");
                           
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;

                        }
                        else
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                            txt_payInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;

                            bdr_invoiceArchive.Visibility = Visibility.Visible;
                            md_invoiceArchive.Visibility = Visibility.Visible;
                            txt_invoiceArchive.Text = MainWindow.resourcemanager.GetString("trInvoice");
                        }
                        btn_save.Content = MainWindow.resourcemanager.GetString("trPay");

                        #endregion

                        await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                        navigateBtnActivate();

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

        private void serialItemsRow(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                BillDetails row = (BillDetails)dg_billDetails.SelectedItems[0];
                int itemId = row.itemId;

                item = (Item)items.ToList().Where(i => i.itemId == itemId).FirstOrDefault().Clone();
                item.itemUnitId = row.itemUnitId;
                item.unitName = row.Unit;

                Window.GetWindow(this).Opacity = 0.2;
                wd_serialNum w = new wd_serialNum();
                w.sourceUserControls = FillCombo.UserControls.payInvoice;
                w.item = item;
                w.itemCount = row.Count;
                if (_InvoiceType.Equals("po"))
                    w.invType = "pd";
                else
                    w.invType = _InvoiceType;

                w.mainInvoiceId = (int)invoice.invoiceId;

                if (!_InvoiceType.Equals("pbd") || (_InvoiceType.Equals("pbd") && row.itemUnitId.Equals(row.basicItemUnitId)))
                {
                    w.itemsSerials = row.itemSerials;
                    w.returnedSerials = row.returnedSerials;
                    w.ItemProperties = row.ItemProperties;
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
                    row.valid = w.valid;
                    refrishBillDetails();

                }
                //else if (item.type == "sn")
                else 
                {
                    row.itemSerials = w.itemsSerials;
                    row.returnedSerials = w.returnedSerials;
                    row.StoreProperties = w.StoreProperties;
                    row.ReturnedProperties = w.ReturnedProperties;

                    #region valid icon
                    bool isValid = true;
                    if (_InvoiceType == "pd" || _InvoiceType == "pbd" || _InvoiceType == "po")
                    {
                        if ((item.type == "sn" && _InvoiceType.Equals("pbd") && row.returnedSerials.Count < row.Count)||
                            (invoice.invType.Equals("pd") || row.itemSerials.Count() < row.Count))
                            isValid = false;

                    }
                    #endregion
                    row.valid = isValid;
                    refrishBillDetails();
                }
                //else if (w.serialsSkip || w.serialsSave)
                //{
                //    row.itemSerials = w.itemsSerials;
                //    row.returnedSerials = w.returnedSerials;
                //    row.valid = true;
                //    refrishBillDetails();

                //}
                //else if (w.propertiesSkip || w.propertiesSave)
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
    }
}
