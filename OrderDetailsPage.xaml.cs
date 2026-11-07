using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace SavelyevShoes
{
    /// <summary>
    /// Логика взаимодействия для OrderDetailsPage.xaml
    /// </summary>
    public partial class OrderDetailsPage : Page
    {
        private int _orderId;
        private User _currentUser;
        public OrderDetailsPage(int orderId, User user)
        {
            InitializeComponent();
            _orderId = orderId;
            _currentUser = user;

            var context = SavelyevShoesEntities.GetContext();

            // шапка
            var order = context.Order.Include(o => o.User).FirstOrDefault(o => o.ID_Order == _orderId);
            if (order != null)
            {
                OrderNumberTB.Text = order.OrderNumber.ToString();
                OrderDateTB.Text = order.OrderDate.ToString("dd.MM.yyyy");
                ClientNameTB.Text = $"{order.User.Surname} {order.User.Name} {order.User.Patronymic}";
            }

            // загрузка списка товаров
            var orderItems = context.OrderItem
                .Where(oi => oi.ID_Order == _orderId)
                .Include(oi => oi.StockItem.Product)
                .Include(oi => oi.StockItem.Size)
                .ToList();

            OrderDetailsListView.ItemsSource = orderItems;

            decimal totalSum = orderItems.Sum(oi => oi.StockItem.Product.Price * oi.Quantity);
            TotalSumTB.Text = "Итого: " + totalSum.ToString("0.00") + " руб.";
            _currentUser = user;
        }

        private void UpdateTotalSum()
        {
            if (OrderDetailsListView.ItemsSource is List<OrderItem> items)
            {
                decimal totalSum = items.Sum(oi => oi.StockItem.Product.Price * oi.Quantity);
                TotalSumTB.Text = "Итого: " + totalSum.ToString("0.00") + " руб.";
            }
        }

        private void AddCountBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.Role.Role1 != "Администратор")
            {
                MessageBox.Show("Редактировать заказ может только Администратор!");
                return;
            }

            Button button = sender as Button;
            OrderItem item = button.DataContext as OrderItem;
            if (item == null) return;

            if (item.StockItem.AvailableQuantity > item.Quantity)
            {
                item.Quantity++;

                var context = SavelyevShoesEntities.GetContext();
                context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();

                OrderDetailsListView.Items.Refresh();
                UpdateTotalSum();
            }
            else
            {
                MessageBox.Show("На складе больше нет такого количества");
            }
        }

        private void SubtractCountBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.Role.Role1 != "Администратор")
            {
                MessageBox.Show("Редактировать заказ может только Администратор!");
                return;
            }

            Button button = sender as Button;
            OrderItem item = button.DataContext as OrderItem;
            if (item == null) return;

            if (item.Quantity > 1)
            {
                item.Quantity--;

                var context = SavelyevShoesEntities.GetContext();
                context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges(); 

                OrderDetailsListView.Items.Refresh();
                UpdateTotalSum();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.Role.Role1 != "Администратор")
            {
                MessageBox.Show("Редактировать заказ может только Администратор!");
                return;
            }

            Button button = sender as Button;
            OrderItem item = button.DataContext as OrderItem;
            if (item == null) return;

            var result = MessageBox.Show("Удалить этот товар из заказа?", "Удаление", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var context = SavelyevShoesEntities.GetContext();

            var itemToDelete = context.OrderItem.FirstOrDefault(i => i.ID_OrderItem == item.ID_OrderItem);
            if (itemToDelete != null)
            {
                context.OrderItem.Remove(itemToDelete);
                context.SaveChanges();
            }

            var orderItems = context.OrderItem
                .Where(oi => oi.ID_Order == _orderId)
                .Include(oi => oi.StockItem.Product)
                .Include(oi => oi.StockItem.Size)
                .ToList();

            OrderDetailsListView.ItemsSource = orderItems;
            UpdateTotalSum();

            if (orderItems.Count == 0)
            {
                var orderToDelete = context.Order.FirstOrDefault(o => o.ID_Order == _orderId);

                if (orderToDelete != null)
                {
                    context.Order.Remove(orderToDelete);
                    context.SaveChanges();
                }

                MessageBox.Show("Все товары удалены");
                Manager.MainFrame.GoBack();
            }
        }
    }
}
