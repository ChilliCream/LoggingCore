﻿using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Thor.Core.Abstractions;
using Xunit;

namespace Thor.Core.Transmission.EventHub.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        #region AddEventHubTelemetryEventTransmission

        [Fact(DisplayName = "AddEventHubTelemetryEventTransmission: Resolve telemetry transmitter")]
        public void AddEventHubTelemetryEventTransmission()
        {
            // arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"Tracing:EventHub:ConnectionString", "Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=Send;SharedAccessKey=67bHkkKw92k/pH6zU7ikSEXxo2oJJ67Kabf5CS4tg367=;EntityPath=rumba"}
            };

            builder.AddInMemoryCollection(data);

            IConfiguration configuration = builder.Build();

            // act
            services.AddEventHubTelemetryEventTransmission(configuration);

            // assert
            ServiceProvider provider = services.BuildServiceProvider();

            Assert.IsType<EventHubTransmitter>(provider.GetService<ITelemetryTransmitter>());
        }

        #endregion
    }
}