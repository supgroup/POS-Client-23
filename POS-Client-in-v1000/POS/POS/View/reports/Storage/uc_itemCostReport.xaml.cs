using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
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
using POS.View.windows;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_itemCostReport.xaml
    /// </summary>
    public partial class uc_itemCostReport : UserControl
    {
        #region variables
        List<Storage> itemsCost;
        IEnumerable<Storage> itemsCostQuery;

        Statistics statisticModel = new Statistics();

        List<Branch> comboBranches;

        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        string searchText = "";
        #endregion

        private static uc_itemCostReport _instance;
        public static uc_itemCostReport Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_itemCostReport();
                return _instance;
            }
        }
        public uc_itemCostReport()
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

                itemsCost = await statisticModel.GetPurchaseCost((int)MainWindow.branchID, (int)MainWindow.userID);
                fillBranches();
                fillItems();

                chk_allBranches.IsChecked = true;
                chk_allItems.IsChecked = true;
                chk_allUnits.IsChecked = true;

                await Search();

                #region key up
                // key_up branch
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                // key_up item
                cb_items.IsTextSearchEnabled = false;
                cb_items.IsEditable = true;
                cb_items.StaysOpenOnEdit = true;
                cb_items.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_items.Text = "";
                // key_up item
                cb_units.IsTextSearchEnabled = false;
                cb_units.IsEditable = true;
                cb_units.StaysOpenOnEdit = true;
                cb_units.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_units.Text = "";
                #endregion

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_item.Tag.ToString());

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

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_items, MainWindow.resourcemanager.GetString("trItem") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_units, MainWindow.resourcemanager.GetString("trUnit")+"...");

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItems.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allUnits.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_itemUnits.Header = MainWindow.resourcemanager.GetString("trItem")+"-"+ MainWindow.resourcemanager.GetString("trUnit");
            col_sectionLocations.Header = MainWindow.resourcemanager.GetString("trSection") + "-" + MainWindow.resourcemanager.GetString("trLocation");
            col_itemsCost.Header = MainWindow.resourcemanager.GetString("trCost");

            txt_total.Text = MainWindow.resourcemanager.GetString("trCost");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
         }
        async Task<IEnumerable<Storage>> RefreshItemsCostList()
        {
            itemsCost = await statisticModel.GetPurchaseCost((int)MainWindow.branchID, (int)MainWindow.userID);
            return itemsCost;
        }
        async Task Search()
        {
            if (itemsCost is null)
                await RefreshItemsCostList();

            searchText = txt_search.Text.ToLower();
            itemsCostQuery = itemsCost
                .Where(s =>
            (
            s.branchName.ToLower().Contains(searchText)
            ||
            s.ItemUnits.ToString().ToLower().Contains(searchText)
            ||
            s.SectionLoactionName.ToString().ToLower().Contains(searchText)
            )
            &&
            //branchID
            (
            chk_allBranches.IsChecked.Value ?
                true :
                (cb_branches.SelectedIndex != -1 ? s.branchId == Convert.ToInt32(cb_branches.SelectedValue) : false)
            )
            &&
            //itemID
            (
            chk_allItems.IsChecked.Value ?
                true :
                (cb_items.SelectedIndex != -1 ? s.itemId == Convert.ToInt32(cb_items.SelectedValue) : false)
            )
            &&
            //unitID
            (
            chk_allUnits.IsChecked.Value ?
                true :
                (cb_units.SelectedIndex != -1 ? s.unitId == Convert.ToInt32(cb_units.SelectedValue) : false)
            )
            );
            ////lstColumnChart = itemTrasferInvoicesQuery;

            RefreshIemsCostView();
            fillColumnChart();
            fillPieChart();
            //fillRowChart();
        }
        void RefreshIemsCostView()
        {
            dg_itemsCost.ItemsSource = itemsCostQuery;
            txt_count.Text = itemsCostQuery.Count().ToString();

            decimal total = 0;
            total = itemsCostQuery.Select(b => b.avgPurchasePrice.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);
        }
        List<Branch> branches = new List<Branch>();
        private void fillBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            branches = itemsCost.GroupBy(i => i.branchId).Select(i => new Branch { name = i.FirstOrDefault().branchName, branchId = i.FirstOrDefault().branchId }).ToList();
            cb_branches.ItemsSource = branches;
        }
        List<Item> items = new List<Item>();
        private void fillItems()
        {
            cb_items.SelectedValuePath = "itemId";
            cb_items.DisplayMemberPath = "name";
            items = itemsCost.GroupBy(i => i.itemId).Select(i => new Item { itemId = i.FirstOrDefault().itemId, name = i.FirstOrDefault().itemName }).ToList();
            cb_items.ItemsSource = items;
        }
        List<Unit> units = new List<Unit>();
        private void fillUnits(int itemId)
        {
            cb_units.SelectedValuePath = "unitId";
            cb_units.DisplayMemberPath = "name";
            units = itemsCost.Where(i=> i.itemId == itemId).GroupBy(i => i.unitId).Select(i => new Unit { unitId = i.FirstOrDefault().unitId, name = i.FirstOrDefault().unitName }).ToList();
            cb_units.ItemsSource = units;
        }
        #endregion

        #region events
        private async void Cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.IsEnabled = false;
                cb_branches.Text = "";
                cb_branches.ItemsSource = branches;
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
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.IsEnabled = true;

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
        private async void Cb_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_items.SelectedIndex != -1)
                {
                    fillUnits((int)cb_items.SelectedValue);
                    chk_allUnits.IsEnabled = true;
                    chk_allUnits.IsChecked = true;
                    await Search();
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
        private void Cb_items_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = items.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Chk_allItems_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_items.SelectedIndex = -1;
                cb_items.IsEnabled = false;
                cb_items.Text = "";
                cb_items.ItemsSource = items;
                chk_allUnits.IsEnabled = false;
                chk_allUnits.IsChecked = true;

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
        private async void Chk_allItems_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_items.IsEnabled = true;

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
        private async void Cb_units_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
        private void Cb_units_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = units.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Chk_allUnits_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_units.SelectedIndex = -1;
                cb_units.IsEnabled = false;
                cb_units.Text = "";
                cb_units.ItemsSource = units;
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
        private async void Chk_allUnits_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_units.IsEnabled = true;
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
        private async void Txt_search_SelectionChanged(object sender, RoutedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
        private void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                searchText = "";
                
                chk_allBranches.IsChecked = true;
                chk_allItems.IsChecked = true;
                chk_allUnits.IsChecked = true;

                //  await Search();

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

        #region report
        //   ReportCls reportclass = new ReportCls();
        //   LocalReport rep = new LocalReport();
        //     SaveFileDialog saveFileDialog = new SaveFileDialog();

        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            //string firstTitle = "destroied";
            //string secondTitle = "";
            //string subTitle = "";
            string Title = "";

         
            string branchval = "";
            string itemval = "";

            string searchval = "";
            string Available = "";
            string Sold = "";
            string notSold = "";
            string unitval = "";
            
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Storage\ItemCost\Ar\ArItemCost.rdlc";


            }
            else
            {
                addpath = @"\Reports\StatisticReport\Storage\ItemCost\En\ItemCost.rdlc";

            }
           
            branchval = cb_branches.SelectedItem != null
       && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
       ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");

            itemval = cb_items.SelectedItem != null
               && (chk_allItems.IsChecked == false || chk_allItems.IsChecked == null)
               && branchval != ""
               ? cb_items.Text : (chk_allItems.IsChecked == true && branchval != "" ? all : "");
            unitval = cb_units.SelectedItem != null
           && (chk_allUnits.IsChecked == false || chk_allUnits.IsChecked == null)
         && itemval != ""
           ? cb_units.Text : (chk_allUnits.IsChecked == true && itemval != "" ? all : "");

            //available sold
            /*
             *      chk_isNotSold.Content = MainWindow.resourcemanager.GetString("trAvailable");
            chk_isSold.Content = MainWindow.resourcemanager.GetString("trSold");
             * */
            // secondTitle = "destroied";
            //  subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + MainWindow.resourcemanagerreport.GetString("trItemsCost");
            paramarr.Add(new ReportParameter("trTitle", Title));


           // paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
           // paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
            paramarr.Add(new ReportParameter("BranchVal", branchval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));
            paramarr.Add(new ReportParameter("UnitVal", unitval));

            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
       
          
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));



            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            clsReports.itemCostReportSTS(itemsCostQuery, rep, reppath, paramarr);
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
                LocalReportExtensions.PrintToPrinter(rep);
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

        #region charts
        private void fillRowChart()
        {
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<string> titles1 = new List<string>();
            List<decimal> x = new List<decimal>();
            titles.Clear();
            titles1.Clear();

            var result = itemsCostQuery
                .GroupBy(s => new { s.itemUnitId })
                .Select(s => new Storage
                {
                    itemId = s.FirstOrDefault().itemId,
                    unitId = s.FirstOrDefault().unitId,
                    avgPurchasePrice = s.Sum(g => g.avgPurchasePrice),
                    itemName = s.FirstOrDefault().itemName,
                    unitName = s.FirstOrDefault().unitName,
                });
            x = result.Select(m => decimal.Parse(SectionData.DecTostring((decimal)m.avgPurchasePrice))).ToList();
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
                    lable.Add(titles.Skip(i).FirstOrDefault() + titles1.Skip(i).FirstOrDefault());
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
                  ); ;
                    break;
                }

            }
            chart1.Series = piechartData;
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            var res = itemsCostQuery.GroupBy(x => new { x.branchId }).Select(x => new Storage
            {
                branchId = x.FirstOrDefault().branchId,
                branchName = x.FirstOrDefault().branchName,
                avgPurchasePrice = x.Sum(g => (decimal)(g.avgPurchasePrice))
            });

            names.AddRange(res.Select(nn => nn.branchName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cP = new List<decimal>();

            int xCount = 6;
            if (names.Count() <= 6) xCount = names.Count();

            for (int i = 0; i < xCount; i++)
            {
                cP.Add(decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().avgPurchasePrice.Value)));
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (names.Count() > 6)
            {
                decimal b = 0;
                for (int i = 6; i < names.Count(); i++)
                {
                    b = b + decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().avgPurchasePrice.Value));
                }
                if (!(b == 0))
                {
                    cP.Add(b);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trBranch")
            }); ;

            DataContext = this;
            cartesianChart.Series = columnChartData;
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
