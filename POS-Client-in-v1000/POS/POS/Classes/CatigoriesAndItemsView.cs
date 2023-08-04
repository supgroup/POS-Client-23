using POS.Classes;
using POS.controlTemplate;
using POS.View;
using POS.View.catalog;
using POS.View.storage;
using POS.View.windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static POS.View.uc_categorie;

namespace POS.Classes
{
    public class CatigoriesAndItemsView 
    {

        //public string language  { get; set; }
        public uc_categorie ucCategorie;
        public UC_item ucItem;
        public uc_payInvoice ucPayInvoice;
        //public uc_itemsImport ucItemsImport;
        public uc_itemsExport ucItemsExport;
        public uc_itemsDestroy ucItemsDestroy;
        public uc_receiptOfPurchaseInvoice ucreceiptOfPurchaseInvoice;
        public uc_packageOfItems ucPackageOfItems;
        public uc_serviceItem ucServiceItem;
        public UC_users ucUsers;
        public UC_vendors ucVendors;
        public UC_Customer ucCustomer;
        public wd_items wdItems;

        public Grid gridCatigories;
        public Grid gridCatigorieItems;
        private int _idCatigories;
        private int _idItem;
        private int _idPayInvoice;
        private int _idItemsImport;
        private int _idItemsExport;
        private int _idItemsDestroy;
        private int _idReceiptOfPurchaseInvoice;
        private int _idPackageOfItems;
        private int _idServiceItem;
        private int _idUsers;
        private int _idCustomer;
        private int _idVendors;
        private int _idwdItems;
        public int idCatigories
        {
            get => _idCatigories; set
            {
                _idCatigories = value;
                INotifyPropertyChangedIdCatigories();
            }
        }
        public int idItem
        {
            get => _idItem; set
            {
                _idItem = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idPayInvoice
        {
            get => _idPayInvoice; set
            {
                _idPayInvoice = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idItemsImport
        {
            get => _idItemsImport; set
            {
                _idItemsImport = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idItemsExport
        {
            get => _idItemsExport; set
            {
                _idItemsExport = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idItemsDestroy
        {
            get => _idItemsDestroy; set
            {
                _idItemsDestroy = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idReceiptOfPurchaseInvoice
        {
            get => _idReceiptOfPurchaseInvoice; set
            {
                _idReceiptOfPurchaseInvoice = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idPackageOfItems
        {
            get => _idPackageOfItems; set
            {
                _idPackageOfItems = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idServiceItem
        {
            get => _idServiceItem; set
            {
                _idServiceItem = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idUsers
        {
            get => _idUsers; set
            {
                _idUsers = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idCustomer
        {
            get => _idCustomer; set
            {

                _idCustomer = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idVendors
        {
            get => _idVendors; set
            {
                _idVendors = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        public int idwdItems
        {
            get => _idwdItems; set
            {
                _idwdItems = value;
                INotifyPropertyChangedIdCatigorieItems();
            }
        }
        private async void INotifyPropertyChangedIdCatigories()
        {
            try
            {
                if (ucCategorie != null)
            {
                ucCategorie.ChangeCategorieIdEvent(idCatigories);
            }
            else if (ucItem != null)
            {
                await ucItem.ChangeCategoryIdEvent(idCatigories);
            }
            else  if (ucPayInvoice != null)
            {
                ucPayInvoice.ChangeCategoryIdEvent(idCatigories);

            }
            else if (ucItemsExport != null)
            {
                ucItemsExport.ChangeCategoryIdEvent(idCatigories);

            }
            else if (ucItemsDestroy != null)
            {
                //ucItemsDestroy.ChangeCategoryIdEvent(idCatigories);

            }
            else if (ucreceiptOfPurchaseInvoice != null)
            {
                //ucreceiptOfPurchaseInvoice.ChangeCategoryIdEvent(idCatigories);

            }
            else if (ucPackageOfItems != null)
            {
                await ucPackageOfItems.ChangeCategoryIdEvent(idCatigories);

            }
            else if (ucServiceItem != null)
            {
                await ucServiceItem.ChangeCategoryIdEvent(idCatigories);

            }
            else if (wdItems != null)
            {
                await wdItems.ChangeCategoryIdEvent(idCatigories);

            }
            }
            catch (Exception ex)
            {

               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        private async void INotifyPropertyChangedIdCatigorieItems()
        {
            try
            {
                if (ucItem != null)
                {
                    await ucItem.ChangeItemIdEvent(idItem);
                }
                else if (ucPayInvoice != null)
                {
                     ucPayInvoice.ChangeItemIdEvent(idItem);

                }
                else if (ucItemsExport != null)
                {
                    await ucItemsExport.ChangeItemIdEvent(idItem);

                }
                else if (ucreceiptOfPurchaseInvoice != null)
                {
                    //ucreceiptOfPurchaseInvoice.ChangeItemIdEvent(idItem);

                }
                else if (ucPackageOfItems != null)
                {
                    await ucPackageOfItems.ChangeItemIdEvent(idItem);

                }
                else if (ucServiceItem != null)
                {
                    await ucServiceItem.ChangeItemIdEvent(idItem);

                }
                else if (ucUsers != null)
                {
                    ucUsers.ChangeItemIdEvent(idItem);

                }
                else if (ucCustomer != null)
                {
                    ucCustomer.ChangeItemIdEvent(idItem);

                }
                else if (ucVendors != null)
                {
                    await ucVendors.ChangeItemIdEvent(idItem);

                }
                else if (wdItems != null)
                {
                    wdItems.ChangeItemIdEvent(idItem);

                }
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }
        #region Catalog
        public int pastCatalogCard = -1;
         /// ////////////////////drag and drop///////////////////////////////////////
        List<Category> newCategories = new List<Category>();
        //List<updateCategoryuser> categoriesUser = new List<updateCategoryuser>();
        int _columnCount , index = 0 ;
        Category category = new Category();
        Category categoryModel = new Category();
        //Categoryuser categorUserModel = new Categoryuser();
        /// ////////////////////////////////////////////////////////////
       
        public void FN_refrishCatalogCard(List<Category> categories ,int columnCount)
        {
            gridCatigories.Children.Clear();
            int row = 0;
            int column = 0;

            //int[] count = categoriesRowColumnCount(1, columnCount);
            //if (columnCount == -1)
            //    count[1] = -1;
                foreach (var item in categories)
            {
                #region
                /* Orginal Cod
               FN_createCatalogCard(item, row, column, columnCount);
               if (column != -1)
               {
                   column++;
                   if (column == columnCount)
                   {
                       column = 0;
                       row++;
                   }
               }
               else
               {
                   column++;
               }
               */
                #endregion

                CardViewItems itemCardView = new CardViewItems();
                itemCardView.category = item;
                itemCardView.row = row;
                itemCardView.column = column;
                newCategories = categories;
                _columnCount = columnCount;
                FN_createCatalogCard(itemCardView, columnCount);

                if (column != -1)
                    {
                    column++;
                        if (column == columnCount)
                        {
                        column = 0;
                        row++;
                        }
                    }
                    else
                    {
                     column++;
                    }
              
            }
        }

        public UC_squareCard FN_createCatalogCard(CardViewItems categoryCardView, int columnCount,   string BorderBrush = "#DFDFDF")
        {
            UC_squareCard uc = new UC_squareCard(categoryCardView);
            uc.ContentId = categoryCardView.category.categoryId;
            //uc.squareCardText = categoryCardView.category.name;
            uc.squareCardBorderBrush = BorderBrush;
            //uc.squareCardImageSource = image;
            uc.Margin = new Thickness(5, 0, 5, 0);
            //uc.Tag = "categorie" + categoryCardView.category.categoryId;
            uc.Name = "categorie" + categoryCardView.category.categoryId;
            //uc.Row = row;
            //uc.Column = column;
            //uc.rowCount = rowCount;
            uc.columnCount = columnCount;
            Grid.SetColumn(uc, categoryCardView.column);
            Grid.SetRow(uc, categoryCardView.row);
            //uc.Tag = "1";
            gridCatigories.Children.Add(uc);
            //uc.MouseDoubleClick += new MouseButtonEventHandler(catalogCard_MouseDoubleClick);
            //uc.MouseEnter += new MouseEventHandler(UserControl_MouseEnter);
            //////////////////darg and drop////////////////////
            uc.AllowDrop = true;
            uc.MouseDown += this.ucMouseDown;
            uc.DragEnter += this.ucDragEnter;
            uc.MouseUp += this.ucMouseUp;
            //uc.TouchLeave += this.ucTouchLeave;
            //uc.MouseMove += this.ucMouseMove;
            uc.Drop += this.ucDrop;
            ////////////////////////////////////////////////////
            return uc;
        }

        private void ucTouchLeave(object sender, TouchEventArgs e)
        {
            doubleClickCategory(sender);
        }

        private void ucMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(null);
            //if (position != _startPoint)
            //{
            //    //get dragged id
            //    index = newCategories.FindIndex(c => c.categoryId == (sender as UC_squareCard).ContentId);
            //    DragDrop.DoDragDrop(sender as UC_squareCard, (sender as UC_squareCard).ContentId.ToString(), DragDropEffects.All);
            //}
            //else
                doubleClickCategory(sender as UC_squareCard);

        }

        Point _startPoint;
        bool IsDragging = false;
        private void ucMouseMove(object sender, MouseEventArgs e)
        {

            // if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);
                IsDragging = true;

                //if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                //        Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                //{
                //    StartDrag(e);

                //}
            }
        }


        ////////////////darg and drop events/////////////////////
        private async void ucDrop(object sender, DragEventArgs e)
        {
            try
            {
                //get dropped id
                int curIndex = newCategories.FindIndex(c => c.categoryId == (sender as UC_squareCard).ContentId);
                //get dropped category
                category = await categoryModel.getById((sender as UC_squareCard).ContentId);
               // int droppedSequence = category.sequence.Value;
                //get dragged category
                Category dragedCategory = await categoryModel.getById(Convert.ToInt32(e.Data.GetData(DataFormats.Text, true)));
               // int draggedSequence = dragedCategory.sequence.Value;
                //set dropped category
                newCategories[curIndex] = dragedCategory;
                //set dragged category
                newCategories[index] = category;
                //update sequences

               // newCategories[curIndex].sequence = droppedSequence;
               // newCategories[index].sequence = draggedSequence;

                //update displaying list
                FN_refrishCatalogCard(newCategories, _columnCount);
                MessageBox.Show("drop");
                //foreach (var c in newCategories)
                //{
                //    updateCategoryuser catUser = new updateCategoryuser();
                //    catUser.id = c.id.Value;
                //    catUser.userId = MainWindow.userID.Value;
                //    catUser.categoryId = c.categoryId;
                //    //catUser.sequence = c.sequence;
                //    catUser.createUserId = MainWindow.userID.Value;
                //    catUser.updateUserId = MainWindow.userID.Value;
                //    categoriesUser.Add(catUser);
                //}
                //await categorUserModel.UpdateCatUserList(MainWindow.userID.Value, categoriesUser);
            }
            catch //(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }

        private void ucDragEnter(object sender, DragEventArgs e)
            {
                try
                {
                    e.Effects = DragDropEffects.Copy;
            }
            catch //(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }
       
        private void ucMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _startPoint = e.GetPosition(null);
                //if ((e.LeftButton == MouseButtonState.Pressed)&& (e.ClickCount > 0))
                //{
                //    //mousePressed = DateTime.Now;
                //    //DateTime _lastKeystroke = new DateTime(0);
                //    //TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
                //    //MessageBox.Show(elapsed.TotalMilliseconds.ToString());
                //    //if (!elapsed.TotalMilliseconds.ToString().Equals(63765843722372.3) )
                //    //if(IsDragging)
                //    //{
                //        //get dragged id
                //        index = newCategories.FindIndex(c => c.categoryId == (sender as UC_squareCard).ContentId);
                //        DragDrop.DoDragDrop(sender as UC_squareCard, (sender as UC_squareCard).ContentId.ToString(), DragDropEffects.All);
                //      //  MessageBox.Show("drag");
                //    //}
                //    //else
                //    //{
                //        doubleClickCategory(sender as UC_squareCard);
                //      //  MessageBox.Show("click");
                //    //}
                //}
            }
            catch //(Exception ex)
            {
                //SectionData.ExceptionMessage(ex, this);
            }
        }
        ////////////////////////////////////////////////////////////
        ///
        /*
        void doubleClickCategory(object sender)
        {
            try
            {
                UC_squareCard uc = (UC_squareCard)sender;
                uc = gridCatigories.Children.OfType<UC_squareCard>().Where(x => x.Name.ToString() == "categorie" + uc.categoryCardView.category.categoryId).FirstOrDefault();

                gridCatigories.Children.Remove(uc);

                FN_createCatalogCard(uc.categoryCardView, uc.columnCount, "#178DD2");


                if (pastCatalogCard != -1 && pastCatalogCard != uc.categoryCardView.category.categoryId)
                {
                    var pastUc = new UC_squareCard() { ContentId = pastCatalogCard };
                    pastUc = gridCatigories.Children.OfType<UC_squareCard>().Where(x => x.Name.ToString() == "categorie" + pastUc.ContentId).FirstOrDefault();
                    if (pastUc != null)
                    {
                        gridCatigories.Children.Remove(pastUc);
                        FN_createCatalogCard(pastUc.categoryCardView, pastUc.columnCount,
                         "#DFDFDF");
                    }
                }

                pastCatalogCard = uc.categoryCardView.category.categoryId;
                idCatigories = uc.categoryCardView.category.categoryId;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
            void doubleClickCategory(object sender)
        {
            try
            {
                UC_squareCard uc = (UC_squareCard)sender;
                uc = gridCatigories.Children.OfType<UC_squareCard>().Where(x => x.Name.ToString() == "categorie" + uc.categoryCardView.category.categoryId).FirstOrDefault();

                //gridCatigories.Children.Remove(uc);

                //FN_createCatalogCard(uc.categoryCardView, uc.columnCount, "#178DD2");
                uc.squareCardBorderBrush = "#178DD2";


                if (pastCatalogCard != -1 && pastCatalogCard != uc.categoryCardView.category.categoryId)
                {
                    var pastUc = new UC_squareCard() { ContentId = pastCatalogCard };
                    pastUc = gridCatigories.Children.OfType<UC_squareCard>().Where(x => x.Name.ToString() == "categorie" + pastUc.ContentId).FirstOrDefault();
                    if (pastUc != null)
                    {
                        //gridCatigories.Children.Remove(pastUc);
                        //FN_createCatalogCard(pastUc.categoryCardView, pastUc.columnCount,
                        // "#DFDFDF");
                        pastUc.squareCardBorderBrush = "#DFDFDF";
                    }
                }

                pastCatalogCard = uc.categoryCardView.category.categoryId;
                idCatigories = uc.categoryCardView.category.categoryId;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
        #region Catalog Items


        private int pastCatalogItem = -1;
        internal uc_receiptInvoice ucReceiptInvoice;
        public void  FN_refrishCatalogItem(List<Item> items, string language, string cardType)
        {
            gridCatigorieItems.Children.Clear();
            int row = 0;
            int column = 0;
            foreach (var item in items)
            {

                CardViewItems itemCardView = new CardViewItems();
                itemCardView.item = item;
                itemCardView.cardType = cardType;
                itemCardView.language = language;
                itemCardView.row = row;
                itemCardView.column = column;
                FN_createRectangelCard(itemCardView);
               

                column++;
                if (column == 3)
                {
                    column = 0;
                    row++;
                }
            }
        }
        UC_rectangleCard FN_createRectangelCard(CardViewItems itemCardView, string BorderBrush = "#DFDFDF")
        {
            UC_rectangleCard uc = new UC_rectangleCard(itemCardView);
            uc.rectangleCardBorderBrush = BorderBrush;
            uc.Name = "CardName" + itemCardView.item.itemId;
            Grid.SetRow(uc, itemCardView.row);
            Grid.SetColumn(uc, itemCardView.column);
            gridCatigorieItems.Children.Add(uc);
            //uc.MouseDoubleClick += new MouseButtonEventHandler(rectangleCardView_MouseDoubleClick);
            uc.MouseDown += this.ucItemMouseDown;
            return uc;
        }

        private void ucItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 0)
                doubleClickItem(sender);
        }
        /*
        private void doubleClickItem(object sender)
        {
            try
            {
                UC_rectangleCard uc = (UC_rectangleCard)sender;
                uc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + uc.cardViewitem.item.itemId).FirstOrDefault();

                gridCatigorieItems.Children.Remove(uc);
                FN_createRectangelCard(uc.cardViewitem, "#178DD2");
                if (pastCatalogItem != -1 && pastCatalogItem != uc.cardViewitem.item.itemId)
                {
                    var pastUc = new UC_rectangleCard() { contentId = pastCatalogItem };
                    pastUc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + pastUc.contentId).FirstOrDefault();
                    if (pastUc != null)
                    {
                        gridCatigorieItems.Children.Remove(pastUc);
                        FN_createRectangelCard(pastUc.cardViewitem, "#DFDFDF");
                    }
                }
                pastCatalogItem = uc.cardViewitem.item.itemId;
                idItem = uc.cardViewitem.item.itemId;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        */
        private void doubleClickItem(object sender)
        {
            try
            {
                UC_rectangleCard uc = (UC_rectangleCard)sender;
                uc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + uc.cardViewitem.item.itemId).FirstOrDefault();

                //uc.rectangleCardBorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#178DD2"));
                uc.rectangleCardBorderBrush ="#178DD2";

                //gridCatigorieItems.Children.Remove(uc);
                //FN_createRectangelCard(uc.cardViewitem, "#178DD2");
                if (pastCatalogItem != -1 && pastCatalogItem != uc.cardViewitem.item.itemId)
                {
                    var pastUc = new UC_rectangleCard() { contentId = pastCatalogItem };
                    pastUc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + pastUc.contentId).FirstOrDefault();
                    if (pastUc != null)
                    {
                        //    gridCatigorieItems.Children.Remove(pastUc);
                        //    FN_createRectangelCard(pastUc.cardViewitem, "#DFDFDF");
                        pastUc.rectangleCardBorderBrush = "#DFDFDF";
                    }

                }
                pastCatalogItem = uc.cardViewitem.item.itemId;
                idItem = uc.cardViewitem.item.itemId;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #region User
        public void FN_refrishUsers(List<User> users, string language, string cardType)
        {
            gridCatigorieItems.Children.Clear();
            int row = 0;
            int column = 0;
            foreach (var item in users)
            {

                CardViewItems itemCardView = new CardViewItems();
                itemCardView.user = item;
                itemCardView.cardType = cardType;
                itemCardView.language = language;
                itemCardView.row = row;
                itemCardView.column = column;
                FN_createRectangelCardUsers(itemCardView);


                column++;
                if (column == 3)
                {
                    column = 0;
                    row++;
                }
            }
        }
        UC_rectangleCard FN_createRectangelCardUsers(CardViewItems itemCardView, string BorderBrush = "#DFDFDF")
        {
            UC_rectangleCard uc = new UC_rectangleCard(itemCardView);
            uc.rectangleCardBorderBrush = BorderBrush;
            uc.Name = "CardName" + itemCardView.user.userId;
            Grid.SetRow(uc, itemCardView.row);
            Grid.SetColumn(uc, itemCardView.column);
            gridCatigorieItems.Children.Add(uc);
            //uc.MouseDoubleClick += new MouseButtonEventHandler(rectangleCardViewUsers_MouseDoubleClick);
            uc.MouseDown += this.ucUserMouseDown;
            return uc;
        }

        private void ucUserMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 0)
                doubleClickUser(sender);
        }

        private void doubleClickUser(object sender)
        {
            try
            {
                UC_rectangleCard uc = (UC_rectangleCard)sender;
                uc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + uc.cardViewitem.user.userId).FirstOrDefault();

                gridCatigorieItems.Children.Remove(uc);
                FN_createRectangelCardUsers(uc.cardViewitem, "#178DD2");
                if (pastCatalogItem != -1 && pastCatalogItem != uc.cardViewitem.user.userId)
                {
                    var pastUc = new UC_rectangleCard() { contentId = pastCatalogItem };
                    pastUc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + pastUc.contentId).FirstOrDefault();
                    if (pastUc != null)
                    {
                        gridCatigorieItems.Children.Remove(pastUc);
                        FN_createRectangelCardUsers(pastUc.cardViewitem, "#DFDFDF");
                    }
                }
                pastCatalogItem = uc.cardViewitem.user.userId;
                idItem = uc.cardViewitem.user.userId;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }


        #endregion
        #region Agent
        public void FN_refrishAgents(List<Agent> agents, string language, string cardType)
        {
            gridCatigorieItems.Children.Clear();
            int row = 0;
            int column = 0;
            foreach (var item in agents)
            {

                CardViewItems itemCardView = new CardViewItems();
                itemCardView.agent = item;
                itemCardView.cardType = cardType;
                itemCardView.language = language;
                itemCardView.row = row;
                itemCardView.column = column;
                FN_createRectangelCardAgents(itemCardView);


                column++;
                if (column == 3)
                {
                    column = 0;
                    row++;
                }
            }
        }
        UC_rectangleCard FN_createRectangelCardAgents(CardViewItems itemCardView, string BorderBrush = "#DFDFDF")
        {
            UC_rectangleCard uc = new UC_rectangleCard(itemCardView);
            uc.rectangleCardBorderBrush = BorderBrush;
            uc.Name = "CardName" + itemCardView.agent.agentId;
            Grid.SetRow(uc, itemCardView.row);
            Grid.SetColumn(uc, itemCardView.column);
            gridCatigorieItems.Children.Add(uc);
            //uc.MouseDoubleClick += new MouseButtonEventHandler(rectangleCardViewAgents_MouseDoubleClick);
            uc.MouseDown += this.ucAgentMouseDown;
            return uc;
        }

        private void ucAgentMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UC_rectangleCard uc = (UC_rectangleCard)sender;
                uc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + uc.cardViewitem.agent.agentId).FirstOrDefault();

                gridCatigorieItems.Children.Remove(uc);
                FN_createRectangelCardAgents(uc.cardViewitem, "#178DD2");
                if (pastCatalogItem != -1 && pastCatalogItem != uc.cardViewitem.agent.agentId)
                {
                    var pastUc = new UC_rectangleCard() { contentId = pastCatalogItem };
                    pastUc = gridCatigorieItems.Children.OfType<UC_rectangleCard>().Where(x => x.Name.ToString() == "CardName" + pastUc.contentId).FirstOrDefault();
                    if (pastUc != null)
                    {
                        gridCatigorieItems.Children.Remove(pastUc);
                        FN_createRectangelCardAgents(pastUc.cardViewitem, "#DFDFDF");
                    }
                }
                pastCatalogItem = uc.cardViewitem.agent.agentId;
                idItem = uc.cardViewitem.agent.agentId;
            }
            catch (Exception ex)
            {
               SectionData.ExceptionMessage(ex, this, this.GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }


        #endregion
        
        #endregion



    }
}
