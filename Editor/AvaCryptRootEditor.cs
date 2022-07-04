using UnityEditor;
using UnityEngine;
using GeoTetra.GTAvaUtil;

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
        bool _lockKeys = true;

        Texture2D HeaderTexture;
        void OnEnable()
        {
            _distortRatioProperty = serializedObject.FindProperty("_distortRatio");
            _keysProperty = serializedObject.FindProperty("_bitKeys");
            _vrcSavedParamsPathProperty = serializedObject.FindProperty("_vrcSavedParamsPath");
            HeaderTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.geotetra.gtavacrypt/Textures/Titlebar.png", typeof(Texture2D));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button(new GUIContent(HeaderTexture, "Vist my Discord for help!"), EditorStyles.label, GUILayout.Height(Screen.width / 8)))
            {
                Application.OpenURL("https://discord.gg/nbzqtaVP9J");
            }
            
            AvaCryptV2Root avaCryptV2Root = target as AvaCryptV2Root;
            
            //Do the big important buttons
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Encrypt Avatar", "Validate the AnimatorController, then create encrypted avatar."), GUILayout.Height(Screen.width / 10), GUILayout.Width((Screen.width / 2) - 20f)))
            {
                avaCryptV2Root.EncryptAvatar();
            }

            if (GUILayout.Button(new GUIContent("Write Keys", "Write your keys to saved attributes!"), GUILayout.Height(Screen.width / 10), GUILayout.Width((Screen.width / 2) - 20f)))
            {
                avaCryptV2Root.WriteBitKeysToExpressions();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //Do the properties
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width((Screen.width / 2) - 20f));
            _distortRatioProperty.floatValue = GUILayout.HorizontalSlider(_distortRatioProperty.floatValue, .1f, .4f);
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distort Ratio:");
            GUILayout.FlexibleSpace();
            EditorGUILayout.FloatField(_distortRatioProperty.floatValue);
            GUILayout.EndHorizontal();
            GUILayout.Label("Set high enough so your encrypted mesh is visuall. Default = .1", EditorStyles.wordWrappedLabel);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width((Screen.width / 2) - 20f));
            GUILayout.Space(3);
            GUILayout.Label("VRC Saved Paramters Path");
            _vrcSavedParamsPathProperty.stringValue = EditorGUILayout.TextField(_vrcSavedParamsPathProperty.stringValue);
            GUILayout.Label("Ensure this is pointing to your LocalAvatarData folder!", EditorStyles.wordWrappedLabel);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Unlock Poi mats", "Unlock All Poi Materials In Hierarchy")))
            {
                MenuUtilites.UnlockAllPoiMaterialsInHierarchy(null);
            }
            if (_lockKeys)
            {
                if (GUILayout.Button(new GUIContent("Unlock bitKeys", "Prevent changes to key selection"), GUILayout.Width((Screen.width / 2) - 20f))) _lockKeys = !_lockKeys;
            }
            else if (GUILayout.Button(new GUIContent("Lock BitKeys", "Prevent changes to key selection"), GUILayout.Width((Screen.width / 2) - 20f))) _lockKeys = !_lockKeys;
            GUILayout.EndHorizontal();

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginDisabledGroup(_lockKeys);

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Bitkeys", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("These are the keys used to encrypt the mesh.");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();

                //Display in 4 columns
                for (int i = 0; i < _keysProperty.arraySize/4; i++)
                {
                    GUILayout.BeginHorizontal();
                    _keysProperty.GetArrayElementAtIndex(i).boolValue = GUILayout.Toggle(_keysProperty.GetArrayElementAtIndex(i).boolValue, "BitKey" + i);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                for (int i = _keysProperty.arraySize / 4; i < _keysProperty.arraySize / 2; i++)
                {
                    GUILayout.BeginHorizontal();
                    _keysProperty.GetArrayElementAtIndex(i).boolValue = GUILayout.Toggle(_keysProperty.GetArrayElementAtIndex(i).boolValue, "BitKey" + i);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                for (int i = _keysProperty.arraySize / 2; i < (_keysProperty.arraySize / 4) * 3 ; i++)
                {
                    GUILayout.BeginHorizontal();
                    _keysProperty.GetArrayElementAtIndex(i).boolValue = GUILayout.Toggle(_keysProperty.GetArrayElementAtIndex(i).boolValue, "BitKey" + i);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                for (int i = (_keysProperty.arraySize / 4) * 3; i < _keysProperty.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    _keysProperty.GetArrayElementAtIndex(i).boolValue = GUILayout.Toggle(_keysProperty.GetArrayElementAtIndex(i).boolValue, "BitKey" + i);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                //Generate key button
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Generate new Keys", "Generate new key overriding old one. Will need to write key again!")))
                {
                    avaCryptV2Root.GenerateNewKey();
                }
                EditorGUI.EndDisabledGroup();
            }
            
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            _debugFoldout = EditorGUILayout.Foldout(_debugFoldout, "Debug");
            if (_debugFoldout)
            {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Validate Animator Controller", "Validate all parameters, layers and animations are correct in this avatar's AnimatorController."), GUILayout.Height(Screen.width / 10), GUILayout.Width((Screen.width / 2) - 20f)))
                {
                    avaCryptV2Root.ValidateAnimatorController();
                }

                if (GUILayout.Button(new GUIContent("Delete AvaCryptV1 Objects From Controller", "Deletes all the objects AvaCrypt V1 wrote to your controller."), GUILayout.Height(Screen.width / 10), GUILayout.Width((Screen.width / 2) - 20f)))
                {
                    avaCryptV2Root.DeleteAvaCryptV1ObjectsFromController();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}