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

        private void OnEnable()
        {
            _keyNamesProperty = serializedObject.FindProperty("_keynames");
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _keysProperty = serializedObject.FindProperty("_keys");
            _thirdsProperty = serializedObject.FindProperty("_averageToThirds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AvaCryptV2Root avaCryptV2Root = target as AvaCryptV2Root;

            EditorGUILayout.Space();
            GUILayout.Label("Validate all parameters, layers and animations are correct in this avatar's AnimatorController.");
            if (GUILayout.Button("Validate Animator Controller"))
            {
                avaCryptV2Root.ValidateAnimatorController();
            }

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
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Average keys to Thirds");
            GUILayout.TextArea("Disabling this will make your keys more robust, but it may cause syncing issues in VRC.\n" +
                               "An old version of VRC had the syncing issue, so in AvaCrypt v1 this was enabled by default\n" +
                               "But new VRC seems to not have the issue? Lets disable by default. Re-enable if sync issues arise.");
            EditorGUILayout.PropertyField(_thirdsProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}