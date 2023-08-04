using netoaster;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
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
using System.IO;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.View.sectionData.Charts;
using System.Windows.Resources;

namespace POS.View.accounts
{
    /// <summary>
    /// Interaction logic for uc_paymentsAccounts.xaml
    /// </summary>
    public partial class uc_paymentsAccounts : UserControl
    {
        #region variables
        User userModel = new User();
        ShippingCompanies shCompanyModel = new ShippingCompanies();
        Card cardModel = new Card();
        Bonds bondModel = new Bonds();
        Pos posModel = new Pos();
        CashTransfer cashModel = new CashTransfer();
        CashTransfer cashtrans = new CashTransfer();

        IEnumerable<ShippingCompanies> shCompanies;
        IEnumerable<Card> cards;
        IEnumerable<CashTransfer> cashesQuery;
        IEnumerable<CashTransfer> cashesQueryExcel;

        IEnumerable<CashTransfer> cashes;
        static private int _SelectedCard = -1;

        string searchText = "";
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        public List<Invoice> invoicesLst = new List<Invoice>();
        public List<CashTransfer> cashesLst = new List<CashTransfer>();

        List<Button> cardBtnList = new List<Button>();
        List<Ellipse> cardEllipseList = new List<Ellipse>();
        bool hasProcessNum = false;

        string createPermission = "payments_create";
        string reportsPermission = "payments_reports";
        #endregion

        private static uc_paymentsAccounts _instance;
        public static uc_paymentsAccounts Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_paymentsAccounts();
                return _instance;
            }
        }
        public uc_paymentsAccounts()
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
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucPaymentsAccounts.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucPaymentsAccounts.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                #region Style Date
                /////////////////////////////////////////////////////////////
                dp_startSearchDate.Loaded += delegate
                {

                    var textBox1 = (TextBox)dp_startSearchDate.Template.FindName("PART_TextBox", dp_startSearchDate);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_startSearchDate.Background;
                        textBox1.BorderThickness = dp_startSearchDate.BorderThickness;
                    }
                };
                /////////////////////////////////////////////////////////////
                dp_endSearchDate.Loaded += delegate
                {

                    var textBox1 = (TextBox)dp_endSearchDate.Template.FindName("PART_TextBox", dp_endSearchDate);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_endSearchDate.Background;
                        textBox1.BorderThickness = dp_endSearchDate.BorderThickness;
                    }
                };
                /////////////////////////////////////////////////////////////
                #endregion

                dp_endSearchDate.SelectedDate = DateTime.Now;
                dp_startSearchDate.SelectedDate = DateTime.Now;
                btn_invoices.IsEnabled = false;

                #region fill deposit to combo
                var depositlist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trVendor")     , Value = "v" },
                new { Text = MainWindow.resourcemanager.GetString("trCustomer")   , Value = "c" },
                new { Text = MainWindow.resourcemanager.GetString("trUser")       , Value = "u" },
                new { Text = MainWindow.resourcemanager.GetString("trSalary")     , Value = "s" },
                new { Text = MainWindow.resourcemanager.GetString("trGeneralExpenses")     , Value = "e" },
                new { Text = MainWindow.resourcemanager.GetString("trAdministrativePull")  , Value = "m" },
                new { Text = MainWindow.resourcemanager.GetString("trShippingCompanies")   , Value = "sh" },
                new { Text = MainWindow.resourcemanager.GetString("trTaxCollection")       , Value = "tax" }
                 };
                cb_depositTo.DisplayMemberPath = "Text";
                cb_depositTo.SelectedValuePath = "Value";
                cb_depositTo.ItemsSource = depositlist;
                #endregion

                await fillVendors();

                await fillCustomers();

                await fillUsers();

                await fillShippingCompanies();

                #region fill process type
                var typelist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trCash")       , Value = "cash" },
                new { Text = MainWindow.resourcemanager.GetString("trDocument")   , Value = "doc" },
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
                cb_recipientC.IsTextSearchEnabled = false;
                cb_recipientC.IsEditable = true;
                cb_recipientC.StaysOpenOnEdit = true;
                cb_recipientC.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_recipientC.Text = "";

                cb_recipientV.IsTextSearchEnabled = false;
                cb_recipientV.IsEditable = true;
                cb_recipientV.StaysOpenOnEdit = true;
                cb_recipientV.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_recipientV.Text = "";

                cb_recipientU.IsTextSearchEnabled = false;
                cb_recipientU.IsEditable = true;
                cb_recipientU.StaysOpenOnEdit = true;
                cb_recipientU.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_recipientU.Text = "";

                cb_recipientSh.IsTextSearchEnabled = false;
                cb_recipientSh.IsEditable = true;
                cb_recipientSh.StaysOpenOnEdit = true;
                cb_recipientSh.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_recipientSh.Text = "";
                #endregion

                dp_startSearchDate.SelectedDateChanged += this.dp_SelectedStartDateChanged;
                dp_endSearchDate.SelectedDateChanged += this.dp_SelectedEndDateChanged;

                btn_image.IsEnabled = false;
                btn_preview.IsEnabled = false;
                btn_pdf.IsEnabled = false;
                btn_printInvoice.IsEnabled = false;

                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
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
                button.DataContext = item;
                button.Tag = item.cardId;
                button.Padding = new Thickness(0, 0, 0, 0);
                button.Margin = new Thickness(2.5, 5, 2.5, 5);
                button.Background = null;
                button.BorderBrush = null;
                button.Height = 35;
                button.Width = 35;
                button.Click += card_Click;
                
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
                cardImageLoad(ellipse, item.image,(DateTime)item.updateDate);
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
            SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
            SectionData.clearTextBlockValidate(txt_card, p_errorCard);
            var button = sender as Button;
            _SelectedCard = int.Parse(button.Tag.ToString());

            Card card = button.DataContext as Card;

            txt_card.Text = card.name;

            if (card.hasProcessNum.Value)
            {
                tb_docNumCard.Visibility = Visibility.Visible;
                hasProcessNum = true;
            }
            else
            {
                tb_docNumCard.Visibility = Visibility.Collapsed;
                hasProcessNum = false;
            }
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
        async void cardImageLoad(Ellipse ellipse, string image, DateTime updateDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(image))
                {
                    // clearImg(ellipse);
                    bool isModified = SectionData.chkImgChng(image, updateDate, Global.TMPCardsFolder);
                    if (!isModified)
                        SectionData.ellipsLocalImg("Card", image, ellipse);
                    else
                    {
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

        int s = 0;
        // string s1 = "false";
        int s1 = 0;
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);
                s = 0; s1 = 0;
                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (MainWindow.posLogIn.boxState == "o") // box is open
                    {
                        #region validate
                        //chk empty cash
                        SectionData.validateEmptyTextBox(tb_cash, p_errorCash, tt_errorCash, "trEmptyCashToolTip");

                        //chk empty doc date
                        TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);

                        if (grid_document.IsVisible)
                        {
                            SectionData.validateEmptyTextBox(dpDate, p_errorDocDate, tt_errorDocDate, "trEmptyDocDateToolTip");
                        }
                        else
                        {

                        }
                        //chk empty doc num
                        if (grid_cheque.IsVisible)
                        {
                            SectionData.validateEmptyTextBox(tb_docNumCheque, p_errorDocNumCheque, tt_errorDocNumCheque, "trEmptyDocNumToolTip");
                        }
                        else
                        {
                            SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
                        }
                       
                        //chk empty deposit to
                        SectionData.validateEmptyComboBox(cb_depositTo, p_errorDepositTo, tt_errorDepositTo, "trErrorEmptyDepositToToolTip");

                        //chk empty recipient
                        if (cb_recipientV.IsVisible)
                            SectionData.validateEmptyComboBox(cb_recipientV, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                        else
                            SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);

                        if (cb_recipientC.IsVisible)
                            SectionData.validateEmptyComboBox(cb_recipientC, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                        else
                            SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);

                        if (cb_recipientU.IsVisible)
                            SectionData.validateEmptyComboBox(cb_recipientU, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                        else
                            SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);

                        if (cb_recipientSh.IsVisible)
                            SectionData.validateEmptyComboBox(cb_recipientSh, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                        else
                            SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);

                        if (tb_recipientText.IsVisible)
                            SectionData.validateEmptyTextBox(tb_recipientText, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                        else
                            SectionData.clearValidate(tb_recipientText, p_errorRecipient);

                        //chk empty payment type
                        SectionData.validateEmptyComboBox(cb_paymentProcessType, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");

                        //chk enough money
                        if ((!tb_cash.Text.Equals("")) && (! chkEnoughBalance(decimal.Parse(tb_cash.Text))))
                            SectionData.showTextBoxValidate(tb_cash, p_errorCash, tt_errorCash, "trPopNotEnoughBalance");

                        //chk empty card 
                        if (gd_card.IsVisible)
                        {
                            if (txt_card.Text.Equals(""))
                                SectionData.validateEmptyTextBlock(txt_card, p_errorCard, tt_errorCard, "trSelectCreditCard");
                            else
                            {
                                SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                                if (hasProcessNum)
                                    SectionData.validateEmptyTextBox(tb_docNumCard, p_errorDocCard, tt_errorDocCard, "trEmptyDocNumToolTip");
                                else
                                    SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                            }
                        }
                       
                        //chk enough money
                        bool enoughMoney = true;
                        if ((!cb_paymentProcessType.Text.Equals("")) && !tb_cash.Text.Equals("") && (cb_paymentProcessType.SelectedValue.ToString().Equals("cash")) &&
                            (! chkEnoughBalance(decimal.Parse(tb_cash.Text))))
                        {
                            enoughMoney = false;
                            SectionData.showTextBoxValidate(tb_cash, p_errorCash, tt_errorCash, "trPopNotEnoughBalance");
                        }
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

                        #region save

                        if ((!tb_cash.Text.Equals("")) && (amount > 0) && (!cb_depositTo.Text.Equals("")) && (!cb_paymentProcessType.Text.Equals("")) &&
                       (((cb_recipientV.IsVisible) && (!cb_recipientV.Text.Equals(""))) || (!cb_recipientV.IsVisible)) &&
                       (((cb_recipientC.IsVisible) && (!cb_recipientC.Text.Equals(""))) || (!cb_recipientC.IsVisible)) &&
                       (((cb_recipientU.IsVisible) && (!cb_recipientU.Text.Equals(""))) || (!cb_recipientU.IsVisible)) &&
                       (((cb_recipientSh.IsVisible) && (!cb_recipientSh.Text.Equals(""))) || (!cb_recipientSh.IsVisible)) &&
                       (((tb_recipientText.IsVisible) && (!tb_recipientText.Text.Equals(""))) || (!tb_recipientText.IsVisible)) &&
                       (((tb_docNumCheque.IsVisible) && (!tb_docNumCheque.Text.Equals(""))) || (!tb_docNumCheque.IsVisible)) &&
                       (((dp_docDate.IsVisible) && (!dp_docDate.Text.Equals(""))) || (!dp_docDate.IsVisible)) &&
                       (((gd_card.IsVisible) && (!txt_card.Text.Equals("")) && ((!tb_docNumCard.Text.Equals("")) || (!tb_docNumCard.IsVisible))) || (!gd_card.IsVisible)) &&
                        enoughMoney
                       )
                        {
                        string recipient = cb_depositTo.SelectedValue.ToString();
                        int agentid = 0;

                        CashTransfer cash = new CashTransfer();

                        cash.transType = "p";
                        cash.posId = MainWindow.posID.Value;
                        cash.transNum = cash.transType + cb_depositTo.SelectedValue.ToString();
                        cash.cash = decimal.Parse(tb_cash.Text);
                        cash.purpose = tb_purpose.Text;
                        cash.notes = tb_note.Text;
                        cash.createUserId = MainWindow.userID;
                        cash.side = cb_depositTo.SelectedValue.ToString();
                        cash.processType = cb_paymentProcessType.SelectedValue.ToString();

                        if (cb_recipientV.IsVisible)
                        { cash.agentId = Convert.ToInt32(cb_recipientV.SelectedValue); agentid = Convert.ToInt32(cb_recipientV.SelectedValue); }

                        if (cb_recipientC.IsVisible)
                        { cash.agentId = Convert.ToInt32(cb_recipientC.SelectedValue); agentid = Convert.ToInt32(cb_recipientC.SelectedValue); }

                        if (cb_recipientU.IsVisible)
                            cash.userId = Convert.ToInt32(cb_recipientU.SelectedValue);

                        if (cb_recipientSh.IsVisible)
                            cash.shippingCompanyId = Convert.ToInt32(cb_recipientSh.SelectedValue);

                        if (tb_recipientText.IsVisible)
                            cash.otherSide = tb_recipientText.Text;

                        if (cb_paymentProcessType.SelectedValue.ToString().Equals("card"))
                        {
                            cash.cardId = _SelectedCard;
                            cash.docNum = tb_docNumCard.Text;
                        }

                        if (cb_paymentProcessType.SelectedValue.ToString().Equals("doc"))
                            cash.docNum = await cashModel.generateDocNumber("pbnd");

                        if (cb_paymentProcessType.SelectedValue.ToString().Equals("cheque"))
                            cash.docNum = tb_docNumCheque.Text;
 
                        if (cb_paymentProcessType.SelectedValue.ToString().Equals("doc"))
                        {
                            int res = (int)await saveBond(cash.docNum, cash.cash.Value, dp_docDate.SelectedDate.Value, "p");
                            //if(res.Equals(-3))
                            //{
                            //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                            //    return;
                            //}
                            //else
                            cash.bondId = res;
                        }
                            if (cb_recipientV.IsVisible || cb_recipientC.IsVisible)
                            {
                                if (tb_cash.IsReadOnly)
                                    s1 = (int)await cashModel.PayListOfInvoices(cash.agentId.Value, invoicesLst, "pay", cash);
                                else
                                    s1 = (int)await cashModel.PayByAmmount(cash.agentId.Value, decimal.Parse(tb_cash.Text), "pay", cash);
                            }
                            else if (cb_recipientU.IsVisible && cash.side != "s")
                            {
                                if (tb_cash.IsReadOnly)
                                    s1 = (int)await cashModel.payListCommissionCashes(cashesLst, cash);
                                else
                                    s1 = (int)await cashModel.payCommissionCashesByAmount(cash.userId.Value, cash.cash.Value, cash);
                            }
                            else if (cb_recipientSh.IsVisible)
                            {
                                if (tb_cash.IsReadOnly)
                                {
                                    cash.side = cb_depositTo.SelectedValue.ToString() + "d";
                                    s1 = (int)await cashModel.payDeliveryCostOfInvoices(cash.shippingCompanyId.Value, cashesLst, cash);
                                }
                                else
                                    s = (int)await cashModel.payDeliveryCostByAmount(cash.shippingCompanyId.Value, cash.cash.Value, cash);
                            }
                            else
                                s = (int)await cashModel.Save(cash);
                               

                        if ((!s.Equals("0")) || (!s1.Equals("")) || (s1.Equals("-1")))
                        {
                            if (cb_paymentProcessType.SelectedValue.ToString().Equals("cash"))
                               AppSettings.PosBalance =  await calcBalance(cash.cash.Value, recipient, agentid);

                            //if ((cb_recipientU.IsVisible) && (cash.side == "u"))
                               // await userModel.editUserBalance(cash.cash.Value, cash.userId.Value);
                               // await calcUserBalance(Convert.ToSingle(cash.cash.Value), cash.userId.Value);

                           // if (cb_recipientSh.IsVisible && !tb_cash.IsReadOnly)
                              //  await shCompanyModel.editShippingComBalance(cash.cash.Value, cash.shippingCompanyId.Value);
                               // await calcShippingComBalance(cash.cash.Value, cash.shippingCompanyId.Value);

                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            Btn_clear_Click(null, null);

                            await RefreshCashesList();
                            await Search();
                            await MainWindow.refreshBalance();
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
                SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
                catch (Exception ex)
                {
                    if (sender != null)
                        SectionData.EndAwait(grid_ucPaymentsAccounts);
                   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_invoices_Click(object sender, RoutedEventArgs e)
        {//invoices
            try
            {
                invoicesLst.Clear();
                cashesLst.Clear();
                Window.GetWindow(this).Opacity = 0.2;
                wd_invoicesList w = new wd_invoicesList();

                if (cb_depositTo.SelectedValue.ToString() == "v")
                    w.agentId = Convert.ToInt32(cb_recipientV.SelectedValue);
                else if (cb_depositTo.SelectedValue.ToString() == "c")
                    w.agentId = Convert.ToInt32(cb_recipientC.SelectedValue);
                else if (cb_depositTo.SelectedValue.ToString() == "u")
                    w.userId = Convert.ToInt32(cb_recipientU.SelectedValue);
                else if (cb_depositTo.SelectedValue.ToString() == "sh")
                    w.shippingCompanyId = Convert.ToInt32(cb_recipientSh.SelectedValue);

                w.invType = "pay";

                w.ShowDialog();
                if (w.isActive)
                {
                    tb_cash.Text = SectionData.DecTostring(w.sum);
                    tb_cash.IsReadOnly = true;
                    cb_recipientC.IsEnabled = false;
                    cb_recipientV.IsEnabled = false;
                    cb_recipientU.IsEnabled = false;
                    cb_recipientSh.IsEnabled = false;
                    tb_recipientText.IsEnabled = false;
                    invoicesLst.AddRange(w.selectedInvoices);
                    cashesLst.AddRange(w.selectedCashtansfers);
                }
                Window.GetWindow(this).Opacity = 1;
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        async Task<IEnumerable<CashTransfer>> RefreshCashesList()
        {
            //cashes = await cashModel.GetCashTransferAsync("p", "all");
            cashes = await cashModel.GetCashBond("p", "all");
            cashes = cashes.Where(x => (x.processType != "balance" && x.processType != "destroy" && x.processType != "shortage" && x.processType != "commissionAgent")).GroupBy(x => x.transNum).Select(x => new CashTransfer
            {
                cashTransId = x.FirstOrDefault().cashTransId,
                transType = x.FirstOrDefault().transType,
                posId = x.FirstOrDefault().posId,
                userId = x.FirstOrDefault().userId,
                agentId = x.FirstOrDefault().agentId,
                invId = x.FirstOrDefault().invId,
                transNum = x.FirstOrDefault().transNum,
                createDate = x.FirstOrDefault().createDate,
                updateDate = x.FirstOrDefault().updateDate,
                cash = x.Sum(g => g.cash),
                updateUserId = x.FirstOrDefault().updateUserId,
                createUserId = x.FirstOrDefault().createUserId,
                notes = x.FirstOrDefault().notes,
                posIdCreator = x.FirstOrDefault().posIdCreator,
                isConfirm = x.FirstOrDefault().isConfirm,
                cashTransIdSource = x.FirstOrDefault().cashTransIdSource,
                side = x.FirstOrDefault().side,
                docName = x.FirstOrDefault().docName,
                docNum = x.FirstOrDefault().docNum,
                docImage = x.FirstOrDefault().docImage,
                bankId = x.FirstOrDefault().bankId,
                bankName = x.FirstOrDefault().bankName,
                agentName = x.FirstOrDefault().agentName,
                usersName = x.FirstOrDefault().usersName,// side =u
                posName = x.FirstOrDefault().posName,
                posCreatorName = x.FirstOrDefault().posCreatorName,
                processType = x.FirstOrDefault().processType,
                cardId = x.FirstOrDefault().cardId,
                bondId = x.FirstOrDefault().bondId,
                usersLName = x.FirstOrDefault().usersLName,// side =u
                createUserName = x.FirstOrDefault().createUserName,
                createUserLName = x.FirstOrDefault().createUserLName,
                createUserJob = x.FirstOrDefault().createUserJob,
                cardName = x.FirstOrDefault().cardName,
                bondDeserveDate = x.FirstOrDefault().bondDeserveDate,
                bondIsRecieved = x.FirstOrDefault().bondIsRecieved,
                shippingCompanyId = x.FirstOrDefault().shippingCompanyId,
                shippingCompanyName = x.FirstOrDefault().shippingCompanyName,
                isInvPurpose = x.FirstOrDefault().isInvPurpose,
                purpose = x.FirstOrDefault().purpose,
                invNumber = x.FirstOrDefault().invNumber,
                invType = x.FirstOrDefault().invType,
                otherSide = x.FirstOrDefault().otherSide
            });

            return cashes;

        }
        void RefreshCashView()
        {
            dg_paymentsAccounts.ItemsSource = cashesQuery;
            cashes = cashes.Where(x => x.processType != "balance");
            txt_count.Text = cashesQuery.Count().ToString();
        }
        async Task Search()
        {
            try
            {
                if (cashes is null)
                    await RefreshCashesList();

                if (chb_all.IsChecked == false)
                {
                    searchText = tb_search.Text.ToLower();
                    cashesQuery = cashes.Where(s => (s.transNum.ToLower().Contains(searchText)
                    || s.cash.ToString().ToLower().Contains(searchText)
                    )
                    && (s.side == "v" || s.side == "c" || s.side == "u" || s.side == "s" || s.side == "e" || s.side == "m" || s.side == "sh" || s.side == "shd" || s.side == "tax")
                    && s.transType == "p"
                    && s.processType != "inv"
                    && s.updateDate.Value.Date >= dp_startSearchDate.SelectedDate.Value.Date
                    && s.updateDate.Value.Date <= dp_endSearchDate.SelectedDate.Value.Date
                    );
                }
                else
                {
                    searchText = tb_search.Text.ToLower();
                    cashesQuery = cashes.Where(s => (s.transNum.ToLower().Contains(searchText)
                    || s.cash.ToString().ToLower().Contains(searchText)
                    )
                    && (s.side == "v" || s.side == "c" || s.side == "u" || s.side == "s" || s.side == "e" || s.side == "m" || s.side == "sh" || s.side == "shd" || s.side == "tax")
                    && s.transType == "p"
                    && s.processType != "inv"
                    );
                }
                cashesQueryExcel = cashesQuery.ToList();
                RefreshCashView();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private void translate()
        {
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trTransaferDetails");
            txt_Payments.Text = MainWindow.resourcemanager.GetString("trPayments");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_depositTo, MainWindow.resourcemanager.GetString("trDepositToHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_recipientV, MainWindow.resourcemanager.GetString("trRecipientHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_recipientC, MainWindow.resourcemanager.GetString("trRecipientHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_recipientU, MainWindow.resourcemanager.GetString("trRecipientHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_recipientSh, MainWindow.resourcemanager.GetString("trRecipientHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_recipientText, MainWindow.resourcemanager.GetString("trRecipientHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentProcessType, MainWindow.resourcemanager.GetString("trPaymentTypeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_docNum, MainWindow.resourcemanager.GetString("trDocNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_docNumCheque, MainWindow.resourcemanager.GetString("trDocNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_docNumCard, MainWindow.resourcemanager.GetString("trProcessNumHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_docDate, MainWindow.resourcemanager.GetString("trDocDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCashHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_purpose, MainWindow.resourcemanager.GetString("PaymentPurpose"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));

            chb_all.Content = MainWindow.resourcemanager.GetString("trAll");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startSearchDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endSearchDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            dg_paymentsAccounts.Columns[0].Header = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            dg_paymentsAccounts.Columns[1].Header = MainWindow.resourcemanager.GetString("trRecepient");
            dg_paymentsAccounts.Columns[2].Header = MainWindow.resourcemanager.GetString("trPaymentTypeTooltip");
            dg_paymentsAccounts.Columns[3].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_paymentsAccounts.Columns[4].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

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

            btn_add.Content = MainWindow.resourcemanager.GetString("trSave");
            txt_imageButton.Text = MainWindow.resourcemanager.GetString("trImage");
            txt_previewButton.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_printInvoiceButton.Text = MainWindow.resourcemanager.GetString("trPrint");
            txt_pdfButton.Text = MainWindow.resourcemanager.GetString("trPdfBtn");

        }
        private bool chkEnoughBalance(decimal ammount)
        {

            if (AppSettings.PosBalance >= ammount)
            { return true; }
            else { return false; }
        }
        private async Task<decimal> saveBond(string num, decimal ammount, Nullable<DateTime> date, string type)
        {
            Bonds bond = new Bonds();
            bond.number = num;
            bond.amount = ammount;
            bond.deserveDate = date;
            bond.type = type;
            bond.isRecieved = 0;
            bond.createUserId = MainWindow.userID.Value;

            //int s = (int)await bondModel.Save(bond,MainWindow.posID.Value);
            int s = (int)await bondModel.Save(bond, 0);

            return s;

        }
        private async Task<decimal> calcBalance(decimal ammount, string recipient, int agentid)
        {//balance for pos
            ammount *= -1;
            Pos pos = await posModel.EditBalance(MainWindow.posID.Value, ammount);


            return (decimal)pos.balance;
        }
        private async Task<string> SimLongRunningProcessAsync()
        {
            await Task.Delay(2000);
            return "Success";
        }
        void FN_ExportToExcel()
        {
            var QueryExcel = cashesQueryExcel.AsEnumerable().Select(x => new
            {
                TransNum = x.transNum,
                DepositTo = x.side,
                Recipient = x.agentName,
                OpperationType = x.processType,
                Cash = x.cash
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trDepositTo");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trRecepient");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trPaymentTypeTooltip");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trCashTooltip");

            ExportToExcel.Export(DTForExcel);

        }
        private async Task fillVendors()
        {
            try
            {
                await FillCombo.FillComboVendorsActiveForAccountType_p(cb_recipientV);
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
                await FillCombo.FillComboCustomersActiveForAccountType_p(cb_recipientC);
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
                await FillCombo.FillComboUsersActiveForAccountType_p(cb_recipientU);
                //users = await userModel.GetActiveForAccount("p");
                //cb_recipientU.ItemsSource = users;
                //cb_recipientU.DisplayMemberPath = "username";
                //cb_recipientU.SelectedValuePath = "userId";
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async Task fillShippingCompanies()
        {
            try
            {//
                await FillCombo.FillComboShippingCompaniesActiveForAccountNotLocalType_p(cb_recipientSh);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        #endregion

        #region events
        private async void dp_SelectedStartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                await RefreshCashesList();
                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void dp_SelectedEndDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                await RefreshCashesList();
                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_paymentsAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                SectionData.clearValidate(tb_docNum, p_errorDocNum);

                if (grid_document.IsVisible)
                {
                    TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                    SectionData.clearValidate(dpDate, p_errorDocDate);
                }

                #region clear validate
                SectionData.clearValidate(tb_cash, p_errorCash);
                SectionData.clearComboBoxValidate(cb_depositTo, p_errorDepositTo);
                SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_paymentProcessType, p_errorpaymentProcessType);
                SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
                SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                #endregion

                if (dg_paymentsAccounts.SelectedIndex != -1)
                {
                    cashtrans = dg_paymentsAccounts.SelectedItem as CashTransfer;
                    cb_paymentProcessType.SelectedIndex = -1;
                    this.DataContext = cashtrans;
                    tb_purpose.Text = SectionData.purposeConverter(cashtrans);

                    if (cashtrans != null)
                    {
                        btn_image.IsEnabled = true;
                        btn_preview.IsEnabled = true;
                        btn_pdf.IsEnabled = true;
                        btn_printInvoice.IsEnabled = true;
                        tb_cash.Text = SectionData.DecTostring(cashtrans.cash);

                        cb_depositTo.SelectedValue = cashtrans.side;
                        ///////////////////////////
                        btn_add.IsEnabled = false;
                        cb_depositTo.IsEnabled = false;
                        cb_recipientV.IsEnabled = false;
                        cb_recipientC.IsEnabled = false;
                        cb_recipientU.IsEnabled = false;
                        cb_recipientSh.IsEnabled = false;
                        tb_recipientText.IsEnabled = false;
                        cb_paymentProcessType.IsEnabled = false;
                        tb_docNum.IsEnabled = false;
                        dp_docDate.IsEnabled = false;
                        tb_docNumCheque.IsEnabled = false;
                        dp_docDateCheque.IsEnabled = false;
                        tb_docNumCard.IsEnabled = false;
                        gd_card.IsEnabled = false;
                        tb_cash.IsEnabled = false;
                        tb_purpose.IsEnabled = false;
                        tb_note.IsEnabled = false;
                        /////////////////////////
                        ///
                        #region receipient
                        switch (cb_depositTo.SelectedValue.ToString())
                        {
                            case "v":
                                cb_recipientV.SelectedIndex = -1;
                                try
                                { cb_recipientV.SelectedValue = cashtrans.agentId.Value; }
                                catch (Exception ex)
                                {
                                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                                }
                                cb_recipientC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                                cb_recipientU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                                cb_recipientSh.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                                tb_recipientText.Visibility = Visibility.Collapsed; SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                                break;
                            case "c":
                                cb_recipientC.SelectedIndex = -1;
                                try
                                { cb_recipientC.SelectedValue = cashtrans.agentId.Value; }
                                catch (Exception ex)
                                {
                                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                                }
                                cb_recipientV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                                cb_recipientU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                                cb_recipientSh.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                                tb_recipientText.Visibility = Visibility.Collapsed; SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                                break;
                            case "u":
                            case "s":
                                cb_recipientU.SelectedIndex = -1;
                                try
                                { cb_recipientU.SelectedValue = cashtrans.userId.Value; }
                                catch (Exception ex)
                                {
                                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                                }
                                cb_recipientV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                                cb_recipientC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                                cb_recipientSh.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                                tb_recipientText.Visibility = Visibility.Collapsed; SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                                break;
                            case "sh":
                                cb_recipientSh.SelectedIndex = -1;
                                try
                                { cb_recipientSh.SelectedValue = cashtrans.shippingCompanyId.Value; }
                                catch (Exception ex)
                                {
                                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                                }
                                cb_recipientC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                                cb_recipientV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                                cb_recipientU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                                break;
                            case "e":
                            case "m":
                                //cb_recipientSh.SelectedIndex = -1;
                                //try
                                //{ cb_recipientSh.SelectedValue = cashtrans.shippingCompanyId.Value; }
                                cb_recipientV.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                                cb_recipientC.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                                cb_recipientU.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                                cb_recipientSh.Visibility = Visibility.Collapsed; SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                                break;
                        }
                        #endregion

                        tb_transNum.Text = cashtrans.transNum;

                        cb_paymentProcessType.SelectedValue = cashtrans.processType;

                        if (cashtrans.cardId != null)
                            _SelectedCard = (int)cashtrans.cardId;

                        dp_docDate.SelectedDate = cashtrans.bondDeserveDate;
                    }
                }

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                cashtrans = new CashTransfer();
                btn_image.IsEnabled = false;
                btn_preview.IsEnabled = false;
                btn_pdf.IsEnabled = false;
                btn_printInvoice.IsEnabled = false;
                btn_add.IsEnabled = true;
                btn_invoices.Visibility = Visibility.Collapsed;
                btn_invoices.IsEnabled = false;
                tb_cash.IsReadOnly = false;
                tb_transNum.Text = "";
                cb_depositTo.SelectedIndex = -1;
                cb_paymentProcessType.SelectedIndex = -1;
                _SelectedCard = -1;
                gd_card.Visibility = Visibility.Collapsed;
                p_errorDocCard.Visibility = Visibility.Collapsed;
                p_errorDocNum.Visibility = Visibility.Collapsed;
                p_errorDocNumCheque.Visibility = Visibility.Collapsed;
                tb_docNum.Clear();
                tb_docNumCheque.Clear();
                tb_docNumCard.Clear();
                dp_docDate.SelectedDate = null;
                tb_cash.Clear();
                tb_purpose.Clear();
                tb_note.Clear();
                cb_recipientV.Visibility = Visibility.Collapsed;
                cb_recipientC.Visibility = Visibility.Collapsed;
                cb_recipientU.Visibility = Visibility.Collapsed;
                cb_recipientSh.Visibility = Visibility.Collapsed;
                tb_recipientText.Visibility = Visibility.Collapsed;
                if (grid_document.IsVisible)
                {
                    TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                    SectionData.clearValidate(dpDate, p_errorDocDate);
                }
                grid_document.Visibility = Visibility.Collapsed;
                grid_cheque.Visibility = Visibility.Collapsed;
                SectionData.clearValidate(tb_docNum, p_errorDocNum);
                SectionData.clearValidate(tb_cash, p_errorCash);
                SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
                SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                SectionData.clearComboBoxValidate(cb_depositTo, p_errorDepositTo);
                SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                SectionData.clearComboBoxValidate(cb_paymentProcessType, p_errorpaymentProcessType);
                SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                ///////////////////////////
                btn_add.IsEnabled = true;
                cb_depositTo.IsEnabled = true;
                cb_recipientV.IsEnabled = true;
                cb_recipientC.IsEnabled = true;
                cb_recipientU.IsEnabled = true;
                cb_recipientSh.IsEnabled = true;
                tb_recipientText.IsEnabled = true;
                cb_paymentProcessType.IsEnabled = true;
                tb_docNum.IsEnabled = true;
                dp_docDate.IsEnabled = true;
                tb_docNumCheque.IsEnabled = true;
                tb_docNumCard.IsEnabled = true;
                dp_docDateCheque.IsEnabled = true;
                gd_card.IsEnabled = true;
                tb_cash.IsEnabled = true;
                tb_purpose.IsEnabled = true;
                tb_note.IsEnabled = true;
                txt_card.Text = "";
                tb_docNumCard.Visibility = Visibility.Collapsed;
                //set border color
                foreach (var el in cardEllipseList)
                {
                    el.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
                }
                /////////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_confirm_Click(object sender, RoutedEventArgs e)
        {//confirm

        }
        private void Btn_image_Click(object sender, RoutedEventArgs e)
        {//image
            try
            {
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
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                await RefreshCashesList();
                await Search();

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_paymentProcessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//type selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                TextBox dpDate = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDate);
                TextBox dpDateCheque = (TextBox)dp_docDate.Template.FindName("PART_TextBox", dp_docDateCheque);

                switch (cb_paymentProcessType.SelectedIndex)
                {
                    case 0://cash
                        grid_document.Visibility = Visibility.Collapsed;
                        tb_docNum.Clear();
                        dp_docDate.SelectedDate = null;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        tb_docNumCheque.Clear();
                        dp_docDateCheque.SelectedDate = null;
                        gd_card.Visibility = Visibility.Collapsed;
                        _SelectedCard = -1;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
                        SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                        break;

                    case 1://doc
                        grid_document.Visibility = Visibility.Visible;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        tb_docNumCheque.Clear();
                        dp_docDateCheque.SelectedDate = null;
                        gd_card.Visibility = Visibility.Collapsed;
                        _SelectedCard = -1;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                        break;

                    case 2://cheque
                        grid_cheque.Visibility = Visibility.Visible;
                        grid_document.Visibility = Visibility.Collapsed;
                        tb_docNum.Clear();
                        dp_docDate.SelectedDate = null;
                        gd_card.Visibility = Visibility.Collapsed;
                        _SelectedCard = -1;
                        tb_docNumCard.Visibility = Visibility.Collapsed;
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearValidate(tb_docNumCard, p_errorDocCard);
                        SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                        break;

                    case 3://card
                        grid_document.Visibility = Visibility.Collapsed;
                        tb_docNum.Clear();
                        dp_docDate.SelectedDate = null;
                        grid_cheque.Visibility = Visibility.Collapsed;
                        tb_docNum.Clear();
                        dp_docDate.SelectedDate = null;
                        gd_card.Visibility = Visibility.Visible;
                        if (cashtrans.cardId != null)
                        {
                            Button btn = cardBtnList.Where(c => (int)c.Tag == cashtrans.cardId.Value).FirstOrDefault();
                            card_Click(btn, null);
                        }
                        SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                        SectionData.clearValidate(tb_docNum, p_errorDocNum);
                        SectionData.clearValidate(tb_docNumCheque, p_errorDocNumCheque);
                        break;
                }
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_depositTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//deposit selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                switch (cb_depositTo.SelectedIndex)
                {
                    case 0://vendor
                        cb_recipientV.SelectedIndex = -1;
                        cb_recipientV.IsEnabled = true;
                        cb_recipientV.Visibility = Visibility.Visible;
                        btn_invoices.Visibility = Visibility.Visible;
                        btn_invoices.IsEnabled = false;
                        cb_recipientC.Visibility = Visibility.Collapsed;
                        cb_recipientU.Visibility = Visibility.Collapsed;
                        cb_recipientSh.Visibility = Visibility.Collapsed;
                        tb_recipientText.Visibility = Visibility.Collapsed;
                        SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                        SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 50, 5);
                        break;
                    case 1://customer
                        cb_recipientC.SelectedIndex = -1;
                        cb_recipientC.IsEnabled = true;
                        cb_recipientV.Visibility = Visibility.Collapsed;
                        btn_invoices.Visibility = Visibility.Visible;
                        btn_invoices.IsEnabled = false;
                        cb_recipientC.Visibility = Visibility.Visible;
                        cb_recipientU.Visibility = Visibility.Collapsed;
                        cb_recipientSh.Visibility = Visibility.Collapsed;
                        tb_recipientText.Visibility = Visibility.Collapsed;
                        SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                        SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 50, 5);
                        break;
                    case 2://user
                        cb_recipientU.SelectedIndex = -1;
                        cb_recipientU.IsEnabled = true;
                        cb_recipientV.Visibility = Visibility.Collapsed;
                        btn_invoices.Visibility = Visibility.Visible;
                        btn_invoices.IsEnabled = false;
                        cb_recipientC.Visibility = Visibility.Collapsed;
                        cb_recipientU.Visibility = Visibility.Visible;
                        cb_recipientSh.Visibility = Visibility.Collapsed;
                        tb_recipientText.Visibility = Visibility.Collapsed;
                        cb_recipientU.Margin = cb_recipientC.Margin;
                        SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                        SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 50, 5);
                        break;
                    case 3://salary
                        cb_recipientU.SelectedIndex = -1;
                        cb_recipientV.Visibility = Visibility.Collapsed;
                        btn_invoices.Visibility = Visibility.Collapsed;
                        btn_invoices.IsEnabled = false;
                        cb_recipientC.Visibility = Visibility.Collapsed;
                        cb_recipientU.Visibility = Visibility.Visible;
                        cb_recipientSh.Visibility = Visibility.Collapsed;
                        tb_recipientText.Visibility = Visibility.Collapsed;
                        cb_recipientU.Margin = cb_depositTo.Margin;
                        SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                        SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 50, 5);
                        break;
                    case 4: //general expenses
                    case 5://administrative pull
                        cb_recipientV.Visibility = Visibility.Collapsed;
                        btn_invoices.Visibility = Visibility.Collapsed;
                        btn_invoices.IsEnabled = false;
                        cb_recipientC.Visibility = Visibility.Collapsed;
                        cb_recipientU.Visibility = Visibility.Collapsed;
                        cb_recipientSh.Visibility = Visibility.Collapsed;
                        tb_recipientText.Visibility = Visibility.Visible;
                        //cb_recipientV.Text = ""; cb_recipientC.Text = ""; cb_recipientU.Text = ""; cb_recipientSh.Text = "";
                        SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 15, 5);
                        break;
                    case 6://shipping company
                        cb_recipientSh.SelectedIndex = -1;
                        cb_recipientSh.IsEnabled = true;
                        cb_recipientV.Visibility = Visibility.Collapsed;
                        btn_invoices.Visibility = Visibility.Visible;
                        btn_invoices.IsEnabled = true;
                        cb_recipientC.Visibility = Visibility.Collapsed;
                        cb_recipientU.Visibility = Visibility.Collapsed;
                        cb_recipientSh.Visibility = Visibility.Visible;
                        tb_recipientText.Visibility = Visibility.Collapsed;
                        SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                        SectionData.clearValidate(tb_recipientText, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 50, 5);
                        break;
                    case 7://tax
                        cb_recipientV.Visibility = Visibility.Collapsed;
                        btn_invoices.Visibility = Visibility.Collapsed;
                        btn_invoices.IsEnabled = false;
                        cb_recipientC.Visibility = Visibility.Collapsed;
                        cb_recipientU.Visibility = Visibility.Collapsed;
                        cb_recipientSh.Visibility = Visibility.Collapsed;
                        tb_recipientText.Visibility = Visibility.Visible;
                        //cb_recipientV.Text = ""; cb_recipientC.Text = ""; cb_recipientU.Text = ""; cb_recipientSh.Text = "";
                        SectionData.clearComboBoxValidate(cb_recipientV, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientC, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientU, p_errorRecipient);
                        SectionData.clearComboBoxValidate(cb_recipientSh, p_errorRecipient);
                        p_errorRecipient.Margin = new Thickness(5, 5, 15, 5);
                        break;
                }

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
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
                else if ((sender as TextBox).Name == "tb_docNumCard")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorDocCard, tt_errorDocCard, "trEmptyProcessNumToolTip");
                else if ((sender as TextBox).Name == "tb_docNumCheque")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorDocNumCheque, tt_errorDocNumCheque, "trEmptyDocNumToolTip");
                 else if ((sender as TextBox).Name == "tb_recipientText")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorRecipient, tt_errorRecipient, "trErrorEmptyDepositToToolTip");

            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_depositTo")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorDepositTo, tt_errorDepositTo, "trErrorEmptyDepositToToolTip");
                else if ((sender as ComboBox).Name == "cb_recipientV")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                else if ((sender as ComboBox).Name == "cb_recipientC")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                else if ((sender as ComboBox).Name == "cb_recipientU")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                else if ((sender as ComboBox).Name == "cb_recipientSh")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorRecipient, tt_errorRecipient, "trErrorEmptyRecipientToolTip");
                else if ((sender as ComboBox).Name == "cb_paymentProcessType")
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
        private void Tb_cash_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only decimal
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;
            else
                e.Handled = true;
        }
        private void Tb_docNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only int
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void Btn_salaries_Click(object sender, RoutedEventArgs e)
        {//salaries
        }
        private void Tb_EnglishDigit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only english and digits
            Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
            if (!regex.IsMatch(e.Text))
                e.Handled = true;
        }
        private void Cb_recipientV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((cb_recipientV.SelectedIndex != -1) && (cb_recipientV.IsEnabled))
                    btn_invoices.IsEnabled = true;
                else
                    btn_invoices.IsEnabled = false;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_recipientC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((cb_recipientC.SelectedIndex != -1) && (cb_recipientC.IsEnabled))
                    btn_invoices.IsEnabled = true;
                else
                    btn_invoices.IsEnabled = false;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_recipientU_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((cb_recipientU.SelectedIndex != -1) && (cb_recipientU.IsEnabled))
                    btn_invoices.IsEnabled = true;
                else
                    btn_invoices.IsEnabled = false;
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
        private  void Chb_all_Checked(object sender, RoutedEventArgs e)
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
        private void Cb_recipientV_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.vendorsActiveForAccountListType_p.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_recipientC_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.customersActiveForAccountListType_p.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_recipientU_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.usersActiveForAccountListType_p.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_recipientSh_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = FillCombo.shippingCompaniesActiveForAccountListNotLocalType_p.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region reports
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string startDate = "";
            string endDate = "";
            string searchval = "";
            string Allchk = "";
            //  List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
            string addpath;
         
            if (isArabic)
            {
                addpath = @"\Reports\Account\Ar\ArPayAccReport.rdlc";
            }
            else addpath = @"\Reports\Account\En\PayAccReport.rdlc";

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
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //cashesQueryExcel = cashesQuery.ToList();
            clsReports.paymentAccReport(cashesQuery, rep, reppath, paramarr);
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
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    #region
                    //Thread t1 = new Thread(() =>
                    //{
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
                    //});
                    //t1.Start();
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);
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
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_preview1_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
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
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);
                /////////////////////
                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    //cashesQueryExcel = cashesQuery.ToList();
                    win_lvc win = new win_lvc(cashesQueryExcel, 8);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
                /////////////////////
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_pdf1_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one"))
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
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (cashtrans.cashTransId > 0)
                    {
                        BuildVoucherReport();

                        saveFileDialog.Filter = "PDF|*.pdf;";

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string filepath = saveFileDialog.FileName;
                            try { LocalReportExtensions.ExportToPDF(rep, filepath); }
                            catch (Exception ex)
                            {
                                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                            }

                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    if (cashtrans.cashTransId > 0)
                    {
                        BuildVoucherReport();

                        if (MainWindow.docPapersize == "A4")
                        {
                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                        }
                        else //A5
                        {
                            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name,2);

                        }
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void BuildVoucherReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\Ar\ArPayReportA4.rdlc";
                }
                else //A5
                {
                    addpath = @"\Reports\Account\Ar\ArPayReport.rdlc";
                }

            }
            else
            {
                if (MainWindow.docPapersize == "A4")
                {
                    addpath = @"\Reports\Account\En\PayReportA4.rdlc";
                }
                else //A5
                {
                    addpath = @"\Reports\Account\En\PayReport.rdlc";
                }
            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            rep.ReportPath = reppath;
            rep.DataSources.Clear();
            rep.EnableExternalImages = true;
            paramarr = reportclass.fillPayReport(cashtrans);
            clsReports.Header(paramarr);      
            rep.SetParameters(paramarr);       
            rep.Refresh();
        }
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_ucPaymentsAccounts);

                if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    string pdfpath;
                    pdfpath = @"\Thumb\report\temp.pdf";
                    pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                    if (cashtrans.cashTransId > 0)
                    {

                        BuildVoucherReport();
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

                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
            }
            catch (Exception ex)
            {
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_ucPaymentsAccounts);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

     
    }
}

