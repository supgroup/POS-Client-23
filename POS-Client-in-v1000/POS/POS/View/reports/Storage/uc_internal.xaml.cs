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
using POS.View.storage;
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{
    public partial class uc_internal : UserControl
    {
        #region variables
        List<ItemTransferInvoice> itemsInternalTransfer;
        IEnumerable<ItemTransferInvoice> itemsTransferReport = new List<ItemTransferInvoice>();
        IEnumerable<ItemTransferInvoice> invTypeCount;

        Statistics statisticModel = new Statistics();

        List<Branch> comboBranches;

        private int selectedTab = 0;
        #endregion

        private static uc_internal _instance;
        public static uc_internal Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_internal();
                return _instance;
            }
        }
        public uc_internal()
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

                itemsInternalTransfer = await statisticModel.GetInternalMov((int)MainWindow.branchID, (int)MainWindow.userID);

                comboInternalItemsItems = statisticModel.getExternalItemCombo(itemsInternalTransfer);
                comboInternalItemsUnits = statisticModel.getExternalUnitCombo(itemsInternalTransfer);

                fillComboInternalOperatorType();

                #region keyup
                /////////////////////////////
                cb_internalItemsFromBranches.IsTextSearchEnabled = false;
                cb_internalItemsFromBranches.IsEditable = true;
                cb_internalItemsFromBranches.StaysOpenOnEdit = true;
                cb_internalItemsFromBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_internalItemsFromBranches.Text = "";
                //
                cb_internalItemsToBranches.IsTextSearchEnabled = false;
                cb_internalItemsToBranches.IsEditable = true;
                cb_internalItemsToBranches.StaysOpenOnEdit = true;
                cb_internalItemsToBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_internalItemsToBranches.Text = "";
                //
                cb_internalItemsItems.IsTextSearchEnabled = false;
                cb_internalItemsItems.IsEditable = true;
                cb_internalItemsItems.StaysOpenOnEdit = true;
                cb_internalItemsItems.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_internalItemsItems.Text = "";
                //
                cb_internalItemsUnits.IsTextSearchEnabled = false;
                cb_internalItemsUnits.IsEditable = true;
                cb_internalItemsUnits.StaysOpenOnEdit = true;
                cb_internalItemsUnits.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_internalItemsUnits.Text = "";
                ///////////////////////////////////
                cb_internalOperaterFromBranches.IsTextSearchEnabled = false;
                cb_internalOperaterFromBranches.IsEditable = true;
                cb_internalOperaterFromBranches.StaysOpenOnEdit = true;
                cb_internalOperaterFromBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_internalOperaterFromBranches.Text = "";
                #endregion

                btn_internalItems_Click(btn_item, null);

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
            tt_operator.Content = MainWindow.resourcemanager.GetString("trOperators");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_internalItemsFromBranches, MainWindow.resourcemanager.GetString("trFromBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_internalItemsItems, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_internalItemsToBranches, MainWindow.resourcemanager.GetString("trToBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_internalItemsUnits, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_internalOperaterFromBranches, MainWindow.resourcemanager.GetString("trBranch/StoreHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_internalOperaterType, MainWindow.resourcemanager.GetString("trOperator") + "...");

            chk_internalItemsFromAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_internalItemsAllItems.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_internalItemsToAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_internalItemsAllUnits.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_internalOperaterFromAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_internalOperatorAllTypes.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_internalItemsTwoWay.Content = MainWindow.resourcemanager.GetString("trTwoWays");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_InternalItemsEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_internalItemsStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_InternalOperatorEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_internalOperatorStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_invNumber.Header = MainWindow.resourcemanager.GetString("trNo");
            col_invTypeNumber.Header = MainWindow.resourcemanager.GetString("trType");
            col_invDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_branchFrom.Header = MainWindow.resourcemanager.GetString("trFromBranch");
            col_branchTo.Header = MainWindow.resourcemanager.GetString("trToBranch");
            col_quantity.Header = MainWindow.resourcemanager.GetString("trQTR");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
         }
        public void paint()
        {
            grid_internalItems.Visibility = Visibility.Hidden;
            grid_internalOperater.Visibility = Visibility.Hidden;

            bdr_item.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_operator.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_item.Fill = Brushes.White;
            path_operator.Fill = Brushes.White;
        }
        private void fillComboBranches(ComboBox cb)
        {
            cb.SelectedValuePath = "branchId";
            cb.DisplayMemberPath = "name";
            cb.ItemsSource = comboBranches;
        }
        private void hideAllColumn()
        {
            grid_internalItems.Visibility = Visibility.Hidden;
            grid_internalOperater.Visibility = Visibility.Hidden;
            col_branch.Visibility = Visibility.Hidden;
            col_item.Visibility = Visibility.Hidden;
            col_unit.Visibility = Visibility.Hidden;
            col_quantity.Visibility = Visibility.Hidden;
            col_invTypeNumber.Visibility = Visibility.Hidden;
            col_invNumber.Visibility = Visibility.Hidden;
            col_invDate.Visibility = Visibility.Hidden;
            col_invTypeNumber.Visibility = Visibility.Hidden;
            col_branchFrom.Visibility = Visibility.Hidden;
            col_branchTo.Visibility = Visibility.Hidden;

        }
        private void showSelectedTabColumn()
        {
            hideAllColumn();

            if (selectedTab == 0)
            {
                grid_internalItems.Visibility = Visibility.Visible;
                col_branchFrom.Visibility = Visibility.Visible;
                col_branchTo.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_invTypeNumber.Visibility = Visibility.Visible;
                col_invDate.Visibility = Visibility.Visible;
                col_invNumber.Visibility = Visibility.Visible;

            }
            else if (selectedTab == 1)
            {
                grid_internalOperater.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_invTypeNumber.Visibility = Visibility.Visible;
                col_invDate.Visibility = Visibility.Visible;
                col_invNumber.Visibility = Visibility.Visible;
            }
        }

        List<ExternalitemCombo> comboInternalItemsItems;
        List<ExternalitemCombo> items;
        List<ExternalUnitCombo> comboInternalItemsUnits;
        List<ExternalUnitCombo> units;
        private void fillComboInternalItemsItems()
        {
            cb_internalItemsItems.SelectedValuePath = "ItemId";
            cb_internalItemsItems.DisplayMemberPath = "ItemName";
            items = comboInternalItemsItems.GroupBy(x => x.ItemId).Select(x => new ExternalitemCombo
            {
                ItemId = x.FirstOrDefault().ItemId,
                BranchId = x.FirstOrDefault().BranchId,
                ItemName = x.FirstOrDefault().ItemName
            }
            ).ToList();
            cb_internalItemsItems.ItemsSource = items;
        }
        private void fillComboInternalItemsUnits()
        {
            var temp = cb_internalItemsItems.SelectedItem as ExternalitemCombo;

            cb_internalItemsUnits.SelectedValuePath = "UnitId";
            cb_internalItemsUnits.DisplayMemberPath = "UnitName";

            if (temp != null)
            {
                units = comboInternalItemsUnits
                     .Where(x => x.ItemId == temp.ItemId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
                cb_internalItemsUnits.ItemsSource = units;
            }
            else
            {
                units = comboInternalItemsUnits
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).ToList();
                cb_internalItemsUnits.ItemsSource = units;
            }
        }
        private void fillComboInternalOperatorType()
        {//type
            var dislist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trExport")     , Value = "ex" },
                new { Text = MainWindow.resourcemanager.GetString("trImport")     , Value = "im" }
                 };
            cb_internalOperaterType.SelectedValuePath = "Value";
            cb_internalOperaterType.DisplayMemberPath = "Text";
            cb_internalOperaterType.ItemsSource = dislist;
        }
        IEnumerable<ItemTransferInvoice> lst;
        private IEnumerable<ItemTransferInvoice> fillListInternal(IEnumerable<ItemTransferInvoice> itemsTransfer, ComboBox comboFromBranch, ComboBox comboToBranch, ComboBox Items, ComboBox unit, DatePicker startDate, DatePicker endDate, CheckBox chkAllFromBranches, CheckBox chkAllToBranches, CheckBox chkAllItems, CheckBox chkAllUnits, CheckBox towWays)
        {
            var selectedFromBranch = comboFromBranch.SelectedItem as Branch;

            if (selectedTab == 0)
            {
                var selectedToBranch = comboToBranch.SelectedItem as Branch;
                var selectedItem = Items.SelectedItem as ExternalitemCombo;
                var selectedUnit = unit.SelectedItem as ExternalUnitCombo;
                var result = itemsTransfer.Where(x => (

                           (Items.SelectedItem != null ? (x.itemId == selectedItem.ItemId) : true)
                        && (unit.SelectedItem != null ? (x.unitId == selectedUnit.UnitId) : true)
                        && (dp_internalItemsStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
                        && (dp_InternalItemsEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
                        //two ways
                        &&
                          towWays.IsChecked == true ?
                              //fromBranch
                              (selectedFromBranch != null ? x.exportBranchId == Convert.ToInt32(cb_internalItemsFromBranches.SelectedValue) || x.importBranchId == Convert.ToInt32(cb_internalItemsFromBranches.SelectedValue) : true)
                              &&
                              //toBranch
                              (selectedToBranch != null ? x.exportBranchId == Convert.ToInt32(cb_internalItemsToBranches.SelectedValue) || x.importBranchId == Convert.ToInt32(cb_internalItemsToBranches.SelectedValue) : true)
                         :
                              //fromBranch
                              (selectedFromBranch != null ? x.exportBranchId == Convert.ToInt32(cb_internalItemsFromBranches.SelectedValue) : true)
                              &&
                              //toBranch
                              (selectedToBranch != null ? x.importBranchId == Convert.ToInt32(cb_internalItemsToBranches.SelectedValue) : true)
                        ));
                lst = result;
                return result;
            }

            else if (selectedTab == 1)
            {
                var selectedToBranch = comboToBranch.SelectedItem as internalTypeCombo;
                var result = itemsTransfer.Where(x => (

                                         (comboFromBranch.SelectedItem != null ? (x.branchId == selectedFromBranch.branchId) : true)
                                      && (comboToBranch.SelectedItem != null ? (x.invType == cb_internalOperaterType.SelectedValue.ToString()) : true)
                                      && (dp_internalOperatorStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date) : true)
                                      && (dp_InternalOperatorEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date) : true)
                        ));
                lst = result;
                return result;
            }
            return null;

        }
        private void fillEventsCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
        private void fillEvents()
        {
            var lst = fillListInternal(itemsInternalTransfer, cb_internalItemsFromBranches, cb_internalItemsToBranches, cb_internalItemsItems, cb_internalItemsUnits, dp_internalItemsStartDate, dp_InternalItemsEndDate, chk_internalItemsFromAllBranches, chk_internalItemsToAllBranches, chk_internalItemsAllItems, chk_internalItemsAllUnits, chk_internalItemsTwoWay);
            if (selectedTab == 1)
                lst = fillListInternal(itemsInternalTransfer, cb_internalOperaterFromBranches, cb_internalOperaterType, null, null, dp_internalOperatorStartDate, dp_InternalOperatorEndDate, null, null, null, null, null);

            itemsTransferReport = lst;
            dgStock.ItemsSource = lst;
            txt_count.Text = lst.Count().ToString();

            fillInternalColumnChart(lst);
            fillInternalPieChart(lst);

        }
        private void fillEmptyEvents()
        {
            var lst = new List<ItemTransferInvoice>();
            itemsTransferReport = lst;
            dgStock.ItemsSource = lst;
            txt_count.Text = lst.Count().ToString();

            fillInternalColumnChart(lst);
            fillInternalPieChart(lst);
        }
        #endregion

        #region tabs
        private async void btn_internalItems_Click(object sender, RoutedEventArgs e)
        {///items
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 0;
                txt_search.Text = "";

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_item);
                path_item.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                showSelectedTabColumn();

                await SectionData.fillBranchesWithoutMain(cb_internalItemsFromBranches);
                await SectionData.fillBranchesWithoutMain(cb_internalItemsToBranches);
                fillComboInternalItemsItems();
                fillComboInternalItemsUnits();

                dp_internalItemsStartDate.SelectedDate = null;
                dp_InternalItemsEndDate.SelectedDate = null;

                chk_internalItemsFromAllBranches.IsChecked = true;
                chk_internalItemsToAllBranches.IsChecked = true;
                chk_internalItemsAllItems.IsChecked = true;
                chk_internalItemsAllUnits.IsChecked = true;

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
        private async void btn_internalOperator_Click(object sender, RoutedEventArgs e)
        {//operators
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 1;
                txt_search.Text = "";

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_operator);
                path_operator.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                showSelectedTabColumn();

                //fillComboBranches(cb_internalOperaterFromBranches);
                await SectionData.fillBranchesWithoutMain(cb_internalOperaterFromBranches);
                dp_internalOperatorStartDate.SelectedDate = null;
                dp_InternalOperatorEndDate.SelectedDate = null;
                chk_internalOperaterFromAllBranches.IsChecked = true;
                chk_internalOperatorAllTypes.IsChecked = true;

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
        #endregion

        #region events
        private void chk_internalItemsAllItems_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsItems.IsEnabled = false;
                cb_internalItemsItems.SelectedItem = null;
                chk_internalItemsAllUnits.IsEnabled = true;
                chk_internalItemsAllUnits.IsChecked = true;
                chk_internalItemsAllUnits_Checked(chk_internalItemsAllUnits, null);
                cb_internalItemsItems.Text = "";
                cb_internalItemsItems.ItemsSource = items;
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
        private void chk_internalItemsAllItems_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsItems.IsEnabled = true;
                cb_internalItemsUnits.IsEnabled = false;
                chk_internalItemsAllUnits.IsEnabled = false;
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
        private void chk_internalItemsAllUnits_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsUnits.IsEnabled = false;
                cb_internalItemsUnits.SelectedItem = null;
                cb_internalItemsUnits.Text = "";
                cb_internalItemsUnits.ItemsSource = units;
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
        private void chk_internalItemsAllUnits_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsUnits.IsEnabled = true;

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
        private void cb_internalItemsToBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_internalItemsToBranches.SelectedItem != null)
                {
                    chk_internalItemsTwoWay.IsEnabled = true;

                    chk_internalItemsAllItems.IsEnabled = true;
                    chk_internalItemsAllUnits.IsEnabled = true;
                    chk_internalItemsAllItems_Checked(chk_internalItemsAllItems, null);

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
        private void chk_internalItemsFromAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsFromBranches.Text = "";
                cb_internalItemsFromBranches.ItemsSource = SectionData.BranchesAllWithoutMainList;
                cb_internalItemsFromBranches.IsEnabled = false;
                cb_internalItemsFromBranches.SelectedItem = null;

                if ((chk_internalItemsFromAllBranches.IsChecked == true) && (chk_internalItemsToAllBranches.IsChecked == true))
                { chk_internalItemsTwoWay.IsEnabled = false; chk_internalItemsTwoWay.IsChecked = false; }
                else
                    chk_internalItemsTwoWay.IsEnabled = true;

                chk_internalItemsAllItems.IsEnabled = true;
                chk_internalItemsAllUnits.IsEnabled = true;
                chk_internalItemsAllItems_Checked(chk_internalItemsAllItems, null);
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
        private void chk_internalItemsFromAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsFromBranches.IsEnabled = true;

                if ((chk_internalItemsFromAllBranches.IsChecked == true) && (chk_internalItemsToAllBranches.IsChecked == true))
                { chk_internalItemsTwoWay.IsEnabled = false; chk_internalItemsTwoWay.IsChecked = false; }
                else
                    chk_internalItemsTwoWay.IsEnabled = true;

                cb_internalItemsItems.IsEnabled = false;
                cb_internalItemsUnits.IsEnabled = false;
                chk_internalItemsAllItems.IsEnabled = false;
                chk_internalItemsAllUnits.IsEnabled = false;
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
        private void chk_internalItemsToAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsToBranches.Text = "";
                cb_internalItemsToBranches.ItemsSource = SectionData.BranchesAllWithoutMainList;
                cb_internalItemsToBranches.IsEnabled = false;
                cb_internalItemsToBranches.SelectedItem = null;

                if ((chk_internalItemsFromAllBranches.IsChecked == true) && (chk_internalItemsToAllBranches.IsChecked == true))
                { chk_internalItemsTwoWay.IsEnabled = false; chk_internalItemsTwoWay.IsChecked = false; }
                else
                    chk_internalItemsTwoWay.IsEnabled = true;

                chk_internalItemsAllItems.IsEnabled = true;
                chk_internalItemsAllUnits.IsEnabled = true;
                chk_internalItemsAllItems_Checked(chk_internalItemsAllItems, null);
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
        private void chk_internalItemsToAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalItemsToBranches.IsEnabled = true;

                if ((chk_internalItemsFromAllBranches.IsChecked == true) && (chk_internalItemsToAllBranches.IsChecked == true))
                { chk_internalItemsTwoWay.IsEnabled = false; chk_internalItemsTwoWay.IsChecked = false; }
                else
                    chk_internalItemsTwoWay.IsEnabled = true;

                cb_internalItemsItems.IsEnabled = false;
                cb_internalItemsUnits.IsEnabled = false;
                chk_internalItemsAllItems.IsEnabled = false;
                chk_internalItemsAllUnits.IsEnabled = false;
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
        private void chk_internalOperaterFromAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalOperaterFromBranches.Text = "";
                cb_internalOperaterFromBranches.ItemsSource = SectionData.BranchesAllWithoutMainList;
                cb_internalOperaterFromBranches.IsEnabled = false;
                cb_internalOperaterFromBranches.SelectedItem = null;

                chk_internalOperatorAllTypes.IsEnabled = true;
                chk_internalOperatorAllTypes.IsChecked = true;
                cb_internalOperaterType.IsEnabled = false;
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
        private void chk_internalOperaterFromAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalOperaterFromBranches.IsEnabled = true;
                cb_internalOperaterType.IsEnabled = false;
                chk_internalOperatorAllTypes.IsEnabled = false;
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
        private void chk_internalOperatorAllTypes_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalOperaterType.IsEnabled = false;
                cb_internalOperaterType.SelectedItem = null;

                

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
        private void chk_internalOperatorAllTypes_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_internalOperaterType.IsEnabled = true;

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
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    //var lst = fillListInternal(itemsInternalTransfer, cb_internalItemsFromBranches, cb_internalItemsToBranches, cb_internalItemsItems, cb_internalItemsUnits, dp_internalItemsStartDate, dp_InternalItemsEndDate, chk_internalItemsFromAllBranches, chk_internalItemsToAllBranches, chk_internalItemsAllItems, chk_internalItemsAllUnits, chk_internalItemsTwoWay)
                    lst = lst
                               .Where(s => (s.exportBranch.ToLower().Contains(txt_search.Text.ToLower()) ||
                               s.importBranch.ToLower().Contains(txt_search.Text.ToLower()) ||
                               s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                               s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                               s.InvTypeNumber.ToLower().Contains(txt_search.Text.ToLower())
                               ));
                }
                else if (selectedTab == 1)
                {
                    //var lst = fillListInternal(itemsInternalTransfer, cb_internalOperaterFromBranches, cb_internalOperaterType, null, null, dp_internalOperatorStartDate, dp_InternalOperatorEndDate, null, null, null, null, null)
                    lst = lst
                                .Where(s => (s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                s.InvTypeNumber.ToLower().Contains(txt_search.Text.ToLower())
                                ));
                }

                dgStock.ItemsSource = lst;
                txt_count.Text = lst.Count().ToString();

                fillInternalColumnChart(lst);
                fillInternalPieChart(lst);

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
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                itemsInternalTransfer = await statisticModel.GetInternalMov((int)MainWindow.branchID, (int)MainWindow.userID);

                if (selectedTab == 0)
                {
                    cb_internalItemsFromBranches.SelectedItem = null;
                    cb_internalItemsToBranches.SelectedItem = null;
                    cb_internalItemsItems.SelectedItem = null;
                    cb_internalItemsUnits.SelectedItem = null;
                    chk_internalItemsTwoWay.IsChecked = false;
                    chk_internalItemsFromAllBranches.IsChecked = true;
                    chk_internalItemsToAllBranches.IsChecked = true;
                    chk_internalItemsAllItems.IsChecked = true;
                    chk_internalItemsAllUnits.IsChecked = true;
                    dp_internalItemsStartDate.SelectedDate = null;
                    dp_InternalItemsEndDate.SelectedDate = null;

                }
                else if (selectedTab == 1)
                {
                    
                    cb_internalOperaterFromBranches.SelectedItem = null;
                    cb_internalOperaterType.SelectedItem = null;
                    dp_internalOperatorStartDate.SelectedDate = null;
                    dp_InternalOperatorEndDate.SelectedDate = null;
                    chk_internalOperaterFromAllBranches.IsChecked = true;
                    chk_internalOperaterFromAllBranches.IsChecked = true;
                   // chk_internalOperatorAllTypes.IsChecked = false;
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
        private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_internalItemsItems.SelectedItem != null)
            {
                chk_internalItemsAllUnits.IsEnabled = true;
                //    chk_internalItemsAllUnits_Checked(chk_internalItemsAllUnits, null);

            }
            if (cb_internalOperaterFromBranches.SelectedItem != null)
            {
                chk_internalOperatorAllTypes.IsEnabled = true;
                //  chk_internalOperatorAllTypes_Checked(chk_internalOperatorAllTypes, null);
            }
            fillEventsCall(sender);
        }
        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
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
        Invoice invoice;
        private async void DgStock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                invoice = new Invoice();
                if (dgStock.SelectedIndex != -1)
                {
                    ItemTransferInvoice item = dgStock.SelectedItem as ItemTransferInvoice;
                    if (item.invoiceId > 0)
                    {
                        invoice = await invoice.GetByInvoiceId(item.invoiceId);
                        MainWindow.mainWindow.BTN_storage_Click(MainWindow.mainWindow.btn_storage, null);
                        View.uc_storage.Instance.UserControl_Loaded(null, null);
                        View.uc_storage.Instance.Btn_itemsExport_Click(View.uc_storage.Instance.btn_importExport, null);
                        uc_itemsExport.Instance.UserControl_Loaded(null, null);
                        uc_itemsExport._ProcessType = invoice.invType;
                        uc_itemsExport.Instance.invoice = invoice;
                        uc_itemsExport.isFromReport = true;
                        await uc_itemsExport.Instance.fillOrderInputs(invoice);
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
        private void Cb_internalItemsFromBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_internalItemsFromBranches.SelectedItem != null)
                {
                    chk_internalItemsAllItems.IsEnabled = true;
                    chk_internalItemsAllUnits.IsEnabled = true;
                    chk_internalItemsAllItems_Checked(chk_internalItemsAllItems, null);
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
        private void Cb_internalItemsFromBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_internalItemsItems_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = items.Where(p => p.ItemName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_internalItemsToBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_internalItemsUnits_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = units.Where(p => p.UnitName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_internalOperaterFromBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
    
            string addpath = "";
            string firstTitle = "internal";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string inchk = "";
            string outchk = "";
            string startDate = "";
            string endDate = "";
            string fromval = "";
            string toval = "";
            string itemval = "";
            string unitval = "";
            string searchval = "";
            string TwoWays = "";
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Internal\Ar\ArItem.rdlc";
                    secondTitle = "items";
                    paramarr.Add(new ReportParameter("trFromBranchStore", MainWindow.resourcemanagerreport.GetString("trFromBranch/Store")));

                    paramarr.Add(new ReportParameter("trToBranchStore", MainWindow.resourcemanagerreport.GetString("trToBranch/Store")));
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    startDate = dp_internalItemsStartDate.SelectedDate != null ? SectionData.DateToString(dp_internalItemsStartDate.SelectedDate) : "";

                    endDate = dp_InternalItemsEndDate.SelectedDate != null ? SectionData.DateToString(dp_InternalItemsEndDate.SelectedDate) : "";

                    fromval = cb_internalItemsFromBranches.SelectedItem != null
                       && (chk_internalItemsFromAllBranches.IsChecked == false || chk_internalItemsFromAllBranches.IsChecked == null)
                       ? cb_internalItemsFromBranches.Text : (chk_internalItemsFromAllBranches.IsChecked == true ? all : "");


                    toval = cb_internalItemsToBranches.SelectedItem != null
                       && (chk_internalItemsToAllBranches.IsChecked == false || chk_internalItemsToAllBranches.IsChecked == null)
                       ? cb_internalItemsToBranches.Text : (chk_internalItemsToAllBranches.IsChecked == true ? all : "");


                    itemval = cb_internalItemsItems.SelectedItem != null
                       && (chk_internalItemsAllItems.IsChecked == false || chk_internalItemsAllItems.IsChecked == null)
                       && fromval != "" && toval != ""
                       ? cb_internalItemsItems.Text : (chk_internalItemsAllItems.IsChecked == true  && fromval != "" && toval != "" ? all : "");

                    unitval = cb_internalItemsUnits.SelectedItem != null
                      && (chk_internalItemsAllUnits.IsChecked == false || chk_internalItemsAllUnits.IsChecked == null)
                    && itemval != ""
                      ? cb_internalItemsUnits.Text : (chk_internalItemsAllUnits.IsChecked == true && itemval != "" ? all : "");
                    TwoWays = chk_internalItemsTwoWay.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trTwoWays") : "";
            

                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Internal\Ar\ArOperator.rdlc";
                    secondTitle = "operator";
                    paramarr.Add(new ReportParameter("trFromBranchStore", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trOperator")));      
                    startDate = dp_internalOperatorStartDate.SelectedDate != null ? SectionData.DateToString(dp_internalOperatorStartDate.SelectedDate) : "";
                    endDate = dp_InternalOperatorEndDate.SelectedDate != null ? SectionData.DateToString(dp_InternalOperatorEndDate.SelectedDate) : "";

                    fromval = cb_internalOperaterFromBranches.SelectedItem != null
               && (chk_internalOperaterFromAllBranches.IsChecked == false || chk_internalOperaterFromAllBranches.IsChecked == null)
               ? cb_internalOperaterFromBranches.Text : (chk_internalOperaterFromAllBranches.IsChecked == true ? all : "");

                    itemval = cb_internalOperaterType.SelectedItem != null
                       && (chk_internalOperatorAllTypes.IsChecked == false || chk_internalOperatorAllTypes.IsChecked == null)
                       && fromval != ""  
                       ? cb_internalOperaterType.Text : (chk_internalOperatorAllTypes.IsChecked == true && fromval != ""  ? all : "");
                }

            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Internal\En\Item.rdlc";
                    secondTitle = "items";
                    paramarr.Add(new ReportParameter("trFromBranchStore", MainWindow.resourcemanagerreport.GetString("trFromBranch/Store")));

                    paramarr.Add(new ReportParameter("trToBranchStore", MainWindow.resourcemanagerreport.GetString("trToBranch/Store")));
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
                    paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
                    startDate = dp_internalItemsStartDate.SelectedDate != null ? SectionData.DateToString(dp_internalItemsStartDate.SelectedDate) : "";

                    endDate = dp_InternalItemsEndDate.SelectedDate != null ? SectionData.DateToString(dp_InternalItemsEndDate.SelectedDate) : "";

                    fromval = cb_internalItemsFromBranches.SelectedItem != null
               && (chk_internalItemsFromAllBranches.IsChecked == false || chk_internalItemsFromAllBranches.IsChecked == null)
               ? cb_internalItemsFromBranches.Text : (chk_internalItemsFromAllBranches.IsChecked == true ? all : "");


                    toval = cb_internalItemsToBranches.SelectedItem != null
               && (chk_internalItemsToAllBranches.IsChecked == false || chk_internalItemsToAllBranches.IsChecked == null)
               ? cb_internalItemsToBranches.Text : (chk_internalItemsToAllBranches.IsChecked == true ? all : "");


                    itemval = cb_internalItemsItems.SelectedItem != null
                       && (chk_internalItemsAllItems.IsChecked == false || chk_internalItemsAllItems.IsChecked == null)
                       && fromval != "" && toval != ""
                       ? cb_internalItemsItems.Text : (chk_internalItemsAllItems.IsChecked == true && fromval != "" && toval != "" ? all : "");

                    unitval = cb_internalItemsUnits.SelectedItem != null
                      && (chk_internalItemsAllUnits.IsChecked == false || chk_internalItemsAllUnits.IsChecked == null)
                    && itemval != ""
                      ? cb_internalItemsUnits.Text : (chk_internalItemsAllUnits.IsChecked == true && itemval != "" ? all : "");
                    TwoWays = chk_internalItemsTwoWay.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trTwoWays") : "";


                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Internal\En\Operator.rdlc";
                    secondTitle = "operator";
                    paramarr.Add(new ReportParameter("trFromBranchStore", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
                    paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trOperator")));
                    startDate = dp_internalOperatorStartDate.SelectedDate != null ? SectionData.DateToString(dp_internalOperatorStartDate.SelectedDate) : "";
                    endDate = dp_InternalOperatorEndDate.SelectedDate != null ? SectionData.DateToString(dp_InternalOperatorEndDate.SelectedDate) : "";

                    fromval = cb_internalOperaterFromBranches.SelectedItem != null
               && (chk_internalOperaterFromAllBranches.IsChecked == false || chk_internalOperaterFromAllBranches.IsChecked == null)
               ? cb_internalOperaterFromBranches.Text : (chk_internalOperaterFromAllBranches.IsChecked == true ? all : "");

                    itemval = cb_internalOperaterType.SelectedItem != null
                       && (chk_internalOperatorAllTypes.IsChecked == false || chk_internalOperatorAllTypes.IsChecked == null)
                       && fromval != ""
                       ? cb_internalOperaterType.Text : (chk_internalOperatorAllTypes.IsChecked == true && fromval != "" ? all : "");
                }

            }
            paramarr.Add(new ReportParameter("fromVal", fromval));
            paramarr.Add(new ReportParameter("toVal", toval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));
            paramarr.Add(new ReportParameter("UnitVal", unitval));

            paramarr.Add(new ReportParameter("trTwoWays", TwoWays));
            //  paramarr.Add(new ReportParameter("trOut", outchk));

            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));


            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //
          
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trFromBranch", MainWindow.resourcemanagerreport.GetString("trFromBranch")));
            paramarr.Add(new ReportParameter("trToBranch", MainWindow.resourcemanagerreport.GetString("trToBranch")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));    

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            clsReports.itemTransferInvoiceInternal(itemsTransferReport, rep, reppath, paramarr);
            clsReports.itemTransferInvoiceInternal(lst, rep, reppath, paramarr);

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
                //t1.Start();
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

        #region charts
        private void fillInternalRowChart()
        {
            #region
            //      MyAxis.Labels = new List<string>();
            //      List<string> names = new List<string>();
            //      List<int> pTemp = new List<int>();
            //      List<int> sTemp = new List<int>();
            //      List<int> pbTemp = new List<int>();
            //      List<int> sbTemp = new List<int>();


            //      var temp = fillListInternal(itemsInternalTransfer, cb_internalItemsFromBranches, cb_internalItemsToBranches, cb_internalItemsItems, cb_internalItemsUnits, dp_internalItemsStartDate, dp_InternalItemsEndDate, chk_internalItemsFromAllBranches, chk_internalItemsToAllBranches, chk_internalItemsAllItems, chk_internalItemsAllUnits, chk_internalItemsTwoWay);
            //      if (selectedTab == 1)
            //      {
            //          temp = fillListInternal(itemsInternalTransfer, cb_internalOperaterFromBranches, cb_internalOperaterType, null, null, dp_internalOperatorStartDate, dp_InternalOperatorEndDate, null, null, null, null, null);
            //      }

            //      var result = temp.GroupBy(x => new { x.branchId, x.invoiceId }).Select(x => new ItemTransferInvoice
            //      {
            //          invType = x.FirstOrDefault().invType,
            //          branchId = x.FirstOrDefault().branchId
            //      });

            //      invCount = result.GroupBy(x => x.branchId).Select(x => new ItemTransferInvoice
            //      {
            //          PCount = x.Where(g => g.invType == "p").Count(),
            //          SCount = x.Where(g => g.invType == "s").Count(),
            //          PbCount = x.Where(g => g.invType == "pb").Count(),
            //          SbCount = x.Where(g => g.invType == "sb").Count()
            //      });




            //      for (int i = 0; i < agentsCount.Count(); i++)
            //      {
            //          pTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().PCount);
            //          pbTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().PbCount);
            //          sTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().SCount);
            //          sbTemp.Add(invCount.ToList().Skip(i).FirstOrDefault().SbCount);
            //      }
            //      var tempName = temp.GroupBy(s => new { s.branchId, s.invType }).Select(s => new
            //      {
            //          locationName = s.FirstOrDefault().branchName
            //      });
            //      names.AddRange(tempName.Select(nn => nn.locationName));

            //      SeriesCollection rowChartData = new SeriesCollection();
            //      for (int i = 0; i < pTemp.Count(); i++)
            //      {
            //          MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            //      }

            //      rowChartData.Add(
            //    new LineSeries
            //    {
            //        Values = pTemp.AsChartValues(),
            //        //Title = "Purchase"
            //        Title = MainWindow.resourcemanager.GetString("tr_Purchases")
            //    });
            //      rowChartData.Add(
            //new LineSeries
            //{
            //    Values = pbTemp.AsChartValues(),
            //    //Title = "Purchase Returns"
            //    Title = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoice")
            //});
            //      rowChartData.Add(
            //new LineSeries
            //{
            //    Values = sTemp.AsChartValues(),
            //    //Title = "Sale"
            //    Title = MainWindow.resourcemanager.GetString("tr_Sales")
            //});
            //      rowChartData.Add(
            //new LineSeries
            //{
            //    Values = sbTemp.AsChartValues(),
            //    //Title = "Sale Returns"
            //    Title = MainWindow.resourcemanager.GetString("trSalesReturnInvoice")
            //});
            //      rowChart.Series = rowChartData;
            //      DataContext = this;
            #endregion
        }
        private void fillInternalColumnChart(IEnumerable<ItemTransferInvoice> lst)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            //var temp = lst;

            var res = lst.GroupBy(x => new { x.exportBranchId, x.invNumber }).Select(x => new ItemTransferInvoice
            {
                invType = x.FirstOrDefault().invType,
                branchId = x.FirstOrDefault().exportBranchId,
                branchName = x.FirstOrDefault().exportBranch
            });
            var res1 = lst.GroupBy(x => new { x.importBranchId, x.invNumber }).Select(x => new ItemTransferInvoice
            {
                invType = x.FirstOrDefault().invType,
                branchId = x.FirstOrDefault().importBranchId,
                branchName = x.FirstOrDefault().importBranch
            });

            List<ItemTransferInvoice> result = new List<ItemTransferInvoice>();

            result.AddRange(res.ToList());

            if (selectedTab == 0)
            {
                if (chk_internalItemsTwoWay.IsChecked == true)
                    result.AddRange(res1.ToList());
            }

            invTypeCount = result.GroupBy(x => x.branchId).Select(x => new ItemTransferInvoice
            {
                ImportCount = x.Where(g => g.invType == "im").Count(),
                ExportCount = x.Where(g => g.invType == "ex").Count()
            }
            );

            var tempName = result.GroupBy(s => new { s.branchId }).Select(s => new
            {
                itemName = s.FirstOrDefault().branchName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();


            for (int i = 0; i < invTypeCount.Count(); i++)
            {
                cP.Add(invTypeCount.ToList().Skip(i).FirstOrDefault().ExportCount);
                cPb.Add(invTypeCount.ToList().Skip(i).FirstOrDefault().ImportCount);
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trExport")
            }); ;
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cPb.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trImport")
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillInternalPieChart(IEnumerable<ItemTransferInvoice> lst)
        {
            List<string> titles = new List<string>();
            List<string> titles1 = new List<string>();
            List<decimal> x = new List<decimal>();
            titles.Clear();

            //var temp = lst;

            var titleTemp = lst.GroupBy(m => new { m.itemId, m.unitId }).Select(m => new
            {
                agentName = m.FirstOrDefault().itemName + "\n" + m.FirstOrDefault().unitName
            });

            titles.AddRange(titleTemp.Select(jj => jj.agentName));
            var result = lst
                .GroupBy(s => new { s.itemId, s.unitId })
                .Select(s => new ItemTransferInvoice
                {
                    itemId = s.FirstOrDefault().itemId,
                    unitId = s.FirstOrDefault().unitId,
                    quantity = s.Sum(g => g.quantity),
                    itemName = s.FirstOrDefault().itemName,
                    unitName = s.FirstOrDefault().unitName,
                }).OrderByDescending(s => s.quantity);

            x = result.Select(m => (decimal)m.quantity).ToList();
            titles = result.Select(m => m.itemName).ToList();
            titles1 = result.Select(m => m.unitName).ToList();
            int count = x.Count();
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < count; i++)
            {
                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();
                if (i < 5)
                {
                    final.Add(x.Max());
                    lable.Add(titles.Skip(i).FirstOrDefault() + "-" + titles1.Skip(i).FirstOrDefault());
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
                //else if (buttonTag.Equals("rowChart"))
                //{
                //    w.type = "cartesianChart";
                //    w.cartesianChart.Series = rowChart.Series;
                //    w.axcolumn.Labels = MyAxis.Labels;
                //}
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
