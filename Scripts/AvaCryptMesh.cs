#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptMesh
    {
        private int[] _sign0;
        private int[] _keySign0;
        private int[] _keyIndex1;
        private int[] _keyIndex0;
        private int[] _sign1;
        private int[] _keyIndex2;
        private int[] _keySign1;
        private int[] _keyIndex3;
        private float[] _randomMultiplier;

        public void InitializeRandoms(int count)
        {
            _sign0 = new int[count];
            _keyIndex0 = new int[count];
            _keySign0 = new int[count];
            _keyIndex1 = new int[count];
            _sign1 = new int[count];
            _keyIndex2 = new int[count];
            _keySign1 = new int[count];
            _keyIndex3 = new int[count];
            _randomMultiplier = new float[count];

            for (int i = 0; i < count; ++i)
            {
                _sign0[i] = Random.Range(0, 2);
                _keyIndex0[i] = Random.Range(0, count);
                _keySign0[i] = Random.Range(0, 2);
                _keyIndex1[i] = Random.Range(0, count);
                _sign1[i] = Random.Range(0, 2);
                _keyIndex2[i] = Random.Range(0, count);
                _keySign1[i] = Random.Range(0, 2);
                _keyIndex3[i] = Random.Range(0, count);

                _randomMultiplier[i] = Random.Range(0f, 2f);
            }
        }

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
                floatKeys[i] = (float) keys[i] * _randomMultiplier[i];
            }

            StringBuilder sb0 = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            float[] comKey = new float[keys.Length];
            for (int i = 0; i < keys.Length; ++i)
            {
                float firstAdd = _keySign0[i] > 0
                    ? floatKeys[_keyIndex0[i]] - floatKeys[_keyIndex1[i]]
                    : floatKeys[_keyIndex0[i]] + floatKeys[_keyIndex1[i]];
                float firstSign = _sign0[i] > 0 ? Mathf.Sin(firstAdd) : Mathf.Cos(firstAdd);

                float secondAdd = _keySign1[i] > 0
                    ? floatKeys[_keyIndex2[i]] - floatKeys[_keyIndex3[i]]
                    : floatKeys[_keyIndex2[i]] + floatKeys[_keyIndex3[i]];
                float secondSign = _sign1[i] > 0 ? Mathf.Sin(secondAdd) : Mathf.Cos(secondAdd);

                comKey[i] = firstSign * _randomMultiplier[i] * secondSign;

                string firstAddStr = _keySign0[i] > 0 ? "-" : "+";
                string firstSignStr = _sign0[i] > 0 ? "sin" : "cos";
                string secondAddStr = _keySign1[i] > 0 ? "-" : "+";
                string secondSignStr = _sign1[i] > 0 ? "sin" : "cos";

                sb0.AppendLine($"key{i} *= {_randomMultiplier[i]};");
                sb1.AppendLine(
                    $"float comKey{i} = {firstSignStr}(key{_keyIndex0[i]} {firstAddStr} key{_keyIndex1[i]}) * {_randomMultiplier[i]} * {secondSignStr}(key{_keyIndex2[i]} {secondAddStr} key{_keyIndex3[i]});");
            }

            var searchResult = AssetDatabase.FindAssets("CGI_GTModelDecode");
            if (searchResult.Length == 0)
            {
                Debug.LogError("CGI_GTModelDecode.cginc not found!");
            }
            else
            {
                System.IO.File.WriteAllText(AssetDatabase.GUIDToAssetPath(searchResult[0]), $"{ModelShaderDecodeFirst}{sb0.ToString()}{sb1.ToString()}{ModelShaderDecodeSecond}");
            }
            
            float maxDistance = mesh.bounds.max.magnitude - mesh.bounds.min.magnitude;

            float minRange = maxDistance * -distortRatio;
            const float maxRange = 0;
            for (int v = 0; v < newVertices.Length; ++v)
            {
                uv7Offsets[v].x = Random.Range(minRange, maxRange);
                uv7Offsets[v].y = Random.Range(minRange, maxRange);

                uv8Offsets[v].x = Random.Range(minRange, maxRange);
                uv8Offsets[v].y = Random.Range(minRange, maxRange);

                newVertices[v] += normals[v] * (uv7Offsets[v].x * comKey[0]);
                newVertices[v] += normals[v] * (uv7Offsets[v].y * comKey[1]);
                newVertices[v] += normals[v] * (uv7Offsets[v].x * comKey[2]);
                newVertices[v] += normals[v] * (uv8Offsets[v].y * comKey[3]);
                newVertices[v] += normals[v] * (uv8Offsets[v].x * comKey[4]);
                newVertices[v] += normals[v] * (uv8Offsets[v].y * comKey[5]);
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
                    mesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                    float weight = mesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    string shapeName = mesh.GetBlendShapeName(shapeIndex);
                    newMesh.AddBlendShapeFrame(shapeName, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            AssetDatabase.CreateAsset(newMesh, obfuscatedMeshPath);
            AssetDatabase.SaveAssets();

            return newMesh;
        }

        private const string ModelShaderDecodeFirst =
            @"float _Key0;
            float _Key1;
            float _Key2;
            float _Key3;
            float _Key4;
            float _Key5;

            float4 modelDecode(float4 vertex, float3 normal, float2 uv0, float2 uv1)
            {
                // if key is 0 don't apply
                if (_Key0 == 0)
                {
                    return vertex;
                }

                // add .5 to fix odd offset from vrc parameter sync
                float key0 = ((int)(_Key0 + .5));
                float key1 = ((int)(_Key1 + .5));
                float key2 = ((int)(_Key2 + .5));
                float key3 = ((int)(_Key3 + .5));
                float key4 = ((int)(_Key4 + .5));
                float key5 = ((int)(_Key5 + .5));

                // round to three to minimize potential of parameter off sync
                key0 = ((int)(key0 / 3)) * 3 + 1;
                key1 = ((int)(key1 / 3)) * 3 + 1;
                key2 = ((int)(key2 / 3)) * 3 + 1;
                key3 = ((int)(key3 / 3)) * 3 + 1;
                key4 = ((int)(key4 / 3)) * 3 + 1;
                key5 = ((int)(key5 / 3)) * 3 + 1;

                // AvaCrypt Randomly Generated Begin:
                ";
        private const string ModelShaderDecodeSecond =
            @"  // AvaCrypt Randomly Generated End:
               
                vertex.xyz -= normal * (uv0.x * comKey0);
                vertex.xyz -= normal * (uv0.y * comKey1);
                vertex.xyz -= normal * (uv0.x * comKey2);
                vertex.xyz -= normal * (uv1.y * comKey3);
                vertex.xyz -= normal * (uv1.x * comKey4);
                vertex.xyz -= normal * (uv1.y * comKey5);
                
                return vertex;
            }";
    }
}
#endif