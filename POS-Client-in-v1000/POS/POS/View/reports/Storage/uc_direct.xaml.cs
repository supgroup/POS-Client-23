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
using static POS.Classes.Statistics;
using LiveCharts.Helpers;
using LiveCharts;
using LiveCharts.Wpf;
using System.Resources;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.Reporting.WinForms;
using System.Threading;
using System.IO;
using POS.View.windows;
using POS.View.storage;
using System.Collections.ObjectModel;
using POS.converters;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_direct.xaml
    /// </summary>
    /// 
  
    public partial class uc_direct : UserControl
    {
        #region variables
        Statistics statisticModel = new Statistics();
        List<ItemTransferInvoice> itemsTransfer;
        List<ItemTransferInvoice> Invoices;
        List<ExternalitemCombo> comboDirectItems;
        List<ExternalUnitCombo> comboDirectUnits;
        IEnumerable<ItemTransferInvoice> temp = null;

        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private int selectedTab = 0;

        string searchText = "";
        #endregion

        public uc_direct()
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

        private static uc_direct _instance;
        public static uc_direct Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_direct();
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

                #region key up
                cb_directItemsBranches.IsTextSearchEnabled = false;
                cb_directItemsBranches.IsEditable = true;
                cb_directItemsBranches.StaysOpenOnEdit = true;
                //cb_directItemsBranches.Text = "";
                cb_directItemsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;

                cb_directItems.IsTextSearchEnabled = false;
                cb_directItems.IsEditable = true;
                cb_directItems.StaysOpenOnEdit = true;
                cb_directItems.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_directItems.Text = "";

                cb_directUnits.IsTextSearchEnabled = false;
                cb_directUnits.IsEditable = true;
                cb_directUnits.StaysOpenOnEdit = true;
                cb_directUnits.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_directUnits.Text = "";
                #endregion
                tb_totalCurrency.Text = AppSettings.Currency;

                col_reportChartWidth = col_reportChart.ActualWidth;

                itemsTransfer = await statisticModel.GetDirectInMov((int)MainWindow.branchID, (int)MainWindow.userID);
                comboDirectItems = statisticModel.getExternalItemCombo(itemsTransfer);
                comboDirectUnits = statisticModel.getExternalUnitCombo(itemsTransfer);

                Invoices = await statisticModel.GetDirectInvoice((int)MainWindow.branchID, (int)MainWindow.userID);


                txt_search.Text = "";

                //SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_item.Tag.ToString());


                await SectionData.fillBranchesWithoutMain(cb_directItemsBranches);
                chk_directAllBranches.IsChecked = true;
                dp_directStartDate.SelectedDate = null;
                dp_directEndDate.SelectedDate = null;
                //fillEvents();

                btn_branch_Click(btn_branch, null);
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

        #region charts
        private void fillDirectRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> pTemp = new List<int>();
           
            var result = temp.GroupBy(x => new { x.itemUnitId }).Select(x => new ItemTransferInvoice
            {
                itemUnitId = x.FirstOrDefault().itemUnitId,
                itemName = x.FirstOrDefault().itemName+"-"+ x.FirstOrDefault().unitName,
                itemsCount = x.Sum(g => (int)(g.quantity))
            });

            for (int i = 0; i < result.Count(); i++)
            {
                pTemp.Add(result.ToList().Skip(i).FirstOrDefault().itemsCount.Value);
            }
          
            names.AddRange(result.Select(nn => nn.itemName));

            SeriesCollection rowChartData = new SeriesCollection();
            for (int i = 0; i < pTemp.Count(); i++)
            {
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            rowChartData.Add(
              new LineSeries
              {
                  Values = pTemp.AsChartValues(),
                  Title = MainWindow.resourcemanager.GetString("trItemUnit")

              });

            rowChart.Series = rowChartData;
            DataContext = this;
        }

        private void fillDirectColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<int> z = null;

            var res = temp;
            if (selectedTab == 0)
            {
                var result = res.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    countP = s.Where(m => m.invType == "is").Count(),
                    countD = s.Where(m => m.invType == "isd").Count()
                });
                x = result.Select(m => m.countP);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));


                List<string> lable = new List<string>();
                SeriesCollection columnChartData = new SeriesCollection();
                List<int> cP = new List<int>();
                List<int> cD = new List<int>();
                List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("tr_Invoice"),
                MainWindow.resourcemanager.GetString("trDraft")
            };
                for (int i = 0; i < x.Count(); i++)
                {
                    cP.Add(x.ToList().Skip(i).FirstOrDefault());
                    cD.Add(z.ToList().Skip(i).FirstOrDefault());
                    axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
                }

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
                   Values = cD.AsChartValues(),
                   Title = titles[1],
                   DataLabels = true,
               });

                DataContext = this;
                cartesianChart.Series = columnChartData;
            }
            else if (selectedTab == 1)
            {
                res = temp.GroupBy(xx => new { xx.branchId }).Select(xx => new ItemTransferInvoice
                {
                    branchId = xx.FirstOrDefault().branchId,
                    branchName = xx.FirstOrDefault().branchName,
                    itemsCount = xx.Sum(g => (int)(g.quantity))
                });

                names.AddRange(res.Select(nn => nn.branchName));

                List<string> lable = new List<string>();
                SeriesCollection columnChartData = new SeriesCollection();
                List<int> cP = new List<int>();

                int xCount = 6;
                if (names.Count() <= 6) xCount = names.Count();

                for (int i = 0; i < xCount; i++)
                {
                    cP.Add(res.ToList().Skip(i).FirstOrDefault().itemsCount.Value);
                    axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
                }
                if (names.Count() > 6)
                {
                    int b = 0;
                    for (int i = 6; i < names.Count(); i++)
                    {
                        b = b + res.ToList().Skip(i).FirstOrDefault().itemsCount.Value;
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
        }

        private void fillDirectPieChart()
        {


            List<string> titles = new List<string>();
            List<string> titles1 = new List<string>();
            List<decimal> x = new List<decimal>();
            titles.Clear();
            titles1.Clear();


            var result = temp;

            if (selectedTab == 0)
            {
                result = result
                  .GroupBy(s => new { s.invType })
                  .Select(s => new ItemTransferInvoice
                  {
                      branchId = s.FirstOrDefault().branchId,
                      quantity = s.Sum(g => g.quantity),
                      branchName = s.FirstOrDefault().branchName,
                  }).OrderByDescending(s => s.quantity);
                x = result.Select(m => (decimal)m.quantity).ToList();
                titles = result.Select(m => m.branchName).ToList();
                 int count = x.Count();
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
                      ); ;
                        break;
                    }

                }
                chart1.Series = piechartData;

            }
            else if (selectedTab == 1)
            {
                 result = result
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


        }

        #endregion

        #region methods
        private void translate()
        {
            tt_item.Content = MainWindow.resourcemanager.GetString("trItems");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_directItemsBranches, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_directItems, MainWindow.resourcemanager.GetString("trItemHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_directUnits, MainWindow.resourcemanager.GetString("trUnitHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_directStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_directEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_endTime, MainWindow.resourcemanager.GetString("trEndTime") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_startTime, MainWindow.resourcemanager.GetString("trStartTime") + "...");

            chk_directAllBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_directAllItems.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_directAllUnits.Content = MainWindow.resourcemanager.GetString("trAll");

            rad_invoice.Content = MainWindow.resourcemanager.GetString("tr_Invoice");
            rad_draft.Content = MainWindow.resourcemanager.GetString("trDraft");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");


            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_Num.Header = MainWindow.resourcemanager.GetString("trNo");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_Num1.Header = MainWindow.resourcemanager.GetString("trNo");
            col_date1.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_type.Header = MainWindow.resourcemanager.GetString("trType");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_unit.Header = MainWindow.resourcemanager.GetString("trUnit");
            col_quantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_price.Header = MainWindow.resourcemanager.GetString("trPrice");
            col_total.Header = MainWindow.resourcemanager.GetString("trTotal");
            

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
         }
        private IEnumerable<ItemTransferInvoice> fillList()
        {
            var result = new List<ItemTransferInvoice>();
            if (selectedTab == 0)
            {
                result = Invoices.Where(x =>
                          (cb_directItemsBranches.SelectedItem != null ? (x.branchId == Convert.ToInt32(cb_directItemsBranches.SelectedValue)) : true)
                       && (cb_directItems.SelectedItem != null ? (x.itemId == Convert.ToInt32(cb_directItems.SelectedValue)) : true)
                       && (cb_directUnits.SelectedItem != null ? (x.unitId == Convert.ToInt32(cb_directUnits.SelectedValue)) : true)
                       && (dp_directStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= dp_directStartDate.SelectedDate.Value.Date) : true)
                       && (dp_directEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= dp_directEndDate.SelectedDate.Value.Date) : true)
                       ).ToList();

                result = result.Where(i => (rad_invoice.IsChecked == true ? (i.invType == "is" ) : false)
                                       ||
                                       (rad_draft.IsChecked == true ? rad_invoice.IsChecked == true ? i.invType == "isd" :
                                                                      false? i.invType == "pbd" :
                                                                     (i.invType == "isd" || i.invType == "pbd") : false)
                                ).ToList();
            }
            else if(selectedTab == 1)
            {
                result = itemsTransfer.Where(x =>
                           (cb_directItemsBranches.SelectedItem != null ? (x.branchId == Convert.ToInt32(cb_directItemsBranches.SelectedValue)) : true)
                        && (cb_directItems.SelectedItem != null ? (x.itemId == Convert.ToInt32(cb_directItems.SelectedValue)) : true)
                        && (cb_directUnits.SelectedItem != null ? (x.unitId == Convert.ToInt32(cb_directUnits.SelectedValue)) : true)
                        && (dp_directStartDate.SelectedDate != null ? (x.updateDate.Value.Date >= dp_directStartDate.SelectedDate.Value.Date) : true)
                        && (dp_directEndDate.SelectedDate != null ? (x.updateDate.Value.Date <= dp_directEndDate.SelectedDate.Value.Date) : true)
                        && (dt_startTime.SelectedTime != null ? x.updateDate >= dt_startTime.SelectedTime : true)
                        && (dt_endTime.SelectedTime != null ? x.updateDate <= dt_endTime.SelectedTime : true)
                        ).ToList();
            }


            return result;

        }
        private void fillEvents()
        {
            temp = fillList();

            dgDirect.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();

            decimal total = 0;
            if(selectedTab == 0)
            {
                if (rad_draft.IsChecked.Value == true && (rad_invoice.IsChecked.Value == true))
                    total = temp.Where(i => i.invType != "isd").Select(b => b.totalNet.Value).Sum();
                else
                    total = temp.Select(b => b.totalNet.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);
            }
            else if (selectedTab == 1)
            {
                if (rad_draft.IsChecked.Value == true && (rad_invoice.IsChecked.Value == true))
                    total = temp.Where(i => i.invType != "isd").Select(b => b.total.Value).Sum();
                else
                    total = temp.Select(b => b.total.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);
            }

            fillDirectColumnChart();
            fillDirectPieChart();
        }
        private void fillEmptyEvents()
        {
            temp = new List<ItemTransferInvoice>();

            dgDirect.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();

            decimal total = 0;
            tb_total.Text = SectionData.DecTostring(total);

            fillDirectColumnChart();
            fillDirectPieChart();
        }
        private void fillComboDirectItems()
        {
            var temp = cb_directItemsBranches.SelectedItem as Branch;
            cb_directItems.SelectedValuePath = "ItemId";
            cb_directItems.DisplayMemberPath = "ItemName";
            if (temp == null)
            {
                cb_directItems.ItemsSource = comboDirectItems
                    .GroupBy(x => x.ItemId)
                    .Select(g => new ExternalitemCombo
                    {
                        ItemId = g.FirstOrDefault().ItemId,
                        ItemName = g.FirstOrDefault().ItemName,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();
            }
            else
            {
                cb_directItems.ItemsSource = comboDirectItems
                    .Where(x => x.BranchId == temp.branchId)
                    .GroupBy(x => x.ItemId)
                    .Select(g => new ExternalitemCombo
                    {
                        ItemId = g.FirstOrDefault().ItemId,
                        ItemName = g.FirstOrDefault().ItemName,
                        BranchId = g.FirstOrDefault().BranchId
                    }).ToList();
            }
        }
        List<ExternalUnitCombo> units = new List<ExternalUnitCombo>();
        private void fillComboDirectUnits()
        {
            #region old
            var temp = cb_directItems.SelectedItem as ExternalitemCombo;
            var temp1 = cb_directItemsBranches.SelectedItem as Branch;

            cb_directUnits.SelectedValuePath = "UnitId";
            cb_directUnits.DisplayMemberPath = "UnitName";
            if (temp == null && temp1 == null)
            {
                units = comboDirectUnits
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).Distinct().ToList();
                cb_directUnits.ItemsSource = units;
            }
            else if (temp != null && temp1 == null)
            {
                units = comboDirectUnits
                     .Where(x => x.ItemId == temp.ItemId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).Distinct().ToList();
                cb_directUnits.ItemsSource = units;
            }
            else if (temp == null && temp1 != null)
            {
                units = comboDirectUnits
                     .Where(x => x.BranchId == temp1.branchId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).Distinct().ToList();
                cb_directUnits.ItemsSource = units;
            }
            else
            {
                units = comboDirectUnits
                    .Where(x => x.ItemId == temp.ItemId && x.BranchId == temp1.branchId)
                    .GroupBy(x => x.UnitId)
                    .Select(g => new ExternalUnitCombo
                    {
                        UnitId = g.FirstOrDefault().UnitId,
                        UnitName = g.FirstOrDefault().UnitName,
                        ItemId = g.FirstOrDefault().ItemId
                    }).Distinct().ToList();
                cb_directUnits.ItemsSource = units;
            }
            #endregion
            //units = itemsTransfer.Where(i => i.itemId = (int)cb_directItems.SelectedValue).GroupBy(i => i.unitId).Select(i => new Unit { name = i.FirstOrDefault().unitName , unitId = i.FirstOrDefault().unitId}).ToList();
            //cb_directUnits.SelectedValuePath = "unitId";
            //cb_directUnits.DisplayMemberPath = "name";
            //cb_directUnits.ItemsSource = units;
        }
        #endregion

        #region tabs
        private async void btn_branch_Click(object sender, RoutedEventArgs e)
        {//branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_directItemsBranches, MainWindow.resourcemanager.GetString("trBranchHint"));

                txt_search.Text = "";
                selectedTab = 0;

                hideSatacks();
                stk_tagsBranches.Visibility = Visibility.Visible;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_branch);
                path_branch.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hideAllColumn();

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;

                col_Num.Visibility = Visibility.Visible;
                col_date.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_total.Visibility = Visibility.Visible;



                //var dgtc = dg_selectedStores.Columns[0] as DataGridTextColumn;
                //col_total.Binding = new System.Windows.Data.Binding("totalNet");


                accuracyConverter _accuracyConverter = new accuracyConverter();
                Binding binding = new Binding("totalNet");
                binding.Converter = _accuracyConverter;
                col_total.Binding =  binding;

                dp_invoiceTypeSearch.Visibility = Visibility.Visible;
                dp_timeSearch.Visibility = Visibility.Visible;
                rd_gridDirectItems2.Height = new GridLength(0, GridUnitType.Pixel);
                Grid.SetRow(cb_directItemsBranches, 1);
                Grid.SetRow(chk_directAllBranches, 1);
                Grid.SetRow(dp_dateSearch, 0);

                //fillComboBranches();
                await SectionData.fillBranchesWithoutMain(cb_directItemsBranches);
                changeTabReset();

                // key_up search Person name
                cb_directItemsBranches.IsTextSearchEnabled = false;
                cb_directItemsBranches.IsEditable = true;
                cb_directItemsBranches.StaysOpenOnEdit = true;
                cb_directItemsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;

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
        private async void btn_item_Click(object sender, RoutedEventArgs e)
        {//pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_directItemsBranches, MainWindow.resourcemanager.GetString("trPosHint"));

                txt_search.Text = "";
                selectedTab = 1;

                hideSatacks();
                stk_tagsPos.Visibility = Visibility.Visible;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_item);
                path_item.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hideAllColumn();
                
                col_item.Visibility = Visibility.Visible;
                col_unit.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                col_Num1.Visibility = Visibility.Visible;
                col_date1.Visibility = Visibility.Visible;
                col_quantity.Visibility = Visibility.Visible;
                col_price.Visibility = Visibility.Visible;
                col_total.Visibility = Visibility.Visible;



                //var dgtc = dg_selectedStores.Columns[0] as DataGridTextColumn;
                //col_total.Binding = new System.Windows.Data.Binding("total");

                accuracyConverter _accuracyConverter = new accuracyConverter();
                Binding binding = new Binding("total");
                binding.Converter = _accuracyConverter;
                col_total.Binding = binding;


                dp_invoiceTypeSearch.Visibility = Visibility.Collapsed;
                dp_timeSearch.Visibility = Visibility.Collapsed;
                rd_gridDirectItems2.Height = new GridLength(1, GridUnitType.Auto);
                Grid.SetRow(cb_directItemsBranches, 0);
                Grid.SetRow(chk_directAllBranches, 0);
                Grid.SetRow(dp_dateSearch, 0);


                //fillComboPos();
                await SectionData.fillBranchesWithoutMain(cb_directItemsBranches);
                changeTabReset();

                // key_up search Person name
                cb_directItemsBranches.IsTextSearchEnabled = false;
                cb_directItemsBranches.IsEditable = true;
                cb_directItemsBranches.StaysOpenOnEdit = true;
                cb_directItemsBranches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;

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


        private void hideSatacks()
        {
            stk_tagsBranches.Visibility = Visibility.Collapsed;
            stk_tagsItems.Visibility = Visibility.Collapsed;
            stk_tagsPos.Visibility = Visibility.Collapsed;
            
        }
        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_branch.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_item.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_branch.Fill = Brushes.White;
            path_item.Fill = Brushes.White;
             


            col_branch.Visibility = Visibility.Collapsed;
            col_item.Visibility = Visibility.Collapsed;
        }

        private void hideAllColumn()
        {
            col_Num.Visibility = Visibility.Hidden;
            col_date.Visibility = Visibility.Hidden;
            col_Num1.Visibility = Visibility.Hidden;
            col_date1.Visibility = Visibility.Hidden;
            col_type.Visibility = Visibility.Hidden;
            col_item.Visibility = Visibility.Hidden;
            col_unit.Visibility = Visibility.Hidden;
            col_branch.Visibility = Visibility.Hidden;
            col_agentType.Visibility = Visibility.Hidden;
            col_agent.Visibility = Visibility.Hidden;
            col_invType.Visibility = Visibility.Hidden;
            col_invNumber.Visibility = Visibility.Hidden;
            col_itemUnits.Visibility = Visibility.Hidden;
            col_quantity.Visibility = Visibility.Hidden;
            col_price.Visibility = Visibility.Hidden;
            col_total.Visibility = Visibility.Hidden;
            col_agentTypeAgent.Visibility = Visibility.Hidden;
            col_invTypeNumber.Visibility = Visibility.Hidden;
        }
        private void changeTabReset()
        {
            cb_directItemsBranches.SelectedItem = null;

            rad_draft.IsChecked = false;
            rad_invoice.IsChecked = true;
            dp_directEndDate.SelectedDate = null;
            dp_directStartDate.SelectedDate = null;
            dt_startTime.SelectedTime = null;
            dt_endTime.SelectedTime = null;
            chk_directAllBranches.IsChecked = true;
            cb_directItemsBranches.IsEnabled = false;
            //isClickedAllBranches = false;
            Chk_directAllBranches_Checked(chk_directAllBranches, null);
        }
        private void selectionChangedCall(object sender)
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
        #endregion



        #region events
        private void Cb_directItemsBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directItems.IsEnabled = true;
                chk_directAllItems.IsEnabled = true;
                chk_directAllItems.IsChecked = true;

                fillComboDirectItems();
                if (cb_directItemsBranches.SelectedItem != null)
                {
                    chk_directAllItems.IsChecked = true;
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
        private void Cb_directItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directUnits.IsEnabled = true;
                chk_directAllUnits.IsEnabled = true;

                fillComboDirectUnits();
                if (cb_directItems.SelectedItem != null)
                {
                    chk_directAllUnits.IsChecked = true;
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
        private void Chk_directAllBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directItemsBranches.IsEnabled = false;
                cb_directItemsBranches.SelectedItem = null;

                //cb_directItems.IsEnabled = true;
                chk_directAllItems.IsEnabled = true;
                chk_directAllItems.IsChecked = true;
                chk_directAllUnits.IsEnabled = true;
                cb_directUnits.IsEnabled = false;
                fillComboDirectItems();
                cb_directItemsBranches.Text = "";
                cb_directItemsBranches.ItemsSource = SectionData.BranchesAllWithoutMainList;
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
        private void Chk_directAllBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directItemsBranches.IsEnabled = true;
                cb_directItems.IsEnabled = false;
                cb_directItems.SelectedItem = null;
                chk_directAllItems.IsEnabled = false;
                chk_directAllItems.IsChecked = false;
                //unit
             
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
        private void Chk_directAllItems_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directItems.IsEnabled = false;
                cb_directItems.SelectedItem = null;
                cb_directUnits.IsEnabled = false;
                chk_directAllUnits.IsEnabled = true;

                chk_directAllUnits.IsChecked = true;

                fillComboDirectUnits();

                cb_directItems.Text = "";
                cb_directItems.ItemsSource = comboDirectItems;
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
        private void Chk_directAllItems_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directItems.IsEnabled = true;
                cb_directUnits.IsEnabled = false;
                cb_directUnits.SelectedItem = null;
                chk_directAllUnits.IsEnabled = false;
                chk_directAllUnits.IsChecked = false;
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
        private void Chk_directAllUnits_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directUnits.IsEnabled = false;
                cb_directUnits.SelectedItem = null;
                cb_directUnits.Text = "";
                cb_directUnits.ItemsSource = units;
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
        private void Chk_directAllUnits_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_directUnits.IsEnabled = true;
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
        private void Dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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
        Invoice invoice;
        private async void DgDirect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                invoice = new Invoice();
                if (dgDirect.SelectedIndex != -1)
                {
                    ItemTransferInvoice item = dgDirect.SelectedItem as ItemTransferInvoice;
                    if (item.invoiceId > 0)
                    {
                        invoice = await invoice.GetByInvoiceId(item.invoiceId);
                        MainWindow.mainWindow.BTN_storage_Click(MainWindow.mainWindow.btn_storage, null);
                        View.uc_storage.Instance.UserControl_Loaded(null, null);
                        View.uc_storage.Instance.Btn_receiptOfPurchaseInvoice_Click(View.uc_storage.Instance.btn_reciptOfInvoice, null);
                        uc_receiptOfPurchaseInvoice.Instance.UserControl_Loaded(null, null);
                        uc_receiptOfPurchaseInvoice._InvoiceType = invoice.invType;
                        uc_receiptOfPurchaseInvoice.Instance.invoice = invoice;
                        uc_receiptOfPurchaseInvoice.isFromReport = true;
                        await uc_receiptOfPurchaseInvoice.Instance.fillInvoiceInputs(invoice);
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
        private void Cb_directUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        private void Txt_search_SelectionChanged(object sender, RoutedEventArgs e)
        {//search
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);

                temp = temp
                              .Where(s => (s.invNumber.ToString().ToLower().Contains(txt_search.Text.ToLower()) ||
                                           s.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                           s.itemName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                           s.unitName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                           s.quantity.ToString().ToLower().Contains(txt_search.Text.ToLower())
                                     ));

                dgDirect.ItemsSource = temp;
                txt_count.Text = temp.Count().ToString();


                decimal total = 0;
                if (selectedTab == 0)
                {
                    if (rad_draft.IsChecked.Value == true && (rad_invoice.IsChecked.Value == true))
                        total = temp.Where(i => i.invType != "isd").Select(b => b.totalNet.Value).Sum();
                    else
                        total = temp.Select(b => b.totalNet.Value).Sum();
                    tb_total.Text = SectionData.DecTostring(total);
                }
                else if (selectedTab == 1)
                {
                    if (rad_draft.IsChecked.Value == true && (rad_invoice.IsChecked.Value == true))
                        total = temp.Where(i => i.invType != "isd").Select(b => b.total.Value).Sum();
                    else
                        total = temp.Select(b => b.total.Value).Sum();
                    tb_total.Text = SectionData.DecTostring(total);
                }

                fillDirectColumnChart();
                fillDirectPieChart();

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
                itemsTransfer = await statisticModel.GetDirectInMov((int)MainWindow.branchID, (int)MainWindow.userID);

                txt_search.Text = "";
                searchText = "";
                chk_directAllBranches.IsChecked = true;
                chk_directAllItems.IsChecked = true;
                chk_directAllUnits.IsChecked = true;
                cb_directItemsBranches.SelectedItem = null;
                cb_directItems.SelectedItem = null;
                cb_directUnits.SelectedItem = null;

                dp_directStartDate.SelectedDate = null;
                dp_directEndDate.SelectedDate = null;

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

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!rad_invoice.IsChecked.Value && !rad_draft.IsChecked.Value )
                rad_invoice.IsChecked = true;
            selectionChangedCall(sender);
        }
        private void Cb_directItemsBranches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = SectionData.BranchesAllWithoutMainList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_directItems_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = comboDirectItems.Where(p => p.ItemName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_directUnits_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = units.Where(p => p.UnitName.ToLower().Contains(tb.Text.ToLower())).ToList();
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
        /*
         trBranchHint
trItemHint
trUnitHint
trStartDateHint
trEndDateHint

         * */
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath = "";
            string firstTitle = "DirectEntry";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string branchval = "";
            string itemval = "";
            string unitval = "";
            string searchval = "";
            string invchk = "";
            string retchk = "";
            string drftchk = "";
            string startTime = "";
            string endTime = "";
            string invtype = "";
            List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    //invoice
                    secondTitle = "invoice";
                    addpath = @"\Reports\StatisticReport\Storage\Direct\Ar\ArInvoice.rdlc";
                }
                else
                {
                    //selectedTab == 1 item
                    secondTitle = "items";
                    addpath = @"\Reports\StatisticReport\Storage\Direct\Ar\ArItem.rdlc";

                }
            }
            else
            {
                if (selectedTab == 0)
                {
                    //invoice
                    secondTitle = "invoice";
                    addpath = @"\Reports\StatisticReport\Storage\Direct\En\Invoice.rdlc";
                }
                else
                {
                    //selectedTab == 1 item
                    secondTitle = "items";
                    addpath = @"\Reports\StatisticReport\Storage\Direct\En\Item.rdlc";
                }
            }
            //filter
            startDate = dp_directStartDate.SelectedDate != null ? SectionData.DateToString(dp_directStartDate.SelectedDate) : "";

            endDate = dp_directEndDate.SelectedDate != null ? SectionData.DateToString(dp_directEndDate.SelectedDate) : "";

            startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
            invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
            // retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";
            drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";
            invTypelist.Add(invchk);
            //  invTypelist.Add(retchk);
            invTypelist.Add(drftchk);
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
            branchval = cb_directItemsBranches.SelectedItem != null
       && (chk_directAllBranches.IsChecked == false || chk_directAllBranches.IsChecked == null)
       ? cb_directItemsBranches.Text : (chk_directAllBranches.IsChecked == true ? all : "");

            itemval = cb_directItems.SelectedItem != null
               && (chk_directAllItems.IsChecked == false || chk_directAllItems.IsChecked == null)
               && branchval != ""
               ? cb_directItems.Text : (chk_directAllItems.IsChecked == true && branchval != "" ? all : "");

            unitval = cb_directUnits.SelectedItem != null
              && (chk_directAllUnits.IsChecked == false || chk_directAllUnits.IsChecked == null)
            && itemval != ""
              ? cb_directUnits.Text : (chk_directAllUnits.IsChecked == true && itemval != "" ? all : "");

            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            paramarr.Add(new ReportParameter("invtype", invtype));
            paramarr.Add(new ReportParameter("StartTimeVal", startTime));
            paramarr.Add(new ReportParameter("EndTimeVal", endTime));
            paramarr.Add(new ReportParameter("trStartTime", MainWindow.resourcemanagerreport.GetString("trStartTime")));
            paramarr.Add(new ReportParameter("trEndTime", MainWindow.resourcemanagerreport.GetString("trEndTime")));

            //end filter
            paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trUnitHint", MainWindow.resourcemanagerreport.GetString("trUnit")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("BranchVal", branchval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));
            paramarr.Add(new ReportParameter("UnitVal", unitval));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            //
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            clsReports.itemTransferInvoiceDirect(temp, rep, reppath, paramarr);
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
