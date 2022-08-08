using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    [CustomEditor(typeof(AvaCryptV2Root))]
    [CanEditMultipleObjects]
    public class AvaCryptRootEditor : Editor
    {
        SerializedProperty m_KeyNamesProperty;
        SerializedProperty m_IgnoredMaterialsProperty;
        SerializedProperty m_AdditionalMaterialsProperty;
        SerializedProperty m_DistortRatioProperty;
        SerializedProperty m_KeysProperty;
        SerializedProperty m_ThirdsProperty;
        SerializedProperty m_VrcSavedParamsPathProperty;
        bool m_DebugFoldout = false;
        ReorderableList m_IgnoreList; 
        ReorderableList m_AdditionalList; 
        
        void OnEnable()
        {
            m_DistortRatioProperty = serializedObject.FindProperty("_distortRatio");
            m_KeysProperty = serializedObject.FindProperty("_bitKeys");
            m_VrcSavedParamsPathProperty = serializedObject.FindProperty("_vrcSavedParamsPath");
            m_AdditionalMaterialsProperty = serializedObject.FindProperty("m_AdditionalMaterials");
            m_IgnoredMaterialsProperty = serializedObject.FindProperty("m_IgnoredMaterials");
            
            m_AdditionalList = new ReorderableList(serializedObject, m_AdditionalMaterialsProperty, true, true, true, true)
            {
                drawElementCallback = AdditionalDrawListItems,
                drawHeaderCallback = AdditionalDrawHeader
            };

            m_IgnoreList = new ReorderableList(serializedObject, m_IgnoredMaterialsProperty, true, true, true, true)
            {
                drawElementCallback = IgnoreDrawListItems,
                drawHeaderCallback = IgnoreDrawHeader
            };
        }
        
        void AdditionalDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Additional Materials");
        }
        
        void AdditionalDrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = m_AdditionalList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element);
        }
        
        void IgnoreDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Ignored Materials");
        }
        
        void IgnoreDrawListItems(Rect rect, int index, bool isActive, bool isFocused)
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
            EditorGUILayout.PropertyField(m_VrcSavedParamsPathProperty);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_DistortRatioProperty);
            EditorGUILayout.Space();
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.Space();
            m_AdditionalList.DoLayoutList();
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            m_IgnoreList.DoLayoutList();
            EditorGUILayout.Space();
            
            EditorGUILayout.Separator();

            EditorGUILayout.Space();
            for (int i = 0; i < m_KeysProperty.arraySize; ++i)
            {
                EditorGUILayout.PropertyField(m_KeysProperty.GetArrayElementAtIndex(i), new GUIContent($"BitKey{i}"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            m_DebugFoldout = EditorGUILayout.Foldout(m_DebugFoldout, "Debug");
            if (m_DebugFoldout)
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