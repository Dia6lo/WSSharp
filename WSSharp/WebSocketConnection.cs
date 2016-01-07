using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProtoBuf;

namespace WSSharp
{
	public class WebSocketConnection<TIncoming, TOutcoming>
		where TIncoming: class 
		where TOutcoming: class
	{
		private WebSocket socket;
		private TIncoming handler;
		private List<MethodInfo> incomingMethods;
		private List<MethodInfo> outcomingMethods;
		public TOutcoming Outcoming;

		public WebSocketConnection(WebSocket socket, TIncoming handler)
		{
			this.handler = handler;
			this.socket = socket;
			incomingMethods = typeof(TIncoming).GetMethods()
					  .Where(m => m.GetCustomAttributes(typeof(MethodContractAttribute)).Any())
					  .ToList();
			outcomingMethods = typeof(TOutcoming).GetMethods()
					  .Where(m => m.GetCustomAttributes(typeof(MethodContractAttribute)).Any())
					  .ToList();
			if (incomingMethods.Count > 256)
			{
				throw new OutOfMemoryException("You can create not more than 256 MethodContract-attributed methods.");
			}
			Outcoming = Proxy<TOutcoming>.Create(Invoke);
		}

		public bool IsAvailable => socket.Connected;

		private object Invoke(MethodInfo method, object[] args)
		{
			var argTypes = method.GetParameters().Select(t => t.ParameterType).ToArray();
			var argPairs = args.Select((obj, i) => new {Obj = obj, Type = argTypes[i]});
			socket.Stream.WriteByte((byte)outcomingMethods.IndexOf(method));
			foreach (var arg in argPairs)
			{
				Serializer.SerializeWithLengthPrefix(socket.Stream, Convert.ChangeType(arg.Obj, arg.Type), PrefixStyle.Base128);
			}
			return null;
		}

		public void StartReceiving()
		{
			Read();
		}

		private async void Read()
		{
			try
			{
				while (true)
				{
					if (!IsAvailable)
						return;
					var methodIndex = await Task.Factory.StartNew(socket.Stream.ReadByte);
					var method = incomingMethods[methodIndex];
					var argTypes = method.GetParameters().Select(t => t.ParameterType).ToArray();
					var args = new List<object>();
					foreach (var argType in argTypes)
					{
						var deserializer = typeof(Serializer)
							.GetMethod("DeserializeWithLengthPrefix", new []{ typeof(Stream), typeof(PrefixStyle)})
							.MakeGenericMethod(argType);
						args.Add(deserializer.Invoke(null, new object[] {socket.Stream, PrefixStyle.Base128}));
					}
					method.Invoke(handler, args.ToArray());
				}
			}
			finally
			{
				socket.Dispose();
			}
		}
	}
}
