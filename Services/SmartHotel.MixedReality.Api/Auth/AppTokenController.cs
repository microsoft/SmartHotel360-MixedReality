

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SmartHotel.MixedReality.Api.Auth
{
    [Route("v1/apptoken")]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class AppTokenController : ControllerBase
    {
        private readonly string _applicationId;
        private readonly string _applicationKey;
        private readonly string _aadTenantId;
        private readonly object _spatialAnchorsAccountId;
        private readonly string _aadAuthority;
        private readonly SpatialServicesSettings _spatialServicesOptions;

        public AppTokenController(IOptions<SpatialServicesSettings> spatialServicesOptions)
        {
            // For now this is loaded from config. In future it should be a property of the incoming request
            // allowing us to support multiple applications
            _spatialServicesOptions = spatialServicesOptions.Value;
            _applicationId = _spatialServicesOptions.ApplicationId;
            _applicationKey = _spatialServicesOptions.ApplicationKey;
            _aadTenantId = _spatialServicesOptions.TenantId;
            _spatialAnchorsAccountId = _spatialServicesOptions.AccountId;
            _aadAuthority = $"https://login.microsoftonline.com/{_aadTenantId}";
        }

        // GET v1/apptoken
        [HttpGet]
        public async Task<string> GetAsync()
        {
            // TODO: make your application-specific authorization and authentication logic here
            // As an example, validate your user's MSA credentials, or Facebook, Google, etc...

            var aadRelyingParty = "https://sts.mixedreality.azure.com/";
            var clientKey = new ClientCredential( _applicationId, _applicationKey );
            var authContext = new AuthenticationContext( _aadAuthority );

            //var client = new ConfidentialClientApplication(aadClientId, aadAuthority, redirectUri, aadClientSecret, userTokenCache, appTokenCache);
            AuthenticationResult result;
            try
            {
                result = await authContext.AcquireTokenAsync( aadRelyingParty, clientKey );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.ToString() );
                throw new Exception( "Failed to authenticate" );
            }

            // now the the VPG token
            var mrCloudtoken = string.Empty;

            // Second, get a MR Cloud token
            using ( HttpRequestMessage httpRequest = new HttpRequestMessage() )
            {
                Uri.TryCreate( "https://mrc-auth-prod.trafficmanager.net/Accounts/" + _spatialAnchorsAccountId + "/token", UriKind.Absolute, out Uri uri );
                httpRequest.Method = System.Net.Http.HttpMethod.Get;
                httpRequest.RequestUri = uri;
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", result.AccessToken );
                httpRequest.Headers.Add( "X-MRC-CV", Guid.NewGuid().ToString() );

                var httpClient = new HttpClient();

                using ( HttpResponseMessage httpResponse = await httpClient.SendAsync( httpRequest ) )
                {
                    httpResponse.EnsureSuccessStatusCode();

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    JObject responseJson = JObject.Parse(responseContent);

                    mrCloudtoken = responseJson["AccessToken"].ToObject<string>();
                }
            }

            // return the result
            return mrCloudtoken;

        }
    }
}

