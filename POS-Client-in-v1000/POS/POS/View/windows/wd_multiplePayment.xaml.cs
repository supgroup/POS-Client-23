using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using WPFTabTip;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_multiplePayment.xaml
    /// </summary>
    public partial class wd_multiplePayment : Window
    {
        public wd_multiplePayment()
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
        static private object _Sender;
        public bool isOk { get; set; }
        public bool isPurchase { get; set; }
        public bool hasCredit { get; set; }
        public bool hasDeliveryCompany { get; set; }
        public decimal maxCredit { get; set; }
        ImageBrush brush = new ImageBrush();
        static private string _SelectedPaymentType = "cash";
        static private string _SelectedPaymentTypeText = "Cash";
        static private int _SelectedCard = -1;
        static private Card selectedCard = new Card();
        public List<CashTransfer> listPayments = new List<CashTransfer>();
        CashTransfer cashTrasnfer;
        Card cardModel = new Card();
        public IEnumerable<Card> cards;
        public Invoice invoice = new Invoice();
        public Agent agent = new Agent();
        bool amountIsValid = false;
        public bool checkMaxCredit = false;

        public string windowOfSourceName = "";
        public decimal theRemine = 0;
        async Task refreshCards()
        {
            try
            {
                if (FillCombo.cardsList is null)
                    await FillCombo.RefreshCards();
                cards = FillCombo.cardsList;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
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

                await refreshCards();
                configurProcessType();

                // get it from invoice
                loading_fillCardCombo();

                //////////////////////////
                //invoice.agentId
                //////////////////////////
                tb_moneySympol1.Text =
                    tb_moneySympol2.Text =
                    tb_moneySympol3.Text = AppSettings.Currency;
                invoice.paid = 0;
                tb_cash.Text = invoice.totalNet.ToString();
                tb_total.Text = invoice.totalNet.ToString();


                #region max credit
                if (checkMaxCredit)
                {
                    if (agent != null)
                    {
                        if (agent.isLimited)
                        {
                            //decimal maxCredit = 0;
                            if (agent.maxDeserve != 0)
                                maxCredit = getCusAvailableBlnc();
                             hasCredit = true;
                        }
                        else
                        {
                            hasCredit = false;
                            maxCredit = 0;
                        }
                    }
                    else
                    {
                        hasCredit = false;
                        maxCredit = 0;
                    }
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
        private decimal getCusAvailableBlnc()
        {
            decimal maxCredit = 0;

            float customerBalance = agent.balance;

            if (agent.balanceType == 0)
                maxCredit = agent.maxDeserve + (decimal)customerBalance;
            else
            {
                maxCredit = agent.maxDeserve - (decimal)customerBalance;
                if (maxCredit < 0)
                    maxCredit = 0;
            }
            return maxCredit;
        }
        private void translate()
        {

            txt_title.Text = MainWindow.resourcemanager.GetString("trMultiplePayment");
            txt_theRemineTitle.Text = MainWindow.resourcemanager.GetString("trTheRemine");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");
            txt_sumTitle.Text = MainWindow.resourcemanager.GetString("trCashPaid");
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_paymentProcessType, MainWindow.resourcemanager.GetString("trPaymentProcessType"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCash"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_processNum, MainWindow.resourcemanager.GetString("trProcessNumTooltip"));
            chk_cash.Content = MainWindow.resourcemanager.GetString("trCash");
            chk_card.Content = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private void configurProcessType()
        {
            /*
            cb_paymentProcessType.DisplayMemberPath = "Text";
            cb_paymentProcessType.SelectedValuePath = "Value";
            if (invoice.invType.Equals("sbd"))
            {
                var typelist = new[] {
                 new { Text = MainWindow.resourcemanager.GetString("trCash")       , Value = "cash" },
                };
                cb_paymentProcessType.ItemsSource = typelist;
            }
            else
            {
                var typelist = new[] {
                new { Text = MainWindow.resourcemanager.GetString("trCash")       , Value = "cash" },
                new { Text = MainWindow.resourcemanager.GetString("trAnotherPaymentMethods") , Value = "card" },
                 };

                cb_paymentProcessType.ItemsSource = typelist;
            }
            cb_paymentProcessType.SelectedIndex = 0;
            */
            if (invoice.invType.Equals("sbd"))
            {
                chk_cash.Visibility = Visibility.Visible;
                chk_card.Visibility = Visibility.Collapsed;
            }
            else
            {
                chk_cash.Visibility = Visibility.Visible;
                chk_card.Visibility = Visibility.Visible;
            }

            chk_cash.IsChecked = true;
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            this.Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch //(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Btn_save_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isPurchase &&
               (
               hasCredit == true || hasDeliveryCompany == true ||
               invoice.paid >= invoice.totalNet
               ))
                {
                    if (invoice.totalNet - invoice.paid > 0)
                    {
                        #region Accept
                        this.Opacity = 0;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = (invoice.totalNet - invoice.paid) + " " + AppSettings.Currency + " " + MainWindow.resourcemanager.GetString("questionAddOnCredit");
                        w.ShowDialog();
                        this.Opacity = 1;
                        if (w.isOk)
                        {
                            #endregion
                            #region add balance
                            if (invoice.totalNet - invoice.paid > 0)
                            {
                                cashTrasnfer = new CashTransfer();

                                ///////////////////////////////////////////////////
                                cashTrasnfer.agentId = invoice.agentId;
                                cashTrasnfer.invId = invoice.invoiceId;
                                cashTrasnfer.processType = "balance";
                                cashTrasnfer.cash = invoice.totalNet - invoice.paid;
                                cashTrasnfer.isInvPurpose = true;
                                listPayments.Add(cashTrasnfer);
                            }
                            isOk = true;
                            this.Close();
                            #endregion
                        }
                    }
                    else
                    {
                        
                            #region add balance
                            if (invoice.totalNet - invoice.paid > 0)
                            {
                                cashTrasnfer = new CashTransfer();

                                ///////////////////////////////////////////////////
                                cashTrasnfer.agentId = invoice.agentId;
                                cashTrasnfer.invId = invoice.invoiceId;
                                cashTrasnfer.processType = "balance";
                                cashTrasnfer.cash = invoice.totalNet - invoice.paid;
                                cashTrasnfer.isInvPurpose = true;
                                listPayments.Add(cashTrasnfer);
                        }
                            isOk = true;
                            this.Close();
                            #endregion
                    }
                }
                else if (isPurchase && (hasCredit == true || invoice.paid >= invoice.totalNet))
                {
                    if (listPayments.Where(x => x.processType == "cash").Count() > 0 &&
                        listPayments.Where(x => x.processType == "cash").FirstOrDefault().cash > AppSettings.PosBalance
                        && invoice.invType != "pbd")
                    {
                        isOk = false;
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                    }
                    else
                    {
                        if (invoice.totalNet - invoice.paid > 0)
                        {
                            cashTrasnfer = new CashTransfer();

                            ///////////////////////////////////////////////////
                            cashTrasnfer.agentId = invoice.agentId;
                            cashTrasnfer.invId = invoice.invoiceId;
                            cashTrasnfer.processType = "balance";
                            cashTrasnfer.cash = invoice.totalNet - invoice.paid;
                                cashTrasnfer.isInvPurpose = true;
                            listPayments.Add(cashTrasnfer);
                        }
                        //lst_payments.Items.Add(s);
                        ///////////////////////////////////////////////////
                        isOk = true;
                        this.Close();
                    }
                }
                else 
                {
                    isOk = false;
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trAmountPaidEqualInvoiceValue"), animation: ToasterAnimation.FadeIn);
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_processNum")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorProcessNum, tt_errorProcessNum, "trEmptyProcessNumToolTip");
                }
                if (name == "ComboBox")
                {

                    //if ((sender as ComboBox).Name == "cb_paymentProcessType")
                    //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");

                }
                if (name == "TextBlock")
                {

                    if ((sender as TextBlock).Name == "txt_card")
                        SectionData.validateEmptyTextBlock((TextBlock)sender, p_errorCard, tt_errorCard, "trSelectCreditCard");
                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void paymentProcessType_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                if (checkBox.Name == "chk_cash")
                {
                    chk_card.IsChecked = false;
                }
                else if (checkBox.Name == "chk_card")
                {
                    chk_cash.IsChecked = false;
                }

                if (chk_cash.IsChecked.Value)
                {
                    //case 0://cash
                    _SelectedPaymentType = "cash";
                    _SelectedPaymentTypeText = chk_cash.Content.ToString();
                    gd_card.Visibility = Visibility.Collapsed;
                    tb_processNum.Clear();
                    _SelectedCard = -1;
                    txt_card.Text = "";
                    selectedCard.commissionValue = 0;
                    selectedCard.commissionRatio = 0;
                    SectionData.clearTextBlockValidate(txt_card, p_errorCard);
                    SectionData.clearValidate(tb_processNum, p_errorCard);
                }
                else if (chk_card.IsChecked.Value)
                {
                    //case 1://card
                    _SelectedPaymentType = "card";
                    _SelectedPaymentTypeText = chk_card.Content.ToString();
                    gd_card.Visibility = Visibility.Visible;
                    foreach (var el in cardEllipseList)
                    {
                        el.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void paymentProcessType_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "chk_cash")
                        chk_cash.IsChecked = true;
                    else if (cb.Name == "chk_card")
                        chk_card.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
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
                ellipse.Height = 35;
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

            }
            #endregion
        }
        List<Ellipse> cardEllipseList = new List<Ellipse>();
        void card_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                _SelectedCard = int.Parse(button.Tag.ToString());
                //txt_card.Text = button.DataContext.ToString();
                Card card = button.DataContext as Card;
                txt_card.Text = card.name;
                selectedCard.commissionValue = card.commissionValue;
                selectedCard.commissionRatio = card.commissionRatio;
                if (card.hasProcessNum.Value)
                    tb_processNum.Visibility = Visibility.Visible;
                else
                    tb_processNum.Visibility = Visibility.Collapsed;
                tb_processNum.Text = "";
                //set border color
                foreach (var el in cardEllipseList)
                {
                    if ((int)el.Tag == (int)button.Tag)
                        el.Stroke = Application.Current.Resources["MainColorBlue"] as SolidColorBrush;
                    else
                        el.Stroke = Application.Current.Resources["MainColorOrange"] as SolidColorBrush;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        void loading_fillCardCombo()
        {
            try
            {
                InitializeCardsPic(cards);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
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
        //async void userImageLoad(Ellipse ellipse, string image)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(image))
        //        {
        //            clearImg(ellipse);

        //            byte[] imageBuffer = await cardModel.downloadImage(image); // read this as BLOB from your DB
        //            var bitmapImage = new BitmapImage();
        //            using (var memoryStream = new System.IO.MemoryStream(imageBuffer))
        //            {
        //                bitmapImage.BeginInit();
        //                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //                bitmapImage.StreamSource = memoryStream;
        //                bitmapImage.EndInit();
        //            }
        //            ellipse.Fill = new ImageBrush(bitmapImage);
        //        }
        //        else
        //        {
        //            clearImg(ellipse);
        //        }
        //    }
        //    catch
        //    {
        //        clearImg(ellipse);
        //    }
        //}
        private void clearImg(Ellipse ellipse)
        {
            Uri resourceUri = new Uri("pic/no-image-icon-90x90.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            brush.ImageSource = temp;
            ellipse.Fill = brush;
        }
        private void PreventSpaces(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            try
            {
                var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
                if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                    e.Handled = false;

                else
                    e.Handled = true;
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
        private void Tb_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _Sender = sender;
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_cash")
                {
                    SectionData.InputJustNumber(ref txb);
                }
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
            amountIsValid = true;
            if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_cash")
                    amountIsValid = SectionData.validateEmptyTextBox((TextBox)sender, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
            }

        }
        private void Tb_cash_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {//only decimal
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;
            else
                e.Handled = true;
        }
        private void Btn_enter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tb_cash.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f8f8f8"));
                p_errorCash.Visibility = Visibility.Collapsed;
                //listPayments
                string s = "";
                cashTrasnfer = new CashTransfer();
                try
                {
                    cashTrasnfer.cash = decimal.Parse(tb_cash.Text);
                }
                catch
                {
                    cashTrasnfer.cash = 0;
                }
                if (cashTrasnfer.cash > 0)
                {
                    decimal totalCashList = (listPayments.Where(x => x.processType == "cash").Count() > 0) ?
                        listPayments.Where(x => x.processType == "cash").FirstOrDefault().cash.Value
                        : 0;



                    //if (cashTrasnfer.cash + invoice.paid <= invoice.totalNet || !(cb_paymentProcessType.SelectedValue.ToString().Equals("card")  && cashTrasnfer.cash - (invoice.totalNet - invoice.paid) > totalCashList))
                    if (cashTrasnfer.cash + invoice.paid <= invoice.totalNet || !(_SelectedPaymentType.Equals("card")  && cashTrasnfer.cash - (invoice.totalNet - invoice.paid) > totalCashList))
                    {
                        cashTrasnfer.agentId = invoice.agentId;
                        cashTrasnfer.invId = invoice.invoiceId;
                        //cashTrasnfer.processType = cb_paymentProcessType.SelectedValue.ToString();
                        cashTrasnfer.processType = _SelectedPaymentType;
                        //if (cb_paymentProcessType.SelectedValue.ToString().Equals("cash"))
                        if (_SelectedPaymentType.Equals("cash"))
                        {
                            //s = cb_paymentProcessType.Text + " : " + cashTrasnfer.cash;
                            s = validateDuplicate(cashTrasnfer.cash.Value);
                        }
                        //else if (cb_paymentProcessType.SelectedValue.ToString().Equals("card"))
                        else if (_SelectedPaymentType.Equals("card"))
                        {
                            if (_SelectedCard == -1)
                            {
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectCreditCard"), animation: ToasterAnimation.FadeIn);
                                return;
                            }
                            else if (tb_processNum.Visibility == Visibility.Visible && string.IsNullOrEmpty(tb_processNum.Text))
                            {
                                SectionData.SetError(tb_processNum, p_errorProcessNum, tt_errorProcessNum, "trIsRequired");
                                return;
                            }
                            else
                            {

                                cashTrasnfer.cardId = _SelectedCard;
                                cashTrasnfer.docNum = tb_processNum.Text;
                                cashTrasnfer.commissionValue = selectedCard.commissionValue;
                                cashTrasnfer.commissionRatio = selectedCard.commissionRatio;
                                s = txt_card.Text + " : " + cashTrasnfer.cash;
                            }
                        }
                        //else if (cb_paymentProcessType.SelectedValue.ToString().Equals("admin"))
                        else if (_SelectedPaymentType.Equals("admin"))
                        {
                            s = validateDuplicate(cashTrasnfer.cash.Value);
                        }

                        lst_payments.Items.Add(s);
                        cashTrasnfer.isInvPurpose = true;
                        listPayments.Add(cashTrasnfer);
                        invoice.paid += cashTrasnfer.cash;

                        #region test if we have remain
                        if (invoice.paid > invoice.totalNet)
                        {

                            int index = 0;
                            foreach (var item in lst_payments.Items)
                            {
                                if (item.ToString().Contains(MainWindow.resourcemanager.GetString("trCash")))
                                {
                                    //List<string> str = item.ToString().Split(':').ToList<string>();
                                    //str[1] = str[1].Replace(" ", "");
                                    //dec += decimal.Parse(str[1]);
                                    //invoice.paid -= decimal.Parse(str[1]);
                                    //hasDuplicate = true;
                                    break;
                                }
                                index++;
                            }
                            //if (hasDuplicate)
                            {

                                decimal difference = invoice.paid.Value - invoice.totalNet.Value;
                                listPayments[index].cash -= difference;
                                lst_payments.Items[index] = MainWindow.resourcemanager.GetString("trCash") + " : " + listPayments[index].cash;
                                invoice.paid -= difference;
                                theRemine += difference;

                                if (listPayments[index].cash == 0)
                                {
                                    listPayments.Remove(listPayments[index]);
                                    lst_payments.Items.Remove(lst_payments.Items[index]);
                                }
                            }
                        }

                        txt_sum.Text = (invoice.paid + theRemine).ToString();
                        txt_theRemine.Text = theRemine.ToString();


                        if (invoice.paid >= invoice.totalNet)
                            txt_sum.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush;
                        else
                            txt_sum.Foreground = Application.Current.Resources["mediumRed"] as SolidColorBrush;

                        tb_cash.Text = (invoice.totalNet - invoice.paid).ToString();

                        #endregion
                    }
                    else
                    {
                                SectionData.SetError(tb_cash, p_errorCash, tt_errorCash, "trAmountGreaterInvoiceValue");
                    }
                }
                else
                {
                                SectionData.SetError(tb_cash, p_errorCash, tt_errorCash, "trZeroAmmount");
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        string validateDuplicate(decimal dec)
        {
            try
            {
                string s = "";
                //decimal dec = 0;
                //List<string> str1 = s.ToString().Split(':').ToList<string>();
                //str1[0] = str1[0].Replace(" ", "");
                //dec = Decimal.Parse(str1[0]);
                bool hasDuplicate = false;
                int index = 0;
                foreach (var item in lst_payments.Items)
                {
                    //if (item.ToString().Contains(cb_paymentProcessType.Text))
                    if (item.ToString().Contains(_SelectedPaymentTypeText))
                    {
                        List<string> str = item.ToString().Split(':').ToList<string>();
                        str[1] = str[1].Replace(" ", "");
                        dec += decimal.Parse(str[1]);
                        invoice.paid -= decimal.Parse(str[1]);
                        hasDuplicate = true;
                        break;
                    }
                    index++;
                }
                if (hasDuplicate)
                {
                    listPayments.Remove(listPayments[index]);
                    lst_payments.Items.Remove(lst_payments.Items[index]);
                }

                cashTrasnfer.cash = dec;
                //s = cb_paymentProcessType.Text + " : " + cashTrasnfer.cash;
                s = _SelectedPaymentTypeText + " : " + cashTrasnfer.cash;
                return s;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return "";
            }
        }
        private void Btn_clearSerial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lst_payments.Items.Clear();
                listPayments.Clear();
                invoice.paid = 0;
                tb_cash.Text =
                    txt_sum.Text =
                    txt_theRemine.Text = "0";


            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Lst_payments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lst_payments.SelectedItem != null)
                {


                    List<string> str = lst_payments.SelectedItem.ToString().Split(':').ToList<string>();
                    str[1] = str[1].Replace(" ", "");
                    invoice.paid -= decimal.Parse(str[1]);
                    txt_sum.Text = (invoice.paid + theRemine).ToString();

                    listPayments.Remove(listPayments[lst_payments.SelectedIndex]);
                    lst_payments.Items.Remove(lst_payments.SelectedItem);

                    if (invoice.paid >= invoice.totalNet)
                        txt_sum.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush;
                    else
                        txt_sum.Foreground = Application.Current.Resources["mediumRed"] as SolidColorBrush;

                    tb_cash.Text = (invoice.totalNet - invoice.paid).ToString();

                    #region test remain
                    if (theRemine > 0)
                    {
                        try
                        {
                            string s = "";
                            cashTrasnfer = new CashTransfer();
                            try
                            {
                                cashTrasnfer.cash = theRemine;
                            }
                            catch
                            {
                                cashTrasnfer.cash = 0;
                            }
                            if (cashTrasnfer.cash > 0)
                            {
                                decimal totalCashList = (listPayments.Where(x => x.processType == "cash").Count() > 0) ?
                                    listPayments.Where(x => x.processType == "cash").FirstOrDefault().cash.Value
                                    : 0;



                                //if (cashTrasnfer.cash + invoice.paid <= invoice.totalNet || !(cb_paymentProcessType.SelectedValue.ToString().Equals("card") && cashTrasnfer.cash > totalCashList))
                                {
                                    cashTrasnfer.agentId = invoice.agentId;
                                    cashTrasnfer.invId = invoice.invoiceId;
                                    //cb_paymentProcessType.SelectedValue = "cash";
                                    chk_cash.IsChecked = true;
                                    //cashTrasnfer.processType = cb_paymentProcessType.SelectedValue.ToString();
                                    cashTrasnfer.processType = _SelectedPaymentType;
                                    {
                                        s = validateDuplicate(cashTrasnfer.cash.Value);
                                    }


                                    lst_payments.Items.Add(s);
                                    cashTrasnfer.isInvPurpose = true;
                                    listPayments.Add(cashTrasnfer);
                                    invoice.paid += cashTrasnfer.cash;
                                    theRemine = 0;

                                    #region test if we have remain
                                    if (invoice.paid > invoice.totalNet)
                                    {

                                        int index = 0;
                                        foreach (var item in lst_payments.Items)
                                        {
                                            if (item.ToString().Contains(MainWindow.resourcemanager.GetString("trCash")))
                                            {
                                                break;
                                            }
                                            index++;
                                        }
                                        {

                                            decimal difference = invoice.paid.Value - invoice.totalNet.Value;
                                            listPayments[index].cash -= difference;
                                            lst_payments.Items[index] = MainWindow.resourcemanager.GetString("trCash") + " : " + listPayments[index].cash;
                                            invoice.paid -= difference;
                                            theRemine += difference;

                                            if (listPayments[index].cash == 0)
                                            {
                                                listPayments.Remove(listPayments[index]);
                                                lst_payments.Items.Remove(lst_payments.Items[index]);
                                            }
                                        }
                                    }

                                    txt_sum.Text = (invoice.paid + theRemine).ToString();
                                    txt_theRemine.Text = theRemine.ToString();


                                    if (invoice.paid >= invoice.totalNet)
                                        txt_sum.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush;
                                    else
                                        txt_sum.Foreground = Application.Current.Resources["mediumRed"] as SolidColorBrush;

                                    tb_cash.Text = (invoice.totalNet - invoice.paid).ToString();

                                    #endregion
                                }
                                //else
                                //{
                                //    HelpClass.SetValidate(p_error_cash, "trAmountGreaterInvoiceValue");
                                //}
                            }
                            else
                            {
                                SectionData.SetError(tb_cash, p_errorCash, tt_errorCash, "trZeroAmmount");
                            }
                        }
                        catch (Exception ex)
                        {
                           SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_Keyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (TabTip.Close())
                {
                    #pragma warning disable CS0436 // Type conflicts with imported type
                    TabTip.OpenUndockedAndStartPoolingForClosedEvent();
                    #pragma warning restore CS0436 // Type conflicts with imported type
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }

    }
}
