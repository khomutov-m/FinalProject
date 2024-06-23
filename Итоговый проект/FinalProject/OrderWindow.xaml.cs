using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FinalProject
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<PickupPoint> pickupPoints = new List<PickupPoint>();
        List<Product> SelectedProducts;
        public double CostOrder { get; set; }
        public Order CurrentOrder { get; set; }
        public double DiscountSum { get; set; }
        private bool isClosingFromButton = false;

        public OrderWindow(Order order, List<Product> selectedProducts)
        {
            InitializeComponent();

            DataContext = this;
            ReceiptCodeTextBlock.DataContext = order;
            OrderDateTextBlock.DataContext = order;
            OrderIdTextBlock.DataContext = order;
            FullNameTextBlock.DataContext = App.CurrentUser;

            CurrentOrder = order;
            SelectedProducts = selectedProducts;

            UpdateDiscountSum(SelectedProducts);
            ProductsListBox.ItemsSource = SelectedProducts;
            GetPickUpPoints();
            PickupPointsComboBox.ItemsSource = pickupPoints;
            PickupPointsComboBox.SelectedIndex =
                pickupPoints.IndexOf(pickupPoints.Where(pickupPoint => pickupPoint.PickupPointId == order.OrderPickupPoint).FirstOrDefault());
            CostOrder = selectedProducts.Sum(selectedProduct => selectedProduct.ProductCostWithDiscount);
        }
        private void GetPickUpPoints()
        {
            try
            {
                pickupPoints = DataAccessLayer.GetPickupPoints();
            }
            catch
            {
                MessageBox.Show("Не удалось получить список пунктов выдачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PickupPointsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentOrder.OrderPickupPoint = pickupPoints[PickupPointsComboBox.SelectedIndex].PickupPointId;
            DataAccessLayer.UpdateOrder(CurrentOrder);
        }

        private void UpdateDiscountSum(List<Product> products)
        {
            foreach (Product product in products)
            {
                if (product.ProductDiscountAmount > 0)
                    DiscountSum += product.ProductDiscountAmount * product.ProductCost*product.ProductAmount*0.01;
            }
        }

        private void SaveTicketButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolder = folderBrowserDialog.SelectedPath;
                string filePath = System.IO.Path.Combine(selectedFolder, "ticket.txt");

                try
                {
                    foreach (Product product in SelectedProducts)
                    {
                        DataAccessLayer.AddProductToOrder(product, CurrentOrder);
                        product.ProductQuantityInStock -= product.ProductAmount;
                        DataAccessLayer.UpdateProduct(product.ProductArticleNumber,product.ProductQuantityInStock);
                    }

                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.WriteLine(
                            $"{OrderDateTextBlock.Text}\n" +
                            $"{OrderIdTextBlock.Text}\n" +
                            $"{OrderCostTextBlock.Text}\n" +
                            $"{DiscountSumTextBlock.Text}\n" +
                            $"Пункт выдачи: {((PickupPoint)PickupPointsComboBox.SelectedItem).PickupPointAddress}\n" +
                            $"{ReceiptCodeTextBlock.Text}\n" +
                            $"Состав заказа: {DataAccessLayer.GetOrderList(CurrentOrder.OrderId)}"
                            );
                    }
                    
                    MessageBox.Show("Талон успешно сохранен в файл ticket.txt", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    isClosingFromButton = true;
                    Close();
                }
                catch
                {
                    MessageBox.Show("Не удалось создать талон", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Product product = button.DataContext as Product;
            try
            {
                DataAccessLayer.RemoveProductFromOrder(product,CurrentOrder);
                SelectedProducts.Remove(product);
                ProductsListBox.Items.Refresh();
            }
            catch
            {
                MessageBox.Show("Не удалось удалить товар из заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AmountProductTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void AmountProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Text == "") 
            {
                textbox.Text = "1";
                return; 
            }
            Product product = textbox.DataContext as Product;
            if (Convert.ToInt32(textbox.Text) > product.ProductQuantityInStock)
                textbox.Text = product.ProductQuantityInStock.ToString();
            if (Convert.ToInt32(textbox.Text) <= 0)
                textbox.Text = "1";
            if(ProductsListBox.SelectedIndex<0)
                ProductsListBox.SelectedIndex = 0;
            product.ProductAmount = Convert.ToInt32(textbox.Text);
            DiscountSum = 0;
            CostOrder = 0;
            UpdateDiscountSum(SelectedProducts);
            DiscountSumTextBlock.Text = $"Сумма скидки: {DiscountSum} руб.";
            CostOrder = SelectedProducts.Sum(selectedProduct => selectedProduct.ProductCostWithDiscount);
            OrderCostTextBlock.Text = $"Итоговая стоимость: {CostOrder} руб.";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosingFromButton)
            { 
                isClosingFromButton = false; 
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены?\nЗаказ не сохраниться", "Закрытие", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
                foreach (Product product in SelectedProducts)
                    DataAccessLayer.RemoveProductFromOrder(product,CurrentOrder);
                DataAccessLayer.RemoveOrder(CurrentOrder.OrderId);
            }
        }
    }
}
