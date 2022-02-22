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
        private string[] _avaCryptKeyNames;
        private AnimationClip[] _clipsFalse;
        private AnimationClip[] _clipsTrue;

        private const string StateMachineName = "AvaCryptKey{0} State Machine";
        private const string BlendTreeName = "AvaCryptKey{0} Blend Tree";
        private const string BitKeySwitchName = "AvaCryptKey{0}{1} BitKey Switch";

        public void InitializeCount(int count)
        {
            _clipsFalse = new AnimationClip[count];
            _clipsTrue = new AnimationClip[count];
            _avaCryptKeyNames = new string[count];
            for (int i = 0; i < _avaCryptKeyNames.Length; ++i)
            {
                _avaCryptKeyNames[i] = $"BitKey{i}";
            }
        }
        
        public void ValidateAnimations(GameObject gameObject, AnimatorController controller)
        {
            for (int i = 0; i < _avaCryptKeyNames.Length; ++i)
            {
                ValidateClip(gameObject, controller, i);
            }

            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                for (int i = 0; i < _clipsFalse.Length; ++i)
                {
                    string transformPath = AnimationUtility.CalculateTransformPath(meshRenderer.transform, gameObject.transform);
                    _clipsFalse[i].SetCurve(transformPath, typeof(MeshRenderer), $"material._BitKey{i}", new AnimationCurve(new Keyframe(0, 0)));
                    _clipsTrue[i].SetCurve(transformPath, typeof(MeshRenderer), $"material._BitKey{i}", new AnimationCurve(new Keyframe(0, 1)));
                }
            }
            
            SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                for (int i = 0; i < _clipsFalse.Length; ++i)
                {
                    string transformPath = AnimationUtility.CalculateTransformPath(skinnedMeshRenderer.transform,gameObject.transform);
                    _clipsFalse[i].SetCurve(transformPath, typeof(SkinnedMeshRenderer), $"material._BitKey{i}", new AnimationCurve(new Keyframe(0, 0)));
                    _clipsTrue[i].SetCurve(transformPath, typeof(SkinnedMeshRenderer), $"material._BitKey{i}", new AnimationCurve(new Keyframe(0, 1)));
                }
            }
            
            AssetDatabase.SaveAssets();
        }

        private void ValidateClip(GameObject gameObject, AnimatorController controller, int index)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string controllerFileName = System.IO.Path.GetFileName(controllerPath);
            
            string clipName = $"{gameObject.name}_{_avaCryptKeyNames[index]}";
            string clipNameFalse = $"{clipName}_False";
            string clipNameFalseFile = $"{clipNameFalse}.anim";
            string clipNameTrue = $"{clipName}_True";
            string clipNameTrueFile = $"{clipNameTrue}.anim";
            
            if (controller.animationClips.All(c => c.name != clipNameFalse))
            {
                _clipsFalse[index] = new AnimationClip()
                {
                    name = clipNameFalse
                };
                string clip0Path = controllerPath.Replace(controllerFileName, clipNameFalseFile);
                AssetDatabase.CreateAsset(_clipsFalse[index], clip0Path);
                AssetDatabase.SaveAssets();
                Debug.Log($"Adding and Saving Clip: {clip0Path}");
            }
            else
            {
                _clipsFalse[index] = controller.animationClips.FirstOrDefault(c => c.name == clipNameFalse);
                Debug.Log($"Found clip: {clipNameFalse}");
            }
            
            if (controller.animationClips.All(c => c.name != clipNameTrue))
            {
                _clipsTrue[index] = new AnimationClip()
                {
                    name = clipNameTrue
                };
                string clip100Path = controllerPath.Replace(controllerFileName, clipNameTrueFile);
                AssetDatabase.CreateAsset(_clipsTrue[index], clip100Path);
                AssetDatabase.SaveAssets();
                Debug.Log($"Adding and Saving Clip: {clip100Path}");
            }
            else
            {
                _clipsTrue[index] = controller.animationClips.FirstOrDefault(c => c.name == clipNameTrue);
                Debug.Log($"Found clip: {clipNameTrue}");
            }
        }
        
        public void ValidateParameters(AnimatorController controller)
        {
            foreach (string keyName in _avaCryptKeyNames)
            {
                if (controller.parameters.All(parameter => parameter.name != keyName))
                {
                    controller.AddParameter(keyName, AnimatorControllerParameterType.Bool);
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
            for (int i = 0; i < _avaCryptKeyNames.Length; ++i)
            {
                if (controller.layers.All(l => l.name != _avaCryptKeyNames[i]))
                {
                    CreateLayer(i, controller);
                }
                else
                {
                    Debug.Log($"Layer already existing: {_avaCryptKeyNames[i]}");
                    AnimatorControllerLayer layer = controller.layers.FirstOrDefault(l => l.name == _avaCryptKeyNames[i]);

                    if (layer.stateMachine == null)
                    {
                        Debug.Log("Layer missing state machine.");
                        
                        // Try to delete blend tree and layers if by chance they still exist for some reason.
                        DeleteObjectFromController(controller, string.Format(StateMachineName, i));
                        
                        controller.RemoveLayer(controller.layers.ToList().IndexOf(layer));
                        
                        CreateLayer(i, controller);
                    }
                    else
                    {
                        ValidateBitKeySwitch(i, layer, controller);
                    }
                }
            }
        }
        
        private void ValidateBitKeySwitch(int index, AnimatorControllerLayer layer, AnimatorController controller)
        {
            string trueSwitchName = string.Format(BitKeySwitchName, "True", index);
            string falseSwitchName = string.Format(BitKeySwitchName, "False", index);
            
            if (layer.stateMachine.states.All(s => s.state.name != trueSwitchName))
            {
                Debug.Log($"Layer missing BitKeySwtich. {trueSwitchName}");
                DeleteObjectFromController(controller, trueSwitchName);
                AddBitKeySwitch(index, layer, controller);
            }
            else
            {
                Debug.Log($"Layer BitKey Switch Validated {trueSwitchName}.");
                
                // BlendTree blendTree = layer.stateMachine.states.FirstOrDefault(s => s.state.name == trueSwitchName).state.motion as BlendTree;
                //
                // // Just re-assign since ChildMotions aren't their own ScriptableObjects.
                // ChildMotion childMotion0 = new ChildMotion
                // {
                //     motion = _clipsFalse[index],
                //     timeScale = 1
                // };
                // ChildMotion childMotion1 = new ChildMotion
                // {
                //     motion = _clipsTrue[index],
                //     timeScale = 1
                // };
                // blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};
                
                AssetDatabase.SaveAssets();;
            }
            
            if (layer.stateMachine.states.All(s => s.state.name != falseSwitchName))
            {
                Debug.Log($"Layer missing BitKeySwtich. {falseSwitchName}");
                DeleteObjectFromController(controller, falseSwitchName);
                AddBitKeySwitch(index, layer, controller);
            }
            else
            {
                Debug.Log($"Layer BitKey Switch Validated {falseSwitchName}.");
                
                // BlendTree blendTree = layer.stateMachine.states.FirstOrDefault(s => s.state.name == trueSwitchName).state.motion as BlendTree;
                //
                // // Just re-assign since ChildMotions aren't their own ScriptableObjects.
                // ChildMotion childMotion0 = new ChildMotion
                // {
                //     motion = _clipsFalse[index],
                //     timeScale = 1
                // };
                // ChildMotion childMotion1 = new ChildMotion
                // {
                //     motion = _clipsTrue[index],
                //     timeScale = 1
                // };
                // blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};
                
                AssetDatabase.SaveAssets();;
            }
        }
        
        // private void ValidateLayerBlendTree(int index, AnimatorControllerLayer layer, AnimatorController controller)
        // {
        //     string blendTreeName = string.Format(BlendTreeName, index);
        //     
        //     if (layer.stateMachine.states.All(s => s.state.name != blendTreeName))
        //     {
        //         Debug.Log($"Layer missing blend tree. {blendTreeName}");
        //         DeleteObjectFromController(controller, blendTreeName);
        //         AddBitKeySwitch(index, layer, controller);
        //     }
        //     else
        //     {
        //         Debug.Log($"Layer Blend Tree Validated {blendTreeName}.");
        //         
        //         BlendTree blendTree = layer.stateMachine.states.FirstOrDefault(s => s.state.name == blendTreeName).state.motion as BlendTree;
        //         
        //         // Just re-assign since ChildMotions aren't their own ScriptableObjects.
        //         ChildMotion childMotion0 = new ChildMotion
        //         {
        //             motion = _clipsFalse[index],
        //             timeScale = 1
        //         };
        //         ChildMotion childMotion1 = new ChildMotion
        //         {
        //             motion = _clipsTrue[index],
        //             timeScale = 1
        //         };
        //         blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};
        //         
        //         AssetDatabase.SaveAssets();;
        //     }
        // }

        private void CreateLayer(int index, AnimatorController controller)
        {
            Debug.Log($"Creating layer: {_avaCryptKeyNames[index]}");
            
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = _avaCryptKeyNames[index],
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine
                {
                    name = string.Format(StateMachineName, index)
                },
            };

            controller.AddLayer(layer);
            AssetDatabase.AddObjectToAsset(layer.stateMachine, controllerPath);
            AssetDatabase.SaveAssets();
            
            AddBitKeySwitch(index, layer, controller);
        }
        
        private void AddBitKeySwitch(int index, AnimatorControllerLayer layer, AnimatorController controller)
        {
            string trueSwitchName = string.Format(BitKeySwitchName, "True", index);
            string falseSwitchName = string.Format(BitKeySwitchName, "False", index);
            
            AnimatorState falseState = layer.stateMachine.AddState(falseSwitchName);
            falseState.motion = _clipsFalse[index];
            falseState.speed = 1;
            
            AnimatorCondition falseCondition = new AnimatorCondition
            {
                mode = AnimatorConditionMode.IfNot,
                parameter = _avaCryptKeyNames[index],
                threshold = 0
            };

            AnimatorStateTransition falseTransition = layer.stateMachine.AddAnyStateTransition(falseState);
            falseTransition.canTransitionToSelf = false;
            falseTransition.duration = 0;
            falseTransition.conditions = new[] {falseCondition};

            AnimatorState trueState = layer.stateMachine.AddState(trueSwitchName);
            trueState.motion = _clipsTrue[index];
            trueState.speed = 1;
            
            AnimatorCondition trueCondition = new AnimatorCondition
            {
                mode = AnimatorConditionMode.If,
                parameter = _avaCryptKeyNames[index],
            };
            
            AnimatorStateTransition trueTransition = layer.stateMachine.AddAnyStateTransition(trueState);
            trueTransition.canTransitionToSelf = false;
            trueTransition.duration = 0;
            trueTransition.conditions = new[] {trueCondition};
            
            AssetDatabase.SaveAssets();
        }

        // private void AddBlendTree(int index, AnimatorControllerLayer layer, AnimatorController controller)
        // {
        //     string controllerPath = AssetDatabase.GetAssetPath(controller);
        //     string blendTreeName = string.Format(BlendTreeName, index);
        //     
        //     AnimatorState state = layer.stateMachine.AddState(blendTreeName);
        //     state.speed = 1;
        //     
        //     BlendTree blendTree = new BlendTree
        //     {
        //         name = blendTreeName,
        //         blendType = BlendTreeType.Simple1D,
        //         blendParameter = _avaCryptKeyNames[index],
        //     };
        //     
        //     ChildMotion childMotion0 = new ChildMotion
        //     {
        //         motion = _clipsFalse[index],
        //         timeScale = 1
        //     };
        //     ChildMotion childMotion1 = new ChildMotion
        //     {
        //         motion = _clipsTrue[index],
        //         timeScale = 1
        //     };
        //     blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};
        //     
        //     state.motion = blendTree;
        //     AssetDatabase.AddObjectToAsset(blendTree, controllerPath);
        //     
        //     AssetDatabase.SaveAssets();
        // }
        
        public void CleanupController(AnimatorController controller)
        {
            for (int i = 0; i < _avaCryptKeyNames.Length; ++i)
            {
                DeleteObjectFromController(controller, string.Format(StateMachineName, i));
                DeleteObjectFromController(controller, string.Format(BlendTreeName, i));
                // todo make delete layers
                // DeleteObjectFromController(controller, string.Format(BitKeySwitchName, "False", i));
                // DeleteObjectFromController(controller, string.Format(BitKeySwitchName, "True", i));
            }
        }

        void DeleteObjectFromController(AnimatorController controller, string name)
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