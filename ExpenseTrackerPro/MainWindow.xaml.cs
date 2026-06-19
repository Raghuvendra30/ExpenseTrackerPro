using ExpenseTrackerPro.Data;
using ExpenseTrackerPro.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExpenseTrackerPro
{
    public partial class MainWindow : Window
    {
        private int selectedExpenseId = -1;
        private void LoadExpenses()
        {
            using var db = new ExpenseDbContext();

            expenseGrid.ItemsSource =
                db.Expenses.ToList();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Select a category");
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Enter a valid amount");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Enter description");
                return;
            }

            using var db = new ExpenseDbContext();

            if (selectedExpenseId == -1)
            {
                Expense expense = new Expense
                {
                    Category = ((ComboBoxItem)cmbCategory.SelectedItem)?.Content?.ToString() ?? "",
                    Amount = amount,
                    Description = txtDescription.Text,
                    Date = DateTime.Now
                };

                db.Expenses.Add(expense);
            }
            else
            {
                var expense = db.Expenses.Find(selectedExpenseId);

                if (expense != null)
                {
                    expense.Category =
                        ((ComboBoxItem)cmbCategory.SelectedItem)?.Content?.ToString() ?? "";

                    expense.Amount = amount;
                    expense.Description = txtDescription.Text;
                }
            }

            string successMessage =
                selectedExpenseId == -1
                ? "Expense added successfully!"
                : "Expense updated successfully!";

            db.SaveChanges();
            MessageBox.Show(
                successMessage,
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadExpenses();
            UpdateDashboard();

            selectedExpenseId = -1;

            btnSave.Content = "Save Expense";

            cmbCategory.SelectedIndex = 0;
            txtAmount.Clear();
            txtDescription.Clear();
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (expenseGrid.SelectedItem is not Expense selectedExpense)
            {
                MessageBox.Show("Please select an expense to delete.");
                return;
            }

            var result = MessageBox.Show(
                "Delete selected expense?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using var db = new ExpenseDbContext();

            var expense = db.Expenses.Find(selectedExpense.Id);

            if (expense != null)
            {
                db.Expenses.Remove(expense);
                db.SaveChanges();
            }

            LoadExpenses();
            UpdateDashboard();

            cmbCategory.SelectedIndex = 0;
            txtAmount.Clear();
            txtDescription.Clear();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            using var db = new ExpenseDbContext();

            string keyword = txtSearch.Text;

            keyword = keyword.Trim().ToLower();

            expenseGrid.ItemsSource = db.Expenses
                .Where(x =>
                    x.Category != null &&
                    x.Category.ToLower().Contains(keyword))
                .ToList();
        }

        private void expenseGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (expenseGrid.SelectedItem is Expense expense)
            {
                selectedExpenseId = expense.Id;

                txtAmount.Text = expense.Amount.ToString();
                txtDescription.Text = expense.Description;

                foreach (ComboBoxItem item in cmbCategory.Items)
                {
                    if (item.Content.ToString() == expense.Category)
                    {
                        cmbCategory.SelectedItem = item;
                        break;
                    }
                }

                btnSave.Content = "Update Expense";
            }
        }
        private void UpdateDashboard()
        {
            using var db = new ExpenseDbContext();

            var expenses = db.Expenses.ToList();

            txtTotalExpense.Text =
                $"₹{expenses.Sum(x => x.Amount):N0}";

            txtTotalEntries.Text =
                expenses.Count.ToString();

            txtHighestExpense.Text =
                expenses.Any()
                ? $"₹{expenses.Max(x => x.Amount):N0}"
                : "₹0";
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            using var db = new ExpenseDbContext();

            var expenses = db.Expenses.ToList();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "expenses.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using StreamWriter writer =
                    new StreamWriter(saveFileDialog.FileName);

                writer.WriteLine("Id,Category,Amount,Description,Date");

                foreach (var expense in expenses)
                {
                    writer.WriteLine(
                        $"{expense.Id}," +
                        $"{expense.Category}," +
                        $"{expense.Amount}," +
                        $"{expense.Description}," +
                        $"{expense.Date}");
                }

                MessageBox.Show(
                    "CSV exported successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "ExpenseTracker Pro v1.0\n\n" +
                "Built with:\n" +
                "• C#\n" +
                "• WPF\n" +
                "• SQLite\n" +
                "• Entity Framework Core\n\n" +
                "Developer: Raghuvendra Pratap Singh",
                "About");
        }
        public MainWindow()
        {
            InitializeComponent();

            using (var db = new ExpenseDbContext())
            {
                db.Database.EnsureCreated();
            }

            LoadExpenses();
            UpdateDashboard();
        }
    }
}