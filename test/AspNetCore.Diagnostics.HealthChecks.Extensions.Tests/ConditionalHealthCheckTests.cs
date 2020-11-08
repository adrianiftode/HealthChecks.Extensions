using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AspNetCore.Diagnostics.HealthChecks.Extensions.Tests
{
    public class ConditionalHealthCheckTests
    {
        [Fact]
        public void Ctor_Logger_Is_Optional()
        {
            // Arrange
            Action act = () => new ConditionalHealthCheck(() => null, (_, __) => null, null,logger: null);

            // Act
            act.Should().NotThrow();
        }

        [Fact]
        public void Ctor_Options_Are_Optional()
        {
            // Arrange
            Action act = () => new ConditionalHealthCheck(() => null, (_, __) => null, options:null, null);

            // Act
            act.Should().NotThrow();
        }

        [Fact]
        public void Ctor_Health_Check_Factory_Is_Required()
        {
            // Arrange
            Action act = () => new ConditionalHealthCheck(null!, (_, __) => null, null, null);

            // Act
            act.Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be("healthCheckFactory");
        }

        [Fact]
        public void Ctor_Health_Predicate_Is_Required()
        {
            // Arrange
            Action act = () => new ConditionalHealthCheck(() => null, null!, null, null);

            // Act
            act.Should().ThrowExactly<ArgumentNullException>()
                .And.ParamName.Should().Be("predicate");
        }

        [Fact]
        public async Task OriginalHealthCheck_Is_Executed_When_ThePredicate_Returns_True()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(true), null, null);

            // Act
            await sut.CheckHealthAsync(new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build());

            // Assert
            decoratedHealthCheckMock.Verify(c => c.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OriginalHealthCheck_Is_Not_Executed_When_ThePredicate_Returns_False()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), null, null);

            // Act
            await sut.CheckHealthAsync(new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build());

            // Assert
            decoratedHealthCheckMock.Verify(c => c.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Context_Contains_NotChecked_Tag_When_The_Original_Check_Is_Not_Executed()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), null, null);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            await sut.CheckHealthAsync(context);

            // Assert
            context.Registration.Tags.Should().Contain("NotChecked");
        }

        [Fact]
        public async Task Context_Contains_Given_Tag_When_The_Original_Check_Is_Not_Executed()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), new ConditionalHealthOptions
            {
                NotCheckedTagName = "MyTag"
            }, null);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            await sut.CheckHealthAsync(context);

            // Assert
            context.Registration.Tags.Should().Contain("MyTag");
        }

        [Fact]
        public async Task Context_Does_Not_Contain_NotChecked_Tag_When_The_Original_Check_Is_Executed()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(true), null, null);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            await sut.CheckHealthAsync(context);

            // Assert
            context.Registration.Tags.Should().NotContain("NotChecked");
        }

        [Fact]
        public async Task Result_Has_A_Meaningful_Description()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), null, null);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            var result = await sut.CheckHealthAsync(context);

            // Assert
            result.Description.Should().Match("*`TheCheck` will not be evaluated*");
        }

        [Fact]
        public async Task Result_Is_Healthy()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), null, null);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            var result = await sut.CheckHealthAsync(context);

            // Assert
            result.Status.Should().Be(HealthStatus.Healthy);
        }

        [Fact]
        public async Task Result_Is_As_Set_In_Options()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), new ConditionalHealthOptions
            {
                HealthStatusWhenNotChecked = HealthStatus.Degraded
            }, null);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            var result = await sut.CheckHealthAsync(context);

            // Assert
            result.Status.Should().Be(HealthStatus.Degraded);
        }

        [Fact]
        public async Task Logs_a_Debug_Message_When_The_Original_Health_Check_Is_Not_Executed()
        {
            // Arrange
            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
            var loggerMock = new Mock<ILogger<ConditionalHealthCheck>>();
            var sut = new ConditionalHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(false), null, loggerMock.Object);
            var context = new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
                .Build();

            // Act
            await sut.CheckHealthAsync(context);

            // Assert
            loggerMock.VerifyLog(logger => logger.LogDebug("HealthCheck `TheCheck` will not be executed as its checking condition is not met."));
        }
    }
}