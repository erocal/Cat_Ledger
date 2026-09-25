using System.Threading.Tasks;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
    }
}