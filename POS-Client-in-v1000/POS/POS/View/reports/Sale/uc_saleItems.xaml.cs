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
using System.Globalization;
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Threading;
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{
    public partial class uc_saleItems : UserControl
    {
        #region variables
        private int selectedTab = 0;
        Statistics statisticModel = new Statistics();
        List<ItemTransferInvoice> Items;

        //for combo boxes
        /*************************/
        Branch selectedBranch;
        ItemUnitCombo selectedItem;

        List<Branch> comboBranches;
        List<ItemUnitCombo> itemUnitCombos;

        ObservableCollection<Branch> comboBrachTemp = new ObservableCollection<Branch>();
        ObservableCollection<ItemUnitCombo> comboItemTemp = new ObservableCollection<ItemUnitCombo>();

        ObservableCollection<Branch> dynamicComboBranches;
        ObservableCollection<ItemUnitCombo> dynamicComboItem;

        Branch branchModel = new Branch();
        /*************************/

        ObservableCollection<int> selectedBranchId = new ObservableCollection<int>();

        ObservableCollection<int> selectedItemId = new ObservableCollection<int>();
        // all btn
        bool allItemsChk = false;
        bool allBranchesItemChk = false;
        bool allTypesChk = false;

        // report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        IEnumerable<ItemTransferInvoice> temp;
        #endregion

        public uc_saleItems()
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

        private static uc_saleItems _instance;
        public static uc_saleItems Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_saleItems();
                return _instance;
            }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tb_totalCurrency.Text = AppSettings.Currency;

                Items = await statisticModel.GetSaleitem((int)MainWindow.branchID, (int)MainWindow.userID);

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

                #region key up
                cb_ItemsBranches.IsTextSearchEnabled = false;
                cb_ItemsBranches.IsEditable = true;
                cb_ItemsBranches.StaysOpenOnEdit = true;
                cb_ItemsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_ItemsBranches.Text = "";

                cb_Items.IsTextSearchEnabled = false;
                cb_Items.IsEditable = true;
                cb_Items.StaysOpenOnEdit = true;
                cb_Items.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_Items.Text = "";

                cb_collect.IsTextSearchEnabled = false;
                cb_collect.IsEditable = true;
                cb_collect.StaysOpenOnEdit = true;
                cb_collect.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_collect.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                #region branch
                //comboBranches = await branchModel.GetAllWithoutMain("b");
                if (FillCombo.branchesAllWithoutMainReport is null)
                    await FillCombo.RefreshBranchsWithoutMainReport();
                comboBranches = FillCombo.branchesAllWithoutMainReport;
                #endregion

                itemUnitCombos = statisticModel.GetIUComboList(Items);

                dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
                dynamicComboItem = new ObservableCollection<ItemUnitCombo>(itemUnitCombos);

                fillComboBranches(cb_collect);
                fillComboItemsBranches(cb_ItemsBranches);
                fillComboItemTypes();

                resetItemTab();
                chk_allItems.IsEnabled = false;

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_items.Tag.ToString());

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
            tt_items.Content = MainWindow.resourcemanager.GetString("trItems");
            tt_collect.Content = MainWindow.resourcemanager.GetString("trBestSeller");

            chk_itemInvoice.Content = MainWindow.resourcemanager.GetString("tr_Invoice");
            chk_itemReturn.Content = MainWindow.resourcemanager.GetString("trReturned");
            chk_itemDrafs.Content = MainWindow.resourcemanager.GetString("trDraft");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_ItemsBranches, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_Items, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_Types, MainWindow.resourcemanager.GetString("trType") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_collect, MainWindow.resourcemanager.GetString("trBranchHint"));

            chk_allcollect.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allBranchesItem.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItems.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allTypes.Content = MainWindow.resourcemanager.GetString("trAll");


            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_ItemEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_ItemStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_collectEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_collectStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_number.Header = MainWindow.resourcemanager.GetString("trNo.");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_type.Header = MainWindow.resourcemanager.GetString("trType");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_itQuantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_invCount.Header = MainWindow.resourcemanager.GetString("trInvoices");
            col_price.Header = MainWindow.resourcemanager.GetString("trPrice");
            col_total.Header = MainWindow.resourcemanager.GetString("trTotal");
            col_avg.Header = MainWindow.resourcemanager.GetString("trItem") + "/" + MainWindow.resourcemanager.GetString("tr_Invoice");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

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
        private void fillComboBranches(ComboBox cb)
        {
            cb.SelectedValuePath = "branchId";
            cb.DisplayMemberPath = "name";
            cb.ItemsSource = dynamicComboBranches;
        }
        private void fillComboItemsBranches(ComboBox cb)
        {
            cb.SelectedValuePath = "branchId";
            cb.DisplayMemberPath = "name";
            cb.ItemsSource = comboBranches;
        }
        List<ItemUnitCombo> items = new List<ItemUnitCombo>();
        private void fillComboItems()
        {
            cb_Items.SelectedValuePath = "itemUnitId";
            cb_Items.DisplayMemberPath = "itemUnitName";
            items = Items.Where(i => i.ITtype.ToString() == cb_Types.SelectedValue.ToString()).GroupBy(i => i.ITitemUnitId).Select(i => new ItemUnitCombo
            {
                itemUnitId = i.FirstOrDefault().ITitemUnitId.Value,
                itemUnitName = i.FirstOrDefault().ITitemName + "-" + i.FirstOrDefault().ITunitName,
            }).ToList();

            cb_Items.ItemsSource = items.Where(x=> !selectedItemId.Contains(x.itemUnitId));
        }
        private void fillComboItems(ObservableCollection<ItemUnitCombo> items)
        {
            cb_Items.SelectedValuePath = "itemUnitId";
            cb_Items.DisplayMemberPath = "itemUnitName";
            cb_Items.ItemsSource = items;
        }
        public void notCheckAll()
        {
            fillEmptyItemsEvent();
            dynamicComboItem = new ObservableCollection<ItemUnitCombo>(itemUnitCombos);
            fillComboItems();
        }
        public void notCheckAllCollect()
        {
            collectListAll = new List<ItemTransferInvoice>();
            fillEmptyCollectEvent();
            dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
            fillComboBranches(cb_collect);
        }
        private void fillComboItemTypes()
        {
            var typelist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trNormal")               , Value = "n" },//normal
                //new { Text = MainWindow.resourcemanager.GetString("trHaveExpirationDate")   , Value = "d" },//expired
                new { Text = MainWindow.resourcemanager.GetString("trHaveSerialNumber")     , Value = "sn" },//serial
                new { Text = MainWindow.resourcemanager.GetString("trService")              , Value = "sr" },//service
                new { Text = MainWindow.resourcemanager.GetString("trPackage")              , Value = "p" },//package
                 };
            cb_Types.DisplayMemberPath = "Text";
            cb_Types.SelectedValuePath = "Value";
            cb_Types.ItemsSource = typelist;
        }
        public void fillItemsEvent()
        {
            temp = fillList(Items, cb_ItemsBranches, cb_Items, chk_itemInvoice, chk_itemReturn, dp_ItemStartDate, dp_ItemEndDate)
                .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));

            dgInvoice.ItemsSource = temp;

            txt_count.Text = dgInvoice.Items.Count.ToString();

            decimal total = 0;
            total = temp.Select(b => b.subTotal.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            fillPieChart();
            fillColumnChart();
            fillRowChart(selectedItemId);
            //if (cb_ItemsBranches.SelectedItem != null)
            //{
            //    chk_allTypes.IsEnabled = true;
            //    allTypesSet();
            //    allItemsSet();
            //}


        }
        public void fillEmptyItemsEvent()
        {
            temp = new List<ItemTransferInvoice>();
            itemList = new List<ItemTransferInvoice>();
            //  Items = new List<ItemTransferInvoice>();
            //  temp = fillList(Items, cb_ItemsBranches, cb_Items, chk_itemInvoice, chk_itemReturn, dp_ItemStartDate, dp_ItemEndDate)
            //    .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));

            dgInvoice.ItemsSource = temp;

            txt_count.Text = dgInvoice.Items.Count.ToString();
            decimal total = 0;
            total = temp.Select(b => b.subTotal.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            fillPieChart();
            fillColumnChart();
            fillEmptyRowChart(selectedItemId);
        }
        public void fillCollectEvent()
        {
            temp = fillCollectListBranch(Items, dp_collectStartDate, dp_collectEndDate)
                .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));

            dgInvoice.ItemsSource = temp;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            fillPieChartCollect();
            fillColumnChartCollect();
            fillRowChartCollect();
        }
        public void fillEmptyCollectEvent()
        {
            List<ItemTransferInvoice> items = new List<ItemTransferInvoice>();
            temp = fillCollectListBranch(items, dp_collectStartDate, dp_collectEndDate)
                .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));

            dgInvoice.ItemsSource = temp;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            fillPieChartCollect();
            fillColumnChartCollect();
            fillRowChartCollect();
        }
        public void fillCollectEventAll()
        {
            temp = fillCollectListAll(Items, dp_collectStartDate, dp_collectEndDate);

            dgInvoice.ItemsSource = temp;

            txt_count.Text = dgInvoice.Items.Count.ToString();

            fillPieChartCollect();
            fillColumnChartCollect();
            fillRowChartCollect();
        }
        public void paint()
        {
            bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_items.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_collect.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_items.Fill = Brushes.White;
            path_collect.Fill = Brushes.White;
        }
        private void resetItemTab()
        {
            Chk_allBranchesItem_Checked(chk_allBranchesItem, null);
            //Chk_allTypes_Checked(chk_allTypes, null);
            //Chk_allItems_Checked(chk_allItems, null);
            chk_itemInvoice.IsChecked = true;

            dp_ItemStartDate.SelectedDate = null;
            dp_ItemEndDate.SelectedDate = null;
        }
        // Chk_allcollect_Checked(chk_allcollect,null);
        private void resetCollectTab()
        {
            Chk_allcollect_Checked(chk_allcollect, null);
            //Chk_allTypes_Checked(chk_allTypes, null);
            //Chk_allItems_Checked(chk_allItems, null);
            chk_allcollect.IsChecked = true;

            dp_collectStartDate.SelectedDate = null;
            dp_collectEndDate.SelectedDate = null;
        }
        private void allItemsUnSet()
        {

            allItemsChk = false;
            chk_allItems.IsChecked = false;
            chk_allItems.IsEnabled = false;
            cb_Items.IsEnabled = false;
            cb_Items.SelectedItem = null;

            comboItemTemp.Clear();
            stk_tagsItems.Children.Clear();
            selectedItemId.Clear();
        }
        private void allItemsSet()
        {

            allItemsChk = true;
            chk_allItems.IsChecked = true;
            cb_Items.SelectedItem = null;
            cb_Items.IsEnabled = false;
            chk_allItems.IsEnabled = true;
            //for (int i = 0; i < comboItemTemp.Count; i++)
            //{
            //    ItemUnitCombo iutemp = comboItemTemp.Skip(i).FirstOrDefault();
            //    int count = dynamicComboItem.Where(x => x.itemUnitId == iutemp.itemUnitId).ToList().Count();
            //    if (count == 0)
            //        dynamicComboItem.Add(iutemp);
            //    //dynamicComboItem.Add(comboItemTemp.Skip(i).FirstOrDefault());
            //}
            comboItemTemp.Clear();
            stk_tagsItems.Children.Clear();
            selectedItemId.Clear();
        }
        private void allTypesSet()
        {
            allTypesChk = true;
            chk_allTypes.IsChecked = true;
            cb_Types.SelectedItem = null;
            cb_Types.IsEnabled = false;
            chk_allTypes.IsEnabled = true;
        }
        private void allTypesUnset()
        {

            allTypesChk = false;
            chk_allTypes.IsChecked = false;
            cb_Types.IsEnabled = false;
            cb_Types.SelectedItem = null;
            //
            cb_Items.IsEnabled = false;
            chk_allItems.IsEnabled = false;

        }
        #endregion

        #region tabs
        private void btn_items_Click(object sender, RoutedEventArgs e)
        {//items
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                txt_search.Text = "";
                selectedTab = 0;

                ReportsHelp.showSelectedStack(grid_stacks, stk_tagsItems);

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_items);
                path_items.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                ReportsHelp.hideAllColumns(dgInvoice);
                //show columns
                grid_items.Visibility = Visibility.Visible;
                grid_collect.Visibility = Visibility.Hidden;
                col_number.Visibility = Visibility.Visible;
                col_date.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;
                col_itQuantity.Visibility = Visibility.Visible;
                col_price.Visibility = Visibility.Visible;
                col_total.Visibility = Visibility.Visible;
                brd_total.Visibility = Visibility.Visible;
                resetItemTab();
                //  fillItemsEvent();
                //stk_tagsBranches.Children.Clear();
                //selectedBranchId.Clear();
                //if ( == false)
                //{
                //    notCheckAll();
                //}
                //else
                //{
                //    fillEvent();
                //}
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
        private void Btn_collect_Click(object sender, RoutedEventArgs e)
        {//collect
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                txt_search.Text = "";
                selectedTab = 1;

                ReportsHelp.showSelectedStack(grid_stacks, stk_tagsBranches);

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_collect);
                path_collect.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                ReportsHelp.hideAllColumns(dgInvoice);
                //show columns
                grid_items.Visibility = Visibility.Hidden;
                grid_collect.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_itQuantity.Visibility = Visibility.Visible;
                col_invCount.Visibility = Visibility.Visible;
                col_avg.Visibility = Visibility.Visible;
                brd_total.Visibility = Visibility.Collapsed;
                resetCollectTab();

                //if (stk_tagsBranches.Children.Count == 0)
                //{
                //    fillCollectEventAll();
                //}
                //else
                //{
                //    fillCollectEvent();
                //}


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

        #region Items Events
        private void Chip_OnDeleteItemClick(object sender, RoutedEventArgs e)
        {
            var currentChip = (Chip)sender;
            stk_tagsItems.Children.Remove(currentChip);
            var m = comboItemTemp.Where(j => j.itemUnitId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
            dynamicComboItem.Add(m.FirstOrDefault());
            selectedItemId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
            if (selectedItemId.Count == 0)
            {
                cb_Items.SelectedItem = null;
                allItemsSet();
            }
            fillComboItems();
            fillItemsEvent();

        }
        private void cb_Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Items.SelectedItem != null)
            {
                if (stk_tagsItems.Children.Count < 5)
                {
                    selectedItem = cb_Items.SelectedItem as ItemUnitCombo;
                    var b = new MaterialDesignThemes.Wpf.Chip()
                    {
                        Content = selectedItem.itemUnitName,
                        Name = "btn" + selectedItem.itemUnitId.ToString(),
                        IsDeletable = true,
                        Margin = new Thickness(5, 0, 5, 0)
                    };
                    b.DeleteClick += Chip_OnDeleteItemClick;
                    stk_tagsItems.Children.Add(b);
                    comboItemTemp.Add(selectedItem);
                    selectedItemId.Add(selectedItem.itemUnitId);
                    dynamicComboItem.Remove(selectedItem);
                    fillComboItems();
                    fillItemsEvent();
                }
            }
        }
        private void chk_allItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_Items.IsEnabled == true)
                {
                    cb_Items.SelectedItem = null;
                    cb_Items.IsEnabled = false;
                    cb_Items.Text = "";
                    cb_Items.ItemsSource = dynamicComboItem;
                    for (int i = 0; i < comboItemTemp.Count; i++)
                    {
                        ItemUnitCombo iutemp = comboItemTemp.Skip(i).FirstOrDefault();
                        int count = dynamicComboItem.Where(x => x.itemUnitId == iutemp.itemUnitId).ToList().Count();
                        if (count == 0)
                            dynamicComboItem.Add(iutemp);
                        //dynamicComboItem.Add(comboItemTemp.Skip(i).FirstOrDefault());
                    }
                    comboItemTemp.Clear();
                    stk_tagsItems.Children.Clear();
                    selectedItemId.Clear();
                }
                else
                {
                    cb_Items.IsEnabled = true;
                }

                fillItemsEvent();
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
        private void fillItemsEventCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (allItemsChk == false
              && (selectedTab == 0 && stk_tagsItems.Children.Count == 0))

                {
                    notCheckAll();
                }
                else
                {
                    fillItemsEvent();
                }
                // fillItemsEvent();

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
        private void fillCollectEventsCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillCollectEvent();

                if (stk_tagsBranches.Children.Count == 0)
                {
                    fillCollectEventAll();
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
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            fillItemsEventCall(sender);
        }
        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillItemsEventCall(sender);
        }
        private void dt_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            fillItemsEventCall(sender);
        }
        private void Cb_ItemsBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_ItemsBranches.SelectedItem != null)
            {
                // chk_allTypes.IsEnabled = true;
                allTypesSet();
                allItemsSet();
            }
            else
            {
                allTypesUnset();
                allItemsUnSet();
            }
            fillItemsEvent();
        }
        private void Cb_Types_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Types.SelectedItem != null || (cb_Types.SelectedItem == null && allTypesChk))
            {
                allItemsSet();
                fillComboItems();
                chk_allItems.IsEnabled = true;
            }
            else
            {
                allItemsUnSet();
            }
            fillItemsEvent();
        }
        private void Chk_allBranchesItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                allBranchesItemChk = true;
                cb_ItemsBranches.SelectedItem = null;
                cb_ItemsBranches.IsEnabled = false;
                cb_ItemsBranches.Text = "";
                cb_ItemsBranches.ItemsSource = comboBranches;
                //cb_Types.IsEnabled = false;
                //chk_allTypes.IsEnabled = true;
                ////
                //cb_Items.IsEnabled = false;
                //chk_allItems.IsEnabled = true;
                allTypesSet();
                allItemsSet();
                fillItemsEvent();
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
        private void Chk_allBranchesItem_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                allBranchesItemChk = false;
                chk_allBranchesItem.IsChecked = false;
                cb_ItemsBranches.IsEnabled = true;

                cb_Types.IsEnabled = false;
                chk_allTypes.IsEnabled = false;
                //
                cb_Items.IsEnabled = false;
                chk_allItems.IsEnabled = false;
                allTypesUnset();
                allItemsUnSet();
                fillEmptyItemsEvent();

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
        private void Chk_allItems_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                allItemsChk = true;
                cb_Items.SelectedItem = null;
                cb_Items.IsEnabled = false;
                cb_Items.Text = "";
                //cb_Items.ItemsSource = items;
                //fillComboItems();
                for (int i = 0; i < comboItemTemp.Count; i++)
                {
                    ItemUnitCombo iutemp = comboItemTemp.Skip(i).FirstOrDefault();
                    int count = dynamicComboItem.Where(x => x.itemUnitId == iutemp.itemUnitId).ToList().Count();
                    if (count == 0)
                        dynamicComboItem.Add(iutemp);
                   // dynamicComboItem.Add(comboItemTemp.Skip(i).FirstOrDefault());
                }
                comboItemTemp.Clear();
                stk_tagsItems.Children.Clear();
                selectedItemId.Clear();
                fillItemsEvent();

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
        private void Chk_allItems_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                allItemsChk = false;
                chk_allItems.IsChecked = false;
                cb_Items.IsEnabled = true;

                notCheckAll();
                //  fillItemsEvent();

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
        private void Chk_allTypes_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //chk_allTypes.IsEnabled
                cb_Types.Text = "";
                cb_Types.SelectedIndex = -1;
                allTypesSet();
                allItemsSet();
                fillItemsEvent();
                chk_allItems.IsChecked = true;
                chk_allItems.IsEnabled = false;

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
        private void Chk_allTypes_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                allTypesChk = false;
                chk_allTypes.IsChecked = false;
                cb_Types.IsEnabled = true;
                //
                //cb_Items.IsEnabled = false;
                //chk_allItems.IsEnabled = false;
                allItemsUnSet();
                fillEmptyItemsEvent();
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

        #region Charts
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<int> x = new List<int>();
            titles.Clear();

            //var temp = itemList
            var tempPie = temp
             .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));
            var titleTemp = tempPie.GroupBy(jj => jj.ITitemUnitId)
             .Select(g => new ItemUnitCombo
             {
                 itemUnitId = (int)g.FirstOrDefault().ITitemUnitId,
                 itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName
             }).ToList();
            titles.AddRange(titleTemp.Select(jj => jj.itemUnitName));
            var result = tempPie.GroupBy(s => s.ITitemUnitId).Select(s => new
            {
                ITitemUnitId = s.Key,
                count = s.Count()
            });
            x = result.Select(m => m.count).ToList();
            int count = x.Count();
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < count; i++)
            {
                List<long> final = new List<long>();
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
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> x = new List<int>();
            IEnumerable<int> y = null;

            //var temp = itemList
            var tempColumn = temp
                   .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));
            var result = tempColumn.GroupBy(s => s.ITitemUnitId).Select(s => new
            {
                ITitemUnitId = s.Key,
                countP = s.Where(m => m.invType == "s").Count(),
                countPb = s.Where(m => m.invType == "sb").Count(),

            });
            x = result.Select(m => m.countP).ToList();
            y = result.Select(m => m.countPb);
            var tempName = tempColumn.GroupBy(jj => jj.ITitemUnitId)
             .Select(g => new ItemUnitCombo { itemUnitId = (int)g.FirstOrDefault().ITitemUnitId, itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName }).ToList();
            names.AddRange(tempName.Select(nn => nn.itemUnitName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();
            List<int> cD = new List<int>();
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("trSales"),
                MainWindow.resourcemanager.GetString("trReturned"),
                MainWindow.resourcemanager.GetString("trDraft"),
            };
            int count = x.Count();
            for (int i = 0; i < count; i++)
            {
                if (i < 6)
                {
                    cP.Add(x.Max());
                    x.Remove(x.Max());
                }
                else
                {
                    cP.Add(x.Sum());
                    break;
                }
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (count >= 6)
                axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));

            //3 فوق بعض
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                Title = titles[0],
                DataLabels = true,
            });
            columnChartData.Add(
           new StackedColumnSeries
           {
               Values = cPb.AsChartValues(),
               Title = titles[1],
               DataLabels = true,
           });


            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillRowChart(ObservableCollection<int> stackedButton)
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;
            IEnumerable<decimal> pbTemp = null;
            IEnumerable<decimal> resultTemp = null;

            var temp = fillRowChartList(Items, chk_itemInvoice, chk_itemReturn, chk_itemDrafs, dp_ItemStartDate, dp_ItemEndDate, dt_itemStartTime, dt_ItemEndTime);
            temp = temp.Where(j => (selectedItemId.Count != 0 ? stackedButton.Contains((int)j.ITitemUnitId) : true));
            var result = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
            {
                ITitemUnitId = s.Key,
                totalP = s.Where(x => x.invType == "s").Sum(x => x.totalNet),
                totalPb = s.Where(x => x.invType == "sb").Sum(x => x.totalNet),
            }
         );
            var resultTotal = result.Select(x => new { x.ITitemUnitId, total = x.totalP - x.totalPb }).ToList();
            pTemp = result.Select(x => (decimal)x.totalP);
            pbTemp = result.Select(x => (decimal)x.totalPb);
            resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            var tempName = temp.GroupBy(jj => jj.ITitemUnitId)
             .Select(g => new ItemUnitCombo { itemUnitId = (int)g.FirstOrDefault().ITitemUnitId, itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName }).ToList();
            names.AddRange(tempName.Select(nn => nn.itemUnitName));
            /********************************************************************************/
            SeriesCollection rowChartData = new SeriesCollection();
            List<decimal> purchase = new List<decimal>();
            List<decimal> returns = new List<decimal>();
            List<decimal> sub = new List<decimal>();
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("trNetSales"),
                MainWindow.resourcemanager.GetString("trTotalReturn"),
                MainWindow.resourcemanager.GetString("trTotalSales")
            };

            int xCount = 0;
            if (pTemp.Count() <= 6) xCount = pTemp.Count();

            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (pTemp.Count() > 6)
            {
                decimal purchaseSum = 0, returnsSum = 0, subSum = 0;
                for (int i = 0; i < xCount; i++)
                {
                    purchaseSum = purchaseSum + pTemp.ToList().Skip(i).FirstOrDefault();
                    returnsSum = returnsSum + pbTemp.ToList().Skip(i).FirstOrDefault();
                    subSum = subSum + resultTemp.ToList().Skip(i).FirstOrDefault();
                }
                if (!((purchaseSum == 0) && (returnsSum == 0) && (subSum == 0)))
                {
                    purchase.Add(purchaseSum);
                    returns.Add(returnsSum);
                    sub.Add(subSum);
                    MyAxis.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = purchase.AsChartValues(),
              Title = titles[0]
          });
            rowChartData.Add(
         new LineSeries
         {
             Values = returns.AsChartValues(),
             Title = titles[1]
         });
            rowChartData.Add(
        new LineSeries
        {
            Values = sub.AsChartValues(),
            Title = titles[2]

        });
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void fillEmptyRowChart(ObservableCollection<int> stackedButton)
        {
            List<ItemTransferInvoice> items = new List<ItemTransferInvoice>();
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;
            IEnumerable<decimal> pbTemp = null;
            IEnumerable<decimal> resultTemp = null;

            var temp = fillRowChartList(items, chk_itemInvoice, chk_itemReturn, chk_itemDrafs, dp_ItemStartDate, dp_ItemEndDate, dt_itemStartTime, dt_ItemEndTime);
            temp = temp.Where(j => (selectedItemId.Count != 0 ? stackedButton.Contains((int)j.ITitemUnitId) : true));
            var result = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
            {
                ITitemUnitId = s.Key,
                totalP = s.Where(x => x.invType == "s").Sum(x => x.totalNet),
                totalPb = s.Where(x => x.invType == "sb").Sum(x => x.totalNet),
            }
         );
            var resultTotal = result.Select(x => new { x.ITitemUnitId, total = x.totalP - x.totalPb }).ToList();
            pTemp = result.Select(x => (decimal)x.totalP);
            pbTemp = result.Select(x => (decimal)x.totalPb);
            resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            var tempName = temp.GroupBy(jj => jj.ITitemUnitId)
             .Select(g => new ItemUnitCombo { itemUnitId = (int)g.FirstOrDefault().ITitemUnitId, itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName }).ToList();
            names.AddRange(tempName.Select(nn => nn.itemUnitName));
            /********************************************************************************/
            SeriesCollection rowChartData = new SeriesCollection();
            List<decimal> purchase = new List<decimal>();
            List<decimal> returns = new List<decimal>();
            List<decimal> sub = new List<decimal>();
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("trNetSales"),
                MainWindow.resourcemanager.GetString("trTotalReturn"),
                MainWindow.resourcemanager.GetString("trTotalSales")
            };

            int xCount = 0;
            if (pTemp.Count() <= 6) xCount = pTemp.Count();

            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (pTemp.Count() > 6)
            {
                decimal purchaseSum = 0, returnsSum = 0, subSum = 0;
                for (int i = 0; i < xCount; i++)
                {
                    purchaseSum = purchaseSum + pTemp.ToList().Skip(i).FirstOrDefault();
                    returnsSum = returnsSum + pbTemp.ToList().Skip(i).FirstOrDefault();
                    subSum = subSum + resultTemp.ToList().Skip(i).FirstOrDefault();
                }
                if (!((purchaseSum == 0) && (returnsSum == 0) && (subSum == 0)))
                {
                    purchase.Add(purchaseSum);
                    returns.Add(returnsSum);
                    sub.Add(subSum);
                    MyAxis.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = purchase.AsChartValues(),
              Title = titles[0]
          });
            rowChartData.Add(
         new LineSeries
         {
             Values = returns.AsChartValues(),
             Title = titles[1]
         });
            rowChartData.Add(
        new LineSeries
        {
            Values = sub.AsChartValues(),
            Title = titles[2]

        });
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion

        #region Collect Events
        private void Dp_colletSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillCollectEventsCall(sender);
        }
        private void Chip_OnDeleteBranchClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                var currentChip = (Chip)sender;
                stk_tagsBranches.Children.Remove(currentChip);
                var m = comboBrachTemp.Where(j => j.branchId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                dynamicComboBranches.Add(m.FirstOrDefault());
                selectedBranchId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                if (selectedBranchId.Count == 0)
                {
                    cb_collect.SelectedItem = null;

                }

                if (stk_tagsBranches.Children.Count == 0)
                {

                    fillCollectEventAll();
                    chk_allcollect.IsChecked = true;
                    cb_collect.IsEnabled = false;
                }
                else
                {
                    fillCollectEvent();
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
        private void Cb_collect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_collect.SelectedItem != null)
                {
                    if (stk_tagsBranches.Children.Count < 5)
                    {
                        selectedBranch = cb_collect.SelectedItem as Branch;
                        var b = new MaterialDesignThemes.Wpf.Chip()
                        {
                            Content = selectedBranch.name,
                            Name = "btn" + selectedBranch.branchId.ToString(),
                            IsDeletable = true,
                            Margin = new Thickness(5, 0, 5, 0)
                        };
                        b.DeleteClick += Chip_OnDeleteBranchClick;
                        stk_tagsBranches.Children.Add(b);
                        comboBrachTemp.Add(selectedBranch);
                        selectedBranchId.Add(selectedBranch.branchId);
                        dynamicComboBranches.Remove(selectedBranch);
                        fillCollectEvent();
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
        private void Chk_allcollect_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_collect.SelectedItem = null;
                cb_collect.IsEnabled = false;
                //chk_allcollect.IsChecked = true;
                cb_collect.Text = "";
                cb_collect.ItemsSource = dynamicComboBranches;
                for (int i = 0; i < comboBrachTemp.Count; i++)
                {
                    dynamicComboBranches.Add(comboBrachTemp.Skip(i).FirstOrDefault());
                }
                comboBrachTemp.Clear();
                stk_tagsBranches.Children.Clear();
                selectedBranchId.Clear();
                fillCollectEvent();
                //if (stk_tagsBranches.Children.Count == 0)
                //{
                //    fillCollectEventAll();
                //}
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
        private void Chk_allcollect_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_collect.IsEnabled = true;

                notCheckAllCollect();
                //  fillCollectEvent();
                //if (stk_tagsBranches.Children.Count == 0)
                //{
                //    fillCollectEventAll();
                //}
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

        #region fillLists
        List<ItemTransferInvoice> itemList = null;
        private List<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> Invoices, ComboBox cbBranch, ComboBox cbitem, CheckBox chkInvoice, CheckBox chkReturn, DatePicker startDate, DatePicker endDate)
        {
            var selectedBranch = cbBranch.SelectedItem as Branch;
            var selectedItemUnit = cbitem.SelectedItem as ItemUnitCombo;

            var result = Invoices.Where(x => (

           ((chkReturn.IsChecked == true ? (x.invType == "sb") : false) || (chkInvoice.IsChecked == true ? (x.invType == "s") : false))
                      && (cbBranch.SelectedItem != null ? x.branchCreatorId == selectedBranch.branchId : true)
                      && (cb_Items.SelectedItem != null ? x.ITitemUnitId == selectedItemUnit.itemUnitId : true)
                      && (cb_Types.SelectedItem != null ? x.ITtype == cb_Types.SelectedValue.ToString() : true)
                      && (startDate.SelectedDate != null ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                      && (endDate.SelectedDate != null ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)));

            itemList = result.ToList();
            return result.ToList();
        }

        private IEnumerable<ItemTransferInvoice> fillRowChartList(IEnumerable<ItemTransferInvoice> Invoices, CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft, DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            var result = Invoices.Where(x => (
                         (startDate.SelectedDate != null ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                        && (endDate.SelectedDate != null ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)));

            return result;
        }
        List<ItemTransferInvoice> collectListBranch = null;
        private List<ItemTransferInvoice> fillCollectListBranch(IEnumerable<ItemTransferInvoice> Invoices, DatePicker startDate, DatePicker endDate)
        {
            var temp = Invoices
                .Where(x =>
                 (startDate.SelectedDate != null ? ((DateTime)x.updateDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                && (endDate.SelectedDate != null ? ((DateTime)x.updateDate).Date <= ((DateTime)endDate.SelectedDate).Date : true))
                .GroupBy(obj => new
                {
                    obj.branchCreatorId,
                    obj.ITitemUnitId
                }).Select(obj => new ItemTransferInvoice
                {
                    branchCreatorId = obj.FirstOrDefault().branchCreatorId,
                    branchCreatorName = obj.FirstOrDefault().branchCreatorName,
                    itemUnitId = obj.FirstOrDefault().itemUnitId,
                    ItemUnits = obj.FirstOrDefault().ItemUnits,
                    invoiceId = obj.FirstOrDefault().invoiceId,
                    ITquantity = obj.Sum(x => x.ITquantity),
                    ITitemName = obj.FirstOrDefault().ITitemName,
                    ITitemId = obj.FirstOrDefault().ITitemId,
                    ITunitName = obj.FirstOrDefault().ITunitName,
                    ITunitId = obj.FirstOrDefault().ITunitId,
                    ITupdateDate = obj.FirstOrDefault().ITupdateDate,
                    itemAvg = obj.Average(x => x.ITquantity),
                    count = obj.Count()
                }).OrderByDescending(obj => obj.ITquantity);

            collectListBranch = temp.ToList();
            return temp.ToList();
        }
        List<ItemTransferInvoice> collectListAll = null;
        private List<ItemTransferInvoice> fillCollectListAll(IEnumerable<ItemTransferInvoice> Invoices, DatePicker startDate, DatePicker endDate)
        {
            var temp = Invoices
                .Where(x =>
                 (startDate.SelectedDate != null ? ((DateTime)x.updateDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                && (endDate.SelectedDate != null ? ((DateTime)x.updateDate).Date <= ((DateTime)endDate.SelectedDate).Date : true))
                .GroupBy(obj => new
                {
                    obj.ITitemUnitId
                }).Select(obj => new ItemTransferInvoice
                {
                    branchCreatorId = obj.FirstOrDefault().branchCreatorId,
                    branchCreatorName = "All Branches",
                    itemUnitId = obj.FirstOrDefault().itemUnitId,
                    ItemUnits = obj.FirstOrDefault().ItemUnits,
                    invoiceId = obj.FirstOrDefault().invoiceId,
                    ITquantity = obj.Sum(x => x.ITquantity),
                    ITitemName = obj.FirstOrDefault().ITitemName,
                    ITitemId = obj.FirstOrDefault().ITitemId,
                    ITunitName = obj.FirstOrDefault().ITunitName,
                    ITunitId = obj.FirstOrDefault().ITunitId,
                    itemAvg = obj.Average(x => x.ITquantity),
                    ITupdateDate = obj.FirstOrDefault().ITupdateDate,
                    count = obj.Count()
                }).OrderByDescending(obj => obj.ITquantity);

            collectListAll = temp.ToList();
            return temp.ToList();
        }
        #endregion

        #region General Events
        private void Cb_ItemsBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = comboBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_Items_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = items.Where(p => p.itemUnitName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_collect_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = dynamicComboBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Collect all generations of memory.
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
        private async void DgInvoice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                invoice = new Invoice();
                if (dgInvoice.SelectedIndex != -1)
                {
                    ItemTransferInvoice item = dgInvoice.SelectedItem as ItemTransferInvoice;
                    if (item.invoiceId > 0)
                    {
                        invoice = await invoice.GetByInvoiceId(item.invoiceId);
                        MainWindow.mainWindow.BTN_sales_Click(MainWindow.mainWindow.btn_sales, null);
                        uc_sales.Instance.UserControl_Loaded(null, null);
                        uc_sales.Instance.Btn_receiptInvoice_Click(uc_sales.Instance.btn_reciptInvoice, null);
                        uc_receiptInvoice.Instance.UserControl_Loaded(null, null);
                        uc_receiptInvoice._InvoiceType = invoice.invType;
                        uc_receiptInvoice.Instance.invoice = invoice;
                        uc_receiptInvoice.isFromReport = true;
                        if (item.archived == 0)
                            uc_receiptInvoice.archived = false;
                        else
                            uc_receiptInvoice.archived = true;
                        await uc_receiptInvoice.Instance.fillInvoiceInputs(invoice);
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
        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                IEnumerable<ItemTransferInvoice> tempSearch = itemList;

                if (selectedTab == 0)
                {
                    //tempSearch = itemList.Where(s =>
                    temp = temp.Where(s =>
                                                s.invNumber.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                 s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                 s.ITitemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                 s.ITunitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                 s.ITquantity.ToString().ToLower().Contains(txt_search.Text.ToLower()) ||
                                                 s.ITprice.ToString().ToLower().Contains(txt_search.Text.ToLower()) ||
                                                 s.subTotal.ToString().ToLower().Contains(txt_search.Text.ToLower())
                                                 );
                    decimal total = 0;
                    total = temp.Select(b => b.subTotal.Value).Sum();
                    tb_total.Text = SectionData.DecTostring(total);
                    fillPieChart();
                    fillColumnChart();
                    fillRowChart(selectedItemId);
                }
                else if (selectedTab == 1)
                {
                    var items = dgInvoice.ItemsSource as IEnumerable<ItemTransferInvoice>;
                    //tempSearch = items.Where(s =>
                    temp = temp.Where(s =>
                                                     s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                     s.ITitemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                     s.ITunitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                     s.count.ToString().ToLower().Contains(txt_search.Text.ToLower()) ||
                                                     s.ITquantity.ToString().ToLower().Contains(txt_search.Text.ToLower()) ||
                                                     s.itemAvg.ToString().ToLower().Contains(txt_search.Text.ToLower())
                                                     );
                    fillPieChartCollect();
                    fillColumnChartCollect();
                    fillRowChartCollect();
                }

                //dgInvoice.ItemsSource = tempSearch;
                dgInvoice.ItemsSource = temp;
                txt_count.Text = dgInvoice.Items.Count.ToString();


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
                Items = await statisticModel.GetSaleitem((int)MainWindow.branchID, (int)MainWindow.userID);
                txt_search.Clear();

                if (selectedTab == 0)
                {
                    chk_allBranchesItem.IsChecked = true;
                    chk_allItems.IsChecked = true;
                    chk_itemInvoice.IsChecked = true;
                    chk_itemReturn.IsChecked = true;
                    //dp_ItemStartDate.SelectedDate = null;
                    //dp_ItemEndDate.SelectedDate = null;
                    resetItemTab();
                    fillItemsEvent();
                }
                else if (selectedTab == 1)
                {
                    dp_collectEndDate.SelectedDate = null;
                    dp_collectStartDate.SelectedDate = null;
                    chk_allcollect.IsChecked = true;
                    stk_tagsBranches.Children.Clear();
                    resetItemTab();
                    fillCollectEventAll();
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

        #region collect charts
        private void fillPieChartCollect()
        {
            try
            {
                List<string> titles = new List<string>();
                List<long> x = new List<long>();
                titles.Clear();
                var temp = collectListAll;
                if (stk_tagsBranches.Children.Count > 0)
                {
                    temp = collectListBranch
                     .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true)).ToList();
                }
                var titleTemp = temp.Select(obj => obj.ITitemUnitName1);
                titles.AddRange(titleTemp);
                x = temp.Select(m => (long)m.ITquantity).ToList();
                int count = x.Count();
                SeriesCollection piechartData = new SeriesCollection();
                for (int i = 0; i < count; i++)
                {
                    List<long> final = new List<long>();
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
            catch { }
        }

        private void fillColumnChartCollect()
        {
            try
            {
                axcolumn.Labels = new List<string>();
                List<string> names = new List<string>();
                List<int> x = new List<int>();

                var temp = collectListAll;
                if (stk_tagsBranches.Children.Count > 0)
                {
                    temp = collectListBranch
                     .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true)).ToList();
                }

                x = temp.Select(m => m.count).ToList();
                var tempName = temp.OrderByDescending(obj => obj.count).Select(obj => obj.ITitemUnitName1);
                names.AddRange(tempName);

                SeriesCollection columnChartData = new SeriesCollection();
                List<int> cP = new List<int>();
                List<int> cPb = new List<int>();
                List<int> cD = new List<int>();
                List<string> titles = new List<string>()
            {
               MainWindow.resourcemanager.GetString("trInvoices")
            };
                int count = x.Count();
                for (int i = 0; i < count; i++)
                {
                    if (i < 5)
                    {
                        cP.Add(x.Max());
                        x.Remove(x.Max());
                    }
                    else
                    {
                        cP.Add(x.Sum());
                        break;
                    }
                    axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());

                }
                axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                //3 فوق بعض
                columnChartData.Add(
                new ColumnSeries
                {
                    Values = cP.AsChartValues(),
                    Title = titles[0],
                    DataLabels = true,
                });

                DataContext = this;
                cartesianChart.Series = columnChartData;
            }
            catch { }
        }

        private void fillRowChartCollect()
        {
            try
            {
                int endYear = DateTime.Now.Year;
                int startYear = endYear - 1;
                int startMonth = DateTime.Now.Month;
                int endMonth = startMonth;
                if (dp_collectStartDate.SelectedDate != null && dp_collectEndDate.SelectedDate != null)
                {
                    startYear = dp_collectStartDate.SelectedDate.Value.Year;
                    endYear = dp_collectEndDate.SelectedDate.Value.Year;
                    startMonth = dp_collectStartDate.SelectedDate.Value.Month;
                    endMonth = dp_collectEndDate.SelectedDate.Value.Month;
                }
                MyAxis.Labels = new List<string>();
                List<string> names = new List<string>();
                List<CashTransferSts> resultList = new List<CashTransferSts>();

                var temp = collectListAll;
                if (stk_tagsBranches.Children.Count > 0)
                {
                    temp = collectListBranch
                     .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true)).ToList();
                }

                SeriesCollection rowChartData = new SeriesCollection();
                var tempName = temp.Select(s => s.IupdateDate);
                names.Add("x");

                List<string> lable = new List<string>();
                SeriesCollection columnChartData = new SeriesCollection();
                List<long> cash = new List<long>();

                if (endYear - startYear <= 1)
                {
                    for (int year = startYear; year <= endYear; year++)
                    {
                        for (int month = startMonth; month <= 12; month++)
                        {
                            var firstOfThisMonth = new DateTime(year, month, 1);
                            var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                            var drawCash = temp.ToList().Where(c => c.ITupdateDate > firstOfThisMonth && c.ITupdateDate <= firstOfNextMonth).Sum(obj => (long)obj.ITquantity);
                            cash.Add(drawCash);
                            MyAxis.Labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);

                            if (year == endYear && month == endMonth)
                            {
                                break;
                            }
                            if (month == 12)
                            {
                                startMonth = 1;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int year = startYear; year <= endYear; year++)
                    {
                        var firstOfThisYear = new DateTime(year, 1, 1);
                        var firstOfNextMYear = firstOfThisYear.AddYears(1);
                        var drawCash = temp.ToList().Where(c => c.ITupdateDate > firstOfThisYear && c.ITupdateDate <= firstOfNextMYear).Sum(obj => (long)obj.ITquantity);
                        cash.Add(drawCash);
                        MyAxis.Labels.Add(year.ToString());
                    }
                }
                rowChartData.Add(
              new LineSeries
              {
                  Values = cash.AsChartValues(),
                  Title = MainWindow.resourcemanager.GetString("trQuantity")
              });

                DataContext = this;
                rowChart.Series = rowChartData;
            }
            catch { }
        }


        #endregion

        #region reports
        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {//settings

        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //Thread t1 = new Thread(() =>
                //{
                ExcelSalesItems();
                //});
                //t1.Start();

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
        private void ExcelSalesItems()
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
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    printSalesItems();
                });
                t1.Start();
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
        private void printSalesItems()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    pdfSelesItems();
                });
                t1.Start();
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
        private void pdfSelesItems()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
            });
        }
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string itemtype = "";

            string firstTitle = "saleItems";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string selecteditems = "";
            string trSelecteditems = "";
            string invchk = "";
            string retchk = "";

            string startDate = "";
            string endDate = "";
            string searchval = "";

            string branchval = "";
            string invtype = "";
            string itemType = "";

            List<string> invTypelist = new List<string>();

            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {

                if (selectedTab == 0)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        if (chk_allTypes.IsChecked == true || cb_Types.SelectedValue == null)
                        {
                            itemtype = "";
                        }
                        else
                        {
                            itemtype = clsReports.itemTypeConverter(cb_Types.SelectedValue.ToString());
                        }

                        secondTitle = "items";
                        subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);

                        Title = MainWindow.resourcemanagerreport.GetString("trSalesReport");
                        paramarr.Add(new ReportParameter("trTitle", Title));

                    });



                    trSelecteditems = "trItems";
                    selecteditems = clsReports.stackToString(stk_tagsItems);
                    // cb_Types.
                    addpath = @"\Reports\StatisticReport\Sale\Item\Ar\ArItem.rdlc";

                    startDate = dp_ItemStartDate.SelectedDate != null ? SectionData.DateToString(dp_ItemStartDate.SelectedDate) : "";

                    endDate = dp_ItemEndDate.SelectedDate != null ? SectionData.DateToString(dp_ItemEndDate.SelectedDate) : "";
                    branchval = cb_ItemsBranches.SelectedItem != null
               && (chk_allBranchesItem.IsChecked == false || chk_allBranchesItem.IsChecked == null)
               ? cb_ItemsBranches.Text : (chk_allBranchesItem.IsChecked == true ? all : "");

                    itemType = cb_Types.SelectedItem != null
                    && (chk_allTypes.IsChecked == false || chk_allTypes.IsChecked == null)
                    ? clsReports.itemTypeConverter(cb_Types.SelectedValue.ToString()) : (chk_allTypes.IsChecked == true ? all : "");

                    invchk = chk_itemInvoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
                    retchk = chk_itemReturn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";

                    //  paramarr.Add(new ReportParameter("trCode", MainWindow.resourcemanagerreport.GetString("trCode")));
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Sale\Item\Ar\ArBestSel.rdlc";
                    secondTitle = "BestSeller";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    Title = MainWindow.resourcemanagerreport.GetString("trSalesReport") + " / " + subTitle;
                    paramarr.Add(new ReportParameter("trTitle", Title));

                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);

                    startDate = dp_collectStartDate.SelectedDate != null ? SectionData.DateToString(dp_collectStartDate.SelectedDate) : "";

                    endDate = dp_collectEndDate.SelectedDate != null ? SectionData.DateToString(dp_collectEndDate.SelectedDate) : "";

                }
            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Sale\Item\En\EnItem.rdlc";
                    this.Dispatcher.Invoke(() =>
                    {
                        if (chk_allTypes.IsChecked == true || cb_Types.SelectedValue == null)
                        {
                            itemtype = "";
                        }
                        else
                        {
                            itemtype = clsReports.itemTypeConverter(cb_Types.SelectedValue.ToString());
                        }
                        secondTitle = "items";
                        subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                        Title = MainWindow.resourcemanagerreport.GetString("trSalesReport");
                        paramarr.Add(new ReportParameter("trTitle", Title));
                    });
                    trSelecteditems = "trItems";
                    selecteditems = clsReports.stackToString(stk_tagsItems);
                    startDate = dp_ItemStartDate.SelectedDate != null ? SectionData.DateToString(dp_ItemStartDate.SelectedDate) : "";

                    endDate = dp_ItemEndDate.SelectedDate != null ? SectionData.DateToString(dp_ItemEndDate.SelectedDate) : "";
                    branchval = cb_ItemsBranches.SelectedItem != null
               && (chk_allBranchesItem.IsChecked == false || chk_allBranchesItem.IsChecked == null)
               ? cb_ItemsBranches.Text : (chk_allBranchesItem.IsChecked == true ? all : "");

                    itemType = cb_Types.SelectedItem != null
                    && (chk_allTypes.IsChecked == false || chk_allTypes.IsChecked == null)
                    ? clsReports.itemTypeConverter(cb_Types.SelectedValue.ToString()) : (chk_allTypes.IsChecked == true ? all : "");

                    invchk = chk_itemInvoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
                    retchk = chk_itemReturn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";


                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Sale\Item\En\EnBestSel.rdlc";

                    secondTitle = "BestSeller";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    Title = MainWindow.resourcemanagerreport.GetString("trSalesReport") + " / " + subTitle;
                    paramarr.Add(new ReportParameter("trTitle", Title));
                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);

                    startDate = dp_collectStartDate.SelectedDate != null ? SectionData.DateToString(dp_collectStartDate.SelectedDate) : "";

                    endDate = dp_collectEndDate.SelectedDate != null ? SectionData.DateToString(dp_collectEndDate.SelectedDate) : "";

                }
            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            //filter
            invTypelist.Add(invchk);
            invTypelist.Add(retchk);
            int i = 0;
            foreach (string r in invTypelist)
            {

                if (r != null && r != "")
                {
                    if (i == 0)
                    {
                        invtype = r;
                    }
                    else
                    {
                        invtype = invtype + " , " + r;
                    }
                    i++;
                }

            }
            paramarr.Add(new ReportParameter("invtype", invtype));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("itemtype", itemType));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("branchval", branchval));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            /*
             *trNo
trDate
trItem
trUnit
trQTR
trInvoices
trPrice
tr_Invoice

             * */
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trInvoices", MainWindow.resourcemanagerreport.GetString("trInvoices")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("tr_Invoice", MainWindow.resourcemanagerreport.GetString("tr_Invoice")));
            //cb_Types cb_Types.SelectedValue.ToString() itemtypeconverter //chk_allTypes
            clsReports.saleitemStsReport(temp, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            paramarr.Add(new ReportParameter("itemtype", itemtype));

            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            paramarr.Add(new ReportParameter("trSelecteditems", MainWindow.resourcemanagerreport.GetString(trSelecteditems)));
            paramarr.Add(new ReportParameter("selecteditems", selecteditems));
            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                Window.GetWindow(this).Opacity = 0.2;
                /////////////////////
                string pdfpath = "";
                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                BuildReport();
                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                ///////////////////
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
