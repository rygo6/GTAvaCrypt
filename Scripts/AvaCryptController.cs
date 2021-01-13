#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
using BlendTree = UnityEditor.Animations.BlendTree;
using Object = UnityEngine.Object;

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptController
    {
        private readonly string[] AvaCryptKeyNames = {"AvaCryptKey0", "AvaCryptKey1", "AvaCryptKey2", "AvaCryptKey3"};
        private readonly AnimationClip[] _clips0 = new AnimationClip[4];
        private readonly AnimationClip[] _clips100 = new AnimationClip[4];

        private const string StateMachineName = "AvaCryptKey{0} State Machine";
        private const string BlendTreeName = "AvaCryptKey{0} Blend Tree";
        
        public void ValidateAnimations(GameObject gameObject, AnimatorController controller)
        {
            for (int i = 0; i < AvaCryptKeyNames.Length; ++i)
            {
                ValidateClip(gameObject, controller, i);
            }

            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                for (int i = 0; i < _clips0.Length; ++i)
                {
                    string transformPath = AnimationUtility.CalculateTransformPath(meshRenderer.transform, gameObject.transform);
                    _clips0[i].SetCurve(transformPath, typeof(MeshRenderer), $"material._Key{i}", new AnimationCurve(new Keyframe(0, 0)));
                    _clips100[i].SetCurve(transformPath, typeof(MeshRenderer), $"material._Key{i}", new AnimationCurve(new Keyframe(0, 100)));
                }
            }
            
            SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                for (int i = 0; i < _clips0.Length; ++i)
                {
                    string transformPath = AnimationUtility.CalculateTransformPath(skinnedMeshRenderer.transform,gameObject.transform);
                    _clips0[i].SetCurve(transformPath, typeof(SkinnedMeshRenderer), $"material._Key{i}", new AnimationCurve(new Keyframe(0, 0)));
                    _clips100[i].SetCurve(transformPath, typeof(SkinnedMeshRenderer), $"material._Key{i}", new AnimationCurve(new Keyframe(0, 100)));
                }
            }
            
            AssetDatabase.SaveAssets();
        }

        private void ValidateClip(GameObject gameObject, AnimatorController controller, int index)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string controllerFileName = System.IO.Path.GetFileName(controllerPath);
            
            string clipName = $"{gameObject.name}_{AvaCryptKeyNames[index]}";
            string clipName0 = $"{clipName}_0";
            string clipName0File = $"{clipName0}.anim";
            string clipName100 = $"{clipName}_100";
            string clipName100File = $"{clipName100}.anim";
            
            if (controller.animationClips.All(c => c.name != clipName0))
            {
                _clips0[index] = new AnimationClip()
                {
                    name = clipName0
                };
                string clip0Path = controllerPath.Replace(controllerFileName, clipName0File);
                AssetDatabase.CreateAsset(_clips0[index], clip0Path);
                AssetDatabase.SaveAssets();
                Debug.Log($"Adding and Saving Clip: {clip0Path}");
            }
            else
            {
                _clips0[index] = controller.animationClips.FirstOrDefault(c => c.name == clipName0);
                Debug.Log($"Found clip: {clipName0}");
            }
            
            if (controller.animationClips.All(c => c.name != clipName100))
            {
                _clips100[index] = new AnimationClip()
                {
                    name = clipName100
                };
                string clip100Path = controllerPath.Replace(controllerFileName, clipName100File);
                AssetDatabase.CreateAsset(_clips100[index], clip100Path);
                AssetDatabase.SaveAssets();
                Debug.Log($"Adding and Saving Clip: {clip100Path}");
            }
            else
            {
                _clips100[index] = controller.animationClips.FirstOrDefault(c => c.name == clipName100);
                Debug.Log($"Found clip: {clipName100}");
            }
        }
        
        public void ValidateParameters(AnimatorController controller)
        {
            foreach (string keyName in AvaCryptKeyNames)
            {
                if (controller.parameters.All(parameter => parameter.name != keyName))
                {
                    controller.AddParameter(keyName, AnimatorControllerParameterType.Float);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Adding parameter: {keyName}");
                }
                else
                {
                    Debug.Log($"Parameter already added: {keyName}");
                }
            }
        }

        public void ValidateLayers(AnimatorController controller)
        {
            for (int i = 0; i < AvaCryptKeyNames.Length; ++i)
            {
                if (controller.layers.All(l => l.name != AvaCryptKeyNames[i]))
                {
                    CreateLayer(i, controller);
                }
                else
                {
                    Debug.Log($"Layer already existing: {AvaCryptKeyNames[i]}");
                    AnimatorControllerLayer layer = controller.layers.FirstOrDefault(l => l.name == AvaCryptKeyNames[i]);

                    if (layer.stateMachine == null)
                    {
                        Debug.Log("Layer missing state machine.");
                        
                        // Try to delete blend tree and layers if by chance they still exist for some reason.
                        DeleteObjectFromController(controller, string.Format(StateMachineName, i));
                        DeleteObjectFromController(controller, string.Format(BlendTreeName, i));
                        
                        controller.RemoveLayer(controller.layers.ToList().IndexOf(layer));
                        
                        CreateLayer(i, controller);
                    }
                    else
                    {
                        ValidateLayerBlendTree(i, layer, controller);
                    }
                }
            }
        }
        
        private void ValidateLayerBlendTree(int index, AnimatorControllerLayer layer, AnimatorController controller)
        {
            string blendTreeName = string.Format(BlendTreeName, index);
            
            if (layer.stateMachine.states.All(s => s.state.name != blendTreeName))
            {
                Debug.Log($"Layer missing blend tree. {blendTreeName}");
                DeleteObjectFromController(controller, blendTreeName);
                AddBlendTree(index, layer, controller);
            }
            else
            {
                Debug.Log($"Layer Blend Tree Validated {blendTreeName}.");
                
                BlendTree blendTree = layer.stateMachine.states.FirstOrDefault(s => s.state.name == blendTreeName).state.motion as BlendTree;
                
                // Just re-assign since ChildMotions aren't their own ScriptableObjects.
                ChildMotion childMotion0 = new ChildMotion
                {
                    motion = _clips0[index],
                    timeScale = 1
                };
                ChildMotion childMotion1 = new ChildMotion
                {
                    motion = _clips100[index],
                    timeScale = 1
                };
                blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};
                
                AssetDatabase.SaveAssets();;
            }
        }

        private void CreateLayer(int index, AnimatorController controller)
        {
            Debug.Log($"Creating layer: {AvaCryptKeyNames[index]}");
            
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = AvaCryptKeyNames[index],
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine(),
            };
            layer.stateMachine.name = string.Format(StateMachineName, index);
            
            controller.AddLayer(layer);
            AssetDatabase.AddObjectToAsset(layer.stateMachine, controllerPath);
            AssetDatabase.SaveAssets();
            
            AddBlendTree(index, layer, controller);
        }

        private void AddBlendTree(int index, AnimatorControllerLayer layer, AnimatorController controller)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string blendTreeName = string.Format(BlendTreeName, index);
            
            AnimatorState state = layer.stateMachine.AddState(blendTreeName);
            state.speed = 1;
            
            BlendTree blendTree = new BlendTree
            {
                name = blendTreeName,
                blendType = BlendTreeType.Simple1D,
                blendParameter = AvaCryptKeyNames[index],
            };
            
            ChildMotion childMotion0 = new ChildMotion
            {
                motion = _clips0[index],
                timeScale = 1
            };
            ChildMotion childMotion1 = new ChildMotion
            {
                motion = _clips100[index],
                timeScale = 1
            };
            blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};
            
            state.motion = blendTree;
            AssetDatabase.AddObjectToAsset(blendTree, controllerPath);
            
            AssetDatabase.SaveAssets();
        }

        private void DeleteObjectFromController(AnimatorController controller, string name)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            foreach (Object subObject in AssetDatabase.LoadAllAssetsAtPath(controllerPath))
            {
                if (subObject.hideFlags == HideFlags.None && subObject.name == name)
                {
                    AssetDatabase.RemoveObjectFromAsset(subObject);
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}
#endif