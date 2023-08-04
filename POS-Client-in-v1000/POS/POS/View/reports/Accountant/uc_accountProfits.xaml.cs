using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Threading;
using POS.View.windows;
using System.Resources;
using System.Reflection;
using static POS.Classes.Statistics;
using System.Globalization;
using Newtonsoft.Json;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_accountProfits.xaml
    /// </summary>
    public partial class uc_accountProfits : UserControl
    {
        #region variables
        IEnumerable<ItemUnitInvoiceProfit> profits;
        IEnumerable<ItemUnitInvoiceProfit> profitsNetProfits;
        IEnumerable<ItemUnitInvoiceProfit> profitsInvoices;
        IEnumerable<ItemUnitInvoiceProfit> profitsItems;
        Statistics statisticsModel = new Statistics();
        IEnumerable<ItemUnitInvoiceProfit> profitsQuery;
        string searchText = "";
        int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion

        private static uc_accountProfits _instance;

        public static uc_accountProfits Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_accountProfits();
                return _instance;
            }
        }
        public uc_accountProfits()
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

                tb_totalCurrency.Text = AppSettings.Currency;

                profitsNetProfits = await statisticsModel.GetProfitNet(MainWindow.branchID.Value, MainWindow.userID.Value,
                                                                       SectionData.DateTodbString(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)),
                                                                       SectionData.DateTodbString(DateTime.Now));
                profitsInvoices = await statisticsModel.GetInvoiceProfit(MainWindow.branchID.Value, MainWindow.userID.Value);
                profitsItems = await statisticsModel.GetItemProfit(MainWindow.branchID.Value, MainWindow.userID.Value);

                Btn_netProfit_Click(btn_netProfit, null);

                #region key up
                // key_up branch
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                // key_up section
                cb_pos.IsTextSearchEnabled = false;
                cb_pos.IsEditable = true;
                cb_pos.StaysOpenOnEdit = true;
                cb_pos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_pos.Text = "";
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

        #region methods
        private void translate()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allPos.Content = MainWindow.resourcemanager.GetString("trAll");

            tt_invoice.Content = MainWindow.resourcemanager.GetString("trInvoices");
            tt_item.Content = MainWindow.resourcemanager.GetString("trItems");

            col_num.Header = MainWindow.resourcemanager.GetString("trNo");
            col_invType.Header = MainWindow.resourcemanager.GetString("trType");
            col_invDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_invTotal.Header = MainWindow.resourcemanager.GetString("trTotal");
            col_itemName.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unitName.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_quantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_pos.Header = MainWindow.resourcemanager.GetString("trPOS");
            col_invoiceProfit.Header = MainWindow.resourcemanager.GetString("trProfits");
            col_netProfit.Header = MainWindow.resourcemanager.GetString("trAmount");
            col_itemProfit.Header = MainWindow.resourcemanager.GetString("trProfits");
            col_description.Header = MainWindow.resourcemanager.GetString("trDescription");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
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
        async Task<IEnumerable<ItemUnitInvoiceProfit>> RefreshItemUnitInvoiceProfit()
        {
            if (selectedTab == 0)
                profits = profitsNetProfits;
            else if (selectedTab == 1)
                profits = profitsInvoices;
            else if (selectedTab == 2)
                profits = profitsItems.ToList();
            return profits;
        }
        IEnumerable<ItemUnitInvoiceProfit> profitsTemp = new List<ItemUnitInvoiceProfit>();
        async Task Search()
        {
            try
            {
                if (isTabChanged)
                    await RefreshItemUnitInvoiceProfit();

                isTabChanged = false;
                searchText = txt_search.Text.ToLower();

                profitsTemp = JsonConvert.DeserializeObject<List<ItemUnitInvoiceProfit>>(JsonConvert.SerializeObject(

                    profits
                        .Where(p =>
                    //start date
                    (dp_startDate.SelectedDate != null ? p.updateDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
                    &&
                    //end date
                    (dp_endDate.SelectedDate != null ? p.updateDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
                    ).ToList()
                    ));

                //profitsTemp = profits
                //    .Where(p =>
                ////start date
                //(dp_startDate.SelectedDate != null ? p.updateDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
                //&&
                ////end date
                //(dp_endDate.SelectedDate != null ? p.updateDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
                //).ToList()
                //;

                if (selectedTab == 0)
                    await SearchProfit();
                else if (selectedTab == 1)
                    await SearchInvoice();
                else if (selectedTab == 2)
                    await SearchItem();

                RefreshProfitsView();
                fillColumnChart();
                fillPieChart();
                fillRowChart();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void SearchEmpty()
        {
            try
            {
                //if (profits is null)
               
                searchText = txt_search.Text.ToLower();

                profitsTemp = new List<ItemUnitInvoiceProfit>();
                profitsQuery = profitsTemp;
             
                RefreshProfitsView();
                //fillCombo1();
                fillColumnChart();
                fillPieChart();
                fillRowChart();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        async Task SearchProfit()
        {
            profitsQuery = profitsTemp
            .Where(s =>
            (
            s.invNumber != null ? s.invNumber.ToLower().Contains(searchText) : s.transNum.ToLower().Contains(searchText)
            )
            &&
            //branchID/itemID
            (
            chk_allBranches.IsChecked.Value ?
                true
                :
                cb_branches.SelectedIndex != -1 ? s.branchCreatorId == Convert.ToInt32(cb_branches.SelectedValue) : false
            )
            );

        }
        async Task SearchInvoice()
        {
            profitsQuery = profitsTemp
            .Where(s =>
            (
            s.invNumber.ToLower().Contains(searchText)
            ||
            s.totalNet.ToString().ToLower().Contains(searchText)
            )
            &&
            //branchID/itemID
            (
                chk_allBranches.IsChecked.Value ?
                true
                :
                cb_branches.SelectedIndex != -1 ? s.branchCreatorId == Convert.ToInt32(cb_branches.SelectedValue) : false
            )
            &&
            //posID/unitID
            (
                cb_pos.SelectedIndex != -1 ? s.posId == Convert.ToInt32(cb_pos.SelectedValue) : true
            ))
            ;

        }
        async Task SearchItem()
        {
            var quantities = profitsTemp.GroupBy(s => s.ITitemUnitId).Select(inv => new
            {
                ITquantity = inv.Sum(p => p.ITquantity.Value),
                itemunitProfit = inv.Sum(p => p.itemunitProfit),
                ITitemUnitId = inv.FirstOrDefault().ITitemUnitId,
                

            }).ToList();

            profitsTemp = profitsTemp.GroupBy(s => s.ITitemUnitId).SelectMany(inv => inv.Take(1)).ToList();

            profitsQuery = profitsTemp
            .Where(s =>
            (
            s.invNumber.ToLower().Contains(searchText)
            ||
            s.ITitemName.ToLower().Contains(searchText)
            ||
            s.ITunitName.ToLower().Contains(searchText)
            )
            &&
            //branchID/itemID
            (
               chk_allBranches.IsChecked.Value ?
               true
               : cb_branches.SelectedIndex != -1 ? s.ITitemId == Convert.ToInt32(cb_branches.SelectedValue) : false
            )
            &&
            //posID/unitID
            (
                cb_pos.SelectedIndex != -1 ? s.ITunitId == Convert.ToInt32(cb_pos.SelectedValue) : true)
            ).ToList()
            ;

            int i = 0;
            foreach (var x in profitsQuery)
            {
                //if (x.ITitemUnitId == quantities[i].ITitemUnitId)
                //{
                //x.ITquantity =  quantities[i].ITquantity;
                //x.itemunitProfit = quantities[i].itemunitProfit;
                x.ITquantity = quantities.Where(q => q.ITitemUnitId == x.ITitemUnitId).FirstOrDefault().ITquantity;
                x.itemunitProfit = quantities.Where(q => q.ITitemUnitId == x.ITitemUnitId).FirstOrDefault().itemunitProfit;
                //}
                i++;
            }
        }
        void RefreshProfitsView()
        {
            dgFund.ItemsSource = profitsQuery;
            txt_count.Text = profitsQuery.Count().ToString();

            decimal total = 0;
            if (selectedTab == 0 || selectedTab == 1)
                total = profitsQuery.Select(b => b.invoiceProfit).Sum();
            else
                total = profitsQuery.Select(b => b.itemunitProfit).Sum();

            tb_total.Text = SectionData.DecTostring(total);
        }
        List<Branch> branches = new List<Branch>();
        List<Item> items = new List<Item>();
        private void fillCombo1()
        {
            if (selectedTab == 0 || selectedTab == 1)
            {
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));
                cb_branches.SelectedValuePath = "branchId";
                cb_branches.DisplayMemberPath = "name";
                //cb_branches.ItemsSource = profits.GroupBy(g => g.branchCreatorId).Select(i => new { i.FirstOrDefault().branchCreatorName, i.FirstOrDefault().branchCreatorId });
                branches = profits.GroupBy(g => g.branchCreatorId).Select(i => new Branch{ name = i.FirstOrDefault().branchCreatorName, branchId = i.FirstOrDefault().branchCreatorId.Value }).ToList();
                cb_branches.ItemsSource = branches;
            }
            else if (selectedTab == 2)
            {
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trItemHint"));
                cb_branches.SelectedValuePath = "itemId";
                cb_branches.DisplayMemberPath = "name";
                //cb_branches.ItemsSource = profits.GroupBy(g => g.ITitemId).Select(i => new { i.FirstOrDefault().ITitemId, i.FirstOrDefault().ITitemName });
                items = profits.GroupBy(g => g.ITitemId).Select(i => new Item { itemId = i.FirstOrDefault().ITitemId.Value, name = i.FirstOrDefault().ITitemName }).ToList();
                cb_branches.ItemsSource = items;
            }
        }
        List<Pos> poss = new List<Pos>();
        List<ItemUnit> itemunits = new List<ItemUnit>();
        private void fillCombo2(int bID)
        {
            if (selectedTab == 1)
            {
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPosHint"));
                cb_pos.SelectedValuePath = "posId";
                cb_pos.DisplayMemberPath = "name";
                //cb_pos.ItemsSource = profits.Where(b => b.branchCreatorId == bID).GroupBy(g => g.posId).Select(i => new
                //{
                //    i.FirstOrDefault().posId,
                //    i.FirstOrDefault().posName
                //});
                poss = profits.Where(b => b.branchCreatorId == bID).GroupBy(g => g.posId).Select(i => new Pos
                {
                    posId = i.FirstOrDefault().posId.Value,
                    name = i.FirstOrDefault().posName
                }).ToList();
                cb_pos.ItemsSource = poss;
            }
            else if (selectedTab == 2)
            {
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trUnitHint"));
                cb_pos.SelectedValuePath = "itemUnitId";
                cb_pos.DisplayMemberPath = "itemName";
                //cb_pos.ItemsSource = profits.Where(b => b.ITitemId == bID).GroupBy(g => g.ITunitId).Select(i => new
                //{
                //    i.FirstOrDefault().ITunitId,
                //    i.FirstOrDefault().ITunitName
                //});
                itemunits = profits.Where(b => b.ITitemId == bID).GroupBy(g => g.ITunitId).Select(i => new ItemUnit
                {
                    itemUnitId = i.FirstOrDefault().ITunitId.Value,
                    itemName = i.FirstOrDefault().ITunitName
                }).ToList();
                cb_pos.ItemsSource = itemunits;
            }

        }
        private void hideAllColumns()
        {
            col_itemName.Visibility = Visibility.Collapsed;
            col_unitName.Visibility = Visibility.Collapsed;
            col_quantity.Visibility = Visibility.Collapsed;
            col_num.Visibility = Visibility.Collapsed;
            col_invType.Visibility = Visibility.Collapsed;
            col_invDate.Visibility = Visibility.Collapsed;
            col_invTotal.Visibility = Visibility.Collapsed;
            col_branch.Visibility = Visibility.Collapsed;
            col_pos.Visibility = Visibility.Collapsed;
            col_invoiceProfit.Visibility = Visibility.Collapsed;
            col_itemProfit.Visibility = Visibility.Collapsed;
            col_netProfit.Visibility = Visibility.Collapsed;
            col_description.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region tabs
        bool isFirstTime = true;
        bool isTabChanged = false;
        private async void Btn_netProfit_Click(object sender, RoutedEventArgs e)
        {//net profits
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                isTabChanged = true;
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPosHint"));
                txt_total.Text = MainWindow.resourcemanager.GetString("trNetProfit_");
                row_pos.Height = new GridLength(0);
                hideAllColumns();
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDay = date;
                isFirstTime = true;
                dp_startDate.SelectedDate = firstDayOfMonth;
                dp_endDate.SelectedDate = lastDay;
                isFirstTime = false;
                selectedTab = 0;

                col_num.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_description.Visibility = Visibility.Visible;
                col_netProfit.Visibility = Visibility.Visible;

                txt_search.Text = "";

                path_invoice.Fill = Brushes.White;
                path_item.Fill = Brushes.White;
                //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_netProfit);
                path_netProfit.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                chk_allBranches.IsChecked = true;

                await Search();
                fillCombo1();

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
        private async void Btn_invoice_Click(object sender, RoutedEventArgs e)
        {//invoices
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                isTabChanged = true;
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPosHint"));
                txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");
                row_pos.Height = row_branch.Height;
                hideAllColumns();
                selectedTab = 1;
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;

                col_num.Visibility = Visibility.Visible;
                col_invType.Visibility = Visibility.Visible;
                col_invDate.Visibility = Visibility.Visible;
                col_invTotal.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_pos.Visibility = Visibility.Visible;
                col_invoiceProfit.Visibility = Visibility.Visible;

                txt_search.Text = "";

                path_netProfit.Fill = Brushes.White;
                path_item.Fill = Brushes.White;
                //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_invoice);
                path_invoice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                chk_allBranches.IsChecked = true;

                await Search();
                fillCombo1();

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
        private async void Btn_item_Click(object sender, RoutedEventArgs e)
        {//items
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                isTabChanged = true;
                hideAllColumns();
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trItemHint"));
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trUnitHint"));
                row_pos.Height = row_branch.Height;
                txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");
                selectedTab = 2;
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;

                chk_allBranches.IsChecked = true;
                chk_allPos.IsChecked = true;

                col_itemName.Visibility = Visibility.Visible;
                col_unitName.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_itemProfit.Visibility = Visibility.Visible;

                txt_search.Text = "";

                path_netProfit.Fill = Brushes.White;
                path_invoice.Fill = Brushes.White;
                //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_item);
                path_item.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await Search();
                fillCombo1();

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
        private async void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {//select all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.IsEnabled = false;
                cb_branches.Text = "";
                if(selectedTab == 0 || selectedTab == 1)
                    cb_branches.ItemsSource = branches;
                else
                    cb_branches.ItemsSource = items;
                chk_allPos.IsEnabled = false;
                chk_allPos.IsChecked = true;
                cb_pos.SelectedItem = null;
                cb_pos.IsEnabled = false;
                await Search();

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
        private async void Chk_allBranches_Unchecked(object sender, RoutedEventArgs e)
        {//unselect all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.IsEnabled = true;
                chk_allPos.IsEnabled = false;
                chk_allPos.IsChecked = false;
                cb_pos.SelectedItem = null;
                cb_pos.IsEnabled = false;
                SearchEmpty();
                //await Search();

                //if (selectedTab == 0)
                //    await SearchProfit();
                //else if (selectedTab == 1)
                //    await SearchInvoice();
                //else
                //    await SearchItem();

                //RefreshProfitsView();
                //fillCombo1();
                //fillColumnChart();
                //fillPieChart();
                //fillRowChart();

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
        private async void Chk_allPos_Checked(object sender, RoutedEventArgs e)
        {//select all pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_pos.SelectedIndex = -1;
                cb_pos.IsEnabled = false;
                cb_pos.Text = "";
                if (selectedTab == 0 || selectedTab == 1)
                    cb_pos.ItemsSource = poss;
                else
                    cb_pos.ItemsSource = itemunits;
                await Search();

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
        private async void Chk_allPos_Unchecked(object sender, RoutedEventArgs e)
        {//unselect all pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_pos.IsEnabled = true;
                SearchEmpty();
            //    await Search();

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
        private async void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
           
            await Search();
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            //callSearch(sender);
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Clear();
                await RefreshItemUnitInvoiceProfit();
                if (selectedTab == 0)
                {
                    DateTime date = DateTime.Now;
                    var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                    var lastDay = date;
                    dp_startDate.SelectedDate = firstDayOfMonth;
                    dp_endDate.SelectedDate = lastDay;
                }
                else
                {
                    dp_startDate.SelectedDate = null;
                    dp_endDate.SelectedDate = null;
                }
               
                chk_allBranches.IsChecked = true;

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
        private void RefreshView_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {//change selection

            callSearch(sender);

        }
        private async void callSearch(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (!isFirstTime)
                    await Search();

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
        private async void cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_branches.SelectedItem==null) {
                    chk_allPos.IsEnabled = false;
                    SearchEmpty();
                } else
                {
                    chk_allPos.IsEnabled = true;
                    chk_allPos.IsChecked = true;
                    await Search();
                }
                
                fillCombo2(Convert.ToInt32(cb_branches.SelectedValue));

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
        private void Cb_pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select pos
            callSearch(sender);
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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                if (selectedTab == 0 || selectedTab == 1)
                    combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                else
                    combo.ItemsSource = items.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_pos_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                if (selectedTab == 1)
                    combo.ItemsSource = poss.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
                else if (selectedTab == 2)
                    combo.ItemsSource = itemunits.Where(p => p.itemName.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region charts
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<decimal> profit = new List<decimal>();

            var temp = profitsQuery;

            int count = 0;
            //profit
            if (selectedTab == 0)
            {
                var tempName = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchName = s.FirstOrDefault().branchCreatorName
                });
                count = tempName.Count();
                names.AddRange(tempName.Select(nn => nn.branchName));

                var tempProfit = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    profit = s.Sum(p => decimal.Parse(SectionData.DecTostring(p.invoiceProfit)))
                });

                profit.AddRange(tempProfit.Select(nn => nn.profit));
            }
            //invoice
            if (selectedTab == 1)
            {
                var tempName = temp.GroupBy(s => s.posId).Select(s => new
                {
                    posName = s.FirstOrDefault().posName + "/" + s.FirstOrDefault().branchCreatorName
                });
                count = tempName.Count();
                names.AddRange(tempName.Select(nn => nn.posName));

                var tempProfit = temp.GroupBy(s => s.posId).Select(s => new
                {
                    profit = s.Sum(p => decimal.Parse(SectionData.DecTostring(p.invoiceProfit)))
                });

                profit.AddRange(tempProfit.Select(nn => nn.profit));
            }
            //item
            else if (selectedTab == 2)
            {
                var tempName = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
                {
                    name = s.FirstOrDefault().ITitemName + "/" + s.FirstOrDefault().ITunitName
                });
                count = tempName.Count();
                names.AddRange(tempName.Select(nn => nn.name));

                var tempProfit = temp.GroupBy(s => s.ITitemId).Select(s => new
                {
                    profit = s.Sum(p => decimal.Parse(SectionData.DecTostring(p.itemunitProfit)))
                });

                profit.AddRange(tempProfit.Select(nn => nn.profit));
            }
            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();

            List<decimal> cWon = new List<decimal>();
            List<decimal> cLoss = new List<decimal>();

            List<string> titles = new List<string>()
            {
               MainWindow.resourcemanager.GetString("trProfit") ,
               MainWindow.resourcemanager.GetString("trLoss")
            };
            int x = 6;
            if (count <= 6) x = count;
            for (int i = 0; i < x; i++)
            {
                if (profit.ToList().Skip(i).FirstOrDefault() > 0)
                {
                    cWon.Add(profit.ToList().Skip(i).FirstOrDefault());
                    cLoss.Add(0);
                }
                else
                {
                    cWon.Add(0);
                    cLoss.Add(-1 * profit.ToList().Skip(i).FirstOrDefault());
                }
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (count > 6)
            {
                decimal profitSum = 0;
                for (int i = 6; i < count; i++)
                {
                    profitSum = profitSum + profit.ToList().Skip(i).FirstOrDefault();
                }
                if (!((profitSum == 0)))
                {
                    if (profitSum > 0)
                    {
                        cWon.Add(profitSum);
                        cLoss.Add(0);
                    }
                    else
                    {
                        cWon.Add(0);
                        cLoss.Add(-1 * profitSum);
                    }

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
                new StackedColumnSeries
                {
                    Values = cWon.AsChartValues(),
                    Title = titles[0],
                    DataLabels = true,
                });
            columnChartData.Add(
               new StackedColumnSeries
               {
                   Values = cLoss.AsChartValues(),
                   Title = titles[1],
                   DataLabels = true,
               });
            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<string> finalTitles = new List<string>();
            IEnumerable<decimal> x = null;

            var temp = profitsQuery;
            int count = 0;
            if (selectedTab == 0 || selectedTab == 1)
            {
                var titleTemp = temp.GroupBy(m => m.branchCreatorName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));

                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    profit = s.Sum(p => p.invoiceProfit)
                });
                x = result.Select(m => decimal.Parse(SectionData.DecTostring(m.profit)));
                count = x.Count();
            }
            else if (selectedTab == 2)
            {
                var titleTemp = temp.GroupBy(m => m.ITitemId).Select(d => new
                {
                    ITitemId = d.Key,
                    name = d.FirstOrDefault().ITitemName
                }
                );
                titles.AddRange(titleTemp.Select(jj => jj.name));

                var result = temp.GroupBy(s => s.ITitemId).Select(s => new
                {
                    ITitemUnitId = s.Key,
                    profit = s.Sum(p => p.itemunitProfit)
                });

                //x = result.Select(m => decimal.Parse(SectionData.DecTostring(m.profit)));
                x = result.Where(m => m.profit >= 0).Select(m => decimal.Parse(SectionData.DecTostring(m.profit)));
                count = x.Count();
            }
            SeriesCollection piechartData = new SeriesCollection();
           
            int xCount = 6;
            if (count < 6) xCount = count;

            for (int i = 0; i < xCount; i++)
            {
                List<decimal> final = new List<decimal>();

                //if (x.ToList().Skip(i).FirstOrDefault() > 0)
                {
                    final.Add(x.ToList().Skip(i).FirstOrDefault());
                    finalTitles.Add(titles[i]);

                    piechartData.Add(
                   new PieSeries
                   {
                       Values = final.AsChartValues(),
                       Title = finalTitles.Skip(i).FirstOrDefault(),
                       DataLabels = true,
                   }
                );
                }
            }

            if (count > 6)
            {
                decimal finalSum = 0;

                for (int i = 6; i < count; i++)
                {
                  //  if (x.ToList().Skip(i).FirstOrDefault() > 0)
                    {
                        finalSum = finalSum + x.ToList().Skip(i).FirstOrDefault();
                    }
                }

                List<decimal> final = new List<decimal>();
                List<string> lable = new List<string>();

                if (finalSum > 0)
                    final.Add(finalSum);

                piechartData.Add(
                new PieSeries
                {
                    Values = final.AsChartValues(),
                    Title = MainWindow.resourcemanager.GetString("trOthers"),
                    DataLabels = true,
                }
                );
            }

            chart1.Series = piechartData;
        }
        private void fillRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> ids = new List<int>();
            List<int> otherIds = new List<int>();

            List<ItemUnitInvoiceProfit> resultList = new List<ItemUnitInvoiceProfit>();
            SeriesCollection rowChartData = new SeriesCollection();

            if (selectedTab == 0 || selectedTab == 1)
            {
                var tempName = profitsQuery.GroupBy(s => new { s.branchCreatorId }).Select(s => new
                {
                    id = s.Key,
                    name = s.FirstOrDefault().branchCreatorName
                });
                names.AddRange(tempName.Select(nn => nn.name.ToString()));
                ids.AddRange(tempName.Select(mm => mm.id.branchCreatorId.Value));
            }
            else if (selectedTab == 2)
            {
                var tempName = profitsQuery.GroupBy(s => new { s.ITitemId }).Select(s => new
                {
                    id = s.Key,
                    name = s.FirstOrDefault().ITitemName
                });
                names.AddRange(tempName.Select(nn => nn.name.ToString()));
                ids.AddRange(tempName.Select(mm => mm.id.ITitemId.Value));
            }

            int x = 6;
            if (names.Count() < 6) x = names.Count();
            for (int i = 0; i < x; i++)
            {

                drawPoints(names[i], ids[i], rowChartData, 'n', otherIds);
            }
            //others
            if (names.Count() > 6)
            {
                for (int i = names.Count - x; i < names.Count; i++)
                    otherIds.Add(ids[i]);
                drawPoints(MainWindow.resourcemanager.GetString("trOthers"), 0, rowChartData, 'o', otherIds);
            }

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void drawPoints(string name, int id, SeriesCollection rowChartData, char ch, List<int> otherIds)
        {
            int endYear = DateTime.Now.Year;
            int startYear = endYear - 1;
            int startMonth = DateTime.Now.Month;
            int endMonth = startMonth;
            if (dp_startDate.SelectedDate != null && dp_endDate.SelectedDate != null)
            {
                startYear = dp_startDate.SelectedDate.Value.Year;
                endYear = dp_endDate.SelectedDate.Value.Year;
                startMonth = dp_startDate.SelectedDate.Value.Month;
                endMonth = dp_endDate.SelectedDate.Value.Month;
            }
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> profitLst = new List<decimal>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);

                        if (selectedTab == 0 || selectedTab == 1)
                        {
                            if (ch == 'n')
                            {
                                var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.branchCreatorId.Value == id)
                                                              .Select(b => b.invoiceProfit).Sum();

                                profitLst.Add(decimal.Parse(SectionData.DecTostring(drawProfit)));
                            }
                            else if (ch == 'o')
                            {
                                decimal sum = 0;
                                for (int i = 0; i < otherIds.Count; i++)
                                {
                                    var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.branchCreatorId.Value == otherIds[i])
                                                             .Select(b => b.invoiceProfit).Sum();
                                    sum = sum + drawProfit;
                                }
                                profitLst.Add(decimal.Parse(SectionData.DecTostring(sum)));
                            }
                        }
                        else if (selectedTab == 2)
                        {
                            if (ch == 'n')
                            {
                                var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.ITitemId.Value == id)
                                                              .Select(b => b.itemunitProfit).Sum();

                                profitLst.Add(decimal.Parse(SectionData.DecTostring(drawProfit)));
                            }
                            else if (ch == 'o')
                            {
                                decimal sum = 0;
                                for (int i = 0; i < otherIds.Count; i++)
                                {
                                    var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.ITitemId.Value == otherIds[i])
                                                             .Select(b => b.itemunitProfit).Sum();
                                    sum = sum + drawProfit;
                                }
                                profitLst.Add(decimal.Parse(SectionData.DecTostring(sum)));
                            }
                        }
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

                    if (selectedTab == 0 || selectedTab == 1)
                    {
                        if (ch == 'n')
                        {
                            var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.branchCreatorId.Value == id)
                                                           .Select(b => b.invoiceProfit).Sum();

                            profitLst.Add(decimal.Parse(SectionData.DecTostring(drawProfit)));
                        }
                        else if (ch == 'o')
                        {
                            decimal sum = 0;
                            for (int i = 0; i < otherIds.Count; i++)
                            {
                                var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.branchCreatorId.Value == otherIds[i])
                                                          .Select(b => b.invoiceProfit).Sum();
                                sum = sum + drawProfit;
                            }
                            profitLst.Add(decimal.Parse(SectionData.DecTostring(sum)));
                        }
                    }
                    else if (selectedTab == 2)
                    {
                        if (ch == 'n')
                        {
                            var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.ITitemId.Value == id)
                                                           .Select(b => b.itemunitProfit).Sum();

                            profitLst.Add(decimal.Parse(SectionData.DecTostring(drawProfit)));
                        }
                        else if (ch == 'o')
                        {
                            decimal sum = 0;
                            for (int i = 0; i < otherIds.Count; i++)
                            {
                                var drawProfit = profitsQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.ITitemId.Value == otherIds[i])
                                                           .Select(b => b.itemunitProfit).Sum();
                                sum = sum + drawProfit;
                            }
                            profitLst.Add(decimal.Parse(SectionData.DecTostring(sum)));
                        }
                    }
                    MyAxis.Labels.Add(year.ToString());
                }
            }

            rowChartData.Add(
                        new LineSeries
                        {
                            Values = profitLst.AsChartValues(),
                            Title = name
                        });

        }
        #endregion

        #region reports
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
                List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();

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
                List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();

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


        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;

            string firstTitle = "accountProfits";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string branchVal = "";
            string posVal = "";
            string itemVal = "";
            string unitVal = "";
            //  string cardval = "";
            string searchval = "";
            string trBranch = "";
            string trPos = "";


            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
          
            if (isArabic)
            {
                if (selectedTab == 0)/////////////////????????????????????net profits
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Profit\Ar\NetProfit.rdlc";
                    secondTitle = "netProfit";
                    trBranch = MainWindow.resourcemanagerreport.GetString("trBranch");
                  
                    paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trNetProfit_")));
                }
             else   if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Profit\Ar\Profit.rdlc";
                    secondTitle = "invoice";
                    trBranch = MainWindow.resourcemanagerreport.GetString("trBranch");
                    trPos = MainWindow.resourcemanagerreport.GetString("trPOS");
                    paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
                   

                }
                else
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Profit\Ar\ProfitItem.rdlc";
                    secondTitle = "items";
                    trBranch = MainWindow.resourcemanagerreport.GetString("trItem");
                    trPos = MainWindow.resourcemanagerreport.GetString("trUnit");
                    paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
         

                }

                //Reports\StatisticReport\Sale\Daily\Ar
            }
            else
            {
                if (selectedTab == 0)//////////////////////??????????????????? net profits
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Profit\En\NetProfit.rdlc";
                    secondTitle = "netProfit";
                    trBranch = MainWindow.resourcemanagerreport.GetString("trBranch");
      
                    paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trNetProfit_")));

                }
                else  if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Profit\En\Profit.rdlc";
                    secondTitle = "invoice";
                    trBranch = MainWindow.resourcemanagerreport.GetString("trBranch");
                    trPos = MainWindow.resourcemanagerreport.GetString("trPOS");
                    paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));
       

                }
                else
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Profit\En\ProfitItem.rdlc";
                    secondTitle = "items";
                    trBranch = MainWindow.resourcemanagerreport.GetString("trItem");
                    trPos = MainWindow.resourcemanagerreport.GetString("trUnit");
                    paramarr.Add(new ReportParameter("trTotal", MainWindow.resourcemanagerreport.GetString("trTotal")));


                }
            }
            //filter
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";

            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";
            //startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            //endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
            branchVal = cb_branches.SelectedItem != null
                && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
                ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");

            posVal = cb_pos.SelectedItem != null
               && (chk_allPos.IsChecked == false || chk_allPos.IsChecked == null)
               && branchVal != ""
               ? cb_pos.Text : (chk_allPos.IsChecked == true && branchVal != "" ? all : ""); 
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            
            paramarr.Add(new ReportParameter("trBranch", trBranch));
            paramarr.Add(new ReportParameter("trPOS", trPos));
            paramarr.Add(new ReportParameter("branchVal", branchVal));
            paramarr.Add(new ReportParameter("posVal", posVal));
            //paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            //paramarr.Add(new ReportParameter("trUnit", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
      
            
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            // IEnumerable<ItemUnitInvoiceProfit>
            if (selectedTab == 0)
            {

                clsReports.ProfitNetReport(profitsQuery, rep, reppath, paramarr,dp_startDate.SelectedDate.Value, dp_endDate.SelectedDate);
            }
            else
            {
                clsReports.ProfitReport(profitsQuery, rep, reppath, paramarr);
            }

            paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));

            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
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
