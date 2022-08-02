using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    [CustomEditor(typeof(AvaCryptV2Root))]
    [CanEditMultipleObjects]
    public class AvaCryptRootEditor : Editor
    {
        SerializedProperty _keyNamesProperty;
        SerializedProperty m_IgnoredMaterialsProperty;
        SerializedProperty _distortRatioProperty;
        SerializedProperty _keysProperty;
        SerializedProperty _thirdsProperty;
        SerializedProperty _vrcSavedParamsPathProperty;
        bool _debugFoldout = false;
        ReorderableList m_IgnoreList; 
        
        void OnEnable()
        {
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _keysProperty = serializedObject.FindProperty("_bitKeys");
            _vrcSavedParamsPathProperty = serializedObject.FindProperty("_vrcSavedParamsPath");
            m_IgnoredMaterialsProperty = serializedObject.FindProperty("m_IgnoredMaterials");
            
            m_IgnoreList = new ReorderableList(serializedObject, m_IgnoredMaterialsProperty, true, true, true, true);
            m_IgnoreList.drawElementCallback = DrawListItems;
            m_IgnoreList.drawHeaderCallback = DrawHeader; 
        }
        
        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Ignored Materials");
        }
        
        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = m_IgnoreList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
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
            m_IgnoreList.DoLayoutList();
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
                GUILayout.Label("Deletes all the objects AvaCrypt wrote to your controller.");
                GUILayout.Label("Try running this if something gets weird with encrypting.");
                if (GUILayout.Button("Delete AvaCrypt Objects From Controller"))
                {
                    avaCryptV2Root.DeleteAvaCryptObjectsFromController();
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