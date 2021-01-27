//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Moq;
//using Xunit;

//namespace HealthChecks.Extensions.Tests
//{
//    public class RetryHealthCheckTests
//    {
//        [Fact]
//        public async Task OriginalHealthCheck_Is_Executed_When_ThePredicate_Returns_True()
//        {
//            // Arrange
//            var decoratedHealthCheckMock = new Mock<IHealthCheck>();
//            var sut = new RetryHealthCheck(() => decoratedHealthCheckMock.Object, (_, __) => Task.FromResult(true), null, null);

//            // Act
//            await sut.CheckHealthAsync(new HealthCheckContextBuilder().WithInstance(decoratedHealthCheckMock.Object)
//                .Build());

//            // Assert
//            decoratedHealthCheckMock.Verify(c => c.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()), Times.Once);
//        }
//    }
//}