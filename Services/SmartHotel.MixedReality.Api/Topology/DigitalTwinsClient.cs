using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SmartHotel.MixedReality.Api.Topology
{
    public interface IDigitalTwinsClient
    {
        HttpClient CreateHttpClient();
        Task<string> GetFromDigitalTwinsAsString(string requestUri);
        Task<byte[]> GetFromDigitalTwinsAsBytes( string requestUri );
    }

    public class DigitalTwinsClient : IDigitalTwinsClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly DigitalTwinsSettings _digitalTwinsSettings;

        private Lazy<string> AccessToken { get; }

        public DigitalTwinsClient(IHttpClientFactory clientFactory, IOptions<DigitalTwinsSettings> digitalTwinsSettings)
        {
            _clientFactory = clientFactory;
            _digitalTwinsSettings = digitalTwinsSettings.Value;
            AccessToken = new Lazy<string>( () => AuthenticateAndGetAccessTokenAsync().GetAwaiter().GetResult() );
        }

        public HttpClient CreateHttpClient()
        {
            HttpClient httpClient = _clientFactory.CreateClient();
            string managementBaseUrl = _digitalTwinsSettings.ManagementApiUrl;
            string protectedManagementBaseUrl = managementBaseUrl.EndsWith( '/' ) ? managementBaseUrl : $"{managementBaseUrl}/";
            httpClient.BaseAddress = new Uri( protectedManagementBaseUrl );
            httpClient.DefaultRequestHeaders.Add( "Authorization", $"Bearer {AccessToken.Value}" );
            return httpClient;
        }

        public async Task<string> GetFromDigitalTwinsAsString( string requestUri )
        {
            HttpClient httpClient = CreateHttpClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync( requestUri );
            string content = await httpResponse.Content.ReadAsStringAsync();
            if ( !httpResponse.IsSuccessStatusCode )
            {
                throw new Exception( $"Error when calling Digital Twins with request ({requestUri}): {content}" );
            }

            return content;
        }


        public async Task<byte[]> GetFromDigitalTwinsAsBytes( string requestUri )
        {
            HttpClient httpClient = CreateHttpClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync( requestUri );
            byte[] content = await httpResponse.Content.ReadAsByteArrayAsync();
            if ( !httpResponse.IsSuccessStatusCode )
            {
                throw new Exception( $"Error when calling Digital Twins with request {requestUri}" );
            }

            return content;
        }

        private async Task<string> AuthenticateAndGetAccessTokenAsync()
        {
            AuthenticationContext authContext = new AuthenticationContext( $"{_digitalTwinsSettings.AadInstance}{_digitalTwinsSettings.TenantId}" );
            try
            {

                AuthenticationResult result = await authContext.AcquireTokenAsync( _digitalTwinsSettings.DigitalTwinsResourceId, new ClientCredential( _digitalTwinsSettings.ClientId, _digitalTwinsSettings.ClientSecret ) );
                return result.AccessToken;
            }
            catch ( AdalServiceException ex )
            {
                Console.WriteLine( $"Error Authenticating: {ex}" );
                return null;
            }
        }
    }
}