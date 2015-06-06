using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Duality;
using Duality.Editor;
using Duality.Editor.Forms;
using ScriptingPlugin.Resources;

namespace ScriptingPlugin.Editor
{
	public class ScriptingEditorPlugin : EditorPlugin
	{
		private bool _debuggerAttachedLastFrame;
		
		private static ScriptsSolutionEditor _scriptsSolutionEditor;
		private static string _cSharpProjectPath;
		private static string _fSharpProjectPath;
		
		private ISourceFilePathGenerator _sourceFilePathGenerator;
		private ScriptResourceEvents _scriptResourceEvents;
		private IScriptMetadataService _scriptMetadataService;
		
		public const string PathPartCsharp = "CSharp";
		public const string PathPartFsharp = "FSharp";
		
		public override string Id
		{
			get { return "ScriptingEditorPlugin"; }
		}

		public static string CSharpProjectPath
		{
			get { return _cSharpProjectPath; }
		}

		public static string FSharpProjectPath
		{
			get { return _fSharpProjectPath; }
		}

		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);
			var fileSystem = new FileSystem();
			_scriptsSolutionEditor = _scriptsSolutionEditor ?? new ScriptsSolutionEditor(fileSystem, EditorHelper.SourceCodeDirectory);
			_scriptMetadataService = new ScriptMetadataService(fileSystem);

			ScriptReloader.ReloadOutOfDateScripts(fileSystem, _scriptMetadataService);
			AddScriptProjectsToSolution();

			_sourceFilePathGenerator = new SourceFilePathGenerator();
			_scriptResourceEvents = _scriptResourceEvents ?? new ScriptResourceEvents(fileSystem, _sourceFilePathGenerator);
			_scriptResourceEvents.AddDefaultScriptTemplate<CSharpScript>(new CSharpScriptTemplate{ProjectPath = _cSharpProjectPath});
			_scriptResourceEvents.AddDefaultScriptTemplate<FSharpScript>(new FSharpScriptTemplate{ProjectPath = _fSharpProjectPath});

			FileEventManager.ResourceCreated += _scriptResourceEvents.OnResourceCreated;
			FileEventManager.ResourceRenamed += _scriptResourceEvents.OnResourceRenamed;
			FileEventManager.ResourceDeleting += _scriptResourceEvents.OnResourceDeleting;

			PrecompileScripts<CSharpScript>(CSharpScript.FileExt);
			PrecompileScripts<FSharpScript>(FSharpScript.FileExt);
		}

		public static void AddScriptProjectsToSolution()
		{
			_cSharpProjectPath = _scriptsSolutionEditor.AddToSolution(PathPartCsharp, "Scripts.csproj", Resources.Resources.ScriptsProjectTemplate);
			_fSharpProjectPath = _scriptsSolutionEditor.AddToSolution(PathPartFsharp, "FSharpScripts.fsproj", Resources.Resources.FSharpProjectTemplate);
		}

		private static void PrecompileScripts<T>(string scriptFileExtension) where T : ScriptResourceBase
		{
			Task.Factory.StartNew(() =>
			{
				Log.Editor.Write("Precompiling {0} scripts...", scriptFileExtension);
				var sw = Stopwatch.StartNew();

				var resources = Resource.GetResourceFiles();
				var scripts = resources.Where(r => r.EndsWith(scriptFileExtension));
				Parallel.ForEach(scripts,
					new ParallelOptions { MaxDegreeOfParallelism = 3 },
					scriptRes =>
					{
						var script = ContentProvider.RequestContent<T>(scriptRes);
						script.Res.Instantiate();
					});

				sw.Stop();
				Log.Editor.Write("Precompiled {0} scripts in {1} seconds.", scriptFileExtension, sw.Elapsed.TotalSeconds);
			});
		}

		private void DualityEditorAppOnIdling(object sender, EventArgs eventArgs)
		{
//			if (Debugger.IsAttached && _debuggerAttachedLastFrame == false)
//			{
//				Log.Editor.Write("Reloading scripts with debug information...");
//				
//				ScriptingPluginCorePlugin.FSharpScriptCompiler.SetPdbEditor(new PdbEditor());
//
//				var fsharpScripts = Resource.GetResourceFiles().Where(r => r.EndsWith(FSharpScript.FileExt));
//				foreach (var scriptPath in fsharpScripts)
//				{
//					var script = ContentProvider.RequestContent<FSharpScript>(scriptPath);
//					script.Res.Reload();
//				}
//
//				_debuggerAttachedLastFrame = true;
//			}
//			else if (Debugger.IsAttached == false && _debuggerAttachedLastFrame)
//			{
//				ScriptingPluginCorePlugin.FSharpScriptCompiler.SetPdbEditor(new NullPdbEditor());
//
//				_debuggerAttachedLastFrame = false;
//			}
		}
	}
}
