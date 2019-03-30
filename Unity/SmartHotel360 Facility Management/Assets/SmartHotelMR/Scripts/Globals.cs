using System;

namespace SmartHotelMR
{
	public static class Globals
	{
		//Base URL of SmartHotelMR Service
		private static readonly string _serviceBaseUrl = "";
		public static string ServiceBaseUrl
		{
			get { return new Uri( new Uri( _serviceBaseUrl ), "/v1" ).ToString(); }
		}
		//API Key required to talk to SmartHotelMR Service
		public static readonly string ApiKey = "";

		//Azure Spatial Anchors Account ID
		public static readonly string SpatialAnchorsAccountId = "";
	}
}
