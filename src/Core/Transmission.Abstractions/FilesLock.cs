using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Thor.Core.Transmission.Abstractions
{
    internal static class FilesLock
    {
        private static readonly ConcurrentDictionary<string, AsyncLock> Files =
            new ConcurrentDictionary<string, AsyncLock>();

        internal static async ValueTask<Releaser> WriteLockAsync(
            string fileName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var asyncLock = new AsyncLock();
            Files.TryAdd(fileName, asyncLock);

            return new Releaser(await asyncLock.LockAsync(cancellationToken));
        }

        internal static async ValueTask<Releaser> ReadLockAsync(
            string fileName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (Files.ContainsKey(fileName))
            {
                Files.TryRemove(fileName, out AsyncLock asyncLock);

                return new Releaser(await asyncLock.LockAsync(cancellationToken));
            }

            return Releaser.Empty;
        }

        internal struct Releaser : IDisposable
        {
            internal static Releaser Empty = new Releaser(AsyncLock.Releaser.Empty);

            private AsyncLock.Releaser _releaser;

            internal Releaser(AsyncLock.Releaser releaser)
            {
                _releaser = releaser;
            }

            public void Dispose()
            {
                _releaser.Dispose();
            }
        }
    }
}
