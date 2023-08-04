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
using System.Resources;
using System.Reflection;

namespace POS.View.purchases
{
    public partial class uc_statistic : UserControl
    {
        private int selectedTab = 0;
        private Statistics statisticModel = new Statistics();

        private List<ItemTransferInvoice> itemTransferInvoices = new List<ItemTransferInvoice>();

        private static uc_statistic _instance;
        public static uc_statistic Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_statistic();
                return _instance;
            }
        }
        public uc_statistic()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            itemTransferInvoices = await statisticModel.GetUserdailyPurinvoice((int)MainWindow.branchID, (int)MainWindow.userID, SectionData.DateTodbString(dp_invoiceDate.SelectedDate.Value.Date));

            dp_invoiceDate.SelectedDate = DateTime.Now;
            dp_orderDate.SelectedDate = DateTime.Now;

            chk_invoice.IsChecked = true;
            chk_orderInvoice.IsChecked = true;

            dgInvoice.ItemsSource = fillList();
        }

        private void fillInvoiceEvents()
        {
            dgInvoice.ItemsSource = fillList();
            fillColumnChart();
            fillRowChart();
            fillPieChart();
        }
        private void fillOrderEvents()
        {
            dgInvoice.ItemsSource = fillList();
            fillColumnChart();
            fillRowChart();
            fillPieChart();
        }
        private void fillQuotationEvents()
        {
            dgInvoice.ItemsSource = fillList();
            fillColumnChart();
            fillRowChart();
            fillPieChart();
        }

 
        private void Btn_Invoice_Click(object sender, RoutedEventArgs e)
        {
            selectedTab = 0;
            txt_search.Text = "";
            path_order.Fill = Brushes.White;
            bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
            ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_invoice);
            path_invoice.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            ReportsHelp.showTabControlGrid(grid_father, grid_invoice);
            ReportsHelp.isEnabledButtons(grid_tabControl, btn_invoice);
            fillInvoiceEvents();
        }

        private void Btn_order_Click(object sender, RoutedEventArgs e)
        {
            selectedTab = 1;
            txt_search.Text = "";
            path_invoice.Fill = Brushes.White;
            bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);
            ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_order);
            path_order.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            ReportsHelp.showTabControlGrid(grid_father, grid_order);
            ReportsHelp.isEnabledButtons(grid_tabControl, btn_order);
            fillOrderEvents();
        }


        private void Chk_orderInvoice_Checked(object sender, RoutedEventArgs e)
        {
            fillOrderEvents();
        }

        private void Chk_orderInvoice_Unchecked(object sender, RoutedEventArgs e)
        {
            fillOrderEvents();
        }

        private void Chk_orderReturn_Unchecked(object sender, RoutedEventArgs e)
        {
            fillOrderEvents();
        }

        private void Chk_orderReturn_Checked(object sender, RoutedEventArgs e)
        {
            fillOrderEvents();
        }

        private void Chk_orderDraft_Unchecked(object sender, RoutedEventArgs e)
        {
            fillOrderEvents();
        }

        private void Chk_orderDraft_Checked(object sender, RoutedEventArgs e)
        {
            fillOrderEvents();
        }

        private void Dp_orderDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillOrderEvents();
        }



        private void Dp_quotationDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillQuotationEvents();
        }

        private void Chk_quotationInvoice_Checked(object sender, RoutedEventArgs e)
        {
            fillQuotationEvents();
        }

        private void Chk_quotationInvoice_Unchecked(object sender, RoutedEventArgs e)
        {
            fillQuotationEvents();
        }

        private void Chk_quotationReturn_Unchecked(object sender, RoutedEventArgs e)
        {
            fillQuotationEvents();
        }

        private void Chk_quotationReturn_Checked(object sender, RoutedEventArgs e)
        {
            fillQuotationEvents();
        }

        private void Chk_quotationDraft_Unchecked(object sender, RoutedEventArgs e)
        {
            fillQuotationEvents();
        }

        private void Chk_quotationDraft_Checked(object sender, RoutedEventArgs e)
        {
            fillQuotationEvents();
        }



        private void chk_invoice_Checked(object sender, RoutedEventArgs e)
        {
            fillInvoiceEvents();
        }

        private void chk_invoice_Unchecked(object sender, RoutedEventArgs e)
        {
            fillInvoiceEvents();
        }

        private void chk_return_Checked(object sender, RoutedEventArgs e)
        {
            fillInvoiceEvents();
        }

        private void chk_return_Unchecked(object sender, RoutedEventArgs e)
        {
            fillInvoiceEvents();
        }

        private void chk_drafs_Unchecked(object sender, RoutedEventArgs e)
        {
            fillInvoiceEvents();
        }

        private void chk_drafs_Checked(object sender, RoutedEventArgs e)
        {
            fillInvoiceEvents();
        }

        private void Dp_invoiceDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillInvoiceEvents();
        }



        private List<ItemTransferInvoice> fillList()
        {
            var temp = itemTransferInvoices.Where(obj => (
            obj.updateUserId == MainWindow.userID
              && ((chk_invoice.IsChecked == true ? obj.invType == "p" || obj.invType == "pw" : false) || (chk_return.IsChecked == true ? obj.invType == "pb" || obj.invType == "pbw" : false) || (chk_invoice.IsChecked == true ? obj.invType == "pbd" || obj.invType == "pd" : false))
                   && (dp_invoiceDate.SelectedDate != null ? obj.updateDate.Value.Date.ToShortDateString() == dp_invoiceDate.SelectedDate.Value.Date.ToShortDateString() : true)
                )
                );
            if (selectedTab == 1)
            {
                temp = itemTransferInvoices.Where(obj => (obj.updateUserId == MainWindow.userID
                         && ((chk_orderInvoice.IsChecked == true ? obj.invType == "po" : false) || (chk_orderDraft.IsChecked == true ? obj.invType == "pod" : false))
                                   && (dp_orderDate.SelectedDate != null ? obj.updateDate.Value.Date.ToShortDateString() == dp_orderDate.SelectedDate.Value.Date.ToShortDateString() : true)
                                )
                                );
            }
            return temp.ToList();
        }

        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;

            var temp = itemTransferInvoices.Where(obj => (
            
                ((chk_invoice.IsChecked == true ? obj.invType == "p" || obj.invType == "pw" : false) || (chk_return.IsChecked == true ? obj.invType == "pb" || obj.invType == "pbw" : false) || (chk_invoice.IsChecked == true ? obj.invType == "pbd" || obj.invType == "pd" : false))
                    && (dp_invoiceDate.SelectedDate != null ? obj.updateDate.Value.Date.ToShortDateString() == dp_invoiceDate.SelectedDate.Value.Date.ToShortDateString() : true)
                 )
                 );
            if (selectedTab == 1)
            {
                temp = itemTransferInvoices.Where(obj => (
                          ((chk_orderInvoice.IsChecked == true ? obj.invType == "po" : false) || (chk_orderDraft.IsChecked == true ? obj.invType == "pod" : false))
                                   && (dp_orderDate.SelectedDate != null ? obj.updateDate.Value.Date.ToShortDateString() == dp_orderDate.SelectedDate.Value.Date.ToShortDateString() : true)
                                )
                                );
            }

            var titleTemp = temp.GroupBy(m => m.cUserAccName);

            var result = temp.GroupBy(s => new { s.updateUserId, s.cUserAccName }).Select(s => new
            {
                updateUserId = s.FirstOrDefault().updateUserId,
                cUserAccName = s.FirstOrDefault().cUserAccName,
                count = s.Count()
            });
            x = result.Select(m => m.count);
            int final1 = 0;
            int final = 0;


            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < x.Count(); i++)
            {

                List<string> lable = new List<string>();
                if (result.Select(obj => obj.updateUserId).Skip(i).FirstOrDefault() == MainWindow.userID)
                {
                    final = x.ToList().Skip(i).FirstOrDefault();
                    titles.Add(result.Select(jj => jj.cUserAccName).Skip(i).FirstOrDefault());
                }
                else
                {
                    final1 += x.ToList().Skip(i).FirstOrDefault();
                    titles.Add("Others");
                }
            }

            List<int> final2 = new List<int>();
            final2.Add(final);
            final2.Add(final1);
            for (int i = 0; i < 2; i++)
            {
                List<int> final3 = new List<int>();
                final3.Add(final2.Skip(i).FirstOrDefault());
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final3.AsChartValues(),
                      Title = titles.Skip(i).FirstOrDefault(),
                      DataLabels = true,
                  }
              );
            }
            chart1.Series = piechartData;
        }

        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<int> y = null;
            IEnumerable<int> z = null;

            var temp = fillList();
            var result = temp.GroupBy(s => s.updateUserId).Select(s => new
            {
                updateUserId = s.Key,
                countP = s.Where(m => m.invType == "p" ||m.invType=="pw").Count(),
                countPb = s.Where(m => m.invType == "pb" || m.invType == "pbw").Count(),
                countD = s.Where(m => m.invType == "pd" || m.invType == "pbd").Count()

            });
            if (selectedTab == 1)
            {
                result = temp.GroupBy(s => s.updateUserId).Select(s => new
                {
                    updateUserId = s.Key,
                    countP = s.Where(m => m.invType == "po").Count(),
                    countPb = 0,
                    countD = s.Where(m => m.invType == "pod").Count()

                });
            }

            x = result.Select(m => m.countP);
            y = result.Select(m => m.countPb);
            z = result.Select(m => m.countD);
            var tempName = temp.GroupBy(s => s.uUserAccName).Select(s => new
            {
                uUserName = s.Key
            });
            names.AddRange(tempName.Select(nn => nn.uUserName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();
            List<int> cD = new List<int>();
            List<string> titles = new List<string>()
            {
                "مبيعات","مرتجع","مسودة"
            };
            for (int i = 0; i < x.Count(); i++)
            {
                cP.Add(x.ToList().Skip(i).FirstOrDefault());
                cPb.Add(y.ToList().Skip(i).FirstOrDefault());
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
               Values = cPb.AsChartValues(),
               Title = titles[1],
               DataLabels = true,
           });
            columnChartData.Add(
           new StackedColumnSeries
           {
               Values = cD.AsChartValues(),
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
            var temp = fillList();
            var result = temp.GroupBy(s => s.updateUserId).Select(s => new
            {
                updateUserId = s.Key,
                totalP = s.Where(x => x.invType == "p" || x.invType == "pw").Sum(x => x.totalNet),
                totalPb = s.Where(m => m.invType == "pb" || m.invType == "pbw").Sum(x => x.totalNet)
            }
         );
            if (selectedTab == 1)
            {
                result = temp.GroupBy(s => s.updateUserId).Select(s => new
                {
                    updateUserId = s.Key,
                    totalP = s.Where(x => x.invType == "po").Sum(x => x.totalNet),
                    totalPb = s.Where(x => x.invType == "pod").Sum(x => x.totalNet)
                });
            }
       
            var resultTotal = result.Select(x => new { x.updateUserId, total = x.totalP - x.totalPb }).ToList();
            pTemp = result.Select(x => (decimal)x.totalP);
            pbTemp = result.Select(x => (decimal)x.totalPb);
            resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            var tempName = temp.GroupBy(s => s.uUserAccName).Select(s => new
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
                "اجمالي المبيعات","اجمالي المرتجع","صافي المبيعات"
            };
            for (int i = 0; i < pTemp.Count(); i++)
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
          });
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
    }
}
