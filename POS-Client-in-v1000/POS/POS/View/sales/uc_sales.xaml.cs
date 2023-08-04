using POS.Classes;
using POS.View.sales;
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

namespace POS.View
{
    /// <summary>
    /// Interaction logic for uc_sales.xaml
    /// </summary>
    public partial class uc_sales : UserControl
    {
        private static uc_sales _instance;
        public static uc_sales Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_sales();
                return _instance;
            }
        }
        public uc_sales()
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
        private void translate()
        {
            btn_reciptInvoice.Content = MainWindow.resourcemanager.GetString("trInvoice");
            btn_salesStatistic.Content = MainWindow.resourcemanager.GetString("trDaily");
            btn_coupon.Content = MainWindow.resourcemanager.GetString("trCoupon");
            btn_offer.Content = MainWindow.resourcemanager.GetString("trOffer");
            //btn_medals.Content = MainWindow.resourcemanager.GetString("trMedals");

            btn_quotation.Content = MainWindow.resourcemanager.GetString("trQuotations");
            btn_slice.Content = MainWindow.resourcemanager.GetString("prices");
            //btn_price.Content = MainWindow.resourcemanager.GetString("pricing");
            btn_salesOrders.Content = MainWindow.resourcemanager.GetString("trOrders");
            //btn_membership.Content = MainWindow.resourcemanager.GetString("trMembership");



        }
        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region translate
            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);

               

                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucSales.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucSales.FlowDirection = FlowDirection.RightToLeft;
                }

                translate();
                #endregion

                if (!stopPermission)
                    permission();

                #region menu state
                string menuState = AppSettings.menuIsOpen;
                if (menuState.Equals("open"))
                    ex.IsExpanded = true;
                else
                    ex.IsExpanded = false;
                #endregion
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
                Btn_receiptInvoice_Click(btn_reciptInvoice, null);
                if (borders.Count() != 0)
                    stopPermission = true;
            }
            
        }
        void refreashBackground()
        {

            btn_reciptInvoice.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_reciptInvoice.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_salesStatistic.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_salesStatistic.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_coupon.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_coupon.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_offer.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_offer.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));


            btn_quotation.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_quotation.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_slice.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_slice.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            //btn_price.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            //btn_price.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_salesOrders.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_salesOrders.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            //btn_medals.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            //btn_medals.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            //btn_membership.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            //btn_membership.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

        }
        void refreashBachgroundClick(Button btn)
        {
            refreashBackground();
            btn.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
        }
        public void Btn_receiptInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_reciptInvoice);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_receiptInvoice.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
                //MainWindow.mainWindow.initializationMainTrack(btn_reciptInvoice.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_statistic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_salesStatistic);
                grid_main.Children.Clear();
                //grid_main.Children.Add(uc_salesStatistic.Instance);
                grid_main.Children.Add(uc_dailySales.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_coupon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_coupon);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_coupon.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_offer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_offer);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_offer.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
       

        public void Btn_quotations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_quotation);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_quotations.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void Btn_slice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_slice);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_slice.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        /*
        public void Btn_price_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_price);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_price.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        public void Btn_orders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_salesOrders);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_orders.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        //private void Btn_medals_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        refreashBachgroundClick(btn_medals);
        //        grid_main.Children.Clear();
        //        grid_main.Children.Add(uc_medals.Instance);
        //        Button button = sender as Button;
        //        MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);

        //    }
        //    catch (Exception ex)
        //    {
        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}

        //private void Btn_membership_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        refreashBachgroundClick(btn_membership);
        //        grid_main.Children.Clear();
        //        grid_main.Children.Add(uc_membership.Instance);
        //        Button button = sender as Button;
        //        MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
        //    }
        //    catch (Exception ex)
        //    {
        //       SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
        //    }
        //}

        private async void Ex_Collapsed(object sender, RoutedEventArgs e)
        {
            try
            {
                int cId = (int)await SectionData.getCloseValueId();
                await SectionData.saveMenuState(cId);
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
                await SectionData.saveMenuState(oId);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        
    }
}
