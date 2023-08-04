using LiveCharts.Wpf;
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
    /// Interaction logic for wd_chart.xaml
    /// </summary>
    public partial class wd_chart : Window
    {
        public string type { get; set; }
        public wd_chart()
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
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                //if (e.Key == Key.Return)
                //{
                //    Btn_select_Click(null, null);
                //}
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
                translat();
                #endregion
                if (type == "cartesianChart")
                {
                    cartesianChart.Visibility = Visibility.Visible;
                    pieChart.Visibility = Visibility.Collapsed;
                    cartesianChartHome.Visibility = Visibility.Collapsed;
                }
                else if (type == "pieChart")
                {
                    cartesianChart.Visibility = Visibility.Collapsed;
                    pieChart.Visibility = Visibility.Visible;
                    cartesianChartHome.Visibility = Visibility.Collapsed;
                }
                else if(type == "cartesianChartHome")
                {
                    cartesianChart.Visibility = Visibility.Collapsed;
                    pieChart.Visibility = Visibility.Collapsed;
                    cartesianChartHome.Visibility = Visibility.Visible;
                }
                //DataContext = this;
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
        private void translat()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("charts");
            btn_print.Content = MainWindow.resourcemanager.GetString("trPrint");

        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void Btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (type == "cartesianChart")
                    PrintingChart.Print(cartesianChart, this);
                else if (type == "pieChart")
                    PrintingChart.Print(pieChart, this);
                else if (type == "cartesianChartHome")
                    PrintingChart.Print(cartesianChartHome, this);
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, false);
            }
        }
    }
}
