using System;
using Deefault.Common.Result;
using FluentAssertions;
using Xunit;

namespace ResultExtensions.Tests
{
    public class ResultTests
    {
        [Fact]
        public void TrueOperator_Should_Return_True()
        {
            var result = new Result();

            var entered = false;
            if (result)
            {
                entered = true;
            }

            entered.Should().BeTrue();
        }

        [Fact]
        public void TrueOperator_Should_Return_False()
        {
            var result = new Result("asdasd");

            // ReSharper disable once ReplaceWithSingleAssignment.False
            var entered = false;
            if (!result)
            {
                entered = true;
            }

            entered.Should().BeTrue();
        }
    }
    
    public class ResultOfTTests
    {
        [Fact]
        public void TrueOperator_Should_Return_True()
        {
            var result = Result.Succeed(2);

            var entered = false;
            if (result)
            {
                entered = true;
            }

            entered.Should().BeTrue();
            result.Value.Should().Be(2);
        }

        [Fact]
        public void TrueOperator_Should_Return_False()
        {
            var result = Result.Fail<int>("error");

            // ReSharper disable once ReplaceWithSingleAssignment.False
            var entered = false;
            if (!result)
            {
                entered = true;
            }

            entered.Should().BeTrue();
            result.Error.Should().Be("error");
        }
    }
}