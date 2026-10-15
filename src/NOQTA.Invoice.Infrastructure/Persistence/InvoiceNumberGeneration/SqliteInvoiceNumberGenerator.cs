using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NOQTA.Invoice.Application.Abstractions.Persistence;

namespace NOQTA.Invoice.Infrastructure.Persistence.InvoiceNumberGeneration;

public sealed class SqliteInvoiceNumberGenerator
    : IInvoiceNumberGenerator
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS "InvoiceNumberSequences"
        (
            "Id" INTEGER NOT NULL
                CONSTRAINT "PK_InvoiceNumberSequences"
                PRIMARY KEY,
            "NextNumber" INTEGER NOT NULL
        );
        """;

    private const string NextNumberSql = """
        INSERT INTO "InvoiceNumberSequences"
            ("Id", "NextNumber")
        VALUES
            (1, 1)
        ON CONFLICT("Id")
        DO UPDATE SET
            "NextNumber" =
                "InvoiceNumberSequences"."NextNumber" + 1
        RETURNING "NextNumber";
        """;

    private readonly InvoiceDbContext _dbContext;

    public SqliteInvoiceNumberGenerator(
        InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> GetNextAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            CreateTableSql,
            cancellationToken);

        var connection =
            _dbContext.Database.GetDbConnection();

        var openedHere =
            connection.State !=
            System.Data.ConnectionState.Open;

        if (openedHere)
        {
            connection.Open();
        }

        try
        {
            await using var command =
                connection.CreateCommand();

            command.CommandText =
                NextNumberSql;

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            if (result is null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "The invoice number sequence did not return a value.");
            }

            return Convert.ToInt64(result);
        }
        finally
        {
            if (openedHere)
            {
                connection.Close();
            }
        }
    }
}