using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace WSSharp
{
	public class Proxy<T> : RealProxy
	{
		private readonly Func<MethodInfo, object[], object> onInvocation;

		private Proxy(Func<MethodInfo, object[], object> onInvocation)
			: base(typeof(T))
		{
			this.onInvocation = onInvocation;
		}

		public static T Create(Func<MethodInfo, object[], object> onInvocation)
		{
			return (T)new Proxy<T>(onInvocation).GetTransparentProxy();
		}

		public override IMessage Invoke(IMessage msg)
		{
			var methodCall = (IMethodCallMessage)msg;
			var method = (MethodInfo)methodCall.MethodBase;

			try
			{
				var result = onInvocation(method, methodCall.Args);
				return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
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
