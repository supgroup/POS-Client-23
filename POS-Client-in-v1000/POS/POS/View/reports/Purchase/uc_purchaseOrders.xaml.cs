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
using POS.View.purchases;
using System.Resources;
using System.Reflection;

namespace POS.View.reports
{

    public partial class uc_purchaseOrders : UserControl
    {
        public uc_purchaseOrders()
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

        #region variables
        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private int selectedTab = 0;

        Statistics statisticModel = new Statistics();

        List<ItemTransferInvoice> Invoices;

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

        ObservableCollection<int> selectedBranchId = new ObservableCollection<int>();
        ObservableCollection<int> selectedPosId = new ObservableCollection<int>();
        ObservableCollection<int> selectedVendorsId = new ObservableCollection<int>();
        ObservableCollection<int> selectedUserId = new ObservableCollection<int>();
        #endregion

        private static uc_purchaseOrders _instance;
        public static uc_purchaseOrders Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_purchaseOrders();
                return _instance;
            }
        }

        private List<ItemTransferInvoice> converter(List<ItemTransferInvoice> query)
        {
            foreach (var item in query)
            {
                if (item.invType == "p")
                {
                    item.invType = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                }
                else if (item.invType == "pw")
                {
                    item.invType = MainWindow.resourcemanager.GetString("trPurchaseInvoice");
                }
                else if (item.invType == "pb")
                {
                    item.invType = MainWindow.resourcemanager.GetString("trPurchaseReturnInvoice");
                }
                else if (item.invType == "pd")
                {
                    item.invType = MainWindow.resourcemanager.GetString("trDraftPurchaseBill");
                }
                else if (item.invType == "pbd")
                {
                    item.invType = MainWindow.resourcemanager.GetString("trPurchaseReturnDraft");
                }
            }
            return query;

        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Invoices = await statisticModel.GetPurorderitemcount((int)MainWindow.branchID, (int)MainWindow.userID);

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
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                #region branch
                //comboBranches = await branchModel.GetAllWithoutMain("b");
                if (FillCombo.branchesAllWithoutMainReport is null)
                    await FillCombo.RefreshBranchsWithoutMainReport();
                comboBranches = FillCombo.branchesAllWithoutMainReport;
                #endregion

                #region pos
                //comboPoss = await posModel.Get();
                if (FillCombo.posAllReport is null)
                    await FillCombo.RefreshPosAllReport();
                comboPoss = FillCombo.posAllReport;
                #endregion

                #region vendor
                //comboVendors = await agentModel.Get("v");
                if (FillCombo.vendorsListReport is null)
                    await FillCombo.RefreshVendorAllReport();
                comboVendors = FillCombo.vendorsListReport;
                #endregion

                #region user
                //comboUsers = await userModel.Get();
                if (FillCombo.usersList is null)
                    await FillCombo.RefreshUsers();
                comboUsers = FillCombo.usersList;
                #endregion


                dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
                dynamicComboPoss = new ObservableCollection<Pos>(comboPoss);
                dynamicComboVendors = new ObservableCollection<Agent>(comboVendors);
                dynamicComboUsers = new ObservableCollection<User>(comboUsers);

                fillComboBranches();

                selectedTab = 0;
                changeTabReset();
              //  btn_branch_Click(btn_branch, null);

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

        #region charts
        private void fillPieChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;

            //var temp = invLst;
            var temp = list;

            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true)).ToList();
                var titleTemp = temp.GroupBy(m => m.branchCreatorName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new { branchCreatorId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true)).ToList();
                var titleTemp = temp.GroupBy(m => new { m.posName, m.posId });
                titles.AddRange(titleTemp.Select(jj => jj.Key.posName));
                var result = temp.GroupBy(s => s.posId).Select(s => new { posId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 2)
            {
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true)).ToList();
                var titleTemp = temp.GroupBy(m => m.agentName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.agentId).Select(s => new { agentId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 3)
            {
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true)).ToList();
                var titleTemp = temp.GroupBy(m => m.cUserAccName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.updateUserId).Select(s => new { updateUserId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
           
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
            if(x.Count() > 6)
            {
                int finalSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    finalSum = finalSum + x.ToList().Skip(i).FirstOrDefault();
                }
                if(finalSum != 0)
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
        private void fillColumnChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<int> x = null;

            //var temp = invLst;
            var temp = list;

            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true)).ToList();
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    countP = s.Count(),
                });
                x = result.Select(m => m.countP);
                var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
                {
                    branchName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.branchName));
            }
            else if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true)).ToList();
                var result = temp.GroupBy(s => s.posId).Select(s => new
                {
                    posId = s.Key,
                    countP = s.Count(),
                });
                x = result.Select(m => m.countP);
                var tempName = temp.GroupBy(s => s.posName).Select(s => new
                {
                    posName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.posName));
            }
            else if (selectedTab == 2)
            {
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true)).ToList();
                var result = temp.GroupBy(s => s.agentId).Select(s => new
                {
                    agentId = s.Key,
                    countP = s.Count(),
                });
                x = result.Select(m => m.countP);
                var tempName = temp.GroupBy(s => s.agentName).Select(s => new
                {
                    vendorName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.vendorName));
            }
            else if (selectedTab == 3)
            {
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true)).ToList();
                var result = temp.GroupBy(s => s.updateUserId).Select(s => new
                {
                    updateUserId = s.Key,
                    countP = s.Count(),

                });
                x = result.Select(m => m.countP);
                var tempName = temp.GroupBy(s => s.uUserAccName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }

            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();

            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();
            for (int i = 0; i < xCount; i++)
            {
                cP.Add(x.ToList().Skip(i).FirstOrDefault());
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if(x.Count() > 6)
            {
                int cPSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    cPSum = cPSum + x.ToList().Skip(i).FirstOrDefault();
                }
                if(cPSum != 0)
                {
                    cP.Add(cPSum);
                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                Title = MainWindow.resourcemanager.GetString("trPuchaseOrders"),
                DataLabels = true,
            });
       
            DataContext = this;
            cartesianChart.Series = columnChartData;
           
        }
        private void fillRowChart(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {
            #region
            //  MyAxis.Labels = new List<string>();
            //  List<string> names = new List<string>();
            //  IEnumerable<decimal> pTemp = null;
            //  IEnumerable<decimal> pbTemp = null;
            //  IEnumerable<decimal> resultTemp = null;

            //  if (selectedTab == 0)
            //  {
            //      var temp = fillRowChartList(Invoices, chk_invoice, chk_return, chk_drafs, dp_startDate, dp_endDate, dt_startTime, dt_endTime);
            //      temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
            //      var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
            //      {
            //          branchCreatorId = s.Key,
            //          totalP = s.Sum(x => x.totalNet),
            //          totalPb = s.Sum(x => x.totalNet)
            //      }

            //   );
            //      var resultTotal = result.Select(x => new { x.branchCreatorId, total = x.totalP - x.totalPb }).ToList();
            //      pTemp = result.Select(x => (decimal)x.totalP);
            //      pbTemp = result.Select(x => (decimal)x.totalPb);
            //      resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            //      var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
            //      {
            //          uUserName = s.Key
            //      });
            //      names.AddRange(tempName.Select(nn => nn.uUserName));
            //  }
            //  if (selectedTab == 1)
            //  {
            //      var temp = fillRowChartList(Invoices, chk_posInvoice, chk_posReturn, chk_posDraft, dp_posStartDate, dp_posEndDate, dt_posStartTime, dt_posEndTime);
            //      temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
            //      var result = temp.GroupBy(s => s.posId).Select(s => new
            //      {
            //          posId = s.Key,
            //          totalP = s.Sum(x => x.totalNet),
            //          totalPb = s.Sum(x => x.totalNet)
            //      }
            //   );
            //      var resultTotal = result.Select(x => new { x.posId, total = x.totalP - x.totalPb }).ToList();
            //      pTemp = result.Select(x => (decimal)x.totalP);
            //      pbTemp = result.Select(x => (decimal)x.totalPb);
            //      resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            //      var tempName = temp.GroupBy(s => s.posName).Select(s => new
            //      {
            //          uUserName = s.Key
            //      });
            //      names.AddRange(tempName.Select(nn => nn.uUserName));
            //  }
            //  if (selectedTab == 2)
            //  {
            //      var temp = fillRowChartList(Invoices, chk_vendorsInvoice, chk_vendorsReturn, chk_vendorsDraft, dp_vendorsStartDate, dp_vendorsEndDate, dt_vendorsStartTime, dt_vendorsEndTime);
            //      temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
            //      var result = temp.GroupBy(s => s.agentId).Select(s => new
            //      {
            //          agentId = s.Key,
            //          totalP = s.Sum(x => x.totalNet),
            //          totalPb = s.Sum(x => x.totalNet)
            //      }
            //   );
            //      var resultTotal = result.Select(x => new { x.agentId, total = x.totalP - x.totalPb }).ToList();
            //      pTemp = result.Select(x => (decimal)x.totalP);
            //      pbTemp = result.Select(x => (decimal)x.totalPb);
            //      resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            //      var tempName = temp.GroupBy(s => s.agentName).Select(s => new
            //      {
            //          uUserName = s.Key
            //      });
            //      names.AddRange(tempName.Select(nn => nn.uUserName));
            //  }
            //  if (selectedTab == 3)
            //  {
            //      var temp = fillRowChartList(Invoices, chk_usersInvoice, chk_usersReturn, chk_usersDraft, dp_usersStartDate, dp_usersEndDate, dt_usersStartTime, dt_usersEndTime);
            //      temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
            //      var result = temp.GroupBy(s => s.updateUserId).Select(s => new
            //      {
            //          updateUserId = s.Key,
            //          totalP = s.Sum(x => x.totalNet),
            //          totalPb = s.Sum(x => x.totalNet)
            //      }
            //   );
            //      var resultTotal = result.Select(x => new { x.updateUserId, total = x.totalP - x.totalPb }).ToList();
            //      pTemp = result.Select(x => (decimal)x.totalP);
            //      pbTemp = result.Select(x => (decimal)x.totalPb);
            //      resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            //      var tempName = temp.GroupBy(s => s.uUserAccName).Select(s => new
            //      {
            //          uUserName = s.Key
            //      });
            //      names.AddRange(tempName.Select(nn => nn.uUserName));
            //  }
            //  if (selectedTab == 4)
            //  {
            //      var temp = fillRowChartList(Items, chk_itemInvoice, chk_itemReturn, chk_itemDrafs, dp_ItemStartDate, dp_ItemEndDate, dt_itemStartTime, dt_ItemEndTime);
            //      temp = temp.Where(j => (selectedItemId.Count != 0 ? stackedButton.Contains((int)j.ITitemUnitId) : true));
            //      var result = temp.GroupBy(s => s.ITitemUnitId).Select(s => new
            //      {
            //          ITitemUnitId = s.Key,
            //          totalP = s.Sum(x => x.totalNet),
            //          totalPb = s.Sum(x => x.totalNet),

            //      }
            //   );
            //      var resultTotal = result.Select(x => new { x.ITitemUnitId, total = x.totalP - x.totalPb }).ToList();
            //      pTemp = result.Select(x => (decimal)x.totalP);
            //      pbTemp = result.Select(x => (decimal)x.totalPb);
            //      resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
            //      var tempName = temp.GroupBy(jj => jj.ITitemUnitId)
            //       .Select(g => new ItemUnitCombo { itemUnitId = (int)g.FirstOrDefault().ITitemUnitId, itemUnitName = g.FirstOrDefault().ITitemName + "-" + g.FirstOrDefault().ITunitName }).ToList();
            //      names.AddRange(tempName.Select(nn => nn.itemUnitName));
            //  }
            //  /********************************************************************************/


            //  SeriesCollection rowChartData = new SeriesCollection();
            //  List<decimal> purchase = new List<decimal>();
            //  List<decimal> returns = new List<decimal>();
            //  List<decimal> sub = new List<decimal>();
            //  List<string> titles = new List<string>()
            //  {
            //      //"اجمالي المبيعات","اجمالي المرتجع","صافي المبيعات"
            //      MainWindow.resourcemanager.GetString("trTotalPurchases"),
            //      MainWindow.resourcemanager.GetString("trTotalReturn"),
            //      MainWindow.resourcemanager.GetString("trNetPurchases")
            //  };
            //  for (int i = 0; i < pTemp.Count(); i++)
            //  {
            //      purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
            //      returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
            //      sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());
            //      MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            //  }

            //  rowChartData.Add(
            //new LineSeries
            //{
            //    Values = purchase.AsChartValues(),
            //    Title = titles[0]
            //});

            //  DataContext = this;
            //  rowChart.Series = rowChartData;
            #endregion
        }
        #endregion

        #region methods
        private void translate()
        {
            tt_branch.Content = MainWindow.resourcemanager.GetString("trBranches");
            tt_pos.Content = MainWindow.resourcemanager.GetString("trPOSs");
            tt_vendors.Content = MainWindow.resourcemanager.GetString("trVendors");
            tt_users.Content = MainWindow.resourcemanager.GetString("trUsers");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_endTime, MainWindow.resourcemanager.GetString("trEndTime") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_startTime, MainWindow.resourcemanager.GetString("trStartTime") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_No.Header = MainWindow.resourcemanager.GetString("trNo.");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_pos.Header = MainWindow.resourcemanager.GetString("trPOS");
            col_vendor.Header = MainWindow.resourcemanager.GetString("trVendor");
            col_agentCompany.Header = MainWindow.resourcemanager.GetString("trCompany");
            col_user.Header = MainWindow.resourcemanager.GetString("trUser");
            col_item.Header = MainWindow.resourcemanager.GetString("trItem");
            col_count.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_itQuantity.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_price.Header = MainWindow.resourcemanager.GetString("trPrice");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
         }
        private void fillComboBranches()
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = dynamicComboBranches;
        }
        private void fillComboPos()
        {
            cb_branches.SelectedValuePath = "posId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = dynamicComboPoss;
        }
        private void fillComboVendors()
        {
            cb_branches.SelectedValuePath = "agentId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = dynamicComboVendors;
        }
        private void fillComboUsers()
        {
            cb_branches.SelectedValuePath = "userId";
            cb_branches.DisplayMemberPath = "fullName";
            cb_branches.ItemsSource = dynamicComboUsers;
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
        private IEnumerable<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> Invoices
         , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            fillDates(startDate, endDate, startTime, endTime);
            var result = Invoices.Where(x => (
                       ((startDate.SelectedDate != null && startTime.SelectedTime == null) ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                      && ((endDate.SelectedDate != null && endTime.SelectedTime == null) ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)
                      && (startTime.SelectedTime != null ? x.invDate >= startTime.SelectedTime : true)
                      && (endTime.SelectedTime != null ? x.invDate <= endTime.SelectedTime : true)));

            invLst = result;
            return result;
        }
        public List<ItemTransferInvoice> filltoprint()
        {
            List<ItemTransferInvoice> xx = new List<ItemTransferInvoice>();
            if (selectedTab == 0)
            {
                xx = fillPdfList(cb_branches, selectedBranchId);
            }
            else if (selectedTab == 1)
            {
                xx = fillPdfList(cb_branches, selectedPosId);
            }
            else if (selectedTab == 2)
            {
                xx = fillPdfList(cb_branches, selectedVendorsId);
            }
            else if (selectedTab == 3)
            {
                xx = fillPdfList(cb_branches, selectedUserId);
            }
            return xx;
        }
        List<ItemTransferInvoice> list = new List<ItemTransferInvoice>();

        private List<ItemTransferInvoice> fillPdfList(ComboBox comboBox, ObservableCollection<int> stackedButton)
        {

            var temp = fillList(Invoices, dp_startDate, dp_endDate, dt_startTime, dt_endTime);

            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                list = temp.ToList();
            }
            else if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                list = temp.ToList();
            }
            else if (selectedTab == 2)
            {
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                list = temp.ToList();
            }
            else if (selectedTab == 3)
            {
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                list = temp.ToList();
            }

            return list;
        }
        IEnumerable<ItemTransferInvoice> itemTransfers = null;
        ObservableCollection<int> selected = new ObservableCollection<int>();

        public void fillEvent()
        {
            dgInvoice.ItemsSource = filltoprint();
            txt_count.Text = dgInvoice.Items.Count.ToString();

            if (selectedTab == 0)
                selected = selectedBranchId;
            if (selectedTab == 1)
                selected = selectedPosId;
            if (selectedTab == 2)
                selected = selectedVendorsId;
            if (selectedTab == 3)
                selected = selectedUserId;

            fillPieChart(cb_branches, selected);
            fillColumnChart(cb_branches, selected);
            //fillRowChart(cb_branches, selected);
        }
        public void fillEmptyEvent()
        {
            // reportQuery = filltoprint();

            dgInvoice.ItemsSource = new List<ItemTransferInvoice>();

            txt_count.Text = dgInvoice.Items.Count.ToString();

            ObservableCollection<int> selected = new ObservableCollection<int>();
            if (selectedTab == 0)
                selected = selectedBranchId;
            if (selectedTab == 1)
                selected = selectedPosId;
            if (selectedTab == 2)
                selected = selectedVendorsId;
            if (selectedTab == 3)
                selected = selectedUserId;

            invLst = new List<ItemTransferInvoice>();
            fillPieChart(cb_branches, selected);
            fillColumnChart(cb_branches, selected);
           
        }
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

            path_branch.Fill = Brushes.White;
            path_pos.Fill = Brushes.White;
            path_vendors.Fill = Brushes.White;
            path_users.Fill = Brushes.White;

            col_item.Visibility = Visibility.Collapsed;
            col_branch.Visibility = Visibility.Collapsed;
            col_pos.Visibility = Visibility.Collapsed;
            col_user.Visibility = Visibility.Collapsed;
            col_vendor.Visibility = Visibility.Collapsed;
        }
        private void hideAllColumn()
        {
            col_branch.Visibility = Visibility.Hidden;
            col_pos.Visibility = Visibility.Hidden;
            col_vendor.Visibility = Visibility.Hidden;
            col_agentCompany.Visibility = Visibility.Hidden;
            col_user.Visibility = Visibility.Hidden;
            col_item.Visibility = Visibility.Hidden;
            col_count.Visibility = Visibility.Hidden;
            col_itQuantity.Visibility = Visibility.Hidden;
            col_price.Visibility = Visibility.Hidden;

        }
        private void changeTabReset()
        {
            cb_branches.SelectedItem = null;
             
            dp_endDate.SelectedDate = null;
            dp_startDate.SelectedDate = null;
            dt_startTime.SelectedTime = null;
            dt_endTime.SelectedTime = null;
            chk_allBranches.IsChecked = true;
            cb_branches.IsEnabled = false;
            isClickedAllBranches = false;
           
            chk_allBranches_Click(chk_allBranches, null);
        }
        #endregion

        #region tabs
        private void btn_branch_Click(object sender, RoutedEventArgs e)
        {//branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));

                txt_search.Text = "";
                hideSatacks();
                stk_tagsBranches.Visibility = Visibility.Visible;
                selectedTab = 0;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_branch);
                path_branch.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                fillComboBranches();
               // fillEvent();
              
                hideAllColumn();
                col_branch.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;

                changeTabReset();
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
        {//pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trPosHint"));

                txt_search.Text = "";
                hideSatacks();
                stk_tagsPos.Visibility = Visibility.Visible;
                selectedTab = 1;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_pos);
                path_pos.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                fillComboPos();
               // fillEvent();
                changeTabReset();
                hideAllColumn();
                col_branch.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;
                col_pos.Visibility = Visibility.Visible;
                //   col_totalNet.Visibility = Visibility.Visible;

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
        {//vendor
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trVendorHint"));

                txt_search.Text = "";
                hideSatacks();
                stk_tagsVendors.Visibility = Visibility.Visible;
                selectedTab = 2;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_vendors);
                path_vendors.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                fillComboVendors();
                // fillEvent();
                changeTabReset();
                hideAllColumn();

                col_branch.Visibility = Visibility.Visible;
                col_vendor.Visibility = Visibility.Visible;
                col_agentCompany.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;

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
        {//users
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trUserHint"));

                txt_search.Text = "";
                hideSatacks();
                stk_tagsUsers.Visibility = Visibility.Visible;
                selectedTab = 3;

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_users);
                path_users.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                fillComboUsers();
             //   fillEvent();
                changeTabReset();
                hideAllColumn();

                col_branch.Visibility = Visibility.Visible;
                col_pos.Visibility = Visibility.Visible;
                col_user.Visibility = Visibility.Visible;
                
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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Invoices = await statisticModel.GetPurorderitemcount((int)MainWindow.branchID, (int)MainWindow.userID);

                cb_branches.SelectedItem = null;
                dp_endDate.SelectedDate = null;
                dp_startDate.SelectedDate = null;
                dt_startTime.SelectedTime = null;
                dt_endTime.SelectedTime = null;
                chk_allBranches.IsChecked = false;
                cb_branches.IsEnabled = true;
                if (selectedTab == 0)
                {
                    for (int i = 0; i < comboBrachTemp.Count; i++)
                    {
                        dynamicComboBranches.Add(comboBrachTemp.Skip(i).FirstOrDefault());
                    }
                    comboBrachTemp.Clear();
                    stk_tagsBranches.Children.Clear();
                    selectedBranchId.Clear();
                }

                else if (selectedTab == 1)
                {
                    for (int i = 0; i < comboPosTemp.Count; i++)
                    {
                        dynamicComboPoss.Add(comboPosTemp.Skip(i).FirstOrDefault());
                    }
                    comboPosTemp.Clear();
                    stk_tagsPos.Children.Clear();
                    selectedPosId.Clear();
                }

                else if (selectedTab == 2)
                {
                    for (int i = 0; i < comboVendorTemp.Count; i++)
                    {
                        dynamicComboVendors.Add(comboVendorTemp.Skip(i).FirstOrDefault());
                    }
                    comboVendorTemp.Clear();
                    stk_tagsVendors.Children.Clear();
                    selectedVendorsId.Clear();
                }

                else if (selectedTab == 3)
                {
                    for (int i = 0; i < comboUserTemp.Count; i++)
                    {
                        dynamicComboUsers.Add(comboUserTemp.Skip(i).FirstOrDefault());
                    }
                    comboUserTemp.Clear();
                    stk_tagsUsers.Children.Clear();
                    selectedUserId.Clear();
                }

                // fillEvent();
                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);
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
                    stk_tagsBranches.Children.Remove(currentChip);
                    var m = comboBrachTemp.Where(j => j.branchId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboBranches.Add(m.FirstOrDefault());
                    selectedBranchId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedBranchId.Count == 0)
                    {
                        cb_branches.SelectedItem = null;
                        isClickedAllBranches = false;
                        chk_allBranches_Click(chk_allBranches, null);
                    }
                    else
                    {
                        fillEvent();
                    }
                }
                else if(selectedTab == 1)
                {
                    stk_tagsPos.Children.Remove(currentChip);
                    var m = comboPosTemp.Where(j => j.posId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboPoss.Add(m.FirstOrDefault());
                    selectedPosId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedPosId.Count == 0)
                    {
                        cb_branches.SelectedItem = null;
                        isClickedAllBranches = false;
                        chk_allBranches_Click(chk_allBranches, null);
                    }
                    else
                    {
                        fillEvent();
                    }
                }
                else if (selectedTab == 2)
                {
                    stk_tagsVendors.Children.Remove(currentChip);
                    var m = comboVendorTemp.Where(j => j.agentId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboVendors.Add(m.FirstOrDefault());
                    selectedVendorsId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedVendorsId.Count == 0)
                    {
                        cb_branches.SelectedItem = null;
                        isClickedAllBranches = false;
                        chk_allBranches_Click(chk_allBranches, null);
                    }
                    else
                    {
                        fillEvent();
                    }
                }
                else if (selectedTab == 3)
                {
                    stk_tagsUsers.Children.Remove(currentChip);
                    var m = comboUserTemp.Where(j => j.userId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboUsers.Add(m.FirstOrDefault());
                    selectedUserId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedUserId.Count == 0)
                    {
                        cb_branches.SelectedItem = null;
                        isClickedAllBranches = false;
                        chk_allBranches_Click(chk_allBranches, null);
                    }
                    else
                    {
                        fillEvent();
                    }
                }
              //  fillEvent();

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
        {//select branch
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_branches.SelectedItem != null)
                {
                    if (selectedTab == 0)
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
                        }
                    }
                    else if (selectedTab == 1)
                    {
                        if (stk_tagsPos.Children.Count < 5)
                        {
                            selectedPos = cb_branches.SelectedItem as Pos;
                            var b = new MaterialDesignThemes.Wpf.Chip()
                            {
                                Content = selectedPos.name,
                                Name = "btn" + selectedPos.posId.ToString(),
                                IsDeletable = true,
                                Margin = new Thickness(5, 0, 5, 0)
                            };
                            b.DeleteClick += Chip_OnDeleteClick;
                            stk_tagsPos.Children.Add(b);
                            comboPosTemp.Add(selectedPos);
                            selectedPosId.Add(selectedPos.posId);
                            dynamicComboPoss.Remove(selectedPos);
                        }
                    }
                    else if (selectedTab == 2)
                    {
                        if (stk_tagsVendors.Children.Count < 5)
                        {
                            selectedVendor = cb_branches.SelectedItem as Agent;
                            var b = new MaterialDesignThemes.Wpf.Chip()
                            {
                                Content = selectedVendor.name,
                                Name = "btn" + selectedVendor.agentId.ToString(),
                                IsDeletable = true,
                                Margin = new Thickness(5, 0, 5, 0)
                            };

                            b.DeleteClick += Chip_OnDeleteClick;
                            stk_tagsVendors.Children.Add(b);
                            comboVendorTemp.Add(selectedVendor);
                            selectedVendorsId.Add(selectedVendor.agentId);
                            dynamicComboVendors.Remove(selectedVendor);
                            cb_branches.ItemsSource = dynamicComboVendors;
                        }
                    }
                    else if (selectedTab == 3)
                    {
                        if (stk_tagsUsers.Children.Count < 5)
                        {
                            selectedUser = cb_branches.SelectedItem as User;
                            var b = new MaterialDesignThemes.Wpf.Chip()
                            {
                                Content = selectedUser.fullName,
                                Name = "btn" + selectedUser.userId.ToString(),
                                IsDeletable = true,
                                Margin = new Thickness(5, 0, 5, 0)
                            };
                            b.DeleteClick += Chip_OnDeleteClick;
                            stk_tagsUsers.Children.Add(b);
                            comboUserTemp.Add(selectedUser);
                            selectedUserId.Add(selectedUser.userId);
                            dynamicComboUsers.Remove(selectedUser);
                            cb_branches.ItemsSource = dynamicComboUsers;
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
        bool isClickedAllBranches = false;
        private void chk_allBranches_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if(!isClickedAllBranches)
                {
                    isClickedAllBranches = true;
                    cb_branches.SelectedItem = null;
                    cb_branches.IsEnabled = false;
                    cb_branches.Text = "";

                    chk_allBranches.IsChecked = true;
                    if (selectedTab == 0)
                    {
                        cb_branches.ItemsSource = dynamicComboBranches;
                        for (int i = 0; i < comboBrachTemp.Count; i++)
                        {
                            dynamicComboBranches.Add(comboBrachTemp.Skip(i).FirstOrDefault());
                        }
                        comboBrachTemp.Clear();
                        stk_tagsBranches.Children.Clear();
                        selectedBranchId.Clear();
                    }
                    else if (selectedTab == 1)
                    {
                        cb_branches.ItemsSource = dynamicComboPoss;
                        for (int i = 0; i < comboPosTemp.Count; i++)
                        {
                            dynamicComboPoss.Add(comboPosTemp.Skip(i).FirstOrDefault());
                        }
                        comboPosTemp.Clear();
                        stk_tagsPos.Children.Clear();
                        selectedPosId.Clear();
                    }
                    else if (selectedTab == 2)
                    {
                        cb_branches.ItemsSource = dynamicComboVendors;
                        for (int i = 0; i < comboVendorTemp.Count; i++)
                        {
                            dynamicComboVendors.Add(comboVendorTemp.Skip(i).FirstOrDefault());
                        }
                        comboVendorTemp.Clear();
                        stk_tagsVendors.Children.Clear();
                        selectedVendorsId.Clear();
                    }
                    else if (selectedTab == 3)
                    {
                        cb_branches.ItemsSource = dynamicComboUsers;
                        for (int i = 0; i < comboUserTemp.Count; i++)
                        {
                            dynamicComboUsers.Add(comboUserTemp.Skip(i).FirstOrDefault());
                        }
                        comboUserTemp.Clear();
                        stk_tagsUsers.Children.Clear();
                        selectedUserId.Clear();
                    }
                    fillEvent();
                }
                else
                {
                    cb_branches.IsEnabled = true;
                    isClickedAllBranches = false;

                    cb_branches.SelectedItem = null;
                    chk_allBranches.IsChecked = false;
                    if (selectedTab == 0)
                    {

                        comboBrachTemp.Clear();
                        stk_tagsBranches.Children.Clear();
                        selectedBranchId.Clear();
                        dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
                        fillComboBranches();
                    }
                    else if (selectedTab == 1)
                    {

                        comboPosTemp.Clear();
                        stk_tagsPos.Children.Clear();
                        selectedPosId.Clear();
                        dynamicComboPoss = new ObservableCollection<Pos>(comboPoss);
                        fillComboPos();
                    }
                    else if (selectedTab == 2)
                    {

                        comboVendorTemp.Clear();
                        stk_tagsVendors.Children.Clear();
                        selectedVendorsId.Clear();
                        dynamicComboVendors = new ObservableCollection<Agent>(comboVendors);
                        fillComboVendors();
                    }
                    else if (selectedTab == 3)
                    {

                        comboUserTemp.Clear();
                        stk_tagsUsers.Children.Clear();
                        selectedUserId.Clear();
                        dynamicComboUsers = new ObservableCollection<User>(comboUsers);
                        fillComboUsers();
                    }
                    fillEmptyEvent();
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
                        MainWindow.mainWindow.BTN_purchases_Click(MainWindow.mainWindow.btn_purchase, null);
                        uc_purchases.Instance.UserControl_Loaded(null, null);
                        uc_purchases.Instance.Btn_purchaseOrder_Click(uc_purchases.Instance.btn_purchaseOrder, null);
                        uc_purchaseOrder.Instance.UserControl_Loaded(null, null);
                        uc_purchaseOrder._InvoiceType = invoice.invType;
                        uc_purchaseOrder.Instance.invoice = invoice;
                        uc_purchaseOrder.isFromReport = true;
                        if (item.archived == 0)
                            uc_purchaseOrder.archived = false;
                        else
                            uc_purchaseOrder.archived = true;
                        await uc_purchaseOrder.Instance.fillInvoiceInputs(invoice);
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
        List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();
        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    list = list.Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true)).ToList();

                    list = list
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                          s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                          )).ToList();
                }
                else if (selectedTab == 1)
                {
                    list = list
                        .Where(j => (selectedPosId.Count != 0 ? selectedPosId.Contains((int)j.posId) : true)).ToList();

                    list = list
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.posName.ToLower().Contains(txt_search.Text.ToLower()) ||
                          s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                          )).ToList();
                }

                else if (selectedTab == 2)
                {
                    list = list
                    .Where(j => (selectedVendorsId.Count != 0 ? selectedVendorsId.Contains((int)j.agentId) : true)).ToList();

                    list = list
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.agentName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.agentCompany.ToLower().Contains(txt_search.Text.ToLower()) ||
                          s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                          )).ToList();
                }

                else if (selectedTab == 3)
                {
                    list = list
                    .Where(j => (selectedUserId.Count != 0 ? selectedUserId.Contains((int)j.updateUserId) : true)).ToList();

                    list = list
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.posName.ToLower().Contains(txt_search.Text.ToLower())      ||
                       s.uUserAccName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      )).ToList();
                }

                dgInvoice.ItemsSource = list;
                txt_count.Text = dgInvoice.Items.Count.ToString();
                if (selectedTab == 0)
                    selected = selectedBranchId;
                if (selectedTab == 1)
                    selected = selectedPosId;
                if (selectedTab == 2)
                    selected = selectedVendorsId;
                if (selectedTab == 3)
                    selected = selectedUserId;

                fillPieChart(cb_branches, selected);
                fillColumnChart(cb_branches, selected);

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
        private void fillEventsCall(object sender)
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
        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void dt_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            fillEventsCall(sender);
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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (selectedTab == 0)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 1)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboPoss.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) ).ToList();
                }
                if (selectedTab == 2)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboVendors.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 3)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboUsers.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region report
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            //query = converter(filltoprint());
            query = converter(list);

            string addpath = "";
            string firstTitle = "purchaseOrders";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string selecteditems = "";
            string trSelecteditems = "";
            string startDate = "";
            string endDate = "";
            string searchval = "";
            string startTime = "";
            string endTime = "";
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Ar\ArPurSts.rdlc";
                    secondTitle = "branch";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);

                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Ar\ArPurPosSts.rdlc";
                    secondTitle = "pos";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trPOSs";
                    selecteditems = clsReports.stackToString(stk_tagsPos);
                }
                else if (selectedTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Ar\ArPurVendorSts.rdlc";
                    secondTitle = "vendors";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trVendors";
                    selecteditems = clsReports.stackToString(stk_tagsVendors);
                }
                else if (selectedTab == 3)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Ar\ArPurUserSts.rdlc";
                    secondTitle = "users";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trUsers";
                    selecteditems = clsReports.stackToString(stk_tagsUsers);
                }
                else
                {
                    addpath = @"\Reports\StatisticReport\Purchase\Ar\ArPurItemSts.rdlc";
                    secondTitle = "items";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trItems";
                    selecteditems = clsReports.stackToString(stk_tagsItems);
                }
            }
            else
            {
                //english
                if (selectedTab == 0)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\En\EnPurSts.rdlc";
                    secondTitle = "branch";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);

                }
                else if (selectedTab == 1)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\En\EnPurPosSts.rdlc";
                    secondTitle = "pos";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trPOSs";
                    selecteditems = clsReports.stackToString(stk_tagsPos);
                }
                else if (selectedTab == 2)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\En\EnPurVendorSts.rdlc";
                    secondTitle = "vendors";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trVendors";
                    selecteditems = clsReports.stackToString(stk_tagsVendors);
                }
                else if (selectedTab == 3)
                {
                    addpath = @"\Reports\StatisticReport\Purchase\En\EnPurUserSts.rdlc";
                    secondTitle = "users";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trUsers";
                    selecteditems = clsReports.stackToString(stk_tagsUsers);
                }
                else
                {
                    addpath = @"\Reports\StatisticReport\Purchase\En\EnPurItemSts.rdlc";
                    secondTitle = "items";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trItems";
                    selecteditems = clsReports.stackToString(stk_tagsItems);
                }
            }
            // filter
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";
            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";
            startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
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
            // filter
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trVendor", MainWindow.resourcemanagerreport.GetString("trVendor")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trItem", MainWindow.resourcemanagerreport.GetString("trItem")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));
            paramarr.Add(new ReportParameter("trPrice", MainWindow.resourcemanagerreport.GetString("trPrice")));
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            Title = MainWindow.resourcemanagerreport.GetString("trPurchasesReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", ""));
            paramarr.Add(new ReportParameter("trSelecteditems", MainWindow.resourcemanagerreport.GetString(trSelecteditems)));
            paramarr.Add(new ReportParameter("selecteditems", selecteditems));
            clsReports.PurOrderStsReport(query, rep, reppath, paramarr);
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
                //else if (buttonTag.Equals("rowChart"))
                //{
                //    w.type = "cartesianChart";
                //    w.cartesianChart.Series = rowChart.Series;
                //    w.axcolumn.Labels = MyAxis.Labels;
                //}
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

