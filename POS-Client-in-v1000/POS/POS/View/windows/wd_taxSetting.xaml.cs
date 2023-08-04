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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_taxSetting.xaml
    /// </summary>
    public partial class wd_taxSetting : Window
    {
        #region variables
        SetValues setValuesModel = new SetValues();
        SetValues setVInvoice = new SetValues(); SetValues setVInvoiceBool = new SetValues();
        SetValues setVItem = new SetValues(); SetValues setVItemBool = new SetValues();
        #endregion

        public wd_taxSetting() 
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region translate

                if (winLogIn.lang.Equals("en"))
                {
                    winLogIn.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    winLogIn.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                setVInvoiceBool = await SectionData.getSetValueBySetName("invoiceTax_bool");
                setVInvoice = await SectionData.getSetValueBySetName("invoiceTax_decimal");
                setVItemBool = await SectionData.getSetValueBySetName("itemsTax_bool");
                setVItem = await SectionData.getSetValueBySetName("itemsTax_decimal");

                if (setVInvoiceBool != null)
                    tgl_invoiceTax.IsChecked = Convert.ToBoolean(setVInvoiceBool.value);
                else
                    tgl_invoiceTax.IsChecked = false;
                if (setVInvoice != null)
                {
                    decimal d = decimal.Parse(setVInvoice.value);
                    tb_invoiceTax.Text = SectionData.PercentageDecTostring(d);
                }
                else
                    tb_invoiceTax.Text = "";
                if (setVItemBool != null)
                    tgl_itemsTax.IsChecked = Convert.ToBoolean(setVItemBool.value);
                else
                    tgl_itemsTax.IsChecked = false;
                if (setVItem != null)
                {
                    decimal d = decimal.Parse(setVItem.value);
                    tb_itemsTax.Text = SectionData.PercentageDecTostring(d);
                }
                else
                    tb_itemsTax.Text = "";

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
            txt_title.Text = MainWindow.resourcemanager.GetString("trTax");
            txt_invoiceTax.Text = MainWindow.resourcemanager.GetString("trInvoice");
            txt_itemsTax.Text = MainWindow.resourcemanager.GetString("trItems");
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (name == "TextBox")
                {
                    if ((sender as TextBox).Name == "tb_invoiceTax")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorInvoiceTax, tt_errorInvoiceTax, "trIsRequired");
                    else if ((sender as TextBox).Name == "tb_itemsTax")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorItemsTax, tt_errorItemsTax, "trIsRequired");
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chkTax(string name, bool isChk)
        {
            try
            {
                TextBox tb = new TextBox();
                if (name.Equals("tgl_invoiceTax"))
                    tb = tb_invoiceTax;

                else if (name.Equals("tgl_itemsTax"))
                    tb = tb_itemsTax;

                tb.IsEnabled = isChk;

                if (!isChk) tb.Text = "0";
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region events
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
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
                if ((sender as TextBox).Name == "tb_taxes" || (sender as TextBox).Name == "tb_price")
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
        private void Tb_decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
        private void Tgl_Checked(object sender, RoutedEventArgs e)
        {
            string name = ((ToggleButton)sender).Name;
            chkTax(name, true);
        }
        private void Tgl_Unchecked(object sender, RoutedEventArgs e)
        {
            string name = ((ToggleButton)sender).Name;
            chkTax(name, false);
        }
        #endregion

        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region validate
                if (tgl_invoiceTax.IsChecked == true)
                    SectionData.validateEmptyTextBox(tb_invoiceTax, p_errorInvoiceTax, tt_errorInvoiceTax, "trEmptyTax");
                else
                    SectionData.clearValidate(tb_invoiceTax, p_errorInvoiceTax);
                if (tgl_itemsTax.IsChecked == true)
                    SectionData.validateEmptyTextBox(tb_itemsTax, p_errorItemsTax, tt_errorItemsTax, "trEmptyTax");
                else
                    SectionData.clearValidate(tb_itemsTax, p_errorItemsTax);
                #endregion

                if ((!tb_invoiceTax.Text.Equals("")))
                {
                    if (setVInvoiceBool == null)
                        setVInvoiceBool = new SetValues();
                    //save bool invoice tax
                    setVInvoiceBool.value = tgl_invoiceTax.IsChecked.ToString();
                    setVInvoiceBool.isSystem = 1;
                    setVInvoiceBool.settingId = setVInvoiceBool.settingId;
                    int invoiceBoolRes = (int)await setValuesModel.Save(setVInvoiceBool);

                    if (setVInvoice == null)
                        setVInvoice = new SetValues();
                    //save invoice tax
                    string invTax = "0.000";
                    if (tgl_invoiceTax.IsChecked == true) { decimal d = decimal.Parse(tb_invoiceTax.Text);  invTax = String.Format("{0:0.000}", d); }
                    else invTax = "0.000";
                    setVInvoice.value = invTax;
                    setVInvoice.isSystem = 1;
                    setVInvoice.settingId = setVInvoice.settingId;
                    int invoiceRes = (int)await setValuesModel.Save(setVInvoice);

                    if (setVItemBool == null)
                        setVItemBool = new SetValues();
                    //save bool item tax
                    setVItemBool.value = tgl_itemsTax.IsChecked.ToString();
                    setVItemBool.isSystem = 1;
                    setVItemBool.settingId = setVItemBool.settingId;
                    int itemBoolRes = (int)await setValuesModel.Save(setVItemBool);

                    if (setVItem == null)
                        setVItem = new SetValues();
                    //save item tax
                    string itemTax = "0.000";
                    if (tgl_itemsTax.IsChecked == true) { decimal d = decimal.Parse(tb_itemsTax.Text); itemTax = String.Format("{0:0.000}", d); }
                    else itemTax = "0.000";
                    setVItem.value = itemTax;
                    setVItem.isSystem = 1;
                    setVItem.settingId = setVItem.settingId;
                    int itemRes = (int)await setValuesModel.Save(setVItem);

                    if ((invoiceBoolRes > 0) && (invoiceRes > 0) && (itemBoolRes > 0) )
                    {
                        await FillCombo.RefreshSettingsValues();
                        //update tax in main window
                        AppSettings.invoiceTax_bool = bool.Parse(setVInvoiceBool.value);
                        AppSettings.invoiceTax_decimal = decimal.Parse(setVInvoice.value);
                        AppSettings.itemsTax_bool = bool.Parse(setVItemBool.value);
                        AppSettings.itemsTax_decimal = decimal.Parse(setVItem.value);

                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        this.Close();
                    }
                    else
                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
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

    }
}
