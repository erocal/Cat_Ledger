using System;
using System.Threading.Tasks;
using CatLedger.Application.Finance;
using CatLedger.Infrastructure.Persistence;
using CatLedger.Tests.EditMode.Finance.TestData;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Finance
{
    public sealed class GetTransactionByIdUseCaseTests
    {
        [Test]
        public async Task ExecuteAsync_WhenTransactionExists_ReturnsTransaction()
        {
            var repository =
                new InMemoryTransactionRepository();

            var transaction =
                TransactionTestFactory.Create();

            await repository.AddAsync(transaction);

            var useCase =
                new GetTransactionByIdUseCase(repository);

            var result =
                await useCase.ExecuteAsync(transaction.Id);

            Assert.That(
                result.Id,
                Is.EqualTo(transaction.Id));
        }

        [Test]
        public void ExecuteAsync_WhenTransactionIdIsEmpty_ThrowsArgumentException()
        {
            var repository =
                new InMemoryTransactionRepository();

            var useCase =
                new GetTransactionByIdUseCase(repository);

            Assert.ThrowsAsync<ArgumentException>(
                async () =>
                    await useCase.ExecuteAsync(Guid.Empty));
        }
    }
}