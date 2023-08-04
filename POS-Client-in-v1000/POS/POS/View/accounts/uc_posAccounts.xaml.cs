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
    /// Interaction logic for uc_posAccounts.xaml
    /// </summary>
    public partial class uc_posAccounts : UserControl
    {
        private static uc_posAccounts _instance;

        Pos posModel = new Pos();
        Branch branchModel = new Branch();
        IEnumerable<Pos> poss;
        IEnumerable<Branch> branches;
        CashTransfer cashtrans = new CashTransfer();
        CashTransfer cashModel = new CashTransfer();
        IEnumerable<CashTransfer> cashesQuery;
        IEnumerable<CashTransfer> cashesQueryExcel;
        IEnumerable<CashTransfer> cashes;
        string searchText = "";
        CashTransfer cashtrans2 = new CashTransfer();
        CashTransfer cashtrans3 = new CashTransfer();
        CashTransfer cashtrans2temp = new CashTransfer();
        IEnumerable<CashTransfer> cashes2;
        string basicsPermission = "posAccounting_basics";
        string transAdminPermission = "posAccounting_transAdmin";
        public static uc_posAccounts Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_posAccounts();
                return _instance;
            }
        }
        public uc_posAccounts()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void translate()
        {
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trTransaferDetails");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_posAccounts.Text = MainWindow.resourcemanager.GetString("trTransfers");
            txt_Cash.Text = MainWindow.resourcemanager.GetString("trCash_")+" : ";

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCashHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos1, MainWindow.resourcemanager.GetString("trDepositor")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos2, MainWindow.resourcemanager.GetString("trRecepient")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_state, MainWindow.resourcemanager.GetString("trStateHint"));

            chk_deposit.Content = MainWindow.resourcemanager.GetString("trDeposits");
            chk_receive.Content = MainWindow.resourcemanager.GetString("trReceipts");

            chb_all.Content = MainWindow.resourcemanager.GetString("trAll");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startSearchDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endSearchDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            dg_posAccounts.Columns[0].Header = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            dg_posAccounts.Columns[1].Header = MainWindow.resourcemanager.GetString("trCreator");
            if (chk_deposit.IsChecked == true)
            {
                dg_posAccounts.Columns[2].Header = MainWindow.resourcemanager.GetString("trDepositor");
                dg_posAccounts.Columns[3].Header = MainWindow.resourcemanager.GetString("trRecepient");
            }
            else if(chk_receive.IsChecked == true)
            {
                dg_posAccounts.Columns[2].Header = MainWindow.resourcemanager.GetString("trRecepient");
                dg_posAccounts.Columns[3].Header = MainWindow.resourcemanager.GetString("trDepositor");
            }
            dg_posAccounts.Columns[5].Header = MainWindow.resourcemanager.GetString("trStatus");
            dg_posAccounts.Columns[4].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_posAccounts.Columns[6].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

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
            //tt_state.Content = MainWindow.resourcemanager.GetString("trStateToolTip");

            txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trConfirm");
            txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCancel_");
            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");

        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucposAccounts.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucposAccounts.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                #region Style Date
                /////////////////////////////////////////////////////////////
                SectionData.defaultDatePickerStyle(dp_startSearchDate);
                SectionData.defaultDatePickerStyle(dp_endSearchDate);
                /////////////////////////////////////////////////////////////
                #endregion

                dp_startSearchDate.SelectedDate = DateTime.Now.Date;
                dp_endSearchDate.SelectedDate = DateTime.Now.Date;

                dp_startSearchDate.SelectedDateChanged += this.dp_SelectedStartDateChanged;
                dp_endSearchDate.SelectedDateChanged += this.dp_SelectedEndDateChanged;

                try
                {
                    poss = await posModel.Get();
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }

                //SectionData.fillBranches(cb_fromBranch, "bs");
                //cb_fromBranch.SelectedValue = MainWindow.branchID.Value;
                //SectionData.fillBranches(cb_toBranch, "bs");
                //cb_toBranch.SelectedValue = MainWindow.branchID.Value;


                if (!MainWindow.groupObject.HasPermissionAction(transAdminPermission, MainWindow.groupObjects, "one"))
                {
                    cb_fromBranch.IsEnabled = false;////////////permissions
                    cb_toBranch.IsEnabled = false;/////////////permissions
                }

                #region fill branch combo1
                try
                {
                    await FillCombo.FillComboBranchsActive_b(cb_fromBranch);
                    //branches = await branchModel.GetBranchesActive("b");
                    //cb_fromBranch.ItemsSource = branches;
                    //cb_fromBranch.DisplayMemberPath = "name";
                    //cb_fromBranch.SelectedValuePath = "branchId";
                    cb_fromBranch.SelectedValue = MainWindow.branchID.Value;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill branch combo2
                try
                {
                    await FillCombo.FillComboBranchsActive_b(cb_toBranch);
                    //cb_toBranch.ItemsSource = branches;
                    //cb_toBranch.DisplayMemberPath = "name";
                    //cb_toBranch.SelectedValuePath = "branchId";
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill operation state
                //var dislist = new[] {
                //new { Text = MainWindow.resourcemanager.GetString("trUnConfirmed")  , Value = "0" },
                //new { Text = MainWindow.resourcemanager.GetString("trWaiting")      , Value = "1" },
                //new { Text = MainWindow.resourcemanager.GetString("trConfirmed")    , Value = "2" },
                //new { Text = MainWindow.resourcemanager.GetString("trCreatedOper")  , Value = "3" }
                // };
                //cb_state.DisplayMemberPath = "Text";
                //cb_state.SelectedValuePath = "Value";
                //cb_state.ItemsSource = dislist;
                //cb_state.SelectedIndex = 0;
                #endregion

                btn_add.IsEnabled = true;
                btn_update.IsEnabled = true;
                btn_delete.IsEnabled = false;
                btn_confirm.IsEnabled = false;
                btn_cancel.IsEnabled = false;

                //this.Dispatcher.Invoke(() =>
                //{
                Tb_search_TextChanged(null, null);
                //});

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void dp_SelectedEndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                await RefreshCashesList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void dp_SelectedStartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                await RefreshCashesList();
                Tb_search_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Dg_posAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                SectionData.clearValidate(tb_cash, p_errorCash);
                SectionData.clearComboBoxValidate(cb_pos1, p_errorPos1);
                SectionData.clearComboBoxValidate(cb_pos2, p_errorPos2);

                if (dg_posAccounts.SelectedIndex != -1)
                {
                    cashtrans = dg_posAccounts.SelectedItem as CashTransfer;
                    this.DataContext = cashtrans;

                    if (cashtrans != null)
                    {
                        txt_transNum.Text = cashtrans.transNum;

                        tb_cash.Text = SectionData.DecTostring(cashtrans.cash);

                        //login pos is operation pos
                        if (cashtrans.posId == MainWindow.posID.Value)
                        {
                            if (cashtrans.isConfirm == 0)
                            {
                                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trConfirm"); btn_confirm.IsEnabled = true;
                                txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCancel_");  btn_cancel.IsEnabled = true;
                            }
                            else if (cashtrans.isConfirm == 1)
                            {
                                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trIsConfirmed"); btn_confirm.IsEnabled = false;
                                txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCancel_");      btn_cancel.IsEnabled = false;
                            }
                            else if (cashtrans.isConfirm == 2)
                            {
                                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trConfirm"); btn_confirm.IsEnabled = false;
                                txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCanceled"); btn_cancel.IsEnabled = false;
                            }
                        }
                        else
                        {
                            btn_confirm.IsEnabled = false;
                            btn_cancel.IsEnabled = false;
                            if (cashtrans.isConfirm == 0)
                            {
                                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trConfirm");
                                txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCancel_");
                            }
                            else if (cashtrans.isConfirm == 1)
                            {
                                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trIsConfirmed");
                                txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCancel_");
                                btn_cancel.IsEnabled = false;
                            }
                            else if (cashtrans.isConfirm == 2)
                            {
                                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trConfirm");
                                btn_confirm.IsEnabled = false;
                                txt_cancelButton.Text = MainWindow.resourcemanager.GetString("trCanceled");
                            }
                        }

                        #region get two pos
                        cashes2 = await cashModel.GetbySourcId("p", cashtrans.cashTransId);
                        //to insure that the pull operation is in cashtrans2 
                        if (cashtrans.transType == "p")
                        {
                            cashtrans2 = cashes2.ToList()[0] as CashTransfer;
                            cashtrans3 = cashes2.ToList()[1] as CashTransfer;
                        }
                        else if (cashtrans.transType == "d")
                        {
                            cashtrans2 = cashes2.ToList()[1] as CashTransfer;
                            cashtrans3 = cashes2.ToList()[0] as CashTransfer;
                        }

                        cb_fromBranch.SelectedValue = (MainWindow.posList.Where(x => x.posId == cashtrans3.posId).FirstOrDefault() as Pos).branchId;
                        cb_pos1.SelectedValue = cashtrans3.posId;
                        Cb_pos1_SelectionChanged(cb_pos1 , null);

                        cb_toBranch.SelectedValue = (MainWindow.posList.Where(x => x.posId == cashtrans2.posId).FirstOrDefault() as Pos).branchId;
                        cb_pos2.SelectedValue = cashtrans2.posId;
                        Cb_pos2_SelectionChanged(cb_pos2, null);

                        if ((cashtrans2.isConfirm == 1) && (cashtrans3.isConfirm == 1))
                            btn_update.IsEnabled = false;

                        //else
                        //    btn_update.IsEnabled = true;
                        #endregion
                        if ((cashtrans.posIdCreator == MainWindow.posID.Value || SectionData.isAdminPermision()) && !(cashtrans2.isConfirm == 1 && cashtrans3.isConfirm == 1))
                            btn_delete.IsEnabled = true;
                        else
                            btn_delete.IsEnabled = false;

                    }
                }
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    try
                    {
                        if (cashes is null)
                            await RefreshCashesList();

                        searchText = tb_search.Text.ToLower();
                        if (chb_all.IsChecked == false)
                        {
                            #region old
                            //switch (Convert.ToInt32(cb_state.SelectedValue))
                            //{
                            //    case 0://inconfirmed
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //         || s.transType.Contains(searchText)
                            //         || s.cash.ToString().Contains(searchText)
                            //         || s.posName.Contains(searchText)
                            //         )
                            //        && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                            //        && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                            //        && s.posId == MainWindow.posID.Value
                            //        && s.isConfirm == 0
                            //        );
                            //        break;
                            //    case 1://waiting
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //        && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                            //        && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                            //        && s.posId == MainWindow.posID.Value
                            //        && s.isConfirm == 1
                            //        //&& another is not confirmed 
                            //        && s.isConfirm2 == 0
                            //        );
                            //        break;
                            //    case 2://confirmed
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //        && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                            //        && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                            //        && s.posId == MainWindow.posID.Value
                            //        && s.isConfirm == 1
                            //        //&& another is confirmed
                            //        && s.isConfirm2 == 1
                            //        );
                            //        break;
                            //    case 3://created by me
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //        && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                            //        && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                            //        && s.posIdCreator == MainWindow.posID.Value
                            //        );
                            //        break;
                            //    default://no select
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //       && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                            //       && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                            //       );
                            //        break;
                            //}
                            #endregion
                            if (chk_deposit.IsChecked == true)
                            {
                                cashesQuery = cashes.Where(s => (s.transNum.ToLower().Contains(searchText)
                                    || s.transType.ToLower().Contains(searchText)
                                    || s.cash.ToString().Contains(searchText)
                                    || s.posName.ToLower().Contains(searchText)
                                    )
                                && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                                && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                                && s.transType == "d"
                                );
                            }
                            else if (chk_receive.IsChecked == true)
                            {
                                cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                                || s.transType.Contains(searchText)
                                || s.cash.ToString().Contains(searchText)
                                || s.posName.Contains(searchText)
                                )
                                && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                                && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                                //&& s.posId == MainWindow.posID.Value
                                && s.transType == "p"
                                );
                            }
                        }
                        else
                        {
                            #region old
                            //switch (Convert.ToInt32(cb_state.SelectedValue))
                            //{
                            //    case 0://inconfirmed
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //         || s.transType.Contains(searchText)
                            //         || s.cash.ToString().Contains(searchText)
                            //         || s.posName.Contains(searchText)
                            //         )
                            //        && s.posId == MainWindow.posID.Value
                            //        && s.isConfirm == 0
                            //        );
                            //        break;
                            //    case 1://waiting
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //        && s.posId == MainWindow.posID.Value
                            //        && s.isConfirm == 1
                            //        //&& another is not confirmed 
                            //        && s.isConfirm2 == 0
                            //        );
                            //        break;
                            //    case 2://confirmed
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //        && s.posId == MainWindow.posID.Value
                            //        && s.isConfirm == 1
                            //        //&& another is confirmed
                            //        && s.isConfirm2 == 1
                            //        );
                            //        break;
                            //    case 3://created by me
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //        && s.posIdCreator == MainWindow.posID.Value
                            //        );
                            //        break;
                            //    default://no select
                            //        cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                            //        || s.transType.Contains(searchText)
                            //        || s.cash.ToString().Contains(searchText)
                            //        || s.posName.Contains(searchText)
                            //        )
                            //       );
                            //        break;
                            //}
                            #endregion
                            if (chk_deposit.IsChecked == true)
                            {
                                cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                                    || s.transType.Contains(searchText)
                                    || s.cash.ToString().Contains(searchText)
                                    || s.posName.Contains(searchText)
                                    )
                                && s.transType == "d"
                                );
                            }
                            else if (chk_receive.IsChecked == true)
                            {
                                cashesQuery = cashes.Where(s => (s.transNum.Contains(searchText)
                                || s.transType.Contains(searchText)
                                || s.cash.ToString().Contains(searchText)
                                || s.posName.Contains(searchText)
                                )
                                //&& s.posId == MainWindow.posID.Value
                                && s.transType == "p"
                                );
                            }
                        }

                        RefreshCashView();
                        cashesQueryExcel = cashesQuery.ToList();
                        txt_count.Text = cashesQuery.Count().ToString();
                    }
                    catch (Exception ex)
                    {
                        SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                    }
                }
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        #region validate
                       
                        //chk empty cash
                        SectionData.validateEmptyTextBox(tb_cash, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
                        //chk empty pos1
                        SectionData.validateEmptyComboBox(cb_pos1, p_errorPos1, tt_errorPos1, "trErrorEmptyFromPosToolTip");
                        //chk empty pos2
                        SectionData.validateEmptyComboBox(cb_pos2, p_errorPos2, tt_errorPos2, "trErrorEmptyToPosToolTip");
                        //cash <= 0
                        decimal cash = 0;
                        try
                        {
                            cash = decimal.Parse(tb_cash.Text);
                        }
                        catch (Exception ex)
                        {
                            SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                        }

                        if (cash <= 0)
                            SectionData.showTextBoxValidate(tb_cash, p_errorCash, tt_errorCash, "itMustBeGreaterThanZero");
                        //chk if 2 pos is the same
                        bool isSame = false;
                        if (cb_pos1.SelectedValue == cb_pos2.SelectedValue)
                            isSame = true;
                        if ((cb_pos1.SelectedIndex != -1) && (cb_pos2.SelectedIndex != -1) && (cb_pos1.SelectedValue == cb_pos2.SelectedValue))
                        {
                            SectionData.showComboBoxValidate(cb_pos1, p_errorPos1, tt_errorPos1, "trErrorSamePos");
                            SectionData.showComboBoxValidate(cb_pos2, p_errorPos2, tt_errorPos2, "trErrorSamePos");
                        }
                       
                        #endregion

                        #region add

                        if ((!tb_cash.Text.Equals("")) && (cash > 0) && (!cb_pos1.Text.Equals("")) && (!cb_pos2.Text.Equals("")) && !isSame /*&& !validTransAdmin()*/)
                        {
                            //Pos pos = await posModel.getById(Convert.ToInt32(cb_pos1.SelectedValue));
                            //if (pos.balance < decimal.Parse(tb_cash.Text))
                            //{ Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn); }
                            //else
                            {
                                //first operation
                                CashTransfer cash1 = new CashTransfer();

                                cash1.transType = "d";//deposit
                                //cash1.transNum = await cashModel.generateCashNumber(cash1.transType + "p");
                                cash1.transNum ="dp";
                                cash1.cash = decimal.Parse(tb_cash.Text);
                                cash1.createUserId = MainWindow.userID.Value;
                                cash1.notes = tb_note.Text;
                                cash1.posIdCreator = MainWindow.posID.Value;
                                if (Convert.ToInt32(cb_pos1.SelectedValue) == MainWindow.posID)
                                    cash1.isConfirm = 1;
                                else cash1.isConfirm = 0;
                                cash1.side = "p";//pos
                                cash1.posId = Convert.ToInt32(cb_pos1.SelectedValue);

                                decimal s1 = await cashModel.SaveWithBalanceCheck(cash1);

                                if (s1  > 0)
                                {
                                    await MainWindow.refreshBalance();
                                    //second operation
                                    CashTransfer cash2 = new CashTransfer();

                                    cash2.transType = "p";//pull
                                    //cash2.transNum = await cashModel.generateCashNumber(cash2.transType + "p");
                                    cash2.transNum = "pp";
                                    cash2.cash = decimal.Parse(tb_cash.Text);
                                    cash2.createUserId = MainWindow.userID.Value;
                                    cash2.posIdCreator = MainWindow.posID.Value;
                                    if (Convert.ToInt32(cb_pos2.SelectedValue) == MainWindow.posID)
                                        cash2.isConfirm = 1;
                                    else cash2.isConfirm = 0;
                                    cash2.side = "p";//pos
                                    cash2.posId = Convert.ToInt32(cb_pos2.SelectedValue);
                                    cash2.cashTransIdSource =(int) s1;//id from first operation

                                    decimal s2 = await cashModel.Save(cash2);

                                    if (!s2.Equals(0))
                                    {
                                        #region notification Object
                                        int pos1 = 0;
                                        int pos2 = 0;
                                        if ((int)cb_pos1.SelectedValue != MainWindow.posID.Value)
                                            pos1 = (int)cb_pos1.SelectedValue;
                                        if ((int)cb_pos2.SelectedValue != MainWindow.posID.Value)
                                            pos2 = (int)cb_pos2.SelectedValue;
                                        Notification not = new Notification()
                                        {
                                            title = "trTransferAlertTilte",
                                            ncontent = "trTransferAlertContent",
                                            msgType = "alert",
                                            createUserId = MainWindow.userID.Value,
                                            updateUserId = MainWindow.userID.Value,
                                        };
                                        if (pos1 != 0)
                                            await not.save(not, (int)cb_pos1.SelectedValue, "accountsAlerts_transfers", cb_pos2.Text, 0, pos1);
                                        if (pos2 != 0)
                                            await not.save(not, (int)cb_pos2.SelectedValue, "accountsAlerts_transfers", cb_pos1.Text, 0, pos2);

                                        #endregion

                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                        Btn_clear_Click(null, null);

                                        await RefreshCashesList();
                                        Tb_search_TextChanged(null, null);
                                        await MainWindow.refreshBalance();
                                    }
                                    else
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                }
                                else if(s1.Equals(-3))
                                { Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn); }

                            }
                        }
                        #endregion
                    }
                    else //box is closed
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update"))
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        #region validate
                        //chk empty cash
                        SectionData.validateEmptyTextBox(tb_cash, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
                        //chk empty user
                        SectionData.validateEmptyComboBox(cb_pos1, p_errorPos1, tt_errorPos1, "trErrorEmptyFromPosToolTip");
                        //chk empty bank
                        SectionData.validateEmptyComboBox(cb_pos2, p_errorPos2, tt_errorPos2, "trErrorEmptyToPosToolTip");
                        //chk if 2 pos is the same
                        bool isSame = false;
                        if (cb_pos1.SelectedValue == cb_pos2.SelectedValue)
                            isSame = true;
                        if ((cb_pos1.SelectedIndex != -1) && (cb_pos2.SelectedIndex != -1) && (cb_pos1.SelectedValue == cb_pos2.SelectedValue))
                        {
                            SectionData.showComboBoxValidate(cb_pos1, p_errorPos1, tt_errorPos1, "trErrorSamePos");
                            SectionData.showComboBoxValidate(cb_pos2, p_errorPos2, tt_errorPos2, "trErrorSamePos");
                        }
                        #endregion

                        #region update
                        if ((!tb_cash.Text.Equals("")) && (!cb_pos1.Text.Equals("")) && (!cb_pos2.Text.Equals("")) && !isSame)
                        {
                            //first operation (pull)
                            cashtrans2.cash = decimal.Parse(tb_cash.Text);
                            cashtrans2.notes = tb_note.Text;
                            cashtrans2.posId = Convert.ToInt32(cb_pos1.SelectedValue);

                            decimal s1 = await cashModel.Save(cashtrans2);

                            if (!s1.Equals(0))
                            {
                                //second operation (deposit)
                                cashtrans3.cash = decimal.Parse(tb_cash.Text);
                                cashtrans3.posId = Convert.ToInt32(cb_pos2.SelectedValue);
                                cashtrans3.notes = tb_note.Text;

                                decimal s2 = await cashModel.Save(cashtrans3);

                                if (!s2.Equals(0))
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);

                                    await RefreshCashesList();
                                    Tb_search_TextChanged(null, null);
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }
                        #endregion
                    }
                    else //box is closed
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
                    }

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        if (cashtrans.cashTransId != 0)
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                decimal b = await cashModel.deletePosTrans(cashtrans.cashTransId);

                                if (b == 1)
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                                    //clear textBoxs
                                    Btn_clear_Click(sender, e);
                                }
                                else if (b == 0)
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopCanNotDeleteRequest"), animation: ToasterAnimation.FadeIn);
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                await RefreshCashesList();
                                Tb_search_TextChanged(null, null);
                            }
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
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_confirm_Click(object sender, RoutedEventArgs e)
        {//confirm
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                #region Accept
                Window.GetWindow(this).Opacity = 0.2;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxConfirm");
                w.ShowDialog();
                if (w.isOk)
                {
                    if (MainWindow.posLogIn.boxState == "o")
                    {
                        if (cashtrans.cashTransId != 0)
                        {

                            if (cashtrans.isConfirm2 == 0)
                                await confirmOpr();
                            else
                            {
                                //Pos pos = await posModel.getById(cashtrans.posId.Value);
                                //Pos pos2 = await posModel.getById(cashtrans.pos2Id.Value);
                                //int s1 = 0;
                                decimal res = 0;
                                if (cashtrans.transType == "d")
                                {
                                    res = await cashModel.MakeDeposit(cashtrans);

                                    #region old dina
                                    ////there is enough balance
                                    //if (pos.balance >= cashtrans.cash)
                                    //{
                                    //    pos.balance -= cashtrans.cash;
                                    //    int s = await posModel.save(pos);

                                    //    pos2.balance += cashtrans.cash;
                                    //    s1 = await posModel.save(pos2);
                                    //    if (!s1.Equals(0))//tras done so confirm
                                    //        await confirmOpr();
                                    //    else//error then do not confirm
                                    //        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                    //}
                                    ////there is not enough balance
                                    //else
                                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                                    #endregion
                                }
                                else
                                {
                                    res = await cashModel.MakePull(cashtrans);

                                    #region old dina
                                    ////there is enough balance
                                    //if (pos2.balance >= cashtrans.cash)
                                    //{
                                    //    pos2.balance -= cashtrans.cash;
                                    //    int s = await posModel.save(pos2);

                                    //    pos.balance += cashtrans.cash;
                                    //    s1 = await posModel.save(pos);
                                    //    if (!s1.Equals(0))//tras done so confirm
                                    //        await confirmOpr();
                                    //    else//error then do not confirm
                                    //        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                    //}

                                    ////there is not enough balance
                                    //else
                                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                                    #endregion
                                }


                                if (res >= 0)
                                {
                                    await successConfirm();
                                    AppSettings.PosBalance = res;
                                    MainWindow.setBalance();
                                }
                                else if (res.Equals(-2))
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trIsConfirmed"), animation: ToasterAnimation.FadeIn);
                                else if (res.Equals(-3))
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                //await MainWindow.refreshBalance();
                            }
                        }
                    }
                    else //box is closed
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
                    }
                }
                Window.GetWindow(this).Opacity = 1;
                #endregion
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async Task confirmOpr()
        {
            cashtrans.isConfirm = 1;
            decimal s = await cashModel.ConfirmCashTransfer(cashtrans);

            #region old dina
            //cashtrans.isConfirm = 1;
            //int s = await cashModel.Save(cashtrans);
            ////update date
            //cashtrans2temp = await cashModel.GetByID(cashtrans.cashTrans2Id);
            //int s2 = await cashModel.Save(cashtrans2temp);
            #endregion

            if (s>=0)
            {
                await RefreshCashesList();
                Tb_search_TextChanged(null, null);

                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopConfirm"), animation: ToasterAnimation.FadeIn);

                txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trIsConfirmed");
                btn_confirm.IsEnabled = false;
                btn_cancel.IsEnabled = false;
            }
            else if(s.Equals( -2))
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trIsConfirmed"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowError(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);


        }
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                Clear();

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async Task successConfirm()
        {
            await RefreshCashesList();
            Tb_search_TextChanged(null, null);

            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopConfirm"), animation: ToasterAnimation.FadeIn);

            txt_confirmButton.Text = MainWindow.resourcemanager.GetString("trIsConfirmed");
            btn_confirm.IsEnabled = false;
            btn_cancel.IsEnabled = false;
        }
        private void Clear()
        {
            txt_transNum.Text = "";
            tb_cash.Clear();
            cb_pos1.SelectedIndex = -1;
            cb_pos2.SelectedIndex = -1;
            cb_fromBranch.SelectedValue = MainWindow.branchID.Value;
            cb_toBranch.SelectedIndex = -1;
            tb_note.Clear();

            btn_add.IsEnabled = true;
            btn_update.IsEnabled = true;
            btn_delete.IsEnabled = false;
            btn_confirm.IsEnabled = false;
            btn_cancel.IsEnabled = false;

            SectionData.clearValidate(tb_cash, p_errorCash);
            SectionData.clearComboBoxValidate(cb_pos1, p_errorPos1);
            SectionData.clearComboBoxValidate(cb_pos2, p_errorPos2);
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//export
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
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
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshCashesList();
                    Tb_search_TextChanged(null, null);
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        async Task<IEnumerable<CashTransfer>> RefreshCashesList()
        {
          //  cashes = await cashModel.GetCashTransferForPosAsync("all", "p");
            cashes = await cashModel.GetCashTransferForPosById("all", "p",(int)MainWindow.posID);
            return cashes;

        }
        void RefreshCashView()
        {
            dg_posAccounts.ItemsSource = cashesQuery;
            txt_count.Text = cashesQuery.Count().ToString();
        }
        private void validateEmpty(string name, object sender)
        {
            if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_cash")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_pos1")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorPos1, tt_errorPos1, "trErrorEmptyFromPosToolTip");
                else if ((sender as ComboBox).Name == "cb_pos2")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorPos2, tt_errorPos2, "trErrorEmptyToPosToolTip");
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
        private void PreventSpaces(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private void Tb_cash_PreviewTextInput(object sender, TextCompositionEventArgs e)
        { //only decimal
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;
            else
                e.Handled = true;
        }

        void FN_ExportToExcel()
        {
            var QueryExcel = cashesQuery.AsEnumerable().Select(x => new
            {
                TransNum = x.transNum,
                PosFromName = x.posName,
                PosToName = x.pos2Name,
                OpperationType = x.transType,
                Cash = x.cash
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trFromPos");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trToPos");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trOpperationTypeToolTip");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trCashTooltip");

            ExportToExcel.Export(DTForExcel);
        }
        private async void Cb_state_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                await RefreshCashesList();
                Tb_search_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_pos1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//pos1selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                int bToId = Convert.ToInt32(cb_toBranch.SelectedValue);
                int pFromId = Convert.ToInt32(cb_pos1.SelectedValue);
                var toPos = poss.Where(p => p.branchId == bToId && p.posId != pFromId);
                cb_pos2.ItemsSource = toPos;
                cb_pos2.DisplayMemberPath = "name";
                cb_pos2.SelectedValuePath = "posId";
                cb_pos2.SelectedIndex = -1;

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_pos2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Cb_fromBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//fill pos1
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                int bFromId = Convert.ToInt32(cb_fromBranch.SelectedValue);
                var fromPos = poss.Where(p => p.branchId == bFromId);
                cb_pos1.ItemsSource = fromPos;
                cb_pos1.DisplayMemberPath = "name";
                cb_pos1.SelectedValuePath = "posId";
                cb_pos1.SelectedIndex = -1;

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_toBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { //fill pos combo2
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                int bToId = Convert.ToInt32(cb_toBranch.SelectedValue);
                int pFromId = Convert.ToInt32(cb_pos1.SelectedValue);
                var toPos = poss.Where(p => p.branchId == bToId && p.posId != pFromId);
                cb_pos2.ItemsSource = toPos;
                cb_pos2.DisplayMemberPath = "name";
                cb_pos2.SelectedValuePath = "posId";
                cb_pos2.SelectedIndex = -1;

                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_fromBranch")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorFromBranch, tt_errorFromBranch, "trEmptyBranchToolTip");
                    if ((sender as ComboBox).Name == "cb_toBranch")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorToBranch, tt_errorToBranch, "trEmptyBranchToolTip");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            string startDate = "";
            string endDate = "";
            string searchval = "";
            string Allchk = "";
            string state = "";
            //  List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\Account\Ar\ArPosAccReport.rdlc";
            }
            else
            { addpath = @"\Reports\Account\En\PosAccReport.rdlc"; }
            string title = MainWindow.resourcemanagerreport.GetString("trTransfers");
            if (chk_deposit.IsChecked == true)
            {
                title = title + "/" + MainWindow.resourcemanagerreport.GetString("trDeposits");

                paramarr.Add(new ReportParameter("trCol2Header", MainWindow.resourcemanagerreport.GetString("trDepositor")));
                paramarr.Add(new ReportParameter("trCol3Header", MainWindow.resourcemanagerreport.GetString("trRecepient")));
                state = MainWindow.resourcemanagerreport.GetString("trDeposits");
            }
            else if (chk_receive.IsChecked == true)
            {
                title = title + "/" + MainWindow.resourcemanagerreport.GetString("trReceives");
                paramarr.Add(new ReportParameter("trCol2Header", MainWindow.resourcemanagerreport.GetString("trRecepient")));
                paramarr.Add(new ReportParameter("trCol3Header", MainWindow.resourcemanagerreport.GetString("trDepositor")));
                state = MainWindow.resourcemanagerreport.GetString("trReceipts");
            }
            //filter
            startDate = dp_startSearchDate.SelectedDate != null ? SectionData.DateToString(dp_startSearchDate.SelectedDate) : "";
            endDate = dp_endSearchDate.SelectedDate != null ? SectionData.DateToString(dp_endSearchDate.SelectedDate) : "";
            Allchk = chb_all.IsChecked == true ? all : "";
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("alldateval", Allchk));
            paramarr.Add(new ReportParameter("stateval", state));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            paramarr.Add(new ReportParameter("trCash", MainWindow.resourcemanagerreport.GetString("trCash_")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            clsReports.posAccReport(cashesQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            paramarr.Add(new ReportParameter("trTitle", title));
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void Button_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
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
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    BuildReport();
                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
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
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    //cashesQueryExcel = cashesQuery.ToList();
                    win_lvc win = new win_lvc(cashesQuery, 8);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
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


        private async void search_Checking(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "chk_deposit")
                    {
                        chk_receive.IsChecked = false;
                    }
                    else if (cb.Name == "chk_receive")
                    {
                        chk_deposit.IsChecked = false;
                    }
                }
                SectionData.StartAwait(grid_ucposAccounts);

                translate();
                Clear();
                await RefreshCashesList();
                Tb_search_TextChanged(null, null);

                SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_ucposAccounts);
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
                    if (cb.Name == "chk_deposit")
                        chk_deposit.IsChecked = true;
                    else if (cb.Name == "chk_receive")
                        chk_receive.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {//cancel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucposAccounts);

                if (MainWindow.posLogIn.boxState == "o")
                {
                    if (cashtrans.cashTransId != 0)
                    {
                        int res =(int) await cashModel.canclePosTrans(cashtrans2.cashTransId, cashtrans3.cashTransId);

                        if(res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopCanceled"), animation: ToasterAnimation.FadeIn);
                            await RefreshCashesList();
                            Tb_search_TextChanged(null, null);
                            Btn_clear_Click(null, null);
                        }
                        else if(res.Equals(-2))
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trIsConfirmed"), animation: ToasterAnimation.FadeIn);

                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        #region old
                        //cashtrans2.isConfirm = 2;
                        //cashtrans3.isConfirm = 2;
                        //decimal s2 = await cashModel.Save(cashtrans2);
                        //decimal s3 = await cashModel.Save(cashtrans3);

                        //if ((!s2.Equals(0))&&(!s3.Equals(0)))
                        //{
                        //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopCanceled"), animation: ToasterAnimation.FadeIn);
                        //    await RefreshCashesList();
                        //    Tb_search_TextChanged(null, null);
                        //}
                        //else
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        #endregion
                    }
                }
                else //box is closed
                {
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBoxIsClosed"), animation: ToasterAnimation.FadeIn);
                }
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucposAccounts);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
