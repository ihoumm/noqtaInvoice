using System.Globalization;
using System.Net;
using System.Text;
using NOQTA.Invoice.Application.Invoices.Pdf;

namespace NOQTA.Invoice.Infrastructure.Pdf;

internal static class InvoiceHtmlTemplateRenderer
{
    public static string Render(
        InvoiceDocumentData document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var builder = new StringBuilder();

        builder.Append(
            """
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <title>Invoice</title>

                <style>
                    @page {
                        size: A4;
                        margin: 17mm 15mm 20mm 15mm;

                        @bottom-left {
                            content: "NOQTA Agency";
                            font-family: Arial, Helvetica, sans-serif;
                            font-size: 7.5pt;
                            color: #7A8491;
                        }

                        @bottom-right {
                            content: "Page " counter(page) " of " counter(pages);
                            font-family: Arial, Helvetica, sans-serif;
                            font-size: 7.5pt;
                            color: #7A8491;
                        }
                    }

                    * {
                        box-sizing: border-box;
                    }

                    html,
                    body {
                        margin: 0;
                        padding: 0;
                    }

                    body {
                        font-family: Arial, Helvetica, sans-serif;
                        color: #17212B;
                        background: #FFFFFF;
                        font-size: 9.5pt;
                        line-height: 1.45;
                    }

                    .document {
                        width: 100%;
                    }

                    /* -------------------------------------------------
                       Header
                       ------------------------------------------------- */

                    .header {
                        width: 100%;
                        padding-bottom: 15px;
                        border-bottom: 2px solid #17212B;
                        margin-bottom: 22px;
                    }

                    .header-table {
                        width: 100%;
                        border-collapse: collapse;
                    }

                    .header-left {
                        width: 58%;
                        vertical-align: top;
                    }

                    .header-right {
                        width: 42%;
                        vertical-align: top;
                        text-align: right;
                    }

                    .brand {
                        display: inline-block;
                    }

                    .logo {
                        display: block;
                        max-width: 150px;
                        max-height: 54px;
                        margin-bottom: 8px;
                    }

                    .company-name {
                        color: #17212B;
                        font-size: 19pt;
                        line-height: 1.1;
                        font-weight: 700;
                        letter-spacing: -0.2px;
                        margin: 0 0 6px 0;
                    }

                    .company-contact {
                        color: #687482;
                        font-size: 8pt;
                        line-height: 1.55;
                    }

                    .invoice-label {
                        color: #7A8491;
                        font-size: 8pt;
                        font-weight: 700;
                        letter-spacing: 1.7px;
                        text-transform: uppercase;
                        margin-bottom: 4px;
                    }

                    .invoice-title {
                        color: #17212B;
                        font-size: 27pt;
                        line-height: 1;
                        font-weight: 700;
                        letter-spacing: -0.8px;
                        margin-bottom: 7px;
                    }

                    .invoice-number {
                        color: #44515E;
                        font-size: 9pt;
                        font-weight: 600;
                        margin-bottom: 9px;
                    }

                    .status {
                        display: inline-block;
                        padding: 4px 9px;
                        border-radius: 10px;
                        background: #EEF2F5;
                        color: #44515E;
                        font-size: 7.5pt;
                        line-height: 1;
                        font-weight: 700;
                        letter-spacing: .5px;
                        text-transform: uppercase;
                    }

                    /* -------------------------------------------------
                       Information cards
                       ------------------------------------------------- */

                    .section {
                        margin-bottom: 20px;
                    }

                    .section-label {
                        color: #7A8491;
                        font-size: 7.5pt;
                        font-weight: 700;
                        letter-spacing: 1.15px;
                        text-transform: uppercase;
                        margin-bottom: 7px;
                    }

                    .info-table {
                        width: 100%;
                        border-collapse: separate;
                        border-spacing: 0;
                    }

                    .info-card {
                        width: 100%;
                        border: 1px solid #DDE3E8;
                        background: #F8FAFB;
                        padding: 12px 14px;
                    }

                    .customer-name {
                        color: #17212B;
                        font-size: 11pt;
                        line-height: 1.25;
                        font-weight: 700;
                        margin-bottom: 5px;
                    }

                    .customer-detail {
                        color: #687482;
                        font-size: 8.5pt;
                        line-height: 1.55;
                    }

                    /* -------------------------------------------------
                       Metadata
                       ------------------------------------------------- */

                    .meta-table {
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 18px;
                        margin-bottom: 22px;
                    }

                    .meta-cell {
                        width: 16.666%;
                        padding: 9px 11px;
                        border-top: 1px solid #E4E8EC;
                        border-bottom: 1px solid #E4E8EC;
                        vertical-align: top;
                    }

                    .meta-cell:first-child {
                        padding-left: 0;
                    }

                    .meta-label {
                        color: #89939E;
                        font-size: 6.8pt;
                        font-weight: 700;
                        letter-spacing: .8px;
                        text-transform: uppercase;
                        margin-bottom: 3px;
                    }

                    .meta-value {
                        color: #26323D;
                        font-size: 8.3pt;
                        font-weight: 600;
                        white-space: nowrap;
                    }

                    /* -------------------------------------------------
                       Items
                       ------------------------------------------------- */

                    .items-section {
                        margin-top: 2px;
                    }

                    .items-table {
                        width: 100%;
                        border-collapse: collapse;
                        table-layout: fixed;
                    }

                    .items-table thead {
                        display: table-header-group;
                    }

                    .items-table tr {
                        page-break-inside: avoid;
                    }

                    .items-table th {
                        background: #17212B;
                        color: #FFFFFF;
                        padding: 9px 9px;
                        border: 0;
                        font-size: 7.2pt;
                        line-height: 1.2;
                        font-weight: 700;
                        letter-spacing: .65px;
                        text-transform: uppercase;
                        text-align: left;
                    }

                    .items-table th:first-child {
                        width: 48%;
                    }

                    .items-table th:nth-child(2) {
                        width: 14%;
                    }

                    .items-table th:nth-child(3) {
                        width: 19%;
                    }

                    .items-table th:nth-child(4) {
                        width: 19%;
                    }

                    .items-table th.number,
                    .items-table td.number {
                        text-align: right;
                    }

                    .items-table td {
                        padding: 10px 9px;
                        border-bottom: 1px solid #E5E9ED;
                        color: #35424E;
                        font-size: 8.8pt;
                        vertical-align: top;
                    }

                    .items-table tbody tr:nth-child(even) td {
                        background: #FAFBFC;
                    }

                    .description {
                        color: #202D38;
                        font-weight: 600;
                    }

                    .amount {
                        white-space: nowrap;
                    }

                    /* -------------------------------------------------
                       Totals
                       ------------------------------------------------- */

                    .summary-wrapper {
                        width: 100%;
                        margin-top: 20px;
                        page-break-inside: avoid;
                    }

                    .summary-table {
                        width: 100%;
                        border-collapse: collapse;
                    }

                    .summary-spacer {
                        width: 57%;
                    }

                    .summary-panel {
                        width: 43%;
                        padding-left: 28px;
                        vertical-align: top;
                    }

                    .summary-row {
                        width: 100%;
                        border-bottom: 1px solid #E7EBEE;
                    }

                    .summary-label,
                    .summary-value {
                        padding: 6px 0;
                        color: #687482;
                        font-size: 8.8pt;
                    }

                    .summary-label {
                        text-align: left;
                    }

                    .summary-value {
                        text-align: right;
                        white-space: nowrap;
                    }

                    .total-row {
                        border-top: 2px solid #17212B;
                    }

                    .total-label,
                    .total-value {
                        padding-top: 10px;
                        color: #17212B;
                        font-size: 12.5pt;
                        line-height: 1.2;
                        font-weight: 700;
                    }

                    .total-value {
                        text-align: right;
                        white-space: nowrap;
                    }

                    .currency-note {
                        margin-top: 5px;
                        color: #89939E;
                        font-size: 7pt;
                        text-align: right;
                    }

                    /* -------------------------------------------------
                       Footer note
                       ------------------------------------------------- */

                    .footer-note {
                        margin-top: 30px;
                        padding-top: 12px;
                        border-top: 1px solid #DDE3E8;
                        color: #7A8491;
                        font-size: 7.7pt;
                        line-height: 1.5;
                    }

                    .footer-note strong {
                        color: #4D5A66;
                        font-weight: 700;
                    }

                    /* -------------------------------------------------
                       Print / page behavior
                       ------------------------------------------------- */

                    .avoid-break {
                        page-break-inside: avoid;
                    }

                    .items-table,
                    .summary-wrapper {
                        page-break-inside: auto;
                    }
                </style>
            </head>

            <body>
                <div class="document">
            """);

        /* =============================================================
           HEADER
           ============================================================= */

        builder.Append("<div class=\"header\">");
        builder.Append("<table class=\"header-table\">");
        builder.Append("<tr>");

        builder.Append("<td class=\"header-left\">");
        builder.Append("<div class=\"brand\">");

        if (!string.IsNullOrWhiteSpace(document.Company.LogoPath))
        {
            var logoUri = CreateFileUri(document.Company.LogoPath);

            builder.Append(
                $"<img class=\"logo\" src=\"{Encode(logoUri)}\" alt=\"{Encode(document.Company.LegalName)} logo\">");
        }

        if (!string.IsNullOrWhiteSpace(document.Company.LegalName))
        {
            builder.Append(
                $"<div class=\"company-name\">{Encode(document.Company.LegalName)}</div>");
        }

        builder.Append("<div class=\"company-contact\">");

        if (!string.IsNullOrWhiteSpace(document.Company.Email))
        {
            builder.Append(
                $"{Encode(document.Company.Email)}");
        }

        if (!string.IsNullOrWhiteSpace(document.Company.Phone))
        {
            if (!string.IsNullOrWhiteSpace(document.Company.Email))
            {
                builder.Append(" &nbsp;·&nbsp; ");
            }

            builder.Append(
                $"{Encode(document.Company.Phone)}");
        }

        builder.Append("<br>");

        AppendAddress(
            builder,
            document.Company.Street,
            document.Company.City,
            document.Company.PostalCode,
            document.Company.Country);

        if (!string.IsNullOrWhiteSpace(document.Company.TaxIdentifier))
        {
            builder.Append(
                $"<br>Tax ID: {Encode(document.Company.TaxIdentifier)}");
        }

        builder.Append("</div>");
        builder.Append("</div>");
        builder.Append("</td>");

        builder.Append("<td class=\"header-right\">");

        builder.Append(
            "<div class=\"invoice-label\">Commercial Document</div>");

        builder.Append(
            "<div class=\"invoice-title\">INVOICE</div>");

        builder.Append(
            $"<div class=\"invoice-number\">{Encode(document.Invoice.InvoiceNumber)}</div>");

        builder.Append(
            $"<span class=\"status\">{Encode(document.Invoice.Status)}</span>");

        builder.Append("</td>");

        builder.Append("</tr>");
        builder.Append("</table>");
        builder.Append("</div>");

        /* =============================================================
           BILL TO
           ============================================================= */

        builder.Append("<div class=\"section avoid-break\">");

        builder.Append(
            "<div class=\"section-label\">Bill To</div>");

        builder.Append("<table class=\"info-table\">");
        builder.Append("<tr>");
        builder.Append("<td class=\"info-card\">");

        builder.Append(
            $"<div class=\"customer-name\">{Encode(document.Customer.Name)}</div>");

        if (!string.IsNullOrWhiteSpace(document.Customer.Email))
        {
            builder.Append(
                $"<div class=\"customer-detail\">{Encode(document.Customer.Email)}</div>");
        }

        var customerAddress = BuildAddress(
            document.Customer.Street,
            document.Customer.City,
            document.Customer.PostalCode,
            document.Customer.Country);

        if (!string.IsNullOrWhiteSpace(customerAddress))
        {
            builder.Append(
                $"<div class=\"customer-detail\">{customerAddress}</div>");
        }

        builder.Append("</td>");
        builder.Append("</tr>");
        builder.Append("</table>");

        builder.Append("</div>");

        /* =============================================================
           METADATA
           ============================================================= */

        builder.Append("<table class=\"meta-table\">");
        builder.Append("<tr>");

        AppendMetaCell(
            builder,
            "Type",
            document.Invoice.InvoiceType);

        AppendMetaCell(
            builder,
            "Currency",
            document.Invoice.Currency);

        AppendMetaCell(
            builder,
            "Created",
            FormatDate(document.Invoice.CreatedAt));

        AppendMetaCell(
            builder,
            "Issued",
            FormatNullableDate(document.Invoice.IssuedAt));

        AppendMetaCell(
            builder,
            "Sent",
            FormatNullableDate(document.Invoice.SentAt));

        AppendMetaCell(
            builder,
            "Paid",
            FormatNullableDate(document.Invoice.PaidAt));

        builder.Append("</tr>");
        builder.Append("</table>");

        /* =============================================================
           ITEMS
           ============================================================= */

        builder.Append("<div class=\"items-section\">");

        builder.Append(
            "<div class=\"section-label\">Invoice Items</div>");

        builder.Append("<table class=\"items-table\">");

        builder.Append("<thead>");
        builder.Append("<tr>");

        builder.Append("<th>Description</th>");
        builder.Append("<th class=\"number\">Quantity</th>");
        builder.Append("<th class=\"number\">Unit Price</th>");
        builder.Append("<th class=\"number\">Line Total</th>");

        builder.Append("</tr>");
        builder.Append("</thead>");

        builder.Append("<tbody>");

        foreach (var item in document.Items)
        {
            builder.Append("<tr>");

            builder.Append(
                $"<td class=\"description\">{Encode(item.Description)}</td>");

            builder.Append(
                $"<td class=\"number\">{FormatDecimal(item.Quantity)}</td>");

            builder.Append(
                $"<td class=\"number amount\">{FormatMoney(item.UnitPrice, item.Currency)}</td>");

            builder.Append(
                $"<td class=\"number amount\">{FormatMoney(item.LineTotal, item.Currency)}</td>");

            builder.Append("</tr>");
        }

        builder.Append("</tbody>");
        builder.Append("</table>");
        builder.Append("</div>");

        /* =============================================================
           TOTALS
           ============================================================= */

        builder.Append("<div class=\"summary-wrapper\">");
        builder.Append("<table class=\"summary-table\">");
        builder.Append("<tr>");

        builder.Append(
            "<td class=\"summary-spacer\"></td>");

        builder.Append("<td class=\"summary-panel\">");

        AppendSummaryRow(
            builder,
            "Subtotal",
            FormatMoney(
                document.Totals.Subtotal,
                document.Totals.Currency));

        AppendSummaryRow(
            builder,
            "Discount",
            FormatMoney(
                document.Totals.DiscountAmount,
                document.Totals.Currency));

        AppendSummaryRow(
            builder,
            $"Tax ({FormatDecimal(document.Totals.TaxRate)}%)",
            FormatMoney(
                document.Totals.TaxAmount,
                document.Totals.Currency));

        builder.Append("<table style=\"width:100%; border-collapse:collapse;\">");
        builder.Append("<tr class=\"total-row\">");

        builder.Append(
            "<td class=\"total-label\">Total</td>");

        builder.Append(
            $"<td class=\"total-value\">{FormatMoney(document.Totals.Total, document.Totals.Currency)}</td>");

        builder.Append("</tr>");
        builder.Append("</table>");

        builder.Append(
            $"<div class=\"currency-note\">Amounts shown in {Encode(document.Totals.Currency)}</div>");

        builder.Append("</td>");
        builder.Append("</tr>");
        builder.Append("</table>");
        builder.Append("</div>");

        /* =============================================================
           FOOTER NOTE
           ============================================================= */

        builder.Append(
            """
            <div class="footer-note">
                <strong>Thank you for your business.</strong>
                This invoice was generated by <strong>NOQTA Agency</strong>
            </div>
            """);

        builder.Append("</div>");
        builder.Append("</body>");
        builder.Append("</html>");

        return builder.ToString();
    }

    private static void AppendMetaCell(
        StringBuilder builder,
        string label,
        string value)
    {
        builder.Append("<td class=\"meta-cell\">");

        builder.Append(
            $"<div class=\"meta-label\">{Encode(label)}</div>");

        builder.Append(
            $"<div class=\"meta-value\">{Encode(value)}</div>");

        builder.Append("</td>");
    }

    private static void AppendSummaryRow(
        StringBuilder builder,
        string label,
        string value)
    {
        builder.Append("<table style=\"width:100%; border-collapse:collapse;\">");
        builder.Append("<tr class=\"summary-row\">");

        builder.Append(
            $"<td class=\"summary-label\">{Encode(label)}</td>");

        builder.Append(
            $"<td class=\"summary-value\">{Encode(value)}</td>");

        builder.Append("</tr>");
        builder.Append("</table>");
    }

    private static void AppendAddress(
        StringBuilder builder,
        string? street,
        string? city,
        string? postalCode,
        string? country)
    {
        var address = BuildAddress(
            street,
            city,
            postalCode,
            country);

        builder.Append(address);
    }

    private static string BuildAddress(
        string? street,
        string? city,
        string? postalCode,
        string? country)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(street))
        {
            parts.Add(
                Encode(street));
        }

        var cityParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            cityParts.Add(
                Encode(postalCode));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            cityParts.Add(
                Encode(city));
        }

        if (cityParts.Count > 0)
        {
            parts.Add(
                string.Join(
                    ", ",
                    cityParts));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            parts.Add(
                Encode(country));
        }

        return string.Join(
            "<br>",
            parts);
    }

    private static string FormatMoney(
        decimal amount,
        string currency)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:N2} {1}",
            amount,
            currency);
    }

    private static string FormatDecimal(
        decimal value)
    {
        return value.ToString(
            "0.##",
            CultureInfo.InvariantCulture);
    }

    private static string FormatDate(
        DateTimeOffset value)
    {
        return value
            .ToLocalTime()
            .ToString(
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture);
    }

    private static string FormatNullableDate(
        DateTimeOffset? value)
    {
        return value.HasValue
            ? FormatDate(value.Value)
            : "—";
    }

    private static string CreateFileUri(
        string path)
    {
        var fullPath =
            Path.GetFullPath(path);

        return new Uri(
            fullPath,
            UriKind.Absolute)
            .AbsoluteUri;
    }

    private static string Encode(
        string? value)
    {
        return WebUtility.HtmlEncode(
            value ?? string.Empty);
    }
}