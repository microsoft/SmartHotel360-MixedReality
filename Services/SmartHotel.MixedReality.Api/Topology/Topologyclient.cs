using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHotel.MixedReality.Api.Topology
{
    public interface ITopologyClient
    {
        Task<ICollection<Space>> GetSpaces();
        Task<IDictionary<string, SpaceAlert>> GetRoomSpaceTemperatureAlerts();
        Task<ICollection<DigitalTwinsSpace>> GetTopLevelSpaces();
        Task<ICollection<DigitalTwinsSpace>> GetBrandLevelSpaces();
    }

    public class TopologyClient : ITopologyClient
    {
        private readonly IDigitalTwinsClient _digitalTwinsClient;
        private const string TenantTypeName = "Tenant";
        private const string HotelBrandTypeName = "HotelBrand";
        private const string HotelTypeName = "Hotel";
        private const string FloorTypeName = "Floor";
        private const string RoomTypeName = "Room";
        private readonly string ApiPath = "api/v1.0/";

        private readonly string SpacesPath = "spaces";
        private readonly string DevicesPath = "devices";

        private const string FirstFourLevelsSpacesFilter = "maxlevel=4&minlevel=1";
        private const string FirstLevelSpacesFilter = "maxlevel=2&minlevel=1";
        private const string FifthLevelSpacesFilter = "maxlevel=5&minlevel=5";
        private const string IncludesFilter = "includes";
        private const string PropertiesIncludesFilter = "Properties";
        private const string TypesIncludesFilter = "Types";
        private const string ValuesIncludesFilter = "Values";
        private const string SensorsIncludesFilter = "Sensors";
        private const string SensorsTypesIncludesFilter = "SensorsTypes";
        private const string TraverseDownFilter = "traverse=Down";
        

        public TopologyClient( IDigitalTwinsClient digitalTwinsClient )
        {
            _digitalTwinsClient = digitalTwinsClient;
        }

        public async Task<ICollection<Space>> GetSpaces()
        {
            ICollection<DigitalTwinsSpace> topology = await GetTopLevelSpaces();

            string fifthLevelResponse = await _digitalTwinsClient.GetFromDigitalTwinsAsString($"{ApiPath}{SpacesPath}" +
                                                                                      $"?{FifthLevelSpacesFilter}&{IncludesFilter}={TypesIncludesFilter}" );
            ICollection<DigitalTwinsSpace> fithLevelTopology = JsonConvert.DeserializeObject<ICollection<DigitalTwinsSpace>>( fifthLevelResponse );
            topology = topology.Union( fithLevelTopology ).ToArray();

            Space tenantSpace = null;
            Space hotelBrandSpace = null;
            Space hotelSpace = null;
            Space floorSpace = null;

            Dictionary<string, List<Space>> spacesByParentId = new Dictionary<string, List<Space>>();
            foreach ( DigitalTwinsSpace dtSpace in topology )
            {
                Space space = new Space
                {
                    Id = dtSpace.id,
                    Name = dtSpace.name,
                    FriendlyName = dtSpace.friendlyName,
                    Type = dtSpace.type,
                    TypeId = dtSpace.typeId,
                    Subtype = dtSpace.subtype,
                    SubtypeId = dtSpace.subtypeId,
                    ParentSpaceId = dtSpace.parentSpaceId ?? string.Empty,
                    Properties = dtSpace.properties?.ToList()
                };

                if ( tenantSpace == null && TenantTypeName.Equals( dtSpace.type, StringComparison.OrdinalIgnoreCase ) )
                {
                    tenantSpace = space;
                }
                else if ( tenantSpace == null
                         && hotelBrandSpace == null
                         && HotelBrandTypeName.Equals( dtSpace.type, StringComparison.OrdinalIgnoreCase ) )
                {
                    hotelBrandSpace = space;
                }
                else if ( tenantSpace == null
                         && hotelBrandSpace == null
                         && hotelSpace == null
                         && HotelTypeName.Equals( dtSpace.type, StringComparison.OrdinalIgnoreCase ) )
                {
                    hotelSpace = space;
                }
                else if ( tenantSpace == null
                          && hotelBrandSpace == null
                          && hotelSpace == null
                          && floorSpace == null
                          && FloorTypeName.Equals( dtSpace.type, StringComparison.OrdinalIgnoreCase ) )
                {
                    floorSpace = space;
                }

                if ( !spacesByParentId.TryGetValue( space.ParentSpaceId, out List<Space> spaces ) )
                {
                    spaces = new List<Space>();
                    spacesByParentId.Add( space.ParentSpaceId, spaces );
                }

                spaces.Add( space );
            }

            List<Space> hierarchicalSpaces = new List<Space>();
            Space highestLevelSpace = GetHighestLevelSpace( tenantSpace, hotelBrandSpace, hotelSpace, floorSpace );
            if ( highestLevelSpace != null )
            {
                string highestLevelParentSpaceId = highestLevelSpace.ParentSpaceId;
                hierarchicalSpaces.AddRange( spacesByParentId[highestLevelParentSpaceId] );
                string typeToGetDevicesFor = highestLevelSpace.Type == FloorTypeName ? FloorTypeName : HotelBrandTypeName;
                await BuildSpaceHierarchyAndReturnRoomSpacesAsync( hierarchicalSpaces, spacesByParentId, typeToGetDevicesFor );
            }

            if ( hierarchicalSpaces.Count == 1 && !FloorTypeName.Equals( hierarchicalSpaces[0].Type, StringComparison.OrdinalIgnoreCase ) )
            {
                // If there is only one root space, then ensuring we only send the child spaces to the digitalTwinsClient so it knows
                // to start showing those children.
                hierarchicalSpaces = hierarchicalSpaces[0].ChildSpaces;
            }


            return hierarchicalSpaces;
        }

        public async Task<ICollection<DigitalTwinsSpace>> GetTopLevelSpaces()
        {
            string firstFourLevelsResponse = await _digitalTwinsClient.GetFromDigitalTwinsAsString($"{ApiPath}{SpacesPath}" +
                                                                                                   $"?{FirstFourLevelsSpacesFilter}&{IncludesFilter}={PropertiesIncludesFilter},{TypesIncludesFilter}");
            ICollection<DigitalTwinsSpace> topology =
                JsonConvert.DeserializeObject<ICollection<DigitalTwinsSpace>>(firstFourLevelsResponse);
            return topology;
        }



        public async Task<ICollection<DigitalTwinsSpace>> GetBrandLevelSpaces()
        {
            string firstFourLevelsResponse = await _digitalTwinsClient.GetFromDigitalTwinsAsString( $"{ApiPath}{SpacesPath}" +
                                                                                                    $"?{FirstLevelSpacesFilter}&{IncludesFilter}={PropertiesIncludesFilter},{TypesIncludesFilter}" );
            ICollection<DigitalTwinsSpace> topology =
                JsonConvert.DeserializeObject<ICollection<DigitalTwinsSpace>>( firstFourLevelsResponse );
            return topology;
        }

        public async Task<IDictionary<string, SpaceAlert>> GetRoomSpaceTemperatureAlerts()
        {
            string firstFourLevelsResponse = await _digitalTwinsClient.GetFromDigitalTwinsAsString($"{ApiPath}{SpacesPath}" +
                                                                                           $"?{FirstFourLevelsSpacesFilter}&{IncludesFilter}={TypesIncludesFilter}" );
            Dictionary<string, DigitalTwinsSpace> firstFourLevelsTopology = JsonConvert.DeserializeObject<ICollection<DigitalTwinsSpace>>( firstFourLevelsResponse )
                .ToDictionary( dts => dts.id );

            string fifthLevelResponse = await _digitalTwinsClient.GetFromDigitalTwinsAsString($"{ApiPath}{SpacesPath}" +
                                                                                      $"?{FifthLevelSpacesFilter}&{IncludesFilter}={ValuesIncludesFilter},{TypesIncludesFilter}" );
            ICollection<DigitalTwinsSpace> fifthLevelTopologyWithValues = JsonConvert.DeserializeObject<ICollection<DigitalTwinsSpace>>( fifthLevelResponse );
            Dictionary<string, SpaceAlert> alertMessagesBySpaceId = fifthLevelTopologyWithValues
                .Where( dts => dts.values != null )
                .Select( dts => new
                {
                    dts,
                    value = dts.values.FirstOrDefault( v => "TemperatureAlert".Equals( v.type, StringComparison.OrdinalIgnoreCase ) )
                } )
                .Where( ta => ta.value != null )
                .ToDictionary( spaceIdWithAlert => spaceIdWithAlert.dts.id, spaceIdWithAlert => new SpaceAlert
                {
                    SpaceId = spaceIdWithAlert.dts.id,
                    Message = spaceIdWithAlert.value.value,
                    AncestorSpaceIds = GetAncestorSpaceIds( spaceIdWithAlert.dts, firstFourLevelsTopology )
                } );

            return alertMessagesBySpaceId;
        }

        private async Task BuildSpaceHierarchyAndReturnRoomSpacesAsync( List<Space> hierarchicalSpaces,
            Dictionary<string, List<Space>> allSpacesByParentId, string typeToGetDevicesFor, Dictionary<string, List<Device>> devicesBySpaceIdFromAncestor = null )
        {
            foreach ( Space parentSpace in hierarchicalSpaces )
            {
                Dictionary<string, List<Device>> devicesBySpaceId = devicesBySpaceIdFromAncestor;
                if ( ( devicesBySpaceId == null || devicesBySpaceId.Count == 0 )
                    && parentSpace.Type == typeToGetDevicesFor )
                {
                    devicesBySpaceId = await GetAllDescendantDevicesBySpaceIdForSpace( parentSpace.Id );
                }

                if ( devicesBySpaceId != null
                    && devicesBySpaceId.TryGetValue( parentSpace.Id, out List<Device> devicesForSpace ) )
                {
                    parentSpace.Devices = devicesForSpace;
                }

                if ( allSpacesByParentId.TryGetValue( parentSpace.Id, out List<Space> childSpaces ) )
                {
                    if ( parentSpace.ChildSpaces == null )
                    {
                        parentSpace.ChildSpaces = new List<Space>();
                    }

                    parentSpace.ChildSpaces.AddRange( childSpaces );
                    await BuildSpaceHierarchyAndReturnRoomSpacesAsync( childSpaces, allSpacesByParentId, typeToGetDevicesFor, devicesBySpaceId );
                }
            }
        }

        private async Task<Dictionary<string, List<Device>>> GetAllDescendantDevicesBySpaceIdForSpace( string spaceId )
        {
            string devicesString = await _digitalTwinsClient.GetFromDigitalTwinsAsString($"{ApiPath}{DevicesPath}" +
                                                                                 $"?spaceId={spaceId}&{IncludesFilter}={TypesIncludesFilter},{SensorsIncludesFilter},{SensorsTypesIncludesFilter}" +
                                                                                 $"&{TraverseDownFilter}" );
            Device[] devices = JsonConvert.DeserializeObject<Device[]>( devicesString );

            return devices.GroupBy( d => d.spaceId )
                .ToDictionary( g => g.Key, g => g.ToList() );
        }

        private Space GetHighestLevelSpace( Space tenantSpace, Space hotelBrandSpace, Space hotelSpace, Space floorSpace )
        {
            if ( tenantSpace != null )
            {
                return tenantSpace;
            }

            if ( hotelBrandSpace != null )
            {
                return hotelBrandSpace;
            }

            if ( hotelSpace != null )
            {
                return hotelSpace;
            }

            if ( floorSpace != null )
            {
                return floorSpace;
            }

            return null;
        }

        private string[] GetAncestorSpaceIds( DigitalTwinsSpace childSpace, Dictionary<string, DigitalTwinsSpace> ancestorSpacesById )
        {
            List<string> ancestorSpaceIds = new List<string>();

            DigitalTwinsSpace nextSpace = childSpace;
            while ( !string.IsNullOrWhiteSpace( nextSpace.parentSpaceId ) )
            {
                ancestorSpaceIds.Add( nextSpace.parentSpaceId );
                if ( ancestorSpacesById.TryGetValue( nextSpace.parentSpaceId, out DigitalTwinsSpace parentSpace ) )
                {
                    nextSpace = parentSpace;
                }
                else
                {
                    break;
                }
            }

            return ancestorSpaceIds.ToArray();
        }
    }
}