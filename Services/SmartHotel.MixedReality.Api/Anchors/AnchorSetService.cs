using SmartHotel.MixedReality.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHotel.MixedReality.Api.Anchors
{
    public interface IAnchorSetService
    {
        Task<List<AnchorSet>> GetAllAnchorSets();
        Task<AnchorSet> GetVirtualAnchorSet(string anchorSetId);
        Task<AnchorSet> GetPhysicalAnchorSet(string anchorSetId);
        Task<AnchorSet> CreateAnchorSet(string anchorSetName);
        Task<AnchorSet> CreateVirtualAnchor(string anchorSetId, string anchorId);
        Task<AnchorSet> CreatePhsyicalAnchor(string anchorSetId, string anchorId, string deviceId);
        Task DeleteAnchorSet(string anchorSetId);
        Task<AnchorSet> DeleteAnchor(string anchorSetId, string anchorId);
    }

    public class AnchorSetService : IAnchorSetService
    {
        private readonly IDatabaseHandler<AnchorSet> _anchorSetDatabaseHandler;

        public AnchorSetService(IDatabaseHandler<AnchorSet> anchorSetDatabaseHandler)
        {
            _anchorSetDatabaseHandler = anchorSetDatabaseHandler;
        }

        private async Task<AnchorSet> CreateOrUpdateAnchor(string anchorSetId, string anchorId, string mode, string deviceId = null)
        {
            AnchorSet anchorSet = await _anchorSetDatabaseHandler.FindOne(a => a.Id == anchorSetId);
            if (anchorSet == null)
                throw new NoAnchorSetException(anchorSetId);
            if (anchorSet.Anchors == null)
            {
                anchorSet.Anchors = new List<Anchor>();
            }

            Anchor existingAnchor = anchorSet.Anchors.FirstOrDefault(a => a.Id == anchorId);
            if (existingAnchor != null)
            {
                anchorSet.Anchors.Remove(existingAnchor);
            }
            
            if (mode == VisualizerModes.Virtual)
            {
                Anchor existingVirtualAnchor = anchorSet.Anchors.FirstOrDefault();
                anchorSet.Anchors.Remove(existingVirtualAnchor);
            }

            Anchor anchor = new Anchor() { Id = anchorId, Mode = mode, DeviceId = deviceId };
            anchorSet.Anchors.Add(anchor);
            await _anchorSetDatabaseHandler.ReplaceOneAsync(a => a.Id == anchorSetId, anchorSet);
            return anchorSet;
        }

        public async Task<List<AnchorSet>> GetAllAnchorSets()
        {
            List<AnchorSet> results = await _anchorSetDatabaseHandler.Find(a => true);
            return results;
        }

        public async Task<AnchorSet> GetVirtualAnchorSet(string anchorSetId)
        {
            AnchorSet results = await _anchorSetDatabaseHandler.FindOne(a => a.Id == anchorSetId);
            results.Anchors = results.Anchors?.Where(a => a.Mode == VisualizerModes.Virtual)?.ToList() ?? new List<Anchor>();
            return results;
        }

        public async Task<AnchorSet> GetPhysicalAnchorSet(string anchorSetId)
        {
            AnchorSet results = await _anchorSetDatabaseHandler.FindOne(a => a.Id == anchorSetId);
            results.Anchors = results.Anchors?.Where(a => a.Mode == VisualizerModes.Physical)?.ToList() ?? new List<Anchor>();
            return results;
        }

        public async Task<AnchorSet> CreateAnchorSet(string anchorSetName)
        {
            AnchorSet anchorSet = new AnchorSet() { Name = anchorSetName, Id = Guid.NewGuid().ToString(), Anchors = new List<Anchor>()};
            await _anchorSetDatabaseHandler.InsertOneAsync(anchorSet);
            return anchorSet;
        }

        public async Task<AnchorSet> CreateVirtualAnchor(string anchorSetId, string anchorId)
        {
            AnchorSet anchorSet = await CreateOrUpdateAnchor(anchorSetId, anchorId, VisualizerModes.Virtual);
            return anchorSet;
        }

        public async Task<AnchorSet> CreatePhsyicalAnchor(string anchorSetId, string anchorId, string deviceId)
        {
            AnchorSet anchorSet = await CreateOrUpdateAnchor(anchorSetId, anchorId, VisualizerModes.Physical, deviceId);
            return anchorSet;
        }

        public async Task DeleteAnchorSet(string anchorSetId)
        {
            await _anchorSetDatabaseHandler.DeleteOneAsync(a => a.Id == anchorSetId);
        }

        public async Task<AnchorSet> DeleteAnchor(string anchorSetId, string anchorId)
        {
            AnchorSet anchorSet = await _anchorSetDatabaseHandler.FindOne(a => a.Id == anchorSetId);
            anchorSet.Anchors.RemoveAll(a => a.Id == anchorId);
            await _anchorSetDatabaseHandler.ReplaceOneAsync(a => a.Id == anchorSetId, anchorSet);
            return anchorSet;
        }
    }

    public class NoAnchorSetException : Exception
    {
        public NoAnchorSetException(string anchorSetId) : base($"No anchor set exists with id '{anchorSetId}'.")
        {
            
        }
    }

    public class DatabaseSettings
    {
        public string MongoDbConnectionString { get; set; }
        public string MongoDbName { get; set; }
    }
}
