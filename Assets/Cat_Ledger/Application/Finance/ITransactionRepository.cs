using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);

        Task<Transaction> GetByIdAsync(Guid transactionId);

        Task<IReadOnlyList<Transaction>> GetMatchingAsync(TransactionFilter filter);

        Task UpdateAsync(Transaction transaction);

        Task DeleteAsync(Guid transactionId);
    }
}