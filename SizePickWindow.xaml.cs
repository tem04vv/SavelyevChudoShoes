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
    /// Логика взаимодействия для SizePickWindow.xaml
    /// </summary>
    public partial class SizePickWindow : Window
    {
        public StockItem SelectedStockItem { get; private set; }
        public SizePickWindow(List<StockItem> items)
        {
            InitializeComponent();
            DataContext = items;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (SizeCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите размер");
                return;
            }
            SelectedStockItem = (StockItem)SizeCombo.SelectedItem;
            DialogResult = true;
            Close();
        }
    }
}
