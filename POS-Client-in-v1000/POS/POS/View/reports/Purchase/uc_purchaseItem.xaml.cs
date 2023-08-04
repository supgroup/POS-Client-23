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
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Threading;
using POS.View.purchases;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_purchaseItem.xaml
    /// </summary>
    public partial class uc_purchaseItem : UserControl
    {
        #region variables
        //prin & pdf
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
        // report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        IEnumerable<ItemTransferInvoice> RepQuery;

        ObservableCollection<int> selectedBranchId = new ObservableCollection<int>();

        ObservableCollection<int> selectedItemId = new ObservableCollection<int>();
        #endregion

        private static uc_purchaseItem _instance;
        public static uc_purchaseItem Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_purchaseItem();
                return _instance;
            }
        }
        public uc_purchaseItem()
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

                tb_totalCurrency.Text = AppSettings.Currency;

                Items = await statisticModel.GetPuritem((int)MainWindow.branchID, (int)MainWindow.userID);

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
                cb_Items.IsTextSearchEnabled = false;
                cb_Items.IsEditable = true;
                cb_Items.StaysOpenOnEdit = true;
                cb_Items.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_Items.Text = "";

                cb_ItemsBranches.IsTextSearchEnabled = false;
                cb_ItemsBranches.IsEditable = true;
                cb_ItemsBranches.StaysOpenOnEdit = true;
                cb_ItemsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_ItemsBranches.Text = "";

                cb_collect.IsTextSearchEnabled = false;
                cb_collect.IsEditable = true;
                cb_collect.StaysOpenOnEdit = true;
                cb_collect.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_collect.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                //comboBranches = await branchModel.GetAllWithoutMain("b");

                if (FillCombo.branchesAllWithoutMainReport is null)
                    await FillCombo.RefreshBranchsWithoutMainReport();

                comboBranches = FillCombo.branchesAllWithoutMainReport;

                itemUnitCombos = statisticModel.GetIUComboList(Items);

                dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
                dynamicComboItem = new ObservableCollection<ItemUnitCombo>(itemUnitCombos);

                fillComboBranches(cb_collect);
                fillComboItemsBranches(cb_ItemsBranches);
                fillComboItems();

                chk_itemInvoice.IsChecked = true;
                resetItemTab();
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
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_collect, MainWindow.resourcemanager.GetString("trBranchHint"));

            chk_allcollect.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allBranchesItem.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItems.Content = MainWindow.resourcemanager.GetString("trAll");


            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_ItemEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_ItemStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_collectEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_collectStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_number.Header = MainWindow.resourcemanager.GetString("trNo");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_itQuantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_invCount.Header = MainWindow.resourcemanager.GetString("trInvoices");
            col_price.Header = MainWindow.resourcemanager.GetString("trPrice");
            col_total.Header = MainWindow.resourcemanager.GetString("trTotal");
            col_avg.Header = MainWindow.resourcemanager.GetString("trItem")+"/"+ MainWindow.resourcemanager.GetString("tr_Invoice");

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
        private void fillComboItems()
        {
            cb_Items.SelectedValuePath = "itemUnitId";
            cb_Items.DisplayMemberPath = "itemUnitName";
            cb_Items.ItemsSource = dynamicComboItem;
        }
        public void fillItemsEvent()
        {
            fillComboItems();
            RepQuery = fillList(Items, cb_ItemsBranches, cb_Items, chk_itemInvoice, chk_itemReturn, dp_ItemStartDate, dp_ItemEndDate)
                .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));

            fillPieChart(cb_Items, selectedItemId);
            fillColumnChart(cb_Items, selectedItemId);
            fillRowChart(cb_Items, selectedItemId);

            dgInvoice.ItemsSource = RepQuery;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            decimal total = 0;
            total = RepQuery.Select(b => b.subTotal.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);
        }
        public void fillEmptyItemsEvent()
        {
            RepQuery = new List<ItemTransferInvoice>();
          
            //RepQuery = fillList(Items, cb_ItemsBranches, cb_Items, chk_itemInvoice, chk_itemReturn, dp_ItemStartDate, dp_ItemEndDate)
            //    .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));
            invLst= new List<ItemTransferInvoice>();
            fillPieChart(cb_Items, selectedItemId);
            fillColumnChart(cb_Items, selectedItemId);
            fillRowChart(cb_Items, selectedItemId);

            dgInvoice.ItemsSource = RepQuery;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            decimal total = 0;
            total = RepQuery.Select(b => b.subTotal.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

        }
        public void fillCollectEvent()
        {
            RepQuery = fillCollectListBranch(Items, dp_collectStartDate, dp_collectEndDate)
                .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));

            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillPieChartCollect(cb_collect, selectedBranchId);
            fillColumnChartCollect(cb_collect, selectedBranchId);
            fillRowChartCollect(cb_collect, selectedBranchId);
            dgInvoice.ItemsSource = RepQuery;
            txt_count.Text = dgInvoice.Items.Count.ToString();
           

        }
        public void fillCollectEventAll()
        {
            RepQuery = fillCollectListAll(Items, dp_collectStartDate, dp_collectEndDate);

            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillPieChartCollect(cb_collect, selectedBranchId);
            fillColumnChartCollect(cb_collect, selectedBranchId);
            fillRowChartCollect(cb_collect, selectedBranchId);
            dgInvoice.ItemsSource = RepQuery;
            txt_count.Text = dgInvoice.Items.Count.ToString();
          
        }
        public void fillEmptyCollectEvent()
        {

            RepQuery =new List<ItemTransferInvoice>();
            invLst = new List<ItemTransferInvoice>();
            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillPieChartCollect(cb_collect, selectedBranchId);
            fillColumnChartCollect(cb_collect, selectedBranchId);
            fillRowChartCollect(cb_collect, selectedBranchId);
            dgInvoice.ItemsSource = RepQuery;
            txt_count.Text = dgInvoice.Items.Count.ToString();
          
        }
        private void resetItemTab()
        {
            Chk_allBranchesItem_Checked(chk_allBranchesItem, null);

            chk_itemInvoice.IsChecked = true;
            chk_itemReturn.IsChecked = false;
            dp_ItemStartDate.SelectedDate = null;
            dp_ItemEndDate.SelectedDate = null;
        }
        private void resetCollectTab()
        {
            Chk_allcollect_Checked(chk_allcollect, null);
            //Chk_allTypes_Checked(chk_allTypes, null);
            //Chk_allItems_Checked(chk_allItems, null);
            chk_allcollect.IsChecked = true;

            dp_collectStartDate.SelectedDate = null;
            dp_collectEndDate.SelectedDate = null;
        }
        public void notCheckAll()
        {
            fillEmptyItemsEvent();
            dynamicComboItem = new ObservableCollection<ItemUnitCombo>(itemUnitCombos);

            fillComboItems();
        }
        public void notCheckAllCollect()
        {
            //  collectListAll = new List<ItemTransferInvoice>();
            fillEmptyCollectEvent();
            dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
            fillComboBranches(cb_collect);
        }
        private void allItemsUnSet()
        {
            // allItemsChk = false;
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

            //  allItemsChk = true;
            chk_allItems.IsChecked = true;
            cb_Items.SelectedItem = null;
            cb_Items.IsEnabled = false;
            chk_allItems.IsEnabled = true;
            for (int i = 0; i < comboItemTemp.Count; i++)
            {
                //dynamicComboItem.Add(comboItemTemp.Skip(i).FirstOrDefault());
                ItemUnitCombo iutemp = comboItemTemp.Skip(i).FirstOrDefault();
                int count = dynamicComboItem.Where(x => x.itemUnitId == iutemp.itemUnitId).ToList().Count();
                if (count == 0)
                    dynamicComboItem.Add(iutemp);
            }
            comboItemTemp.Clear();
            stk_tagsItems.Children.Clear();
            selectedItemId.Clear();



        }
        #endregion

        #region tabs
        private void btn_items_Click(object sender, RoutedEventArgs e)
        {//items
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                selectedTab = 0;
                txt_search.Text = "";

                ReportsHelp.showSelectedStack(grid_stacks, stk_tagsItems);

                path_collect.Fill = Brushes.White;

                bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_items);
                path_items.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                ReportsHelp.showTabControlGrid(grid_father, grid_items);

                ReportsHelp.isEnabledButtons(grid_tabControl, btn_items);

                ReportsHelp.hideAllColumns(dgInvoice);
                 
                col_number.Visibility = Visibility.Visible;
                col_date.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_itQuantity.Visibility = Visibility.Visible;
                col_price.Visibility = Visibility.Visible;
                col_total.Visibility = Visibility.Visible;
                bdr_total.Visibility = Visibility.Visible;
                // fillItemsEvent();
                resetItemTab();
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

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

                selectedTab = 1;
                txt_search.Text = "";

                path_items.Fill = Brushes.White;
                path_collect.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

                ReportsHelp.showSelectedStack(grid_stacks, stk_tagsBranches);
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_collect);
                ReportsHelp.showTabControlGrid(grid_father, grid_collect);
                ReportsHelp.isEnabledButtons(grid_tabControl, btn_collect);
                ReportsHelp.hideAllColumns(dgInvoice);

                col_branch.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_itQuantity.Visibility = Visibility.Visible;
                col_invCount.Visibility = Visibility.Visible;
                col_avg.Visibility = Visibility.Visible;
                bdr_total.Visibility = Visibility.Collapsed;
                resetCollectTab();
                //if (stk_tagsBranches.Children.Count == 0)
                //{
                //    fillCollectEventAll();
                //}
                //else
                //{
                //    fillCollectEvent();
                //}

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

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

        #region Events
        private void Chip_OnDeleteItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
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
                else
                {
                    fillItemsEvent();
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
        private void cb_Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
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
                        fillItemsEvent();
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
        private void selectionChangedCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_ItemsBranches.SelectedItem != null)
                {
                    // chk_allTypes.IsEnabled = true;
                    
                    allItemsSet();
                }
                else
                {
                
                    allItemsUnSet();
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
        private void Chk_allBranchesItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                chk_allBranchesItem.IsChecked = true;
                cb_ItemsBranches.SelectedItem = null;
                cb_ItemsBranches.IsEnabled = false;
                cb_ItemsBranches.Text = "";
                cb_ItemsBranches.ItemsSource = comboBranches;
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
               
                chk_allBranchesItem.IsChecked = false;
                cb_ItemsBranches.IsEnabled = true;

              
                //
                cb_Items.IsEnabled = false;
                chk_allItems.IsEnabled = false;
      
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
                chk_allItems.IsChecked = true;
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
                  //  dynamicComboItem.Add(comboItemTemp.Skip(i).FirstOrDefault());
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
                chk_allItems.IsChecked = false;
                cb_Items.IsEnabled = true;
                //  fillItemsEvent();
                notCheckAll();
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
        private void col_selectionChangedCall(object sender)
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
                chk_allcollect.IsChecked = true;
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
                //fillCollectEvent();
                //if (stk_tagsBranches.Children.Count == 0)
                //{
                //    fillCollectEventAll();
                //}
                notCheckAllCollect();
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
        IEnumerable<ItemTransferInvoice> itemTransfers = null;
        IEnumerable<ItemTransferInvoice> query = null;
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    RepQuery = RepQuery.Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.itemId) : true));

                    RepQuery = RepQuery.Where(s => (s.invNumber.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                s.ITunitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                s.ITprice.ToString().ToLower().Contains(txt_search.Text.ToLower())));
                    decimal total = 0;
                    total = RepQuery.Select(b => b.subTotal.Value).Sum();
                    tb_total.Text = SectionData.DecTostring(total);
                    fillPieChart(cb_Items, selectedItemId);
                    fillColumnChart(cb_Items, selectedItemId);
                    fillRowChart(cb_Items, selectedItemId);


                }
                else if (selectedTab == 1)
                {
                    RepQuery = RepQuery.Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));

                    RepQuery = RepQuery.Where(s => (s.ITitemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                      s.ITunitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                                      s.ITquantity.ToString().ToLower().Contains(txt_search.Text.ToLower())));

                    fillPieChartCollect(cb_collect, selectedBranchId);
                    fillColumnChartCollect(cb_collect, selectedBranchId);
                    fillRowChartCollect(cb_collect, selectedBranchId);
                }

                dgInvoice.ItemsSource = RepQuery;

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
                Items = await statisticModel.GetPuritem((int)MainWindow.branchID, (int)MainWindow.userID);

                if (selectedTab == 0)
                {
                    chk_allBranchesItem.IsChecked = true;
                    chk_allItems.IsChecked = true;
                    chk_itemInvoice.IsChecked = true;
                    chk_itemDrafs.IsChecked = true;
                    chk_itemReturn.IsChecked = true;
                    dp_ItemStartDate.SelectedDate = null;
                    dp_ItemEndDate.SelectedDate = null;

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
                }

                else if (selectedTab == 1)
                {
                    chk_allcollect.IsChecked = true;
                    dp_collectStartDate.SelectedDate = null;
                    dp_collectEndDate.SelectedDate = null;
                    for (int i = 0; i < comboBrachTemp.Count; i++)
                        dynamicComboBranches.Add(comboBrachTemp.Skip(i).FirstOrDefault());

                    comboBrachTemp.Clear();
                    stk_tagsBranches.Children.Clear();
                    selectedBranchId.Clear();
                }
                resetItemTab();
                txt_search.Text = "";

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
        private void dt_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectionChangedCall(sender);
        }
        private void cb_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectionChangedCall(sender);
        }
        private void chk_selectionChanged(object sender, RoutedEventArgs e)
        {
            selectionChangedCall(sender);
        }
        private void dt_colselectionChanged(object sender, SelectionChangedEventArgs e)
        {
            col_selectionChangedCall(sender);
        }
        private void t_selectionChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            selectionChangedCall(sender);
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
                        MainWindow.mainWindow.BTN_purchases_Click(MainWindow.mainWindow.btn_purchase, null);
                        uc_purchases.Instance.UserControl_Loaded(null, null);
                        uc_purchases.Instance.btn_payInvoice_Click(uc_purchases.Instance.btn_payInvoice, null);
                        uc_payInvoice.Instance.UserControl_Loaded(null, null);
                        uc_payInvoice._InvoiceType = invoice.invType;
                        uc_payInvoice.Instance.invoice = invoice;
                        uc_payInvoice.isFromReport = true;

                        if (item.archived == 0)
                            uc_payInvoice.archived = false;
                        else
                            uc_payInvoice.archived = true;

                        await uc_payInvoice.Instance.fillInvoiceInputs(invoice);
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
        #endregion

        #region fillLists
        List<ItemTransferInvoice> invLst;
        private List<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> Invoices, ComboBox cbBranch, ComboBox cbitem, CheckBox chkInvoice, CheckBox chkReturn, DatePicker startDate, DatePicker endDate)
        {
            var selectedBranch = cbBranch.SelectedItem as Branch;
            var selectedItemUnit = cbitem.SelectedItem as ItemUnitCombo;

            var result = Invoices.Where(x => (

           ((chkReturn.IsChecked == true ? (x.invType == "pb") : false) || (chkInvoice.IsChecked == true ? (x.invType == "p") : false))
                      && (cbBranch.SelectedItem != null ? x.branchCreatorId == selectedBranch.branchId : true)
                      && (cb_Items.SelectedItem != null ? x.itemUnitId == selectedItemUnit.itemUnitId : true)
                      && (startDate.SelectedDate != null ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                      && (endDate.SelectedDate != null ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)));

            invLst = result.ToList();
            return result.ToList();
        }

        private List<ItemTransferInvoice> fillCollectListBranch(IEnumerable<ItemTransferInvoice> Invoices, DatePicker startDate, DatePicker endDate)
        {

            var temp = Invoices
                .Where(x =>
                 (startDate.SelectedDate != null ? ((DateTime)x.updateDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                && (endDate.SelectedDate != null ? ((DateTime)x.updateDate).Date <= ((DateTime)endDate.SelectedDate).Date: true))
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

            invLst = temp.ToList();
            return temp.ToList();
        }

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

            invLst = temp.ToList();
            return temp.ToList();
        }
        #endregion

        #region reports
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string firstTitle = "purchaseItem";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string selecteditems = "";
            string trSelecteditems = "";
            string addpath = "";

            string invchk = "";
            string retchk = "";
             
            string startDate = "";
            string endDate = "";          
            string searchval = "";
          
            string branchval = "";
            string invtype = "";
            List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Item\Ar\ArItem.rdlc";
                    secondTitle = "items";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trItems";
                    selecteditems = clsReports.stackToString(stk_tagsItems);

                    startDate = dp_ItemStartDate.SelectedDate != null ? SectionData.DateToString(dp_ItemStartDate.SelectedDate) : "";

                    endDate = dp_ItemEndDate.SelectedDate != null ? SectionData.DateToString(dp_ItemEndDate.SelectedDate) : "";
                    branchval =  cb_ItemsBranches.SelectedItem != null
               && (chk_allBranchesItem.IsChecked == false || chk_allBranchesItem.IsChecked == null)
                
               ? cb_ItemsBranches.Text : (chk_allBranchesItem.IsChecked == true  ? all : "");
                    invchk = chk_itemInvoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
                    retchk = chk_itemReturn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";

                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Item\Ar\ArMostPur.rdlc";
                    secondTitle = "MostPurchased";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);

                    startDate = dp_collectStartDate.SelectedDate != null ? SectionData.DateToString(dp_collectStartDate.SelectedDate) : "";

                    endDate = dp_collectEndDate.SelectedDate != null ? SectionData.DateToString(dp_collectEndDate.SelectedDate) : "";

                }
            }
            else
            {
                //english
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Item\En\EnItem.rdlc";
                    secondTitle = "items";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trItems";
                    selecteditems = clsReports.stackToString(stk_tagsItems);

                    startDate = dp_ItemStartDate.SelectedDate != null ? SectionData.DateToString(dp_ItemStartDate.SelectedDate) : "";

                    endDate = dp_ItemEndDate.SelectedDate != null ? SectionData.DateToString(dp_ItemEndDate.SelectedDate) : "";
                    branchval = cb_ItemsBranches.SelectedItem != null
               && (chk_allBranchesItem.IsChecked == false || chk_allBranchesItem.IsChecked == null)

               ? cb_ItemsBranches.Text : (chk_allBranchesItem.IsChecked == true ? all : "");
                    invchk = chk_itemInvoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
                    retchk = chk_itemReturn.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Item\En\EnMostPur.rdlc";
                    secondTitle = "MostPurchased";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
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
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trInvoices", MainWindow.resourcemanagerreport.GetString("trInvoices")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("tr_Invoice", MainWindow.resourcemanagerreport.GetString("tr_Invoice")));

            //  getpuritemcount
            Title = MainWindow.resourcemanagerreport.GetString("trPurchasesReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("dateForm", AppSettings.dateFormat));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            paramarr.Add(new ReportParameter("trSelecteditems", MainWindow.resourcemanagerreport.GetString(trSelecteditems)));
            paramarr.Add(new ReportParameter("selecteditems", selecteditems));
            paramarr.Add(new ReportParameter("Currency", AppSettings.Currency));
            paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
            clsReports.PurStsReport(RepQuery, rep, reppath);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();

        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
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

        #region collect charts
        private void fillPieChartCollect(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            List<string> titles = new List<string>();
            List<long> x = new List<long>();
            titles.Clear();
            //var temp = invLst;
            var temp = RepQuery;
            if (stk_tagsBranches.Children.Count > 0)
            {
                temp = temp
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
        private void fillColumnChartCollect(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> x = new List<int>();

            //var temp = invLst;
            var temp = RepQuery;
            if (stk_tagsBranches.Children.Count > 0)
            {
                temp = temp
                .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true)).ToList();
            }

            x = temp.Select(m => m.count).ToList();

            var tempName = temp.OrderByDescending(obj => obj.count).Select(obj => obj.ITitemUnitName1);
            names.AddRange(tempName);

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();

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
        private void fillRowChartCollect(ComboBox comboBox, ObservableCollection<int> stackedButton)
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

            //var temp = invLst;
            var temp = RepQuery;

            if (stk_tagsBranches.Children.Count > 0)
            {
                temp = temp
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
          }); ;

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion

        #region Charts
        private void fillPieChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            List<string> titles = new List<string>();
            List<int> x = new List<int>();
            titles.Clear();

            //var temp = invLst
            var temp = RepQuery
             .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));
            var titleTemp = temp.GroupBy(jj => jj.ITitemUnitId)
             .Select(g => new ItemUnitCombo
             {
                 itemUnitId = (int)g.FirstOrDefault().ITitemUnitId,
                 itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName
             }).ToList();

            titles.AddRange(titleTemp.Select(jj => jj.itemUnitName));
            var result = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
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
        private void fillColumnChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> x = new List<int>();
            IEnumerable<int> y = null;

            //var temp = invLst
            var temp = RepQuery
                   .Where(j => (selectedItemId.Count != 0 ? selectedItemId.Contains((int)j.ITitemUnitId) : true));
            var result = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
            {
                ITitemUnitId = s.Key,
                countP = s.Where(m => m.invType == "p").Count(),
                countPb = s.Where(m => m.invType == "pb").Count(),

            });
            x = result.Select(m => m.countP).ToList();
            y = result.Select(m => m.countPb);
            var tempName = temp.GroupBy(jj => jj.ITitemUnitId)
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
                MainWindow.resourcemanager.GetString("trDraft")
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
        private void fillRowChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;
            IEnumerable<decimal> pbTemp = null;
            IEnumerable<decimal> resultTemp = null;

            // var temp = invLst.Where(j => (selectedItemId.Count != 0 ? stackedButton.Contains((int)j.ITitemUnitId) : true));
            var temp = RepQuery.Where(j => (selectedItemId.Count != 0 ? stackedButton.Contains((int)j.ITitemUnitId) : true));
            var result = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
            {
                ITitemUnitId = s.Key,
                totalP = s.Where(x => x.invType == "p").Sum(x => x.totalNet),
                totalPb = s.Where(x => x.invType == "pb").Sum(x => x.totalNet),

            }
         );
            var resultTotal = result.Select(x => new { x.ITitemUnitId, total = x.totalP - x.totalPb }).ToList();
            pTemp = result.Select(x => (decimal)x.totalP);
            pbTemp = result.Select(x => (decimal)x.totalPb);
            resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            var tempName = temp.GroupBy(jj => jj.ITitemUnitId)
             .Select(g => new ItemUnitCombo { itemUnitId = (int)g.FirstOrDefault().ITitemUnitId, itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName }).ToList();
            names.AddRange(tempName.Select(nn => nn.itemUnitName));

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

            int xCount = 5;
            if (pTemp.Count() <= 5) xCount = pTemp.Count();
            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if(pTemp.Count() > 5)
            {
                decimal purSum = 0, retSum = 0, subSum = 0;
                for (int i = 5; i < pTemp.Count(); i++)
                {
                    purSum = purSum + pTemp.ToList().Skip(i).FirstOrDefault();
                    retSum = retSum + pbTemp.ToList().Skip(i).FirstOrDefault();
                    subSum = subSum + resultTemp.ToList().Skip(i).FirstOrDefault();
                }
                if(!((purSum == 0) && (retSum == 0) && (subSum == 0)))
                {
                    purchase.Add(purSum);
                    returns.Add(retSum);
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

        private void Cb_ItemsBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = comboBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_Items_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = dynamicComboItem.Where(p => p.itemUnitName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_collect_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = dynamicComboBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
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


