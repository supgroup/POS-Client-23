using netoaster;
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
    /// Interaction logic for wd_transfers.xaml
    /// </summary>
    public partial class wd_transfers : Window
    {
         public wd_transfers()
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
        CashTransfer cashModel = new CashTransfer();
        IEnumerable<CashTransfer> cashesQuery;
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {

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
        private void translat()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trCashtransfers");
            dg_transfers.Columns[0].Header = MainWindow.resourcemanager.GetString("trTransferNumberTooltip");
            dg_transfers.Columns[1].Header = MainWindow.resourcemanager.GetString("trDepositor");
            dg_transfers.Columns[2].Header = MainWindow.resourcemanager.GetString("trRecepient");
            dg_transfers.Columns[3].Header = MainWindow.resourcemanager.GetString("trCashTooltip");

        }

        async Task fillDataGrid()
        {
            if (MainWindow.groupObject.HasPermissionAction(FillCombo.administrativePosTransfersPermission, MainWindow.groupObjects, "one"))
                cashesQuery = await cashModel.GetCashTransferForPosByUserId("all", "p", MainWindow.userLogin.userId);
            else
                cashesQuery = await cashModel.GetNotConfirmdByPosId("all", "p", (int)MainWindow.posID);

            foreach (var c in cashesQuery)
            {
                if (c.transType.Equals("p"))
                {
                    string s = c.posName;
                    c.posName = c.pos2Name;
                    c.pos2Name = s;
                }
            }

            dg_transfers.ItemsSource = cashesQuery;
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
 
            }
        }

        #region Button In DataGrid

        CashTransfer cashtrans2 = new CashTransfer();
        CashTransfer cashtrans3 = new CashTransfer();
        IEnumerable<CashTransfer> cashes2;

        async void confirmRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                #region Accept
                this.Opacity = 0;
                wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxConfirm");
                w.ShowDialog();
                if (w.isOk)
                {
                    for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                        if (vis is DataGridRow)
                        {
                            Pos posModel = new Pos();
                            CashTransfer row = (CashTransfer)dg_transfers.SelectedItems[0];
                            decimal res = 0;
                            if (MainWindow.groupObject.HasPermissionAction(FillCombo.administrativePosTransfersPermission, MainWindow.groupObjects, "one"))
                            {
                              //  userId

                                res = await cashModel.ConfirmAllAndTrans(row);
                            }

                            else
                            {
                               // posId
                                res = await cashModel.ConfirmAndTrans(row);
                            }


                            //Pos pos = await posModel.getById(row.posId.Value);
                            //Pos pos2 = await posModel.getById(row.pos2Id.Value);

                            if (res >= 0)
                                {

                                    AppSettings.PosBalance = res;
                                    MainWindow.setBalance();
                                    await fillDataGrid();
                                }                          
                                 else if (res.Equals(-2))
                                  Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trIsConfirmed"), animation: ToasterAnimation.FadeIn);
                               
                                else if (res.Equals(-3))
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopNotEnoughBalance"), animation: ToasterAnimation.FadeIn);
                            else if (res == (decimal)-22.2)
                            {
                                await fillDataGrid();
                                Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopConfirm"), animation: ToasterAnimation.FadeIn);
                            }

                            else
                                    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                                await MainWindow.refreshBalance();
                            
                        }
                }
                this.Opacity = 1;
                #endregion

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
     
        
        async void cancelRowinDatagrid(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                    if (vis is DataGridRow)
                    {
                        CashTransfer row = (CashTransfer)dg_transfers.SelectedItems[0];

                        #region get two pos
                        cashes2 = await cashModel.GetbySourcId("p", row.cashTransId);
                        //to insure that the pull operation is in cashtrans2 
                        if (row.transType == "p")
                        {
                            cashtrans2 = cashes2.ToList()[0] as CashTransfer;
                            cashtrans3 = cashes2.ToList()[1] as CashTransfer;
                        }
                        else if (row.transType == "d")
                        {
                            cashtrans2 = cashes2.ToList()[1] as CashTransfer;
                            cashtrans3 = cashes2.ToList()[0] as CashTransfer;
                        }

                        #endregion

                        #region cancel
                        int res = (int)await cashModel.canclePosTrans(cashtrans2.cashTransId, cashtrans3.cashTransId);

                        if (res > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopCanceled"), animation: ToasterAnimation.FadeIn);
                            await fillDataGrid();
                        }
                        else if (res.Equals(-2))
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trIsConfirmed"), animation: ToasterAnimation.FadeIn);

                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);

                        #region old
                        //cashtrans2.isConfirm = 2;
                        //cashtrans3.isConfirm = 2;

                        //decimal s2 = await cashModel.Save(cashtrans2);
                        //decimal s3 = await cashModel.Save(cashtrans3);

                        //if ((!s2.Equals(0)) && (!s3.Equals(0)))
                        //{
                        //    Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopCanceled"), animation: ToasterAnimation.FadeIn);
                        //    await fillDataGrid();
                        //}
                        //else
                        //    Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        #endregion
                        #endregion
                    }

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
        #endregion
    }
}
