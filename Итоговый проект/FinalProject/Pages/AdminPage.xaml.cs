using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace FinalProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        //массив для передачи фильтров в DataAccessLayer.GetProducts, желательно заменить данную реализацию фильтров на linq-методы(как в заказах)
        string[] conditionsByDiscountRange = ["", "(ProductDiscountAmount < 10)",
            "(ProductDiscountAmount BETWEEN 10 AND 14.99)", "(ProductDiscountAmount >= 15)"];
        List<PickupPoint> pickupPoints = new List<PickupPoint>();
        List<Product> products = new List<Product>();
        List<Order> orders = new List<Order>();
        // спиcок товаров, добавленных к заказу, потом передается в окно просмотра заказа(OrderWindow)
        List<Product> selectedProducts = new List<Product>();
        Order currentOrder;
        public bool Switch = false;

        public AdminPage()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));//стиль страницы
            DataContext = this;
            GetPickUpPoints();
            GetOrders();
            PickupPointsComboBox.ItemsSource = pickupPoints;
            ProductsListBox.ItemsSource = products;
            OrdersListBox.ItemsSource = orders;
            /*выбор элемента по умолчанию(SelectedIndex) для фильтров товаров реализован здесь, а не в xaml, чтобы UpdateProducts не вызывал ошибок
            (должен вызываться только после InitializeComponent())*/
            SortOrderByPriceComboBox.SelectedIndex = 0;
            SortByPriceComboBox.SelectedIndex = 0;
            OrderDiscountRangeComboBox.SelectedIndex = 0;
            DiscountRangeComboBox.SelectedIndex = 0;
            UpdateProducts();
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

        private void ShowOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow(currentOrder, selectedProducts);
            orderWindow.ShowDialog();
            foreach (Product product in selectedProducts)
            {
                if (product.ProductQuantityInStock == 0)
                    product.ProductDescription = "НЕТ В НАЛИЧИИ";
                products.Add(product);
            }
            selectedProducts.Clear();
            Switch = true;
            ProductsScrollViewer.Height += 20;
            ShowOrderButton.Visibility = Visibility.Collapsed;
            ProductsListBox.ItemsSource= products;
            ProductsListBox.Items.Refresh();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(SearchTextBox.Text))
                HintSearchTextBlock.Visibility = Visibility.Visible;
            else
                HintSearchTextBlock.Visibility = Visibility.Collapsed;
            UpdateProducts();
        }

        private void DiscountRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void SortByPriceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            try
            {
                if (DiscountRangeComboBox.SelectedIndex < 0)
                    DiscountRangeComboBox.SelectedIndex = 0;
                products = DataAccessLayer.GetProducts(
                    conditionsByDiscountRange[DiscountRangeComboBox.SelectedIndex],
                    SearchTextBox.Text,
                    SortByPriceComboBox.SelectedIndex
                    );
                ProductsListBox.ItemsSource = products;
                ProductsListBox.Items.Refresh();
                CountProductsTextBlock.Text = $"{products.Count()}/{DataAccessLayer.GetCountAllProducts()}";
            }
            catch
            {
                MessageBox.Show("Не удалось получить список товаров", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentOrder == null || Switch)
                {
                    DataAccessLayer.AddOrder(out currentOrder);
                    ProductsScrollViewer.Height -= 20;
                    ShowOrderButton.Visibility = Visibility.Visible;
                    Switch = false;
                }
                selectedProducts.Add(products[ProductsListBox.SelectedIndex]);
                products.Remove(products[ProductsListBox.SelectedIndex]);
                ProductsListBox.ItemsSource = products;
                ProductsListBox.Items.Refresh();
            }
            catch
            {
                MessageBox.Show("Не удалось добавить товар в заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            OrderDiscountRangeComboBox.SelectedIndex = 0;
            SortOrderByPriceComboBox.SelectedIndex = 0;
        }

        private void OrderDiscountRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetOrders();
            OrderFilter();
        }

        private void OrderFilter()
        {
            switch (OrderDiscountRangeComboBox.SelectedIndex)
            {
                case 0:
                    orders = orders.Where(order => order.OrderTotalDiscount >= 0 && order.OrderTotalDiscount <= 100).ToList();
                    break;
                case 1:
                    orders = orders.Where(order => order.OrderTotalDiscount >= 0 && order.OrderTotalDiscount <= 10).ToList();
                    break;
                case 2:
                    orders = orders.Where(order => order.OrderTotalDiscount >= 11 && order.OrderTotalDiscount <= 14).ToList();
                    break;
                case 3:
                    orders = orders.Where(order => order.OrderTotalDiscount >= 15).ToList();
                    break;
                default: orders = orders.Where(order => order.OrderTotalDiscount >= 0 && order.OrderTotalDiscount <= 100).ToList();break;
            }
            if (SortOrderByPriceComboBox.SelectedIndex == 0)
                orders = orders.OrderBy(order => order.OrderTotalCost).ToList();
            else
                orders = orders.OrderByDescending(order => order.OrderTotalCost).ToList();
            OrdersListBox.ItemsSource = orders;
            OrdersListBox.Items.Refresh();
        }

        private void SortOrderByPriceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OrderFilter();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var order = (Order)OrdersListBox.SelectedItem;
                if (order != null)
                {
                    order.OrderStatus = OrderStatusTextBox.Text;
                    order.OrderPickupPoint = (PickupPointsComboBox.SelectedItem as PickupPoint).PickupPointId;
                    DataAccessLayer.UpdateOrder(order);
                    OrdersListBox.Items.Refresh();
                }
            }
            catch
            {
                MessageBox.Show("Не удалось изменить параметры заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetOrders()
        {
            try
            {
                orders = DataAccessLayer.GetOrders();
            }
            catch
            {
                MessageBox.Show("Не удалось получить список заказов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrdersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOrder = (OrdersListBox.SelectedItem as Order);
            if (selectedOrder != null)
                PickupPointsComboBox.SelectedItem =
                    pickupPoints.Where(point => point.PickupPointId == selectedOrder.OrderPickupPoint).FirstOrDefault();
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Product product = button.DataContext as Product;
            if (product != null)
            {
                try
                {
                    DataAccessLayer.DeleteProduct(product);
                    selectedProducts.Remove(product);
                    products.Remove(product);
                    ProductsListBox.Items.Refresh();
                }
                catch
                {
                    MessageBox.Show("Не удалось удалить выбранный товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChangeProductButton_Click(object sender, RoutedEventArgs e)
        {
            var product = ProductsListBox.SelectedItem as Product;
            if (product != null)
            {
                try
                {
                    ChangeProduct(product);
                    DataAccessLayer.ChangeProduct(product);
                    ProductsListBox.Items.Refresh();
                }
                catch
                {
                    MessageBox.Show("Не удалось изменить товар.\nПроверьте корректность введенных данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
                MessageBox.Show("Товар не выбран", "Не удалось изменить товар", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            Product product = new();
            try
            {
                ChangeProduct(product);
                DataAccessLayer.AddProduct(product);
                products.Add(product);
                ProductsListBox.Items.Refresh();
            }
            catch
            {
                MessageBox.Show("Не удалось добавить товар.\nПроверьте корректность введенных данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ProductChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsScrollViewer.Height > 210)
            {
                ProductsScrollViewer.Height -= 200;
                ProductChangeButton.Content = "Скрыть";
            }
            else
            {
                ProductsScrollViewer.Height += 200;
                ProductChangeButton.Content = "Отобразить редактор товаров";
            }
        }

        private void ChangeProduct(Product product)
        {
            product.ProductName = ProductNameTextBox.Text;
            product.ProductManufacturer = ProductManufacturerTextBox.Text;
            product.ProductDescription = ProductDescriptionTextBox.Text;
            product.ProductArticleNumber = ProducArticleTextBox.Text;
            product.ProductCategory = ProductCategoryTextBox.Text;
            product.ProductCost = Convert.ToDouble(ProductCostTextBox.Text.Replace(".", ","));
            product.ProductQuantityInStock = Convert.ToInt16(ProductQuantityInStockTextBox.Text);
            product.ProductStatus = ProductStatusTextBox.Text;
        }
    }
}
