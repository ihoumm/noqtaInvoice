using System.Text;
using NOQTA.Invoice.Application.Invoices.Pdf;
using NOQTA.Invoice.Infrastructure.Pdf;

namespace NOQTA.Invoice.Infrastructure.Tests.Pdf;

public sealed class WeasyPrintInvoicePdfRendererIntegrationTests
{
    [Fact]
    public async Task RenderAsync_ProducesValidPdf()
    {
        var executablePath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../.weasyprint-venv/Scripts/weasyprint.exe"));

        Assert.True(
            File.Exists(executablePath),
            $"WeasyPrint executable was not found at: {executablePath}");

        var renderer =
            new WeasyPrintInvoicePdfRenderer(
                executablePath);

        var document =
            CreateDocument();

        var pdf =
            await renderer.RenderAsync(document);

        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);

        Assert.True(
            pdf.Length > 1000,
            $"Expected a real PDF document, but received only {pdf.Length} bytes.");

        var header =
            Encoding.ASCII.GetString(
                pdf,
                0,
                Math.Min(8, pdf.Length));

        Assert.StartsWith(
            "%PDF-",
            header);
    }

    [Fact]
    public async Task RenderAsync_WithLogoAndBlankLegalName_ProducesValidPdf()
    {
        var executablePath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../.weasyprint-venv/Scripts/weasyprint.exe"));

        Assert.True(
            File.Exists(executablePath),
            $"WeasyPrint executable was not found at: {executablePath}");

        var logoPath =
            Path.Combine(
                Path.GetTempPath(),
                $"noqta-test-logo-{Guid.NewGuid():N}.png");

        try
        {
            await File.WriteAllBytesAsync(
                logoPath,
                CreateMinimalPng());

            var renderer =
                new WeasyPrintInvoicePdfRenderer(
                    executablePath);

            var document =
                CreateDocument(
                    legalName: string.Empty,
                    logoPath: logoPath);

            var pdf =
                await renderer.RenderAsync(document);

            Assert.NotNull(pdf);
            Assert.NotEmpty(pdf);

            Assert.True(
                pdf.Length > 1000,
                $"Expected a real PDF document, but received only {pdf.Length} bytes.");

            var header =
                Encoding.ASCII.GetString(
                    pdf,
                    0,
                    Math.Min(8, pdf.Length));

            Assert.StartsWith(
                "%PDF-",
                header);
        }
        finally
        {
            if (File.Exists(logoPath))
            {
                File.Delete(logoPath);
            }
        }
    }

    private static InvoiceDocumentData CreateDocument(
        string legalName = "NOQTA Agency",
        string? logoPath = null)
    {
        return new InvoiceDocumentData(
            new CompanyDocumentData(
                LegalName: legalName,
                Email: "contact@noqta.ma",
                Phone: "+212 600 000 000",
                Street: "Avenue Mohammed V",
                City: "Agadir",
                PostalCode: "80000",
                Country: "Morocco",
                TaxIdentifier: "ICE123456789",
                LogoPath: logoPath),

            new InvoiceDocumentInfo(
                InvoiceNumber: "INV-2026-TEST-0001",
                InvoiceType: "Invoice",
                Status: "Draft",
                Currency: "MAD",
                CreatedAt: new DateTimeOffset(
                    2026,
                    9,
                    22,
                    10,
                    0,
                    0,
                    TimeSpan.Zero),
                IssuedAt: null,
                SentAt: null,
                PaidAt: null),

            new CustomerDocumentData(
                Name: "Atlas Studio",
                Email: "billing@atlas-studio.ma",
                Street: "12 Avenue Mohammed V",
                City: "Agadir",
                PostalCode: "80000",
                Country: "Morocco"),

            new[]
            {
                new InvoiceDocumentItem(
                    Description: "Brand Identity",
                    Quantity: 2m,
                    UnitPrice: 1500m,
                    LineTotal: 3000m,
                    Currency: "MAD"),

                new InvoiceDocumentItem(
                    Description: "Invoice Design",
                    Quantity: 1m,
                    UnitPrice: 750m,
                    LineTotal: 750m,
                    Currency: "MAD")
            },

            new InvoiceDocumentTotals(
                Subtotal: 3750m,
                DiscountAmount: 250m,
                TaxRate: 20m,
                TaxAmount: 700m,
                Total: 4200m,
                Currency: "MAD"));
    }

    private static byte[] CreateMinimalPng()
    {
        return
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00,
            0x1F, 0x15, 0xC4, 0x89,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x44, 0x41, 0x54,
            0x78, 0x9C, 0x63, 0xF8,
            0xCF, 0xC0, 0xF0, 0x1F,
            0x00, 0x05, 0x00, 0x01,
            0xFF, 0x89, 0x99, 0x3D,
            0x1D, 0x00, 0x00, 0x00,
            0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        ];
    }
}