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
using System.Globalization;
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Threading;
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_salePromotion.xaml
    /// </summary>
    public partial class uc_salePromotion : UserControl
    {
        #region variables
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private int selectedTab = 0;

        Statistics statisticModel = new Statistics();

        List<ItemTransferInvoice> coupons;
        List<ItemTransferInvoice> Offers;

        //for combo boxes
        /*************************/
        CouponCombo selectedCouon;
        OfferCombo selectedOffer;

        List<CouponCombo> comboCoupon;
        List<OfferCombo> comboOffer;

        ObservableCollection<CouponCombo> comboCouponTemp = new ObservableCollection<CouponCombo>();
        ObservableCollection<OfferCombo> comboOfferTemp = new ObservableCollection<OfferCombo>();

        ObservableCollection<CouponCombo> dynamicComboCoupon;
        ObservableCollection<OfferCombo> dynamicComboOffer;

        /*************************/

        ObservableCollection<int> selectedcouponId = new ObservableCollection<int>();
        ObservableCollection<int> selectedOfferId = new ObservableCollection<int>();
        #endregion

        private static uc_salePromotion _instance;

        public static uc_salePromotion Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_salePromotion();
                return _instance;
            }
        }
        public uc_salePromotion()
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

                coupons = await statisticModel.GetSalecoupon((int)MainWindow.branchID, (int)MainWindow.userID);

                Offers = await statisticModel.GetPromoOffer((int)MainWindow.branchID, (int)MainWindow.userID);

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
                cb_Coupons.IsTextSearchEnabled = false;
                cb_Coupons.IsEditable = true;
                cb_Coupons.StaysOpenOnEdit = true;
                cb_Coupons.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_Coupons.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                comboCoupon = statisticModel.GetCopComboList(coupons);
                comboOffer = statisticModel.GetOfferComboList(Offers);

                dynamicComboCoupon = new ObservableCollection<CouponCombo>(comboCoupon);
                dynamicComboOffer = new ObservableCollection<OfferCombo>(comboOffer);

                chk_couponInvoice.IsChecked = true;

                btn_coupons_Click(btn_coupons, null);

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
            tt_coupon.Content = MainWindow.resourcemanager.GetString("trCoupons");
            tt_offers.Content = MainWindow.resourcemanager.GetString("trOffer");

            chk_couponInvoice.Content = MainWindow.resourcemanager.GetString("tr_Invoice");
            chk_couponReturn.Content = MainWindow.resourcemanager.GetString("trReturned");
            chk_couponDrafs.Content = MainWindow.resourcemanager.GetString("trDraft");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_Coupons, MainWindow.resourcemanager.GetString("trCouponHint"));

            chk_allCoupon.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_couponEndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_couponStartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_couponEndTime, MainWindow.resourcemanager.GetString("trEndTime") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_couponStartTime, MainWindow.resourcemanager.GetString("trStartTime") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_No.Header = MainWindow.resourcemanager.GetString("trNo.");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_coupon.Header = MainWindow.resourcemanager.GetString("trCoupon");
            col_cTypeValue.Header = MainWindow.resourcemanager.GetString("trValue");
            col_oTypeValue.Header = MainWindow.resourcemanager.GetString("trValue");
            col_couponTotalValue.Header = MainWindow.resourcemanager.GetString("trDiscount");
            col_offersTotalValue.Header = MainWindow.resourcemanager.GetString("trDiscount");
            col_offers.Header = MainWindow.resourcemanager.GetString("trOffer");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_price.Header = MainWindow.resourcemanager.GetString("trPrice");
            col_itQuantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_total.Header = MainWindow.resourcemanager.GetString("trTotal");

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
        private void fillComboCoupon()
        {
            cb_Coupons.SelectedValuePath = "Copcid";
            cb_Coupons.DisplayMemberPath = "Copname";
            cb_Coupons.ItemsSource = dynamicComboCoupon;
        }
        private void fillComboOffer()
        {
            cb_Coupons.SelectedValuePath = "OofferId";
            cb_Coupons.DisplayMemberPath = "Oname";
            cb_Coupons.ItemsSource = dynamicComboOffer;
        }
        private static void fillDates(DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            if (startDate.SelectedDate != null && startTime.SelectedTime != null)
            {
                string x = startDate.SelectedDate.Value.Date.ToShortDateString();
                string y = startTime.SelectedTime.Value.ToShortTimeString();
                string resultStartTime = x + " " + y;
                startTime.SelectedTime = DateTime.Parse(resultStartTime);
                startDate.SelectedDate = DateTime.Parse(resultStartTime);
            }
            if (endDate.SelectedDate != null && endTime.SelectedTime != null)
            {
                string x = endDate.SelectedDate.Value.Date.ToShortDateString();
                string y = endTime.SelectedTime.Value.ToShortTimeString();
                string resultEndTime = x + " " + y;
                endTime.SelectedTime = DateTime.Parse(resultEndTime);
                endDate.SelectedDate = DateTime.Parse(resultEndTime);
            }
        }
        IEnumerable<ItemTransferInvoice> invLst = null;
        private IEnumerable<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> Invoices, CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft
           , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            fillDates(startDate, endDate, startTime, endTime);
            var result = Invoices.Where(x => (

               ((chkDraft.IsChecked == true ? (x.invType == "sd" || x.invType == "sbd") : false)
                          || (chkReturn.IsChecked == true ? (x.invType == "sb") : false)
                          || (chkInvoice.IsChecked == true ? (x.invType == "s") : false))
                      && ((startDate.SelectedDate != null && startTime.SelectedTime == null) ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                      && ((endDate.SelectedDate != null && endTime.SelectedTime == null) ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)
                      && (startTime.SelectedTime != null ? x.invDate >= startTime.SelectedTime : true)
                      && (endTime.SelectedTime != null ? x.invDate <= endTime.SelectedTime : true)));

            invLst = result;
            return result;
        }
        IEnumerable<ItemTransferInvoice> invRowChartLst = null;
        ObservableCollection<int> selected = new ObservableCollection<int>();
        private IEnumerable<ItemTransferInvoice> fillRowChartList(IEnumerable<ItemTransferInvoice> Invoices, CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft
          , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            fillDates(startDate, endDate, startTime, endTime);
            var result = Invoices.Where(x => ((txt_search.Text != null ? x.invNumber.ToLower().Contains(txt_search.Text.ToLower())
              || x.invType.ToLower().Contains(txt_search.Text.ToLower())
              || x.discountType.ToLower().Contains(txt_search.Text.ToLower()) : true)
              &&
                        ((startDate.SelectedDate != null && startTime.SelectedTime == null) ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                        && ((endDate.SelectedDate != null && endTime.SelectedTime == null) ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)
                        && (startTime.SelectedTime != null ? x.invDate >= startTime.SelectedTime : true)
                        && (endTime.SelectedTime != null ? x.invDate <= endTime.SelectedTime : true)));

            invRowChartLst = result;
            return result;
        }
        public void fillEvent()
        {
            if (selectedTab == 0)
            {
                itemTransfers = fillList(coupons, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedcouponId.Count != 0 ? selectedcouponId.Contains((int)j.CopcId) : true));
                fillRowChartList(coupons, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedcouponId.Count != 0 ? selectedcouponId.Contains((int)j.CopcId) : true));
            }
            else if (selectedTab == 1)
            {
                itemTransfers = fillList(Offers, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedOfferId.Count != 0 ? selectedOfferId.Contains((int)j.OofferId) : true));
                fillRowChartList(Offers, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedOfferId.Count != 0 ? selectedOfferId.Contains((int)j.OofferId) : true));
            }
            if (selectedTab == 0)
            {
                selected = selectedcouponId;
                itemTransfers = itemTransfers.Where(j => (selected.Count != 0 ? selectedcouponId.Contains((int)j.CopcId) : true));
            }
            else if (selectedTab == 1)
            {
                selected = selectedOfferId;
                itemTransfers = itemTransfers.Where(j => (selected.Count != 0 ? selectedOfferId.Contains((int)j.OofferId) : true));
                decimal total = 0;
                total = itemTransfers.Select(b => b.subTotal.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);

            }

            dgInvoice.ItemsSource = itemTransfers;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            fillPieChart(selected);
            fillColumnChart(selected);
            fillRowChartCoupAndOffer(cb_Coupons, selected);
        }
        public void fillEmptyEvent()
        {

            //if (selectedTab == 0)
            //{
            //    itemTransfers = fillList(coupons, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedcouponId.Count != 0 ? selectedcouponId.Contains((int)j.CopcId) : true));
            //    fillRowChartList(coupons, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedcouponId.Count != 0 ? selectedcouponId.Contains((int)j.CopcId) : true));
            //}
            //else if (selectedTab == 1)
            //{
            //    itemTransfers = fillList(Offers, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedOfferId.Count != 0 ? selectedOfferId.Contains((int)j.OofferId) : true));
            //    fillRowChartList(Offers, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedOfferId.Count != 0 ? selectedOfferId.Contains((int)j.OofferId) : true));
            //}
            itemTransfers = new List<ItemTransferInvoice>();
            invLst = new List<ItemTransferInvoice>();
            invRowChartLst = new List<ItemTransferInvoice>();
            dgInvoice.ItemsSource = itemTransfers;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            ObservableCollection<int> selected = new ObservableCollection<int>();
            if (selectedTab == 0)
                selected = selectedcouponId;
            else if (selectedTab == 1)
            {
                selected = selectedOfferId;
                decimal total = 0;
                total = itemTransfers.Select(b => b.subTotal.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);
            }
            fillPieChart(selected);
            fillColumnChart(selected);
            fillRowChartCoupAndOffer(cb_Coupons, selected);
        }
        private void hidAllColumns()
        {
            //col_code.Visibility = Visibility.Hidden;
            //col_offerCode.Visibility = Visibility.Hidden;
            col_item.Visibility = Visibility.Hidden;
            col_itQuantity.Visibility = Visibility.Hidden;
            col_total.Visibility = Visibility.Hidden;
            col_coupon.Visibility = Visibility.Hidden;
            col_offers.Visibility = Visibility.Hidden;
            //col_coupoValue.Visibility = Visibility.Hidden;
            //col_couponType.Visibility = Visibility.Hidden;
            col_cTypeValue.Visibility = Visibility.Hidden;
            //col_offersType.Visibility = Visibility.Hidden;
            //col_offersValue.Visibility = Visibility.Hidden;
            col_oTypeValue.Visibility = Visibility.Hidden;
            col_offersTotalValue.Visibility = Visibility.Hidden;
            col_couponTotalValue.Visibility = Visibility.Hidden;
            col_price.Visibility = Visibility.Hidden;
        }

        #endregion

        #region charts
        private void fillPieChart(ObservableCollection<int> stackedButton)
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;

            //var temp = invLst;
            var temp = itemTransfers;
            if (selectedTab == 0)
            {
                titles.Clear();
                temp = temp.Where(j => (selectedcouponId.Count != 0 ? stackedButton.Contains((int)j.CopcId) : true));
                var titleTemp = temp.GroupBy(m => m.Copname);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.CopcId).Select(s => new { CopcId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }

            else if (selectedTab == 1)
            {
                titles.Clear();
                temp = temp.Where(j => (selectedOfferId.Count != 0 ? stackedButton.Contains((int)j.OofferId) : true));
                var titleTemp = temp.GroupBy(m => m.Oname);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.OofferId).Select(s => new { OofferId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }

            SeriesCollection piechartData = new SeriesCollection();
            int xCount = 0;
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
        private void fillColumnChart(ObservableCollection<int> stackedButton)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<int> y = null;
            IEnumerable<int> z = null;

            //var temp = invLst;
            var temp = itemTransfers;
            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedcouponId.Count != 0 ? stackedButton.Contains((int)j.CopcId) : true));
                var result = temp.GroupBy(s => s.CopcId).Select(s => new
                {
                    CopcId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.Copname).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }

            else if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedOfferId.Count != 0 ? stackedButton.Contains((int)j.OofferId) : true));
                var result = temp.GroupBy(s => s.OofferId).Select(s => new
                {
                    CopcId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.Oname).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();
            List<int> cD = new List<int>();

            string title = "trCoupon";
            if (selectedTab == 1)
                title = "trOffer";
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString(title)
                //MainWindow.resourcemanager.GetString("trSales"),
                //MainWindow.resourcemanager.GetString("trReturned"),
                //MainWindow.resourcemanager.GetString("trDraft")
            };
            int xCount = 0;
            if (x.Count() <= 6) xCount = x.Count();
            for (int i = 0; i < xCount; i++)
            {
                cP.Add(x.ToList().Skip(i).FirstOrDefault());
                cPb.Add(y.ToList().Skip(i).FirstOrDefault());
                cD.Add(z.ToList().Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (x.Count() > 6)
            {
                int cPSum = 0, cPbSum = 0, cDSum = 0;
                for (int i = 0; i < xCount; i++)
                {
                    cPSum = cPSum + x.ToList().Skip(i).FirstOrDefault();
                    cPbSum = cPbSum + y.ToList().Skip(i).FirstOrDefault();
                    cDSum = cDSum + z.ToList().Skip(i).FirstOrDefault();
                }
                if (!((cPSum == 0) && (cPbSum == 0) && (cDSum == 0)))
                {
                    cP.Add(cPSum);
                    cPb.Add(cPbSum);
                    cD.Add(cDSum);
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
            // columnChartData.Add(
            //new StackedColumnSeries
            //{
            //    Values = cPb.AsChartValues(),
            //    Title = titles[1],
            //    DataLabels = true,
            //});
            // columnChartData.Add(
            //new StackedColumnSeries
            //{
            //    Values = cD.AsChartValues(),
            //    Title = titles[2],
            //    DataLabels = true,
            //});

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillRowChartCoupAndOffer(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;

            var temp = invRowChartLst;
            if (selectedTab == 0)
            {
                names.Clear();
                temp = temp.Where(j => (selectedcouponId.Count != 0 ? stackedButton.Contains((int)j.CopcId) : true));
                var result = temp.GroupBy(s => new { s.CopcId }).Select(s => new
                {
                    CopcId = s.FirstOrDefault().CopcId,
                    copName = s.FirstOrDefault().Copname,
                    invId = s.FirstOrDefault().invoiceId,
                    copTotalValue = s.Sum(x => x.couponTotalValue)
                }
                );


                var name = temp.GroupBy(s => s.invoiceId).Select(s => new
                {
                    uUserName = s.FirstOrDefault().Copname
                });
                names.AddRange(name.Select(nn => nn.uUserName));
                pTemp = result.Select(x => (decimal)x.copTotalValue);
            }
            if (selectedTab == 1)
            {
                names.Clear();
                temp = temp.Where(j => (selectedOfferId.Count != 0 ? stackedButton.Contains((int)j.OofferId) : true));
                var result1 = temp.GroupBy(s => new { s.OofferId, s.ITitemUnitId }).Select(s => new
                {
                    offerId = s.FirstOrDefault().OofferId,
                    offerName = s.FirstOrDefault().Oname,
                    itemId = s.FirstOrDefault().ITitemUnitId,
                    offerTotalValue = s.Sum(x => x.offerTotalValue)
                }
             );

                var name = result1.GroupBy(s => s.offerId).Select(s => new
                {
                    uUserName = s.FirstOrDefault().offerName,
                    offerTotalValue = s.Sum(x => x.offerTotalValue)
                });
                names.AddRange(name.Select(nn => nn.uUserName));
                pTemp = name.Select(x => (decimal)x.offerTotalValue);
            }

            SeriesCollection rowChartData = new SeriesCollection();
            List<decimal> purchase = new List<decimal>();
            List<decimal> returns = new List<decimal>();
            List<decimal> sub = new List<decimal>();
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("trTotalDiscountValue"),
                //MainWindow.resourcemanager.GetString("trTotalReturn"),
                //MainWindow.resourcemanager.GetString("trSum"),
            };
            int xCount = 0;
            if (pTemp.Count() <= 6) xCount = pTemp.Count();
            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (pTemp.Count() > 6)
            {
                decimal purchaseSum = 0;
                for (int i = xCount; i < pTemp.Count(); i++)
                {
                    purchaseSum = purchaseSum + pTemp.ToList().Skip(i).FirstOrDefault();
                }
                if (purchaseSum != 0)
                {
                    purchase.Add(purchaseSum);
                    MyAxis.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            rowChartData.Add(
                  new LineSeries
                  {
                      Values = purchase.AsChartValues(),
                      Title = titles[0]
                  });

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion

        #region Functions
        private void hideSatacks()
        {
            stk_tagsCoupons.Visibility = Visibility.Collapsed;
            stk_tagsOffers.Visibility = Visibility.Collapsed;
        }

        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_coupon.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_offers.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_coupons.Fill = Brushes.White;
            path_offers.Fill = Brushes.White;

        }

        #endregion

        #region tabControl

        private void btn_coupons_Click(object sender, RoutedEventArgs e)
        {//copouns
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_Coupons, MainWindow.resourcemanager.GetString("trCoponHint"));
                selectedTab = 0;
                txt_search.Text = "";
                hideSatacks();
                stk_tagsCoupons.Visibility = Visibility.Visible;
                bdr_total.Visibility = Visibility.Collapsed;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_coupon);
                path_coupons.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //   fillComboCoupon();
                //  chk_allCoupon.IsChecked = true;
                isClicked = true;
                chk_allCoupon_Click(chk_allCoupon, null);
                dp_couponEndDate.SelectedDate = null;
                dp_couponStartDate.SelectedDate = null;
                dt_couponStartTime.SelectedTime = null;
                dt_couponEndTime.SelectedTime = null;
                // fillEvent();
                //   Btn_refresh_Click(btn_refresh, null);
                hidAllColumns();
                //col_code.Visibility = Visibility.Visible;
                col_coupon.Visibility = Visibility.Visible;
                //col_couponType.Visibility = Visibility.Visible;
                //col_coupoValue.Visibility = Visibility.Visible;
                col_cTypeValue.Visibility = Visibility.Visible;
                col_couponTotalValue.Visibility = Visibility.Visible;

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
        private void btn_offers_Click(object sender, RoutedEventArgs e)
        {//offers
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_Coupons, MainWindow.resourcemanager.GetString("trOfferHint"));
                selectedTab = 1;
                txt_search.Text = "";
                hideSatacks();
                stk_tagsOffers.Visibility = Visibility.Visible;
                bdr_total.Visibility = Visibility.Visible;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_offers);
                path_offers.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                //fillComboOffer();
                //fillEvent();
                isClicked = true;
                chk_allCoupon_Click(chk_allCoupon, null);
                dp_couponEndDate.SelectedDate = null;
                dp_couponStartDate.SelectedDate = null;
                dt_couponStartTime.SelectedTime = null;
                dt_couponEndTime.SelectedTime = null;
                hidAllColumns();

                //col_offerCode.Visibility = Visibility.Visible;
                col_offers.Visibility = Visibility.Visible;
                //col_offersType.Visibility = Visibility.Visible;
                //col_offersValue.Visibility = Visibility.Visible;
                col_oTypeValue.Visibility = Visibility.Visible;
                col_item.Visibility = Visibility.Visible;
                col_price.Visibility = Visibility.Visible;
                col_itQuantity.Visibility = Visibility.Visible;
                col_offersTotalValue.Visibility = Visibility.Visible;
                col_total.Visibility = Visibility.Visible;




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

        #region refreshButton
        private async  void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_Coupons.SelectedItem = null;
                chk_couponDrafs.IsChecked = false;
                chk_couponReturn.IsChecked = false;
                chk_couponInvoice.IsChecked = true;
                dp_couponEndDate.SelectedDate = null;
                dp_couponStartDate.SelectedDate = null;
                dt_couponStartTime.SelectedTime = null;
                dt_couponEndTime.SelectedTime = null;
                chk_allCoupon.IsChecked = true;
                // cb_Coupons.IsEnabled = true;
                coupons = await statisticModel.GetSalecoupon((int)MainWindow.branchID, (int)MainWindow.userID);

                Offers = await statisticModel.GetPromoOffer((int)MainWindow.branchID, (int)MainWindow.userID);
                if (selectedTab == 0)
                {
                    for (int i = 0; i < comboCouponTemp.Count; i++)
                    {
                        dynamicComboCoupon.Add(comboCouponTemp.Skip(i).FirstOrDefault());
                    }
                    comboCouponTemp.Clear();
                    stk_tagsCoupons.Children.Clear();
                    selectedcouponId.Clear();
                    // btn_coupons_Click(btn_coupons, null);

                    // fillEvent();
                }

                else if (selectedTab == 1)
                {
                    for (int i = 0; i < comboOfferTemp.Count; i++)
                    {
                        dynamicComboOffer.Add(comboOfferTemp.Skip(i).FirstOrDefault());
                    }
                    comboOfferTemp.Clear();
                    stk_tagsOffers.Children.Clear();
                    selectedOfferId.Clear();
                  //  fillEvent();
                }
                isClicked = true;
                chk_allCoupon_Click(chk_allCoupon, null);
                fillEvent();
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
        #endregion

        #region settings
        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                List<string> Headers = new List<string>();
                List<string> Headers1 = new List<string>();
                foreach (var item in dgInvoice.Columns)
                {
                    Headers.Add(item.Header.ToString());
                }

                winControlPanel win = new winControlPanel(Headers);

                if (win.ShowDialog() == false)
                {
                    Headers1.Clear();
                    Headers1.AddRange(win.newHeaderResult);
                }
                for (int i = 0; i < Headers1.Count; i++)
                {
                    if (dgInvoice.Columns[i].Header.ToString() == Headers1[i])
                    {
                        dgInvoice.Columns[i].Visibility = Visibility.Visible;
                    }
                    else
                        dgInvoice.Columns[i].Visibility = Visibility.Hidden;
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

        private void Chip_OnDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                var currentChip = (Chip)sender;
                if (selectedTab == 0)
                {
                    stk_tagsCoupons.Children.Remove(currentChip);
                    var m = comboCouponTemp.Where(j => j.Copcid == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboCoupon.Add(m.FirstOrDefault());
                    selectedcouponId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedcouponId.Count == 0)
                    {
                        cb_Coupons.SelectedItem = null;
                        isClicked = true;
                        chk_allCoupon_Click(chk_allCoupon, null);
                    }
                    else
                    {
                        fillEvent();
                    }
                }
                else if (selectedTab == 1)
                {
                    stk_tagsOffers.Children.Remove(currentChip);
                    var m = comboOfferTemp.Where(j => j.OofferId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboOffer.Add(m.FirstOrDefault());
                    selectedOfferId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedOfferId.Count == 0)
                    {
                        cb_Coupons.SelectedItem = null;
                        isClicked = true;
                        chk_allCoupon_Click(chk_allCoupon, null);
                    }
                    else
                    {
                        fillEvent();
                    }
                }
             //   fillEvent();

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
        private void fillEventCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillEvent();

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
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            fillEventCall(sender);
        }
        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventCall(sender);
        }
        private void dt_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            fillEventCall(sender);
        }
        bool isClicked = true;
        private void chk_allCoupon_Click(object sender, RoutedEventArgs e)
        {//all coupons
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (isClicked)
                {

                    cb_Coupons.SelectedItem = null;
                    cb_Coupons.IsEnabled = false;
                    isClicked = false;
                    chk_allCoupon.IsChecked = true;
                    cb_Coupons.Text = "";

                    if (selectedTab == 0)
                    {
                        cb_Coupons.ItemsSource = dynamicComboCoupon;
                        for (int i = 0; i < comboCouponTemp.Count; i++)
                        {
                            dynamicComboCoupon.Add(comboCouponTemp.Skip(i).FirstOrDefault());
                        }
                        comboCouponTemp.Clear();
                        stk_tagsCoupons.Children.Clear();
                        selectedcouponId.Clear();
                    }
                    else if (selectedTab == 1)
                    {
                        cb_Coupons.ItemsSource = dynamicComboOffer;
                        for (int i = 0; i < comboOfferTemp.Count; i++)
                        {
                            dynamicComboOffer.Add(comboOfferTemp.Skip(i).FirstOrDefault());
                        }
                        comboOfferTemp.Clear();
                        stk_tagsOffers.Children.Clear();
                        selectedOfferId.Clear();
                    }
                    fillEvent();
                }
                else
                {
                    chk_allCoupon.IsChecked = false;
                    if (selectedTab == 0)
                    {
                        dynamicComboCoupon = new ObservableCollection<CouponCombo>(comboCoupon);
                        fillComboCoupon();

                        cb_Coupons.IsEnabled = true;

                        stk_tagsCoupons.Children.Clear();
                        selectedcouponId.Clear();
                    }
                    else if (selectedTab == 1)
                    {
                        dynamicComboOffer = new ObservableCollection<OfferCombo>(comboOffer);
                        fillComboOffer();
                        cb_Coupons.IsEnabled = true;

                        comboOfferTemp.Clear();
                        stk_tagsOffers.Children.Clear();
                        selectedOfferId.Clear();
                    }
                    fillEmptyEvent();
                    isClicked = true;
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
        private void cb_Coupons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_Coupons.SelectedItem != null)
                {
                    if (selectedTab == 0)
                    {
                        if (stk_tagsCoupons.Children.Count < 5)
                        {
                            selectedCouon = cb_Coupons.SelectedItem as CouponCombo;
                            var b = new MaterialDesignThemes.Wpf.Chip()
                            {
                                Content = selectedCouon.Copname,
                                Name = "btn" + selectedCouon.Copcid.ToString(),
                                IsDeletable = true,
                                Margin = new Thickness(5, 0, 5, 0)
                            };
                            b.DeleteClick += Chip_OnDeleteClick;
                            stk_tagsCoupons.Children.Add(b);
                            comboCouponTemp.Add(selectedCouon);
                            selectedcouponId.Add(selectedCouon.Copcid);
                            dynamicComboCoupon.Remove(selectedCouon);
                        }
                    }
                    else if (selectedTab == 1)
                    {
                        if (stk_tagsOffers.Children.Count < 5)
                        {
                            selectedOffer = cb_Coupons.SelectedItem as OfferCombo;
                            var b = new MaterialDesignThemes.Wpf.Chip()
                            {
                                Content = selectedOffer.Oname,
                                Name = "btn" + selectedOffer.OofferId.ToString(),
                                IsDeletable = true,
                                Margin = new Thickness(5, 0, 5, 0)
                            };
                            b.DeleteClick += Chip_OnDeleteClick;
                            stk_tagsOffers.Children.Add(b);
                            comboOfferTemp.Add(selectedOffer);
                            selectedOfferId.Add(selectedOffer.OofferId);
                            dynamicComboOffer.Remove(selectedOffer);
                        }
                    }
                    fillEvent();
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
        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    selected = selectedcouponId;

                    //itemTransfers = fillList(coupons, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponEndDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedcouponId.Count != 0 ? selectedcouponId.Contains((int)j.CopcId) : true));
                    itemTransfers = itemTransfers
                        .Where(s => (s.Copname.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      ));
                    invRowChartLst = invRowChartLst.Where(s => (s.Copname.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      ));
                }
                else if (selectedTab == 1)
                {
                    selected = selectedOfferId;

                    //itemTransfers = fillList(Offers, chk_couponInvoice, chk_couponReturn, chk_couponDrafs, dp_couponStartDate, dp_couponStartDate, dt_couponStartTime, dt_couponEndTime).Where(j => (selectedOfferId.Count != 0 ? selectedOfferId.Contains((int)j.OofferId) : true));

                    itemTransfers = itemTransfers
                        .Where(s => (s.Oname.ToLower().Contains(txt_search.Text.ToLower()) ||
                              s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                              ));
                    invRowChartLst = invRowChartLst.Where(s => (s.Oname.ToLower().Contains(txt_search.Text.ToLower()) ||
                             s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                             ));
                    decimal total = 0;
                    total = itemTransfers.Select(b => b.subTotal.Value).Sum();
                    tb_total.Text = SectionData.DecTostring(total);
                }

                dgInvoice.ItemsSource = itemTransfers;
                txt_count.Text = dgInvoice.Items.Count.ToString();

                fillPieChart(selected);
                fillColumnChart(selected);
                fillRowChartCoupAndOffer(cb_Coupons, selected);

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
        private void Cb_Coupons_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            if(selectedTab == 0)
                combo.ItemsSource = dynamicComboCoupon.Where(p => p.Copname.ToLower().Contains(tb.Text.ToLower())).ToList();
            else if(selectedTab == 1)
                combo.ItemsSource = dynamicComboOffer.Where(p => p.Oname.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region reports
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();

            string addpath = "";

            string firstTitle = "promotion";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
    
            string selecteditems = "";
            string trSelecteditems = "";

           
         
            string startDate = "";
            string endDate = "";
            string startTime = "";
            string endTime = "";
           
             
            string searchval = "";
      
            string invtype = "";
        

            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            //trCoupons
            // trOffer

            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Sale\Promotion\Ar\ArCoupons.rdlc";
                    secondTitle = "coupon";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);

                    trSelecteditems = "trCoupons";
                    selecteditems = clsReports.stackToString(stk_tagsCoupons);
             
                }
                else
                {
                    addpath = @"\Reports\StatisticReport\Sale\Promotion\Ar\ArOffers.rdlc";
                    secondTitle = "offers";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trOffer";
                    selecteditems = clsReports.stackToString(stk_tagsOffers);
               
                }
            }
            else
            {
                //english
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Sale\Promotion\En\EnCoupons.rdlc";
                    secondTitle = "coupon";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trCoupons";
                    selecteditems = clsReports.stackToString(stk_tagsCoupons);
                 
                }
                else
                {
                    addpath = @"\Reports\StatisticReport\Sale\Promotion\En\EnOffers.rdlc";
                    secondTitle = "offers";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trOffer";
                    selecteditems = clsReports.stackToString(stk_tagsOffers);

                }
            }
            //filter


            startDate = dp_couponStartDate.SelectedDate != null ? SectionData.DateToString(dp_couponStartDate.SelectedDate) : "";

            endDate = dp_couponEndDate.SelectedDate != null ? SectionData.DateToString(dp_couponEndDate.SelectedDate) : "";
            startTime = dt_couponStartTime.SelectedTime != null ? dt_couponStartTime.Text : "";
            endTime = dt_couponEndTime.SelectedTime != null ? dt_couponEndTime.Text : "";
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("StartTimeVal", startTime));
            paramarr.Add(new ReportParameter("EndTimeVal", endTime));
   
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
             paramarr.Add(new ReportParameter("trStartTime", MainWindow.resourcemanagerreport.GetString("trStartTime")));
            paramarr.Add(new ReportParameter("trEndTime", MainWindow.resourcemanagerreport.GetString("trEndTime")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trCoupon", MainWindow.resourcemanagerreport.GetString("trCoupon")));
            paramarr.Add(new ReportParameter("trValue", MainWindow.resourcemanagerreport.GetString("trValue")));
            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trDiscount")));
            paramarr.Add(new ReportParameter("trOffer", MainWindow.resourcemanagerreport.GetString("trOffer")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
 
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            Title = MainWindow.resourcemanagerreport.GetString("trSalesReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            paramarr.Add(new ReportParameter("trSelecteditems", MainWindow.resourcemanagerreport.GetString(trSelecteditems)));
            paramarr.Add(new ReportParameter("selecteditems", selecteditems));
            clsReports.SalePromoStsReport(itemTransfers, rep, reppath, paramarr);
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
        IEnumerable<ItemTransferInvoice> itemTransfers = null;
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                //Thread t1 = new Thread(() =>
                //    {
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
