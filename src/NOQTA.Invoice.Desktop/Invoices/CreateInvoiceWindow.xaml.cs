using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NOQTA.Invoice.Application.Customers.GetCustomers;
using NOQTA.Invoice.Application.Invoices.CreateInvoice;

namespace NOQTA.Invoice.Desktop.Invoices;

public partial class CreateInvoiceWindow : Window
{
    private readonly GetCustomersService _getCustomersService;
    private readonly CreateInvoiceService _createInvoiceService;

    private bool _hasActiveCustomers;

    private bool _isCreatingInvoice;

    public CreateInvoiceWindow(
        GetCustomersService getCustomersService,
        CreateInvoiceService createInvoiceService)
    {
        InitializeComponent();

        _getCustomersService = getCustomersService;
        _createInvoiceService = createInvoiceService;

        AddItemRow();
    }

    private async void Window_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        ValidationTextBlock.Text = string.Empty;
        _hasActiveCustomers = false;
        CustomerComboBox.IsEnabled = false;

        try
        {
            var customers =
                await _getCustomersService.ExecuteAsync();

            var activeCustomers =
                customers
                    .Where(customer => customer.IsActive)
                    .ToList();

            CustomerComboBox.ItemsSource =
                activeCustomers;

            _hasActiveCustomers =
                activeCustomers.Count > 0;

            if (_hasActiveCustomers)
            {
                CustomerComboBox.SelectedIndex = 0;
                CustomerComboBox.IsEnabled = true;
                return;
            }

            ValidationTextBlock.Text =
                "An active customer is required before creating an invoice.";
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;
        }
    }

    private void AddItemButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddItemRow();
    }

    private void AddItemRow()
    {
        var row = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10)
        };

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(4, GridUnitType.Star)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(16)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.2, GridUnitType.Star)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(16)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1.5, GridUnitType.Star)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(16)
            });

        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Auto
            });

        var descriptionTextBox = new TextBox
        {
            Height = 36,
            Padding = new Thickness(8, 6, 8, 6)
        };

        var quantityTextBox = new TextBox
        {
            Height = 36,
            Padding = new Thickness(8, 6, 8, 6),
            Text = "1"
        };

        var unitPriceTextBox = new TextBox
        {
            Height = 36,
            Padding = new Thickness(8, 6, 8, 6),
            Text = "0"
        };

        var removeButton = new Button
        {
            Content = "Remove",
            Height = 36,
            Padding = new Thickness(10, 4, 10, 4)
        };

        Grid.SetColumn(descriptionTextBox, 0);
        Grid.SetColumn(quantityTextBox, 2);
        Grid.SetColumn(unitPriceTextBox, 4);
        Grid.SetColumn(removeButton, 6);

        row.Children.Add(descriptionTextBox);
        row.Children.Add(quantityTextBox);
        row.Children.Add(unitPriceTextBox);
        row.Children.Add(removeButton);

        ItemsPanel.Children.Add(row);

        quantityTextBox.TextChanged += ItemValueChanged;
        unitPriceTextBox.TextChanged += ItemValueChanged;

        removeButton.Click += (_, _) =>
        {
            if (ItemsPanel.Children.Count <= 1)
                return;

            ItemsPanel.Children.Remove(row);
            UpdateSubtotal();
        };
    }

    private void ItemValueChanged(
        object sender,
        TextChangedEventArgs e)
    {
        UpdateSubtotal();
    }

    private void UpdateSubtotal()
    {
        decimal subtotal = 0m;

        foreach (var child in ItemsPanel.Children)
        {
            if (child is not Grid row)
                continue;

            if (row.Children.Count < 3)
                continue;

            var quantityTextBox =
                row.Children[1] as TextBox;

            var unitPriceTextBox =
                row.Children[2] as TextBox;

            if (quantityTextBox is null ||
                unitPriceTextBox is null)
            {
                continue;
            }

            if (!TryParseDecimal(
                    quantityTextBox.Text,
                    out var quantity) ||
                !TryParseDecimal(
                    unitPriceTextBox.Text,
                    out var unitPrice))
            {
                continue;
            }

            if (quantity <= 0m ||
                unitPrice < 0m)
            {
                continue;
            }

            subtotal += quantity * unitPrice;
        }

        SubtotalTextBlock.Text =
            $"{subtotal:N2} {GetSelectedCurrency()}";
    }

    private async void CreateInvoiceButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (_isCreatingInvoice)
            return;

        ValidationTextBlock.Text = string.Empty;
        _isCreatingInvoice = true;

        try
        {
            if (!_hasActiveCustomers)
                throw new InvalidOperationException(
                    "An active customer is required before creating an invoice.");

            var customerId =
                GetSelectedCustomerId();

            var invoiceType =
                GetSelectedInvoiceType();

            var currencyCode =
                GetSelectedCurrency();

            var items =
                GetInvoiceItems();

            var command =
    new CreateInvoiceCommand(
        customerId,
        invoiceType,
        currencyCode,
        items);

            await _createInvoiceService.ExecuteAsync(
                command);

            DialogResult = true;
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;
        }
        finally
        {
            _isCreatingInvoice = false;
        }
    }

    private Guid GetSelectedCustomerId()
    {
        if (CustomerComboBox.SelectedItem
            is not CustomerListItem customer)
        {
            throw new InvalidOperationException(
                "Customer is required.");
        }

        return customer.Id;
    }

    private string GetSelectedInvoiceType()
    {
        if (InvoiceTypeComboBox.SelectedItem
            is not ComboBoxItem item)
        {
            throw new InvalidOperationException(
                "Invoice type is required.");
        }

        return item.Content?.ToString()
            ?? throw new InvalidOperationException(
                "Invoice type is required.");
    }

    private string GetSelectedCurrency()
    {
        if (CurrencyComboBox.SelectedItem
            is not ComboBoxItem item)
        {
            throw new InvalidOperationException(
                "Currency is required.");
        }

        return item.Content?.ToString()
            ?? throw new InvalidOperationException(
                "Currency is required.");
    }

    private IReadOnlyList<CreateInvoiceItem> GetInvoiceItems()
    {
        var items = new List<CreateInvoiceItem>();

        foreach (var child in ItemsPanel.Children)
        {
            if (child is not Grid row)
                continue;

            var descriptionTextBox =
                row.Children[0] as TextBox;

            var quantityTextBox =
                row.Children[1] as TextBox;

            var unitPriceTextBox =
                row.Children[2] as TextBox;

            if (descriptionTextBox is null ||
                quantityTextBox is null ||
                unitPriceTextBox is null)
            {
                continue;
            }

            var description =
                descriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidOperationException(
                    "Item description is required.");

            if (!TryParseDecimal(
                    quantityTextBox.Text,
                    out var quantity))
            {
                throw new InvalidOperationException(
                    $"Invalid quantity for '{description}'.");
            }

            if (!TryParseDecimal(
                    unitPriceTextBox.Text,
                    out var unitPrice))
            {
                throw new InvalidOperationException(
                    $"Invalid unit price for '{description}'.");
            }

            if (quantity <= 0m)
                throw new InvalidOperationException(
                    $"Quantity for '{description}' must be greater than zero.");

            if (unitPrice < 0m)
                throw new InvalidOperationException(
                    $"Unit price for '{description}' cannot be negative.");

            items.Add(
                new CreateInvoiceItem(
                    description,
                    quantity,
                    unitPrice));
        }

        if (items.Count == 0)
            throw new InvalidOperationException(
                "At least one invoice item is required.");

        return items;
    }

    private static bool TryParseDecimal(
        string value,
        out decimal result)
    {
        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out result);
    }
}