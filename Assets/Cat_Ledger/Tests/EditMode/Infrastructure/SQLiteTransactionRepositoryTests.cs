using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence.SQLite;
using CatLedger.Tests.EditMode.Finance.TestData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CatLedger.Tests.EditMode.Infrastructure
{
    public sealed class SQLiteTransactionRepositoryTests
    {
        [Test]
        public async Task AddAsync_AfterReopeningDatabase_TransactionStillExists()
        {
            string databasePath = Path.Combine(
                Path.GetTempPath(),
                $"catledger-test-{Guid.NewGuid():N}.db3");

            CatLedgerDatabase firstDatabase = null;
            CatLedgerDatabase reopenedDatabase = null;

            try
            {
                DateTimeOffset occurredAt =
                    new DateTimeOffset(
                        2026,
                        9,
                        25,
                        19,
                        30,
                        0,
                        TimeSpan.FromHours(9));

                DateTimeOffset createdAt =
                    new DateTimeOffset(
                        2026,
                        9,
                        26,
                        22,
                        30,
                        0,
                        TimeSpan.FromHours(9));

                var transaction =
                    new Transaction(
                        Guid.NewGuid(),
                        amountInMinorUnits: 1200,
                        type: TransactionType.Expense,
                        category: TransactionCategory.Food,
                        paymentMethod: PaymentMethod.CreditCard,
                        occurredAt: occurredAt,
                        note: "Ramen",
                        createdAt: createdAt,
                        updatedAt: createdAt);

                firstDatabase =
                    new CatLedgerDatabase(databasePath);

                var firstRepository =
                    new SQLiteTransactionRepository(
                        firstDatabase);

                await firstRepository.AddAsync(
                    transaction);

                firstDatabase.Dispose();
                firstDatabase = null;

                reopenedDatabase =
                    new CatLedgerDatabase(databasePath);

                var reopenedRepository =
                    new SQLiteTransactionRepository(
                        reopenedDatabase);

                IReadOnlyList<Transaction>
                    storedTransactions =
                        await reopenedRepository
                            .GetMatchingAsync(
                                new TransactionFilter());

                Assert.That(
                    storedTransactions.Count,
                    Is.EqualTo(1));

                Transaction storedTransaction =
                    storedTransactions[0];

                Assert.That(
                    storedTransaction.Id,
                    Is.EqualTo(transaction.Id));

                Assert.That(
                    storedTransaction.AmountInMinorUnits,
                    Is.EqualTo(1200));

                Assert.That(
                    storedTransaction.Type,
                    Is.EqualTo(TransactionType.Expense));

                Assert.That(
                    storedTransaction.Category,
                    Is.EqualTo(TransactionCategory.Food));

                Assert.That(
                    storedTransaction.PaymentMethod,
                    Is.EqualTo(PaymentMethod.CreditCard));

                Assert.That(
                    storedTransaction.OccurredAt,
                    Is.EqualTo(occurredAt));

                Assert.That(
                    storedTransaction.CreatedAt,
                    Is.EqualTo(createdAt));

                Assert.That(
                    storedTransaction.Note,
                    Is.EqualTo("Ramen"));
            }
            finally
            {
                firstDatabase?.Dispose();
                reopenedDatabase?.Dispose();

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
        }

        [Test]
        public async Task GetMatchingAsync_WithCombinedFilter_ReturnsOnlyMatchingTransactions()
        {
            string databasePath = Path.Combine(
                Path.GetTempPath(),
                $"catledger-test-{Guid.NewGuid():N}.db3");

            CatLedgerDatabase database = null;

            try
            {
                database =
                    new CatLedgerDatabase(databasePath);

                var repository =
                    new SQLiteTransactionRepository(
                        database);

                DateTimeOffset september10 =
                    new DateTimeOffset(
                        2026,
                        9,
                        10,
                        12,
                        0,
                        0,
                        TimeSpan.FromHours(9));

                DateTimeOffset september15 =
                    new DateTimeOffset(
                        2026,
                        9,
                        15,
                        12,
                        0,
                        0,
                        TimeSpan.FromHours(9));

                DateTimeOffset september20 =
                    new DateTimeOffset(
                        2026,
                        9,
                        20,
                        12,
                        0,
                        0,
                        TimeSpan.FromHours(9));

                await repository.AddAsync(
                    TransactionTestFactory.Create(
                        amountInMinorUnits: 500,
                        type: TransactionType.Expense,
                        category: TransactionCategory.Food,
                        occurredAt: september10));

                await repository.AddAsync(
                    TransactionTestFactory.Create(
                        amountInMinorUnits: 800,
                        type: TransactionType.Expense,
                        category: TransactionCategory.Transport,
                        occurredAt: september15));

                await repository.AddAsync(
                    TransactionTestFactory.Create(
                        amountInMinorUnits: 3000,
                        type: TransactionType.Income,
                        category: TransactionCategory.Other,
                        occurredAt: september20));

                DateTimeOffset septemberStart =
                    new DateTimeOffset(
                        2026,
                        9,
                        1,
                        0,
                        0,
                        0,
                        TimeSpan.FromHours(9));

                DateTimeOffset octoberStart =
                    new DateTimeOffset(
                        2026,
                        10,
                        1,
                        0,
                        0,
                        0,
                        TimeSpan.FromHours(9));

                var filter =
                    new TransactionFilter(
                        startOccurredAtInclusive:
                            septemberStart,
                        endOccurredAtExclusive:
                            octoberStart,
                        category:
                            TransactionCategory.Food,
                        type:
                            TransactionType.Expense);

                IReadOnlyList<Transaction> transactions =
                    await repository.GetMatchingAsync(
                        filter);

                Assert.That(
                    transactions.Count,
                    Is.EqualTo(1));

                Assert.That(
                    transactions[0].AmountInMinorUnits,
                    Is.EqualTo(500));

                Assert.That(
                    transactions[0].Category,
                    Is.EqualTo(TransactionCategory.Food));

                Assert.That(
                    transactions[0].Type,
                    Is.EqualTo(TransactionType.Expense));
            }
            finally
            {
                database?.Dispose();

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
        }

        [Test]
        public async Task UpdateAsync_AfterDatabaseReopened_PersistsUpdatedTransaction()
        {
            string databasePath = Path.Combine(
                Path.GetTempPath(),
                $"catledger-test-{Guid.NewGuid():N}.db3");

            CatLedgerDatabase firstDatabase = null;
            CatLedgerDatabase reopenedDatabase = null;

            try
            {
                firstDatabase =
                    new CatLedgerDatabase(databasePath);

                var firstRepository =
                    new SQLiteTransactionRepository(
                        firstDatabase);

                Transaction originalTransaction =
                    TransactionTestFactory.Create(
                        amountInMinorUnits: 500,
                        category: TransactionCategory.Food,
                        paymentMethod: PaymentMethod.Cash);

                await firstRepository.AddAsync(
                    originalTransaction);

                DateTimeOffset updatedAt =
                    new DateTimeOffset(
                        2026,
                        10,
                        5,
                        12,
                        0,
                        0,
                        TimeSpan.FromHours(9));

                originalTransaction.UpdateDetails(
                    amountInMinorUnits: 1200,
                    type: TransactionType.Expense,
                    category: TransactionCategory.Transport,
                    paymentMethod: PaymentMethod.MobilePayment,
                    occurredAt: updatedAt,
                    note: "Updated transaction",
                    updatedAt: updatedAt);

                await firstRepository.UpdateAsync(
                    originalTransaction);

                firstDatabase.Dispose();
                firstDatabase = null;

                reopenedDatabase =
                    new CatLedgerDatabase(databasePath);

                var reopenedRepository =
                    new SQLiteTransactionRepository(
                        reopenedDatabase);

                Transaction loadedTransaction =
                    await reopenedRepository.GetByIdAsync(
                        originalTransaction.Id);

                Assert.That(
                    loadedTransaction.AmountInMinorUnits,
                    Is.EqualTo(1200));

                Assert.That(
                    loadedTransaction.Type,
                    Is.EqualTo(TransactionType.Expense));

                Assert.That(
                    loadedTransaction.Category,
                    Is.EqualTo(TransactionCategory.Transport));

                Assert.That(
                    loadedTransaction.PaymentMethod,
                    Is.EqualTo(PaymentMethod.MobilePayment));

                Assert.That(
                    loadedTransaction.Note,
                    Is.EqualTo("Updated transaction"));

                Assert.That(
                    loadedTransaction.UpdatedAt,
                    Is.EqualTo(updatedAt));
            }
            finally
            {
                firstDatabase?.Dispose();
                reopenedDatabase?.Dispose();

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
        }

        [Test]
        public async Task DeleteAsync_AfterDatabaseReopened_TransactionDoesNotExist()
        {
            string databasePath = Path.Combine(
                Path.GetTempPath(),
                $"catledger-test-{Guid.NewGuid():N}.db3");

            CatLedgerDatabase firstDatabase = null;
            CatLedgerDatabase reopenedDatabase = null;

            try
            {
                firstDatabase =
                    new CatLedgerDatabase(databasePath);

                var firstRepository =
                    new SQLiteTransactionRepository(
                        firstDatabase);

                Transaction transaction =
                    TransactionTestFactory.Create();

                await firstRepository.AddAsync(
                    transaction);

                await firstRepository.DeleteAsync(
                    transaction.Id);

                firstDatabase.Dispose();
                firstDatabase = null;

                reopenedDatabase =
                    new CatLedgerDatabase(databasePath);

                var reopenedRepository =
                    new SQLiteTransactionRepository(
                        reopenedDatabase);

                Assert.ThrowsAsync<KeyNotFoundException>(
                    async () =>
                        await reopenedRepository.GetByIdAsync(
                            transaction.Id));
            }
            finally
            {
                firstDatabase?.Dispose();
                reopenedDatabase?.Dispose();

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
        }
    }
}