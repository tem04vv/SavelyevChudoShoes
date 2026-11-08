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
using System.Security.Cryptography;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Win32;

namespace SavelyevShoes
{
    /// <summary>
    /// Логика взаимодействия для AnalyticsPage.xaml
    /// </summary>
    public partial class AnalyticsPage : Page
    {
        public AnalyticsPage()
        {
            InitializeComponent();

            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            StartDatePicker.SelectedDate = startDate;
            EndDatePicker.SelectedDate = endDate;
        }
        private void UpdateAnalytics()
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                return;
            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            { 
                MessageBox.Show("Дата началы не может быть позже даты окончания!"); 
                return;
            }

            DateTime start = StartDatePicker.SelectedDate.Value.Date;
            DateTime end = EndDatePicker.SelectedDate.Value.Date.AddDays(1);

            var context = SavelyevShoesEntities.GetContext();

            var orders = context.Order
                .Where(o => o.OrderDate >= start && o.OrderDate < end)
                .Include(o => o.OrderItem.Select(oi => oi.StockItem.Product))
                .Include(o => o.OrderItem.Select(oi => oi.StockItem.Product.Manufacturer))
                .ToList();

            TotalOrdersTB.Text = orders.Count.ToString();

            decimal totalSum = 0;
            foreach (var order in orders)
            {
                foreach (var item in order.OrderItem)
                {
                    totalSum += item.Quantity * item.StockItem.Product.Price;
                }
            }
            TotalSumTB.Text = totalSum.ToString("0.00") + " руб.";

            var allItems = orders.SelectMany(o => o.OrderItem).ToList();

            var grouped = allItems
                .GroupBy(oi => new
                {
                    Model = oi.StockItem.Product.ProductName,
                    Manuf = oi.StockItem.Product.Manufacturer.ManufacturerName
                })
                .Select(g => new
                {
                    ModelName = g.Key.Model,
                    ManufacturerName = g.Key.Manuf,
                    TotalQuantity = g.Sum(x => x.Quantity)
                });

            var top5 = grouped
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();
            TopProductsListView.ItemsSource = top5;
        }
        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAnalytics();
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAnalytics();
        }

        private void ExportPdfBtn_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null || StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
                {
                    MessageBox.Show("Некорректный период. Экспорт отменен.");
                    return;
                }
                
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF файлы (*.pdf)|*.pdf";
                sfd.FileName = "Отчет_по_продажам.pdf";

                if (sfd.ShowDialog() != true)
                    return;

                DateTime start = StartDatePicker.SelectedDate.Value.Date;
                DateTime end = EndDatePicker.SelectedDate.Value.Date.AddDays(1);

                var context = SavelyevShoesEntities.GetContext();

                var orders = context.Order
                    .Where(o => o.OrderDate >= start && o.OrderDate < end)
                    .Include(o => o.OrderItem.Select(oi => oi.StockItem.Product))
                    .ToList();

                decimal totalSum = orders.Sum(o => o.OrderItem.Sum(i => i.Quantity * i.StockItem.Product.Price));

                string fontPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, 12);

                Document doc = new Document();
                PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
                doc.Open();

                doc.Add(new iTextSharp.text.Paragraph("Аналитический отчет 'Чудо Обувь'", font));
                doc.Add(new iTextSharp.text.Paragraph($"Период: {start.ToShortDateString()} - {EndDatePicker.SelectedDate.Value.ToShortDateString()}", font));
                doc.Add(new iTextSharp.text.Paragraph($"Всего заказов: {orders.Count}", font));
                doc.Add(new iTextSharp.text.Paragraph($"Общая сумма продаж: {totalSum.ToString("0.00")} руб.", font));
                doc.Add(new iTextSharp.text.Paragraph(" ", font));
                doc.Add(new iTextSharp.text.Paragraph("Топ-5 моделей по продажам:", font));

                foreach (dynamic item in TopProductsListView.Items)
                {
                    string model = item.ModelName;
                    int qty = item.TotalQuantity;
                    doc.Add(new iTextSharp.text.Paragraph($"- {model}: {qty} шт.", font));
                }

                doc.Close();
                MessageBox.Show("Отчет успешно сохранен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании PDF: " + ex.Message);
            }
        }
    }
}
