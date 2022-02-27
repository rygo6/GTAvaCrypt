#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GeoTetra.GTAvaCrypt
{
    public class AvaCryptMesh
    {
        private int[] _sign0;
        private int[] _keySign0;
        private int[] _randomKeyIndex;
        // private int[] _keyIndex1;
        // private int[] _keyIndex0;
        private int[] _sign1;
        // private int[] _keyIndex2;
        private int[] _keySign1;
        // private int[] _keyIndex3;
        private float[] _randomDivideMultiplier;
        private float[] _randomKeyMultiplier;

        const int DivideCount = 4;
        
        void Shuffle<T>(IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void InitializeRandoms(int count)
        {
            int divideCount = count / DivideCount;
            _sign0 = new int[divideCount];

            List<int>  randomKeyIndexList = new List<int>();
            
            // _keyIndex0 = new int[divideCount];
            _keySign0 = new int[divideCount];
            // _keyIndex1 = new int[divideCount];
            _sign1 = new int[divideCount];
            // _keyIndex2 = new int[divideCount];
            _keySign1 = new int[divideCount];
            // _keyIndex3 = new int[divideCount];
            _randomDivideMultiplier = new float[divideCount];
            _randomKeyMultiplier = new float[count];

            for (int i = 0; i < divideCount; ++i)
            {
                _sign0[i] = Random.Range(0, 2);
                // _keyIndex0[i] = Random.Range(0, divideCount);
                _keySign0[i] = Random.Range(0, 2);
                // _keyIndex1[i] = Random.Range(0, divideCount);
                _sign1[i] = Random.Range(0, 2);
                // _keyIndex2[i] = Random.Range(0, divideCount);
                _keySign1[i] = Random.Range(0, 2);
                // _keyIndex3[i] = Random.Range(0, divideCount);

                randomKeyIndexList.Add(i);
                randomKeyIndexList.Add(i);
                randomKeyIndexList.Add(i);
                randomKeyIndexList.Add(i);
                
                _randomDivideMultiplier[i] = Random.Range(0f, 2f);
            }
            
            Shuffle(randomKeyIndexList);
            _randomKeyIndex = randomKeyIndexList.ToArray();

            for (int i = 0; i < count; ++i)
            {
                _randomKeyMultiplier[i] = Random.Range(0f, 2f);
            }
        }
        
        public Mesh EncryptMesh(Mesh mesh, float distortRatio, bool[] keys)
        {
            if (mesh == null) return null;
            
            int divideCount = keys.Length / DivideCount;

            Vector3[] newVertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uv7Offsets = new Vector2[mesh.vertexCount];
            Vector2[] uv8Offsets = new Vector2[mesh.vertexCount];

            float[] decodeKeys = new float[divideCount];

            for (int i = 0; i < divideCount; ++i)
            {
                decodeKeys[i] = (Convert.ToSingle(keys[i*DivideCount]) + _randomKeyMultiplier[i*DivideCount]) *
                                (Convert.ToSingle(keys[i*DivideCount+1]) + _randomKeyMultiplier[i*DivideCount+1]) *
                                (Convert.ToSingle(keys[i*DivideCount+2]) + _randomKeyMultiplier[i*DivideCount+2]) *
                                (Convert.ToSingle(keys[i*DivideCount+3]) + _randomKeyMultiplier[i*DivideCount+3]);
                Debug.Log("decodeKey: " + decodeKeys[i]);
            }

            StringBuilder sb0 = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            float[] comKey = new float[divideCount];
            for (int i = 0; i < divideCount; ++i)
            {
                float firstAdd = _keySign0[i] > 0
                    ? decodeKeys[_randomKeyIndex[i*DivideCount]] - decodeKeys[_randomKeyIndex[i*DivideCount+1]]
                    : decodeKeys[_randomKeyIndex[i*DivideCount]] + decodeKeys[_randomKeyIndex[i*DivideCount+1]];
                float firstSign = _sign0[i] > 0 ? Mathf.Sin(firstAdd) : Mathf.Cos(firstAdd);

                float secondAdd = _keySign1[i] > 0
                    ? decodeKeys[_randomKeyIndex[i*DivideCount+2]] - decodeKeys[_randomKeyIndex[i*DivideCount+3]]
                    : decodeKeys[_randomKeyIndex[i*DivideCount+2]] + decodeKeys[_randomKeyIndex[i*DivideCount+3]];
                float secondSign = _sign1[i] > 0 ? Mathf.Sin(secondAdd) : Mathf.Cos(secondAdd);

                comKey[i] = firstSign * _randomDivideMultiplier[i] * secondSign;

                string firstAddStr = _keySign0[i] > 0 ? "-" : "+";
                string firstSignStr = _sign0[i] > 0 ? "sin" : "cos";
                string secondAddStr = _keySign1[i] > 0 ? "-" : "+";
                string secondSignStr = _sign1[i] > 0 ? "sin" : "cos";

                sb0.AppendLine($"float decodeKey{i} = (_BitKey{i*DivideCount} + {_randomKeyMultiplier[i*DivideCount]}) * (_BitKey{i*DivideCount+1} + {_randomKeyMultiplier[i*DivideCount+1]}) * (_BitKey{i*DivideCount+2} + {_randomKeyMultiplier[i*DivideCount+2]}) * (_BitKey{i*DivideCount+3} + {_randomKeyMultiplier[i*DivideCount+3]});");
                sb1.AppendLine($"float comKey{i} = {firstSignStr}(decodeKey{_randomKeyIndex[i*DivideCount]} {firstAddStr} decodeKey{_randomKeyIndex[i*DivideCount+1]}) * {_randomDivideMultiplier[i]} * {secondSignStr}(decodeKey{_randomKeyIndex[i*DivideCount+2]} {secondAddStr} decodeKey{_randomKeyIndex[i*DivideCount+3]});");
            }

            var searchResult = AssetDatabase.FindAssets("CGI_GTModelDecode");
            if (searchResult.Length == 0)
            {
                Debug.LogError("CGI_GTModelDecode.cginc not found!");
            }
            else
            {
                foreach (string sr in searchResult)
                {
                    Debug.Log($"Writing GTModelDecode {sr}");
                    System.IO.File.WriteAllText(AssetDatabase.GUIDToAssetPath(sr), $"{ModelShaderDecodeFirst}{sb0.ToString()}{sb1.ToString()}{ModelShaderDecodeSecond}");
                }
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
                newVertices[v] += normals[v] * (uv7Offsets[v].y * comKey[3]);
                
                newVertices[v] += normals[v] * (uv8Offsets[v].y * comKey[4]);
                newVertices[v] += normals[v] * (uv8Offsets[v].x * comKey[5]);
                newVertices[v] += normals[v] * (uv8Offsets[v].y * comKey[6]);
                newVertices[v] += normals[v] * (uv8Offsets[v].x * comKey[7]);
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
        
        const string ModelShaderDecodeFirst =
@"
float _EnableAvaCrypt;

float _BitKey0;
float _BitKey1;
float _BitKey2;
float _BitKey3;

float _BitKey4;
float _BitKey5;
float _BitKey6;
float _BitKey7;

float _BitKey8;
float _BitKey9;
float _BitKey10;
float _BitKey11;

float _BitKey12;
float _BitKey13;
float _BitKey14;
float _BitKey15;

float _BitKey16;
float _BitKey17;
float _BitKey18;
float _BitKey19;

float _BitKey20;
float _BitKey21;
float _BitKey22;
float _BitKey23;

float _BitKey24;
float _BitKey25;
float _BitKey26;
float _BitKey27;

float _BitKey28;
float _BitKey29;
float _BitKey30;
float _BitKey31;

float4 modelDecode(float4 vertex, float3 normal, float2 uv0, float2 uv1)
{
    if (!_EnableAvaCrypt) return vertex;

    // AvaCrypt Randomly Generated Begin
";

        const string ModelShaderDecodeSecond =
@"  
    // AvaCrypt Randomly Generated End

    vertex.xyz -= normal * (uv0.x * comKey0);
    vertex.xyz -= normal * (uv0.y * comKey1);
    vertex.xyz -= normal * (uv0.x * comKey2);
    vertex.xyz -= normal * (uv0.y * comKey3);

    vertex.xyz -= normal * (uv1.y * comKey4);
    vertex.xyz -= normal * (uv1.x * comKey5);
    vertex.xyz -= normal * (uv1.y * comKey6);
    vertex.xyz -= normal * (uv1.x * comKey7);

    return vertex;
}
";
    }
}
#endif