using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace WSSharp
{
	public class Proxy<T> : RealProxy
	{
		private readonly Action<MethodInfo> onInvocation;

		private Proxy(Action<MethodInfo> onInvocation)
			: base(typeof(T))
		{
			this.onInvocation = onInvocation;
		}

		public static T Create(Action<MethodInfo> onInvocation)
		{
			return (T)new Proxy<T>(onInvocation).GetTransparentProxy();
		}

		public override IMessage Invoke(IMessage msg)
		{
			var methodCall = (IMethodCallMessage)msg;
			var method = (MethodInfo)methodCall.MethodBase;

			try
			{
				onInvocation(method);
				return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);
			}
			catch (Exception e)
			{
				if (e is TargetInvocationException && e.InnerException != null)
				{
					return new ReturnMessage(e.InnerException, (IMethodCallMessage) msg);
				}

				return new ReturnMessage(e, (IMethodCallMessage) msg);
			}
		}
	}
}
