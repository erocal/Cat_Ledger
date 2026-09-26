using SQLite;

namespace CatLedger.Infrastructure.Persistence.SQLite.Records
{
    [Table("Transactions")]
    public sealed class TransactionRecord
    {
        [PrimaryKey]
        public string Id { get; set; }

        public long AmountInMinorUnits { get; set; }

        public int Type { get; set; }

        public int Category { get; set; }

        public int PaymentMethod { get; set; }

        [Indexed]
        public long OccurredAtUnixMilliseconds { get; set; }

        public int OccurredAtOffsetMinutes { get; set; }

        public long CreatedAtUnixMilliseconds { get; set; }

        public int CreatedAtOffsetMinutes { get; set; }

        public long UpdatedAtUnixMilliseconds { get; set; }

        public int UpdatedAtOffsetMinutes { get; set; }

        public string Note { get; set; }
    }
}