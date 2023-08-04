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

namespace POS.View.setup
{
    /// <summary>
    /// Interaction logic for uc_selectPos.xaml
    /// </summary>
    public partial class uc_selectPos : UserControl
    {
        public uc_selectPos()
        {
            InitializeComponent();
        }
        private static uc_selectPos _instance;
        public static uc_selectPos Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new uc_selectPos();
                return _instance;
            }
        }


        public int posId { get; set; }
        public int branchId { get; set; }
        public string branchName { get; set; }
        public string posName { get; set; }

        public bool isActivated { get; set; }
 
        BrushConverter bc = new BrushConverter();
        Branch branchModel = new Branch();
        Pos posModel = new Pos();
        Branch branch = new Branch();
        IEnumerable<Branch> branches;
        IEnumerable<Pos> poss;
        SetValues valueModel = new SetValues();
        SettingCls setModel = new SettingCls();
        public int settingsPoSId = 0;
        public int userId;
        string deviceCode = "";
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string motherCode = setupConfiguration.GetMotherBoardID();
                string hardCode = setupConfiguration.GetHDDSerialNo();
                deviceCode = motherCode + "-" + hardCode;

               
                await fillBranch();

                if (isActivated)
                {
                    cb_branch.IsEnabled = false;
                    cb_pos.IsEnabled = false;
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
        private async Task fillBranch()
        {
            branches = await branchModel.GetBranchesActive("b");
            cb_branch.ItemsSource = branches;
            cb_branch.DisplayMemberPath = "name";
            cb_branch.SelectedValuePath = "branchId";
            cb_branch.SelectedValue = branchId;
            //Cb_branch_SelectionChanged(null, null);
            if (branchId == 0)
            {
                cb_pos.ItemsSource = new List<Pos>();
                cb_pos.SelectedIndex = -1;
            }

        }
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    //Btn_save_Click(null, null);
                }
            }
            catch (Exception ex)
            {SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name); }
        }
        List<SettingCls> set = new List<SettingCls>();
      
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
        private void validateEmpty(string name, object sender)
        {
            if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_branch")
                    validateEmptyComboBox((ComboBox)sender, p_errorBranch, tt_errorBranch, "trEmptyBranchToolTip");
                if ((sender as ComboBox).Name == "cb_pos")
                    validateEmptyComboBox((ComboBox)sender, p_errorBranch, tt_errorBranch, "trErrorEmptyPosToolTip");
            }
        }
        private async void Cb_branch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch
            try
            {

                if (sender != null)
                    SectionData.StartAwait(grid_main);

                if (cb_branch.SelectedValue != null)
                {
                    int bId = Convert.ToInt32(cb_branch.SelectedValue);
                    branchId = bId;
                    Branch br =(Branch) cb_branch.SelectedItem;
                    branchName = br.name;
                    List<Pos> pos = new List<Pos>();
                    if (isActivated == false)
                        pos = await posModel.GetUnactivated(branchId);
                    else
                    {
                        var post = await posModel.getById(posId);
                        pos.Add(post);
                    }

                    cb_pos.ItemsSource = pos;
                    cb_pos.DisplayMemberPath = "name";
                    cb_pos.SelectedValuePath = "posId";
                    cb_pos.SelectedValue = posId;
                }
                else
                {
                    cb_pos.ItemsSource = new List<Pos>();
                    cb_pos.SelectedIndex = -1;
                    cb_pos.Text = "";

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
        private void validateEmptyComboBox(ComboBox cb, Path p_error, ToolTip tt_error, string tr)
        {
            if (cb.SelectedIndex == -1)
            {
                p_error.Visibility = Visibility.Visible;
                tt_error.Content = wd_setupOtherPos.resourcemanager.GetString(tr);
                cb.Background = (Brush)bc.ConvertFrom("#15FF0000");
            }
            else
            {
                cb.Background = (Brush)bc.ConvertFrom("#f8f8f8");
                p_error.Visibility = Visibility.Collapsed;

            }
        }

        private void Cb_pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            try
            {
                var pos = (Pos)cb_pos.SelectedItem;
                posName = pos.name;
                posId = (int)cb_pos.SelectedValue;
            }
            catch
            {
                //posId = 0;
            }

        }

       
    }
}
