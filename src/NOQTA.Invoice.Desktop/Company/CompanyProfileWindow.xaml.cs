using System.Windows;
using NOQTA.Invoice.Application.Company.GetCompanyProfile;
using NOQTA.Invoice.Application.Company.SaveCompanyProfile;

namespace NOQTA.Invoice.Desktop.Company;

public partial class CompanyProfileWindow : Window
{
    private readonly GetCompanyProfileService _getCompanyProfileService;
    private readonly SaveCompanyProfileService _saveCompanyProfileService;

    public CompanyProfileWindow(
        GetCompanyProfileService getCompanyProfileService,
        SaveCompanyProfileService saveCompanyProfileService)
    {
        InitializeComponent();

        _getCompanyProfileService =
            getCompanyProfileService;

        _saveCompanyProfileService =
            saveCompanyProfileService;

        Loaded += CompanyProfileWindow_Loaded;
    }

    private async void CompanyProfileWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= CompanyProfileWindow_Loaded;

        await LoadCompanyProfileAsync();
    }

    private async Task LoadCompanyProfileAsync()
    {
        ValidationTextBlock.Text = string.Empty;

        try
        {
            var profile =
                await _getCompanyProfileService.ExecuteAsync();

            if (profile is null)
            {
                return;
            }

            LegalNameTextBox.Text =
                profile.LegalName;

            EmailTextBox.Text =
                profile.Email;

            PhoneTextBox.Text =
                profile.Phone;

            StreetTextBox.Text =
                profile.Street;

            CityTextBox.Text =
                profile.City;

            PostalCodeTextBox.Text =
                profile.PostalCode ?? string.Empty;

            CountryTextBox.Text =
                profile.Country;

            TaxIdentifierTextBox.Text =
                profile.TaxIdentifier ?? string.Empty;

            LogoPathTextBox.Text =
                profile.LogoPath ?? string.Empty;
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;
        }
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ValidationTextBlock.Text = string.Empty;
        SaveButton.IsEnabled = false;

        try
        {
            var command =
                new SaveCompanyProfileCommand(
                    LegalNameTextBox.Text,
                    EmailTextBox.Text,
                    PhoneTextBox.Text,
                    StreetTextBox.Text,
                    CityTextBox.Text,
                    PostalCodeTextBox.Text,
                    CountryTextBox.Text,
                    NormalizeOptional(
                        TaxIdentifierTextBox.Text),
                    NormalizeOptional(
                        LogoPathTextBox.Text));

            await _saveCompanyProfileService.ExecuteAsync(
                command);

            DialogResult = true;
        }
        catch (Exception exception)
        {
            ValidationTextBlock.Text =
                exception.Message;

            SaveButton.IsEnabled = true;
        }
    }

    private static string? NormalizeOptional(
        string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}