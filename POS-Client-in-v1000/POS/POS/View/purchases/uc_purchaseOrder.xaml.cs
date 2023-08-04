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
using System.Net.Mail;
using System.Windows.Threading;
using System.Threading;

namespace POS.View.purchases
{
    /// <summary>
    /// Interaction logic for uc_purchaseOrder.xaml
    /// </summary>
    public partial class uc_purchaseOrder : UserControl
    {
        private static uc_purchaseOrder _instance;
        public static uc_purchaseOrder Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_purchaseOrder();
                return _instance;
            }
        }
        public uc_purchaseOrder()
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


        string createPermission = "purchaseOrder_create";
        string reportsPermission = "purchaseOrder_reports";
        string sendEmailPermission = "purchaseOrder_sendEmail";
        string initializeShortagePermission = "purchaseOrder_initializeShortage";

        ObservableCollection<BillDetails> billDetails = new ObservableCollection<BillDetails>();

        Item itemModel = new Item();
        Item item = new Item();
        IEnumerable<Item> items;
        ItemLocation itemLocationModel = new ItemLocation();

        Branch branchModel = new Branch();
        //IEnumerable<Branch> branches;

        Agent agentModel = new Agent();
        IEnumerable<Agent> vendors;

        ItemUnit itemUnitModel = new ItemUnit();
        //List<ItemUnit> barcodesList;
        List<ItemUnit> itemUnits;

        Invoice invoiceModel = new Invoice();
        public Invoice invoice = new Invoice();
        List<Invoice> invoices;
        List<ItemTransfer> invoiceItems;
        List<ItemTransfer> mainInvoiceItems;
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        public List<Control> controls;
        #region for notifications
        private static DispatcherTimer timer;
        public static bool isFromReport = false;
        public static bool archived = false;
        int _DraftCount = 0;
        int _OrdersCount = 0;
        int _DocCount = 0;
        #endregion
        #region //to handle barcode characters
        static private int _SelectedVendor = -1;
        bool _IsFocused = false;
        // for barcode
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private object _Sender;
        #endregion      
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;

        //for bill details
        static private int _SequenceNum = 0;
        static private int _invoiceId;
        static private decimal _Sum = 0;
        static public string _InvoiceType = "pod"; // purchase order draft
        static private decimal _Count = 0;
        static public bool _isLack = false; // for lack invoice
        // for report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        Invoice prInvoice = new Invoice();
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

            txt_shortageInvoice.Text = MainWindow.resourcemanager.GetString("trLack");
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrder");
            txt_vendor.Text = MainWindow.resourcemanager.GetString("trVendor");
            tb_count.Text = MainWindow.resourcemanager.GetString("trCount:");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_vendor, MainWindow.resourcemanager.GetString("trVendorHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));

            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_drafts.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_newDraft.Text = MainWindow.resourcemanager.GetString("trNew");
            txt_purchaseOrder.Text = MainWindow.resourcemanager.GetString("trReady");
            txt_emailMessage.Text = MainWindow.resourcemanager.GetString("trSendEmail");
            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_pdf.Text = MainWindow.resourcemanager.GetString("trPdfBtn");
            txt_printInvoice.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_invoiceImages.Text = MainWindow.resourcemanager.GetString("trImages");
            txt_isApproved.Text = MainWindow.resourcemanager.GetString("trApprove");

            tt_error_previous.Content = MainWindow.resourcemanager.GetString("trPrevious");
            tt_error_next.Content = MainWindow.resourcemanager.GetString("trNext");

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
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
            }
        }
        private async Task<bool> saveBeforeExit()
        {
            bool succssess = false;
            if (billDetails.Count > 0 && _InvoiceType == "pod" && _isLack == false)
            {
                #region Accept
                MainWindow.mainWindow.Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trSaveOrderNotification");

                w.ShowDialog();
                MainWindow.mainWindow.Opacity = 1;
                #endregion
                if (w.isOk)
                {
                    bool valid = validateItemUnits();
                    if (billDetails.Count > 0 && valid)
                    {
                        await addInvoice(_InvoiceType);
                        succssess = true;
                    }
                }
                else
                {
                    clearInvoice();
                    //refreshDraftNotification();
                    setDraftNotification(AppSettings.PurchaseOrdersDraftCount);
                    succssess = true;
                }
            }
            else
            {
                clearInvoice();
                // refreshDraftNotification();
                setDraftNotification(AppSettings.PurchaseOrdersDraftCount);

                succssess = true;
            }
            return succssess;
        }
        private async void Btn_purchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                saveBeforeExit();
                wd_invoice w = new wd_invoice();
                string invoiceType = "po";
                int duration = 1;
                w.invoiceType = invoiceType;
                w.userId = MainWindow.userLogin.userId;
                w.branchCreatorId = MainWindow.loginBranch.branchId;
                w.duration = duration; // view purchase orders which created during  last one day 
                w.fromOrder = true;
                w.condition = "orders";
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
                        refreshNotification();
                        refreshDocCount(invoice.invoiceId);

                        await fillInvoiceInputs(invoice);

                        mainInvoiceItems = invoiceItems;
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrder");
                        invoices = FillCombo.invoices;
                        //invoices = await invoice.getUnHandeldOrders(invoiceType,MainWindow.branchID.Value,0,duration, MainWindow.userID.Value);
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
                #region global Sale Units
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

                // for pagination
                MainWindow.mainWindow.KeyDown += HandleKeyPress;
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
                setTimer();

                #region loading
                loadingList = new List<keyValueBool>();
                bool isDone = true;
                loadingList.Add(new keyValueBool { key = "loading_RefrishItems", value = false });
                loadingList.Add(new keyValueBool { key = "loading_refrishVendors", value = false });
                //loadingList.Add(new keyValueBool { key = "loading_fillBarcodeList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_globalPurchaseUnits", value = false });

                loading_RefrishItems();
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


                refreshNotification();
                //List all the UIElement in the VisualTree
                controls = new List<Control>();
                FindControl(this.grid_main, controls);
                #region datagridChange
                //CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(dg_billDetails.Items);
                //((INotifyCollectionChanged)myCollectionView).CollectionChanged += new NotifyCollectionChangedEventHandler(DataGrid_CollectionChanged);
                #endregion

                #region Permision


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

                #endregion
                #region print - pdf - send email
                if (!isFromReport)
                {
                    btn_printInvoice.Visibility = Visibility.Collapsed;
                    btn_pdf.Visibility = Visibility.Collapsed;
                    sp_Approved.Visibility = Visibility.Collapsed;
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
            public string Product { get; set; }
            public string Unit { get; set; }
            public int Count { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
            //public int OrderId { get; set; }
            public string invType { get; set; }
        }

        #endregion
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
                        _Count -= row.Count;
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

        #region timer to refresh notifications
        private void setTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(180); // 3 minutes
            timer.Tick += timer_Tick;
            timer.Start();
        }
        void timer_Tick(object sendert, EventArgs et)
        {
            try
            {
                if (invoice.invoiceId != 0)
                {
                    refreshNotification();
                    //refreshOrdersNotification();
                    refreshDocCount(invoice.invoiceId);

                    //refreshLackNotification();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region notification
        private async Task refreshNotification()
        {
            try
            {
                int duration = 1;
                InvoiceResult notificationRes = await invoiceModel.getPurOrderNot(MainWindow.userID.Value, duration, MainWindow.branchID.Value);
                refreshDraftNotification();
                setOrdersNotification(notificationRes.InvoiceCount);
                setLackNotification(notificationRes.isThereLack);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        private async void refreshDraftNotification()
        {
            try
            {
                string invoiceType = "pod,pos";
                int duration = 2;
                if (AppSettings.PurchaseOrdersDraftCount <= 0)
                {
                    AppSettings.PurchaseOrdersDraftCount = (int)await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                    AppSettings.PurchaseOrdersDraftCount = AppSettings.PurchaseOrdersDraftCount < 0 ? 0 : AppSettings.PurchaseOrdersDraftCount;
                }
                setDraftNotification(AppSettings.PurchaseOrdersDraftCount);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void setDraftNotification(int draftCount)
        {
            if (draftCount > 0 && invoice != null && (invoice.invType == "pod" || invoice.invType == "pos") && !isFromReport)
                draftCount--;

            if (draftCount != _DraftCount)
            {
                if (draftCount > 9)
                {
                    md_draft.Badge = "+9";
                }
                else if (draftCount == 0) md_draft.Badge = "";
                else
                    md_draft.Badge = draftCount.ToString();
            }
            _DraftCount = draftCount;
        }
        private void setOrdersNotification(int orderCount)
        {
            try
            {
                //string invoiceType = "po";
                //int duration = 1;
                //int orderCount = await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                if (invoice != null && invoice.invType == "po" && !isFromReport)
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
        private void setLackNotification(string isThereLack)
        {
            try
            {
                //string isThereLack = await invoice.isThereLack(MainWindow.branchID.Value);
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

        private async void Btn_updateVendor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    Window.GetWindow(this).Opacity = 0.2;
                    wd_updateVendor w = new wd_updateVendor();
                    //// pass agent id to update windows
                    w.agent.agentId = (int)cb_vendor.SelectedValue;
                    //w.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00178DD2"));
                    w.ShowDialog();
                    await FillCombo.RefreshVendors();
                    await RefrishVendors();

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
            vendors = FillCombo.vendorsList;
        }
        async Task RefrishItems()
        {
            //items = await itemModel.GetAllItems();
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
        public async Task ChangeItemIdEvent(int itemId)
        {
            try
            {
                item = items.ToList().Find(c => c.itemId == itemId);

                if (item != null)
                {
                    this.DataContext = item;

                    // get item units
                    // itemUnits = await itemUnitModel.GetItemUnits(item.itemId);
                    itemUnits = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemId == item.itemId).ToList();
                    // search for default unit for purchase
                    var defaultPurUnit = itemUnits.ToList().Find(c => c.defaultPurchase == 1);
                    if (defaultPurUnit != null)
                    {
                        int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == defaultPurUnit.itemUnitId).FirstOrDefault());
                        if (index == -1)//item doesn't exist in bill
                        {
                            // create new row in bill details data grid
                            addRowToBill(item.name, itemId, defaultPurUnit.mainUnit, defaultPurUnit.itemUnitId, 1, 0, 0);
                        }
                        else // item exist prevoiusly in list
                        {
                            billDetails[index].Count++;
                            billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;


                            _Count += billDetails[index].Count;
                            _Sum += billDetails[index].Price;
                        }
                        //refreshTotalValue();
                        //refrishBillDetails();
                    }
                    else
                    {
                        addRowToBill(item.name, itemId, null, 0, 1, 0, 0);
                        //refrishBillDetails();
                    }
                }
                tb_total.Text = _Count.ToString();
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
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_vendor")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorVendor, tt_errorVendor, "trErrorEmptyVendorToolTip");
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
            #region invoice object
            if ((invType == "po" || invType == "pos") && (invoice.invType == "pod" || invoice.invoiceId == 0))
                invoice.invNumber = "po";
            //invoice.invNumber = await invoice.generateInvNumber("po", MainWindow.loginBranch.code, MainWindow.branchID.Value);
            else if (invType == "pod" && invoice.invoiceId == 0)
                invoice.invNumber = "pod";
            //invoice.invNumber = await invoice.generateInvNumber("pod", MainWindow.loginBranch.code, MainWindow.branchID.Value);

            invoice.branchCreatorId = MainWindow.branchID.Value;
            invoice.branchId = MainWindow.branchID.Value;
            invoice.posId = MainWindow.posID.Value;

            invoice.invType = invType;
            invoice.total = 0;
            invoice.totalNet = 0;

            if (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                invoice.agentId = (int)cb_vendor.SelectedValue;

            invoice.notes = tb_note.Text;
            invoice.taxtype = 2;
            invoice.createUserId = MainWindow.userID;
            invoice.updateUserId = MainWindow.userID;
            byte isApproved = 0;
            if (tgl_ActiveOffer.IsChecked == true)
                isApproved = 1;
            else
                isApproved = 0;
            invoice.isApproved = isApproved;
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
                itemT.invoiceId = 0;
                invoiceItems.Add(itemT);
            }
            #endregion
            // save invoice in DB
            InvoiceResult invoiceResult = await invoiceModel.savePurchaseDraft(invoice, invoiceItems, MainWindow.posID.Value);

            if (invoiceResult.Result > 0)
            {
                invoice.invoiceId = invoiceResult.Result;
                invoice.invNumber = invoiceResult.Message;
                invoice.updateDate = invoiceResult.UpdateDate;
                TimeSpan ts;
                TimeSpan.TryParse(invoiceResult.InvTime, out ts);
                invoice.invTime = ts;

                AppSettings.PurchaseOrdersDraftCount = invoiceResult.PurchaseDraftCount;
                AppSettings.PosBalance = invoiceResult.PosBalance;
                MainWindow.setBalance();

                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

            }
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

        }
        private bool validateInvoiceValues()
        {
            bool valid = true;
            //if (!SectionData.validateEmptyComboBox(cb_vendor, p_errorVendor, tt_errorVendor, "trErrorEmptyVendorToolTip"))
            //    exp_vendor.IsExpanded = true;
            SectionData.validateEmptyComboBox(cb_vendor, p_errorVendor, tt_errorVendor, "trErrorEmptyVendorToolTip");
            if (billDetails.Count == 0)
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trAddInvoiceWithoutItems"), animation: ToasterAnimation.FadeIn);

            if ((cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0") && billDetails.Count > 0)
                valid = true;
            else
                valid = false;
            if (valid)
                valid = validateItemUnits();
            return valid;
        }
        private void Tgl_ActiveOffer_Checked(object sender, RoutedEventArgs e)
        {
            if (tgl_ActiveOffer.IsFocused)
            {
                #region Accept
                if (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0")
                {
                    MainWindow.mainWindow.Opacity = 0.2;
                    wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                    w.contentText = MainWindow.resourcemanager.GetString("trApproveOrderNotification");

                    w.ShowDialog();
                    if (!w.isOk)
                    {
                        tgl_ActiveOffer.IsChecked = false;
                        _InvoiceType = "pod";
                    }
                    else
                    {
                        _InvoiceType = "po";
                        btn_save.Content = MainWindow.resourcemanager.GetString("trSubmit");
                    }
                    MainWindow.mainWindow.Opacity = 1;

                }
                #endregion
                else
                {
                    tgl_ActiveOffer.IsChecked = false;
                    //exp_vendor.IsExpanded = true;
                    SectionData.validateEmptyComboBox(cb_vendor, p_errorVendor, tt_errorVendor, "trEmptyVendorToolTip");
                }
                inputEditable();

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
            }
        }
        private void Tgl_ActiveOffer_Unchecked(object sender, RoutedEventArgs e)
        {
            _InvoiceType = "pod";
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
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    //check mandatory inputs
                    bool valid = validateInvoiceValues();
                    if (valid)
                    {
                        if (tgl_ActiveOffer.IsChecked == true)
                            _InvoiceType = "po";
                        else
                            _InvoiceType = "pos";
                        await addInvoice(_InvoiceType); // po: purchase order
                        refreshNotification();

                        if (_InvoiceType == "po")
                            clearInvoice();
                        else
                            inputEditable();
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
                bool valid = validateItemUnits();
                if (billDetails.Count > 0 && valid && _InvoiceType == "pod")
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
                }
                clearInvoice();
                refreshNotification();

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
            _Count = 0;
            txt_invNumber.Text = "";
            _SequenceNum = 0;
            _SelectedVendor = -1;
            _InvoiceType = "pod"; // purchase order draft
            _isLack = false;
            invoice = new Invoice();
            tb_barcode.Clear();
            cb_vendor.SelectedIndex = -1;
            cb_vendor.SelectedItem = "";
            tb_note.Clear();
            billDetails.Clear();
            tb_total.Text = "";
            btn_updateVendor.IsEnabled = false;
            btn_addVendor.IsEnabled = false;
            md_docImage.Badge = "";
            isFromReport = false;
            archived = false;
            tgl_ActiveOffer.IsChecked = false;
            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseOrder");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            refrishBillDetails();
            inputEditable();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
        }
        #endregion

        private async void Btn_draft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                bool res = await saveBeforeExit();
                while (!res) { }
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();

                // purchase drafts and purchase bounce drafts
                string invoiceType = "pod, pos";
                int duration = 2;
                w.invoiceType = invoiceType;
                w.userId = MainWindow.userLogin.userId;
                w.duration = duration; // view drafts which created during 2 last days 
                w.fromOrder = true;
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
                        refreshNotification();
                        refreshDocCount(invoice.invoiceId);
                        await fillInvoiceInputs(invoice);

                        mainInvoiceItems = invoiceItems;
                        if (_InvoiceType == "pod")
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrderDraft");
                        else
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrderSaved");

                        invoices = FillCombo.invoices;
                        //invoices = await invoice.GetInvoicesByCreator(invoiceType, MainWindow.userID.Value, duration);
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
        private async void Btn_invoices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();

                // purchase invoices
                w.invoiceType = "p , pw";
                w.userId = MainWindow.userLogin.userId;
                w.duration = 1; // view drafts which created during 1 last days 

                w.title = MainWindow.resourcemanager.GetString("trPurchaseInvoices");

                if (w.ShowDialog() == true)
                {
                    if (w.invoice != null)
                    {
                        invoice = w.invoice;

                        this.DataContext = invoice;

                        _InvoiceType = invoice.invType;
                        // set title to bill
                        txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                        brd_total.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA926"));
                        await fillInvoiceInputs(invoice);

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
            txt_invNumber.Text = invoice.invNumber.ToString();
            cb_vendor.SelectedValue = invoice.agentId;
            tb_note.Text = invoice.notes;
            if (invoice.isApproved == 1)
                tgl_ActiveOffer.IsChecked = true;
            else
                tgl_ActiveOffer.IsChecked = false;
            await buildInvoiceDetails();
            inputEditable();
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
                _Count += (int)itemT.quantity;
                _SequenceNum++;
                decimal total = (decimal)(itemT.price * itemT.quantity);
                int orderId = 0;
                if (itemT.invoiceId != null)
                    orderId = (int)itemT.invoiceId;
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
                    //OrderId = orderId,
                    invType = invoice.invType,
                });
            }
            tb_total.Text = _Count.ToString();

            tb_barcode.Focus();

            refrishBillDetails();
        }
        private void inputEditable()
        {
            if (isFromReport && invoice.performed == true)
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                cb_vendor.IsEnabled = false;
                btn_addVendor.IsEnabled = false;
                tb_note.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                btn_clear.IsEnabled = false;
                btn_items.IsEnabled = false;
                tgl_ActiveOffer.Visibility = Visibility.Visible;
                tgl_ActiveOffer.IsEnabled = false;
                btn_save.IsEnabled = false;
            }
            else
            {
                if (_InvoiceType == "pod") // purchase order draft
                {
                    dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                    dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                    cb_vendor.IsEnabled = true;
                    tb_note.IsEnabled = true;
                    tb_barcode.IsEnabled = true;
                    btn_clear.IsEnabled = true;
                    btn_items.IsEnabled = true;
                    btn_addVendor.IsEnabled = true;
                    tgl_ActiveOffer.Visibility = Visibility.Collapsed;
                    tgl_ActiveOffer.IsEnabled = false;
                    btn_save.IsEnabled = true;
                }
                else if (_InvoiceType == "pos") // purchase order saved
                {
                    dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete column visible
                    dg_billDetails.Columns[3].IsReadOnly = false; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                    cb_vendor.IsEnabled = true;
                    btn_addVendor.IsEnabled = true;
                    tb_note.IsEnabled = true;
                    tb_barcode.IsEnabled = true;
                    btn_clear.IsEnabled = true;
                    btn_items.IsEnabled = true;
                    tgl_ActiveOffer.Visibility = Visibility.Visible;
                    tgl_ActiveOffer.IsEnabled = true;
                    btn_save.IsEnabled = true;
                }
                else if (_InvoiceType == "po") // purchase order
                {
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                    cb_vendor.IsEnabled = false;
                    btn_addVendor.IsEnabled = false;
                    tb_note.IsEnabled = false;
                    tb_barcode.IsEnabled = false;
                    btn_clear.IsEnabled = false;
                    btn_items.IsEnabled = false;
                    tgl_ActiveOffer.Visibility = Visibility.Visible;
                    tgl_ActiveOffer.IsEnabled = true;
                    btn_save.IsEnabled = true;
                }

                if (archived) //come from reports
                {
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[3].IsReadOnly = true; //make unit read only
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                    cb_vendor.IsEnabled = false;
                    tb_note.IsEnabled = false;
                    tb_barcode.IsEnabled = false;
                    tgl_ActiveOffer.IsEnabled = false;
                    btn_items.IsEnabled = false;
                    btn_clear.IsEnabled = false;
                    btn_save.IsEnabled = false;
                }
            }
            if (_InvoiceType.Equals("po") || _InvoiceType.Equals("pos"))
            {
                #region print - pdf - send email
                btn_printInvoice.Visibility = Visibility.Visible;
                btn_pdf.Visibility = Visibility.Visible;
                sp_Approved.Visibility = Visibility.Visible;
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
                sp_Approved.Visibility = Visibility.Collapsed;
                btn_emailMessage.Visibility = Visibility.Collapsed;
                bdr_emailMessage.Visibility = Visibility.Collapsed;
                #endregion
            }
            if (!isFromReport)
            {
                btn_next.Visibility = Visibility.Visible;
                btn_previous.Visibility = Visibility.Visible;
            }
        }
        private void Btn_invoiceImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
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



        private void Cb_vendor_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var tb = cb_vendor.Template.FindName("PART_EditableTextBox", cb_vendor) as TextBox;
                cb_vendor.ItemsSource = vendors.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_vendor.ItemsSource = vendors.Where(x => x.name.Contains(cb_vendor.Text));
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
                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && (cb_vendor.SelectedValue != null && cb_vendor.SelectedValue.ToString() != "0"))
                {
                    _SelectedVendor = (int)cb_vendor.SelectedValue;
                    if (_InvoiceType == "pod" || _InvoiceType == "pos")
                        btn_updateVendor.IsEnabled = true;
                }
                else
                {
                    cb_vendor.SelectedValue = _SelectedVendor;
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


        private void refreshTotalValue()
        {
            decimal total = _Sum;
            decimal taxValue = 0;
            decimal taxInputVal = 0;
            if (total != 0)
                taxValue = SectionData.calcPercentage(total, taxInputVal);

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
                await Task.Delay(1000);
                dg_billDetails.Items.Refresh();
                firstTimeForDatagrid = false;
                SectionData.EndAwait(grid_main);
            }
            DataGrid_CollectionChanged(dg_billDetails, null);
            tb_total.Text = _Count.ToString();
            ////tb_sum.Text = _Sum.ToString();
            //if (_Sum != 0)
            //    tb_sum.Text = SectionData.DecTostring(_Sum);
            //else
            //    tb_sum.Text = "0";

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
                if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                {
                    switch (e.Key)
                    {
                        case Key.P:
                            //handle D key
                            btn_printInvoice_Click(null, null);
                            break;
                        case Key.S:
                            //handle X key
                            Btn_save_Click(btn_save, null);
                            break;
                    }
                }



                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 50)
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
                    await dealWithBarcode(_BarcodeStr);
                    if (_Sender != null)
                    {
                        DatePicker dt = _Sender as DatePicker;
                        TextBox tb = _Sender as TextBox;
                        if (dt != null)
                        {
                            if (dt.Name == "dp_desrvedDate" || dt.Name == "dp_invoiceDate")
                                _BarcodeStr = _BarcodeStr.Substring(1);
                        }
                        else if (tb != null)
                        {
                            if (tb.Name == "tb_invoiceNumber" || tb.Name == "tb_note" || tb.Name == "tb_barcode")// remove barcode from text box
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
                    tb_barcode.Text = _BarcodeStr;
                    _BarcodeStr = "";
                    _IsFocused = false;
                    e.Handled = true;
                }
                _Sender = null;

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
                case "po":// this barcode for invoice               
                    Btn_newDraft_Click(null, null);
                    invoice = await invoiceModel.GetInvoicesByNum(barcode);
                    _InvoiceType = invoice.invType;
                    if (_InvoiceType.Equals("po") || _InvoiceType.Equals("pod"))
                    {
                        // set title to bill
                        if (_InvoiceType == "pod")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrderDraft");
                            brd_total.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA926"));
                        }
                        else if (_InvoiceType == "po")
                        {
                            txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trOrders");
                            brd_total.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA926"));
                        }

                        await fillInvoiceInputs(invoice);
                    }
                    break;

                default: // if barcode for item
                         // get item matches barcode
                    if (MainWindow.InvoiceGlobalItemUnitsList != null)
                    {
                        //ItemUnit unit1 = barcodesList.ToList().Find(c => c.barcode == tb_barcode.Text.Trim());
                        ItemUnit unit1 = MainWindow.InvoiceGlobalItemUnitsList.ToList().Find(c => c.barcode == tb_barcode.Text.Trim());

                        // get item matches the barcode
                        if (unit1 != null)
                        {
                            int itemId = (int)unit1.itemId;
                            if (unit1.itemId != 0)
                            {
                                int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == unit1.itemUnitId).FirstOrDefault());

                                if (index == -1)//item doesn't exist in bill
                                {
                                    // get item units
                                    itemUnits = await itemUnitModel.GetItemUnits(itemId);

                                    //get item from list
                                    item = items.ToList().Find(i => i.itemId == itemId);

                                    int count = 1;
                                    decimal price = 0; //?????
                                    decimal total = count * price;
                                    addRowToBill(item.name, item.itemId, unit1.mainUnit, unit1.itemUnitId, count, price, total);
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

        private void addRowToBill(string itemName, int itemId, string unitName, int itemUnitId, int count, decimal price, decimal total)
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
                Count = 1,
                Price = price,
                Total = total,
                invType = invoice.invType,
            });
            _Count++;
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

                if (tgl_ActiveOffer.IsChecked == true)
                    cmb.IsEnabled = false;
                else
                    cmb.IsEnabled = true;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cbm_unitItemDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cmb = sender as ComboBox;

                if (dg_billDetails.SelectedIndex != -1 && cmb.SelectedValue != null)
                {
                    var iu = (ItemUnit)cmb.SelectedItem;

                    billDetails[dg_billDetails.SelectedIndex].Unit = iu.mainUnit;
                    billDetails[dg_billDetails.SelectedIndex].itemUnitId = (int)cmb.SelectedValue;
                    if (tgl_ActiveOffer.IsChecked == true)
                        cmb.IsEnabled = false;
                    else
                        cmb.IsEnabled = true;
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

                                if (tgl_ActiveOffer.IsChecked == true)
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

            //if (dg_billDetails.SelectedIndex != -1)
            //    if (billDetails[dg_billDetails.SelectedIndex].OrderId != 0)
            //        e.Cancel = true;


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

                TextBox t = e.EditingElement as TextBox;  // Assumes columns are all TextBoxes
                var columnName = e.Column.Header.ToString();

                BillDetails row = e.Row.Item as BillDetails;
                int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == row.itemUnitId).FirstOrDefault());

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds < 100)
                {
                    if (columnName == MainWindow.resourcemanager.GetString("trQTR"))
                        t.Text = billDetails[index].Count.ToString();
                }
                else
                {
                    int oldCount = 0;
                    long newCount = 0;

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

                    if (_InvoiceType == "pbd" || _InvoiceType == "pbw")
                    {
                        ItemTransfer item = mainInvoiceItems.ToList().Find(i => i.itemUnitId == row.itemUnitId);
                        if (newCount > item.quantity)
                        {
                            // return old value 
                            t.Text = item.quantity.ToString();

                            newCount = (long)item.quantity;
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                        }
                    }


                    _Count -= oldCount;
                    _Count += newCount;
                    tb_total.Text = _Count.ToString();


                    // update item in billdetails           
                    billDetails[index].Count = (int)newCount;

                    //refrishDataGridItems();
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
        #region print-email
        public async Task<string> SavePurOrderpdf(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string pdfpath = "";

            //

            if (prInvoice.invoiceId > 0)
            {
                //prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                pdfpath = @"\Thumb\report\File.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                //agent
                if (prInvoice.agentId != null)
                {
                    Agent agentinv = new Agent();
                    // agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
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
                    //new lines
                    prInvoice.agentName = "-";
                    prInvoice.agentCompany = "-";
                }

                //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
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
                //user
                User employ = new User();
                // employ = await employ.getUserById((int)prInvoice.updateUserId);
                if (FillCombo.usersAllList is null)
                { await FillCombo.RefreshAllUsers(); }
                employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                prInvoice.uuserName = employ.name;
                prInvoice.uuserLast = employ.lastname;

                ReportCls.checkInvLang();

              //  clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
              //  string reppath = reportclass.GetpayInvoiceRdlcpath(prInvoice);
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
                List<PayedInvclass> repPayedList = new List<PayedInvclass>();
                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));
                rep.SetParameters(paramarr);
                rep.Refresh();

             //   LocalReportExtensions.ExportToPDF(rep, pdfpath);
                if (repsize.paperSize != "A4")
                {
                    LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                }
                else
                {
                    LocalReportExtensions.ExportToPDF(rep, pdfpath);
                }
            }

            return pdfpath;
        }
        //print
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Thread t1 = new Thread(() =>
                    {
                        pdfPurInvoice(invoice, invoiceItems);
                    });
                    t1.Start();
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


        private void btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Thread t1 = new Thread(async() =>
                    {
                        string msg = "";
                        msg=await printPurInvoice(invoice.invoiceId);
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

        public async Task<string> printPurInvoice(int invoiceId )
        {
            string msg = "";
            //  if (invoice.invoiceId > 0)
            try { 
                if (invoiceId > 0)
            {
             
                 Invoice prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);

                if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                                   || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                                   || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                {
                   // Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                    msg = "trPrintDraftInvoice";
                }
                else
                {
                    ReportCls rr = new ReportCls();

                    List<ReportParameter> paramarr = new List<ReportParameter>();


                    if (prInvoice.invoiceId > 0)
                    {
                        // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        //agent
                        Agent agentinv = new Agent();
                        //   agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                        if (FillCombo.agentsList is null)
                        { await FillCombo.RefreshAgents(); }
                        agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                        prInvoice.agentCode = agentinv.code;
                        //new lines
                        prInvoice.agentName = agentinv.name;
                        prInvoice.agentCompany = agentinv.company;

                        //user
                        User employ = new User();
                        //    employ = await employ.getUserById((int)prInvoice.updateUserId);
                        if (FillCombo.usersAllList is null)
                        { await FillCombo.RefreshAllUsers(); }
                        employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                        prInvoice.uuserName = employ.name;
                        prInvoice.uuserLast = employ.lastname;

                        // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
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


                        ReportCls.checkInvLang();

                            //    clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            //string reppath = reportclass.GetpayInvoiceRdlcpath(prInvoice);
                            //rep.ReportPath = reppath;
                            reportSize repsize = new reportSize();
                            int itemscount = 0;
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
                            List<PayedInvclass> repPayedList = new List<PayedInvclass>();
                            rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));
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
                }
            }
            }
            catch
            {
                msg = "trNotCompleted";
            }
            return msg;
        }
        public async void pdfPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            if (prInvoice.invoiceId > 0)
            {
                //  prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);

                if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                   || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                   || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd")
                {
                    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                }
                else
                {

                    List<ReportParameter> paramarr = new List<ReportParameter>();



                    if (prInvoice.invoiceId > 0)
                    {
                        //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                        //agent
                        Agent agentinv = new Agent();
                        //    agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                        if (FillCombo.agentsList is null)
                        { await FillCombo.RefreshAgents(); }
                        agentinv = FillCombo.agentsList.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();

                        prInvoice.agentCode = agentinv.code;
                        //new lines
                        prInvoice.agentName = agentinv.name;
                        prInvoice.agentCompany = agentinv.company;
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
                        // branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                        if (FillCombo.branchsAllList is null)
                        { await FillCombo.RefreshBranchsAll(); }
                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();

                        if (branch.branchId > 0)
                        {
                            prInvoice.branchName = branch.name;
                        }


                        ReportCls.checkInvLang();

                        //clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                        //string reppath = reportclass.GetpayInvoiceRdlcpath(invoice);
                        //rep.ReportPath = reppath;
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
                        List<PayedInvclass> repPayedList = new List<PayedInvclass>();
                        rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));
                        rep.SetParameters(paramarr);
                        rep.Refresh();
                        this.Dispatcher.Invoke(() =>
                        {
                            saveFileDialog.Filter = "PDF|*.pdf;";

                            if (saveFileDialog.ShowDialog() == true)
                            {

                                string filepath = saveFileDialog.FileName;
                              //  LocalReportExtensions.ExportToPDF(rep, filepath);
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

        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invoice.invoiceId > 0)
                    {
                        prInvoice = invoice;
                        // prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                        Window.GetWindow(this).Opacity = 0.2;

                        List<ReportParameter> paramarr = new List<ReportParameter>();
                        string pdfpath;

                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        if (prInvoice.invoiceId > 0)
                        {
                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                            //agent
                            if (prInvoice.agentId != null)
                            {
                                Agent agentinv = new Agent();
                                //  agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
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
                                //new lines
                                prInvoice.agentName = "-";
                                prInvoice.agentCompany = "-";
                            }

                            //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
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
                            //user
                            User employ = new User();
                            // employ = await employ.getUserById((int)prInvoice.updateUserId);
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                            prInvoice.uuserName = employ.name;
                            prInvoice.uuserLast = employ.lastname;

                            ReportCls.checkInvLang();

                            //clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            //string reppath = reportclass.GetpayInvoiceRdlcpath(invoice);
                            //rep.ReportPath = reppath;
                            reportSize repsize = new reportSize();
                            int itemscount = 0;
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
                            List<PayedInvclass> repPayedList = new List<PayedInvclass>();
                            rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", repPayedList));
                            rep.SetParameters(paramarr);
                            rep.Refresh();

                         //   LocalReportExtensions.ExportToPDF(rep, pdfpath);
                            if (repsize.paperSize != "A4")
                            {
                                LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                            }
                            else
                            {
                                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                            }
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
                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSaveInvoiceToPreview"), animation: ToasterAnimation.FadeIn);
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

        private void Btn_emailMessage_Click(object sender, RoutedEventArgs e)
        {//email
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    /////////////////////////////
                    Thread t1 = new Thread(async() =>
                    {
                        string msg = "";
                        msg=await sendPurEmail(invoice, invoiceItems);
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
                    /////////////////////////////
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
        public async Task<string> sendPurEmail(Invoice invoice, List<ItemTransfer> invoiceItems)
        {
            string msg = "";
            try
            {
                SysEmails email = new SysEmails();
                EmailClass mailtosend = new EmailClass();
                email = await email.GetByBranchIdandSide((int)MainWindow.branchID, "purchase");
                Agent toAgent = new Agent();
                toAgent = vendors.Where(x => x.agentId == invoice.agentId).FirstOrDefault();
                //  int? itemcount = invoiceItems.Count();
                if (email.emailId == 0)
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trNoEmailForThisDept"), animation: ToasterAnimation.FadeIn);
                else
                {
                    if (invoice.invoiceId == 0)
                    {
                        //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoOrderToSen"), animation: ToasterAnimation.FadeIn);
                        msg = "trThereIsNoOrderToSen";
                    }
                    else
                    {
                        if (invoiceItems == null || invoiceItems.Count() == 0)
                        {
                            //Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trThereIsNoItemsToSend"), animation: ToasterAnimation.FadeIn);
                            msg = "trThereIsNoItemsToSend";
                        }
                        else
                        {
                            if (toAgent.email.Trim() == "")

                            {
                               // Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTheVendorHasNoEmail"), animation: ToasterAnimation.FadeIn);
                                msg = "trTheVendorHasNoEmail";
                                }
                            else
                            {
                                SetValues setvmodel = new SetValues();

                                string pdfpath = await SavePurOrderpdf(invoice, invoiceItems);
                                mailtosend.AddAttachTolist(pdfpath);
                                List<SetValues> setvlist = new List<SetValues>();
                                setvlist = await setvmodel.GetBySetName("pur_order_email_temp");

                                mailtosend = mailtosend.fillOrderTempData(invoice, invoiceItems, email, toAgent, setvlist);
                                //this.Dispatcher.Invoke(new Action(() =>
                                //{
                                    string resmsg = "";
                                      resmsg = mailtosend.Sendmail();
                                    if (resmsg == "Failure sending mail.")
                                    {
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
            catch
            {
                msg = "trCannotSendEmail";
            }
            return msg  ;
        }

        #endregion
        private async void Btn_items_Click(object sender, RoutedEventArgs e)
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
                w.ShowDialog();
                if (w.isActive)
                {
                    ////// w.selectedItem this is ItemId
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

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                clearInvoice();
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
        private void clearNavigation()
        {
            _Sum = 0;
            _Count = 0;
            txt_invNumber.Text = "";
            _SequenceNum = 0;
            _SelectedVendor = -1;
            invoice = new Invoice();
            tb_barcode.Clear();
            cb_vendor.SelectedIndex = -1;
            cb_vendor.SelectedItem = "";
            tb_note.Clear();
            billDetails.Clear();
            tb_total.Text = "";
            btn_updateVendor.IsEnabled = false;
            btn_addVendor.IsEnabled = false;
            md_docImage.Badge = "";
            isFromReport = false;
            archived = false;
            tgl_ActiveOffer.IsChecked = false;

            refrishBillDetails();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
        }
        private async Task navigateInvoice(int index)
        {
            try
            {
                clearNavigation();
                invoice = invoices[index];
                _invoiceId = invoice.invoiceId;
                _InvoiceType = invoice.invType;
                if (invoice.invType == "pod")
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrderDraft");
                else if (invoice.invType == "pos")
                    txt_payInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaceOrderSaved");
                navigateBtnActivate();
                await fillInvoiceInputs(invoice);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
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
        private async void Btn_shortageInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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

                    Price = decimal.Parse(SectionData.DecTostring((decimal)itemT.price)),
                    Total = total,
                    invType = invoice.invType,
                });

                _Sum += total;
            }

            tb_barcode.Focus();
            refreshTotalValue();

            refrishBillDetails();
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


    }
}
