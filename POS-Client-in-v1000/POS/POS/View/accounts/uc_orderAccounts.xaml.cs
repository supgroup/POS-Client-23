using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.sectionData.Charts;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
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

namespace POS.View.accounts
{
    /// <summary>
    /// Interaction logic for uc_orderAccounts.xaml
    /// </summary>
    public partial class uc_orderAccounts : UserControl
    {
        private static uc_orderAccounts _instance;
        public static uc_orderAccounts Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_orderAccounts();
                return _instance;
            }
        }
        public uc_orderAccounts()
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
        CashTransfer cashModel = new CashTransfer();
        Invoice invoiceModel = new Invoice();
        Branch branchModel = new Branch();
        CashTransfer cashtrans = new CashTransfer();
        Invoice invoice = new Invoice();

        Bonds bondModel = new Bonds();
        Card cardModel = new Card();
        Agent agentModel = new Agent();
        User userModel = new User();
        Pos posModel = new Pos();
        List<Agent> agents;
        List<Agent> customers;
        IEnumerable<User> users;
        IEnumerable<Card> cards;
        IEnumerable<Invoice> invoiceQuery;
        IEnumerable<Invoice> invoiceQueryExcel;
        IEnumerable<Invoice> invoices;
        IEnumerable<Branch> branches;
        int agentId, userId;
        string searchText = "";
        string createPermission = "ordersAccounting_create";
        string reportsPermission = "ordersAccounting_reports";
        string BranchesPermission = "ordersAccounting_allBranches";

        #endregion

        List<Agent> newListAgents = new List<Agent>();
        List<User> newListUsers = new List<User>();
        List<Branch> newListBranches = new List<Branch>();
        private void Btn_confirm_Click(object sender, RoutedEventArgs e)
        {//confirm

        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                //SectionData.fillBranches(cb_branch, "bs");/////permissions


                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucOrderAccounts.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucOrderAccounts.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                #region Style Date
                /////////////////////////////////////////////////////////////
                SectionData.defaultDatePickerStyle(dp_startSearchDate);
                SectionData.defaultDatePickerStyle(dp_endSearchDate);
                /////////////////////////////////////////////////////////////
                #endregion

                #region key up
                cb_branch.IsTextSearchEnabled = false;
                cb_branch.IsEditable = true;
                cb_branch.StaysOpenOnEdit = true;
                cb_branch.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branch.Text = "";

                cb_salesMan.IsTextSearchEnabled = false;
                cb_salesMan.IsEditable = true;
                cb_salesMan.StaysOpenOnEdit = true;
                cb_salesMan.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_salesMan.Text = "";

                cb_customer.IsTextSearchEnabled = false;
                cb_customer.IsEditable = true;
                cb_customer.StaysOpenOnEdit = true;
                cb_customer.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_customer.Text = "";
                #endregion

                dp_endSearchDate.SelectedDate = DateTime.Now;
                dp_startSearchDate.SelectedDate = DateTime.Now;

                dp_startSearchDate.SelectedDateChanged += this.dp_SelectedStartDateChanged;
                dp_endSearchDate.SelectedDateChanged += this.dp_SelectedEndDateChanged;

                btn_image.IsEnabled = false;

                btn_save.IsEnabled = false;

                #region fill process type
                var typelist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trCredit")    , Value = "balance" },
                new { Text = MainWindow.resourcemanager.GetString("trCash")       , Value = "cash" },
                new { Text = MainWindow.resourcemanager.GetString("trDocument")   , Value = "doc" },
                new { Text = MainWindow.resourcemanager.GetString("trCheque")     , Value = "cheque" },
                new { Text = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods") , Value = "card" }
                 };
                cb_paymentProcessType.DisplayMemberPath = "Text";
                cb_paymentProcessType.SelectedValuePath = "Value";
                cb_paymentProcessType.ItemsSource = typelist;
                #endregion

                #region fill card combo
                try
                {
                    if (FillCombo.cardsList is null)
                        await FillCombo.RefreshCards();
                    cards = FillCombo.cardsList;
                    cb_card.ItemsSource = cards;
                    cb_card.DisplayMemberPath = "name";
                    cb_card.SelectedValuePath = "cardId";
                    cb_card.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill branch combo1
                try
                {
                    if (FillCombo.branchsActiveList_b is null)
                        await FillCombo.RefreshBranchsActive_b();
                    branches = FillCombo.branchsActiveList_b.ToList();
                    //branches = await branchModel.GetBranchesActive("b");
                    newListBranches = branches.ToList();
                    var br = new Branch();
                    br.branchId = 0;
                    br.name = MainWindow.resourcemanager.GetString("trAll");
                    newListBranches.Insert(0, br);
                    cb_branch.ItemsSource = newListBranches;
                    cb_branch.DisplayMemberPath = "name";
                    cb_branch.SelectedValuePath = "branchId";
                    cb_branch.SelectedValue = MainWindow.branchID.Value;

                    if (MainWindow.groupObject.HasPermissionAction(BranchesPermission, MainWindow.groupObjects, "one"))
                        cb_branch.IsEnabled = true;
                    else
                        cb_branch.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill agent combo
                agents = new List<Agent>();
                 customers = new List<Agent>();
                try
                {
                    if (FillCombo.customersList is null)
                        await FillCombo.RefreshCustomers();
                    if (FillCombo.vendorsList is null)
                        await FillCombo.RefreshVendors();

                    customers = FillCombo.customersList;
                    agents =  FillCombo.vendorsList;
                    agents.AddRange(customers);

                    newListAgents = customers.ToList();
                    //var cu = new Agent();
                    //cu.agentId = 0;
                    //cu.name = "-";
                    //newListAgents.Insert(0, cu);

                    cb_customer.ItemsSource = newListAgents;
                    cb_customer.DisplayMemberPath = "name";
                    cb_customer.SelectedValuePath = "agentId";
                    cb_customer.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill salesman combo
                try
                {
                    if(FillCombo.usersActiveList is null)
                    await FillCombo.RefreshUsersActive();
                    users = FillCombo.usersActiveList;
                    //users = await userModel.GetUsersActive();

                    newListUsers = users.ToList();
                    var us = new User();
                    us.userId = 0;
                    us.fullName = "-";
                    newListUsers.Insert(0, us);
                    cb_salesMan.ItemsSource = newListUsers;
                    cb_salesMan.DisplayMemberPath = "fullName";
                    cb_salesMan.SelectedValuePath = "userId";
                    cb_salesMan.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill status combo
                var statuslist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trDelivered")  , Value = "rc" },
                new { Text = MainWindow.resourcemanager.GetString("trInDelivery")   , Value = "tr" }
                 };
                cb_state.DisplayMemberPath = "Text";
                cb_state.SelectedValuePath = "Value";
                cb_state.ItemsSource = statuslist;
                #endregion

                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        private void translate()
        {
            txt_order.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trTransaferDetails");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branch, MainWindow.resourcemanager.GetString("trBranchHint"));
            // chk_delivered.Content = MainWindow.resourcemanager.GetString("trDelivered");
            //chk_inDelivery.Content = MainWindow.resourcemanager.GetString("trInDelivery");
            chk_delivered.Content = MainWindow.resourcemanager.GetString("trDone");
            chk_inDelivery.Content = MainWindow.resourcemanager.GetString("trDelivered");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_invoiceNum, MainWindow.resourcemanager.GetString("trInvoiceNumberHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentProcessType, MainWindow.resourcemanager.GetString("trPaymentTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_docNum, MainWindow.resourcemanager.GetString("trDocNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_docDate, MainWindow.resourcemanager.GetString("trDocDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_docNumCheque, MainWindow.resourcemanager.GetString("trDocNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_docNumCard, MainWindow.resourcemanager.GetString("trProcessNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_docDateCheque, MainWindow.resourcemanager.GetString("trDocDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCashHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cashDelivered, MainWindow.resourcemanager.GetString("trCashHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_card, MainWindow.resourcemanager.GetString("trCardHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_salesMan, MainWindow.resourcemanager.GetString("trSalesManHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_customer, MainWindow.resourcemanager.GetString("trCustomerHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_state, MainWindow.resourcemanager.GetString("trStateHint"));

            chb_all.Content = MainWindow.resourcemanager.GetString("trAll");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startSearchDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endSearchDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            dg_orderAccounts.Columns[0].Header = MainWindow.resourcemanager.GetString("trInvoiceNumber");
            dg_orderAccounts.Columns[1].Header = MainWindow.resourcemanager.GetString("trSalesMan");
            dg_orderAccounts.Columns[2].Header = MainWindow.resourcemanager.GetString("trCustomer");
            dg_orderAccounts.Columns[3].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_orderAccounts.Columns[4].Header = MainWindow.resourcemanager.GetString("trCashTooltip");
            dg_orderAccounts.Columns[5].Header = MainWindow.resourcemanager.GetString("trState");
            dg_orderAccounts.Columns[6].Header = MainWindow.resourcemanager.GetString("trState");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_startDate.Content = MainWindow.resourcemanager.GetString("trStartDate");
            tt_endDate.Content = MainWindow.resourcemanager.GetString("trEndDate");
            tt_customer.Content = MainWindow.resourcemanager.GetString("trVendor/Customer");
            btn_save.Content = MainWindow.resourcemanager.GetString("trPay");
            btn_image.Content = MainWindow.resourcemanager.GetString("trImage");
            btn_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            btn_printInvoice.Content = MainWindow.resourcemanager.GetString("trPrint");
            btn_pdf.Content = MainWindow.resourcemanager.GetString("trPdfBtn");
            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trPay");
        }
        private void Clear()
        {
            btn_image.IsEnabled = false;

            tb_cash.Clear();
            tb_cashDelivered.Clear();
            tb_note.Clear();
            tb_invoiceNum.Text = "";
            SectionData.clearValidate(tb_cash, p_errorCash);
            SectionData.clearComboBoxValidate(cb_paymentProcessType, p_errorpaymentProcessType);
            SectionData.clearComboBoxValidate(cb_card, p_errorCard);
            SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
            SectionData.clearValidate(tb_docNum, p_errorDocNum);
            SectionData.clearValidate(tb_docNum, p_errorDocNum);
            SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
        }
        async Task<IEnumerable<Invoice>> RefreshInvoiceList()
        {
            invoices = await invoiceModel.getOrdersForPay(Convert.ToInt32(cb_branch.SelectedValue));
            return invoices;

        }
        void RefreshInvoiceView()
        {
            dg_orderAccounts.ItemsSource = invoiceQuery;
            txt_count.Text = invoiceQuery.Count().ToString();
        }
        void FN_ExportToExcel()
        {
            var QueryExcel = invoiceQuery.AsEnumerable().Select(x => new
            {
                InvoiceNumber = x.invNumber,
                SalesMan = x.shipUserName,
                Customer = x.agentName,
                Cash = x.totalNet,
                Status = x.status
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trInvoiceNumber");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trSalesMan");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trCustomer");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trCashTooltip");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trState");

            ExportToExcel.Export(DTForExcel);

        }
        private async Task fillCustomers()
        {
            await FillCombo.FillComboCustomers_withDefault(cb_customer);
            //agents = await agentModel.GetAgentsActive("c");

            //List<Agent> newListAgents = agents.ToList();
            //var ag = new Agent();
            //ag.agentId = 0;
            //ag.name = "-";
            //newListAgents.Insert(0, ag);

            //cb_customer.ItemsSource = newListAgents;
            //cb_customer.DisplayMemberPath = "name";
            //cb_customer.SelectedValuePath = "agentId";
            //cb_salesMan.SelectedIndex = -1;
        }
        private async Task fillUsers()
        {
            users = await userModel.GetUsersActive();

            cb_salesMan.ItemsSource = users;
            cb_salesMan.DisplayMemberPath = "username";
            cb_salesMan.SelectedValuePath = "userId";
            cb_salesMan.SelectedIndex = -1;
        }
        //private async Task saveOrderStatus(int invoiceId, string status)
        //{
        //    invoiceStatus st = new invoiceStatus();
        //    st.status = status;
        //    st.invoiceId = invoiceId;
        //    st.createUserId = MainWindow.userLogin.userId;
        //    st.isActive = 1;
        //    await invoice.saveOrderStatus(st);
        //}

        //private async Task saveConfiguredCashTrans(CashTransfer cashTransfer)
        //{
        //    switch (cashTransfer.processType)
        //    {
        //        case "cash":// cash: update pos balance   
        //            MainWindow.posLogIn.balance += invoice.totalNet;
        //            await MainWindow.posLogIn.save(MainWindow.posLogIn);
        //            cashTransfer.transType = "d"; //deposit
        //            cashTransfer.posId = MainWindow.posID;
        //            cashTransfer.agentId = invoice.agentId;
        //            cashTransfer.invId = invoice.invoiceId;
        //            cashTransfer.transNum = await cashTransfer.generateCashNumber("dc");
        //            cashTransfer.side = "c"; // customer                    
        //            cashTransfer.createUserId = MainWindow.userID;
        //            await cashTransfer.Save(cashTransfer); //add cash transfer   
        //            break;
        //        case "balance":// balance: update customer balance
        //                       //if (cb_company.SelectedIndex != -1 && companyModel.deliveryType.Equals("com"))
        //                       //    await invoice.recordComSpecificPaidCash(invoice, "si", cashTransfer);
        //                       //else
        //            await invoice.recordConfiguredAgentCash(invoice, "si", cashTransfer);
        //            break;
        //        case "card": // card
        //            cashTransfer.transType = "d"; //deposit
        //            cashTransfer.posId = MainWindow.posID;
        //            cashTransfer.agentId = invoice.agentId;
        //            cashTransfer.invId = invoice.invoiceId;
        //            cashTransfer.transNum = await cashTransfer.generateCashNumber("dc");
        //            cashTransfer.side = "c"; // customer
        //            cashTransfer.createUserId = MainWindow.userID;
        //            await cashTransfer.Save(cashTransfer); //add cash transfer  
        //            break;
        //    }
        //}
        private decimal getCusAvailableBlnc(Agent customer)
        {
            decimal remain = 0;

            float customerBalance = customer.balance;

            if (customer.balanceType == 0)
                remain = invoice.totalNet.Value - (decimal)customerBalance;
            else
                remain = (decimal)customer.balance + invoice.totalNet.Value;
            return remain;
        }
        #endregion

        #region events
        private async void dp_SelectedEndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                await RefreshInvoiceList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void dp_SelectedStartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                await RefreshInvoiceList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_orderAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                #region clear validate
                SectionData.clearComboBoxValidate(cb_paymentProcessType, p_errorpaymentProcessType);
                SectionData.clearComboBoxValidate(cb_card, p_errorpaymentProcessType);
                TextBox tbDocDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                SectionData.clearValidate(tb_docNum, p_errorDocNum);
                SectionData.clearValidate(tb_cash, p_errorCash);
                #endregion

                if (dg_orderAccounts.SelectedIndex != -1)
                {
                    invoice = dg_orderAccounts.SelectedItem as Invoice;
                    this.DataContext = cashtrans;

                    if (invoice != null)
                    {
                        btn_image.IsEnabled = true;

                        tb_invoiceNum.Text = invoice.invNumber;

                        agentId = invoice.agentId.Value;

                        userId = invoice.shipUserId.Value;

                        tb_cash.Text = SectionData.DecTostring(invoice.deserved);

                        tb_cashDelivered.Text = SectionData.DecTostring(invoice.paid);

                        if (invoice.status == "Done")
                        {
                            btn_save.IsEnabled = false;
                            tb_note.IsEnabled = false;
                            SectionData.clearValidate(tb_cash, p_errorCash);
                        }
                        else
                        {
                            btn_save.IsEnabled = true;
                            tb_note.IsEnabled = true;
                        }
                    }
                    else
                    {
                        btn_save.IsEnabled = false;
                        btn_image.IsEnabled = false;
                    }
                }
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                //try
                //{
                if (invoices is null)
                        await RefreshInvoiceList();

                    if (chb_all.IsChecked == false)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            searchText = tb_search.Text.ToLower();
                            if (chk_delivered.IsChecked == true)
                                invoiceQuery = invoices.Where(s => (s.invNumber.ToLower().Contains(searchText)
                                || s.branchCreatorName.ToLower().Contains(searchText)
                                || s.shipUserName.ToLower().Contains(searchText)
                                || s.agentName.ToLower().Contains(searchText)
                                || s.totalNet.ToString().ToLower().Contains(searchText)
                                || s.status.ToLower().Contains(searchText)
                                )
                                && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                                && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                                && s.status == "Done"//rc
                                );
                            else if (chk_inDelivery.IsChecked == true)
                                invoiceQuery = invoices.Where(s => (s.invNumber.ToLower().Contains(searchText)
                               || s.branchCreatorName.ToLower().Contains(searchText)
                               || s.shipUserName.ToLower().Contains(searchText)
                               || s.agentName.ToLower().Contains(searchText)
                               || s.totalNet.ToString().ToLower().Contains(searchText)
                               || s.status.ToLower().Contains(searchText)
                               )
                               && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                               && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                               && s.status == "Delivered"//tr
                               );

                        });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            searchText = tb_search.Text.ToLower();
                            if (chk_delivered.IsChecked == true)
                                invoiceQuery = invoices.Where(s => (s.invNumber.ToLower().Contains(searchText)
                                || s.branchCreatorName.ToLower().Contains(searchText)
                                || s.shipUserName.ToLower().Contains(searchText)
                                || s.agentName.ToLower().Contains(searchText)
                                || s.totalNet.ToString().ToLower().Contains(searchText)
                                || s.status.ToLower().Contains(searchText)
                                )
                                 && s.status == "Done"//rc
                                );
                            else if (chk_inDelivery.IsChecked == true)
                                invoiceQuery = invoices.Where(s => (s.invNumber.ToLower().Contains(searchText)
                                || s.branchCreatorName.ToLower().Contains(searchText)
                                || s.shipUserName.ToLower().Contains(searchText)
                                || s.agentName.ToLower().Contains(searchText)
                                || s.totalNet.ToString().ToLower().Contains(searchText)
                                || s.status.ToLower().Contains(searchText)
                                )
                                 && s.status == "Delivered"//tr
                                );

                        });
                    }

                    invoiceQueryExcel = invoiceQuery.ToList();
                    RefreshInvoiceView();
                //}
                //catch { }
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        //private async Task<decimal> saveBond(string num, decimal ammount, Nullable<DateTime> date, string type)
        //{
        //    Bonds bond = new Bonds();
        //    bond.number = num;
        //    bond.amount = ammount;
        //    bond.deserveDate = date;
        //    bond.type = type;
        //    bond.isRecieved = 0;
        //    bond.createUserId = MainWindow.userID.Value;

        //    int s = await bondModel.Save(bond);

        //    return s;
        //}

        //private async Task calcBalance(decimal ammount)
        //{
        //    int s = 0;
        //    //SC Commerce balance
        //    Pos pos = await posModel.getById(MainWindow.posID.Value);
        //    pos.balance += ammount;

        //    s = await pos.save(pos);
        //}
        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update

        }
        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete

        }
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                Clear();

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_image_Click(object sender, RoutedEventArgs e)
        {//image
            try
            {
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one"))
                {
                    if (cashtrans != null || cashtrans.cashTransId != 0)
                    {
                        wd_uploadImage w = new wd_uploadImage();

                        w.tableName = "invoices";
                        w.tableId = invoice.invoiceId;
                        w.docNum = invoice.invNumber;
                        w.ShowDialog();
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                await RefreshInvoiceList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void validateEmpty(string name, object sender)
        {
            if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_cash")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
                else if ((sender as TextBox).Name == "tb_docNum")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorDocNum, tt_errorDocNum, "trEmptyDocNumToolTip");
                else if ((sender as TextBox).Name == "tb_docNumCheque")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorDocNumCheque, tt_errorDocNumCheque, "trEmptyDocNumToolTip");
                else if ((sender as TextBox).Name == "tb_docNumCard")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorDocCard, tt_errorDocCard, "trEmptyProcessNumToolTip");
            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_paymentProcessType")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");
                else if ((sender as ComboBox).Name == "cb_card")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCard, tt_errorCard, "trEmptyCardTooltip");
            }
            else if (name == "DatePicker")
            {
                if ((sender as DatePicker).Name == "dp_docDate")
                    SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorDocDate, tt_errorDocDate, "trEmptyDocDateToolTip");
                if ((sender as DatePicker).Name == "dp_docDateCheque")
                    SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorDocDateCheque, tt_errorDocDateCheque, "trEmptyDocDateToolTip");
            }
        }
        private void PreventSpaces(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private void Tb_docNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only int
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void Tb_cash_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only decimal
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;
            else
                e.Handled = true;
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_cash")
                    SectionData.InputJustNumber(ref txb);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        string processType = "";
        private void Cb_paymentProcessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//type selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                switch (cb_paymentProcessType.SelectedIndex)
                {
                    case 0://balance
                        grid_doc.Visibility = Visibility.Collapsed;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        cb_card.Visibility = Visibility.Collapsed;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNum);
                        SectionData.clearComboBoxValidate(cb_card, p_errorCard);
                        if (grid_doc.IsVisible)
                        {
                            TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                            SectionData.clearValidate(dpDate, p_errorDocDate);
                        }
                        if (grid_cheque.IsVisible)
                        {
                            TextBox dpDateCheque = (TextBox)dp_docDateCheque.Template.FindName("PART_TextBox", dp_docDateCheque);
                            SectionData.clearValidate(dpDateCheque, p_errorDocNumCheque);
                        }
                        if (invoice != null)
                        {
                            tb_cash.Text = invoice.deserved.ToString();
                            tb_cash.IsEnabled = false;
                        }
                        processType = "1";
                        break;

                    case 1://cash
                        grid_doc.Visibility = Visibility.Collapsed;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        cb_card.Visibility = Visibility.Collapsed;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNum);
                        SectionData.clearComboBoxValidate(cb_card, p_errorCard);
                        if (grid_doc.IsVisible)
                        {
                            TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                            SectionData.clearValidate(dpDate, p_errorDocDate);
                        }
                        if (grid_cheque.IsVisible)
                        {
                            TextBox dpDateCheque = (TextBox)dp_docDateCheque.Template.FindName("PART_TextBox", dp_docDateCheque);
                            SectionData.clearValidate(dpDateCheque, p_errorDocNumCheque);
                        }
                        tb_cash.IsEnabled = true;
                        tb_cash.Clear();
                        SectionData.clearValidate(tb_cash, p_errorCash);
                        processType = "0";
                        break;

                    case 2://doc
                        grid_doc.Visibility = Visibility.Visible;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        cb_card.Visibility = Visibility.Collapsed;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNum);
                        SectionData.clearComboBoxValidate(cb_card, p_errorCard);
                        if (grid_cheque.IsVisible)
                        {
                            TextBox dpDateCheque = (TextBox)dp_docDateCheque.Template.FindName("PART_TextBox", dp_docDateCheque);
                            SectionData.clearValidate(dpDateCheque, p_errorDocNumCheque);
                        }
                        tb_cash.IsEnabled = true;
                        tb_cash.Clear();
                        SectionData.clearValidate(tb_cash, p_errorCash);
                        processType = "0";
                        break;

                    case 3://cheque
                        grid_doc.Visibility = Visibility.Collapsed;
                        grid_cheque.Visibility = Visibility.Visible;
                        cb_card.Visibility = Visibility.Collapsed;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearComboBoxValidate(cb_card, p_errorCard);
                        if (grid_doc.IsVisible)
                        {
                            TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                            SectionData.clearValidate(dpDate, p_errorDocDate);
                        }
                        tb_cash.IsEnabled = true;
                        tb_cash.Clear();
                        SectionData.clearValidate(tb_cash, p_errorCash);
                        processType = "0";
                        break;

                    case 4://card
                        grid_doc.Visibility = Visibility.Collapsed;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        cb_card.Visibility = Visibility.Visible;
                        tb_docNumCard.Visibility = Visibility.Visible;
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNum);
                        SectionData.clearComboBoxValidate(cb_card, p_errorCard);
                        if (grid_doc.IsVisible)
                        {
                            TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                            SectionData.clearValidate(dpDate, p_errorDocDate);
                        }
                        if (grid_cheque.IsVisible)
                        {
                            TextBox dpDateCheque = (TextBox)dp_docDateCheque.Template.FindName("PART_TextBox", dp_docDateCheque);
                            SectionData.clearValidate(dpDateCheque, p_errorDocNumCheque);
                        }
                        tb_cash.IsEnabled = true;
                        tb_cash.Clear();
                        SectionData.clearValidate(tb_cash, p_errorCash);
                        processType = "0";
                        break;
                }

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_invoices_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Cb_salesMan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select salesman
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                invoiceQuery = invoiceQuery.Where(u => u.shipUserId == Convert.ToInt32(cb_salesMan.SelectedValue));
                invoiceQueryExcel = invoiceQuery;
                RefreshInvoiceView();

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_customer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select agent
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                invoiceQuery = invoiceQuery.Where(c => c.agentId == Convert.ToInt32(cb_customer.SelectedValue));
                invoiceQueryExcel = invoiceQuery;
                RefreshInvoiceView();

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_state_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select state
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                invoiceQuery = invoiceQuery.Where(s => s.status == cb_state.SelectedValue.ToString());
                invoiceQueryExcel = invoiceQuery;
                RefreshInvoiceView();

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Cb_branch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                await RefreshInvoiceList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
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
        private async void Chb_all_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dp_startSearchDate.IsEnabled =
            dp_endSearchDate.IsEnabled = false;
                Btn_refresh_Click(btn_refresh, null);
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
                    if (cb.Name == "chk_delivered")
                        chk_delivered.IsChecked = true;
                    else if (cb.Name == "chk_inDelivery")
                        chk_inDelivery.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void search_Checking(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "chk_delivered")
                    {
                        chk_inDelivery.IsChecked = false;
                        dg_orderAccounts.Columns[4].Visibility = Visibility.Collapsed;
                        dg_orderAccounts.Columns[6].Visibility = Visibility.Visible;
                        tb_cash.Visibility = Visibility.Collapsed;
                        tb_cashDelivered.Visibility = Visibility.Visible;
                        btn_save.IsEnabled = false;
                    }
                    else if (cb.Name == "chk_inDelivery")
                    {
                        chk_delivered.IsChecked = false;
                        dg_orderAccounts.Columns[4].Visibility = Visibility.Visible;
                        dg_orderAccounts.Columns[6].Visibility = Visibility.Collapsed;
                        tb_cash.Visibility = Visibility.Visible;
                        tb_cashDelivered.Visibility = Visibility.Collapsed;
                        btn_save.IsEnabled = true;
                    }
                }
                SectionData.StartAwait(grid_ucOrderAccounts);

                Clear();
                await RefreshInvoiceList();
                Tb_search_TextChanged(null, null);

                SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_ucOrderAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Chb_all_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                dp_startSearchDate.IsEnabled =
                dp_endSearchDate.IsEnabled = true;

                Btn_refresh_Click(btn_refresh, null);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one"))
                {
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
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_customer_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = newListAgents.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_salesMan_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = newListUsers.Where(p => p.fullName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_branch_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = newListBranches.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region Doc report
        public void BuildvoucherReport()
        {
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {

                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\Ar\ArReciveReportA4.rdlc";
                }
                else
                {
                    addpath = @"\Reports\Account\Ar\ArReciveReport.rdlc";
                }
            }
            else
            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\En\ReciveReportA4.rdlc";
                }
                else
                {
                    addpath = @"\Reports\Account\En\ReciveReport.rdlc";
                }


            }

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.EnableExternalImages = true;
            rep.SetParameters(reportclass.fillPayReport(cashtrans));

            rep.Refresh();
        }
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (cashtrans.cashTransId > 0)
                    {
                        Window.GetWindow(this).Opacity = 0.2;

                        string pdfpath;
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        //

                        BuildvoucherReport();

                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        wd_previewPdf w = new wd_previewPdf();
                        w.pdfPath = pdfpath;
                        if (!string.IsNullOrEmpty(w.pdfPath))
                        {
                            w.ShowDialog();

                            w.wb_pdfWebViewer.Dispose();

                        }
                        else
                            Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        Window.GetWindow(this).Opacity = 1;


                    }

                    else
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                    if (sender != null)
                        SectionData.EndAwait(grid_ucOrderAccounts);

                }

            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {

                    if (cashtrans.cashTransId > 0)
                    {
                        BuildvoucherReport();

                        saveFileDialog.Filter = "PDF|*.pdf;";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;

                            LocalReportExtensions.ExportToPDF(rep, filepath);

                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {

                    if (cashtrans.cashTransId > 0)
                    {
                        BuildvoucherReport();
                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
       
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//pay
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one"))
                {
                    if (MainWindow.posLogIn.boxState == "o")
                    {
                        bool multipleValid = true;
                        List<CashTransfer> listPayments = new List<CashTransfer>();
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_multiplePayment w = new wd_multiplePayment();
                        w.isPurchase = false;
                        //if (cb_customer.SelectedValue != null)
                        //{
                        if (FillCombo.customersList is null)
                            await FillCombo.RefreshCustomers();
                            Agent customer = FillCombo.customersList.ToList().Find(b => b.agentId == invoice.agentId && b.isLimited == true);
                            if (customer != null)
                            {
                                decimal remain = 0;
                                //if (customer.maxDeserve != 0)
                                //    remain = getCusAvailableBlnc(customer);
                                w.hasCredit = true;
                                w.maxCredit = remain;
                            }
                            else
                            {
                                w.hasCredit = false;
                                w.maxCredit = 0;
                            }
                        //}

                        w.invoice.invType = invoice.invType;
                        w.invoice.totalNet = invoice.totalNet;
                        w.cards = cards;
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        multipleValid = w.isOk;
                        listPayments = w.listPayments;

                        if (multipleValid)
                        {
                            invoiceStatus st = new invoiceStatus();
                            st.status = "Done";
                            st.createUserId = MainWindow.userLogin.userId;
                            st.isActive = 1;
                            InvoiceResult invoiceResult = await invoice.saveOrderPayments(invoice,st,listPayments,MainWindow.branchID.Value,MainWindow.posID.Value);

                            if(invoiceResult.Result > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                Clear();

                                await RefreshInvoiceList();
                                Tb_search_TextChanged(null, null);
                                AppSettings.PosBalance = invoiceResult.PosBalance;
                                MainWindow.setBalance();
                            }
                            else
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                            #region Save old

                            //foreach (var item in listPayments)
                            //{
                            //    await saveConfiguredCashTrans(item);

                            //    // yasin code
                            //    if (item.processType != "balance")
                            //    {
                            //    invoice.paid += item.cash;
                            //    invoice.deserved -= item.cash;
                            //    }

                            //}

                            //int s = await invoice.saveInvoice(invoice);

                            //await saveOrderStatus(invoice.invoiceId, "Done");
                            //if (!s.Equals(0))
                            //{
                            //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            //    Clear();

                            //    await RefreshInvoiceList();
                            //    Tb_search_TextChanged(null, null);
                            //    await MainWindow.refreshBalance();
                            //}
                            //else
                            //{
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            //}

                            #endregion

                        }
                    }
                    else //box is closed
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       

        #region reports
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string firstTitle = "orders";
            string secondTitle = "";
            string state = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string searchval = "";
            string Allchk = "";
            string Branchval = "";
            string salesmanval = "";
            string customerval = "";
            string statevale = "";
            //  List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            if (chk_delivered.IsChecked == true)
            {
                secondTitle = "done";
                state = "d";
                statevale = MainWindow.resourcemanagerreport.GetString("trDone");
            }
            else if (chk_inDelivery.IsChecked == true)
            {
                secondTitle = "delivered";
                state = "i";
                statevale = MainWindow.resourcemanagerreport.GetString("trDelivered");
            }    

            string addpath;
            if (isArabic)
            {
                addpath = @"\Reports\Account\Ar\ArOrderAccReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Account\En\OrderAccReport.rdlc";
            }

            //filter
            startDate = dp_startSearchDate.SelectedDate != null ? SectionData.DateToString(dp_startSearchDate.SelectedDate) : "";
            endDate = dp_endSearchDate.SelectedDate != null ? SectionData.DateToString(dp_endSearchDate.SelectedDate) : "";
            Allchk = chb_all.IsChecked == true ? all : "";
            Branchval = cb_branch.SelectedItem != null? cb_branch.Text : "";
            salesmanval = cb_salesMan.SelectedItem != null ? cb_salesMan.Text : "";
            customerval = cb_customer.SelectedItem != null ? cb_customer.Text : "";
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("alldateval", Allchk));
            paramarr.Add(new ReportParameter("Branchval", Branchval));
            paramarr.Add(new ReportParameter("salesmanval", salesmanval));
            paramarr.Add(new ReportParameter("customerval", customerval));
            paramarr.Add(new ReportParameter("statevale", statevale));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("trBranch", MainWindow.resourcemanagerreport.GetString("trBranch")));           
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            //Title = MainWindow.resourcemanagerreport.GetString("trOrders") + " / " + secondTitle;
            Title = clsReports.ReportTabTitle(firstTitle, secondTitle);
            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("state", state));
            //clsReports.orderReport(invoiceQuery, rep, reppath);
            clsReports.orderReport(invoiceQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    BuildReport();
                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_preview1_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    string pdfpath = "";


                    //
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
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvc win = new win_lvc(invoiceQuery, 7);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucOrderAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    BuildReport();

                    saveFileDialog.Filter = "PDF|*.pdf;";

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filepath = saveFileDialog.FileName;
                        LocalReportExtensions.ExportToPDF(rep, filepath);
                    }
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucOrderAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        #endregion

       
    }
}
