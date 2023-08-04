using MaterialDesignThemes.Wpf;
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
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System.IO;
using POS.View.sales;

namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_offer.xaml
    /// </summary>
    public partial class uc_offer : UserControl
    {

        #region variables
        Offer offer = new Offer();
        Offer offerModel = new Offer();
        ItemUnit itemUnitsModel = new ItemUnit();

        BrushConverter bc = new BrushConverter();
        IEnumerable<Offer> offersQuery;
        IEnumerable<Offer> offers;
        IEnumerable<ItemUnit> itemUnitsQuery;
        IEnumerable<ItemUnit> itemUnits;
        byte tgl_offerState;
        string searchText = "";
        string basicsPermission = "offer_basics";
        string itemsPermission = "offer_items";
        #endregion

        private static uc_offer _instance;
        public static uc_offer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_offer();
                return _instance;
            }
        }
        public uc_offer()
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
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region Numeric

        private decimal _numValue = 0;
        public decimal NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                tb_discountValue.Text = value.ToString();
            }
        }
        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NumValue++;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NumValue--;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       
        #endregion
      
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                #region fill discount type
                var dislist = new[] {
                    new { Text = MainWindow.resourcemanager.GetString("trValueDiscount"), Value = "1" },
                    new { Text = MainWindow.resourcemanager.GetString("trPercentageDiscount"), Value = "2" },
                     };

                cb_discountType.DisplayMemberPath = "Text";
                cb_discountType.SelectedValuePath = "Value";
                cb_discountType.ItemsSource = dislist;
                #endregion

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
                //tb_discountValue.Text = _numValue.ToString();

                btn_items.IsEnabled = false;


                SectionData.clearValidate(tb_code, p_errorCode);

                #region Style Date
                /////////////////////////////////////////////////////////////
                dp_startDate.Loaded += delegate
                {

                    var textBox1 = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_startDate.Background;
                        textBox1.BorderThickness = dp_startDate.BorderThickness;
                    }
                };
                /////////////////////////////////////////////////////////////
                dp_endDate.Loaded += delegate
                {

                    var textBox1 = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_endDate.Background;
                        textBox1.BorderThickness = dp_endDate.BorderThickness;
                    }
                };
                /////////////////////////////////////////////////////////////
                #endregion

                #region Style Time
                /////////////////////////////////////////////////////////////
                tp_startTime.Loaded += delegate
                {

                    var textBox1 = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                    if (textBox1 != null)
                    {
                        textBox1.Background = dp_startDate.Background;
                        textBox1.BorderThickness = dp_startDate.BorderThickness;
                    }
                };
                /////////////////////////////////////////////////////////////
                tp_endTime.Loaded += delegate
                {

                    var textBox1 = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                    if (textBox1 != null)
                    {
                        textBox1.Background = tp_endTime.Background;
                        textBox1.BorderThickness = tp_endTime.BorderThickness;
                    }
                };
                /////////////////////////////////////////////////////////////
                #endregion

                #region prevent editting on date and time
                TextBox tbStartDate = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                tbStartDate.IsReadOnly = true;
                TextBox tbEndDate = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                tbEndDate.IsReadOnly = true;

                TextBox tbStartTime = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                tbStartTime.IsReadOnly = true;
                TextBox tbEndTime = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                tbEndTime.IsReadOnly = true;
                #endregion

                if (offers is null)
                await RefreshOffersList();
                Tb_search_TextChanged(null, null);
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Keyboard.Focus(tb_code);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       
        #region methods
        async Task<IEnumerable<Offer>> RefreshOffersList()
        {
            offers = await offerModel.Get();
            return offers;
        }
        void RefreshOfferView()
        {
            dg_offer.ItemsSource = offersQuery;
            txt_count.Text = offersQuery.Count().ToString();
        }
        private async Task activate()
        {//activate
            offer.isActive = 1;

            int s = (int)await offerModel.save(offer);

            if (s > 0)
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefreshOffersList();
            Tb_search_TextChanged(null, null);

        }
        private void translate()
        {
            txt_active.Text = MainWindow.resourcemanager.GetString("trActive");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            ///////////////////////////////////////------OFFER------///////////////////////////////
            txt_offerHeader.Text = MainWindow.resourcemanager.GetString("trOffer");
            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_code, MainWindow.resourcemanager.GetString("trCodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            txt_isActive.Text = MainWindow.resourcemanager.GetString("trActive");
            txt_details.Text = MainWindow.resourcemanager.GetString("trDetails");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_discountType, MainWindow.resourcemanager.GetString("trTypeDiscountHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_discountValue, MainWindow.resourcemanager.GetString("trDiscountValueHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            TextBox tbStart = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tbStart, MainWindow.resourcemanager.GetString("trStartTimeHint"));
            TextBox tbEnd = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tbEnd, MainWindow.resourcemanager.GetString("trEndTimeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            dg_offer.Columns[0].Header = MainWindow.resourcemanager.GetString("trCode");
            dg_offer.Columns[1].Header = MainWindow.resourcemanager.GetString("trName");
            dg_offer.Columns[2].Header = MainWindow.resourcemanager.GetString("trValue");
            dg_offer.Columns[3].Header = MainWindow.resourcemanager.GetString("trStartDate");
            dg_offer.Columns[4].Header = MainWindow.resourcemanager.GetString("trEndDate");

            //tt_name.Content = MainWindow.resourcemanager.GetString("trName");
            //tt_code.Content = MainWindow.resourcemanager.GetString("trCode");
            //tt_discountType.Content = MainWindow.resourcemanager.GetString("trDiscountType");
            //tt_discountValue.Content = MainWindow.resourcemanager.GetString("trDiscountValue");
            //tt_startDate.Content = MainWindow.resourcemanager.GetString("trStartDate");
            //tt_endDate.Content = MainWindow.resourcemanager.GetString("trEndDate");
            tt_startTime.Content = MainWindow.resourcemanager.GetString("trStartTime");
            tt_endTime.Content = MainWindow.resourcemanager.GetString("trEndTime");
            //tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            //tt_notes.Content = MainWindow.resourcemanager.GetString("trNote");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

            txt_itemsButton.Text = MainWindow.resourcemanager.GetString("trItems");

        }

        #endregion

        #region events
        private async void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (offers is null)
                    await RefreshOffersList();
                tgl_offerState = 1;
                Tb_search_TextChanged(null, null);
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
        private async void Tgl_isActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (offers is null)
                    await RefreshOffersList();
                tgl_offerState = 0;
                Tb_search_TextChanged(null, null);
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
        private void tb_discountValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (tb_discountValue == null)
                //{
                //    return;
                //}
                //var txb = sender as TextBox;

                //if ((sender as TextBox).Name == "tb_discountValue")
                //    SectionData.InputJustNumber(ref txb);

                //if (!decimal.TryParse(tb_discountValue.Text, out _numValue))
                //    tb_discountValue.Text = _numValue.ToString();
                //else
                //    _numValue =decimal.Parse( tb_discountValue.Text);
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_discountValue")
                    SectionData.InputJustNumber(ref txb);

                if (tb_discountValue == null)
                {
                    return;
                }

                if (!decimal.TryParse(tb_discountValue.Text, out _numValue))
                    tb_discountValue.Text = _numValue.ToString();

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void tb_discountValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //try
            //{
            //    Regex regex = new Regex("[^0-9]+");
            //    e.Handled = regex.IsMatch(e.Text);

            //}
            //catch (Exception ex)
            //{
            //   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            //}
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;

            else
                e.Handled = true;
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {//refresh
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshOffersList();
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
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //search
                if (offers is null)
                    await RefreshOffersList();
                searchText = tb_search.Text.ToLower();
                offersQuery = offers.Where(s => (s.code.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText)
                ) && s.isActive == tgl_offerState);
                RefreshOfferView();
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
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //clear
                btn_items.IsEnabled = false;
                tb_code.Clear();
                tb_name.Clear();
                tgl_ActiveOffer.IsChecked = true;
                cb_discountType.SelectedIndex = -1;
                tb_discountValue.Clear();
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                tp_startTime.SelectedTime = null;
                tp_endTime.SelectedTime = null;
                tb_note.Clear();

                SectionData.clearValidate(tb_name, p_errorName);
                SectionData.clearValidate(tb_code, p_errorCode);
                SectionData.clearValidate(tb_discountValue, p_errorDiscountValue);
                SectionData.clearComboBoxValidate(cb_discountType, p_errorDiscountType);
                TextBox tbStartDate = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                SectionData.clearValidate(tbStartDate, p_errorStartDate);
                TextBox tbEndDate = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                SectionData.clearValidate(tbEndDate, p_errorEndDate);
                TextBox tbStartTime = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                SectionData.clearValidate(tbStartTime, p_errorStartTime);
                TextBox tbEndTime = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                SectionData.clearValidate(tbEndTime, p_errorEndTime);

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
        private void Tb_discountValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_code_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {//only english and digits
                Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
                if (!regex.IsMatch(e.Text))
                    e.Handled = true;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
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
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_code")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                    else if ((sender as TextBox).Name == "tb_name")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    else if ((sender as TextBox).Name == "tb_discountValue")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorDiscountValue, tt_errorDiscountValue, "trEmptyDiscountValueToolTip");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_discountType")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorDiscountType, tt_errorDiscountType, "trEmptyDiscountTypeToolTip");
                }
                else if (name == "DatePicker")
                {
                    if ((sender as DatePicker).Name == "dp_startDate")
                        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorStartDate, tt_errorStartDate, "trErrorEmptyStartDateToolTip");
                    else if ((sender as DatePicker).Name == "dp_endDate")
                        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorEndDate, tt_errorEndDate, "trErrorEmptyEndDateToolTip");
                }
                //else if (name == "TimePicker")
                //{
                //    if ((sender as TimePicker).Name == "tp_startTime")
                //    {
                //        TextBox tb = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                //        SectionData.validateEmptyTextBox(tb, p_errorStartTime, tt_errorStartTime, "trEmptyStartTimeToolTip");
                //    }
                //    else if ((sender as TimePicker).Name == "tp_endTime")
                //    {
                //        TextBox tb = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                //        SectionData.validateEmptyTextBox(tb, p_errorEndTime, tt_errorEndTime, "trEmptyEndTimeToolTip");
                //    }
                //}
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_offer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                
                SectionData.clearValidate(tb_name, p_errorName);
                SectionData.clearValidate(tb_code, p_errorCode);
                SectionData.clearValidate(tb_discountValue, p_errorDiscountValue);
                SectionData.clearComboBoxValidate(cb_discountType, p_errorDiscountType);
                TextBox tbStartDate = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                SectionData.clearValidate(tbStartDate, p_errorStartDate);
                TextBox tbEndDate = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                SectionData.clearValidate(tbEndDate, p_errorEndDate);
                TextBox tbStartTime = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                SectionData.clearValidate(tbStartTime, p_errorStartTime);
                TextBox tbEndTime = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                SectionData.clearValidate(tbEndTime, p_errorEndTime);

                if (dg_offer.SelectedIndex != -1)
                {
                    offer = dg_offer.SelectedItem as Offer;
                    this.DataContext = offer;

                    if (offer != null)
                    {
                        btn_items.IsEnabled = true;

                        tb_discountValue.Text = SectionData.PercentageDecTostring(offer.discountValue);
                        tgl_ActiveOffer.IsChecked = Convert.ToBoolean(offer.isActive);
                        cb_discountType.SelectedValue = offer.discountType;
                        //tb_discountValue.Text = (Convert.ToInt32(offer.discountValue)).ToString();
                        dp_startDate.Text = offer.startDate.Value.ToShortDateString();
                        dp_endDate.Text = offer.endDate.Value.ToShortDateString();
                        tp_startTime.Text = offer.startDate.Value.ToShortTimeString();
                        tp_endTime.Text = offer.endDate.Value.ToShortTimeString();

                        #region delete
                        if (offer.canDelete)
                        {
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                            txt_delete_Icon.Kind =
                                     MaterialDesignThemes.Wpf.PackIconKind.Delete;
                            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                        }

                        else
                        {
                            if (offer.isActive == 0)
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
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #region add-update-delete-items
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    #region validate
                    bool isCodeExist = await SectionData.isCodeExist(tb_code.Text, "", "Offer");//cb_discountType
                    //chk empty code
                    SectionData.validateEmptyTextBox(tb_code, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk empty discount type
                    SectionData.validateEmptyComboBox(cb_discountType, p_errorDiscountType, tt_errorDiscountType, "trEmptyDiscountTypeToolTip");
                    //chk empty discount value
                    SectionData.validateEmptyTextBox(tb_discountValue, p_errorDiscountValue, tt_errorDiscountValue, "trEmptyDiscountValueToolTip");
                    //chk empty start date
                    SectionData.validateEmptyDatePicker(dp_startDate, p_errorStartDate, tt_errorStartDate, "trEmptyStartDateToolTip");
                    //chk empty end date
                    SectionData.validateEmptyDatePicker(dp_endDate, p_errorEndDate, tt_errorEndDate, "trEmptyEndDateToolTip");
                    //chk empty start time
                    //TextBox tbStart = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                    //SectionData.validateEmptyTextBox(tbStart, p_errorStartTime, tt_errorStartTime, "trEmptyStartTimeToolTip");
                    //chk empty end time
                    //TextBox tbEnd = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                    //SectionData.validateEmptyTextBox(tbEnd, p_errorEndTime, tt_errorEndTime, "trEmptyEndTimeToolTip");

                    bool isEndDateSmaller = false;
                    if (dp_endDate.SelectedDate < dp_startDate.SelectedDate) isEndDateSmaller = true;

                    //bool isEndTimeSmaller = false;
                    //if (tp_endTime.SelectedTime < tp_startTime.SelectedTime) isEndTimeSmaller = true;
                    bool isGreaterThan100 = false;
                    if (cb_discountType.SelectedValue.ToString() == "2" && !tb_discountValue.Text.Equals("") && int.Parse(tb_discountValue.Text) > 100 )
                        isGreaterThan100 = true;
                    #endregion

                    if ((!tb_name.Text.Equals("")) && (!tb_code.Text.Equals("")) &&
                        (!cb_discountType.Text.Equals("")) && (!tb_discountValue.Text.Equals("")) &&
                        (dp_startDate.Text != null) && (dp_endDate.Text != null) 
                        //&&
                        //(tp_startTime.Text != null) && (tp_endTime.Text != null)
                        )
                    {
                        if ((isCodeExist) || (isEndDateSmaller) || isGreaterThan100)
                        {
                            if (isCodeExist)
                                SectionData.showTextBoxValidate(tb_code, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");

                            if (isEndDateSmaller)
                            {
                                SectionData.showDatePickerValidate(dp_startDate, p_errorStartDate, tt_errorStartDate, "trErrorEndDateSmallerToolTip");
                                SectionData.showDatePickerValidate(dp_endDate, p_errorEndDate, tt_errorEndDate, "trErrorEndDateSmallerToolTip");
                            }
                            //if (isEndTimeSmaller)
                            //{
                            //    SectionData.showTimePickerValidate(tp_startTime, p_errorStartTime, tt_errorStartTime, "trErrorEndTimeSmallerToolTip");
                            //    SectionData.showTimePickerValidate(tp_endTime, p_errorEndTime, tt_errorEndTime, "trErrorEndTimeSmallerToolTip");
                            //}
                            if (isGreaterThan100)
                                SectionData.showTextBoxValidate(tb_discountValue, p_errorDiscountValue, tt_errorDiscountValue, "greaterThan100");
                        }
                        else
                        {
                            string startDateStr = dp_startDate.SelectedDate.Value.ToShortDateString();
                            string startTimeStr = "00:00 AM";
                            if (tp_startTime.Text != null)
                                startTimeStr = tp_startTime.SelectedTime.Value.ToShortTimeString();
                            DateTime startDateTime = DateTime.Parse(startDateStr + " " + startTimeStr);

                            string endDateStr = dp_endDate.SelectedDate.Value.ToShortDateString();
                            string endTimeStr = "00:00 AM";
                            if (tp_endTime.Text != null)
                                endTimeStr = tp_endTime.SelectedTime.Value.ToShortTimeString();
                            DateTime endDateTime = DateTime.Parse(endDateStr + " " + endTimeStr);

                            Offer offer = new Offer();

                            offer.code = tb_code.Text;
                            offer.name = tb_name.Text;
                            offer.notes = tb_note.Text;
                            offer.isActive = Convert.ToByte(tgl_ActiveOffer.IsChecked);
                            offer.discountType = cb_discountType.SelectedValue.ToString();
                            offer.startDate = startDateTime;
                            offer.endDate = endDateTime;
                            offer.discountValue = decimal.Parse(tb_discountValue.Text);
                            offer.createUserId = MainWindow.userID; ;

                            int s = (int)await offerModel.save(offer);

                            if (s>0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                Btn_clear_Click(null, null);

                                await RefreshOffersList();
                                Tb_search_TextChanged(null, null);
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
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
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {

                    if (offer.offerId > 0)
                    {
                        #region validate
                        bool isCodeExist = await SectionData.isCodeExist(tb_code.Text, "", "Offer", offer.offerId);
                        //chk empty code
                        SectionData.validateEmptyTextBox(tb_code, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                        //chk empty name
                        SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                        //chk empty discount type
                        SectionData.validateEmptyComboBox(cb_discountType, p_errorDiscountType, tt_errorDiscountType, "trEmptyDiscountTypeToolTip");
                        //chk empty discount value
                        SectionData.validateEmptyTextBox(tb_discountValue, p_errorDiscountValue, tt_errorDiscountValue, "trEmptyDiscountValueToolTip");
                        //chk empty start date
                        SectionData.validateEmptyDatePicker(dp_startDate, p_errorStartDate, tt_errorStartDate, "trEmptyStartDateToolTip");
                        //chk empty end date
                        SectionData.validateEmptyDatePicker(dp_endDate, p_errorEndDate, tt_errorEndDate, "trEmptyEndDateToolTip");
                        //chk empty start time
                        //TextBox tbStart = (TextBox)tp_startTime.Template.FindName("PART_TextBox", tp_startTime);
                        //SectionData.validateEmptyTextBox(tbStart, p_errorStartTime, tt_errorStartTime, "trEmptyStartTimeToolTip");
                        //chk empty end time
                        //TextBox tbEnd = (TextBox)tp_endTime.Template.FindName("PART_TextBox", tp_endTime);
                        //SectionData.validateEmptyTextBox(tbEnd, p_errorEndTime, tt_errorEndTime, "trEmptyEndTimeToolTip");

                        bool isEndDateSmaller = false;
                        if (dp_endDate.SelectedDate < dp_startDate.SelectedDate) isEndDateSmaller = true;

                        //bool isEndTimeSmaller = false;
                        //if (tp_endTime.SelectedTime < tp_startTime.SelectedTime) isEndTimeSmaller = true;
                        bool isGreaterThan100 = false;
                        if (cb_discountType.SelectedValue.ToString() == "2" && !tb_discountValue.Text.Equals("") && int.Parse(tb_discountValue.Text) > 100)
                            isGreaterThan100 = true;
                        #endregion

                        if ((!tb_name.Text.Equals("")) && (!tb_code.Text.Equals("")) &&
                        (!cb_discountType.Text.Equals("")) && (!tb_discountValue.Text.Equals("")) &&
                        (dp_startDate.Text != null) && (dp_endDate.Text != null) &&
                        (tp_startTime.Text != null) && (tp_endTime.Text != null))
                    {
                        //if ((isCodeExist) || (isEndDateSmaller) || (isEndTimeSmaller))
                        if ((isCodeExist) || (isEndDateSmaller) || isGreaterThan100)
                        {
                            if (isCodeExist)
                                SectionData.showTextBoxValidate(tb_code, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");

                            if (isEndDateSmaller)
                            {
                                SectionData.showDatePickerValidate(dp_startDate, p_errorStartDate, tt_errorStartDate, "trErrorEndDateSmallerToolTip");
                                SectionData.showDatePickerValidate(dp_endDate, p_errorEndDate, tt_errorEndDate, "trErrorEndDateSmallerToolTip");
                            }
                                //if (isEndTimeSmaller)
                                //{
                                //    SectionData.showTimePickerValidate(tp_startTime, p_errorStartTime, tt_errorStartTime, "trErrorEndTimeSmallerToolTip");
                                //    SectionData.showTimePickerValidate(tp_endTime, p_errorEndTime, tt_errorEndTime, "trErrorEndTimeSmallerToolTip");
                                //}
                            if (isGreaterThan100)
                                SectionData.showTextBoxValidate(tb_discountValue, p_errorDiscountValue, tt_errorDiscountValue, "greaterThan100");
                        }
                        else
                        {
                            string startDateStr = dp_startDate.SelectedDate.Value.ToShortDateString();
                            string startTimeStr = "00:00 AM";
                            if (tp_startTime.Text != null)
                                startTimeStr = tp_startTime.SelectedTime.Value.ToShortTimeString();
                            DateTime startDateTime = DateTime.Parse(startDateStr + " " + startTimeStr);

                            string endDateStr = dp_endDate.SelectedDate.Value.ToShortDateString();
                            string endTimeStr = "00:00 AM";
                            if (tp_endTime.Text != null)
                                endTimeStr = tp_endTime.SelectedTime.Value.ToShortTimeString();
                            DateTime endDateTime = DateTime.Parse(endDateStr + " " + endTimeStr);

                            offer.code = tb_code.Text;
                            offer.name = tb_name.Text;
                            offer.notes = tb_note.Text;
                            offer.isActive = Convert.ToByte(tgl_ActiveOffer.IsChecked);
                            offer.discountType = cb_discountType.SelectedValue.ToString();
                            offer.startDate = startDateTime;
                            offer.endDate = endDateTime;
                            offer.discountValue = decimal.Parse(tb_discountValue.Text);
                            offer.createUserId = MainWindow.userID; ;

                            int s = (int)await offerModel.save(offer);

                            if (s>0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);

                                await RefreshOffersList();
                                Tb_search_TextChanged(null, null);
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
                    }

                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trSelectItemFirst"), animation: ToasterAnimation.FadeIn);

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
        private async void Btn_items_Click(object sender, RoutedEventArgs e)
        {
            try
            {//items
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(itemsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    SectionData.clearValidate(tb_code, p_errorCode);

                    itemUnits = await itemUnitsModel.Getall();


                    Window.GetWindow(this).Opacity = 0.2;

                    wd_itemsOfferList w = new wd_itemsOfferList();

                    w.offerId = offer.offerId;
                    w.ShowDialog();
                    if (w.isActive)
                    {

                    }

                    Window.GetWindow(this).Opacity = 1;
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
        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {//delete
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (offer.offerId != 0)
                    {
                        if ((!offer.canDelete) && (offer.isActive == 0))
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
                            if (offer.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!offer.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (offer.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!offer.canDelete) && (offer.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                int b = (int)await offerModel.delete(offer.offerId, MainWindow.userID.Value, offer.canDelete);

                                if (b > 0)
                                {
                                    offer.offerId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }

                        await RefreshOffersList();
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
                Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #region reports

    
        void FN_ExportToExcel()
        {
            var QueryExcel = offersQuery.AsEnumerable().Select(x => new
            {
                Name = x.name,
                Code = x.code,
                DisCountType = x.discountType,
                DisCountValue = x.discountValue,
                StartDate = x.startDate,
                EndDate = x.endDate,
                Notes = x.notes
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trName");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trCode");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trDiscountType");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trDiscountValue");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trSartDate");
            DTForExcel.Columns[5].Caption = MainWindow.resourcemanager.GetString("trEndDate");
            DTForExcel.Columns[6].Caption = MainWindow.resourcemanager.GetString("trNote");

            ExportToExcel.Export(DTForExcel);
        }
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Sale\Ar\OfferReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Sale\En\OfferReport.rdlc";
            }
            //filter   
            stateval = tgl_isActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
              : MainWindow.resourcemanagerreport.GetString("trNotActive");
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            clsReports.offerReport(offersQuery, rep, reppath, paramarr);
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

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    //Thread t1 = new Thread(() =>
                    //{
                        pdfPurInvoice();
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
        public  void pdfPurInvoice()
        {

            BuildReport();
            this.Dispatcher.Invoke(() =>
            {
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
            });
        }
        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                   
                    //Thread t1 = new Thread(() =>
                    //{
                        printPurInvoice();
                    //});
                    //t1.Start();
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
        public  void printPurInvoice()
        {
            BuildReport();
            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }
        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {//excel
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

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
                    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvcSales win = new win_lvcSales(offersQuery, 2);
                    win.ShowDialog();
                    Window.GetWindow(this).Opacity = 1;
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
        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {//preview
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
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
