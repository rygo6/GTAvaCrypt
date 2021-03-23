using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    [CustomEditor(typeof(AvaCryptRoot))]
    [CanEditMultipleObjects]
    public class AvaCryptRootEditor : Editor
    {
        SerializedProperty _distortRatioProperty;
        SerializedProperty _keysProperty;

        private void OnEnable()
        {
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
            EditorGUILayout.LabelField("Set to 0 to disable.");
            for (int i = 0; i < _keysProperty.arraySize; ++i)
            {
                EditorGUILayout.PropertyField(_keysProperty.GetArrayElementAtIndex(i),
                    new GUIContent(_keysProperty.GetArrayElementAtIndex(i).intValue == 0
                        ? $"Key {i} Disabled."
                        : $"Key {i}"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}