using System.Threading.Tasks;
using CatLedger.Application.Finance;
using CatLedger.Domain.Finance;
using CatLedger.Infrastructure.Persistence;
using CatLedger.Tests.EditMode.Finance.TestData;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Finance
{
    public sealed class DeleteTransactionUseCaseTests
    {
        [Test]
        public async Task ExecuteAsync_WithExistingTransaction_RemovesTransaction()
        {
            var repository = new InMemoryTransactionRepository();

            Transaction transaction = TransactionTestFactory.Create();

            await repository.AddAsync(transaction);

            var useCase = new DeleteTransactionUseCase(repository);

            await useCase.ExecuteAsync(transaction.Id);

            Assert.That(
                repository.Transactions.Count,
                Is.EqualTo(0));
        }
    }
}