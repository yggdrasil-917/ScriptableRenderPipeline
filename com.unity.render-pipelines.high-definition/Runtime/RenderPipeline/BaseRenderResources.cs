using System.Reflection;
using System.Linq;

namespace UnityEngine.Rendering.HighDefinition
{
    public abstract class BaseRenderResources : ScriptableObject
    {
        public abstract int Id { get; }
    }

    public abstract class BaseEditorRenderResources : BaseRenderResources
    {
#if UNITY_EDITOR
        protected virtual void OnEnable()
        {
            // Forcing this flag on editor resource assets will cause a player build error if an editor resource
            // would be leaked into a standalone build
            if (!hideFlags.HasFlag(HideFlags.DontSaveInBuild))
                hideFlags |= HideFlags.DontSaveInBuild;
        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(BaseRenderResources), true)]
    class BaseRenderResourcesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Add a "Reload All" button in inspector when we are in developer's mode
            // .. currently only for 'builtin' types to mimic existing behaviour.
            //
            // TODO: Why not just always show this reload button? For assets in read-only packages (like most cases
            //       of SRP usage) this button will be disabled anyway.
            //
            var isDevMode = UnityEditor.EditorPrefs.GetBool("DeveloperMode");
            var isBuiltinType = target is HDRenderPipelineResources || target is HDRenderPipelineRayTracingResources || target is HDRenderPipelineEditorResources;

            if (isDevMode || !isBuiltinType)
            {
                if (GUILayout.Button("Reload All"))
                {
                    // Clear out all ReloadGroup fields
                    foreach (var fieldInfo in targets.GetType().GetFields().Where(fi => fi.IsDefined(typeof(ReloadGroupAttribute))))
                        fieldInfo.SetValue(target, null);

                    // Find appropriate base path from resource asset
                    var script = UnityEditor.MonoScript.FromScriptableObject(target as ScriptableObject);
                    var scriptPath = UnityEditor.AssetDatabase.GetAssetPath(script);
                    var packageRoot = string.Empty;
                    if (scriptPath.StartsWith("Assets/"))
                        packageRoot = "Assets/";
                    else if (scriptPath.StartsWith("Packages/"))
                        packageRoot = scriptPath.Substring(0, scriptPath.IndexOf("/", "Packages/".Length));
                    Debug.Log($"scriptPath: {scriptPath} packageRoot: {packageRoot}");

                    // Reload all
                    ResourceReloader.ReloadAllNullIn(target, packageRoot);
                }
            }
        }
    }
#endif
}
