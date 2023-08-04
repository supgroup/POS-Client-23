using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_purchaseItemsCost.xaml
    /// </summary>
    public partial class uc_purchaseItemsCost : UserControl
    {
        #region variables
        IEnumerable<ItemUnitCost> itemUnitCost;
        Statistics statisticsModel = new Statistics();
        IEnumerable<ItemUnitCost> itemUnitCostQuery;
        string searchText = "";
        
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion
        public uc_purchaseItemsCost()
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
        private static uc_purchaseItemsCost _instance;

        public static uc_purchaseItemsCost Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_purchaseItemsCost();
                return _instance;
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
                itemUnitCost = await statisticsModel.GetItemUnitCost();
                fillItems();
                chk_allItems.IsChecked = true;
                chk_allUnits.IsChecked = true;

                #region key up
                cb_items.IsTextSearchEnabled = false;
                cb_items.IsEditable = true;
                cb_items.StaysOpenOnEdit = true;
                cb_items.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_items.Text = "";

                cb_unit.IsTextSearchEnabled = false;
                cb_unit.IsEditable = true;
                cb_unit.StaysOpenOnEdit = true;
                cb_unit.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_unit.Text = "";
                #endregion

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_itemsCost.Tag.ToString());

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
        
        #region events
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
        private async void Cb_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select item
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUnits((int)cb_items.SelectedValue);
                pnl_unit.IsEnabled = true;
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
        private async void Chk_allItems_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_items.SelectedIndex = -1;
                cb_items.IsEnabled = false;
                pnl_unit.IsEnabled = false;
                cb_unit.SelectedIndex = -1;
                chk_allUnits.IsChecked = true;
                cb_items.Text = "";
                cb_items.ItemsSource = items;

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
        private async void Cb_unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select pos
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
        private async void Chk_allUnits_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_unit.SelectedIndex = -1;
                cb_unit.IsEnabled = false;
                cb_unit.Text = "";
                cb_unit.ItemsSource = units;
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

                cb_unit.IsEnabled = true;
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
        private async void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                searchText = "";
                chk_allItems.IsChecked = true;
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
        private void Cb_items_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = items.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_unit_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = units.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region methods
        private void translate()
        {
            tt_itemsCost.Content = MainWindow.resourcemanager.GetString("trItemsCost");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_items, MainWindow.resourcemanager.GetString("trItem")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_unit, MainWindow.resourcemanager.GetString("trUnit")+"...");

            chk_allItems.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allUnits.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_cost.Header = MainWindow.resourcemanager.GetString("trEntryCost");
            col_finalCost.Header = MainWindow.resourcemanager.GetString("trRealCost");
            col_avg.Header = MainWindow.resourcemanager.GetString("trCostPercentage");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        async Task<IEnumerable<ItemUnitCost>> RefreshItemUnitCostList()
        {
            itemUnitCost = await statisticsModel.GetItemUnitCost();
            return itemUnitCost;

        }
        async Task Search()
        {
            if (itemUnitCost is null)
                await RefreshItemUnitCostList();

            searchText = txt_search.Text.ToLower();
            itemUnitCostQuery = itemUnitCost
                .Where(s =>
            (
            s.itemName.ToLower().Contains(searchText)
            ||
            s.unitName.ToLower().Contains(searchText)
            )
            &&
            //itemID
            (chk_allItems.IsChecked.Value ?
                true :
                (cb_items.SelectedIndex != -1 ? s.itemId == Convert.ToInt32(cb_items.SelectedValue) : false)
            )
            &&
            //unitID
            (chk_allUnits.IsChecked.Value ?
                true :
                (cb_unit.SelectedIndex != -1 ? s.unitId == Convert.ToInt32(cb_unit.SelectedValue) : false)
            )
            );

            RefreshItemsCostView();
            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        List<Item> items = new List<Item>();
        private void fillItems()
        {
            cb_items.SelectedValuePath = "itemId";
            cb_items.DisplayMemberPath = "name";
            items = itemUnitCost.GroupBy(i => i.itemId).Select(i => new Item { name = i.FirstOrDefault().itemName, itemId = i.FirstOrDefault().itemId.Value }).ToList();
            cb_items.ItemsSource = items;
        }
        List<Unit> units = new List<Unit>();
        private void fillUnits(int iID)
        {
            cb_unit.SelectedValuePath = "unitId";
            cb_unit.DisplayMemberPath = "name";
            units = itemUnitCost.Where(b => b.itemId == iID).GroupBy(u => u.unitId).Select(i => new Unit
                                                                     { name = i.FirstOrDefault().unitName, unitId = i.FirstOrDefault().unitId.Value }).ToList();
            cb_unit.ItemsSource = units;

        }
        private void RefreshItemsCostView()
        {
            dg_itemsCost.ItemsSource = itemUnitCostQuery;
            txt_count.Text = itemUnitCostQuery.Count().ToString();
            decimal total = itemUnitCost.Select(b => b.finalcost.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);
        }
        #endregion

        #region charts
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
           
            var res = itemUnitCostQuery.GroupBy(x => new { x.itemUnitId }).Select(x => new 
            {
                name = x.FirstOrDefault().itemName+ "/" + x.FirstOrDefault().unitName,
                cost = x.Sum(g => g.cost),
                finalCost = x.Sum(g => g.avgPurchasePrice)

            });
           
            names.AddRange(res.Select(nn => nn.name));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> entry = new List<decimal>();
            List<decimal> real = new List<decimal>();

            int xCount = 6;
            if (res.Count() <= 6)
                xCount = res.Count();
            for (int i = 0; i < xCount; i++)
            {
                entry.Add(decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().cost.Value)));
                real.Add(decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().finalCost.Value)));
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (res.Count() > 6)
            {
                decimal entrySum = 0, realSum = 0 ;
                for (int i = 6; i < res.Count(); i++)
                {
                    entrySum = entrySum + decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().cost.Value));
                    realSum = realSum + decimal.Parse(SectionData.DecTostring(res.ToList().Skip(i).FirstOrDefault().finalCost.Value));
                }
                if (!(entrySum == 0 && realSum == 0))
                {
                    entry.Add(entrySum);
                    real.Add(realSum);

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new ColumnSeries
            {
                Values = entry.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trEntryCost")
            });
            columnChartData.Add(
            new ColumnSeries
            {
                Values = real.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trRealCost")
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<decimal> resultList = new List<decimal>();
            titles.Clear();

            var result = itemUnitCostQuery.GroupBy(s => s.itemUnitId).Select(s => new
            {
                diffPercent = s.FirstOrDefault().diffPercent != null ? s.FirstOrDefault().diffPercent : 0,
                name = s.FirstOrDefault().itemName+ "/" + s.FirstOrDefault().unitName
            });
            resultList = result.Where(m => m.diffPercent.Value >= 0).Select(m => m.diffPercent.Value).ToList();
            titles = result.Select(m => m.name).ToList();
           
            SeriesCollection piechartData = new SeriesCollection();
            List<string> finalTitles = new List<string>();
            int xCount = 6;
            if (resultList.Count < 6) xCount = resultList.Count;

            for (int i = 0; i < xCount; i++)
            {
                List<decimal> final = new List<decimal>();

                if (resultList.ToList().Skip(i).FirstOrDefault() > 0)
                {
                    final.Add(decimal.Parse(SectionData.DecTostring(resultList.ToList().Skip(i).FirstOrDefault())));
                    finalTitles.Add(titles[i]);

                    piechartData.Add(
                    new PieSeries
                    {
                        Values = final.AsChartValues(),
                        Title = titles[i],
                        DataLabels = true,
                    });
                }
            }

            if (resultList.Count > 6)
            {
                decimal finalSum = 0;

                for (int i = 6; i < resultList.Count; i++)
                {
                    if (resultList.ToList().Skip(i).FirstOrDefault() > 0)
                    {
                        finalSum = finalSum + decimal.Parse(SectionData.DecTostring(resultList.ToList().Skip(i).FirstOrDefault()));
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
          //  int endYear = DateTime.Now.Year;
          //  int startYear = endYear - 1;
          //  int startMonth = DateTime.Now.Month;
          //  int endMonth = startMonth;
            
          //  MyAxis.Labels = new List<string>();
          //  List<string> names = new List<string>();
          //  List<CashTransferSts> resultList = new List<CashTransferSts>();

          //  SeriesCollection rowChartData = new SeriesCollection();

          //  var tempName = itemUnitCostQuery.GroupBy(s => new { s.itemUnitId }).Select(s => new
          //  {
          //      Name = s.FirstOrDefault().updateDate,
          //  });
          //  names.AddRange(tempName.Select(nn => nn.Name.ToString()));
          //  string title = "";
          //  title = MainWindow.resourcemanager.GetString("trRealCost") ;

          //  List<string> lable = new List<string>();
          //  SeriesCollection columnChartData = new SeriesCollection();
          //  List<decimal> taxLst = new List<decimal>();

          //  if (endYear - startYear <= 1)
          //  {
          //      for (int year = startYear; year <= endYear; year++)
          //      {
          //          for (int month = startMonth; month <= 12; month++)
          //          {
          //              var firstOfThisMonth = new DateTime(year, month, 1);
          //              var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
          //              var drawTax = itemUnitCostQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth).Select(c => c.invTaxVal.Value).Sum();
          //              taxLst.Add(drawTax);
          //              MyAxis.Labels.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year);

          //              if (year == endYear && month == endMonth)
          //              {
          //                  break;
          //              }
          //              if (month == 12)
          //              {
          //                  startMonth = 1;
          //                  break;
          //              }
          //          }
          //      }
          //  }
          //  else
          //  {
          //      for (int year = startYear; year <= endYear; year++)
          //      {
          //          var firstOfThisYear = new DateTime(year, 1, 1);
          //          var firstOfNextMYear = firstOfThisYear.AddYears(1);
          //          var drawTax = itemUnitCostQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear).Select(c => c.invTaxVal.Value).Sum();
          //          taxLst.Add(drawTax);
          //          MyAxis.Labels.Add(year.ToString());
          //      }
          //  }
          //  rowChartData.Add(
          //new LineSeries
          //{
          //    Values = taxLst.AsChartValues(),
          //    Title = title
          //}); ;

          //  DataContext = this;
          //  rowChart.Series = rowChartData;
        }

        #endregion

        #region reports
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "itemsCost";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";

            bool isArabic = ReportCls.checkLang();
            string itemval = "";
            string unitval = "";
            string searchval = "";
            
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Purchase\ItemCost\Ar\ArItemCost.rdlc";

            }
            else
            {
                //english
                addpath = @"\Reports\StatisticReport\Purchase\ItemCost\En\EnItemCost.rdlc";
            }

     
            itemval = cb_items.SelectedItem != null
                          && (chk_allItems.IsChecked == false || chk_allItems.IsChecked == null)
                           
                          ? cb_items.Text : (chk_allItems.IsChecked == true   ? all : "");
            unitval = cb_unit.SelectedItem != null
           && (chk_allUnits.IsChecked == false || chk_allUnits.IsChecked == null)
         && itemval != ""
           ? cb_unit.Text : (chk_allUnits.IsChecked == true && itemval != "" ? all : "");

            
            paramarr.Add(new ReportParameter("ItemVal", itemval));
            paramarr.Add(new ReportParameter("UnitVal", unitval));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));


            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));

            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trPurchasesReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //  getpuritemcount
           // paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));

            clsReports.PurItemCostStsReport(itemUnitCostQuery, rep, reppath, paramarr);
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
