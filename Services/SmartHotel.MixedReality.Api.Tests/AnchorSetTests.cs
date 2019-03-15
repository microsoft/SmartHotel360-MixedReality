using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartHotel.MixedReality.Api.Anchors;
using SmartHotel.MixedReality.Api.Data;
using FluentAssertions.Collections;

namespace SmartHotel.MixedReality.Api.Tests
{
    [TestClass]
    public class AnchorSetTests
    {
        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task CreateAnchorSet()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            string anchorSetName = "testanchorset";
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet(anchorSetName);
            Assert.IsNotNull(anchorSet);
            Assert.AreEqual(anchorSet.Name, anchorSetName);
        }

        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(NoAnchorSetException))]
        public async Task CreateVirtualAnchorWithNoAnchorSetThrowsException()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            AnchorSet anchorSet = await anchorSetService.CreateVirtualAnchor("anchorSetId", "anchorId");
        }

        

        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task CreateVirtualAnchor()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            string anchorSetName = "anchorSetId";
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet(anchorSetName);
            AnchorSet newAnchorSet = await anchorSetService.CreateVirtualAnchor(anchorSet.Id, "anchorId");
            newAnchorSet.Anchors.Should().HaveCount(1);
        }

        
        

        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task ThereCanOnlyBeOnVirtualAnchor()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            string anchorSetName = "anchorSetId";
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet(anchorSetName);
            AnchorSet newAnchorSet = await anchorSetService.CreateVirtualAnchor(anchorSet.Id, "anchorId");
            newAnchorSet = await anchorSetService.CreateVirtualAnchor(anchorSet.Id, "anchorId2");
            newAnchorSet.Anchors.Should().HaveCount(1);
            newAnchorSet.Anchors.First().Id.Should().Be("anchorId2");
        }

        
        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task ThereCanBeManyPhysicalAnchors()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            string anchorSetName = "anchorSetId";
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet(anchorSetName);

            AnchorSet newAnchorSet = await anchorSetService.CreatePhsyicalAnchor(anchorSet.Id, "anchorId", "deviceId1");
            newAnchorSet = await anchorSetService.CreatePhsyicalAnchor(anchorSet.Id, "anchorId2", "deviceId2");
            newAnchorSet = await anchorSetService.CreatePhsyicalAnchor(anchorSet.Id, "anchorId3", "deviceId3");
            newAnchorSet.Anchors.Should().HaveCount(3);
        }

        
        
        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task CanRemoveAndAddAnchor()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            string anchorSetName = "anchorSetId";
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet(anchorSetName);

            AnchorSet newAnchorSet = await anchorSetService.CreatePhsyicalAnchor(anchorSet.Id, "anchorId", "deviceId1");
            newAnchorSet = await anchorSetService.CreatePhsyicalAnchor(anchorSet.Id, "anchorId2", "deviceId2");
            newAnchorSet = await anchorSetService.CreatePhsyicalAnchor(anchorSet.Id, "anchorId2", "deviceId3");
            newAnchorSet.Anchors.Should().HaveCount(2);
            newAnchorSet.Anchors[1].DeviceId.Should().Be("deviceId3");
        }

        

        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task CanGetVirtualAnchorSetWithNoAnchors()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet("anchorSetId");

            anchorSet = await anchorSetService.GetVirtualAnchorSet(anchorSet.Id);
            anchorSet.Anchors.Should().NotBeNull();
            anchorSet.Anchors.Should().BeEmpty();
        }

        [TestMethod]
        [Owner("Scott Varcoe")]
        [TestCategory("UnitTest")]
        public async Task CanGetPhysicalAnchorSetWithNoAnchors()
        {
            AnchorSetService anchorSetService = new AnchorSetService(new TestDatabaseHandler<AnchorSet>());
            AnchorSet anchorSet = await anchorSetService.CreateAnchorSet("anchorSetId");

            anchorSet = await anchorSetService.GetPhysicalAnchorSet(anchorSet.Id);
            anchorSet.Anchors.Should().NotBeNull();
            anchorSet.Anchors.Should().BeEmpty();
        }
    }

    public class TestDatabaseHandler<T> : IDatabaseHandler<T>
    {
        public List<T> Entities = new List<T>();
        public Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            return Task.FromResult(Entities.Where(filter.Compile()).ToList()); 
        }

        public Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T entity)
        {
            T entityToReplace = Entities.FirstOrDefault(filter.Compile());
            Entities.Remove(entityToReplace);
            Entities.Add(entity);
            return Task.CompletedTask;
        }

        public Task DeleteOneAsync(Expression<Func<T, bool>> filter)
        {
            T entityToReplace = Entities.FirstOrDefault(filter.Compile());
            Entities.Remove(entityToReplace);
            return Task.CompletedTask;
        }

        public Task<T> InsertOneAsync(T entity)
        {
            Entities.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            return Task.FromResult(Entities.FirstOrDefault(filter.Compile())); 
        }

        public Task<List<T>> FindIn(Expression<Func<T, string>> field, IEnumerable<string> items)
        {
            throw new NotImplementedException();
        }
    }
}
