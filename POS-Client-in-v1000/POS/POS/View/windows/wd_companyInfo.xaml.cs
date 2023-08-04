using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Win32;
using System.Windows.Resources;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Text.RegularExpressions;
using netoaster;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls.Primitives;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_companyInfo.xaml
    /// </summary>
    public partial class wd_companyInfo : Window
    {
        #region variables
        IEnumerable<City> citynumofcountry;
        string imgFileName = "pic/no-image-icon-125x125.png";
        bool isImgPressed = false;
        string _selectedIcon;
        int? countryid;
        Boolean firstchange = false;
        Boolean firstchangefax = false;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        ImageBrush brush = new ImageBrush();
        BrushConverter bc = new BrushConverter();
        SetValues setVName = new SetValues(); SetValues setVAddress = new SetValues(); SetValues setVEmail = new SetValues();
        SetValues setVArabicName = new SetValues(); SetValues setVArabicAddress = new SetValues();
        SetValues setVMobile = new SetValues();
        SetValues setVPhone= new SetValues();
        SetValues setVFax = new SetValues();
        SetValues setVLogo = new SetValues();
        SetValues valueModel = new SetValues();
        //social     
        SetValues setVwebsite = new SetValues();
        SetValues setVcom_social = new SetValues();
        SetValues setVcom_social_icon = new SetValues();

        public bool isFirstTime = false;
        #endregion

        public wd_companyInfo()
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (FillCombo.countryCodesList is null)
                    await FillCombo.RefreshCountryCodes();

                await fillCountries();

                await fillCity();

                #region get settings Ids

                setVName = await SectionData.getSetValueBySetName("com_name");
                setVAddress = await SectionData.getSetValueBySetName("com_address");
                setVEmail = await SectionData.getSetValueBySetName("com_email");
                setVMobile = await SectionData.getSetValueBySetName("com_mobile");
                setVPhone = await SectionData.getSetValueBySetName("com_phone");
                setVFax = await SectionData.getSetValueBySetName("com_fax");
                setVLogo = await SectionData.getSetValueBySetName("com_logo");

                setVwebsite = await SectionData.getSetValueBySetName("com_website");
                setVcom_social = await SectionData.getSetValueBySetName("com_social");
                setVcom_social_icon = await SectionData.getSetValueBySetName("com_social_icon");

                setVLogo.value = AppSettings.logoImage;
                setVArabicName = await SectionData.getSetValueBySetName("com_name_ar");
                setVArabicAddress = await SectionData.getSetValueBySetName("com_address_ar");
                #endregion

                if (!isFirstTime)
                {
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

                    #region get values
                    //get company name
                    tb_name.Text = AppSettings.companyName;
                    //get arabic company name
                    tb_arName.Text = AppSettings.com_name_ar;
                    //get company address
                    tb_address.Text = AppSettings.Address;
                    //get arabic company address
                    tb_arAddress.Text = AppSettings.com_address_ar;
                    //get company email
                    tb_email.Text = AppSettings.Email;
                    //get company mobile
                    SectionData.getMobile(setVMobile.value, cb_areaMobile, tb_mobile);
                    if (tb_phone.Text.Equals(""))
                        cb_areaMobile.SelectedValue = AppSettings.countryId;
                    //get company phone
                    SectionData.getPhone(setVPhone.value, cb_areaPhone, cb_areaPhoneLocal, tb_phone);
                    if (tb_phone.Text.Equals(""))
                        cb_areaPhone.SelectedValue = AppSettings.countryId;
                    Cb_areaPhone_SelectionChanged(cb_areaPhone, null);
                    //get company fax
                    SectionData.getPhone(setVFax.value, cb_areaFax, cb_areaFaxLocal, tb_fax);
                    if (tb_fax.Text.Equals(""))
                        cb_areaFax.SelectedValue = AppSettings.countryId;
                    Cb_areaFax_SelectionChanged(cb_areaFax, null);

                    //social
                    tb_website.Text = AppSettings.com_website;
                    tb_socialMedia.Text = AppSettings.com_social;
                 
                    //get company logo
                    await getImg();

                    #endregion

                }

                if (!string.IsNullOrWhiteSpace(AppSettings.com_social_icon))
                    _selectedIcon = AppSettings.com_social_icon;
                else
                    _selectedIcon = "website";
                path_socialMediaIcon.Fill =  App.Current.Resources[_selectedIcon ] as SolidColorBrush;
                path_socialMediaIcon.Data =  App.Current.Resources[_selectedIcon + "Icon"] as Geometry;
                
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
            txt_companyInfo.Text = MainWindow.resourcemanager.GetString("trComInfo");

            txt_englishInfo.Text = MainWindow.resourcemanager.GetString("englishInformation");
            txt_ArabicInfo.Text = MainWindow.resourcemanager.GetString("arabicInformation");
            txt_OtherInfo.Text = MainWindow.resourcemanager.GetString("trAnotherInfomation");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_name, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_arName, MainWindow.resourcemanager.GetString("trArabicNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_address, MainWindow.resourcemanager.GetString("trAdressHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_arAddress, MainWindow.resourcemanager.GetString("trArabicAddressHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_mobile, MainWindow.resourcemanager.GetString("trMobileHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_phone, MainWindow.resourcemanager.GetString("trPhoneHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_email, MainWindow.resourcemanager.GetString("trEmailHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_fax, MainWindow.resourcemanager.GetString("trFaxHint"));
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private async Task fillCountries()
        {
            cb_areaPhone.ItemsSource = FillCombo.countryCodesList.ToList();
            cb_areaPhone.SelectedValuePath = "countryId";
            cb_areaPhone.DisplayMemberPath = "code";

            cb_areaMobile.ItemsSource = FillCombo.countryCodesList.ToList();
            cb_areaMobile.SelectedValuePath = "countryId";
            cb_areaMobile.DisplayMemberPath = "code";

            cb_areaFax.ItemsSource = FillCombo.countryCodesList.ToList();
            cb_areaFax.SelectedValuePath = "countryId";
            cb_areaFax.DisplayMemberPath = "code";

            cb_areaMobile.SelectedValue = AppSettings.countryId;
            cb_areaPhone.SelectedValue = AppSettings.countryId;
            Cb_areaPhone_SelectionChanged(cb_areaPhone, null);
            cb_areaFax.SelectedValue = AppSettings.countryId;
            Cb_areaFax_SelectionChanged(cb_areaFax, null);
        }
        private async Task fillCity()
        {
            if (FillCombo.citysList is null)
                await FillCombo.RefreshCitys();
        }
        private bool validateEmail(TextBox tb, System.Windows.Shapes.Path p_error, ToolTip tt_error)
        {
            bool isValid = true;
            if (!tb.Text.Equals(""))
            {
                if (!ValidatorExtensions.IsValid(tb.Text))
                {
                    p_error.Visibility = Visibility.Visible;
                    tt_error.Content = ("Email address is not valid");
                    tb.Background = (Brush)bc.ConvertFrom("#15FF0000");
                    isValid = false;
                }
                else
                {
                    p_error.Visibility = Visibility.Collapsed;
                    tb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    isValid = true;
                }
            }
            return isValid;
        }
        public bool validateEmptyTextBox(TextBox tb, System.Windows.Shapes.Path p_error, ToolTip tt_error, string tr)
        {
            bool isValid = true;
            if (tb.Text.Equals(""))
            {
                p_error.Visibility = Visibility.Visible;
                tt_error.Content = (tr);
                tb.Background = (Brush)bc.ConvertFrom("#15FF0000");
                isValid = false;
            }
            else
            {
                tb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_error.Visibility = Visibility.Collapsed;
            }
            return isValid;
        }
        private async Task getImg()
        {
            try
            {
                if (string.IsNullOrEmpty(setVLogo.value))
                {
                    SectionData.clearImg(img_customer);
                }
                else
                {
                    byte[] imageBuffer = await setVLogo.downloadImage(setVLogo.value); // read this as BLOB from your DB

                    var bitmapImage = new BitmapImage();
                    if (imageBuffer != null)
                    {
                        using (var memoryStream = new MemoryStream(imageBuffer))
                        {
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();
                        }

                        img_customer.Background = new ImageBrush(bitmapImage);
                        // configure trmporary path
                        // string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                        string dir = Directory.GetCurrentDirectory();
                        string tmpPath = System.IO.Path.Combine(dir, Global.TMPSettingFolder);
                        tmpPath = System.IO.Path.Combine(tmpPath, setVLogo.value);
                        openFileDialog.FileName = tmpPath;
                    }
                    else
                        SectionData.clearImg(img_customer);
                }
            }
            catch { }
        }

        #endregion

        #region events
        private void Tb_email_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if(isFirstTime)
                    validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                else
                    SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void tb_mobile_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //doesn't allow spaces into textbox
            e.Handled = e.Key == Key.Space;
        }
        private void tb_phone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private void tb_fax_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private void tb_email_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private   void Cb_areaPhone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (firstchange == true)
                {
                    if (cb_areaPhone.SelectedValue != null)
                    {
                        if (cb_areaPhone.SelectedIndex >= 0)
                            countryid = int.Parse(cb_areaPhone.SelectedValue.ToString());

                        //if (citynum != null)
                        if(FillCombo.citysList.Count != 0)
                        {
                            citynumofcountry = FillCombo.citysList.Where(b => b.countryId == countryid).OrderBy(b => b.cityCode).ToList();
                            cb_areaPhoneLocal.ItemsSource = citynumofcountry;
                            cb_areaPhoneLocal.SelectedValuePath = "cityId";
                            cb_areaPhoneLocal.DisplayMemberPath = "cityCode";
                            if (citynumofcountry.Count() > 0)
                            {

                                cb_areaPhoneLocal.Visibility = Visibility.Visible;
                                tb_phone.IsEnabled = false;
                            }
                            else
                            {
                                cb_areaPhoneLocal.Visibility = Visibility.Collapsed;
                                tb_phone.IsEnabled = true;
                            }
                        }

                    }
                }
                else
                {
                    firstchange = true;
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
        private void Cb_areaFax_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (firstchangefax == true)
                {
                    if (cb_areaFax.SelectedValue != null)
                    {
                        if (cb_areaFax.SelectedIndex >= 0)
                            countryid = int.Parse(cb_areaFax.SelectedValue.ToString());
                        //if (citynum != null)
                        if(FillCombo.citysList.Count != 0)
                        {
                            citynumofcountry = FillCombo.citysList.Where(b => b.countryId == countryid).OrderBy(b => b.cityCode).ToList();

                            cb_areaFaxLocal.ItemsSource = citynumofcountry;
                            cb_areaFaxLocal.SelectedValuePath = "cityId";
                            cb_areaFaxLocal.DisplayMemberPath = "cityCode";
                            if (citynumofcountry.Count() > 0)
                            {
                                cb_areaFaxLocal.Visibility = Visibility.Visible;
                                tb_fax.IsEnabled = false;
                            }
                            else
                            {
                                cb_areaFaxLocal.Visibility = Visibility.Collapsed;
                                tb_fax.IsEnabled = true;
                            }
                        }
                    }
                }
                else
                {
                    firstchangefax = true;
                }
            }
            catch(Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Img_customer_Click(object sender, RoutedEventArgs e)
        {//select image
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                isImgPressed = true;
                openFileDialog.Filter = "Images|*.png;*.jpg;*.bmp;*.jpeg;*.jfif";
                if (openFileDialog.ShowDialog() == true)
                {
                    brush.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Relative));
                    img_customer.Background = brush;
                    imgFileName = openFileDialog.FileName;
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
        private void Tb_email_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
            if (!regex.IsMatch(e.Text))
                e.Handled = true;
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
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
            }
            catch (Exception ex)
            { SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void validateEmpty(string name, object sender)
        {
            try
            {
                if (!isFirstTime)
                {
                    if (name == "TextBox")
                    {
                        if ((sender as TextBox).Name == "tb_name")
                            SectionData.validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "trEmptyNameToolTip");
                        if ((sender as TextBox).Name == "tb_arName")
                            SectionData.validateEmptyTextBox((TextBox)sender, p_errorArName, tt_errorArName, "trEmptyNameToolTip");
                        else if ((sender as TextBox).Name == "tb_mobile")
                            SectionData.validateEmptyTextBox((TextBox)sender, p_errorMobile, tt_errorMobile, "trEmptyMobileToolTip");
                    }
                }
                else
                {
                    if (name == "TextBox")
                    {
                        if ((sender as TextBox).Name == "tb_name")
                            validateEmptyTextBox((TextBox)sender, p_errorName, tt_errorName, "Name cann't be empty");
                        else if ((sender as TextBox).Name == "tb_mobile")
                            validateEmptyTextBox((TextBox)sender, p_errorMobile, tt_errorMobile, "Mobile number cann't be empty");
                    }
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
        private void Cb_areaFaxLocal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_areaFaxLocal.SelectedIndex != -1)
                    tb_fax.IsEnabled = true;
                else
                    tb_fax.IsEnabled = false;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_areaPhoneLocal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_areaPhoneLocal.SelectedIndex != -1)
                    tb_phone.IsEnabled = true;
                else
                    tb_phone.IsEnabled = false;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        #endregion
        
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                #region validate
                bool emailError = false;
                if (!isFirstTime)
                {
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "trEmptyNameToolTip");
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_arName, p_errorArName, tt_errorArName, "trEmptyNameToolTip");
                    //validate email
                    SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                    if (!tb_email.Text.Equals(""))
                        if (!ValidatorExtensions.IsValid(tb_email.Text))
                            emailError = true;
                    //chk empty mobile
                    //SectionData.validateEmptyTextBox(tb_mobile, p_errorMobile, tt_errorMobile, "trEmptyMobileToolTip");
                }
                else
                {
                    //chk empty name
                    validateEmptyTextBox(tb_name, p_errorName, tt_errorName, "Name can not be empty");
                    //validate email
                    validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                    if (!tb_email.Text.Equals(""))
                        if (!ValidatorExtensions.IsValid(tb_email.Text))
                            emailError = true;
                    //chk empty mobile
                    //validateEmptyTextBox(tb_mobile, p_errorMobile, tt_errorMobile, "Mobile number cann't be empty");
                }
                #endregion
                
                if ((!tb_name.Text.Equals("")) && (!tb_arName.Text.Equals("")) && !emailError)
                {
                    #region old save
                    ////save name
                    //if (!tb_name.Text.Equals("") && !tb_name.Text.Equals(AppSettings.companyName))
                    //{
                    //    setVName.value = tb_name.Text;
                    //    setVName.isSystem = 1;
                    //    setVName.isDefault = 1;
                    //    setVName.settingId = nameId;
                    //   int sName = (int)await valueModel.Save(setVName);
                    //    if (!sName.Equals(0))
                    //        AppSettings.companyName = tb_name.Text;
                    //}
                    ////save address
                    //if (!tb_address.Text.Equals(AppSettings.Address))
                    //{
                    //    setVAddress.value = tb_address.Text;
                    //    setVAddress.isSystem = 1;
                    //    setVAddress.isDefault = 1;
                    //    setVAddress.settingId = addressId;
                    //   int sAddress = (int)await valueModel.Save(setVAddress);
                    //    if (!sAddress.Equals(0))
                    //        AppSettings.Address = tb_address.Text;
                    //}
                    ////save email
                    //if ((!emailError) && !tb_email.Text.Equals(AppSettings.Email))
                    //{
                    //    setVEmail.value = tb_email.Text;
                    //    setVEmail.isSystem = 1;
                    //    setVEmail.settingId = emailId;
                    //    setVEmail.isDefault = 1;
                    //    int sEmail = (int)await valueModel.Save(setVEmail);

                    //    if (!sEmail.Equals(0))
                    //            AppSettings.Email = tb_email.Text;
                    //}
                    ////save mobile
                    //if(!tb_mobile.Text.Equals(AppSettings.Mobile))
                    //{
                    //    setVMobile.value = cb_areaMobile.Text + "-" + tb_mobile.Text;
                    //    setVMobile.isSystem = 1;
                    //    setVMobile.isDefault = 1;
                    //    setVMobile.settingId = mobileId;
                    //    int sMobile = (int)await valueModel.Save(setVMobile);
                    //    if (!sMobile.Equals(0))
                    //        AppSettings.Mobile = cb_areaMobile.Text +"-"+ tb_mobile.Text;
                    //}
                    ////save phone
                    //if (!tb_phone.Text.Equals(AppSettings.Phone))
                    //{
                    //    setVPhone.value = cb_areaPhone.Text + "-" + cb_areaPhoneLocal.Text + "-" + tb_phone.Text;
                    //    setVPhone.isSystem = 1;
                    //    setVPhone.isDefault = 1;
                    //    setVPhone.settingId = phoneId;
                    //   int sPhone = (int)await valueModel.Save(setVPhone);
                    //    if (!sPhone.Equals(0))
                    //        AppSettings.Phone = setVPhone.value;
                    //}
                    ////save fax
                    //if (!tb_fax.Text.Equals(AppSettings.Fax))
                    //{
                    //    setVFax.value = cb_areaFax.Text + "-" + cb_areaFaxLocal.Text + "-" + tb_fax.Text;
                    //    setVFax.isSystem = 1;
                    //    setVFax.isDefault = 1;
                    //    setVFax.settingId = faxId;
                    //   int sFax = (int)await valueModel.Save(setVFax);
                    //    if (!sFax.Equals(0))
                    //        AppSettings.Fax = setVFax.value;

                    //}
                    ////  save logo
                    //int sLogo =0;
                    //if (isImgPressed)
                    //{
                    //    isImgPressed = false;

                    //    setVLogo.value = sLogo.ToString();
                    //    setVLogo.isSystem = 1;
                    //    setVLogo.isDefault = 1;
                    //    setVLogo.settingId = logoId;
                    //    sLogo = (int)await valueModel.Save(setVLogo);
                    //    if (!sLogo.Equals(0))
                    //    {
                    //        AppSettings.logoImage = setVLogo.value;
                    //        string b = await setVLogo.uploadImage(imgFileName, Md5Encription.MD5Hash("Inc-m" + sLogo), sLogo);
                    //        setVLogo.value = b;
                    //        AppSettings.logoImage = b;
                    //        sLogo = (int)await valueModel.Save(setVLogo);
                    //        await valueModel.getImg(setVLogo.value);
                    //    }
                    //}

                    //if (!isFirstTime)
                    //{
                    //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                    //    await Task.Delay(1500);
                    //}
                    //this.Close();
                    #endregion

                    #region save list of set values
                    List<SetValues> vlst = new List<SetValues>();
                    # region name
                    if (!tb_name.Text.Equals("") && !tb_name.Text.Equals(AppSettings.companyName))
                    {
                        setVName.value = tb_name.Text;
                        setVName.isSystem = 1;
                        setVName.isDefault = 1;
                        setVName.settingId = setVName.settingId;

                        vlst.Add(setVName);
                    }
                    #endregion
                    #region arabic name
                    if (!tb_arName.Text.Equals("") && !tb_arName.Text.Equals(AppSettings.com_name_ar))
                    {
                        setVArabicName.value = tb_arName.Text;
                        setVArabicName.isSystem = 1;
                        setVArabicName.isDefault = 1;
                        setVArabicName.settingId = setVArabicName.settingId;

                        vlst.Add(setVArabicName);
                    }
                    #endregion
                    #region address
                    if (!tb_address.Text.Equals(AppSettings.Address))
                    {
                        setVAddress.value = tb_address.Text;
                        setVAddress.isSystem = 1;
                        setVAddress.isDefault = 1;
                        setVAddress.settingId = setVAddress.settingId;

                        vlst.Add(setVAddress);
                    }
                    #endregion
                    #region arabic address
                    if (!tb_arAddress.Text.Equals(AppSettings.com_address_ar))
                    {
                        setVArabicAddress.value = tb_arAddress.Text;
                        setVArabicAddress.isSystem = 1;
                        setVArabicAddress.isDefault = 1;
                        setVArabicAddress.settingId = setVArabicAddress.settingId;

                        vlst.Add(setVArabicAddress);
                    }
                    #endregion
                    #region email
                    if ((!emailError) && !tb_email.Text.Equals(AppSettings.Email))
                    {
                        setVEmail.value = tb_email.Text;
                        setVEmail.isSystem = 1;
                        setVEmail.settingId = setVEmail.settingId;
                        setVEmail.isDefault = 1;

                        vlst.Add(setVEmail);
                    }
                    #endregion
                    #region mobile
                    if (!tb_mobile.Text.Equals(AppSettings.Mobile))
                    {
                        setVMobile.value = cb_areaMobile.Text + "-" + tb_mobile.Text;
                        setVMobile.isSystem = 1;
                        setVMobile.isDefault = 1;
                        setVMobile.settingId = setVMobile.settingId;

                        vlst.Add(setVMobile);
                    }
                    #endregion
                    #region phone
                    if (!tb_phone.Text.Equals(AppSettings.Phone))
                    {
                        setVPhone.value = cb_areaPhone.Text + "-" + cb_areaPhoneLocal.Text + "-" + tb_phone.Text;
                        setVPhone.isSystem = 1;
                        setVPhone.isDefault = 1;
                        setVPhone.settingId = setVPhone.settingId;

                        vlst.Add(setVPhone);
                    }
                    #endregion
                    #region fax
                    if (!tb_fax.Text.Equals(AppSettings.Fax))
                    {
                        setVFax.value = cb_areaFax.Text + "-" + cb_areaFaxLocal.Text + "-" + tb_fax.Text;
                        setVFax.isSystem = 1;
                        setVFax.isDefault = 1;
                        setVFax.settingId = setVFax.settingId;

                        vlst.Add(setVFax);
                    }
                    #endregion
                    #region logo
                    int sLogo = 0;
                    if (isImgPressed)
                    {

                        setVLogo.value = sLogo.ToString();
                        setVLogo.isSystem = 1;
                        setVLogo.isDefault = 1;
                        setVLogo.settingId = setVLogo.settingId;

                        vlst.Add(setVLogo);
                    }
                    #endregion
                    #region social
                 
                    //social
                    if (!tb_website.Text.Equals(AppSettings.com_website))
                    {
                        setVwebsite.value = tb_website.Text;
                        setVwebsite.isSystem = 1;
                      //  setVwebsite.settingId = setVEmail.settingId;
                        setVwebsite.isDefault = 1;

                        vlst.Add(setVwebsite);
                    }
                    if (!tb_socialMedia.Text.Equals(AppSettings.com_social))
                    {
                        setVcom_social.value = tb_socialMedia.Text;
                        setVcom_social.isSystem = 1;
                      //  setVcom_social.settingId = setVEmail.settingId;
                        setVcom_social.isDefault = 1;
                        vlst.Add(setVcom_social);
                    }

                    if (!_selectedIcon.Equals(AppSettings.com_social))
                    {
                        setVcom_social_icon.value = _selectedIcon;
                        setVcom_social_icon.isSystem = 1;
                        //  setVcom_social.settingId = setVEmail.settingId;
                        setVcom_social_icon.isDefault = 1;
                        vlst.Add(setVcom_social_icon);
                    }
                    #endregion
                    int res = (int)await valueModel.SaveList(vlst);
                    if (!res.Equals(0))
                    {
                        await FillCombo.RefreshSettingsValues();
                        AppSettings.companyName = tb_name.Text;
                        AppSettings.com_name_ar = tb_arName.Text;
                        AppSettings.Address = tb_address.Text;
                        AppSettings.com_address_ar = tb_arAddress.Text;
                        AppSettings.Email = tb_email.Text;
                        AppSettings.Mobile = cb_areaMobile.Text + "-" + tb_mobile.Text;
                        AppSettings.Phone = setVPhone.value;
                        AppSettings.Fax = setVFax.value;
                        AppSettings.logoImage = setVLogo.value;

                        AppSettings.com_website = setVwebsite.value;
                        AppSettings.com_social = setVcom_social.value;
                        AppSettings.com_social_icon = setVcom_social_icon.value;

                        if (isImgPressed)
                        {
                            string b = await setVLogo.uploadImage(imgFileName, Md5Encription.MD5Hash("Inc-m" + sLogo), setVLogo.valId);
                            setVLogo.value = b;
                            AppSettings.logoImage = b;
                            isImgPressed = false;

                        }
                        //await getImg();
                        //await valueModel.getImg(setVLogo.value);
                    }
                    if (!isFirstTime)
                    {
                        Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopSave"), animation: ToasterAnimation.FadeIn);
                        await Task.Delay(1500);
                    }
                    this.Close();
                    #endregion

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

        private void Btn_socialMediaIcon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_mainGrid);
                #region Accept
                this.Opacity = 0;
                wd_selectIcon w = new wd_selectIcon();
                w._selectedIcon = _selectedIcon;
                w.ShowDialog();
                this.Opacity = 1;
                #endregion
                if (w.isOk)
                {

                    // save icon
                    _selectedIcon =  w._selectedIcon;
                    path_socialMediaIcon.Fill = App.Current.Resources[_selectedIcon] as SolidColorBrush;
                    path_socialMediaIcon.Data = App.Current.Resources[_selectedIcon + "Icon"] as Geometry;
                }
                if (sender != null)
                    SectionData.EndAwait(grid_mainGrid);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_mainGrid);
                this.Opacity = 1;
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
