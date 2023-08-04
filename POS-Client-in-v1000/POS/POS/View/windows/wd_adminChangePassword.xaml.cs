using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
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

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_adminChangePassword.xaml
    /// </summary>
    public partial class wd_adminChangePassword : Window
    {
        public wd_adminChangePassword()
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
        BrushConverter bc = new BrushConverter();
        public int userID = 0;
        User userModel = new User();
        User user = new User();

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_changePassword);

                #region translate

                if (AppSettings.lang.Equals("en"))
            {
                MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                grid_changePassword.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                grid_changePassword.FlowDirection = FlowDirection.RightToLeft;
            }

            translate();
                #endregion

                await fillUsers();

                #region key up
                cb_user.IsTextSearchEnabled = false;
                cb_user.IsEditable = true;
                cb_user.StaysOpenOnEdit = true;
                cb_user.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_user.Text = "";
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_changePassword);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_changePassword);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        List<User> users = new List<User>();
        private async Task fillUsers() 
        {
            if (FillCombo.usersActiveList == null)
                await FillCombo.RefreshUsersActive();
            // List<User> users =  await userModel.GetUsersActive();
            users = FillCombo.usersActiveList;
            if (!SectionData.isSupportPermision())
                users = users.Where(x => x.isAdmin == false).ToList();
            cb_user.ItemsSource = users;
            cb_user.DisplayMemberPath = "username";
            cb_user.SelectedValuePath = "userId";
        }

        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trChangePassword");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUserHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
            tt_user.Content = MainWindow.resourcemanager.GetString("trUser");
            tt_password.Content = MainWindow.resourcemanager.GetString("trPassword");
            tt_showPassword.Content = MainWindow.resourcemanager.GetString("trShowPassword");
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
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private bool chkPasswordLength(string password)
        {
            bool b = false;
            if (password.Length < 6)
                b = true;
            return b;
        }
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_changePassword);

                bool wrongPasswordLength = false;
                //chk empty user
                SectionData.validateEmptyComboBox(cb_user , p_errorUser , tt_errorUser , "trEmptyUser");
                //chk empty password
                if (pb_password.Password.Equals(""))
                    SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trEmptyPasswordToolTip");
                else
                {
                    //chk password length
                    wrongPasswordLength = chkPasswordLength(pb_password.Password);
                    if (wrongPasswordLength)
                        SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trErrorPasswordLengthToolTip");
                    else
                        SectionData.clearPasswordValidate(pb_password, p_errorPassword);
                }

                if ((!cb_user.Text.Equals("")) &&(!pb_password.Password.Equals("")) && (!wrongPasswordLength))
                {
                    if (user != null)
                    {
                        string password = Md5Encription.MD5Hash("Inc-m" + pb_password.Password);

                        user.password = password ;
                        if (MainWindow.userLogin.userId == user.userId)
                            MainWindow.userLogin.password = password;

                        int s = (int)await userModel.save(user);

                        if (s>0)
                        {
                            if (Properties.Settings.Default.password != string.Empty && user.userId == MainWindow.userLogin.userId)
                            {
                                Properties.Settings.Default.password = pb_password.Password;
                                Properties.Settings.Default.Save();
                            }
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopPasswordChanged"), animation: ToasterAnimation.FadeIn);
                            await Task.Delay(2000);
                            this.Close();

                            userID =s;
                       
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                }
                if (sender != null)
                    SectionData.EndAwait(grid_changePassword);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_changePassword);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
      
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                cb_user.SelectedIndex = -1;
                pb_password.Clear();
                tb_password.Clear();
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            { 
                tb_password.Text = pb_password.Password;
                tb_password.Visibility = Visibility.Visible;
                pb_password.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            { 
                tb_password.Visibility = Visibility.Collapsed;
                pb_password.Visibility = Visibility.Visible;
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

        private void Tb_validateEmptyTextChange(object sender, RoutedEventArgs e)
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
            if (name == "PasswordBox")
            {
                if ((sender as PasswordBox).Name == "pb_password")
                    if (((PasswordBox)sender).Password.Equals(""))
                        SectionData.showPasswordValidate((PasswordBox)sender, p_errorPassword, tt_errorPassword, "trEmptyPasswordToolTip");
                    else
                        SectionData.clearPasswordValidate((PasswordBox)sender , p_errorPassword);
            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_user")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorUser, tt_errorUser, "trEmptyUser");
            }
        }

        private async void Cb_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select user
            try
            {
                user = await userModel.getUserById(Convert.ToInt32(cb_user.SelectedValue));
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }


        }

        private void Cb_user_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = users.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
