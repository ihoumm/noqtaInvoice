using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using NOQTA.Invoice.Application.Customers.GetCustomers;
using NOQTA.Invoice.Application.Customers.DeactivateCustomer;
using NOQTA.Invoice.Application.Customers.ReactivateCustomer;
using NOQTA.Invoice.Application.Invoices.GetInvoices;
using NOQTA.Invoice.Desktop.Company;
using NOQTA.Invoice.Desktop.Customers;
using NOQTA.Invoice.Desktop.Invoices;
using System.Windows.Media;
using NOQTA.Invoice.Application.Abstractions.Persistence;
using NOQTA.Invoice.Application.Customers.UpdateCustomer;

namespace NOQTA.Invoice.Desktop;

public partial class MainWindow : Window
{
    private readonly IServiceScopeFactory _scopeFactory;

    private IReadOnlyList<CustomerListItem> _customerItems =
        Array.Empty<CustomerListItem>();

    private Guid? _selectedCustomerId;

    private ICollectionView? _customerCollectionView;

    private IReadOnlyList<InvoiceListItem> _invoiceItems =
        Array.Empty<InvoiceListItem>();

    private ICollectionView? _invoiceCollectionView;

    public MainWindow(
        IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();

        _scopeFactory = scopeFactory;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ShowInvoicesView();

        await LoadInvoicesAsync();
    }

    private void CustomersNavigationButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowCustomersView();

        _ = LoadCustomersAsync();
    }

    private void InvoicesNavigationButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowInvoicesView();

        _ = LoadInvoicesAsync();
    }

    private void CompanyProfileNavigationButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var companyProfileWindow =
                scope.ServiceProvider
                    .GetRequiredService<
                        CompanyProfileWindow>();

            companyProfileWindow.Owner = this;

            companyProfileWindow.ShowDialog();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to open company profile",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ShowCustomersView()
    {
        CustomersView.Visibility =
            Visibility.Visible;

        InvoicesView.Visibility =
            Visibility.Collapsed;

        NewCustomerButton.Visibility =
            Visibility.Visible;

        NewInvoiceButton.Visibility =
            Visibility.Collapsed;

        HeaderNewInvoiceButton.Visibility =
            Visibility.Collapsed;
    }

    private void ShowInvoicesView()
    {
        CustomersView.Visibility =
            Visibility.Collapsed;

        InvoicesView.Visibility =
            Visibility.Visible;

        NewCustomerButton.Visibility =
            Visibility.Collapsed;

        NewInvoiceButton.Visibility =
            Visibility.Visible;

        HeaderNewInvoiceButton.Visibility =
            Visibility.Visible;
    }

    private async void NewCustomerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var createCustomerWindow =
                scope.ServiceProvider
                    .GetRequiredService<
                        CreateCustomerWindow>();

            createCustomerWindow.Owner = this;

            var result =
                createCustomerWindow.ShowDialog();

            if (result == true)
            {
                await LoadCustomersAsync();
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to create customer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void NewInvoiceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var createInvoiceWindow =
                scope.ServiceProvider
                    .GetRequiredService<
                        CreateInvoiceWindow>();

            createInvoiceWindow.Owner = this;

            var result =
                createInvoiceWindow.ShowDialog();

            if (result == true)
            {
                await LoadInvoicesAsync();
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to create invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadCustomersAsync(
    Guid? selectedCustomerId = null)
    {
        try
        {
            RefreshCustomersButton.IsEnabled = false;

            CustomersListView.Visibility =
                Visibility.Collapsed;

            EmptyCustomersPanel.Visibility =
                Visibility.Collapsed;

            LoadingCustomersPanel.Visibility =
                Visibility.Visible;

            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        GetCustomersService>();

            var customers =
                await service.ExecuteAsync();

            _customerItems =
                customers;

            CustomerCountTextBlock.Text =
                customers.Count.ToString("N0");

            ConfigureCustomerStatusFilter();

            _customerCollectionView =
                CollectionViewSource.GetDefaultView(
                    _customerItems);

            _customerCollectionView.Filter =
                CustomerFilter;

            CustomersListView.ItemsSource =
                _customerCollectionView;

            var requestedSelectedCustomerId =
                selectedCustomerId;

            _selectedCustomerId = null;

            CustomersListView.SelectedItem =
                null;

            SelectedCustomerTextBlock.Text =
                "No customer selected";

            CustomerDetailsPanel.Visibility =
                Visibility.Collapsed;

            EditCustomerButton.IsEnabled =
                false;

            DeactivateCustomerButton.Visibility =
                Visibility.Collapsed;

            DeactivateCustomerButton.IsEnabled =
                false;

            ReactivateCustomerButton.Visibility =
                Visibility.Collapsed;

            ReactivateCustomerButton.IsEnabled =
                false;

            ApplyCustomerFilter();

            if (requestedSelectedCustomerId.HasValue)
            {
                var selectedCustomer =
                    _customerItems.FirstOrDefault(
                        customer =>
                            customer.Id ==
                            requestedSelectedCustomerId.Value);

                if (selectedCustomer is not null)
                {
                    CustomersListView.SelectedItem =
                        selectedCustomer;
                }
            }

            ApplyCustomerFilter();
        }
        catch (Exception exception)
        {
            _customerItems =
                Array.Empty<CustomerListItem>();

            _customerCollectionView = null;

            CustomerCountTextBlock.Text =
                "0";

            CustomersListView.Visibility =
                Visibility.Collapsed;

            EmptyCustomersPanel.Visibility =
                Visibility.Visible;

            LoadingCustomersPanel.Visibility =
                Visibility.Collapsed;

            EmptyCustomersTextBlock.Text =
                "Unable to load customers.";

            CustomerEmptyStateHintTextBlock.Text =
                "The customer register could not be loaded.";

            MessageBox.Show(
                this,
                exception.Message,
                "Unable to load customers",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            LoadingCustomersPanel.Visibility =
                Visibility.Collapsed;

            RefreshCustomersButton.IsEnabled =
                true;
        }
    }

    private void ConfigureCustomerStatusFilter()
    {
        var selectedStatus =
            CustomerStatusFilterComboBox.SelectedItem as string;

        var statuses =
            new List<string>
            {
                "All customers",
                "Active",
                "Inactive"
            };

        CustomerStatusFilterComboBox.ItemsSource =
            statuses;

        if (!string.IsNullOrWhiteSpace(selectedStatus) &&
            statuses.Contains(
                selectedStatus,
                StringComparer.OrdinalIgnoreCase))
        {
            CustomerStatusFilterComboBox.SelectedItem =
                statuses.First(
                    status =>
                        string.Equals(
                            status,
                            selectedStatus,
                            StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            CustomerStatusFilterComboBox.SelectedIndex = 0;
        }
    }

    private void CustomerSearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        ApplyCustomerFilter();
    }

    private void CustomerStatusFilterComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        ApplyCustomerFilter();
    }

    private void RefreshCustomersButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _ = LoadCustomersAsync();
    }

    private bool CustomerFilter(object item)
    {
        if (item is not CustomerListItem customer)
        {
            return false;
        }

        var searchText =
            CustomerSearchTextBox?.Text?.Trim();

        var selectedStatus =
            CustomerStatusFilterComboBox?.SelectedItem as string;

        var matchesSearch =
            string.IsNullOrWhiteSpace(searchText) ||
            customer.Name.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase) ||
            customer.Email.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase) ||
            customer.Type.ToString().Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase);

        var matchesStatus =
            string.IsNullOrWhiteSpace(selectedStatus) ||
            string.Equals(
                selectedStatus,
                "All customers",
                StringComparison.OrdinalIgnoreCase) ||
            (string.Equals(
                selectedStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase) &&
             customer.IsActive) ||
            (string.Equals(
                selectedStatus,
                "Inactive",
                StringComparison.OrdinalIgnoreCase) &&
             !customer.IsActive);

        return matchesSearch && matchesStatus;
    }

    private void ApplyCustomerFilter()
    {
        if (_customerCollectionView is null)
        {
            UpdateCustomerListVisibility();
            return;
        }

        _customerCollectionView.Refresh();

        UpdateCustomerListVisibility();
    }

    private void UpdateCustomerListVisibility()
    {
        var visibleCustomerCount =
            _customerCollectionView?.Cast<object>().Count() ?? 0;

        if (_customerItems.Count == 0)
        {
            CustomersListView.Visibility =
                Visibility.Collapsed;

            EmptyCustomersPanel.Visibility =
                Visibility.Visible;

            EmptyCustomersTextBlock.Text =
                "No customers found.";

            CustomerEmptyStateHintTextBlock.Text =
                "Create your first customer to start managing your register.";

            return;
        }

        if (visibleCustomerCount == 0)
        {
            CustomersListView.Visibility =
                Visibility.Collapsed;

            EmptyCustomersPanel.Visibility =
                Visibility.Visible;

            EmptyCustomersTextBlock.Text =
                "No matching customers.";

            CustomerEmptyStateHintTextBlock.Text =
                "Try changing the search text or customer status filter.";

            return;
        }

        CustomersListView.Visibility =
            Visibility.Visible;

        EmptyCustomersPanel.Visibility =
            Visibility.Collapsed;
    }

    private async Task LoadInvoicesAsync()
    {
        try
        {
            RefreshInvoicesButton.IsEnabled = false;

            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        GetInvoicesService>();

            var invoices =
                await service.ExecuteAsync();

            _invoiceItems =
                invoices;

            ConfigureInvoiceStatusFilter();

            UpdateInvoiceKpis();

            _invoiceCollectionView =
    CollectionViewSource.GetDefaultView(
        _invoiceItems);

            _invoiceCollectionView.Filter =
                InvoiceFilter;

            InvoicesListView.ItemsSource =
                _invoiceCollectionView;

            ApplyInvoiceFilter();

            UpdateInvoiceListVisibility();
        }
        catch (Exception exception)
        {
            InvoicesListView.Visibility =
                Visibility.Collapsed;

            EmptyInvoicesPanel.Visibility =
                Visibility.Visible;

            EmptyInvoicesTextBlock.Text =
                "Unable to load invoices.";

            InvoiceEmptyStateHintTextBlock.Text =
                "The invoice register could not be loaded.";

            MessageBox.Show(
                this,
                exception.Message,
                "Unable to load invoices",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            RefreshInvoicesButton.IsEnabled = true;
        }
    }

    // private void ConfigureInvoiceStatusFilter()
    // {
    //     var selectedStatus =
    //         InvoiceStatusFilterComboBox.SelectedItem as string;

    //     var statuses =
    //         _invoiceItems
    //             .Select(invoice => invoice.Status)
    //             .Where(status => !string.IsNullOrWhiteSpace(status))
    //             .Distinct(StringComparer.OrdinalIgnoreCase)
    //             .OrderBy(status => status)
    //             .ToList();

    //     statuses.Insert(0, "All statuses");

    //     InvoiceStatusFilterComboBox.ItemsSource =
    //         statuses;

    //     if (!string.IsNullOrWhiteSpace(selectedStatus) &&
    //         statuses.Contains(
    //             selectedStatus,
    //             StringComparer.OrdinalIgnoreCase))
    //     {
    //         InvoiceStatusFilterComboBox.SelectedItem =
    //             statuses.First(
    //                 status =>
    //                     string.Equals(
    //                         status,
    //                         selectedStatus,
    //                         StringComparison.OrdinalIgnoreCase));
    //     }
    //     else
    //     {
    //         InvoiceStatusFilterComboBox.SelectedIndex = 0;
    //     }
    // }

    private void ConfigureInvoiceStatusFilter()
    {
        var statuses =
            _invoiceItems
                .Select(invoice => invoice.Status)
                .Where(status => !string.IsNullOrWhiteSpace(status))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(status => status)
                .ToList();

        statuses.Insert(0, "All statuses");

        InvoiceStatusFilterComboBox.ItemsSource =
            statuses;

        InvoiceStatusFilterComboBox.SelectedIndex = 0;
    }

    private void UpdateInvoiceKpis()
    {
        var totalInvoices =
            _invoiceItems.Count;

        var draftInvoices =
            _invoiceItems.Count(
                invoice =>
                    string.Equals(
                        invoice.Status,
                        "Draft",
                        StringComparison.OrdinalIgnoreCase));

        var paidInvoices =
            _invoiceItems.Count(
                invoice =>
                    string.Equals(
                        invoice.Status,
                        "Paid",
                        StringComparison.OrdinalIgnoreCase));

        var totalValue =
            _invoiceItems.Sum(
                invoice => invoice.Total);

        TotalInvoicesKpiTextBlock.Text =
            totalInvoices.ToString("N0");

        DraftInvoicesKpiTextBlock.Text =
            draftInvoices.ToString("N0");

        PaidInvoicesKpiTextBlock.Text =
            paidInvoices.ToString("N0");

        TotalInvoiceValueKpiTextBlock.Text =
            totalValue.ToString("N2");
    }

    private void InvoiceSearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        ApplyInvoiceFilter();
    }

    private void InvoiceStatusFilterComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        ApplyInvoiceFilter();
    }

    private void RefreshInvoicesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _ = LoadInvoicesAsync();
    }

    private bool InvoiceFilter(object item)
    {
        if (item is not InvoiceListItem invoice)
        {
            return false;
        }

        var searchText =
            InvoiceSearchTextBox?.Text?.Trim();

        var selectedStatus =
            InvoiceStatusFilterComboBox?.SelectedItem as string;

        var matchesSearch =
            string.IsNullOrWhiteSpace(searchText) ||
            invoice.InvoiceNumber.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase) ||
            invoice.CustomerName.Contains(
                searchText,
                StringComparison.OrdinalIgnoreCase);

        var matchesStatus =
            string.IsNullOrWhiteSpace(selectedStatus) ||
            string.Equals(
                selectedStatus,
                "All statuses",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                invoice.Status,
                selectedStatus,
                StringComparison.OrdinalIgnoreCase);

        return matchesSearch && matchesStatus;
    }

    private void ApplyInvoiceFilter()
    {
        if (_invoiceCollectionView is null)
        {
            return;
        }

        _invoiceCollectionView.Refresh();

        UpdateInvoiceListVisibility();
    }

    private void UpdateInvoiceListVisibility()
    {
        var visibleInvoiceCount =
            _invoiceCollectionView?.Cast<object>().Count() ?? 0;

        if (_invoiceItems.Count == 0)
        {
            InvoicesListView.Visibility =
                Visibility.Collapsed;

            EmptyInvoicesPanel.Visibility =
                Visibility.Visible;

            EmptyInvoicesTextBlock.Text =
                "No invoices found.";

            InvoiceEmptyStateHintTextBlock.Text =
                "Create your first invoice to start managing your register.";

            return;
        }

        if (visibleInvoiceCount == 0)
        {
            InvoicesListView.Visibility =
                Visibility.Collapsed;

            EmptyInvoicesPanel.Visibility =
                Visibility.Visible;

            EmptyInvoicesTextBlock.Text =
                "No matching invoices.";

            InvoiceEmptyStateHintTextBlock.Text =
                "Try changing the search text or status filter.";

            return;
        }

        InvoicesListView.Visibility =
            Visibility.Visible;

        EmptyInvoicesPanel.Visibility =
            Visibility.Collapsed;
    }

    private void InvoicesListView_MouseDoubleClick(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (InvoicesListView.SelectedItem
            is not InvoiceListItem invoice)
        {
            return;
        }

        try
        {
            var invoiceDetailsWindow =
                new InvoiceDetailsWindow(
                    _scopeFactory,
                    invoice.Id);

            invoiceDetailsWindow.Owner = this;

            invoiceDetailsWindow.ShowDialog();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to open invoice",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CustomersListView_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (CustomersListView.SelectedItem
            is not CustomerListItem customer)
        {
            _selectedCustomerId = null;

            SelectedCustomerTextBlock.Text =
                "No customer selected";

            CustomerDetailsPanel.Visibility =
                Visibility.Collapsed;

            EditCustomerButton.IsEnabled =
                false;

            DeactivateCustomerButton.Visibility =
                Visibility.Collapsed;

            DeactivateCustomerButton.IsEnabled =
                false;

            ReactivateCustomerButton.Visibility =
                Visibility.Collapsed;

            ReactivateCustomerButton.IsEnabled =
                false;

            return;
        }

        _selectedCustomerId =
            customer.Id;

        SelectedCustomerTextBlock.Text =
            $"Selected: {customer.Name}";

        CustomerDetailsNameTextBlock.Text =
            customer.Name;

        CustomerDetailsTypeTextBlock.Text =
            customer.Type.ToString();

        CustomerDetailsEmailTextBlock.Text =
            customer.Email;

        CustomerDetailsStatusTextBlock.Text =
            customer.IsActive
                ? "Active"
                : "Inactive";

        CustomerDetailsStatusBorder.Background =
            customer.IsActive
                ? (Brush)FindResource(
                    "SuccessBackgroundBrush")
                : (Brush)FindResource(
                    "DangerBackgroundBrush");

        CustomerDetailsStatusTextBlock.Foreground =
            customer.IsActive
                ? (Brush)FindResource(
                    "SuccessBrush")
                : (Brush)FindResource(
                    "DangerBrush");

        CustomerDetailsSubtitleTextBlock.Text =
            customer.Name;

        CustomerDetailsSelectionTextBlock.Text =
            customer.IsActive
                ? "This customer is currently active."
                : "This customer is currently inactive.";

        EditCustomerButton.IsEnabled =
            customer.IsActive;

        DeactivateCustomerButton.Visibility =
            customer.IsActive
                ? Visibility.Visible
                : Visibility.Collapsed;

        DeactivateCustomerButton.IsEnabled =
            customer.IsActive;

        ReactivateCustomerButton.Visibility =
            customer.IsActive
                ? Visibility.Collapsed
                : Visibility.Visible;

        ReactivateCustomerButton.IsEnabled =
            !customer.IsActive;

        CustomerDetailsPanel.Visibility =
            Visibility.Visible;
    }

    private async void DeactivateCustomerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (CustomersListView.SelectedItem
            is not CustomerListItem customer)
        {
            return;
        }

        var confirmation =
            MessageBox.Show(
                this,
                $"Are you sure you want to deactivate {customer.Name}?\n\nThe customer will remain in the register, but cannot be edited or used for new invoices until reactivated.",
                "Deactivate customer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            DeactivateCustomerButton.IsEnabled = false;
            EditCustomerButton.IsEnabled = false;

            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        DeactivateCustomerService>();

            await service.ExecuteAsync(
                new DeactivateCustomerCommand(customer.Id));

            CustomerStatusFilterComboBox.SelectedItem =
                "All customers";

            await LoadCustomersAsync(customer.Id);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to deactivate customer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            DeactivateCustomerButton.IsEnabled = true;
        }
    }

    private async void ReactivateCustomerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (CustomersListView.SelectedItem
            is not CustomerListItem customer)
        {
            return;
        }

        var confirmation =
            MessageBox.Show(
                this,
                $"Are you sure you want to reactivate {customer.Name}?\n\nThe customer will become active again and can be edited or used for new invoices.",
                "Reactivate customer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            ReactivateCustomerButton.IsEnabled = false;

            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        ReactivateCustomerService>();

            await service.ExecuteAsync(
                new ReactivateCustomerCommand(customer.Id));

            CustomerStatusFilterComboBox.SelectedItem =
                "All customers";

            await LoadCustomersAsync(customer.Id);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to reactivate customer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            ReactivateCustomerButton.IsEnabled = true;
        }
    }

    private async void EditCustomerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (CustomersListView.SelectedItem
            is not CustomerListItem customer)
        {
            return;
        }

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var customerRepository =
                scope.ServiceProvider
                    .GetRequiredService<
                        ICustomerRepository>();

            var updateCustomerService =
                scope.ServiceProvider
                    .GetRequiredService<
                        UpdateCustomerService>();

            var editCustomerWindow =
                new EditCustomerWindow(
                    customerRepository,
                    updateCustomerService,
                    customer.Id);

            editCustomerWindow.Owner =
                this;

            var result =
                editCustomerWindow.ShowDialog();

            if (result == true)
            {
                await LoadCustomersAsync(
                    customer.Id);
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Unable to edit customer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}