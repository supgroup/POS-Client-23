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

namespace POS.View.reports.deliveryReports
{
    /// <summary>
    /// Interaction logic for uc_deliveryReports.xaml
    /// </summary>
    public partial class uc_deliveryReports : UserControl
    {
        #region variables
        IEnumerable<Invoice> deliveries;
        Statistics statisticsModel = new Statistics();
        IEnumerable<Invoice> deliveriesQuery;

        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        string searchText = "";
        #endregion

        private static uc_deliveryReports _instance;
        public static uc_deliveryReports Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new uc_deliveryReports();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public uc_deliveryReports()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
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
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                else
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                translate();
                #endregion

                await RefreshDeliveriesList();

                col_reportChartWidth = col_reportChart.ActualWidth;

                #region key up
                // key_up branch
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                // key_up itemunit
                cb_company.IsTextSearchEnabled = false;
                cb_company.IsEditable = true;
                cb_company.StaysOpenOnEdit = true;
                cb_company.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_company.Text = "";
                #endregion

                Btn_preparingOrders_Click(btn_preparingOrders, null);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_preparingOrders.Tag.ToString());
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
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trShippingCompany") + "...");

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allCompanies.Content = MainWindow.resourcemanager.GetString("trAll");

            tt_delivery.Content = MainWindow.resourcemanager.GetString("trShippingCompanies");
            tt_driver.Content = MainWindow.resourcemanager.GetString("trDrivers");
            tt_customer.Content = MainWindow.resourcemanager.GetString("trCustomers");

            //col_orderNum.Header = MainWindow.resourcemanager.GetString("trNo.");
            col_invNum.Header = MainWindow.resourcemanager.GetString("trInvoiceCharp");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_customer.Header = MainWindow.resourcemanager.GetString("trCustomer");
            col_company.Header = MainWindow.resourcemanager.GetString("trCompany");
            col_driver.Header = MainWindow.resourcemanager.GetString("trDriver");
            col_isFree.Header = MainWindow.resourcemanager.GetString("trStatus");
            //col_duration.Header = MainWindow.resourcemanager.GetString("duration");

            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print3.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        async Task Search()
        {
            if (deliveries is null)
                await RefreshDeliveriesList();

            searchText = txt_search.Text.ToLower();
            deliveriesQuery = deliveries
                .Where(s =>
            (
            s.invNumber.ToLower().Contains(searchText)
            ||
            s.branchCreatorName.ToLower().Contains(searchText)
            ||
            s.shipCompanyName.ToLower().Contains(searchText)
            ||
            s.agentName.ToLower().Contains(searchText)
            ||
            (s.shipUserName != null ? s.shipUserName.ToLower().Contains(searchText) : false)
            )
            &&
            //branch
            (cb_branches.SelectedIndex != -1 ? s.branchCreatorId == Convert.ToInt32(cb_branches.SelectedValue) : true)
            &&
            //company
            //(cb_company.SelectedIndex != -1 ?  s.shippingCompanyId == Convert.ToInt32(cb_company.SelectedValue) : true)
            (cb_company.SelectedIndex != -1 ? selectedTab == 0 ? s.shippingCompanyId == Convert.ToInt32(cb_company.SelectedValue) :
                                              selectedTab == 1 ? s.shipUserId == Convert.ToInt32(cb_company.SelectedValue) :
                                              s.agentId == Convert.ToInt32(cb_company.SelectedValue)
                                              : true)
            &&
            //start date
            (dp_startDate.SelectedDate != null ? s.invDate.Value.Date >= dp_startDate.SelectedDate.Value.Date : true)
            &&
            //end date
            (dp_endDate.SelectedDate != null ? s.invDate.Value.Date <= dp_endDate.SelectedDate.Value.Date : true)
            );

            deliveriesQuery = deliveriesQuery.Where(p =>
           (
           (
            selectedTab == 0 //company
               ?
               (p.shippingCompanyId != null && p.shipUserId == null)
               : false
           )
           ||
           (
            selectedTab == 1 //driver
               ?
               (p.shipUserId != null)
               : false
           )
           ||
           (
            selectedTab == 2 //customer
               ?
               (p.agentId != null)
               : false
           )
           )
           );

            RefreshDeliveriesView();

            fillColumnChart();
            fillPieChart();
            fillRowChart();

        }
        private void SearchEmpty()
        {
            searchText = txt_search.Text.ToLower();
            deliveriesQuery = new List<Invoice>();

            RefreshDeliveriesView();

            fillColumnChart();
            fillPieChart();
            fillRowChart();

        }
        void RefreshDeliveriesView()
        {
            dg_delivery.ItemsSource = deliveriesQuery;
            txt_count.Text = deliveriesQuery.Count().ToString();
        }
        async Task<IEnumerable<Invoice>> RefreshDeliveriesList()
        {
            deliveries = await statisticsModel.GetDelivery(MainWindow.loginBranch.branchId, MainWindow.userLogin.userId);
            fillBranches();
            return deliveries;
        }
        private async void callSearch(object sender)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                await Search();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        List<Branch> branches = new List<Branch>();
        private void fillBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            branches = deliveries.GroupBy(i => i.branchCreatorId).Select(i => new Branch { name = i.FirstOrDefault().branchCreatorName, branchId = i.FirstOrDefault().branchCreatorId.Value }).ToList();
            cb_branches.ItemsSource = branches;
        }
        List<ShippingCompanies> companies = new List<ShippingCompanies>();
        private void fillCompanies()
        {
            cb_company.SelectedValuePath = "shippingCompanyId";
            cb_company.DisplayMemberPath = "name";
            companies = deliveries.Where(i => i.shipCompanyName != "Localy").Select(i => new ShippingCompanies { name = i.shipCompanyName, shippingCompanyId = i.shippingCompanyId.Value }).Distinct().ToList();
            cb_company.ItemsSource = companies;
        }
        List<User> drivers = new List<User>();
        private void fillDrivers()
        {
            cb_company.SelectedValuePath = "userId";
            cb_company.DisplayMemberPath = "fullName";
            drivers = deliveries.Where(i => i.shipUserId != null).Select(i => new User { fullName = i.shipUserName, userId = i.shipUserId.Value }).Distinct().ToList();
            cb_company.ItemsSource = drivers;
        }
        List<Agent> customers = new List<Agent>();
        private void fillCustomers()
        {
            cb_company.SelectedValuePath = "agentId";
            cb_company.DisplayMemberPath = "name";
            customers = deliveries.Select(i => new Agent { name = i.agentName, agentId = i.agentId.Value }).Distinct().ToList();
            cb_company.ItemsSource = customers;
        }
        private void paint()
        {
            bdr_delivery.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_drivers.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_customers.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_preparingOrder.Fill = Brushes.White;
            path_driver.Fill = Brushes.White;
            path_customer.Fill = Brushes.White;
        }
        #endregion

        #region events
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Instance = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void RefreshView_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            callSearch(sender);
        }
        private void cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch

            //    callSearch(sender);
            try
            {
                SectionData.StartAwait(grid_main);

                //  await Search();
                fillbyComboValue();
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.Text = "";

                cb_branches.IsEnabled = false;
                fillbyComboValue();
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Chk_allBranches_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                cb_branches.IsEnabled = true;

                SearchEmpty();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_company_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select company
            try
            {
                SectionData.StartAwait(grid_main);

                fillbyComboValue();
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void fillbyComboValue()
        {
            if ((cb_branches.SelectedItem == null && chk_allBranches.IsChecked == false)
                 || (cb_company.SelectedItem == null && chk_allCompanies.IsChecked == false))
            {
                SearchEmpty();
            }
            else
            {
                await Search();
            }
        }
        private async void Chk_allCompanies_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                cb_company.SelectedIndex = -1;
                cb_company.Text = "";
                cb_company.IsEnabled = false;
                fillbyComboValue();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Chk_allCompanies_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);

                cb_company.IsEnabled = true;

                SearchEmpty();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                SectionData.StartAwait(grid_main);

                searchText = "";
                txt_search.Text = "";
                await RefreshDeliveriesList();
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                chk_allBranches.IsChecked = true;
                chk_allCompanies.IsChecked = true;

                //    await Search();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            callSearch(sender);
        }
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = branches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch { }
        }
        private void Cb_company_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                if (selectedTab == 0)
                    combo.ItemsSource = companies.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
                else if (selectedTab == 1)
                    combo.ItemsSource = drivers.Where(p => p.fullName.ToLower().Contains(tb.Text.ToLower())).ToList();
                else if (selectedTab == 2)
                    combo.ItemsSource = customers.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch { }
        }
        #endregion

        #region charts
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<int> deliveriesCount = new List<int>();

            var result = deliveriesQuery.GroupBy(s => new { s.branchCreatorId, s.invNumber }).Select(s => new
            {
                Id = s.Key.branchCreatorId,
                inv = s.Key.invNumber,
            });

            var finalResult = result.GroupBy(s => s.Id).Select(s => new
            {
                quantity = s.Count()
            });
            var tempName = deliveriesQuery.GroupBy(s => s.branchCreatorName).Select(s => new
            {
                name = s.Key
            });
            names.AddRange(tempName.Select(nn => nn.name));

            deliveriesCount.AddRange(finalResult.Select(nn => nn.quantity));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cS = new List<int>();

            List<string> titles = new List<string>()
            {
               MainWindow.resourcemanager.GetString("trInvoice")
            };
            int x = 6;
            if (names.Count() <= 6) x = names.Count();

            for (int i = 0; i < x; i++)
            {
                cS.Add(deliveriesCount.ToList().Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (names.Count() > 6)
            {
                int deliverSum = 0;
                for (int i = 6; i < names.Count(); i++)
                    deliverSum = deliverSum + deliveriesCount.ToList().Skip(i).FirstOrDefault();

                if (deliverSum != 0)
                    cS.Add(deliverSum);

                axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
            }

            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cS.AsChartValues(),
                Title = titles[0],
                DataLabels = true,
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;
            titles.Clear();

            var result = deliveriesQuery.GroupBy(s => new { s.shippingCompanyId, s.invNumber }).Select(s => new
            {
                Id = s.Key.shippingCompanyId,
                inv = s.Key.invNumber,
            });

            var finalResult = result.GroupBy(s => s.Id).Select(s => new
            {
                quantity = s.Count()
            });
            //var tempName = deliveriesQuery.GroupBy(s => s.shippingCompanyName).Select(s => new
            var tempName = deliveriesQuery.GroupBy(s => s.shipCompanyName).Select(s => new
            {
                name = s.Key
            });
            titles.AddRange(tempName.Select(jj => jj.name.ToString()));

            x = finalResult.Select(m => m.quantity);

            SeriesCollection piechartData = new SeriesCollection();

            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();
            for (int i = 0; i < xCount; i++)
            {
                List<int> final = new List<int>();
                List<string> lable = new List<string>();
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
                int xSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    xSum = xSum + x.ToList().Skip(i).FirstOrDefault();
                }

                if (xSum > 0)
                {
                    List<int> final = new List<int>();
                    List<string> lable = new List<string>();
                    final.Add(xSum);
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
        private void fillRowChart()
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


            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();

            SeriesCollection rowChartData = new SeriesCollection();

            var tempName = deliveriesQuery.GroupBy(s => new { s.branchCreatorId, s.invNumber }).Select(s => new
            {
                Name = s.FirstOrDefault().invDate,
            });
            names.AddRange(tempName.Select(nn => nn.Name.ToString()));

            var result = deliveriesQuery.GroupBy(s => new { s.branchCreatorId, s.invNumber }).Select(s => new
            {
                Id = s.Key.branchCreatorId,
                inv = s.Key.invNumber,
                createDate = s.FirstOrDefault().invDate
            });

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> orderLst = new List<int>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        var drawQuantity = result.ToList().Where(c => c.createDate > firstOfThisMonth && c.createDate <= firstOfNextMonth).Select(s => s.inv).Count();
                        orderLst.Add(drawQuantity);
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
                    var drawQuantity = result.ToList().Where(c => c.createDate > firstOfThisYear && c.createDate <= firstOfNextMYear).Select(s => s.inv).Count();

                    orderLst.Add(drawQuantity);

                    MyAxis.Labels.Add(year.ToString());
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = orderLst.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("trBranch") + " / " + MainWindow.resourcemanager.GetString("trInvoice")
          });

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion

        #region reports
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath = "";
            //string firstTitle = "PreparingOrders";
            string secondTitle = "";
            //string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string branchVal = "";
            string companyVal = "";
            string searchval = "";
            string trCompany = "";
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Delivery\Ar\ArDelivery.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trShippingCompanies");
                    trCompany = MainWindow.resourcemanagerreport.GetString("trShippingCompanynohint");
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Delivery\Ar\ArDriver.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trDrivers") +
                          " / " + MainWindow.resourcemanagerreport.GetString("trLocaly");
                    trCompany = MainWindow.resourcemanagerreport.GetString("trDriver");

                }
                else if (selectedTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Delivery\Ar\ArCustomer.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trCustomers");
                    trCompany = MainWindow.resourcemanagerreport.GetString("trCustomer");
                }
            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Delivery\En\EnDelivery.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trShippingCompanies");
                    trCompany = MainWindow.resourcemanagerreport.GetString("trShippingCompanynohint");

                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Delivery\En\Driver.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trDrivers") +
                             " / " + MainWindow.resourcemanagerreport.GetString("trLocaly");
                    trCompany = MainWindow.resourcemanagerreport.GetString("trDriver");
                }
                else if (selectedTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Delivery\En\Customer.rdlc";
                    secondTitle = MainWindow.resourcemanagerreport.GetString("trCustomers");
                    trCompany = MainWindow.resourcemanagerreport.GetString("trCustomer");
                }
            }
            //filter
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";
            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";
            branchVal = cb_branches.SelectedItem != null
                && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
                ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");

            companyVal = cb_company.SelectedItem != null
             && (chk_allCompanies.IsChecked == false || chk_allCompanies.IsChecked == null)
             ? cb_company.Text : (chk_allCompanies.IsChecked == true ? all : "");
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("trCompanytab", trCompany));
            paramarr.Add(new ReportParameter("branchVal", branchVal));
            paramarr.Add(new ReportParameter("companyVal", companyVal));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            //secondTitle = "";
            // subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("deliveryReport") + " / " + secondTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            clsReports.DeliveryReport(deliveriesQuery.ToList(), rep, reppath, paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {

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


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {

                SectionData.StartAwait(grid_main);

                #region

                BuildReport();

                LocalReportExtensions.PrintToPrinter(rep);
                #endregion


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }

        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
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


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {

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
                //dg_orders.ItemsSource = ordersQuery;
                Window.GetWindow(this).Opacity = 1;
                #endregion


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }


        #endregion

        #region tabs
        int selectedTab = 0;
        private async void Btn_preparingOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchText = "";
                txt_search.Text = "";
                selectedTab = 0;
                cb_company.SelectedItem = null;
                cb_company.Text = "";
                cb_branches.Text = "";

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trShippingCompanyHint"));
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_delivery);
                path_preparingOrder.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                col_driver.Visibility = Visibility.Hidden;
                col_company.Visibility = Visibility.Visible;

                fillCompanies();

                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                chk_allBranches.IsChecked = true;
                chk_allCompanies.IsChecked = true;

                await Search();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_drivers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchText = "";
                txt_search.Text = "";
                cb_company.SelectedItem = null;
                cb_company.Text = "";
                cb_branches.Text = "";

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trDriver") + "...");
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 1;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_drivers);
                path_driver.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                col_driver.Visibility = Visibility.Visible;
                col_company.Visibility = Visibility.Hidden;

                fillDrivers();

                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                chk_allBranches.IsChecked = true;
                chk_allCompanies.IsChecked = true;

                await Search();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_customers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchText = "";
                txt_search.Text = "";
                cb_company.SelectedItem = null;
                cb_company.Text = "";
                cb_branches.Text = "";

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trCustomer") + "...");
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 2;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_customers);
                path_customer.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                col_driver.Visibility = Visibility.Visible;
                col_company.Visibility = Visibility.Visible;

                fillCustomers();

                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                chk_allBranches.IsChecked = true;
                chk_allCompanies.IsChecked = true;

                await Search();
            }
            catch (Exception ex)
            {
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
