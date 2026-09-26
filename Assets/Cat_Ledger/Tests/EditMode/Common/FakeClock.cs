using System;
using CatLedger.Application.Common;

namespace CatLedger.Tests.EditMode.Common
{
    public sealed class FakeClock : IClock
    {
        public DateTimeOffset Now { get; private set; }

        public FakeClock(DateTimeOffset initialTime)
        {
            Now = initialTime;
        }

        public void SetTime(DateTimeOffset newTime)
        {
            Now = newTime;
        }

        public void Advance(TimeSpan duration)
        {
            Now = Now.Add(duration);
        }
    }
}