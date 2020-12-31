using UnityEditor;
using UnityEngine;

namespace GeoTetra.ModelObfuscator
{
    public class AvaCryptRoot : MonoBehaviour
    {
        [Header("Higher value causes more distortion. Default = .02")]
        [Range(.005f, .05f)]
        [SerializeField] 
        private float _distortRatio = .02f;
        
        [Header("The four values which must be entered in game to display the model.")]
        [Range(0, 100)]
        [SerializeField] 
        private int _key0;
        
        [Range(0, 100)]
        [SerializeField] 
        private int _key1;
        
        [Range(0, 100)]
        [SerializeField] 
        private int _key2;
        
        [Range(0, 100)]
        [SerializeField] 
        private int _key3;

        #if UNITY_EDITOR
        [ContextMenu("Encrypt Avatar")]
        private void EncryptAvatar()
        {
            string newName = gameObject.name + " Encoded";
            GameObject existingEncodedGameObject = GameObject.Find(newName);
            if (existingEncodedGameObject != null) DestroyImmediate(existingEncodedGameObject);
            
            GameObject encodedGameObject = Instantiate(gameObject);
            encodedGameObject.name = encodedGameObject.name.Replace("(Clone)", "_Encrypted");
            encodedGameObject.SetActive(true);
            
            MeshFilter[] meshFilters = encodedGameObject.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshFilter.sharedMesh = EncryptMesh(meshFilter.sharedMesh);
            }
            
            SkinnedMeshRenderer[] skinnedMeshRenderers = encodedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                skinnedMeshRenderer.sharedMesh = EncryptMesh(skinnedMeshRenderer.sharedMesh);
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
            if (_key0 == 0) _key0 = Random.Range(0, 100);
            if (_key1 == 0) _key1 = Random.Range(0, 100);
            if (_key2 == 0) _key2 = Random.Range(0, 100);
            if (_key3 == 0) _key3 = Random.Range(0, 100);
        }

        private Mesh EncryptMesh(Mesh mesh)
        {
            if (mesh == null) return null;
            
            Vector3[] newVertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uv7Offsets = new Vector2[newVertices.Length];
            Vector2[] uv8Offsets = new Vector2[newVertices.Length];

            float floatKey0 = (float)_key0 * 1f;
            float floatKey1 = (float)_key1 * 2f;
            float floatKey2 = (float)_key2 * 3f;
            float floatKey3 = (float)_key3 * 4f;
            
            float comKey0 = Mathf.Sin((floatKey2 - floatKey1) * 2f) * Mathf.Cos(floatKey3 - floatKey0);
            float comKey1 = Mathf.Sin((floatKey3 - floatKey0) * 3f) * Mathf.Cos(floatKey2 - floatKey1);
            float comKey2 = Mathf.Sin((floatKey0 - floatKey3) * 4f) * Mathf.Cos(floatKey1 - floatKey2);
            float comKey3 = Mathf.Sin((floatKey1 - floatKey2) * 5f) * Mathf.Cos(floatKey0 - floatKey3);

            float maxDistance = mesh.bounds.max.magnitude - mesh.bounds.min.magnitude;

            float minRange = maxDistance * -_distortRatio;
            const float maxRange = 0;
            for (int v = 0; v < newVertices.Length; ++v)
            {
                uv7Offsets[v].x = Random.Range(minRange, maxRange);
                uv7Offsets[v].y = Random.Range(minRange, maxRange);
                
                uv8Offsets[v].x = Random.Range(minRange, maxRange);
                uv8Offsets[v].y = Random.Range(minRange, maxRange);

                newVertices[v] += normals[v] * uv7Offsets[v].x * comKey0;
                newVertices[v] += normals[v] * uv7Offsets[v].y * comKey1;
                newVertices[v] += normals[v] * uv8Offsets[v].x * comKey2;
                newVertices[v] += normals[v] * uv8Offsets[v].y * comKey3;
            }

            string existingMeshPath = AssetDatabase.GetAssetPath(mesh);
            Debug.Log($"Existing Mesh Path {existingMeshPath} {mesh}");
            string obfuscatedMeshPath = existingMeshPath.Replace(".fbx", $"_{mesh}_Encrypted.asset");
            Debug.Log($"Obfuscated Mesh Path {obfuscatedMeshPath}");

            Mesh newMesh = new Mesh
            {
                subMeshCount = mesh.subMeshCount,
                vertices = newVertices,
                colors = mesh.colors,
                normals = mesh.normals,
                tangents = mesh.tangents,
                bindposes = mesh.bindposes,
                boneWeights = mesh.boneWeights,
                uv = mesh.uv,
                uv2 = mesh.uv2,
                uv3 = mesh.uv3,
                uv4 = mesh.uv4,
                uv5 = mesh.uv5,
                uv6 = mesh.uv6,
                uv7 = uv7Offsets,
                uv8 = uv8Offsets
            };

            // transfer sub meshes
            for (int meshIndex = 0; meshIndex < mesh.subMeshCount; ++meshIndex)
            {
                int[] triangles = mesh.GetTriangles(meshIndex);
                newMesh.SetTriangles(triangles, meshIndex);
            }
            
            // transfer blend shapes
            for (int shapeIndex = 0; shapeIndex < mesh.blendShapeCount; ++shapeIndex)
            {
                for (int frameIndex = 0; frameIndex < mesh.GetBlendShapeFrameCount(shapeIndex); ++frameIndex)
                {
                    Vector3[] deltaVertices = new Vector3[newVertices.Length];
                    Vector3[] deltaNormals = new Vector3[newVertices.Length];
                    Vector3[] deltaTangents = new Vector3[newVertices.Length];
                    mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices,deltaNormals,deltaTangents);
                    float weight = mesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    string shapeName = mesh.GetBlendShapeName(shapeIndex);
                    newMesh.AddBlendShapeFrame(shapeName, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }
            
            AssetDatabase.CreateAsset(newMesh, obfuscatedMeshPath);
            AssetDatabase.SaveAssets();

            return newMesh;
        }
        #endif
    }
}
