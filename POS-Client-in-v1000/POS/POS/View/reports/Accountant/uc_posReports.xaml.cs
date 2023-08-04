using LiveCharts;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using POS.Classes;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
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
using static POS.Classes.Statistics;

namespace POS.View.reports
{
    public partial class uc_posReports : UserControl
    {
        #region variables
        Statistics statisticModel = new Statistics();
        List<CashTransferSts> list;
        List<CashTransfer> listCash;

        List<branchFromCombo> fromBranches = new List<branchFromCombo>();
        List<branchToCombo> toBranches = new List<branchToCombo>();

        List<posFromCombo> fromPos;
        List<posToCombo> toPos;

        IEnumerable<AccountantCombo> accCombo;
        #endregion

        private static uc_posReports _instance;
        public static uc_posReports Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_posReports();
                return _instance;
            }
        }
        public uc_posReports()
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

                #region key up
                //from branch
                cb_formBranch.IsTextSearchEnabled = false;
                cb_formBranch.IsEditable = true;
                cb_formBranch.StaysOpenOnEdit = true;
                cb_formBranch.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_formBranch.Text = "";
                //to branch
                cb_toBranch.IsTextSearchEnabled = false;
                cb_toBranch.IsEditable = true;
                cb_toBranch.StaysOpenOnEdit = true;
                cb_toBranch.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_toBranch.Text = "";
                //from pos
                cb_formPos.IsTextSearchEnabled = false;
                cb_formPos.IsEditable = true;
                cb_formPos.StaysOpenOnEdit = true;
                cb_formPos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_formPos.Text = "";
                //to pos
                cb_toPos.IsTextSearchEnabled = false;
                cb_toPos.IsEditable = true;
                cb_toPos.StaysOpenOnEdit = true;
                cb_toPos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_toPos.Text = "";
                //accountant
                cb_Accountant.IsTextSearchEnabled = false;
                cb_Accountant.IsEditable = true;
                cb_Accountant.StaysOpenOnEdit = true;
                cb_Accountant.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_Accountant.Text = "";
                #endregion

                col_reportChartWidth = col_reportChart.ActualWidth;

                tb_totalCurrency.Text = AppSettings.Currency;

                listCash = await statisticModel.GetBytypeAndSideForPos("all", "p");

                accCombo = listCash.GroupBy(g => g.updateUserAcc).Select(g => new AccountantCombo { Accountant = g.FirstOrDefault().updateUserAcc }).ToList();

                fillAccCombo();

                Btn_payments_Click(btn_payments, null);

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
            tt_payments.Content = MainWindow.resourcemanager.GetString("trDeposit");
            tt_pulls.Content = MainWindow.resourcemanager.GetString("trReceive");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_formBranch, MainWindow.resourcemanager.GetString("trFromBranch") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toBranch, MainWindow.resourcemanager.GetString("trToBranch") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_formPos, MainWindow.resourcemanager.GetString("trDepositor") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toPos, MainWindow.resourcemanager.GetString("trRecepient") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_Accountant, MainWindow.resourcemanager.GetString("trAccoutant") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_StartDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_EndDate, MainWindow.resourcemanager.GetString("trEndDateHint"));

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            chk_allFromBranch.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allToBranch.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allFromPos.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allToPos.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allAccountant.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_twoWay.Content = MainWindow.resourcemanager.GetString("trTwoWays");

            col_tansNum.Header = MainWindow.resourcemanager.GetString("trNo");
            col_creatorBranch.Header = MainWindow.resourcemanager.GetString("trCreator");
            col_fromBranch.Header = MainWindow.resourcemanager.GetString("trFromBranch");
            col_fromPos.Header = MainWindow.resourcemanager.GetString("trDepositor");
            col_toBranch.Header = MainWindow.resourcemanager.GetString("trToBranch");
            col_toPos.Header = MainWindow.resourcemanager.GetString("trRecepient");
            col_updateUserAcc.Header = MainWindow.resourcemanager.GetString("trAccoutant");
            col_updateDate.Header = MainWindow.resourcemanager.GetString("trDate");
            col_status.Header = MainWindow.resourcemanager.GetString("trStatus");
            col_cash.Header = MainWindow.resourcemanager.GetString("trAmount");

            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");

            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trHide");

            tt_print1.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_print2.Content = MainWindow.resourcemanager.GetString("trPrint");
        }
        List<CashTransfer> posLst;
        private List<CashTransfer> fillList()
        {
            var result = listCash
          .Where(s =>
          //start date
          (dp_StartDate.SelectedDate != null ? s.updateDate.Value.Date >= dp_StartDate.SelectedDate.Value.Date : true)
          &&
          //end date
          (dp_EndDate.SelectedDate != null ? s.updateDate.Value.Date <= dp_EndDate.SelectedDate.Value.Date : true)
          &&
          //fromBranch
          (cb_formBranch.SelectedIndex != -1 ? s.branchId == Convert.ToInt32(cb_formBranch.SelectedValue) : true)
          &&
          //toBranch
          (cb_toBranch.SelectedIndex != -1 ? s.branch2Id == Convert.ToInt32(cb_toBranch.SelectedValue) : true)
          &&
          //accountant
          (cb_Accountant.SelectedIndex != -1 ? s.updateUserAcc == cb_Accountant.SelectedValue.ToString() : true)
          &&
          //fromPos
          (cb_formPos.SelectedIndex != -1 ? s.posId == Convert.ToInt32(cb_formPos.SelectedValue) : true)
          &&
          //toPos
          (cb_toPos.SelectedIndex != -1 ? s.pos2Id == Convert.ToInt32(cb_toPos.SelectedValue) : true)
          // &&
          // //twoWay
          // (
          // chk_twoWay.IsChecked == true ?
          //     //fromPos
          //     (cb_formPos.SelectedIndex != -1 ? s.fromposId == Convert.ToInt32(cb_formPos.SelectedValue) || s.toposId == Convert.ToInt32(cb_formPos.SelectedValue) : true)
          //     &&
          //     //toPos
          //     (cb_toPos.SelectedIndex != -1 ? s.toposId == Convert.ToInt32(cb_toPos.SelectedValue) || s.fromposId == Convert.ToInt32(cb_toPos.SelectedValue) : true)
          //:
          //     //fromPos
          //     (cb_formPos.SelectedIndex != -1 ? s.fromposId == Convert.ToInt32(cb_formPos.SelectedValue) : true)
          //     &&
          //     //toPos
          //     (cb_toPos.SelectedIndex != -1 ? s.toposId == Convert.ToInt32(cb_toPos.SelectedValue) : true)
          // )
          && s.transType == _transtype
          );
            posLst = result.ToList();
            return result.ToList();
        }
        private void fillComboBranches()
        {
            cb_formBranch.SelectedValuePath = "BranchFromId";
            cb_formBranch.DisplayMemberPath = "BranchFromName";
            cb_formBranch.ItemsSource = fromBranches;

            cb_toBranch.SelectedValuePath = "BranchToId";
            cb_toBranch.DisplayMemberPath = "BranchToName";
            cb_toBranch.ItemsSource = toBranches;
        }
        //private void fillComboFromBranch(ComboBox cb)
        //{
        //    cb.SelectedValuePath = "BranchFromId";
        //    cb.DisplayMemberPath = "BranchFromName";
        //    cb.ItemsSource = fromBranches;
        //}

        //private void fillComboToBranch(ComboBox cb)
        //{
        //    cb.SelectedValuePath = "BranchToId";
        //    cb.DisplayMemberPath = "BranchToName";
        //    cb.ItemsSource = toBranches;
        //}
        List<posFromCombo> fromPosForKeyUp;
        List<posToCombo> toPosForKeyUp;
        private void fillComboFromPos()
        {
            cb_formPos.SelectedValuePath = "PosFromId";
            cb_formPos.DisplayMemberPath = "PosFromName";
            cb_formPos.ItemsSource = fromPos;
            if (cb_formBranch.SelectedItem != null)
            {
                var temp = cb_formBranch.SelectedItem as branchFromCombo;
                fromPosForKeyUp = fromPos.Where(x => x.BranchId == temp.BranchFromId).ToList();
                cb_formPos.ItemsSource = fromPosForKeyUp;
            }
        }
        private void fillComboToPos()
        {
            cb_toPos.SelectedValuePath = "PosToId";
            cb_toPos.DisplayMemberPath = "PosToName";
            cb_toPos.ItemsSource = toPos;
            if (cb_toBranch.SelectedItem != null)
            {
                var temp = cb_toBranch.SelectedItem as branchToCombo;
                toPosForKeyUp = toPos.Where(x => x.BranchId == temp.BranchToId).ToList();
                cb_toPos.ItemsSource = toPosForKeyUp;
            }
        }
        private void fillAccCombo()
        {
            cb_Accountant.SelectedValuePath = "Accountant";
            cb_Accountant.DisplayMemberPath = "Accountant";
            cb_Accountant.ItemsSource = accCombo;
        }
        IEnumerable<CashTransfer> temp = null;
        private void fillEvents()
        {
            temp = fillList();
            dgPayments.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            decimal total = 0;
            total = temp.Select(b => b.cash.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            fillColumnChart();
            //fillPieChart();
            fillRowChart();
        }
        private void fillEmptyEvents()
        {
            temp = new List<CashTransfer>();
            posLst = new List<CashTransfer>();
            dgPayments.ItemsSource = temp;
            txt_count.Text = temp.Count().ToString();
            decimal total = 0;
            total = temp.Select(b => b.cash.Value).Sum();
            tb_total.Text = SectionData.DecTostring(total);

            fillColumnChart();
            //fillPieChart();
            fillRowChart();
        }
        private void FillbyComboValue()
        {
            if ((cb_formBranch.SelectedItem == null && chk_allFromBranch.IsChecked == false)
                   || (cb_formPos.SelectedItem == null && chk_allFromPos.IsChecked == false)
                   || (cb_toBranch.SelectedItem == null && chk_allToBranch.IsChecked == false)
                   || (cb_toPos.SelectedItem == null && chk_allToPos.IsChecked == false)
                   || (cb_Accountant.SelectedItem == null && chk_allAccountant.IsChecked == false)
                   )
            {
                fillEmptyEvents();
            }
            else
            {

                fillEvents();
            }
        }
        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            changeSelection(sender);
        }
        private void changeSelection(object sender)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //  fillEvents();
                FillbyComboValue();
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
        public void paint()
        {
            path_payments.Fill = Brushes.White;
            path_pulls.Fill = Brushes.White;
        }

        #endregion

        #region events
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
        private void Cb_formBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_formBranch.SelectedItem != null)
                {
                    chk_allFromPos.IsEnabled = true;
                    chk_allFromPos.IsChecked = true;

                }
                else
                {
                    chk_allFromPos.IsEnabled = false;
                }
                fillComboFromPos();

                fillEvents();

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
        private void Chk_allFromBranch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_formBranch.IsEnabled = false;
                cb_formBranch.SelectedItem = null;
                cb_formBranch.Text = "";
                cb_formBranch.ItemsSource = fromBranches;
                cb_formPos.IsEnabled = false;
                cb_formPos.SelectedItem = null;
                chk_allFromPos.IsEnabled = false;
                chk_allFromPos.IsChecked = true;
                FillbyComboValue();



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
        private void Chk_allFromBranch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_formBranch.IsEnabled = true;
                cb_formPos.IsEnabled = false;
                chk_allFromPos.IsEnabled = false;
                fillEmptyEvents();
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
        private void Chk_allToBranch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_toBranch.IsEnabled = false;
                cb_toBranch.SelectedItem = null;
                cb_toBranch.Text = "";
                cb_toBranch.ItemsSource = toBranches;
                cb_toPos.IsEnabled = false;
                cb_toPos.SelectedItem = null;
                chk_allToPos.IsEnabled = false;
            
                chk_allToPos.IsChecked = true;
                FillbyComboValue();
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
        private void Chk_allToBranch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_toBranch.IsEnabled = true;

                cb_toPos.IsEnabled = false;
                chk_allToPos.IsEnabled = false;
                fillEmptyEvents();

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
        private void Cb_formPos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_formPos.SelectedItem != null)
                    chk_twoWay.IsEnabled = true;

                FillbyComboValue();

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
        private void Chk_allFromPos_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_formPos.IsEnabled = false;
                cb_formPos.SelectedItem = null;
                cb_formPos.Text = "";
                cb_formPos.ItemsSource = fromPos;
                try
                {
                    if (cb_toPos.SelectedItem == null)
                        chk_twoWay.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                FillbyComboValue();
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
        private void Chk_allFromPos_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_formPos.IsEnabled = true;
                fillEmptyEvents();
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
        private void Chk_allToPos_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_toPos.IsEnabled = false;
                cb_toPos.SelectedItem = null;
                if (cb_formPos.SelectedItem == null)
                    chk_twoWay.IsEnabled = false;
                cb_toPos.Text = "";
                cb_toPos.ItemsSource = toPos;
                FillbyComboValue();
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
        private void Chk_allToPos_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_toPos.IsEnabled = true;
                fillEmptyEvents();
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
        private void Cb_toBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (cb_toBranch.SelectedItem != null)
                {
                    chk_allToPos.IsEnabled = true;
                    chk_allToPos.IsChecked = true;
                }
                else
                {
                    chk_allToPos.IsEnabled = false;
                }
                fillComboToPos();

               // fillEvents();
                FillbyComboValue();
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
        private void Cb_toPos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_toPos.SelectedItem != null)
                    chk_twoWay.IsEnabled = true;

             //   fillEvents();
                FillbyComboValue();
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
        private void Dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            changeSelection(sender);
        }
        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeSelection(sender);
        }
        private void Chk_allAccountant_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_Accountant.IsEnabled = false;
                cb_Accountant.SelectedItem = null;
                cb_Accountant.Text = "";
                cb_Accountant.ItemsSource = accCombo;
                FillbyComboValue();
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
        private void Chk_allAccountant_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_Accountant.IsEnabled = true;
                fillEmptyEvents();
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
                listCash = await statisticModel.GetBytypeAndSideForPos("all", "p");

                cb_formBranch.SelectedItem = null;
                cb_toBranch.SelectedItem = null;
                cb_formPos.SelectedItem = null;
                cb_toPos.SelectedItem = null;
                cb_Accountant.SelectedItem = null;
                chk_allFromBranch.IsChecked = true;
                chk_allToBranch.IsChecked = true;
                //chk_allFromPos.IsChecked = true;
                //chk_allToPos.IsChecked = true;
                chk_allAccountant.IsChecked = true;
                chk_twoWay.IsChecked = false;
                dp_StartDate.SelectedDate = null;
                dp_EndDate.SelectedDate = null;
                txt_search.Text = "";

                fillEvents();

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
        private void Txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //dgPayments.ItemsSource = fillList()
                temp = temp
                                            .Where(obj => (
                                            obj.transNum.ToLower().Contains(txt_search.Text.ToLower()) ||
                                            obj.branchName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                            obj.branch2Name.ToLower().Contains(txt_search.Text.ToLower()) ||
                                            obj.posName.ToLower().Contains(txt_search.Text.ToLower()) ||
                                            obj.pos2Name.ToLower().Contains(txt_search.Text.ToLower()) ||
                                            obj.updateUserAcc.ToLower().Contains(txt_search.Text.ToLower())
                                            ));
                dgPayments.ItemsSource = temp;
                txt_count.Text = temp.Count().ToString();
               // var items = dgPayments.ItemsSource as IEnumerable<CashTransferSts>;
                decimal total = 0;
                total = temp.Select(b => b.cash.Value).Sum();
                tb_total.Text = SectionData.DecTostring(total);

            fillColumnChart();
            fillRowChart();

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
        private void Cb_formBranch_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = fromBranches.Where(p => p.BranchFromName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_formPos_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = fromPosForKeyUp.Where(p => p.PosFromName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_toBranch_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = toBranches.Where(p => p.BranchToName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_toPos_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = toPosForKeyUp.Where(p => p.PosToName.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        private void Cb_Accountant_KeyUp(object sender, KeyEventArgs e)
        {
            var combo = sender as ComboBox;
            var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
            combo.ItemsSource = accCombo.Where(p => p.Accountant.ToLower().Contains(tb.Text.ToLower())).ToList();
        }
        #endregion

        #region charts
        private void fillPieChart()
        {
            List<string> titles = new List<string>();
            List<int> resultList = new List<int>();
            titles.Clear();

            var temp = posLst;
            var result = temp
                .GroupBy(s => new { s.transType })
                .Select(s => new CashTransferSts
                {
                    processTypeCount = s.Count(),
                    processType = s.FirstOrDefault().transType,
                });
            resultList = result.Select(m => m.processTypeCount).ToList();
            titles = result.Select(m => m.processType).ToList();
            SeriesCollection piechartData = new SeriesCollection();
            for (int i = 0; i < resultList.Count(); i++)
            {
                List<int> final = new List<int>();
                List<string> lable = new List<string>();

                final.Add(resultList.Skip(i).FirstOrDefault());
                lable = titles;
                piechartData.Add(
                  new PieSeries
                  {
                      Values = final.AsChartValues(),
                      Title = lable.Skip(i).FirstOrDefault(),
                      DataLabels = true,
                  }
              );

            }
            chart1.Series = piechartData;
        }
        private void fillColumnChart()
        {
            axcolumn.Labels = new List<string>();
            List<string> names = new List<string>();

            //var temp = posLst;
            var tempColumn = temp;
            var res = tempColumn.GroupBy(x => new { x.posId }).Select(x => new CashTransfer
            {
                transType = _transtype,
                posId = x.FirstOrDefault().posId,
                posName = x.FirstOrDefault().posName + "/" + x.FirstOrDefault().branchName,
                cash = x.Sum(g => g.cash)
            });

            List<CashTransfer> result = new List<CashTransfer>();

            result.AddRange(res.ToList());

            var finalResult = result.GroupBy(x => new { x.posId }).Select(x => new CashTransferSts
            {
                transType = x.FirstOrDefault().transType,
                posId = x.FirstOrDefault().posId,
                posName = x.FirstOrDefault().posName,
                depositSum = x.Where(g => g.transType == _transtype).Sum(g => (decimal)g.cash),
            });
            var tempName = finalResult.Select(s => new
            {
                itemName = s.posName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));

            List<string> lable = new List<string>();
            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> cP = new List<decimal>();

            int xCount = 6;

            if (names.Count() <= 6)
                xCount = names.Count();

            for (int i = 0; i < xCount; i++)
            {
                cP.Add(finalResult.ToList().Skip(i).FirstOrDefault().depositSum);
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (names.Count() > 6)
            {
                decimal depositSum = 0;
                for (int i = 6; i < names.Count(); i++)
                {
                    depositSum = depositSum + finalResult.ToList().Skip(i).FirstOrDefault().depositSum;
                }
                if (!(depositSum == 0))
                {
                    cP.Add(depositSum);

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }

            string title = "";
            if (_transtype == "d") title = MainWindow.resourcemanager.GetString("trDeposit");
            else if (_transtype == "p") title = MainWindow.resourcemanager.GetString("trPull");

            columnChartData.Add(
            new StackedColumnSeries
            {
                Values = cP.AsChartValues(),
                DataLabels = true,
                Title = title
            });

            DataContext = this;
            cartesianChart.Series = columnChartData;
        }
        private void fillRowChart()
        {
            MyAxis.Labels = new List<string>();
            List<string> names = new List<string>();
            List<CashTransfer> resultList = new List<CashTransfer>();

            SeriesCollection rowChartData = new SeriesCollection();

            SeriesCollection columnChartData = new SeriesCollection();
            List<decimal> deposit = new List<decimal>();

            //var temp = posLst;
            var tempRow = temp;
            var res = tempRow.GroupBy(x => new { x.branchId }).Select(x => new CashTransfer
            {
                transType = _transtype,
                branchId = x.FirstOrDefault().branchId,
                branchName = x.FirstOrDefault().branchName,
                cash = x.Sum(g => g.cash)
            });

            List<CashTransfer> result = new List<CashTransfer>();

            result.AddRange(res.ToList());

            var finalResult = result.GroupBy(x => new { x.branchId }).Select(x => new CashTransferSts
            {
                transType = x.FirstOrDefault().transType,
                branchId = x.FirstOrDefault().branchId,
                branchName = x.FirstOrDefault().branchName,
                depositSum = x.Where(g => g.transType == _transtype).Sum(g => (decimal)g.cash),
            });
            var tempName = finalResult.Select(s => new
            {
                itemName = s.branchName,
            });
            names.AddRange(tempName.Select(nn => nn.itemName));

            int xCount = 6;

            if (names.Count() <= 6)
                xCount = names.Count();

            for (int i = 0; i < xCount; i++)
            {
                deposit.Add(finalResult.ToList().Skip(i).FirstOrDefault().depositSum);
                axcolumn.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            if (names.Count() > 6)
            {
                decimal depositSum = 0;
                for (int i = 6; i < names.Count(); i++)
                {
                    depositSum = depositSum + finalResult.ToList().Skip(i).FirstOrDefault().depositSum;
                }
                if (!(depositSum == 0))
                {
                    deposit.Add(depositSum);

                    axcolumn.Labels.Add(MainWindow.resourcemanager.GetString("trOthers"));
                }
            }
            for (int i = 0; i < deposit.Count(); i++)
            {
                MyAxis.Labels.Add(names.ToList().Skip(i).FirstOrDefault());
            }

            string title = "";
            if (_transtype == "d") title = MainWindow.resourcemanager.GetString("trDeposit");
            else if (_transtype == "p") title = MainWindow.resourcemanager.GetString("trPull");

            rowChartData.Add(
          new LineSeries
          {
              Values = deposit.AsChartValues(),
              Title = title
          });

            DataContext = this;
            rowChart.Series = rowChartData;
        }
        #endregion

        #region reports
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath = "";
            string firstTitle = "transfers";
            string secondTitle = "";
            string subTitle = "";
            string Title = "";
            string startDate = "";
            string endDate = "";
            string bankval = "";
            string userval = "";
            string Branch1val = "";
            string Branch2val = "";
            string Pos1val = "";
            string Pos2val = "";

            string Accountantval = "";

            //  string cardval = "";
            string searchval = "";


            List<string> invTypelist = new List<string>();
            bool isArabic = ReportCls.checkLang();
            string all = MainWindow.resourcemanagerreport.GetString("trAll");
         
            if (isArabic)
            {
                addpath = @"\Reports\StatisticReport\Accounts\Pos\Ar\ArPosAccReport.rdlc";

            }
            else
            {
                addpath = @"\Reports\StatisticReport\Accounts\Pos\En\PosAccReport.rdlc";
            }
            secondTitle = "pos";


            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();
            subTitle = clsReports.ReportTabTitle(firstTitle, secondTitle);
            Title = MainWindow.resourcemanagerreport.GetString("trAccounting") + " / " + subTitle;
            if (_transtype == "d")
            {
                Title = Title + "/" + MainWindow.resourcemanagerreport.GetString("trDeposits");

                paramarr.Add(new ReportParameter("trPos1Header", MainWindow.resourcemanagerreport.GetString("trDepositor")));
                paramarr.Add(new ReportParameter("trPos2Header", MainWindow.resourcemanagerreport.GetString("trRecepient")));

                paramarr.Add(new ReportParameter("trBranch1Header", MainWindow.resourcemanagerreport.GetString("trFromBranch")));
                paramarr.Add(new ReportParameter("trBranch2Header", MainWindow.resourcemanagerreport.GetString("trToBranch")));
            }
            else if (_transtype == "p")
            {

                Title = Title + "/" + MainWindow.resourcemanagerreport.GetString("trReceives");
                paramarr.Add(new ReportParameter("trPos1Header", MainWindow.resourcemanagerreport.GetString("trRecepient")));
                paramarr.Add(new ReportParameter("trPos2Header", MainWindow.resourcemanagerreport.GetString("trDepositor")));

                paramarr.Add(new ReportParameter("trBranch1Header", MainWindow.resourcemanagerreport.GetString("trToBranch")));
                paramarr.Add(new ReportParameter("trBranch2Header", MainWindow.resourcemanagerreport.GetString("trFromBranch")));

            }
            //filter
            startDate = dp_StartDate.SelectedDate != null ? SectionData.DateToString(dp_StartDate.SelectedDate) : "";

            endDate = dp_EndDate.SelectedDate != null ? SectionData.DateToString(dp_EndDate.SelectedDate) : "";
            //startTime = dt_startTime.SelectedTime != null ? dt_startTime.Text : "";
            //endTime = dt_endTime.SelectedTime != null ? dt_endTime.Text : "";
            Branch1val = cb_formBranch.SelectedItem != null
                && (chk_allFromBranch.IsChecked == false || chk_allFromBranch.IsChecked == null)
                ? cb_formBranch.Text : (chk_allFromBranch.IsChecked == true ? all : "");

            Pos1val = cb_formPos.SelectedItem != null
               && (chk_allFromPos.IsChecked == false || chk_allFromPos.IsChecked == null)
               && Branch1val != ""
               ? cb_formPos.Text : (chk_allFromPos.IsChecked == true && Branch1val != "" ? all : "");
            Branch2val = cb_toBranch.SelectedItem != null
               && (chk_allToBranch.IsChecked == false || chk_allToBranch.IsChecked == null)
               ? cb_toBranch.Text : (chk_allToBranch.IsChecked == true ? all : "");

            Pos2val = cb_toPos.SelectedItem != null
               && (chk_allToPos.IsChecked == false || chk_allToPos.IsChecked == null)
               && Branch2val != ""
               ? cb_toPos.Text : (chk_allToPos.IsChecked == true && Branch2val != "" ? all : "");

            Accountantval = cb_Accountant.SelectedItem != null
            && (chk_allAccountant.IsChecked == false || chk_allAccountant.IsChecked == null)
            && Branch1val != ""
            ? cb_Accountant.Text : (chk_allAccountant.IsChecked == true && Branch1val != "" ? all : "");
            paramarr.Add(new ReportParameter("Branch1val", Branch1val));
            paramarr.Add(new ReportParameter("Pos1val", Pos1val));
            paramarr.Add(new ReportParameter("Branch2val", Branch2val));
            paramarr.Add(new ReportParameter("Pos2val", Pos2val));
            paramarr.Add(new ReportParameter("AccountantVal", Accountantval));
            paramarr.Add(new ReportParameter("StartDateVal", startDate));
            paramarr.Add(new ReportParameter("EndDateVal", endDate));
          
            //paramarr.Add(new ReportParameter("trAccoutant", MainWindow.resourcemanagerreport.GetString("trAccoutant")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            paramarr.Add(new ReportParameter("trStartDate", MainWindow.resourcemanagerreport.GetString("trStartDate")));
            paramarr.Add(new ReportParameter("trEndDate", MainWindow.resourcemanagerreport.GetString("trEndDate")));
            searchval = txt_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter

            paramarr.Add(new ReportParameter("trTitle", Title));
            paramarr.Add(new ReportParameter("totalValue", tb_total.Text));
            //clsReports.cashTransferStsPos(temp, rep, reppath, paramarr);
            clsReports.posAccReportSTS(temp, rep, reppath, paramarr);
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

                #region
                //    Thread t1 = new Thread(() =>
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
                //    t1.Start();
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

        #region tabs
        private void Btn_payments_Click(object sender, RoutedEventArgs e)
        {//deposit
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_formBranch, MainWindow.resourcemanager.GetString("trFromBranch") + "...");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toBranch, MainWindow.resourcemanager.GetString("trToBranch") + "...");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_formPos, MainWindow.resourcemanager.GetString("trDepositor") + "...");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toPos, MainWindow.resourcemanager.GetString("trRecepient") + "...");

                col_fromBranch.Header = MainWindow.resourcemanager.GetString("trFromBranch");
                col_fromPos.Header = MainWindow.resourcemanager.GetString("trDepositor");
                col_toBranch.Header = MainWindow.resourcemanager.GetString("trToBranch");
                col_toPos.Header = MainWindow.resourcemanager.GetString("trRecepient");

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_payments);
                path_payments.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                txt_search.Text = "";

                _transtype = "d";

                dp_StartDate.SelectedDate = null;
                dp_EndDate.SelectedDate = null;
                chk_allFromBranch.IsChecked = true;
                chk_allFromPos.IsEnabled = false;
                chk_allToBranch.IsChecked = true;
                chk_allToPos.IsEnabled = false;
                chk_allAccountant.IsChecked = true;

            //    fillEvents();

                fromBranches = statisticModel.getFromCombo(posLst);
                toBranches = statisticModel.getToCombo(posLst);
                fromPos = statisticModel.getFromPosCombo(posLst);
                toPos = statisticModel.getToPosCombo(posLst);



                fillComboBranches();
                fillComboFromPos();
                fillComboToPos();


           

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
        string _transtype = "d";
        private void Btn_pulls_Click(object sender, RoutedEventArgs e)
        {//pull
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.ReportTabTitle(txt_tabTitle, this.Tag.ToString(), (sender as Button).Tag.ToString());

                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_formBranch, MainWindow.resourcemanager.GetString("trToBranch") + "...");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toBranch, MainWindow.resourcemanager.GetString("trFromBranch") + "...");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_formPos, MainWindow.resourcemanager.GetString("trRecepient") + "...");
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_toPos, MainWindow.resourcemanager.GetString("trDepositor") + "...");

                col_fromBranch.Header = MainWindow.resourcemanager.GetString("trToBranch");
                col_fromPos.Header = MainWindow.resourcemanager.GetString("trRecepient");
                col_toBranch.Header = MainWindow.resourcemanager.GetString("trFromBranch");
                col_toPos.Header = MainWindow.resourcemanager.GetString("trDepositor");

                paint();
                ReportsHelp.paintTabControlBorder(grid_tabControl, bdr_pulls);
                path_pulls.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4E4E4E"));

                txt_search.Text = "";

                _transtype = "p";

                dp_StartDate.SelectedDate = null;
                dp_EndDate.SelectedDate = null;
        

              
                chk_allFromBranch.IsChecked = true;
                chk_allFromPos.IsEnabled = false;
                chk_allToBranch.IsChecked = true;
                chk_allToPos.IsEnabled = false;
                chk_allAccountant.IsChecked = true;
 
             //   fillEvents();

                fromBranches = statisticModel.getFromCombo(posLst);
                toBranches = statisticModel.getToCombo(posLst);
                fromPos = statisticModel.getFromPosCombo(posLst);
                toPos = statisticModel.getToPosCombo(posLst);

                fillComboBranches();
                fillComboFromPos();
                fillComboToPos();


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
