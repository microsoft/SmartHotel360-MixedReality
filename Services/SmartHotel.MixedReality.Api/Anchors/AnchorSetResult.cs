namespace SmartHotel.MixedReality.Api.Anchors
{
    public class AnchorSetResult
    {
        public AnchorSetResult(AnchorSet anchorSet)
        {
            Id = anchorSet.Id;
            Name = anchorSet.Name;
            NumberOfAnchors = anchorSet.Anchors?.Count ?? 0;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public int NumberOfAnchors {get; set; }
    }
}