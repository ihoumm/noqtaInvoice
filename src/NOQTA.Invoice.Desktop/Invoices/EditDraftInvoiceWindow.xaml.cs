using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;
using NOQTA.Invoice.Application.Invoices.UpdateDraftInvoice;

namespace NOQTA.Invoice.Desktop.Invoices;

public partial class EditDraftInvoiceWindow : Window
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Guid _invoiceId;

    private string _currency = "MAD";
    private bool _isLoading;
    private bool _isSaving;

    public EditDraftInvoiceWindow(
        IServiceScopeFactory scopeFactory,
        Guid invoiceId)
    {
        InitializeComponent();

        _scopeFactory = scopeFactory;
        _invoiceId = invoiceId;

        Loaded += EditDraftInvoiceWindow_Loaded;
    }

    private async void EditDraftInvoiceWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadInvoiceAsync();
    }

    private async Task LoadInvoiceAsync()
    {
        try
        {
            _isLoading = true;

            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        GetInvoiceDetailsService>();

            var invoice =
                await service.ExecuteAsync(_invoiceId);

            if (invoice is null)
            {
                MessageBox.Show(
                    this,
                    "The invoice could not be found.",
                    "Invoice not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

            if (!string.Equals(
                    invoice.Status,
                    "Draft",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    this,
                    "Only draft invoices can be edited.",
                    "Invoice cannot be edited",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
                return;
            }

            DisplayInvoice(invoice);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to load invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Close();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void DisplayInvoice(
        InvoiceDetailResult invoice)
    {
        Title =
            $"Edit Draft Invoice - {invoice.InvoiceNumber}";

        InvoiceNumberTextBlock.Text =
            invoice.InvoiceNumber;

        InvoiceInfoTextBlock.Text =
            $"{invoice.InvoiceType} • {invoice.Currency}";

        CustomerNameTextBlock.Text =
            invoice.CustomerName;

        CustomerEmailTextBlock.Text =
            invoice.CustomerEmail;

        CustomerAddressTextBlock.Text =
            FormatAddress(invoice.CustomerAddress);

        _currency = invoice.Currency;

        TaxRateTextBox.Text =
            invoice.TaxRate.ToString(
                "0.####",
                CultureInfo.CurrentCulture);

        DiscountTextBox.Text =
            invoice.DiscountAmount.ToString(
                "0.##",
                CultureInfo.CurrentCulture);

        ItemsPanel.Children.Clear();

        foreach (var item in invoice.Items)
        {
            AddItemRow(item);
        }

        if (invoice.Items.Count == 0)
        {
            AddItemRow();
        }

        RecalculateTotals();
    }

    private void AddItemButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddItemRow();
    }

    private void AddItemRow()
    {
        AddItemRow(null);
    }

    private void AddItemRow(
        InvoiceDetailItem? item)
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
            Text = item?.Description ?? string.Empty,
            Padding = new Thickness(8, 6, 8, 6),
            Tag = "Description"
        };

        descriptionTextBox.TextChanged += ItemInput_TextChanged;

        Grid.SetColumn(descriptionTextBox, 0);
        row.Children.Add(descriptionTextBox);

        var quantityTextBox = new TextBox
        {
            Text = item?.Quantity.ToString(
                       "0.####",
                       CultureInfo.CurrentCulture)
                   ?? "1",
            Padding = new Thickness(8, 6, 8, 6),
            HorizontalContentAlignment = HorizontalAlignment.Right,
            Tag = "Quantity"
        };

        quantityTextBox.TextChanged += ItemInput_TextChanged;

        Grid.SetColumn(quantityTextBox, 2);
        row.Children.Add(quantityTextBox);

        var unitPriceTextBox = new TextBox
        {
            Text = item?.UnitPrice.ToString(
                       "0.##",
                       CultureInfo.CurrentCulture)
                   ?? "0",
            Padding = new Thickness(8, 6, 8, 6),
            HorizontalContentAlignment = HorizontalAlignment.Right,
            Tag = "UnitPrice"
        };

        unitPriceTextBox.TextChanged += ItemInput_TextChanged;

        Grid.SetColumn(unitPriceTextBox, 4);
        row.Children.Add(unitPriceTextBox);

        var removeButton = new Button
        {
            Content = "Remove",
            Padding = new Thickness(12, 6, 12, 6)
        };

        removeButton.Click += RemoveItemButton_Click;

        Grid.SetColumn(removeButton, 6);
        row.Children.Add(removeButton);

        ItemsPanel.Children.Add(row);
    }

    private void RemoveItemButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.Parent is not Grid row)
            return;

        if (ItemsPanel.Children.Count <= 1)
        {
            MessageBox.Show(
                this,
                "An invoice must contain at least one item.",
                "Cannot remove item",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        ItemsPanel.Children.Remove(row);

        RecalculateTotals();
    }

    private void ItemInput_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_isLoading)
            return;

        RecalculateTotals();
    }

    private void TotalsInput_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_isLoading)
            return;

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        decimal subtotal = 0m;

        foreach (var child in ItemsPanel.Children)
        {
            if (child is not Grid row)
                continue;

            var quantityTextBox =
                GetTextBox(row, 2);

            var unitPriceTextBox =
                GetTextBox(row, 4);

            if (quantityTextBox is null ||
                unitPriceTextBox is null)
            {
                continue;
            }

            if (!TryParseDecimal(
                    quantityTextBox.Text,
                    out var quantity) ||
                quantity <= 0m)
            {
                continue;
            }

            if (!TryParseDecimal(
                    unitPriceTextBox.Text,
                    out var unitPrice) ||
                unitPrice < 0m)
            {
                continue;
            }

            subtotal += quantity * unitPrice;
        }

        var taxRate =
            TryParseDecimal(
                TaxRateTextBox.Text,
                out var parsedTaxRate) &&
            parsedTaxRate >= 0m
                ? parsedTaxRate
                : 0m;

        var discount =
            TryParseDecimal(
                DiscountTextBox.Text,
                out var parsedDiscount) &&
            parsedDiscount >= 0m
                ? parsedDiscount
                : 0m;

        var taxableAmount =
            Math.Max(0m, subtotal - discount);

        var tax =
            taxableAmount * taxRate / 100m;

        var total =
            taxableAmount + tax;

        SubtotalTextBlock.Text =
            FormatMoney(subtotal);

        TaxTextBlock.Text =
            FormatMoney(tax);

        TotalTextBlock.Text =
            FormatMoney(total);
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_isSaving)
            return;

        ValidationTextBlock.Text = string.Empty;

        if (!TryBuildCommand(
                out var command,
                out var validationMessage))
        {
            ValidationTextBlock.Text =
                validationMessage;

            return;
        }

        _isSaving = true;
        SaveButton.IsEnabled = false;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        UpdateDraftInvoiceService>();

            await service.ExecuteAsync(command);

            DialogResult = true;
            Close();
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;

            SaveButton.IsEnabled = true;
        }
        finally
        {
            _isSaving = false;
        }
    }

    private bool TryBuildCommand(
        out UpdateDraftInvoiceCommand command,
        out string validationMessage)
    {
        var items =
            new List<UpdateDraftInvoiceItem>();

        for (var index = 0;
             index < ItemsPanel.Children.Count;
             index++)
        {
            if (ItemsPanel.Children[index] is not Grid row)
            {
                validationMessage =
                    "An invoice item row is invalid.";

                command = null!;
                return false;
            }

            var descriptionTextBox =
                GetTextBox(row, 0);

            var quantityTextBox =
                GetTextBox(row, 2);

            var unitPriceTextBox =
                GetTextBox(row, 4);

            if (descriptionTextBox is null ||
                quantityTextBox is null ||
                unitPriceTextBox is null)
            {
                validationMessage =
                    $"Invoice item {index + 1} is invalid.";

                command = null!;
                return false;
            }

            var description =
                descriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(description))
            {
                validationMessage =
                    $"Description for item {index + 1} is required.";

                command = null!;
                return false;
            }

            if (!TryParseDecimal(
                    quantityTextBox.Text,
                    out var quantity))
            {
                validationMessage =
                    $"Quantity for '{description}' is invalid.";

                command = null!;
                return false;
            }

            if (quantity <= 0m)
            {
                validationMessage =
                    $"Quantity for '{description}' must be greater than zero.";

                command = null!;
                return false;
            }

            if (!TryParseDecimal(
                    unitPriceTextBox.Text,
                    out var unitPrice))
            {
                validationMessage =
                    $"Unit price for '{description}' is invalid.";

                command = null!;
                return false;
            }

            if (unitPrice < 0m)
            {
                validationMessage =
                    $"Unit price for '{description}' cannot be negative.";

                command = null!;
                return false;
            }

            items.Add(
                new UpdateDraftInvoiceItem(
                    description,
                    quantity,
                    unitPrice));
        }

        if (items.Count == 0)
        {
            validationMessage =
                "At least one invoice item is required.";

            command = null!;
            return false;
        }

        if (!TryParseDecimal(
                TaxRateTextBox.Text,
                out var taxRate))
        {
            validationMessage =
                "Tax rate is invalid.";

            command = null!;
            return false;
        }

        if (taxRate < 0m)
        {
            validationMessage =
                "Tax rate cannot be negative.";

            command = null!;
            return false;
        }

        if (!TryParseDecimal(
                DiscountTextBox.Text,
                out var discount))
        {
            validationMessage =
                "Discount amount is invalid.";

            command = null!;
            return false;
        }

        if (discount < 0m)
        {
            validationMessage =
                "Discount amount cannot be negative.";

            command = null!;
            return false;
        }

        command =
            new UpdateDraftInvoiceCommand(
                _invoiceId,
                items,
                taxRate,
                discount);

        validationMessage = string.Empty;

        return true;
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static TextBox? GetTextBox(
        Grid row,
        int column)
    {
        return row.Children
            .OfType<TextBox>()
            .FirstOrDefault(textBox =>
                Grid.GetColumn(textBox) == column);
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

    private string FormatMoney(
        decimal amount)
    {
        return $"{amount:N2} {_currency}";
    }

    private static string FormatAddress(
        InvoiceDetailAddress? address)
    {
        if (address is null)
            return "No address provided.";

        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(address.Street))
            lines.Add(address.Street);

        var cityLine =
            string.Join(
                ", ",
                new[]
                {
                    address.PostalCode,
                    address.City
                }.Where(value =>
                    !string.IsNullOrWhiteSpace(value)));

        if (!string.IsNullOrWhiteSpace(cityLine))
            lines.Add(cityLine);

        if (!string.IsNullOrWhiteSpace(address.Country))
            lines.Add(address.Country);

        return string.Join(
            Environment.NewLine,
            lines);
    }
}