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
namespace POS.View.storage
{
    /// <summary>
    /// Interaction logic for uc_itemsExport.xaml
    /// </summary>
    public partial class uc_itemsExport : UserControl
    {
        string importPermission = "importExport_import";
        string exportPermission = "importExport_export";
        string reportsPermission = "importExport_reports";
        string initializeShortagePermission = "importExport_initializeShortage";
        string deliveryPermission = "setUserSetting_delivery";
        string deletePermission = "importExport_delete";

        private static uc_itemsExport _instance;
        public static uc_itemsExport Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_itemsExport();
                return _instance;
            }
        }
        public uc_itemsExport()
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

        public static bool archived = false;

        public static bool isFromReport = false;
        ItemUnit itemUnitModel = new ItemUnit();
        List<ItemUnit> barcodesList;
        List<ItemUnit> itemUnits;

        ItemLocation itemLocationModel = new ItemLocation();
        public Invoice invoice = new Invoice();
        Invoice generatedInvoice = new Invoice();
        List<Invoice> invoices;
        List<ItemTransfer> invoiceItems;
        List<ItemTransfer> mainInvoiceItems;
        List<ItemLocation> readyItemsLoc;
        List<ItemTransfer> orderList;
        User userModel = new User();

        public List<Control> controls;
        static public string _ProcessType = "imd"; //draft import
        bool _isLack = false;

        static private int _SequenceNum = 0;
        static private int _Count = 0;
        static private int _invoiceId;
        // for barcode
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private string _SelectedProcess = "";
        static private int _SelectedBranch = -1;
        bool _IsFocused = false;

        private static DispatcherTimer timer;

        Category categoryModel = new Category();
        Category category = new Category();

        Item itemModel = new Item();
        Item item = new Item();
        IEnumerable<Item> items;

        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;

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
            ////////////////////////////////----Order----/////////////////////////////////
            dg_billDetails.Columns[1].Header = MainWindow.resourcemanager.GetString("trNo.");
            dg_billDetails.Columns[2].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_billDetails.Columns[3].Header = MainWindow.resourcemanager.GetString("trUnit");
            dg_billDetails.Columns[4].Header = MainWindow.resourcemanager.GetString("trQTR");

            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementImport");
            txt_shortageInvoice.Text = MainWindow.resourcemanager.GetString("trLack");
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_newDraft.Text = MainWindow.resourcemanager.GetString("trNew");
            txt_drafts.Text = MainWindow.resourcemanager.GetString("trDrafts");
            txt_orders.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_ordersWait.Text = MainWindow.resourcemanager.GetString("trOrdersWait");
            
            txt_processType.Text = MainWindow.resourcemanager.GetString("trProcessType");
            txt_store.Text = MainWindow.resourcemanager.GetString("trStore/Branch");
            txt_count.Text = MainWindow.resourcemanager.GetString("trCount");
            txt_printInvoice.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_pdf.Text = MainWindow.resourcemanager.GetString("trPdf");
            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");


            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_processType, MainWindow.resourcemanager.GetString("trProcessTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trStore/BranchHint"));

            tt_error_previous.Content = MainWindow.resourcemanager.GetString("trPrevious");
            tt_error_next.Content = MainWindow.resourcemanager.GetString("trNext");


        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                MainWindow.mainWindow.KeyDown -= HandleKeyPress;
                timer.Stop();
                Btn_newDraft_Click(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch 
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
        }
        public async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.KeyDown += HandleKeyPress;
                MainWindow.mainWindow.Closing += ParentWin_Closing;

                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", assembly: Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                catigoriesAndItemsView.ucItemsExport = this;
                setNotifications();
                setTimer();
                configureProcessType();

                await FillBranches();
                await RefrishItems();
                await fillBarcodeList();
                await loading_globalPurchaseUnits();
                //List all the UIElement in the VisualTree
                controls = new List<Control>();
                FindControl(this.grid_main, controls);

                #region key up
                cb_branch.IsTextSearchEnabled = false;
                cb_branch.IsEditable = true;
                cb_branch.StaysOpenOnEdit = true;
                cb_branch.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;

                #endregion

                #region Permision


                if (MainWindow.groupObject.HasPermissionAction(exportPermission, MainWindow.groupObjects, "one"))
                    md_orderWaitCount.Visibility = Visibility.Visible;
                else
                    md_orderWaitCount.Visibility = Visibility.Collapsed;

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
            timer.Interval = TimeSpan.FromMinutes(3);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        void timer_Tick(object sendert, EventArgs et)
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
        #endregion
        #region notifications
        int _OrdersCount = 0;
        int _WaitedOrdersCount = 0;
        int _DraftCount = 0;
        private void setNotifications()
        {
            try
            {
                refreshDraftNotification();
                refreshTransactionNotification();

            }
            catch 
            {
  
            }
        }
        private async void refreshTransactionNotification()
        {
            try
            {
                int duration = 1;
                InvoiceResult notificationRes = await invoice.getTransNot(duration, MainWindow.branchID.Value);

                refreshOrdersNotification(notificationRes.OrdersCount);
                refreshOrderWaitNotification(notificationRes.WaitngExportCount);
                refreshLackNotification(notificationRes.isThereLack);

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
                int draftCount = 0;
                if (AppSettings.ImExpDraftCount <= 0)
                {
                    string invoiceType = "imd ,exd";
                    int duration = 2;
                    draftCount = (int)await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                    draftCount = draftCount < 0 ? 0 : draftCount;
                    AppSettings.ImExpDraftCount = draftCount;
                }
                else
                    draftCount = AppSettings.ImExpDraftCount;
                if (draftCount > 0 && (invoice.invType == "imd" || invoice.invType == "exd"))
                    draftCount--;

                SectionData.refreshNotification(md_draftsCount, ref _DraftCount, draftCount);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void refreshOrderWaitNotification(int waitedOrdersCount)
        {
            try
            {
                if (invoice.invType == "exw")
                    waitedOrdersCount--;

                SectionData.refreshNotification(md_orderWaitCount, ref _WaitedOrdersCount, waitedOrdersCount);

            }
            catch 
            {

            }
        }
        private async void refreshOrdersNotification(int ordersCount)
        {
            try
            {

                if (invoice.invType == "im" || invoice.invType == "ex")
                    ordersCount--;

                SectionData.refreshNotification(md_ordersCount, ref _OrdersCount, ordersCount);
            }
            catch 
            {

            }
        }
        private async Task refreshLackNotification(string isThereLack)
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
        #endregion

        async Task FillBranches()
        {
            await SectionData.fillWithoutCurrent(cb_branch, MainWindow.branchID.Value);

            if (cb_branch.Items.Count > 0)
                cb_branch.SelectedIndex = 0;

        }
        async Task RefrishItems()
        {
            if(cb_processType.SelectedValue == "ex")
            items = await itemModel.GetSaleOrPurItems(0, -1, -1, MainWindow.branchID.Value);
            else
                items = await itemModel.GetSaleOrPurItems(0, -1, -1, (int)cb_branch.SelectedValue);

        }
        async Task loading_globalPurchaseUnits()
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
        }
        private void configureProcessType()
        {
            var processList = new[] {
            new { Text = MainWindow.resourcemanager.GetString("trImport"), Value = "im" },
            new { Text = MainWindow.resourcemanager.GetString("trExport"), Value = "ex"},
             };

            cb_processType.DisplayMemberPath = "Text";
            cb_processType.SelectedValuePath = "Value";
            cb_processType.ItemsSource = processList;
            cb_processType.SelectedIndex = 0;
        }
        async Task fillBarcodeList()
        {
            barcodesList = await itemUnitModel.Getall();
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
                    e.Handled = true;
                }
                _BarcodeStr = "";
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

                        int availableAmount = 0;
                        #region availavble quantity

                        if (_ProcessType == "exd" || _ProcessType =="exw")
                        {
                            availableAmount = (int)await getAvailableAmount(itemId, (int)unit1.itemUnitId, MainWindow.branchID.Value, index + 1);
                        }
                        #endregion

                        int count = 0;
                        if (index == -1)//item doesn't exist in bill
                        {
                            // get item units
                            itemUnits = await itemUnitModel.GetItemUnits(itemId);
                            //get item from list
                            item = items.ToList().Find(i => i.itemId == itemId);

                            count = 1;
                            if (_ProcessType == "exd" || _ProcessType == "exw")
                            {
                                #region check available amount
                                if (availableAmount < count)
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                                    count = availableAmount;
                                }
                                else
                                    _Count++;
                                #endregion
                            }
                            else
                                _Count++;

                            addRowToBill(item.name, item.itemId, unit1.mainUnit, unit1.itemUnitId, count);
                        }
                        else // item exist prevoiusly in list
                        {
                            count = billDetails[index].Count + 1;

                            if (_ProcessType == "exd" || _ProcessType == "exw")
                            {
                                #region check available amount
                                if (availableAmount < count)
                                {
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);

                                    count = availableAmount;
                                }
                                else
                                    _Count++;
                                #endregion
                            }
                            else
                                _Count++;

                            billDetails[index].Count = count;
                        
                        }
                        refrishBillDetails();
                    }
                }
                else
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorItemNotFoundToolTip"), animation: ToasterAnimation.FadeIn);
                }
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
        private void addRowToBill(string itemName, int itemId, string unitName, int itemUnitId, int count)
        {
            #region valid serials
            bool isValid = true;


            int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == itemUnitId).FirstOrDefault());
            if ((_ProcessType=="exw" || _ProcessType == "exd") && item.type == "sn" ||
                (item.type.Equals("p") && item.packageItems != null && item.packageItems.Where(x => x.type == "sn").FirstOrDefault() != null))
            {
                isValid = false;
            }
            #endregion
            // increase sequence for each read
            _SequenceNum++;

            billDetails.Add(new BillDetails()
            {
                ID = _SequenceNum,
                Product = item.name,
                itemId = item.itemId,
                type = item.type,
                Unit = unitName,
                itemUnitId = itemUnitId,
                Count = count,
                packageItems = item.packageItems,
                valid = isValid,
            });

        }
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
            tb_count.Text = _Count.ToString();

        }
        void refrishDataGridItems()
        {
            dg_billDetails.ItemsSource = null;
            dg_billDetails.ItemsSource = billDetails;
            dg_billDetails.Items.Refresh();
            DataGrid_CollectionChanged(dg_billDetails, null);

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
            public int OrderId { get; set; }
            public List<Serial> itemSerials { get; set; }
            public List<StoreProperty> StoreProperties { get; set; }
            public bool valid { get; set; }
            public string type { get; set; }
            public List<Item> packageItems { get; set; }

        }
        #endregion


        private async void Btn_orders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();

                if ((
                       MainWindow.groupObject.HasPermissionAction(importPermission, MainWindow.groupObjects, "one")
                       || SectionData.isAdminPermision()
                       ) &&
                     (
                     MainWindow.groupObject.HasPermissionAction(exportPermission, MainWindow.groupObjects, "one")
                     || SectionData.isAdminPermision()
                     ))
                    w.invoiceType = "im ,ex";
                else if (MainWindow.groupObject.HasPermissionAction(importPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    w.invoiceType = "im";
                else if (MainWindow.groupObject.HasPermissionAction(exportPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                    w.invoiceType = "ex";



                w.condition = "exportImport";
                w.duration = 1;
                w.title = MainWindow.resourcemanager.GetString("trOrders");
                w.branchId = MainWindow.branchID.Value;

                if (w.ShowDialog() == true)
                {
                    if (w.invoice != null)
                    {
                        invoice = w.invoice;
                        _ProcessType = invoice.invType;
                        _invoiceId = invoice.invoiceId;
                        isFromReport = false;
                        archived = false;
                        setNotifications();
                        await fillOrderInputs(invoice);
                        if (_ProcessType == "im")// set title to bill
                        {
                            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementImport");

                        }
                        else if (_ProcessType == "ex")
                        {
                            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementExport");

                        }
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

        private async void Btn_ordersWait_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(exportPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_invoice w = new wd_invoice();

                    w.invoiceType = "exw";
                    w.condition = "export";
                    w.title = MainWindow.resourcemanager.GetString("trOrdersWait");
                    w.branchId = MainWindow.branchID.Value;

                    if (w.ShowDialog() == true)
                    {
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _ProcessType = invoice.invType;
                            _invoiceId = invoice.invoiceId;
                            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementExport");

                            setNotifications();
                            await fillOrderInputs(invoice);
                            invoices = FillCombo.invoices;
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
        /*
        private void Btn_package_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(packagePermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_generatePackage w = new wd_generatePackage();

                    if (w.ShowDialog() == true)
                    {

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
        */
        /*
        private void Btn_unitConversion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(unitConversionPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_unitConversion w = new wd_unitConversion();
                    if (w.ShowDialog() == true)
                    {

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
        */
        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                string name = sender.GetType().Name;
                if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_branch")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorBranch, tt_errorBranch, "trEmptyBranchToolTip");
                }

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

                //items

                Window.GetWindow(this).Opacity = 0.2;
                wd_items w = new wd_items();
                w.CardType = "movement";
                w.ProcessType = cb_processType.SelectedValue.ToString();
                w.items = items;
                if (cb_processType.SelectedValue.Equals("im"))
                    w.branchId = (int)cb_branch.SelectedValue;
                else
                    w.branchId = (int)MainWindow.branchID;

                w.ShowDialog();
                if (w.isActive)
                {
                    for (int i = 0; i < w.selectedItems.Count; i++)
                    {
                        int itemId = w.selectedItems[i];
                        await ChangeItemIdEvent(itemId);
                    }
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
        #region Get Id By Click  Y

        public void ChangeCategoryIdEvent(int categoryId)
        {

        }


        public async Task ChangeItemIdEvent(int itemId)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                if (items != null) item = items.ToList().Find(c => c.itemId == itemId);

                if (item != null)
                {
                    this.DataContext = item;

                    // get item units
                    itemUnits = await itemUnitModel.GetItemUnits(item.itemId);
                    // search for default unit for purchase
                    var defaultPurUnit = itemUnits.ToList().Find(c => c.defaultPurchase == 1);
                    if (defaultPurUnit != null)
                    {
                        int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == defaultPurUnit.itemUnitId).FirstOrDefault());
                        if (index == -1)//item doesn't exist in bill
                        {
                            // create new row in bill details data grid
                            addRowToBill(item.name, itemId, defaultPurUnit.mainUnit, defaultPurUnit.itemUnitId, 1);
                            _Count++;
                        }
                        else // item exist prevoiusly in list
                        {
                            billDetails[index].Count++;
                            billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;
                        }

                    }
                    else
                    {
                        addRowToBill(item.name, itemId, null, 0, 1);
                        _Count++;
                    }

                }
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion
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
                        _Count -= row.Count;

                        // remove item from bill
                        billDetails.RemoveAt(index);

                        ObservableCollection<BillDetails> data = (ObservableCollection<BillDetails>)dg_billDetails.ItemsSource;
                        data.Remove(row);

                        tb_count.Text = _Count.ToString();
                    }
                _SequenceNum = 0;
                for (int i = 0; i < billDetails.Count; i++)
                {
                    _SequenceNum++;
                    billDetails[i].ID = _SequenceNum;
                }
                refrishBillDetails();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        private async void Btn_newDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (billDetails.Count > 0 && (_ProcessType == "imd" || _ProcessType == "exd") && _isLack == false)
                {
                    #region Accept
                    MainWindow.mainWindow.Opacity = 0.2;
                    wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                    w.contentText = MainWindow.resourcemanager.GetString("trSaveProcessDraft");
                    w.ShowDialog();
                    MainWindow.mainWindow.Opacity = 1;
                    #endregion
                    if (w.isOk)
                        await saveDraft();
                }
                clearProcess();
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
        private void clearProcess()
        {
            _Count = 0;
            _SequenceNum = 0;
            _SelectedBranch = -1;
            _SelectedProcess = "imd";
            _ProcessType = "imd";
            _isLack = false;
            cb_processType.SelectedIndex = 0;
            isFromReport = false;
            archived = false;
            invoice = new Invoice();
            generatedInvoice = new Invoice();
            tb_barcode.Clear();
            billDetails.Clear();
            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementImport");

            txt_invNumber.Text = "";
            SectionData.clearComboBoxValidate(cb_branch, p_errorBranch);
            refrishBillDetails();
            inputEditable();
            btn_next.Visibility = Visibility.Collapsed;
            btn_previous.Visibility = Visibility.Collapsed;
        }
        private async Task saveDraft()
        {
            InvoiceResult invoiceResult = new InvoiceResult();
            invoiceItems = new List<ItemTransfer>();
            ItemTransfer itemT;
            for (int i = 0; i < billDetails.Count; i++)
            {
                itemT = new ItemTransfer();

                itemT.quantity = billDetails[i].Count;
                itemT.price = billDetails[i].Price;
                itemT.itemUnitId = billDetails[i].itemUnitId;
                itemT.itemSerials = billDetails[i].itemSerials;
                itemT.createUserId = MainWindow.userID;
                itemT.invoiceId = 0;

                invoiceItems.Add(itemT);
            }
            switch (_ProcessType)
            {
                case "imd":// add or edit import order then add export order
                           // import order
                    invoice.invType = _ProcessType;
                    invoice.branchId = MainWindow.branchID.Value;
                    invoice.branchCreatorId = MainWindow.branchID.Value;
                    invoice.createUserId = MainWindow.userID;
                    invoice.updateUserId = MainWindow.userID;
                    if (invoice.invNumber == null)
                        invoice.invNumber = "im";
                    //export order
                    Invoice exportInv = new Invoice();
                    exportInv.invType = "exi";

                    if (cb_branch.SelectedIndex != -1)
                        exportInv.branchId = (int)cb_branch.SelectedValue;
                    exportInv.branchCreatorId = MainWindow.branchID.Value;
                    exportInv.invNumber = "ex";
                    exportInv.createUserId = MainWindow.userID;
                    exportInv.updateUserId = MainWindow.userID;
                    // save invoice in DB
                    invoiceResult = await invoice.SaveImportOrder(invoice, exportInv, invoiceItems, new Notification(), false);
                    
                    break;
                case "exd":// add or edit export order then add import order
                           // import order
                    invoice.invType = _ProcessType;
                    invoice.branchId = MainWindow.branchID.Value;
                    invoice.branchCreatorId = MainWindow.branchID.Value;
                    invoice.createUserId = MainWindow.userID;
                    invoice.updateUserId = MainWindow.userID;
                    if (invoice.invNumber == null)
                        invoice.invNumber = "ex";

                    //import order
                    Invoice importInvoice = new Invoice();
                    importInvoice.invType = "imi";
                    if (cb_branch.SelectedIndex != -1)
                        importInvoice.branchId = (int)cb_branch.SelectedValue;
                    importInvoice.invNumber = "im";
                    importInvoice.createUserId = MainWindow.userID;
                    importInvoice.updateUserId = MainWindow.userID;
                    // save invoice in DB
                    invoiceResult = await invoice.GenerateExport(invoice, importInvoice, invoiceItems, new List<ItemLocation>(), new Notification(), MainWindow.branchID.Value, MainWindow.userID.Value, false);

                 
                    break;
            }
            if (invoiceResult.Result > 0)
            {
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                AppSettings.ImExpDraftCount = invoiceResult.ImExpDraftCount;
                clearProcess();
                setNotifications();
            }
        }

        private async void Btn_draft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();
                string invoiceType = "imd ,exd";
                int duration = 2;
                w.invoiceType = invoiceType;
                w.userId = MainWindow.userLogin.userId;
                w.duration = duration; // view drafts which updated during 2 last days 
                w.title = MainWindow.resourcemanager.GetString("trDrafts");

                if (w.ShowDialog() == true)
                {
                    if (w.invoice != null)
                    {
                        invoice = w.invoice;
                        _ProcessType = invoice.invType;
                        _invoiceId = invoice.invoiceId;
                        isFromReport = false;
                        archived = false;
                        setNotifications();
                        await fillOrderInputs(invoice);
                        
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
        public async Task fillOrderInputs(Invoice invoice)
        {
            if (invoice.invoiceMainId == null)
                generatedInvoice = await invoice.getgeneratedInvoice(invoice.invoiceId);
            else
                generatedInvoice = await invoice.GetByInvoiceId((int)invoice.invoiceMainId);

            txt_invNumber.Text = invoice.invNumber;

            _Count = invoice.itemsCount.Value;
            tb_count.Text = _Count.ToString();

            cb_branch.SelectedValue = generatedInvoice.branchId;
            switch (_ProcessType)
            {
                case "imd":
                case "im":
                case "imw":
                    cb_processType.SelectedIndex = 0;
                    cb_processType.SelectedValue = "im";
                    break;
                case "exd":
                case "ex":
                case "exw":
                    cb_processType.SelectedIndex = 1;
                    cb_processType.SelectedValue = "ex";
                    break;
            }

            // build invoice details grid
            await buildInvoiceDetails();

            inputEditable();
        }
        private async Task buildInvoiceDetails()
        {
            //get invoice items
            if (invoice.invoiceItems == null)
                invoiceItems = await invoice.GetInvoicesItems(invoice.invoiceId);
            else
                invoiceItems = invoice.invoiceItems;
            // build invoice details grid
            _SequenceNum = 0;
            billDetails.Clear();
            foreach (ItemTransfer itemT in invoiceItems)
            {
                _SequenceNum++;
                decimal total = (decimal)(itemT.price * itemT.quantity);
                int orderId = 0;
                if (itemT.invoiceId != null)
                    orderId = (int)itemT.invoiceId;
                #region valid item serials
                bool isValid = true;

                if (_ProcessType == "exw" || _ProcessType == "exd")
                {
                    if (itemT.itemType == "sn" && itemT.itemSerials.Count() < itemT.quantity)
                        isValid = false;
                    else if (itemT.itemType.Equals("p"))
                    {
                        long packageCount = (long)itemT.quantity;
                        long serialNum = 0;
                        foreach (var p in itemT.packageItems)
                        {
                            if (p.type.Equals("sn"))
                                serialNum += (long)p.itemCount * packageCount;
                        }

                        if (itemT.itemSerials.Count() < serialNum)
                            isValid = false;
                    }
                }
                #endregion
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
                    OrderId = orderId,
                    itemSerials = itemT.itemSerials,
                    StoreProperties=itemT.ItemStoreProperties,
                    packageItems = itemT.packageItems,
                    type = itemT.itemType,
                    valid = isValid,
                });
            }
            tb_barcode.Focus();

            refrishBillDetails();
        }
        private void inputEditable()
        {
            if (invoice.invoiceId == 0)
                cb_processType.IsEnabled = true;
            else
                cb_processType.IsEnabled = false;

            if (_ProcessType == "imd" || _ProcessType == "exd") // return invoice
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete hidden
                dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                cb_processType.IsEnabled = true;
                cb_branch.IsEnabled = true;
                tb_barcode.IsEnabled = true;
                btn_save.IsEnabled = true;
                btn_items.IsEnabled = true;
                btn_deleteInventory.Visibility = Visibility.Collapsed;
            }
            else if (_ProcessType == "im" || _ProcessType == "ex")
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete hidden
                dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                cb_branch.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                btn_save.IsEnabled = false;
                btn_items.IsEnabled = false;
                if(_ProcessType.Equals("im") && invoice.notes.Equals("exw"))
                    btn_deleteInventory.Visibility = Visibility.Visible;
                else
                    btn_deleteInventory.Visibility = Visibility.Collapsed;


            }
            else if (_ProcessType == "imw")
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete hidden
                dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                cb_branch.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                btn_save.IsEnabled = true;
                btn_items.IsEnabled = false;
                btn_deleteInventory.Visibility = Visibility.Visible;

            }
            else if (_ProcessType == "exw")
            {
                dg_billDetails.Columns[0].Visibility = Visibility.Visible; //make delete hidden
                dg_billDetails.Columns[4].IsReadOnly = false; //make count read only
                cb_branch.IsEnabled = false;
                tb_barcode.IsEnabled = false;
                btn_save.IsEnabled = true;
                btn_items.IsEnabled = false;
                btn_deleteInventory.Visibility = Visibility.Visible;

            }
            if (!isFromReport)
            {
                btn_next.Visibility = Visibility.Visible;
                btn_previous.Visibility = Visibility.Visible;
            }
        }
        private async Task save()
        {
            int branchId = 0;
            if (cb_branch.SelectedValue != null)
                branchId = (int)cb_branch.SelectedValue;

            InvoiceResult invoiceResult = new InvoiceResult();
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
                itemT.invoiceId = billDetails[i].OrderId;
                itemT.itemSerials = billDetails[i].itemSerials;
                itemT.ItemStoreProperties = billDetails[i].StoreProperties;
                invoiceItems.Add(itemT);
            }
            #endregion
            switch (_ProcessType)
            {
                case "imd":// add or edit import order then add export order
                           // import order
                    invoice.invType = "im";
                    invoice.branchId = MainWindow.branchID.Value;
                    invoice.branchCreatorId = MainWindow.branchID.Value;
                    invoice.posId = MainWindow.posID.Value;
                    invoice.createUserId = MainWindow.userID;
                    invoice.updateUserId = MainWindow.userID;
                    if (invoice.invNumber == null)
                        invoice.invNumber = "im";

                    #region notification Object

                    Notification not = new Notification()
                    {
                        title = "trExportAlertTilte",
                        ncontent = "trExportAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_ImpExp",
                        branchId = branchId,
                        prefix = MainWindow.loginBranch.name,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };

                    #endregion

                    #region export invoice
                    Invoice exportInv = new Invoice();

                    exportInv.invType = "exw";
                    exportInv.branchCreatorId = MainWindow.branchID.Value;
    
                    if (cb_branch.SelectedIndex != -1)
                        exportInv.branchId = (int)cb_branch.SelectedValue;
                    exportInv.invNumber = "ex";
                    exportInv.createUserId = MainWindow.userID;
                    exportInv.updateUserId = MainWindow.userID;

                    #endregion
                    // save invoice in DB
                    invoiceResult = await invoice.SaveImportOrder(invoice, exportInv, invoiceItems, not);
                 
                    break;

                case "exd":// add or edit export order then add import order
                           // import order
                    #region export order
                    invoice.invType = "ex";
                    invoice.branchId = MainWindow.branchID.Value;
                    invoice.branchCreatorId = MainWindow.branchID.Value;
                    invoice.posId = MainWindow.posID.Value;
                    invoice.createUserId = MainWindow.userID;
                    invoice.updateUserId = MainWindow.userID;
                    if (invoice.invNumber == null)
                        invoice.invNumber = "ex";

                    #endregion
                    #region import order

                    Invoice importInvoice = new Invoice();
                    importInvoice.invType = "im";
                    importInvoice.branchCreatorId = MainWindow.branchID.Value;

                    if (cb_branch.SelectedIndex != -1)
                        importInvoice.branchId = (int)cb_branch.SelectedValue;
                    importInvoice.invNumber = "im";
                    importInvoice.createUserId = MainWindow.userID;
                    importInvoice.updateUserId = MainWindow.userID;

                    #endregion
                    #region notification Object

                    Notification not1 = new Notification()
                    {
                        title = "trExceedMaxLimitAlertTilte",
                        ncontent = "trExceedMaxLimitAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_minMaxItem",
                        branchId = branchId,
                        createDate = DateTime.Now,
                        updateDate = DateTime.Now,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };
                    #endregion
                    invoiceResult = await invoice.GenerateExport(invoice, importInvoice, invoiceItems, readyItemsLoc, not1, MainWindow.branchID.Value, MainWindow.userID.Value);
                    break;

                case "exw":
                    invoice.invType = "ex";
                    invoice.updateUserId = MainWindow.userID;
                    #region notification Object
                    Notification not2 = new Notification()
                    {
                        title = "trExceedMaxLimitAlertTilte",
                        ncontent = "trExceedMaxLimitAlertContent",
                        msgType = "alert",
                        objectName = "storageAlerts_minMaxItem",
                        branchId = branchId,
                        createDate = DateTime.Now,
                        updateDate = DateTime.Now,
                        createUserId = MainWindow.userID.Value,
                        updateUserId = MainWindow.userID.Value,
                    };
                    #endregion
                    // save invoice in DB
                    invoiceResult = await invoice.AcceptWaitingImport(invoice, invoiceItems, readyItemsLoc, not2, MainWindow.branchID.Value, MainWindow.userID.Value);

                    break;
            }

            if (invoiceResult.Result > 0)
            {
                AppSettings.ImExpDraftCount = invoiceResult.ImExpDraftCount;
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                clearProcess();
                setNotifications();
            }
            else if (invoiceResult.Result == -3)// كمية العنصر غير كافية
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

        }
        private async Task<bool> validateOrder()
        {
            bool valid = true;

            if (billDetails.Count == 0)
            {
                valid = false;

                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorQuantIsZeroToolTip"), animation: ToasterAnimation.FadeIn);
                clearProcess();
            }
            else if (!SectionData.validateEmptyComboBox(cb_branch, p_errorBranch, tt_errorBranch, MainWindow.resourcemanager.GetString("trEmptyBranchToolTip")))
            {
                valid = false;
            }

            return valid;
        }
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (
                            (
                                (_ProcessType == "im" || _ProcessType == "imd")
                            &&
                                 (MainWindow.groupObject.HasPermissionAction(importPermission, MainWindow.groupObjects, "one"))
                            )
                        ||
                            (
                                (_ProcessType == "ex" || _ProcessType == "exd" || _ProcessType == "exw")
                            &&
                                (MainWindow.groupObject.HasPermissionAction(exportPermission, MainWindow.groupObjects, "one"))
                            )
                   )
                {
                    bool valid = await validateOrder();
                    if (valid)
                    {
                        //wd_transItemsLocation w;
                        switch (_ProcessType)
                        {
                            case "exw":
                            case "exd":

                                Window.GetWindow(this).Opacity = 0.2;
                                wd_transItemsLocation w = new wd_transItemsLocation();
                                orderList = new List<ItemTransfer>();
                                foreach (BillDetails d in billDetails)
                                {
                                    if (d.Count == 0)
                                    {
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorQuantIsZeroToolTip"), animation: ToasterAnimation.FadeIn);
                                        Window.GetWindow(this).Opacity = 1;
                                        if (sender != null)
                                            SectionData.EndAwait(grid_main);
                                        return;
                                    }
                                    else
                                    {
                                        orderList.Add(new ItemTransfer()
                                        {
                                            itemName = d.Product,
                                            itemId = d.itemId,
                                            unitName = d.Unit,
                                            itemUnitId = d.itemUnitId,
                                            quantity = d.Count,
                                            invoiceId = 0,
                                        });
                                    }
                                }
                                w.orderList = orderList;
                                w.ShowDialog();
                                if (w.isOk == true)
                                {
                                    if (w.selectedItemsLocations != null)
                                    {
                                        List<ItemLocation> itemsLocations = w.selectedItemsLocations;

                                        readyItemsLoc = new List<ItemLocation>();

                                        for (int i = 0; i < itemsLocations.Count; i++)
                                        {
                                            if (itemsLocations[i].isSelected == true)
                                                readyItemsLoc.Add(itemsLocations[i]);
                                        }
                                      
                                        await save();
                                    }
                                }
                                Window.GetWindow(this).Opacity = 1;
                                break;
                            case "emw":
                                //process transfer items
                                await save();
                                break;
                            default:
                                await save();
                                break;
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

        private void Cb_branch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && cb_branch.SelectedIndex != -1)
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

        private async void Dg_billDetails_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {

                TextBox t = e.EditingElement as TextBox;  // Assumes columns are all TextBoxes
                var columnName = e.Column.Header.ToString();

                BillDetails row = e.Row.Item as BillDetails;
                int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == row.itemUnitId && p.OrderId == row.OrderId).FirstOrDefault());

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds < 100)
                {
                    if (columnName == MainWindow.resourcemanager.GetString("trQuantity"))
                        t.Text = billDetails[index].Count.ToString();
                }
                else
                {
                    int availableAmount = 0;

                    int oldCount = 0;
                    if (!t.Text.Equals(""))
                        oldCount = int.Parse(t.Text);
                    else
                        oldCount = 0;
                    int newCount = 0;
                    //"tb_amont"
                    if (columnName == "QTY")
                    {
                        if (_ProcessType == "exd")
                        {
                            if (row.type == "sn" || (row.type.Equals("p") && row.packageItems != null && row.packageItems.Where(x => x.type == "sn").FirstOrDefault() != null))
                                billDetails[index].valid = false;

                            availableAmount = (int)await getAvailableAmount(row.itemId, row.itemUnitId, MainWindow.branchID.Value, row.ID);
                            if (availableAmount < oldCount)
                            {

                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableToolTip"), animation: ToasterAnimation.FadeIn);
                                newCount = newCount + availableAmount;
                                t.Text = availableAmount.ToString();
                            }
                            else
                            {
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
                        }
                        else
                        {
                            if (!t.Text.Equals(""))
                                newCount = int.Parse(t.Text);
                            else
                                newCount = 0;
                        }
                    }
                    else
                        newCount = row.Count;


                    _Count -= oldCount;
                    _Count += newCount;

                    //  refresh count text box
                    tb_count.Text = _Count.ToString();

                    // update item in billdetails           
                    billDetails[index].Count = (int)newCount;
                    if (invoiceItems != null)
                        invoiceItems[index].quantity = (int)newCount;
                }

                refrishDataGridItems();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async Task<decimal> getAvailableAmount(int itemId, int itemUnitId, int branchId, int ID)
        {
            var itemUnits = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemId == itemId).ToList();
            int availableAmount = (int)await itemLocationModel.getAmountInBranch(itemUnitId, branchId);
            var smallUnits = await itemUnitModel.getSmallItemUnits(itemId, itemUnitId);
            foreach (ItemUnit u in itemUnits)
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
                        unitValue = (int)await itemUnitModel.largeToSmallUnitQuan(itemUnitId, u.itemUnitId);
                        quantity = quantity / unitValue;
                    }
                    else
                    {
                        unitValue = (int)await itemUnitModel.smallToLargeUnit(itemUnitId, u.itemUnitId);

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
        private void Cb_processType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                if (elapsed.TotalMilliseconds > 100 && cb_processType.SelectedIndex != -1)
                {
                    _SelectedProcess = (string)cb_processType.SelectedValue;
                    if (invoice.invoiceId == 0)
                        _ProcessType = cb_processType.SelectedValue + "d";
                    if (cb_processType.SelectedValue.ToString() == "im")
                    {
                        btn_save.Content = MainWindow.resourcemanager.GetString("trImport");
                        txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementImport");

                    }
                    else if (cb_processType.SelectedValue.ToString() == "ex")
                    {
                        btn_save.Content = MainWindow.resourcemanager.GetString("trExport");
                        txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementExport");

                        //ereaseQuantity();
                    }
                    RefrishItems();
                }
                else
                {
                    cb_processType.SelectedValue = _SelectedProcess;
                }

                foreach (var itemT in billDetails)
                {
                    #region valid item serials
                    bool isValid = true;

                    if (_ProcessType == "exw" || _ProcessType == "exd")
                    {
                        if (itemT.type == "sn" && itemT.itemSerials.Count() < itemT.Count)
                            isValid = false;
                        else if (itemT.type.Equals("p"))
                        {
                            long packageCount = (long)itemT.Count;
                            long serialNum = 0;
                            foreach (var p in itemT.packageItems)
                            {
                                if (p.type.Equals("sn"))
                                    serialNum += (long)p.itemCount * packageCount;
                            }

                            if (itemT.itemSerials.Count() < serialNum)
                                isValid = false;
                        }
                       
                    }
                    #endregion
                    itemT.valid = isValid;
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
        private async Task<Boolean> checkItemsAmounts()
        {
            Boolean available = true;
            for (int i = 0; i < billDetails.Count; i++)
            {
                int availableAmount = (int)await itemLocationModel.getAmountInBranch(billDetails[i].itemUnitId, MainWindow.branchID.Value);
                if (availableAmount < billDetails[i].Count)
                {
                    available = false;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip") + " " + billDetails[i].Product, animation: ToasterAnimation.FadeIn);
                    return available;
                }
            }
            return available;
        }
        //private void ereaseQuantity()
        //{
        //    foreach (BillDetails b in billDetails)
        //    {
        //        b.Count = 0;
        //    }
        //    refrishDataGridItems();
        //}
        #region print

        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    ///  
                    if (invoiceItems != null)
                    {
                        Thread t1 = new Thread(() =>
                    {
                        printExport();
                    });
                        t1.Start();
                    }
                    //////////////////////////////////////
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

        private async void printExport()
        {
            reportSize repsize = new reportSize();
            repsize= await BuildReport();
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

            this.Dispatcher.Invoke(() =>
            {
               // LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
                if (repsize.paperSize == "A4")
                {

                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, short.Parse(AppSettings.rep_print_count));

                }
                else
                {
                    LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, short.Parse(AppSettings.rep_print_count), repsize.width, repsize.height);

                }
            });
        }

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (invoiceItems != null)
                    {
                        Thread t1 = new Thread(() =>
                    {
                        pdfExport();
                    });
                        t1.Start();
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

        private async void pdfExport()
        {
            reportSize repsize = new reportSize();
            repsize= await BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                   // LocalReportExtensions.ExportToPDF(rep, filepath);
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

        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invoiceItems != null)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        /////////////////////
                        string pdfpath = "";
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                        reportSize repsize = new reportSize();
                        repsize = await BuildReport();
                     //   LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        if (repsize.paperSize != "A4")
                        {
                            LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                        }
                        else
                        {
                            LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        }
                        ///////////////////
                        wd_previewPdf w = new wd_previewPdf();
                        w.pdfPath = pdfpath;
                        if (!string.IsNullOrEmpty(w.pdfPath))
                        {
                            w.ShowDialog();
                            w.wb_pdfWebViewer.Dispose();
                        }
                        Window.GetWindow(this).Opacity = 1;
                    }
                    //////////////////////////////////////
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


        public async Task<reportSize> BuildReport()
        {
            Invoice prInvoice = invoice;
            List<ReportParameter> paramarr = new List<ReportParameter>();
            reportSize repsize = new reportSize();
            // string reppath = reportclass.GetDirectEntryRdlcpath(prInvoice);
            if (prInvoice.invoiceId > 0)
            {
               
               // string addpath;
                string isArabic = ReportCls.checkInvLang();
                //if (isArabic == "ar")
                //{//ItemsExport
                //    addpath = @"\Reports\Store\Ar\ArMovement.rdlc";
                //}
                //else if (isArabic == "en")
                //{
                //    addpath = @"\Reports\Store\En\Movement.rdlc";
                //}
                //else
                //{
                //    addpath = @"\Reports\Store\Both\Movement.rdlc";
                //}
                //string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
                //rep.ReportPath = reppath;

                // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                //if (prInvoice.agentId != null)
                //{
                //    Agent agentinv = new Agent();
                //    //  agentinv = vendors.Where(X => X.agentId == prInvoice.agentId).FirstOrDefault();
                //    agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                //    prInvoice.agentCode = agentinv.code;
                //    //new lines
                //    prInvoice.agentName = agentinv.name;
                //    prInvoice.agentCompany = agentinv.company;
                //}
                //else
                //{

                //    prInvoice.agentCode = "-";
                //    //new lines
                //    prInvoice.agentName = "-";
                //    prInvoice.agentCompany = "-";
                //}
                User employ = new User();
                //employ = await employ.getUserById((int)prInvoice.updateUserId);
                if (FillCombo.usersAllList is null)
                { await FillCombo.RefreshAllUsers(); }
                employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                prInvoice.uuserName = employ.name;
                prInvoice.uuserLast = employ.lastname;


                Branch branchfrom = new Branch();
                Branch branchto = new Branch();
                //

                //
                //if (prInvoice.invoiceMainId == null)
                //{
                //    branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                //    prInvoice.branchCreatorName = branch.name;
                //}
                //branch creator
                if (FillCombo.branchsAllList is null)
                { await FillCombo.RefreshBranchsAll(); }

                if (prInvoice.invoiceMainId == null)
                {
                    if (prInvoice.branchId > 0)
                    {
                        //FROM
                        //  branchfrom = await branchModel.getBranchById((int)prInvoice.branchId);
                        branchfrom = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();
                        prInvoice.branchCreatorName = branchfrom.name;
                        //TO
                        Invoice secondinv = new Invoice();
                        secondinv = await invoice.getgeneratedInvoice(prInvoice.invoiceId);
                        if (secondinv.branchId != null)
                        {
                            //   branchto = await branchModel.getBranchById((int)secondinv.branchId);
                            branchto = FillCombo.branchsAllList.Where(b => b.branchId == (int)secondinv.branchId).FirstOrDefault();

                            prInvoice.branchName = branchto.name;
                        }
                        else
                        {
                            prInvoice.branchName = "-";
                        }

                    }
                    else
                    {

                    }

                }
                else
                {// NOT THE CREATOR OF ORDER
                    if (prInvoice.branchId > 0)
                    {
                        //TO
                        // branchto = await branchModel.getBranchById((int)prInvoice.branchId);
                        branchto = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();
                        prInvoice.branchName = branchto.name;
                        //FROM
                        Invoice secondinv = new Invoice();
                        secondinv = await invoice.GetByInvoiceId((int)prInvoice.invoiceMainId); ;
                        if (secondinv.branchId != null)
                        {
                            //   branchfrom = await branchModel.getBranchById((int)secondinv.branchId);
                            branchfrom = FillCombo.branchsAllList.Where(b => b.branchId == (int)secondinv.branchId).FirstOrDefault();
                            prInvoice.branchCreatorName = branchfrom.name;
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
                }
                // end branch  

                paramarr.Add(new ReportParameter("trFromBranchType", clsReports.BranchStoreConverter(branchfrom.type,"en")));
                paramarr.Add(new ReportParameter("trToBranchType", clsReports.BranchStoreConverter(branchto.type,"en")));
              if(isArabic == "both")
                {
                    paramarr.Add(new ReportParameter("trFromBranchTypeAr", clsReports.BranchStoreConverter(branchfrom.type, "both")));
                    paramarr.Add(new ReportParameter("trToBranchTypeAr", clsReports.BranchStoreConverter(branchto.type, "both")));
                }  
                foreach (var i in invoiceItems)
                {
                    i.price = decimal.Parse(SectionData.DecTostring(i.price));
                    i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
                }
               
                int itemscount = 0;
                clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                itemscount = invoiceItems.Count();
                //printer
                clsReports clsrep = new clsReports();
                reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                repsize.paperSize = repsset.paperSize;
                repsize = reportclass.GetMovementRdlcpath(prInvoice, itemscount, repsize.paperSize);
                repsize.printerName = repsset.printerName;
                //end 

               // repsize = reportclass.GetMovementRdlcpath(prInvoice, itemscount);
                string reppath = repsize.reppath;
                rep.ReportPath = reppath;
                //clsReports.purchaseInvoiceReport(invoiceItems, rep, reppath);
                clsReports.setInvoiceLanguage(paramarr);
                clsReports.InvoiceHeader(paramarr);
                paramarr = reportclass.fillMovment(prInvoice, paramarr);

                if (_ProcessType == "im" || _ProcessType == "imw" || _ProcessType == "imd")
                {

                    paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trInternalMovementImport")));
                    paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trInternalMovementImport")));

                }
                else if (_ProcessType == "ex" || _ProcessType == "exw" || _ProcessType == "exd")
                {

                    paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trInternalMovementExport")));
                    paramarr.Add(new ReportParameter("TitleAr", MainWindow.resourcemanagerAr.GetString("trInternalMovementExport")));

                }

                //if (prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb" || prInvoice.invType == "pd" || prInvoice.invType == "isd" || prInvoice.invType == "is" || prInvoice.invType == "pbw")
                //{
                //    CashTransfer cachModel = new CashTransfer();
                //    List<PayedInvclass> payedList = new List<PayedInvclass>();
                //    payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                //    decimal sump = payedList.Sum(x => x.cash).Value;
                //    decimal deservd = (decimal)prInvoice.totalNet - sump;
                //    //convertter
                //    foreach (var p in payedList)
                //    {
                //        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                //    }
                //    paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                //    paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                //    paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                //    rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));


                //}
                //  multiplePaytable(paramarr);
                List<PayedInvclass> payedList = new List<PayedInvclass>();
                rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));
                rep.SetParameters(paramarr);
                rep.Refresh();
            }
            return repsize;
        }
        #endregion
        private void Cbm_unitItemDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                var cmb = sender as ComboBox;

                if (dg_billDetails.SelectedIndex != -1 && cmb != null)
                {
                    var iu = (ItemUnit)cmb.SelectedItem;
                    billDetails[dg_billDetails.SelectedIndex].Unit = iu.mainUnit;
                    billDetails[dg_billDetails.SelectedIndex].itemUnitId = (int)cmb.SelectedValue;
                   
                    if (_ProcessType == "exw" || _ProcessType == "exd")
                    {
                        billDetails[dg_billDetails.SelectedIndex].itemSerials = new List<Serial>();
                        billDetails[dg_billDetails.SelectedIndex].StoreProperties = new List<StoreProperty>();

                        if (billDetails[dg_billDetails.SelectedIndex].type == "sn")
                            billDetails[dg_billDetails.SelectedIndex].valid = false;
                        else if (billDetails[dg_billDetails.SelectedIndex].type.Equals("p"))
                        {
                            long packageCount = (long)billDetails[dg_billDetails.SelectedIndex].Count;
 
                            foreach (var p in billDetails[dg_billDetails.SelectedIndex].packageItems)
                            {
                                if (p.type.Equals("sn"))
                                {
                                    billDetails[dg_billDetails.SelectedIndex].valid = false;
                                    break;
                                }
                            }

                        }
                    }
                    refrishBillDetails();
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
                                combo.SelectedValue = (int)item.itemUnitId;

                                #region disable & enable unit comboBox
                                if (_ProcessType == "ex" || _ProcessType == "im" || _ProcessType == "exw" || _ProcessType == "imw")
                                    combo.IsEnabled = false;
                                else
                                    combo.IsEnabled = true;
                                #endregion
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
        private void Cbm_unitItemDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var cmb = sender as ComboBox;
                //billDetails
                if (billDetails.Count == 1)
                {

                    cmb.SelectedValue = (int)billDetails[0].itemUnitId;
                }
                #region disable & enable unit comboBox
                if (_ProcessType == "ex" || _ProcessType == "im" || _ProcessType == "exw" || _ProcessType == "imw")
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
        private void Dg_billDetails_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            int column = dg_billDetails.CurrentCell.Column.DisplayIndex;
            if ((_ProcessType == "ex" || _ProcessType == "im" || _ProcessType == "exw" || _ProcessType == "imw")
                && column == 3)
                e.Cancel = true;
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
                clearProcess();
                invoice = invoices[index];
                _ProcessType = invoice.invType;
                _invoiceId = invoice.invoiceId;
                navigateBtnActivate();
                await fillOrderInputs(invoice);

                #region set title according to invoice type
                if (_ProcessType == "im" || _ProcessType == "imw" || _ProcessType == "imd")
                {
                    txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementImport");
                }
                else if (_ProcessType == "ex" || _ProcessType == "exw" || _ProcessType == "exd")
                {
                    txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trInternalMovementExport");

                }
                #endregion
            }
            catch //(Exception ex)
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
                //invoice = invoices[index];
                //_ProcessType = invoice.invType;
                //_invoiceId = invoice.invoiceId;
                //navigateBtnActivate();
                //await fillOrderInputs(invoice);
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

        private async void Btn_shortageInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (invoice.invoiceId != 0)
                    clearProcess();

                _isLack = true;
                cb_processType.SelectedIndex = 0;
                cb_processType.IsEnabled = false;
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
            invoiceItems = await invoice.getShortageItems(MainWindow.branchID.Value);
            _Count = invoiceItems.Count;
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
                    Price = decimal.Parse(SectionData.DecTostring((decimal)itemT.price)),
                    Total = total,
                });
            }

            tb_barcode.Focus();

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
                item.unitName = row.Unit;

                Window.GetWindow(this).Opacity = 0.2;
                wd_serialNum w = new wd_serialNum();
                w.sourceUserControls = FillCombo.UserControls.itemsExport;
                w.item = item;
                w.itemCount = row.Count;
                w.invType = _ProcessType;
                w.itemsSerials = row.itemSerials;
                w.StoreProperties = row.StoreProperties;
                w.valid = row.valid;


                w.ShowDialog();
                if (w.isOk == true)
                {
                    row.itemSerials = w.itemsSerials;
                    row.StoreProperties = w.StoreProperties;
                    row.valid = w.valid;
                    refrishBillDetails();
                }
                //else if (item.type == "sn")
                else 
                {
                    row.itemSerials = w.itemsSerials;
                    row.StoreProperties = w.StoreProperties;

                    #region valid icon
                    bool isValid = true;

                    if (_ProcessType == "exw" || _ProcessType == "exd")
                    {
                        if (item.type == "sn" && row.itemSerials.Count() < row.Count)
                            isValid = false;
                        else if (item.type.Equals("p"))
                        {
                            long packageCount = (long)row.Count;
                            long serialNum = 0;
                            foreach (var p in item.packageItems)
                            {
                                if (p.type.Equals("sn"))
                                    serialNum += (long)p.itemCount * packageCount;
                            }

                            if (row.itemSerials.Count() < serialNum)
                                isValid = false;
                        }
                    }
                    #endregion

                    row.valid = isValid;
                    refrishBillDetails();
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

        private async void Btn_deleteInventory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(deletePermission, MainWindow.groupObjects, "one"))
                {
                    #region confirm
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                    w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                    #endregion

                    if (w.isOk)
                    {
                        var res = await invoice.deleteMovment(invoice.invoiceId, MainWindow.userID.Value);
                        if (res.Result > 0)
                        {

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);

                            clearProcess();
                            setNotifications();
                        }
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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

        private void Cb_processType_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Cb_branch_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;


                List<Branch> branches = new List<Branch>();
                Branch branchModel = new Branch();
                if (SectionData.isAdminPermision())
                    branches = SectionData.branchsAllList.ToList();
                else
                    branches = SectionData.BranchesByBranchandUserList.ToList();

                branchModel = branches.Where(s => s.branchId == MainWindow.branchID.Value).FirstOrDefault<Branch>();
                branches.Remove(branchModel);
                branches = branches.Where(b => b.type != "" && b.branchId != 1).ToList();
                combo.ItemsSource = branches.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
