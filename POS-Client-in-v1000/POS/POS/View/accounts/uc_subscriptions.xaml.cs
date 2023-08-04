using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using netoaster;
using POS.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS.View.accounts
{
    /// <summary>
    /// Interaction logic for uc_subscriptions.xaml
    /// </summary>
    public partial class uc_subscriptions : UserControl
    {

        string createPermission = "subscriptions_create";
        string reportsPermission = "subscriptions_reports";
        private static uc_subscriptions _instance;
        public static uc_subscriptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_subscriptions();
                return _instance;
            }
        }
        public uc_subscriptions()
        {
            InitializeComponent();
        }

        private void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Tgl_isActive_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Tgl_isActive_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_exportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.groupObject.HasPermissionAction(reportsPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
            {
             


            }
                else
                    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
            }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Tb_name_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void validationControl_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void validationTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Btn_add_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Dg_subscriptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.initializationMainTrack(this.Tag.ToString(), 1);
        }

        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.groupObject.HasPermissionAction(createPermission, MainWindow.groupObjects, "one") || SectionData.isAdminPermision())
            {


            }
            else
                Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
        }

        private void Cb_customer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Cb_customer_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Cb_customer_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_updateCustomer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cb_paymentProcessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        private void Btn_pdf_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
    }
}
