﻿using System;
using System.Diagnostics.Tracing;
using Thor.Core.Transmission.Abstractions;

namespace Thor.Core.Session.Abstractions
{
    /// <summary>
    /// An <c>ETW</c> telemetry session to listen to events.
    /// </summary>
    public interface ITelemetrySession
        : IDisposable
    {
        /// <summary>
        /// Enables a custom event provider by its name and the desired severity.
        /// </summary>
        /// <param name="name">A provider name.</param>
        /// <param name="level">A level of verbosity.</param>
        void EnableProvider(string name, EventLevel level);

        /// <summary>
        /// Attaches a transmitter for telemetry event transmission.
        /// </summary>
        /// <param name="transmitter">A transmitter instance.</param>
        void Attach(ITelemetryEventTransmitter transmitter);
    }
}