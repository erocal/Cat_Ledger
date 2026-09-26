using System;
using System.IO;
using System.Threading.Tasks;
using CatLedger.Infrastructure.Persistence.SQLite.Records;
using SQLite;

namespace CatLedger.Infrastructure.Persistence.SQLite
{
    public sealed class CatLedgerDatabase
    {
        private readonly SQLiteAsyncConnection _connection;
        private Task _initializationTask;

        internal SQLiteAsyncConnection Connection => _connection;

        public CatLedgerDatabase(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException(
                    "Database path cannot be empty.",
                    nameof(databasePath));
            }

            string directoryPath = Path.GetDirectoryName(databasePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            _connection = new SQLiteAsyncConnection(databasePath);
        }

        public Task InitializeAsync()
        {
            if (_initializationTask != null)
            {
                return _initializationTask;
            }

            _initializationTask = InitializeInternalAsync();

            return _initializationTask;
        }

        public Task CloseAsync()
        {
            return _connection.CloseAsync();
        }

        private async Task InitializeInternalAsync()
        {
            await _connection.CreateTableAsync<TransactionRecord>();
        }
    }
}