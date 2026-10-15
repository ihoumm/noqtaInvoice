using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Invoices.GetInvoiceDetails;
using NOQTA.Invoice.Application.Invoices.IssueInvoice;
using NOQTA.Invoice.Application.Invoices.MarkInvoiceAsSent;
using NOQTA.Invoice.Application.Invoices.MarkInvoiceAsPaid;
using NOQTA.Invoice.Application.Invoices.GenerateInvoicePdf;
using NOQTA.Invoice.Application.Invoices.CancelInvoice;

namespace NOQTA.Invoice.Desktop.Invoices;

public partial class InvoiceDetailsWindow : Window
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Guid _invoiceId;

    public InvoiceDetailsWindow(
        IServiceScopeFactory scopeFactory,
        Guid invoiceId)
    {
        InitializeComponent();

        _scopeFactory = scopeFactory;
        _invoiceId = invoiceId;

        Loaded += InvoiceDetailsWindow_Loaded;
    }

    private async void InvoiceDetailsWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadInvoiceAsync();
    }

    private async Task LoadInvoiceAsync()
    {
        try
        {
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
    }

    private void DisplayInvoice(
        InvoiceDetailResult invoice)
    {
        Title =
            $"Invoice Details - {invoice.InvoiceNumber}";

        InvoiceNumberTextBlock.Text =
            invoice.InvoiceNumber;

        InvoiceTypeStatusTextBlock.Text =
            $"{invoice.InvoiceType} • {invoice.Status} • {invoice.Currency}";

        EditDraftButton.IsEnabled =
    string.Equals(
        invoice.Status,
        "Draft",
        StringComparison.OrdinalIgnoreCase);

        IssueInvoiceButton.IsEnabled =
    string.Equals(
        invoice.Status,
        "Draft",
        StringComparison.OrdinalIgnoreCase);

        MarkAsSentButton.IsEnabled =
    string.Equals(
        invoice.Status,
        "Issued",
        StringComparison.OrdinalIgnoreCase);

        MarkAsPaidButton.IsEnabled =
    string.Equals(
        invoice.Status,
        "Sent",
        StringComparison.OrdinalIgnoreCase);

        CancelInvoiceButton.IsEnabled =
        string.Equals(
            invoice.Status,
            "Draft",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            invoice.Status,
            "Issued",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            invoice.Status,
            "Sent",
            StringComparison.OrdinalIgnoreCase);

        CreatedAtTextBlock.Text =
            invoice.CreatedAt.ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm");

        CustomerNameTextBlock.Text =
            invoice.CustomerName;

        CustomerEmailTextBlock.Text =
            invoice.CustomerEmail;

        CustomerAddressTextBlock.Text =
            FormatAddress(invoice.CustomerAddress);

        ItemsListView.ItemsSource =
            invoice.Items;

        SubtotalTextBlock.Text =
            FormatMoney(invoice.Subtotal, invoice.Currency);

        DiscountTextBlock.Text =
            FormatMoney(invoice.DiscountAmount, invoice.Currency);

        TaxLabelTextBlock.Text =
            $"Tax ({invoice.TaxRate:N2}%)";

        TaxTextBlock.Text =
            FormatMoney(invoice.TaxAmount, invoice.Currency);

        TotalTextBlock.Text =
            FormatMoney(invoice.Total, invoice.Currency);

        DatesTextBlock.Text =
            FormatDates(invoice);
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

    private static string FormatDates(
        InvoiceDetailResult invoice)
    {
        var dates = new List<string>
        {
            $"Created: {invoice.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm}"
        };

        if (invoice.IssuedAt is not null)
        {
            dates.Add(
                $"Issued: {invoice.IssuedAt.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
        }

        if (invoice.SentAt is not null)
        {
            dates.Add(
                $"Sent: {invoice.SentAt.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
        }

        if (invoice.PaidAt is not null)
        {
            dates.Add(
                $"Paid: {invoice.PaidAt.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
        }

        return string.Join(
            "    •    ",
            dates);
    }

    private static string FormatMoney(
        decimal amount,
        string currency)
    {
        return $"{amount:N2} {currency}";
    }

    private async void GeneratePdfButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        GeneratePdfButton.IsEnabled = false;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        GenerateInvoicePdfService>();

            var result =
                await service.ExecuteAsync(
                    new GenerateInvoicePdfCommand(
                        _invoiceId));

            if (result is null)
            {
                MessageBox.Show(
                    this,
                    "The invoice could not be found.",
                    "Invoice not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBox.Show(
                this,
                $"The PDF was generated successfully.\n\n{result.FilePath}",
                "PDF Generated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to generate PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            GeneratePdfButton.IsEnabled = true;
        }
    }

    private void EditDraftButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var editWindow =
            new EditDraftInvoiceWindow(
                _scopeFactory,
                _invoiceId)
            {
                Owner = this
            };

        var result =
            editWindow.ShowDialog();

        if (result == true)
        {
            _ = LoadInvoiceAsync();
        }
    }

    private async void IssueInvoiceButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            "Issue this invoice?\n\nOnce issued, the invoice can no longer be modified.",
            "Issue Invoice",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        IssueInvoiceButton.IsEnabled = false;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<IssueInvoiceService>();

            await service.ExecuteAsync(
                new IssueInvoiceCommand(_invoiceId));

            await LoadInvoiceAsync();

            MessageBox.Show(
                this,
                "The invoice has been issued successfully.",
                "Invoice Issued",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to issue invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            IssueInvoiceButton.IsEnabled = true;
        }
    }

    private async void MarkAsSentButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            "Mark this invoice as sent?",
            "Mark as Sent",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        MarkAsSentButton.IsEnabled = false;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        MarkInvoiceAsSentService>();

            await service.ExecuteAsync(
                new MarkInvoiceAsSentCommand(_invoiceId));

            await LoadInvoiceAsync();

            MessageBox.Show(
                this,
                "The invoice has been marked as sent successfully.",
                "Invoice Sent",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to mark invoice as sent",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            MarkAsSentButton.IsEnabled = true;
        }
    }

    private async void MarkAsPaidButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            "Mark this invoice as paid?",
            "Mark as Paid",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        MarkAsPaidButton.IsEnabled = false;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        MarkInvoiceAsPaidService>();

            await service.ExecuteAsync(
                new MarkInvoiceAsPaidCommand(_invoiceId));

            await LoadInvoiceAsync();

            MessageBox.Show(
                this,
                "The invoice has been marked as paid successfully.",
                "Invoice Paid",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to mark invoice as paid",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            MarkAsPaidButton.IsEnabled = true;
        }
    }

    private async void CancelInvoiceButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show(
            this,
            "Cancel this invoice?\n\nA cancelled invoice cannot be issued, sent, or paid.",
            "Cancel Invoice",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
            return;

        CancelInvoiceButton.IsEnabled = false;

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        CancelInvoiceService>();

            await service.ExecuteAsync(
                new CancelInvoiceCommand(_invoiceId));

            await LoadInvoiceAsync();

            MessageBox.Show(
                this,
                "The invoice has been cancelled successfully.",
                "Invoice Cancelled",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to cancel invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            CancelInvoiceButton.IsEnabled = true;
        }
    }
}