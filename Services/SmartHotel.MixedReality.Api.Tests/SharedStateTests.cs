using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using NodaTime.Testing;
using SmartHotel.MixedReality.Api.SharedState;
using System.Threading.Tasks;

namespace SmartHotel.MixedReality.Api.Tests
{
    [TestClass]
    public class SharedStateTests
    {
        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task WhenSharedStateNotSet_ReturnNull()
        {
            SharedStateController controller = new SharedStateController(new TestDatabaseHandler<SharedState.SharedState>(), SystemClock.Instance);
            string anchorSetId = "anchorSetId";
            SharedState.SharedState state = controller.GetSharedState(anchorSetId).Result;
            state.Should().BeNull();
        }

        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task SetSharedState()
        {
            //Arrange
            SharedStateController controller = new SharedStateController(new TestDatabaseHandler<SharedState.SharedState>(), SystemClock.Instance);
            string anchorSetId = "anchorSetId";
            string spaceid = "spaceId";
            SharedStateDto sharedState = new SharedStateDto(new SharedState.SharedState()
            {
                Id = anchorSetId,
                CurrentSelectedSpace = spaceid
            });

            //Act
            await controller.UpdateSharedState(anchorSetId, sharedState);

            //Assert
            SharedState.SharedState fetchedState = await controller.GetSharedState(anchorSetId);
            fetchedState.Id.Should().Be(anchorSetId);
            fetchedState.CurrentSelectedSpace.Should().Be(spaceid);
        }


        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task UpdateSharedState_SetsCreatedTimeAndUpdatedTime()
        {
            Instant createdInstant = SystemClock.Instance.GetCurrentInstant();
            //Arrange
            SharedStateController controller = new SharedStateController(new TestDatabaseHandler<SharedState.SharedState>(), new FakeClock(createdInstant));
            string anchorSetId = "anchorSetId";
            string spaceid = "spaceId";
            SharedStateDto sharedState = new SharedStateDto(new SharedState.SharedState()
            {
                Id = anchorSetId,
                CurrentSelectedSpace = spaceid
            });

            //Act
            await controller.UpdateSharedState(anchorSetId, sharedState);

            //Assert
            SharedState.SharedState fetchedState = await controller.GetSharedState(anchorSetId);
            fetchedState.CreatedAt.Should().Be(createdInstant);
            fetchedState.UpdatedAt.Should().Be(createdInstant);

        }

        
        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task UpdateExistingSharedState_SetsUpdatedTime()
        {
            Instant createdInstant = SystemClock.Instance.GetCurrentInstant();
            //Arrange
            FakeClock fakeClock = new FakeClock(createdInstant);
            SharedStateController controller = new SharedStateController(new TestDatabaseHandler<SharedState.SharedState>(), fakeClock);
            string anchorSetId = "anchorSetId";
            string spaceid = "spaceId";
            SharedStateDto sharedState = new SharedStateDto(new SharedState.SharedState()
            {
                Id = anchorSetId,
                CurrentSelectedSpace = spaceid
            });

            //Act
            await controller.UpdateSharedState(anchorSetId, sharedState);

            fakeClock.AdvanceMinutes(1);
            await controller.UpdateSharedState(anchorSetId, sharedState);

            //Assert
            SharedState.SharedState fetchedState = await controller.GetSharedState(anchorSetId);
            fetchedState.CreatedAt.Should().Be(createdInstant);
            fetchedState.UpdatedAt.Should().Be(createdInstant.Plus(Duration.FromMinutes(1)));
        }
    }
}
