using System;
using System.Threading.Tasks;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence;
using CatLedger.Tests.EditMode.Common;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Finance
{
    public sealed class AddTransactionUseCaseTests
    {
        [Test]
        public async Task ExecuteAsync_WithValidRequest_AddsTransactionToRepository()
        {
            // Arrange
            DateTimeOffset currentTime =
                new DateTimeOffset(2026, 9, 26, 18, 0, 0, TimeSpan.FromHours(9));

            var clock = new FakeClock(currentTime);
            var transactionRepository = new InMemoryTransactionRepository();

            var useCase = new AddTransactionUseCase(
                transactionRepository,
                clock);

            DateTimeOffset occurredAt =
                new DateTimeOffset(2026, 9, 25, 19, 30, 0, TimeSpan.FromHours(9));

            var request = new AddTransactionRequest(
                amountInMinorUnits: 1200,
                type: TransactionType.Expense,
                category: TransactionCategory.Food,
                paymentMethod: PaymentMethod.CreditCard,
                occurredAt: occurredAt,
                note: "一蘭拉麵");

            // Act
            Transaction createdTransaction =
                await useCase.ExecuteAsync(request);

            // Assert
            Assert.That(
                transactionRepository.Transactions.Count,
                Is.EqualTo(1));

            Transaction storedTransaction =
                transactionRepository.Transactions[0];

            Assert.That(
                storedTransaction.Id,
                Is.EqualTo(createdTransaction.Id));

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
                storedTransaction.Note,
                Is.EqualTo("一蘭拉麵"));

            Assert.That(
                storedTransaction.CreatedAt,
                Is.EqualTo(currentTime));

            Assert.That(
                storedTransaction.UpdatedAt,
                Is.EqualTo(currentTime));
        }
    }
}