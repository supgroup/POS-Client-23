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
    /// Interaction logic for uc_banksAccounts.xaml
    /// </summary>
    public partial class uc_banksAccounts : UserControl
    {
        #region variables
        CashTransfer cashtrans = new CashTransfer();
        CashTransfer cashModel = new CashTransfer();
        Pos posModel = new Pos();
        IEnumerable<CashTransfer> cashesQueryExcel;
        IEnumerable<CashTransfer> cashesQuery;
        IEnumerable<CashTransfer> cashes;
        string searchText = "";

        User userModel = new User();
        IEnumerable<User> users;

        Branch branchModel = new Branch();
        //IEnumerable<Branch> branches;

        Bank bankModel = new Bank();
        IEnumerable<Bank> banksQueryExcel;
        IEnumerable<Bank> banksQuery;
        IEnumerable<Bank> banks;

        BrushConverter bc = new BrushConverter();

        wd_acceptUser w = new wd_acceptUser();
        string createPermission = "banksAccounting_create";
        string reportsPermission = "banksAccounting_reports";
        private static uc_banksAccounts _instance;
        #endregion

        public static uc_banksAccounts Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_banksAccounts();
                return _instance;
            }
        }
        public uc_banksAccounts()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
                btn_add.IsEnabled = true;

                #region Style Date
                /////////////////////////////////////////////////////////////
                SectionData.defaultDatePickerStyle(dp_startSearchDate);
                SectionData.defaultDatePickerStyle(dp_endSearchDate);
                /////////////////////////////////////////////////////////////
                #endregion

                dp_endSearchDate.SelectedDate = DateTime.Now;
                dp_startSearchDate.SelectedDate = DateTime.Now;

                #region fill operation type
                var dislist = new[] {
            new { Text = MainWindow.resourcemanager.GetString("trDeposit"), Value = "d" },
            new { Text = MainWindow.resourcemanager.GetString("trPull"), Value = "p" },
             };

                cb_opperationType.DisplayMemberPath = "Text";
                cb_opperationType.SelectedValuePath = "Value";
                cb_opperationType.ItemsSource = dislist;
                #endregion

                #region fill users combo
                try
                {
                    await FillCombo.FillComboUsers(cb_user);
                    //users = await userModel.GetUsersActive();
                    //cb_user.ItemsSource = users;
                    //cb_user.DisplayMemberPath = "username";
                    //cb_user.SelectedValuePath = "userId";
                    //cb_user.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region fill banks combo
                try
                {
                    await FillCombo.FillComboBanks(cb_bank);
                    //banks = await bankModel.Get();
                    //banksQuery = banks.Where(s => s.isActive == 1);
                    //cb_bank.ItemsSource = banksQuery;
                    //cb_bank.DisplayMemberPath = "name";
                    //cb_bank.SelectedValuePath = "bankId";
                    //cb_bank.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region translate

                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucBankAccounts.FlowDirection = FlowDirection.LeftToRight;

                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucBankAccounts.FlowDirection = FlowDirection.RightToLeft;

                }

                translate();
                #endregion

                #region key up
                //item
                cb_user.IsTextSearchEnabled = false;
                cb_user.IsEditable = true;
                cb_user.StaysOpenOnEdit = true;
                cb_user.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_user.Text = "";

                cb_bank.IsTextSearchEnabled = false;
                cb_bank.IsEditable = true;
                cb_bank.StaysOpenOnEdit = true;
                cb_bank.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_bank.Text = "";
                #endregion

                #region prevent editting on date and time
                //TextBox tbStartDate = (TextBox)dp_startSearchDate.Template.FindName("PART_TextBox", dp_startSearchDate);
                //tbStartDate.IsReadOnly = true;
                //TextBox tbEndDate = (TextBox)dp_endSearchDate.Template.FindName("PART_TextBox", dp_endSearchDate);
                //tbEndDate.IsReadOnly = true;
                #endregion

                dp_startSearchDate.SelectedDateChanged += this.dp_SelectedStartDateChanged;
                dp_endSearchDate.SelectedDateChanged += this.dp_SelectedEndDateChanged;

                btn_image.IsEnabled = false;
                btn_preview.IsEnabled = false;
                btn_pdf.IsEnabled = false;
                btn_printInvoice.IsEnabled = false;

                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        private void translate()
        {
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trTransaferDetails");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_bankAccounts.Text = MainWindow.resourcemanager.GetString("trBankAccounts");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCashHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_depositNumber, MainWindow.resourcemanager.GetString("trDepositeNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_opperationType, MainWindow.resourcemanager.GetString("trOpperationTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUserHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_bank, MainWindow.resourcemanager.GetString("trBankHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            try
            {
                if (cb_opperationType.SelectedValue.ToString() == "d")
                    btn_add.Content = MainWindow.resourcemanager.GetString("trDeposit");
                else if (cb_opperationType.SelectedValue.ToString() == "p")
                    btn_add.Content = MainWindow.resourcemanager.GetString("trPull");
            }
            catch { btn_add.Content = MainWindow.resourcemanager.GetString("trSave"); }

            chb_all.Content = MainWindow.resourcemanager.GetString("trAll");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startSearchDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endSearchDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            dg_bankAccounts.Columns[0].Header = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            dg_bankAccounts.Columns[1].Header = MainWindow.resourcemanager.GetString("trBank");
            dg_bankAccounts.Columns[2].Header = MainWindow.resourcemanager.GetString("trDepositeNumTooltip");
            dg_bankAccounts.Columns[3].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_bankAccounts.Columns[4].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");

            tt_confirmUser.Content = MainWindow.resourcemanager.GetString("trConfirmUserTooltip");
            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

            txt_imageButton.Text = MainWindow.resourcemanager.GetString("trImage");
            txt_previewButton.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_printInvoiceButton.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_pdfButton.Text = MainWindow.resourcemanager.GetString("trPdfBtn");
        }
        //private async Task calcBalance(decimal ammount)
        //{
        //    Pos pos = await posModel.getById(MainWindow.posID.Value);

        //    pos.balance += ammount;

        //    int s = await posModel.save(pos);

        //}
        private async Task<decimal> calcBalance(decimal ammount)
        {
            Pos pos = await posModel.EditBalance(MainWindow.posID.Value, ammount);


            return (decimal)pos.balance;

        }
        void FN_ExportToExcel()
        {
            var QueryExcel = cashesQuery.AsEnumerable().Select(x => new
            {
                Tranfer_Number = x.transNum,
                Bank = x.bankId,
                DipRecNum = x.docNum,
                Cash = x.cash
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trTransferNum");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trBank");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trDepositeReceiptNum");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trCash");

            ExportToExcel.Export(DTForExcel);
        }
        async Task<IEnumerable<CashTransfer>> RefreshCashesList()
        {
            cashes = await cashModel.GetCashTransferAsync("all", "bn");
            return cashes;
        }
        void RefreshCashView()
        {
            dg_bankAccounts.ItemsSource = cashesQuery;
            txt_count.Text = cashesQuery.Count().ToString();
        }
        #endregion

        #region events
        private async void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                cashtrans.cashTransId = 0;
                try
                {
                    tb_transNum.Text = await SectionData.generateNumber(Convert.ToChar(cb_opperationType.SelectedValue), "bn");
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                btn_add.IsEnabled = true;
                cb_opperationType.IsEnabled = true;
                cb_user.IsEnabled = true;
                cb_bank.IsEnabled = true;
                tb_cash.IsEnabled = true;
                tb_depositNumber.IsEnabled = false;
                tb_note.IsEnabled = true;
                btn_image.IsEnabled = false;
                btn_preview.IsEnabled = false;
                btn_pdf.IsEnabled = false;
                btn_printInvoice.IsEnabled = false;

                tb_cash.Clear();
                tb_depositNumber.Clear();
                cb_opperationType.SelectedIndex = -1;
                cb_bank.ItemsSource = FillCombo.banksList;
                cb_bank.SelectedIndex = -1;
                cb_user.ItemsSource = FillCombo.usersList;
                cb_user.SelectedIndex = -1;
                tb_note.Clear();

                SectionData.clearValidate(tb_cash, p_errorCash);
                SectionData.clearValidate(tb_depositNumber, p_errorDepositNumber);
                SectionData.clearComboBoxValidate(cb_opperationType, p_errorOpperationType);
                SectionData.clearComboBoxValidate(cb_user, p_errorUser);
                SectionData.clearComboBoxValidate(cb_bank, p_errorBank);
                p_confirmUser.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E65B65"));

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
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
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_image_Click(object sender, RoutedEventArgs e)
        {//image
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (cashtrans != null || cashtrans.cashTransId != 0)
                    {
                        wd_uploadImage w = new wd_uploadImage();

                        w.tableName = "cashTransfer";
                        w.tableId = cashtrans.cashTransId;
                        w.docNum = cashtrans.docNum;
                        w.ShowDialog();
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                await RefreshCashesList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
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
        private void Tb_depositNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only int
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }
        private void validateEmpty(string name, object sender)
        {
            if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_cash")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
                else if ((sender as TextBox).Name == "tb_depositNumber")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorDepositNumber, tt_errorDepositNumber, "trEmptyDepositeNumberToolTip");
            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_opperationType")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorOpperationType, tt_errorOpperationType, "trErrorEmptyOpperationTypeToolTip");
                else if ((sender as ComboBox).Name == "cb_user")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorUser, tt_errorUser, "trErrorEmptyUserToolTip");
                else if ((sender as ComboBox).Name == "cb_bank")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorBank, tt_errorBank, "trErrorEmptyBankToolTip");
            }
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
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
        private void Tb_validateEmptyTextChangeCash(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_cash")
                    SectionData.InputJustNumber(ref txb);
                Cb_user_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Tb_validateEmptyLostFocusCash(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                Cb_user_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void OnlyInt(object sender, TextCompositionEventArgs e)
        {
            //only int
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void Cb_opperationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_opperationType.SelectedValue.ToString() == "d")
                    btn_add.Content = MainWindow.resourcemanager.GetString("trDeposit");
                else if (cb_opperationType.SelectedValue.ToString() == "p")
                    btn_add.Content = MainWindow.resourcemanager.GetString("trPull");
                else
                    btn_add.Content = MainWindow.resourcemanager.GetString("trSave");
            }
            catch //(Exception ex)
            {
                //   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                btn_add.Content = MainWindow.resourcemanager.GetString("trSave");
            }
        }
        private void Cb_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select user
            try
            {
                if ((cb_user.SelectedIndex != -1) && (!tb_cash.Text.Equals("")))
                    btn_confirmUser.IsEnabled = true;
                else
                    btn_confirmUser.IsEnabled = false;
            }
            catch (Exception ex)
            {
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
        private void Chb_all_Checked(object sender, RoutedEventArgs e)
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
        private void Cb_user_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.usersList.Where(p => p.fullName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_bank_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.banksList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private async void dp_SelectedStartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                await RefreshCashesList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void dp_SelectedEndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                await RefreshCashesList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_bankAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);
                SectionData.clearValidate(tb_cash, p_errorCash);
                SectionData.clearValidate(tb_depositNumber, p_errorDepositNumber);
                SectionData.clearComboBoxValidate(cb_opperationType, p_errorOpperationType);
                SectionData.clearComboBoxValidate(cb_user, p_errorUser);
                SectionData.clearComboBoxValidate(cb_bank, p_errorBank);
                if (dg_bankAccounts.SelectedIndex != -1)
                {
                    cashtrans = dg_bankAccounts.SelectedItem as CashTransfer;
                    this.DataContext = cashtrans;

                    if (cashtrans != null)
                    {
                        btn_image.IsEnabled = true;
                        btn_preview.IsEnabled = true;
                        btn_pdf.IsEnabled = true;
                        btn_printInvoice.IsEnabled = true;

                        tb_cash.Text = SectionData.DecTostring(cashtrans.cash);

                        cb_opperationType.SelectedValue = cashtrans.transType;
                        cb_user.SelectedValue = cashtrans.userId;
                        cb_bank.SelectedValue = cashtrans.bankId;
                        p_confirmUser.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2BB673"));
                        if (string.IsNullOrEmpty(cashtrans.docNum))
                        {
                            cb_opperationType.IsEnabled = false;
                            cb_user.IsEnabled = false;
                            cb_bank.IsEnabled = false;
                            tb_cash.IsEnabled = false;
                            tb_depositNumber.IsEnabled = true;
                            tb_note.IsEnabled = false;
                            btn_add.IsEnabled = true;
                            btn_add.Content = MainWindow.resourcemanager.GetString("trCompletion");
                        }
                        else
                        {
                            cb_opperationType.IsEnabled = false;
                            cb_user.IsEnabled = false;
                            cb_bank.IsEnabled = false;
                            tb_cash.IsEnabled = false;
                            tb_depositNumber.IsEnabled = false;
                            tb_note.IsEnabled = false;
                            btn_add.IsEnabled = false;
                            btn_add.Content = MainWindow.resourcemanager.GetString("trCompleted");
                        }

                    }
                }
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                //try
                //{
                if (cashes is null)
                    await RefreshCashesList();

                if (chb_all.IsChecked == false)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        searchText = tb_search.Text.ToLower();
                        cashesQuery = cashes.Where(s => (s.transNum.ToLower().Contains(searchText)
                        || s.cash.ToString().ToLower().Contains(searchText)
                        || s.bankName.ToLower().Contains(searchText)
                        || s.docNum.Contains(searchText)
                        )
                        && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                        && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                        );
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        searchText = tb_search.Text.ToLower();
                        cashesQuery = cashes.Where(s => (s.transNum.ToLower().Contains(searchText)
                        || s.cash.ToString().ToLower().Contains(searchText)
                        || s.bankName.ToLower().Contains(searchText)
                        || s.docNum.Contains(searchText)
                        )
                        );
                    });
                }
                RefreshCashView();
                cashesQueryExcel = cashesQuery.ToList();
                //}
                //catch { }

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        private async void Btn_confirmUser_Click(object sender, RoutedEventArgs e)
        {//confirm user
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                Window.GetWindow(this).Opacity = 0.2;

                w.userID = Convert.ToInt32(cb_user.SelectedValue);
                User user = await userModel.getUserById(w.userID);
                w.tb_userName.Text = user.username;
                w.user = user;

                w.ShowDialog();

                Window.GetWindow(this).Opacity = 1;

                if (w.isOk == true)
                    p_confirmUser.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2BB673"));
                else p_confirmUser.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E65B65"));

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_confirm_Click(object sender, RoutedEventArgs e)
        {//confirm

        }
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        if (cashtrans.cashTransId == 0)
                        {
                            #region validate
                            //chk empty cash
                            SectionData.validateEmptyTextBox(tb_cash, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
                            //chk empty dicount type
                            SectionData.validateEmptyComboBox(cb_opperationType, p_errorOpperationType, tt_errorOpperationType, "trErrorEmptyOpperationTypeToolTip");
                            //chk empty user
                            SectionData.validateEmptyComboBox(cb_user, p_errorUser, tt_errorUser, "trErrorEmptyUserToolTip");
                            //chk empty bank
                            SectionData.validateEmptyComboBox(cb_bank, p_errorBank, tt_errorBank, "trErrorEmptyBankToolTip");
                            //chk user confirmation
                            bool isuserConfirmed = w.isOk;
                            //cash <= 0
                            decimal amount = 0;
                            try
                            {
                                amount = decimal.Parse(tb_cash.Text);
                            }
                            catch (Exception ex)
                            {
                                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                            }

                            if (amount <= 0)
                                SectionData.showTextBoxValidate(tb_cash, p_errorCash, tt_errorCash, "itMustBeGreaterThanZero");
                            #endregion

                            #region add
                            if ((!tb_cash.Text.Equals("")) && (amount > 0) &&
                                (!cb_opperationType.Text.Equals("")) && (!cb_user.Text.Equals("")) &&
                                (!cb_bank.Text.Equals("")) &&
                                (isuserConfirmed)
                                )

                            {
                                CashTransfer cash = new CashTransfer();

                                cash.transType = cb_opperationType.SelectedValue.ToString();
                                cash.userId = Convert.ToInt32(cb_user.SelectedValue);
                                try
                                {
                                    cash.transNum = await cashModel.generateCashNumber(cb_opperationType.SelectedValue.ToString() + "bn");
                                }
                                catch (Exception ex)
                                {
                                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                                }
                                cash.cash = decimal.Parse(tb_cash.Text);
                                cash.createUserId = MainWindow.userID.Value;
                                cash.notes = tb_note.Text;
                                cash.posId = MainWindow.posID.Value;
                                cash.side = "bn";
                                cash.isConfirm = 0;
                                cash.bankId = Convert.ToInt32(cb_bank.SelectedValue);

                                int s = (int)await cashModel.Save(cash);

                                if (!s.Equals(0))
                                {

                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                    Btn_clear_Click(null, null);
                                    w.isOk = false;

                                    dg_bankAccounts.ItemsSource = await RefreshCashesList();
                                    Tb_search_TextChanged(null, null);
                                    await MainWindow.refreshBalance();
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }
                        else
                        {
                            if (cashtrans.isConfirm == 0)
                            {
                                //chk empty deposite number
                                SectionData.validateEmptyTextBox(tb_depositNumber, p_errorDepositNumber, tt_errorDepositNumber, "trEmptyDepositNumberToolTip");
                                if (!tb_depositNumber.Text.Equals(""))
                                {
                                    cashtrans.isConfirm = 1;
                                    cashtrans.docNum = tb_depositNumber.Text;

                                    int s = (int)await cashModel.confirmBankTransfer(cashtrans);
                                    if (s >= 0)
                                    {
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCompleted"), animation: ToasterAnimation.FadeIn);
                                        btn_add.IsEnabled = false;
                                        btn_add.Content = MainWindow.resourcemanager.GetString("trCompleted");

                                        dg_bankAccounts.ItemsSource = await RefreshCashesList();
                                        Tb_search_TextChanged(null, null);

                                        AppSettings.PosBalance = s;
                                        MainWindow.setBalance();

                                    }
                                    else if (s.Equals(-3))
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                                    else
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                    #region old
                                    //int s = (int)await cashModel.Save(cashtrans);

                                    //if (!s.Equals(0))
                                    //{
                                    //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCompleted"), animation: ToasterAnimation.FadeIn);
                                    //    btn_add.IsEnabled = false;
                                    //    btn_add.Content = MainWindow.resourcemanager.GetString("trCompleted");

                                    //    dg_bankAccounts.ItemsSource = await RefreshCashesList();
                                    //    Tb_search_TextChanged(null, null);

                                    //    decimal ammount = cashtrans.cash.Value;
                                    //    if (cashtrans.transType.Equals("d")) ammount *= -1;
                                    //    AppSettings.PosBalance = await calcBalance(ammount);
                                    //    MainWindow.setBalance();
                                    //    //await MainWindow.refreshBalance();
                                    //}
                                    //else
                                    //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                                    #endregion
                                }
                            }
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
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region reports
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            string startDate = "";
            string endDate = "";
            string searchval = "";
            string Allchk = "";
            //  List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");

            if (isArabic)
            {
                addpath = @"\Reports\Account\Ar\ArBankAccReport.rdlc";
            }
            else addpath = @"\Reports\Account\EN\BankAccReport.rdlc";
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            foreach (var r in cashesQuery)
            {
                r.cash = decimal.Parse(SectionData.DecTostring(r.cash));
            }
            //filter
            startDate = dp_startSearchDate.SelectedDate != null ? SectionData.DateToString(dp_startSearchDate.SelectedDate) : "";
            endDate = dp_endSearchDate.SelectedDate != null ? SectionData.DateToString(dp_endSearchDate.SelectedDate) : "";
            Allchk = chb_all.IsChecked == true ? all : "";
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("alldateval", Allchk));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            clsReports.bankAccReport(cashesQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            clsReports.Header(paramarr);
            clsReports.bankdg(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

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

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);
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
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_preview1_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);
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

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvc win = new win_lvc(banksQuery, 2);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }
        public void getBankData(List<ReportParameter> paramarr)
        {
            string pay = "";
            string processType = "";
            //   string cardname = "";
            // string caprocesstype = "";
            string title = "";
            string userName = "";
            string bankName = "";
            string trNum = "";
            string trOperation = "";
            ////////////////////////////////
            ///
            title = MainWindow.resourcemanagerreport.GetString("trVoucher");
            if (cashtrans.transType == "p")
            {
                trOperation = MainWindow.resourcemanagerreport.GetString("trReceiptOperation");
                trNum = MainWindow.resourcemanagerreport.GetString("trRecieptNum");               
                paramarr.Add(new ReportParameter("trRecieptFromBank", MainWindow.resourcemanagerreport.GetString("RecieptFromBank")));
            }
            else
            {
                // title = MainWindow.resourcemanagerreport.GetString("trReceiptVoucher");
                trOperation = MainWindow.resourcemanagerreport.GetString("trDeposit");
                trNum = MainWindow.resourcemanagerreport.GetString("trDepositeNum");
                paramarr.Add(new ReportParameter("trRecieptFromBank", MainWindow.resourcemanagerreport.GetString("DeposittoBank")));
            }
            userName = cashtrans.usersName + " " + cashtrans.usersLName;
            bankName = cashtrans.bankName;
            ////////////////////////////////////
            processType = MainWindow.resourcemanagerreport.GetString("trCash");
            paramarr.Add(new ReportParameter("title", title));
            paramarr.Add(new ReportParameter("docnum", cashtrans.docNum));
            paramarr.Add(new ReportParameter("bondNumber", tb_transNum.Text));
            //  paramarr.Add(new ReportParameter("deserveDate", SectionData.DateToString(bond.deserveDate)));

            //  paramarr.Add(new ReportParameter("isRecieved", bond.isRecieved.ToString()));
            paramarr.Add(new ReportParameter("trPay", pay));             
            paramarr.Add(new ReportParameter("sideName", bankName));
            paramarr.Add(new ReportParameter("trProcessType", processType));       
            paramarr.Add(new ReportParameter("transType", cashtrans.transType));                                                                                
            paramarr.Add(new ReportParameter("user_name", userName)); 
            paramarr.Add(new ReportParameter("date", reportclass.DateToString(cashtrans.updateDate))); 
            paramarr.Add(new ReportParameter("currency", AppSettings.Currency)); 
            paramarr.Add(new ReportParameter("amount_in_words", reportclass.ConvertAmountToWords(cashtrans.cash)));//ok
            paramarr.Add(new ReportParameter("job", "Employee"));
            paramarr.Add(new ReportParameter("trNum", trNum)); 
            paramarr.Add(new ReportParameter("amount", SectionData.DecTostring(cashtrans.cash)));
            paramarr.Add(new ReportParameter("trOperation", trOperation)); 
            paramarr.Add(new ReportParameter("trcashAmount", MainWindow.resourcemanagerreport.GetString("cashAmount")));
            paramarr.Add(new ReportParameter("trVoucherno", MainWindow.resourcemanagerreport.GetString("Voucherno")));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));

            paramarr.Add(new ReportParameter("trAmountInWords", MainWindow.resourcemanagerreport.GetString("AmountInWords")));
            paramarr.Add(new ReportParameter("trRecivedPurpose", MainWindow.resourcemanagerreport.GetString("RecivedPurpose")));
            paramarr.Add(new ReportParameter("trPaymentPurpose", MainWindow.resourcemanagerreport.GetString("PaymentPurpose")));
            paramarr.Add(new ReportParameter("trManager", MainWindow.resourcemanagerreport.GetString("Manager")));

        }
        public void buildBankDocReport()
        {

            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            bool isArabic = ReportCls.checkLang();
            // bond.type
            if (isArabic)
            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\Ar\ArBankDocA4.rdlc";
                }
                else//A5
                {
                    addpath = @"\Reports\Account\Ar\ArBankDoc.rdlc";
                }
            }
            else

            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\EN\BankDocA4.rdlc";
                }
                else//A5
                {
                    addpath = @"\Reports\Account\EN\BankDoc.rdlc";
                }

            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            getBankData(paramarr);
            clsReports.bondsDocReport(rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);
            rep.Refresh();

        }
        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {// doc
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region

                    if (cashtrans != null)
                    {
                        buildBankDocReport();
                        if (MainWindow.docPapersize == "A4")
                        {
                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                        }
                        else //A5
                        {
                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, 2);

                        }
                        //  LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                    }
                    else
                    {
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBondNotReceived"), animation: ToasterAnimation.FadeIn);
                    }


                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//doc
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (!string.IsNullOrEmpty(cashtrans.docNum) && cashtrans.userId != null)
                    {
                        buildBankDocReport();
                        saveFileDialog.Filter = "PDF|*.pdf;";
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            LocalReportExtensions.ExportToPDF(rep, filepath);
                        }
                    }
                    else
                    {
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trProcessNotConfirmed"), animation: ToasterAnimation.FadeIn);
                    }

                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//doc
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBankAccounts);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region

                    if (!string.IsNullOrEmpty(cashtrans.docNum) && cashtrans.userId != null)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        string pdfpath = "";
                        //
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        buildBankDocReport();
                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                        wd_previewPdf w = new wd_previewPdf();
                        w.pdfPath = pdfpath;
                        if (!string.IsNullOrEmpty(w.pdfPath))
                        {
                            w.ShowDialog();
                            w.wb_pdfWebViewer.Dispose();
                        }
                        Window.GetWindow(this).Opacity = 1;
                    }
                    else
                    {
                        Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trProcessNotConfirmed"), animation: ToasterAnimation.FadeIn);
                    }

                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBankAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion



    }
}
