using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    [CustomEditor(typeof(AvaCryptRoot))]
    [CanEditMultipleObjects]
    public class AvaCryptRootEditor : Editor
    {
        private SerializedProperty _keyNamesProperty;
        SerializedProperty _distortRatioProperty;
        SerializedProperty _keysProperty;

        private void OnEnable()
        {
            _keyNamesProperty = serializedObject.FindProperty("_keynames");
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _keysProperty = serializedObject.FindProperty("_keys");
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
            EditorGUILayout.Space();
            
            EditorGUILayout.Separator();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Keys");
            GUILayout.TextArea("Keys should be named values that would be hard to discern as being a key.\n\n" +
                                     "For example, 'EyeX', 'TailDown', 'ShirtColor' etc.\n\n" +
                                     "This is so that a VRC client cannot autodetect keys, and a ripper would have to brute force all of them.");
            for (int i = 0; i < _keysProperty.arraySize; ++i)
            {
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField(new GUIContent($"Key {i}"));
                
                EditorGUILayout.PropertyField(_keyNamesProperty.GetArrayElementAtIndex(i), new GUIContent($"Name"));
                EditorGUILayout.PropertyField(_keysProperty.GetArrayElementAtIndex(i),
                    new GUIContent(_keysProperty.GetArrayElementAtIndex(i).intValue == 0
                        ? $"Disabled"
                        : $"Value"));
                
                                
                EditorGUILayout.Separator();
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}