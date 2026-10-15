using System.Windows;
using NOQTA.Invoice.Application.Customers.CreateCustomer;

namespace NOQTA.Invoice.Desktop.Customers;

public partial class CreateCustomerWindow : Window
{
    private readonly CreateCustomerService _createCustomerService;

    public CreateCustomerWindow(
        CreateCustomerService createCustomerService)
    {
        InitializeComponent();

        _createCustomerService = createCustomerService;
    }

    private async void CreateCustomerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ValidationTextBlock.Text = string.Empty;

        try
        {
            var customerType =
                GetSelectedCustomerType();

            var command = new CreateCustomerCommand(
                customerType,
                NameTextBox.Text,
                EmailTextBox.Text,
                StreetTextBox.Text,
                CityTextBox.Text,
                PostalCodeTextBox.Text,
                CountryTextBox.Text);

            var result =
                await _createCustomerService.ExecuteAsync(command);

            DialogResult = true;
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;
        }
    }

    private string GetSelectedCustomerType()
    {
        if (CustomerTypeComboBox.SelectedItem
            is not System.Windows.Controls.ComboBoxItem item)
        {
            throw new InvalidOperationException(
                "Customer type is required.");
        }

        return item.Content?.ToString()
            ?? throw new InvalidOperationException(
                "Customer type is required.");
    }
}