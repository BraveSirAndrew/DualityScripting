using Duality;

namespace ScriptingPlugin
{
	/// <summary>
	/// This is the base class for all scripts. The scripting system will try to instantiate a type that inherits from
	/// DualityScript when given a script to compile.
	/// </summary>
	public class DualityScript
	{
		public GameObject GameObj { get; set; }

		public virtual void Update()
		{
			
		}

		public virtual void EditorUpdate()
		{
			
		}

		public virtual void CollisionBegin(CollisionEventArgs args)
		{
			
		}

		public virtual void CollisionEnd(CollisionEventArgs args)
		{
			
		}

		public virtual void CollisionSolve(CollisionEventArgs args)
		{
			
		}

		public virtual void ComponentAdded(Component comp)
		{
			
		}

		public virtual void ComponentRemoving(Component comp)
		{
			
		}

		public virtual void GameObjectParentChanged(GameObject oldParent, GameObject newParent)
		{
			
		}

		public virtual void HandleMessage(GameObject sender, GameMessage msg)
		{
			
		}

		public virtual void Init()
		{
			
		}

		public virtual void Saving()
		{
			
		}

		public virtual void Shutdown()
		{
			
		}
	}
}