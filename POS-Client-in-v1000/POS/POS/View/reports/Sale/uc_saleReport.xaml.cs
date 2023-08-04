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
    /// Interaction logic for uc_saleReport.xaml
    /// </summary>
    public partial class uc_saleReport : UserControl
    {
        #region variables
        ObservableCollection<int> selected = new ObservableCollection<int>();

        //prin & pdf
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private int selectedTab = 0;

        Statistics statisticModel = new Statistics();

        List<ItemTransferInvoice> Invoices;

        //for combo boxes
        /*************************/
        Branch selectedBranch;
        Pos selectedPos;
        Agent selectedVendor;
        User selectedUser;
        Slice selectedSlice;

        List<Branch> comboBranches;
        List<Pos> comboPoss;
        List<Agent> comboVendors;
        List<User> comboUsers;
        List<Slice> comboSlices;

        ObservableCollection<Branch> comboBrachTemp = new ObservableCollection<Branch>();
        ObservableCollection<Pos> comboPosTemp = new ObservableCollection<Pos>();
        ObservableCollection<Agent> comboVendorTemp = new ObservableCollection<Agent>();
        ObservableCollection<User> comboUserTemp = new ObservableCollection<User>();
        ObservableCollection<Slice> comboSliceTemp = new ObservableCollection<Slice>();

        ObservableCollection<Branch> dynamicComboBranches;
        ObservableCollection<Pos> dynamicComboPoss;
        ObservableCollection<Agent> dynamicComboVendors;
        ObservableCollection<User> dynamicComboUsers;
        ObservableCollection<Slice> dynamicComboSlices;

        ObservableCollection<int> selectedBranchId = new ObservableCollection<int>();
        ObservableCollection<int> selectedPosId = new ObservableCollection<int>();
        ObservableCollection<int> selectedVendorsId = new ObservableCollection<int>();
        ObservableCollection<int> selectedUserId = new ObservableCollection<int>();
        ObservableCollection<int> selectedSliceId = new ObservableCollection<int>();

        private static uc_saleReport _instance;
        #endregion

        public static uc_saleReport Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_saleReport();
                return _instance;
            }
        }

        public uc_saleReport()
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

        bool isFirst = true;
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                tb_totalCurrency.Text = AppSettings.Currency;
                //col_refNo.Visibility = Visibility.Collapsed;

                Invoices = await statisticModel.GetSaleitemcount((int)MainWindow.branchID, (int)MainWindow.userID);

                fillPaymentTypes();
                fillCards();

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

                #region customer
                if (FillCombo.customersListReport is null)
                    await FillCombo.RefreshCustomerAllReport();
                comboVendors = FillCombo.customersListReport;
                Agent agent = new Agent();
                agent.agentId = 0;
                agent.name = MainWindow.resourcemanager.GetString("trCashCustomer");
                comboVendors.Insert(0, agent);
                #endregion

                #region user
                //comboUsers = await userModel.Get();
                if (FillCombo.usersList is null)
                    await FillCombo.RefreshUsers();
                comboUsers = FillCombo.usersList;
                #endregion

                #region slice
                if (FillCombo.slicesList is null)
                    await FillCombo.RefreshSlices();
                comboSlices = FillCombo.slicesList;
                #endregion

                dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
                dynamicComboPoss = new ObservableCollection<Pos>(comboPoss);
                dynamicComboVendors = new ObservableCollection<Agent>(comboVendors);
                dynamicComboUsers = new ObservableCollection<User>(comboUsers);
                dynamicComboSlices = new ObservableCollection<Slice>(comboSlices);

                fillComboBranches();

                chk_allBranches.IsChecked = true;
                chk_allBranches_Click(chk_allBranches, null);

                col_processType0.Visibility = Visibility.Collapsed;

                btn_branch_Click(btn_branch, null);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), btn_pos.Tag.ToString());


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
            tt_branch.Content = MainWindow.resourcemanager.GetString("trBranches");
            tt_pos.Content = MainWindow.resourcemanager.GetString("trPOSs");
            tt_vendors.Content = MainWindow.resourcemanager.GetString("trCustomers");
            tt_users.Content = MainWindow.resourcemanager.GetString("trUsers");
            tt_slices.Content = MainWindow.resourcemanager.GetString("invoiceClasses");

            //chk_invoice.Content = MainWindow.resourcemanager.GetString("tr_Invoice");
            //chk_return.Content = MainWindow.resourcemanager.GetString("trReturned");
            //chk_drafs.Content = MainWindow.resourcemanager.GetString("trDraft");

            rad_invoice.Content = MainWindow.resourcemanager.GetString("tr_Invoice");
            rad_return.Content = MainWindow.resourcemanager.GetString("trReturned");
            rad_draft.Content = MainWindow.resourcemanager.GetString("trDraft");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_payments, MainWindow.resourcemanager.GetString("trPaymentType") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_card, MainWindow.resourcemanager.GetString("trCardHint"));

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allPaymentTypes.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allCards.Content = MainWindow.resourcemanager.GetString("trAll");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_endTime, MainWindow.resourcemanager.GetString("trEndTime") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dt_startTime, MainWindow.resourcemanager.GetString("trStartTime") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            col_No.Header = MainWindow.resourcemanager.GetString("trNo.");
            //col_refNo.Header = MainWindow.resourcemanager.GetString("trRefNo.");
            col_type.Header = MainWindow.resourcemanager.GetString("trType");
            col_date.Header = MainWindow.resourcemanager.GetString("trDate");
            col_branch.Header = MainWindow.resourcemanager.GetString("trBranch");
            col_pos.Header = MainWindow.resourcemanager.GetString("trPOS");
            col_vendor.Header = MainWindow.resourcemanager.GetString("trCustomer");
            col_agentCompany.Header = MainWindow.resourcemanager.GetString("trCompany");
            col_user.Header = MainWindow.resourcemanager.GetString("trUser");
            col_slice.Header = MainWindow.resourcemanager.GetString("invoiceClass");
            col_count.Header = MainWindow.resourcemanager.GetString("trQTR");
            col_discount.Header = MainWindow.resourcemanager.GetString("trDiscount");
            col_tax.Header = MainWindow.resourcemanager.GetString("trTax");
            col_totalNet.Header = MainWindow.resourcemanager.GetString("trTotal");
            col_totalNet0.Header = MainWindow.resourcemanager.GetString("trPaymentValue");
            col_processType.Header = MainWindow.resourcemanager.GetString("trPaymentType");
            col_processType0.Header = MainWindow.resourcemanager.GetString("trPaymentType");
            col_payments.Header = MainWindow.resourcemanager.GetString("trPayments");

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
        private void fillComboSlices()
        {
            cb_branches.SelectedValuePath = "sliceId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = dynamicComboSlices;
        }
        private void fillComboBranches(ObservableCollection<Branch> Branches)
        {
            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = Branches;
        }
        private void fillComboPos(ObservableCollection<Pos> Poss)
        {
            cb_branches.SelectedValuePath = "posId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = Poss;
        }
        private void fillComboVendors(ObservableCollection<Agent> Vendors)
        {
            cb_branches.SelectedValuePath = "agentId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = Vendors;
        }
        private void fillComboUsers(ObservableCollection<User> Users)
        {
            cb_branches.SelectedValuePath = "userId";
            cb_branches.DisplayMemberPath = "fullName";
            cb_branches.ItemsSource = Users;
        }
        private void fillComboSlices(ObservableCollection<Slice> Slices)
        {
            cb_branches.SelectedValuePath = "sliceId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = Slices;
        }
        private static void fillDates(DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            try
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
            catch//(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        IEnumerable<ItemTransferInvoice> invLst = null;
        IEnumerable<ItemTransferInvoice> invColumnChartLst = null;
        //private IEnumerable<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> Invoices, CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft
        //   , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        private IEnumerable<ItemTransferInvoice> fillList(IEnumerable<ItemTransferInvoice> Invoices, DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        {
            fillDates(startDate, endDate, startTime, endTime);

            var result = Invoices.Where(x => (

                         //((chkDraft.IsChecked == true ? (x.invType == "sd" || x.invType == "sbd") : false)
                         //           || (chkReturn.IsChecked == true ? (x.invType == "sb") : false)
                         //           || (chkInvoice.IsChecked == true ? (x.invType == "s") : false))
                         //&& 
                         ((startDate.SelectedDate != null && startTime.SelectedTime == null) ? ((DateTime)x.invDate).Date >= ((DateTime)startDate.SelectedDate).Date : true)
                      && ((endDate.SelectedDate != null && endTime.SelectedTime == null) ? ((DateTime)x.invDate).Date <= ((DateTime)endDate.SelectedDate).Date : true)
                      && (startTime.SelectedTime != null ? x.invDate >= startTime.SelectedTime : true)
                      && (endTime.SelectedTime != null ? x.invDate <= endTime.SelectedTime : true))
                       &&
                //paymentID
                (
                 chk_allPaymentTypes.IsChecked.Value ?
                    true :
                    (
                    cb_payments.SelectedIndex != -1 ?
                        (
                         cb_payments.SelectedValue.ToString() == "multiple" ?
                           (x.cachTransferList.Count > 1 || (x.cachTransferList.Count == 1 && x.cachTransferList.Sum(t => t.cash.Value) < x.totalNet))
                         :
                         (
                           cb_payments.SelectedValue.ToString() == "balance" ?
                               (x.cachTransferList.Count == 0 || (x.cachTransferList.Count >= 1 && (x.cachTransferList.Sum(t => t.cash.Value) < x.totalNet) || (x.cachTransferList.Any(c => c.processType == "balance"))))
                             :
                               x.cachTransferList.Any(c => c.processType == cb_payments.SelectedValue.ToString())
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
                         x.cachTransferList.Any(c => c.cardName == cb_card.SelectedValue.ToString())
                            : false
                )
                      );
            invColumnChartLst = result;

            result = result.Where(i => (rad_invoice.IsChecked == true ? (i.invType == "s") : false)
                                       ||
                                       (rad_return.IsChecked == true ? (i.invType == "sb") : false)
                                       ||
                                       (rad_draft.IsChecked == true ? rad_invoice.IsChecked == true ? i.invType == "sd" :
                                                                         rad_return.IsChecked == true ? i.invType == "sbd" :
                                                                        (i.invType == "sd" || i.invType == "sbd") : false)
                                  );

            //if (rad_invoice.IsChecked == true)
            //{
            //    result = result.Where(x => x.invType == "s");
            //}
            //else if (rad_return.IsChecked == true)
            //{
            //    result = result.Where(x => x.invType == "sb");
            //}
            //else if (rad_draft.IsChecked == true)
            //{
            //    result = result.Where(x => x.invType == "sd" || x.invType == "sbd");
            //}
            invLst = result;
            return result;
        }
        //private IEnumerable<ItemTransferInvoice> fillRowChartList(IEnumerable<ItemTransferInvoice> Invoices, CheckBox chkInvoice, CheckBox chkReturn, CheckBox chkDraft
        //  , DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
        private IEnumerable<ItemTransferInvoice> fillRowChartList(IEnumerable<ItemTransferInvoice> Invoices, DatePicker startDate, DatePicker endDate, TimePicker startTime, TimePicker endTime)
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
            return result;
        }
        public void fillEvent()
        {
            itemTransfers = fillList(Invoices, dp_startDate, dp_endDate, dt_startTime, dt_endTime);

            //hide tax column if region tax equals to 0
            if (!AppSettings.invoiceTax_bool.Value)
                col_tax.Visibility = Visibility.Hidden;
            else
                col_tax.Visibility = Visibility.Visible;

            foreach (var i in itemTransfers)
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

            if (selectedTab == 0)
            {
                selected = selectedBranchId;
                itemTransfers = itemTransfers.Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));
            }
            if (selectedTab == 1)
            {
                selected = selectedPosId;
                itemTransfers = itemTransfers.Where(j => (selectedPosId.Count != 0 ? selectedPosId.Contains((int)j.posId) : true));
            }
            if (selectedTab == 2)
            {
                selected = selectedVendorsId;
                itemTransfers = itemTransfers.Where(j => (selectedVendorsId.Count != 0 ? selectedVendorsId.Contains((int)j.agentId) : true));
            }
            if (selectedTab == 3)
            {
                selected = selectedUserId;
                itemTransfers = itemTransfers.Where(j => (selectedUserId.Count != 0 ? selectedUserId.Contains((int)j.updateUserId) : true));
            }
            if (selectedTab == 4)
            {
                selected = selectedSliceId;
                itemTransfers = itemTransfers.Where(j => (selectedSliceId.Count != 0 ? selectedSliceId.Contains((int)j.sliceId) : true));
            }


            dgInvoice.ItemsSource = itemTransfers;
            txt_count.Text = dgInvoice.Items.Count.ToString();

            decimal total = 0;
            //total = itemTransfers.Select(b => b.totalNet.Value).Sum();
            //tb_total.Text = SectionData.DecTostring(total);

            if (rad_draft.IsChecked.Value == true && (rad_return.IsChecked.Value == true || rad_invoice.IsChecked.Value == true))
                total = itemTransfers.Where(i => i.invType != "sd" || i.invType == "sbd").Select(b => b.totalNetRep.Value).Sum();

            else
                total = itemTransfers.Select(b => b.totalNetRep.Value).Sum();

            if (rad_draft.IsChecked.Value == true && rad_return.IsChecked.Value == false && rad_invoice.IsChecked.Value == false)
                col_invoices.Visibility = Visibility.Hidden;
            else
                col_invoices.Visibility = Visibility.Visible;

            tb_total.Text = SectionData.DecTostring(total);

            fillPieChart(selected);
            fillColumnChart(selected);
            fillRowChart(selected);
        }
        public void fillEmptyEvent()
        {
            //  itemTransfers = fillList(Invoices, chk_invoice, chk_return, chk_drafs, dp_startDate, dp_endDate, dt_startTime, dt_endTime);

            //hide tax column if region tax equals to 0
            if (!AppSettings.invoiceTax_bool.Value)
                col_tax.Visibility = Visibility.Hidden;
            else
                col_tax.Visibility = Visibility.Visible;

            ObservableCollection<int> selected = new ObservableCollection<int>();
            if (selectedTab == 0)
            {
                selected = selectedBranchId;
                itemTransfers = itemTransfers.Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));
            }
            if (selectedTab == 1)
            {
                selected = selectedPosId;
                itemTransfers = itemTransfers.Where(j => (selectedPosId.Count != 0 ? selectedPosId.Contains((int)j.posId) : true));
            }
            if (selectedTab == 2)
            {
                selected = selectedVendorsId;
                itemTransfers = itemTransfers.Where(j => (selectedVendorsId.Count != 0 ? selectedVendorsId.Contains((int)j.agentId) : true));
            }
            if (selectedTab == 3)
            {
                selected = selectedUserId;
                itemTransfers = itemTransfers.Where(j => (selectedUserId.Count != 0 ? selectedUserId.Contains((int)j.updateUserId) : true));
            }

            if (selectedTab == 4)
            {
                selected = selectedSliceId;
                itemTransfers = itemTransfers.Where(j => (selectedSliceId.Count != 0 ? selectedSliceId.Contains((int)j.sliceId) : true));
            }
            dgInvoice.ItemsSource = itemTransfers;
            txt_count.Text = dgInvoice.Items.Count.ToString();
            decimal total = 0;
            //total = itemTransfers.Select(b => b.totalNet.Value).Sum();
            //tb_total.Text = SectionData.DecTostring(total);
            total = itemTransfers.Select(b => b.totalNet0).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            fillPieChart(selected);
            fillColumnChart(selected);
            fillEmptyRowChart(selected);
        }
        private void hidAllColumns()
        {
            col_type.Visibility = Visibility.Hidden;
            col_branch.Visibility = Visibility.Hidden;
            col_pos.Visibility = Visibility.Hidden;
            col_vendor.Visibility = Visibility.Hidden;
            col_agentCompany.Visibility = Visibility.Hidden;
            col_user.Visibility = Visibility.Hidden;
            col_discount.Visibility = Visibility.Hidden;
            col_count.Visibility = Visibility.Hidden;
            col_totalNet.Visibility = Visibility.Hidden;
            col_tax.Visibility = Visibility.Hidden;
            col_slice.Visibility = Visibility.Hidden;
        }
        private void hideSatacks()
        {
            stk_tagsBranches.Visibility = Visibility.Collapsed;
            stk_tagsSlices.Visibility = Visibility.Collapsed;
            stk_tagsPos.Visibility = Visibility.Collapsed;
            stk_tagsUsers.Visibility = Visibility.Collapsed;
            stk_tagsVendors.Visibility = Visibility.Collapsed;
        }
        public void paint()
        {
            //bdrMain.RenderTransform = Animations.borderAnimation(50, bdrMain, true);

            bdr_branch.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_pos.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_vendors.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_users.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));
            bdr_slices.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

            path_branch.Fill = Brushes.White;
            path_pos.Fill = Brushes.White;
            path_vendors.Fill = Brushes.White;
            path_users.Fill = Brushes.White;
            path_slices.Fill = Brushes.White;
        }
        public void notCheckAll()
        {
            //isClickedAllBranches= false;
            itemTransfers = new List<ItemTransferInvoice>();
            invLst = itemTransfers;
            dgInvoice.ItemsSource = itemTransfers;
            decimal total = 0;
            total = itemTransfers.Select(b => b.totalNet.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            txt_count.Text = dgInvoice.Items.Count.ToString();
            fillEmptyEvent();


            dynamicComboBranches = new ObservableCollection<Branch>(comboBranches);
            dynamicComboPoss = new ObservableCollection<Pos>(comboPoss);
            dynamicComboVendors = new ObservableCollection<Agent>(comboVendors);
            dynamicComboUsers = new ObservableCollection<User>(comboUsers);
            dynamicComboSlices = new ObservableCollection<Slice>(comboSlices);

            if (selectedTab == 0)
            {
                fillComboBranches(dynamicComboBranches);
            }
            else if (selectedTab == 1)
            {
                fillComboPos(dynamicComboPoss);

            }
            else if (selectedTab == 2)
            {
                fillComboVendors(dynamicComboVendors);

            }
            else if (selectedTab == 3)
            {
                fillComboUsers(dynamicComboUsers);

            }
            else if (selectedTab == 4)
            {
                fillComboSlices(dynamicComboSlices);

            }

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

        #region Events

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Invoices = await statisticModel.GetSaleitemcount((int)MainWindow.branchID, (int)MainWindow.userID);

                cb_branches.SelectedItem = null;
                //chk_drafs.IsChecked = false;
                //chk_return.IsChecked = false;
                //chk_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;
                rad_invoice.IsChecked = true;
                dp_endDate.SelectedDate = null;
                dp_startDate.SelectedDate = null;
                dt_startTime.SelectedTime = null;
                dt_endTime.SelectedTime = null;
                chk_allBranches.IsChecked = true;
                chk_allPaymentTypes.IsChecked = true;
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
                else if (selectedTab == 4)
                {
                    for (int i = 0; i < comboSliceTemp.Count; i++)
                    {
                        dynamicComboSlices.Add(comboSliceTemp.Skip(i).FirstOrDefault());
                    }
                    comboSliceTemp.Clear();
                    stk_tagsSlices.Children.Clear();
                    selectedSliceId.Clear();
                }


                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);
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
        private void fillEventsCall(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //fillEvent();
                if (isClickedAllBranches == false
                    && ((selectedTab == 0 && stk_tagsBranches.Children.Count == 0) ||
                  (selectedTab == 1 && stk_tagsPos.Children.Count == 0) ||
                   (selectedTab == 2 && stk_tagsVendors.Children.Count == 0) ||
                   (selectedTab == 3 && stk_tagsUsers.Children.Count == 0) ||
                   (selectedTab == 4 && stk_tagsSlices.Children.Count == 0)))
                {
                    notCheckAll();
                }
                else
                {
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
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            if (!rad_invoice.IsChecked.Value && !rad_draft.IsChecked.Value && !rad_return.IsChecked.Value)
                rad_invoice.IsChecked = true;
            fillEventsCall(sender);
        }
        private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            fillEventsCall(sender);
        }
        private void dt_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            fillEventsCall(sender);
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
                else if (selectedTab == 1)
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
                else if (selectedTab == 4)
                {
                    stk_tagsSlices.Children.Remove(currentChip);
                    var m = comboSliceTemp.Where(j => j.sliceId == (Convert.ToInt32(currentChip.Name.Remove(0, 3))));
                    dynamicComboSlices.Add(m.FirstOrDefault());
                    selectedSliceId.Remove(Convert.ToInt32(currentChip.Name.Remove(0, 3)));
                    if (selectedSliceId.Count == 0)
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
        {
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
                    else if (selectedTab == 4)
                    {
                        if (stk_tagsSlices.Children.Count < 5)
                        {
                            selectedSlice = cb_branches.SelectedItem as Slice;
                            var b = new MaterialDesignThemes.Wpf.Chip()
                            {
                                Content = selectedSlice.name,
                                Name = "btn" + selectedSlice.sliceId.ToString(),
                                IsDeletable = true,
                                Margin = new Thickness(5, 0, 5, 0)
                            };
                            b.DeleteClick += Chip_OnDeleteClick;
                            stk_tagsSlices.Children.Add(b);
                            comboSliceTemp.Add(selectedSlice);
                            selectedSliceId.Add(selectedSlice.sliceId);
                            dynamicComboSlices.Remove(selectedSlice);
                            cb_branches.ItemsSource = dynamicComboSlices;
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

                if (!isClickedAllBranches)
                {
                    isClickedAllBranches = true;
                    cb_branches.SelectedItem = null;
                    cb_branches.IsEnabled = false;
                    chk_allBranches.IsChecked = true;
                    cb_branches.Text = "";
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
                    else if (selectedTab == 4)
                    {
                        cb_branches.ItemsSource = dynamicComboSlices;
                        for (int i = 0; i < comboSliceTemp.Count; i++)
                        {
                            dynamicComboSlices.Add(comboSliceTemp.Skip(i).FirstOrDefault());
                        }
                        comboSliceTemp.Clear();
                        stk_tagsSlices.Children.Clear();
                        selectedSliceId.Clear();
                    }
                    if (isFirst)
                        isFirst = false;
                    else
                        fillEvent();
                }
                else
                {
                    chk_allBranches.IsChecked = false;
                    isClickedAllBranches = false;
                    cb_branches.IsEnabled = true;
                    notCheckAll();
                    // fillComboBranches();
                    //itemTransfers = null;
                    //dgInvoice.ItemsSource = null;
                    //fillColumnChart();
                    //fillPieChart();
                    //fillRowChart();
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
        IEnumerable<ItemTransferInvoice> itemTransfers = null;
        private void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (selectedTab == 0)
                {
                    itemTransfers = invLst
                        .Where(j => (selectedBranchId.Count != 0 ? selectedBranchId.Contains((int)j.branchCreatorId) : true));

                    itemTransfers = itemTransfers
                        .Where(s =>
                            (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                             s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                            )).ToList();
                }
                else if (selectedTab == 1)
                {
                    itemTransfers = invLst
                        .Where(j => (selectedPosId.Count != 0 ? selectedPosId.Contains((int)j.posId) : true));

                    itemTransfers = itemTransfers
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.posName.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      )).ToList();
                }

                else if (selectedTab == 2)
                {
                    itemTransfers = invLst
                    .Where(j => (selectedVendorsId.Count != 0 ? selectedVendorsId.Contains((int)j.agentId) : true));

                    itemTransfers = itemTransfers
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.agentName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.agentCompany.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      )).ToList();
                }

                else if (selectedTab == 3)
                {
                    itemTransfers = invLst
                    .Where(j => (selectedUserId.Count != 0 ? selectedUserId.Contains((int)j.updateUserId) : true));

                    itemTransfers = itemTransfers
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.posName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.uUserAccName.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      ));
                }
                else if (selectedTab == 4)
                {
                    itemTransfers = invLst;
                    //.Where(j => (selectedSliceId.Count != 0 ? selectedSliceId.Contains((int)j.sliceId) : true));?????????????????

                    itemTransfers = itemTransfers
                        .Where(s => (s.branchCreatorName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.posName.ToLower().Contains(txt_search.Text.ToLower()) ||
                       s.uUserAccName.ToLower().Contains(txt_search.Text.ToLower()) ||
                      s.invNumber.ToLower().Contains(txt_search.Text.ToLower())
                      ));
                }
                dgInvoice.ItemsSource = itemTransfers;
                txt_count.Text = dgInvoice.Items.Count.ToString();
                decimal total = 0;
                total = itemTransfers.Select(b => b.totalNet.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);

                fillPieChart(selected);
                fillColumnChart(selected);
                fillRowChart(selected);

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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (selectedTab == 0)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 1)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboPoss.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
                }
                else if (selectedTab == 2)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboVendors.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 3)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboUsers.Where(p => p.name.ToLower().Contains(tb.Text.ToLower()) || (p.mobile != null && p.mobile.Contains(tb.Text))).ToList();
                }
                else if (selectedTab == 4)
                {
                    var combo = sender as ComboBox;
                    var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                    tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                    combo.ItemsSource = dynamicComboSlices.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_payments_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                        fillEvent();
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
        private void Chk_allPaymentTypes_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_payments.SelectedIndex = -1;
                cb_payments.IsEnabled = false;
                chk_allCards.IsChecked = true;
                pnl_cards.Visibility = Visibility.Collapsed;
                this.DataContext = itemTransfers;
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
        private void Chk_allPaymentTypes_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_payments.IsEnabled = true;
                this.DataContext = itemTransfers;
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
        private void Cb_card_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_card.SelectedIndex != -1)
                {
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
        private void Chk_allCards_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_card.SelectedIndex = -1;
                cb_card.IsEnabled = false;
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
        private void Chk_allCards_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_card.IsEnabled = true;
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
                            w.invoiceLst = Invoices.Where(i => i.invNumber == row.mainInvNumber).ToList();

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
        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cb_payments.SelectedIndex != -1 && !cb_payments.SelectedValue.ToString().Equals("multiple"))
                {
                    Button btnPayments = sender as Button;

                    ItemTransferInvoice iTI = itemTransfers.Where(i => i.invNumber == btnPayments.Tag.ToString()).FirstOrDefault();
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
                    this.DataContext = itemTransfers;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        private void search_Checking(object sender, RoutedEventArgs e)
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
                    fillEvent();
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
                selected = selectedBranchId;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_branch);
                path_branch.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hidAllColumns();
                //chk_invoice.IsChecked = true;
                //chk_drafs.IsChecked = false;
                //chk_return.IsChecked = false;

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;

                //show columns
                col_branch.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;
                col_totalNet.Visibility = Visibility.Visible;
                col_discount.Visibility = Visibility.Visible;
                col_tax.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;

                fillComboBranches();

                stk_tagsBranches.Children.Clear();
                selectedBranchId.Clear();
                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);
                //{
                //    notCheckAll();
                //}
                //else
                //{
                //    fillEvent();
                //}

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
                selected = selectedPosId;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_pos);
                path_pos.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hidAllColumns();
                //chk_invoice.IsChecked = true;
                //chk_drafs.IsChecked = false;
                //chk_return.IsChecked = false;

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;
                //show columns
                col_branch.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;
                col_pos.Visibility = Visibility.Visible;
                col_totalNet.Visibility = Visibility.Visible;
                col_discount.Visibility = Visibility.Visible;
                col_tax.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;

                fillComboPos();
                stk_tagsPos.Children.Clear();
                selectedPosId.Clear();
                //  fillEvent();
                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);

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
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trCustomerHint"));

                txt_search.Text = "";
                hideSatacks();
                stk_tagsVendors.Visibility = Visibility.Visible;
                selectedTab = 2;
                selected = selectedVendorsId;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_vendors);
                path_vendors.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hidAllColumns();
                //chk_invoice.IsChecked = true;
                //chk_drafs.IsChecked = false;
                //chk_return.IsChecked = false;

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;
                //show columns
                col_branch.Visibility = Visibility.Visible;
                col_vendor.Visibility = Visibility.Visible;
                col_agentCompany.Visibility = Visibility.Visible;
                col_discount.Visibility = Visibility.Visible;
                col_count.Visibility = Visibility.Visible;
                col_totalNet.Visibility = Visibility.Visible;
                col_tax.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;

                fillComboVendors();
                stk_tagsVendors.Children.Clear();
                selectedVendorsId.Clear();
                //   fillEvent();
                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);

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
                selected = selectedUserId;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_users);
                path_users.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hidAllColumns();
                //chk_invoice.IsChecked = true;
                //chk_drafs.IsChecked = false;
                //chk_return.IsChecked = false;

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;
                //show columns
                col_branch.Visibility = Visibility.Visible;
                col_discount.Visibility = Visibility.Visible;
                col_pos.Visibility = Visibility.Visible;
                col_user.Visibility = Visibility.Visible;
                col_totalNet.Visibility = Visibility.Visible;
                col_tax.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;

                fillComboUsers();
                stk_tagsUsers.Children.Clear();
                selectedUserId.Clear();
                //  fillEvent();
                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);

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
        private void Btn_slices_Click(object sender, RoutedEventArgs e)
        {//slices
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("slice") + "...");

                txt_search.Text = "";
                hideSatacks();
                stk_tagsSlices.Visibility = Visibility.Visible;
                selectedTab = 4;
                selected = selectedSliceId;
                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_slices);
                path_slices.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                hidAllColumns();
                //chk_invoice.IsChecked = true;
                //chk_drafs.IsChecked = false;
                //chk_return.IsChecked = false;

                rad_invoice.IsChecked = true;
                rad_draft.IsChecked = false;
                rad_return.IsChecked = false;

                chk_allPaymentTypes.IsChecked = true;
                chk_allCards.IsChecked = true;
                //show columns
                col_branch.Visibility = Visibility.Visible;
                col_discount.Visibility = Visibility.Visible;
                col_pos.Visibility = Visibility.Visible;
                //col_user.Visibility = Visibility.Visible;
                col_slice.Visibility = Visibility.Visible;
                col_totalNet.Visibility = Visibility.Visible;
                col_tax.Visibility = Visibility.Visible;
                col_type.Visibility = Visibility.Visible;

                fillComboSlices();
                stk_tagsSlices.Children.Clear();
                selectedSliceId.Clear();
                //  fillEvent();
                isClickedAllBranches = false;
                chk_allBranches_Click(chk_allBranches, null);

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

        Invoice invoice;

        #region reports
        public async Task BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath = "";
            string firstTitle = "invoice";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";

            string selecteditems = "";
            string trSelecteditems = "";

            string invchk = "";
            string retchk = "";
            string drftchk = "";
            string startDate = "";
            string endDate = "";

            string paymentval = "";
            string cardval = "";
            string searchval = "";
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
                    //if(rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\Ar\ArReturn\ArPurSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\Ar\ArPurSts.rdlc";
                    //}

                    secondTitle = "branch";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);

                }
                else if (selectedTab == 1)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\Ar\ArReturn\ArPurPosSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\Ar\ArPurPosSts.rdlc";
                    //}
                    secondTitle = "pos";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trPOSs";
                    selecteditems = clsReports.stackToString(stk_tagsPos);
                }
                else if (selectedTab == 2)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\Ar\ArReturn\ArPurVendorSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\Ar\ArPurVendorSts.rdlc";
                    //}

                    secondTitle = "customers";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trCustomers";
                    selecteditems = clsReports.stackToString(stk_tagsVendors);
                }
                else if (selectedTab == 3)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\Ar\ArReturn\ArPurUserSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\Ar\ArPurUserSts.rdlc";
                    //}

                    secondTitle = "users";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trUsers";
                    selecteditems = clsReports.stackToString(stk_tagsUsers);

                }
                else
                {//slice 
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\Ar\ArReturn\ArPurUserSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\Ar\ArSlice.rdlc";
                    //}

                    secondTitle = "slices";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "slices";
                    selecteditems = clsReports.stackToString(stk_tagsSlices);

                }

            }
            else
            {
                //english
                if (selectedTab == 0)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\En\EnReturn\EnPurSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\En\EnPurSts.rdlc";
                    //}

                    secondTitle = "branch";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trBranches";
                    selecteditems = clsReports.stackToString(stk_tagsBranches);
                }
                else if (selectedTab == 1)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\En\EnReturn\EnPurPosSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\En\EnPurPosSts.rdlc";
                    //}

                    secondTitle = "pos";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trPOSs";
                    selecteditems = clsReports.stackToString(stk_tagsPos);
                }
                else if (selectedTab == 2)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\En\EnReturn\EnPurVendorSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\En\EnPurVendorSts.rdlc";
                    //}

                    secondTitle = "customers";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trCustomers";
                    selecteditems = clsReports.stackToString(stk_tagsVendors);
                }
                else if (selectedTab == 3)
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\En\EnReturn\EnPurUserSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\En\EnPurUserSts.rdlc";
                    //}

                    secondTitle = "users";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "trUsers";
                    selecteditems = clsReports.stackToString(stk_tagsUsers);
                }
                else
                {
                    //if (rad_return.IsChecked == true)
                    //{
                    //    addpath = @"\Reports\StatisticReport\Sale\En\EnReturn\EnPurUserSts.rdlc";
                    //}
                    //else
                    //{
                    addpath = @"\Reports\StatisticReport\Sale\En\EnSlice.rdlc";
                    //}
                    secondTitle = "slices";
                    subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
                    trSelecteditems = "slices";
                    selecteditems = clsReports.stackToString(stk_tagsSlices);
                }

            }
            //for payment 
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
            //end 

            //filter
            startDate = dp_startDate.SelectedDate != null ? SectionData.DateToString(dp_startDate.SelectedDate) : "";

            endDate = dp_endDate.SelectedDate != null ? SectionData.DateToString(dp_endDate.SelectedDate) : "";
            startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";

            paymentval = cb_payments.SelectedItem != null
               && (chk_allPaymentTypes.IsChecked == false || chk_allPaymentTypes.IsChecked == null)
               && selecteditems != ""
               ? clsReports.PaymentComboConvert(cb_payments.SelectedValue.ToString()) : (chk_allPaymentTypes.IsChecked == true && selecteditems != "" ? all : "");

            cardval = (cb_card.SelectedItem != null
            && (chk_allCards.IsChecked == false) && cb_card.IsVisible == true)
            ? cb_card.Text : (chk_allCards.IsChecked == true && chk_allCards.IsVisible == true ? all : "");

            invchk = rad_invoice.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("tr_Invoice") : "";
            retchk = rad_return.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trReturned") : "";
            drftchk = rad_draft.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trDraft") : "";
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
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("StartTimeVal", startTime));
            paramarr.Add(new ReportParameter("EndTimeVal", endTime));
            paramarr.Add(new ReportParameter("cardval", cardval));
            paramarr.Add(new ReportParameter("trPaymentType", MainWindow.resourcemanagerreport.GetString("trPaymentType")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trTheCreditCard", MainWindow.resourcemanagerreport.GetString("trTheCreditCard")));
            paramarr.Add(new ReportParameter("trStartTime", MainWindow.resourcemanagerreport.GetString("trStartTime")));
            paramarr.Add(new ReportParameter("trEndTime", MainWindow.resourcemanagerreport.GetString("trEndTime")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            paramarr.Add(new ReportParameter("trNo", MainWindow.resourcemanagerreport.GetString("trNo.")));
            paramarr.Add(new ReportParameter("trType", MainWindow.resourcemanagerreport.GetString("trType")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));
            paramarr.Add(new ReportParameter("trPOS", MainWindow.resourcemanagerreport.GetString("trPOS")));
            paramarr.Add(new ReportParameter("trCustomer", MainWindow.resourcemanagerreport.GetString("trCustomer")));
            paramarr.Add(new ReportParameter("trCompany", MainWindow.resourcemanagerreport.GetString("trCompany")));
            paramarr.Add(new ReportParameter("trUser", MainWindow.resourcemanagerreport.GetString("trUser")));
            paramarr.Add(new ReportParameter("trQTR", MainWindow.resourcemanagerreport.GetString("trQTR")));

            paramarr.Add(new ReportParameter("trDiscount", MainWindow.resourcemanagerreport.GetString("trDiscount")));
            paramarr.Add(new ReportParameter("trTax", MainWindow.resourcemanagerreport.GetString("trTax")));
            // paramarr.Add(new ReportParameter("trPaymentValue", MainWindow.resourcemanagerreport.GetString("trPaymentValue")));
            paramarr.Add(new ReportParameter("trPayments", MainWindow.resourcemanagerreport.GetString("trPayments")));

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            Title = MainWindow.resourcemanagerreport.GetString("trSalesReport") + " / " + subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            paramarr.Add(new ReportParameter("trSelecteditems", MainWindow.resourcemanagerreport.GetString(trSelecteditems)));
            paramarr.Add(new ReportParameter("selecteditems", selecteditems));
            await clsReports.SaleInvoicePaymentReport(itemTransfers, rep, reppath, paramarr, 0, cb_paymentsValue, cb_cardValue,
                 rad_invoice.IsChecked, rad_return.IsChecked, rad_draft.IsChecked
                );
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private async void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                await BuildReport();

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
        private async void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                await BuildReport();

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
        private async void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region
                //  Thread t1 = new Thread(() =>
                //{

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

                //});
                //  t1.Start();
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
        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
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
                await BuildReport();



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
        private void fillPieChart(ObservableCollection<int> stackedButton)
        {
            List<string> titles = new List<string>();
            IEnumerable<int> x = null;

            var temp = invLst;

            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                var titleTemp = temp.GroupBy(m => m.branchCreatorName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new { branchCreatorId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                var titleTemp = temp.GroupBy(m => new { m.posName, m.posId });
                titles.AddRange(titleTemp.Select(jj => jj.Key.posName));
                var result = temp.GroupBy(s => s.posId).Select(s => new { posId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 2)
            {
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                var titleTemp = temp.GroupBy(m => m.agentName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.agentId).Select(s => new { agentId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 3)
            {
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                var titleTemp = temp.GroupBy(m => m.uUserAccName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.updateUserId).Select(s => new { updateUserId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }
            else if (selectedTab == 4)
            {
                temp = temp.Where(j => (selectedSliceId.Count != 0 ? stackedButton.Contains((int)j.sliceId) : true));
                var titleTemp = temp.GroupBy(m => m.sliceName);
                titles.AddRange(titleTemp.Select(jj => jj.Key));
                var result = temp.GroupBy(s => s.sliceId).Select(s => new { sliceId = s.Key, count = s.Count() });
                x = result.Select(m => m.count);
            }

            int index = titles.FindIndex(s => s == "unknown");

            if (index != -1)
                titles[index] = MainWindow.resourcemanager.GetString("trCashCustomer");

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
        private void fillColumnChart(ObservableCollection<int> stackedButton)
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<int> x = null;
            IEnumerable<int> y = null;
            IEnumerable<int> z = null;

            var temp = invColumnChartLst;
            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()
                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            else if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                var result = temp.GroupBy(s => s.posId).Select(s => new
                {
                    posId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()
                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.posName).Select(s => new
                {
                    uUserName = s.Key + "/" + s.FirstOrDefault().branchCreatorName
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            else if (selectedTab == 2)
            {
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                var result = temp.GroupBy(s => s.agentId).Select(s => new
                {
                    agentId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.agentName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            else if (selectedTab == 3)
            {
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                var result = temp.GroupBy(s => s.updateUserId).Select(s => new
                {
                    updateUserId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.uUserAccName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            else if (selectedTab == 4)
            {
                temp = temp.Where(j => (selectedSliceId.Count != 0 ? stackedButton.Contains((int)j.sliceId) : true));
                var result = temp.GroupBy(s => s.sliceId).Select(s => new
                {
                    sliceId = s.Key,
                    countP = s.Where(m => m.invType == "s").Count(),
                    countPb = s.Where(m => m.invType == "sb").Count(),
                    countD = s.Where(m => m.invType == "sd" || m.invType == "sbd").Count()

                });
                x = result.Select(m => m.countP);
                y = result.Select(m => m.countPb);
                z = result.Select(m => m.countD);
                var tempName = temp.GroupBy(s => s.sliceName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            SeriesCollection columnChartData = new SeriesCollection();
            List<int> cP = new List<int>();
            List<int> cPb = new List<int>();
            List<int> cD = new List<int>();
            List<string> titles = new List<string>()
            {
                MainWindow.resourcemanager.GetString("trSales"),
                MainWindow.resourcemanager.GetString("trReturned"),
                MainWindow.resourcemanager.GetString("trDraft")
            };

            int xCount = 6;
            if (x.Count() <= 6) xCount = x.Count();

            for (int i = 0; i < xCount; i++)
            {
                cP.Add(x.ToList().Skip(i).FirstOrDefault());
                cPb.Add(y.ToList().Skip(i).FirstOrDefault());
                cD.Add(z.ToList().Skip(i).FirstOrDefault());

                int index = names.FindIndex(s => s == "unknown");

                if (index != -1)
                    names[index] = MainWindow.resourcemanager.GetString("trCashCustomer");

                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (x.Count() > 6)
            {
                int cPSum = 0, cPbSum = 0, cDSum = 0;
                for (int i = 6; i < x.Count(); i++)
                {
                    cPbSum = cPSum + x.ToList().Skip(i).FirstOrDefault();
                    cPbSum = cPbSum + y.ToList().Skip(i).FirstOrDefault();
                    cDSum = cDSum + z.ToList().Skip(i).FirstOrDefault();
                }
                if (!((cPbSum == 0) && (cPbSum == 0) && (cDSum == 0)))
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
        private void fillRowChart(ObservableCollection<int> stackedButton)
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = null;
            IEnumerable<decimal> pbTemp = null;
            IEnumerable<decimal> resultTemp = null;

            var temp = fillRowChartList(Invoices, dp_startDate, dp_endDate, dt_startTime, dt_endTime);
            if (selectedTab == 0)
            {
                temp = temp.Where(j => (selectedBranchId.Count != 0 ? stackedButton.Contains((int)j.branchCreatorId) : true));
                var result = temp.GroupBy(s => s.branchCreatorId).Select(s => new
                {
                    branchCreatorId = s.Key,
                    totalP = s.Where(x => x.invType == "s").Sum(x => x.totalNetRep),
                    totalPb = s.Where(x => x.invType == "sb").Sum(x => x.totalNetRep)
                }

             );
                var resultTotal = result.Select(x => new { x.branchCreatorId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
                var tempName = temp.GroupBy(s => s.branchCreatorName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            if (selectedTab == 1)
            {
                temp = temp.Where(j => (selectedPosId.Count != 0 ? stackedButton.Contains((int)j.posId) : true));
                var result = temp.GroupBy(s => s.posId).Select(s => new
                {
                    posId = s.Key,
                    totalP = s.Where(x => x.invType == "s").Sum(x => x.totalNetRep),
                    totalPb = s.Where(x => x.invType == "sb").Sum(x => x.totalNetRep)
                }
             );
                var resultTotal = result.Select(x => new { x.posId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
                var tempName = temp.GroupBy(s => s.posName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            if (selectedTab == 2)
            {
                temp = temp.Where(j => (selectedVendorsId.Count != 0 ? stackedButton.Contains((int)j.agentId) : true));
                var result = temp.GroupBy(s => s.agentId).Select(s => new
                {
                    agentId = s.Key,
                    totalP = s.Where(x => x.invType == "s").Sum(x => x.totalNetRep),
                    totalPb = s.Where(x => x.invType == "sb").Sum(x => x.totalNetRep)
                }
             );
                var resultTotal = result.Select(x => new { x.agentId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
                var tempName = temp.GroupBy(s => s.agentName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }
            if (selectedTab == 3)
            {
                temp = temp.Where(j => (selectedUserId.Count != 0 ? stackedButton.Contains((int)j.updateUserId) : true));
                var result = temp.GroupBy(s => s.updateUserId).Select(s => new
                {
                    updateUserId = s.Key,
                    totalP = s.Where(x => x.invType == "s").Sum(x => x.totalNetRep),
                    totalPb = s.Where(x => x.invType == "sb").Sum(x => x.totalNetRep)
                }
             );
                var resultTotal = result.Select(x => new { x.updateUserId, total = x.totalP - x.totalPb }).ToList();
                pTemp = result.Select(x => (decimal)x.totalP);
                pbTemp = result.Select(x => (decimal)x.totalPb);
                resultTemp = result.Select(x => (decimal)x.totalP - (decimal)x.totalPb);
                var tempName = temp.GroupBy(s => s.uUserAccName).Select(s => new
                {
                    uUserName = s.Key
                });
                names.AddRange(tempName.Select(nn => nn.uUserName));
            }

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

            int xCount = 0;
            if (pTemp.Count() <= 6) xCount = pTemp.Count();
            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());

                int index = names.FindIndex(s => s == "unknown");

                if (index != -1)
                    names[index] = MainWindow.resourcemanager.GetString("trCashCustomer");

                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (pTemp.Count() > 6)
            {
                decimal purchaseSum = 0, returnsSum = 0, subSum = 0;
                for (int i = 6; i < pTemp.Count(); i++)
                {
                    purchaseSum = purchaseSum + pTemp.ToList().Skip(i).FirstOrDefault();
                    returnsSum = returnsSum + pbTemp.ToList().Skip(i).FirstOrDefault();
                    subSum = subSum + resultTemp.ToList().Skip(i).FirstOrDefault();
                }
                if (!((purchaseSum == 0) && (returnsSum == 0) && (subSum == 0)))
                {
                    purchase.Add(purchaseSum);
                    returns.Add(returnsSum);
                    sub.Add(subSum);
                    MyAxis.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
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
        private void fillEmptyRowChart(ObservableCollection<int> stackedButton)
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            IEnumerable<decimal> pTemp = new List<decimal>();
            IEnumerable<decimal> pbTemp = new List<decimal>();
            IEnumerable<decimal> resultTemp = new List<decimal>();
            IEnumerable<ItemTransferInvoice> temp = new List<ItemTransferInvoice>();

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

            int xCount = 0;
            if (pTemp.Count() <= 6) xCount = pTemp.Count();
            for (int i = 0; i < xCount; i++)
            {
                purchase.Add(pTemp.ToList().Skip(i).FirstOrDefault());
                returns.Add(pbTemp.ToList().Skip(i).FirstOrDefault());
                sub.Add(resultTemp.ToList().Skip(i).FirstOrDefault());

                int index = names.FindIndex(s => s == "unknown");

                if (index != -1)
                    names[index] = MainWindow.resourcemanager.GetString("trCashCustomer");

                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }
            if (pTemp.Count() > 6)
            {
                decimal purchaseSum = 0, returnsSum = 0, subSum = 0;
                for (int i = 6; i < pTemp.Count(); i++)
                {
                    purchaseSum = purchaseSum + pTemp.ToList().Skip(i).FirstOrDefault();
                    returnsSum = returnsSum + pbTemp.ToList().Skip(i).FirstOrDefault();
                    subSum = subSum + resultTemp.ToList().Skip(i).FirstOrDefault();
                }
                if (!((purchaseSum == 0) && (returnsSum == 0) && (subSum == 0)))
                {
                    purchase.Add(purchaseSum);
                    returns.Add(returnsSum);
                    sub.Add(subSum);
                    MyAxis.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
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
