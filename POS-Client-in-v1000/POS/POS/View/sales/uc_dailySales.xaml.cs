using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
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


namespace POS.View.sales
{
    /// <summary>
    /// Interaction logic for uc_dailySales.xaml
    /// </summary>
    public partial class uc_dailySales : UserControl
    {
        #region variables
        IEnumerable<ItemTransferInvoice> itemTrasferInvoices;
        Statistics statisticsModel = new Statistics();
        IEnumerable<ItemTransferInvoice> itemTrasferInvoicesQuery;
        string searchText = "";
        int selectedTab = 0;
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        #endregion

        private static uc_dailySales _instance;

        public static uc_dailySales Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_dailySales();
                return _instance;
            }
        }


        public uc_dailySales()
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

                //col_refNo.Visibility = Visibility.Collapsed;

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

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;

                fillPaymentTypes();

                fillCards();

                itemTrasferInvoices = await statisticsModel.GetUserdailyinvoice((int)MainWindow.branchID,
                                                                               (int)MainWindow.userID,
                                                                                  SectionData.DateTodbString(DateTime.Now));

                col_processType0.Visibility = Visibility.Collapsed;
                Btn_Invoice_Click(btn_invoice, null);
                col_reportChartWidth = col_reportChart.ActualWidth;

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_invoice.Tag.ToString());

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
        private async void Btn_Invoice_Click(object sender, RoutedEventArgs e)
        {//invoice tab
            try
            {
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                selectedTab = 0;
                txt_search.Text = "";

                if (dp_invoiceDate.SelectedDate == null || dp_invoiceDate.SelectedDate.Value.Date != DateTime.Now.Date)
                    dp_invoiceDate.SelectedDate = DateTime.Now;

                isFromLoadedEvent = false;

                rad_invoice.Content = MainWindow.resourcemanager.GetString("tr_Invoice");
                rad_return.Content = MainWindow.resourcemanager.GetString("trReturned");
                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                col_processType.Visibility = Visibility.Visible;

                path_order.Fill = Brushes.White;
                path_quotation.Fill = Brushes.White;
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_invoice);
                path_invoice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await Search();
                rowToHide.Height = rowToShow.Height;

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_order_Click(object sender, RoutedEventArgs e)
        {//order tab
            try
            {
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 1;
                txt_search.Text = "";

                if (dp_invoiceDate.SelectedDate.Value.Date != DateTime.Now.Date)
                    dp_invoiceDate.SelectedDate = DateTime.Now;

                rad_invoice.Content = MainWindow.resourcemanager.GetString("trOrder");
                rad_return.Content = MainWindow.resourcemanager.GetString("trSaved");
                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;
                col_processType.Visibility = Visibility.Hidden;

                path_invoice.Fill = Brushes.White;
                path_quotation.Fill = Brushes.White;
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_order);
                path_order.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await Search();
                rowToHide.Height = new GridLength(0);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }
        private async void Btn_quotation_Click(object sender, RoutedEventArgs e)
        {//quotation tab
            try
            {
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                selectedTab = 2;
                txt_search.Text = "";

                if (dp_invoiceDate.SelectedDate.Value.Date != DateTime.Now.Date)
                    dp_invoiceDate.SelectedDate = DateTime.Now;

                rad_invoice.Content = MainWindow.resourcemanager.GetString("trQuotation");
                rad_return.Content = MainWindow.resourcemanager.GetString("trSaved");
                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;
                col_processType.Visibility = Visibility.Hidden;

                path_invoice.Fill = Brushes.White;
                path_order.Fill = Brushes.White;
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_quotation);
                path_quotation.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                await Search();
                rowToHide.Height = new GridLength(0);

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        bool isFromLoadedEvent = true;

        #region methods
        private void translate()
        {
            tt_invoice.Content = MainWindow.resourcemanager.GetString("trInvoices");
            tt_order.Content = MainWindow.resourcemanager.GetString("trOrders");
            tt_quotation.Content = MainWindow.resourcemanager.GetString("trQuotations_");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_invoiceDate, MainWindow.resourcemanager.GetString("trDate"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_payments, MainWindow.resourcemanager.GetString("trPaymentType") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_card, MainWindow.resourcemanager.GetString("trCardHint"));

            rad_invoice.Content = MainWindow.resourcemanager.GetString("trInvoice");
            rad_return.Content = MainWindow.resourcemanager.GetString("trReturned");
            rad_draft.Content = MainWindow.resourcemanager.GetString("trDraft");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_payments, MainWindow.resourcemanager.GetString("trPaymentType") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_card, MainWindow.resourcemanager.GetString("trCardHint"));

            chk_allPaymentTypes.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allCards.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_No.Header = MainWindow.resourcemanager.GetString("trNo");
            //col_refNo.Header = MainWindow.resourcemanager.GetString("trRefNo.");
            col_type.Header = MainWindow.resourcemanager.GetString("trType");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_pos.Header = MainWindow.resourcemanager.GetString("trPOS");
            //col_user.Header = MainWindow.resourcemanager.GetString("trUser");
            col_totalNet.Header = MainWindow.resourcemanager.GetString("trTotal");
            col_totalNet0.Header = MainWindow.resourcemanager.GetString("trPaymentValue");
            col_processType.Header = MainWindow.resourcemanager.GetString("trPaymentType");
            col_processType0.Header = MainWindow.resourcemanager.GetString("trPaymentType");
            col_payments.Header = MainWindow.resourcemanager.GetString("trPayments");

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
        async Task<IEnumerable<ItemTransferInvoice>> RefreshItemTransferInvoiceList()
        {
            if (!isFromLoadedEvent)
                itemTrasferInvoices = await statisticsModel.GetUserdailyinvoice((int)MainWindow.branchID,
                                                                             (int)MainWindow.userID,
                                                                                SectionData.DateTodbString(dp_invoiceDate.SelectedDate.Value.Date));
            return itemTrasferInvoices;

        }
        IEnumerable<ItemTransferInvoice> lstColumnChart = null;
        async Task Search()
        {
            if (!isFromLoadedEvent)
            {

                if (itemTrasferInvoices is null)
                    await RefreshItemTransferInvoiceList();

                searchText = txt_search.Text.ToLower();
                itemTrasferInvoicesQuery = itemTrasferInvoices
                    .Where(s =>
                (
                s.invNumber.ToLower().Contains(searchText)
                ||
                s.tax.ToString().ToLower().Contains(searchText)
                )
                &&
                //(//invType
                //    (
                //        selectedTab == 0 //invoice
                //        ?
                //        (chk_invoice.IsChecked == true ? s.invType == "s" : false)
                //        ||
                //        (chk_return.IsChecked == true ? s.invType == "sb" : false)
                //        ||
                //        (chk_drafs.IsChecked == true ? s.invType == "sd" || s.invType == "sbd" : false)
                //        : false
                //    )
                //    ||
                //    (
                //        selectedTab == 1 //order
                //        ?
                //        (chk_invoice.IsChecked == true ? s.invType == "or" : false)
                //        ||
                //        (chk_return.IsChecked == true ? s.invType == "ors" : false)
                //        ||
                //        (chk_drafs.IsChecked == true ? s.invType == "ord" : false)
                //        : false
                //    )
                //    ||
                //    (
                //        selectedTab == 2 //quotation
                //        ?
                //        (chk_invoice.IsChecked == true ? s.invType == "q" : false)
                //        ||
                //        (chk_return.IsChecked == true ? s.invType == "qs" : false)
                //        ||
                //        (chk_drafs.IsChecked == true ? s.invType == "qd" : false)
                //        : false
                //    )
                //)
                //&&
                //paymentID
                (
                 chk_allPaymentTypes.IsChecked.Value ?
                    true :
                    (
                    cb_payments.SelectedIndex != -1 ?
                        (
                         cb_payments.SelectedValue.ToString() == "multiple" ?
                           (s.cachTransferList.Count > 1 || (s.cachTransferList.Count == 1 && s.cachTransferList.Sum(x => x.cash.Value) < s.totalNet))
                         :
                         (
                           cb_payments.SelectedValue.ToString() == "balance" ?
                               (s.cachTransferList.Count == 0 || (s.cachTransferList.Count >= 1 && (s.cachTransferList.Sum(x => x.cash.Value) < s.totalNet) || (s.cachTransferList.Any(c => c.processType == "balance"))))
                             :
                               s.cachTransferList.Any(c => c.processType == cb_payments.SelectedValue.ToString())
                         )
                        )
                        : false
                     )
                )
                &&
                //cardID
                (
                 chk_allCards.IsChecked.Value ?
                    true :
                    cb_card.SelectedIndex != -1 ?
                         s.cachTransferList.Any(c => c.cardName == cb_card.SelectedValue.ToString())
                            : false
                )
                );
                lstColumnChart = itemTrasferInvoicesQuery;

                //invType
                itemTrasferInvoicesQuery = itemTrasferInvoicesQuery.Where(s =>
                   (
                    selectedTab == 0 //invoice
                    ?
                    (rad_invoice.IsChecked == true ? s.invType == "s" : false)
                    ||
                    (rad_return.IsChecked == true ? s.invType == "sb" : false)
                    ||
                    (rad_draft.IsChecked == true ? rad_invoice.IsChecked == true ? s.invType == "sd" :
                                                                                   rad_return.IsChecked == true ? s.invType == "sbd" :
                                                                                   (s.invType == "sd" || s.invType == "sbd") : false)
                    : false
                    )
                    ||
                    (
                    selectedTab == 1 //order
                    ?
                    (rad_invoice.IsChecked == true ? s.invType == "or" : false)
                    ||
                    (rad_return.IsChecked == true ? s.invType == "ors" : false)
                    ||
                    (rad_draft.IsChecked == true ? rad_return.IsChecked == true ? false : s.invType == "ord" : false)
                    : false
                    )
                    ||
                    (
                    selectedTab == 2 //quotation
                    ?
                    (rad_invoice.IsChecked == true ? s.invType == "q" : false)
                    ||
                    (rad_return.IsChecked == true ? s.invType == "qs" : false)
                    ||
                    (rad_draft.IsChecked == true ? rad_return.IsChecked == true ? false : s.invType == "qd" : false)
                    : false
                    )
                );

                RefreshIemTrasferInvoicesView();
                fillColumnChart();
                fillPieChart();
                fillRowChart();
            }
        }
        void RefreshIemTrasferInvoicesView()
        {
            foreach (var i in itemTrasferInvoicesQuery)
            {
                if (cb_payments.SelectedIndex != -1 && !(cb_payments.SelectedValue.ToString().Equals("multiple")) && !(cb_payments.SelectedValue.ToString().Equals("balance")))
                {

                    if (cb_payments.SelectedValue.ToString().Equals("card"))
                    {
                        int cardCount = i.cachTransferList.Count(x => x.processType == "card");
                        if (cardCount == 1)
                        {
                            i.processType0 = i.cachTransferList.Where(x => x.processType == "card").FirstOrDefault().cardName.ToString();
                            i.totalNet0 = i.cachTransferList.Where(x => x.processType == cb_payments.SelectedValue.ToString()).Sum(x => x.cash.Value);
                        }
                        else
                        {
                            if (cb_card.SelectedIndex != -1)
                            {
                                i.processType0 = cb_card.SelectedValue.ToString();
                                i.totalNet0 = i.cachTransferList.Where(x => x.cardName == cb_card.SelectedValue.ToString()).Sum(x => x.cash.Value);
                            }
                            else
                            {
                                i.processType0 = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods");
                                i.totalNet0 = i.cachTransferList.Where(x => x.processType == cb_payments.SelectedValue.ToString()).Sum(x => x.cash.Value);
                            }
                        }
                    }
                    else
                    {
                        i.processType0 = cb_payments.SelectedValue.ToString();
                        i.totalNet0 = i.cachTransferList.Where(x => x.processType == cb_payments.SelectedValue.ToString()).Sum(x => x.cash.Value);
                    }

                    col_processType.Visibility = Visibility.Collapsed;
                    col_processType0.Visibility = Visibility.Visible;

                }
                else if (cb_payments.SelectedIndex != -1 && cb_payments.SelectedValue.ToString().Equals("balance"))
                {
                    i.processType0 = cb_payments.SelectedValue.ToString();

                    if (i.cachTransferList.Count == 0)
                        i.totalNet0 = i.totalNet.Value;
                    else if (i.cachTransferList.Count == 1 && i.totalNet > i.cachTransferList.FirstOrDefault().cash)
                        i.totalNet0 = i.cachTransferList.FirstOrDefault().cash.Value;
                    else if (i.cachTransferList.Count > 1 && i.totalNet > i.cachTransferList.Sum(x => x.cash))
                        i.totalNet0 = (i.totalNet - i.cachTransferList.Sum(x => x.cash)).Value;
                    //else
                    //    i.totalNet0 = i.cachTransferList.Where(x => x.processType == "balance").Sum(x => x.cash).Value;

                    col_processType.Visibility = Visibility.Collapsed;
                    col_processType0.Visibility = Visibility.Visible;
                }
                else
                {
                    i.processType0 = i.processType;
                    i.totalNet0 = i.totalNet.Value;

                    col_processType.Visibility = Visibility.Visible;
                    col_processType0.Visibility = Visibility.Collapsed;
                }
            }

            dgInvoice.ItemsSource = itemTrasferInvoicesQuery;
            txt_count.Text = itemTrasferInvoicesQuery.Count().ToString();

            decimal total = 0;
            //total = itemTrasferInvoicesQuery.Select(b => b.totalNet0).Sum();
            if (rad_draft.IsChecked.Value == true && (rad_return.IsChecked.Value == true || rad_invoice.IsChecked.Value == true))
                total = itemTrasferInvoicesQuery.Where(i => i.invType != "sd" || i.invType == "sbd").Select(b => b.totalNetRep.Value).Sum();

            else
                total = itemTrasferInvoicesQuery.Select(b => b.totalNetRep.Value).Sum();

            if (rad_draft.IsChecked.Value == true && rad_return.IsChecked.Value == false && rad_invoice.IsChecked.Value == false)
                col_invoices.Visibility = Visibility.Hidden;
            else
                col_invoices.Visibility = Visibility.Visible;

            tb_total.Text = SectionData.DecTostring(total);
        }
        private void fillPaymentTypes()
        {
            var typelist = new[] {
            new { Text = MainWindow.resourcemanager.GetString("trCash")                  , Value = "cash" },
            new { Text = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods") , Value = "card" },
            new { Text = MainWindow.resourcemanager.GetString("trCredit")                , Value = "balance" },
            new { Text = MainWindow.resourcemanager.GetString("trMultiplePayment")       , Value = "multiple" },
                };
            cb_payments.DisplayMemberPath = "Text";
            cb_payments.SelectedValuePath = "Value";
            cb_payments.ItemsSource = typelist;
        }
        private async void fillCards()
        {
            if (FillCombo.cardsList is null)
                await FillCombo.RefreshCards();

            cb_card.DisplayMemberPath = "name";
            cb_card.SelectedValuePath = "name";
            cb_card.ItemsSource = FillCombo.cardsList;
        }
        #endregion

        #region charts
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;

            titles.Clear();
            var temp = itemTrasferInvoicesQuery;
            var titleTemp = temp.GroupBy(m => m.branchCreatorName);
            titles.AddRange(titleTemp.Select(jj => jj.Key));
            var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new { branchCreatorId = s.Key, count = s.Count() });
            x = result.Select(m => m.count);

            SeriesCollection piechartData = new SeriesCollection();
            int xCount = x.Count();
            if (x.Count() <= 6) xCount = x.Count();
            else xCount = 6;
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
                for (int i = 6; i < x.Count(); i++)
                {
                    List<int> final = new List<int>();
                    List<string> lable = new List<string>();

                    final.Add(x.ToList().Skip(i).FirstOrDefault());
                    if (final.Count > 0)
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
        //SeriesCollection columnChartData = new SeriesCollection();
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<int> y = null;
            IEnumerable<int> z = null;

            string trChk1 = "", trChk2 = "", condition1 = "", condition2 = "", condition3 = "", condition4 = "";

            if (selectedTab == 0)
            { trChk1 = "tr_Sales"; trChk2 = "trReturned"; condition1 = "s"; condition2 = "sb"; condition3 = "sd"; condition4 = "sbd"; }
            else if (selectedTab == 1)
            { trChk1 = "trOrder"; trChk2 = "trSaved"; condition1 = "or"; condition2 = "ors"; condition3 = "ord"; condition4 = "ord"; }
            else if (selectedTab == 2)
            { trChk1 = "trQuotation"; trChk2 = "trSaved"; condition1 = "q"; condition2 = "qs"; condition3 = "qd"; condition4 = "qd"; }

            var temp = lstColumnChart;
            var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
            {
                branchCreatorId = s.Key,
                countS = s.Where(m => m.invType == condition1).Count(),
                countSb = s.Where(m => m.invType == condition2).Count(),
                countSd = s.Where(m => (m.invType == condition3) || (m.invType == condition4)).Count()
            });
            x = result.Select(m => m.countS);
            y = result.Select(m => m.countSb);
            z = result.Select(m => m.countSd);
            var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
            {
                uUserName = s.Key
            });
            names.AddRange(tempName.Select(nn => nn.uUserName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cS = new List<int>();
            List<int> cSb = new List<int>();
            List<int> cSd = new List<int>();

            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString(trChk1),
                MainWindow.resourcemanager.GetString(trChk2),
                MainWindow.resourcemanager.GetString("trDraft")
            };
            int xCount;
            if (x.Count() <= 6) xCount = x.Count();
            else xCount = 6;
            for (int i = 0; i < xCount; i++)
            {
                cS.Add(x.ToList().Skip(i).FirstOrDefault());
                cSb.Add(y.ToList().Skip(i).FirstOrDefault());
                cSd.Add(z.ToList().Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (x.Count() > 6)
            {
                int cSSum = 0, cSbSum = 0, cSdSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    cSSum = cSSum + x.ToList().Skip(i).FirstOrDefault();
                    cSbSum = cSbSum + y.ToList().Skip(i).FirstOrDefault();
                    cSdSum = cSdSum + z.ToList().Skip(i).FirstOrDefault();
                }
                if (!((cSSum == 0) && (cSbSum == 0) && (cSdSum == 0)))
                {
                    cS.Add(cSSum);
                    cSb.Add(cSbSum);
                    cSd.Add(cSdSum);
                    axcolumn.Labels.Add("trOthers");
                }
            }

            //3 فوق بعض
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cS.AsChartValues(),
                Title = titles[0],
                DataLabels = true,
            });
            columnChartData.Add(
           new StackedColumnSeries
           {
               Values = cSb.AsChartValues(),
               Title = titles[1],
               DataLabels = true,
           });
            columnChartData.Add(
           new StackedColumnSeries
           {
               Values = cSd.AsChartValues(),
               Title = titles[2],
               DataLabels = true,
           });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;
            IEnumerable<decimal> pbTemp = null;
            IEnumerable<decimal> resultTemp = null;

            var temp = itemTrasferInvoicesQuery;
            var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
            {
                branchCreatorId = s.Key,
                totalS = s.Where(x => x.invType == "s").Sum(x => x.totalNet),
                totalSb = s.Where(x => x.invType == "sb").Sum(x => x.totalNet)
            }
            );
            var resultTotal = result.Select(x => new { x.branchCreatorId, total = x.totalS - x.totalSb }).ToList();
            pTemp = result.Select(x => (decimal)x.totalS);
            pbTemp = result.Select(x => (decimal)x.totalSb);
            resultTemp = result.Select(x => (decimal)x.totalS);
            var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
            {
                uUserName = s.Key
            });
            names.AddRange(tempName.Select(nn => nn.uUserName));

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
            for (int i = 0; i < pbTemp.Count(); i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            rowChartData.Add(
          new LineSeries
          {
              Values = purchase.AsChartValues(),
              Title = titles[0]
          }); ;
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

        #region events
        DateTime prevSelectedDate;
        private async void RefreshView_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {//select date
            try
            {
                if (prevSelectedDate != dp_invoiceDate.SelectedDate.Value)
                {
                    if (sender != null && !(isFromLoadedEvent))
                        SectionData.StartAwait(grid_main);

                    prevSelectedDate = dp_invoiceDate.SelectedDate.Value;
                    await RefreshItemTransferInvoiceList();
                    await Search();

                    if (sender != null && !(isFromLoadedEvent))
                        SectionData.EndAwait(grid_main);
                }
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void RefreshViewCheckbox(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (!rad_invoice.IsChecked.Value && !rad_draft.IsChecked.Value && !rad_return.IsChecked.Value)
                    rad_invoice.IsChecked = true;
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

                dp_invoiceDate.SelectedDate = DateTime.Now;
                await RefreshItemTransferInvoiceList();
                txt_search.Text = "";
                searchText = "";
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
                        MainWindow.mainWindow.BTN_sales_Click(MainWindow.mainWindow.btn_sales, null);
                        uc_sales.Instance.UserControl_Loaded(null, null);
                        if (selectedTab == 0)
                        {
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
                        else if (selectedTab == 1)
                        {
                            uc_sales.Instance.Btn_orders_Click(uc_sales.Instance.btn_salesOrders, null);
                            uc_orders.Instance.UserControl_Loaded(null, null);
                            uc_orders._InvoiceType = invoice.invType;
                            uc_orders.Instance.invoice = invoice;
                            uc_orders.isFromReport = true;
                            if (item.archived == 0)
                                uc_orders.archived = false;
                            else
                                uc_orders.archived = true;
                            await uc_orders.Instance.fillInvoiceInputs(invoice);

                        }
                        else if (selectedTab == 2)
                        {
                            uc_sales.Instance.Btn_quotations_Click(uc_sales.Instance.btn_quotation, null);
                            uc_quotations.Instance.UserControl_Loaded(null, null);
                            uc_quotations._InvoiceType = invoice.invType;
                            uc_quotations.Instance.invoice = invoice;
                            uc_quotations.isFromReport = true;
                            if (item.archived == 0)
                                uc_quotations.archived = false;
                            else
                                uc_quotations.archived = true;
                            await uc_quotations.Instance.fillInvoiceInputs(invoice);

                        }
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
        private async void Cb_payments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_payments.SelectedIndex != -1)
                {
                    if (cb_payments.SelectedValue.ToString().Equals("card"))
                    {
                        pnl_cards.Visibility = Visibility.Visible;
                        chk_allCards.IsChecked = true;
                        Chk_allCards_Checked(chk_allCards, null);
                    }
                    else
                    {
                        pnl_cards.Visibility = Visibility.Collapsed;
                        await Search();
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
        private async void Cb_card_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_card.SelectedIndex != -1)
                {
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
        private async void Chk_allPaymentTypes_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_payments.SelectedIndex = -1;
                cb_payments.IsEnabled = false;
                //cb_card.SelectedIndex = -1;
                chk_allCards.IsChecked = true;
                pnl_cards.Visibility = Visibility.Collapsed;
                this.DataContext = itemTrasferInvoicesQuery;
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
        private async void Chk_allPaymentTypes_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_payments.IsEnabled = true;
                this.DataContext = itemTrasferInvoicesQuery;
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
        private async void Chk_allCards_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_card.SelectedIndex = -1;
                cb_card.IsEnabled = false;
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
        private async void Chk_allCards_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_card.IsEnabled = true;
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

        #region datagrid events
        private void multiplePayments(object sender, RoutedEventArgs e)
        {//payments
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string processType = "";
                string cardName = "";

                if (cb_payments.SelectedIndex != -1)
                    processType = cb_payments.SelectedValue.ToString();
                if (cb_card.SelectedIndex != -1)
                    cardName = cb_card.SelectedValue.ToString();

                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        ItemTransferInvoice row = (ItemTransferInvoice)dgInvoice.SelectedItems[0];

                        Statistics statisticsModel = new Statistics();

                        Window.GetWindow(this).Opacity = 0.2;
                        wd_payments w = new wd_payments();
                        w.itemTransferInvoice = row;
                        w.processType = processType;
                        w.cardName = cardName;
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;

                    }

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
        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cb_payments.SelectedIndex != -1 && !cb_payments.SelectedValue.ToString().Equals("multiple"))
                {
                    Button btnPayments = sender as Button;

                    ItemTransferInvoice iTI = itemTrasferInvoicesQuery.Where(i => i.invNumber == btnPayments.Tag.ToString()).FirstOrDefault();
                    if (iTI.cachTransferList.Count > 0 && iTI.cachTransferList.Count(i => i.processType == cb_payments.SelectedValue.ToString()) > 1)
                        btnPayments.Visibility = Visibility.Visible;
                    else
                        btnPayments.Visibility = Visibility.Collapsed;

                    if (iTI.cachTransferList.Count > 0 && iTI.cachTransferList.Count(i => i.cardName == cb_card.SelectedValue.ToString()) > 1)
                        btnPayments.Visibility = Visibility.Visible;
                    else
                        btnPayments.Visibility = Visibility.Collapsed;

                }
                else
                {
                    this.DataContext = itemTrasferInvoicesQuery;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void invoices(object sender, RoutedEventArgs e)
        {//invoices
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        ItemTransferInvoice row = (ItemTransferInvoice)dgInvoice.SelectedItems[0];

                        Window.GetWindow(this).Opacity = 0.2;
                        wd_inoicesForReports w = new wd_inoicesForReports();

                        if (row.invType == "sb")
                            w.invoiceLst = itemTrasferInvoices.Where(i => i.invNumber == row.mainInvNumber).ToList();

                        else if (row.invType == "s")
                            w.returnLst = row.returnInvList;

                        w.invType = row.invType;
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;

                    }

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
        private async void search_Checking(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsChecked == true)
                {
                    if (cb.Name == "rad_invoice")
                    {
                        rad_return.IsChecked = false;
                        rad_draft.IsChecked = false;
                        //col_refNo.Visibility = Visibility.Collapsed;
                    }
                    else if (cb.Name == "rad_return")
                    {
                        rad_invoice.IsChecked = false;
                        rad_draft.IsChecked = false;
                        //col_refNo.Visibility = Visibility.Visible;
                    }
                    else if (cb.Name == "rad_draft")
                    {
                        rad_invoice.IsChecked = false;
                        rad_return.IsChecked = false;
                        //col_refNo.Visibility = Visibility.Collapsed;
                    }
                    await Search();
                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chk_uncheck(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "rad_invoice")
                        rad_invoice.IsChecked = true;
                    else if (cb.Name == "rad_return")
                        rad_return.IsChecked = true;
                    else if (cb.Name == "rad_draft")
                        rad_draft.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region reports
        public async Task BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string firstTitle = "sales";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string addpath;
            string invchk = "";
            string retchk = "";
            string drftchk = "";
            string startDate = "";
            //string endDate = "";

            string paymentval = "";
            string cardval = "";
            string searchval = "";
            //string startTime = "";
            //string endTime = "";
            string branch = "";
            string pos = "";
            string invtype = "";
            List<string> invTypelist = new List<string>();


            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (isArabic)
            {

                if (selectedTab == 0)
                {
                    secondTitle = "invoice";
                    //if (rad_return.IsChecked == true)
                    //{

                    //    addpath = @"\Reports\Sale\Daily\Ar\ArReturn\dailySale.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\Sale\Daily\Ar\dailySale.rdlc";
                    //}

                    invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
                    retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";
                    drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";

                }
                else if (selectedTab == 1)
                {
                    secondTitle = "order";
                    addpath = @"\Reports\Sale\Daily\Ar\dailySaleO.rdlc";
                    invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trOrder") : "";
                    retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trSaved") : "";
                    drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";

                }
                else
                {
                    //  selectedTab == 2
                    secondTitle = "quotation";
                    addpath = @"\Reports\Sale\Daily\Ar\dailySaleQ.rdlc";
                    invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trQuotation") : "";
                    retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trSaved") : "";
                    drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";

                }
                subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            }
            else
            {

                if (selectedTab == 0)
                {
                    secondTitle = "invoice";
                    //if (rad_return.IsChecked == true)
                    //{

                    //    addpath = @"\Reports\Sale\Daily\En\EnReturn\dailySale.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\Sale\Daily\En\dailySale.rdlc";
                    //}
                    invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
                    retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";
                    drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";

                }
                else if (selectedTab == 1)
                {
                    secondTitle = "order";
                    addpath = @"\Reports\Sale\Daily\En\dailySaleO.rdlc";
                    invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trOrder") : "";
                    retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trSaved") : "";
                    drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";

                }
                else
                {
                    //  selectedTab == 2
                    secondTitle = "quotation";
                    addpath = @"\Reports\Sale\Daily\En\dailySaleQ.rdlc";
                    invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trQuotation") : "";
                    retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trSaved") : "";
                    drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";

                }
                subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            }

            //filter
            startDate = dp_invoiceDate.SelectedDate != null ? SectionData.DateToString(dp_invoiceDate.SelectedDate) : "";

            //branch = cb_branches.SelectedItem != null
            //             && (chk_allBranches.IsChecked == false || chk_allBranches.IsChecked == null)
            //             ? cb_branches.Text : (chk_allBranches.IsChecked == true ? all : "");

            //pos = cb_pos.SelectedItem != null
            //         && (chk_allPos.IsChecked == false || chk_allPos.IsChecked == null)
            //     && branch != "" ? cb_pos.Text : (chk_allPos.IsChecked == true && branch != "" ? all : "");

            paymentval = cb_payments.SelectedItem != null
               && (chk_allPaymentTypes.IsChecked == false || chk_allPaymentTypes.IsChecked == null)
               ? clsReports.PaymentComboConvert(cb_payments.SelectedValue.ToString()) : (chk_allPaymentTypes.IsChecked == true ? all : "");

            cardval = (cb_card.SelectedItem != null
            && (chk_allCards.IsChecked == false) && cb_card.IsVisible == true)
            ? cb_card.Text : (chk_allCards.IsChecked == true && chk_allCards.IsVisible == true ? all : "");
            invTypelist.Add(invchk);
            invTypelist.Add(retchk);
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
            paramarr.Add(new ReportParameter("paymentval", paymentval));
            paramarr.Add(new ReportParameter("invtype", invtype));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));

            paramarr.Add(new ReportParameter("cardval", cardval));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trTheCreditCard", MainWindow.resourcemanagerreport.GetString("trTheCreditCard")));
            //paramarr.Add(new ReportParameter("branchval", branch));
            //paramarr.Add(new ReportParameter("posval", pos));

            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            ReportCls.checkLang();
            Title = MainWindow.resourcemanagerreport.GetString("trSalesReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));


            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            string cb_paymentsValue = "";
            string cb_cardValue = "";
            if ((bool)chk_allPaymentTypes.IsChecked)
            {
                cb_paymentsValue = "all";
            }
            else
            {
                cb_paymentsValue = cb_payments.SelectedValue.ToString();
            }
            if ((bool)chk_allCards.IsChecked)
            {
                cb_cardValue = "all";
            }
            else
            {
                cb_cardValue = cb_card.SelectedValue.ToString();
            }
            //  rad_invoice.IsChecked
            //rad_return
            // rad_draft
            await clsReports.SaledailyReport(itemTrasferInvoicesQuery.ToList(), rep, reppath, paramarr, selectedTab, cb_paymentsValue, cb_cardValue,
                rad_invoice.IsChecked, rad_return.IsChecked, rad_draft.IsChecked);

            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        private async Task pdfdaily()
        {

            await BuildReport();

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
        private async void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                /////////////////////////////////////
                //Thread t1 = new Thread(() =>
                //{
                await pdfdaily();
                //});
                //t1.Start();
                //////////////////////////////////////
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private async Task printDaily()
        {
            await BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }
        private async void Btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                /////////////////////////////////////
                //Thread t1 = new Thread(() =>
                //{
                await printDaily();
                //});
                //t1.Start();
                //////////////////////////////////////

                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        private async Task ExcelDaily()
        {

            await BuildReport();

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
        private async void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                //Thread t1 = new Thread(() =>
                //{
                await ExcelDaily();

                //});
                //t1.Start();
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                //{
                #region
                Window.GetWindow(this).Opacity = 0.2;
                /////////////////////
                string pdfpath = "";
                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                await BuildReport();
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
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
                    //w.cartesianChart.Series = cartesianChart.Series;
                    //w.cartesianChart.AxisX = cartesianChart.AxisX;
                    //w.DataContext = this;
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
                    //w.cartesianChart.Series = rowChart.Series;
                    //w.cartesianChart.AxisX = rowChart.AxisX;
                    w.cartesianChart.Series = rowChart.Series;
                    w.axcolumn.Labels = MyAxis.Labels;
                }
                w.ShowDialog();

                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);


                /*
                var myChart = new LiveCharts.Wpf.CartesianChart
                {
                    DisableAnimations = true,
                    Width = 1260,
                    Height = 900,
                    Series = cartesianChart.Series,
                };

                var viewbox = new Viewbox();
                viewbox.Child = myChart;
                viewbox.Measure(myChart.RenderSize);
                viewbox.Arrange(new Rect(new System.Windows.Point(0, 0), myChart.RenderSize));
                myChart.Update(true, true); //force chart redraw
                viewbox.UpdateLayout();

                //SaveToPng(myChart, "pic/bodyCompo.png");

                PrintingChart.Print(myChart);
                */
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_payments_KeyUp(object sender, KeyEventArgs e)
        {

        }
        /*
public void SaveToPng(FrameworkElement visual, string fileName)
{
   var encoder = new PngBitmapEncoder();
   EncodeVisual(visual, fileName, encoder);
}

private static void EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
{
   var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
   bitmap.Render(visual);
   var frame = BitmapFrame.Create(bitmap);
   encoder.Frames.Add(frame);
   using (var stream = File.Create(fileName))
       encoder.Save(stream);
}
*/
    }
}
