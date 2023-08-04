using POS.Classes;
using POS.View.storage;
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
    /// Interaction logic for uc_storage.xaml
    /// </summary>
    public partial class uc_storage : UserControl
    {
        private static uc_storage _instance;
        public static uc_storage Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_storage();
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
        private void translate()
        {
            btn_locations.Content = MainWindow.resourcemanager.GetString("trLocation");
            btn_section.Content = MainWindow.resourcemanager.GetString("trSection");
            btn_reciptOfInvoice.Content = MainWindow.resourcemanager.GetString("trInvoice");
            btn_itemsStorage.Content = MainWindow.resourcemanager.GetString("trStorage");
            btn_importExport.Content = MainWindow.resourcemanager.GetString("trMovements");
            btn_itemsDestroy.Content = MainWindow.resourcemanager.GetString("trDestructive");
            btn_serial.Content = MainWindow.resourcemanager.GetString("features");
            btn_shortage.Content = MainWindow.resourcemanager.GetString("trShortage");
            btn_inventory.Content = MainWindow.resourcemanager.GetString("trStocktaking");
            //btn_storageStatistic.Content = MainWindow.resourcemanager.GetString("trStatistic");
        }
        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region menu state
            string menuState = AppSettings.menuIsOpen;
            if (menuState.Equals("open"))
                ex.IsExpanded = true;
            else
                ex.IsExpanded = false;
            #endregion
            //if (sender != null)
            //    SectionData.StartAwait(grid_main);
            #region translate
            try
            {
               

                if (AppSettings.lang.Equals("en"))
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly());
                    grid_ucStorage.FlowDirection = FlowDirection.LeftToRight;
                }
                else
                {
                    MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly());
                    grid_ucStorage.FlowDirection = FlowDirection.RightToLeft;
                }
                translate();

                #endregion
                if(!stopPermission)
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
                Btn_locations_Click(btn_locations, null);
                if (borders.Count() != 0)
                    stopPermission = true;
            }
        }
        void refreashBackground()
        {

            btn_locations.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_locations.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_section.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_section.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_reciptOfInvoice.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_reciptOfInvoice.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_itemsStorage.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_itemsStorage.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));


            //btn_itemsImport.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            //btn_itemsImport.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_importExport.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_importExport.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));


            btn_itemsDestroy.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_itemsDestroy.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_serial.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_serial.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));

            btn_shortage.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_shortage.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));


            btn_inventory.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#67686D"));
            btn_inventory.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
        }
        void refreashBachgroundClick(Button btn)
        {
            refreashBackground();
            btn.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
            btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
        }
        private void Btn_locations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_locations);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_locations.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_section_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_section);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_section.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        public void Btn_receiptOfPurchaseInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_reciptOfInvoice);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_receiptOfPurchaseInvoice.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_itemsStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_itemsStorage);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_itemsStorage.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public void Btn_itemsExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_importExport);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_itemsExport.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_itemsDestroy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_itemsDestroy);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_itemsDestroy.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
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
                refreashBachgroundClick(btn_serial);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_serial.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_shortage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_shortage);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_itemsShortage.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_inventory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refreashBachgroundClick(btn_inventory);
                grid_main.Children.Clear();
                grid_main.Children.Add(uc_inventory.Instance);

                Button button = sender as Button;
                MainWindow.mainWindow.initializationMainTrack(button.Tag.ToString(), 1);
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
