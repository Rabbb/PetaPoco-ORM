using System.Runtime.InteropServices;
using System;

namespace PetaPoco
{
    /// <summary>
    /// A scoped Transaction object to facilitate maintaining transaction depth counts and proper rollbacks.
    /// </summary>
    public class Transaction : ITransaction
    {
        private IDatabase? _db;
        private bool isDisposed;

        /// <summary>
        /// Creates a new Transaction instance for the specified database, and begins the transaction.
        /// </summary>
        /// <param name="database">The database instance that will execute the transaction.</param>
        /// <seealso cref="IDatabase.BeginTransaction"/>
        public Transaction(IDatabase database)
        {
            _db = database;
            _db.BeginTransaction();
        }

        /// <inheritdoc/>
        public void Complete()
        {
            if (_db != null)
            {
                _db.CompleteTransaction();
                _db = null;
            }
        }


        /// <summary>
        /// Closes the transaction scope, rolling back the transaction if not completed by a call to <see cref="Complete"/>.
        /// </summary>
        /// <seealso cref="IDatabase.AbortTransaction"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The bulk of the clean-up code is implemented in Dispose(bool)
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            _db?.AbortTransaction();
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
            }

            isDisposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~Transaction()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}
