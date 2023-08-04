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
using System.Threading;
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

namespace POS.View.reports
{
    public partial class uc_banksReport : UserControl
    {
        #region variables
        Statistics statisticModel = new Statistics();
        List<CashTransferSts> payments;
        List<CashTransferSts> recipient;

        IEnumerable<VendorCombo> userPaymentsCombo;
        IEnumerable<AccountantCombo> accPaymentsCombo;

        int selectedTab = 0;
        #endregion

        public uc_banksReport()
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
        private static uc_banksReport _instance;
        public static uc_banksReport Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_banksReport();
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

                payments = await statisticModel.GetPayments();// Deposite
                recipient = await statisticModel.GetReceipt();// Pull

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

                #region key up
                // key_up branch
                cb_paymentsBank.IsTextSearchEnabled = false;
                cb_paymentsBank.IsEditable = true;
                cb_paymentsBank.StaysOpenOnEdit = true;
                cb_paymentsBank.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_paymentsBank.Text = "";
                // key_up user
                cb_paymentsUser.IsTextSearchEnabled = false;
                cb_paymentsUser.IsEditable = true;
                cb_paymentsUser.StaysOpenOnEdit = true;
                cb_paymentsUser.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_paymentsUser.Text = "";
                // key_up accountant
                cb_paymentsAccountant.IsTextSearchEnabled = false;
                cb_paymentsAccountant.IsEditable = true;
                cb_paymentsAccountant.StaysOpenOnEdit = true;
                cb_paymentsAccountant.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_paymentsAccountant.Text = "";
                #endregion

                Btn_vendor_Click(btn_payments, null);

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

        #region tabs
        private void Btn_vendor_Click(object sender, RoutedEventArgs e)
        {//payments
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                hideAllColumn();
                selectedTab = 0;

                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_Type.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                col_user.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                txt_search.Text = "";
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_payments);
                path_payments.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                fillBanksCombo(payments);

                userPaymentsCombo = statisticModel.getUserAcc(payments, "bn");
                fillUserCombo(userPaymentsCombo, cb_paymentsUser);

                accPaymentsCombo = statisticModel.getAccounantCombo(payments, "bn");
                fillAccCombo(accPaymentsCombo, cb_paymentsAccountant);

                dp_paymentsStartDate.SelectedDate = null;
                dp_paymentsEndDate.SelectedDate = null;
                chk_allpaymentsAccountant.IsChecked=true;
                chk_allPaymentsBanks.IsChecked = true;
                chk_allPyamentsUser.IsChecked = true;

                fillEvents(payments);

                chk_allPaymentsBanks.IsChecked = true;
                chk_allPyamentsUser.IsChecked = true;
                chk_allpaymentsAccountant.IsChecked = true;

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
        private void Btn_customer_Click(object sender, RoutedEventArgs e)
        {//received
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 1;
                hideAllColumn();
                txt_search.Text = "";

                //view columns
                col_tansNum.Visibility = Visibility.Visible;
                col_Type.Visibility = Visibility.Visible;
                col_updateUserAcc.Visibility = Visibility.Visible;
                col_user.Visibility = Visibility.Visible;
                col_updateDate.Visibility = Visibility.Visible;
                col_cash.Visibility = Visibility.Visible;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_recipient);
                path_recipient.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                fillBanksCombo(recipient);

                userPaymentsCombo = statisticModel.getUserAcc(recipient, "bn");
                fillUserCombo(userPaymentsCombo, cb_paymentsUser);

                accPaymentsCombo = statisticModel.getAccounantCombo(recipient, "bn");
                fillAccCombo(accPaymentsCombo, cb_paymentsAccountant);

                dp_paymentsStartDate.SelectedDate = null;
                dp_paymentsEndDate.SelectedDate = null;
                chk_allpaymentsAccountant.IsChecked = true;
                chk_allPaymentsBanks.IsChecked = true;
                chk_allPyamentsUser.IsChecked = true;

                fillEvents(recipient);

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

        #region methods
        private void translate()
        {
            tt_payments.Content = MainWindow.resourcemanager.GetString("trPull");
            tt_recipient.Content = MainWindow.resourcemanager.GetString("trDeposit");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentsBank, MainWindow.resourcemanager.GetString("trBank") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentsUser, MainWindow.resourcemanager.GetString("trUser") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentsAccountant, MainWindow.resourcemanager.GetString("trAccoutant") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_paymentsStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_paymentsEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            chk_allPaymentsBanks.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allPyamentsUser.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allpaymentsAccountant.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_tansNum.Header = MainWindow.resourcemanager.GetString("trNo");
            col_Type.Header = MainWindow.resourcemanager.GetString("trType");
            col_updateUserAcc.Header = MainWindow.resourcemanager.GetString("trAccoutant");
            col_Bank.Header = MainWindow.resourcemanager.GetString("trBank");
            col_user.Header = MainWindow.resourcemanager.GetString("trUser");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_cash.Header = MainWindow.resourcemanager.GetString("trAmount");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        List<Bank> bankslist = new List<Bank>();
        private void fillBanksCombo(List<CashTransferSts> lst)
        {
            bankslist = lst.Where(g => g.bankId != null).GroupBy(g => g.bankId).Select(g => new Bank{ bankId = g.FirstOrDefault().bankId.Value, name = g.FirstOrDefault().bankName }).ToList();
            cb_paymentsBank.SelectedValuePath = "bankId";
            cb_paymentsBank.DisplayMemberPath = "name";
            cb_paymentsBank.ItemsSource = bankslist;
        }
        private void fillUserCombo(IEnumerable<VendorCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "UserId";
            cb.DisplayMemberPath = "UserAcc";
            cb.ItemsSource = list;
        }
        private void fillAccCombo(IEnumerable<AccountantCombo> list, ComboBox cb)
        {
            cb.SelectedValuePath = "Accountant";
            cb.DisplayMemberPath = "Accountant";
            cb.ItemsSource = list;
        }
        IEnumerable<CashTransferSts> temp = null;
        private void fillEvents(List<CashTransferSts> lst)
        {
            temp = fillList(lst, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);
            dgPayments.ItemsSource = temp;
            txt_count.Text = temp.ToList().Count.ToString();
            
            decimal total = 0;
            total = temp.Select(b => b.cash.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            //fillPieChart();
            fillColumnChart();
            fillRowChart();
        }
        private void fillemptyEvents(List<CashTransferSts> lst)
        {
            temp = new List<CashTransferSts>();
                dgPayments.ItemsSource = temp;
            txt_count.Text = temp.ToList().Count.ToString();
            
            decimal total = 0;
            total = temp.Select(b => b.cash.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            //fillPieChart();
            fillColumnChart();
            fillEmptyRowChart();
        }
        private void fillbyComboValue()
        {
            if ((cb_paymentsAccountant.SelectedItem == null && chk_allpaymentsAccountant.IsChecked == false) ||
                 (cb_paymentsBank.SelectedItem == null && chk_allPaymentsBanks.IsChecked == false) ||
                 (cb_paymentsUser.SelectedItem == null && chk_allPyamentsUser.IsChecked == false))
            {
                fillEmptyByType();
            }
            else
            {
                fillByType();
            }
        }
        private void fillEmptyByType()
        {

            //if (selectedTab == 0)
            temp = new List<CashTransferSts>();
            fillemptyEvents(new List<CashTransferSts>());
            //else if (selectedTab == 1)
            //    fillEvents(recipient);

        }
        private void fillByType()
        {
            if (selectedTab == 0)
                fillEvents(payments);
            else if (selectedTab == 1)
                fillEvents(recipient);
        }
        List<CashTransferSts> bankLst;
        private List<CashTransferSts> fillList(List<CashTransferSts> payments, ComboBox vendor, ComboBox payType, ComboBox accountant
          , DatePicker startDate, DatePicker endDate)
        {
            var result = payments.Where(x => (
                          (cb_paymentsBank.SelectedItem != null ? x.bankId == (int)cb_paymentsBank.SelectedValue : true)
                       && (payType.SelectedItem != null ? x.userId == (int)cb_paymentsUser.SelectedValue : true)
                       && (accountant.SelectedItem != null ? x.updateUserAcc == (string)cb_paymentsAccountant.SelectedValue : true)
                       && (startDate.SelectedDate != null ? x.updateDate.Value.Date >= startDate.SelectedDate.Value.Date : true)
                       && (endDate.SelectedDate != null ? x.updateDate.Value.Date <= endDate.SelectedDate.Value.Date : true)));

            bankLst = result.ToList();
            return result.ToList();
        }
        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
            bdr_payments.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_recipient.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            path_payments.Fill = Brushes.White;
            path_recipient.Fill = Brushes.White;
        }
        private void hideAllColumn()
        {
            col_tansNum.Visibility = Visibility.Hidden;
            col_Type.Visibility = Visibility.Hidden;
            col_updateUserAcc.Visibility = Visibility.Hidden;
            col_user.Visibility = Visibility.Hidden;
            col_updateDate.Visibility = Visibility.Hidden;
            col_cash.Visibility = Visibility.Hidden;

        }
        #endregion

        #region charts
        private void fillPieChart()
        {
            #region
            //List<string> titles = new List<string>();
            //List<int> resultList = new List<int>();
            //titles.Clear();
            //var temp = fillList(payments, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn");
            //if (selectedTab == 1)
            //{
            //    temp = fillList(recipient, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn");
            //}

            //var result = temp
            //    .GroupBy(s => new { s.transType })
            //    .Select(s => new CashTransferSts
            //    {
            //        processTypeCount = s.Count(),
            //        processType = s.FirstOrDefault().transType,
            //    });
            //resultList = result.Select(m => m.processTypeCount).ToList();
            //titles = result.Select(m => m.transType).ToList();
            //for (int t = 0; t < titles.Count; t++)
            //{
            //    string s = "";
            //    switch (titles[t])
            //    {
            //        case "p": s = MainWindow.resourcemanager.GetString("trPull"); break;
            //        case "d": s = MainWindow.resourcemanager.GetString("trDeposit"); break;
            //    }
            //    titles[t] = s;
            //}
            //SeriesCollection piechartData = new SeriesCollection();
            //for (int i = 0; i < resultList.Count(); i++)
            //{
            //    List<int> final = new List<int>();
            //    List<string> lable = new List<string>();

            //    final.Add(resultList.Skip(i).FirstOrDefault());
            //    lable = titles;
            //    piechartData.Add(
            //      new PieSeries
            //      {
            //          Values = final.AsChartValues(),
            //          Title = lable.Skip(i).FirstOrDefault(),
            //          DataLabels = true,
            //      }
            //  );

            //}
            //chart1.Series = piechartData;
            #endregion
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();

            var res = temp.GroupBy(x => new { x.bankId, x.transType }).Select(x => new CashTransferSts
            {
                bankName = x.FirstOrDefault().bankName,
                transType = x.FirstOrDefault().transType,
                bankId = x.FirstOrDefault().bankId,
                pullCount = x.Where(g => g.transType == "d").Count(),
                depositCount = x.Where(g => g.transType == "p").Count(),
                pullSum = x.Where(g => g.transType == "p").Select(g => g.cash.Value).Sum(),
                depositSum = x.Where(g => g.transType == "d").Select(g => g.cash.Value).Sum()
            });
            resultList = res.GroupBy(x => x.bankId).Select(x => new CashTransferSts
            {
                bankName = x.FirstOrDefault().bankName,
                pullCount = x.Sum(g => g.pullCount),
                depositCount = x.Sum(g => g.depositCount),
                bankId = x.FirstOrDefault().bankId,
                transType = x.FirstOrDefault().transType,
                pullSum = x.FirstOrDefault().pullSum,
                depositSum = x.FirstOrDefault().depositSum
            }
            ).ToList();

            var tempName = res.GroupBy(s => new { s.bankId }).Select(s => new
            {
                itemName = s.FirstOrDefault().bankName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> deposite = new List<decimal>();
            List<decimal> pull = new List<decimal>();

            int xCount = 6;
            if (resultList.Count() <= 6)
                xCount = resultList.Count();
            for (int i = 0; i < xCount; i++)
            {
                deposite.Add(resultList.ToList().Skip(i).FirstOrDefault().depositSum);
                pull.Add(resultList.ToList().Skip(i).FirstOrDefault().pullSum);

                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (resultList.Count() > 6)
            {
                decimal pullSum = 0, depositSum = 0;
                for (int i = 6; i < resultList.Count; i++)
                {
                    pullSum = pullSum + resultList.ToList().Skip(i).FirstOrDefault().pullSum;
                    depositSum = depositSum + resultList.ToList().Skip(i).FirstOrDefault().depositSum;

                }
                if (!((pullSum == 0) && (depositSum == 0)))
                {
                    deposite.Add(depositSum);
                    pull.Add(pullSum);

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = deposite.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trDeposit")
            });
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = pull.AsChartValues(),
                DataLabels = true,
                Title = MainWindow.resourcemanager.GetString("trPull")
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillRowChart()
        {
            int endYear = DateTime.Now.Year;
            int startYear = endYear - 1;
            int startMonth = DateTime.Now.Month;
            int endMonth = startMonth;

            IEnumerable<CashTransferSts> payLst = null;
            IEnumerable<CashTransferSts> recLst = null;

            payLst = fillList(payments, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);
            recLst = fillList(recipient, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);

            if (dp_paymentsStartDate.SelectedDate != null && dp_paymentsEndDate.SelectedDate != null)
            {
                startYear = dp_paymentsStartDate.SelectedDate.Value.Year;
                endYear = dp_paymentsEndDate.SelectedDate.Value.Year;
                startMonth = dp_paymentsStartDate.SelectedDate.Value.Month;
                endMonth = dp_paymentsEndDate.SelectedDate.Value.Month;
            }

            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();
            var temp = bankLst;

            SeriesCollection rowChartData = new SeriesCollection();
            var tempName = temp.GroupBy(s => new { s.bankId }).Select(s => new
            {
                itemName = s.FirstOrDefault().updateDate,
            });
            names.AddRange(tempName.Select(nn => nn.itemName.ToString()));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cash = new List<decimal>();
            List<decimal> card = new List<decimal>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        var drawCash = payLst.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.transType == "p").Sum(c => c.cash);
                        var drawCard = recLst.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.transType == "d").Sum(c => c.cash);
                        cash.Add((decimal)drawCash);
                        card.Add((decimal)drawCard);

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
                    var drawCash = payLst.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "cash").Count();
                    var drawCard = recLst.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "cash").Count();
                    cash.Add(drawCash);
                    card.Add(drawCard);
                    MyAxis.Labels.Add(year.ToString());
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = card.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("trDeposit")
          }); ;
            rowChartData.Add(
         new LineSeries
         {
             Values = cash.AsChartValues(),
             Title = MainWindow.resourcemanager.GetString("trPull")
         });

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        private void fillEmptyRowChart()
        {
            int endYear = DateTime.Now.Year;
            int startYear = endYear - 1;
            int startMonth = DateTime.Now.Month;
            int endMonth = startMonth;

            IEnumerable<CashTransferSts> payLst = null;
            IEnumerable<CashTransferSts> recLst = null;
           
            payLst = fillList(new List<CashTransferSts>(), cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);
            recLst = fillList(new List<CashTransferSts>(), cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);

            if (dp_paymentsStartDate.SelectedDate != null && dp_paymentsEndDate.SelectedDate != null)
            {
                startYear = dp_paymentsStartDate.SelectedDate.Value.Year;
                endYear = dp_paymentsEndDate.SelectedDate.Value.Year;
                startMonth = dp_paymentsStartDate.SelectedDate.Value.Month;
                endMonth = dp_paymentsEndDate.SelectedDate.Value.Month;
            }

            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransferSts> resultList = new List<CashTransferSts>();
            var temp = bankLst;

            SeriesCollection rowChartData = new SeriesCollection();
            var tempName = temp.GroupBy(s => new { s.bankId }).Select(s => new
            {
                itemName = s.FirstOrDefault().updateDate,
            });
            names.AddRange(tempName.Select(nn => nn.itemName.ToString()));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cash = new List<decimal>();
            List<decimal> card = new List<decimal>();

            if (endYear - startYear <= 1)
            {
                for (int year = startYear; year <= endYear; year++)
                {
                    for (int month = startMonth; month <= 12; month++)
                    {
                        var firstOfThisMonth = new DateTime(year, month, 1);
                        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
                        var drawCash = payLst.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.transType == "p").Sum(c => c.cash);
                        var drawCard = recLst.ToList().Where(c => c.updateDate > firstOfThisMonth && c.updateDate <= firstOfNextMonth && c.transType == "d").Sum(c => c.cash);
                        cash.Add((decimal)drawCash);
                        card.Add((decimal)drawCard);

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
                    var drawCash = payLst.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "cash").Count();
                    var drawCard = recLst.ToList().Where(c => c.updateDate > firstOfThisYear && c.updateDate <= firstOfNextMYear && c.processType == "cash").Count();
                    cash.Add(drawCash);
                    card.Add(drawCard);
                    MyAxis.Labels.Add(year.ToString());
                }
            }
            rowChartData.Add(
          new LineSeries
          {
              Values = card.AsChartValues(),
              Title = MainWindow.resourcemanager.GetString("trDeposit")
          }); ;
            rowChartData.Add(
         new LineSeries
         {
             Values = cash.AsChartValues(),
             Title = MainWindow.resourcemanager.GetString("trPull")
         });

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion
      
        #region events
        private void Chk_allBanks_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_paymentsBank.IsEnabled = false;
                cb_paymentsBank.SelectedItem = null;
                cb_paymentsBank.Text = "";
                cb_paymentsBank.ItemsSource = bankslist;
                fillbyComboValue();
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
        private void Chk_allBanks_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_paymentsBank.IsEnabled = true;
                fillEmptyByType();
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
        private void Cb_paymentsUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillbyComboValue();
               // fillEvents(payments);

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
        private void Chk_allPyamentsUser_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_paymentsUser.IsEnabled = false;
                cb_paymentsUser.SelectedItem = null;
                cb_paymentsUser.Text = "";
                cb_paymentsUser.ItemsSource = userPaymentsCombo;
                fillbyComboValue();
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
        private void Chk_allPyamentsUser_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_paymentsUser.IsEnabled = true;
                fillEmptyByType();
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
        private void Chk_allpaymentsAccountant_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_paymentsAccountant.IsEnabled = false;
                cb_paymentsAccountant.SelectedItem = null;
                cb_paymentsAccountant.Text = "";
                cb_paymentsAccountant.ItemsSource = accPaymentsCombo;
                fillbyComboValue();
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
        private void Chk_allpaymentsAccountant_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_paymentsAccountant.IsEnabled = true;
                fillEmptyByType();
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
        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillbyComboValue();
                //fillByType();

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

                fillByType();

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
                if (selectedTab == 0)
                    payments = await statisticModel.GetPayments();// Deposite
                else if (selectedTab == 1)
                    recipient = await statisticModel.GetReceipt();// Pull

                cb_paymentsBank.SelectedItem = null;
                cb_paymentsUser.SelectedItem = null;
                cb_paymentsAccountant.SelectedItem = null;
                //chk_allpaymentsAccountant.IsChecked = false;
                //chk_allPaymentsBanks.IsChecked = false;
                //chk_allPyamentsUser.IsChecked = false;
                dp_paymentsStartDate.SelectedDate = null;
                dp_paymentsEndDate.SelectedDate = null;

                dp_paymentsStartDate.SelectedDate = null;
                dp_paymentsEndDate.SelectedDate = null;
                chk_allpaymentsAccountant.IsChecked = true;
                chk_allPaymentsBanks.IsChecked = true;
                chk_allPyamentsUser.IsChecked = true;

                fillByType();

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

                IEnumerable<CashTransferSts> tempSearch;
                if (selectedTab == 0)
                    tempSearch = fillList(payments, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);
                else
                    tempSearch = fillList(recipient, cb_paymentsBank, cb_paymentsUser, cb_paymentsAccountant, dp_paymentsStartDate, dp_paymentsEndDate).Where(s => s.side == "bn" && s.isConfirm == 1);

                temp = tempSearch.Where(obj => (
                    obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.bankName.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower()) ||
                    obj.userAcc.ToLower().Contains(txt_search.Text.ToLower())
                    ));
                dgPayments.ItemsSource = temp;
                var items = dgPayments.ItemsSource as IEnumerable<CashTransferSts>;
                decimal total = 0;
                total = items.Select(b => b.cash.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);
                txt_count.Text = items.ToList().Count.ToString();

                fillColumnChart();
                fillRowChart();

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
        private void Cb_paymentsBank_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = bankslist.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_paymentsUser_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = userPaymentsCombo.Where(p => p.UserAcc.ToLower().Contains(tb.Text.ToLower()) || (p.UserMobile != null && p.UserMobile.Contains(tb.Text))).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_paymentsAccountant_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = accPaymentsCombo.Where(p => p.Accountant.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region reports
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "banksReport";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";

            string startDate = "";
            string endDate = "";
            string bankval = "";
            string userval = "";
            string Accountantval = "";
    
            //  string cardval = "";
            string searchval = "";

        
            List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
         
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Bank\Ar\ArDeposite.rdlc";
                    secondTitle = "pull";
                 


                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Bank\Ar\ArPull.rdlc";
                    secondTitle = "deposit";
                 
                }

            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Bank\En\Deposite.rdlc";
                    secondTitle = "pull";
                
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Bank\En\Pull.rdlc";
                    secondTitle = "deposit";
                 
                }

            }
            //filter
            startDate = dp_paymentsStartDate.SelectedDate != null ? SectionData.DateToString(dp_paymentsStartDate.SelectedDate) : "";

            endDate = dp_paymentsEndDate.SelectedDate != null ? SectionData.DateToString(dp_paymentsEndDate.SelectedDate) : "";
            //startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            //endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
           bankval = cb_paymentsBank.SelectedItem != null
               && (chk_allPaymentsBanks.IsChecked == false || chk_allPaymentsBanks.IsChecked == null)
               ? cb_paymentsBank.Text : (chk_allPaymentsBanks.IsChecked == true ? all : "");

            userval = cb_paymentsUser.SelectedItem != null
               && (chk_allPyamentsUser.IsChecked == false || chk_allPyamentsUser.IsChecked == null)
               && bankval != ""
               ? cb_paymentsUser.Text : (chk_allPyamentsUser.IsChecked == true && bankval != "" ? all : "");

            Accountantval = cb_paymentsAccountant.SelectedItem != null
            && (chk_allpaymentsAccountant.IsChecked == false || chk_allpaymentsAccountant.IsChecked == null)
            && bankval != ""
            ? cb_paymentsAccountant.Text : (chk_allpaymentsAccountant.IsChecked == true && bankval != "" ? all : "");
            paramarr.Add(new ReportParameter("bankval", bankval));
            paramarr.Add(new ReportParameter("userval", userval));
            paramarr.Add(new ReportParameter("AccountantVal", Accountantval));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));    
            paramarr.Add(new ReportParameter("trBank", MainWindow.resourcemanagerreport.GetString("trBank")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trAmount", MainWindow.resourcemanagerreport.GetString("trAmount")));
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            clsReports.cashTransferStsBank(temp, rep, reppath, paramarr);
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


                //    });
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
                Window.GetWindow(this).Opacity = 0.2;
                string pdfpath = "";

                List<ReportParameter> paramarr = new List<ReportParameter>();

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
                //else if (buttonTag.Equals("chart1"))
                //{
                //    w.type = "chart1";
                //    w.pieChart.Series = chart1.Series;

                //}
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
