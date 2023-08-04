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
    /// Interaction logic for wd_deliveryReceiptInvoice.xaml
    /// </summary>
    public partial class wd_deliveryReceiptInvoice : Window
    {
        public decimal deliveryCost { get; set; }
        public int shippingCompanyId { get; set; }
        public int shipUserId { get; set; }
        User userModel = new User();
        ShippingCompanies companyModel = new ShippingCompanies();
        List<ShippingCompanies> companies;
        List<User> users;
        public wd_deliveryReceiptInvoice()
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

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_delivery);

                deliveryCost = (decimal) companyModel.deliveryCost;
                shippingCompanyId = (int)cb_company.SelectedValue;
                if (cb_user.SelectedIndex != -1)
                    shipUserId = (int)cb_user.SelectedValue;
                DialogResult = true;
                this.Close();

                if (sender != null)
                    SectionData.EndAwait(grid_delivery);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_delivery);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_delivery);

                #region translate
                if (AppSettings.lang.Equals("en"))
            {
                MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
            }
            else
            {
                MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
            }

            translate();
            #endregion

                await fillShippingCompanies();
                await fillUsers();

                #region key up
                cb_company.IsTextSearchEnabled = false;
                cb_company.IsEditable = true;
                cb_company.StaysOpenOnEdit = true;
                cb_company.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_company.Text = "";

                cb_user.IsTextSearchEnabled = false;
                cb_user.IsEditable = true;
                cb_user.StaysOpenOnEdit = true;
                cb_user.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_user.Text = "";
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_delivery);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_delivery);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void translate()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trDelivery");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_company, MainWindow.resourcemanager.GetString("trCompanyHint"));
            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");
        }
        private async Task fillShippingCompanies()
        {
            companies = await companyModel.Get();
            cb_company.ItemsSource = companies;
            cb_company.DisplayMemberPath = "name";
            cb_company.SelectedValuePath = "shippingCompanyId";

            if (shippingCompanyId != 0)
            {
                cb_company.SelectedValue = shippingCompanyId;
                companyModel = companies.Find(c => c.shippingCompanyId == (int)cb_company.SelectedValue);
                if (companyModel.deliveryType == "local")
                {
                    cb_user.Visibility = Visibility.Visible;
                }
                else
                {
                    cb_user.Visibility = Visibility.Collapsed;
                }
            }
        }
        private async Task fillUsers()
        {
            users = await userModel.GetUsersActive();
            cb_user.ItemsSource = users;
            cb_user.DisplayMemberPath = "name";
            cb_user.SelectedValuePath = "userId";

            if (shipUserId  != 0)
            {
                cb_user.SelectedValue = shipUserId;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Cb_company_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_delivery);
                if (cb_company.SelectedIndex != -1)
            {
                companyModel = companies.Find(c => c.shippingCompanyId == (int)cb_company.SelectedValue);
               
                if(companyModel.deliveryType == "local")
                {
                    cb_user.Visibility = Visibility.Visible;                   
                }
                else
                {
                    cb_user.SelectedIndex = -1;
                    cb_user.Visibility = Visibility.Collapsed;
                }
            }
                if (sender != null)
                    SectionData.EndAwait(grid_delivery);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_delivery);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Cb_company_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Cb_company_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = companies.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
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
