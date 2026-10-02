using System.IO;
using CatLedger.Application.Common;
using CatLedger.Application.Finance;
using CatLedger.Infrastructure.Persistence.SQLite;
using CatLedger.Infrastructure.Time;
using CatLedger.Presentation.Finance.AddTransaction;
using CatLedger.Presentation.Finance.TransactionList;
using CatLedger.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CatLedger.Presentation.Bootstrap
{
    public sealed class CatLedgerLifetimeScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField]
        private AppNavigator appNavigator;

        [Header("Finance Views")]
        [SerializeField]
        private AddTransactionView addTransactionView;

        [SerializeField]
        private TransactionListView transactionListView;

        protected override void Configure(
            IContainerBuilder builder)
        {
            RegisterInfrastructure(builder);
            RegisterFinanceUseCases(builder);
            RegisterPresentation(builder);
        }

        private static void RegisterInfrastructure(
            IContainerBuilder builder)
        {
            string databasePath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "catledger.db3");

            var database =
                new CatLedgerDatabase(databasePath);

            builder.RegisterInstance(database);

            builder.Register<IClock, SystemClock>(
                Lifetime.Singleton);

            builder.Register<
                ITransactionRepository,
                SQLiteTransactionRepository>(
                Lifetime.Singleton);
        }

        private static void RegisterFinanceUseCases(
            IContainerBuilder builder)
        {
            builder.Register<AddTransactionUseCase>(
                Lifetime.Transient);

            builder.Register<GetTransactionsUseCase>(
                Lifetime.Transient);

            builder.Register<UpdateTransactionUseCase>(
                Lifetime.Transient);

            builder.Register<DeleteTransactionUseCase>(
                Lifetime.Transient);
        }

        private void RegisterPresentation(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(
                appNavigator);

            builder.RegisterComponent(
                addTransactionView);

            builder.RegisterComponent(
                transactionListView);

            builder.RegisterEntryPoint<
                AddTransactionPresenter>();

            builder.RegisterEntryPoint<
                TransactionListPresenter>();
        }
    }
}