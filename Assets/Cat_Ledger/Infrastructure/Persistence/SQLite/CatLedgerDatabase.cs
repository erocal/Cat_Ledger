using System;
using System.IO;
using CatLedger.Infrastructure.Persistence.SQLite.Records;
using SQLite;

namespace CatLedger.Infrastructure.Persistence.SQLite
{
    public sealed class CatLedgerDatabase : IDisposable
    {
        private readonly SQLiteConnection _connection;

        private bool _isInitialized;
        private bool _isDisposed;

        internal SQLiteConnection Connection
        {
            get
            {
                ThrowIfDisposed();
                return _connection;
            }
        }

        public CatLedgerDatabase(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException(
                    "Database path cannot be empty.",
                    nameof(databasePath));
            }

            string directoryPath =
                Path.GetDirectoryName(databasePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            _connection =
                new SQLiteConnection(databasePath);
        }

        public void Initialize()
        {
            ThrowIfDisposed();

            if (_isInitialized)
            {
                return;
            }

            _connection.CreateTable<TransactionRecord>();

            _isInitialized = true;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _connection.Dispose();

            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    nameof(CatLedgerDatabase));
            }
        }
    }
}