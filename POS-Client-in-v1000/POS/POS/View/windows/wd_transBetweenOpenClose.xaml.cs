using POS.Classes;
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
using System.Windows.Shapes;

namespace POS.View.windows
{
    /// <summary>
    /// Interaction logic for wd_transBetweenOpenClose.xaml
    /// </summary>
    public partial class wd_transBetweenOpenClose : Window
    {

        Statistics statisticsModel = new Statistics();
        IEnumerable<OpenClosOperatinModel> cashesQuery;
        public int openCashTransID = 0 , closeCashTransID = 0;

        public wd_transBetweenOpenClose()
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
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
                translat();
                #endregion

                await fillDataGrid();

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

        async Task fillDataGrid()
        {
            cashesQuery = await statisticsModel.GetTransBetweenOpenClose(openCashTransID , closeCashTransID);
            cashesQuery = cashesQuery.Where(c => c.transType != "c"  );
            dg_transfers.ItemsSource = cashesQuery;
        }

        private void translat()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trOperations");
            dg_transfers.Columns[0].Header = MainWindow.resourcemanager.GetString("trNo");
            dg_transfers.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_transfers.Columns[2].Header = MainWindow.resourcemanager.GetString("trDescription");
            dg_transfers.Columns[3].Header = MainWindow.resourcemanager.GetString("trCashTooltip");
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

        private void Window_Unloaded(object sender, RoutedEventArgs e)
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

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
