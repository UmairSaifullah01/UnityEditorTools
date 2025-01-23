using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace THEBADDEST.EditorTools
{

    [CustomEditor(typeof(Object), true)]
    // [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeEditor : Editor
    {
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
    
            var targetObject = target;
    
            if (targetObject == null) return;
    
            var methods = targetObject.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(ButtonAttribute), false);
                if (attributes.Length > 0)
                {
                    var buttonAttribute = (ButtonAttribute)attributes[0];

                    // Check the number of parameters
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        // No parameters
                        if (GUILayout.Button(method.Name))
                        {
                            method.Invoke(targetObject, null);
                        }
                    }

                }
            }
        }
    }


}
