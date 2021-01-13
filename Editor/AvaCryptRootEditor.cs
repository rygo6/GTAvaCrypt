using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    [CustomEditor(typeof(AvaCryptRoot))]
    [CanEditMultipleObjects]
    public class AvaCryptRootEditor : Editor 
    {
        SerializedProperty _distortRatioProperty;
        SerializedProperty _key0Property;
        SerializedProperty _key1Property;
        SerializedProperty _key2Property;
        SerializedProperty _key3Property;
        
        void OnEnable()
        {
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _key0Property = serializedObject.FindProperty("_key0");
            _key1Property = serializedObject.FindProperty("_key1");
            _key2Property = serializedObject.FindProperty("_key2");
            _key3Property = serializedObject.FindProperty("_key3");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            AvaCryptRoot avaCryptRoot = target as AvaCryptRoot;
            
            EditorGUILayout.Space();
            GUILayout.Label("Validate all parameters, layers and animations are correct in this avatar's AnimatorController.");
            if (GUILayout.Button("Validate Animator Controller"))
            {
                avaCryptRoot.ValidateAnimatorController();
            }
            EditorGUILayout.Space();
            GUILayout.Label("Validate the AnimatorController, then create encrypted avatar.");
            if (GUILayout.Button("Encrypt Avatar"))
            {
                avaCryptRoot.EncryptAvatar();
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_distortRatioProperty);
            EditorGUILayout.PropertyField(_key0Property);
            EditorGUILayout.PropertyField(_key1Property);
            EditorGUILayout.PropertyField(_key2Property);
            EditorGUILayout.PropertyField(_key3Property);
            serializedObject.ApplyModifiedProperties();
        }
    }
}