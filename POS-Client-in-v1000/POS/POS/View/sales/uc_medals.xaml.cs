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

namespace POS.View.sales
{
    /// <summary>
    /// Interaction logic for uc_medals.xaml
    /// </summary>
    public partial class uc_medals : UserControl
    {

        Medal medalModel = new Medal();
        Medal medal = new Medal();
        IEnumerable<Medal> medalsQuery;
        IEnumerable<Medal> medals;
        byte tgl_medalState;
        string searchText = "";
        BrushConverter bc = new BrushConverter();
        string symbol = "";
        string basicsPermission = "medals_basics";
        string customersPermission = "medals_customers";
        private static uc_medals _instance;
        public static uc_medals Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_medals();
                return _instance;
            }
        }
        public uc_medals()
        {
            try
            {
                InitializeComponent();

                if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1440)
                {
                    txt_deleteButton.Visibility = Visibility.Visible;
                    txt_addButton.Visibility = Visibility.Visible;
                    txt_updateButton.Visibility = Visibility.Visible;
                    txt_add_Icon.Visibility = Visibility.Visible;
                    txt_update_Icon.Visibility = Visibility.Visible;
                    txt_delete_Icon.Visibility = Visibility.Visible;
                }
                else if (System.Windows.SystemParameters.PrimaryScreenWidth >= 1360)
                {
                    txt_add_Icon.Visibility = Visibility.Collapsed;
                    txt_update_Icon.Visibility = Visibility.Collapsed;
                    txt_delete_Icon.Visibility = Visibility.Collapsed;
                    txt_deleteButton.Visibility = Visibility.Visible;
                    txt_addButton.Visibility = Visibility.Visible;
                    txt_updateButton.Visibility = Visibility.Visible;
                }
                else
                {
                    txt_deleteButton.Visibility = Visibility.Collapsed;
                    txt_addButton.Visibility = Visibility.Collapsed;
                    txt_updateButton.Visibility = Visibility.Collapsed;
                    txt_add_Icon.Visibility = Visibility.Visible;
                    txt_update_Icon.Visibility = Visibility.Visible;
                    txt_delete_Icon.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex,this);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {//load
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                if (MainWindow.lang.Equals("en"))
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


                SectionData.clearValidate(tb_name, p_errorName);

                await RefreshMedalsList();
                Tb_search_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Keyboard.Focus(tb_name);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this);
            }

        }
        private void translate()
        {
            txt_medals.Text = MainWindow.resourcemanager.GetString("trMedals");
            txt_details.Text = MainWindow.resourcemanager.GetString("trDetails");
            txt_active.Text = MainWindow.resourcemanager.GetString("trActive");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_cashPointsRequired, MainWindow.resourcemanager.GetString("trCashPointsRequiredHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_invoiceCountPointsRequired, MainWindow.resourcemanager.GetString("trInvoiceCountPointsHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
            btn_customers.Content = MainWindow.resourcemanager.GetString("trCustomers");

            dg_medal.Columns[0].Header = MainWindow.resourcemanager.GetString("trName");
            dg_medal.Columns[1].Header = MainWindow.resourcemanager.GetString("trCashPointsRequired");
            dg_medal.Columns[2].Header = MainWindow.resourcemanager.GetString("trInvoiceCountPoints");
            dg_medal.Columns[3].Header = MainWindow.resourcemanager.GetString("trNote");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");

            tt_name.Content = MainWindow.resourcemanager.GetString("trName");
            tt_cashPointsRequired.Content = MainWindow.resourcemanager.GetString("trCashPointsRequired");
            tt_invoiceCountPointsRequired.Content = MainWindow.resourcemanager.GetString("trInvoiceCountPoints");
            tt_notes.Content = MainWindow.resourcemanager.GetString("trNote");
            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            tt_clear.Content = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {//clear
                tb_name.Clear();
                tb_cashPointsRequired.Clear();
                tb_invoiceCountPointsRequired.Clear();
                symbol = "";
                tb_notes.Clear();

                //clear validate
                SectionData.clearValidate(tb_name, p_errorName);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {//refresh
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshMedalsList();
                    Tb_search_TextChanged(null, null);
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
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {//search
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    if (medals is null)
                        await RefreshMedalsList();
                    searchText = tb_search.Text.ToLower();
                    medalsQuery = medals.Where(s => s.name.ToLower().Contains(searchText)
                    && s.isActive == tgl_medalState);

                    RefreshMedalView();
                }
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //pdf
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {

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
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //print
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {

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
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //export
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    this.Dispatcher.Invoke(() =>
                {
                    Thread t1 = new Thread(FN_ExportToExcel);
                    t1.SetApartmentState(ApartmentState.STA);
                    t1.Start();
                });

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
                SectionData.ExceptionMessage(ex, this);
            }

        }

        void FN_ExportToExcel()
        {
            var QueryExcel = medalsQuery.AsEnumerable().Select(x => new
            {
                Name = x.name,
                Notes = x.notes
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trName");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trNote");

            ExportToExcel.Export(DTForExcel);
        }


        private void Btn_customers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //customers
                if (MainWindow.groupObject.HasPermissionAction(customersPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    wd_customersList w = new wd_customersList();
                    w.medalId = medal.medalId;
                    w.ShowDialog();
                   
                    Window.GetWindow(this).Opacity = 1;
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
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //add
                 if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk not exist
                    string txt = tb_name.Text;
                    bool isExist = medalsQuery.Any(i => i.name == tb_name.Text);

                    if (isExist)
                    {
                        p_errorName.Visibility = Visibility.Visible;
                        tt_errorName.Content = MainWindow.resourcemanager.GetString("trDublicateMedal");
                        tb_name.Background = (Brush)bc.ConvertFrom("#15FF0000");
                    }

                    if ((!tb_name.Text.Equals("")) && !isExist)
                    {
                        int cashPoint = 0, invoiceCount = 0;
                        if (!tb_cashPointsRequired.Text.Equals(""))
                            cashPoint = Convert.ToInt32(tb_cashPointsRequired.Text);
                        if (!tb_invoiceCountPointsRequired.Text.Equals(""))
                            invoiceCount = Convert.ToInt32(tb_invoiceCountPointsRequired.Text);

                        Medal medal = new Medal();

                        medal.name = tb_name.Text;
                        medal.notes = tb_notes.Text;
                        //medal.symbol = path_symbol.Data.ToString();
                        medal.CashPointsRequired = cashPoint;
                        medal.invoiceCountPointsRequired = invoiceCount;
                        medal.createUserId = MainWindow.userID;
                        medal.isActive = 0;

                        string s = await medalModel.Save(medal);
                        if (!s.Equals("0"))
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            Btn_clear_Click(null, null);
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await RefreshMedalsList();
                        Tb_search_TextChanged(null, null);
                    }

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
                SectionData.ExceptionMessage(ex, this);
            }

        }


        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //update
                 if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk not exist
                    string txt = tb_name.Text;
                    bool isExist = medalsQuery.Any(i => i.name == tb_name.Text && i.medalId != medal.medalId);
                    if (isExist)
                    {
                        p_errorName.Visibility = Visibility.Visible;
                        tt_errorName.Content = MainWindow.resourcemanager.GetString("trDublicateMedal");
                        tb_name.Background = (Brush)bc.ConvertFrom("#15FF0000");
                    }
                    if ((!tb_name.Text.Equals("")) && !isExist)
                    {
                        int cashPoint = 0, invoiceCount = 0;
                        if (!tb_cashPointsRequired.Text.Equals(""))
                            cashPoint = Convert.ToInt32(tb_cashPointsRequired.Text);
                        if (!tb_invoiceCountPointsRequired.Text.Equals(""))
                            invoiceCount = Convert.ToInt32(tb_invoiceCountPointsRequired.Text);

                        medal.name = tb_name.Text;
                        medal.notes = tb_notes.Text;
                        //medal.symbol = path_symbol.Data.ToString();
                        medal.CashPointsRequired = cashPoint;
                        medal.invoiceCountPointsRequired = invoiceCount;
                        medal.createUserId = MainWindow.userID;
                        medal.isActive = 1;

                        string s = await medalModel.Save(medal);
                        //MessageBox.Show(s);
                        if (!s.Equals("0"))
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        await RefreshMedalsList();
                        Tb_search_TextChanged(null, null);

                    }
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
                SectionData.ExceptionMessage(ex, this);
            }

        }


        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //delete
                 if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (medal.medalId != 0)
                    {
                        if ((!medal.canDelete) && (medal.isActive == 0))
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                                await activate();
                        }
                        else
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            if (medal.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!medal.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                 string popupContent = "";
                                if (medal.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!medal.canDelete) && (medal.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                string b = await medalModel.deleteMedal(medal.medalId, MainWindow.userID.Value, medal.canDelete);
                                 if (b.Equals("medal is Deleted Successfully"))
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);

                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }

                        await RefreshMedalsList();
                        Tb_search_TextChanged(null, null);
                    }
                    //clear textBoxs
                    Btn_clear_Click(null, null);
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
                SectionData.ExceptionMessage(ex, this);
            }

        }


        private async Task activate()
        {//activate
            medal.isActive = 1;

            string s = await medalModel.Save(medal);

            if (!s.Equals("0"))
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefreshMedalsList();
            Tb_search_TextChanged(null, null);
        }

        private async void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //active
                 if (medals is null)
                    await RefreshMedalsList();
                tgl_medalState = 1;
                Tb_search_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private async void Tgl_isActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {//disactive
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (medals is null)
                    await RefreshMedalsList();
                tgl_medalState = 0;
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this);
            }

        }

        async Task<IEnumerable<Medal>> RefreshMedalsList()
        {
             
            medals = await medalModel.GetAll();
            return medals;
        }
        void RefreshMedalView()
        {
            dg_medal.ItemsSource = medalsQuery;
            txt_count.Text = medalsQuery.Count().ToString();
        }

        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_name")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "trEmptyNameToolTip");

                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Tb_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_cashPointsRequired")
                    SectionData.InputJustNumber(ref txb);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Dg_medal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                //selection
                SectionData.clearValidate(tb_name, p_errorName);

                if (dg_medal.SelectedIndex != -1)
                {
                    medal = dg_medal.SelectedItem as Medal;
                    this.DataContext = medal;

                    if (medal != null)
                    {
                        #region delete
                        if (medal.canDelete)
                        {
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                            txt_delete_Icon.Kind =
                                     MaterialDesignThemes.Wpf.PackIconKind.Delete;
                            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                        }

                        else
                        {
                            if (medal.isActive == 0)
                            {
                                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trActive");
                                txt_delete_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Check;
                                tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trActive");

                            }
                            else
                            {
                                txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trInActive");
                                txt_delete_Icon.Kind =
                                     MaterialDesignThemes.Wpf.PackIconKind.Cancel;
                                tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trInActive");

                            }
                        }
                        #endregion
                    }
                }

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this);
            }

        }

        private void Btn_symbol_Click(object sender, RoutedEventArgs e)
        {//symbol
            try
            {
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Tb_PreventSpaces(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Tb_discountValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this);
            }
        }

        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);


                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {

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
                SectionData.ExceptionMessage(ex, this);
            }


        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
    }
}
