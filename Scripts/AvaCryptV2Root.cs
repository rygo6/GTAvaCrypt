using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using Thry;
using UnityEditor;
using UnityEditor.Animations;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
#endif

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptV2Root : MonoBehaviour
    {
        [Header("Set high enough so your encrypted mesh is visuall. Default = .1")]
        [Range(.1f, .4f)]
        [SerializeField] 
        float _distortRatio = .2f;

        [Header("Ensure this is pointing to your LocalAvatarData folder!")]
        [SerializeField] 
        string _vrcSavedParamsPath = string.Empty;

        [Header("Materials in this list will be ignored.")]
        [SerializeField] 
        List<Material> m_IgnoredMaterials = new List<Material>();
        
        [SerializeField] 
        bool[] _bitKeys = new bool[KeyCount];
        
        const int KeyCount = 32;
        StringBuilder _sb = new StringBuilder();
        
        #if UNITY_EDITOR
        readonly AvaCryptController _avaCryptController = new AvaCryptController();

        public void ValidateAnimatorController()
        {
            AnimatorController controller = GetAnimatorController();

            _avaCryptController.InitializeCount(_bitKeys.Length);
            _avaCryptController.ValidateAnimations(gameObject, controller);
            _avaCryptController.ValidateParameters(controller);
            _avaCryptController.ValidateLayers(controller);
        }

        AnimatorController GetAnimatorController()
        {
            if (transform.parent != null)
            {
                EditorUtility.DisplayDialog("AvaCryptRoot component not on a Root GameObject.", 
                    "The GameObject which the AvaCryptRoot component is placed on must not be the child of any other GameObject.", 
                    "Ok");
                return null;
            }
            
            Animator animator = GetComponent<Animator>();
            if (animator == null)
            {
                EditorUtility.DisplayDialog("No Animator.", 
                    "Add an animator to the Avatar's root GameObject.", 
                    "Ok");
                return null;
            }
            
            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;
            if(runtimeController == null)
            {
                EditorUtility.DisplayDialog("Animator has no AnimatorController.", 
                    "Add an AnimatorController to the Animator component.", 
                    "Ok");
                return null;
            }
     
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(runtimeController));
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Could not get AnimatorController.", 
                    "This shouldn't happen... don't know why this would happen.", 
                    "Ok");
                return null;
            }

            return controller;
        }
        
        public void EncryptAvatar()
        {
            ValidateAnimatorController();
            
            string newName = gameObject.name + "_Encrypted";
            
            // delete old GO, do as such in case its disabled
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] sceneRoots = scene.GetRootGameObjects();
            foreach(GameObject oldGameObject in sceneRoots)
            {
                if (oldGameObject.name == newName) DestroyImmediate(oldGameObject);
            }

            GameObject encodedGameObject = Instantiate(gameObject);
            encodedGameObject.name = newName;
            encodedGameObject.SetActive(true);
            
            // _avaCryptMesh.InitializeRandoms(_bitKeys.Length);
            AvaCryptData data = new AvaCryptData(_bitKeys.Length);
            string decodeShader = AvaCryptMaterial.GenerateDecodeShader(data, _bitKeys);

            MeshFilter[] meshFilters = encodedGameObject.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = encodedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<Material> aggregateIgnoredMaterials = new List<Material>();

            // Gather all materials to ignore based on if they are shared in mesh
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.GetComponent<MeshRenderer>() != null)
                {
                    var materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;
                    AddMaterialsToIgnoreList(materials, aggregateIgnoredMaterials);
                }
            }
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var materials = skinnedMeshRenderer.sharedMaterials;
                AddMaterialsToIgnoreList(materials, aggregateIgnoredMaterials);
            }

            // Do encrypting
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.GetComponent<MeshRenderer>() != null)
                {
                    var materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;
                    if (EncryptMaterials(materials, decodeShader, aggregateIgnoredMaterials))
                    {
                        meshFilter.sharedMesh = AvaCryptMesh.EncryptMesh(meshFilter.sharedMesh, _distortRatio, data);
                    }
                    else
                    {
                        Debug.Log($"Ignoring Encrypt on {meshFilter.gameObject} contains ignored material!");
                    }
                }
            }
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var materials = skinnedMeshRenderer.sharedMaterials;
                if (EncryptMaterials(materials, decodeShader, aggregateIgnoredMaterials))
                {
                    skinnedMeshRenderer.sharedMesh = AvaCryptMesh.EncryptMesh(skinnedMeshRenderer.sharedMesh, _distortRatio, data);
                }
                else
                {
                    Debug.Log($"Ignoring Encrypt on {skinnedMeshRenderer.gameObject} contains ignored material!");
                }
            }

            AvaCryptV2Root[] avaCryptRoots = encodedGameObject.GetComponentsInChildren<AvaCryptV2Root>();
            foreach (AvaCryptV2Root avaCryptRoot in avaCryptRoots)
            {
                DestroyImmediate(avaCryptRoot);
            }
            
            // Disable old for convienence.
            gameObject.SetActive(false);
        }

        void AddMaterialsToIgnoreList(Material[] materials, List<Material> aggregateIgnoredMaterials)
        {
            foreach (var material in materials)
            {
                if (m_IgnoredMaterials.Contains(material))
                {
                    aggregateIgnoredMaterials.AddRange(materials);
                    return;
                }
            }
        }

        bool EncryptMaterials(Material[] materials, string decodeShader,  List<Material> aggregateIgnoredMaterials)
        {
            bool materialEncrypted = false;
            bool ignoredMats = false;
            foreach (var mat in materials)
            {
                if (mat != null && mat.shader.name.Contains(".poiyomi/Poiyomi 8"))
                {
                    if (!mat.shader.name.Contains("Hidden/Locked"))
                    {
                        ShaderOptimizer.SetLockedForAllMaterials(new []{mat}, 1, true, false, false);
                    }
                    
                    if (!mat.shader.name.Contains("Hidden/Locked"))
                    {
                        Debug.LogError($"{mat.name} {mat.shader.name} Trying to Inject not-locked shader?!");
                        continue;
                    }

                    if (aggregateIgnoredMaterials.Contains(mat))
                    {
                        ignoredMats = true;
                        continue;
                    }

                    string shaderPath = AssetDatabase.GetAssetPath(mat.shader);
                    string path = Path.GetDirectoryName(shaderPath);
                    string decodeShaderPath = Path.Combine(path, "GTModelDecode.cginc");
;                   File.WriteAllText(decodeShaderPath, decodeShader);

                    string shaderText = File.ReadAllText(shaderPath);
                    const string avaComment = "//AvaCrypt Injected";
                    if (!shaderText.Contains("//AvaCrypt Injected"))
                    {
                        _sb.Clear();
                        _sb.AppendLine(avaComment);
                        _sb.Append(shaderText);
                        _sb.Replace(AvaCryptMaterial.DefaultPoiUV, AvaCryptMaterial.AlteredPoiUV);
                        // _sb.Replace(AvaCryptMaterial.DefaultPoiUVArray, AvaCryptMaterial.AlteredPoiUVArray);
                        _sb.Replace(AvaCryptMaterial.DefaultPoiVert, AvaCryptMaterial.AlteredPoiVert);
                        _sb.Replace(AvaCryptMaterial.DefaultVertSetup, AvaCryptMaterial.AlteredVertSetup);
                        // _sb.Replace(AvaCryptMaterial.DefaultUvTransfer, AvaCryptMaterial.AlteredUvTransfer);
                        File.WriteAllText(shaderPath, _sb.ToString());
                    }

                    materialEncrypted = true;
                }
            }

            return materialEncrypted && !ignoredMats;
        }

        public void WriteBitKeysToExpressions()
        {
#if  VRC_SDK_VRCSDK3
            var descriptor = GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                Debug.LogError("Keys not written! Couldn't find VRCAvatarDescriptor next to GTAvaCryptRoot");
                EditorUtility.DisplayDialog("Keys not written! Missing PipelineManager!", "Put AvaCryptRoot next to VRCAvatarDescriptor and run Write Keys again.", "Okay");
                return;
            }

            if (descriptor.expressionParameters == null)
            {
                Debug.LogError("Keys not written! Expressions is not filled in on VRCAvatarDescriptor!");
                EditorUtility.DisplayDialog("Keys not written! Expressions is not filled in on VRCAvatarDescriptor!", "Fill in the Parameters slot on the VRCAvatarDescriptor and run again.", "Okay");
                return;
            }

            if (AddBitKeys(descriptor.expressionParameters))
            {
                WriteKeysToSaveFile();
            }

#else
            Debug.LogError("Can't find VRC SDK?");
            EditorUtility.DisplayDialog("Can't find VRC SDK?", "You need to isntall VRC SDK.", "Okay");
#endif
        }

        public void WriteKeysToSaveFile()
        {
#if  VRC_SDK_VRCSDK3
            var pipelineManager = GetComponent<PipelineManager>();
            if (pipelineManager == null)
            {
                Debug.LogError("Keys not written! Couldn't find PipelineManager next to GTAvaCryptRoot");
                EditorUtility.DisplayDialog("Keys not written! Couldn't find PipelineManager next to GTAvaCryptRoot", "Put AvaCryptRoot next to PipelineManager and run Write Keys again.", "Okay");
                return;
            }

            if (string.IsNullOrWhiteSpace(pipelineManager.blueprintId))
            {
                Debug.LogError("Blueprint ID not filled in!");
                EditorUtility.DisplayDialog("Keys not written! Blueprint ID not filled in!", "You need to first populate your PipelineManager with a Blueprint ID before keys can be written. Publish your avatar to get the Blueprint ID, attach the ID through the PipelineManager then run Write Keys again.","Okay");
                return;
            }

            if (!Directory.Exists(_vrcSavedParamsPath))
            {
                Debug.LogError("Keys not written! Could not find VRC LocalAvatarData folder!");
                EditorUtility.DisplayDialog("Could not find VRC LocalAvatarData folder!", "Ensure the VRC Saved Params Path is point to your LocalAvatarData folder, should be at C:\\Users\\username\\AppData\\LocalLow\\VRChat\\VRChat\\LocalAvatarData\\, then run Write Keys again.","Okay");
                return;
            }

            foreach (var userDir in Directory.GetDirectories(_vrcSavedParamsPath))
            {
                string filePath = $"{userDir}\\{pipelineManager.blueprintId}";
                Debug.Log($"Writing keys to {filePath}");
                ParamFile paramFile = null;
                if (File.Exists(filePath))
                {
                    Debug.Log($"Avatar param file already exists, loading and editing.");
                    var json = File.ReadAllText(filePath);
                    paramFile = JsonUtility.FromJson<ParamFile>(json);
                }

                if (paramFile == null)
                {
                    paramFile = new ParamFile();
                    paramFile.animationParameters = new List<ParamFileEntry>();
                }

                for (int i = 0; i < KeyCount; ++i)
                {
                    int entryIndex = paramFile.animationParameters.FindIndex(p => p.name == $"BitKey{i}");
                    if (entryIndex != -1)
                    {
                        paramFile.animationParameters[entryIndex].value = _bitKeys[i] ? 1 : 0;
                    }
                    else
                    {
                        var newEntry = new ParamFileEntry()
                        {
                            name = $"BitKey{i}",
                            value = _bitKeys[i] ? 1 : 0
                        };
                        paramFile.animationParameters.Add(newEntry);
                    }
                }
                
                System.IO.File.WriteAllText(filePath, JsonUtility.ToJson(paramFile));
            }
            
            EditorUtility.DisplayDialog("Successfully Wrote Keys!", "Your avatar should now just work in VRChat. If you accidentally hit 'Reset Avatar' in VRC 3.0 menu, you need to run this again.","Okay");
            
#else
            Debug.LogError("Can't find VRC SDK?");
            EditorUtility.DisplayDialog("Can't find VRC SDK?", "You need to isntall VRC SDK.", "Okay");
#endif
        }

        [Serializable]
        public class ParamFile
        {
            public List<ParamFileEntry> animationParameters;
        }
        
        [Serializable]
        public class ParamFileEntry
        {
            public string name;
            public float value;
        }

        void Reset()
        {
            GenerateNewKey();
            _vrcSavedParamsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\VRChat\\VRChat\\LocalAvatarData\\";
        }


        [ContextMenu("CleanupBlendTrees")]
        public void GenerateNewKey()
        {
            for (int i = 0; i < _bitKeys.Length; ++i)
            {
                _bitKeys[i] = Random.Range(-1f, 1f) > 0;
            }
        }
        
#if  VRC_SDK_VRCSDK3
        [MenuItem("CONTEXT/VRCExpressionParameters/Add BitKeys")]
        static void AddBitKeys(MenuCommand command)
        {
            VRCExpressionParameters parameters = (VRCExpressionParameters) command.context;
            AddBitKeys(parameters);
        }

        public static bool AddBitKeys(VRCExpressionParameters parameters)
        {
            List<VRCExpressionParameters.Parameter> paramList = parameters.parameters.ToList();
            
            for (int i = 0; i < KeyCount; ++i)
            {
                string bitKeyName = $"BitKey{i}";
                
                int index = Array.FindIndex(parameters.parameters, p => p.name == bitKeyName);
                if (index != -1)
                {
                    Debug.Log($"Found BitKey in params {bitKeyName}");
                    parameters.parameters[index].saved = true;
                    parameters.parameters[index].defaultValue = 0;
                    parameters.parameters[index].valueType = VRCExpressionParameters.ValueType.Bool;
                }
                else
                {
                    Debug.Log($"Adding BitKey in params {bitKeyName}");
                    var newParam = new VRCExpressionParameters.Parameter
                    {
                        name = bitKeyName,
                        saved = true,
                        defaultValue = 0,
                        valueType = VRCExpressionParameters.ValueType.Bool
                    };
                    paramList.Add(newParam);
                }
            }
            
            parameters.parameters = paramList.ToArray();
            
            int remainingCost = VRCExpressionParameters.MAX_PARAMETER_COST - parameters.CalcTotalCost();;
            Debug.Log(remainingCost);
            if (remainingCost < 0)
            {
                Debug.LogError("Adding BitKeys took up too many parameters!");
                EditorUtility.DisplayDialog("Adding BitKeys took up too many parameters!", "Go to your VRCExpressionParameters and remove some unnecessary parameters to make room for the 32 BitKey bools and run this again.", "Okay");
                return false;
            }
            
            EditorUtility.SetDirty(parameters);

            return true;
        }
        
        [MenuItem("CONTEXT/VRCExpressionParameters/Remove BitKeys")]
        static void RemoveBitKeys(MenuCommand command)
        {
            VRCExpressionParameters parameters = (VRCExpressionParameters) command.context;
            RemoveBitKeys(parameters);
        }
        
        public static void RemoveBitKeys(VRCExpressionParameters parameters)
        {
            List<VRCExpressionParameters.Parameter> parametersList = parameters.parameters.ToList();
            parametersList.RemoveAll(p => p.name.Contains("BitKey"));
            parameters.parameters = parametersList.ToArray();
            
            EditorUtility.SetDirty(parameters);
        }
#endif   

        [ContextMenu("Delete AvaCryptV1 Objects From Controller")]
        public void DeleteAvaCryptV1ObjectsFromController()
        {
            _avaCryptController.DeleteAvaCryptV1ObjectsFromController(GetAnimatorController());
        }
#endif
    }
}
