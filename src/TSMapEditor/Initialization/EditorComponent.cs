namespace TSMapEditor.Initialization
{
    public class EditorComponent
    {
        public EditorComponent(IEditorComponentManager componentManager)
        {
            EditorComponentManager = componentManager;
        }

        protected IEditorComponentManager EditorComponentManager { get; }
    }
}
