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
using System.Resources;
using System.Reflection;
using POS.View.windows;
using System.IO;
using POS.View.sectionData.Charts;

namespace POS.View.accounts
{
    /// <summary>
    /// Interaction logic for uc_dailyClosing.xaml
    /// </summary>
    public partial class uc_dailyClosing : UserControl
    {
        #region variables
        BrushConverter bc = new BrushConverter();
        Pos posModel = new Pos();
        Pos pos = new Pos();
        IEnumerable<Pos> possQuery;
        IEnumerable<Pos> poss;
        bool tgl_posState;
        string searchText = "";
        string boxStatePermission = "dailyClosing_boxState";
        string transferPermission = "dailyClosing_transfer";
        string reportPermission = "dailyClosing_report";

        public string status = "";
        IEnumerable<OpenClosOperatinModel> Boxquery;
        bool isAdmin;
        //IEnumerable<CashTransfer> cashesQuery;
        //IEnumerable<CashTransfer> cashes;
        //CashTransfer cashModel = new CashTransfer();
        Statistics stsModel = new Statistics();
        List<CardsSts> cardtransList = new List<CardsSts>();
        #endregion

        public uc_dailyClosing()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private static uc_dailyClosing _instance;
        public static uc_dailyClosing Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_dailyClosing();
                return _instance;
            }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

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
                cb_pos.IsTextSearchEnabled = false;
                cb_pos.IsEditable = true;
                cb_pos.StaysOpenOnEdit = true;
                cb_pos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_pos.Text = "";
                #endregion

                isAdmin = SectionData.isAdminPermision();

                BuildBillDesign(cardtransList.Where(x => x.total > 0 || x.total < 0).ToList());

                await Search();

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
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(transferPermission, MainWindow.groupObjects, "one"))
                {
                    //if (cashesQuery.Count() > 0)
                       // Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCantDoProcess"), animation: ToasterAnimation.FadeIn);
                   // else
                    {
                        bool valid = validate();
                        if (valid)
                        {
                            var res = await transfer();
                            if (res >= 0)
                            {
                                if (pos.posId == MainWindow.posLogIn.posId)
                                {
                                    AppSettings.PosBalance = res;
                                    MainWindow.setBalance();
                                }
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                await fillPosInfo();

                            }
                            else if (res.Equals(-3))
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
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

        #region events
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
        private async void Btn_isClose_Click(object sender, RoutedEventArgs e)
        {//open-close
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(boxStatePermission, MainWindow.groupObjects, "one"))
                {
                    #region Accept
                    Window.GetWindow(this).Opacity = 0.2;
                    wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                    w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxConfirm");
                    w.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                    #endregion

                    if (w.isOk)
                    {
                        if (pos.boxState.Equals("c"))
                            status = "o";
                        else
                            status = "c";

                        await openCloseBox(status);
                        await fillPosInfo();
                        await fillCashquery();

                    }
                }

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Window.GetWindow(this).Opacity = 1;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region open - close - validate
        private async Task openCloseBox(string status)
        {
            CashTransfer cashTransfer = new CashTransfer();
            cashTransfer.processType = "box";
            cashTransfer.transType = status;
            cashTransfer.cash = pos.balance;
            cashTransfer.createUserId = MainWindow.userID.Value;
            cashTransfer.posId = pos.posId;
            if (status == "o")
                cashTransfer.transNum = await cashTransfer.generateCashNumber("bc");
            else
                cashTransfer.transNum = await cashTransfer.getLastOpenTransNum(pos.posId);
            int res = (int)await posModel.updateBoxState(pos.posId, status, Convert.ToInt32(isAdmin), MainWindow.userLogin.userId, cashTransfer);
            if (res > 0)
            {
                if (pos.posId == MainWindow.posLogIn.posId)
                    await MainWindow.refreshBalance();
                await RefreshPosList();
                await Search();
                pos.boxState = status;
                this.DataContext = null;
                this.DataContext = pos;
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
            cash1.cash = decimal.Parse(tb_cash.Text);
            cash1.createUserId = MainWindow.userID.Value;
            cash1.posIdCreator = pos.posId;
            cash1.isConfirm = 1;
            cash1.side = "p";//pos
            cash1.posId = pos.posId;

            CashTransfer cash2 = new CashTransfer();

            cash2.transType = "p";//pull
            cash2.transNum = "pp";
            cash2.cash = decimal.Parse(tb_cash.Text);
            cash2.createUserId = MainWindow.userID.Value;
            cash2.posIdCreator = pos.posId;
            cash2.isConfirm = 0;
            cash2.side = "p";//pos
            cash2.posId = Convert.ToInt32(cb_pos.SelectedValue);

            var res = await cash1.transferPosBalance(cash1, cash2);

            return res;

        }
        private Boolean validate()
        {
            SectionData.validateEmptyTextBox(tb_cash, p_errorCash, tt_errorCash, "trEmptyCashToolTip");
            SectionData.validateEmptyComboBox(cb_pos, p_errorPos, tt_errorPos, "trErrorEmptyPosToolTip");

            if (cb_pos.SelectedIndex == -1 || tb_cash.Text.Equals(""))
                return false;
            return true;
        }
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await Search();

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
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshPosList();
                    await Search();
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        private async void Dg_pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.clearComboBoxValidate(cb_pos, p_errorPos);

                if (dg_pos.SelectedIndex != -1)
                {
                    pos = dg_pos.SelectedItem as Pos;
                    this.DataContext = pos;
                    await fillPos();
                    await fillPosInfo();
                    await fillCashquery();
                }

                if (pos != null)
                {

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
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                Clear();

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
        private void Cb_pos_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = posLst.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
        }

        #endregion

        #region methods
        IEnumerable<Pos> posLst;
        private async Task fillPos()
        {
           
            //posLst = poss.Where(p => p.posId != MainWindow.posID && p.posId != pos.posId);
            posLst = poss.Where(p =>  p.posId != pos.posId);
            cb_pos.ItemsSource = posLst;
            cb_pos.DisplayMemberPath = "name";
            cb_pos.SelectedValuePath = "posId";
            cb_pos.SelectedIndex = -1;
        }

        private async Task fillCashquery()
        {
            List<CardsSts> tmpcard = new List<CardsSts>();
            CardsSts cardcashrow = new CardsSts();
            cardtransList = new List<CardsSts>(); 

            if (pos != null)
            {
                Boxquery = await stsModel.GetTransfromOpen(pos.posId);
            tmpcard = await clsReports.calctotalCards(Boxquery);        
                // open cash
                cardcashrow = new CardsSts();
                cardcashrow = clsReports.BoxOpenCashCalc(Boxquery.ToList());
            cardcashrow.name = MainWindow.resourcemanager.GetString("trOpenCash");
                cardcashrow.cardId = -2;
                //add open cash row
                cardtransList.Add(cardcashrow);  
             // cash
             cardcashrow = new CardsSts();
            cardcashrow = clsReports.BoxCashCalc(Boxquery.ToList());
            cardcashrow.name = MainWindow.resourcemanager.GetString("trCash");
            //add cash row
            cardtransList.Add(cardcashrow);
            //add card list
            cardtransList.AddRange(tmpcard);
            }
            else
            {
                Boxquery = null;
            }
            #region
            
            BuildBillDesign(cardtransList.Where(x => x.total > 0 || x.total < 0||(x.cardId==-2 && pos.boxState=="o")).ToList());
            #endregion

        }
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trDailyClosing");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            txt_posNameTitle.Text = winLogIn.resourcemanager.GetString("trPosTooltip");
            txt_branchTitle.Text = winLogIn.resourcemanager.GetString("trBranch");
            txt_cash.Text = winLogIn.resourcemanager.GetString("trCash");
            txt_boxState.Text = winLogIn.resourcemanager.GetString("trBoxState");
            txt_cashBalance.Text = winLogIn.resourcemanager.GetString("trCashBalance");
            txt_transfer.Text = winLogIn.resourcemanager.GetString("trTransfer");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cash, MainWindow.resourcemanager.GetString("trCashHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, winLogIn.resourcemanager.GetString("trPosHint"));

            tt_pos.Content = winLogIn.resourcemanager.GetString("trPosTooltip");
            btn_save.Content = MainWindow.resourcemanager.GetString("trTransfer");


            dg_pos.Columns[0].Header = MainWindow.resourcemanager.GetString("trPosTooltip");
            dg_pos.Columns[1].Header = MainWindow.resourcemanager.GetString("trBranch");
            dg_pos.Columns[2].Header = MainWindow.resourcemanager.GetString("trCash");
            dg_pos.Columns[3].Header = MainWindow.resourcemanager.GetString("trBoxState");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");


            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
        }
        async Task Search()
        {
            if (poss is null)
                await RefreshPosList();

            searchText = tb_search.Text.ToLower();

            possQuery = poss.Where(s => (
            s.name.ToLower().Contains(searchText)
            )
            );
            RefreshPosView();
        }
        async Task<IEnumerable<Pos>> RefreshPosList()
        {

            poss = await posModel.GetByUserId(MainWindow.userLogin.userId);
            return poss;
        }
        void RefreshPosView()
        {
            dg_pos.ItemsSource = possQuery;
            txt_count.Text = possQuery.Count().ToString();
        }
        private async void Clear()
        {
            SectionData.clearComboBoxValidate(cb_pos, p_errorPos);

            dg_pos.SelectedItem = null;
            pos = dg_pos.SelectedItem as Pos;
            this.DataContext = pos;
            cb_pos.ItemsSource = null;
            await fillPosInfo();
            await fillCashquery();
        }
        private async Task fillPosInfo()
        {
            if (pos != null)
            {
                if (pos.posId == MainWindow.posLogIn.posId)
                    await MainWindow.refreshBalance();
                

                if (pos.balance != 0)
                {
                    tb_cash.Text = SectionData.DecTostring(pos.balance);
                }
                else
                {
                    tb_cash.Text = "0";
                }

                if (pos.boxState == "c")
                {
                    txt_balanceState.Text = MainWindow.resourcemanager.GetString("trUnavailable");
                    txt_stateValue.Text = MainWindow.resourcemanager.GetString("trClosed");
                    path_isClose.Data = App.Current.Resources["unlock"] as Geometry;
                    btn_save.IsEnabled = false;
                    cb_pos.IsEnabled = false;
                }
                else
                {
                    txt_balanceState.Text = MainWindow.resourcemanager.GetString("trAvailable");
                    txt_stateValue.Text = MainWindow.resourcemanager.GetString("trOpen");
                    path_isClose.Data = App.Current.Resources["lock"] as Geometry;
                    cb_pos.IsEnabled = true;
                    btn_save.IsEnabled = true;
                }

            }
            else
            {

                tb_cash.Text = "0";
                txt_balanceState.Text = MainWindow.resourcemanager.GetString("trUnavailable");
                txt_stateValue.Text = MainWindow.resourcemanager.GetString("trClosed");
                path_isClose.Data = App.Current.Resources["unlock"] as Geometry;
                btn_save.IsEnabled = false;
                cb_pos.IsEnabled = false;
            }
        }
        void BuildBillDesign(List<CardsSts> cardsList)
        {
           if(cardsList.Count == 0)
                sp_reportDaily.Visibility = Visibility.Collapsed;
           else
                sp_reportDaily.Visibility = Visibility.Visible;


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
        private void validateEmptyComboBox(ComboBox cb, System.Windows.Shapes.Path p_error, ToolTip tt_error, string tr)
        {
            if (cb.SelectedIndex == -1)
            {
                p_error.Visibility = Visibility.Visible;
                tt_error.Content = winLogIn.resourcemanager.GetString(tr);
                cb.Background = (Brush)bc.ConvertFrom("#15FF0000");
            }
            else
            {
                cb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_error.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region repots
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private void pdf()
        {
            BuildReport();


            saveFileDialog.Filter = "PDF|*.pdf;";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToPDF(rep, filepath);
            }

        }
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            string searchval = "";
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Account\Ar\ArClosing.rdlc";
            }
            else
            {
                addpath = @"\Reports\Account\En\EnClosing.rdlc";
            }
            //filter
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            // trDailyClosing
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);            
            // possQuery
            paramarr.Add(new ReportParameter("trTitle", MainWindow.resourcemanagerreport.GetString("trDailyClosing")));
            clsReports.posClosingReport(possQuery, rep, reppath, paramarr);
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
                if (MainWindow.groupObject.HasPermissionAction(reportPermission, MainWindow.groupObjects, "one"))
                {
                    pdf();
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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

                if (MainWindow.groupObject.HasPermissionAction(reportPermission, MainWindow.groupObjects, "one"))
                {
                    /////////////////////////////////////
                    //Thread t1 = new Thread(() =>
                    //{
                    print();
                    //});
                    //t1.Start();
                    //////////////////////////////////////
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvc win = new win_lvc(possQuery, 6);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(reportPermission, MainWindow.groupObjects, "one"))
                {
                    #region
                    excel();
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

                if (MainWindow.groupObject.HasPermissionAction(reportPermission, MainWindow.groupObjects, "one"))
                {
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
                    //////////////////////////////////////
                    #endregion
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);

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
        private void print()
        {
            BuildReport();


            LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));

        }
        private void excel()
        {
            BuildReport();

            saveFileDialog.Filter = "EXCEL|*.xls;";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filepath = saveFileDialog.FileName;
                LocalReportExtensions.ExportToExcel(rep, filepath);
            }



        }

        #endregion

        #region datagrid events
       

        private async Task BuildOperationReport(Pos selectedPos)
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
            Title = subTitle;
            paramarr.Add(new ReportParameter("trTitle", Title));
            string reppath = reportclass.PathUp(System.IO.Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            string posName = "";
            string branchName = "";
            posName = selectedPos.name;           
            branchName = selectedPos.branchName;
            List<CardsSts> cardList = new List<CardsSts>();
            cardList = cardtransList.Skip(1).ToList();
            clsReports.BoxStateReport(Boxquery.ToList(), rep, reppath, paramarr, cardList, selectedPos.balance.ToString(), branchName, posName);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
        private async void previewRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                //for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                //    if (vis is DataGridRow)
                //    {
                //Pos row = (Pos)dg_pos.SelectedItems[0];
                string pdfpath = "";
                            pdfpath = @"\Thumb\report\temp.pdf";
                            pdfpath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, pdfpath);

                        await BuildOperationReport(pos);
                        LocalReportExtensions.ExportToPDF(rep, pdfpath);
                            wd_previewPdf w = new wd_previewPdf();
                            w.pdfPath = pdfpath;
                            if (!string.IsNullOrEmpty(w.pdfPath))
                            {
                                w.ShowDialog();
                                w.wb_pdfWebViewer.Dispose();
                            }
                            Window.GetWindow(this).Opacity = 1;
                    //}

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

        private async void printRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                //for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                //    if (vis is DataGridRow)
                //    {
                //Pos row = (Pos)dg_pos.SelectedItems[0];
                await BuildOperationReport(pos);
                        LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
                //}

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

        private async void  pdfRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                Window.GetWindow(this).Opacity = 0.2;
                //for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                //    if (vis is DataGridRow)
                //    {
                //Pos row = (Pos)dg_pos.SelectedItems[0];
                await BuildOperationReport(pos);

                        saveFileDialog.Filter = "PDF|*.pdf;";

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string filepath = saveFileDialog.FileName;
                                LocalReportExtensions.ExportToPDF(rep, filepath);
                            }
                 //}
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
        #endregion

    }
}
