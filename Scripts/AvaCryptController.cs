#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
using BlendTree = UnityEditor.Animations.BlendTree;

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptController
    {
        private readonly string[] AvaCryptKeyNames = {"AvaCryptKey0", "AvaCryptKey1", "AvaCryptKey2", "AvaCryptKey3"};
        private readonly AnimationClip[] _clips0 = new AnimationClip[4];
        private readonly AnimationClip[] _clips100 = new AnimationClip[4];
        
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
        }

        private void ValidateClip(GameObject gameObject, AnimatorController controller, int index)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string controllerFileName = System.IO.Path.GetFileName(controllerPath);
            
            string clipName = $"{gameObject.name}_{AvaCryptKeyNames[index]}";
            string clipName0 = $"{clipName}_0.anim";
            string clipName100 = $"{clipName}_100.anim";
            
            if (controller.animationClips.All(c => c.name != clipName0))
            {
                _clips0[index] = new AnimationClip()
                {
                    name = clipName0
                };
                string clip0Path = controllerPath.Replace(controllerFileName, clipName0);
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
                string clip100Path = controllerPath.Replace(controllerFileName, clipName100);
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
                if (controller.layers.All(layer => layer.name != AvaCryptKeyNames[i]))
                {
                    AnimatorControllerLayer layer0 = CreateLayer(i);
                    controller.AddLayer(layer0);
                    Debug.Log($"Adding layer: {AvaCryptKeyNames[i]}");
                }
                else
                {
                    Debug.Log($"Layer already added: {AvaCryptKeyNames[i]}");
                }
            }
        }

        private AnimatorControllerLayer CreateLayer(int index)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = AvaCryptKeyNames[index],
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine(),
            };

            AnimatorState state = layer.stateMachine.AddState("Blend Tree");

            BlendTree blendTree = new BlendTree
            {
                name = "Blend Tree",
                blendType = BlendTreeType.Simple1D,
                blendParameter = AvaCryptKeyNames[index],
            };

            ChildMotion childMotion0 = new ChildMotion
            {
                motion = _clips0[index]
            };
            ChildMotion childMotion1 = new ChildMotion
            {
                motion = _clips100[index]
            };
            blendTree.children = new ChildMotion[2] {childMotion0, childMotion1};

            state.motion = blendTree;

            return layer;
        }
    }
}
#endif