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
using static POS.View.windows.wd_setupFirstPos;

namespace POS.View.setup
{
    /// <summary>
    /// Interaction logic for uc_serverConfig.xaml
    /// </summary>
    public partial class uc_serverConfig : UserControl
    {
        public uc_serverConfig()
        {
            InitializeComponent();
        }

        private static uc_serverConfig _instance ;
        public static uc_serverConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_serverConfig();
                return _instance;
            }
        }
        public string serverUri { get; set; }
        public string activationkey { get; set; }
        public int posId { get; set; }
        public int branchId { get; set; }
        public bool isActivated { get; set; }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (cbx_isAlreadyActivate.IsChecked == true)
                isActivated = true;
            //serverUri = activationkey = "";

            //txt_activationkeyTitle.Visibility =
            //    tb_activationkey.Visibility =
            //     Visibility.Visible;
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                tb_msg.Text = "";
                TextBox textBox = sender as TextBox;
                if (textBox.Name.Equals("tb_serverUri"))
                {
                    serverUri = tb_serverUri.Text;

                }
                else if (textBox.Name.Equals("tb_activationkey"))
                {
                    activationkey = tb_activationkey.Text;
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void validateEmpty(string name, object sender)
        {
            if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_serverUri")
                   validateEmptyTextBox(tb_serverUri, p_errorServerUri, tt_errorServerUri, "trEmptyError");
                else if ((sender as TextBox).Name == "tb_activationkey")
                    validateEmptyTextBox(tb_activationkey, p_errorActivationkey, tt_errorActivationkey, "trEmptyError");
            }
            else if (name == "ComboBox")
            {
                //if ((sender as ComboBox).Name == "cb_paymentProcessType")
                //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorpaymentProcessType, tt_errorpaymentProcessType, "trErrorEmptyPaymentTypeToolTip");
                //else if ((sender as ComboBox).Name == "cb_card")
                //    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorCard, tt_errorCard, "trEmptyCardTooltip");
            }
        }
        public static BrushConverter bc = new BrushConverter();
        public static bool validateEmptyTextBox(TextBox tb, Path p_error, ToolTip tt_error, string tr)
        {
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                p_error.Visibility = Visibility.Visible;
                //tt_error.Content = wd_setupFirstPos.resourcemanager.GetString(tr);
                tt_error.Content = "This field cann't be empty";
                tb.Background = (Brush)bc.ConvertFrom("#15FF0000");
                isValid = false;
            }
            else
            {
                tb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_error.Visibility = Visibility.Collapsed;
            }
            return isValid;
        }
        private void Tb_validateEmptyLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = sender.GetType().Name;
                validateEmpty(name, sender);
              
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private async void Cbx_isAlreadyActivate_Checked(object sender, RoutedEventArgs e)
        {

            try {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                if (!tb_serverUri.Text.Equals(""))
                {
                    string url = tb_serverUri.Text;
                    bool validUrl = setupConfiguration.validateUrl(url);
                    if (!validUrl)
                    {
                        tb_msg.Text = "Wrong URI";
                        cbx_isAlreadyActivate.IsChecked = false;
                        isActivated = false;
                    }
                    else
                    {
                        Global.APIUri = url + "/api/";
                        string motherCode = setupConfiguration.GetMotherBoardID();
                        string hardCode = setupConfiguration.GetHDDSerialNo();
                        string deviceCode = motherCode + "-" + hardCode;
                        var pos = await setupConfiguration.checkPreviousActivate(deviceCode);
                        if (!pos.posId.Equals(0))
                        {
                            posId = pos.posId;
                            branchId = (int)pos.branchId;
                            isActivated = true;
                            txt_activationkeyTitle.Visibility =
                            tb_activationkey.Visibility =
                            p_errorActivationkey.Visibility = Visibility.Collapsed;
                            tb_msg.Text = "";
                            tb_activationkey.Text = "A";
                        }
                        else
                        {
                            isActivated = false;
                            cbx_isAlreadyActivate.IsChecked = false;
                            tb_msg.Text = "This device has not been activated before";
                        }
                    }

                }
                else
                {
                    cbx_isAlreadyActivate.IsChecked = false;
                    isActivated = false;
                }

                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
            catch
            {
                if (sender != null)
                    SectionData.EndAwait(grid_main);
            }
        }

        private void Cbx_isAlreadyActivate_Unchecked(object sender, RoutedEventArgs e)
        {
            txt_activationkeyTitle.Visibility =
               tb_activationkey.Visibility =
                Visibility.Visible;
        }
    }
}
