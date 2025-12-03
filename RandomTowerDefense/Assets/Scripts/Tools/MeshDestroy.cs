using System;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Tools
{
    /// <summary>
    /// MeshDestroy - メッシュ破壊システム（プロシージャル切断とフラグメント化）
    ///
    /// 主な機能:
    /// - ランダムプレーンによるメッシュ切断アルゴリズム
    /// - カスケード破壊システム（段階的細分化）
    /// - 物理ベースの爆発力適用
    /// - UVマッピングと法線ベクトル保持
    /// - リアルタイムメッシュジオメトリ生成
    /// - GameObjectフラグメント自動生成
    /// </summary>
    public class MeshDestroy : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// デフォルトカット段階数
        /// </summary>
        private const int DEFAULT_CUT_CASCADE = 1;

        /// <summary>
        /// 境界拡張値
        /// </summary>
        private const float BOUNDS_EXPANSION = 0.5f;

        /// <summary>
        /// 三角形の頂点数
        /// </summary>
        private const int TRIANGLE_VERTEX_COUNT = 3;

        /// <summary>
        /// 三角形の全頂点が同じ側にある場合の数
        /// </summary>
        private const int ALL_VERTICES_SAME_SIDE = 3;

        /// <summary>
        /// 三角形の全頂点が異なる側にある場合の数
        /// </summary>
        private const int NO_VERTICES_SAME_SIDE = 0;

        #endregion

        #region Private Fields

        private bool _edgeSet = false;
        private Vector3 _edgeVertex = Vector3.zero;
        private Vector2 _edgeUV = Vector2.zero;
        private Plane _edgePlane = new Plane();

        /// <summary>
        /// カスケード切断回数
        /// </summary>
        public int CutCascades = DEFAULT_CUT_CASCADE;
        public float ExplodeForce = 0;

        private Mesh _originalMesh;

        #endregion

        private void Start()
        {
            _originalMesh = GetComponent<MeshFilter>().mesh;
        }
        public void DestroyMesh()
        {
            _originalMesh.RecalculateBounds();
            var parts = new List<PartMesh>();
            var subParts = new List<PartMesh>();

            var mainPart = new PartMesh()
            {
                UV = _originalMesh.uv,
                Vertices = _originalMesh.vertices,
                Normals = _originalMesh.normals,
                Triangles = new int[_originalMesh.subMeshCount][],
                Bounds = _originalMesh.bounds
            };
            for (int i = 0; i < _originalMesh.subMeshCount; ++i)
                mainPart.Triangles[i] = _originalMesh.GetTriangles(i);

            parts.Add(mainPart);

            for (var c = 0; c < CutCascades; c++)
            {
                for (var i = 0; i < parts.Count; ++i)
                {
                    var bounds = parts[i].Bounds;
                    bounds.Expand(BOUNDS_EXPANSION);

                    var plane = new Plane(UnityEngine.Random.onUnitSphere,
                        new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                         UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                        UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));


                    subParts.Add(GenerateMesh(parts[i], plane, true));
                    subParts.Add(GenerateMesh(parts[i], plane, false));
                }
                parts = new List<PartMesh>(subParts);
                subParts.Clear();
            }

            for (var i = 0; i < parts.Count; ++i)
            {
                parts[i].MakeGameobject(this, _originalMesh);
                if (parts[i].GameObject == null) continue;

                var rb = parts[i].GameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForceAtPosition(parts[i].Bounds.center * ExplodeForce, transform.position);
                }
            }

            gameObject.SetActive(false);
        }

        private PartMesh GenerateMesh(PartMesh original, Plane plane, bool left)
        {
            var partMesh = new PartMesh() { };
            var ray1 = new Ray();
            var ray2 = new Ray();


            for (var i = 0; i < original.Triangles.Length; ++i)
            {
                var triangles = original.Triangles[i];
                _edgeSet = false;

                for (var j = 0; j < triangles.Length; j = j + TRIANGLE_VERTEX_COUNT)
                {
                    var sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                    var sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                    var sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                    var sideCount = (sideA ? 1 : 0) +
                                    (sideB ? 1 : 0) +
                                    (sideC ? 1 : 0);
                    if (sideCount == NO_VERTICES_SAME_SIDE)
                    {
                        continue;
                    }
                    if (sideCount == ALL_VERTICES_SAME_SIDE)
                    {
                        partMesh.AddTriangle(i,
                                             original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]], original.Vertices[triangles[j + 2]],
                                             original.Normals[triangles[j]], original.Normals[triangles[j + 1]], original.Normals[triangles[j + 2]],
                                             original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                        continue;
                    }

                    //cut points
                    var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                    ray1.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]] - original.Vertices[triangles[j + singleIndex]];
                    ray1.direction = dir1;
                    plane.Raycast(ray1, out var enter1);
                    var lerp1 = enter1 / dir1.magnitude;

                    ray2.origin = original.Vertices[triangles[j + singleIndex]];
                    var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]] - original.Vertices[triangles[j + singleIndex]];
                    ray2.direction = dir2;
                    plane.Raycast(ray2, out var enter2);
                    var lerp2 = enter2 / dir2.magnitude;

                    //first vertex = ancor
                    AddEdge(i,
                            partMesh,
                            left ? plane.normal * -1f : plane.normal,
                            ray1.origin + ray1.direction.normalized * enter1,
                            ray2.origin + ray2.direction.normalized * enter2,
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]], lerp2));

                    if (sideCount == 1)
                    {
                        partMesh.AddTriangle(i,
                                            original.Vertices[triangles[j + singleIndex]],
                                            //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]], lerp2),
                                            ray1.origin + ray1.direction.normalized * enter1,
                                            ray2.origin + ray2.direction.normalized * enter2,
                                            original.Normals[triangles[j + singleIndex]],
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]], lerp2),
                                            original.UV[triangles[j + singleIndex]],
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]], lerp2));

                        continue;
                    }

                    if (sideCount == 2)
                    {
                        partMesh.AddTriangle(i,
                                            ray1.origin + ray1.direction.normalized * enter1,
                                            original.Vertices[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]],
                                            original.Vertices[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]],
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            original.Normals[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]],
                                            original.Normals[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]],
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            original.UV[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]],
                                            original.UV[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]]);
                        partMesh.AddTriangle(i,
                                            ray1.origin + ray1.direction.normalized * enter1,
                                            original.Vertices[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]],
                                            ray2.origin + ray2.direction.normalized * enter2,
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            original.Normals[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]],
                                            Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]], lerp2),
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % TRIANGLE_VERTEX_COUNT)]], lerp1),
                                            original.UV[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]],
                                            Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % TRIANGLE_VERTEX_COUNT)]], lerp2));
                        continue;
                    }


                }
            }

            partMesh.FillArrays();

            return partMesh;
        }

        private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
        {
            if (!_edgeSet)
            {
                _edgeSet = true;
                _edgeVertex = vertex1;
                _edgeUV = uv1;
            }
            else
            {
                _edgePlane.Set3Points(_edgeVertex, vertex1, vertex2);

                partMesh.AddTriangle(subMesh,
                                    _edgeVertex,
                                    _edgePlane.GetSide(_edgeVertex + normal) ? vertex1 : vertex2,
                                    _edgePlane.GetSide(_edgeVertex + normal) ? vertex2 : vertex1,
                                    normal,
                                    normal,
                                    normal,
                                    _edgeUV,
                                    uv1,
                                    uv2);
            }
        }

        public class PartMesh
        {
            private List<Vector3> _Verticies = new List<Vector3>();
            private List<Vector3> _Normals = new List<Vector3>();
            private List<List<int>> _Triangles = new List<List<int>>();
            private List<Vector2> _UVs = new List<Vector2>();
            public Vector3[] Vertices;
            public Vector3[] Normals;
            public int[][] Triangles;
            public Vector2[] UV;
            public GameObject GameObject;
            public Bounds Bounds = new Bounds();

            public PartMesh()
            {

            }

            public void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1, Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                if (_Triangles.Count - 1 < submesh)
                    _Triangles.Add(new List<int>());

                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert1);
                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert2);
                _Triangles[submesh].Add(_Verticies.Count);
                _Verticies.Add(vert3);
                _Normals.Add(normal1);
                _Normals.Add(normal2);
                _Normals.Add(normal3);
                _UVs.Add(uv1);
                _UVs.Add(uv2);
                _UVs.Add(uv3);

                Bounds.min = Vector3.Min(Bounds.min, vert1);
                Bounds.min = Vector3.Min(Bounds.min, vert2);
                Bounds.min = Vector3.Min(Bounds.min, vert3);
                Bounds.max = Vector3.Min(Bounds.max, vert1);
                Bounds.max = Vector3.Min(Bounds.max, vert2);
                Bounds.max = Vector3.Min(Bounds.max, vert3);
            }

            public void FillArrays()
            {
                Vertices = _Verticies.ToArray();
                Normals = _Normals.ToArray();
                UV = _UVs.ToArray();
                Triangles = new int[_Triangles.Count][];
                for (var i = 0; i < _Triangles.Count; ++i)
                    Triangles[i] = _Triangles[i].ToArray();
            }

            public void MakeGameobject(MeshDestroy original, Mesh originalMesh)
            {
                GameObject = new GameObject(original.name);
                GameObject.transform.position = original.transform.position;
                GameObject.transform.rotation = original.transform.rotation;
                GameObject.transform.localScale = original.transform.localScale;

                var mesh = new Mesh();
                mesh.name = originalMesh.name;

                mesh.vertices = Vertices;
                mesh.normals = Normals;
                mesh.uv = UV;
                for (var i = 0; i < Triangles.Length; ++i)
                    mesh.SetTriangles(Triangles[i], i, true);
                Bounds = mesh.bounds;

                var renderer = GameObject.AddComponent<MeshRenderer>();
                renderer.materials = original.GetComponent<MeshRenderer>().materials;

                var filter = GameObject.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                var collider = GameObject.AddComponent<MeshCollider>();
                collider.convex = true;

                var rigidbody = GameObject.AddComponent<Rigidbody>();
                var meshDestroy = GameObject.AddComponent<MeshDestroy>();
                meshDestroy.CutCascades = original.CutCascades;
                meshDestroy.ExplodeForce = original.ExplodeForce;

            }
        }
    }
}
