using System;
using System.Collections.Generic;
using System.Reflection;
using Duality;
using Duality.Helpers;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin
{
	[Serializable]
	public class ScriptComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpCollisionListener, ICmpHandlesMessages 
	{
		[NonSerialized]
		private DualityScript _scriptInstance;
		[NonSerialized]
		private Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

		private Dictionary<string, object> _scriptPropertyValues = new Dictionary<string, object>();

		public ContentRef<ScriptResourceBase> Script { get; set; }

		public Dictionary<string, object> ScriptPropertyValues
		{
			get { return _scriptPropertyValues; }
			set { _scriptPropertyValues = value; }
		}

		public void OnInit(InitContext context)
		{
			if (context != InitContext.Activate)
				return;

			if (Script.Res == null)
			{
				if(DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
					Log.Game.WriteError("The script attached to '{0}' is null.", GameObj);

				return;
			}

			Script.Res.Reloaded += OnScriptReloaded;
			InstantiateScript();

			if (_scriptInstance == null)
				return;

			SetScriptPropertyValues();
			SafeExecute(_scriptInstance.Init, "Init");
		}

		public void OnShutdown(ShutdownContext context)
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.Shutdown, "Shutdown");
		}

		public void OnUpdate()
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.Update, "Update");
		}

		public void OnCollisionBegin(Component sender, CollisionEventArgs args)
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.CollisionBegin, "CollisionBegin", args);
		}

		public void OnCollisionEnd(Component sender, CollisionEventArgs args)
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.CollisionEnd, "CollisionEnd", args);
		}

		public void OnCollisionSolve(Component sender, CollisionEventArgs args)
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.CollisionSolve, "CollisionSolve", args);
		}

		public void OnGameObjectParentChanged(GameObject oldParent, GameObject newParent)
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.GameObjectParentChanged, "GameObjectParentChanged", oldParent, newParent);
		}

		public void HandleMessage(GameObject sender, GameMessage msg)
		{
			if (_scriptInstance == null)
				return;

			SafeExecute(_scriptInstance.HandleMessage, "HandleMessage", sender, msg);
		}

		public void CallMethod(string methodName)
		{
			if (_scriptInstance == null)
				return;

			var methodInfo = _methodCache.ContainsKey(methodName) ? _methodCache[methodName] : _scriptInstance.GetType().GetMethod(methodName);
			if (methodInfo == null)
			{
				Log.Game.WriteWarning("{0}: Couldn't find method '{1}'", GetType(), methodName);
				return;
			}

			if (_methodCache.ContainsKey(methodName) == false)
				_methodCache.Add(methodName, methodInfo);

			try
			{
				methodInfo.Invoke(_scriptInstance, null);
			}
			catch (Exception e)
			{
				Log.Game.WriteError("An error occurred while executing script method '{0}' on '{1}':{2}", methodName, Script.Name, e.Message);
			}
		}

		public void SetScriptPropertyValue(string propertyName, object value)
		{
			if (_scriptPropertyValues.ContainsKey(propertyName))
				_scriptPropertyValues[propertyName] = value;
			else
				_scriptPropertyValues.Add(propertyName, value);
		}

		public object GetScriptPropertyValue(string propertyName)
		{
			return _scriptPropertyValues.ContainsKey(propertyName) ? _scriptPropertyValues[propertyName] : new object();
		}

		public void ClearScriptPropertyValues()
		{
			_scriptPropertyValues.Clear();
		}

		private void InstantiateScript()
		{
			if (Script.Res == null)
			{
				Log.Editor.WriteWarning("Attempting to instanciate Script but resource is null {0}", Script);
				return;
			}
			_scriptInstance = Script.Res.Instantiate();
			if (_scriptInstance == null)
				return;

			_scriptInstance.GameObj = GameObj;
		}

		private void OnScriptReloaded(object sender, EventArgs eventArgs)
		{
			InstantiateScript();
			if (_scriptInstance == null)
				return;
			SetScriptPropertyValues();
		}

		private void SafeExecute(Action action, string methodName)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				Log.Game.WriteError("An error occurred while executing script {0} on '{1}':{2}", methodName, Script.Name, e.Message);
			}
		}

		private void SafeExecute<T>(Action<T> action, string methodName, T args)
		{
			try
			{
				action(args);
			}
			catch (Exception e)
			{
				Log.Game.WriteError("An error occurred while executing script {0} on '{1}':{2}", methodName, Script.Name, e.Message);
			}
		}

		private void SafeExecute<T1, T2>(Action<T1, T2> action, string methodName, T1 argOne, T2 argTwo)
		{
			try
			{
				action(argOne, argTwo);
			}
			catch (Exception e)
			{
				Log.Game.WriteError("An error occurred while executing script {0} on '{1}':{2}", methodName, Script.Name, e.Message);
			}
		}

		private void SetScriptPropertyValues()
		{
			Guard.NotNull(_scriptInstance);
			Guard.NotNull(_scriptPropertyValues);

			var removalList = new List<string>();
			foreach (var propertyValue in _scriptPropertyValues)
			{
				var property = _scriptInstance.GetType().GetProperty(propertyValue.Key);

				if (property == null)
				{
					Log.Editor.WriteWarning("{0}: Property name {1} not found in script {2}. Removing property value.", GetType().Name,
						propertyValue.Key, Script.Name);
					removalList.Add(propertyValue.Key);
					continue;
				}

				property.SetValue(_scriptInstance, propertyValue.Value);
			}

			foreach (var key in removalList)
			{
				_scriptPropertyValues.Remove(key);
			}
		}
	}
}
