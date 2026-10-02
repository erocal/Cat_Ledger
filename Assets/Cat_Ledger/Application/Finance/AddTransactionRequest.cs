using System;
using CatLedger.Domain.Finance;

namespace CatLedger.Application.Finance
{

    /// <summary>
    /// 輸入收支請求
    /// </summary>
    public sealed class AddTransactionRequest
    {

        public TransactionDetails Details { get; }

        public AddTransactionRequest(TransactionDetails details)
        {
            Details = details
                ?? throw new ArgumentNullException(nameof(details));
        }

    }
}