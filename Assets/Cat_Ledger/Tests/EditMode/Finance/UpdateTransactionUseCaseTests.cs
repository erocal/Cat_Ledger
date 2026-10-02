using System;
using System.Threading.Tasks;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence;
using CatLedger.Tests.EditMode.Common;
using CatLedger.Tests.EditMode.Finance.TestData;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Finance
{
    public sealed class UpdateTransactionUseCaseTests
    {
        [Test]
        public async Task ExecuteAsync_WithValidRequest_UpdatesTransactionDetails()
        {
            DateTimeOffset originalTime =
                new DateTimeOffset(
                    2026, 9, 20, 12, 0, 0,
                    TimeSpan.FromHours(9));

            DateTimeOffset updatedTime =
                new DateTimeOffset(
                    2026, 9, 27, 20, 0, 0,
                    TimeSpan.FromHours(9));

            var repository =
                new InMemoryTransactionRepository();

            Transaction transaction =
                TransactionTestFactory.Create(
                    amountInMinorUnits: 500,
                    category: TransactionCategory.Food,
                    occurredAt: originalTime);

            await repository.AddAsync(transaction);

            var clock = new FakeClock(updatedTime);

            var useCase =
                new UpdateTransactionUseCase(
                    repository,
                    clock);

            TransactionDetails details = new TransactionDetails(
                    amountInMinorUnits: 800,
                    type: TransactionType.Expense,
                    category: TransactionCategory.Transport,
                    paymentMethod: PaymentMethod.MobilePayment,
                    occurredAt: originalTime,
                    note: "Bus and train"
                );

            var request =
                new UpdateTransactionRequest(
                    transaction.Id,
                    details
                    );

            Transaction updatedTransaction =
                await useCase.ExecuteAsync(request);

            Assert.That(
                updatedTransaction.AmountInMinorUnits,
                Is.EqualTo(800));

            Assert.That(
                updatedTransaction.Category,
                Is.EqualTo(TransactionCategory.Transport));

            Assert.That(
                updatedTransaction.PaymentMethod,
                Is.EqualTo(PaymentMethod.MobilePayment));

            Assert.That(
                updatedTransaction.Note,
                Is.EqualTo("Bus and train"));

            Assert.That(
                updatedTransaction.UpdatedAt,
                Is.EqualTo(updatedTime));

            Assert.That(
                updatedTransaction.CreatedAt,
                Is.EqualTo(transaction.CreatedAt));
        }
    }
}