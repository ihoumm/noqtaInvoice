using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Customers.UpdateCustomer;
using NOQTA.Invoice.Domain.Customers;

namespace NOQTA.Invoice.Desktop.Customers;

public partial class EditCustomerWindow : Window
{
    private readonly ICustomerRepository _customerRepository;
    private readonly UpdateCustomerService _updateCustomerService;
    private readonly Guid _customerId;

    public EditCustomerWindow(
        ICustomerRepository customerRepository,
        UpdateCustomerService updateCustomerService,
        Guid customerId)
    {
        InitializeComponent();

        _customerRepository = customerRepository;
        _updateCustomerService = updateCustomerService;
        _customerId = customerId;

        Loaded += EditCustomerWindow_Loaded;
    }

    private async void EditCustomerWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= EditCustomerWindow_Loaded;

        try
        {
            SaveCustomerButton.IsEnabled = false;

            var customer =
                await _customerRepository.GetByIdAsync(
                    _customerId);

            if (customer is null)
            {
                ValidationTextBlock.Text =
                    "The customer could not be found.";

                return;
            }

            if (!customer.IsActive)
            {
                ValidationTextBlock.Text =
                    "Inactive customers cannot be edited.";

                return;
            }

            CustomerTypeComboBox.SelectedItem =
                CustomerTypeComboBox.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(
                        item =>
                            string.Equals(
                                item.Content?.ToString(),
                                customer.Type.ToString(),
                                StringComparison.OrdinalIgnoreCase));

            NameTextBox.Text =
                customer.Name;

            EmailTextBox.Text =
                customer.Email.Value;

            StreetTextBox.Text =
                customer.Address?.Street ?? string.Empty;

            CityTextBox.Text =
                customer.Address?.City ?? string.Empty;

            PostalCodeTextBox.Text =
                customer.Address?.PostalCode ?? string.Empty;

            CountryTextBox.Text =
                customer.Address?.Country ?? string.Empty;

            SaveCustomerButton.IsEnabled = true;
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;
        }
    }

    private async void SaveCustomerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ValidationTextBlock.Text =
            string.Empty;

        try
        {
            SaveCustomerButton.IsEnabled = false;

            var customerType =
                GetSelectedCustomerType();

            var command =
                new UpdateCustomerCommand(
                    _customerId,
                    customerType,
                    NameTextBox.Text,
                    EmailTextBox.Text,
                    StreetTextBox.Text,
                    CityTextBox.Text,
                    PostalCodeTextBox.Text,
                    CountryTextBox.Text);

            await _updateCustomerService.ExecuteAsync(
                command);

            DialogResult = true;
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;

            SaveCustomerButton.IsEnabled = true;
        }
    }

    private CustomerType GetSelectedCustomerType()
    {
        if (CustomerTypeComboBox.SelectedItem
            is not ComboBoxItem item)
        {
            throw new InvalidOperationException(
                "Customer type is required.");
        }

        var value =
            item.Content?.ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                "Customer type is required.");
        }

        if (!Enum.TryParse<CustomerType>(
                value,
                ignoreCase: false,
                out var customerType))
        {
            throw new InvalidOperationException(
                "Customer type is invalid.");
        }

        return customerType;
    }
}