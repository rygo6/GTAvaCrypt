using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    [CustomEditor(typeof(AvaCryptV2Root))]
    [CanEditMultipleObjects]
    public class AvaCryptRootEditor : Editor
    {
        SerializedProperty _keyNamesProperty;
        SerializedProperty _distortRatioProperty;
        SerializedProperty _keysProperty;
        SerializedProperty _thirdsProperty;
        bool _debugFoldout = false;

        private void OnEnable()
        {
            // _keyNamesProperty = serializedObject.FindProperty("_keynames");
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _keysProperty = serializedObject.FindProperty("_bitKeys");
            // _thirdsProperty = serializedObject.FindProperty("_averageToThirds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AvaCryptV2Root avaCryptV2Root = target as AvaCryptV2Root;
            
            EditorGUILayout.Space();
            GUILayout.Label("Validate the AnimatorController, then create encrypted avatar.");
            if (GUILayout.Button("Encrypt Avatar"))
            {
                avaCryptV2Root.EncryptAvatar();
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_distortRatioProperty);
            EditorGUILayout.Space();
            
            EditorGUILayout.Separator();

            EditorGUILayout.Space();
            for (int i = 0; i < _keysProperty.arraySize; ++i)
            {
                EditorGUILayout.PropertyField(_keysProperty.GetArrayElementAtIndex(i), new GUIContent($"BitKey{i}"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            _debugFoldout = EditorGUILayout.Foldout(_debugFoldout, "Debug");
            if (_debugFoldout)
            {
                EditorGUILayout.Space();
                GUILayout.Label("Validate all parameters, layers and animations are correct in this avatar's AnimatorController.");
                if (GUILayout.Button("Validate Animator Controller"))
                {
                    avaCryptV2Root.ValidateAnimatorController();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}