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
    /// Interaction logic for uc_receiptOfPurchaseInvoice.xaml
    /// </summary>
    public partial class uc_receiptOfPurchaseInvoice : UserControl
    {
        string reciptPermission = "reciptOfInvoice_recipt";
        string returnPermission = "reciptOfInvoice_return";
        string reportsPermission = "reciptOfInvoice_reports";
        string inputsPermission = "reciptOfInvoice_inputs";
        string printCountPermission = "reciptOfInvoice_printCount";

        private static uc_receiptOfPurchaseInvoice _instance;
        public static uc_receiptOfPurchaseInvoice Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_receiptOfPurchaseInvoice();
                return _instance;
            }
        }
        public uc_receiptOfPurchaseInvoice()
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
        Category categoryModel = new Category();
        Category category = new Category();
        Item itemModel = new Item();
        Item item = new Item();
        IEnumerable<Item> items;
        ItemUnit itemUnitModel = new ItemUnit();
        List<ItemUnit> barcodesList;
        List<ItemUnit> itemUnits;
        public List<Control> controls;

        Invoice invoiceModel = new Invoice();
        public Invoice invoice = new Invoice();
        List<Invoice> invoices;
        List<ItemTransfer> invoiceItems;
        List<ItemTransfer> mainInvoiceItems;
        ItemLocation itemLocationModel = new ItemLocation();
        bool _IsFocused = false;

        Agent agentModel = new Agent();
        IEnumerable<Agent> vendors;
        int _InvoiceCount = 0;
        int _DraftCount = 0;
        int _ReturnCount = 0;
        int prInvoiceId;
        //Branch branchModel = new Branch();
        // IEnumerable<Branch> branches;
        // for barcode
        DateTime _lastKeystroke = new DateTime(0);
        static private string _BarcodeStr = "";
        static private object _Sender;
        //for bill details
        static private int _SequenceNum = 0;
        static private int _invoiceId;
        static public string _InvoiceType = "isd"; // immidiatlly in storage draft
        static private decimal _Sum = 0;
        static private decimal _Count = 0;
        //tglItemState
        private static DispatcherTimer timer;
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
            ///
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcodeHint"));
            txt_printCount.Text = MainWindow.resourcemanager.GetString("trAdditional");
            txt_printInvoice.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_items.Text = MainWindow.resourcemanager.GetString("trItems");
            txt_drafts.Text = MainWindow.resourcemanager.GetString("trDrafts");
            txt_newDraft.Text = MainWindow.resourcemanager.GetString("trNew");
            txt_count.Text = MainWindow.resourcemanager.GetString("trCount");
            txt_pdf.Text = MainWindow.resourcemanager.GetString("trPdf");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            dg_billDetails.Columns[1].Header = MainWindow.resourcemanager.GetString("trNo.");
            dg_billDetails.Columns[2].Header = MainWindow.resourcemanager.GetString("trItem");
            dg_billDetails.Columns[3].Header = MainWindow.resourcemanager.GetString("trUnit");
            dg_billDetails.Columns[4].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_billDetails.Columns[5].Header = MainWindow.resourcemanager.GetString("trPrice");
            dg_billDetails.Columns[6].Header = MainWindow.resourcemanager.GetString("trTotal");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trStore/BranchHint"));

            txt_store.Text = MainWindow.resourcemanager.GetString("trBranch/Store");
            txt_invoices.Text = MainWindow.resourcemanager.GetString("trInvoices");
            txt_sum.Text = MainWindow.resourcemanager.GetString("trSum");
            txt_returnInvoice.Text = MainWindow.resourcemanager.GetString("trPurchases");
            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trDirectEntry");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            tt_error_previous.Content = MainWindow.resourcemanager.GetString("trPrevious");
            tt_error_next.Content = MainWindow.resourcemanager.GetString("trNext");
          
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
        async void loading_RefrishVendors()
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
                if (item.key.Equals("loading_RefrishVendors"))
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
                await SectionData.fillBranches(cb_branch, "bs");

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
        bool loadingSuccess_globalSaleUnits = false;
        async void loading_globalSaleUnits()
        {
            try
            {
                #region global Sale Units
              MainWindow.InvoiceGlobalItemUnitsList = await itemUnitModel.Getall();

                if (MainWindow.InvoiceGlobalItemUnitsList is null)
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

        public async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                MainWindow.mainWindow.KeyDown += HandleKeyPress;
               tb_moneyIcon.Text = AppSettings.Currency;
                tb_moneyIconTotal.Text = AppSettings.Currency;
                controls = new List<Control>();

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
               
                setNotifications();
                setTimer();
               
                #region hid - display tax inputs
                if (MainWindow.tax == 0)
                    sp_tax.Visibility = Visibility.Collapsed;
                else
                    sp_tax.Visibility = Visibility.Visible;
                #endregion
                #region loading
                loadingList = new List<keyValueBool>();
                bool isDone = true;
                loadingList.Add(new keyValueBool { key = "loading_RefrishItems", value = false });
                loadingList.Add(new keyValueBool { key = "loading_RefrishVendors", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillBarcodeList", value = false });
                loadingList.Add(new keyValueBool { key = "loading_fillBranches", value = false });
                loadingList.Add(new keyValueBool { key = "loading_globalSaleUnits", value = false });

                loading_RefrishItems();
                loading_RefrishVendors();
                loading_fillBarcodeList();
                loading_fillBranches();
                loading_globalSaleUnits();
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

                FindControl(this.grid_main, controls);

                #region datagridChange
                //CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(dg_billDetails.Items);
                //((INotifyCollectionChanged)myCollectionView).CollectionChanged += new NotifyCollectionChangedEventHandler(DataGrid_CollectionChanged);
                #endregion


                #region Permision
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

                if (MainWindow.groupObject.HasPermissionAction(reciptPermission, MainWindow.groupObjects, "one"))
                    md_invoiceCount.Visibility = Visibility.Visible;
                else
                    md_invoiceCount.Visibility = Visibility.Collapsed;

                if (MainWindow.groupObject.HasPermissionAction(returnPermission, MainWindow.groupObjects, "one"))
                    md_returnsCount.Visibility = Visibility.Visible;
                else
                    md_returnsCount.Visibility = Visibility.Collapsed;

                if (MainWindow.groupObject.HasPermissionAction(inputsPermission, MainWindow.groupObjects, "one"))
                {
                    md_draft.Visibility = Visibility.Visible;
                    btn_newDraft.Visibility = Visibility.Visible;
                    btn_items.IsEnabled = true;
                }
                else
                {
                    md_draft.Visibility = Visibility.Collapsed;
                    btn_newDraft.Visibility = Visibility.Collapsed;
                    btn_items.IsEnabled = false;
                }

                if (SectionData.isAdminPermision())
                {
                    txt_branch.Visibility = Visibility.Collapsed;
                    gd_branch.Visibility = Visibility.Visible;
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
        #region notifications
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
                if (sendert != null)
                    SectionData.StartAwait(grid_main);
                refreshInvoiceNotification();
                refreshInvoiceReturnNotification();
                if (sendert != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sendert != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void setNotifications()
        {
            try
            {

                refreshDraftNotification();
                refreshInvoiceNotification();
                refreshInvoiceReturnNotification();

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
                if (AppSettings.DirectStorageDraftCount <= 0)
                {
                    string invoiceType = "isd";
                    int duration = 2;
                     AppSettings.DirectStorageDraftCount = (int)await invoice.GetCountByCreator(invoiceType, MainWindow.userID.Value, duration);
                    AppSettings.DirectStorageDraftCount = AppSettings.DirectStorageDraftCount < 0 ? 0 : AppSettings.DirectStorageDraftCount;
                }
                int draftCount = AppSettings.DirectStorageDraftCount;
                if (draftCount > 0 && _InvoiceType == "isd" && invoice.invoiceId != 0 && invoice != null && !isFromReport)
                    draftCount--;

                SectionData.refreshNotification(md_draft, ref _DraftCount, draftCount);
               
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async void refreshInvoiceNotification()
        {
            try
            {
                string invoiceType = "p, pb,is";
                int duration = 1;

                if (invoice == null)
                    invoice = new Invoice();
                int invoiceCount = (int)await invoice.GetCountBranchInvoices(invoiceType, 0, MainWindow.branchID.Value,duration);
                if ((invoice.invType == "pb" || invoice.invType == "p" || invoice.invType == "is") && invoice != null)
                    invoiceCount--;

                if (invoiceCount != _InvoiceCount)
                {
                    if (invoiceCount > 9)
                    {
                        md_invoiceCount.Badge = "+9";
                    }
                    else if (invoiceCount == 0) md_invoiceCount.Badge = "";
                    else
                        md_invoiceCount.Badge = invoiceCount.ToString();
                }
                _InvoiceCount = invoiceCount;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async void refreshInvoiceReturnNotification()
        {
            try
            {
                string invoiceType = "pw,pbw";
                if (invoice == null)
                    invoice = new Invoice();
                int returnsCount = (int)await invoice.GetCountBranchInvoices(invoiceType, 0, MainWindow.branchID.Value);
                if ((invoice.invType == "pbw" || invoice.invType == "pw") && invoice != null)
                    returnsCount--;

                SectionData.refreshNotification(md_returnsCount, ref _ReturnCount, returnsCount);
      
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }

        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.KeyDown -= HandleKeyPress;
                timer.Stop();
                saveBeforeExit();

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch //(Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
        }
        private async Task saveBeforeExit()
        {
            if (billDetails.Count > 0 && _InvoiceType == "isd")
            {
                #region Accept
                MainWindow.mainWindow.Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                //w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                w.contentText = MainWindow.resourcemanager.GetString("trSaveInvoiceNotification");
                w.ShowDialog();
                MainWindow.mainWindow.Opacity = 1;
                #endregion
                if (w.isOk)
                {
                   InvoiceResult invoiceResult = await addInvoice("isd");
                    clearInvoice();
                    _InvoiceType = "isd";
                }
            }
            clearInvoice();

        }
        #region bill

        public class BillDetails
        {
            public int ID { get; set; }
            public int itemId { get; set; }
            public int itemUnitId { get; set; }
            public string Product { get; set; }
            public string Unit { get; set; }
            public string UnitName { get; set; }
            public int Count { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
            public int OrderId { get; set; }
            public List<StoreProperty> ItemProperties { get; set; }
            public List<Serial> itemSerials { get; set; }
            public List<StoreProperty> StoreProperties { get; set; }
            public List<Item> packageItems { get; set; }

            public bool valid { get; set; }
            public string type { get; set; }
        }
        #endregion
        async Task RefrishItems()
        {
            //items = await itemModel.GetAllItems();
            items = await itemModel.GetSaleOrPurItems(0, 0, 1, MainWindow.branchID.Value);
        }
        async Task RefrishVendors()
        {
            if (FillCombo.vendorsList is null)
                await FillCombo.RefreshVendors();
            //vendors = await agentModel.GetAgentsActive("v");
            vendors = FillCombo.vendorsList.ToList();
        }
        async Task fillBarcodeList()
        {
            barcodesList = await itemUnitModel.getAllBarcodes();
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

        #region barcode
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
                    }
                }

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                {
                    switch (e.Key)
                    {
                        case Key.P:
                            //handle D key
                            //btn_printInvoice_Click(null, null);
                            break;
                        case Key.S:
                            //handle X key
                            Btn_save_Click(null, null);
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
                    tb_barcode.Text = _BarcodeStr;
                    _BarcodeStr = "";
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
                case "is":// this barcode for invoice
                    clearInvoice();
                    invoice = await invoiceModel.GetInvoicesByNum(barcode, MainWindow.branchID.Value);
                    // _InvoiceType = invoice.invType;
                    await fillInvoiceInputs(invoice);
                    break;
                default: // if barcode for item
                         // get item matches barcode
                    if (barcodesList != null)
                    {
                        ItemUnit unit1 = barcodesList.ToList().Find(c => c.barcode == barcode.Trim());

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
                                    decimal price = (decimal)unit1.cost;
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

                        // remove item from bill
                        billDetails.RemoveAt(index);

                        ObservableCollection<BillDetails> data = (ObservableCollection<BillDetails>)dg_billDetails.ItemsSource;
                        data.Remove(row);
                    }
                _SequenceNum = 0;
                _Sum = 0;
                for (int i = 0; i < billDetails.Count; i++)
                {
                    _SequenceNum++;
                    billDetails[i].ID = _SequenceNum;
                    _Sum += billDetails[i].Total;
                    _Count = _SequenceNum;
                }
                // calculate new total
                refreshTotalValue();
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion



        private async void Btn_returnInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(returnPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    wd_invoice w = new wd_invoice();

                    // sale invoices
                    string invoiceType = "pbw, pw";
                    w.invoiceType = invoiceType;
                    w.invoiceStatus = "return";
                    w.branchId = MainWindow.branchID.Value;

                    w.title = MainWindow.resourcemanager.GetString("trReturnInvoices");

                    if (w.ShowDialog() == true)
                    {
                        btn_items.IsEnabled = false;
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;
                            _invoiceId = invoice.invoiceId;
                            _InvoiceType = invoice.invType;
                            isFromReport = false;
                            setNotifications();
                            #region set title to bill
                            if (_InvoiceType == "pbw")
                            {
                                txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                                btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                                // orange #FFA926 red #D22A17
                                txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                            }
                            else if(_InvoiceType == "pw")
                            {
                                txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                                txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                                btn_save.Content = MainWindow.resourcemanager.GetString("trStoreBtn");
                            }
                            #endregion
                            await fillInvoiceInputs(invoice);
                            mainInvoiceItems = invoiceItems;
                            invoices = FillCombo.invoices;

                           // invoices = await invoice.getBranchInvoices(invoiceType, 0, MainWindow.branchID.Value);
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
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void clearInvoice()
        {

            _SequenceNum = 0;
            _Count = 0;
            _Sum = 0;
            // _InvoiceType = "pbw";
            _InvoiceType = "isd";
            invoice = new Invoice();
            txt_branch.Text = "_____________";
            txt_invNumber.Text = "";
            billDetails.Clear();
            tb_count.Text = "0";
            tb_total.Text = "0";
            tb_sum.Text = "0";
            cb_branch.SelectedIndex = -1;
            btn_items.IsEnabled = true;
            isFromReport = false;
            txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trDirectEntry");
            txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
            refrishBillDetails();
            inputEditable();
            if (!isFromReport)
            {
                btn_next.Visibility = Visibility.Collapsed;
                btn_previous.Visibility = Visibility.Collapsed;
            }
        }
        /*
        private void Cbm_unitItemDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cmb = sender as ComboBox;

                if (dg_billDetails.SelectedIndex != -1 && cmb != null)
                    billDetails[dg_billDetails.SelectedIndex].itemUnitId = (int)cmb.SelectedValue;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        /*
        private void Cbm_unitItemDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                //billDetails
                if (billDetails.Count == 1)
                {
                    var cmb = sender as ComboBox;
                    cmb.SelectedValue = (int)billDetails[0].itemUnitId;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        /*
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
        */
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
        private async void Btn_newDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (billDetails.Count > 0 && _InvoiceType == "isd")
                {

                    #region Accept
                    MainWindow.mainWindow.Opacity = 0.2;
                    wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                    w.contentText = MainWindow.resourcemanager.GetString("trSaveInvoiceNotification");
                    w.ShowDialog();
                    MainWindow.mainWindow.Opacity = 1;
                    #endregion
                    if (w.isOk)
                    {
                        InvoiceResult invoiceResult = await addInvoice(_InvoiceType);
                        if (invoiceResult.Result > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            invoice.invoiceId = invoiceResult.Result;
                            invoice.invNumber = invoiceResult.Message;
                            invoice.updateDate = invoiceResult.UpdateDate;
                            TimeSpan ts;
                            TimeSpan.TryParse(invoiceResult.InvTime, out ts);
                            invoice.invTime = ts;

                            AppSettings.DirectStorageDraftCount = invoiceResult.PurchaseDraftCount;

                        }
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                    clearInvoice();
                    refreshDraftNotification();
                }
                else
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
        private async void Btn_draft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Window.GetWindow(this).Opacity = 0.2;
                wd_invoice w = new wd_invoice();
                string invoiceType = "isd";
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
                        txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trDirectEntryDraft");
                        await fillInvoiceInputs(invoice);
                        setNotifications();
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
        private void refreshTotalValue()
        {
            decimal total = _Sum;
            decimal taxValue = 0;

            if (_Sum != 0)
                tb_sum.Text = SectionData.DecTostring(_Sum);
            else
                tb_sum.Text = "0";
            total = total + taxValue;

            if (total != 0)
                tb_total.Text = SectionData.DecTostring(total);
            else tb_total.Text = "0";
            tb_count.Text = _SequenceNum.ToString();

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
      
        #region Get Id By Click  Y

        public void ChangeCategoryIdEvent(int categoryId)
        {

        }

        public async Task ChangeItemIdEvent(int itemId)
        {
            try
            {
                SectionData.StartAwait(grid_main);
                item = items.ToList().Find(c => c.itemId == itemId);

                if (item != null)
                {
                    this.DataContext = item;

                    // get item units
                    //itemUnits = await itemUnitModel.GetItemUnits(item.itemId);
                    itemUnits = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemId == item.itemId).ToList();
                    // search for default unit for purchase
                    var defaultPurUnit = itemUnits.ToList().Find(c => c.defaultPurchase == 1);
                    if (defaultPurUnit != null)
                    {
                        int index = billDetails.IndexOf(billDetails.Where(p => p.itemUnitId == defaultPurUnit.itemUnitId && p.OrderId == 0).FirstOrDefault());
                        if (index == -1)//item doesn't exist in bill
                        {
                            // create new row in bill details data grid
                            decimal price = (decimal)defaultPurUnit.cost; //?????
                            decimal total = price;
                            addRowToBill(item.name, itemId, defaultPurUnit.mainUnit, defaultPurUnit.itemUnitId, 1, price, total);
                        }
                        else // item exist prevoiusly in list
                        {
                            billDetails[index].Count++;
                            billDetails[index].Total = billDetails[index].Count * billDetails[index].Price;

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
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
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
                ItemProperties = item.ItemProperties,
            });
            _Sum += total;
        }

        #endregion

        private async void Btn_invoices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(reciptPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    int duration = 1;
                    wd_invoice w = new wd_invoice();

                    // sale invoices
                    string invoiceType = "p , pb, is";
                    w.invoiceType = invoiceType;
                    w.branchId = MainWindow.branchID.Value;
                    w.duration = duration;
                    w.title = MainWindow.resourcemanager.GetString("trInvoices");

                    if (w.ShowDialog() == true)
                    {
                        btn_items.IsEnabled = false;
                        if (w.invoice != null)
                        {
                            invoice = w.invoice;

                            _InvoiceType = invoice.invType;
                            _invoiceId = invoice.invoiceId;
                            isFromReport = false;
                            setNotifications();
                            #region set title to bill
                            if (_InvoiceType == "p")
                            {
                                txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                                txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                                btn_save.Content = MainWindow.resourcemanager.GetString("trStoreBtn");
                            }
                            else if(_InvoiceType == "pb")
                            {
                                txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                                btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                                // orange #FFA926 red #D22A17
                                txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                            }
                            else if(_InvoiceType == "is")
                            {
                                txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trDirectEntry");
                                txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                                btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
                            }
                            #endregion
                            await fillInvoiceInputs(invoice);
                        invoices = FillCombo.invoices;
                           // invoices = await invoice.getBranchInvoices(invoiceType, 0, MainWindow.branchID.Value,duration);
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
            txt_branch.Text = invoice.branchName;
            cb_branch.SelectedValue = invoice.branchId;
            txt_invNumber.Text = invoice.invNumber;
            if (invoice.total != null)
                _Sum = (decimal)invoice.total;
            else
                _Sum = 0;
            // build invoice details grid
            await buildInvoiceDetails(invoice.invoiceId);
            refreshTotalValue();
            inputEditable();
        }
        private async Task buildInvoiceDetails(int invoiceId)
        {
            if (invoice.invoiceItems == null)
                invoiceItems = await invoiceModel.GetInvoicesItems(invoice.invoiceId);
            else
                invoiceItems = invoice.invoiceItems;
            // build invoice details grid
            _SequenceNum = 0;
            billDetails.Clear();
            _Count = 0;
            foreach (ItemTransfer itemT in invoiceItems)
            {
                _SequenceNum++;
                _Count += (int)itemT.quantity;
                decimal total = (decimal)(itemT.price * itemT.quantity);

                #region valid item serials
                bool isValid = true;

                if (_InvoiceType == "is" || _InvoiceType == "isd")
                {
                    if (itemT.itemType == "sn" && itemT.itemSerials.Count() < itemT.quantity)
                        isValid = false;                   
                }
                #endregion
                var unit = MainWindow.InvoiceGlobalItemUnitsList.Where(a => a.itemUnitId == itemT.itemUnitId).FirstOrDefault();

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
                    UnitName = itemT.unitName,
                    itemSerials = itemT.itemSerials,
                    packageItems = itemT.packageItems,
                    type = itemT.itemType,
                    valid = isValid,
                    ItemProperties = unit.ItemProperties,
                    StoreProperties = itemT.ItemStoreProperties,
                });
            }

            tb_count.Text = _Count.ToString();
            tb_barcode.Focus();

            refrishBillDetails();
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
        }
        void refrishDataGridItems()
        {
            dg_billDetails.ItemsSource = null;
            dg_billDetails.ItemsSource = billDetails;
            dg_billDetails.Items.Refresh();
            DataGrid_CollectionChanged(dg_billDetails, null);

        }
        private void inputEditable()
        {
            switch (_InvoiceType)
            {
                case "pw":// wait purchase invoice
                case "pbw":
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column unvisible
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count read only
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price read only

                    sp_sum.Visibility = Visibility.Collapsed;
                    tb_sum.Visibility = Visibility.Collapsed;
                    txt_sum.Visibility = Visibility.Collapsed;
                    tb_moneyIcon.Visibility = Visibility.Collapsed;
                    txt_total.Visibility = Visibility.Collapsed;
                    tb_total.Visibility = Visibility.Collapsed;
                    tb_moneyIconTotal.Visibility = Visibility.Collapsed;

                    if (SectionData.isAdminPermision())
                    {
                        txt_branch.Visibility = Visibility.Visible;
                        gd_branch.Visibility = Visibility.Collapsed;
                    }
                    btn_save.IsEnabled = true;

                    break;
             
                case "isd":
                    dg_billDetails.Columns[0].Visibility = Visibility.Visible;
                    dg_billDetails.Columns[4].IsReadOnly = false; //make count editable
                    dg_billDetails.Columns[5].IsReadOnly = false; //make price read only

                    sp_sum.Visibility = Visibility.Visible;
                    tb_sum.Visibility = Visibility.Visible;
                    txt_sum.Visibility = Visibility.Visible;
                    tb_moneyIcon.Visibility = Visibility.Visible;
                    txt_total.Visibility = Visibility.Visible;
                    tb_total.Visibility = Visibility.Visible;
                    tb_moneyIconTotal.Visibility = Visibility.Visible;

                    if (SectionData.isAdminPermision())
                    {
                        txt_branch.Visibility = Visibility.Collapsed;
                        gd_branch.Visibility = Visibility.Visible;
                        cb_branch.IsEnabled = true;
                    }
                    btn_save.IsEnabled = true;

                    break;
                case "p":
                case "pb":
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed; //make delete column visible
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count editable
                    dg_billDetails.Columns[5].IsReadOnly = true; //make count editable

                    sp_sum.Visibility = Visibility.Collapsed;
                    tb_sum.Visibility = Visibility.Collapsed;
                    txt_sum.Visibility = Visibility.Collapsed;
                    tb_moneyIcon.Visibility = Visibility.Collapsed;
                    txt_total.Visibility = Visibility.Collapsed;
                    tb_total.Visibility = Visibility.Collapsed;
                    tb_moneyIconTotal.Visibility = Visibility.Collapsed;

                    if (SectionData.isAdminPermision())
                    {
                        txt_branch.Visibility = Visibility.Visible;
                        gd_branch.Visibility = Visibility.Collapsed;
                    }
                    btn_save.IsEnabled = false;
                    break;
                case "is":
                    dg_billDetails.Columns[0].Visibility = Visibility.Collapsed;
                    dg_billDetails.Columns[4].IsReadOnly = true; //make count editable
                    dg_billDetails.Columns[5].IsReadOnly = true; //make price read only

                    sp_sum.Visibility = Visibility.Visible;
                    tb_sum.Visibility = Visibility.Visible;
                    txt_sum.Visibility = Visibility.Visible;
                    tb_moneyIcon.Visibility = Visibility.Visible;
                    txt_total.Visibility = Visibility.Visible;
                    tb_total.Visibility = Visibility.Visible;
                    tb_moneyIconTotal.Visibility = Visibility.Visible;

                    if (SectionData.isAdminPermision())
                    {
                        txt_branch.Visibility = Visibility.Visible;
                        gd_branch.Visibility = Visibility.Collapsed;
                        cb_branch.IsEnabled = false;
                    }

                    btn_save.IsEnabled = false;
                    break;
        }
            if (!isFromReport)
            {
                btn_next.Visibility = Visibility.Visible;
                btn_previous.Visibility = Visibility.Visible;
            }
        }


        #region data grid pay invoice
        private void Cbm_unitItemDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                //billDetails
                var cmb = sender as ComboBox;
                cmb.SelectedValue = (int)billDetails[0].itemUnitId;

                if (_InvoiceType == "p" || _InvoiceType == "pw" || _InvoiceType == "pb" ||
                    _InvoiceType == "pbw"|| _InvoiceType == "is")
                    cmb.IsEnabled = false;
                else
                    cmb.IsEnabled = true;

                //if (billDetails[0].OrderId != 0)
                //    cmb.IsEnabled = false;
                //else
                //    cmb.IsEnabled = true;

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
                    billDetails[dg_billDetails.SelectedIndex].StoreProperties = new List<StoreProperty>();
                  
                    #region valid serial icon
                    if (_InvoiceType == "isd")
                    {
                        billDetails[dg_billDetails.SelectedIndex].itemSerials = new List<Serial>();
                        if (billDetails[dg_billDetails.SelectedIndex].type == "sn")
                            billDetails[dg_billDetails.SelectedIndex].valid = false;                       
                    }
                    #endregion

                    #region change price of unit

                    TextBlock tb;

                    int _datagridSelectedIndex = dg_billDetails.SelectedIndex;
                    int itemUnitId = (int)cmb.SelectedValue;
                    billDetails[_datagridSelectedIndex].itemUnitId = (int)cmb.SelectedValue;

                    dynamic unit;
                    if (MainWindow.InvoiceGlobalItemUnitsList == null)
                    {
                        unit = new Item();
                        unit = barcodesList.ToList().Find(x => x.itemUnitId == (int)cmb.SelectedValue && x.itemId == billDetails[_datagridSelectedIndex].itemId);
                    }
                    else
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
                    tb = dg_billDetails.Columns[4].GetCellContent(dg_billDetails.Items[_datagridSelectedIndex]) as TextBlock;

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

                    #region update unit properties
                    billDetails[dg_billDetails.SelectedIndex].ItemProperties = unit.ItemProperties;
                    #endregion
                    refreshTotalValue();

                    // update item in billdetails           
                    billDetails[_datagridSelectedIndex].Count = (int)newCount;
                    billDetails[_datagridSelectedIndex].Price = newPrice;
                    billDetails[_datagridSelectedIndex].Total = total;
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

                                if (_InvoiceType == "p" || _InvoiceType == "pw" || _InvoiceType == "pb" ||
                   _InvoiceType == "pbw" || _InvoiceType == "is")
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
            if (dg_billDetails.SelectedIndex != -1 && column == 3)
               if (_InvoiceType == "p" || _InvoiceType == "pw" || _InvoiceType == "pb" ||
                   _InvoiceType == "pbw" || _InvoiceType == "is")
                        e.Cancel = true;
        }
        private void Dg_billDetails_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                    if (columnName == MainWindow.resourcemanager.GetString("trQTR"))
                        t.Text = billDetails[index].Count.ToString();
                    else if (columnName == MainWindow.resourcemanager.GetString("trPrice"))
                        t.Text = SectionData.DecTostring(billDetails[index].Price);

                }
                else
                {
                    int oldCount = 0;
                    long newCount = 0;
                    decimal newPrice = 0;
                    decimal oldPrice = 0;
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

                    if (_InvoiceType == "pbw")
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
                    tb_count.Text = _Count.ToString();


                    if (columnName == MainWindow.resourcemanager.GetString("trPrice") && !t.Text.Equals(""))
                        newPrice = decimal.Parse(t.Text);
                    else
                        newPrice = row.Price;

                    oldPrice = row.Price;

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
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion





        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (billDetails.Count > 0)
                {
                    InvoiceResult invoiceResult = new InvoiceResult();
                    if (_InvoiceType == "pw") //p  wait purchase invoice
                    {
                        invoiceResult = await receiptInvoice();
                    }
                    else if (_InvoiceType == "pbw")
                    {
                        invoiceResult = await returnInvoice("pb");

                    }
                    else if (_InvoiceType == "isd" || _InvoiceType == "is")
                    {

                        invoiceResult = await addInvoice("is");
                        #region old
                        //#region notification Object
                        //Notification not = new Notification()
                        //{
                        //    title = "trExceedMaxLimitAlertTilte",
                        //    ncontent = "trExceedMaxLimitAlertContent",
                        //    msgType = "alert",
                        //    createUserId = MainWindow.userID.Value,
                        //    updateUserId = MainWindow.userID.Value,
                        //};
                        //#endregion
                        //await itemLocationModel.recieptInvoice(invoiceItems, MainWindow.branchID.Value, MainWindow.userID.Value, "storageAlerts_minMaxItem", not); // increase item quantity in DB
                        //if (_InvoiceType == "is")
                        //invoiceModel.saveAvgPurchasePrice(invoiceItems);

                        //clearInvoice();
                        //refreshDraftNotification();
                        #endregion

                    }
                    if (invoiceResult != null)
                    {
                        if (invoiceResult.Result > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            invoice.invoiceId = invoiceResult.Result;
                            invoice.invNumber = invoiceResult.Message;
                            invoice.updateDate = invoiceResult.UpdateDate;
                            TimeSpan ts;
                            TimeSpan.TryParse(invoiceResult.InvTime, out ts);
                            invoice.invTime = ts;

                            AppSettings.DirectStorageDraftCount = invoiceResult.PurchaseDraftCount;

                            prInvoice = invoice;
                            prInvoiceId = invoice.invoiceId;
                            clearInvoice();

                            refreshDraftNotification();
                            refreshInvoiceReturnNotification();
                            refreshInvoiceNotification();
                            #region print 
                            string invtype = prInvoice.invType;
                            if (invtype == "is")
                            {
                                Thread t = new Thread(async () =>
                                {
                                    string msg = "";
                                    if (AppSettings.print_on_save_directentry == "1")
                                    {
                                    //List<PayedInvclass> payedlist = new List<PayedInvclass>();
                                    //payedlist = await cashTransfer.PayedBycashlist(listPayments);
                                    msg = await printPurInvoice(prInvoice, invoiceItems);
                                    //printPurInvoice(prInvoiceId);
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
                                    }

                                });
                                t.Start();

                            }
                            #endregion

                        }
                        else if (invoiceResult.Result == -3) // كمية العنصر غير كافية
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip") + " " + invoiceResult.Message, animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
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
        private async Task<InvoiceResult> addInvoice(string invType)
        {
            InvoiceResult invoiceResult = new InvoiceResult();
            int branchId = 0;

            #region invoice object
            if (invType == "isd" && invoice.invoiceId == 0)
                invoice.invNumber = "isd";
            //invoice.invNumber = await invoice.generateInvNumber("isd", MainWindow.loginBranch.code, MainWindow.branchID.Value);
            else if (invType == "is")
                invoice.invNumber = "is";
                //invoice.invNumber = await invoice.generateInvNumber("is", MainWindow.loginBranch.code, MainWindow.branchID.Value);


            invoice.branchCreatorId = MainWindow.branchID.Value;
            invoice.branchId = MainWindow.branchID.Value;
            invoice.posId = MainWindow.posID.Value;
            invoice.invType = invType;

            invoice.total = _Sum;
            invoice.totalNet = decimal.Parse(tb_total.Text);
            invoice.paid = 0;
            invoice.deserved = invoice.totalNet;
            invoice.createUserId = MainWindow.userID.Value;
            invoice.updateUserId = MainWindow.userID.Value;
            invoice.isOrginal = true;
            invoice.cashReturn = 0;
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
                itemT.itemName = billDetails[i].Product;
                itemT.unitName = billDetails[i].Unit;
                itemT.itemSerials = billDetails[i].itemSerials;
                itemT.ItemStoreProperties = billDetails[i].StoreProperties;
                invoiceItems.Add(itemT);
            }
            #endregion

            switch (invType)
            {
                case "is":
                    #region notification Object
                    Notification not = new Notification()
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

                    if (cb_branch.SelectedIndex != -1)
                        branchId = (int)cb_branch.SelectedValue;
                    else
                        branchId = MainWindow.branchID.Value;

                    invoice.branchId = branchId;
                    invoiceResult = await invoiceModel.saveDirectEntry(invoice, invoiceItems, not, MainWindow.posID.Value);
                    break;

                default:

                    if (cb_branch.SelectedIndex != -1)
                        branchId = (int)cb_branch.SelectedValue;
                    else
                        branchId = MainWindow.branchID.Value;

                    invoice.branchId = branchId;
                    invoiceResult =await  invoiceModel.savePurchaseDraft(invoice, invoiceItems, MainWindow.posID.Value);
                    break;
            };
            return invoiceResult;

            #region old
            // save invoice in DB
            //int invoiceId = (int)await invoiceModel.saveInvoice(invoice);
            //prInvoiceId = invoiceId;
            //invoice.invoiceId = invoiceId;
            //if (invoiceId != 0)
            //{
            // add invoice details

            //await invoiceModel.saveInvoiceItems(invoiceItems, invoiceId);

            // Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
            //if (invType == "is")
            //invoiceModel.saveAvgPurchasePrice(invoiceItems);
            // }
            //else
            //  Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
            #endregion
        }
        private async Task<InvoiceResult> returnInvoice(string type)
        {
            InvoiceResult invoiceResult = new InvoiceResult();
            #region invoice items
            invoiceItems = new List<ItemTransfer>();
            ItemTransfer itemT;
            decimal returnValue = 0;
            for (int i = 0; i < billDetails.Count; i++)
            {
                itemT = new ItemTransfer();

                itemT.invoiceId = 0;
                itemT.itemName = billDetails[i].Product;
                itemT.itemId = billDetails[i].itemId;
                itemT.unitName = billDetails[i].Unit;
                itemT.quantity = billDetails[i].Count;
                itemT.price = billDetails[i].Price;
                itemT.itemUnitId = billDetails[i].itemUnitId;
                itemT.itemName = billDetails[i].Product;
                itemT.createUserId = MainWindow.userID;

                returnValue += (decimal)itemT.price * (decimal)itemT.quantity;
                invoiceItems.Add(itemT);
            }
            #endregion
            Window.GetWindow(this).Opacity = 0.2;
            wd_transItemsLocation w =  new wd_transItemsLocation();
            w.orderList = invoiceItems;
            w.ShowDialog();
            if (w.isOk == true)
            {
                if (w.selectedItemsLocations != null)
                {
                    List<ItemLocation> itemsLocations = w.selectedItemsLocations;
                    List<ItemLocation> readyItemsLoc = new List<ItemLocation>();

                    for (int i = 0; i < itemsLocations.Count; i++)
                    {
                        if (itemsLocations[i].isSelected == true)
                            readyItemsLoc.Add(itemsLocations[i]);
                    }

                    #region invoice object
                    invoice.invType = type;
                    invoice.updateUserId = MainWindow.userID.Value;
                    decimal total = 0;
                    // calculate total and totalnet
                    for (int i = 0; i < billDetails.Count; i++)
                    {
                        total += (decimal)billDetails[i].Price * (decimal)billDetails[i].Count;
                    }
                    invoice.total = total;
                    invoice.taxtype = 2;
                    decimal taxValue = SectionData.calcPercentage(total, (decimal)invoice.tax);
                    invoice.totalNet = total + taxValue;
                    invoice.paid = 0;
                    invoice.deserved = invoice.totalNet;
                    #endregion

                    #region posCash posCash with type inv
                    CashTransfer cashT = invoice.posCashTransfer(invoice, "pb");
                    #endregion
                    #region notification Object
                    Notification not = new Notification()
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
                    invoiceResult = await invoiceModel.returnPurInvoice(invoice, invoiceItems, not, cashT, readyItemsLoc, MainWindow.branchID.Value, MainWindow.posID.Value);

                    #region old
                    // int invoiceId = (int)await invoiceModel.saveInvoice(invoice);
                    // if (invoiceId != 0)
                    //{
                    //await invoice.recordPosCashTransfer(invoice, "pb");
                    //if(invoice.agentId != null)
                    //await invoice.recordCashTransfer(invoice, "pb");
                    //await invoiceModel.saveInvoiceItems(invoiceItems, invoiceId);

                    //#region notification Object
                    //Notification not = new Notification()
                    //{
                    //    title = "trExceedMinLimitAlertTilte",
                    //    ncontent = "trExceedMinLimitAlertContent",
                    //    msgType = "alert",
                    //    createDate = DateTime.Now,
                    //    updateDate = DateTime.Now,
                    //    createUserId = MainWindow.userID.Value,
                    //    updateUserId = MainWindow.userID.Value,
                    //};
                    //#endregion
                    //for (int i = 0; i < readyItemsLoc.Count; i++)
                    //{
                    //    int itemLocId = readyItemsLoc[i].itemsLocId;
                    //    int quantity = (int)readyItemsLoc[i].quantity;
                    //    await itemLocationModel.decreaseItemLocationQuantity(itemLocId, quantity, MainWindow.userID.Value, "storageAlerts_minMaxItem", not);
                    //}
                    // refreshInvoiceReturnNotification();
                    //clearInvoice();
                    // Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                    //}
                    //else
                    //  Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    #endregion

                }
            }
            else
                invoiceResult = null;

            Window.GetWindow(this).Opacity = 1;
            return invoiceResult;


        }
        private async Task<InvoiceResult> receiptInvoice()
        {
            #region invoice object 
            invoice.invType = "p";
            invoice.updateUserId = MainWindow.userID.Value;
            #endregion
            #region notification Object
            Notification not = new Notification()
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

           InvoiceResult invoiceResult =  await invoiceModel.recieptWaitingPurchase(invoice,invoiceItems, not,MainWindow.branchID.Value);

            #region old
            //await invoiceModel.saveInvoice(invoice);
            //await itemLocationModel.recieptInvoice(invoiceItems, MainWindow.branchID.Value, MainWindow.userID.Value, "storageAlerts_minMaxItem", not); // increase item quantity in DB
            //clearInvoice();
            //refreshInvoiceNotification();

            #endregion
            return invoiceResult;
        }

        #region report
        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (invoiceItems != null)
                    {
                        string msg = "";

                        Thread t1 = new Thread(async() =>
                        {
                            msg=await printPurInvoice(invoice, invoiceItems);
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

        //private void printReceipt()
        //{

        //    BuildReport();

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
        //    });
        //}

        //private void pdfReceipt()
        //{
        //    BuildReport();

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        saveFileDialog.Filter = "PDF|*.pdf;";

        //        if (saveFileDialog.ShowDialog() == true)
        //        {
        //            string filepath = saveFileDialog.FileName;
        //            LocalReportExtensions.ExportToPDF(rep, filepath); 
        //        }
        //    });
        //}
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        Invoice prInvoice = new Invoice();
        Branch branchModel = new Branch();
        List<ReportParameter> paramarr = new List<ReportParameter>();
        //private void BuildReport()
        //{
        //    List<ReportParameter> paramarr = new List<ReportParameter>();
        //    //ReceiptPurchase
        //    string addpath;
        //    bool isArabic = ReportCls.checkLang();
        //    if (isArabic)
        //    {
        //        addpath = @"\Reports\Store\Ar\ArReceiptPurchaseReport.rdlc";//////??????????
        //    }
        //    else
        //        addpath = @"\Reports\Store\En\ReceiptPurchaseReport.rdlc";/////////////?????????????
        //    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

        //    ReportCls.checkLang();

        //    clsReports.ReceiptPurchaseReport(invoiceItems, rep, reppath, paramarr);/////??????????????
        //    clsReports.setReportLanguage(paramarr);
        //    clsReports.Header(paramarr);

        //    rep.SetParameters(paramarr);

        //    rep.Refresh();
        //}

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
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
                        previewPurInvoice(invoice, invoiceItems);
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

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (invoiceItems != null)
                    {
                        //    Thread t1 = new Thread(() =>
                        //{
                        pdfPurInvoice(invoice, invoiceItems);
                        //});
                        //t1.Start();
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

        public async Task<reportSize> buildDirectReport(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            reportSize repsize = new reportSize();
            paramarr = new List<ReportParameter>();
            //string reppath = reportclass.GetDirectEntryRdlcpath(prInvoice);
           // rep.ReportPath = reppath;
            if (prInvoice.invoiceId > 0)
            {
              //  invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                if (prInvoice.agentId != null)
                {
                    Agent agentinv = new Agent();
                    //agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                    if (FillCombo.agentsList is null)
                        await FillCombo.RefreshAgents();
                    agentinv = FillCombo.agentsList.Where(x => x.agentId == (int)prInvoice.agentId).FirstOrDefault();
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
                User employ = new User();
            //    employ = await employ.getUserById((int)prInvoice.updateUserId);
                if (FillCombo.usersAllList is null)
                { await FillCombo.RefreshAllUsers(); }
                employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                prInvoice.uuserName = employ.name;
                prInvoice.uuserLast = employ.lastname;


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
                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

                      //  branch = await branchModel.getBranchById((int)prInvoice.branchId);
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
                
                int itemscount = 0;
                clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                itemscount = invoiceItems.Count();
                //repsize = reportclass.GetDirectEntryRdlcpath(prInvoice, itemscount);
                //printer
                clsReports clsrep = new clsReports();
                reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                repsize.paperSize = repsset.paperSize;
                repsize = reportclass.GetDirectEntryRdlcpath(prInvoice, itemscount, repsize.paperSize);
                repsize.printerName = repsset.printerName;
                //end 

                string reppath = repsize.reppath;
                rep.ReportPath = reppath;

              //  clsReports.purchaseInvoiceReport(invoiceItems, rep, reppath);
                clsReports.setInvoiceLanguage(paramarr);
                clsReports.InvoiceHeader(paramarr);
                paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);


           //     if (prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb" || prInvoice.invType == "pd" || prInvoice.invType == "isd" || prInvoice.invType == "is" || prInvoice.invType == "pbw")
                {
                    CashTransfer cachModel = new CashTransfer();
                    List<PayedInvclass> payedList = new List<PayedInvclass>();
                    if (prInvoice.cachTrans.Count==0)
                    {
                        payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                    }
                    else
                    {
                        payedList = prInvoice.cachTrans;
                    }

                    decimal sump = payedList.Sum(x => x.cash).Value;
                    decimal deservd = (decimal)prInvoice.totalNet - sump;
                    //convertter
                    foreach (var p in payedList)
                    {
                        p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                    }
                    paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                    paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                    paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                    rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));


                }
                //  multiplePaytable(paramarr);

                rep.SetParameters(paramarr);
                rep.Refresh();

            }
            return repsize;
        }
        public async void pdfPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            try
            {
                if (prInvoice.invoiceId > 0)
                {
                    // prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);
                    reportSize repsize = new reportSize();
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
                  || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd" || prInvoice.invType == "isd")
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                        }
                        else
                        {

                            repsize= await buildDirectReport(prInvoice, invoiceItems);

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



                                    rep.SetParameters(paramarr);

                                    rep.Refresh();

                                    if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
                                    {

                                        this.Dispatcher.Invoke(() =>
                                        {
                                            if (repsize.paperSize != "A4")
                                            {
                                                LocalReportExtensions.customExportToPDF(rep, filepath, repsize.width, repsize.height);
                                            }
                                            else
                                            {
                                                LocalReportExtensions.ExportToPDF(rep, filepath);
                                            }
                                          //  LocalReportExtensions.ExportToPDF(rep, filepath);

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
            catch(Exception ex)
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
                    reportSize repsize = new reportSize();
                    //   prInvoice = await invoiceModel.GetByInvoiceId(invoice.invoiceId);

                    if (int.Parse(AppSettings.Allow_print_inv_count) <= prInvoice.printedcount)
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);

                    }
                    else
                    {

                        Window.GetWindow(this).Opacity = 0.2;
                        string pdfpath;

                        //
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                        //////////////
                        paramarr = new List<ReportParameter>();


                        if (prInvoice.invoiceId > 0)
                        {

                            repsize= await buildDirectReport(prInvoice,  invoiceItems);

                            /////////////////////////
                            //copy count
                            if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pw" || prInvoice.invType == "is")
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

                                 //   LocalReportExtensions.ExportToPDF(rep, pdfpath);
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

                              //  LocalReportExtensions.ExportToPDF(rep, pdfpath);
                                if (repsize.paperSize != "A4")
                                {
                                    LocalReportExtensions.customExportToPDF(rep, pdfpath, repsize.width, repsize.height);
                                }
                                else
                                {
                                    LocalReportExtensions.ExportToPDF(rep, pdfpath);
                                }

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
        public async Task<string> printPurInvoice(Invoice prInvoice, List<ItemTransfer> invoiceItems)
        {
            string msg = "";
            try
            {
                if (prInvoice.invoiceId > 0)
                {
                    reportSize repsize = new reportSize();
                    //prInvoice = new Invoice();
                    //prInvoice = await invoiceModel.GetByInvoiceId(prInvoiceId);
                    //
                    if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
                                 || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
                                 || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd" || prInvoice.invType == "isd")
                    {
                        //this.Dispatcher.Invoke(() =>
                        //{
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
                        //});
                        msg = "trPrintDraftInvoice";
                    }
                    else
                    {
                       
                            ///////////////////////////////////////////////////////
                            paramarr = new List<ReportParameter>();

                          //  string reppath = reportclass.GetDirectEntryRdlcpath(prInvoice);
                      //  rep.ReportPath = reppath;
                            if (prInvoice.invoiceId > 0)
                            {
                               // invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
                                if (prInvoice.agentId != null)
                                {
                                    Agent agentinv = new Agent();
                                    //agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
                                    if (FillCombo.agentsList is null)
                                        await FillCombo.RefreshAgents();
                                    agentinv = FillCombo.agentsList.Where(x => x.agentId == (int)prInvoice.agentId).FirstOrDefault();

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
                                //User employ = new User();
                                //employ = await employ.getUserById((int)prInvoice.updateUserId);
                            User employ = new User();
                       
                            if (FillCombo.usersAllList is null)
                            { await FillCombo.RefreshAllUsers(); }
                            employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();

                            prInvoice.uuserName = employ.name;
                                prInvoice.uuserLast = employ.lastname;

                                Branch branch = new Branch();
                   
                         
                            if (FillCombo.branchsAllList is null)
                            { await FillCombo.RefreshBranchsAll(); }
                            branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();


                          //  branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
                                if (branch.branchId > 0)
                                {
                                    prInvoice.branchCreatorName = branch.name;
                                }
                                //branch reciver
                                if (prInvoice.branchId != null)
                                {
                                    if (prInvoice.branchId > 0)
                                    {
                                    branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

                                  //  branch = await branchModel.getBranchById((int)prInvoice.branchId);
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
                        
                            int itemscount = 0;
                            clsReports.purchaseInvoiceReport(invoiceItems, rep, "");
                            itemscount = invoiceItems.Count();
                            //repsize = reportclass.GetDirectEntryRdlcpath(prInvoice, itemscount);
                            //printer
                            clsReports clsrep = new clsReports();
                            reportSize repsset = await clsrep.CheckPrinterSetting(prInvoice.invType);
                            repsize.paperSize = repsset.paperSize;
                            repsize = reportclass.GetDirectEntryRdlcpath(prInvoice, itemscount, repsize.paperSize);
                            repsize.printerName = repsset.printerName;
                            //end 

                            string reppath = repsize.reppath;
                            rep.ReportPath = reppath;

                        //    clsReports.purchaseInvoiceReport(invoiceItems, rep, reppath);
                                clsReports.setReportLanguage(paramarr);
                                clsReports.InvoiceHeader(paramarr);
                                paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);


                               // if ((prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb" || prInvoice.invType == "pd" || prInvoice.invType == "isd" || prInvoice.invType == "is" || prInvoice.invType == "pbw"))
                                {
                                    CashTransfer cachModel = new CashTransfer();
                                    List<PayedInvclass> payedList = new List<PayedInvclass>();
                                 //   payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
                                    //decimal sump = payedList.Sum(x => x.cash).Value;
                                    //decimal deservd = (decimal)prInvoice.totalNet - sump;
                                    //convertter
                                    //foreach (var p in payedList)
                                    //{
                                    //    p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
                                    //}
                                    //paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

                                    //paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
                                    //paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
                                    rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));


                                }
                                //  multiplePaytable(paramarr);

                                rep.SetParameters(paramarr);
                                rep.Refresh();

                            }

                            //////////////////////////////////////////////////////////////////////
                            //copy count
                            if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb" || prInvoice.invType == "pw" || prInvoice.invType == "is")
                            {

                                paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));

                                for (int i = 1; i <= short.Parse(AppSettings.directentry_copy_count); i++)
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

                                //this.Dispatcher.Invoke(() =>
                                //{
                                  //  LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.directentry_copy_count));
                            if (repsize.paperSize == "A4")
                            {

                                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, repsize.printerName, short.Parse(AppSettings.directentry_copy_count));

                            }
                            else
                            {
                                LocalReportExtensions.customPrintToPrinter(rep, repsize.printerName, short.Parse(AppSettings.directentry_copy_count), repsize.width, repsize.height);

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

                    //
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
        //public async void printPurInvoice(int prInvoiceId)
        //{
        //    try
        //    {


        //        if (prInvoiceId > 0)
        //        {
        //            prInvoice = new Invoice();
        //            prInvoice = await invoiceModel.GetByInvoiceId(prInvoiceId);
        //            //
        //            if (prInvoice.invType == "pd" || prInvoice.invType == "sd" || prInvoice.invType == "qd"
        //                         || prInvoice.invType == "sbd" || prInvoice.invType == "pbd"
        //                         || prInvoice.invType == "ord" || prInvoice.invType == "imd" || prInvoice.invType == "exd" || prInvoice.invType == "isd")
        //            {
        //                this.Dispatcher.Invoke(() =>
        //                {
        //                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPrintDraftInvoice"), animation: ToasterAnimation.FadeIn);
        //                });
        //            }
        //            else
        //            {
        //                if (prInvoice.invoiceId > 0)
        //                {
        //                    ///////////////////////////////////////////////////////
        //                    paramarr = new List<ReportParameter>();

        //                    string reppath = reportclass.GetDirectEntryRdlcpath(prInvoice);
        //                    if (prInvoice.invoiceId > 0)
        //                    {
        //                        invoiceItems = await invoiceModel.GetInvoicesItems(prInvoice.invoiceId);
        //                        if (prInvoice.agentId != null)
        //                        {
        //                            Agent agentinv = new Agent();
        //                            //agentinv = await agentinv.getAgentById((int)prInvoice.agentId);
        //                            if (FillCombo.agentsList is null)
        //                                await FillCombo.RefreshAgents();
        //                            agentinv = FillCombo.agentsList.Where(x => x.agentId == (int)prInvoice.agentId).FirstOrDefault();

        //                            prInvoice.agentCode = agentinv.code;
        //                            //new lines
        //                            prInvoice.agentName = agentinv.name;
        //                            prInvoice.agentCompany = agentinv.company;
        //                        }
        //                        else
        //                        {

        //                            prInvoice.agentCode = "-";
        //                            //new lines
        //                            prInvoice.agentName = "-";
        //                            prInvoice.agentCompany = "-";
        //                        }
        //                        User employ = new User();
        //                       // employ = await employ.getUserById((int)prInvoice.updateUserId);
        //                        if (FillCombo.usersAllList is null)
        //                        { await FillCombo.RefreshAllUsers(); }
        //                        employ = FillCombo.usersAllList.Where(u => u.userId == (int)prInvoice.updateUserId).FirstOrDefault();


        //                        prInvoice.uuserName = employ.name;
        //                        prInvoice.uuserLast = employ.lastname;

        //                        Branch branch = new Branch();
        //                       // branch = await branchModel.getBranchById((int)prInvoice.branchCreatorId);
        //                        if (FillCombo.branchsAllList is null)
        //                        { await FillCombo.RefreshBranchsAll(); }
        //                        branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchCreatorId).FirstOrDefault();


        //                        if (branch.branchId > 0)
        //                        {
        //                            prInvoice.branchCreatorName = branch.name;
        //                        }
        //                        //branch reciver
        //                        if (prInvoice.branchId != null)
        //                        {
        //                            if (prInvoice.branchId > 0)
        //                            {
        //                                branch = FillCombo.branchsAllList.Where(b => b.branchId == (int)prInvoice.branchId).FirstOrDefault();

        //                               // branch = await branchModel.getBranchById((int)prInvoice.branchId);
        //                                prInvoice.branchName = branch.name;
        //                            }
        //                            else
        //                            {
        //                                prInvoice.branchName = "-";
        //                            }

        //                        }
        //                        else
        //                        {
        //                            prInvoice.branchName = "-";
        //                        }
        //                        // end branch reciever


        //                        ReportCls.checkLang();
        //                        foreach (var i in invoiceItems)
        //                        {
        //                            i.price = decimal.Parse(SectionData.DecTostring(i.price));
        //                            i.subTotal = decimal.Parse(SectionData.DecTostring(i.price * i.quantity));
        //                        }
        //                        clsReports.purchaseInvoiceReport(invoiceItems, rep, reppath);
        //                        clsReports.setReportLanguage(paramarr);
        //                        clsReports.Header(paramarr);
        //                        paramarr = reportclass.fillPurInvReport(prInvoice, paramarr);


        //                        if ((prInvoice.invType == "p" || prInvoice.invType == "pw" || prInvoice.invType == "pbd" || prInvoice.invType == "pb" || prInvoice.invType == "pd" || prInvoice.invType == "isd" || prInvoice.invType == "is" || prInvoice.invType == "pbw"))
        //                        {
        //                            //CashTransfer cachModel = new CashTransfer();
        //                            List<PayedInvclass> payedList = new List<PayedInvclass>();
        //                            //payedList = await cachModel.GetPayedByInvId(prInvoice.invoiceId);
        //                            //decimal sump = payedList.Sum(x => x.cash).Value;
        //                            //decimal deservd = (decimal)prInvoice.totalNet - sump;
        //                            //convertter
        //                            //foreach (var p in payedList)
        //                            //{
        //                            //    p.cash = decimal.Parse(reportclass.DecTostring(p.cash));
        //                            //}
        //                            //paramarr.Add(new ReportParameter("cashTr", MainWindow.resourcemanagerreport.GetString("trCashType")));

        //                            //paramarr.Add(new ReportParameter("sumP", reportclass.DecTostring(sump)));
        //                            //paramarr.Add(new ReportParameter("deserved", reportclass.DecTostring(deservd)));
        //                            rep.DataSources.Add(new ReportDataSource("DataSetPayedInvclass", payedList));


        //                        }
        //                        //  multiplePaytable(paramarr);

        //                        rep.SetParameters(paramarr);
        //                        rep.Refresh();

        //                    }

        //                    //////////////////////////////////////////////////////////////////////
        //                    //copy count
        //                    if (prInvoice.invType == "s" || prInvoice.invType == "sb" || prInvoice.invType == "p" || prInvoice.invType == "pb" || prInvoice.invType == "pw" || prInvoice.invType == "is")
        //                    {

        //                        paramarr.Add(new ReportParameter("isOrginal", prInvoice.isOrginal.ToString()));

        //                        for (int i = 1; i <= short.Parse(AppSettings.directentry_copy_count); i++)
        //                        {
        //                            if (i > 1)
        //                            {
        //                                // update paramarr->isOrginal
        //                                foreach (var item in paramarr.Where(x => x.Name == "isOrginal").ToList())
        //                                {
        //                                    StringCollection myCol = new StringCollection();
        //                                    myCol.Add(prInvoice.isOrginal.ToString());
        //                                    item.Values = myCol;


        //                                }
        //                                //end update

        //                            }
        //                            rep.SetParameters(paramarr);

        //                            rep.Refresh();

        //                            if (int.Parse(AppSettings.Allow_print_inv_count) > prInvoice.printedcount)
        //                            {

        //                                this.Dispatcher.Invoke(() =>
        //                                {

        //                                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, 1);


        //                                });


        //                                int res = 0;
        //                                res = (int)await invoiceModel.updateprintstat(prInvoice.invoiceId, 1, false, true);
        //                                prInvoice.printedcount = prInvoice.printedcount + 1;

        //                                prInvoice.isOrginal = false;


        //                            }
        //                            else
        //                            {
        //                                this.Dispatcher.Invoke(() =>
        //                                {
        //                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trYouExceedLimit"), animation: ToasterAnimation.FadeIn);
        //                                });

        //                            }

        //                        }
        //                    }
        //                    else
        //                    {

        //                        this.Dispatcher.Invoke(() =>
        //                        {
        //                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.directentry_copy_count));
        //                        });

        //                    }
        //                    // end copy count

        //                    /*
        //                    this.Dispatcher.Invoke(() =>
        //                    {
        //                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, 1);
        //                    });
        //                    */


        //                }
        //            }

        //            //
        //        }
        //    }
        //    catch //(Exception ex)
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            Toaster.ShowWarning(Window.GetWindow(this), message: "Not completed", animation: ToasterAnimation.FadeIn);

        //        });

        //    }
        //}
        #endregion


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
        private void clearNavigation()
        {
            _SequenceNum = 0;
            _Count = 0;
            _Sum = 0;
            invoice = new Invoice();
            txt_branch.Text = "_____________";
            txt_invNumber.Text = "";
            billDetails.Clear();
            tb_count.Text = "0";
            tb_total.Text = "0";
            tb_sum.Text = "0";
            btn_items.IsEnabled = true;
            isFromReport = false;
            refrishBillDetails();
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

                #region set title according to invoice type
                if (_InvoiceType == "p" || _InvoiceType == "pw")
                {
                    txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                    txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                    btn_save.Content = MainWindow.resourcemanager.GetString("trStoreBtn");
                }
                else if (_InvoiceType == "pb" || _InvoiceType == "pbw")
                {
                    txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trReturnedInvoice");
                    btn_save.Content = MainWindow.resourcemanager.GetString("trReturn");
                    // orange #FFA926 red #D22A17
                    txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush;
                }
                else if (_InvoiceType == "is")
                {
                    txt_titleDataGridInvoice.Text = MainWindow.resourcemanager.GetString("trDirectEntry");
                    txt_titleDataGridInvoice.Foreground = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                    btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
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

        private void Tb_taxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_taxValue")
                    SectionData.InputJustNumber(ref txb);
                _Sender = sender;
                refreshTotalValue();
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
                item.unitName = row.UnitName;

                Window.GetWindow(this).Opacity = 0.2;
                wd_serialNum w = new wd_serialNum();
                w.sourceUserControls = FillCombo.UserControls.receiptOfPurchaseInvoice;
                w.item = item;
                w.itemCount = row.Count;
                w.invType = _InvoiceType;
                w.ItemProperties = row.ItemProperties;
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

                    #region valid item serials
                    bool isValid = true;

                    if (_InvoiceType == "is" || _InvoiceType == "isd")
                    {
                        if (item.type == "sn" && row.itemSerials.Count() < row.Count)
                            isValid = false;
                    }
                    #endregion
                    row.valid = isValid;
                    refrishBillDetails();
                }
                //else if (w.serialsSkip || w.serialsSave)
                //{
                //    row.itemSerials = w.itemsSerials;
                //    row.valid = true;
                //    refrishBillDetails();

                //}
                //else if (w.propertiesSkip || w.propertiesSave)
                //{
                //    row.StoreProperties = w.StoreProperties;
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