﻿using ChilliCream.Tracing.Abstractions;
using FluentAssertions;
using System;
using Xunit;

namespace ChilliCream.Tracing.Tests
{
    public class ActivityStackTests
    {
        [Fact(DisplayName = "Id: Should return empty")]
        public void Id_Empty()
        {
            // act
            Guid result = ActivityStack.Id;

            // assert
            result.Should().BeEmpty();
        }

        [Fact(DisplayName = "Id: Should return an id")]
        public void Id_NotEmpty()
        {
            IDisposable disposable = null;

            try
            {
                // arrange
                Guid activityId = Guid.NewGuid();
                disposable = ActivityStack.Push(activityId);

                // act
                Guid result = ActivityStack.Id;

                // assert
                result.Should().Be(activityId);
            }
            finally
            {
                disposable?.Dispose();
            }
        }
    }
}