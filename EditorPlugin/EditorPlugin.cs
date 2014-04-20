using System;
using System.IO;
using Duality;
using Duality.Editor;
using Duality.Editor.Forms;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptingEditorPlugin : EditorPlugin
	{
		private bool _debuggerAttachedLastFrame;
		
		private ScriptsSolutionEditor _scriptsSolutionEditor;
		internal static string ScriptsProjectPath;
		private ScriptResourceEvents _scriptResourceEvents;

		public const string Scripts = "Scripts";

		public override string Id
		{
			get { return "ScriptingEditorPlugin"; }
		}
		
		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);
			_scriptsSolutionEditor = _scriptsSolutionEditor ?? new ScriptsSolutionEditor(EditorHelper.SourceCodeDirectory);
			_scriptResourceEvents = _scriptResourceEvents ?? new ScriptResourceEvents();

			ReloadOutOfDateScripts();

			FileEventManager.ResourceCreated += _scriptResourceEvents.OnResourceCreated;
			FileEventManager.ResourceRenamed += _scriptResourceEvents.OnResourceRenamed;
			ScriptsProjectPath = Path.Combine(EditorHelper.SourceCodeDirectory, Scripts, Scripts + ".csproj");
			_scriptsSolutionEditor.ModifySolution(Scripts);

			DualityEditorApp.EditorIdling += DualityEditorAppOnIdling;
			
		}

		private void DualityEditorAppOnIdling(object sender, EventArgs eventArgs)
		{
			if (System.Diagnostics.Debugger.IsAttached && _debuggerAttachedLastFrame == false)
			{
				foreach (var script in ContentProvider.GetAvailableContent<ScriptResource>())
				{
					script.Res.Reload();
				}

				foreach (var script in ContentProvider.GetAvailableContent<FSharpScript>())
				{
					script.Res.Reload();
				}

				_debuggerAttachedLastFrame = true;
			}
			else if (System.Diagnostics.Debugger.IsAttached == false && _debuggerAttachedLastFrame)
			{
				_debuggerAttachedLastFrame = false;
			}
		}

		private void ReloadOutOfDateScripts()
		{
			foreach (var script in ContentProvider.GetAvailableContent<ScriptResource>())
			{
				ReloadScript(script);
			}
			foreach (var script in ContentProvider.GetAvailableContent<FSharpScript>())
			{
				ReloadScript(script);
			}
		}

		private static void ReloadScript<T>(ContentRef<T> script) where T : ScriptResourceBase
		{
			var metafilePath = Path.GetFullPath(script.Res.GetMetafilePath());

			if (string.IsNullOrEmpty(metafilePath))
				return;

			if (string.IsNullOrEmpty(script.Res.SourcePath))
				return;

			if (File.Exists(metafilePath) && File.GetLastWriteTime(script.Res.SourcePath) > File.GetLastWriteTime(metafilePath))
			{
				script.Res.Script = File.ReadAllText(script.Res.SourcePath);
				script.Res.Reload();
				DualityEditorApp.NotifyObjPropChanged(null, new ObjectSelection(script.Res));
			}
		}
	}
}
