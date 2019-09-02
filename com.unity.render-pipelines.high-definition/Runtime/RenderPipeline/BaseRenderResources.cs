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
        new BaseRenderResources target => (BaseRenderResources)base.target;

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
                    // Force reload all resources
                    ResourceReloader.ClearReloadableFields(target);
                    ResourceReloader.ReloadAllNullIn(target);
                }
            }
        }
    }
#endif
}
