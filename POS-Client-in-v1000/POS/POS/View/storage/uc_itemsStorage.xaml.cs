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
using static POS.View.storage.uc_receiptOfPurchaseInvoice;

namespace POS.View.storage
{
    /// <summary>
    /// Interaction logic for uc_itemsStorage.xaml
    /// </summary>
    public partial class uc_itemsStorage : UserControl
    {
        private static uc_itemsStorage _instance;

        ItemLocation itemLocation = new ItemLocation();
        IEnumerable<ItemLocation> itemLocationList;
        IEnumerable<ItemLocation> itemLocationListQuery;

        Classes.Section sectionModel = new Classes.Section();

        Location locationModel = new Location();
        Invoice invoiceModel = new Invoice();
        IEnumerable<Location> locations;
        string searchText = "";
        string transferPermission = "itemsStorage_transfer";
        string reportsPermission = "itemsStorage_reports";
        public static uc_itemsStorage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_itemsStorage();
                return _instance;
            }
        }
        public uc_itemsStorage()
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

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

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

                await refreshGrids();
                searsh();

                await fillSections();

                #region Style Date
                dp_startDate.Loaded += delegate
                {

                    var textBox1 = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_startDate.Background;
                        textBox1.BorderThickness = dp_startDate.BorderThickness;
                    }
                };
                dp_endDate.Loaded += delegate
                {

                    var textBox1 = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_endDate.Background;
                        textBox1.BorderThickness = dp_endDate.BorderThickness;
                    }
                };

                #endregion

                #region key up
                cb_section.IsTextSearchEnabled = false;
                cb_section.IsEditable = true;
                cb_section.StaysOpenOnEdit = true;
                cb_section.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_section.Text = "";

                cb_XYZ.IsTextSearchEnabled = false;
                cb_XYZ.IsEditable = true;
                cb_XYZ.StaysOpenOnEdit = true;
                cb_XYZ.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_XYZ.Text = "";
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                chk_stored.IsChecked = true;
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async Task refreshGrids()
        {
            await refreshItemsLocations();
            await refreshFreeZoneItemsLocations();
            await refreshLockedItems();
            await refreshLackItems();
        }
        private  void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                 searsh();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       void searsh()
        {

            if (chk_stored.IsChecked == true)
            {
                // await refreshItemsLocations();
                itemLocationList = locationsItems.ToList();
                grid_details.Visibility = Visibility.Visible;
                btn_transfer.Visibility = Visibility.Visible;

                grid_lack.Visibility = Visibility.Collapsed;

                col_startDate.Visibility = Visibility.Visible;
                col_endDate.Visibility = Visibility.Visible;
            }
            else if (chk_freezone.IsChecked == true)
            {
                //await refreshFreeZoneItemsLocations();
                itemLocationList = freeZoneItems.ToList();

                grid_details.Visibility = Visibility.Visible;
                btn_transfer.Visibility = Visibility.Visible;

                grid_lack.Visibility = Visibility.Collapsed;

                col_startDate.Visibility = Visibility.Collapsed;
                col_endDate.Visibility = Visibility.Collapsed;
            }
            else if (chk_locked.IsChecked == true)
            {
                //await refreshLockedItems();
                itemLocationList = lockedItems.ToList();

                grid_details.Visibility = Visibility.Visible;
                btn_transfer.Visibility = Visibility.Visible;

                grid_lack.Visibility = Visibility.Collapsed;

                col_startDate.Visibility = Visibility.Visible;
                col_endDate.Visibility = Visibility.Visible;
            }
            else if (chk_lack.IsChecked == true)
            {
                //await refreshLackItems();
                itemLocationList = lackItems.ToList();

                grid_details.Visibility = Visibility.Collapsed;
                btn_transfer.Visibility = Visibility.Collapsed;

                grid_lack.Visibility = Visibility.Visible;
            }
            clearInputs();
            if(chk_lack.IsChecked == true)
            {
                if (itemLocationList != null)
                {
                    itemLocationListQuery = itemLocationList.Where(s => (s.itemName.ToLower().Contains(searchText) ||
                    s.unitName.ToLower().Contains(searchText)));

                    dg_itemsStorage.ItemsSource = itemLocationListQuery;
                    txt_count.Text = itemLocationListQuery.Count().ToString();
                }
            }
            else if (chk_stored.IsChecked == true || chk_freezone.IsChecked == true || chk_locked.IsChecked == true)
            {              
                searchText = tb_search.Text.ToLower();
                if (itemLocationList != null)
                {
                    itemLocationListQuery = itemLocationList.Where(s => (s.itemName.ToLower().Contains(searchText) ||
                    s.unitName.ToLower().Contains(searchText) ||
                    s.section.ToLower().Contains(searchText) ||
                    s.location.ToLower().Contains(searchText)));

                    dg_itemsStorage.ItemsSource = itemLocationListQuery;
                    txt_count.Text = itemLocationListQuery.Count().ToString();
                }
            }

        }
        private void Tgl_IsActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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

        private void Tgl_IsActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
        List<ItemLocation> locationsItems = new List<ItemLocation>();
        private async Task refreshItemsLocations()
        {
            //itemLocationList = await itemLocation.get(MainWindow.branchID.Value);
            locationsItems = await itemLocation.get(MainWindow.branchID.Value);
        }

        List<ItemLocation> freeZoneItems = new List<ItemLocation>();
        private async Task refreshFreeZoneItemsLocations()
        {
            //itemLocationList = await itemLocation.GetFreeZoneItems(MainWindow.branchID.Value);
            freeZoneItems = await itemLocation.GetFreeZoneItems(MainWindow.branchID.Value);
        }
        List<ItemLocation> lockedItems = new List<ItemLocation>();
        private async Task refreshLockedItems()
        {
            //itemLocationList = await itemLocation.GetLockedItems(MainWindow.branchID.Value);
            lockedItems = await itemLocation.GetLockedItems(MainWindow.branchID.Value);
        }

        List<ItemLocation> lackItems = new List<ItemLocation>();
        private async Task refreshLackItems()
        {
            //itemLocationList = await itemLocation.GetLackItems(MainWindow.branchID.Value);
            lackItems = await itemLocation.GetLackItems(MainWindow.branchID.Value);
        }
        private void translate()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            ////////////////////////////////----invoice----/////////////////////////////////
            dg_itemsStorage.Columns[0].Header = MainWindow.resourcemanager.GetString("trItemUnit");
            dg_itemsStorage.Columns[1].Header = MainWindow.resourcemanager.GetString("trSectionLocation");
            dg_itemsStorage.Columns[2].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_itemsStorage.Columns[3].Header = MainWindow.resourcemanager.GetString("trReserved");
            dg_itemsStorage.Columns[4].Header = MainWindow.resourcemanager.GetString("trStartDate");
            dg_itemsStorage.Columns[5].Header = MainWindow.resourcemanager.GetString("trEndDate");
            dg_itemsStorage.Columns[6].Header = MainWindow.resourcemanager.GetString("trNote");
            dg_itemsStorage.Columns[7].Header = MainWindow.resourcemanager.GetString("trOrderNum");



            dg_lack.Columns[0].Header = MainWindow.resourcemanager.GetString("trCharp");
            dg_lack.Columns[1].Header = MainWindow.resourcemanager.GetString("trType");
            dg_lack.Columns[2].Header = MainWindow.resourcemanager.GetString("trQTR");

            txt_itemsStorageHeader.Text = MainWindow.resourcemanager.GetString("trItemStorage");
            txt_Location.Text = MainWindow.resourcemanager.GetString("trLocationt");
            txt_date.Text = MainWindow.resourcemanager.GetString("expirationDate");
            txt_details.Text = MainWindow.resourcemanager.GetString("trDetails");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_itemName, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_quantity, MainWindow.resourcemanager.GetString("trQuantityHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_section, MainWindow.resourcemanager.GetString("trSectionHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_XYZ, MainWindow.resourcemanager.GetString("trLocation")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));


            chk_stored.Content = MainWindow.resourcemanager.GetString("trStored");
            chk_freezone.Content = MainWindow.resourcemanager.GetString("trFreeZone");
            chk_locked.Content = MainWindow.resourcemanager.GetString("trReserved");
            chk_lack.Content = MainWindow.resourcemanager.GetString("trLack");
            btn_transfer.Content = MainWindow.resourcemanager.GetString("trTransfer");
            btn_locked.Content = MainWindow.resourcemanager.GetString("trUnlock");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
        }

        private async Task refreshLocations()
        {
            if (cb_section.SelectedIndex != -1)
            {
                locations = await locationModel.getLocsBySectionId((int)cb_section.SelectedValue);
                locations = locations.Where(l => l.isActive == 1);
                cb_XYZ.ItemsSource = locations;
                cb_XYZ.SelectedValuePath = "locationId";
                cb_XYZ.DisplayMemberPath = "name";
            }
        }
        private async Task fillSections()
        {
            await FillCombo.FillComboBranchSections(cb_section);

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
  

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await refreshGrids();
                searsh();

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

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {

                dg_collapsed.IsEnabled = false;
                dg_collapsed.Opacity = 0.2;



            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            try
            {

                dg_collapsed.IsEnabled = true;
                dg_collapsed.Opacity = 1;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private bool validateMandatoryInputs()
        {
            SectionData.validateEmptyTextBox(tb_quantity, p_errorQuantity, tt_errorQuantity, "trEmptyQuantityToolTip");
            SectionData.validateEmptyComboBox(cb_section, p_errorSection, tt_errorSection, "trEmptySectionToolTip");
            SectionData.validateEmptyComboBox(cb_XYZ, p_errorXYZ, tt_errorXYZ, "trErrorEmptyLocationToolTip");
            if (itemLocation.isExpired)
            {
                if(dp_startDate.SelectedDate == null)
                    SectionData.showDatePickerValidate(dp_startDate, p_errorStartDate, tt_errorStartDate, "trEmptyStartDateToolTip");
                if (dp_endDate.SelectedDate == null)
                    SectionData.showDatePickerValidate(dp_endDate, p_errorEndDate, tt_errorEndDate, "trEmptyEndDateToolTip");
            }

            if (!tb_quantity.Text.Equals("") && cb_section.SelectedIndex != -1
                            && cb_XYZ.SelectedIndex != -1 && (!itemLocation.isExpired ||
                            (itemLocation.isExpired && dp_startDate.SelectedDate != null && dp_endDate.SelectedDate != null)))
                return true;
            else
                return false;
        }
        private async void Btn_locked_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(transferPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (dg_itemsStorage.SelectedIndex != -1)
                    {
                        if (itemLocation != null && !tb_quantity.Text.Equals(""))
                        {
                            int quantity = int.Parse(tb_quantity.Text);
                            ItemLocation newLocation = new ItemLocation();
                            newLocation.itemsLocId = itemLocation.itemsLocId;
                            newLocation.itemUnitId = itemLocation.itemUnitId;
                            newLocation.locationId = itemLocation.locationId;
                            newLocation.quantity = quantity;
                            newLocation.startDate = dp_startDate.SelectedDate;
                            newLocation.endDate = dp_endDate.SelectedDate;
                            newLocation.note = tb_notes.Text;
                            newLocation.updateUserId = MainWindow.userID.Value;
                            newLocation.createUserId = MainWindow.userID.Value;
                            int res = (int)await itemLocation.unlockItem(newLocation, MainWindow.branchID.Value);
                            if (res > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                            }
                            else 
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                            if (chk_stored.IsChecked == true)
                                await refreshItemsLocations();
                            else if (chk_freezone.IsChecked == true)
                                await refreshFreeZoneItemsLocations();
                            else if(chk_locked.IsChecked == true)
                            {
                                await refreshLockedItems();
                            }

                            clearInputs();
                            //}
                            //else
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTranseToSameLocation"), animation: ToasterAnimation.FadeIn);
                            Tb_search_TextChanged(null, null);
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
        private async void Btn_transfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(transferPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (dg_itemsStorage.SelectedIndex != -1)
                    {
                       bool valid = validateMandatoryInputs();
                        //if (itemLocation != null &&
                        //    !tb_quantity.Text.Equals("") && cb_section.SelectedIndex != -1
                        //    && cb_XYZ.SelectedIndex != -1 && (!itemLocation.isExpired ||
                        //    (itemLocation.isExpired && dp_startDate.SelectedDate != null && dp_endDate.SelectedDate != null)))
                        if(valid)
                        {
                            int oldLocationId = (int)itemLocation.locationId;
                            int newLocationId = (int)cb_XYZ.SelectedValue;
                            if (oldLocationId != newLocationId)
                            {
                                int quantity = int.Parse(tb_quantity.Text);
                                ItemLocation newLocation = new ItemLocation();
                                newLocation.itemUnitId = itemLocation.itemUnitId;
                                newLocation.invoiceId = itemLocation.invoiceId;
                                newLocation.locationId = newLocationId;
                                newLocation.quantity = quantity;
                                newLocation.startDate = dp_startDate.SelectedDate;
                                newLocation.endDate = dp_endDate.SelectedDate;
                                newLocation.note = tb_notes.Text;
                                newLocation.updateUserId = MainWindow.userID.Value;
                                newLocation.createUserId = MainWindow.userID.Value;

                                int res = (int)await itemLocation.trasnferItem(itemLocation.itemsLocId, newLocation);
                                if (res > 0)
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                                }
                                else if (res == -3) // كمية العنصر غير كافية
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountNotAvailableFromToolTip") , animation: ToasterAnimation.FadeIn);
                                else 
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                clearInputs();
                                await refreshGrids();
                                //if (chk_stored.IsChecked == true)
                                //    await refreshItemsLocations();
                                //else if (chk_freezone.IsChecked == true)
                                //    await refreshFreeZoneItemsLocations();
                                //else
                                //{ }

                              
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trTranseToSameLocation"), animation: ToasterAnimation.FadeIn);
                            Tb_search_TextChanged(null, null);
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
        private async void Cb_section_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                await refreshLocations();
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

        private async void Dg_itemsStorage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (dg_itemsStorage.SelectedIndex != -1)
                {
                    clearInputs();
                    itemLocation = dg_itemsStorage.SelectedItem as ItemLocation;
                    if (chk_lack.IsChecked == true)
                    {
                        var invoices = await invoiceModel.GetItemUnitOrders((int)itemLocation.itemUnitId, MainWindow.branchID.Value);

                        dg_lack.ItemsSource = null;
                        dg_lack.ItemsSource = invoices;
                    }
                    else
                    {
                        this.DataContext = itemLocation;
                        dp_startDate.SelectedDate = itemLocation.startDate;
                        dp_endDate.SelectedDate = itemLocation.endDate;
                        if (itemLocation.isExpired)
                        {
                            gd_date.Visibility = Visibility.Visible;
                            dock_date.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            gd_date.Visibility = Visibility.Collapsed;
                            dock_date.Visibility = Visibility.Collapsed;
                        }
                        #region order is Ready
                        if (itemLocation.invType == "or")
                            btn_locked.IsEnabled = false;
                        else
                            btn_locked.IsEnabled = true;
                        #endregion
                        if (chk_stored.IsChecked == true)
                        {
                            tb_itemName.IsReadOnly = true;
                            dp_endDate.IsEnabled = true;
                            dp_startDate.IsEnabled = true;
                            cb_section.SelectedValue = itemLocation.sectionId;
                            tb_quantity.Text = itemLocation.quantity.ToString();
                            await refreshLocations();
                            cb_XYZ.SelectedValue = itemLocation.locationId;
                        }
                        else if (chk_freezone.IsChecked == true)
                        {
                            dp_endDate.IsEnabled = true;
                            dp_startDate.IsEnabled = true;
                            cb_section.SelectedIndex = -1;
                            cb_XYZ.SelectedIndex = -1;
                            tb_quantity.Text = itemLocation.quantity.ToString();

                        }
                        else if (chk_locked.IsChecked == true)
                        {
                            tb_quantity.Text = itemLocation.lockedQuantity.ToString();
                            if (int.Parse(tb_quantity.Text) == 0)
                                btn_locked.IsEnabled = false;
                            else
                                btn_locked.IsEnabled = true;

                            cb_section.SelectedValue = itemLocation.sectionId;
                            await refreshLocations();
                            cb_XYZ.SelectedValue = itemLocation.locationId;

                        }
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

        private void Tb_quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                if (itemLocation != null && !tb_quantity.Text.Equals(""))
                {
                    if (int.Parse(tb_quantity.Text) > itemLocation.quantity)
                    {
                        tb_quantity.Text = itemLocation.quantity.ToString();
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorAmountIncreaseToolTip"), animation: ToasterAnimation.FadeIn);
                    }
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void clearInputs()
        {
            tb_itemName.Clear();
            tb_quantity.Clear();
            cb_section.SelectedIndex = -1;
            cb_XYZ.SelectedIndex = -1;
            dp_startDate.SelectedDate = null;
            dp_startDate.Text = "";
            dp_endDate.SelectedDate = null;
            dp_endDate.Text = "";
            tb_quantity.Text = "";
            tb_notes.Clear();
            itemLocation = new ItemLocation();

            if (gd_date.Visibility == Visibility.Visible)
            {
                TextBox tbStartDate = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                SectionData.clearValidate(tbStartDate, p_errorStartDate);
                TextBox tbEndDate = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                SectionData.clearValidate(tbEndDate, p_errorEndDate);
            }
            gd_date.Visibility = Visibility.Collapsed;
            dock_date.Visibility = Visibility.Collapsed;

            SectionData.clearComboBoxValidate(cb_section, p_errorSection);
            SectionData.clearComboBoxValidate(cb_XYZ, p_errorXYZ);
            SectionData.clearValidate(tb_quantity, p_errorQuantity);
            //if (gd_date.Visibility == Visibility.Visible)
            //{
                
            //}
        }

        private void Dp_date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (dp_endDate.SelectedDate != null && dp_startDate.SelectedDate != null)
                {
                    if (dp_endDate.SelectedDate < dp_startDate.SelectedDate)
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trErrorStartBeforEndToolTip"), animation: ToasterAnimation.FadeIn);
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
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_quantity")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorQuantity, tt_errorQuantity, "trEmptyQuantityToolTip");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_section")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorSection, tt_errorSection, "trEmptySectionToolTip");
                    else if ((sender as ComboBox).Name == "cb_XYZ")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorXYZ, tt_errorXYZ, "trErrorEmptyLocationToolTip");
                }
                else if (name == "DatePicker")
                {
                    if ((sender as DatePicker).Name == "dp_startDate")
                        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorStartDate, tt_errorStartDate, "trEmptyStartDateToolTip");
                    else if ((sender as DatePicker).Name == "dp_endDate")
                        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorEndDate, tt_errorEndDate, "trEmptyEndDateToolTip");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #region report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        string repTitle2 = "";
        string selectedChk = "";

        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                //if (chk_locked.IsChecked == true)
                //{
                //    addpath = @"\Reports\Store\Ar\ArStorageResReport.rdlc";
                //}
                //else
                //{
                    addpath = @"\Reports\Store\Ar\ArStorageReport.rdlc";
                //}



            }
            else
            {
                //if (chk_locked.IsChecked == true)
                //{
                //    addpath = @"\Reports\Store\EN\StorageResReport.rdlc";
                //}
                //else
                //{
                    addpath = @"\Reports\Store\EN\StorageReport.rdlc";
                //}

            }
            if (repTitle2 == "")
            {
                repTitle2 = "trStored";

            }
           if(selectedChk=="")
            {
                selectedChk = "stored";
            }
            //filter   

            //paramarr.Add(new ReportParameter("stateval", stateval));
            //    paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            paramarr.Add(new ReportParameter("Title", MainWindow.resourcemanagerreport.GetString("trStorage")));
            paramarr.Add(new ReportParameter("trItemUnit", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trReserved", MainWindow.resourcemanagerreport.GetString("trReserved")));
            paramarr.Add(new ReportParameter("trSectionLocation", MainWindow.resourcemanagerreport.GetString("trSectionLocation")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("trNote", MainWindow.resourcemanagerreport.GetString("trNote")));
            paramarr.Add(new ReportParameter("trOrderNum", MainWindow.resourcemanagerreport.GetString("trOrderNum")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("selectedChk", selectedChk));
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            /*
             stored
freezone
reserved
lack
* */
             
            ReportCls.checkLang();
            clsReports.setReportLanguage(paramarr);

            clsReports.itemLocation(itemLocationListQuery, rep, reppath, paramarr);
            paramarr.Add(new ReportParameter("Title2", MainWindow.resourcemanagerreport.GetString(repTitle2)));
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
                    if (itemLocationListQuery != null)
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
                    if (itemLocationListQuery != null)
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
                    if (itemLocationListQuery != null)
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
                    if (itemLocationListQuery != null)
                    {
                        //Thread t1 = new Thread(() =>
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
                        //t1.Start();
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

        private void search_Checking(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                CheckBox cb = sender as CheckBox;
                if (chk_freezone != null)
                {
                    if (cb.Name == "chk_stored")
                    {
                        chk_freezone.IsChecked = false;
                        chk_locked.IsChecked = false;
                        chk_lack.IsChecked = false;
                        btn_locked.Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[1].Visibility = Visibility.Visible;
                        dg_itemsStorage.Columns[6].Visibility = Visibility.Visible;

                        dg_itemsStorage.Columns[7].Visibility = Visibility.Collapsed; //make order num column unvisible
                        dg_itemsStorage.Columns[3].Visibility = Visibility.Collapsed; //reserved column
                        dg_itemsStorage.Columns[4].Visibility = Visibility.Visible;
                        dg_itemsStorage.Columns[5].Visibility = Visibility.Visible;
                        repTitle2 = "trStored";
                        selectedChk = "stored";

                    }
                    else if (cb.Name == "chk_freezone")
                    {
                        chk_stored.IsChecked = false;
                        chk_locked.IsChecked = false;
                        chk_lack.IsChecked = false;
                        btn_locked.Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[1].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[6].Visibility = Visibility.Visible;

                        dg_itemsStorage.Columns[7].Visibility = Visibility.Collapsed; //make order num column unvisible
                        dg_itemsStorage.Columns[3].Visibility = Visibility.Collapsed; //reserved column
                        dg_itemsStorage.Columns[4].Visibility = Visibility.Visible;
                        dg_itemsStorage.Columns[5].Visibility = Visibility.Visible;
                        repTitle2 = "trFreeZone";
                        selectedChk = "freezone";

                    }
                    else if(cb.Name == "chk_locked")
                    {
                        chk_stored.IsChecked = false;
                        chk_freezone.IsChecked = false;
                        chk_lack.IsChecked = false;
                        btn_locked.Visibility = Visibility.Visible;
                        dg_itemsStorage.Columns[1].Visibility = Visibility.Visible;
                        dg_itemsStorage.Columns[6].Visibility = Visibility.Visible;

                        dg_itemsStorage.Columns[7].Visibility = Visibility.Visible; //make order num column visible
                        dg_itemsStorage.Columns[3].Visibility = Visibility.Visible; //reserved column
                        dg_itemsStorage.Columns[4].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[5].Visibility = Visibility.Collapsed;
                        repTitle2 ="trReserved";
                        selectedChk = "reserved";
                    }
                    else if (cb.Name == "chk_lack")
                    {
                        chk_stored.IsChecked = false;
                        chk_freezone.IsChecked = false;
                        chk_locked.IsChecked = false;
                        btn_locked.Visibility = Visibility.Collapsed;

                        dg_itemsStorage.Columns[1].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[3].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[4].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[5].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[6].Visibility = Visibility.Collapsed;
                        dg_itemsStorage.Columns[7].Visibility = Visibility.Collapsed;

                        repTitle2 = "trLack";
                        selectedChk = "lack";
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
                    if (cb.Name == "chk_stored")
                        chk_stored.IsChecked = true;
                    else if (cb.Name == "chk_freezone")
                        chk_freezone.IsChecked = true;
                    else if(cb.Name == "chk_locked")
                        chk_locked.IsChecked = true;
                    else if (cb.Name == "chk_lack")
                        chk_lack.IsChecked = true;
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
                    if (itemLocationListQuery != null)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        win_Storagelvc win = new win_Storagelvc(itemLocationListQuery, 2);
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

        private async void Btn_saveDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(transferPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (dg_itemsStorage.SelectedIndex != -1)
                    {
                        if(SectionData.validateEmptyDatePicker(dp_startDate, p_errorStartDate, tt_errorStartDate ) && SectionData.validateEmptyDatePicker(dp_endDate, p_errorEndDate, tt_errorEndDate))
                        {

                        var startDate = dp_startDate.SelectedDate;
                        var endDate = dp_endDate.SelectedDate;
                        int res = (int)await itemLocation.changeUnitExpireDate(itemLocation.itemsLocId, (DateTime)startDate,(DateTime)endDate,MainWindow.userID.Value);
                        if (res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            //await searsh();
                            await refreshGrids();
                            searsh();
                        }
                       else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
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

        private void Cb_section_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = FillCombo.branchSectionsList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_XYZ_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = locations.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
