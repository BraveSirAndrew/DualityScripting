using System;
using System.Diagnostics;
using System.IO.Abstractions;
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
		internal static string CSharpProjectPath;
		internal static string FSharpProjectPath;
		private ScriptResourceEvents _scriptResourceEvents;
		
		public const string PathPartCsharp = "CSharp";
		public const string PathPartFsharp = "FSharp";
		
		public override string Id
		{
			get { return "ScriptingEditorPlugin"; }
		}
		
		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);
			_scriptsSolutionEditor = _scriptsSolutionEditor ?? new ScriptsSolutionEditor(new FileSystem(), EditorHelper.SourceCodeDirectory);

			ScriptReloader.ReloadOutOfDateScripts(new FileSystem());
			CSharpProjectPath = _scriptsSolutionEditor.AddToSolution(PathPartCsharp, "Scripts.csproj", Resources.Resources.ScriptsProjectTemplate);
			FSharpProjectPath = _scriptsSolutionEditor.AddToSolution(PathPartFsharp, "FSharpScripts.fsproj", Resources.Resources.FSharpProjectTemplate);
			_scriptResourceEvents = _scriptResourceEvents ?? new ScriptResourceEvents(new FileSystem(), new ProjectConstants()
			{
				CSharpProjectPath = CSharpProjectPath,
				CSharpScriptExtension = ScriptingPluginCorePlugin.CSharpScriptExtension,
				FSharpProjectPath = FSharpProjectPath,
				FSharpScriptExtension = ScriptingPluginCorePlugin.FSharpScriptExtension
			});

			FileEventManager.ResourceCreated += _scriptResourceEvents.OnResourceCreated;
			FileEventManager.ResourceRenamed += _scriptResourceEvents.OnResourceRenamed;
			DualityEditorApp.EditorIdling += DualityEditorAppOnIdling;
		}

		private void DualityEditorAppOnIdling(object sender, EventArgs eventArgs)
		{
			if (Debugger.IsAttached && _debuggerAttachedLastFrame == false)
			{
				Log.Editor.Write("Reloading scripts with debug information...");
				Log.Editor.PushIndent();

				var sw = Stopwatch.StartNew();

				ScriptingPluginCorePlugin.FSharpScriptCompiler.SetPdbEditor(new PdbEditor());

				foreach (var script in ContentProvider.GetAvailableContent<FSharpScript>())
				{
					script.Res.Reload();
				}

				_debuggerAttachedLastFrame = true;
				
				sw.Stop();
				Log.Editor.PopIndent();
				Log.Editor.Write("Reloading scripts took {0} ms", sw.ElapsedMilliseconds);
			}
			else if (Debugger.IsAttached == false && _debuggerAttachedLastFrame)
			{
				ScriptingPluginCorePlugin.FSharpScriptCompiler.SetPdbEditor(new NullPdbEditor());

				_debuggerAttachedLastFrame = false;
			}
		}
	}
}
