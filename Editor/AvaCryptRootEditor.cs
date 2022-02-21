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
        SerializedProperty _vrcSavedParamsPathProperty;
        bool _debugFoldout = false;

        void OnEnable()
        {
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _keysProperty = serializedObject.FindProperty("_bitKeys");
            _vrcSavedParamsPathProperty = serializedObject.FindProperty("_vrcSavedParamsPath");
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
            GUILayout.Label("Write your keys to saved attributes!");
            if (GUILayout.Button("Write Keys"))
            {
                avaCryptV2Root.WriteBitKeysToExpressions();
            }
            EditorGUILayout.PropertyField(_vrcSavedParamsPathProperty);

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
                
                EditorGUILayout.Space();
                GUILayout.Label("Deletes all the layers which AvaCrypt added to the AnimatorController.");
                if (GUILayout.Button("Cleanup Controller"))
                {
                    avaCryptV2Root.CleanupController();
                }
                
                EditorGUILayout.Space();
                GUILayout.Label("Generate new key overriding old one. Will need to write key again!");
                if (GUILayout.Button("Generate Key"))
                {
                    avaCryptV2Root.GenerateNewKey();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}