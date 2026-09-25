using System;

namespace CatLedger.Application.Common
{
    public interface IClock
    {
        DateTimeOffset Now { get; }
    }
}