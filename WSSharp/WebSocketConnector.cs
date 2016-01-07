using System;
using System.Net;
using System.Net.Sockets;

namespace WSSharp
{
	public class WebSocketConnector<TIncoming, TOutcoming>
		where TIncoming : class
		where TOutcoming : class
	{
		protected readonly TIncoming Handler;
		protected readonly IPAddress IPAddress;
		protected readonly int Port;
		protected readonly IPEndPoint Endpoint;
		protected readonly WebSocket Socket;

		protected WebSocketConnector(string location, TIncoming handler)
		{
			Handler = handler;
			var uri = new Uri(location);
			IPAddress = ParseIPAddress(uri);
			Port = uri.Port;
			Endpoint = new IPEndPoint(IPAddress, Port);
			Socket = new WebSocket(new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.IP));
		}

		protected void CreateConnection(WebSocket socket, ConnectionDelegate<TIncoming, TOutcoming> onConnection)
		{
			var connection = new WebSocketConnection<TIncoming, TOutcoming>(socket, Handler);
			connection.StartReceiving();
			onConnection?.Invoke(connection);
		}

		private static IPAddress ParseIPAddress(Uri uri)
		{
			var ipStr = uri.Host;

			if (ipStr == "0.0.0.0" || ipStr == "[0000:0000:0000:0000:0000:0000:0000:0000]")
			{
				return IPAddress.IPv6Any;
			}
			try
			{
				return IPAddress.Parse(ipStr);
			}
			catch (Exception ex)
			{
				throw new FormatException(
					"Failed to parse the IP address part of the location. Please make sure you specify a valid IP address. Use 0.0.0.0 or [::] to listen on all interfaces.",
					ex);
			}
		}
	}
	
	public delegate void ConnectionDelegate<TIncoming, TOutcoming>
		(WebSocketConnection<TIncoming, TOutcoming> connection)
		where TIncoming : class
		where TOutcoming : class;
}