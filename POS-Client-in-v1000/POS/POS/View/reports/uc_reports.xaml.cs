using POS.Classes;
using POS.View.purchases;
using POS.View.reports.deliveryReports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS.View.reports
{
    /// <summary>
    /// Interaction logic for uc_reports.xaml
    /// </summary>
    public partial class uc_reports : UserControl
    {
        private static uc_reports _instance;
        public static uc_reports Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_reports();
                return _instance;
            }
        }

        private void translate()
        {
            btn_salesReports.Content = MainWindow.resourcemanager.GetString("trSales");
            btn_purchaseReports.Content = MainWindow.resourcemanager.GetString("trPurchases");
            btn_storageReports.Content = MainWindow.resourcemanager.GetString("trStorage");
            btn_accountsReports.Content = MainWindow.resourcemanager.GetString("trAccounting");
            btn_deliveryReports.Content = MainWindow.resourcemanager.GetString("trDelivery");
            //btn_usersReports.Content = MainWindow.resourcemanager.GetString("trUsers");
        }

        public uc_reports()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);


                #region menu state
                string menuState = AppSettings.menuIsOpen;
                if (menuState.Equals("open"))
                    ex.IsExpanded = true;
                else
                    ex.IsExpanded = false;
                #endregion

                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucReports.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucReports.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                //btn_salesReports_Click(null, null);

                if (!stopPermission)
                permission();
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
            }
            catch (Exception ex)
            {
                //if (sender != null)
                //    SectionData.EndAwait(grid_main);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public bool stopPermission;
        async void permission()
        {
            bool loadWindow = false;
            var borders = FindControls.FindVisualChildren<Border>(this);
            if (borders.Count() == 0)
                await Task.Delay(0500);
            borders = FindControls.FindVisualChildren<Border>(this);
            if (!SectionData.isAdminPermision())
                foreach (Border border in FindControls.FindVisualChildren<Border>(this))
                {
                    if (border.Tag != null)
                        if (MainWindow.groupObject.HasPermission(border.Tag.ToString(), MainWindow.groupObjects))

                        {
                            border.Visibility = Visibility.Visible;
                            if (!loadWindow)
                            {
                                Button button = FindControls.FindVisualChildren<Button>(this).Where(x => x.Name == "btn_" + border.Tag).FirstOrDefault();
                                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                loadWindow = true;
                            }
                        }
                        else border.Visibility = Visibility.Collapsed;
                if (borders.Count() != 0)
                stopPermission = true;
                }
            else
            {
                foreach (Border border in FindControls.FindVisualChildren<Border>(this))
                    if (border.Tag != null)
                        border.Visibility = Visibility.Visible;
                btn_salesReports_Click(btn_salesReports, null);
                if (borders.Count() != 0)
                stopPermission = true;
            }
        }

        public void refreashBackground()
        {

            btn_salesReports.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_salesReports.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_purchaseReports.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_purchaseReports.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_storageReports.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_storageReports.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_accountsReports.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_accountsReports.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_deliveryReports.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_deliveryReports.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            //btn_usersReports.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            //btn_usersReports.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

        }

        public void refreashBachgroundClick(Button btn)
        {
            refreashBackground();
            btn.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
        }

        private void btn_salesReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_saleReportGeneral uc = new uc_saleReportGeneral();
                refreashBachgroundClick(btn_salesReports);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_purchaseReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_purchaseReportGeneral uc = new uc_purchaseReportGeneral();
                refreashBachgroundClick(btn_purchaseReports);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_storageReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_storage uc = new uc_storage();
                refreashBachgroundClick(btn_storageReports);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void btn_accountsReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_accountant uc = new uc_accountant();
                refreashBachgroundClick(btn_accountsReports);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_deliveryReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //uc_deliveryReports uc = new uc_deliveryReports();
                uc_deliveryReportsGeneral uc = new uc_deliveryReportsGeneral();
                refreashBachgroundClick(btn_deliveryReports);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_usersReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //refreashBachgroundClick(btn_usersReports);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_usersReport.Instance);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Ex_Collapsed(object sender, RoutedEventArgs e)
        {
            try
            {
                int cId = (int)await SectionData.getCloseValueId();
                SectionData.saveMenuState(cId);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Ex_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                int oId = (int)await SectionData.getOpenValueId();
                SectionData.saveMenuState(oId);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        
    }
}
