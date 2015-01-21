using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duality;
using Flow;

namespace ScriptingPlugin
{
	internal static class CoroutineHelper
	{
		private static MethodInfo _newCoroutineMethod;

		public static ICoroutine<T> CreateCoroutine<T>(string coroutineName, MethodInfo methodInfo, IFactory factory, object scriptInstance)
		{
			// verify the method signature
			if (!ValidateParameters(coroutineName, methodInfo))
				return null;

			var expectedReturnType = typeof(IEnumerator<>).MakeGenericType(typeof(T));
			if (!ValidateReturnType(coroutineName, methodInfo, expectedReturnType))
				return null;

			var coroutineType = typeof(Flow.Func<,>).MakeGenericType(typeof(IGenerator), expectedReturnType);
			var coroutine = Delegate.CreateDelegate(coroutineType, scriptInstance, coroutineName);

			if (_newCoroutineMethod == null)
				MakeNewCoroutineMethod<T>(factory);

			if (_newCoroutineMethod == null)
			{
				Log.Game.WriteError("Could not create the NewCoroutine method", Log.CurrentMethod());
				return null;
			}

			return (ICoroutine<T>)_newCoroutineMethod.Invoke(factory, new[] { coroutine });
		}

		private static void MakeNewCoroutineMethod<T>(IFactory factory)
		{
			var newCoroutineGenericMethod = factory.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(m => m.Name.Contains("NewCoroutine"))
				.FirstOrDefault(m => m.GetParameters().Length == 1);

			if(newCoroutineGenericMethod == null)
				throw new InvalidOperationException("Couldn't find the NewCoroutine method that takes one parameter");

			_newCoroutineMethod = newCoroutineGenericMethod.MakeGenericMethod(typeof (T));
		}

		private static bool ValidateReturnType(string coroutineName, MethodInfo methodInfo, Type expectedReturnType)
		{
			if (methodInfo.ReturnType != expectedReturnType)
			{
				Log.Game.WriteError("The return type of '{0}' must be of type '{1}'", coroutineName, expectedReturnType.Name);
				return false;
			}
			return true;
		}

		private static bool ValidateParameters(string coroutineName, MethodInfo methodInfo)
		{
			var parameters = methodInfo.GetParameters();
			if (parameters.Length != 1)
			{
				Log.Game.WriteError("Expected a method with one parameter, but '{0}' has '{1}'", coroutineName, parameters.Length);
				return false;
			}

			if (!typeof(IGenerator).IsAssignableFrom(parameters[0].ParameterType))
			{
				Log.Game.WriteError("The parameter '{0}' in '{1}' must be of type IGenerator or derived type", parameters[0].Name,
					coroutineName);
				return false;
			}
			return true;
		}
	}
}