using System;
using Duality;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin
{
	[Serializable]
	public class ScriptComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpCollisionListener, ICmpHandlesMessages 
	{
		[NonSerialized]
		private DualityScript _scriptInstance;

		public ContentRef<ScriptResource> Script { get; set; }

		public void OnInit(InitContext context)
		{
			if (context != InitContext.Activate)
				return;

			if (Script.Res == null)
			{
				Log.Game.WriteError("The script attached to '{0}' is null.");
				return;
			}

			Script.Res.Reloaded += OnScriptReloaded;
			InstantiateScript();

			if (_scriptInstance == null)
				return;

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

		private void InstantiateScript()
		{
			_scriptInstance = Script.Res.Instantiate();
			if (_scriptInstance == null)
				return;

			_scriptInstance.GameObj = GameObj;
		}

		private void OnScriptReloaded(object sender, EventArgs eventArgs)
		{
			InstantiateScript();
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
	}
}
