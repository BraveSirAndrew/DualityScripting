using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdamsLair.WinForms.PropertyEditing;
using Duality;
using Duality.Editor;
using Duality.Editor.Plugins.Base.PropertyEditors;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor.PropertyEditors
{
	[PropertyEditorAssignment(typeof(ScriptComponentPropertyEditor), "MatchToProperty")]
	public class ScriptComponentPropertyEditor : ComponentPropertyEditor
	{
		private readonly List<PropertyEditor> _propertyEditors = new List<PropertyEditor>();
		private bool _isInvokingDirectChild;
 
		protected override bool IsAutoCreateMember(MemberInfo info)
		{
			return false;
		}

		protected override void OnUpdateFromObjects(object[] values)
		{
			base.OnUpdateFromObjects(values);

			UpdateScriptPropertyEditors(values.Cast<ScriptComponent>());
		}

		protected override void BeforeAutoCreateEditors()
		{
			base.BeforeAutoCreateEditors();

			var scriptEditor = ParentGrid.CreateEditor(typeof (ContentRef<ScriptResourceBase>), this);
			if (scriptEditor != null)
			{
				scriptEditor.BeginUpdate();
				scriptEditor.PropertyName = "Script";
				scriptEditor.Getter = () => GetValue().OfType<ScriptComponent>().Select(o => (object)o.Script);
				scriptEditor.Setter = ScriptSetter;
				scriptEditor.ValueChanged += OnScriptChanged;
				AddPropertyEditor(scriptEditor);
				scriptEditor.EndUpdate();
			}

			var scriptComponents = GetValue().Cast<ScriptComponent>();
			UpdateScriptPropertyEditors(scriptComponents);
		}

		protected override void OnValueChanged(object sender, PropertyEditorValueEventArgs args)
		{
			if (_isInvokingDirectChild) return;

			// Find the direct descendant editor on the path to the changed one
			var directChild = args.Editor;
			while (directChild != null && !HasPropertyEditor(directChild))
				directChild = directChild.ParentEditor;
			
			if (directChild == args.Editor) 
				return;

			if (directChild != null && directChild != args.Editor && directChild.EditedMember != null)
			{
				try
				{
					_isInvokingDirectChild = true;
					if (directChild.EditedMember is PropertyInfo || directChild.EditedMember is FieldInfo)
						NotifyScriptComponentsChanged();
				}
				finally
				{
					_isInvokingDirectChild = false;
				}
			}

			UpdatePrefabModifiedState(directChild);
		}

		private void NotifyScriptComponentsChanged()
		{
			var scriptComponents = GetValue().Cast<ScriptComponent>();

			foreach (var scriptComponent in scriptComponents)
				DualityEditorApp.NotifyObjPropChanged(scriptComponent, new ObjectSelection(scriptComponent),
					typeof (ScriptComponent).GetProperty("ScriptPropertyValues"));
		}

		private void OnScriptChanged(object sender, PropertyEditorValueEventArgs propertyEditorValueEventArgs)
		{
			UpdateScriptPropertyEditors(GetValue().Cast<ScriptComponent>());
		}

		private void UpdateScriptPropertyEditors(IEnumerable<ScriptComponent> scriptComponents)
		{
			RemoveScriptPropertyEditors();

			foreach (var scriptComponent in scriptComponents)
			{
				if (scriptComponent == null || scriptComponent.Script.Res == null)
					continue;

				var scriptInstance = scriptComponent.Script.Res.Instantiate();

				if (scriptInstance == null)
					continue;

				var properties = scriptInstance
					.GetType()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				
				if(properties.Any() == false)
					continue;

				foreach (var propertyInfo in properties)
				{
					if (propertyInfo == null)
						continue;

					var info = propertyInfo;

					var propertyEditor = ParentGrid.CreateEditor(propertyInfo.PropertyType, this);
					propertyEditor.BeginUpdate();
					propertyEditor.PropertyName = propertyInfo.Name;
					propertyEditor.EditedMember = propertyInfo;
					propertyEditor.Getter = () => GetValue().Cast<ScriptComponent>().Select(o => o.GetScriptPropertyValue(propertyInfo.Name));
					propertyEditor.Setter = info.CanWrite ? values => ScriptPropertyValuesSetter(info.Name, values) : (Action<IEnumerable<object>>) null;
					_propertyEditors.Add(propertyEditor);
					ParentGrid.ConfigureEditor(propertyEditor);
					AddPropertyEditor(propertyEditor);
					propertyEditor.EndUpdate();
				}
			}
		}

		private void RemoveScriptPropertyEditors()
		{
			foreach (var propertyEditor in _propertyEditors)
			{
				RemovePropertyEditor(propertyEditor);
			}
			_propertyEditors.Clear();
		}

		private void ScriptSetter(IEnumerable<object> values)
		{
			var scriptComponents = GetValue().Cast<ScriptComponent>();
			foreach (var scriptComponent in scriptComponents)
			{
				scriptComponent.ClearScriptPropertyValues();
			}

			MemberPropertySetter(typeof (ScriptComponent).GetProperty("Script"), GetValue(), values);
		}

		private void ScriptPropertyValuesSetter(string name, IEnumerable<object> values)
		{
			var scriptComponents = GetValue().Cast<ScriptComponent>();

			foreach (var scriptComponent in scriptComponents)
			{
				scriptComponent.SetScriptPropertyValue(name, values.FirstOrDefault());
				DualityEditorApp.NotifyObjPropChanged(scriptComponent, new ObjectSelection(scriptComponent), typeof(ScriptComponent).GetProperty("ScriptPropertyValues"));
			}
		}

		public static int MatchToProperty(Type propertyType, ProviderContext context)
		{
			if (typeof(ScriptComponent).IsAssignableFrom(propertyType) && context.ParentEditor is GameObjectOverviewPropertyEditor)
				return PropertyEditorAssignmentAttribute.PrioritySpecialized;

			return PropertyEditorAssignmentAttribute.PriorityNone;
		}
	}
}