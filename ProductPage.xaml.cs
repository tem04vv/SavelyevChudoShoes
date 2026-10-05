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

namespace SavelyevShoes
{
    /// <summary>
    /// Логика взаимодействия для ShoesPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            var currentProducts= SavelyevShoesEntities.GetContext().Product.ToList();

            foreach(var p in currentProducts)
            {
                if (p.ProductName != null)
                {
                    p.ProductName = p.ProductName.Replace("\r", " ").Replace("\n"," ").Replace("  ", " ");
                }
            }

            ProductListView.ItemsSource = currentProducts;
        }
    }
}
