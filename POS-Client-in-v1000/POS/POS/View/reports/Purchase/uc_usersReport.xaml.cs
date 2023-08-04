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
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.Threading;

namespace POS.View.reports
{
    public partial class uc_usersReport : UserControl
    {
        private int selectedTab = 0;

        Statistics statisticModel = new Statistics();

        List<Invoice> Invoices;

        List<Invoice> rowChartInvoice;

        //for combo boxes
        /*************************/
        Branch selectedBranch;
        Pos selectedPos;
        Agent selectedVendor;
        User selectedUser;

        List<Branch> comboBranches;
        List<Pos> comboPoss;
        List<Agent> comboVendors;
        List<User> comboUsers;

        ObservableCollection<Branch> comboBrachTemp = new ObservableCollection<Branch>();
        ObservableCollection<Pos> comboPosTemp = new ObservableCollection<Pos>();
        ObservableCollection<Agent> comboVendorTemp = new ObservableCollection<Agent>();
        ObservableCollection<User> comboUserTemp = new ObservableCollection<User>();

        ObservableCollection<Branch> dynamicComboBranches;
        ObservableCollection<Pos> dynamicComboPoss;
        ObservableCollection<Agent> dynamicComboVendors;
        ObservableCollection<User> dynamicComboUsers;

        Branch branchModel = new Branch();
        Pos posModel = new Pos();
        Agent agentModel = new Agent();
        User userModel = new User();
        /*************************/

        Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

        ObservableCollection<int> selectedBranchId = new ObservableCollection<int>();
        ObservableCollection<int> selectedPosId = new ObservableCollection<int>();
        ObservableCollection<int> selectedVendorsId = new ObservableCollection<int>();
        ObservableCollection<int> selectedUserId = new ObservableCollection<int>();

        public string[] Labels { get; set; }
        public string[] Formatter { get; set; }
        private static uc_usersReport _instance;

        // report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        public static uc_usersReport Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_usersReport();
                return _instance;
            }
        }

        public uc_usersReport()
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
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                List<Invoice> templist = new List<Invoice>();
                templist= await statisticModel.GetPurinv((int)MainWindow.branchID, (int)MainWindow.userID);

                Invoices = templist;
                rowChartInvoice = templist;

                comboBranches = await branchModel.GetAllWithoutMain("b");
                comboPoss = await posModel.Get();
                comboVendors = await agentModel.Get("v");
                comboUsers = await userModel.Get();

                dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
                dynamicComboPoss = new ObservableCollection<Pos>(comboPoss);
                dynamicComboVendors = new ObservableCollection<Agent>(comboVendors);
                dynamicComboUsers = new ObservableCollection<User>(comboUsers);

                fillComboBranches();
                fillComboPos();
                fillComboUsers();
                fillComboVendors();

                chk_invoice.IsChecked = true;
                chk_posInvoice.IsChecked = true;
                chk_vendorsInvoice.IsChecked = true;
                chk_usersInvoice.IsChecked = true;
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_branch.Tag.ToString());
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

        private void fillComboBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = dynamicComboBranches;
        }

        private void fillComboPos()
        {
            cb_pos.SelectedValuePath = "posId";
            cb_pos.DisplayMemberPath = "name";
            cb_pos.ItemsSource = dynamicComboPoss;
        }

        private void fillComboVendors()
        {
            cb_vendors.SelectedValuePath = "agentId";
            cb_vendors.DisplayMemberPath = "name";
            cb_vendors.ItemsSource = dynamicComboVendors;
        }

        private void fillComboUsers()
        {
            cb_users.SelectedValuePath = "userId";
            cb_users.DisplayMemberPath = "username";
            cb_users.ItemsSource = dynamicComboUsers;
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

        private IEnumerable<Invoice> fillList(CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft
           , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            fillDates(startDate, endDate, startTime, endTime);
            var result = Invoices.Where(x => (

              (txt_search.Text != null ? x.invNumber.ToLower().Contains(txt_search.Text.ToLower())
              || x.invType.ToLower().Contains(txt_search.Text.ToLower())
              || x.discountType.ToLower().Contains(txt_search.Text.ToLower()) : true)
              && ((chkDraft.IsChecked == true ? (x.invType == "pd" || x.invType == "pbd") : false)
                          || (chkReturn.IsChecked == true ? (x.invType == "pb") : false)
                          || (chkInvoice.IsChecked == true ? (x.invType == "p") : false))
                      && (startDate.SelectedDate != null ? x.invDate >= startDate.SelectedDate : true)
                      && (endDate.SelectedDate != null ? x.invDate <= endDate.SelectedDate : true)
                      && (startTime.SelectedTime != null ? x.invDate >= startTime.SelectedTime : true)
                      && (endTime.SelectedTime != null ? x.invDate <= endTime.SelectedTime : true)));
            return result;
        }

        private IEnumerable<Invoice> fillRowChartList(CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft
          , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            fillDates(startDate, endDate, startTime, endTime);
            var result = Invoices.Where(x => ((txt_search.Text != null ? x.invNumber.ToLower().Contains(txt_search.Text.ToLower())
              || x.invType.ToLower().Contains(txt_search.Text.ToLower())
              || x.discountType.ToLower().Contains(txt_search.Text.ToLower()) : true)
              &&
                         (startDate.SelectedDate != null ? x.invDate >= startDate.SelectedDate : true)
                        && (endDate.SelectedDate != null ? x.invDate <= endDate.SelectedDate : true)
                        && (startTime.SelectedTime != null ? x.invDate >= startTime.SelectedTime : true)
                        && (endTime.SelectedTime != null ? x.invDate <= endTime.SelectedTime : true)));
            return result;
        }

        private void fillPieChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;
            if (selectedTab == 0)
            {
                titles.Clear();
                var temp = fillList(chk_invoice, chk_return, chk_drafs, dp_startDate, dp_endDate, dt_startTime, dt_endTime);
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                var titleTemp = temp.GroupBy(m => m.branchCreatorName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new { branchCreatorId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 1)
            {
                titles.Clear();
                var temp = fillList(chk_posInvoice, chk_posReturn, chk_posDraft, dp_posStartDate, dp_posEndDate, dt_posStartTime, dt_posEndTime);
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                var titleTemp = temp.GroupBy(m => new { m.posName, m.posId });
                titles.AddRange(titleTemp.Select(jj => jj.Key.posName));
                var result = temp.GroupBy(s => s.posId).Select(s => new { posId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 2)
            {
                titles.Clear();
                var temp = fillList(chk_vendorsInvoice, chk_vendorsReturn, chk_vendorsDraft, dp_vendorsStartDate, dp_vendorsEndDate, dt_vendorsStartTime, dt_vendorsEndTime);
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                var titleTemp = temp.GroupBy(m => m.agentName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.agentId).Select(s => new { agentId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 3)
            {
                titles.Clear();
                var temp = fillList(chk_usersInvoice, chk_usersReturn, chk_usersDraft, dp_usersStartDate, dp_usersEndDate, dt_usersStartTime, dt_usersEndTime);
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                var titleTemp = temp.GroupBy(m => m.cUserAccName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.createUserId).Select(s => new { userId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < x.Count(); i++)
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
            chart1.Series = piechartData;
        }

        private void fillColumnChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            IEnumerable<int> x = null;
            IEnumerable<int> y = null;
            IEnumerable<int> z = null;
            if (selectedTab == 0)
            {
                var temp = fillList(chk_invoice, chk_return, chk_drafs, dp_startDate, dp_endDate, dt_startTime, dt_endTime);
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    countP = s.Where(m => m.invType == "p").Count(),
                    countPb = s.Where(m => m.invType == "pb").Count(),
                    countD = s.Where(m => m.invType == "pd" || m.invType == "pbd").Count()
                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
            }
            else if (selectedTab == 1)
            {
                var temp = fillList(chk_posInvoice, chk_posReturn, chk_posDraft, dp_posStartDate, dp_posEndDate, dt_posStartTime, dt_posEndTime);
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                var result = temp.GroupBy(s => s.posId).Select(s => new
                {
                    posId = s.Key,
                    countP = s.Where(m => m.invType == "p").Count(),
                    countPb = s.Where(m => m.invType == "pb").Count(),
                    countD = s.Where(m => m.invType == "pd" || m.invType == "pbd").Count()
                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
            }
            else if (selectedTab == 2)
            {
                var temp = fillList(chk_vendorsInvoice, chk_vendorsReturn, chk_vendorsDraft, dp_vendorsStartDate, dp_vendorsEndDate, dt_vendorsStartTime, dt_vendorsEndTime);
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                var result = temp.GroupBy(s => s.agentId).Select(s => new
                {
                    agentId = s.Key,
                    countP = s.Where(m => m.invType == "p").Count(),
                    countPb = s.Where(m => m.invType == "pb").Count(),
                    countD = s.Where(m => m.invType == "pd" || m.invType == "pbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
            }
            else if (selectedTab == 3)
            {
                var temp = fillList(chk_usersInvoice, chk_usersReturn, chk_usersDraft, dp_usersStartDate, dp_usersEndDate, dt_usersStartTime, dt_usersEndTime);
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                var result = temp.GroupBy(s => s.createUserId).Select(s => new
                {
                    createUserId = s.Key,
                    countP = s.Where(m => m.invType == "p").Count(),
                    countPb = s.Where(m => m.invType == "pb").Count(),
                    countD = s.Where(m => m.invType == "pd" || m.invType == "pbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
            }
            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();
            List<int> cD = new List<int>();
            List<string> titles = new List<string>()
            {
                "مشتريات","مرتجع","مسودة"
            };
            for (int i = 0; i < x.Count(); i++)
            {
                cP.Add(x.ToList().Skip(i).FirstOrDefault());
                cPb.Add(y.ToList().Skip(i).FirstOrDefault());
                cD.Add(z.ToList().Skip(i).FirstOrDefault());
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

        private void fillRowChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            IEnumerable<decimal> pTemp = null;
            IEnumerable<decimal> pbTemp = null;
            IEnumerable<decimal> resultTemp = null;

            if (selectedTab == 0)
            {
                var temp = fillRowChartList(chk_invoice, chk_return, chk_drafs, dp_startDate, dp_endDate, dt_startTime, dt_endTime);
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    totalP = s.Where(x => x.invType == "p").Sum(x => x.totalNet),
                    totalPb = s.Where(x => x.invType == "pb").Sum(x => x.totalNet)
                }
             );
                var resultTotal = result.Select(x => new { x.branchCreatorId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP);
            }
            if (selectedTab == 1)
            {
                var temp = fillRowChartList(chk_posInvoice, chk_posReturn, chk_posDraft, dp_posStartDate, dp_posEndDate, dt_posStartTime, dt_posEndTime);
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                var result = temp.GroupBy(s => s.posId).Select(s => new
                {
                    posId = s.Key,
                    totalP = s.Where(x => x.invType == "p").Sum(x => x.totalNet),
                    totalPb = s.Where(x => x.invType == "pb").Sum(x => x.totalNet)
                }
             );
                var resultTotal = result.Select(x => new { x.posId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP);
            }
            if (selectedTab == 2)
            {
                var temp = fillRowChartList(chk_vendorsInvoice, chk_vendorsReturn, chk_vendorsDraft, dp_vendorsStartDate, dp_vendorsEndDate, dt_vendorsStartTime, dt_vendorsEndTime);
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                var result = temp.GroupBy(s => s.agentId).Select(s => new
                {
                    agentId = s.Key,
                    totalP = s.Where(x => x.invType == "p").Sum(x => x.totalNet),
                    totalPb = s.Where(x => x.invType == "pb").Sum(x => x.totalNet)
                }
             );
                var resultTotal = result.Select(x => new { x.agentId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP);
            }
            if (selectedTab == 3)
            {
                var temp = fillRowChartList(chk_usersInvoice, chk_usersReturn, chk_usersDraft, dp_usersStartDate, dp_usersEndDate, dt_usersStartTime, dt_usersEndTime);
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                var result = temp.GroupBy(s => s.createUserId).Select(s => new
                {
                    createUserId = s.Key,
                    totalP = s.Where(x => x.invType == "p").Sum(x => x.totalNet),
                    totalPb = s.Where(x => x.invType == "pb").Sum(x => x.totalNet)
                }
             );
                var resultTotal = result.Select(x => new { x.createUserId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP);
            }


            SeriesCollection rowChartData = new SeriesCollection();
            List<decimal> purchase = new List<decimal>();
            List<decimal> returns = new List<decimal>();
            List<decimal> sub = new List<decimal>();
            List<string> titles = new List<string>()
            {
                         "اجمالي المشتريات","اجمالي المرتجع","صافي المشتريات"
            };
            for (int i = 0; i < pbTemp.Count(); i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());
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

        public void fillBranchEvent()
        {
            var temp = fillList(chk_invoice, chk_return, chk_drafs, dp_startDate, dp_endDate, dt_startTime, dt_endTime);
            dgInvoice.ItemsSource = temp.Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));
            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillPieChart(cb_branches, selectedBranchId);
            fillColumnChart(cb_branches, selectedBranchId);
            fillRowChart(cb_branches, selectedBranchId);
        }
        public void fillPosEvent()
        {
            var temp = fillList(chk_posInvoice, chk_posReturn, chk_posDraft, dp_posStartDate, dp_posEndDate, dt_posStartTime, dt_posEndTime);
            dgInvoice.ItemsSource = temp.Where(j => (selectedPosId.Count != 0 ? selectedPosId.Contains((int)j.posId) : true));
            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillRowChart(cb_pos, selectedPosId);
            fillPieChart(cb_pos, selectedPosId);
            fillColumnChart(cb_pos, selectedPosId);
        }
        public void fillVendorsEvent()
        {
            var temp = fillList(chk_vendorsInvoice, chk_vendorsReturn, chk_vendorsDraft, dp_vendorsStartDate, dp_vendorsEndDate, dt_vendorsStartTime, dt_vendorsEndTime);
            dgInvoice.ItemsSource = temp.Where(j => (selectedVendorsId.Count != 0 ? selectedVendorsId.Contains((int)j.agentId) : true));
            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillPieChart(cb_vendors, selectedVendorsId);
            fillColumnChart(cb_vendors, selectedVendorsId);
            fillRowChart(cb_vendors, selectedVendorsId);
        }
        public void fillUsersEvent()
        {
            var temp = fillList(chk_usersInvoice, chk_usersReturn, chk_usersDraft, dp_usersStartDate, dp_usersEndDate, dt_usersStartTime, dt_usersEndTime);
            dgInvoice.ItemsSource = temp.Where(j => (selectedUserId.Count != 0 ? selectedUserId.Contains((int)j.updateUserId) : true));
            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillPieChart(cb_users, selectedUserId);
            fillColumnChart(cb_users, selectedUserId);
            fillRowChart(cb_users, selectedUserId);
        }

        #region Functions
        private void hideSatacks()
        {
            stk_tagsBranches.Visibility = Visibility.Collapsed;
            stk_tagsItems.Visibility = Visibility.Collapsed;
            stk_tagsPos.Visibility = Visibility.Collapsed;
            stk_tagsUsers.Visibility = Visibility.Collapsed;
            stk_tagsVendors.Visibility = Visibility.Collapsed;
        }

        public void paint()
        {
            bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_branch.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_pos.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_vendors.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_users.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_items.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_branch.Fill = Brushes.White;
            path_pos.Fill = Brushes.White;
            path_vendors.Fill = Brushes.White;
            path_users.Fill = Brushes.White;
            path_items.Fill = Brushes.White;

            grid_branch.Visibility = Visibility.Hidden;
            grid_pos.Visibility = Visibility.Hidden;
            grid_vendors.Visibility = Visibility.Hidden;
            grid_users.Visibility = Visibility.Hidden;
            grid_items.Visibility = Visibility.Hidden;

            col_branch.Visibility = Visibility.Collapsed;
            col_item.Visibility = Visibility.Collapsed;
            col_pos.Visibility = Visibility.Collapsed;
            col_user.Visibility = Visibility.Collapsed;
            col_vendor.Visibility = Visibility.Collapsed;
        }

        private void isEnabledButtons()
        {
            btn_branch.IsEnabled = true;
            btn_pos.IsEnabled = true;
            btn_vendors.IsEnabled = true;
            btn_users.IsEnabled = true;
            btn_items.IsEnabled = true;
        }
        #endregion

        #region Events
        #region tabControl
        private void btn_branch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                hideSatacks();
                stk_tagsBranches.Visibility = Visibility.Visible;
                selectedTab = 0;
                paint();
                col_branch.Visibility = Visibility.Visible;
                bdr_branch.Background = Brushes.White;
                path_branch.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_branch.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_branch.IsEnabled = false;
                btn_branch.Opacity = 1;
                fillBranchEvent();
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
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

        private void btn_pos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                hideSatacks();
                stk_tagsPos.Visibility = Visibility.Visible;
                selectedTab = 1;
                paint();
                col_pos.Visibility = Visibility.Visible;
                col_branch.Visibility = Visibility.Visible;
                bdr_pos.Background = Brushes.White;
                path_pos.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_pos.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_pos.IsEnabled = false;
                btn_pos.Opacity = 1;
                fillPosEvent();
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
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

        private void btn_vendors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                hideSatacks();
                stk_tagsVendors.Visibility = Visibility.Visible;
                selectedTab = 2;
                paint();
                col_vendor.Visibility = Visibility.Visible;
                bdr_vendors.Background = Brushes.White;
                path_vendors.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_vendors.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_vendors.IsEnabled = false;
                btn_vendors.Opacity = 1;
                fillVendorsEvent();
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
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

        private void btn_users_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                hideSatacks();
                stk_tagsUsers.Visibility = Visibility.Visible;
                selectedTab = 3;
                paint();
                col_user.Visibility = Visibility.Visible;
                bdr_users.Background = Brushes.White;
                path_users.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_users.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_users.IsEnabled = false;
                btn_users.Opacity = 1;
                fillUsersEvent();
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
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

        private void btn_items_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                txt_search.Text = "";
                selectedTab = 4;
                paint();
                col_item.Visibility = Visibility.Visible;
                bdr_items.Background = Brushes.White;
                path_items.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
                grid_items.Visibility = Visibility.Visible;
                isEnabledButtons();
                btn_items.IsEnabled = false;
                btn_items.Opacity = 1;
                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
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

        #region refreshButton
        private void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    cb_branches.SelectedItem = null;
                    chk_drafs.IsChecked = false;
                    chk_return.IsChecked = false;
                    chk_invoice.IsChecked = true;
                    dp_endDate.SelectedDate = null;
                    dp_startDate.SelectedDate = null;
                    dt_startTime.SelectedTime = null;
                    dt_endTime.SelectedTime = null;
                    chk_allBranches.IsChecked = false;
                    cb_branches.IsEnabled = true;
                    for (int i = 0; i < comboBrachTemp.Count; i++)
                    {
                        dynamicComboBranches.Add(comboBrachTemp.Skip(i).FirstOrDefault());
                    }
                    comboBrachTemp.Clear();
                    stk_tagsBranches.Children.Clear();
                    selectedBranchId.Clear();
                    fillBranchEvent();
                }

                else if (selectedTab == 1)
                {
                    cb_pos.SelectedItem = null;
                    chk_posDraft.IsChecked = false;
                    chk_posReturn.IsChecked = false;
                    chk_posInvoice.IsChecked = true;
                    dp_posEndDate.SelectedDate = null;
                    dp_posStartDate.SelectedDate = null;
                    dt_posStartTime.SelectedTime = null;
                    dt_posEndTime.SelectedTime = null;
                    chk_allPos.IsChecked = false;
                    cb_pos.IsEnabled = true;
                    for (int i = 0; i < comboPosTemp.Count; i++)
                    {
                        dynamicComboPoss.Add(comboPosTemp.Skip(i).FirstOrDefault());
                    }
                    comboPosTemp.Clear();
                    stk_tagsPos.Children.Clear();
                    selectedPosId.Clear();
                    fillPosEvent();
                }

                else if (selectedTab == 2)
                {
                    cb_vendors.SelectedItem = null;
                    chk_vendorsDraft.IsChecked = false;
                    chk_vendorsReturn.IsChecked = false;
                    chk_vendorsInvoice.IsChecked = true;
                    dp_vendorsEndDate.SelectedDate = null;
                    dp_vendorsStartDate.SelectedDate = null;
                    dt_vendorsStartTime.SelectedTime = null;
                    dt_vendorsEndTime.SelectedTime = null;
                    chk_allVendors.IsChecked = false;
                    cb_vendors.IsEnabled = true;
                    for (int i = 0; i < comboVendorTemp.Count; i++)
                    {
                        dynamicComboVendors.Add(comboVendorTemp.Skip(i).FirstOrDefault());
                    }
                    comboVendorTemp.Clear();
                    stk_tagsVendors.Children.Clear();
                    selectedVendorsId.Clear();
                    fillVendorsEvent();

                }

                else if (selectedTab == 3)
                {
                    cb_users.SelectedItem = null;
                    chk_usersDraft.IsChecked = false;
                    chk_usersReturn.IsChecked = false;
                    chk_usersInvoice.IsChecked = true;
                    dp_usersEndDate.SelectedDate = null;
                    dp_usersStartDate.SelectedDate = null;
                    dt_usersStartTime.SelectedTime = null;
                    dt_usersEndTime.SelectedTime = null;
                    chk_allUsers.IsChecked = false;
                    cb_users.IsEnabled = true;
                    for (int i = 0; i < comboUserTemp.Count; i++)
                    {
                        dynamicComboUsers.Add(comboUserTemp.Skip(i).FirstOrDefault());
                    }
                    comboUserTemp.Clear();
                    stk_tagsUsers.Children.Clear();
                    selectedUserId.Clear();
                    fillUsersEvent();

                }
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

        #region branchesEvents
        private void chk_invoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void chk_return_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void chk_drafs_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void chk_invoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void chk_return_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void chk_drafs_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void dp_startDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void dp_endDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void dt_startTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void dp_endTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillBranchEvent();
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

        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    fillBranchEvent();
                }
                if (selectedTab == 1)
                {
                    fillPosEvent();
                }
                if (selectedTab == 2)
                {
                    fillVendorsEvent();
                }
                if (selectedTab == 3)
                {
                    fillUsersEvent();
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

        #region posEvents
        private void chk_posInvoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void chk_posReturn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void chk_posDrafs_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void chk_posInvoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void chk_posReturn_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void chk_posDrafs_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void dp_posStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void dp_posEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void dt_posStartTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        private void dt_posEndTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                fillPosEvent();
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

        #region vendorsEvents
        private void chk_vendorsInvoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void chk_vendorsReturn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void chk_vendorsDraft_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void chk_vendorsInvoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void chk_vendorsReturn_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void chk_vendorsDraft_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void dp_vendorsStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void dp_vendorsEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void dt_vendorsStartTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        private void dt_vendorsEndTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillVendorsEvent();
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

        #region usersEvents
        private void chk_usersInvoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void chk_usersReturn_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void chk_usersDraft_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void chk_usersInvoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void chk_usersReturn_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void chk_usersDraft_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void dp_usersStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void dp_usersEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void dt_usersEndTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        private void dt_usersStartTime_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                fillUsersEvent();
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

        #endregion

        private void Chip_OnDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                var currentChip = (Chip)sender;
                stk_tagsBranches.Children.Remove(currentChip);
                var m = comboBrachTemp.Where(j => j.branchId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                dynamicComboBranches.Add(m.FirstOrDefault());
                selectedBranchId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                if (selectedBranchId.Count == 0)
                {
                    cb_branches.SelectedItem = null;
                }
                fillBranchEvent();
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

        private void Chip_OnDeletePosClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                var currentChip = (Chip)sender;
                stk_tagsPos.Children.Remove(currentChip);
                var m = comboPosTemp.Where(j => j.posId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                dynamicComboPoss.Add(m.FirstOrDefault());
                selectedPosId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                if (selectedPosId.Count == 0)
                {
                    cb_pos.SelectedItem = null;
                }
                fillPosEvent();
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

        private void Chip_OnDeleteVendorClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                var currentChip = (Chip)sender;
                stk_tagsVendors.Children.Remove(currentChip);
                var m = comboVendorTemp.Where(j => j.agentId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                dynamicComboVendors.Add(m.FirstOrDefault());
                selectedVendorsId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                if (selectedVendorsId.Count == 0)
                {
                    cb_vendors.SelectedItem = null;
                }
                fillVendorsEvent();
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

        private void Chip_OnDeleteUserClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                var currentChip = (Chip)sender;
                stk_tagsUsers.Children.Remove(currentChip);
                var m = comboUserTemp.Where(j => j.userId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                dynamicComboUsers.Add(m.FirstOrDefault());
                selectedUserId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                if (selectedUserId.Count == 0)
                {
                    cb_users.SelectedItem = null;
                }
                fillUsersEvent();
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

        private void cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_branches.SelectedItem != null)
                {
                    if (stk_tagsBranches.Children.Count < 5)
                    {
                        selectedBranch = cb_branches.SelectedItem as Branch;
                        var b = new MaterialDesignThemes.Wpf.Chip()
                        {
                            Content = selectedBranch.name,
                            Name = "btn" + selectedBranch.branchId.ToString(),
                            IsDeletable = true,
                            Margin = new Thickness(5, 0, 5, 0)
                        };
                        b.DeleteClick += Chip_OnDeleteClick;
                        stk_tagsBranches.Children.Add(b);
                        comboBrachTemp.Add(selectedBranch);
                        selectedBranchId.Add(selectedBranch.branchId);
                        dynamicComboBranches.Remove(selectedBranch);
                        fillBranchEvent();
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

        private void cb_pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_pos.SelectedItem != null)
                {
                    if (stk_tagsPos.Children.Count < 5)
                    {
                        selectedPos = cb_pos.SelectedItem as Pos;
                        var b = new MaterialDesignThemes.Wpf.Chip()
                        {
                            Content = selectedPos.name,
                            Name = "btn" + selectedPos.posId.ToString(),
                            IsDeletable = true,
                            Margin = new Thickness(5, 0, 5, 0)
                        };
                        b.DeleteClick += Chip_OnDeletePosClick;
                        stk_tagsPos.Children.Add(b);
                        comboPosTemp.Add(selectedPos);
                        selectedPosId.Add(selectedPos.posId);
                        dynamicComboPoss.Remove(selectedPos);
                        fillPosEvent();
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

        private void cb_vendors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_vendors.SelectedItem != null)
                {
                    if (stk_tagsVendors.Children.Count < 5)
                    {
                        selectedVendor = cb_vendors.SelectedItem as Agent;
                        var b = new MaterialDesignThemes.Wpf.Chip()
                        {
                            Content = selectedVendor.name,
                            Name = "btn" + selectedVendor.agentId.ToString(),
                            IsDeletable = true,
                            Margin = new Thickness(5, 0, 5, 0)
                        };
                        b.DeleteClick += Chip_OnDeleteVendorClick;
                        stk_tagsVendors.Children.Add(b);
                        comboVendorTemp.Add(selectedVendor);
                        selectedVendorsId.Add(selectedVendor.agentId);
                        dynamicComboVendors.Remove(selectedVendor);
                        fillVendorsEvent();
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

        private void cb_users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_users.SelectedItem != null)
                {
                    if (stk_tagsUsers.Children.Count < 5)
                    {
                        selectedUser = cb_users.SelectedItem as User;
                        var b = new MaterialDesignThemes.Wpf.Chip()
                        {
                            Content = selectedUser.username,
                            Name = "btn" + selectedUser.userId.ToString(),
                            IsDeletable = true,
                            Margin = new Thickness(5, 0, 5, 0)
                        };
                        b.DeleteClick += Chip_OnDeleteUserClick;
                        stk_tagsUsers.Children.Add(b);
                        comboUserTemp.Add(selectedUser);
                        selectedUserId.Add(selectedUser.userId);
                        dynamicComboUsers.Remove(selectedUser);
                        fillUsersEvent();
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

        private void chk_allBranches_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_branches.IsEnabled == true)
                {
                    cb_branches.SelectedItem = null;
                    cb_branches.IsEnabled = false;
                    for (int i = 0; i < comboBrachTemp.Count; i++)
                    {
                        dynamicComboBranches.Add(comboBrachTemp.Skip(i).FirstOrDefault());
                    }
                    comboBrachTemp.Clear();
                    stk_tagsBranches.Children.Clear();
                    selectedBranchId.Clear();

                }
                else
                {
                    cb_branches.IsEnabled = true;
                }
                fillBranchEvent();
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

        private void chk_allPos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_pos.IsEnabled == true)
                {
                    cb_pos.SelectedItem = null;
                    cb_pos.IsEnabled = false;
                    for (int i = 0; i < comboPosTemp.Count; i++)
                    {
                        dynamicComboPoss.Add(comboPosTemp.Skip(i).FirstOrDefault());
                    }
                    comboPosTemp.Clear();
                    stk_tagsPos.Children.Clear();
                    selectedPosId.Clear();
                }
                else
                {
                    cb_pos.IsEnabled = true;
                }

                fillPosEvent();
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

        private void chk_allVendors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_vendors.IsEnabled == true)
                {
                    cb_vendors.SelectedItem = null;
                    cb_vendors.IsEnabled = false;
                    for (int i = 0; i < comboVendorTemp.Count; i++)
                    {
                        dynamicComboVendors.Add(comboVendorTemp.Skip(i).FirstOrDefault());
                    }
                    comboVendorTemp.Clear();
                    stk_tagsVendors.Children.Clear();
                    selectedVendorsId.Clear();
                }
                else
                {
                    cb_vendors.IsEnabled = true;
                }

                fillVendorsEvent();
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

        private void chk_allUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_users.IsEnabled == true)
                {
                    cb_users.SelectedItem = null;
                    cb_users.IsEnabled = false;
                    for (int i = 0; i < comboUserTemp.Count; i++)
                    {
                        dynamicComboUsers.Add(comboUserTemp.Skip(i).FirstOrDefault());
                    }
                    comboUserTemp.Clear();
                    stk_tagsUsers.Children.Clear();
                    selectedUserId.Clear();
                }
                else
                {
                    cb_users.IsEnabled = true;
                }

                fillUsersEvent();
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

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    pdfUser();
                });
                t1.Start();
                //////////////////////////////////////

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

        private void pdfUser()
        {
            BuildReport();

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

        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //Thread t1 = new Thread(() =>
                //{
                    ExcelUser();
                //});
                //t1.Start();

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

        private void ExcelUser()
        {
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
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                Window.GetWindow(this).Opacity = 0.2;
                /////////////////////
                string pdfpath = "";
                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);
                BuildReport();
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

        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArVendor.rdlc";////////?????????
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\Ar\ArCustomer.rdlc";/////////?????????
                }
            }
            else
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\Vendor.rdlc";//////////////????????????
                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Accounts\Recipient\En\Customer.rdlc";//////////////????????????
                }
            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            //clsReports.packageReport(temp , rep, reppath, paramarr);/////////////////????????????????
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                /////////////////////////////////////
                Thread t1 = new Thread(() =>
                {
                    printUser();
                });
                t1.Start();
                //////////////////////////////////////

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

        private void printUser()
        {
            BuildReport();

            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
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
                /*
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
    }
}



