using System;
using CatLedger.Domain.Finance;
using NUnit.Framework;

namespace CatLedger.Tests.EditMode.Finance
{
    public sealed class TransactionTests
    {
        [Test]
        public void Constructor_WithZeroAmount_ThrowsArgumentOutOfRangeException()
        {
            DateTimeOffset currentTime =
                new DateTimeOffset(2026, 9, 26, 18, 0, 0, TimeSpan.FromHours(9));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Transaction(
                    Guid.NewGuid(),
                    amountInMinorUnits: 0,
                    type: TransactionType.Expense,
                    category: TransactionCategory.Food,
                    paymentMethod: PaymentMethod.Cash,
                    occurredAt: currentTime,
                    note: string.Empty,
                    createdAt: currentTime,
                    updatedAt: currentTime);
            });
        }

        [Test]
        public void Constructor_WithWhitespaceNote_NormalizesNoteToEmptyString()
        {
            DateTimeOffset currentTime =
                new DateTimeOffset(2026, 9, 26, 18, 0, 0, TimeSpan.FromHours(9));

            var transaction = new Transaction(
                Guid.NewGuid(),
                amountInMinorUnits: 1200,
                type: TransactionType.Expense,
                category: TransactionCategory.Food,
                paymentMethod: PaymentMethod.Cash,
                occurredAt: currentTime,
                note: "   ",
                createdAt: currentTime,
                updatedAt: currentTime);

            Assert.That(
                transaction.Note,
                Is.EqualTo(string.Empty));
        }

        [Test]
        public void Constructor_WithNote_TrimsLeadingAndTrailingWhitespace()
        {
            DateTimeOffset currentTime =
                new DateTimeOffset(2026, 9, 26, 18, 0, 0, TimeSpan.FromHours(9));

            var transaction = new Transaction(
                Guid.NewGuid(),
                amountInMinorUnits: 1200,
                type: TransactionType.Expense,
                category: TransactionCategory.Food,
                paymentMethod: PaymentMethod.Cash,
                occurredAt: currentTime,
                note: "  Dinner  ",
                createdAt: currentTime,
                updatedAt: currentTime);

            Assert.That(
                transaction.Note,
                Is.EqualTo("Dinner"));
        }

    }
}