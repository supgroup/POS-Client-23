using LiveCharts;
using LiveCharts.Wpf;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Data.SqlClient;
using LiveCharts.Helpers;
using POS.View.windows;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using static POS.Classes.Statistics;
using System.Globalization;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Resources;

namespace POS.View.reports
{
    public partial class uc_stock : UserControl
    {
        #region variables
        List<Storage> storages;

        IEnumerable<Storage> storeLst;

        Statistics statisticModel = new Statistics();

        List<Branch> comboBranches;

        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private int selectedStockTab = 0;
        IEnumerable<itemCombo> comboItems;
        IEnumerable<unitCombo> comboUnits;
        IEnumerable<sectionCombo> comboSection;
        IEnumerable<locationCombo> comboLocation;

        #endregion

        private static uc_stock _instance;
        public static uc_stock Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_stock();
                return _instance;
            }
        }
        public uc_stock()
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

                col_reportChartWidth = col_reportChart.ActualWidth;

                storages = await statisticModel.GetStorage((int)MainWindow.branchID, (int)MainWindow.userID);

                comboItems = statisticModel.getItemCombo(storages);
                comboUnits = statisticModel.getUnitCombo(storages);
                comboSection = statisticModel.getSectionCombo(storages);
                comboLocation = statisticModel.getLocationCombo(storages);

                chk_allBranchesItem.IsChecked = true;
                chk_allBranchesLocation.IsChecked = true;
                chk_allBranchesCollect.IsChecked = true;
                chk_allBranchesExpired.IsChecked = true;

                chk_allItemsItem.IsChecked = true;
                chk_allSectionsLocation.IsChecked = true;
                chk_allItemsCollect.IsChecked = true;
                chk_allItemsExpired.IsChecked = true;

                chk_allUnitsItem.IsChecked = true;
                chk_allLocationsLocation.IsChecked = true;
                chk_allUnitsCollect.IsChecked = true;
                chk_allUnitsExpired.IsChecked = true;

                Btn_item_Click(btn_item, null);

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

        #region methods
        private void translate()
        {
            tt_item.Content = MainWindow.resourcemanager.GetString("trItems");
            tt_location.Content = MainWindow.resourcemanager.GetString("trLocations");
            tt_collect.Content = MainWindow.resourcemanager.GetString("trBestSeller");
            tt_expiredDate.Content = MainWindow.resourcemanager.GetString("trExpired");
            //items
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branchesItem, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemsItem, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_unitsItem, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDateItem, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDateItem, MainWindow.resourcemanager.GetString("trEndDateHint"));
            chk_expireDateItem.Content = MainWindow.resourcemanager.GetString("trExpired");
            chk_allBranchesItem.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItemsItem.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allUnitsItem.Content = MainWindow.resourcemanager.GetString("trAll");
            //location
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branchesLocation, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_sectionsLocation, MainWindow.resourcemanager.GetString("trSection"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_locationsLocation, MainWindow.resourcemanager.GetString("trLocation"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDateLocation, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDateLocation, MainWindow.resourcemanager.GetString("trExpired")+"...");
            chk_expireDateLocation.Content = MainWindow.resourcemanager.GetString("trExpired");
            chk_allBranchesLocation.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allSectionsLocation.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allLocationsLocation.Content = MainWindow.resourcemanager.GetString("trAll");
            //collect
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branchesCollect, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemsCollect, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_unitsCollect, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDateCollect, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDateCollect, MainWindow.resourcemanager.GetString("trEndDateHint"));
            chk_expireDateCollect.Content = MainWindow.resourcemanager.GetString("trExpired");
            chk_allBranchesCollect.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItemsCollect.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allUnitsCollect.Content = MainWindow.resourcemanager.GetString("trAll");
            //expired
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branchesExpired, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemsExpired, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_unitsExpired, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDateExpired, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDateExpired, MainWindow.resourcemanager.GetString("trExpiredDate")+"...");
            chk_expireDateExpired.Content = MainWindow.resourcemanager.GetString("trExpired");
            chk_allBranchesItem.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItemsItem.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allUnitsItem.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_section.Header = MainWindow.resourcemanager.GetString("trSection");
            col_location.Header = MainWindow.resourcemanager.GetString("trLocation");
            col_itemUnits.Header = MainWindow.resourcemanager.GetString("trItemUnit");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_locationSection.Header = MainWindow.resourcemanager.GetString("trSectionLocation");
            col_startDate.Header = MainWindow.resourcemanager.GetString("trStartDate");
            col_endDate.Header = MainWindow.resourcemanager.GetString("trExpiredDate");
            col_MinCollect.Header = MainWindow.resourcemanager.GetString("trMinStock");
            col_MaxCollect.Header = MainWindow.resourcemanager.GetString("trMaxStock");
            col_quantity.Header = MainWindow.resourcemanager.GetString("trQTR");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print3.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            grid_byItem.Visibility = Visibility.Hidden;
            grid_byLocation.Visibility = Visibility.Hidden;
            grid_collect.Visibility = Visibility.Hidden;
            grid_expired.Visibility = Visibility.Hidden;

            bdr_item.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_location.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_collect.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_expiredDate.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_item.Fill = Brushes.White;
            path_location.Fill = Brushes.White;
            path_collect.Fill = Brushes.White;
            path_expiredDate.Fill = Brushes.White;
        }
        List<itemCombo> itemsLst = new List<itemCombo>();
        private void fillComboItems(ComboBox cbBranches, ComboBox cbItems)
        {
            var temp = cbBranches.SelectedItem as Branch;
            cbItems.SelectedValuePath = "ItemId";
            cbItems.DisplayMemberPath = "ItemName";
            if (temp == null)
            {
                itemsLst = comboItems
                    .GroupBy(x => x.ItemId)
                    .Select(g => new itemCombo
                    {
                        ItemId = g.FirstOrDefault().ItemId,
                        ItemName = g.FirstOrDefault().ItemName,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();
            }
            else
            {
                itemsLst = comboItems
                    .Where(x => x.BranchId == temp.branchId)
                    .GroupBy(x => x.ItemId)
                    .Select(g => new itemCombo
                    {
                        ItemId = g.FirstOrDefault().ItemId,
                        ItemName = g.FirstOrDefault().ItemName,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();
            }
            cbItems.ItemsSource = itemsLst;
        }
        List<unitCombo> unitsLst = new List<unitCombo>();
        private void fillComboUnits(ComboBox cbItems, ComboBox cbUnits)
        {
            var temp = cbItems.SelectedItem as itemCombo;
            cbUnits.SelectedValuePath = "UnitId";
            cbUnits.DisplayMemberPath = "UnitName";
            if (temp == null)
            {
                unitsLst = comboUnits
                    .GroupBy(x => x.UnitId)
                    .Select(g => new unitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
            }
            else
            {
                unitsLst = comboUnits
                    .Where(x => x.ItemId == temp.ItemId && x.BranchId == temp.BranchId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new unitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList(); 
            }
            cbUnits.ItemsSource = unitsLst;
        }
        List<sectionCombo> sectionsLst = new List<sectionCombo>();
        private void fillComboSection()
        {
            var temp = cb_branchesLocation.SelectedItem as Branch;
            cb_sectionsLocation.SelectedValuePath = "SectionId";
            cb_sectionsLocation.DisplayMemberPath = "SectionName";
            var result = comboSection;
            if (temp == null)
            {
                sectionsLst = result.GroupBy(x => x.SectionId)
                   .Select(g => new sectionCombo { SectionId = g.FirstOrDefault().SectionId, SectionName = g.FirstOrDefault().SectionName, BranchId = g.FirstOrDefault().BranchId }).ToList();
                cb_sectionsLocation.ItemsSource = sectionsLst;
                //cb_sectionsLocation.ItemsSource = result.GroupBy(x => x.SectionId)
                //   .Select(g => new sectionCombo { SectionId = g.FirstOrDefault().SectionId, SectionName = g.FirstOrDefault().SectionName, BranchId = g.FirstOrDefault().BranchId }).ToList();
            }
            else
            {
                sectionsLst = result.Where(x => x.BranchId == temp.branchId).GroupBy(x => x.SectionId)
                   .Select(g => new sectionCombo { SectionId = g.FirstOrDefault().SectionId, SectionName = g.FirstOrDefault().SectionName, BranchId = g.FirstOrDefault().BranchId }).ToList();
                cb_sectionsLocation.ItemsSource = sectionsLst;
                //cb_sectionsLocation.ItemsSource = result.Where(x => x.BranchId == temp.branchId).GroupBy(x => x.SectionId)
                //   .Select(g => new sectionCombo { SectionId = g.FirstOrDefault().SectionId, SectionName = g.FirstOrDefault().SectionName, BranchId = g.FirstOrDefault().BranchId }).ToList();
            }

        }
        List<locationCombo> locationsnLst = new List<locationCombo>();
        private void fillComboLoaction()
        {
            var temp = cb_sectionsLocation.SelectedItem as sectionCombo;
            cb_locationsLocation.SelectedValuePath = "locationId";
            cb_locationsLocation.DisplayMemberPath = "LocationName";
            if (temp == null)
            {
                locationsnLst = comboLocation.GroupBy(x => x.LocationId)
                    .Select(g => new locationCombo { LocationId = g.FirstOrDefault().LocationId, LocationName = g.FirstOrDefault().LocationName, SectionId = g.FirstOrDefault().SectionId }).ToList();
                cb_locationsLocation.ItemsSource = locationsnLst;
                //cb_locationsLocation.ItemsSource = comboLocation.GroupBy(x => x.LocationId)
                //    .Select(g => new locationCombo { LocationId = g.FirstOrDefault().LocationId, LocationName = g.FirstOrDefault().LocationName, SectionId = g.FirstOrDefault().SectionId }).ToList();
            }
            else
            {
                locationsnLst = comboLocation.Where(x => x.SectionId == temp.SectionId && x.BranchId == temp.BranchId).GroupBy(x => x.LocationId)
                    .Select(g => new locationCombo { LocationId = g.FirstOrDefault().LocationId, LocationName = g.FirstOrDefault().LocationName, SectionId = g.FirstOrDefault().SectionId }).ToList();
                cb_locationsLocation.ItemsSource = locationsnLst;
                //cb_locationsLocation.ItemsSource = comboLocation.Where(x => x.SectionId == temp.SectionId && x.BranchId == temp.BranchId).GroupBy(x => x.LocationId)
                //    .Select(g => new locationCombo { LocationId = g.FirstOrDefault().LocationId, LocationName = g.FirstOrDefault().LocationName, SectionId = g.FirstOrDefault().SectionId }); ;
            }
        }
        private IEnumerable<Storage> fillList(IEnumerable<Storage> storages, ComboBox comboBranch, ComboBox comboItem, ComboBox comboUnit, DatePicker startDate, DatePicker endDate, CheckBox chkAllBranches, CheckBox chkAllItems, CheckBox chkAllUnits, CheckBox expireDate)
        {
            var selectedBranch = comboBranch.SelectedItem as Branch;
            var selectedItem = comboItem.SelectedItem as itemCombo;
            var selectedUnit = comboUnit.SelectedItem as unitCombo;
            var selectedSection = comboItem.SelectedItem as sectionCombo;
            var selectedLocation = comboUnit.SelectedItem as locationCombo;
            #region old
            //var result = storages.Where(x => (
            ////selectedStockTab != 1 ? (
            ////item & collect
            ////selectedStockTab == 0 || selectedStockTab == 2 ? (
            //////////////selectedStockTab != 3 ? 
            //////////////(
            //////////////   //(expireDate != null ? ((chk_expireDateItem.IsChecked == true ? (x.endDate != null) : true)
            //////////////   //&& (startDate.SelectedDate != null ? (x.startDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
            //////////////   //&& (endDate.SelectedDate != null ? (x.endDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)) : true)
            //////////////   //&& 
            //////////////   (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
            //////////////&& (comboItem.SelectedItem != null ? (x.itemId == selectedItem.ItemId) : true)
            //////////////&& (comboUnit.SelectedItem != null ? (x.unitId == selectedUnit.UnitId) : true)
            //////////////)
            ////location
            ////:
            ////selectedStockTab == 1 ? (
            ////    //expireDate.IsChecked == true ? (x.endDate != null) : true)
            ////    //&& 
            ////   (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
            ////&& (comboItem.SelectedItem != null ? (x.sectionId == selectedSection.SectionId) : true)
            ////&& (comboUnit.SelectedItem != null ? (x.locationId == selectedLocation.LocationId) : true)
            //////&& (startDate.SelectedDate != null ? x.startDate != null ? (x.startDate.Value.Date >= startDate.SelectedDate.Value.Date) : true :true)
            //////&& (endDate.SelectedDate != null ? (x.endDate.Value.Date <= endDate.SelectedDate.Value.Date) : true))
            ////)
            ////////////////////:
            ////expire
            //(x.isExpired == true 
            ////&& (startDate.SelectedDate != null ? (x.startDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
            ////&& (endDate.SelectedDate != null ? (x.endDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
            ////&& (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
            ////&& (comboItem.SelectedItem != null ? (x.itemId == selectedItem.ItemId) : true)
            ////&& (comboUnit.SelectedItem != null ? (x.unitId == selectedUnit.UnitId) : true)
            //)
            //));
            #endregion
            var result = storages.Where(x => (comboBranch.SelectedItem != null ? (x.branchId == selectedBranch.branchId) : true)
                                             &&
                                             (selectedStockTab == 1 ?
                                                                       (comboItem.SelectedItem   != null ? (x.sectionId == selectedSection.SectionId) : true)
                                                                    && (comboUnit.SelectedItem   != null ? (x.locationId == selectedLocation.LocationId) : true)
                                             : 
                                                                       (comboItem.SelectedItem   != null ? (x.itemId == selectedItem.ItemId) : true)
                                                                    && (comboUnit.SelectedItem   != null ? (x.unitId == selectedUnit.UnitId) : true)
                                             )
                                             &&
                                             (selectedStockTab == 3 ?
                                                                       x.isExpired == true
                                                                    && (startDate.SelectedDate != null ? x.endDate != null ? (x.endDate.Value.Date   >= startDate.SelectedDate.Value.Date) : true : true)
                                                                    && (endDate.SelectedDate   != null ? x.endDate != null ? (x.endDate.Value.Date   <= endDate.SelectedDate.Value.Date) : true : true)
                                             : true
                                             )
                                       );
            storeLst = result;
            return result;
        }
        private void fillEventsCall(object sender)
        {
            try
            {
                fillEvents();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void fillEvents()
        {
            if (selectedStockTab == 0)
                temp = fillList(storages, cb_branchesItem, cb_itemsItem, cb_unitsItem, dp_startDateItem, dp_endDateItem, chk_allBranchesItem, chk_allItemsItem, chk_allUnitsItem, chk_expireDateItem);

            else if (selectedStockTab == 1)
                temp = fillList(storages, cb_branchesLocation, cb_sectionsLocation, cb_locationsLocation, dp_startDateLocation, dp_endDateLocation, chk_allBranchesLocation, chk_allSectionsLocation, chk_allLocationsLocation, chk_expireDateLocation);

            else if (selectedStockTab == 2)
            {
                temp = fillList(storages, cb_branchesCollect, cb_itemsCollect, cb_unitsCollect, null, null, chk_allBranchesCollect, chk_allItemsCollect, chk_allUnitsCollect, null).GroupBy(x => new { x.branchId, x.itemUnitId })
                   .Select(s => new Storage
                   {
                       branchId = s.FirstOrDefault().branchId,
                       branchName = s.FirstOrDefault().branchName,
                       storeCost = s.FirstOrDefault().storeCost,
                       itemUnitId = s.FirstOrDefault().itemUnitId,
                       ItemUnits = s.FirstOrDefault().ItemUnits,
                       itemId = s.FirstOrDefault().itemId,
                       itemName = s.FirstOrDefault().itemName,
                       unitId = s.FirstOrDefault().unitId,
                       unitName = s.FirstOrDefault().unitName,
                       quantity = s.Sum(g => g.quantity),
                       minUnitName = s.FirstOrDefault().minUnitName + ": " + s.FirstOrDefault().min,
                       maxUnitName = s.FirstOrDefault().maxUnitName + ": " + s.FirstOrDefault().max,
                       itemType = s.FirstOrDefault().itemType
                   });
            }
            else if (selectedStockTab == 3)
                temp = fillList(storages, cb_branchesExpired, cb_itemsExpired, cb_unitsExpired, dp_startDateExpired, dp_endDateExpired, chk_allBranchesExpired, chk_allItemsExpired, chk_allUnitsExpired, chk_expireDateExpired);

            dgStock.ItemsSource = temp;

            txt_count.Text = dgStock.Items.Count.ToString();

            showSelectedTabColumn();

            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        private void fillEmptyEvents()
        {

            temp = new List<Storage>();

            dgStock.ItemsSource = temp;

            txt_count.Text = dgStock.Items.Count.ToString();

            showSelectedTabColumn();

            fillColumnChart(temp);
            fillPieChart();
            fillRowChart(temp);
        }
        private void fillComboBranches(ComboBox cb)
        {
            cb.SelectedValuePath = "branchId";
            cb.DisplayMemberPath = "name";
            cb.ItemsSource = comboBranches;
        }
        private void hideAllColumn()
        {
            col_branch.Visibility = Visibility.Hidden;
            col_item.Visibility = Visibility.Hidden;
            col_unit.Visibility = Visibility.Hidden;
            col_locationSection.Visibility = Visibility.Hidden;
            col_quantity.Visibility = Visibility.Hidden;
            col_startDate.Visibility = Visibility.Hidden;
            col_endDate.Visibility = Visibility.Hidden;
            col_location.Visibility = Visibility.Hidden;
            col_section.Visibility = Visibility.Hidden;
            col_itemUnits.Visibility = Visibility.Hidden;
            col_MaxCollect.Visibility = Visibility.Hidden;
            col_MinCollect.Visibility = Visibility.Hidden;

        }
        private void showSelectedTabColumn()
        {
            hideAllColumn();

            if (selectedStockTab == 0)
            {
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_locationSection.Visibility = Visibility.Visible;
            }
            else if (selectedStockTab == 1)
            {
                col_branch.Visibility = Visibility.Visible;
                col_section.Visibility = Visibility.Visible;
                col_location.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_itemUnits.Visibility = Visibility.Visible;
            }
            else if (selectedStockTab == 2)
            {
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_MinCollect.Visibility = Visibility.Visible;
                col_MaxCollect.Visibility = Visibility.Visible;
            }
            else if (selectedStockTab == 3)
            {
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_locationSection.Visibility = Visibility.Visible;
                col_startDate.Visibility = Visibility.Visible;
                col_endDate.Visibility = Visibility.Visible;
            }

        }

        #endregion

        #region events
        private void cb_branchesItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillComboItems(cb_branchesItem, cb_itemsItem);
                chk_allItemsItem.IsChecked = true;
                chk_allItemsItem.IsEnabled = true;

                fillEvents();

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
        private void cb_itemsItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allUnitsItem.IsEnabled = true;
                chk_allUnitsItem.IsChecked = true;
                fillComboUnits(cb_itemsItem, cb_unitsItem);

                fillEvents();

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
        private void chk_allBranchesItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //branch
                cb_branchesItem.IsEnabled = false;
                cb_branchesItem.SelectedItem = null;
                //item
                cb_itemsItem.IsEnabled = false;

                cb_itemsItem.SelectedItem = null;
                chk_allItemsItem.IsChecked = true;
                chk_allItemsItem.IsEnabled = true;
                //unit
                chk_allUnitsItem.IsChecked = true;
                chk_allUnitsItem.IsEnabled = true;
                cb_unitsItem.IsEnabled = false;
                cb_unitsItem.SelectedItem = null;

                cb_branchesItem.Text = "";
                cb_branchesItem.ItemsSource = SectionData.BranchesAllWithoutMainList;

                fillEvents();
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
        private void chk_allBranchesItem_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                //branch
                cb_branchesItem.IsEnabled = true;
                cb_branchesItem.SelectedItem = null;
                //item
               
                cb_itemsItem.SelectedItem = null;
                chk_allItemsItem.IsChecked = false;
                chk_allItemsItem.IsEnabled = false;
           
                //unit
                chk_allUnitsItem.IsChecked = false;
                chk_allUnitsItem.IsEnabled = false;
                cb_unitsItem.IsEnabled = false;
                cb_unitsItem.SelectedItem = null;
                cb_itemsItem.IsEnabled = false;
                fillEmptyEvents();
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
        private void chk_allItemsItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemsItem.IsEnabled = false;
                cb_itemsItem.SelectedItem = null;

                chk_allUnitsItem.IsChecked = true;
                chk_allUnitsItem.IsEnabled = true;
                cb_unitsItem.IsEnabled = false;
                cb_unitsItem.SelectedItem = null;
                cb_itemsItem.Text = "";
                cb_itemsItem.ItemsSource = itemsLst;
                fillEvents();
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
        private void chk_allItemsItem_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemsItem.IsEnabled = true;
                cb_itemsItem.SelectedItem = null;

                // chk_allUnitsItem.IsChecked = false;
                 fillComboItems(cb_branchesItem, cb_itemsItem);
                chk_allUnitsItem.IsEnabled = false;
                cb_unitsItem.IsEnabled = false;
                cb_unitsItem.SelectedItem = null;
                fillEmptyEvents();
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
        private void chk_allUnitsItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allUnitsItem.IsChecked = true;
                chk_allUnitsItem.IsEnabled = true;
                cb_unitsItem.IsEnabled = false;
                cb_unitsItem.SelectedItem = null;
                cb_unitsItem.Text = "";
                cb_unitsItem.ItemsSource = unitsLst;

                fillEvents();
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
        private void chk_allUnitsItem_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_unitsItem.IsEnabled = true;
              //  chk_allUnitsItem.IsChecked = false;
                chk_allUnitsItem.IsEnabled = true;
               // fillComboItems(cb_branchesItem, cb_itemsItem);
                //cb_unitsItem.SelectedItem = null;
                fillEvents();
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
        private void chk_expireDateItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                dp_endDateItem.IsEnabled = true;
                dp_startDateItem.IsEnabled = true;

                fillEvents();

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
        private void chk_expireDateItem_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                dp_endDateItem.IsEnabled = false;
                dp_startDateItem.IsEnabled = false;

                fillEvents();

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
        private void cb_branchesLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allSectionsLocation.IsEnabled = true;
                chk_allSectionsLocation.IsChecked = true;
                fillComboSection();

                fillEvents();

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
        private void cb_sectionsLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allLocationsLocation.IsEnabled = true;
                chk_allLocationsLocation.IsChecked = true;
                fillComboLoaction();

                fillEvents();

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
        private void chk_allBranchesLocation_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //branch
                cb_branchesLocation.IsEnabled = false;
                cb_branchesLocation.SelectedItem = null;
                //sectio
                cb_sectionsLocation.IsEnabled = false;

                cb_sectionsLocation.SelectedItem = null;
                chk_allSectionsLocation.IsChecked = true;
                chk_allSectionsLocation.IsEnabled = true;
                //location
                chk_allLocationsLocation.IsChecked = true;
                chk_allLocationsLocation.IsEnabled = true;
                cb_locationsLocation.IsEnabled = false;
                cb_locationsLocation.SelectedItem = null;
                cb_branchesLocation.Text = "";
                cb_branchesLocation.ItemsSource = SectionData.BranchesAllWithoutMainList;

                fillEvents();
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
        private void chk_allBranchesLocation_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //branch
                cb_branchesLocation.IsEnabled = true;
                cb_branchesLocation.SelectedItem = null;
                //section

                cb_sectionsLocation.SelectedItem = null;
                chk_allSectionsLocation.IsChecked = false;
                chk_allSectionsLocation.IsEnabled = false;

                //location
                chk_allLocationsLocation.IsChecked = false;
                chk_allLocationsLocation.IsEnabled = false;
                cb_locationsLocation.IsEnabled = false;
                cb_locationsLocation.SelectedItem = null;
                //sec
                cb_sectionsLocation.IsEnabled = false;
              
                fillEmptyEvents();
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
        private void chk_allSectionsLocation_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

               
                cb_sectionsLocation.IsEnabled = false;
                cb_sectionsLocation.SelectedItem = null;
                
                chk_allLocationsLocation.IsChecked = true;
                chk_allLocationsLocation.IsEnabled = true;
                cb_locationsLocation.IsEnabled = false;
                cb_locationsLocation.SelectedItem = null;
                cb_sectionsLocation.Text = "";
                cb_sectionsLocation.ItemsSource = sectionsLst;

                fillEvents();
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
        private void chk_allSectionsLocation_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                
                cb_sectionsLocation.IsEnabled = true;
                cb_sectionsLocation.SelectedItem = null;

                chk_allLocationsLocation.IsChecked = false;
                chk_allLocationsLocation.IsEnabled = false;
                cb_locationsLocation.IsEnabled = false;
                cb_locationsLocation.SelectedItem = null;
                fillComboSection();
                fillEmptyEvents();

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
        private void chk_allLocationsLocation_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_locationsLocation.IsEnabled = false;
                cb_locationsLocation.SelectedItem = null;

                chk_allLocationsLocation.IsChecked = true;
                chk_allLocationsLocation.IsEnabled = true;

                cb_locationsLocation.Text = "";
                cb_locationsLocation.ItemsSource = locationsnLst;
               
                fillEvents();
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
        private void chk_allLocationsLocation_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_locationsLocation.IsEnabled = true;

          
                chk_allLocationsLocation.IsChecked = false;
                chk_allLocationsLocation.IsEnabled = true;

                cb_locationsLocation.SelectedItem = null;

                fillEmptyEvents();
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
        private void chk_expireDateLocation_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                dp_endDateLocation.IsEnabled = true;
                dp_startDateLocation.IsEnabled = true;

                fillEvents();

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
        private void chk_expireDateLocation_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                dp_endDateLocation.IsEnabled = false;
                dp_startDateLocation.IsEnabled = false;

                fillEvents();

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
        private void cb_branchesCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allItemsCollect.IsEnabled = true;
                chk_allItemsCollect.IsChecked = true;
                fillComboItems(cb_branchesCollect, cb_itemsCollect);
                fillEvents();

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
        private void cb_itemsCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allUnitsCollect.IsEnabled = true;
                chk_allUnitsCollect.IsChecked = true;
                fillComboUnits(cb_itemsCollect, cb_unitsCollect);

                fillEvents();

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
        private void chk_allBranchesCollect_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //branch
                cb_branchesCollect.IsEnabled = false;
                cb_branchesCollect.SelectedItem = null;
                //item
                cb_itemsCollect.IsEnabled = false;

                cb_itemsCollect.SelectedItem = null;
                chk_allItemsCollect.IsChecked = true;
                chk_allItemsCollect.IsEnabled = true;
                //unit
                chk_allUnitsCollect.IsChecked = true;
                chk_allUnitsCollect.IsEnabled = true;
                cb_unitsCollect.IsEnabled = false;
                cb_unitsCollect.SelectedItem = null;

                cb_branchesCollect.Text = "";
                cb_branchesCollect.ItemsSource = SectionData.BranchesAllWithoutMainList;
                fillEvents();


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
        private void chk_allBranchesCollect_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                
                //branch
                cb_branchesCollect.IsEnabled = true;
                cb_branchesCollect.SelectedItem = null;
                //item

                cb_itemsCollect.SelectedItem = null;
                chk_allItemsCollect.IsChecked = false;
                chk_allItemsCollect.IsEnabled = false;

                //unit
                chk_allUnitsCollect.IsChecked = false;
                chk_allUnitsCollect.IsEnabled = false;
                cb_unitsCollect.IsEnabled = false;
                cb_unitsCollect.SelectedItem = null;
                cb_itemsCollect.IsEnabled = false;
                fillEmptyEvents();

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
        private void chk_allItemsCollect_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemsCollect.IsEnabled = false;
                cb_itemsCollect.SelectedItem = null;
                cb_itemsCollect.Text = "";
                cb_itemsCollect.ItemsSource = itemsLst;

                chk_allUnitsCollect.IsChecked = true;
                chk_allUnitsCollect.IsEnabled = true;
                cb_unitsCollect.IsEnabled = false;
                cb_unitsCollect.SelectedItem = null;
                fillEvents();
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
        private void chk_allItemsCollect_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemsCollect.IsEnabled = true;


                cb_itemsCollect.SelectedItem = null;

                chk_allUnitsCollect.IsChecked = false;
                chk_allUnitsCollect.IsEnabled = false;
                cb_unitsCollect.IsEnabled = false;
                cb_unitsCollect.SelectedItem = null;
                fillComboItems(cb_branchesCollect, cb_itemsCollect);
                fillEmptyEvents();

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
        private void chk_allUnitsCollect_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allUnitsCollect.IsChecked = true;
                chk_allUnitsCollect.IsEnabled = true;
                cb_unitsCollect.IsEnabled = false;
                cb_unitsCollect.SelectedItem = null;
                cb_unitsCollect.Text = "";
                cb_unitsCollect.ItemsSource = unitsLst;
                fillEvents();
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
        private void chk_allUnitsCollect_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_unitsCollect.IsEnabled = true;
                chk_allUnitsCollect.IsChecked = false;
                chk_allUnitsCollect.IsEnabled = true;

                cb_unitsCollect.SelectedItem = null;
                fillEmptyEvents();

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
        private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void Cb_branchesExpired_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillComboItems(cb_branchesExpired, cb_itemsExpired);
                chk_allItemsExpired.IsChecked = true;
                chk_allItemsExpired.IsEnabled = true;

                fillEvents();

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
        private void Chk_allBranchesExpired_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //branch
                cb_branchesExpired.IsEnabled = false;
                cb_branchesExpired.SelectedItem = null;
                cb_branchesExpired.Text = "";
                cb_branchesExpired.ItemsSource = SectionData.BranchesAllWithoutMainList;
                //item
                cb_itemsExpired.IsEnabled = false;

                cb_itemsExpired.SelectedItem = null;
                chk_allItemsExpired.IsChecked = true;
                chk_allItemsExpired.IsEnabled = true;
                //unit
                chk_allUnitsExpired.IsChecked = true;
                chk_allUnitsExpired.IsEnabled = true;
                cb_unitsExpired.IsEnabled = false;
                cb_unitsExpired.SelectedItem = null;

                fillEvents();
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
        private void Chk_allBranchesExpired_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //branch
                cb_branchesExpired.IsEnabled = true;
                cb_branchesExpired.SelectedItem = null;

                //item
                cb_itemsExpired.SelectedItem = null;
                chk_allItemsExpired.IsChecked = false;
                chk_allItemsExpired.IsEnabled = false;

                //unit
                chk_allUnitsExpired.IsChecked = false;
                chk_allUnitsExpired.IsEnabled = false;
                cb_unitsExpired.IsEnabled = false;
                cb_unitsExpired.SelectedItem = null;
                cb_itemsExpired.IsEnabled = false;

                fillEmptyEvents();

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
        private void Cb_itemsExpired_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allUnitsExpired.IsEnabled = true;
                chk_allUnitsExpired.IsChecked = true;
                fillComboUnits(cb_itemsExpired, cb_unitsExpired);

                fillEvents();

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
        private void Chk_allItemsExpired_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemsExpired.IsEnabled = false;
                cb_itemsExpired.SelectedItem = null;
                cb_itemsExpired.Text = "";
                cb_itemsExpired.ItemsSource = itemsLst;

                chk_allUnitsExpired.IsChecked = true;
                chk_allUnitsExpired.IsEnabled = true;
                cb_unitsExpired.IsEnabled = false;
                cb_unitsExpired.SelectedItem = null;

                fillEvents();

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
        private void Chk_allItemsExpired_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemsExpired.IsEnabled = true;
                cb_itemsExpired.SelectedItem = null;

                // chk_allUnitsItem.IsChecked = false;
                fillComboItems(cb_branchesExpired, cb_itemsExpired);
                chk_allUnitsExpired.IsEnabled = false;
                cb_unitsExpired.IsEnabled = false;
                cb_unitsExpired.SelectedItem = null;
                fillEmptyEvents();
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
        private void Cb_unitsExpired_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void Chk_allUnitsExpired_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allUnitsExpired.IsChecked = true;
                chk_allUnitsExpired.IsEnabled = true;
                cb_unitsExpired.IsEnabled = false;
                cb_unitsExpired.SelectedItem = null;
                cb_unitsExpired.Text = "";
                cb_unitsExpired.ItemsSource = unitsLst;

                fillEvents();

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
        private void Chk_allUnitsExpired_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_unitsExpired.IsEnabled = true;
                chk_allUnitsExpired.IsEnabled = true;

                fillEvents();

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
        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (selectedStockTab == 0)
                {
                    temp = storeLst
                        .Where(s => (s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.Secname.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.LoactionName.ToLower().Contains(txt_search.Text.ToLower())
                       ));
                }
                else if (selectedStockTab == 1)
                {

                    temp = storeLst

                      .Where(s => (s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.Secname.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.LoactionName.ToLower().Contains(txt_search.Text.ToLower())
                      ));
                }
                else
                {
                    temp = storeLst
                       .Where(s => (s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.unitName.ToLower().Contains(txt_search.Text.ToLower())

                       ));
                    storeLst = temp;
                   
                }
                dgStock.ItemsSource = temp;
                txt_count.Text = dgStock.Items.Count.ToString();
                fillColumnChart();
                fillPieChart();
                fillRowChart();

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
        private async void btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                storages = await statisticModel.GetStorage((int)MainWindow.branchID, (int)MainWindow.userID);

                txt_search.Text = "";

                if (selectedStockTab == 0)
                {
                    cb_branchesItem.SelectedItem = null;
                    cb_itemsItem.SelectedItem = null;
                    cb_unitsItem.SelectedItem = null;
                    chk_allBranchesItem.IsChecked = true;
                    chk_expireDateItem.IsChecked = false;
                    dp_endDateItem.SelectedDate = null;
                    dp_startDateItem.SelectedDate = null;
                }
                else if (selectedStockTab == 1)
                {
                    cb_branchesLocation.SelectedItem = null;
                    cb_locationsLocation.SelectedItem = null;
                    cb_sectionsLocation.SelectedItem = null;
                    chk_allBranchesLocation.IsChecked = true;
                    chk_expireDateLocation.IsChecked = false;
                    dp_endDateLocation.SelectedDate = null;
                    dp_startDateLocation.SelectedDate = null;
                    chk_allBranchesLocation_Checked(chk_allBranchesLocation, null);
                }
                else if (selectedStockTab == 2)
                {
                    cb_branchesCollect.SelectedItem = null;
                    cb_itemsCollect.SelectedItem = null;
                    cb_unitsCollect.SelectedItem = null;
                    chk_allBranchesCollect.IsChecked = true;
                    chk_expireDateCollect.IsChecked = false;
                    dp_endDateCollect.SelectedDate = null;
                    dp_startDateCollect.SelectedDate = null;
                    chk_allBranchesCollect_Checked(chk_allBranchesCollect, null);
                }
                else
                {
                    cb_branchesExpired.SelectedItem = null;
                    cb_itemsExpired.SelectedItem = null;
                    cb_unitsExpired.SelectedItem = null;
                    chk_allBranchesExpired.IsChecked = true;
                    chk_expireDateExpired.IsChecked = false;
                    dp_endDateExpired.SelectedDate = null;
                    dp_startDateExpired.SelectedDate = null;
                }
                fillEvents();
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
        private void Cb_branchesItem_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_itemsItem_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                if(selectedStockTab == 1)
                    combo.ItemsSource = sectionsLst.Where(p => p.SectionName.ToLower().Contains(tb.Text.ToLower())).ToList();
                else
                    combo.ItemsSource = itemsLst.Where(p => p.ItemName.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_unitsItem_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                if(selectedStockTab == 1)
                    combo.ItemsSource = locationsnLst.Where(p => p.LocationName.ToLower().Contains(tb.Text.ToLower())).ToList();
                else
                    combo.ItemsSource = unitsLst.Where(p => p.UnitName.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        IEnumerable<Storage> temp = null;
        #endregion

        #region tabs
        private async void Btn_item_Click(object sender, RoutedEventArgs e)
        {//items
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                //fillComboBranches(cb_branchesItem);
                await SectionData.fillBranchesWithoutMain(cb_branchesItem);
                chk_allBranchesItem.IsChecked = true;

                selectedStockTab = 0;
                txt_search.Text = "";
                chk_allBranchesItem.IsChecked = true;
                chk_allItemsItem.IsChecked = true;
                chk_allUnitsItem.IsChecked = true;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_item);
                grid_byItem.Visibility = Visibility.Visible;
                path_item.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                chk_allBranchesItem_Checked(chk_allBranchesItem, null);

                #region key up
                // key_up branch
                cb_branchesItem.IsTextSearchEnabled = false;
                cb_branchesItem.IsEditable = true;
                cb_branchesItem.StaysOpenOnEdit = true;
                cb_branchesItem.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branchesItem.Text = "";
                // key_up item
                cb_itemsItem.IsTextSearchEnabled = false;
                cb_itemsItem.IsEditable = true;
                cb_itemsItem.StaysOpenOnEdit = true;
                cb_itemsItem.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_itemsItem.Text = "";
                // key_up unit
                cb_unitsItem.IsTextSearchEnabled = false;
                cb_unitsItem.IsEditable = true;
                cb_unitsItem.StaysOpenOnEdit = true;
                cb_unitsItem.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_unitsItem.Text = "";
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
        private async void btn_location_Click(object sender, RoutedEventArgs e)
        {//location
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                selectedStockTab = 1;
                txt_search.Text = "";
                chk_allBranchesLocation.IsChecked = true;
                chk_allSectionsLocation.IsChecked = true;
                chk_allLocationsLocation.IsChecked = true;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_location);
                grid_byLocation.Visibility = Visibility.Visible;
                path_location.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await SectionData.fillBranchesWithoutMain(cb_branchesLocation);

                chk_allBranchesLocation_Checked(chk_allBranchesLocation, null);

                #region key up
                // key_up branch
                cb_branchesLocation.IsTextSearchEnabled = false;
                cb_branchesLocation.IsEditable = true;
                cb_branchesLocation.StaysOpenOnEdit = true;
                cb_branchesLocation.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branchesLocation.Text = "";
                // key_up section
                cb_sectionsLocation.IsTextSearchEnabled = false;
                cb_sectionsLocation.IsEditable = true;
                cb_sectionsLocation.StaysOpenOnEdit = true;
                cb_sectionsLocation.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_sectionsLocation.Text = "";
                // key_up location
                cb_locationsLocation.IsTextSearchEnabled = false;
                cb_locationsLocation.IsEditable = true;
                cb_locationsLocation.StaysOpenOnEdit = true;
                cb_locationsLocation.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_locationsLocation.Text = "";
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
        private async void btn_collect_Click(object sender, RoutedEventArgs e)
        {//collect
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedStockTab = 2;
                txt_search.Text = "";
                chk_allBranchesCollect.IsChecked = true;
                chk_allItemsCollect.IsChecked = true;
                chk_allUnitsCollect.IsChecked = true;
                col_endDate.Visibility = Visibility.Hidden;
                col_startDate.Visibility = Visibility.Hidden;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_collect);
                grid_collect.Visibility = Visibility.Visible;
                path_collect.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await SectionData.fillBranchesWithoutMain(cb_branchesCollect);

                showSelectedTabColumn();
                chk_allBranchesCollect_Checked(chk_allBranchesCollect, null);

                #region key up
                // key_up branch
                cb_branchesCollect.IsTextSearchEnabled = false;
                cb_branchesCollect.IsEditable = true;
                cb_branchesCollect.StaysOpenOnEdit = true;
                cb_branchesCollect.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branchesCollect.Text = "";
                // key_up section
                cb_itemsCollect.IsTextSearchEnabled = false;
                cb_itemsCollect.IsEditable = true;
                cb_itemsCollect.StaysOpenOnEdit = true;
                cb_itemsCollect.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_itemsCollect.Text = "";
                // key_up location
                cb_unitsCollect.IsTextSearchEnabled = false;
                cb_unitsCollect.IsEditable = true;
                cb_unitsCollect.StaysOpenOnEdit = true;
                cb_unitsCollect.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_unitsCollect.Text = "";
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
        private async void Btn_expiredDate_Click(object sender, RoutedEventArgs e)
        {//expired date
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedStockTab = 3;
                txt_search.Text = "";
                chk_allBranchesExpired.IsChecked = true;
                chk_allItemsExpired.IsChecked = true;
                chk_allUnitsExpired.IsChecked = true;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_expiredDate);
                grid_expired.Visibility = Visibility.Visible;
                path_expiredDate.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await SectionData.fillBranchesWithoutMain(cb_branchesExpired);

                showSelectedTabColumn();
                Chk_allBranchesExpired_Checked(chk_allBranchesExpired, null);

                #region key up
                // key_up branch
                cb_branchesExpired.IsTextSearchEnabled = false;
                cb_branchesExpired.IsEditable = true;
                cb_branchesExpired.StaysOpenOnEdit = true;
                cb_branchesExpired.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branchesExpired.Text = "";
                // key_up section
                cb_itemsExpired.IsTextSearchEnabled = false;
                cb_itemsExpired.IsEditable = true;
                cb_itemsExpired.StaysOpenOnEdit = true;
                cb_itemsExpired.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_itemsExpired.Text = "";
                // key_up location
                cb_unitsExpired.IsTextSearchEnabled = false;
                cb_unitsExpired.IsEditable = true;
                cb_unitsExpired.StaysOpenOnEdit = true;
                cb_unitsExpired.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_unitsExpired.Text = "";
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
        #endregion

        #region charts
        private void fillRowChart(IEnumerable<Storage> List)
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;

            //temp = List;

            var result = temp.GroupBy(s => new { s.sectionId, s.locationId }).Select(s => new
            {
                itemId = s.FirstOrDefault().itemId,
                quantity = s.Sum(g => g.quantity)
            }
        );
            var resultTotal = result.Select(x => new { x.itemId, total = x.quantity }).ToList();
            pTemp = result.Select(x => (decimal)x.quantity);

            var tempName = temp.GroupBy(s => new { s.locationId, s.Secname }).Select(s => new
            {
                locationName = s.FirstOrDefault().branchName + "\n" + s.FirstOrDefault().Secname + " - " + s.FirstOrDefault().x + s.FirstOrDefault().y + s.FirstOrDefault().z
            });
            names.AddRange(tempName.Select(nn => nn.locationName));

            SeriesCollection rowChartData = new SeriesCollection();
            List<decimal> purchase = new List<decimal>();
            for (int i = 0; i < pTemp.Count(); i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            rowChartData.Add(
          new LineSeries
          {
              Values = purchase.AsChartValues(),
              Title = "Items Quantity"

          });
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void fillColumnChart(IEnumerable<Storage> List)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            //temp = List;

            var result = temp.GroupBy(s => new { s.itemId, s.unitId }).Select(s => new Storage
            {
                branchId = s.FirstOrDefault().branchId,
                itemId = s.FirstOrDefault().itemId,
                unitId = s.FirstOrDefault().unitId,
                quantity = s.Sum(g => g.quantity)
            });
            var cc = result.Select(m => new { m.itemId, m.quantity, m.unitId });

            var tempName = temp.GroupBy(s => new { s.itemName, s.unitName }).Select(s => new
            {
                itemName = s.FirstOrDefault().itemName,
                unitName = s.FirstOrDefault().unitName
            });
            names.AddRange(tempName.Select(nn => nn.itemName + " - " + nn.unitName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cP = new List<decimal>();
            List<decimal> cPb = new List<decimal>();
            List<decimal> cD = new List<decimal>();


            for (int i = 0; i < cc.Count(); i++)
            {
                cP.Add(cc.ToList().Select(jj => (decimal)jj.quantity).Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                Title = MainWindow.resourcemanager.GetString("trQuantity"),
                DataLabels = true,
            });
            DataContext = this;
            cartesianChart.Series = columnChartData;
            fillRowChart(temp);
        }
        private void fillRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;

            //var result = storeLst.GroupBy(s => new { s.sectionId, s.locationId }).Select(s => new
            var result = temp.GroupBy(s => new { s.sectionId, s.locationId }).Select(s => new
            {
                itemId = s.FirstOrDefault().itemId,
                quantity = s.Sum(g => g.quantity)
            }
            );
            var resultTotal = result.Select(x => new { x.itemId, total = x.quantity }).ToList();
            pTemp = result.Select(x => (decimal)x.quantity);

            var tempName = temp.GroupBy(s => new { s.locationId, s.Secname }).Select(s => new
            {
                locationName = s.FirstOrDefault().branchName + "\n" + s.FirstOrDefault().Secname + " - " + s.FirstOrDefault().x + s.FirstOrDefault().y + s.FirstOrDefault().z
            });
            names.AddRange(tempName.Select(nn => nn.locationName));

            SeriesCollection rowChartData = new SeriesCollection();
            List<decimal> purchase = new List<decimal>();
            int xCount = 6;
            if (pTemp.Count() <= 6) xCount = pTemp.Count();
            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (pTemp.Count() > 6)
            {
                decimal d = 0;
                for (int i = 6; i < pTemp.Count(); i++)
                {
                    d = d + pTemp.ToList().Skip(i).FirstOrDefault();
                }
                if (d != 0)
                {
                    purchase.Add(d);
                    MyAxis.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }

            rowChartData.Add(
          new LineSeries
          {
              Values = purchase.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("trQuantity")

          });
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            //var result = storeLst.GroupBy(s => new { s.itemId, s.unitId }).Select(s => new Storage
            var result = temp.GroupBy(s => new { s.itemId, s.unitId }).Select(s => new Storage
            {
                branchId = s.FirstOrDefault().branchId,
                itemId = s.FirstOrDefault().itemId,
                unitId = s.FirstOrDefault().unitId,
                quantity = s.Sum(g => g.quantity)
            });
            var cc = result.Select(m => new { m.itemId, m.quantity, m.unitId });

            var tempName = temp.GroupBy(s => new { s.itemName, s.unitName }).Select(s => new
            {
                itemName = s.FirstOrDefault().itemName,
                unitName = s.FirstOrDefault().unitName
            });
            names.AddRange(tempName.Select(nn => nn.itemName + " - " + nn.unitName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cP = new List<decimal>();

            int xCount = 6;
            if (cc.Count() <= 6) xCount = cc.Count();
            for (int i = 0; i < xCount; i++)
            {
                cP.Add(cc.ToList().Select(jj => (decimal)jj.quantity).Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (cc.Count() > 6)
            {
                decimal d = 0;
                for (int i = 6; i < cc.Count(); i++)
                {
                    d = d + cc.ToList().Select(jj => (decimal)jj.quantity).Skip(i).FirstOrDefault();
                }
                if (d != 0)
                {
                    cP.Add(d);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                Title = MainWindow.resourcemanager.GetString("trQuantity"),
                DataLabels = true,
            });
            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<decimal> x = new List<decimal>();
            titles.Clear();

            //var titleTemp = storeLst.GroupBy(m => m.branchName);
            var titleTemp = temp.GroupBy(m => m.branchName);

            titles.AddRange(titleTemp.Select(jj => jj.Key));
            var result = temp.GroupBy(s => s.branchId).Select(s => new Storage
            {
                branchId = s.FirstOrDefault().branchId,
                quantity = s.Sum(g => g.quantity),
                branchName = s.FirstOrDefault().branchName,
            }).OrderByDescending(s => s.quantity);
            x = result.Select(m => (decimal)m.quantity).ToList();
            int count = x.Count();
            titles = result.Select(m => m.branchName).ToList();
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < count; i++)
            {
                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();
                if (i < 5)
                {
                    final.Add(x.Max());
                    lable.Add(titles.Skip(i).FirstOrDefault());
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = lable.FirstOrDefault(),
                          DataLabels = true,
                      }
                  );
                    x.Remove(x.Max());
                }
                else
                {
                    final.Add(x.Sum());
                    piechartData.Add(
                      new PieSeries
                      {
                          Values = final.AsChartValues(),
                          Title = MainWindow.resourcemanager.GetString("trOthers"),
                          DataLabels = true,
                      }
                  );
                    break;
                }

            }
            chart1.Series = piechartData;
        }
        #endregion
        
        #region report
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "stock";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string expired = "";
            string startDate = "";
            string endDate = "";
            string branchval="";
            string itemval = "";
            string unitval = "";
            string searchval = "";
            
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedStockTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\Ar\ArItem.rdlc";

                    secondTitle = "items";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    //expired = chk_expireDateItem.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
             //startDate = dp_startDateItem.SelectedDate != null ? SectionData.DateToString(dp_startDateItem.SelectedDate) : "";
                  
                    //endDate = dp_endDateItem.SelectedDate != null ? SectionData.DateToString(dp_endDateItem.SelectedDate) : "";
                   
                   // cb_branchesItem.Text 
                  //  branchval = .SelectedValue;
                  branchval= cb_branchesItem.SelectedItem != null 
                        && (chk_allBranchesItem.IsChecked==false|| chk_allBranchesItem.IsChecked == null)
                        ? cb_branchesItem.Text :(chk_allBranchesItem.IsChecked == true? all:"");

                    itemval = cb_itemsItem.SelectedItem != null
                       && (chk_allItemsItem.IsChecked == false || chk_allItemsItem.IsChecked == null)
                       ? cb_itemsItem.Text : (chk_allItemsItem.IsChecked == true ? all : "")  ;

                    unitval = cb_unitsItem.SelectedItem != null
                      && (chk_allUnitsItem.IsChecked == false || chk_allUnitsItem.IsChecked == null)
                      ? cb_unitsItem.Text : (chk_allUnitsItem.IsChecked == true ? all : "")  ;
                }
                else if (selectedStockTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\Ar\ArLocation.rdlc";
                    secondTitle = "location";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trSection")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trLocation")));
                    //expired = chk_expireDateLocation.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
                   
                    //startDate = dp_startDateLocation.SelectedDate != null ? SectionData.DateToString(dp_startDateLocation.SelectedDate) : "";
                
                    //endDate = dp_endDateLocation.SelectedDate != null ? SectionData.DateToString(dp_endDateLocation.SelectedDate) : "";

                    branchval = cb_branchesLocation.SelectedItem != null
                 && (chk_allBranchesLocation.IsChecked == false || chk_allBranchesLocation.IsChecked == null)
                 ? cb_branchesLocation.Text : (chk_allBranchesLocation.IsChecked == true ? all : "") ;

                    itemval = cb_sectionsLocation.SelectedItem != null
                       && (chk_allSectionsLocation.IsChecked == false || chk_allSectionsLocation.IsChecked == null)
                       ? cb_sectionsLocation.Text : (chk_allSectionsLocation.IsChecked == true ? all : "") ;

                    unitval = cb_locationsLocation.SelectedItem != null
                      && (chk_allLocationsLocation.IsChecked == false || chk_allLocationsLocation.IsChecked == null)
                      ? cb_locationsLocation.Text : (chk_allLocationsLocation.IsChecked == true ? all : "") ;
                }
                else if (selectedStockTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\Ar\ArCollect.rdlc";
                    secondTitle = "collect";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    //expired = chk_expireDateCollect.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
                   

                    //startDate = dp_startDateCollect.SelectedDate != null ? SectionData.DateToString(dp_startDateCollect.SelectedDate) : "";
                  
                    //endDate = dp_endDateCollect.SelectedDate != null ? SectionData.DateToString(dp_endDateCollect.SelectedDate) : "";

                    branchval = cb_branchesCollect.SelectedItem != null
                && (chk_allBranchesCollect.IsChecked == false || chk_allBranchesCollect.IsChecked == null)
                ? cb_branchesCollect.Text : (chk_allBranchesCollect.IsChecked == true ? all : "") ;

                    itemval = cb_itemsCollect.SelectedItem != null
                       && (chk_allItemsCollect.IsChecked == false || chk_allItemsCollect.IsChecked == null)
                       ? cb_itemsCollect.Text : (chk_allItemsCollect.IsChecked == true ? all : "") ;

                    unitval = cb_unitsCollect.SelectedItem != null
                      && (chk_allUnitsCollect.IsChecked == false || chk_allUnitsCollect.IsChecked == null)
                      ? cb_unitsCollect.Text : (chk_allUnitsCollect.IsChecked == true ? all : "") ;


                }
                else if (selectedStockTab ==3)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\Ar\ArExpierd.rdlc";

                    secondTitle = "trExpired";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    //expired = chk_expireDateItem.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";


                    startDate = dp_startDateExpired.SelectedDate != null ? SectionData.DateToString(dp_startDateExpired.SelectedDate) : "";

                    endDate = dp_endDateExpired.SelectedDate != null ? SectionData.DateToString(dp_endDateExpired.SelectedDate) : "";

                    // cb_branchesItem.Text 
                    //  branchval = .SelectedValue;
                    branchval = cb_branchesExpired.SelectedItem != null
                          && (chk_allBranchesExpired.IsChecked == false || chk_allBranchesExpired.IsChecked == null)
                          ? cb_branchesExpired.Text : (chk_allBranchesExpired.IsChecked == true ? all : "");

                    itemval = cb_itemsExpired.SelectedItem != null
                       && (chk_allItemsExpired.IsChecked == false || chk_allItemsExpired.IsChecked == null)
                       ? cb_itemsExpired.Text : (chk_allItemsExpired.IsChecked == true ? all : "");

                    unitval = cb_unitsExpired.SelectedItem != null
                      && (chk_allUnitsExpired.IsChecked == false || chk_allUnitsExpired.IsChecked == null)
                      ? cb_unitsExpired.Text : (chk_allUnitsExpired.IsChecked == true ? all : "");
                }
            }
            else
            {
                if (selectedStockTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\En\Item.rdlc";
                    secondTitle = "items";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    //expired = chk_expireDateItem.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
             

                    //startDate = dp_startDateItem.SelectedDate != null ? SectionData.DateToString(dp_startDateItem.SelectedDate) : "";
             
                    //endDate = dp_endDateItem.SelectedDate != null ? SectionData.DateToString(dp_endDateItem.SelectedDate) : "";

                    branchval = cb_branchesItem.SelectedItem != null
                    && (chk_allBranchesItem.IsChecked == false || chk_allBranchesItem.IsChecked == null)
                    ? cb_branchesItem.Text : (chk_allBranchesItem.IsChecked == true ? all : "") ;

                    itemval = cb_itemsItem.SelectedItem != null
                       && (chk_allItemsItem.IsChecked == false || chk_allItemsItem.IsChecked == null)
                       ? cb_itemsItem.Text : (chk_allItemsItem.IsChecked == true ? all : "") ;

                    unitval = cb_unitsItem.SelectedItem != null
                      && (chk_allUnitsItem.IsChecked == false || chk_allUnitsItem.IsChecked == null)
                      ? cb_unitsItem.Text : (chk_allUnitsItem.IsChecked == true ? all : "") ;
                }
                else if (selectedStockTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\En\Location.rdlc";
                    secondTitle = "location";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trSection")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trLocation")));
                    //expired = chk_expireDateLocation.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
               

                    //startDate = dp_startDateLocation.SelectedDate != null ? SectionData.DateToString(dp_startDateLocation.SelectedDate) : "";
                  
                    //endDate = dp_endDateLocation.SelectedDate != null ? SectionData.DateToString(dp_endDateLocation.SelectedDate) : "";
                    branchval = cb_branchesLocation.SelectedItem != null
                   && (chk_allBranchesLocation.IsChecked == false || chk_allBranchesLocation.IsChecked == null)
                   ? cb_branchesLocation.Text : (chk_allBranchesLocation.IsChecked == true ? all : "") ;

                    itemval = cb_sectionsLocation.SelectedItem != null
                       && (chk_allSectionsLocation.IsChecked == false || chk_allSectionsLocation.IsChecked == null)
                       ? cb_sectionsLocation.Text : (chk_allSectionsLocation.IsChecked == true ? all : "") ;

                    unitval = cb_locationsLocation.SelectedItem != null
                      && (chk_allLocationsLocation.IsChecked == false || chk_allLocationsLocation.IsChecked == null)
                      ? cb_locationsLocation.Text : (chk_allLocationsLocation.IsChecked == true ? all : "") ;

                }
                else if (selectedStockTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\En\Collect.rdlc";
                    secondTitle = "collect";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    //expired = chk_expireDateCollect.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
                 

                    //startDate = dp_startDateCollect.SelectedDate != null ? SectionData.DateToString(dp_startDateCollect.SelectedDate) : "";
                 
                    //endDate = dp_endDateCollect.SelectedDate != null ? SectionData.DateToString(dp_endDateCollect.SelectedDate) : "";

                    branchval = cb_branchesCollect.SelectedItem != null
                && (chk_allBranchesCollect.IsChecked == false || chk_allBranchesCollect.IsChecked == null)
                ? cb_branchesCollect.Text : (chk_allBranchesCollect.IsChecked == true ? all : "");

                    itemval = cb_itemsCollect.SelectedItem != null
                       && (chk_allItemsCollect.IsChecked == false || chk_allItemsCollect.IsChecked == null)
                       ? cb_itemsCollect.Text : (chk_allItemsCollect.IsChecked == true ? all : "");

                    unitval = cb_unitsCollect.SelectedItem != null
                      && (chk_allUnitsCollect.IsChecked == false || chk_allUnitsCollect.IsChecked == null)
                      ? cb_unitsCollect.Text : (chk_allUnitsCollect.IsChecked == true ? all : "");
                }
                else if (selectedStockTab == 3)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Stock\En\Expired.rdlc";
                    secondTitle = "trExpired";
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    //expired = chk_expireDateItem.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trExpired") : "";
                    startDate = dp_startDateExpired.SelectedDate != null ? SectionData.DateToString(dp_startDateExpired.SelectedDate) : "";

                    endDate = dp_endDateExpired.SelectedDate != null ? SectionData.DateToString(dp_endDateExpired.SelectedDate) : "";

                    // cb_branchesItem.Text 
                    //  branchval = .SelectedValue;
                    branchval = cb_branchesExpired.SelectedItem != null
                          && (chk_allBranchesExpired.IsChecked == false || chk_allBranchesExpired.IsChecked == null)
                          ? cb_branchesExpired.Text : (chk_allBranchesExpired.IsChecked == true ? all : "");

                    itemval = cb_itemsExpired.SelectedItem != null
                       && (chk_allItemsExpired.IsChecked == false || chk_allItemsExpired.IsChecked == null)
                       ? cb_itemsExpired.Text : (chk_allItemsExpired.IsChecked == true ? all : "");

                    unitval = cb_unitsExpired.SelectedItem != null
                      && (chk_allUnitsExpired.IsChecked == false || chk_allUnitsExpired.IsChecked == null)
                      ? cb_unitsExpired.Text : (chk_allUnitsExpired.IsChecked == true ? all : "");
                }
            }
            //filter
            paramarr.Add(new ReportParameter("BranchStore", branchval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));
            paramarr.Add(new ReportParameter("UnitVal", unitval));
            paramarr.Add(new ReportParameter("trExpired", expired));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));


            //end filter

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            clsReports.storageStock(temp, rep, reppath, paramarr);
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

                #region

                BuildReport();
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
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
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                BuildReport();
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
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
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
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
                //                t1.Start();
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
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
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
        #endregion

        bool showChart = true;
        
        double col_reportChartWidth = 0;
        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {
            
            showChart = !showChart;

            if (showChart)
            {
                col_reportChart.Width = new GridLength(col_reportChartWidth);

                path_reportPath1.Fill = Application.Current.Resources["reportPath1"] as SolidColorBrush;
                path_reportPath2.Fill = Application.Current.Resources["reportPath2"] as SolidColorBrush;
                path_reportPath3.Fill = Application.Current.Resources["reportPath3"] as SolidColorBrush;
                tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");
            }
            else
            {
                col_reportChart.Width = new GridLength(0);

                path_reportPath1.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                path_reportPath2.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                path_reportPath3.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                tt_pieChart.Content = MainWindow.resourcemanager.GetString("trShow");
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
                if (buttonTag.Equals("cartesianChart"))
                {
                    w.type = "cartesianChart";
                    w.cartesianChart.Series = cartesianChart.Series;
                    w.axcolumn.Labels = axcolumn.Labels;

                }
                else if (buttonTag.Equals("chart1"))
                {
                    w.type = "pieChart";
                    w.pieChart.Series = chart1.Series;

                }
                else if (buttonTag.Equals("rowChart"))
                {
                    w.type = "cartesianChart";
                    w.cartesianChart.Series = rowChart.Series;
                    w.axcolumn.Labels = MyAxis.Labels;
                }
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
