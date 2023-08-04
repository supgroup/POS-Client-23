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
    /// Interaction logic for wd_acceptUser.xaml
    /// </summary>
    public partial class wd_acceptUser : Window
    {
        public wd_acceptUser()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        BrushConverter bc = new BrushConverter();
        public bool isOk { get; set; }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isOk = false;
                this.Close();
            }
            catch(Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public User user = new User();
        public int userID = 0;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
               
                #region translate

                if (AppSettings.lang.Equals("en"))
            {
                MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                grid_acceptUser.FlowDirection = FlowDirection.LeftToRight;

            }
            else
            {
                MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                grid_acceptUser.FlowDirection = FlowDirection.RightToLeft;

            }

            translate();
                #endregion

            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translate()
        {
            txt_user.Text = MainWindow.resourcemanager.GetString("trUserConfirm");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_userName, MainWindow.resourcemanager.GetString("trUserNameHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(pb_password, MainWindow.resourcemanager.GetString("trPasswordHint"));

            tt_userName.Content = MainWindow.resourcemanager.GetString("trUserName");
            tt_password.Content = MainWindow.resourcemanager.GetString("trPassword");

            btn_confirmation.Content = MainWindow.resourcemanager.GetString("trConfirm");

        }
       
        private void Btn_confirmation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_acceptUser);

                chkUser();

                if (sender != null)
                    SectionData.EndAwait(grid_acceptUser);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_acceptUser);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Btn_confirmation_Click(null, null);
                }
            }
            catch(Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void chkUser()
        {
            string password = Md5Encription.MD5Hash("Inc-m" + pb_password.Password);

          
            if ((tb_userName.Text.Trim().Equals(user.username)) && (password.Trim().Equals(user.password)))
             isOk = true; 
            else
                isOk = false;

            if (isOk) 
                this.Close();
            else
            {
                SectionData.showPasswordValidate(pb_password, p_errorPassword, tt_errorPassword, "trErrorPasswordToolTip");
                p_showPassword.Visibility = Visibility.Collapsed;
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
        private void Tb_userName_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            { 
                SectionData.validateEmptyTextBox(tb_userName, p_errorUserName, tt_errorUserName, "trEmptyUserNameToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_userName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.validateEmptyTextBox(tb_userName, p_errorUserName, tt_errorUserName, "trEmptyUserNameToolTip");
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
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
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Tb_password_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SectionData.clearValidate(tb_password, p_errorPassword);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Pb_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.clearPasswordValidate(pb_password, p_errorPassword);
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
                tb_userName.Clear();
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
    }
}
