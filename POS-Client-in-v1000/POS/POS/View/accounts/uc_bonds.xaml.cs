using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.sectionData.Charts;
using POS.View.windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace POS.View.accounts
{
    /// <summary>
    /// Interaction logic for uc_bonds.xaml
    /// </summary>
    public partial class uc_bonds : UserControl
    {
        #region variables
        Card cardModel = new Card();
        IEnumerable<Card> cards;

        Bonds bondModel = new Bonds();
        IEnumerable<Bonds> bonds;
        Bonds bond = new Bonds();

        IEnumerable<Bonds> bondsQuery;
        IEnumerable<Bonds> bondsQueryExcel;
        string searchText = "";
        static private int _SelectedCard = -1;

        CashTransfer cashModel = new CashTransfer();
        IEnumerable<CashTransfer> cashbondsQuery;
        string createPermission = "bonds_create";
        string reportsPermission = "bonds_reports";
        private static uc_bonds _instance;
        byte tgl_bondState;

        List<Button> cardBtnList = new List<Button>();
        List<Ellipse> cardEllipseList = new List<Ellipse>();
        #endregion

        public static uc_bonds Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_bonds();
                return _instance;
            }
        }
        public uc_bonds()
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
                    SectionData.StartAwait(grid_ucBonds);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucBonds.FlowDirection = FlowDirection.LeftToRight;

                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucBonds.FlowDirection = FlowDirection.RightToLeft;

                }
                translate();
                #endregion

                await fillVendors();

                await fillCustomers();

                await fillUsers();

                cb_depositorV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorV, p_errordepositor);
                cb_depositorC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorC, p_errordepositor);
                cb_depositorU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorU, p_errordepositor);

                #region fill process type
                var typelist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trCash")       , Value = "cash" },
                new { Text = MainWindow.resourcemanager.GetString("trCheque")     , Value = "cheque" },
                new { Text = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods") , Value = "card" },
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
                    InitializeCardsPic(cards);
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                #endregion

                #region key up
                //item
                cb_depositorV.IsTextSearchEnabled = false;
                cb_depositorV.IsEditable = true;
                cb_depositorV.StaysOpenOnEdit = true;
                cb_depositorV.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_depositorV.Text = "";

                cb_depositorC.IsTextSearchEnabled = false;
                cb_depositorC.IsEditable = true;
                cb_depositorC.StaysOpenOnEdit = true;
                cb_depositorC.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_depositorC.Text = "";

                cb_depositorU.IsTextSearchEnabled = false;
                cb_depositorU.IsEditable = true;
                cb_depositorU.StaysOpenOnEdit = true;
                cb_depositorU.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_depositorU.Text = "";
                #endregion

                SectionData.defaultDatePickerStyle(dp_deservecDate);

                dp_startSearchDate.SelectedDate = DateTime.Now.Date;
                dp_endSearchDate.SelectedDate = DateTime.Now.Date;

                sDate = dp_startSearchDate.SelectedDate.Value;
                eDate = dp_endSearchDate.SelectedDate.Value;

                dp_startSearchDate.SelectedDateChanged += this.dp_SelectedStartDateChanged;
                dp_endSearchDate.SelectedDateChanged += this.dp_SelectedEndDateChanged;

                btn_pay.IsEnabled = false;
                btn_image.IsEnabled = false;
                btn_preview.IsEnabled = false;
                btn_pdf.IsEnabled = false;
                btn_printInvoice.IsEnabled = false;

                await RefreshBondsList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_pay_Click(object sender, RoutedEventArgs e)
        {//pay
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        #region validate
                        //chk empty payment type
                        SectionData.validateEmptyComboBox(cb_paymentProcessType, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");
                        //chk empty card 
                        if (gd_card.IsVisible)
                        {
                            SectionData.validateEmptyTextBlock(txt_card, p_errorCard, tt_errorCard, "trSelectCreditCard");
                            if(tb_processNum.IsVisible)
                                SectionData.showTextBoxValidate(tb_processNum, p_errorProcessNum, tt_errorProcessNum, "trEmptyProcessNumToolTip");
                        }
                        else
                        {
                            SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                            SectionData.clearValidate(tb_processNum, p_errorProcessNum);
                        }
                        //chk cheque num
                        if (tb_chequeProcessNum.IsVisible)
                        {
                            SectionData.showTextBoxValidate(tb_chequeProcessNum, p_chequeProcessNum, tt_chequeProcessNum, "trEmptyProcessNumToolTip");
                        }
                        else
                        {
                            SectionData.clearValidate(tb_chequeProcessNum, p_chequeProcessNum);
                        }
                        //chk empty payment type
                        SectionData.validateEmptyComboBox(cb_paymentProcessType, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");
                        //chk enough money
                        bool enoughMoney = true;
                        if ((!cb_paymentProcessType.Text.Equals("")) && (cb_paymentProcessType.SelectedValue.ToString().Equals("cash")) &&
                            (bond.type.Equals("p")) && (!chkEnoughBalance(decimal.Parse(tb_amount.Text))))
                        {
                            enoughMoney = false;
                            SectionData.showTextBoxValidate(tb_amount, p_errorAmount, tt_errorAmount, "trPopNotEnoughBalance");
                        }
                        #endregion

                        #region pay

                        if ((!cb_paymentProcessType.Text.Equals("")) &&

                            (((gd_card.IsVisible) && (!txt_card.Text.Equals("")) && (!tb_processNum.Text.Equals(""))|| !tb_processNum.IsVisible) || (!gd_card.IsVisible)) &&

                            ((!tb_processNum.Text.Equals("")) || (!tb_processNum.IsVisible)) &&

                            enoughMoney)
                        {
                            //update bond
                            bond.isRecieved = 1;

                            int s = (int)await bondModel.SaveWithBalanceCheck(bond, decimal.Parse(tb_amount.Text), MainWindow.posID.Value);

                            //if (s.Equals(-3))
                            //{
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                            //}
                            //else 
                            if (!s.Equals(0))
                            {
                                //save new cashtransfer
                                CashTransfer cash = new CashTransfer();

                                cash.transType = bond.type;
                                cash.posId = MainWindow.posID.Value;
                                cash.transNum = bond.number;
                                cash.cash = decimal.Parse(tb_amount.Text);
                                cash.notes = tb_note.Text;
                                cash.createUserId = MainWindow.userID;
                                cash.side = "bnd";
                                if (gd_card.IsVisible)
                                    cash.docNum = tb_processNum.Text;
                                else if (tb_chequeProcessNum.IsVisible)
                                    cash.docNum = tb_chequeProcessNum.Text;
                                cash.processType = cb_paymentProcessType.SelectedValue.ToString();

                                cash.bondId = bond.bondId;

                                if (cb_depositorV.IsVisible)
                                    cash.agentId = Convert.ToInt32(cb_depositorV.SelectedValue);

                                if (cb_depositorC.IsVisible)
                                    cash.agentId = Convert.ToInt32(cb_depositorC.SelectedValue);

                                if (cb_depositorU.IsVisible)
                                    cash.userId = Convert.ToInt32(cb_depositorU.SelectedValue);

                                if (cb_paymentProcessType.SelectedValue.ToString().Equals("card"))
                                    cash.cardId = _SelectedCard;

                                s = (int)await cashModel.SaveCashWithCommission(cash);

                                if (!s.Equals(0))
                                {
                                    if (cb_paymentProcessType.SelectedValue.ToString().Equals("cash"))
                                    {
                                        AppSettings.PosBalance = await calcBalance(bond.type, cash.cash.Value);
                                        MainWindow.setBalance();
                                    }

                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);

                                    Btn_clear_Click(null, null);

                                    await RefreshBondsList();
                                    Tb_search_TextChanged(null, null);
                                    await MainWindow.refreshBalance();

                                }
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region card
        void InitializeCardsPic(IEnumerable<Card> cards)
        {
            #region cardImageLoad
            dkp_cards.Children.Clear();
            int userCount = 0;
            foreach (var item in cards)
            {
                #region Button
                Button button = new Button();
                //button.DataContext = item.name;
                button.DataContext = item;
                button.Tag = item.cardId;
                button.Padding = new Thickness(0, 0, 0, 0);
                button.Margin = new Thickness(2.5, 5, 2.5, 5);
                button.Background = null;
                button.BorderBrush = null;
                button.Height = 35;
                button.Width = 35;
                button.Click += card_Click;
                //Grid.SetColumn(button, 4);
                #region grid
                Grid grid = new Grid();
                #region 
                Ellipse ellipse = new Ellipse();
                //ellipse.Margin = new Thickness(-5, 0, -5, 0);
                ellipse.StrokeThickness = 1;
                ellipse.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
                ellipse.Height = 35;
                ellipse.Width = 35;
                ellipse.FlowDirection = FlowDirection.LeftToRight;
                ellipse.ToolTip = item.name;
                ellipse.Tag = item.cardId;
                userImageLoad(ellipse, item.image);
                Grid.SetColumn(ellipse, userCount);
                grid.Children.Add(ellipse);
                cardEllipseList.Add(ellipse);
                #endregion
                #endregion
                button.Content = grid;
                #endregion
                dkp_cards.Children.Add(button);
                cardBtnList.Add(button);

            }
            #endregion

        }
        void card_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _SelectedCard = int.Parse(button.Tag.ToString());
            //txt_card.Text = button.DataContext.ToString();
            Card card = button.DataContext as Card;
            txt_card.Text = card.name;
            if (card.hasProcessNum.Value)
                tb_processNum.Visibility = Visibility.Visible;
            else
                tb_processNum.Visibility = Visibility.Collapsed;

            //set border color
            foreach (var el in cardEllipseList)
            {
                if ((int)el.Tag == (int)button.Tag)
                    el.Stroke = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                else
                    el.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
            }
        }
        ImageBrush brush = new ImageBrush();
        async void userImageLoad(Ellipse ellipse, string image)
        {
            try
            {
                if (!string.IsNullOrEmpty(image))
                {
                    clearImg(ellipse);

                    byte[] imageBuffer = await cardModel.downloadImage(image); // read this as BLOB from your DB
                    var bitmapImage = new BitmapImage();
                    using (var memoryStream = new System.IO.MemoryStream(imageBuffer))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                    }
                    ellipse.Fill = new ImageBrush(bitmapImage);
                }
                else
                {
                    clearImg(ellipse);
                }
            }
            catch
            {
                clearImg(ellipse);
            }
        }
        private void clearImg(Ellipse ellipse)
        {
            Uri resourceUri = new Uri("pic/no-image-icon-90x90.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            brush.ImageSource = temp;
            ellipse.Fill = brush;
        }
        #endregion

        #region methods
        private async Task<decimal> calcBalance(string _type, decimal ammount)
        {//balance for pos

            if (_type.Equals("p"))
                ammount = ammount * -1;


            Pos pos = await posModel.EditBalance(MainWindow.posID.Value, ammount);


            return (decimal)pos.balance;

        }
        //private async Task calcBalance(string _type, decimal ammount)
        //{//balance for pos
        //    int s = 0;
        //    //SC Commerce balance
        //    Pos pos = await posModel.getById(MainWindow.posID.Value);
        //    if (_type.Equals("p"))
        //        pos.balance -= ammount;
        //    else
        //        pos.balance += ammount;

        //    s = await pos.save(pos);
        //}
        private bool chkEnoughBalance(decimal ammount)
        {
            if (AppSettings.PosBalance >= ammount)
            { return true; }
            else { return false; }

        }
        //private async Task<bool> chkEnoughBalance(decimal ammount)
        //{
        //    Pos pos = await posModel.getById(MainWindow.posID.Value);
        //    if (pos.balance.Value >= ammount)
        //    { return true; }
        //    else { return false; }

        //}
        void FN_ExportToExcel()
        {
            var QueryExcel = bondsQueryExcel.AsEnumerable().Select(x => new
            {
                TransNum = x.number,
                Ammount = x.amount,
                Recipient = x.type,
                Notes = x.notes
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trCash");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trRecepient");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trNote");

            ExportToExcel.Export(DTForExcel);


        }
        private void translate()
        {
            txt_bonds.Text = MainWindow.resourcemanager.GetString("trBonds");
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            txt_isRecieved.Text = MainWindow.resourcemanager.GetString("trReceivedToggle");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_number, MainWindow.resourcemanager.GetString("trDocNumTooltip"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_depositorV, MainWindow.resourcemanager.GetString("trDepositorHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_depositorC, MainWindow.resourcemanager.GetString("trDepositorHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_depositorU, MainWindow.resourcemanager.GetString("trDepositorHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentProcessType, MainWindow.resourcemanager.GetString("trPaymentTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_amount, MainWindow.resourcemanager.GetString("trCashHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_deservecDate, MainWindow.resourcemanager.GetString("trDocDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_processNum, MainWindow.resourcemanager.GetString("trProcessNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_chequeProcessNum, MainWindow.resourcemanager.GetString("trProcessNumHint"));

            chb_all.Content = MainWindow.resourcemanager.GetString("trAll");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startSearchDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endSearchDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            dg_bonds.Columns[0].Header = MainWindow.resourcemanager.GetString("trDocNumTooltip");
            dg_bonds.Columns[1].Header = MainWindow.resourcemanager.GetString("trRecipientTooltip");
            dg_bonds.Columns[2].Header = MainWindow.resourcemanager.GetString("trPaymentTypeTooltip");
            dg_bonds.Columns[3].Header = MainWindow.resourcemanager.GetString("trDeservedDate");
            dg_bonds.Columns[4].Header = MainWindow.resourcemanager.GetString("trPayDate");
            dg_bonds.Columns[5].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

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

            btn_pay.Content = MainWindow.resourcemanager.GetString("trPay");
            txt_imageButton.Text = MainWindow.resourcemanager.GetString("trImage");
            txt_previewButton.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_printInvoiceButton.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_pdfButton.Text = MainWindow.resourcemanager.GetString("trPdfBtn");
        }
        void RefreshBondView()
        {
            dg_bonds.ItemsSource = bondsQuery;
            txt_count.Text = bondsQuery.Count().ToString();
        }
        async Task<IEnumerable<Bonds>> RefreshBondsList()
        {
            bonds = await bondModel.GetAll();
            return bonds;
        }
        private async Task fillVendors()
        {
            try
            {
                await FillCombo.FillComboVendors(cb_depositorV);
                //agents = await agentModel.GetAgentsActive("v");
                //cb_depositorV.ItemsSource = agents;
                //cb_depositorV.DisplayMemberPath = "name";
                //cb_depositorV.SelectedValuePath = "agentId";
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async Task fillCustomers()
        {
            try
            {
                await FillCombo.FillComboCustomers(cb_depositorC);
                //agents = await agentModel.GetAgentsActive("c");
                //cb_depositorC.ItemsSource = agents;
                //cb_depositorC.DisplayMemberPath = "name";
                //cb_depositorC.SelectedValuePath = "agentId";
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async Task fillUsers()
        {
            try
            {
                await FillCombo.FillComboUsers(cb_depositorU);
                //users = await userModel.GetUsersActive();
                //cb_depositorU.ItemsSource = users;
                //cb_depositorU.DisplayMemberPath = "username";
                //cb_depositorU.SelectedValuePath = "userId";
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        #endregion

        #region events
        CashTransfer ca = new CashTransfer();
        private async void Dg_bonds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_ucBonds);

                #region clear validate
                SectionData.clearValidate(tb_number, p_errorNumber);
                SectionData.clearValidate(tb_amount, p_errorAmount);
                SectionData.clearValidate(tb_chequeProcessNum, p_chequeProcessNum);
                SectionData.clearValidate(tb_depositor, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_depositorV, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_depositorC, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_depositorU, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_paymentProcessType, p_errorpaymentProcessType);
                SectionData.clearValidate(tb_processNum, p_errorProcessNum);
                SectionData.clearValidate(tb_chequeProcessNum, p_errorProcessNum);
                SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                TextBox tbDocDate = (TextBox)dp_deservecDate.Template.FindName("PART_TextBox", dp_deservecDate);
                SectionData.clearValidate(tbDocDate, p_errorDocDate);
                #endregion

                if (dg_bonds.SelectedIndex != -1)
                {
                    bond = dg_bonds.SelectedItem as Bonds;
                    this.DataContext = bond;

                    if (bond != null)
                    {
                        btn_image.IsEnabled = true;
                        btn_preview.IsEnabled = true;
                        btn_pdf.IsEnabled = true;
                        btn_printInvoice.IsEnabled = true;

                        tb_number.Text = bond.number;

                        tb_amount.Text = SectionData.DecTostring(bond.amount);

                        dp_deservecDate.Text = SectionData.DateToString(bond.deserveDate);

                        if (bond.isRecieved == 1)
                        {
                            btn_pay.Content = MainWindow.resourcemanager.GetString("trPaid");
                            btn_pay.IsEnabled = false;
                        }
                        else
                        {
                            btn_pay.Content = MainWindow.resourcemanager.GetString("trPay");
                            btn_pay.IsEnabled = true;
                        }

                        if (bond.ctside.Equals("v"))
                        {
                            cb_depositorV.SelectedValue = bond.ctagentId.Value;
                            cb_depositorV.Visibility = Visibility.Visible;
                            tb_depositor.Visibility = Visibility.Collapsed; SectionData.clearValidate(tb_depositor, p_errordepositor);
                            cb_depositorC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorC, p_errordepositor);
                            cb_depositorU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorU, p_errordepositor);
                        }
                        else if (bond.ctside.Equals("c"))
                        {
                            cb_depositorC.Visibility = Visibility.Visible;
                            cb_depositorC.SelectedValue = bond.ctagentId.Value;
                            tb_depositor.Visibility = Visibility.Collapsed; SectionData.clearValidate(tb_depositor, p_errordepositor);
                            cb_depositorV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorV, p_errordepositor);
                            cb_depositorU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorU, p_errordepositor);
                        }
                        else if (bond.ctside.Equals("u"))
                        {
                            cb_depositorU.SelectedValue = bond.ctuserId.Value;
                            cb_depositorU.Visibility = Visibility.Visible;
                            tb_depositor.Visibility = Visibility.Collapsed; SectionData.clearValidate(tb_depositor, p_errordepositor);
                            cb_depositorV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorV, p_errordepositor);
                            cb_depositorC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorC, p_errordepositor);
                        }
                        else
                        {
                            tb_depositor.Visibility = Visibility.Visible;
                            cb_depositorV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorV, p_errordepositor);
                            cb_depositorC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorC, p_errordepositor);
                            cb_depositorU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_depositorU, p_errordepositor);

                            switch (bond.ctside)
                            {
                                case "s": tb_depositor.Text = MainWindow.resourcemanager.GetString("trSalary"); break;
                                case "e": tb_depositor.Text = MainWindow.resourcemanager.GetString("trGeneralExpenses"); break;
                                case "m":
                                    if (bond.cttransType == "p")
                                        tb_depositor.Text = MainWindow.resourcemanager.GetString("trAdministrativePull");
                                    else if (bond.cttransType == "d")
                                        tb_depositor.Text = MainWindow.resourcemanager.GetString("trAdministrativeDeposit");
                                    break;
                            }

                        }
                        if (bond.isRecieved == 1)
                        {
                            //cashbondsQuery = await cashModel.GetCashTransferAsync("all", "bnd");
                            cashbondsQuery = await cashModel.GetCashBond("all", "bnd");

                            ca = cashbondsQuery.Where(c => c.bondId == bond.bondId).FirstOrDefault();
                            cb_paymentProcessType.SelectedValue = ca.processType;
                            if (cb_paymentProcessType.SelectedValue.ToString().Equals("card"))
                            {
                                _SelectedCard = (int)ca.cardId;
                                Card card = new Card();
                                card = await card.getById(_SelectedCard);
                                txt_card.Text = card.name;
                                tb_processNum.Text = ca.docNum;

                                Button btn = cardBtnList.Where(c => (int)c.Tag == _SelectedCard).FirstOrDefault();
                                card_Click(btn, null);
                            }
                            else if (cb_paymentProcessType.SelectedValue.ToString().Equals("cheque"))
                            {
                                tb_chequeProcessNum.Text = ca.docNum;
                            }
                            else
                            {
                                _SelectedCard = -1;
                                tb_processNum.Clear();
                                tb_chequeProcessNum.Clear();
                                gd_card.Visibility = Visibility.Collapsed;
                                tb_chequeProcessNum.Visibility = Visibility.Collapsed;
                                p_chequeProcessNum.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            cb_paymentProcessType.SelectedIndex = -1;
                            _SelectedCard = -1;
                            tb_processNum.Clear();
                            tb_chequeProcessNum.Clear();
                            gd_card.Visibility = Visibility.Collapsed;
                            tb_chequeProcessNum.Visibility = Visibility.Collapsed;
                        }

                    }

                }
                //if (sender != null)
                //    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        Pos posModel = new Pos();
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);
                //try
                //{
                if (bonds is null)
                    await RefreshBondsList();
                if (chb_all.IsChecked == false)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        searchText = tb_search.Text.ToLower();

                        bondsQuery = bonds.Where(s => (
                        s.number.ToLower().Contains(searchText)
                        ||
                        s.amount.ToString().ToLower().Contains(searchText)
                        || s.type.ToString().ToLower().Contains(searchText)
                        )
                        );
                        bondsQuery = bondsQuery.ToList().Where(s =>
                        sDate != null ? s.updateDate.Value.Date >= sDate : true
                       );
                        bondsQuery = bondsQuery.ToList().Where(s =>
                        eDate != null ? s.updateDate.Value.Date <= eDate : true
                      );
                        bondsQuery = bondsQuery.ToList().Where(s =>
                     s.isRecieved == tgl_bondState
                     );
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        searchText = tb_search.Text.ToLower();
                        bondsQuery = bonds.Where(s => (
                        s.number.ToLower().Contains(searchText)
                        ||
                        s.amount.ToString().ToLower().Contains(searchText)
                        || s.type.ToString().ToLower().Contains(searchText)
                        )
                        && s.isRecieved == tgl_bondState
                        );
                    });
                }

                bondsQueryExcel = bondsQuery.ToList();
                RefreshBondView();
                //}
                //catch { }
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tgl_isRecieved_Unchecked(object sender, RoutedEventArgs e)
        {//unreceived
            if (bonds is null)
                await RefreshBondsList();
            tgl_bondState = 0;
            Tb_search_TextChanged(null, null);
        }
        private async void Tgl_isRecieved_Checked(object sender, RoutedEventArgs e)
        {//received
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

                if (bonds is null)
                    await RefreshBondsList();
                tgl_bondState = 1;
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

                await RefreshBondsList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
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
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                ca = new CashTransfer();
                tb_number.Text = "";
                tb_amount.Clear();
                tb_note.Clear();
                btn_image.IsEnabled = false;
                btn_preview.IsEnabled = false;
                btn_pdf.IsEnabled = false;
                btn_printInvoice.IsEnabled = false;

                tb_chequeProcessNum.Clear();
                tb_processNum.Clear();
                gd_card.Visibility = Visibility.Collapsed;
                p_chequeProcessNum.Visibility = Visibility.Collapsed;

                cb_depositorV.SelectedIndex = -1;
                cb_depositorC.SelectedIndex = -1;
                cb_depositorU.SelectedIndex = -1;
                cb_paymentProcessType.SelectedIndex = -1;
                _SelectedCard = -1;
                dp_deservecDate.SelectedDate = null;

                cb_depositorV.Visibility = Visibility.Collapsed;
                cb_depositorC.Visibility = Visibility.Collapsed;
                cb_depositorU.Visibility = Visibility.Collapsed;

                SectionData.clearValidate(tb_number, p_errorNumber);
                SectionData.clearValidate(tb_amount, p_errorAmount);

                SectionData.clearComboBoxValidate(cb_depositorV, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_depositorC, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_depositorU, p_errordepositor);
                SectionData.clearComboBoxValidate(cb_paymentProcessType, p_errorpaymentProcessType);
                SectionData.clearValidate(tb_processNum, p_errorProcessNum);
                SectionData.clearValidate(tb_chequeProcessNum, p_chequeProcessNum);
                SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                TextBox tbDocDate = (TextBox)dp_deservecDate.Template.FindName("PART_TextBox", dp_deservecDate);
                SectionData.clearValidate(tbDocDate, p_errorDocDate);

                txt_card.Text = "";
                tb_processNum.Visibility = Visibility.Collapsed;
                //set border color
                foreach (var el in cardEllipseList)
                {
                    el.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
                }
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
                if ((sender as TextBox).Name == "tb_processNum")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorProcessNum, tt_errorProcessNum, "trEmptyProcessNumToolTip");
                else if ((sender as TextBox).Name == "tb_chequeProcessNum")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_chequeProcessNum, tt_chequeProcessNum, "trEmptyProcessNumToolTip");
            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_paymentProcessType")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");
                else if ((sender as ComboBox).Name == "cb_card")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCard, tt_errorCard, "trEmptyCardTooltip");
            }
        }
        private void PreventSpaces(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private async void dp_SelectedEndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_ucBonds);

                if (dp_endSearchDate.SelectedDate.Value != null)
                    eDate = dp_endSearchDate.SelectedDate.Value;

                await RefreshBondsList();
                Tb_search_TextChanged(null, null);

                //if (sender != null)
                //    SectionData.EndAwait(grid_ucBonds);
            }
            catch //(Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_ucBonds);
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        DateTime sDate, eDate;
        private async void dp_SelectedStartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_ucBonds);

                if (dp_startSearchDate.SelectedDate.Value != null)
                    sDate = dp_startSearchDate.SelectedDate.Value;

                await RefreshBondsList();
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch //(Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_ucBonds);
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        private void Cb_paymentProcessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//type selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

                TextBox dpDate = (TextBox)dp_deservecDate.Template.FindName("PART_TextBox", dp_deservecDate);

                switch (cb_paymentProcessType.SelectedIndex)
                {

                    case 0://cash
                        gd_card.Visibility = Visibility.Collapsed;
                        tb_chequeProcessNum.Visibility = Visibility.Collapsed;
                        _SelectedCard = -1;
                        SectionData.clearValidate(tb_processNum, p_errorProcessNum);
                        SectionData.clearValidate(tb_chequeProcessNum, p_chequeProcessNum);
                        SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                        break;

                    case 1://cheque
                        tb_chequeProcessNum.Visibility = Visibility.Visible;
                        gd_card.Visibility = Visibility.Collapsed;
                        _SelectedCard = -1;
                        SectionData.clearValidate(tb_processNum, p_errorProcessNum);
                        SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                        break;

                    case 2://card
                        gd_card.Visibility = Visibility.Visible;
                        tb_chequeProcessNum.Visibility = Visibility.Collapsed;
                        txt_card.Text = "";
                        tb_processNum.Text = "";
                        if (ca.cardId != null)
                        {
                            Button btn = cardBtnList.Where(c => (int)c.Tag == ca.cardId.Value).FirstOrDefault();
                            card_Click(btn, null);
                        }
                        SectionData.clearValidate(tb_chequeProcessNum, p_chequeProcessNum);
                        break;
                }

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
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
        private void Tb_EnglishDigit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only english and digits
            Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
            if (!regex.IsMatch(e.Text))
                e.Handled = true;
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
        private void Cb_depositorV_KeyUp(object sender, KeyEventArgs e)
        {
            //var combo = sender as ComboBox;
            //var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                //tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            //combo.ItemsSource = FillCombo.vendorsList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_depositorC_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.customersList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_depositorU_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.usersList.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }

        #endregion

        #region reports
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
                addpath = @"\Reports\Account\Ar\ArBondAccReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Account\EN\BondAccReport.rdlc";
            }
            //filter
            startDate = dp_startSearchDate.SelectedDate != null ? SectionData.DateToString(dp_startSearchDate.SelectedDate) : "";
            endDate = dp_endSearchDate.SelectedDate != null ? SectionData.DateToString(dp_endSearchDate.SelectedDate) : "";
            state= tgl_isRecieved.IsChecked == true ?  MainWindow.resourcemanagerreport.GetString("trReceivedToggle")
                : MainWindow.resourcemanagerreport.GetString("NotRecieved");        
            Allchk = chb_all.IsChecked == true ? all : "";
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
            paramarr.Add(new ReportParameter("alldateval", Allchk));
            paramarr.Add(new ReportParameter("stateval", state));
            paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            foreach (var r in bondsQueryExcel)
            {
                r.amount = decimal.Parse(SectionData.DecTostring(r.amount));
                r.deserveDate = Convert.ToDateTime(SectionData.DateToString(r.deserveDate));
            }
            clsReports.bondsReport(bondsQueryExcel, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    //Thread t1 = new Thread(() =>
                    //{
                    List<ReportParameter> paramarr = new List<ReportParameter>();
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
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_image_Click(object sender, RoutedEventArgs e)
        {//image
            try
            {
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (bonds != null || bond.bondId != 0)
                    {
                        wd_uploadImage w = new wd_uploadImage();

                        w.tableName = "bondes";
                        w.tableId = bond.bondId;
                        w.docNum = bond.number;
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
        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (bond.isRecieved == 1)
                    {
                        buildbondDocReport();
                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBondNotRecieved"), animation: ToasterAnimation.FadeIn);
                    }
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public async void getBondData(List<ReportParameter> paramarr)
        {
            if (bond != null)
            {
                string pay = "";
                string name = "";
                string processType = "";
                string cardname = "";
                string caprocesstype = "";

                if (bond.isRecieved == 1)
                {
                    pay = MainWindow.resourcemanagerreport.GetString("trPaid");
                }
                else
                {
                    pay = MainWindow.resourcemanagerreport.GetString("trPay");
                }

                if (bond.ctside.Equals("v") || bond.ctside.Equals("c"))
                {
                    // int gid =(int) bond.ctagentId;
                    name = bond.ctagentName;
                    // name = agents.Where(x => x.agentId == gid).FirstOrDefault().name;
                }

                else if (bond.ctside.Equals("u"))
                {
                    // name = users.Where(x => x.userId == bond.ctuserId).FirstOrDefault().name+ " " + users.Where(x => x.userId == bond.ctuserId).FirstOrDefault().lastname;
                    name = bond.ctusersName + " " + bond.ctusersLName;
                }
                else
                {
                    switch (bond.ctside)
                    {
                        case "s": name = MainWindow.resourcemanager.GetString("trSalary"); break;
                        case "e": name = MainWindow.resourcemanager.GetString("trGeneralExpenses"); break;
                        case "m":
                            if (bond.cttransType == "p")
                                name = MainWindow.resourcemanager.GetString("trAdministrativePull");
                            else if (bond.cttransType == "d")
                                name = MainWindow.resourcemanager.GetString("trAdministrativeDeposit");
                            break;
                    }
                }

                if (bond.isRecieved == 1)
                {
                    CashTransfer ca = new CashTransfer();
                    ca = cashbondsQuery.Where(c => c.bondId == bond.bondId).FirstOrDefault();
                    caprocesstype = ca.processType;
                    switch (ca.processType)
                    {
                        case "cash": processType = MainWindow.resourcemanagerreport.GetString("trCash"); break;
                        case "cheque": processType = MainWindow.resourcemanagerreport.GetString("trCheque"); break;
                        case "card": processType = MainWindow.resourcemanagerreport.GetString("trAnotherPaymentMethods"); break;
                    }

                    if (ca.processType == "card")
                    {
                        cardname = cards.Where(c => c.cardId == ca.cardId).FirstOrDefault().name;
                    }

                }
                string title = "";

                if (bond.type == "p")
                    title = MainWindow.resourcemanagerreport.GetString("trPayVocher");
                else
                    title = MainWindow.resourcemanagerreport.GetString("trReceiptVoucher");

                if (FillCombo.usersList is null)
                    await FillCombo.RefreshUsers();
                string updateusername = FillCombo.usersList.Where(x => x.userId == bond.updateUserId).FirstOrDefault().name + " " + FillCombo.usersList.Where(x => x.userId == bond.updateUserId).FirstOrDefault().lastname;

                paramarr.Add(new ReportParameter("title", title));

                paramarr.Add(new ReportParameter("bondNumber", bond.number));
                paramarr.Add(new ReportParameter("amount", SectionData.DecTostring(bond.amount)));//ok
                paramarr.Add(new ReportParameter("deserveDate", SectionData.DateToString(bond.deserveDate)));
                paramarr.Add(new ReportParameter("isRecieved", bond.isRecieved.ToString()));
                paramarr.Add(new ReportParameter("trPay", pay));
                paramarr.Add(new ReportParameter("ctside", bond.ctside));
                paramarr.Add(new ReportParameter("sideName", name));
                paramarr.Add(new ReportParameter("trProcessType", processType));
                paramarr.Add(new ReportParameter("cardName", cardname));
                paramarr.Add(new ReportParameter("transType", bond.type));
                paramarr.Add(new ReportParameter("type", caprocesstype));
                paramarr.Add(new ReportParameter("user_name", updateusername));
                paramarr.Add(new ReportParameter("date", reportclass.DateToString(bond.updateDate)));
                paramarr.Add(new ReportParameter("currency", AppSettings.Currency));
                paramarr.Add(new ReportParameter("amount_in_words", reportclass.ConvertAmountToWords(bond.amount)));
                paramarr.Add(new ReportParameter("job", "Employee"));

                paramarr.Add(new ReportParameter("trcashAmount", MainWindow.resourcemanagerreport.GetString("cashAmount")));
                paramarr.Add(new ReportParameter("trBondno", MainWindow.resourcemanagerreport.GetString("Bondno")));
                paramarr.Add(new ReportParameter("trDate", MainWindow.resourcemanagerreport.GetString("trDate")));
                paramarr.Add(new ReportParameter("trRecivedFromMr", MainWindow.resourcemanagerreport.GetString("RecivedFromMr")));
                paramarr.Add(new ReportParameter("trPaytoMr", MainWindow.resourcemanagerreport.GetString("PaytoMr")));
                paramarr.Add(new ReportParameter("trAmountInWords", MainWindow.resourcemanagerreport.GetString("AmountInWords")));
                paramarr.Add(new ReportParameter("trRecivedPurpose", MainWindow.resourcemanagerreport.GetString("RecivedPurpose")));
                paramarr.Add(new ReportParameter("trPaymentPurpose", MainWindow.resourcemanagerreport.GetString("PaymentPurpose")));
                paramarr.Add(new ReportParameter("trReceiver", MainWindow.resourcemanagerreport.GetString("Receiver")));

            }
        }
        public void buildbondDocReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            bool isArabic = ReportCls.checkLang();
            // bond.type
            if (isArabic)
            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\Ar\ArBondDocA4.rdlc";
                }
                else//A5
                {
                    addpath = @"\Reports\Account\Ar\ArBondDoc.rdlc";
                }
            }
            else

            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\EN\BondDocA4.rdlc";
                }
                else//A5
                {
                    addpath = @"\Reports\Account\EN\BondDoc.rdlc";
                }

            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            getBondData(paramarr);
            clsReports.bondsDocReport(rep, reppath, paramarr);
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
                    SectionData.StartAwait(grid_ucBonds);

                BuildReport();
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void Btn_preview1_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);
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
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvc win = new win_lvc(bondsQuery, 6);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {

                    #region
                    if (bond.isRecieved == 1)
                    {
                        buildbondDocReport();
                        saveFileDialog.Filter = "PDF|*.pdf;";
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            LocalReportExtensions.ExportToPDF(rep, filepath);
                        }
                    }
                    else
                    {
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBondNotRecieved"), animation: ToasterAnimation.FadeIn);
                    }
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    if (bond.isRecieved == 1)
                    {
                        Window.GetWindow(this).Opacity = 0.2;
                        string pdfpath = "";
                        //
                        pdfpath = @"\Thumb\report\temp.pdf";
                        pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        buildbondDocReport();
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
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBondNotRecieved"), animation: ToasterAnimation.FadeIn);
                    }

                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucBonds);

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
                    SectionData.EndAwait(grid_ucBonds);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucBonds);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion



    }
}
