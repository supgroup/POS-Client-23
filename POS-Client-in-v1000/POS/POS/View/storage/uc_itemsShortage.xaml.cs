using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
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

namespace POS.View.storage
{
    /// <summary>
    /// Interaction logic for uc_itemsShortage.xaml
    /// </summary>
    public partial class uc_itemsShortage : UserControl
    {
        private static uc_itemsShortage _instance;
        public static uc_itemsShortage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_itemsShortage();
                return _instance;
            }
        }
        public uc_itemsShortage()
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
        string savePermission = "shortage_save";
        string reportsPermission = "shortage_reports";


        InventoryItemLocation invItemLocModel = new InventoryItemLocation();
        InventoryItemLocation invItemLoc = new InventoryItemLocation();
        ItemLocation itemLocationModel = new ItemLocation();
        Invoice invoiceModel = new Invoice();
        Branch branchModel = new Branch();
        Inventory inventory;
        IEnumerable<InventoryItemLocation> inventoriesItems;
        IEnumerable<ItemTransferInvoice> shortageItems;
        User userModel = new User();
        List<User> users;

        private string _ItemType = "";
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;
        private static int _serialCount = 0;
        string searchText = "";
        IEnumerable<InventoryItemLocation> invItemsQuery;
        IEnumerable<ItemTransferInvoice> shortageItemsQuery;
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

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

                await refreshShortageDetails();
                await fillUsers();
                branchModel = MainWindow.loginBranch;
                
                #region key up
                 cb_user.IsTextSearchEnabled = false;
                cb_user.IsEditable = true;
                cb_user.StaysOpenOnEdit = true;
                cb_user.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_user.Text = "";
                #endregion

                //Tb_search_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                chk_stocktaking.IsChecked = true;

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async Task fillUsers()
        {
            if (FillCombo.usersActiveList is null)
                await FillCombo.RefreshUsersActive();
             users = FillCombo.usersActiveList.ToList();
            //users = await userModel.GetUsersActive();
            var user = new User();
            user.userId = 0;
            user.fullName = "-";
            users.Insert(0, user);
            cb_user.ItemsSource = users;
            cb_user.DisplayMemberPath = "fullName";
            cb_user.SelectedValuePath = "userId";
        }
        IEnumerable<Item> items;
        // item object
        Item item = new Item();
        ItemUnit itemUnit = new ItemUnit();
        async Task<IEnumerable<Item>> RefrishItems()
        {
            items = await item.GetItemsWichHasUnits();
            return items;
        }

        private void translate()
        {

            ////////////////////////////////----Grid----/////////////////////////////////
            col_inventoryNum.Header = MainWindow.resourcemanager.GetString("trCharp");
            col_invNum.Header = MainWindow.resourcemanager.GetString("trCharp");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
           col_location.Header = MainWindow.resourcemanager.GetString("trSectionLocation");
            col_itemUnit.Header = MainWindow.resourcemanager.GetString("trItemUnit");
            col_amount1.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_amount2.Header = MainWindow.resourcemanager.GetString("trQTR");
           col_total.Header = MainWindow.resourcemanager.GetString("trAmount");

            txt_itemsShortageHeader.Text = MainWindow.resourcemanager.GetString("trItemShortage");
            txt_shortage.Text = MainWindow.resourcemanager.GetString("trShortage");
            chk_stocktaking.Content = MainWindow.resourcemanager.GetString("trStocktaking");
            chk_daily.Content = MainWindow.resourcemanager.GetString("trDaily");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_itemUnit, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_quantity, MainWindow.resourcemanager.GetString("trQuantity"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_amount, MainWindow.resourcemanager.GetString("cashAmount"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUserHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_reasonOfShortage, MainWindow.resourcemanager.GetString("trReasonOfShortageHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

            btn_shortage.Content = MainWindow.resourcemanager.GetString("trShortage");
            btn_refresh.ToolTip = MainWindow.resourcemanager.GetString("trRefresh");

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
        #region Excel
   

        private void FN_ExportToExcel()
        {
            var QueryExcel = invItemsQuery.AsEnumerable().Select(x => new
            {
                InventoryNumber = x.inventoryNum,
                Date = x.inventoryDate,
                SectionLocation = x.section + "-" + x.location,
                ItemUnit = x.itemName + "-" + x.unitName,
                Ammount = x.amount
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trInventoryNum");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trInventoryDate");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trSectionLocation");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trItemUnit");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trAmount");

            ExportToExcel.Export(DTForExcel);
        }
        #endregion
        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                string name = sender.GetType().Name;
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_reasonOfShortage")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorReasonOfShortage, tt_errorReasonOfShortage, "trEmptyReasonToolTip");
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private bool validateDistroy()
        {
            bool valid = true;
            SectionData.validateEmptyTextBox(tb_reasonOfShortage, p_errorReasonOfShortage, tt_errorReasonOfShortage, "trEmptyReasonToolTip");
            if (tb_reasonOfShortage.Text.Equals(""))
            {
                valid = false;
                return valid;
            }

            if (int.Parse(tb_quantity.Text) < lst_serials.Items.Count)
            {
                valid = false;
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorSerialMoreItemCountToolTip"), animation: ToasterAnimation.FadeIn);
            }
            return valid;
        }
        private async Task recordCash(Invoice invoice)
        {
            User user = new User();
            float newBalance = 0;
            user = await user.getUserById((int)cb_user.SelectedValue);

            CashTransfer cashTrasnfer = new CashTransfer();
            cashTrasnfer.posId = MainWindow.posID;
            cashTrasnfer.userId = (int)cb_user.SelectedValue;
            cashTrasnfer.invId = invoice.invoiceId;
            cashTrasnfer.createUserId = invoice.createUserId;
            cashTrasnfer.processType = "shortage";
            cashTrasnfer.isCommissionPaid = 0;
            cashTrasnfer.side = "u"; // user
            cashTrasnfer.transType = "p"; //deposit
            cashTrasnfer.paid = 0;
            cashTrasnfer.deserved = invoice.totalNet;
            cashTrasnfer.transNum = "pu";
            //cashTrasnfer.transNum = await cashTrasnfer.generateCashNumber("pu");

            if (user.balanceType == 0)
            {
                if (invoice.totalNet <= (decimal)user.balance)
                {
                    invoice.paid = invoice.totalNet;
                    invoice.deserved = 0;
                    newBalance = user.balance - (float)invoice.totalNet;
                    user.balance = newBalance;
                }
                else
                {
                    invoice.paid = (decimal)user.balance;
                    invoice.deserved = invoice.totalNet - (decimal)user.balance;
                    newBalance = (float)invoice.totalNet - user.balance;
                    user.balance = newBalance;
                    user.balanceType = 1;
                }

                cashTrasnfer.cash = invoice.paid;

                await invoice.saveInvoice(invoice);
                await cashTrasnfer.Save(cashTrasnfer); //add cash transfer
                await user.save(user);
            }
            else if (user.balanceType == 1)
            {
                newBalance = user.balance + (float)invoice.totalNet;
                user.balance = newBalance;
                await user.save(user);
            }
        }
        private async void Btn_shortage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(savePermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (invItemLoc.id != 0)
                    {
                        bool valid = validateDistroy();
                        if (valid)
                        {
                            int itemUnitId = 0;
                            int itemId = 0;
                            string serialNum = "";

                            itemUnitId = invItemLoc.itemUnitId;
                            itemId = invItemLoc.itemId;
                            invItemLoc.notes = tb_notes.Text;
                            invItemLoc.fallCause = tb_reasonOfShortage.Text;

                            #region add invoice

                            invoiceModel.invNumber = "sh";
                            invoiceModel.branchCreatorId = MainWindow.branchID.Value;
                            invoiceModel.posId = MainWindow.posID.Value;
                            invoiceModel.createUserId = MainWindow.userID.Value;
                            invoiceModel.invType = "sh"; // shortage
                            invoiceModel.totalNet = (decimal)invItemLoc.avgPurchasePrice * int.Parse(tb_quantity.Text);
                            invoiceModel.total = decimal.Parse(tb_amount.Text) * int.Parse(tb_quantity.Text);
                            invoiceModel.paid = 0;
                            invoiceModel.deserved = invoiceModel.totalNet;
                            invoiceModel.notes = tb_notes.Text;
                            
                            if (cb_user.SelectedIndex != -1 && cb_user.SelectedIndex != 0)
                                invoiceModel.userId = (int)cb_user.SelectedValue;
                            #endregion

                            List<ItemTransfer> orderList = new List<ItemTransfer>();

                            if (_ItemType == "sn")
                                serialNum = tb_serialNum.Text;

                            if (lst_serials.Items.Count > 0)
                            {
                                for (int j = 0; j < lst_serials.Items.Count; j++)
                                {
                                    serialNum += lst_serials.Items[j];
                                    if (j != lst_serials.Items.Count - 1)
                                        serialNum += ",";
                                }
                            }

                            #region notification Object
                            Notification not = new Notification()
                            {
                                title = "trExceedMinLimitAlertTilte",
                                ncontent = "trExceedMinLimitAlertContent",
                                msgType = "alert",
                                objectName = "storageAlerts_minMaxItem",
                                createDate = DateTime.Now,
                                updateDate = DateTime.Now,
                                createUserId = MainWindow.userID.Value,
                                updateUserId = MainWindow.userID.Value,
                            };
                            #endregion

                            orderList.Add(new ItemTransfer()
                            {
                                itemName = invItemLoc.itemName,
                                itemId = invItemLoc.itemId,
                                unitName = invItemLoc.unitName,
                                itemUnitId = invItemLoc.itemUnitId,
                                quantity = invItemLoc.amount,
                                itemSerial = serialNum,
                                price = (decimal)invItemLoc.avgPurchasePrice,
                                invoiceId = 0,
                                inventoryItemLocId = invItemLoc.id,
                                createUserId = MainWindow.userID,
                            });

                            var res = await invoiceModel.shortageItem(invoiceModel, orderList, invItemLoc,not);
                            if (res > 0)
                            {
                                await refreshShortageDetails();
                                Btn_clear_Click(null, null);
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
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
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async Task refreshShortageDetails()
        {
            inventoriesItems = await invItemLocModel.GetShortageItem(MainWindow.branchID.Value);
            dg_itemShortage.ItemsSource = inventoriesItems;
        }
        async Task<IEnumerable<InventoryItemLocation>> RefreshinvItemList()
        {
            inventoriesItems = await invItemLocModel.GetShortageItem(MainWindow.branchID.Value);
            return inventoriesItems;
        }
        async Task<IEnumerable<ItemTransferInvoice>> RefreshShortageList()
        {
            shortageItems = await invoiceModel.GetDailyShortage(MainWindow.branchID.Value, MainWindow.userID.Value);
            return shortageItems;
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //search 

                searchText = tb_search.Text.ToLower();
                if (chk_stocktaking.IsChecked == true)
                {
                    if (inventoriesItems is null)
                    await RefreshinvItemList();
              
                invItemsQuery = inventoriesItems.Where(s => (s.inventoryNum.ToLower().Contains(searchText)
                || s.section.ToLower().Contains(searchText)
                || s.location.ToLower().Contains(searchText)
                || s.itemName.ToLower().Contains(searchText)
                || s.unitName.ToLower().Contains(searchText)
                || s.amount.ToString().ToLower().Contains(searchText)
                )

                );
                RefreshinvItemView();

                }
                else if (chk_daily.IsChecked == true)
                {
                    if (shortageItems is null)
                        await RefreshShortageList();

                    shortageItemsQuery = shortageItems.Where(s => (s.invNumber.ToLower().Contains(searchText)
                      || s.itemName.ToLower().Contains(searchText)
                      || s.unitName.ToLower().Contains(searchText)
                      || s.total.ToString().ToLower().Contains(searchText)
              )

              );
                    RefreshShortageView();
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

        void RefreshinvItemView()
        {
            dg_itemShortage.ItemsSource = null;
            dg_itemShortage.ItemsSource = invItemsQuery;
            txt_count.Text = invItemsQuery.Count().ToString();
        }
         void RefreshShortageView()
        {
            dg_itemShortage.ItemsSource = null;
            dg_itemShortage.ItemsSource = shortageItemsQuery;
            txt_count.Text = shortageItemsQuery.Count().ToString();
        }

        decimal avgPrice = 0;
        private void Dg_itemShortage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dg_itemShortage.SelectedItem != null)
                {
                    invItemLoc = dg_itemShortage.SelectedItem as InventoryItemLocation;
                    tb_itemUnit.Visibility = Visibility.Visible;
                    if (invItemLoc.itemType == "sn")
                        grid_serial.Visibility = Visibility.Visible;
                    tb_quantity.IsEnabled = false;

                    tb_itemUnit.Text = invItemLoc.itemName + " - " + invItemLoc.unitName;
                    this.DataContext = invItemLoc;

                    avgPrice = (decimal)invItemLoc.avgPurchasePrice;
                    setCashValue();
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
        private void setCashValue()
        {
            int quantity = 0;
            if (tb_quantity.Text != "")
                quantity = int.Parse(tb_quantity.Text);

            tb_amount.Text = (avgPrice * quantity).ToString();
        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (invItemsQuery != null)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        win_Storagelvc win = new win_Storagelvc(invItemsQuery, 1);
                        win.ShowDialog();
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

        private void Tb_decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //decimal
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


        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (invItemLoc != null)
                    invItemLoc.id = 0;
                _ItemType = "";
                cb_user.SelectedIndex = -1;
                avgPrice = 0;
                DataContext = new InventoryItemLocation();
                grid_serial.Visibility = Visibility.Collapsed;
                tb_notes.Clear();
                tb_reasonOfShortage.Clear();
                tb_notes.Clear();
                tb_amount.Clear();
                tb_itemUnit.Text = "";
                invoiceModel = new Invoice();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tgl_IsActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                Btn_clear_Click(null, null);
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

        private void Tgl_IsActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                dg_itemShortage.SelectedItem = null;
                Btn_clear_Click(null, null);
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
        private void Btn_clearSerial_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                _serialCount = 0;
                lst_serials.Items.Clear();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_serialNum_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                if (e.Key == Key.Return && !tb_quantity.Text.Equals(""))
                {
                    string s = tb_serialNum.Text;
                    int itemCount = int.Parse(tb_quantity.Text);

                    if (!s.Equals(""))
                    {
                        int found = lst_serials.Items.IndexOf(s);

                        if (_serialCount == itemCount)
                        {
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningItemCountIs:") + " " + itemCount, animation: ToasterAnimation.FadeIn);
                        }
                        else if (found == -1)
                        {
                            lst_serials.Items.Add(tb_serialNum.Text);
                            _serialCount++;
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trWarningSerialExists"), animation: ToasterAnimation.FadeIn);
                    }
                    tb_serialNum.Clear();
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

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await RefreshinvItemList();
                await RefreshShortageList();
                Tb_search_TextChanged(null, null);
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
        #region report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";
            
            //string chk_stocktaking = "";
            //string chk_daily = "";
            string chk_stock = "";
            string chk_day = "";
            List<string> invTypelist = new List<string>();
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Store\Ar\ArShortageReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Store\EN\ShortageReport.rdlc";
            }
            //filter  
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            chk_stock = chk_stocktaking.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trStocktaking") : "";
            chk_day = chk_daily.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDaily") : "";
              invTypelist.Add(chk_stock);
            invTypelist.Add(chk_day);
            
            int i = 0;
            foreach (string r in invTypelist)
            {

                if (r != null && r != "")
                {
                    if (i == 0)
                    {
                        stateval = r;
                    }
                    else
                    {
                        stateval = stateval + " , " + r;
                    }
                    i++;
                }
            }
       
            //end filter
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trShortages")));
            paramarr.Add(new ReportParameter("trCharp", MainWindow.resourcemanagerreport.GetString("trCharp")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trSectionLocation", MainWindow.resourcemanagerreport.GetString("trSectionLocation")));           
            paramarr.Add(new ReportParameter("trItemUnit", MainWindow.resourcemanagerreport.GetString("trItemUnit")));   
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));            
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
         
            ReportCls.checkLang();
            if (chk_stocktaking.IsChecked == true)
            {
                paramarr.Add(new ReportParameter("isdaily", false.ToString()));
                clsReports.invItemShortage(invItemsQuery, rep, reppath, paramarr);
            }
            else
            {
                paramarr.Add(new ReportParameter("isdaily", true.ToString()));
                clsReports.invItemShortagedaily(shortageItemsQuery, rep, reppath, paramarr);
            }
           
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }
            private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invItemsQuery != null)
                    {
                        BuildReport();

                        saveFileDialog.Filter = "PDF|*.pdf;";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            LocalReportExtensions.ExportToPDF(rep, filepath);
                        }
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
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invItemsQuery != null)
                    {
                        BuildReport();
                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
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

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invItemsQuery != null)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        string pdfpath = "";
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        BuildReport();
                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        wd_previewPdf w = new wd_previewPdf();
                        w.pdfPath = pdfpath;
                        if (!string.IsNullOrEmpty(w.pdfPath))
                        {
                            w.ShowDialog();
                            w.wb_pdfWebViewer.Dispose();
                        }
                        Window.GetWindow(this).Opacity = 1;
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
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invItemsQuery != null)
                    {
                        BuildReport();
                        this.Dispatcher.Invoke(() =>
                        {
                            saveFileDialog.Filter = "EXCEL|*.xls;";
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string filepath = saveFileDialog.FileName;
                                LocalReportExtensions.ExportToExcel(rep, filepath);
                            }
                        });
                    
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
        #endregion
        private void Cb_user_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                List<Slice> sLst = new List<Slice>();
                combo.ItemsSource = users.ToList().Where(p => p.fullName.ToLower().Contains(tb.Text.ToLower())).ToList();
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
                if (cb_user.SelectedValue != null && cb_user.SelectedValue.ToString() != "0" && cb_user.SelectedIndex != -1)
                {
                    //if (int.Parse(cb_user.SelectedValue.ToString()) != -1)
                    //{
                    //}
                    tb_amount.IsEnabled = true;
                }
                else
                    tb_amount.IsEnabled = false;

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

        private void search_Checking(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                CheckBox cb = sender as CheckBox;
                if (chk_stocktaking != null)
                {
                    if (cb.Name == "chk_stocktaking")
                    {
                        chk_daily.IsChecked = false;

                        grid_controlers.IsEnabled = true;
                        btn_shortage.IsEnabled = true;

                        col_inventoryNum.Visibility = Visibility.Visible;
                        col_date.Visibility = Visibility.Visible;
                        col_location.Visibility = Visibility.Visible;
                        col_amount1.Visibility = Visibility.Visible;
                        col_invNum.Visibility = Visibility.Collapsed;
                        col_amount2.Visibility = Visibility.Collapsed;

                    }
                    else if (cb.Name == "chk_daily")
                    {
                        chk_stocktaking.IsChecked = false;

                        grid_controlers.IsEnabled = false;
                        btn_shortage.IsEnabled = false;

                        col_invNum.Visibility = Visibility.Visible;
                        col_amount2.Visibility = Visibility.Visible;
                        col_date.Visibility = Visibility.Collapsed;
                        col_location.Visibility = Visibility.Collapsed;
                        col_inventoryNum.Visibility = Visibility.Collapsed;
                        col_amount1.Visibility = Visibility.Collapsed;
                     //   Btn_clear_Click(btn_clear, null);

                    }
                }

                Tb_search_TextChanged(tb_search, null);

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

        private void chk_uncheck(object sender, RoutedEventArgs e)
        {

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "chk_stocktaking")
                        chk_stocktaking.IsChecked = true;
                    else if (cb.Name == "chk_daily")
                        chk_daily.IsChecked = true;
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
    }
}
