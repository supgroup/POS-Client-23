using netoaster;
using POS.Classes;
using POS.View.windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
 
using Microsoft.Reporting.WinForms;
 using System.ComponentModel;

namespace POS.View.delivery
{
    /// <summary>
    /// Interaction logic for uc_driversManagement.xaml
    /// </summary>
    public partial class uc_driversManagement : UserControl
    {
        IEnumerable<User> drivers;
        IEnumerable<User> driversQuery;
        User userModel = new User();
        User driver = new User();

        IEnumerable<ShippingCompanies> companies;
        IEnumerable<ShippingCompanies> companiesQuery;
        ShippingCompanies companyModel = new ShippingCompanies();
        ShippingCompanies company = new ShippingCompanies();
        string searchText = "";
        byte tgl_driverState;

        string viewPermission = "driversManagement_view";
        string residentialSectorsPermission = "driversManagement_residentialSectors";
        string activateDriverPermission = "driversManagement_activateDriver";

        List<Invoice> orders;
        Invoice orderModel = new Invoice();
        List<Invoice> driverOrder = new List<Invoice>();

        private static uc_driversManagement _instance;

        public static uc_driversManagement Instance
        {
            get
            {
                if(_instance is null)
                _instance = new uc_driversManagement();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public uc_driversManagement()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                SectionData.StartAwait(grid_main);

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    grid_main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    grid_main.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();
                #endregion

                await Search();
                
                await RefreshOrdersList();

                SectionData.EndAwait(grid_main);
            }
                catch (Exception ex)
                {
                    SectionData.EndAwait(grid_main);
                   SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region methods
        async Task<IEnumerable<Invoice>> RefreshOrdersList()
        {
            orders = await orderModel.GetOrdersWithDelivery(MainWindow.loginBranch.branchId, "Collected,Ready");
            orders = orders.Where(o => o.status == "Collected" || o.status == "Ready").ToList();
            return orders;
        }
        async Task Search()
        {
            try
            {
                if (chk_drivers.IsChecked.Value)
                {
                    if (drivers is null)
                        await RefreshDriversList();
                    searchText = tb_search.Text.ToLower();
                    driversQuery = drivers.Where(s => (
                       s.name.ToLower().Contains(searchText)
                    || s.mobile.ToString().ToLower().Contains(searchText)
                    ));
                    RefreshDriverView();
                }
                else if (chk_shippingCompanies.IsChecked.Value)
                {
                    if (companies is null)
                       await RefreshCompaniesList();
                    searchText = tb_search.Text.ToLower();
                    companiesQuery = companies.Where(x => x.deliveryType == "com").ToList();
                    companiesQuery = companiesQuery.Where(s => (
                       s.name.ToLower().Contains(searchText)
                    || s.mobile.ToString().ToLower().Contains(searchText)
                    ));
                    RefreshCompanyView();
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
        async Task<IEnumerable<User>> RefreshDriversList()
        {
            if(FillCombo.driversList is null)
            await FillCombo.RefreshDrivers();
            drivers = FillCombo.driversList.ToList();
            //drivers = await userModel.getBranchSalesMan(MainWindow.branchID.Value , "deliveryPermission");
            return drivers;
        }
        async Task<IEnumerable<ShippingCompanies>> RefreshCompaniesList()
        {
            if (FillCombo.shippingCompaniesAllList is null)
                await FillCombo.RefreshShippingCompaniesAll();
            
            //companies = await companyModel.Get();
            companies = FillCombo.shippingCompaniesAllList.ToList();
            companies = companies.Where(c => c.deliveryType != "local");
            return companies;
        }
        void RefreshDriverView()
        {
            dg_user.ItemsSource = driversQuery;
        }
        void RefreshCompanyView()
        {
            dg_user.ItemsSource = companiesQuery;
        }
        private void translate()
        {
            // Title
            txt_title.Text = MainWindow.resourcemanager.GetString("deliveryList");

            txt_details.Text = MainWindow.resourcemanager.GetString("trDetails");

            chk_drivers.Content = MainWindow.resourcemanager.GetString("drivers");
            chk_shippingCompanies.Content = MainWindow.resourcemanager.GetString("trShippingCompanies");

            txt_driverUserName.Text = MainWindow.resourcemanager.GetString("trUserName");
            txt_driverName.Text = MainWindow.resourcemanager.GetString("trDriver");
            txt_driverMobile.Text = MainWindow.resourcemanager.GetString("trMobile");
            txt_driverSectorsCount.Text = MainWindow.resourcemanager.GetString("trResidentialSectors");
            txt_driverOrdersCount.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_driverStatus.Text = MainWindow.resourcemanager.GetString("trStatus");

            txt_companyName.Text = MainWindow.resourcemanager.GetString("trCompany");
            txt_companyMobile.Text = MainWindow.resourcemanager.GetString("trMobile");
            txt_companyEmail.Text = MainWindow.resourcemanager.GetString("trEmail");
            txt_companyOrdersCount.Text = MainWindow.resourcemanager.GetString("trOrders");
            txt_companyStatus.Text = MainWindow.resourcemanager.GetString("trStatus");

            txt_preview.Text = MainWindow.resourcemanager.GetString("trPreview");
            txt_print.Text = MainWindow.resourcemanager.GetString("trPrint");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_search, MainWindow.resourcemanager.GetString("trSearchHint"));

            col_driversUsername.Header = MainWindow.resourcemanager.GetString("trUserName");
            col_driversName.Header = MainWindow.resourcemanager.GetString("trName");
            col_driversMobile.Header = MainWindow.resourcemanager.GetString("trMobile");

            col_companyName.Header = MainWindow.resourcemanager.GetString("trName");
            col_companyMobile.Header = MainWindow.resourcemanager.GetString("trMobile");
            col_companyEmail.Header = MainWindow.resourcemanager.GetString("trEmail");
            col_companyAvailable.Header = MainWindow.resourcemanager.GetString("trStatus");

            tt_refresh.Content = MainWindow.resourcemanager.GetString("trRefresh");

        }
        async Task changeDeliveryType()
        {
            SectionData.StartAwait(grid_main);

            if (col_driversUsername is null)
                await Task.Delay(500);
            if (chk_drivers.IsChecked.Value)
            {
                col_driversUsername.Visibility = Visibility.Visible;
                col_driversName.Visibility = Visibility.Visible;
                col_driversMobile.Visibility = Visibility.Visible;
                sp_driverDetails.Visibility = Visibility.Visible;
                tb_sectorsCount.Text = "0";
                this.DataContext = new User();

                col_companyName.Visibility = Visibility.Collapsed;
                col_companyMobile.Visibility = Visibility.Collapsed;
                col_companyEmail.Visibility = Visibility.Collapsed;
                col_companyAvailable.Visibility = Visibility.Collapsed;
                sp_companyDetails.Visibility = Visibility.Collapsed;

            }
            else if (chk_shippingCompanies.IsChecked.Value)
            {
                col_driversUsername.Visibility = Visibility.Collapsed;
                col_driversName.Visibility = Visibility.Collapsed;
                col_driversMobile.Visibility = Visibility.Collapsed;
                sp_driverDetails.Visibility = Visibility.Collapsed;

                col_companyName.Visibility = Visibility.Visible;
                col_companyMobile.Visibility = Visibility.Visible;
                col_companyEmail.Visibility = Visibility.Visible;
                col_companyAvailable.Visibility = Visibility.Visible;
                sp_companyDetails.Visibility = Visibility.Visible;
                this.DataContext = new ShippingCompanies();

            }

            await Search();

            SectionData.EndAwait(grid_main);
        }
        #endregion

        #region events
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
            Instance = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {//refresh
            try
            {
                SectionData.StartAwait(grid_main);

                await FillCombo.RefreshDrivers();
                await RefreshDriversList();

                await FillCombo.RefreshShippingCompaniesAll();
                await RefreshCompaniesList();
                searchText = "";
                tb_search.Text = "";
                await Search();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {//search
            try
            {
                SectionData.StartAwait(grid_main);

                await Search();

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void deliveryType_check(object sender, RoutedEventArgs e)
        {
            try
            {
                SectionData.StartAwait(grid_main);
                CheckBox cb = sender as CheckBox;
                await RefreshOrdersList();
                if (cb.IsFocused)
                {
                    if (cb.IsChecked == true)
                    {
                        if (cb.Name == "chk_drivers")
                        {
                            chk_shippingCompanies.IsChecked = false;
                        }
                        else if (cb.Name == "chk_shippingCompanies")
                        {
                            chk_drivers.IsChecked = false;
                        }
                    }
                }
                
                await changeDeliveryType();
                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void deliveryType_uncheck(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                   if (cb.Name == "chk_drivers")
                        chk_drivers.IsChecked = true;
                   else if (cb.Name == "chk_shippingCompanies")
                        chk_shippingCompanies.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Dg_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//selection
            try
            {
                SectionData.StartAwait(grid_main);
                
                if (dg_user.SelectedIndex != -1)
                {
                    int ordersCount = 0;

                    if (chk_drivers.IsChecked.Value)
                    {
                        driver = dg_user.SelectedItem as User;
                        this.DataContext = driver;
                        if (driver != null)
                        {
                            if (orders != null)
                                driverOrder = orders.Where(o => o.shipUserId == null ? false : (int)o.shipUserId == driver.userId).ToList();

                            if (chk_drivers.IsChecked == true)
                                ordersCount = driverOrder.Count();

                        }
                        tb_driverOrdersCount.Text = ordersCount.ToString();
                    }
                    else if (chk_shippingCompanies.IsChecked.Value)
                    {
                        company = dg_user.SelectedItem as ShippingCompanies;
                        this.DataContext = company;
                        if (company != null)
                        {
                            if (orders != null)
                                driverOrder = orders.Where(o => (int)o.shippingCompanyId == company.shippingCompanyId && o.status == "Ready").ToList();

                            if (chk_shippingCompanies.IsChecked == true)
                                ordersCount = driverOrder.Count();

                        }
                        tb_companyOrdersCount.Text = ordersCount.ToString();
                    }

                }

                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        #endregion

        #region report

        //report  parameters
        ReportCls reportclass = new ReportCls();
        LocalReport rep = new LocalReport();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        // end report parameters


        public void BuildReport()
        {
            List<ReportParameter> paramarr = new List<ReportParameter>();

            string addpath;
            bool isArabic = ReportCls.checkLang();
            if (isArabic)
            {
                addpath = @"\Reports\Delivery\Ar\ArDriversManag.rdlc";
            }
            else
            {
                addpath = @"\Reports\Delivery\En\EnDriversManag.rdlc";
            }
            string reppath = reportclass.PathUp(Directory.GetCurrentDirectory(), 2, addpath);
            string trDeliveryMan = "";
            string deliveryMan = "";
            if (chk_drivers.IsChecked.Value)
            {
                driver = dg_user.SelectedItem as User;

                trDeliveryMan = MainWindow.resourcemanagerreport.GetString("trDriver");
                if (driver != null)
                {
                    deliveryMan = driver.name + " " + driver.lastname;


                }
                else
                {
                    deliveryMan = "-";
                }
            }
            else if (chk_shippingCompanies.IsChecked.Value)
            {
                company = dg_user.SelectedItem as ShippingCompanies;
                trDeliveryMan = MainWindow.resourcemanagerreport.GetString("trShippingCompanynohint");

                if (company != null)
                {
                    deliveryMan = company.name;
                }
                else
                {
                    deliveryMan = "-";
                }
            }

            paramarr.Add(new ReportParameter("trDeliveryMan", trDeliveryMan));
            paramarr.Add(new ReportParameter("deliveryMan", deliveryMan));

            clsReports.driverManagement(driverOrder.ToList(), rep, reppath, paramarr);
            clsReports.setReportLanguage(paramarr);
            clsReports.Header(paramarr);

            rep.SetParameters(paramarr);

            rep.Refresh();

        }

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.StartAwait(grid_main);
                //if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                //{

                #region
                BuildReport();
                LocalReportExtensions.PrintToPrinterbyNameAndCopy(rep, MainWindow.rep_printer_name, AppSettings.rep_print_count == null ? short.Parse("1") : short.Parse(AppSettings.rep_print_count));
                #endregion
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }


        private void Btn_preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SectionData.StartAwait(grid_main);
                //if (FillCombo.groupObject.HasPermissionAction(basicsPermission, FillCombo.groupObjects, "report"))
                //{
                #region
                Window.GetWindow(this).Opacity = 0.2;

                string pdfpath = "";
                //
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
                //}
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);


                SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {

                    Window.GetWindow(this).Opacity = 1;
                SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }


        #endregion

     
    }
}
