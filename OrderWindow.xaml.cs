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
using System.Windows.Shapes;

namespace SavelyevShoes
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<OrderItem> _items;
        private User _user;
        public OrderWindow(List<OrderItem> items, User user)
        {
            InitializeComponent();
            _items = items;
            _user = user;

            var context = SavelyevShoesEntities.GetContext();

            foreach (var item in _items)
            {
                var si = context.StockItem
                    .Include("Product")
                    .Include("Size")
                    .FirstOrDefault(s => s.ID_StockItem == item.ID_StockItem);
                item.StockItem = si;
            }

            int newNumber = (context.Order.Max(o => (int?)o.OrderNumber) ?? 0) + 1;

            OrderDateTB.Text = DateTime.Now.ToString("dd.MM.yyyy");
            UserNameTB.Text = $"{user.Surname} {user.Name} {user.Patronymic}";
            OrderNumberTB.Text = newNumber.ToString();
            TotalTB.Text = _items.Sum(i => i.StockItem.Product.Price * i.Quantity).ToString("0.00") + " руб.";

            ItemsListView.ItemsSource = _items;
        }
        

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var context = SavelyevShoesEntities.GetContext();

            int newNumber = (context.Order.Max(o => (int?)o.OrderNumber) ?? 0) + 1;

            var order = new Order
            {
                OrderNumber = newNumber,
                OrderDate = DateTime.Now,
                ID_User = _user.ID_User
            };

            context.Order.Add(order);
            context.SaveChanges();

            foreach (var item in _items)
            {
                context.OrderItem.Add(new OrderItem
                {
                    ID_Order = order.ID_Order,
                    ID_StockItem = item.ID_StockItem,
                    Quantity = item.Quantity
                });

                var stock = context.StockItem.FirstOrDefault(s => s.ID_StockItem == item.ID_StockItem);
                if (stock != null)
                    stock.AvailableQuantity -= item.Quantity;
            }

            context.SaveChanges();

            MessageBox.Show($"Заказ №{order.OrderNumber} оформлен");
            DialogResult = true;
            Close();
        }

        private void AddQuantityBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            OrderItem item = button.DataContext as OrderItem;

            if (item == null)
            {
                return;
            }

            if (item.StockItem.AvailableQuantity > item.Quantity)
            {
                item.Quantity++;
                TotalTB.Text = _items.Sum(i => i.StockItem.Product.Price * i.Quantity).ToString("0.00") + " руб.";
                ItemsListView.Items.Refresh();
            }
            else
            {
                MessageBox.Show("На складе больше нет такого количества");
            }
        }

        private void DeleteQuantityBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            OrderItem item = button.DataContext as OrderItem;

            if (item == null)
            {
                return;
            }

            if (item.Quantity > 1)
            {
                item.Quantity--;
                TotalTB.Text = _items.Sum(i => i.StockItem.Product.Price * i.Quantity).ToString("0.00") + " руб.";
                ItemsListView.Items.Refresh();
            }
        }

        private void DeleteOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            OrderItem item = button.DataContext as OrderItem;

            if (item == null)
            {
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар из заказа?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _items.Remove(item);
                TotalTB.Text = _items.Sum(i => i.StockItem.Product.Price * i.Quantity).ToString("0.00") + " руб.";
                ItemsListView.Items.Refresh();
                if (_items.Count == 0)
                {
                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
