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
    /// Interaction logic for wd_changePassword.xaml
    /// </summary>
    public partial class wd_changePassword : Window
    {
        public wd_changePassword()
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

        public int userID = 0;
        User userModel = new User();

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            txt_title.Text = MainWindow.resourcemanager.GetString("trChangePassword");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_oldPassword, MainWindow.resourcemanager.GetString("trOldPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_oldPassword, MainWindow.resourcemanager.GetString("trOldPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_newPassword, MainWindow.resourcemanager.GetString("trNewPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_newPassword, MainWindow.resourcemanager.GetString("trNewPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_confirmPassword, MainWindow.resourcemanager.GetString("trConfirmedPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_confirmPassword, MainWindow.resourcemanager.GetString("trConfirmedPasswordHint"));

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

            tt_oldPassword.Content = MainWindow.resourcemanager.GetString("trOldPassword");
            tt_newPassword.Content = MainWindow.resourcemanager.GetString("trNewPassword");
            tt_confirmPassword.Content = MainWindow.resourcemanager.GetString("trConfirmedPassword");
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
                    SectionData.StartAwait(grid_main);

                bool wrongOldPasswordLength = false, wrongNewPasswordLength = false, wrongConfirmPasswordLength = false;
                //chk empty old password
                if (pb_oldPassword.Password.Equals(""))
                    SectionData.showPasswordValidate(pb_oldPassword, p_errorOldPassword, tt_errorOldPassword, "trEmptyPasswordToolTip");
                else
                {
                    //chk password length
                    wrongOldPasswordLength = chkPasswordLength(pb_oldPassword.Password);
                    if (wrongOldPasswordLength)
                        SectionData.showPasswordValidate(pb_oldPassword, p_errorOldPassword, tt_errorOldPassword, "trErrorPasswordLengthToolTip");
                    else
                        SectionData.clearPasswordValidate(pb_oldPassword, p_errorOldPassword);
                }
                //chk empty new password
                if (pb_newPassword.Password.Equals(""))
                    SectionData.showPasswordValidate(pb_newPassword, p_errorNewPassword, tt_errorNewPassword, "trEmptyPasswordToolTip");
                else
                {
                    //chk password length
                    wrongNewPasswordLength = chkPasswordLength(pb_newPassword.Password);
                    if (wrongNewPasswordLength)
                        SectionData.showPasswordValidate(pb_newPassword, p_errorNewPassword, tt_errorNewPassword, "trErrorPasswordLengthToolTip");
                    else
                        SectionData.clearPasswordValidate(pb_newPassword, p_errorNewPassword);
                }
                //chk empty confirm password
                if (pb_confirmPassword.Password.Equals(""))
                    SectionData.showPasswordValidate(pb_confirmPassword, p_errorConfirmPassword, tt_errorConfirmPassword, "trEmptyPasswordToolTip");
                else
                {
                    //chk password length
                    wrongConfirmPasswordLength = chkPasswordLength(pb_confirmPassword.Password);
                    if (wrongConfirmPasswordLength)
                        SectionData.showPasswordValidate(pb_confirmPassword, p_errorConfirmPassword, tt_errorConfirmPassword, "trErrorPasswordLengthToolTip");
                    else
                        SectionData.clearPasswordValidate(pb_confirmPassword, p_errorConfirmPassword);
                }


                if ((!pb_oldPassword.Password.Equals("")) && (!wrongOldPasswordLength) &&
                   (!pb_newPassword.Password.Equals("")) && (!wrongNewPasswordLength) &&
                   (!pb_confirmPassword.Password.Equals("")) && (!wrongConfirmPasswordLength))
                {
                    //get password for logined user
                    string loginPassword = MainWindow.userLogin.password;

                    string enteredPassword = Md5Encription.MD5Hash("Inc-m" + pb_oldPassword.Password);

                    if (!loginPassword.Equals(enteredPassword))
                    {
                        SectionData.showPasswordValidate(pb_oldPassword, p_errorOldPassword, tt_errorOldPassword, "trWrongPassword");
                    }
                    else
                    {
                        SectionData.clearPasswordValidate(pb_oldPassword, p_errorOldPassword);
                        bool isNewEqualConfirmed = true;
                        if (pb_newPassword.Password.Equals(pb_confirmPassword.Password)) isNewEqualConfirmed = true;
                        else isNewEqualConfirmed = false;

                        if (!isNewEqualConfirmed)
                        {
                            SectionData.showPasswordValidate(pb_newPassword, p_errorNewPassword, tt_errorNewPassword, "trErrorNewPasswordNotEqualConfirmed");
                            SectionData.showPasswordValidate(pb_confirmPassword, p_errorConfirmPassword, tt_errorConfirmPassword, "trErrorNewPasswordNotEqualConfirmed");
                        }
                        else
                        {
                            SectionData.clearPasswordValidate(pb_newPassword, p_errorNewPassword);
                            SectionData.clearPasswordValidate(pb_confirmPassword, p_errorConfirmPassword);
                            //change password
                            string newPassword = Md5Encription.MD5Hash("Inc-m" + pb_newPassword.Password);
                            MainWindow.userLogin.password = newPassword;
                            int s = (int)await userModel.save(MainWindow.userLogin);
                            
                            if (s>0)
                            {
                                userID = s;
                                //if (!Properties.Settings.Default.password.Equals(""))
                                //{
                                //    Properties.Settings.Default.password = pb_newPassword.Password;
                                //    Properties.Settings.Default.Save();
                                //}
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopPasswordChanged"), animation: ToasterAnimation.FadeIn);
                                this.Close();
                            }
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

        private void P_showOldPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                tb_oldPassword.Text = pb_oldPassword.Password;
                tb_oldPassword.Visibility = Visibility.Visible;
                pb_oldPassword.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showOldPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                tb_oldPassword.Visibility = Visibility.Collapsed;
                pb_oldPassword.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showNewPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                tb_newPassword.Text = pb_newPassword.Password;
                tb_newPassword.Visibility = Visibility.Visible;
                pb_newPassword.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showNewPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                tb_newPassword.Visibility = Visibility.Collapsed;
                pb_newPassword.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showConfirmPassword_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                tb_confirmPassword.Text = pb_confirmPassword.Password;
                tb_confirmPassword.Visibility = Visibility.Visible;
                pb_confirmPassword.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void P_showConfirmPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                tb_confirmPassword.Visibility = Visibility.Collapsed;
                pb_confirmPassword.Visibility = Visibility.Visible;
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
                pb_oldPassword.Clear();
                tb_oldPassword.Clear();
                pb_newPassword.Clear();
                tb_newPassword.Clear();
                pb_confirmPassword.Clear();
                tb_confirmPassword.Clear();
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
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
                if (name == "PasswordBox")
                {
                    if ((sender as PasswordBox).Name == "pb_oldPassword")
                        if (((PasswordBox)sender).Password.Equals(""))
                            SectionData.showPasswordValidate((PasswordBox)sender, p_errorOldPassword, tt_errorOldPassword, "trEmptyPasswordToolTip");
                        else
                            SectionData.clearPasswordValidate((PasswordBox)sender, p_errorOldPassword);
                    else if ((sender as PasswordBox).Name == "pb_newPassword")
                        if (((PasswordBox)sender).Password.Equals(""))
                            SectionData.showPasswordValidate((PasswordBox)sender, p_errorNewPassword, tt_errorNewPassword, "trEmptyPasswordToolTip");
                        else
                            SectionData.clearPasswordValidate((PasswordBox)sender, p_errorNewPassword);
                    else if ((sender as PasswordBox).Name == "pb_confirmPassword")
                        if (((PasswordBox)sender).Password.Equals(""))
                            SectionData.showPasswordValidate((PasswordBox)sender, p_errorConfirmPassword, tt_errorConfirmPassword, "trEmptyPasswordToolTip");
                        else
                            SectionData.clearPasswordValidate((PasswordBox)sender, p_errorConfirmPassword);
                }
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
    }
}
