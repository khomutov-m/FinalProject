using System.Windows;
using System.Windows.Controls;

namespace FinalProject.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        string[] conditionsByDiscountRange = ["", "(ProductDiscountAmount < 10)",
            "(ProductDiscountAmount BETWEEN 10 AND 14.99)", "(ProductDiscountAmount >= 15)"];
        List<Product> products = new List<Product>();
        List<Product> selectedProducts = new List<Product>();
        Order currentOrder;
        public bool Switch = false;

        public ClientPage()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));

            ProductsListBox.ItemsSource = products;
            SortByPriceComboBox.SelectedIndex = 0;
            DiscountRangeComboBox.SelectedIndex = 0;
            UpdateProducts();
        }

        private void ShowOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow(currentOrder, selectedProducts);
            orderWindow.ShowDialog();
            foreach (Product product in selectedProducts)
            {
                if (product.ProductQuantityInStock !=0)
                products.Add(product);
            }
            selectedProducts.Clear();
            Switch = true;
            ProductsScrollViewer.Height += 20;
            ShowOrderButton.Visibility = Visibility.Collapsed;
            ProductsListBox.ItemsSource = products;
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
                products = products.Where(product => product.ProductQuantityInStock != 0).ToList();
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
    }
}
