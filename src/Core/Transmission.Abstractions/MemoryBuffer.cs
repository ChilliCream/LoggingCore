using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using Thor.Core.Transmission.Abstractions;

namespace Thor.Core.Transmission.EventHub
{
    /// <summary>
    /// A memory transmission buffer
    /// </summary>
    public class MemoryBuffer<TData>
        : IMemoryBuffer<TData>
        where TData : class
    {
        private readonly BufferOptions _options;
        private readonly ChannelWriter<TData> _itemsWrite;
        private readonly ChannelReader<TData> _itemsRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryBuffer{TData}"/> class.
        /// </summary>
        /// <param name="options"></param>
        public MemoryBuffer(BufferOptions options)
        {
            _options = options;

            var items = Channel.CreateBounded<TData>(_options.Size);
            _itemsWrite = items.Writer;
            _itemsRead = items.Reader;
        }

        /// <inheritdoc />
        public void Enqueue(TData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if(!_itemsWrite.TryWrite(data))
            {
                SpinWait.SpinUntil(() =>
                        _itemsWrite.TryWrite(data),
                    _options.EnqueueTimeout);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TData> Dequeue(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (await _itemsRead.WaitToReadAsync(cancellationToken))
            {
                yield return await _itemsRead.ReadAsync(cancellationToken);
            }
        }
    }
}
