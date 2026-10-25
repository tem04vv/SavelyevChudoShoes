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
using System.Data.Entity;
using System.Data.Entity.Migrations.Model;

namespace SavelyevShoes
{
    /// <summary>
    /// Логика взаимодействия для ShoesPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public User _currentUser;
        private static Dictionary<int, int> _orderItems = new Dictionary<int, int>();
        public ProductPage(User user)
        {
            InitializeComponent();

            // авторизация
            _currentUser = user;

            if (user != null)
            {
                UserInfoTextBlock.Text = $"Вы вошли как: {user.Surname} {user.Name} ({user.Role.Role1})";
            }
            else
            {
                UserInfoTextBlock.Text = "Вы вошли как гость";
            }

            if (user != null && (user.Role.Role1 == "Менеджер" || user.Role.Role1 == "Администратор"))
            {
                AllOrdersBtn.Visibility = Visibility.Visible;
            }
            var currentProducts= SavelyevShoesEntities.GetContext().Product
                .Include(p => p.StockItem.Select(s => s.Size))
                .ToList();

            foreach(var p in currentProducts)
            {
                if (p.ProductName != null)
                {
                    p.ProductName = p.ProductName.Replace("\r", " ").Replace("\n"," ").Replace("  ", " ");
                }
            }

            ProductListView.ItemsSource = currentProducts;

            SortComboBox.SelectedIndex = 0;
            FilterComboBox.SelectedIndex = 0;
            UpdateProducts();
        }
        public void UpdateProducts()
        {
            var currentProducts = SavelyevShoesEntities.GetContext().Product.ToList();

            // фильтрация
            if (FilterComboBox.SelectedIndex == 1)
            {
                currentProducts = currentProducts.Where(c => c.Category.CategoryName == "Детская обувь").ToList();
            }
            if (FilterComboBox.SelectedIndex == 2)
            {
                currentProducts = currentProducts.Where(c => c.Category.CategoryName == "Женская обувь").ToList();
            }
            if (FilterComboBox.SelectedIndex == 3)
            {
                currentProducts = currentProducts.Where(c => c.Category.CategoryName == "Мужская обувь").ToList();
            }

            // поиск
            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(SearchTextBox.Text.ToLower())).ToList();

            // сортировка
            if (SortComboBox.SelectedIndex == 1)
            {
                currentProducts = currentProducts.OrderByDescending(p => p.Price).ToList();
            }
            if (SortComboBox.SelectedIndex == 2)
            {
                currentProducts = currentProducts.OrderBy(p => p.Price).ToList();
            }
            ProductListView.ItemsSource = currentProducts.ToList();

            UpdateShopBasketButtonVisibility();
        }

        private void AddToOrder(StockItem stockItem)
        {
            int id = stockItem.ID_StockItem;

            if (_orderItems.ContainsKey(id))
            {
                _orderItems[id]++;
            }
            else
            {
                _orderItems[id] = 1;
            }

            UpdateShopBasketButtonVisibility();
            MessageBox.Show($"Товар \"{stockItem.Product.ProductName}\" (размер {stockItem.Size.SizeDisplay}) добавлен к заказу");
        }

        private void UpdateShopBasketButtonVisibility()
        {
            if (_orderItems.Count > 0)
                ShopBasketBtn.Visibility = Visibility.Visible;
            else
                ShopBasketBtn.Visibility = Visibility.Collapsed;
            
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(ProductListView.SelectedItem is Product selectedProduct))
                return;

            if (selectedProduct.StockItem == null || selectedProduct.StockItem.Count == 0)
            {
                MessageBox.Show("Нет доступных размеров");
                return;
            }

            var picker = new SizePickWindow(selectedProduct.StockItem.ToList());
            picker.Owner = Application.Current.MainWindow;

            if (picker.ShowDialog() == true)
            {
                AddToOrder(picker.SelectedStockItem);
            }
        }

        private void ShopBasketBtn_Click(object sender, RoutedEventArgs e)
        {

            if (_orderItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            var orderItems = new List<OrderItem>();
            var context = SavelyevShoesEntities.GetContext();

            foreach (var pair in _orderItems)
            {
                var stockItem = context.StockItem
                    .Include(s => s.Product)
                    .Include(s => s.Size)
                    .FirstOrDefault(s => s.ID_StockItem == pair.Key);

                if (stockItem == null) continue;

                orderItems.Add(new OrderItem
                {
                    ID_StockItem = stockItem.ID_StockItem,
                    Quantity = pair.Value
                });
            }

            var orderWindow = new OrderWindow(orderItems, _currentUser);
            orderWindow.Owner = Application.Current.MainWindow;
            orderWindow.ShowDialog();

            if (orderWindow.DialogResult == true)
            {
                _orderItems.Clear();
                UpdateShopBasketButtonVisibility();
            }
        }

        private void ProductListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_currentUser == null)
            {
                e.Handled = true;
            }
        }

        private void AllOrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new OrdersPage());
        }
    }
}
