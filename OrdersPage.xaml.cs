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

namespace SavelyevShoes
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private User _currentUser;
        public OrdersPage(User user)
        {
            InitializeComponent();

            _currentUser = user;

            var orders = SavelyevShoesEntities.GetContext().Order
                .Include(o => o.User)
                .ToList();
            OrderListView.ItemsSource = orders;
            LoadOrders();
        }

        private void LoadOrders()
        {
            var context = SavelyevShoesEntities.GetContext();
            var orders = context.Order.Include(o => o.User).ToList();
            OrderListView.ItemsSource = orders;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }
        private void OrderListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrderListView.SelectedItem is Order selectedOrder)
            {
                if (selectedOrder != null)
                {
                    Manager.MainFrame.Navigate(new OrderDetailsPage(selectedOrder.ID_Order, _currentUser));
                }
            }
        }

        private void DeleteOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OrderListView.SelectedItem is Order selectedOrder)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот закакз?", "Удаление",
                    MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    return;

                var context = SavelyevShoesEntities.GetContext();

                var itemsToDelete = context.OrderItem
                    .Where(i => i.ID_Order == selectedOrder.ID_Order)
                    .ToList();

                context.OrderItem.RemoveRange(itemsToDelete);

                var orderToDelete = context.Order.FirstOrDefault(o => o.ID_Order == selectedOrder.ID_Order);
                if (orderToDelete != null)
                {
                    context.Order.Remove(orderToDelete);
                }

                context.SaveChanges();

                OrderListView.ItemsSource = context.Order.Include(o => o.User).ToList();
            }
        }
    }
}
