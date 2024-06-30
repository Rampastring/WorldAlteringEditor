using System;
using System.Collections.Generic;
using System.Linq;

namespace TSMapEditor.Initialization
{
    public interface IEditorComponentManager
    {
        void ClearSessionComponents();
        T Get<T>();
        T GetSingleInstanceComponent<T>();
        void RegisterPermanentComponent<T>(T component);
        void RegisterSessionComponent<T>(T component);
        void UnregisterComponent<T>();
    }

    public class EditorComponentManager : IEditorComponentManager
    {
        private Dictionary<Type, bool> componentPermanenceMap = new Dictionary<Type, bool>();
        private Dictionary<Type, object> singleInstanceComponentMap = new Dictionary<Type, object>();

        public void RegisterSessionComponent<T>(T component) => RegisterComponent(component, false);
        public void RegisterPermanentComponent<T>(T component) => RegisterComponent(component, true);

        private void RegisterComponent<T>(T component, bool isPermanent)
        {
            var type = typeof(T);

            if (singleInstanceComponentMap.ContainsKey(type))
                throw new InvalidOperationException($"Type {type.Name} is already registered as a single-instance component!");

            singleInstanceComponentMap.Add(type, component);
            componentPermanenceMap.Add(type, isPermanent);
        }

        public void UnregisterComponent<T>()
        {
            singleInstanceComponentMap.Remove(typeof(T));
            componentPermanenceMap.Remove(typeof(T));
        }

        public T Get<T>() => GetSingleInstanceComponent<T>();

        public T GetSingleInstanceComponent<T>()
        {
            return (T)singleInstanceComponentMap[typeof(T)];
        }

        public void ClearSessionComponents()
        {
            var keys = singleInstanceComponentMap.Keys.ToList();
            foreach (var key in keys)
            {
                if (!componentPermanenceMap[key])
                {
                    singleInstanceComponentMap.Remove(key);
                    componentPermanenceMap.Remove(key);
                }
            }
        }
    }
}
