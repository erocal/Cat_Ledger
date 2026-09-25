using System;
using CatLedger.Application.Common;

namespace CatLedger.Infrastructure.Time
{
    public sealed class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}