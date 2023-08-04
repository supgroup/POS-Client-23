using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for uc_serialReport.xaml
    /// </summary>
    public partial class uc_serialReport : UserControl
    {
        #region variabls
        IEnumerable<SerialSts> serials;
        Statistics statisticsModel = new Statistics();
        IEnumerable<SerialSts> serialsQuery;
        string searchText = "";
        int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion

        public uc_serialReport()
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
        private static uc_serialReport _instance;

        public static uc_serialReport Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_serialReport();
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
                // key_up branch
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                // key_up itemunit
                cb_itemUnit.IsTextSearchEnabled = false;
                cb_itemUnit.IsEditable = true;
                cb_itemUnit.StaysOpenOnEdit = true;
                cb_itemUnit.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_itemUnit.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                await RefreshSerialsList();

                chk_allItemUnit.IsChecked = true;
                chk_allBranches.IsChecked = true;
                chk_isSold.IsChecked    = false;
                chk_isNotSold.IsChecked = true;

                await Search();


                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), this.Tag.ToString());

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
        async Task<IEnumerable<SerialSts>> RefreshSerialsList()
        {
            //if (!isFromLoadedEvent)
            serials = await statisticsModel.GetSerials(MainWindow.branchID.Value, MainWindow.userID.Value);
            foreach(var s in serials)
            {
                s.itemName = s.itemName + "-" + s.unitName;
            }

            fillBranches();
            fillItemsUnit();

            return serials;
        }
        async Task Search()
        {
            //if (!isFromLoadedEvent)
            {
                if (serials is null)
                    await RefreshSerialsList();

                searchText = txt_search.Text.ToLower();
                serialsQuery = serials
                    .Where(s =>
                (
                s.serialNum.ToLower().Contains(searchText)
                )
                &&
                (//sold
                    (chk_isSold.IsChecked == true    ? s.isSold == true && s.isActive == 1: false)
                    ||
                    (chk_isNotSold.IsChecked == true ? s.isSold == false : false)
                )
                &&
                //branchID
                (
                chk_allBranches.IsChecked.Value ?
                    true :
                    (cb_branches.SelectedIndex != -1 ? s.branchId == Convert.ToInt32(cb_branches.SelectedValue) : false)
                )
                &&
                //itemunitID
                (
                chk_allItemUnit.IsChecked.Value ?
                    true :
                    (cb_itemUnit.SelectedIndex != -1 ? s.itemUnitId == Convert.ToInt32(cb_itemUnit.SelectedValue) : false)
                )
                &&
                //start date
                (dp_startDate.SelectedDate != null   ? s.updateDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
                &&
                //end date
                (dp_endDate.SelectedDate != null     ? s.updateDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
                );

                RefreshSerialsView();
                fillColumnChart();
                fillPieChart();
                fillRowChart();
            }
        }
        void RefreshSerialsView()
        {
            dg_serial.ItemsSource = serialsQuery;
            txt_count.Text = dg_serial.Items.Count.ToString();
        }
        private void translate()
        {
            tt_serial.Content = MainWindow.resourcemanager.GetString("trSerials");

            chk_isNotSold.Content = MainWindow.resourcemanager.GetString("trAvailable");
            chk_isSold.Content = MainWindow.resourcemanager.GetString("trSold");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch")+"/"+ MainWindow.resourcemanager.GetString("trStore")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemUnit, MainWindow.resourcemanager.GetString("trItem")+ " - " + MainWindow.resourcemanager.GetString("trUnit")+"...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDate") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDate") + "...");

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItemUnit.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_No.Header = MainWindow.resourcemanager.GetString("trSerialNum");
            col_InvNo.Header = MainWindow.resourcemanager.GetString("trInvoiceCharp");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch") + "/" + MainWindow.resourcemanager.GetString("trStore");
            col_itemunit.Header = MainWindow.resourcemanager.GetString("trItem")+" - "+ MainWindow.resourcemanager.GetString("trUnit");
            col_available.Header = MainWindow.resourcemanager.GetString("trStatus");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");

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
        List<Branch> branches = new List<Branch>();
        private void fillBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            branches = serials.GroupBy(i => i.branchId).Select(i => new Branch { name = i.FirstOrDefault().branchName, branchId = i.FirstOrDefault().branchId.Value }).ToList();
            cb_branches.ItemsSource = branches;
        }
        List<ItemUnit> itemunits = new List<ItemUnit>();
        private void fillItemsUnit()
        {
            cb_itemUnit.SelectedValuePath = "itemUnitId";
            cb_itemUnit.DisplayMemberPath = "itemName";
            itemunits = serials.GroupBy(i => i.itemUnitId).Select(i => new ItemUnit { itemName = i.FirstOrDefault().itemName, itemUnitId = i.FirstOrDefault().itemUnitId.Value }).ToList();
            cb_itemUnit.ItemsSource = itemunits;
        }
        #endregion

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
        private async void RefreshViewCheckbox(object sender, RoutedEventArgs e)
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
        private async void RefreshView_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {//select date
            try
            {
                //if (prevSelectedDate != dp_invoiceDate.SelectedDate.Value)
                //{
                //if (sender != null && !(isFromLoadedEvent))
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //    //if (e.OriginalSource!= e.Source)

                //    prevSelectedDate = dp_invoiceDate.SelectedDate.Value;
                await RefreshSerialsList();
                await Search();
                fillBranches();
                fillItemsUnit();

                //if (sender != null && !(isFromLoadedEvent))
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               //}
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Cb_itemUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select itemunit
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
        private async void Chk_allItemUnit_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemUnit.SelectedIndex = -1;
                cb_itemUnit.IsEnabled = false;
                cb_itemUnit.Text = "";
                cb_itemUnit.ItemsSource = itemunits;

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
        private async void Chk_allItemUnit_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemUnit.IsEnabled = true;
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
        private async void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.Text = "";
                cb_branches.IsEnabled = false;
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

                await RefreshSerialsList();
                txt_search.Text = "";
                searchText = "";
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
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
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_itemUnit_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = itemunits.Where(p => p.itemName.ToLower().Contains(tb.Text.ToLower())).ToList();
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

            string startDate = "";
            string endDate = "";
            string branchval = "";
            string itemval = "";

            string searchval = "";
            string Available = "";
            string Sold = "";
            string notSold = "";
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Storage\Serial\Ar\ArSerial.rdlc";


            }
            else
            {
                addpath = @"\Reports\StatisticReport\Storage\Serial\En\Serial.rdlc";

            }
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";

            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";

            branchval = cb_branches.SelectedItem != null
       && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
       ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");

            itemval = cb_itemUnit.SelectedItem != null
               && (chk_allItemUnit.IsChecked == false || chk_allItemUnit.IsChecked == null)
               && branchval != ""
               ? cb_itemUnit.Text : (chk_allItemUnit.IsChecked == true && branchval != "" ? all : "");

            //available sold
            /*
             *      chk_isNotSold.Content = MainWindow.resourcemanager.GetString("trAvailable");
            chk_isSold.Content = MainWindow.resourcemanager.GetString("trSold");
             * */
            // secondTitle = "destroied";
            //  subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + MainWindow.resourcemanagerreport.GetString("trSerials");
            paramarr.Add(new ReportParameter("trTitle", Title));


            paramarr.Add(new ReportParameter("trItemHint", MainWindow.resourcemanagerreport.GetString("trItemUnit")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch/Store")));
            paramarr.Add(new ReportParameter("BranchVal", branchval));
            paramarr.Add(new ReportParameter("ItemVal", itemval));

            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDateHint", MainWindow.resourcemanagerreport.GetString("trStartDate")));

            paramarr.Add(new ReportParameter("trEndDateHint", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            Sold = chk_isSold.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trSold") : "";
            notSold = chk_isNotSold.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trAvailable") : "";
            
            Sold = Sold + " , " + notSold;
            Sold = Sold.IndexOf(',') == 1 || Sold.IndexOf(',') == Sold.Length - 2 ? Sold.Replace(",", "") : Sold;
            paramarr.Add(new ReportParameter("soldval", Sold));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));



            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            clsReports.SerialReportSTS(serialsQuery, rep, reppath, paramarr);
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
            int endYear = DateTime.Now.Year;
            int startYear = endYear - 1;
            int startMonth = DateTime.Now.Month;
            int endMonth = startMonth;
            if (dp_startDate.SelectedDate != null && dp_endDate.SelectedDate != null)
            {
                startYear = dp_startDate.SelectedDate.Value.Year;
                endYear = dp_startDate.SelectedDate.Value.Year;
                startMonth = dp_startDate.SelectedDate.Value.Month;
                endMonth = dp_endDate.SelectedDate.Value.Month;
            }


            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();

            SeriesCollection rowChartData = new SeriesCollection();
            
            var tempName = serialsQuery.GroupBy(s => new { s.itemUnitId }).Select(s => new
            {
                Name = s.FirstOrDefault().updateDate,
            });
            names.AddRange(tempName.Select(nn => nn.Name.ToString()));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> available = new List<int>();
            List<int> sold = new List<int>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        var drawAvailable = serialsQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.isSold == false).Count();
                        var drawSold      = serialsQuery.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.isSold == true).Count();

                        available.Add(drawAvailable);
                        sold.Add(drawSold);

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
                    var drawAvailable = serialsQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.isSold == false).Count();
                    var drawSold      = serialsQuery.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.isSold == true).Count();

                    available.Add(drawAvailable);
                    sold.Add(drawSold);
                    MyAxis.Labels.Add(year.ToString());
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = available.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("trAvailable")
          }); ;
            rowChartData.Add(
         new LineSeries
         {
             Values = sold.AsChartValues(),
             Title = MainWindow.resourcemanager.GetString("trSold")
         });
           
            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;

            var result = serials.Where(s => s.isSold==false).GroupBy(s => s.branchId).Select(s => new
            {
                branchId = s.Key,
                branchName = s.FirstOrDefault().branchName ,
                count = s.Count()
            });
            x = result.Select(m => m.count);
            titles.AddRange(result.Select(jj => jj.branchName));

            SeriesCollection piechartData = new SeriesCollection();
            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();
            for (int i = 0; i < xCount; i++)
            {
                List<int> final = new List<int>();

                final.Add(x.ToList().Skip(i).FirstOrDefault());
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final.AsChartValues(),
                      Title = titles.Skip(i).FirstOrDefault(),
                      DataLabels = true,
                  }
              );
            }
            if (x.Count() > 6)
            {
                int finalSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    finalSum = finalSum + x.ToList().Skip(i).FirstOrDefault();
                }
                if (finalSum != 0)
                {
                    List<int> final = new List<int>();

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
            }
            chart1.Series = piechartData;
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<SerialSts> resultList = new List<SerialSts>();
            IEnumerable<int> x = null;
            IEnumerable<int> y = null;

            var result = serialsQuery.GroupBy(s => s.itemUnitId).Select(s => new
            {
                itemUnitId = s.Key,
                itemName = s.FirstOrDefault().itemName,
                countP  = s.Where(m => m.isSold == false).Count(),
                countPb = s.Where(m => m.isSold == true).Count(),
            });
            x = result.Select(m => m.countP);
            y = result.Select(m => m.countPb);
           
            names.AddRange(result.Select(nn => nn.itemName));

            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("trAvailable"),
                MainWindow.resourcemanager.GetString("trSold"),
            };

            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();

            for (int i = 0; i < xCount; i++)
            {
                cP.Add(x.ToList().Skip(i).FirstOrDefault());
                cPb.Add(y.ToList().Skip(i).FirstOrDefault());

                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (x.Count() > 6)
            {
                int cPSum = 0, cPbSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    cPbSum = cPSum + x.ToList().Skip(i).FirstOrDefault();
                    cPbSum = cPbSum + y.ToList().Skip(i).FirstOrDefault();
                }
                if (!((cPbSum == 0) && (cPbSum == 0)))
                {
                    cP.Add(cPSum);
                    cPb.Add(cPbSum);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
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
               Values = cPb.AsChartValues(),
               Title = titles[1],
               DataLabels = true,
           });

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
