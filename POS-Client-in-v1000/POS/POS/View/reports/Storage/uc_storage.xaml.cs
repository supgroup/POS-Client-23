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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS.View.reports
{
    public partial class uc_storage : UserControl
    {
        private static uc_storage _instance;
        public static uc_storage Instance
        {
            get
            {
                if (_instance == null) _instance = new uc_storage();
                return _instance;
            }
        }
        public uc_storage()
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

        private void Btn_stock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_stock uc = new uc_stock();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);

                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_external_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_external uc = new uc_external();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_internal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_internal uc = new uc_internal();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_direct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_direct uc = new uc_direct();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_stocktaking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_stocktaking uc = new uc_stocktaking();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Btn_destroied_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_destroied uc = new uc_destroied();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_serial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uc_serialReport uc = new uc_serialReport();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_itemCost_Click(object sender, RoutedEventArgs e)
        {//item cost
            try
            {
                uc_itemCostReport uc = new uc_itemCostReport();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_properties_Click(object sender, RoutedEventArgs e)
        {//properties
            try
            {
                uc_propertiesReport uc = new uc_propertiesReport();
                main.Children.Add(uc);
                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 2);
                sc_main.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                #region translate
                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    main.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    main.FlowDirection = FlowDirection.RightToLeft;
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
            txt_stockInfo.Text = MainWindow.resourcemanager.GetString("trStock");
            txt_stockHint.Text = MainWindow.resourcemanager.GetString("trItemLocationCollect");

            txt_externalInfo.Text = MainWindow.resourcemanager.GetString("trExternal");
            txt_externalHint.Text = MainWindow.resourcemanager.GetString("trItemsAgentsInvoiceHint");

            txt_internalInfo.Text = MainWindow.resourcemanager.GetString("trInternal");
            txt_internalHint.Text = MainWindow.resourcemanager.GetString("trItemsOperatorHint");

            txt_directInfo.Text = MainWindow.resourcemanager.GetString("trDirectEntry");
            txt_directHint.Text = MainWindow.resourcemanager.GetString("invoicesItemsHint");

            txt_stocktakingInfo.Text = MainWindow.resourcemanager.GetString("trStocktaking");
            txt_stocktakingHint.Text = MainWindow.resourcemanager.GetString("trArchivesShortfallsHint");

            txt_destroiedInfo.Text = MainWindow.resourcemanager.GetString("trDestructive");
            txt_destroiedHint.Text = MainWindow.resourcemanager.GetString("trBranchItemHint");

            txt_serialInfo.Text = MainWindow.resourcemanager.GetString("trSerials");
            txt_serialHint.Text = MainWindow.resourcemanager.GetString("trSerials")+"...";

            txt_itemCostInfo.Text = MainWindow.resourcemanager.GetString("trItemsCost");
            txt_itemCostHint.Text = MainWindow.resourcemanager.GetString("trItemsCost") + "...";

            txt_propertiesInfo.Text = MainWindow.resourcemanager.GetString("trProperties");
            txt_propertiesHint.Text = MainWindow.resourcemanager.GetString("trProperties") + "...";
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
