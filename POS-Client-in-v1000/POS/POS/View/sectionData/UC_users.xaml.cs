using Microsoft.Win32;
using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using Microsoft.Reporting.WinForms;
using POS.View.windows;
using POS.View.sectionData.Charts;
using System.Windows.Interop;

namespace POS.View
{
    /// <summary>
    /// Interaction logic for UC_users.xaml
    /// </summary>
    public partial class UC_users : UserControl
    {
        CatigoriesAndItemsView catigoriesAndItemsView = new CatigoriesAndItemsView();

        User userModel = new User();

        User user = new User();

        IEnumerable<User> usersQuery;
        IEnumerable<User> users;
        byte tgl_userState;
        string searchText = "";

        BrushConverter bc = new BrushConverter();
        //phone variabels
        IEnumerable<CountryCode> countrynum;
        IEnumerable<City> citynum;
        IEnumerable<City> citynumofcountry;

        int? countryid;
        Boolean firstchange = false;
       
        CountryCode countrycodes = new CountryCode();
        City cityCodes = new City();

        OpenFileDialog openFileDialog = new OpenFileDialog();

        ImageBrush brush = new ImageBrush();

        int index = 0;

        string imgFileName = "pic/no-image-icon-125x125.png";
        bool isImgPressed = false;
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        string basicsPermission = "users_basics";
        string storesPermission = "users_stores";

        private static UC_users _instance;
        public static UC_users Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UC_users();
                return _instance;
            }
        }
        public UC_users()
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
            catch(Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        //area code methods
        async Task<IEnumerable<CountryCode>> RefreshCountry()
        {
            if (FillCombo.countryCodesList is null)
                await FillCombo.RefreshCountryCodes();
            countrynum = FillCombo.countryCodesList.ToList();
            //countrynum = await countrycodes.GetAllCountries();
            return countrynum;
        }
        private async Task fillCountries()
        {
           
                if (countrynum is null)
                    await RefreshCountry();

                cb_areaPhone.ItemsSource = countrynum.ToList();
                cb_areaPhone.SelectedValuePath = "countryId";
                cb_areaPhone.DisplayMemberPath = "code";

                cb_areaMobile.ItemsSource = countrynum.ToList();
                cb_areaMobile.SelectedValuePath = "countryId";
                cb_areaMobile.DisplayMemberPath = "code";

                cb_areaMobile.SelectedValue = AppSettings.countryId;
                Cb_areaMobile_SelectionChanged(cb_areaMobile, null);
                cb_areaPhone.SelectedValue = AppSettings.countryId;
                Cb_areaPhone_SelectionChanged(cb_areaPhone, null);

        }

        async Task<IEnumerable<City>> RefreshCity()
        {
            if (FillCombo.citysList is null)
                await FillCombo.RefreshCitys();
            citynum = FillCombo.citysList.ToList();
            //citynum = await cityCodes.Get();
            return citynum;

        }
        private async Task fillCity()
        {
                if (citynum is null)
                    await RefreshCity();
        }

        private async Task fillJobCombo()
        {
                if (users == null)
                users = await RefreshUsersList();
                usersQuery = users.Where(s => s.isActive == 1);
                List<User> userList = new List<User>();
                userList.AddRange(usersQuery.ToList());
                for (int i = 0; i < userList.Count(); i++)
                    if (!cb_job.Items.Contains(userList[i].job))
                        cb_job.Items.Add(userList[i].job);
        }

        private async Task<bool> chkIfUserNameIsExists(string username , int uId)
        {
            bool b = false;
                if (users == null) users = await RefreshUsersList();
                if (users.Any(i => i.username == username && i.userId != uId))
                    b = true;
                else b = false;
            return b;
        }
        private bool chkPasswordLength(string password)
        {
           bool b = false;
           
                if (password.Length < 6)
                    b = true;
           
            return b;
        }

        //end areacod
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
     
        private void translate()
        {
           
            txt_user.Text = MainWindow.resourcemanager.GetString("trUser");
            txt_activeSearch.Text = MainWindow.resourcemanager.GetString("trActive");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));
            txt_userInfomration.Text = MainWindow.resourcemanager.GetString("trUserInformation");
            txt_commission.Text = MainWindow.resourcemanager.GetString("commission");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_firstName, MainWindow.resourcemanager.GetString("trNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_lastName, MainWindow.resourcemanager.GetString("trLastNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_email, MainWindow.resourcemanager.GetString("trEmailHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_mobile, MainWindow.resourcemanager.GetString("trMobileHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_phone, MainWindow.resourcemanager.GetString("trPhoneHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_address, MainWindow.resourcemanager.GetString("trAdressHint"));
            
            txt_hasCommission.Text = MainWindow.resourcemanager.GetString("commissionOnInvoice");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_commissionValue, MainWindow.resourcemanager.GetString("commissionValue") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_commissionRatio, MainWindow.resourcemanager.GetString("commissionRatio") + "...");
            txt_workInformation.Text = MainWindow.resourcemanager.GetString("trWorkInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_job, MainWindow.resourcemanager.GetString("trJobHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_workHours, MainWindow.resourcemanager.GetString("trWorkHoursHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_details, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_groupId, MainWindow.resourcemanager.GetString("trPermissions"));
            txt_loginInformation.Text = MainWindow.resourcemanager.GetString("trLoginInformation");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_userName, MainWindow.resourcemanager.GetString("trUserNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            txt_addButton.Text = MainWindow.resourcemanager.GetString("trAdd");
            txt_updateButton.Text = MainWindow.resourcemanager.GetString("trUpdate");
            txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
            tt_add_Button.Content = MainWindow.resourcemanager.GetString("trAdd");
            tt_update_Button.Content = MainWindow.resourcemanager.GetString("trUpdate");
            tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");
            //btn_stores.Content = MainWindow.resourcemanager.GetString("trBranchs/Stores");

            dg_users.Columns[0].Header = MainWindow.resourcemanager.GetString("trName");
            dg_users.Columns[1].Header = MainWindow.resourcemanager.GetString("trJob");
            dg_users.Columns[2].Header = MainWindow.resourcemanager.GetString("trPermissions");
            dg_users.Columns[3].Header = MainWindow.resourcemanager.GetString("trWorkHours");
            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");


            txt_branchButton.Text = MainWindow.resourcemanager.GetString("trBranch");
            txt_storesButton.Text = MainWindow.resourcemanager.GetString("trStore");
            txt_sliceButton.Text = MainWindow.resourcemanager.GetString("prices");

            btn_clear.ToolTip = MainWindow.resourcemanager.GetString("trClear");
            tt_report.Content = MainWindow.resourcemanager.GetString("trPdf");
            tt_print.Content = MainWindow.resourcemanager.GetString("trPrint");
            tt_excel.Content = MainWindow.resourcemanager.GetString("trExcel");
            tt_preview.Content = MainWindow.resourcemanager.GetString("trPreview");
            tt_pieChart.Content = MainWindow.resourcemanager.GetString("trPieChart");
            tt_count.Content = MainWindow.resourcemanager.GetString("trCount");

        }
        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {//clear
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                this.DataContext = new User();
                user.userId = 0;
                tb_address.Clear();
                tb_password.Clear();
                tb_userName.Clear();
                pb_password.Clear();
                tb_firstName.Clear();
                tb_lastName.Clear();
                cb_job.SelectedIndex = -1;
                tb_workHours.Clear();
                tb_details.Clear();
                tb_phone.Clear();
                //tb_mobile.IsEnabled = false;
                tb_phone.IsEnabled = false;
                cb_areaMobile.SelectedValue = AppSettings.countryId;
                Cb_areaMobile_SelectionChanged(cb_areaMobile, null);
                cb_areaPhone.SelectedValue = AppSettings.countryId;
                Cb_areaPhone_SelectionChanged(cb_areaPhone, null);
                cb_areaPhoneLocal.SelectedIndex = -1;
                tb_mobile.Clear();
                tb_email.Clear();
                //clear img
                Uri resourceUri = new Uri("pic/no-image-icon-125x125.png", UriKind.Relative);
                StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                brush.ImageSource = temp;
                img_user.Background = brush;
                //img_user.Source = new BitmapImage(new Uri("pic/no-image-icon-125x125.png"));

                btn_branch.IsEnabled = false;
                btn_stores.IsEnabled = false;
                btn_slice.IsEnabled = false;
                btn_branch.Visibility = Visibility.Visible;
                btn_stores.Visibility = Visibility.Visible;
                btn_slice.Visibility = Visibility.Visible;
                grid_groupId.Visibility = Visibility.Visible;
                grid_userNameLabel.Visibility = Visibility.Collapsed;
                grid_userNameInput.Visibility = Visibility.Visible;

                p_errorFirstName.Visibility = Visibility.Collapsed;
                p_errorLastName.Visibility = Visibility.Collapsed;
                p_errorUserName.Visibility = Visibility.Collapsed;
                p_errorUserNameLabel.Visibility = Visibility.Collapsed;
                p_errorPassword.Visibility = Visibility.Collapsed;
                p_errorJob.Visibility = Visibility.Collapsed;

                tb_firstName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_lastName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_userName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                cb_job.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                SectionData.clearValidate(tb_mobile, p_errorMobile);
                SectionData.clearPasswordValidate(pb_password, p_errorPassword);
                p_showPassword.Visibility = Visibility.Visible;

                

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
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
                btn_branch.IsEnabled = false;
                btn_stores.IsEnabled = false;
                btn_slice.IsEnabled = false;
                btn_branch.Visibility = Visibility.Visible;
                btn_stores.Visibility = Visibility.Visible;
                btn_slice.Visibility = Visibility.Visible;
                grid_groupId.Visibility = Visibility.Visible;
                grid_userNameLabel.Visibility = Visibility.Collapsed;
                grid_userNameInput.Visibility = Visibility.Visible;

                // for pagination onTop Always
                btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                //CreateGridCardContainer();
                catigoriesAndItemsView.ucUsers = this;


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
                await FillCombo.FillComboGroup(cb_groupId);
                //fill job combo
                await fillJobCombo();
                if (citynum == null)
                    await RefreshCity();
                await fillCountries();
                await fillCity();
                try
                {
                    if (users is null)
                        await RefreshUsersList();
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
                }
                Tb_search_TextChanged(null, null);

                if (sender != null)
                    SectionData.EndAwait(grid_main);
                Keyboard.Focus(tb_firstName);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region Numeric


        private int _numValue = 0;

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                tb_workHours.Text = value.ToString();
            }
        }



        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue++;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumValue--;
        }
        
        private void Tb_workHours_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (tb_workHours == null)
                {
                    return;
                }

                if (!int.TryParse(tb_workHours.Text, out _numValue))
                    tb_workHours.Text = _numValue.ToString();
            }
            catch (Exception ex)
            {

               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

       
        #endregion

        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {//add
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {

                    #region validate
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_firstName, p_errorFirstName, tt_errorFirstName, "trEmptyNameToolTip");
                    //chk empty last name
                    SectionData.validateEmptyTextBox(tb_lastName, p_errorLastName, tt_errorLastName, "trEmptyLastNameToolTip");
                    //chk empty job
                    SectionData.validateEmptyComboBox(cb_job, p_errorJob, tt_errorJob, "trEmptyJobToolTip");
                    //chk empty mobile
                    SectionData.validateEmptyTextBox(tb_mobile, p_errorMobile, tt_errorMobile, "trEmptyMobileToolTip");
                    //chk empty username
                    SectionData.validateEmptyTextBox(tb_userName, p_errorUserName, tt_errorUserName, "trEmptyUserNameToolTip");
                    SectionData.validateEmptyTextBox(tb_userName, p_errorUserNameLabel, tt_errorUserNameLabel, "trEmptyUserNameToolTip");
                    if (pb_password.Password.Equals("") && grid_userNameInput.IsVisible == true)
                    {
                        SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trEmptyPasswordToolTip");
                        p_showPassword.Visibility = Visibility.Collapsed;
                    }
                    //validate email
                    SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                    //chk duplicate userName
                    bool duplicateUserName = false;
                    duplicateUserName = await chkIfUserNameIsExists(tb_userName.Text, 0);
                    if (duplicateUserName)
                    {
                        p_errorUserName.Visibility = Visibility.Visible;
                        p_errorUserNameLabel.Visibility = Visibility.Visible;
                        tt_errorUserName.Content = MainWindow.resourcemanager.GetString("trErrorDuplicateUserNameToolTip");
                        tt_errorUserNameLabel.Content = MainWindow.resourcemanager.GetString("trErrorDuplicateUserNameToolTip");
                        tb_userName.Background = (Brush)bc.ConvertFrom("#15FF0000");
                    }
                    //chk password length
                    bool passLength = false;
                    passLength = chkPasswordLength(pb_password.Password);
                    string phoneStr = "";
                    if (!tb_phone.Text.Equals(""))
                        phoneStr = cb_areaPhone.Text + "-" + cb_areaPhoneLocal.Text + "-" + tb_phone.Text;

                    bool emailError = false;
                    if (!tb_email.Text.Equals(""))
                        if (!SectionData.IsValid(tb_email.Text))
                            emailError = true;
                    #endregion

                    if ((!tb_firstName.Text.Equals("")) && (!tb_lastName.Text.Equals("")) && (!tb_userName.Text.Equals("")) &&
                                                           (!tb_mobile.Text.Equals("")) &&
                                                           (!pb_password.Password.Equals("")) && (!cb_job.Text.Equals("")))
                    {
                        Nullable<int> groupId = null;
                        if (cb_groupId.SelectedIndex != -1)
                            groupId = (int)cb_groupId.SelectedValue;
                        if ((emailError) || (duplicateUserName) || (passLength))
                        {
                            if (emailError)
                                SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                            //if (duplicateUserName)
                            //{
                            //    p_errorUserName.Visibility = Visibility.Visible;
                            //    p_errorUserNameLabel.Visibility = Visibility.Visible;
                            //    tt_errorUserName.Content = MainWindow.resourcemanager.GetString("trErrorDuplicateUserNameToolTip");
                            //    tt_errorUserNameLabel.Content = MainWindow.resourcemanager.GetString("trErrorDuplicateUserNameToolTip");
                            //    tb_userName.Background = (Brush)bc.ConvertFrom("#15FF0000");
                            //}
                            if (passLength)
                            {
                                p_errorPassword.Visibility = Visibility.Visible;
                                p_showPassword.Visibility = Visibility.Collapsed;
                                tt_errorPassword.Content = MainWindow.resourcemanager.GetString("trErrorPasswordLengthToolTip");
                                tb_password.Background = (Brush)bc.ConvertFrom("#15FF0000");
                                pb_password.Background = (Brush)bc.ConvertFrom("#15FF0000");
                            }
                        }
                        else
                        {
                            user.userId = 0;
                            tb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                            pb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                            user.username = tb_userName.Text;
                            user.password = Md5Encription.MD5Hash("Inc-m" + pb_password.Password);
                            user.name = tb_firstName.Text;
                            user.lastname = tb_lastName.Text;
                            user.job = cb_job.Text;
                            user.workHours = tb_workHours.Text;
                            user.details = "";
                            user.phone = phoneStr;
                            user.mobile = cb_areaMobile.Text + "-" + tb_mobile.Text;
                            user.email = tb_email.Text;
                            user.address = tb_address.Text;
                            user.balance = 0;
                            user.balanceType = 0;
                            user.isActive = 1;
                            user.createUserId = MainWindow.userID.Value;
                            user.updateUserId = MainWindow.userID.Value;
                            user.notes = tb_details.Text;
                            user.role = "";
                            user.groupId = groupId;

                            user.hasCommission = (bool)tgl_hasCommission.IsChecked;
                            try { user.commissionValue = decimal.Parse(tb_commissionValue.Text); }
                            catch { user.commissionValue = 0; }
                            try { user.commissionRatio = decimal.Parse(tb_commissionRatio.Text); }
                            catch { user.commissionRatio = 0;  }

                            int s = (int)await userModel.save(user);
                            if (s == -1)// إظهار رسالة الترقية
                                Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpgrade"), animation: ToasterAnimation.FadeIn);

                            else if ( s == 0) // an error occure
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                            else
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                                Btn_clear_Click(null, null);
                            }      

                            if (isImgPressed)
                            {
                                int userId = s;
                                string b = await userModel.uploadImage(imgFileName,
                                    Md5Encription.MD5Hash("Inc-m" + userId.ToString()), userId);
                                user.image = b;
                                isImgPressed = false;
                            }

                            await RefreshUsersList();
                            Tb_search_TextChanged(null, null);

                            await fillJobCombo();
                            Btn_clear_Click(null,null);
                            //SectionData.getMobile(user.mobile, cb_areaMobile, tb_mobile);
                            //SectionData.getPhone(user.phone, cb_areaPhone, cb_areaPhoneLocal, tb_phone);
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
                    if (user.userId > 0)
                    {
                        #region validate
                        //chk empty name
                        SectionData.validateEmptyTextBox(tb_firstName, p_errorFirstName, tt_errorFirstName, "trEmptyNameToolTip");
                    //chk empty last name
                    SectionData.validateEmptyTextBox(tb_lastName, p_errorLastName, tt_errorLastName, "trEmptyLastNameToolTip");
                    //chk empty mobile
                    SectionData.validateEmptyTextBox(tb_mobile, p_errorMobile, tt_errorMobile, "trEmptyMobileToolTip");
                    //chk empty job
                    SectionData.validateEmptyComboBox(cb_job, p_errorJob, tt_errorJob, "trEmptyJobToolTip");
                    //chk empty username
                    SectionData.validateEmptyTextBox(tb_userName, p_errorUserName, tt_errorUserName, "trEmptyUserNameToolTip");
                    SectionData.validateEmptyTextBox(tb_userName, p_errorUserNameLabel, tt_errorUserNameLabel, "trEmptyUserNameToolTip");
                    //chk empty password
                    if (pb_password.Password.Equals(""))
                    { SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trEmptyPasswordToolTip"); p_showPassword.Visibility = Visibility.Collapsed; }
                    //validate email
                    SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                    //chk duplicate userName
                    bool duplicateUserName = false;
                    duplicateUserName = await chkIfUserNameIsExists(tb_userName.Text, user.userId);

                    string phoneStr = "";
                    if (!tb_phone.Text.Equals("")) phoneStr = cb_areaPhone.Text + "-" + cb_areaPhoneLocal.Text + "-" + tb_phone.Text;

                    bool emailError = false;

                    if (!tb_email.Text.Equals(""))
                        if (!SectionData.IsValid(tb_email.Text))
                            emailError = true;
                    #endregion

                    if ((!tb_firstName.Text.Equals("")) && (!tb_lastName.Text.Equals("")) && (!tb_userName.Text.Equals("")) && (!cb_job.Text.Equals("")) && (!tb_mobile.Text.Equals("")))
                    {
                            Nullable<int> groupId = null;
                            if (cb_groupId.SelectedIndex != -1)
                                groupId = (int)cb_groupId.SelectedValue;
                            if ((emailError) || (duplicateUserName))
                            {

                                if (emailError)
                                    SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
                                if (duplicateUserName)
                                {
                                    p_errorUserName.Visibility = Visibility.Visible;
                                    p_errorUserNameLabel.Visibility = Visibility.Visible;
                                    tt_errorUserName.Content = MainWindow.resourcemanager.GetString("trErrorDuplicateUserNameToolTip");
                                    tt_errorUserNameLabel.Content = MainWindow.resourcemanager.GetString("trErrorDuplicateUserNameToolTip");
                                    tb_userName.Background = (Brush)bc.ConvertFrom("#15FF0000");
                                }
                            }
                            else
                            {
                                tb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                                pb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                                user.userId = user.userId;
                                user.username = tb_userName.Text;
                                user.name = tb_firstName.Text;
                                user.lastname = tb_lastName.Text;
                                user.job = cb_job.Text;
                                user.workHours = tb_workHours.Text;
                                user.details = "";
                                user.phone = phoneStr;
                                user.mobile = cb_areaMobile.Text + "-" + tb_mobile.Text;
                                user.email = tb_email.Text;
                                user.address = tb_address.Text;
                                //user.isActive = 1;
                                user.isOnline = 1;
                                user.createUserId = MainWindow.userID.Value;
                                user.updateUserId = MainWindow.userID.Value;
                                user.notes = tb_details.Text;
                                user.role = "";
                                user.groupId = groupId;

                                user.hasCommission = (bool)tgl_hasCommission.IsChecked;
                                try { user.commissionValue = decimal.Parse(tb_commissionValue.Text); }
                                catch { user.commissionValue = 0; }
                                try { user.commissionRatio = decimal.Parse(tb_commissionRatio.Text); }
                                catch { user.commissionRatio = 0; }

                                int s = (int)await userModel.save(user);

                                if (!s.Equals("0"))
                                {
                                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopUpdate"), animation: ToasterAnimation.FadeIn);
                                    if (MainWindow.userLogin.userId == s)
                                        MainWindow.userLogin = user;
                                }
                                else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                if (isImgPressed)
                                {
                                    int userId = s;
                                    string b = await userModel.uploadImage(imgFileName, Md5Encription.MD5Hash("Inc-m" + userId.ToString()), userId);
                                    user.image = b;
                                    isImgPressed = false;
                                    if (!b.Equals(""))
                                    {
                                        getImg();
                                    }
                                    else
                                    {
                                        SectionData.clearImg(img_user);
                                    }
                                }
                                await RefreshUsersList();
                                Tb_search_TextChanged(null, null);
                                SectionData.getMobile(user.mobile, cb_areaMobile, tb_mobile);

                                SectionData.getPhone(user.phone, cb_areaPhone, cb_areaPhoneLocal, tb_phone);

                                await fillJobCombo();

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

        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {//delete
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "delete") || SectionData.isAdminPermision())
                {
                if (user.userId != 1 && user.userId != 2)
                {
                    if (user.userId != 0)
                    {

                        if ((!user.canDelete) && (user.isActive == 0))
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
                            if (user.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                            if (!user.canDelete)
                                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDeactivate");
                            w.ShowDialog();
                            Window.GetWindow(this).Opacity = 1;
                            #endregion

                            if (w.isOk)
                            {
                                string popupContent = "";
                                if (user.canDelete) popupContent = MainWindow.resourcemanager.GetString("trPopDelete");
                                if ((!user.canDelete) && (user.isActive == 1)) popupContent = MainWindow.resourcemanager.GetString("trPopInActive");
                             
                                int b = (int)await userModel.delete(user.userId, MainWindow.userID.Value, user.canDelete) ;

                                    if (b > 0)
                                    {
                                        user.userId = 0;
                                        Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                    }
                                    else
                                        Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                             
                            }
                        }

                        await RefreshUsersList();
                        Tb_search_TextChanged(null, null);

                            await fillJobCombo();
                        //clear textBoxs
                        Btn_clear_Click(null, null);
                    }
                }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trCannotDeleteTheMainUser"), animation: ToasterAnimation.FadeIn);
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
            user.isActive = 1;
           
                int s = (int)await userModel.save(user) ;

                if (s>0) 
                    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopActive"), animation: ToasterAnimation.FadeIn);
                else 
                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                await RefreshUsersList();
                Tb_search_TextChanged(null, null);
        }

        private void Tb_userName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_userName, p_errorUserName, tt_errorUserName, "trEmptyUserNameToolTip");
                SectionData.validateEmptyTextBox(tb_userName, p_errorUserNameLabel, tt_errorUserNameLabel, "trEmptyUserNameToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Tb_userName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_userName, p_errorUserName, tt_errorUserName, "trEmptyUserNameToolTip");
                SectionData.validateEmptyTextBox(tb_userName, p_errorUserNameLabel, tt_errorUserNameLabel, "trEmptyUserNameToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void Tb_password_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pb_password.Password.Equals(""))
                {
                    p_errorPassword.Visibility = Visibility.Visible;
                    tt_errorPassword.Content = MainWindow.resourcemanager.GetString("trEmptyPasswordToolTip");
                    pb_password.Background = (Brush)bc.ConvertFrom("#15FF0000");
                    p_showPassword.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                    p_errorPassword.Visibility = Visibility.Collapsed;
                    p_showPassword.Visibility = Visibility.Visible;
                }
            }
            catch(Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tb_firstName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_firstName, p_errorFirstName, tt_errorFirstName, "trEmptyNameToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Tb_firstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_firstName, p_errorFirstName, tt_errorFirstName, "trEmptyFirstNameToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void Tb_lastName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_lastName, p_errorLastName, tt_errorLastName, "trEmptyLastNameToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Tb_lastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_lastName, p_errorLastName, tt_errorLastName, "trEmptyLastNameToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Tb_email_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmail(tb_email, p_errorEmail, tt_errorEmail);
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        
        int x = 0;
        private void Cb_job_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (x == 0)
                    SectionData.validateEmptyComboBox(cb_job, p_errorJob, tt_errorJob, "trEmptyJobToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        private void P_showPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                tb_password.Text = pb_password.Password;
                tb_password.Visibility = Visibility.Visible;
                pb_password.Visibility = Visibility.Collapsed;
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void P_showPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                tb_password.Visibility = Visibility.Collapsed;
                pb_password.Visibility = Visibility.Visible;
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        async Task<IEnumerable<User>> RefreshUsersList()
        {
            users = await FillCombo.RefreshUsers();
            return users;
        }
        void RefreshUserView()
        {
            dg_users.ItemsSource = usersQuery;
                txt_count.Text = usersQuery.Count().ToString();
        }

        private async void Tgl_userIsActive_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (users is null)
                    await RefreshUsersList();
                tgl_userState = 1;
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

        private async void Tgl_userIsActive_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (users is null)
                    await RefreshUsersList();
                tgl_userState = 0;
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

        private async void Cb_areaPhone_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                        if (citynum is null)
                            await RefreshCity();
                        citynumofcountry = citynum.Where(b => b.countryId == countryid).OrderBy(b => b.cityCode).ToList();

                        cb_areaPhoneLocal.ItemsSource = citynumofcountry;
                        cb_areaPhoneLocal.SelectedValuePath = "cityId";
                        cb_areaPhoneLocal.DisplayMemberPath = "cityCode";
                        if (citynumofcountry.Count() > 0)
                        {

                            cb_areaPhoneLocal.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            cb_areaPhoneLocal.Visibility = Visibility.Collapsed;
                            tb_phone.IsEnabled = true;
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

        private void Img_user_Click(object sender, RoutedEventArgs e)
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
                    img_user.Background = brush;
                    imgFileName = openFileDialog.FileName;
                }
                else
                { }
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
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    await RefreshUsersList();
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

        private async void getImg()
        {
            try
            {

                if (string.IsNullOrEmpty(user.image))
                {
                    SectionData.clearImg(img_user);
                }
                else
                {
                    byte[] imageBuffer = await userModel.downloadImage(user.image); // read this as BLOB from your DB

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

                        img_user.Background = new ImageBrush(bitmapImage);

                        //string dir = Directory.GetCurrentDirectory();
                        //string tmpPath = System.IO.Path.Combine(dir, Global.TMPAgentsFolder);
                        //tmpPath = System.IO.Path.Combine(tmpPath, user.image);
                        //openFileDialog.FileName = tmpPath;
                    }
                    else
                        SectionData.clearImg(img_user);
                }
            }
            catch
            {
                SectionData.clearImg(img_user);
            }
        }



        void FN_ExportToExcel()
        {
           
                var QueryExcel = usersQuery.AsEnumerable().Select(x => new
                {
                    UserName = x.username,
                    Name = x.name,
                    LastName = x.lastname,
                    Job = x.job,
                    WorkHours = x.workHours,
                    Mobile = x.mobile,
                    Phone = x.phone,
                    Address = x.address,
                    Notes = x.notes,
                });
                var DTForExcel = QueryExcel.ToDataTable();
                DTForExcel.Columns[0].Caption = MainWindow.resourcemanager.GetString("trUserName");
                DTForExcel.Columns[1].Caption = MainWindow.resourcemanager.GetString("trName");
                DTForExcel.Columns[2].Caption = MainWindow.resourcemanager.GetString("trLastName");
                DTForExcel.Columns[3].Caption = MainWindow.resourcemanager.GetString("trJob");
                DTForExcel.Columns[4].Caption = MainWindow.resourcemanager.GetString("trWorkHours");
                DTForExcel.Columns[5].Caption = MainWindow.resourcemanager.GetString("trMobile");
                DTForExcel.Columns[6].Caption = MainWindow.resourcemanager.GetString("trPhone");
                DTForExcel.Columns[7].Caption = MainWindow.resourcemanager.GetString("trAddress");
                DTForExcel.Columns[8].Caption = MainWindow.resourcemanager.GetString("trNote");

                ExportToExcel.Export(DTForExcel);
           
        }
        #region report
        private void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();
            string searchval = "";
            string stateval = "";
            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\SectionData\Ar\ArUserReport.rdlc";
            }
            else
            {
                addpath = @"\Reports\SectionData\En\UserReport.rdlc";
            }
            //filter   
            stateval = tgl_userIsActive.IsChecked == true ? MainWindow.resourcemanagerreport.GetString("trActive_")
              : MainWindow.resourcemanagerreport.GetString("trNotActive");
            paramarr.Add(new ReportParameter("stateval", stateval));
            paramarr.Add(new ReportParameter("trActiveState", MainWindow.resourcemanagerreport.GetString("trState")));
            paramarr.Add(new ReportParameter("trSearch", MainWindow.resourcemanagerreport.GetString("trSearch")));
            searchval = tb_search.Text;
            paramarr.Add(new ReportParameter("searchVal", searchval));
            //end filter
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            ReportCls.checkLang();
            clsReports.userReport(usersQuery, rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);
            rep.SetParameters(paramarr);
            rep.Refresh();
        }
            private void Btn_print_Click(object sender, RoutedEventArgs e)
        {//print
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    BuildReport();
                    LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, short.Parse(AppSettings.rep_print_count));
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

        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {//pdf
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    #region
                    BuildReport();

                    saveFileDialog.Filter = "PDF|*.pdf;";

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filepath = saveFileDialog.FileName;
                        LocalReportExtensions.ExportToPDF(rep, filepath);
                    }
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
        private void btn_pieChart_Click(object sender, RoutedEventArgs e)
        {//pie
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "report") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;
                    win_lvc win = new win_lvc(usersQuery, 3);
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
            #endregion
            private void Tb_preventSpaces_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                SectionData.InputJustNumber(ref textBox);
                e.Handled = e.Key == Key.Space;
            }
            catch (Exception ex)
            {

                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_email_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("^[a-zA-Z0-9. -_?]*$");
                if (!regex.IsMatch(e.Text))
                    e.Handled = true;
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }


        private void Cb_job_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                ComboBox cbm = sender as ComboBox;
                SectionData.searchInComboBox(cbm);
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        
        #region Categor and Item
        #region Refrish Y
        void RefrishItemsCard(IEnumerable<User> _user)
        {
          
                grid_containerCard.Children.Clear();
                catigoriesAndItemsView.gridCatigorieItems = grid_containerCard;
                catigoriesAndItemsView.FN_refrishUsers(_user.ToList(), "en", "User");
           

        }
        #endregion
        #region Get Id By Click  Y
        private   void DG_users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                p_errorFirstName.Visibility = Visibility.Collapsed;
                p_errorLastName.Visibility = Visibility.Collapsed;
                p_errorUserName.Visibility = Visibility.Collapsed;
                p_errorUserNameLabel.Visibility = Visibility.Collapsed;
                p_errorPassword.Visibility = Visibility.Collapsed;
                p_errorEmail.Visibility = Visibility.Collapsed;
                p_errorJob.Visibility = Visibility.Collapsed;

                tb_firstName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_lastName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_email.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_userName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                pb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_showPassword.Visibility = Visibility.Visible;
                cb_job.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                SectionData.clearValidate(tb_mobile, p_errorMobile);

                if (dg_users.SelectedIndex != -1)
                {
                    user = dg_users.SelectedItem as User;
                    this.DataContext = user;
                    tb_commissionValue.Text = SectionData.DecTostring(user.commissionValue);
                    string s = string.Format("{0:G29}", decimal.Parse(SectionData.DecTostring(user.commissionRatio)));
                    tb_commissionRatio.Text = s;

                }
                if (user != null)
                {
                    tb_workHours.Text = user.workHours;
                    btn_branch.IsEnabled = true;
                    btn_stores.IsEnabled = true;
                    btn_slice.IsEnabled = true;
                    if (user.isAdmin.Value)
                    {
                        btn_branch.Visibility = Visibility.Collapsed;
                        btn_stores.Visibility = Visibility.Collapsed;
                        btn_slice.Visibility = Visibility.Collapsed;
                        grid_groupId.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btn_branch.Visibility = Visibility.Visible;
                        btn_stores.Visibility = Visibility.Visible;
                        btn_slice.Visibility = Visibility.Visible;
                        grid_groupId.Visibility = Visibility.Visible;
                    }

                    grid_userNameLabel.Visibility = Visibility.Visible;
                    grid_userNameInput.Visibility = Visibility.Collapsed;

                    if (user.userId != 0)
                    {
                        pb_password.Password = tb_password.Text.Trim();
                    }

                    SectionData.getMobile(user.mobile, cb_areaMobile, tb_mobile);
                    SectionData.getPhone(user.phone, cb_areaPhone, cb_areaPhoneLocal, tb_phone);
                    if (tb_phone.Text.Equals(""))
                        cb_areaPhone.SelectedValue = AppSettings.countryId;

                    #region image
                    bool isModified = SectionData.chkImgChng(user.image, (DateTime)user.updateDate, Global.TMPUsersFolder);
                    if (isModified)
                        getImg();
                    else
                        SectionData.getLocalImg("User", user.image, img_user);
                    #endregion
                    //getImg();

                    #region delete
                    if (user.canDelete)
                    {
                        txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                        txt_delete_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Delete;
                        tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                    }

                    else
                    {
                        if (user.isActive == 0)
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

                    index = dg_users.SelectedIndex;
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
        public void ChangeItemIdEvent(int userId)
        {

            btn_branch.IsEnabled = true;
            btn_stores.IsEnabled = true;
            btn_slice.IsEnabled = true;

            

            grid_userNameLabel.Visibility = Visibility.Visible;
            grid_userNameInput.Visibility = Visibility.Collapsed;
            #region
            p_errorFirstName.Visibility = Visibility.Collapsed;
                p_errorLastName.Visibility = Visibility.Collapsed;
                p_errorUserName.Visibility = Visibility.Collapsed;
            p_errorUserNameLabel.Visibility = Visibility.Collapsed;
                p_errorPassword.Visibility = Visibility.Collapsed;
                p_errorEmail.Visibility = Visibility.Collapsed;
                p_errorJob.Visibility = Visibility.Collapsed;

                tb_firstName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_lastName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_email.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_userName.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                tb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                pb_password.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                cb_job.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                user = users.ToList().Find(c => c.userId == userId);
                this.DataContext = user;
            if (user.isAdmin.Value)
            {
                btn_branch.Visibility = Visibility.Collapsed;
                btn_stores.Visibility = Visibility.Collapsed;
                btn_slice.Visibility = Visibility.Collapsed;
                grid_groupId.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_branch.Visibility = Visibility.Visible;
                btn_stores.Visibility = Visibility.Visible;
                btn_slice.Visibility = Visibility.Visible;
                grid_groupId.Visibility = Visibility.Visible;
            }
            if (user != null)
                {
                    if (user.userId != 0)
                    {
                        pb_password.Password = tb_password.Text.Trim();
                    }

                    SectionData.getMobile(user.mobile, cb_areaMobile, tb_mobile);

                    SectionData.getPhone(user.phone, cb_areaPhone, cb_areaPhoneLocal, tb_phone);

                    getImg();

                    #region delete
                    if (user.canDelete)
                    {
                        txt_deleteButton.Text = MainWindow.resourcemanager.GetString("trDelete");
                        txt_delete_Icon.Kind =
                                 MaterialDesignThemes.Wpf.PackIconKind.Delete;
                        tt_delete_Button.Content = MainWindow.resourcemanager.GetString("trDelete");

                    }

                    else
                    {
                        if (user.isActive == 0)
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
                #endregion
        }
        #endregion
        #region Switch Card/DataGrid Y
        private void Btn_itemsInCards_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                grid_datagrid.Visibility = Visibility.Collapsed;
                grid_pagination.Visibility = Visibility.Visible;
                grid_cards.Visibility = Visibility.Visible;
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_itemsInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

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

        private void Btn_itemsInGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                grid_cards.Visibility = Visibility.Collapsed;
                grid_pagination.Visibility = Visibility.Collapsed;
                grid_datagrid.Visibility = Visibility.Visible;

                path_itemsInGrid.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                path_itemsInCards.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4e4e4e"));

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
        #endregion
        #region Search Y


        /// <summary>
        /// Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
             if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "show") || SectionData.isAdminPermision())
                {
                    p_errorFirstName.Visibility = Visibility.Collapsed;
                    tb_firstName.Background = (Brush)bc.ConvertFrom("#f8f8f8");

                    if (users is null)
                        await RefreshUsersList();
                    searchText = tb_search.Text.ToLower();
                    pageIndex = 1;

                    #region
                    usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                    s.name.ToLower().Contains(searchText) ||
                    s.username.ToLower().Contains(searchText) ||
                    s.job.ToLower().Contains(searchText)
                    ) && s.isActive == tgl_userState);
                    //txt_count.Text = usersQuery.Count().ToString();
                    if (btns is null)
                        btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                    RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
                    #endregion
                    RefreshUserView();
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
        #region Pagination Y
        Pagination pagination = new Pagination();
        Button[] btns;
        public int pageIndex = 1;

        private void Tb_pageNumberSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                try
                {
                    if (int.Parse(tb_pageNumberSearch.Text) < 1)
                        return;
                }
                catch
                {
                    if (!string.IsNullOrWhiteSpace(tb_pageNumberSearch.Text))
                    {
                        return;
                    }
                }

                if (sender != null)
                    SectionData.StartAwait(grid_main);
                usersQuery = users.Where(x => x.isActive == tgl_userState);

                if (tb_pageNumberSearch.Text.Equals(""))
                {
                    pageIndex = 1;
                }
                else if (((usersQuery.Count() - 1) / 12) + 1 < int.Parse(tb_pageNumberSearch.Text))
                {
                    pageIndex = ((usersQuery.Count() - 1) / 12) + 1;
                }
                else
                {
                    pageIndex = int.Parse(tb_pageNumberSearch.Text);
                }

                #region
                usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText) ||
                s.username.ToLower().Contains(searchText) ||
                s.job.ToLower().Contains(searchText)
                ) && s.isActive == tgl_userState);
                if (btns is null)
                    btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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
        private void Btn_firstPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                pageIndex = 1;
                #region
                usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText) ||
                s.username.ToLower().Contains(searchText) ||
                s.job.ToLower().Contains(searchText)
                ) && s.isActive == tgl_userState);
                if (btns is null)
                    btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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
        private void Btn_prevPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                pageIndex = int.Parse(btn_prevPage.Content.ToString());

                #region
                usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText) ||
                s.username.ToLower().Contains(searchText) ||
                s.job.ToLower().Contains(searchText)
                ) && s.isActive == tgl_userState);
                if (btns is null)
                    btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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
        private void Btn_activePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                pageIndex = int.Parse(btn_activePage.Content.ToString());
                #region
                usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText) ||
                s.username.ToLower().Contains(searchText) ||
                s.job.ToLower().Contains(searchText)
                ) && s.isActive == tgl_userState);
                if (btns is null)
                    btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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
        private void Btn_nextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                pageIndex = int.Parse(btn_nextPage.Content.ToString());
                #region
                usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText) ||
                s.username.ToLower().Contains(searchText) ||
                s.job.ToLower().Contains(searchText)
                ) && s.isActive == tgl_userState);
                if (btns is null)
                    btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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
        private void Btn_lastPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                usersQuery = users.Where(x => x.isActive == tgl_userState);
                pageIndex = ((usersQuery.Count() - 1) / 12) + 1;
                #region
                usersQuery = users.Where(s => (s.lastname.ToLower().Contains(searchText) ||
                s.name.ToLower().Contains(searchText) ||
                s.username.ToLower().Contains(searchText) ||
                s.job.ToLower().Contains(searchText)
                ) && s.isActive == tgl_userState);
                if (btns is null)
                    btns = new Button[] { btn_firstPage, btn_prevPage, btn_activePage, btn_nextPage, btn_lastPage };
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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
        #endregion

        #endregion
        private void Grid_containerCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                RefrishItemsCard(pagination.refrishPagination(usersQuery, pageIndex, btns));
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



        private void Btn_stores_Click(object sender, RoutedEventArgs e)
        {//stores
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (MainWindow.groupObject.HasPermissionAction(storesPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
                {
                    Window.GetWindow(this).Opacity = 0.2;

                    wd_branchesList w = new wd_branchesList();
                    w.Id = user.userId;
                    w.userOrBranch = 'u';
                    w.branchType = (sender as Button).Tag.ToString();
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
                    Window.GetWindow(this).Opacity = 1;
                if (sender != null)
                    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Btn_slice_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
                SectionData.StartAwait(grid_main);
            if (MainWindow.groupObject.HasPermissionAction(storesPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
            {
                Window.GetWindow(this).Opacity = 0.2;

                wd_sliceList w = new wd_sliceList();
                w.Id = user.userId;
                w.ShowDialog();

                Window.GetWindow(this).Opacity = 1;
            }
            else
                Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
            if (sender != null)
                SectionData.EndAwait(grid_main);
        }
        private void tb_mobile_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_mobile, p_errorMobile, tt_errorMobile, "trEmptyMobileToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void tb_mobile_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_mobile, p_errorMobile, tt_errorMobile, "trEmptyMobileToolTip");
            }
            catch(Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }

        private void Pb_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pb_password.Password.Equals(""))
                {
                    SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trEmptyPasswordToolTip");
                    p_showPassword.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SectionData.clearPasswordValidate(pb_password, p_errorPassword);
                    p_showPassword.Visibility = Visibility.Visible;
                }
            }
            catch(Exception ex) {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
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

        private void Cb_areaMobile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cb_areaMobile.SelectedIndex != -1)
                    tb_mobile.IsEnabled = true;
                else
                    tb_mobile.IsEnabled = false;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Tgl_hasCommission_Checked(object sender, RoutedEventArgs e)
        {
            tb_commissionValue.IsEnabled =
            tb_commissionRatio.IsEnabled =
            true;
        }
        private void Tgl_hasCommission_Unchecked(object sender, RoutedEventArgs e)
        {
           tb_commissionValue.IsEnabled =
           tb_commissionRatio.IsEnabled =
           false;
        }

        private void commission_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((TextBox)sender).Text.Contains(e.Text)))
                e.Handled = false;

            else
                e.Handled = true;
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
        private decimal _commissionValue = 0;

        private void commission_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                //validateEmpty(name, sender);
                var txb = sender as TextBox;
                //if (txb.Name == "tb_discountValue")
                SectionData.InputJustNumber(ref txb);

                if (txb == null)
                {
                    return;
                }

                if (!decimal.TryParse(txb.Text, out _commissionValue))
                    txb.Text = _commissionValue.ToString();

                try
                {
                    if (txb.Name == "tb_commissionRatio")
                    {
                        if (_commissionValue > 100)
                            txb.Text = "100";
                        else if (_commissionValue < 0)
                            txb.Text = "0";
                        txb.Select(txb.Text.Length, 0);
                    }
                }
                catch
                {
                    txb.Text = "";
                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

       
    }
}

