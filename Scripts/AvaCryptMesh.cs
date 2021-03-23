#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptMesh
    {
        public Mesh EncryptMesh(Mesh mesh, float distortRatio, int[] keys)
        {
            if (mesh == null) return null;
            
            Vector3[] newVertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uv7Offsets = new Vector2[newVertices.Length];
            Vector2[] uv8Offsets = new Vector2[newVertices.Length];

            float[] floatKeys = new float[keys.Length];

            for (int i = 0; i < keys.Length; ++i)
            {
                floatKeys[i] = (float)keys[i] * ((float)i + 1f);
            }
            
            float comKey0 = Mathf.Sin((floatKeys[0] - floatKeys[5]) * 1.5f) * Mathf.Cos(floatKeys[2] - floatKeys[3]);
            float comKey1 = Mathf.Sin((floatKeys[1] - floatKeys[4]) * 2.0f) * Mathf.Cos(floatKeys[1] - floatKeys[4]);
            float comKey2 = Mathf.Sin((floatKeys[2] - floatKeys[3]) * 2.5f) * Mathf.Cos(floatKeys[0] - floatKeys[5]);
            float comKey3 = Mathf.Sin((floatKeys[3] - floatKeys[2]) * 3.0f) * Mathf.Cos(floatKeys[5] - floatKeys[0]);
            float comKey4 = Mathf.Sin((floatKeys[4] - floatKeys[1]) * 3.5f) * Mathf.Cos(floatKeys[4] - floatKeys[1]);
            float comKey5 = Mathf.Sin((floatKeys[5] - floatKeys[0]) * 4.0f) * Mathf.Cos(floatKeys[3] - floatKeys[2]);

            float maxDistance = mesh.bounds.max.magnitude - mesh.bounds.min.magnitude;

            float minRange = maxDistance * -distortRatio;
            const float maxRange = 0;
            for (int v = 0; v < newVertices.Length; ++v)
            {
                uv7Offsets[v].x = Random.Range(minRange, maxRange);
                uv7Offsets[v].y = Random.Range(minRange, maxRange);
                
                uv8Offsets[v].x = Random.Range(minRange, maxRange);
                uv8Offsets[v].y = Random.Range(minRange, maxRange);

                newVertices[v] += normals[v] * uv7Offsets[v].x * comKey0;
                newVertices[v] += normals[v] * uv7Offsets[v].y * comKey1;
                newVertices[v] += normals[v] * uv7Offsets[v].x * comKey2;
                newVertices[v] += normals[v] * uv8Offsets[v].y * comKey3;
                newVertices[v] += normals[v] * uv8Offsets[v].x * comKey4;
                newVertices[v] += normals[v] * uv8Offsets[v].y * comKey5;
            }

            string existingMeshPath = AssetDatabase.GetAssetPath(mesh);
            string obfuscatedMeshPath = null;
            Debug.Log($"Existing Mesh Path {existingMeshPath} {mesh}");
            if (existingMeshPath.Contains(".fbx"))
            {
                obfuscatedMeshPath = existingMeshPath.Replace(".fbx", $"_{mesh}_Encrypted.asset");
            }
            else if (existingMeshPath.Contains(".asset"))
            {
                obfuscatedMeshPath = existingMeshPath.Replace(".asset", $"_{mesh}_Encrypted.asset");
            }
            else if (existingMeshPath.Contains(".obj"))
            {
                obfuscatedMeshPath = existingMeshPath.Replace(".obj", $"_{mesh}_Encrypted.asset");
            }
            else
            {
                Debug.LogWarning("Mesh file type not known?");
                return null;
            }

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
    }
}
#endif