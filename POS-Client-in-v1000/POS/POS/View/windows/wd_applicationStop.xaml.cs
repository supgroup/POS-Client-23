using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_applicationStop.xaml
    /// </summary>
    public partial class wd_applicationStop : Window
    {
        public wd_applicationStop()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        BrushConverter bc = new BrushConverter();
        Pos posModel = new Pos();
        IEnumerable<Pos> poss;
        //IEnumerable<CashTransfer> cashesQuery;
       // IEnumerable<CashTransfer> cashes;
        //CashTransfer cashModel = new CashTransfer();
        bool isAdmin;
        public string status = "";
        public int settingsPoSId = 0;
        public int userId;
        bool flag = false;

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_window);

                txt_moneyIcon.Text = AppSettings.Currency;

                #region translate

                if (AppSettings.lang.Equals("en"))
                     grid_window.FlowDirection = FlowDirection.LeftToRight;
                else
                     grid_window.FlowDirection = FlowDirection.RightToLeft;

                translate();
                #endregion

                await fillPosInfo();
                await fillPos();
                await fillCashquery();
                isAdmin = SectionData.isAdminPermision();

                BuildBillDesign(cardtransList.Where(x => x.total > 0 || x.total < 0).ToList());

                #region key up
                cb_pos.IsTextSearchEnabled = false;
                cb_pos.IsEditable = true;
                cb_pos.StaysOpenOnEdit = true;
                cb_pos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_pos.Text = "";
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_window);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_window);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        List<Pos> posLst = new List<Pos>();
        private async Task fillPos()
        {

            //poss = await posModel.Get();
            if (FillCombo.posAllReport is null)
                await FillCombo.RefreshPosAllReport();
            posLst = FillCombo.posAllReport.Where(p => p.branchId == MainWindow.branchID && p.posId != MainWindow.posID).ToList();
            cb_pos.ItemsSource = posLst;
            cb_pos.DisplayMemberPath = "name";
            cb_pos.SelectedValuePath = "posId";
            cb_pos.SelectedIndex = -1;
        }
        private async Task fillCashquery()
        {
            List<CardsSts> tmpcard = new List<CardsSts>();
            Boxquery = await stsModel.GetTransfromOpen((int)MainWindow.posID);
            tmpcard = await clsReports.calctotalCards(Boxquery);

            cardtransList = new List<CardsSts>();
            // open cash
            CardsSts cardcashrow = new CardsSts();
            
            cardcashrow = clsReports.BoxOpenCashCalc(Boxquery.ToList());         
            cardcashrow.name = MainWindow.resourcemanager.GetString("trOpenCash");
            //add cash row
            cardtransList.Add(cardcashrow);
            // cash
            cardcashrow = new CardsSts();
            cardcashrow = clsReports.BoxCashCalc(Boxquery.ToList());
            cardcashrow.name =  MainWindow.resourcemanager.GetString("trCash");
            //add cash row
            cardtransList.Add(cardcashrow);
            //add card list
            cardtransList.AddRange(tmpcard);
        }
        private async Task fillPosInfo()
        {
            await MainWindow.refreshBalance();
            //cashes = await cashModel.GetCashTransferForPosById("all", "p",(int)MainWindow.posID);
            //cashesQuery = cashes.Where(s => s.isConfirm == 1 
            //                                    && s.posId == MainWindow.posID.Value
            //                                    && s.isConfirm2 == 0).ToList();


            if (MainWindow.posLogIn.balance != 0)
            {
                txt_cashValue.Text = SectionData.DecTostring(MainWindow.posLogIn.balance);
                tb_cash.Text = SectionData.DecTostring(MainWindow.posLogIn.balance);
            }
            else
            {
                txt_cashValue.Text = "0";
                tb_cash.Text = "0";
            }

            status = MainWindow.posLogIn.boxState;
            if (MainWindow.posLogIn.boxState == "c")
            {
                txt_balanceState.Text = MainWindow.resourcemanager.GetString("trUnavailable");
                txt_stateValue.Text = MainWindow.resourcemanager.GetString("trClosed");
                //btn_isClose.Content = MainWindow.resourcemanager.GetString("trOpen");
                path_isClose.Data = App.Current.Resources["unlock"] as Geometry;
                txt_stateValue.Foreground = Application.Current.Resources["MainColorRed"] as SolidColorBrush; ;
                btn_save.IsEnabled = false;
                cb_pos.IsEnabled = false;

            }
            else
            {
                txt_stateValue.Text = MainWindow.resourcemanager.GetString("trOpen");
                //btn_isClose.Content = MainWindow.resourcemanager.GetString("trClose");
                path_isClose.Data = App.Current.Resources["lock"] as Geometry;

                txt_stateValue.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush; ;
                cb_pos.IsEnabled = true;

            }
        }
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trDailyClosing");
            txt_cash.Text = MainWindow.resourcemanager.GetString("trCash");
            txt_boxState.Text = MainWindow.resourcemanager.GetString("trBoxState");
            txt_cashBalance.Text = MainWindow.resourcemanager.GetString("trCashBalance");
            txt_cashBalance1.Text = MainWindow.resourcemanager.GetString("trCashBalance");
            txt_transfer.Text = MainWindow.resourcemanager.GetString("trTransfer");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCashHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPosHint"));

            tt_pos.Content = MainWindow.resourcemanager.GetString("trPosTooltip");
            btn_save.Content = MainWindow.resourcemanager.GetString("trTransfer");
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
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
         private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_window);
                //if (cashesQuery.Count() > 0)
                   // Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCantDoProcess"), animation: ToasterAnimation.FadeIn);
                //else
                { 
                    bool valid = validate();
                    if (valid)
                    {
                        var res = await transfer();
                        if (res >= 0)
                        {
                            AppSettings.PosBalance = res;
                            MainWindow.setBalance();
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            await fillPosInfo();

                        }
                        else if (res.Equals(-3))
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                
                if (sender != null)
                    SectionData.EndAwait(grid_window);

            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_window);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                cb_pos.SelectedIndex = -1;
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
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
        private void validateEmpty(string name, object sender)
        {
            if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_pos")
                    validateEmptyComboBox((ComboBox)sender, p_errorPos, tt_errorPos, "trErrorEmptyPosToolTip");
            }
            else if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_cash")
                SectionData.validateEmptyTextBox((TextBox)sender, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
            }
        }
         private void validateEmptyComboBox(ComboBox cb, Path p_error, ToolTip tt_error, string tr)
        {
            if (cb.SelectedIndex == -1)
            {
                p_error.Visibility = Visibility.Visible;
                tt_error.Content = MainWindow.resourcemanager.GetString(tr);
                cb.Background = (Brush)bc.ConvertFrom("#15FF0000");
            }
            else
            {
                cb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_error.Visibility = Visibility.Collapsed;
            }
        }
        private async void Btn_isClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_window);
                #region Accept
                this.Opacity = 0;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxConfirm");
                w.ShowDialog();
                this.Opacity = 1;
                #endregion
                if (w.isOk)
                {

                    if (MainWindow.posLogIn.boxState.Equals("c"))
                        status = "o";
                    else
                        status = "c";

                    await openCloseBox(status);
                    await fillPosInfo();
                    await fillCashquery();
                    BuildBillDesign(cardtransList.Where(x => x.total > 0 || x.total < 0).ToList());


                }
                if (sender != null)
                    SectionData.EndAwait(grid_window);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_window);
                this.Opacity = 1;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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
        #region open - close - validate
        private async Task openCloseBox(string status)
        {
            CashTransfer cashTransfer = new CashTransfer();
            cashTransfer.processType = "box";
            cashTransfer.transType = status;
            cashTransfer.cash = MainWindow.posLogIn.balance;
            cashTransfer.createUserId = MainWindow.userID.Value;
            cashTransfer.posId = (int)MainWindow.posID;
            if (status == "o")
                cashTransfer.transNum = await cashTransfer.generateCashNumber("bc");
            else
                cashTransfer.transNum = await cashTransfer.getLastOpenTransNum((int)MainWindow.posID);
            int res = (int)await posModel.updateBoxState((int)MainWindow.posID,status,Convert.ToInt32(isAdmin),MainWindow.userLogin.userId,cashTransfer);
            if (res > 0)
            {
                await MainWindow.refreshBalance();
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
            }
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

        }
        private async Task<decimal> transfer()
        {
            //add cash transfer
            CashTransfer cash1 = new CashTransfer();

            cash1.transType = "d";//deposit
            cash1.transNum = "dp";
           // cash1.cash = MainWindow.posLogIn.balance;
            cash1.cash = decimal.Parse(tb_cash.Text);
            cash1.createUserId = MainWindow.userID.Value;
            cash1.posIdCreator = MainWindow.posID.Value;
            cash1.isConfirm = 1;
            cash1.side = "p";//pos
            cash1.posId = Convert.ToInt32(MainWindow.posID.Value);


            CashTransfer cash2 = new CashTransfer();

            cash2.transType = "p";//pull
            cash2.transNum = "pp";
            cash2.cash = decimal.Parse(tb_cash.Text);
            cash2.createUserId = MainWindow.userID.Value;
            cash2.posIdCreator = MainWindow.posID.Value;
            cash2.isConfirm = 0;
            cash2.side = "p";//pos
            cash2.posId = Convert.ToInt32(cb_pos.SelectedValue);

            var res = await cash1.transferPosBalance(cash1,cash2);
          

            return res;
        }
        private Boolean validate()
        {
           
            SectionData.validateEmptyTextBox(tb_cash, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
            SectionData.validateEmptyComboBox(cb_pos, p_errorPos, tt_errorPos, "trErrorEmptyPosToolTip");
            if (cb_pos.SelectedIndex == -1 || tb_cash.Text.Equals(""))
                return false;

            if (decimal.Parse(tb_cash.Text) == 0)
            {
                SectionData.SetError(tb_cash, p_errorCash, tt_errorCash, "itMustBeGreaterThanZero");
                return false;
            }

           
            return true;
        }
        #endregion
        #region print
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        IEnumerable<OpenClosOperatinModel> Boxquery;
        Statistics stsModel = new Statistics();
        List<CardsSts> cardtransList = new List<CardsSts>();
        private async Task  BuildOperationReport(int posId)
        {

            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "closing";//trDailyClosing
            string secondTitle = "";
            string subTitle = "";
            string Title = "";

            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Accounts\box\Ar\ArBox.rdlc";

            }
            else
            {
                //english
                addpath = @"\Reports\StatisticReport\Accounts\box\En\EnBox.rdlc";
            }

            secondTitle = "operations";// trOperations
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title =  subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));

            string reppath = reportclass.PathUp(System.IO.Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            //  getpuritemcount
            //paramarr.Add(new ReportParameter("totalBalance", tb_total.Text));
            //  OpenClosOperatinModel
            // openclosrow= opquery.ToList().Where(x => x.processType == "box").ToList();
            string posName = "";
            string branchName = "";
            if (FillCombo.branchsAllList is null)
            { await FillCombo.RefreshBranchsAll(); }
            if (FillCombo.posAllReport is null)
            { await FillCombo.RefreshPosAllReport(); }
            Pos postemp = FillCombo.posAllReport.Where(p => p.posId == (int)MainWindow.posID).FirstOrDefault();
            posName = postemp.name;

            branchName= FillCombo.branchsAllList.Where(p => p.branchId == (int)postemp.branchId).FirstOrDefault().name;
            List<CardsSts> cardList = new List<CardsSts>();
            cardList = cardtransList.Skip(1).ToList();
            clsReports.BoxStateReport(Boxquery.ToList(), rep, reppath, paramarr, cardList, txt_cashValue.Text,  branchName,  posName);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();
        }
        private async void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_window);

                #region
                //Window.GetWindow(this).Opacity = 0.2;
                this.Opacity = 0;
                string pdfpath = "";

                pdfpath = @"\Thumb\report\temp.pdf";
                pdfpath = reportclass.PathUp(System.IO.Directory.GetCurrentDirectory(), 2, pdfpath);

              await  BuildOperationReport((int)MainWindow.posID);

                LocalReportExtensions.ExportToPDF(rep, pdfpath);
                wd_previewPdf w = new wd_previewPdf();
                w.pdfPath = pdfpath;
                if (!string.IsNullOrEmpty(w.pdfPath))
                {
                    w.ShowDialog();
                    w.wb_pdfWebViewer.Dispose();
                }
                //Window.GetWindow(this).Opacity = 1;
                this.Opacity = 1;
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_window);
            }
            catch (Exception ex)
            {
                //Window.GetWindow(this).Opacity = 1;
                this.Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_window);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            //pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_window);

                #region

           await     BuildOperationReport((int)MainWindow.posID);
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }

                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_window);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_window);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async  void Btn_printInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_window);
                List<ItemTransferInvoice> query = new List<ItemTransferInvoice>();

                #region
            await    BuildOperationReport((int)MainWindow.posID);

                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_window);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_window);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
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

       void BuildBillDesign(List<CardsSts> cardsList)
        {
            //sv_billDetail.Children.Clear();
            #region Grid Container
            Grid gridContainer = new Grid();
            gridContainer.Margin = new Thickness(5);
            int rowCount = cardsList.Count;
            RowDefinition[] rd = new RowDefinition[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                rd[i] = new RowDefinition();
                rd[i].Height = new GridLength(1, GridUnitType.Auto);
            }
            for (int i = 0; i < rowCount; i++)
            {
                gridContainer.RowDefinitions.Add(rd[i]);
            }
            /////////////////////////////////////////////////////
            int colCount = 3;
            ColumnDefinition[] cd = new ColumnDefinition[colCount];
            for (int i = 0; i < colCount; i++)
            {
                cd[i] = new ColumnDefinition();
            }
            cd[0].Width = new GridLength(1, GridUnitType.Star);
            cd[1].Width = new GridLength(1, GridUnitType.Auto);
            cd[2].Width = new GridLength(1, GridUnitType.Auto);
            for (int i = 0; i < colCount; i++)
            {
                gridContainer.ColumnDefinitions.Add(cd[i]);
            }
            /////////////////////////////////////////////////////



            #endregion
            int index = 0;
            foreach (var item in cardsList)
            {
                #region   name
                var name = new TextBlock();
                name.Tag = "name-" + index;
                name.Text = item.name;
                name.Margin = new Thickness(5);
                name.FontSize = 14;
                name.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                name.FontWeight = FontWeights.Medium;

                Grid.SetRow(name, index);
                Grid.SetColumn(name, 0);
                gridContainer.Children.Add(name);
                #endregion

                #region   cash
                var cash = new TextBlock();
                cash.Tag = "cash-" + index;
                cash.Text = item.total.ToString();
                cash.Margin = new Thickness(5);
                cash.FontSize = 14;
                cash.HorizontalAlignment = HorizontalAlignment.Right;
                //cash.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                if (item.total > 0)
                    cash.Foreground = Application.Current.Resources["mediumGreen"] as SolidColorBrush;
                else
                    cash.Foreground = Application.Current.Resources["mediumRed"] as SolidColorBrush;

                Grid.SetRow(cash, index);
                Grid.SetColumn(cash, 1);
                gridContainer.Children.Add(cash);
                #endregion

                #region   currency
                var currency = new TextBlock();
                currency.Tag = "currency-" + index;
                currency.Text = AppSettings.Currency;
                currency.Margin = new Thickness(5);
                currency.FontSize = 14;
                currency.Foreground = Application.Current.Resources["MainColorGrey"] as SolidColorBrush;
                //currency.FontWeight = FontWeights.Medium;
                currency.HorizontalAlignment = HorizontalAlignment.Right;

                Grid.SetRow(currency, index);
                Grid.SetColumn(currency, 2);
                gridContainer.Children.Add(currency);
                #endregion

                index++;
            }

            sv_cards.Content = gridContainer;

        }

        private void Cb_pos_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = posLst.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
