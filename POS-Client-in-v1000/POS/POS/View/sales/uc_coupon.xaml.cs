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
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using POS.View.sales;
using POS.View.windows;

namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_coupon.xaml
    /// </summary>
    public partial class uc_coupon : UserControl
    {
        Coupon coupon = new Coupon();
        Coupon couponModel = new Coupon();
        private static uc_coupon _instance;
        BrushConverter bc = new BrushConverter();
        IEnumerable<Coupon> couponsQuery;
        IEnumerable<Coupon> coupons;
        byte tgl_couponState;
        string searchText = "";
        string basicsPermission = "coupon_basics";
        public static uc_coupon Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_coupon();
                return _instance;
            }
        }
        public uc_coupon()
        {
            //try
            //{
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

            //}
            //catch (Exception ex)
            //{
            //    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            //}
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {//cb_typeDiscount     tb_discountValue
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);

                #region fill discount type combo
                var dislist = new[] {
                    new { Text = MainWindow.resourcemanager.GetString("trValueDiscount"), Value = "1" },
                    new { Text = MainWindow.resourcemanager.GetString("trPercentageDiscount"), Value = "2" },
                     };
                cb_typeDiscount.DisplayMemberPath = "Text";
                cb_typeDiscount.SelectedValuePath = "Value";
                cb_typeDiscount.ItemsSource = dislist;
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
                Keyboard.Focus(tb_code);

                SectionData.clearValidate(tb_code, p_errorCode);

                img_barcode.Source = null;

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

                #region prevent editting on date and time
                //TextBox tbStartDate = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                //tbStartDate.IsReadOnly = true;
                //TextBox tbEndDate = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                //tbEndDate.IsReadOnly = true;
                #endregion
                if(coupons is null)
                await RefreshCouponsList();
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

        private void tb_discountValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
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
        #endregion
        private async void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                  if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (coupons is null)
                    await RefreshCouponsList();
                tgl_couponState = 1;
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
                if (coupons is null)
                    await RefreshCouponsList();
                tgl_couponState = 0;
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
        private void translate()
        {
            txt_couponHeader.Text = MainWindow.resourcemanager.GetString("trCoupon");
            txt_active.Text = MainWindow.resourcemanager.GetString("trActive");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            txt_baseInformation.Text = MainWindow.resourcemanager.GetString("trBaseInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_code, MainWindow.resourcemanager.GetString("trCodeHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_details, MainWindow.resourcemanager.GetString("trDetailsHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_barcode, MainWindow.resourcemanager.GetString("trBarcode"));
            txt_isActive.Text = MainWindow.resourcemanager.GetString("trActive");
            txt_details.Text = MainWindow.resourcemanager.GetString("trDetails");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_typeDiscount, MainWindow.resourcemanager.GetString("trTypeDiscountHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_discountValue, MainWindow.resourcemanager.GetString("trDiscountValueHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_startDate, MainWindow.resourcemanager.GetString("trStartDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp_endDate, MainWindow.resourcemanager.GetString("trEndDateHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_MinInvoiceValue, MainWindow.resourcemanager.GetString("trMinimumInvoiceValueHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_MaxInvoiceValue, MainWindow.resourcemanager.GetString("trMaximumInvoiceValueHint"));
            //MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_remainQuantity, MainWindow.resourcemanager.GetString("trRemainQuantityHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_quantity, MainWindow.resourcemanager.GetString("trQuantityHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_note, MainWindow.resourcemanager.GetString("trNoteHint"));

            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");

            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

            dg_coupon.Columns[0].Header = MainWindow.resourcemanager.GetString("trCode");
            dg_coupon.Columns[1].Header = MainWindow.resourcemanager.GetString("trName");
            dg_coupon.Columns[2].Header = MainWindow.resourcemanager.GetString("trValue");
            dg_coupon.Columns[3].Header = MainWindow.resourcemanager.GetString("trQTR");
            dg_coupon.Columns[4].Header = MainWindow.resourcemanager.GetString("trRemainQuantity");
            dg_coupon.Columns[5].Header = MainWindow.resourcemanager.GetString("trvalidity");
            tt_name.Content = MainWindow.resourcemanager.GetString("trName");
            tt_code.Content = MainWindow.resourcemanager.GetString("trCode");
            tt_barcode.Content = MainWindow.resourcemanager.GetString("trBarcode");
            tt_discountType.Content = MainWindow.resourcemanager.GetString("trDiscountType");
            tt_discountValue.Content = MainWindow.resourcemanager.GetString("trDiscountValue");
            tt_startDate.Content = MainWindow.resourcemanager.GetString("trStartDate");
            tt_endDate.Content = MainWindow.resourcemanager.GetString("trEndDate");
            tt_minInvoiceValue.Content = MainWindow.resourcemanager.GetString("trMinInvoiceValue");
            tt_maxInvoiceValue.Content = MainWindow.resourcemanager.GetString("trMaxInvoiceValue");
            tt_quantity.Content = MainWindow.resourcemanager.GetString("trQuantity");
            tt_search.Content = MainWindow.resourcemanager.GetString("trSearch");
            tt_notes.Content = MainWindow.resourcemanager.GetString("trNote");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

            txt_branchButton.Text = MainWindow.resourcemanager.GetString("trPrintBarcode");


        }


        private  string genBarCode(string code)
        {
            string s = "cop-" + code;
            return s;
         }

        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    #region validate
                    bool codeNotExist = await SectionData.CouponCodeNotExist(tb_code.Text, 0);

                    //chk empty code
                    SectionData.validateEmptyTextBox(tb_code, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk empty discount type
                    SectionData.validateEmptyComboBox(cb_typeDiscount, p_errorTypeDiscount, tt_errorTypeDiscount, "trEmptyDiscountTypeToolTip");
                    //chk empty discount value
                    SectionData.validateEmptyTextBox(tb_discountValue, p_errorValueDiscount, tt_errorValueDiscount, "trEmptyDiscountValueToolTip");

                    bool isEndDateSmaller = false;
                    if ((dp_startDate.SelectedDate != null) && (dp_endDate.SelectedDate != null))
                        if (dp_endDate.SelectedDate < dp_startDate.SelectedDate) isEndDateSmaller = true;

                    bool isMaxInvoiceValueSmaller = false;
                    try
                    {
                        if (!decimal.Parse(tb_MaxInvoiceValue.Text).Equals(0))
                            if (decimal.Parse(tb_MaxInvoiceValue.Text) < decimal.Parse(tb_MinInvoiceValue.Text)) isMaxInvoiceValueSmaller = true;
                    }
                    catch (Exception ex)
                    {
                        SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                    }
                    #endregion

                    if ((!tb_name.Text.Equals("")) && (!tb_code.Text.Equals("")) &&
                        (!cb_typeDiscount.Text.Equals("")) && (!tb_discountValue.Text.Equals(""))
                        )
                    {
                        if ((!codeNotExist) || (isEndDateSmaller) || (isMaxInvoiceValueSmaller))
                        {
                            if (!codeNotExist)
                                SectionData.showTextBoxValidate(tb_code, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");
                            if (isEndDateSmaller)
                            {
                                SectionData.showDatePickerValidate(dp_startDate, p_errorStartDate, tt_errorStartDate, "trErrorEndDateSmallerToolTip");
                                SectionData.showDatePickerValidate(dp_endDate, p_errorEndDate, tt_errorEndDate, "trErrorEndDateSmallerToolTip");
                            }
                            if (isMaxInvoiceValueSmaller)
                            {
                                SectionData.showTextBoxValidate(tb_MinInvoiceValue, p_errorMinInvoiceValue, tt_errorMinInvoiceValue, "trErrorMaxInvoiceSmallerToolTip");
                                SectionData.showTextBoxValidate(tb_MaxInvoiceValue, p_errorMaxInvoiceValue, tt_errorMaxInvoiceValue, "trErrorMaxInvoiceSmallerToolTip");
                            }
                        }
                        else
                        {
                            Coupon coupon = new Coupon();

                            coupon.code = tb_code.Text;
                            coupon.name = tb_name.Text;
                            coupon.details = tb_details.Text;
                            coupon.notes = tb_note.Text;
                            coupon.barcode = tb_barcode.Text;
                            coupon.isActive = Convert.ToByte(tgl_ActiveCoupon.IsChecked);
                            coupon.discountType = Convert.ToByte(cb_typeDiscount.SelectedValue);
                            if(dp_startDate.SelectedDate != null)
                                coupon.startDate = DateTime.Parse(dp_startDate.Text);
                            if (dp_endDate.SelectedDate != null)
                                coupon.endDate = DateTime.Parse(dp_endDate.Text);
                            coupon.discountValue = decimal.Parse(tb_discountValue.Text);
                            if (!tb_MinInvoiceValue.Text.Equals(""))
                                coupon.invMin = decimal.Parse(tb_MinInvoiceValue.Text);
                            else
                                coupon.invMin = 0;
                            if (!tb_MaxInvoiceValue.Text.Equals(""))
                                coupon.invMax = decimal.Parse(tb_MaxInvoiceValue.Text);
                            else
                                coupon.invMax = 0;
                            if (string.IsNullOrWhiteSpace(tb_quantity.Text))
                                coupon.quantity = 0;
                            else
                                coupon.quantity = Int32.Parse(tb_quantity.Text);
                            coupon.remainQ = coupon.quantity;
                            coupon.createUserId = MainWindow.userID; ;

                            int s = (int)await couponModel.save(coupon);

                            if (s>0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                Btn_clear_Click(null, null);

                                await RefreshCouponsList();
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

        private  void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_typeDiscount.IsEnabled = true;
                tb_discountValue.IsEnabled = true;

                tb_code.Clear();
                tb_name.Clear();
                tb_details.Clear();
                tb_barcode.Clear();
                tgl_ActiveCoupon.IsChecked = true;
                cb_typeDiscount.SelectedIndex = -1;
                tb_discountValue.Clear();
                dp_startDate.SelectedDate = null;
                dp_endDate.SelectedDate = null;
                tb_MinInvoiceValue.Clear();
                tb_MaxInvoiceValue.Clear();
                tb_quantity.Text = "0";
                //tb_remainQuantity.Clear();
                tb_note.Clear();
                img_barcode.Source = null;

                //tb_barcode.Visibility = Visibility.Collapsed;
                //img_barcode.Visibility = Visibility.Collapsed;

                SectionData.clearValidate(tb_name, p_errorName);
                SectionData.clearValidate(tb_code, p_errorCode);
                SectionData.clearValidate(tb_barcode, p_errorBarcode);
                SectionData.clearValidate(tb_MinInvoiceValue, p_errorMinInvoiceValue);
                SectionData.clearValidate(tb_MaxInvoiceValue, p_errorMaxInvoiceValue);
                //SectionData.clearValidate(tb_quantity, p_errorQuantity);
                SectionData.clearValidate(tb_discountValue, p_errorValueDiscount);
                SectionData.clearComboBoxValidate(cb_typeDiscount, p_errorTypeDiscount);
                TextBox tbStart = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                SectionData.clearValidate(tbStart, p_errorStartDate);
                TextBox tbEnd = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                SectionData.clearValidate(tbEnd, p_errorEndDate);

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
        {
            try
            {//refresh
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                   await RefreshCouponsList();
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

  


        void FN_ExportToExcel()
        {
            var QueryExcel = couponsQuery.AsEnumerable().Select(x => new
            {
                Name = x.name,
                Code = x.code,
                DisCountType = x.discountType,
                DisCountValue = x.discountValue,
                StartDate = x.startDate,
                EndDate = x.endDate,
                MinInvoiceValue = x.invMin,
                MaxInvoiceValue = x.invMax,
                Quantity = x.quantity,
                Notes = x.notes
            });
            var DTForExcel = QueryExcel.ToDataTable();
            DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trName");
            DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trCode");
            DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trDiscountType");
            DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trDiscountValue");
            DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trSartDate");
            DTForExcel.Columns[5].Caption = MainWindow.resourcemanager.GetString("trEndDate");
            DTForExcel.Columns[6].Caption = MainWindow.resourcemanager.GetString("trMinInvoice");
            DTForExcel.Columns[7].Caption = MainWindow.resourcemanager.GetString("trMaxInvoice");
            DTForExcel.Columns[8].Caption = MainWindow.resourcemanager.GetString("trQuantity");
            DTForExcel.Columns[9].Caption = MainWindow.resourcemanager.GetString("trNote");

            ExportToExcel.Export(DTForExcel);
        }


        private void Dg_coupon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.clearValidate(tb_name, p_errorName);
                SectionData.clearValidate(tb_code, p_errorCode);
                SectionData.clearValidate(tb_barcode, p_errorBarcode);
                SectionData.clearValidate(tb_MinInvoiceValue, p_errorMinInvoiceValue);
                SectionData.clearValidate(tb_MaxInvoiceValue, p_errorMaxInvoiceValue);
                SectionData.clearValidate(tb_discountValue, p_errorValueDiscount);
                SectionData.clearComboBoxValidate(cb_typeDiscount, p_errorTypeDiscount);
                TextBox tbStart = (TextBox)dp_startDate.Template.FindName("PART_TextBox", dp_startDate);
                SectionData.clearValidate(tbStart, p_errorStartDate);
                TextBox tbEnd = (TextBox)dp_endDate.Template.FindName("PART_TextBox", dp_endDate);
                SectionData.clearValidate(tbEnd, p_errorEndDate);
               
                if (dg_coupon.SelectedIndex != -1)
                {
                    coupon = dg_coupon.SelectedItem as Coupon;

                    this.DataContext = coupon;
                    if (coupon != null)
                    {

                        cb_typeDiscount.IsEnabled = false;
                        tb_discountValue.IsEnabled = false;

                        tb_discountValue.Text = SectionData.PercentageDecTostring(coupon.discountValue);
                        tb_MinInvoiceValue.Text = SectionData.DecTostring(coupon.invMin);
                        tb_MaxInvoiceValue.Text = SectionData.DecTostring(coupon.invMax);
                        tb_quantity.Text = coupon.quantity.ToString();
                        tgl_ActiveCoupon.IsChecked = Convert.ToBoolean(coupon.isActive);
                        cb_typeDiscount.SelectedValue = coupon.discountType;
                        //tb_discountValue.Text = (Convert.ToInt32(coupon.discountValue)).ToString();
                        tb_barcode.Visibility = Visibility.Visible;
                        img_barcode.Visibility = Visibility.Visible;
                        tb_barcode.Text = coupon.barcode;
                        drawBarcode(coupon.barcode);

                        #region delete
                        if (coupon.canDelete)
                        {
                            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                            txt_delete_Icon.Kind =
                                     MaterialDesignThemes.Wpf.PackIconKind.Delete;
                            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                        }

                        else
                        {
                            if (coupon.isActive == 0)
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
                    this.DataContext = coupon;
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

        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {//delete
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                    if (coupon.cId != 0)
                    {
                        if ((!coupon.canDelete) && (coupon.isActive == 0))
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxActivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                              await  activate();
                        }
                        else
                        {
                            #region
                            Window.GetWindow(this).Opacity = 0.2;
                            wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                            if (coupon.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!coupon.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion
                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (coupon.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!coupon.canDelete) && (coupon.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");

                                int b = (int)await couponModel.delete(coupon.cId, MainWindow.userID.Value, coupon.canDelete);

                                if (b > 0)
                                {
                                    coupon.cId = 0;
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopDelete"), animation: ToasterAnimation.FadeIn);
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            }
                        }

                        await RefreshCouponsList();
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

        private async Task activate()
        {//activate
            coupon.isActive = 1;

            int s = (int)await couponModel.save(coupon);

            if (s>0)
                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
            else
                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

            await RefreshCouponsList();
            Tb_search_TextChanged(null, null);

        }

        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {//update
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "update") || SectionData.isAdminPermision())
                {

                    if (coupon.cId > 0)
                    {
                        #region validate
                        bool codeNotExist = await SectionData.CouponCodeNotExist(tb_code.Text, coupon.cId);

                    //chk empty code
                    SectionData.validateEmptyTextBox(tb_code, p_errorCode, tt_errorCode, "trEmptyCodeToolTip");
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk empty discount type
                    SectionData.validateEmptyComboBox(cb_typeDiscount, p_errorTypeDiscount, tt_errorTypeDiscount, "trEmptyDiscountTypeToolTip");
                    //chk empty discount value
                    SectionData.validateEmptyTextBox(tb_discountValue, p_errorValueDiscount, tt_errorValueDiscount, "trEmptyDiscountValueToolTip");

                    bool isEndDateSmaller = false;
                    if((dp_startDate.SelectedDate != null)&&(dp_endDate.SelectedDate != null))
                        if (dp_endDate.SelectedDate < dp_startDate.SelectedDate) isEndDateSmaller = true;

                    bool isMaxInvoiceValueSmaller = false;
                        try
                        {
                            if (!decimal.Parse(tb_MaxInvoiceValue.Text).Equals(0))
                                if (decimal.Parse(tb_MaxInvoiceValue.Text) < decimal.Parse(tb_MinInvoiceValue.Text)) isMaxInvoiceValueSmaller = true;
                        }
                        catch (Exception ex)
                        {
                            SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                        }
                        #endregion

                        if ((!tb_name.Text.Equals("")) && (!tb_code.Text.Equals("")) &&
                        (!cb_typeDiscount.Text.Equals("")) && (!tb_discountValue.Text.Equals(""))
                        )
                    {
                        if ((!codeNotExist) || (isEndDateSmaller) || (isMaxInvoiceValueSmaller))
                        {
                            if (!codeNotExist)
                                SectionData.showTextBoxValidate(tb_code, p_errorCode, tt_errorCode, "trDuplicateCodeToolTip");
                            if (isEndDateSmaller)
                            {
                                SectionData.showDatePickerValidate(dp_startDate, p_errorStartDate, tt_errorStartDate, "trErrorEndDateSmallerToolTip");
                                SectionData.showDatePickerValidate(dp_endDate, p_errorEndDate, tt_errorEndDate, "trErrorEndDateSmallerToolTip");
                            }
                            if (isMaxInvoiceValueSmaller)
                            {
                                SectionData.showTextBoxValidate(tb_MinInvoiceValue, p_errorMinInvoiceValue, tt_errorMinInvoiceValue, "trErrorMaxInvoiceSmallerToolTip");
                                SectionData.showTextBoxValidate(tb_MaxInvoiceValue, p_errorMaxInvoiceValue, tt_errorMaxInvoiceValue, "trErrorMaxInvoiceSmallerToolTip");
                            }
                        }
                        else
                        {
                            coupon.code = tb_code.Text;
                            coupon.name = tb_name.Text;
                            coupon.details = tb_details.Text;
                            coupon.notes = tb_note.Text;
                            coupon.barcode = tb_barcode.Text;
                            coupon.isActive = Convert.ToByte(tgl_ActiveCoupon.IsChecked);
                            coupon.discountType = Convert.ToByte(cb_typeDiscount.SelectedValue);
                            if (!string.IsNullOrEmpty(dp_startDate.Text))
                                coupon.startDate = DateTime.Parse(dp_startDate.Text);
                            else
                                coupon.startDate = null;
                            if (!string.IsNullOrEmpty(dp_endDate.Text))
                                coupon.endDate = DateTime.Parse(dp_endDate.Text);
                            else
                                coupon.endDate = null;
                            coupon.discountValue = decimal.Parse(tb_discountValue.Text);
                            if (!tb_MinInvoiceValue.Text.Equals(""))
                                coupon.invMin = decimal.Parse(tb_MinInvoiceValue.Text);
                            else
                                coupon.invMin = 0;
                            if (!tb_MaxInvoiceValue.Text.Equals(""))
                                coupon.invMax = decimal.Parse(tb_MaxInvoiceValue.Text);
                            else
                                coupon.invMax = 0;

                            if (string.IsNullOrWhiteSpace(tb_quantity.Text))
                                coupon.quantity = 0;
                            else
                                coupon.quantity = Int32.Parse(tb_quantity.Text);

                            coupon.remainQ = coupon.quantity;
                            coupon.createUserId = MainWindow.userID;
                         
                            int s = (int)await couponModel.save(coupon);

                            if (s>0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);

                                await RefreshCouponsList();
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

        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    if (coupons is null)
                        await RefreshCouponsList();

                    searchText = tb_search.Text.ToLower();
                    couponsQuery = coupons.Where(s => (s.code.ToLower().Contains(searchText) ||
                    s.name.ToLower().Contains(searchText) ||
                    s.barcode.ToLower().Contains(searchText)
                    ) && s.isActive == tgl_couponState);

                    RefreshCouponView();
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


        async Task<IEnumerable<Coupon>> RefreshCouponsList()
        {
            coupons = await couponModel.Get();
            return coupons;
        }
        void RefreshCouponView()
        {
            dg_coupon.ItemsSource = couponsQuery;
            txt_count.Text = couponsQuery.Count().ToString();
           
        }

        private void Tb_preventSpaces(object sender, KeyEventArgs e)
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

        private void Tb_OnlyLatinAndDigits(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
                if (!regex.IsMatch(e.Text))
                    e.Handled = true;

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void drawBarcode(string barcodeStr)
        {//barcode image
            // create encoding object
            Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;

            if (barcodeStr != "")
            {
                System.Drawing.Bitmap serial_bitmap = (System.Drawing.Bitmap)barcode.Draw(barcodeStr, 60);
                System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
                //generate bitmap
                img_barcode.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(serial_bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            else
                img_barcode.Source = null;
        }

        private void Tb_quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //only int
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);



            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        private void Tb_Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                //decimal
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

        private  void Tb_code_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                string name = sender.GetType().Name;
                validateEmpty(name, sender);

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

        private  void Tb_code_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);
                if (tb_barcode.Text.Equals(""))
                {
                   
                    p_errorCode.Visibility = Visibility.Visible;
                    tt_errorCode.Content = MainWindow.resourcemanager.GetString("trEmptyCodeToolTip");
                    tb_code.Background = (Brush)bc.ConvertFrom("#15FF0000");
                }
                tb_code.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_errorCode.Visibility = Visibility.Collapsed;

                tb_barcode.Text = genBarCode(tb_code.Text);
                drawBarcode(tb_barcode.Text);

                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
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
                    //else if ((sender as TextBox).Name == "tb_MinInvoiceValue")
                    //    SectionData.validateEmptyTextBox((TextBox)sender, p_errorMinInvoiceValue, tt_errorMinInvoiceValue, "trEmptyMinInvoiceValueToolTip");
                    //else if ((sender as TextBox).Name == "tb_MaxInvoiceValue")
                    //    SectionData.validateEmptyTextBox((TextBox)sender, p_errorMaxInvoiceValue, tt_errorMaxInvoiceValue, "trEmptyMaxInvoiceValueToolTip");
                    //else if ((sender as TextBox).Name == "tb_quantity")
                    //    SectionData.validateEmptyTextBox((TextBox)sender, p_errorQuantity, tt_errorQuantity, "trEmptyQuantityToolTip");
                    else if ((sender as TextBox).Name == "tb_discountValue")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorValueDiscount, tt_errorValueDiscount, "trEmptyDiscountValueToolTip");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_typeDiscount")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorTypeDiscount, tt_errorTypeDiscount, "trEmptyDiscountTypeToolTip");
                }
                //else if (name == "DatePicker")
                //{
                //    if ((sender as DatePicker).Name == "dp_startDate")
                //        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorStartDate, tt_errorStartDate, "trEmptyStartDateToolTip");
                //    else if ((sender as DatePicker).Name == "dp_endDate")
                //        SectionData.validateEmptyDatePicker((DatePicker)sender, p_errorEndDate, tt_errorEndDate, "trEmptyEndDateToolTip");//
                //}

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
                var txb = sender as TextBox;
                if ((sender as TextBox).Name == "tb_MinInvoiceValue" || (sender as TextBox).Name == "tb_MaxInvoiceValue"
                    //|| (sender as TextBox).Name == "tb_quantity"
                    )
                    SectionData.InputJustNumber(ref txb);



            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #region report
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string addpath;
            string searchval = "";
            string stateval = "";
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Sale\Ar\CouponReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\Sale\En\CouponReport.rdlc";
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
            clsReports.couponReport(couponsQuery, rep, reppath, paramarr);
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
                        pdfPurCoupon();
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

        public async void pdfPurCoupon()
        {
            BuildReport();
            //this.Dispatcher.Invoke(() =>
            //{
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
            //});
        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    /////////////////////////////////////
                    //Thread t1 = new Thread(() =>
                    //{
                        printPurCoupon();
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

        public async void printPurCoupon()
        {
            BuildReport();
            this.Dispatcher.Invoke(() =>
            {
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
            });
        }
        private void Btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pieChart
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvcSales win = new win_lvcSales(couponsQuery, 1);
                    win.ShowDialog();
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
        public void buildbarcodereport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            bool isArabic = ReportCls.checkLang();

            addpath = @"\Reports\Sale\coupon\coupExport.rdlc";

            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);

            ReportCls.checkLang();

            clsReports.couponExportReport(rep, reppath, paramarr, tb_barcode.Text);



            rep.SetParameters(paramarr);

            rep.Refresh();



        }
        private void Btn_printBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (tb_barcode.Text != null && tb_barcode.Text != "")
            {
                buildbarcodereport();
                saveFileDialog.Filter = "PDF|*.pdf;";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filepath = saveFileDialog.FileName;
                    LocalReportExtensions.ExportToPDF(rep, filepath);
                }
            }
            else
            {
              Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trBarcodeEmpty"), animation: ToasterAnimation.FadeIn);
            }
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
        #endregion

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
    }
}
