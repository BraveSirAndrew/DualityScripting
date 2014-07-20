using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
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
		private IScriptMetadataService _scriptMetadataService;
		
		public const string PathPartCsharp = "CSharp";
		public const string PathPartFsharp = "FSharp";
		
		public override string Id
		{
			get { return "ScriptingEditorPlugin"; }
		}
		
		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);
			var fileSystem = new FileSystem();
			_scriptsSolutionEditor = _scriptsSolutionEditor ?? new ScriptsSolutionEditor(fileSystem, EditorHelper.SourceCodeDirectory);
			_scriptMetadataService = new ScriptMetadataService(fileSystem);

			ScriptReloader.ReloadOutOfDateScripts(fileSystem, _scriptMetadataService);
			CSharpProjectPath = _scriptsSolutionEditor.AddToSolution(PathPartCsharp, "Scripts.csproj", Resources.Resources.ScriptsProjectTemplate);
			FSharpProjectPath = _scriptsSolutionEditor.AddToSolution(PathPartFsharp, "FSharpScripts.fsproj", Resources.Resources.FSharpProjectTemplate);
			
			_scriptResourceEvents = _scriptResourceEvents ?? new ScriptResourceEvents(fileSystem, new SourceFilePathGenerator());
			_scriptResourceEvents.AddDefaultScriptTemplate<CSharpScript>(new CSharpScriptTemplate{ProjectPath = CSharpProjectPath});
			_scriptResourceEvents.AddDefaultScriptTemplate<FSharpScript>(new FSharpScriptTemplate{ProjectPath = FSharpProjectPath});

			FileEventManager.ResourceCreated += _scriptResourceEvents.OnResourceCreated;
			FileEventManager.ResourceRenamed += _scriptResourceEvents.OnResourceRenamed;
			FileEventManager.ResourceDeleting += _scriptResourceEvents.OnResourceDeleting;
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

				var fsharpScripts = Resource.GetResourceFiles().Where(r => r.EndsWith(FSharpScript.FileExt));
				foreach (var scriptPath in fsharpScripts)
				{
					var script = ContentProvider.RequestContent<FSharpScript>(scriptPath);
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
