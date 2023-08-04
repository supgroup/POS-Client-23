using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_setupGeneralSetting.xaml
    /// </summary>
    public partial class wd_setupGeneralSetting : Window
    {
        public wd_setupGeneralSetting()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        List<SetValues> languages = new List<SetValues>();
        List<SettingCls> set = new List<SettingCls>();
        SettingCls setModel = new SettingCls();
        SetValues valueModel = new SetValues();
        CountryCode countryModel = new CountryCode();

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                set = await setModel.GetAll();

                translate();

                await fillLanguages();

                await fillRegions();

                await fillCurrencies();

                fillDateFormats();

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

        DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
        private void fillDateFormats()
        {
            var date = DateTime.Now;
            var typelist = new[] {
            new { Text =  date.ToString(dtfi.ShortDatePattern), Value = "ShortDatePattern" },
            new { Text =  date.ToString(dtfi.LongDatePattern) , Value = "LongDatePattern" },
            new { Text =  date.ToString(dtfi.MonthDayPattern) , Value = "MonthDayPattern" },
            new { Text =  date.ToString(dtfi.YearMonthPattern), Value = "YearMonthPattern" }
             };

            cb_dateForm.DisplayMemberPath = "Text";
            cb_dateForm.SelectedValuePath = "Value";
            cb_dateForm.ItemsSource = typelist;
        }

        private async Task fillCurrencies()
        {
            cb_currency.ItemsSource = await countryModel.GetAllRegion();
            cb_currency.DisplayMemberPath = "currency";
            cb_currency.SelectedValuePath = "countryId";
        }

        private async Task fillRegions()
        {
            cb_region.ItemsSource = await countryModel.GetAllRegion();
            cb_region.DisplayMemberPath = "name";
            cb_region.SelectedValuePath = "countryId";
        }


        private async Task fillLanguages()
        {
            languages = await valueModel.GetBySetName("language");
            foreach (var v in languages)
            {
                if (v.value == "en") v.value = "English";
                else if (v.value == "ar") v.value = "Arabic";

            }
            cb_language.ItemsSource = languages;
            cb_language.DisplayMemberPath = "value";
            cb_language.SelectedValuePath = "valId";

        }

        private void translate()
        {
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_language, "Language...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_region, "Region...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_currency, "Currency...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_tax, "Tax...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_dateForm, "Date Form...");
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception)
            {

            }
        }
        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            { //string name = sender.GetType().Name;
              //validateEmpty(name, sender);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {  //string name = sender.GetType().Name;
               //validateEmpty(name, sender);
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
                    if ((sender as TextBox).Name == "tb_tax")
                        SectionData.validateEmptyTextBox((TextBox)sender, p_errorTax, tt_errorTax, "trEmptyTax");
                }
                else if (name == "ComboBox")
                {
                    if ((sender as ComboBox).Name == "cb_language")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorLanguage, tt_errorLanguage, "trEmptyLanguage");
                    else if ((sender as ComboBox).Name == "cb_region")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorRegion, tt_errorRegion, "trEmptyRegion");
                    else if ((sender as ComboBox).Name == "cb_currency")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCurrency, tt_errorCurrency, "trEmptyCurrency");
                    else if ((sender as ComboBox).Name == "cb_dateForm")
                        SectionData.validateEmptyComboBox((ComboBox)sender, p_errorDateForm, tt_errorDateForm, "trEmptyDateFormat");

                }

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {//decimal
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
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (e.Key == Key.Return)
                {
                    Btn_save_Click(null, null);
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
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                 saveLanguage();
                await saveRegion();
                //saveCurrency();
                await saveTax();
                await saveDateform();

                //move to next pabe
                wd_companyInfo comInfo = new wd_companyInfo();
                comInfo.isFirstTime = true;
                this.Close();
                comInfo.ShowDialog();
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

        private async Task saveDateform()
        {
            if (!cb_dateForm.Text.Equals(""))
            {
                var setDate = set.Where(t => t.name == "dateForm").FirstOrDefault();
                SetValues dateForm = new SetValues();
                dateForm.value = cb_dateForm.SelectedValue.ToString();
                dateForm.isSystem = 1;
                dateForm.settingId = setDate.settingId;
                int s = (int)await valueModel.Save(dateForm);
                if (!s.Equals("0"))
                {
                    //update dateForm in main window
                    AppSettings.dateFormat = dateForm.value;
                }
            }
            else
            {
                AppSettings.dateFormat = "ShortDatePattern";
            }
        }

        private async Task saveTax()
        {
            if (!tb_tax.Text.Equals(""))
            {
                var setTax = set.Where(t => t.name == "tax").FirstOrDefault();
                SetValues tax = new SetValues();
                tax.value = tb_tax.Text;
                tax.isSystem = 1;
                tax.settingId = setTax.settingId;
               int s = (int)await valueModel.Save(tax);
                if (!s.Equals(0))
                {
                    //update tax in main window
                    MainWindow.tax = decimal.Parse(tax.value);
                }
            }
            else
            {
                MainWindow.tax = 0;
            }
        }

        private async Task saveCurrency()////////????????????????
        {
            // string s = "";
            int s = 0;
            if (!cb_currency.Text.Equals(""))
            {
                int currencyId = Convert.ToInt32(cb_currency.SelectedValue);
                if (currencyId != 0)
                {
                    s = (int)await countryModel.UpdateIsdefault(currencyId);//////?????
                    if (!s.Equals("0"))
                    {
                        //update currency in main window
                        List<CountryCode> c = await countryModel.GetAllRegion();
                        AppSettings.Currency = c.Where(r => r.countryId ==s).FirstOrDefault<CountryCode>().currency;
                    }
                }
            }
            else
            {
                AppSettings.Currency = "KWD";
            }

        }

        private async Task saveRegion()
        {
            int s = 0;
            //string s = "";
            List<CountryCode> c = await countryModel.GetAllRegion();

            if (!cb_region.Text.Equals(""))
            {
                int regionId = Convert.ToInt32(cb_region.SelectedValue);
                if (regionId != 0)
                {
                    s = (int)await countryModel.UpdateIsdefault(regionId);

                    if (!s.Equals("0"))
                    {
                        //update region and currency in main window
                        var Region = c.Where(r => r.countryId == s).FirstOrDefault<CountryCode>();
                        AppSettings.Currency = Region.currency;
                    }
                }
            }
            else
            {
               var Region = c.Where(r => r.name == "Kuwait").FirstOrDefault<CountryCode>();
                AppSettings.Currency = Region.currency; ;
            }
        }

        private  void saveLanguage()
        {
            if (!cb_language.Text.Equals(""))
            {
                winLogIn.lang = cb_language.SelectedValue.ToString();
                AppSettings.lang = cb_language.SelectedValue.ToString();
            }
            else
            {
                winLogIn.lang = "en";
                AppSettings.lang = "en";
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
        { //decimal
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

        private void Cb_region_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select country
            try
            {
                cb_currency.SelectedValue = cb_region.SelectedValue;
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
    }
}
