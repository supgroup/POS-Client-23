using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for uc_itemsDestroy.xaml
    /// </summary>
    public partial class uc_itemsDestroy : UserControl
    {
        string destroyPermission = "itemsDestroy_destroy";
        string reportsPermission = "itemsDestroy_reports";
        private static uc_itemsDestroy _instance;
        public static uc_itemsDestroy Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_itemsDestroy();
                return _instance;
            }
        }
        public uc_itemsDestroy()
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
        Category categoryModel = new Category();
        Category category = new Category();
        //IEnumerable<Category> categories;
        InventoryItemLocation invItemLocModel = new InventoryItemLocation();
        InventoryItemLocation invItemLoc = new InventoryItemLocation();
        ItemLocation itemLocationModel = new ItemLocation();
        Invoice invoiceModel = new Invoice();
        Branch branchModel = new Branch();
        IEnumerable<InventoryItemLocation> inventoriesItems;
        IEnumerable<ItemTransferInvoice> destroyedItems;
        User userModel = new User();
        List<User> users;

        //int? categoryParentId = 0;
        private string _ItemType = "";
        public byte tglCategoryState = 1;
        public byte tglItemState = 1;
        public string txtItemSearch;
        private static int _serialCount = 0;
        string searchText = "";
        IEnumerable<InventoryItemLocation> invItemsQuery;
        IEnumerable<ItemTransferInvoice> destroyedItemsQuery;
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

                await refreshDestroyDetails();
                await fillItemCombo();
                await fillUsers();
                branchModel = MainWindow.loginBranch;
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
               await  refreshDestroyDetails();
                await RefreshDestroyedList();

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
       // IEnumerable<Item> items;
        IEnumerable<ItemLocation> items;
        // item object
        Item item = new Item();
        ItemUnit itemUnit = new ItemUnit();
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

        decimal avgPrice = 0;
        private async void Cb_item_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                if (cb_item.SelectedValue != null && cb_item.SelectedIndex != -1)
                    if (int.Parse(cb_item.SelectedValue.ToString()) != -1)
                    {
                        //var list = await itemUnit.GetItemUnits(int.Parse(cb_item.SelectedValue.ToString()));
                        //cb_unit.ItemsSource = list;
                        //cb_unit.SelectedValuePath = "itemUnitId";
                        //cb_unit.DisplayMemberPath = "mainUnit";
                        //cb_unit.SelectedIndex = 0;
                        int itemUnitId = (int)cb_item.SelectedValue;
                        var item = items.ToList().Find(x => x.itemUnitId == itemUnitId);
                        avgPrice = (decimal)item.avgPurchasePrice;
                        _ItemType = item.itemType;

                        setCashValue();
                        if (item.itemType == "sn")
                        {
                            grid_serial.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            grid_serial.Visibility = Visibility.Collapsed;
                        }

                    }
                Btn_clearSerial_Click(null, null);
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

        async Task<IEnumerable<ItemLocation>> RefrishItems()
        {
            items = await itemLocationModel.getForDestroy(MainWindow.branchID.Value);
            items = items.Where(x => x.itemType != "sr").ToList();
            return items;
        }
        private async Task fillItemCombo()
        {
            //if (items is null)
             await RefrishItems();

            foreach(var t in items)
            {
                t.itemName = t.itemName + " - " + t.unitName;
            }
            cb_item.ItemsSource = items.ToList();
            cb_item.SelectedValuePath = "itemUnitId";
            cb_item.DisplayMemberPath = "itemName";
        }

        private void setCashValue()
        {
            int quantity = 0;
            if (tb_quantity.Text != "")
                quantity = int.Parse(tb_quantity.Text);

            tb_amount.Text =(avgPrice * quantity).ToString();
        }
        private void translate()
        {
            txt_itemsDestroyHeader.Text = MainWindow.resourcemanager.GetString("trDestructiveItem");
            txt_destroy.Text = MainWindow.resourcemanager.GetString("trDestructionInfo");
            txt_manually.Text = MainWindow.resourcemanager.GetString("trManually");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_itemUnit, MainWindow.resourcemanager.GetString("trItem")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_quantity, MainWindow.resourcemanager.GetString("trQuantity") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_amount, MainWindow.resourcemanager.GetString("cashAmount") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUser")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_reasonOfDestroy, MainWindow.resourcemanager.GetString("trReason") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNote") + "...");

            btn_destroy.Content = MainWindow.resourcemanager.GetString("trDestruct");
            chk_stocktaking.Content = MainWindow.resourcemanager.GetString("trStocktaking");
            chk_daily.Content = MainWindow.resourcemanager.GetString("trDaily");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_invertoryNum.Header = MainWindow.resourcemanager.GetString("trCharp");
            col_invNum.Header = MainWindow.resourcemanager.GetString("trCharp");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_location.Header = MainWindow.resourcemanager.GetString("trSection")+"-"+ MainWindow.resourcemanager.GetString("trLocation");
            col_itemUnit.Header = MainWindow.resourcemanager.GetString("trItem") + "-" + MainWindow.resourcemanager.GetString("trUnit");
            col_amountDistroyed.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_quantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_total.Header = MainWindow.resourcemanager.GetString("trAmount");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
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
                    if ((sender as TextBox).Name == "tb_reasonOfDestroy")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorReasonOfDestroy, tt_errorReasonOfDestroy, "trEmptyReasonToolTip");
                }

            }
            catch (Exception ex)
            {
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
        private bool validateDistroy()
        {
            bool valid = true;
            SectionData.validateEmptyTextBox(tb_reasonOfDestroy, p_errorReasonOfDestroy, tt_errorReasonOfDestroy, "trEmptyReasonToolTip");
            SectionData.validateEmptyTextBox(tb_quantity, p_errorQuantity, tt_errorQuantity, "trEmptyReasonToolTip");
            if (tb_reasonOfDestroy.Text.Equals(""))
            {
                valid = false;
                return valid;
            }
            if (tb_quantity.Text.Equals(""))
            {
                valid = false;
                return valid;
            }
            if (invItemLoc == null || invItemLoc.id == 0)
            {
                SectionData.validateEmptyComboBox(cb_item, p_errorItem, tt_errorItem, "trEmptyItemToolTip");

                if (cb_item.SelectedIndex == -1)
                {
                    valid = false;
                    return valid;
                }
            }
            if (int.Parse(tb_quantity.Text) < lst_serials.Items.Count)
            {
                valid = false;
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorSerialMoreItemCountToolTip"), animation: ToasterAnimation.FadeIn);
            }
            return valid;
        }
        private async void Btn_destroy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(destroyPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    bool valid = validateDistroy();
                    if (valid)
                    {
                        int itemUnitId = 0;
                        int itemId = 0;
                        //int invoiceId = 0;
                        string serialNum = "";
                        if (invItemLoc.id != 0)
                        {
                            itemUnitId = invItemLoc.itemUnitId;
                            itemId = invItemLoc.itemId;
                        }
                        else
                        {
                            //itemUnitId = (int)cb_unit.SelectedValue;
                            itemUnitId = (int)cb_item.SelectedValue;
                            itemId = (int)cb_item.SelectedValue;
                        }
                        if (_ItemType == "sn")
                            serialNum = tb_serialNum.Text;
                        invItemLoc.cause = tb_reasonOfDestroy.Text;
                        invItemLoc.notes = tb_notes.Text;
                        if (lst_serials.Items.Count > 0)
                        {
                            for (int j = 0; j < lst_serials.Items.Count; j++)
                            {
                                serialNum += lst_serials.Items[j];
                                if (j != lst_serials.Items.Count - 1)
                                    serialNum += ",";
                            }
                        }

                        decimal price = 0;
                        decimal total = 0;

                        #region invoice Object
                        invoiceModel.invNumber = "ds";
                        invoiceModel.branchCreatorId = MainWindow.branchID.Value;
                        invoiceModel.posId = MainWindow.posID.Value;
                        invoiceModel.createUserId = MainWindow.userID.Value;
                        invoiceModel.invType = "d"; // destroy                      
                        invoiceModel.paid = 0;
                        invoiceModel.deserved = invoiceModel.totalNet;
                        invoiceModel.notes = tb_notes.Text;
                        if (cb_user.SelectedIndex != -1 && cb_user.SelectedIndex != 0)
                            invoiceModel.userId = (int)cb_user.SelectedValue;
                        #endregion
                        List<ItemTransfer> orderList = new List<ItemTransfer>();
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
                        if (invItemLoc.id != 0)
                        {
                            price = (decimal)invItemLoc.avgPurchasePrice;
                            total = price * int.Parse(tb_quantity.Text);
                            invoiceModel.totalNet = total;
                            invoiceModel.total = decimal.Parse(tb_amount.Text);

                            orderList.Add(new ItemTransfer()
                                {
                                    itemName = invItemLoc.itemName,
                                    itemId = invItemLoc.itemId,
                                    unitName = invItemLoc.unitName,
                                    itemUnitId = invItemLoc.itemUnitId,
                                    quantity = invItemLoc.amountDestroyed,
                                    itemSerial = serialNum,
                                    price = price,
                                    invoiceId =0,
                                    inventoryItemLocId = invItemLoc.id,
                                    createUserId = MainWindow.userID,
                                });

                            var res = await invoiceModel.destroyItem(invoiceModel, orderList, invItemLoc,not);
                            if(res > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                                await refreshDestroyDetails();
                                Btn_clear_Click(null, null);
                            }
                            else
                                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                            #region old
                            // invoiceId = (int)await invoiceModel.saveWithItems(invoiceModel,orderList);
                            //if (invoiceId != 0)
                            //{
                            // invoiceModel.invoiceId = invoiceId;
                            //await invItemLoc.distroyItem(invItemLoc);
                            //if (cb_user.SelectedIndex != -1 && cb_user.SelectedIndex != 0)
                            //  await recordCash(invoiceModel);

                            // await itemLocationModel.decreaseItemLocationQuantity((int)invItemLoc.itemLocationId, (int)invItemLoc.amountDestroyed, MainWindow.userID.Value, "storageAlerts_minMaxItem", not);
                            // await refreshDestroyDetails();
                            // Btn_clear_Click(null, null);
                            //Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            //}
                            //else
                            //    Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            #endregion
                        }
                        else
                        {
                            //var avgPrice = items.Where(x => x.itemId == (int)cb_item.SelectedValue).Select(x => x.avgPurchasePrice).Single();
                            //if (avgPrice != null)
                            //price = (decimal)avgPrice;
                            //total = price * int.Parse(tb_quantity.Text);
                            //invoiceModel.total = total;
                            //invoiceModel.totalNet = total;
                            var item = items.Where(x => x.itemUnitId == (int)cb_item.SelectedValue).FirstOrDefault();
                            total = decimal.Parse(tb_amount.Text);
                            invoiceModel.total = total;
                            invoiceModel.totalNet = avgPrice * long.Parse(tb_quantity.Text);
                            
                            orderList.Add(new ItemTransfer()
                            {
                                itemName = cb_item.SelectedItem.ToString(),
                                itemId = item.itemId,
                                unitName =item.unitName,
                                itemUnitId = (int)cb_item.SelectedValue,
                                quantity = long.Parse(tb_quantity.Text),
                                itemSerial = serialNum,
                                //price = price,
                                price = avgPrice,
                                cause = tb_reasonOfDestroy.Text,
                                invoiceId=0,
                                createUserId = MainWindow.userID,
                            });
                            // اتلاف عنصر يدوياً بدون جرد
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_transItemsLocation w;
                            w = new wd_transItemsLocation();
                            w.orderList = orderList;
                            if (w.ShowDialog() == true)
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

                                    var res = await invoiceModel.manualDestroyItem(invoiceModel,orderList, readyItemsLoc,not);
                                    if(res > 0)
                                    {
                                        Btn_clear_Click(null, null);
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                                    }
                                    else if(res.Equals(-3))
                                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip"), animation: ToasterAnimation.FadeIn);
                                    else
                                        Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                    #region old
                                    // invoiceId = (int)await invoiceModel.saveWithItems(invoiceModel, orderList);

                                    //if (invoiceId != 0)
                                    //{
                                    //    for (int i = 0; i < readyItemsLoc.Count; i++)
                                    //    {
                                    //        int itemLocId = readyItemsLoc[i].itemsLocId;
                                    //        int quantity = (int)readyItemsLoc[i].quantity;
                                    //        await itemLocationModel.decreaseItemLocationQuantity(itemLocId, quantity, MainWindow.userID.Value, "storageAlerts_minMaxItem", not);
                                    //    }
                                    //    Btn_clear_Click(null, null);
                                    //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                    //}
                                    // else
                                    //Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                    #endregion
                                }
                            }
                            Window.GetWindow(this).Opacity = 1;
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


        private async Task refreshDestroyDetails()
        {
            dg_itemDestroy.ItemsSource = null;
            inventoriesItems = await invItemLocModel.GetItemToDestroy(MainWindow.branchID.Value);
            dg_itemDestroy.ItemsSource = inventoriesItems.ToList();
        }

        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
                    //&& s.isActive == tgl_offerState
                    );
                    RefreshinvItemView();
                }
                else if (chk_daily.IsChecked == true)
                {
                    if (destroyedItems is null)
                        await RefreshDestroyedList();

                    destroyedItemsQuery = destroyedItems.Where(s => (s.invNumber.ToLower().Contains(searchText)
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

        async Task<IEnumerable<InventoryItemLocation>> RefreshinvItemList()
        {
             inventoriesItems = await invItemLocModel.GetItemToDestroy(MainWindow.branchID.Value);
             return inventoriesItems;
        }

        async Task<IEnumerable<ItemTransferInvoice>> RefreshDestroyedList()
        {
            destroyedItems = await invoiceModel.GetDailyDestructive(MainWindow.branchID.Value, MainWindow.userID.Value);
            return destroyedItems;
        }
        void RefreshinvItemView()
        {
            dg_itemDestroy.ItemsSource = null;

            dg_itemDestroy.ItemsSource = invItemsQuery;
            txt_count.Text = invItemsQuery.Count().ToString();
        }
        void RefreshShortageView()
        {
            dg_itemDestroy.ItemsSource = null;
            dg_itemDestroy.ItemsSource = destroyedItemsQuery;
            txt_count.Text = destroyedItemsQuery.Count().ToString();
        }
        private void Dg_itemDestroy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (dg_itemDestroy.SelectedItem != null)
                {
                    invItemLoc = dg_itemDestroy.SelectedItem as InventoryItemLocation;
                    tb_itemUnit.Visibility = Visibility.Visible;
                    grid_itemUnit.Visibility = Visibility.Collapsed;
                    if (invItemLoc.itemType == "sn")
                        grid_serial.Visibility = Visibility.Visible;
                    tb_quantity.IsEnabled = false;
                    tb_itemUnit.Text = invItemLoc.itemName + " - " + invItemLoc.unitName;
                    this.DataContext = invItemLoc;
                    tgl_manually.IsChecked = false;

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

        private void Cb_item_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                cb_item.ItemsSource = items.Where(x => x.itemName.Contains(cb_item.Text));
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            if (invItemLoc != null)
                invItemLoc.id = 0;
            _ItemType = "";
            avgPrice = 0;
            DataContext = new InventoryItemLocation();
            cb_item.SelectedIndex =
            cb_user.SelectedIndex = -1;
            grid_serial.Visibility = Visibility.Collapsed;
            tb_notes.Clear();
            tb_reasonOfDestroy.Clear();
            tb_itemUnit.Text = "";
            tb_notes.Clear();
            invoiceModel = new Invoice();
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

                dg_itemDestroy.SelectedItem = null;
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

        void tglManuallyChecking()
        {
            if (tgl_manually.IsChecked == true)
            {
                tb_itemUnit.Visibility = Visibility.Collapsed;
                grid_itemUnit.Visibility = Visibility.Visible;
                tb_quantity.IsEnabled = true;
            }
            else
            {
                tb_itemUnit.Visibility = Visibility.Visible;
                grid_itemUnit.Visibility = Visibility.Collapsed;
            }
            grid_serial.Visibility = Visibility.Collapsed;

            Btn_clear_Click(null, null);
        }
        private void Tgl_manually_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tglManuallyChecking();
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

        private void Tgl_manually_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tglManuallyChecking();
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
            cashTrasnfer.processType = "balance";
            cashTrasnfer.side = "u"; // user
            cashTrasnfer.transType = "d"; //deposit
            cashTrasnfer.transNum = await cashTrasnfer.generateCashNumber("du");

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
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Btn_clear_Click(null, null);
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
        #region report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";        
            string chk_stock = "";
            string chk_day = "";
            List<string> invTypelist = new List<string>();
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Store\Ar\ArDestroyReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Store\EN\DestroyReport.rdlc";
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
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trDestructives")));
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
                //destroyedItemsQuery
                paramarr.Add(new ReportParameter("isdaily", true.ToString()));
                clsReports.invItemShortagedaily(destroyedItemsQuery, rep, reppath, paramarr);
            }

            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        //public void BuildReport()
        //{
        //    List<ReportParameter> paramarr = new List<ReportParameter>();
        //    string searchval = "";
        //    string addpath;
        //    bool isArabic = ReportCls.checkLang();
        //    if (isArabic)
        //    {
        //        addpath = @"\Reports\Store\Ar\ArDestroyReport.rdlc";
        //    }
        //    else
        //    {
        //        addpath = @"\Reports\Store\EN\DestroyReport.rdlc";
        //    }
        //    //filter  
        //    paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
        //    searchval = tb_search.Text;
        //    paramarr.Add(new ReportParameter("searchVal", searchval));
        //    //end filter
        //    paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trDestructives")));
        //    paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
        //    paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
        //    paramarr.Add(new ReportParameter("trSection", MainWindow.resourcemanagerreport.GetString("trSection")));
        //    paramarr.Add(new ReportParameter("trLocation", MainWindow.resourcemanagerreport.GetString("trLocation")));
        //    paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
        //    paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
        //    paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));

        //    string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
        //    ReportCls.checkLang();
        //    clsReports.invItem(invItemsQuery, rep, reppath, paramarr);
        //    clsReports.setReportLanguage(paramarr);
        //    clsReports.Header(paramarr);
        //    rep.SetParameters(paramarr);
        //    rep.Refresh();

        //}
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
        {//export to excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (invItemsQuery != null)
                    {
                        //    Thread t1 = new Thread(() =>
                        //{
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
                        //});
                        //    t1.Start();
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
                combo.ItemsSource = users.ToList().Where(p => p.fullName.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
           
        }

        private void Tb_quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                setCashValue();
            }
            catch { }
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
                        btn_destroy.IsEnabled = true;

                        col_invertoryNum.Visibility = Visibility.Visible;
                        col_date.Visibility = Visibility.Visible;
                        col_location.Visibility = Visibility.Visible;
                        col_amountDistroyed.Visibility = Visibility.Visible;
                        col_invNum.Visibility = Visibility.Collapsed;
                        col_quantity.Visibility = Visibility.Collapsed;

                    }
                    else if (cb.Name == "chk_daily")
                    {
                        chk_stocktaking.IsChecked = false;

                        grid_controlers.IsEnabled = false;
                        btn_destroy.IsEnabled = false;

                        col_invertoryNum.Visibility = Visibility.Collapsed;
                        col_date.Visibility = Visibility.Collapsed;
                        col_location.Visibility = Visibility.Collapsed;
                        col_amountDistroyed.Visibility = Visibility.Collapsed;
                        col_invNum.Visibility = Visibility.Visible;
                        col_quantity.Visibility = Visibility.Visible;

                        Btn_clear_Click(btn_clear, null);

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
