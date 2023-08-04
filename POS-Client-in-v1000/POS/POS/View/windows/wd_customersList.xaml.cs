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
    /// Interaction logic for wd_customersList.xaml
    /// </summary>
    public partial class wd_customersList : Window
    {
        public wd_customersList()
        {
            InitializeComponent();
        }
        public bool isActive;

        Agent agent = new Agent();
        MedalAgent medalAgent = new MedalAgent();

        List<Agent> allAgents = new List<Agent>();
        List<Agent> allAgentsSource = new List<Agent>();
        List<MedalAgent> selectedAgents = new List<MedalAgent>();

        Agent agentModel = new Agent();
        MedalAgent medalAgentModel = new MedalAgent();
        public string txtAgentSearch;
        public int medalId;
        /// <summary>
        /// Selcted Items if selectedItems Have Items At the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {//load
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_customerList);

                allAgents = (await agentModel.Get("c")).Where(x => x.isActive == 1).ToList();
                allAgentsSource = allAgents;
                selectedAgents = (await medalAgentModel.GetAll()).Where(x => x.medalId == medalId).ToList();

                for (int i  = 0; i < selectedAgents.Count; i++)
                {
                    if (allAgents.Any(s => s.agentId == selectedAgents[i].agentId))
                        agent = await agentModel.getAgentById(selectedAgents[0].agentId.Value);
                        allAgents.Remove(agent);
                }

                dg_allAgents.ItemsSource = allAgents;
                dg_allAgents.SelectedValuePath = "agentId";
                dg_allAgents.DisplayMemberPath = "name";

                dg_selectedAgents.ItemsSource = selectedAgents;
                dg_selectedAgents.SelectedValuePath = "agentId";
                dg_selectedAgents.DisplayMemberPath = "agentName";

                #region translate
                if (AppSettings.lang.Equals("en"))
                { MainWindow.resourcemanager = new ResourceManager("POS.en_file", Assembly.GetExecutingAssembly()); grid_customerList.FlowDirection = FlowDirection.LeftToRight; }
                else
                { MainWindow.resourcemanager = new ResourceManager("POS.ar_file", Assembly.GetExecutingAssembly()); grid_customerList.FlowDirection = FlowDirection.RightToLeft; }

                translat();
                #endregion

                if (sender != null)
                    SectionData.EndAwait(grid_customerList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_customerList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void translat()
        {
            txt_title.Text = MainWindow.resourcemanager.GetString("trCustomers");
            MaterialDesignThemes.Wpf.HintAssist.SetHint(txb_search, MainWindow.resourcemanager.GetString("trSearchHint"));//
            txt_customer.Text = MainWindow.resourcemanager.GetString("trCustomers");
            txt_selectedCustomers.Text = MainWindow.resourcemanager.GetString("trSelectedCustomers");

            tt_selectAllItem.Content = MainWindow.resourcemanager.GetString("trSelectAllItems");
            tt_unselectAllItem.Content = MainWindow.resourcemanager.GetString("trUnSelectAllItems");
            tt_selectItem.Content = MainWindow.resourcemanager.GetString("trSelectOneItem");
            tt_unselectItem.Content = MainWindow.resourcemanager.GetString("trUnSelectOneItem");

            dg_allAgents.Columns[0].Header = MainWindow.resourcemanager.GetString("trCustomer");
            dg_selectedAgents.Columns[0].Header = MainWindow.resourcemanager.GetString("trCustomer");

            btn_save.Content = MainWindow.resourcemanager.GetString("trSave");

        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    Btn_save_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private async void Btn_save_Click(object sender, RoutedEventArgs e)
        {//save
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_customerList);

                string s = await medalAgentModel.UpdateAgentsByMedalId(medalId, selectedAgents, MainWindow.userID.Value);

                isActive = true;
                this.Close();

                if (sender != null)
                    SectionData.EndAwait(grid_customerList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_customerList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_colse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isActive = false;
                this.Close();
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        
        private  void Btn_selectedAll_Click(object sender, RoutedEventArgs e)
        {//select all
            try
            {
                int x = allAgents.Count;
                for (int i = 0; i < x; i++)
                {
                    dg_allAgents.SelectedIndex = 0;
                    Btn_selectedAgent_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private void Btn_selectedAgent_Click(object sender, RoutedEventArgs e)
        {//select one
            try
            {
                agent = dg_allAgents.SelectedItem as Agent;
                if (agent != null)
                {
                    MedalAgent mA = new MedalAgent();
                    mA.id = 0;
                    mA.agentName = agent.name;
                    mA.agentId = agent.agentId;
                    mA.medalId = medalId;

                    allAgents.Remove(agent);

                    selectedAgents.Add(mA);

                    dg_allAgents.ItemsSource = allAgents;
                    dg_selectedAgents.ItemsSource = selectedAgents;

                    dg_allAgents.Items.Refresh();
                    dg_selectedAgents.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void Btn_unSelectedAgent_Click(object sender, RoutedEventArgs e)
        {//unselect one
            try
            {
                medalAgent = dg_selectedAgents.SelectedItem as MedalAgent;
                Agent i = new Agent();
                if (medalAgent != null)
                {
                    int id = medalAgent.agentId.Value;

                    i = await agentModel.getAgentById(id);

                    allAgents.Add(i);

                    selectedAgents.Remove(medalAgent);
                    dg_allAgents.ItemsSource = allAgents;
                    dg_selectedAgents.ItemsSource = selectedAgents;

                    dg_allAgents.Items.Refresh();
                    dg_selectedAgents.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        private  void Btn_unSelectedAll_Click(object sender, RoutedEventArgs e)
        {//unselect all
            try
            { 
                int x = selectedAgents.Count;
                for (int i = 0; i < x; i++)
                {
                    dg_selectedAgents.SelectedIndex = 0;

                    Btn_unSelectedAgent_Click(null, null);
                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private void Txb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                    SectionData.StartAwait(grid_customerList);

                txtAgentSearch = txb_search.Text.ToLower();
                dg_allAgents.ItemsSource = allAgents.Where(x => (x.code.ToLower().Contains(txtAgentSearch) ||
                x.name.ToLower().Contains(txtAgentSearch)
                ) && x.isActive == 1);

                if (sender != null)
                    SectionData.EndAwait(grid_customerList);
            }
            catch (Exception ex)
            {
                if (sender != null)
                    SectionData.EndAwait(grid_customerList);
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Dg_selectedAgents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            { 
                Btn_unSelectedAgent_Click(null, null);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Dg_allAgents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Btn_selectedAgent_Click(null, null);
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
