using System.Diagnostics;
using System.Text;
using NOQTA.Invoice.Application.Invoices.Pdf;

namespace NOQTA.Invoice.Infrastructure.Pdf;

public sealed class WeasyPrintInvoicePdfRenderer
    : IInvoicePdfRenderer
{
    private readonly string _executablePath;

    public WeasyPrintInvoicePdfRenderer(
        string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException(
                "WeasyPrint executable path is required.",
                nameof(executablePath));
        }

        _executablePath = executablePath;
    }

    public async Task<byte[]> RenderAsync(
        InvoiceDocumentData document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!File.Exists(_executablePath))
        {
            throw new FileNotFoundException(
                "WeasyPrint executable was not found.",
                _executablePath);
        }

        var temporaryDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA",
                "Invoice",
                "Pdf",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(temporaryDirectory);

        var htmlPath =
            Path.Combine(
                temporaryDirectory,
                "invoice.html");

        var pdfPath =
            Path.Combine(
                temporaryDirectory,
                "invoice.pdf");

        try
        {
            var html =
                InvoiceHtmlTemplateRenderer.Render(document);

            await File.WriteAllTextAsync(
                htmlPath,
                html,
                new UTF8Encoding(false),
                cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            startInfo.ArgumentList.Add(
                htmlPath);

            startInfo.ArgumentList.Add(
                pdfPath);

            using var process =
                new Process
                {
                    StartInfo = startInfo
                };

            if (!process.Start())
            {
                throw new InvalidOperationException(
                    "Failed to start WeasyPrint.");
            }

            var standardOutputTask =
                process.StandardOutput.ReadToEndAsync(
                    cancellationToken);

            var standardErrorTask =
                process.StandardError.ReadToEndAsync(
                    cancellationToken);

            await process.WaitForExitAsync(
                cancellationToken);

            var standardOutput =
                await standardOutputTask;

            var standardError =
                await standardErrorTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(standardError)
                        ? $"WeasyPrint exited with code {process.ExitCode}."
                        : $"WeasyPrint failed with exit code {process.ExitCode}: {standardError}");
            }

            if (!File.Exists(pdfPath))
            {
                throw new InvalidOperationException(
                    "WeasyPrint completed successfully but did not produce a PDF.");
            }

            return await File.ReadAllBytesAsync(
                pdfPath,
                cancellationToken);
        }
        finally
        {
            try
            {
                if (Directory.Exists(temporaryDirectory))
                {
                    Directory.Delete(
                        temporaryDirectory,
                        recursive: true);
                }
            }
            catch
            {
                // Temporary cleanup must not hide the original result/error.
            }
        }
    }
}