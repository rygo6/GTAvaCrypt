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
        private float _distortRatio = .04f;

        [Range(0, 100)] 
        [SerializeField] 
        private int[] _keys = new int[6];

        #if UNITY_EDITOR
        private readonly AvaCryptController _avaCryptController = new AvaCryptController();
        private readonly AvaCryptMesh _avaCryptMesh = new AvaCryptMesh();
        
        public void ValidateAnimatorController()
        {
            AnimatorController controller = GetAnimatorController();

            _avaCryptController.InitializeCount(_keys.Length);
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
            if(runtimeController == null)
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
            
            MeshFilter[] meshFilters = encodedGameObject.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshFilter.sharedMesh = _avaCryptMesh.EncryptMesh(meshFilter.sharedMesh, _distortRatio, _keys);
            }
            
            SkinnedMeshRenderer[] skinnedMeshRenderers = encodedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                skinnedMeshRenderer.sharedMesh = _avaCryptMesh.EncryptMesh(skinnedMeshRenderer.sharedMesh, _distortRatio, _keys);
            }
            
            AvaCryptRoot[] avaCryptRoots = encodedGameObject.GetComponentsInChildren<AvaCryptRoot>();
            foreach (AvaCryptRoot avaCryptRoot in avaCryptRoots)
            {
                DestroyImmediate(avaCryptRoot);
            }
            
            // Disable old for convienence.
            gameObject.SetActive(false);
        }

        private void Reset()
        {
            // Start at 3 because 0 is kept to show unencrypted avatars normally.
            for (int i = 0; i < _keys.Length; ++i)
            {
                if (_keys[i] == 0) _keys[i] = Random.Range(3, 100);
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < _keys.Length; ++i)
            {
                _keys[i] = RoundToThree(_keys[i]);
                _keys[i] = Skip76(_keys[i]);
            }
        }

        private int RoundToThree(int value)
        {
            // allow 0 someone can disable a key
            if (value == 0) return 0;
            if (value < 4) return 4;
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
