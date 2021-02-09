#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptMesh
    {
        public Mesh EncryptMesh(Mesh mesh, float key0, float key1, float key2, float key3, float distortRatio)
        {
            if (mesh == null || !mesh.isReadable)
            {
                return null;
            }

            var newVertices = mesh.vertices;
            var normals = mesh.normals;

            var uv7Offsets = new Vector2[newVertices.Length];
            var uv8Offsets = new Vector2[newVertices.Length];

            var floatKey0 = key0 * 1f;
            var floatKey1 = key1 * 2f;
            var floatKey2 = key2 * 3f;
            var floatKey3 = key3 * 4f;

            var comKey0 = Mathf.Sin((floatKey2 - floatKey1) * 2f) * Mathf.Cos(floatKey3 - floatKey0);
            var comKey1 = Mathf.Sin((floatKey3 - floatKey0) * 3f) * Mathf.Cos(floatKey2 - floatKey1);
            var comKey2 = Mathf.Sin((floatKey0 - floatKey3) * 4f) * Mathf.Cos(floatKey1 - floatKey2);
            var comKey3 = Mathf.Sin((floatKey1 - floatKey2) * 5f) * Mathf.Cos(floatKey0 - floatKey3);

            var maxDistance = mesh.bounds.max.magnitude - mesh.bounds.min.magnitude;

            var minRange = maxDistance * -distortRatio;
            const float maxRange = 0;

            for (var v = 0; v < newVertices.Length; v++)
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

            var existingMeshPath = AssetDatabase.GetAssetPath(mesh);

            if (string.IsNullOrEmpty(existingMeshPath) || existingMeshPath.Contains("unity default resources"))
            {
                Debug.LogError("Asset For Mesh Not Found, Invalid Or Is A Built In Unity Mesh!");
                return null;
            }

            Debug.Log($"Existing Mesh Path For {mesh.name} Is {existingMeshPath}");

            //Do Not Care What File Type The Mesh Is, Attempt Anyway.
            //The Inline If Statement Is A Fallback Check, It Gets The Path Combined With The Filename Without Extension With Our Own Extension, If The Path Is Null, It Would Then Use Enviroment.CurrentDirectory Via Inheritance As The Path.
            var encryptedMeshPath = Path.GetDirectoryName(existingMeshPath) != null ? (Path.Combine(Path.GetDirectoryName(existingMeshPath), Path.GetFileNameWithoutExtension(existingMeshPath)) + $"_{mesh.name}_Encrypted.asset") : (Path.GetFileNameWithoutExtension(existingMeshPath) +$"_{mesh.name}_Encrypted.asset");

            Debug.Log($"Encrypted Mesh Path {encryptedMeshPath}");

            var newMesh = new Mesh
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
            for (var meshIndex = 0; meshIndex < mesh.subMeshCount; meshIndex++)
            {
                var triangles = mesh.GetTriangles(meshIndex);

                newMesh.SetTriangles(triangles, meshIndex);
            }

            // transfer blend shapes
            for (int shapeIndex = 0; shapeIndex < mesh.blendShapeCount; shapeIndex++)
            {
                for (var frameIndex = 0; frameIndex < mesh.GetBlendShapeFrameCount(shapeIndex); frameIndex++)
                {
                    var deltaVertices = new Vector3[newVertices.Length];
                    var deltaNormals = new Vector3[newVertices.Length];
                    var deltaTangents = new Vector3[newVertices.Length];

                    mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    var weight = mesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    var shapeName = mesh.GetBlendShapeName(shapeIndex);

                    newMesh.AddBlendShapeFrame(shapeName, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            AssetDatabase.CreateAsset(newMesh, encryptedMeshPath);
            AssetDatabase.SaveAssets();

            return newMesh;
        }
    }
}
#endif
