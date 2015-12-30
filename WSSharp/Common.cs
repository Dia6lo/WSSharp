using System;
using System.Net;

namespace WSSharp
{
	internal static class Common
	{
		public static IPAddress ParseIPAddress(Uri uri)
		{
			var ipStr = uri.Host;

			if (ipStr == "0.0.0.0" || ipStr == "[0000:0000:0000:0000:0000:0000:0000:0000]") {
				return IPAddress.IPv6Any;
			}
			try {
				return IPAddress.Parse(ipStr);
			}
			catch (Exception ex) {
				throw new FormatException(
					"Failed to parse the IP address part of the location. Please make sure you specify a valid IP address. Use 0.0.0.0 or [::] to listen on all interfaces.",
					ex);
			}
		}
	}
}