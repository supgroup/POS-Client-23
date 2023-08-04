using POS.Classes;
using POS.View.purchases;
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
    /// Interaction logic for uc_purchases.xaml
    /// </summary>
    /// 

    public partial class uc_purchases : UserControl
    {

        private static uc_purchases _instance;
        public static uc_purchases Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_purchases();
                return _instance;
            }
        }
        public uc_purchases()
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
            btn_payInvoice.Content = MainWindow.resourcemanager.GetString("trInvoice");
            btn_purchaseOrder.Content = MainWindow.resourcemanager.GetString("trOrders");
            //btn_purchaseStatistic.Content = MainWindow.resourcemanager.GetString("trStatistic");


        }
        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
          

            try
            {
                //if (sender != null)
                //    SectionData.StartAwait(grid_main);
                #region translate


                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucPurchases.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucPurchases.FlowDirection = FlowDirection.RightToLeft;
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
                btn_payInvoice_Click(btn_payInvoice, null);
                if (borders.Count() != 0)
                    stopPermission = true;
            }
        }
        void refreashBackground()
        {

            btn_payInvoice.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_payInvoice.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_purchaseOrder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_purchaseOrder.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            //btn_purchaseStatistic.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            //btn_purchaseStatistic.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

        }

        void refreashBachgroundClick(Button btn)
        {
            refreashBackground();
            btn.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
        }
        public void btn_payInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_payInvoice);

                grid_main.Children.Clear();
                grid_main.Children.Add(uc_payInvoice.Instance);
                //uc_payInvoice uc = new uc_payInvoice();
                //grid_main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void Btn_purchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_purchaseOrder);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_purchaseOrder.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
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
                //refreashBachgroundClick(btn_purchaseStatistic);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_statistic.Instance);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
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
