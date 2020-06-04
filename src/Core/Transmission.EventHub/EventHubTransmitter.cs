using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Thor.Core.Abstractions;
using Thor.Core.Transmission.Abstractions;

namespace Thor.Core.Transmission.EventHub
{
    /// <summary>
    /// A telemetry event transmitter for <c>Azure</c> <c>EventHub</c>.
    /// </summary>
    public sealed class EventHubTransmitter
        : ITelemetryEventTransmitter
        , IDisposable
    {
        private readonly CancellationTokenSource _disposeToken = new CancellationTokenSource();
        private readonly IMemoryBuffer<EventData> _buffer;
        private readonly ITransmissionBuffer<EventData> _aggregator;
        private readonly ITransmissionSender<EventData[]> _sender;
        private readonly ITransmissionStorage<EventData> _storage;
        private readonly EventsOptions _options;
        private readonly Task _storeTask;
        private readonly Task _aggregateTask;
        private readonly Task _sendTask;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubTransmitter"/> class.
        /// </summary>
        public EventHubTransmitter(
            IMemoryBuffer<EventData> buffer,
            ITransmissionBuffer<EventData> aggregator,
            ITransmissionSender<EventData[]> sender,
            ITransmissionStorage<EventData> storage,
            EventsOptions options)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _storeTask = TaskHelper
                .StartLongRunning(StoreAsync, _disposeToken.Token);
            _aggregateTask = TaskHelper
                .StartLongRunning(AggregateAsync, _disposeToken.Token);
            _sendTask = TaskHelper
                .StartLongRunning(SendAsync, _disposeToken.Token);
        }

        /// <inheritdoc />
        public void Enqueue(TelemetryEvent data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!_disposeToken.IsCancellationRequested)
            {
                _buffer.Enqueue(data.Map());
            }
        }

        private async Task SendAsync()
        {
            await _sender
                .SendAsync(_aggregator.Dequeue(_disposeToken.Token), _disposeToken.Token)
                .ConfigureAwait(false);
        }

        private async Task StoreAsync()
        {
            await _storage
                .EnqueueAsync(_buffer.Dequeue(_disposeToken.Token), _disposeToken.Token)
                .ConfigureAwait(false);
        }

        private async Task AggregateAsync()
        {
            await foreach (EventData data in _storage.DequeueAsync(_disposeToken.Token))
            {
                await _aggregator.Enqueue(data, _disposeToken.Token);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposeToken.Cancel();

                SpinWait.SpinUntil(() =>
                        _sendTask.Status != TaskStatus.Running &&
                        _aggregateTask.Status != TaskStatus.Running &&
                        _storeTask.Status != TaskStatus.Running,
                    TimeSpan.FromSeconds(5));

                _disposeToken?.Dispose();
                _disposed = true;
            }
        }
    }
}
