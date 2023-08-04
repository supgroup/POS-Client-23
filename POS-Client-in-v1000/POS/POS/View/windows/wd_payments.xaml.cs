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
    /// Interaction logic for wd_payments.xaml
    /// </summary>
    public partial class wd_payments : Window
    {
        public ItemTransferInvoice itemTransferInvoice;
        public string processType;
        public string cardName;

        public wd_payments()
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

                translate();
                #endregion

                CashTransfer c = new CashTransfer();
                List<CashTransfer> cLst = new List<CashTransfer>();
                cLst = itemTransferInvoice.cachTransferList;

                if (itemTransferInvoice.totalNet > itemTransferInvoice.cachTransferList.Sum(x => x.cash))
                {
                    c.processType = "balance";
                    c.cash = itemTransferInvoice.totalNet - itemTransferInvoice.cachTransferList.Sum(x => x.cash);

                    cLst.Add(c);
                }

                if (processType != "")
                {
                    if (processType != "multiple")
                        cLst = cLst.Where(i => i.processType == processType).ToList();

                }
                if (cardName != "")
                    cLst = cLst.Where(i => i.cardName == cardName).ToList();

                dg_payments.ItemsSource = cLst;

                //tb_total.Text = itemTransferInvoice.totalNet.ToString();
                tb_total.Text = SectionData.DecTostring(cLst.Sum(x => x.cash));

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

        private void translate()
        {
            txt_payments.Text = MainWindow.resourcemanager.GetString("trPayments");
            txt_total.Text = MainWindow.resourcemanager.GetString("trTotal");
            
            dg_payments.Columns[0].Header = MainWindow.resourcemanager.GetString("trProcessType");
            dg_payments.Columns[1].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

            btn_colse.ToolTip = MainWindow.resourcemanager.GetString("trClose");
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

        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {//close
            this.Close();
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
    }
}
