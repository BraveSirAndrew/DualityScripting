using System;
using Duality;
using Duality.Helpers;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin
{
	[Serializable]
	public class ScriptComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpCollisionListener, ICmpComponentListener, ICmpGameObjectListener, ICmpHandlesMessages 
	{
		[NonSerialized]
		private DualityScript _scriptInstance;

		public ContentRef<ScriptResource> Script { get; set; }

		public void OnInit(InitContext context)
		{
			if (context != InitContext.Activate)
				return;

			if (Script == null)
				return;

			Script.Res.Reloaded += OnScriptReloaded;
			InstantiateScript();

			if (_scriptInstance == null)
				return;

			_scriptInstance.Init();
		}

		public void OnShutdown(ShutdownContext context)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.Shutdown();
		}

		public void OnUpdate()
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.Update();
		}

		public void OnCollisionBegin(Component sender, CollisionEventArgs args)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.CollisionBegin(args);
		}

		public void OnCollisionEnd(Component sender, CollisionEventArgs args)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.CollisionEnd(args);
		}

		public void OnCollisionSolve(Component sender, CollisionEventArgs args)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.CollisionSolve(args);
		}

		public void OnComponentAdded(Component comp)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.ComponentAdded(comp);
		}

		public void OnComponentRemoving(Component comp)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.ComponentRemoving(comp);
		}

		public void OnGameObjectParentChanged(GameObject oldParent, GameObject newParent)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.GameObjectParentChanged(oldParent, newParent);
		}

		public void HandleMessage(GameObject sender, GameMessage msg)
		{
			if (_scriptInstance == null)
				return;

			_scriptInstance.HandleMessage(msg);
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
	}
}
