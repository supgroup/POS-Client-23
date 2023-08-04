using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
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
using POS.View.windows;
namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_propertiesReport.xaml
    /// </summary>
    public partial class uc_propertiesReport : UserControl
    {
        #region variabls
        IEnumerable<StorePropertySts> properties;
        Statistics statisticsModel = new Statistics();
        IEnumerable<StorePropertySts> propertiesQuery;
        IEnumerable<StorePropertySts> propertiesQueryForCharts;
        string searchText = "";
        int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        bool showChart = true;
        double col_reportChartWidth = 0;
        #endregion

        public uc_propertiesReport()
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

        private static uc_propertiesReport _instance;
        public static uc_propertiesReport Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_propertiesReport();
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

                await RefreshPropertiesList();

                chk_allItemUnit.IsChecked = true;
                chk_allBranches.IsChecked = true;
                //chk_isSold.IsChecked = false;
                //chk_isNotSold.IsChecked = true;

                await Search();

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

                Btn_available_Click(btn_available, null);

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
        private void paint()
        {
            bdr_Available.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_sold.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_available.Fill = Brushes.White;
            path_sold.Fill = Brushes.White;
        }
        async Task Search()
        {
            if (properties is null)
                await RefreshPropertiesList();

            searchText = txt_search.Text.ToLower();
            propertiesQueryForCharts = properties
                .Where(s =>
            (
            s.StorePropertiesValueString.ToLower().Contains(searchText)
            ||
            (s.invNumber != null ? s.invNumber.ToLower().Contains(searchText) : false)
            )
            //&&
            //(//sold
            //    (chk_isSold.IsChecked == true ? s.isSold == true && s.isActive == 1: false)
            //    ||
            //    (chk_isNotSold.IsChecked == true ? s.isSold == false : false)
            //)
            &&
            (//sold
               (s.itemsTransId == null && s.isSold == false && s.isActive == 1) || (s.isSold == true && s.isActive == 1)
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
            (dp_startDate.SelectedDate != null ? s.updateDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
            &&
            //end date
            (dp_endDate.SelectedDate != null ? s.updateDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
            );

            propertiesQuery = propertiesQueryForCharts.Where(p =>
            (
            (
             selectedTab == 0 //available
                ?
                (p.itemsTransId == null && p.isSold == false && p.isActive == 1)
                : false
            )
            ||
            (
             selectedTab == 1 //sold
                ?
                (p.itemsTransId != null && p.isSold == true && p.isActive == 1)
                : false
            )
            )
            );
            RefreshPropertiesView();
            fillColumnChart();
            fillPieChart();
            fillRowChart();
        }
        List<ItemUnit> itemUnits = new List<ItemUnit>();
        private void fillItemsUnit()
        {
            cb_itemUnit.SelectedValuePath = "itemUnitId";
            cb_itemUnit.DisplayMemberPath = "itemName";
            itemUnits = properties.GroupBy(i => i.itemUnitId).Select(i => new ItemUnit { itemName = i.FirstOrDefault().itemName, itemUnitId = i.FirstOrDefault().itemUnitId.Value }).Distinct().ToList();
            cb_itemUnit.ItemsSource = itemUnits;
        }
        List<Branch> branches = new List<Branch>();
        private void fillBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            branches = properties.GroupBy(i => i.branchId).Select(i => new Branch { name = i.FirstOrDefault().branchName, branchId = i.FirstOrDefault().branchId.Value }).Distinct().ToList();
            cb_branches.ItemsSource = branches;
        }
        async Task<IEnumerable<StorePropertySts>> RefreshPropertiesList()
        {
            properties = await statisticsModel.GetProperties(MainWindow.branchID.Value, MainWindow.userID.Value);
            foreach (var p in properties)
            {
                p.itemName = p.itemName + "-" + p.unitName;
                //p.propName = p.propName + "-" + p.propValue;
            }

            fillBranches();
            fillItemsUnit();

            return properties;
        }
        void RefreshPropertiesView()
        {
            dg_property.ItemsSource = propertiesQuery;
            txt_count.Text = propertiesQuery.Count().ToString();
        }
        private void translate()
        {
            tt_available.Content = MainWindow.resourcemanager.GetString("trAvailable");
            tt_sold.Content = MainWindow.resourcemanager.GetString("trSold");

            //chk_isNotSold.Content = MainWindow.resourcemanager.GetString("trAvailable");
            //chk_isSold.Content = MainWindow.resourcemanager.GetString("trSold");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch") + "/" + MainWindow.resourcemanager.GetString("trStore") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_itemUnit, MainWindow.resourcemanager.GetString("trItem") + "-" + MainWindow.resourcemanager.GetString("trUnit") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDate") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDate") + "...");

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allItemUnit.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_No.Header = MainWindow.resourcemanager.GetString("trProperties");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch") + "/" + MainWindow.resourcemanager.GetString("trStore");
            col_itemunit.Header = MainWindow.resourcemanager.GetString("trItem") + "-" + MainWindow.resourcemanager.GetString("trUnit");
            col_available.Header = MainWindow.resourcemanager.GetString("trStatus");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_InvNo.Header = MainWindow.resourcemanager.GetString("trInvoiceCharp");
            col_count.Header = MainWindow.resourcemanager.GetString("trQTR");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_charts.Content = MainWindow.resourcemanager.GetString("showHideCharts");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print3.Content = MainWindow.resourcemanager.GetString("trPrint");
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
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await RefreshPropertiesList();
                await Search();
                fillBranches();
                fillItemsUnit();

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

                await RefreshPropertiesList();
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
        private async void Chk_allItemUnit_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_itemUnit.SelectedIndex = -1;
                cb_itemUnit.IsEnabled = false;
                cb_itemUnit.Text = "";
                cb_itemUnit.ItemsSource = itemUnits;

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
        private void Cb_itemUnit_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = itemUnits.Where(p => p.itemName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
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
            string secondTitle = "";
            //string subTitle = "";
            string Title = "";

            string startDate = "";
            string endDate = "";
            string branchval = "";
            string itemval = "";

            string searchval = "";
       
        
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Property\Ar\ArAvailable.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trAvailable");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Property\Ar\ArSold.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trSold");
                    startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";

                    endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";

                }

            }
                else
                {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Property\En\Available.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trAvailable");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Storage\Property\En\Sold.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trSold");
                    startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";

                    endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";

                }
            }              
                branchval = cb_branches.SelectedItem != null
           && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
           ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");

                itemval = cb_itemUnit.SelectedItem != null
                   && (chk_allItemUnit.IsChecked == false || chk_allItemUnit.IsChecked == null)
                   && branchval != ""
                   ? cb_itemUnit.Text : (chk_allItemUnit.IsChecked == true && branchval != "" ? all : "");
                Title = MainWindow.resourcemanagerreport.GetString("trStorageReport") + " / " + MainWindow.resourcemanagerreport.GetString("trProperties")
                + " / " + secondTitle;
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
           
                searchval = txt_search.Text;
                paramarr.Add(new ReportParameter("searchVal", searchval));
                string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

                ReportCls.checkLang();

                clsReports.PropertyReportSTS(propertiesQuery, rep, reppath, paramarr);
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

            SeriesCollection rowChartData = new SeriesCollection();

            var tempName = propertiesQueryForCharts.GroupBy(s => new { s.itemUnitId }).Select(s => new
            {
                Name = s.FirstOrDefault().updateDate,
            });
            names.AddRange(tempName.Select(nn => nn.Name.ToString()));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<long> available = new List<long>();
            List<long> sold = new List<long>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        var drawAvailable = propertiesQueryForCharts.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.itemsTransId == null && c.isSold == false && c.isActive == 1).Sum(m => m.count);
                        var drawSold = propertiesQueryForCharts.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.itemsTransId != null && c.isSold == true && c.isActive == 1).Sum(m => m.count);

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
                    var drawAvailable = propertiesQueryForCharts.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.itemsTransId == null && c.isSold == false && c.isActive == 1).Sum(m => m.count);
                    var drawSold = propertiesQueryForCharts.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.itemsTransId != null && c.isSold == true && c.isActive == 1).Sum(m => m.count);

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
            IEnumerable<long> x = null;

            var result = properties.Where(s => s.itemsTransId == null && s.isSold == false && s.isActive == 1).GroupBy(s => s.branchId).Select(s => new
            {
                branchId = s.Key,
                branchName = s.FirstOrDefault().branchName,
                count = s.Sum(m => m.count)
            });
            x = result.Select(m => m.count);
            titles.AddRange(result.Select(jj => jj.branchName));

            SeriesCollection piechartData = new SeriesCollection();
            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();
            for (int i = 0; i < xCount; i++)
            {
                List<long> final = new List<long>();

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
                long finalSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    finalSum = finalSum + x.ToList().Skip(i).FirstOrDefault();
                }
                if (finalSum != 0)
                {
                    List<long> final = new List<long>();

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
            IEnumerable<long> x = null;
            IEnumerable<long> y = null;

            var result = propertiesQueryForCharts.GroupBy(s => s.itemUnitId).Select(s => new
            {
                itemUnitId = s.Key,
                itemName = s.FirstOrDefault().itemName,
                countP = s.Where(m => m.itemsTransId == null && m.isSold == false && m.isActive == 1).Sum(m => m.count),
                countPb = s.Where(m => m.itemsTransId != null && m.isSold == true && m.isActive == 1).Sum(m => m.count),
            });
            x = result.Select(m => m.countP);
            y = result.Select(m => m.countPb);

            names.AddRange(result.Select(nn => nn.itemName));

            SeriesCollection columnChartData = new SeriesCollection();
            List<long> cP = new List<long>();
            List<long> cPb = new List<long>();
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
                long cPSum = 0, cPbSum = 0;
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

        #region tabs
        private async void Btn_available_Click(object sender, RoutedEventArgs e)
        {//available
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 0;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_Available);
                path_available.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                chk_allBranches.IsChecked = true;
                chk_allItemUnit.IsChecked = true;

                dp_startDate.Visibility = Visibility.Collapsed;
                dp_endDate.Visibility = Visibility.Collapsed;
                col_updateDate.Visibility = Visibility.Hidden;
                col_count.Visibility = Visibility.Visible;
                col_InvNo.Visibility = Visibility.Hidden;

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
        private async void Btn_sold_Click(object sender, RoutedEventArgs e)
        {//sold
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                selectedTab = 1;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_sold);
                path_sold.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                chk_allBranches.IsChecked = true;
                chk_allItemUnit.IsChecked = true;

                dp_startDate.Visibility = Visibility.Visible;
                dp_endDate.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;
                col_InvNo.Visibility = Visibility.Visible;

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
