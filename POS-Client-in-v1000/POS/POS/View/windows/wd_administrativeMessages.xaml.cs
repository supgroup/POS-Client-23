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
    /// Interaction logic for wd_administrativeMessages.xaml
    /// </summary>
    public partial class wd_administrativeMessages : Window
    {
        #region variables
        public bool isOk { get; set; }
        IEnumerable<AdminMessages> messages;
        IEnumerable<MessagesPos> messagesPos;
        AdminMessages message = new AdminMessages();
        MessagesPos messagePos = new MessagesPos();
        List<Pos> posLst = new List<Pos>();
        List<int> _posIdList = new List<int>();
        List<User> usersLst = new List<User>();
        List<int> _userIdList = new List<int>();
        List<int> listisReadedId = new List<int>();
        #endregion

        public wd_administrativeMessages()
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
       
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
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

                if (!MainWindow.groupObject.HasPermissionAction(FillCombo.administrativeMessagesPermission, MainWindow.groupObjects, "one"))
                {
                    col_writeMessage.Width = new GridLength(0, GridUnitType.Pixel);
                    btn_readMessage.BorderThickness = new Thickness(1, 1, 1, 0);
                    MaterialDesignThemes.Wpf.ButtonAssist.SetCornerRadius(btn_readMessage, new CornerRadius(7,7,0,0));
                    btn_readMessage.Background = Application.Current.Resources["MainColor"] as SolidColorBrush;
                    btn_readMessage.BorderBrush = Application.Current.Resources["MainColor"] as SolidColorBrush;
                    path_readMessage.Fill = Application.Current.Resources["White"] as SolidColorBrush;
                    txt_readMessageTitle.Foreground = Application.Current.Resources["White"] as SolidColorBrush;
                    icon_close.Foreground = Application.Current.Resources["White"] as SolidColorBrush;
                    btn_readMessage.Click -= Btn_readMessage_Click;
                }

                #region
                await RefreshMessagesPosList();
                RefreshMessagesPosView();
                #endregion

                #region key up
                cb_branches.IsTextSearchEnabled = false;
                cb_branches.IsEditable = true;
                cb_branches.StaysOpenOnEdit = true;
                cb_branches.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_branches.Text = "";

                cb_user.IsTextSearchEnabled = false;
                cb_user.IsEditable = true;
                cb_user.StaysOpenOnEdit = true;
                cb_user.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_user.Text = "";

                cb_pos.IsTextSearchEnabled = false;
                cb_pos.IsEditable = true;
                cb_pos.StaysOpenOnEdit = true;
                cb_pos.FontFamily = Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                //cb_pos.Text = "";
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
        
        #region methods
        async Task<IEnumerable<AdminMessages>> RefreshMessagesList()
        {//write
            messages = await message.GetByCreatUserId(MainWindow.userLogin.userId);
            return messages;
        }
        void RefreshMessagesView()
        {
            dg_writeMessages.ItemsSource = messages.OrderByDescending(x => x.createDate);
        }
        async Task<IEnumerable<MessagesPos>> RefreshMessagesPosList()
        {//read
            messagesPos = await messagePos.GetByPosIdUserId(MainWindow.posLogIn.posId , MainWindow.userLogin.userId);
            return messagesPos;
        }
        void RefreshMessagesPosView()
        {
            grid_messageDetails.DataContext = new MessagesPos();
            tb_msgContentReply.Text = "";

           
            dg_readMessage.ItemsSource = messagesPos.OrderByDescending(x => x.createDate);

            
        }
        private void translate()
        {
            txt_writeMessageTitle.Text = MainWindow.resourcemanager.GetString("compose");
            txt_readMessageTitle.Text = MainWindow.resourcemanager.GetString("inbox");

            txt_messageDetails.Text = MainWindow.resourcemanager.GetString("messageDetails");
            txt_messageDetails2.Text = MainWindow.resourcemanager.GetString("messageDetails");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_title, MainWindow.resourcemanager.GetString("trTitle")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_msgContent, MainWindow.resourcemanager.GetString("content")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_notes, MainWindow.resourcemanager.GetString("trNoteHint"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_recipients, MainWindow.resourcemanager.GetString("recipients"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPOS")+"...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_user, MainWindow.resourcemanager.GetString("trUser")+"...");

            chk_allBranches.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allPos.Content = MainWindow.resourcemanager.GetString("trAll");
            chk_allUser.Content = MainWindow.resourcemanager.GetString("trAll");

            chk_pos.Content = MainWindow.resourcemanager.GetString("trPOSs");
            chk_user.Content = MainWindow.resourcemanager.GetString("trUsers");

            btn_save.Content = MainWindow.resourcemanager.GetString("send");

            dg_writeMessages.Columns[0].Header = MainWindow.resourcemanager.GetString("trTitle");
            dg_writeMessages.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");

            dg_messagesPos.Columns[0].Header = MainWindow.resourcemanager.GetString("trUser");
            dg_messagesPos.Columns[1].Header = MainWindow.resourcemanager.GetString("trBranch");
            dg_messagesPos.Columns[2].Header = MainWindow.resourcemanager.GetString("trPOS");
            dg_messagesPos.Columns[4].Header = MainWindow.resourcemanager.GetString("trDate");

            dg_readMessage.Columns[0].Header = MainWindow.resourcemanager.GetString("trTitle");
            dg_readMessage.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_readMessage.Columns[2].Header = MainWindow.resourcemanager.GetString("trBranch");
            dg_readMessage.Columns[3].Header = MainWindow.resourcemanager.GetString("trUser");

            txt_messageTitleTitle.Text = MainWindow.resourcemanager.GetString("trTitle");
            txt_dateTitle.Text = MainWindow.resourcemanager.GetString("trDate");
            txt_userNameTitle.Text = MainWindow.resourcemanager.GetString("trUser");
            txt_branchNameTitle.Text = MainWindow.resourcemanager.GetString("trBranch");
            txt_msgContentTitle.Text = MainWindow.resourcemanager.GetString("content");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_msgContentReply, MainWindow.resourcemanager.GetString("reply") + "...");

            MaterialDesignThemes.Wpf.HintAssist.SetHint(txt_recipients, MainWindow.resourcemanager.GetString("recipients"));
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_branches, MainWindow.resourcemanager.GetString("trBranch") + "...");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(cb_pos, MainWindow.resourcemanager.GetString("trPOS") + "...");

            txt_reply.Text = MainWindow.resourcemanager.GetString("reply");
             MaterialDesignThemes.Wpf.HintAssist.SetHint(tb_msgContentReply, MainWindow.resourcemanager.GetString("replyMessage"));

            #region 
            txt_messageTitleTitle.Text = MainWindow.resourcemanager.GetString("trTitle");
            txt_dateTitle.Text = MainWindow.resourcemanager.GetString("trDate");
            txt_branchNameTitle.Text = MainWindow.resourcemanager.GetString("trBranch");
            txt_userNameTitle.Text = MainWindow.resourcemanager.GetString("trUser");
            txt_msgContentTitle.Text = MainWindow.resourcemanager.GetString("content");

            dg_readMessage.Columns[0].Header = MainWindow.resourcemanager.GetString("trTitle");
            dg_readMessage.Columns[1].Header = MainWindow.resourcemanager.GetString("trDate");
            dg_readMessage.Columns[2].Header = MainWindow.resourcemanager.GetString("trBranch");
            dg_readMessage.Columns[3].Header = MainWindow.resourcemanager.GetString("trUser");
            #endregion
        }
        private void Clear()
        {
            grid_writeMessage.DataContext = new AdminMessages();
            chk_allBranches.IsChecked = true;

            SectionData.clearValidate(tb_title, p_errorTitle);
            SectionData.clearValidate(tb_msgContent, p_errorMsgContent);
            SectionData.clearComboBoxValidate(cb_branches, p_errorBranch);
            SectionData.clearComboBoxValidate(cb_pos, p_errorPos);
            SectionData.clearComboBoxValidate(cb_user, p_errorUser);
        }
        private void ClearRecipient()
        {
            chk_allBranches.IsChecked = true;
            chk_allUser.IsChecked = true;
            chk_pos.IsChecked = true;
            chk_user.IsChecked = false;
            sendToType_check(chk_pos , null);

            SectionData.clearComboBoxValidate(cb_branches, p_errorBranch);
            SectionData.clearComboBoxValidate(cb_pos, p_errorPos);
            SectionData.clearComboBoxValidate(cb_user, p_errorUser);
        }
        private void validateEmpty(string name, object sender)
        {
            if (name == "TextBox")
            {
                if ((sender as TextBox).Name == "tb_title")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorTitle, tt_errorTitle, "trIsRequired");
                else if ((sender as TextBox).Name == "tb_msgContent")
                    SectionData.validateEmptyTextBox((TextBox)sender, p_errorMsgContent, tt_error_msgContent, "trIsRequired");
            }
            else if (name == "ComboBox")
            {
                if ((sender as ComboBox).Name == "cb_branches")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorBranch, tt_errorTitle, "trIsRequired");
                else if ((sender as ComboBox).Name == "cb_pos")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorPos, tt_errorPos, "trIsRequired");
                else if ((sender as ComboBox).Name == "cb_user")
                    SectionData.validateEmptyComboBox((ComboBox)sender, p_errorUser, tt_errorUser, "trIsRequired");
            }
        }
        private bool chkIsExist()
        {
            bool isExist = false;

            isExist = messages.Any(m => m.title == tb_title.Text && m.msgContent == tb_msgContent.Text);

            return isExist;
        }
        async Task fillBranches()
        {
            if (FillCombo.branchsActiveList_b is null)
                await FillCombo.RefreshBranchsActive_b();

            cb_branches.SelectedValuePath = "branchId";
            cb_branches.DisplayMemberPath = "name";
            cb_branches.ItemsSource = FillCombo.branchsActiveList_b;
        }
        async Task fillPos(int bID)
        {
            if (FillCombo.posAllReport is null)
                await FillCombo.RefreshPosAllReport();

            cb_pos.SelectedValuePath = "posId";
            cb_pos.DisplayMemberPath = "name";
            cb_pos.ItemsSource = FillCombo.posAllReport.Where(b => b.branchId == bID && b.isActive == 1);
        }
        List<User> users = new List<User>();
        async Task fillUsers()
        {
            if (FillCombo.usersActiveList is null)
                await FillCombo.RefreshUsersActive();
            users = FillCombo.usersActiveList.Where(u => u.userId != MainWindow.userLogin.userId).ToList();
            cb_user.SelectedValuePath = "userId";
            cb_user.DisplayMemberPath = "fullName";
            cb_user.ItemsSource = users;
        }
        void switchTab(bool status, Button button)
        {

            if (status)
            {
                // open
                button.BorderBrush = Application.Current.Resources["Grey"] as SolidColorBrush;
                button.Background = Application.Current.Resources["White"] as SolidColorBrush;
                Path path = FindControls.FindVisualChildren<Path>(button).FirstOrDefault();
                path.Fill = Application.Current.Resources["Grey"] as SolidColorBrush;
                TextBlock textBlock = FindControls.FindVisualChildren<TextBlock>(button).FirstOrDefault();
                textBlock.Foreground = Application.Current.Resources["Grey"] as SolidColorBrush;

            }
            else
            {
                // close
                button.BorderBrush = Application.Current.Resources["Grey"] as SolidColorBrush;
                button.Background = Application.Current.Resources["LightGrey"] as SolidColorBrush;
                Path path = FindControls.FindVisualChildren<Path>(button).FirstOrDefault();
                path.Fill = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush;
                TextBlock textBlock = FindControls.FindVisualChildren<TextBlock>(button).FirstOrDefault();
                textBlock.Foreground = Application.Current.Resources["MainColorlightGrey"] as SolidColorBrush;

            }
            if (button.Name == "btn_readMessage" && status)
                icon_close.Foreground = Application.Current.Resources["Grey"] as SolidColorBrush;
            else
                icon_close.Foreground = Application.Current.Resources["White"] as SolidColorBrush;

        }
        #endregion

        #region events
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            this.Close();
        }
        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {

                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void space_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            SectionData.InputJustNumber(ref textBox);
            e.Handled = e.Key == Key.Space;
        }
        private void Tb_validateEmptyTextChange(object sender, TextChangedEventArgs e)
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
        private void input_LostFocus(object sender, RoutedEventArgs e)
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
        private async void cb_branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select branch
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string name = sender.GetType().Name;
                validateEmpty(name, sender);
               
                if (cb_branches.SelectedItem == null)
                {
                    chk_allPos.IsEnabled = false;
                }
                else
                {
                    chk_allPos.IsEnabled = true;
                    //chk_allPos.IsChecked = true;
                }

                await fillPos((int)cb_branches.SelectedValue);

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
        private void Chk_allBranches_Checked(object sender, RoutedEventArgs e)
        {//select all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.SelectedIndex = -1;
                cb_branches.IsEnabled = false;

                chk_allPos.IsEnabled = false;
                chk_allPos.IsChecked = true;
                cb_pos.SelectedItem = null;
                cb_pos.IsEnabled = false;
                
                SectionData.clearComboBoxValidate(cb_branches, p_errorBranch);

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
        private void Chk_allBranches_Unchecked(object sender, RoutedEventArgs e)
        {//unselect all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_branches.IsEnabled = true;
                chk_allPos.IsEnabled = false;
                chk_allPos.IsChecked = true;
                cb_pos.SelectedItem = null;
                cb_pos.IsEnabled = false;

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
        private void Cb_pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string name = sender.GetType().Name;
                validateEmpty(name, sender);
                SectionData.clearComboBoxValidate(cb_pos, p_errorPos);

                if (cb_pos.SelectedIndex != -1)
                {
                    if (!posLst.Contains((Pos)cb_pos.SelectedItem))
                    {
                        posLst.Add((Pos)cb_pos.SelectedItem);
                        _posIdList.Add((int)cb_pos.SelectedValue);
                    }
                    lst_pos.ItemsSource = null;
                    lst_pos.ItemsSource = posLst;
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
        private void Chk_allPos_Checked(object sender, RoutedEventArgs e)
        {//select all pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                SectionData.clearComboBoxValidate(cb_pos, p_errorPos);
                cb_pos.SelectedIndex = -1;
                cb_pos.IsEnabled = false;
                posLst.Clear();
                _posIdList.Clear();
                lst_pos.ItemsSource = null;

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
        private void Chk_allPos_Unchecked(object sender, RoutedEventArgs e)
        {//unselect all pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_pos.IsEnabled = true;

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
        private void Lst_pos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                posLst = posLst.Where(p => p.posId != (int)lst_pos.SelectedValue).ToList();
                _posIdList.Remove((int)lst_pos.SelectedValue);
                lst_pos.ItemsSource = null;
                lst_pos.ItemsSource = posLst;
                lst_pos.Items.Refresh();
            }
            catch
            {

            }
        }
        private void Lst_users_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                usersLst = usersLst.Where(u => u.userId != (int)lst_users.SelectedValue).ToList();
                _userIdList.Remove((int)lst_users.SelectedValue);
                lst_users.ItemsSource = null;
                lst_users.ItemsSource = usersLst;
                lst_users.Items.Refresh();
            }
            catch
            {

            }
        }
        private async void sendToType_check(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.IsChecked == true)
                    {
                        if (cb.Name == "chk_pos")
                        {
                            chk_user.IsChecked = false;
                        }
                        else if (cb.Name == "chk_user")
                        {
                            chk_pos.IsChecked = false;
                        }
                    }
                }
                if (chk_pos.IsChecked.Value)
                {
                    grid_sendToPos.Visibility = Visibility.Visible;
                    scv_sendToPos.Visibility = Visibility.Visible;

                    grid_sendToUsers.Visibility = Visibility.Collapsed;
                    scv_sendToUsers.Visibility = Visibility.Collapsed;

                    usersLst.Clear();
                    _userIdList.Clear();
                    lst_users.ItemsSource = null;
                    chk_allUser.IsChecked = true;
                }
                else if (chk_user.IsChecked.Value)
                {
                    grid_sendToPos.Visibility = Visibility.Collapsed;
                    scv_sendToPos.Visibility = Visibility.Collapsed;

                    grid_sendToUsers.Visibility = Visibility.Visible;
                    scv_sendToUsers.Visibility = Visibility.Visible;

                    posLst.Clear();
                    _posIdList.Clear();
                    lst_pos.ItemsSource = null;
                    chk_allBranches.IsChecked = true;
                }

            }
            catch (Exception ex)
            {
                SectionData.EndAwait(grid_main);
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void sendToType_uncheck(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb.IsFocused)
                {
                    if (cb.Name == "chk_pos")
                        chk_pos.IsChecked = true;
                    else if (cb.Name == "chk_user")
                        chk_user.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        MessagesPos msgToReply = new MessagesPos();
        private async void Dg_readMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dg_readMessage.SelectedIndex != -1)
                {
                    msgToReply = dg_readMessage.SelectedItem as MessagesPos;
                    grid_messageDetails.DataContext = msgToReply;

                    foreach (var item in messagesPos)
                    {
                        if(item.msgPosId == msgToReply.msgPosId)
                        {
                            item.isReaded = true;
                        }
                    }
                    dg_readMessage.ItemsSource = messagesPos.OrderByDescending(x => x.createDate);

                    listisReadedId.Clear();
                    listisReadedId.Add(msgToReply.msgPosId);
                    await messagePos.updateIsReaded(listisReadedId, MainWindow.userLogin.userId);

                    //msgToReply.isReaded = true;
                    //listisReadedId.Clear();
                    //listisReadedId.Add(msgToReply.msgPosId);
                    //await messagePos.updateIsReaded(listisReadedId, MainWindow.userLogin.userId);
                    //messagesPos.ToList()[1].isReaded = false;
                }
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Dg_writeMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dg_writeMessages.SelectedIndex != -1)
                {
                    col_messagesPos.Height = new GridLength(0, GridUnitType.Star);
                    message = dg_writeMessages.SelectedItem as AdminMessages;
                    grid_writeMessage.DataContext = message;
                }

            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clear();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Chk_allUser_Checked(object sender, RoutedEventArgs e)
        {//select all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_user.SelectedIndex = -1;
                cb_user.IsEnabled = false;

                SectionData.clearComboBoxValidate(cb_user, p_errorUser);

                usersLst.Clear();
                _userIdList.Clear();
                lst_users.ItemsSource = null;

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
        private void Chk_allUser_Unchecked(object sender, RoutedEventArgs e)
        {//unselect all branches
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                cb_user.IsEnabled = true;

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
        private void Cb_user_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//select pos
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                string name = sender.GetType().Name;
                validateEmpty(name, sender);

                if (cb_user.SelectedIndex != -1)
                {
                    if (!usersLst.Contains((User)cb_user.SelectedItem))
                    {
                        usersLst.Add((User)cb_user.SelectedItem);
                        _userIdList.Add((int)cb_user.SelectedValue);
                    }
                    lst_users.ItemsSource = null;
                    lst_users.ItemsSource = usersLst;
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
        private void Cb_branches_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = FillCombo.branchsActiveList_b.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_user_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = users.Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Cb_pos_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var combo = sender as ComboBox;
                var tb = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                var lst = FillCombo.posAllReport.Where(b => b.branchId == (int)cb_branches.SelectedValue && b.isActive == 1);
            tb.FontFamily =Application.Current.Resources["Font-cairo-regular"] as FontFamily;
                combo.ItemsSource = lst.ToList().Where(p => p.name.ToLower().Contains(tb.Text.ToLower())).ToList();
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region tabs
        private async void Btn_writeMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchTab(true, btn_writeMessage);
                brd_writeMessage.Visibility = Visibility.Visible;

                switchTab(false, btn_readMessage);
                brd_readMessage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                col_messagesPos.Height = new GridLength(0, GridUnitType.Pixel);

                await fillBranches();
                chk_allBranches.IsChecked = true;

                await fillUsers();
                chk_allUser.IsChecked = true;

                await RefreshMessagesList();
                RefreshMessagesView();

                lst_pos.ItemsSource = posLst;
                lst_pos.DisplayMemberPath = "name";
                lst_pos.SelectedValuePath = "posId";

                lst_users.ItemsSource = usersLst;
                lst_users.DisplayMemberPath = "fullName";
                lst_users.SelectedValuePath = "userId";
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
        private async void Btn_readMessage_Click(object sender, RoutedEventArgs e)
        {
          
                try
                {
                    switchTab(false, btn_writeMessage);
                    brd_writeMessage.Visibility = Visibility.Collapsed;

                    switchTab(true, btn_readMessage);
                    brd_readMessage.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                await RefreshMessagesPosList();
                RefreshMessagesPosView();

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

        #region datagrid events
        private async void viewPos_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                AdminMessages row = (AdminMessages)dg_writeMessages.SelectedItems[0];

                if (row.msgId != 0)
                {
                    List<MessagesPos> mPos = await messagePos.GetBymsgId(row.msgId);
                    dg_messagesPos.ItemsSource = mPos;
                }
                col_messagesPos.Height = new GridLength(1, GridUnitType.Star);

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
        private async void delete_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);

                AdminMessages row = (AdminMessages)dg_writeMessages.SelectedItems[0];
              
                if (row.msgId != 0)
                {
                    if (row.canDelete)
                    {
                        #region
                        Window.GetWindow(this).Opacity = 0.2;
                        wd_acceptCancelPopup w = new wd_acceptCancelPopup();
                        w.contentText = MainWindow.resourcemanager.GetString("trMessageBoxDelete");
                        w.ShowDialog();
                        Window.GetWindow(this).Opacity = 1;
                        #endregion

                        if (w.isOk)
                        {
                            string popupContent = "";
                            popupContent = MainWindow.resourcemanager.GetString("trPopDelete");

                            int b = (int)await message.Delete(row.msgId, MainWindow.userLogin.userId, true);

                            if (b > 0)
                            {
                                Toaster.ShowSuccess(Window.GetWindow(this), message: popupContent, animation: ToasterAnimation.FadeIn);
                                await RefreshMessagesList();
                                RefreshMessagesView();
                            }
                            else
                                Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                        }
                    }
                   
                    
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
       
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//send
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    #region validate
                    //chk empty name
                    SectionData.validateEmptyTextBox(tb_title, p_errorTitle, tt_errorTitle, "trIsRequired");
                    //chk empty mobile
                    SectionData.validateEmptyTextBox(tb_msgContent, p_errorMsgContent, tt_error_msgContent, "trIsRequired");
                    //chk empty branch
                    bool emptyBranch = false;
                    if (!chk_allBranches.IsChecked.Value && cb_branches.SelectedIndex == -1)
                    {
                        SectionData.validateEmptyComboBox(cb_branches, p_errorBranch, tt_errorBranch, "trIsRequired");
                        emptyBranch = true;
                    }
                    //chk empty pos
                    bool emptyPos = false;
                    if (!chk_allPos.IsChecked.Value && cb_pos.SelectedIndex == -1)
                    {
                        SectionData.validateEmptyComboBox(cb_pos, p_errorPos, tt_errorPos, "trIsRequired");
                        emptyPos = true;
                    }
                    //chk empty user
                    bool emptyUser = false;
                    if (!chk_allUser.IsChecked.Value && cb_user.SelectedIndex == -1)
                    {
                        SectionData.validateEmptyComboBox(cb_user, p_errorUser, tt_errorUser, "trIsRequired");
                        emptyPos = true;
                    }
                    #region
                    //chk if exist
                    //if (chkIsExist())
                    //{
                    //    SectionData.showTextBoxValidate(tb_title, p_errorTitle, tt_errorTitle, "trDuplicateCodeToolTip");
                    //    SectionData.showTextBoxValidate(tb_msgContent, p_errorMsgContent, tt_error_msgContent, "trDuplicateCodeToolTip");
                    //}
                    #endregion
                    #endregion

                    #region send
                    if ((!tb_title.Text.Equals("")) && (!tb_msgContent.Text.Equals("")) && !emptyBranch && !emptyPos && !emptyUser)
                    {
                        message = new AdminMessages();
                        
                        message.title = tb_title.Text;
                        message.msgContent = tb_msgContent.Text;
                        message.isActive = true;
                        message.createUserId = MainWindow.userID;
                        message.notes = tb_notes.Text;
                        message.branchCreatorId = MainWindow.loginBranch.branchId;
                        message.branchCreatorName = MainWindow.loginBranch.name;
                       
                        int s = 0;
                        if (chk_pos.IsChecked.Value)
                        {
                            // send To all pos in all branch
                            if (chk_allBranches.IsChecked.Value)
                                s = (int)await messagePos.SendMessage(message, true);
                            // send To all pos in one branch
                            else if (cb_branches.SelectedIndex != -1 && chk_allPos.IsChecked.Value)
                                s = (int)await messagePos.SendMessage(message, branchId: (int)cb_branches.SelectedValue);
                            // send To specified pos 
                            else if (cb_pos.SelectedIndex != -1)
                                s = (int)await messagePos.SendMessage(message, posIdList: _posIdList);
                        }
                        else if(chk_user.IsChecked.Value)
                        {
                            // send To all users
                            if (chk_allUser.IsChecked.Value)
                                s = (int)await messagePos.SendMessageToUsers(message, true);
                            // send To specified users 
                            else if (cb_user.SelectedIndex != -1)
                                s = (int)await messagePos.SendMessageToUsers(message, userIdList: _userIdList);
                        }
                        if (s > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            Clear();
                            ClearRecipient();
                            await RefreshMessagesList();
                            RefreshMessagesView();
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                    #endregion
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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
        
        private async void Btn_reply_Click(object sender, RoutedEventArgs e)
        {//reply
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_main);
                //if (MainWindow.groupObject.HasPermissionAction(basicsPermission, MainWindow.groupObjects, "add") || SectionData.isAdminPermision())
                {
                    #region validate
                    //chk empty reply
                    SectionData.validateEmptyTextBox(tb_msgContentReply, p_errorMsgContentReply, tt_error_msgContentReply, "trIsRequired");
                    #endregion

                    #region send
                    if (!tb_msgContentReply.Text.Equals("") && msgToReply.msgPosId != 0)
                    {
                        //AdminMessages newMessage = new AdminMessages();
                        //newMessage = await message.GetById(msgToReply.msgId.Value);
                        message = new AdminMessages();
                        message.title = msgToReply.title;
                        message.msgContent = tb_msgContentReply.Text;
                        message.isActive = true;
                        message.notes = msgToReply.notes;
                        message.createUserId = MainWindow.userLogin.userId;
                        message.branchCreatorId = MainWindow.loginBranch.branchId;
                        message.mainMsgId = msgToReply.msgId;
                        
                        _userIdList.Clear();
                        _userIdList.Add(msgToReply.createUserId.Value);
                        int s = 0;
                        s = (int)await messagePos.SendMessageToUsers(message, userIdList: _userIdList);
                        if (s > 0)
                        {
                            Toaster.ShowSuccess(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopAdd"), animation: ToasterAnimation.FadeIn);
                            tb_msgContentReply.Text = "";
                        }
                        else
                            Toaster.ShowWarning(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trPopError"), animation: ToasterAnimation.FadeIn);
                    }
                    #endregion
                }
                //else
                //    Toaster.ShowInfo(Window.GetWindow(this), message: MainWindow.resourcemanager.GetString("trdontHavePermission"), animation: ToasterAnimation.FadeIn);
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

       
    }
}
