using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptRoot : MonoBehaviour
    {
        [Header("Higher value causes more distortion. Default = .02")]
        [Range(.005f, .2f)]
        [SerializeField]
        private float _distortRatio = .02f;

        [Header("The four values which must be entered in game to display the model.")]
        [Range(3, 100)]
        [SerializeField]
        private int _key0;

        [Range(3, 100)]
        [SerializeField]
        private int _key1;

        [Range(3, 100)]
        [SerializeField]
        private int _key2;

        [Range(3, 100)]
        [SerializeField]
        private int _key3;

#if UNITY_EDITOR
        private readonly AvaCryptController _avaCryptController = new AvaCryptController();
        private readonly AvaCryptMesh _avaCryptMesh = new AvaCryptMesh();

        public void ValidateAnimatorController()
        {
            AnimatorController controller = GetAnimatorController();

            _avaCryptController.ValidateAnimations(gameObject, controller);
            _avaCryptController.ValidateParameters(controller);
            _avaCryptController.ValidateLayers(controller);
        }

        private AnimatorController GetAnimatorController()
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
            if (runtimeController == null)
            {
                EditorUtility.DisplayDialog("Animator has no AnimatorController.",
                    "Add an AnimatorController to the Animator component.",
                    "Ok");
                return null;
            }

            AnimatorController controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(UnityEditor.AssetDatabase.GetAssetPath(runtimeController));
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

            // Disable old for convienence.
            gameObject.SetActive(false);

            string newName = gameObject.name + "_Encrypted";

            // delete old GO, do as such in case its disabled
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] sceneRoots = scene.GetRootGameObjects();

            foreach (GameObject oldGameObject in sceneRoots)
            {
                if (oldGameObject.name == newName)
                {
                    DestroyImmediate(oldGameObject);
                }
            }

            GameObject encodedGameObject = Instantiate(gameObject);
            encodedGameObject.name = newName;
            encodedGameObject.SetActive(true);

            MeshFilter[] meshFilters = encodedGameObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.GetComponent<MeshRenderer>() != null)
                {
                    var Materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;

                    foreach (var Material in Materials)
                    {
                        if (Material.HasProperty("_Key0"))
                        {
                            meshFilter.sharedMesh = _avaCryptMesh.EncryptMesh(meshFilter.sharedMesh, _key0, _key1, _key2, _key3, _distortRatio);
                            break;
                        }
                    }
                }
            }

            SkinnedMeshRenderer[] skinnedMeshRenderers = encodedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var Materials = skinnedMeshRenderer.sharedMaterials;

                foreach (var Material in Materials)
                {
                    if (Material.HasProperty("_Key0"))
                    {
                        skinnedMeshRenderer.sharedMesh = _avaCryptMesh.EncryptMesh(skinnedMeshRenderer.sharedMesh, _key0, _key1, _key2, _key3, _distortRatio);
                        break;
                    }
                }
            }

            AvaCryptRoot[] avaCryptRoots = encodedGameObject.GetComponentsInChildren<AvaCryptRoot>(true);
            foreach (AvaCryptRoot avaCryptRoot in avaCryptRoots)
            {
                DestroyImmediate(avaCryptRoot);
            }
        }

        private void Reset()
        {
            // Start at 3 because 0 is kept to show unencrypted avatars normally.
            if (_key0 == 0)
            {
                _key0 = Random.Range(3, 100);
            }

            if (_key1 == 0)
            {
                _key1 = Random.Range(3, 100);
            }

            if (_key2 == 0)
            {
                _key2 = Random.Range(3, 100);
            }

            if (_key3 == 0)
            {
                _key3 = Random.Range(3, 100);
            }
        }

        private void OnValidate()
        {
            _key0 = RoundToThree(_key0);
            _key1 = RoundToThree(_key1);
            _key2 = RoundToThree(_key2);
            _key3 = RoundToThree(_key3);

            _key0 = Skip76(_key0);
            _key1 = Skip76(_key1);
            _key2 = Skip76(_key2);
            _key3 = Skip76(_key3);
        }

        private int RoundToThree(int value)
        {
            return (value / 3) * 3 + 1;
        }

        /// <summary>
        /// This is super specific to current version of VRC.
        /// There is a big which doesn't let you select 76 in radial menu, so skip it.
        /// </summary>
        private int Skip76(int value)
        {
            if (value == 76)
            {
                return value -= 3;
            }

            return value;
        }
#endif
    }
}
