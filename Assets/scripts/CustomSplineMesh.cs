#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
#endif

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Interpolators = UnityEngine.Splines.Interpolators;

namespace Unity.Splines.Examples
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer), typeof(MeshRenderer), typeof(MeshFilter))]
    public class CustomSplineMesh : MonoBehaviour
    {
        [SerializeField] List<SplineData<float>> m_Widths = new List<SplineData<float>>();
        public float roadWidth = 1f;
        public bool roundCaps = true;
        [SerializeField] Vector3 capRotationOffset;

        public List<SplineData<float>> Widths
        {
            get
            {
                foreach (var width in m_Widths)
                {
                    if (width.DefaultValue == 0)
                        width.DefaultValue = 1f;
                }

                return m_Widths;
            }
        }

        [SerializeField] SplineContainer m_Spline;

        public SplineContainer Container
        {
            get
            {
                if (m_Spline == null)
                    m_Spline = GetComponent<SplineContainer>();

                return m_Spline;
            }
            set => m_Spline = value;
        }

        [SerializeField] int m_SegmentsPerMeter = 1;

        [SerializeField] Mesh m_Mesh;

        [SerializeField] float m_TextureScale = 1f;

        public IReadOnlyList<Spline> splines => LoftSplines;

        public IReadOnlyList<Spline> LoftSplines
        {
            get
            {
                if (m_Spline == null)
                    m_Spline = GetComponent<SplineContainer>();

                if (m_Spline == null)
                {
                    Debug.LogError("Cannot loft road mesh because Spline reference is null");
                    return null;
                }

                return m_Spline.Splines;
            }
        }

        [Obsolete("Use LoftMesh instead.", false)]
        public Mesh mesh => LoftMesh;

        public Mesh LoftMesh
        {
            get
            {
                if (m_Mesh != null)
                    return m_Mesh;

                m_Mesh = new Mesh();
                GetComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Road");
                return m_Mesh;
            }
        }

        [Obsolete("Use SegmentsPerMeter instead.", false)]
        public int segmentsPerMeter => SegmentsPerMeter;

        public int SegmentsPerMeter => Mathf.Min(10, Mathf.Max(1, m_SegmentsPerMeter));

        List<Vector3> m_Positions = new List<Vector3>();
        List<Vector3> m_Normals = new List<Vector3>();
        List<Vector2> m_Textures = new List<Vector2>();
        List<int> m_Indices = new List<int>();

        public void OnEnable()
        {
            // Avoid to point to an existing instance when duplicating the GameObject
            if (m_Mesh != null)
                m_Mesh = null;

            if (m_Spline == null)
                m_Spline = GetComponent<SplineContainer>();

            LoftAllRoads();

#if UNITY_EDITOR
            EditorSplineUtility.AfterSplineWasModified += OnAfterSplineWasModified;
            EditorSplineUtility.RegisterSplineDataChanged<float>(OnAfterSplineDataWasModified);
            Undo.undoRedoPerformed += LoftAllRoads;
#endif

            SplineContainer.SplineAdded += OnSplineContainerAdded;
            SplineContainer.SplineRemoved += OnSplineContainerRemoved;
            SplineContainer.SplineReordered += OnSplineContainerReordered;
            Spline.Changed += OnSplineChanged;
        }

        public void OnDisable()
        {
#if UNITY_EDITOR
            EditorSplineUtility.AfterSplineWasModified -= OnAfterSplineWasModified;
            EditorSplineUtility.UnregisterSplineDataChanged<float>(OnAfterSplineDataWasModified);
            Undo.undoRedoPerformed -= LoftAllRoads;
#endif

            if (m_Mesh != null)
#if UNITY_EDITOR
                DestroyImmediate(m_Mesh);
#else
                Destroy(m_Mesh);
#endif

            SplineContainer.SplineAdded -= OnSplineContainerAdded;
            SplineContainer.SplineRemoved -= OnSplineContainerRemoved;
            SplineContainer.SplineReordered -= OnSplineContainerReordered;
            Spline.Changed -= OnSplineChanged;
        }

        void OnSplineContainerAdded(SplineContainer container, int index)
        {
            if (container != m_Spline)
                return;

            if (m_Widths.Count < LoftSplines.Count)
            {
                var delta = LoftSplines.Count - m_Widths.Count;
                for (var i = 0; i < delta; i++)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(this, "Modifying Widths SplineData");
#endif
                    m_Widths.Add(new SplineData<float>() { DefaultValue = 1f });
                }
            }

            LoftAllRoads();
        }

        void OnSplineContainerRemoved(SplineContainer container, int index)
        {
            if (container != m_Spline)
                return;

            if (index < m_Widths.Count)
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Modifying Widths SplineData");
#endif
                m_Widths.RemoveAt(index);
            }

            LoftAllRoads();
        }

        void OnSplineContainerReordered(SplineContainer container, int previousIndex, int newIndex)
        {
            if (container != m_Spline)
                return;

            LoftAllRoads();
        }

        void OnAfterSplineWasModified(Spline s)
        {
            if (LoftSplines == null)
                return;

            foreach (var spline in LoftSplines)
            {
                if (s == spline)
                {
                    LoftAllRoads();
                    break;
                }
            }
        }

        void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            OnAfterSplineWasModified(spline);
        }

        void OnAfterSplineDataWasModified(SplineData<float> splineData)
        {
            foreach (var width in m_Widths)
            {
                if (splineData == width)
                {
                    LoftAllRoads();
                    break;
                }
            }
        }

        void AddRoundedCaps(Spline spline, float width, Quaternion capRotation)
        {
            SplineUtility.Evaluate(spline, 0f, out var startPos, out var startDir, out var startUp);
            SplineUtility.Evaluate(spline, 1f, out var endPos, out var endDir, out var endUp);

            var startUpVector = spline.EvaluateUpVector(0f);
            var endUpVector = spline.EvaluateUpVector(1f);


            int segments = 8;
            float angleStep = Mathf.PI / segments;

            // Use a consistent up vector for both caps
            Vector3 consistentUp = Vector3.up;

            // Add start cap
            AddCircularCap(startPos, -startDir, startUpVector, width, segments, angleStep, false);

            // Add end cap
            AddCircularCap(endPos, endDir, endUpVector, width, segments, angleStep, false);
        }

        void AddCircularCap(Vector3 centerPos, Vector3 direction, Vector3 up, float width, int segments,
            float angleStep, bool isStartCap)
        {
            Vector3 right = Vector3.Cross(up, direction.normalized).normalized;
            Vector3 normalizedDirection = direction.normalized;
            var prevVertCount = m_Positions.Count;

            // Center vertex
            m_Positions.Add(centerPos);
            m_Normals.Add(up);
            m_Textures.Add(new Vector2(0.5f, 1f)); //isStartCap ? 0f : 1f));

            // Create vertices around the cap
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector3 offset = right * Mathf.Cos(angle) * width + normalizedDirection * Mathf.Sin(angle) * width;

                m_Positions.Add(centerPos + offset);
                m_Normals.Add(up);
                m_Textures.Add(new Vector2(
                    (Mathf.Cos(angle) + 1f) * 0.5f,
                    isStartCap ? Mathf.Sin(angle) * 0.5f : 1f - Mathf.Sin(angle) * 0.5f
                ));
            }

            // Add triangles for cap
            for (int i = 0; i < segments; i++)
            {
                if (isStartCap)
                {
                    m_Indices.Add(prevVertCount);
                    m_Indices.Add(prevVertCount + i + 1);
                    m_Indices.Add(prevVertCount + i + 2);
                }
                else
                {
                    m_Indices.Add(prevVertCount);
                    m_Indices.Add(prevVertCount + i + 2);
                    m_Indices.Add(prevVertCount + i + 1);
                }
            }
        }


        // Mapping from triangle index to spline index
        private Dictionary<int, int> triangleToSplineMap = new Dictionary<int, int>();

        public void LoftAllRoads()
        {
            LoftMesh.Clear();
            m_Positions.Clear();
            m_Normals.Clear();
            m_Textures.Clear();
            m_Indices.Clear();
            triangleToSplineMap.Clear(); // Clear the mapping

            for (var i = 0; i < LoftSplines.Count; i++)
                Loft(LoftSplines[i], i);

            LoftMesh.SetVertices(m_Positions);
            LoftMesh.SetNormals(m_Normals);
            LoftMesh.SetUVs(0, m_Textures);
            LoftMesh.subMeshCount = 1;
            LoftMesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);
            LoftMesh.UploadMeshData(false);

            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        public void Loft(Spline spline, int splineIndex)
        {
            if (spline == null || spline.Count < 2)
                return;

            float length = spline.GetLength();
            if (length <= 0.001f)
                return;

            var segmentsPerLength = SegmentsPerMeter * length;
            var segments = Mathf.CeilToInt(segmentsPerLength);
            var segmentStepT = (1f / SegmentsPerMeter) / length;
            var steps = segments + 1;
            var vertexCount = steps * 2;
            var triangleCount = segments * 6;
            var prevVertexCount = m_Positions.Count;

            m_Positions.Capacity += vertexCount;
            m_Normals.Capacity += vertexCount;
            m_Textures.Capacity += vertexCount;
            m_Indices.Capacity += triangleCount;

            var t = 0f;
            for (int i = 0; i < steps; i++)
            {
                SplineUtility.Evaluate(spline, t, out var pos, out var dir, out var up);

                var scale = transform.lossyScale;
                var tangent = math.normalizesafe(math.cross(up, dir)) *
                              new float3(1f / scale.x, 1f / scale.y, 1f / scale.z);

                var w = roadWidth;
                if (splineIndex < m_Widths.Count)
                {
                    w = m_Widths[splineIndex].DefaultValue;
                    if (m_Widths[splineIndex] != null && m_Widths[splineIndex].Count > 0)
                    {
                        w = m_Widths[splineIndex].Evaluate(spline, t, PathIndexUnit.Normalized,
                            new Interpolators.LerpFloat());
                        w = math.clamp(w, .001f, 10000f);
                    }
                }

                m_Positions.Add(pos - (tangent * w));
                m_Positions.Add(pos + (tangent * w));
                m_Normals.Add(up);
                m_Normals.Add(up);
                m_Textures.Add(new Vector2(0f, t * m_TextureScale));
                m_Textures.Add(new Vector2(1f, t * m_TextureScale));

                t = math.min(1f, t + segmentStepT);
            }

            if (roundCaps)
            {
                AddRoundedCaps(spline, roadWidth, Quaternion.Euler(capRotationOffset));
            }

            for (int i = 0, n = prevVertexCount; i < triangleCount; i += 6, n += 2)
            {
                m_Indices.Add(n + 2);
                m_Indices.Add(n + 1);
                m_Indices.Add(n + 0);
                m_Indices.Add(n + 2);
                m_Indices.Add(n + 3);
                m_Indices.Add(n + 1);

                // Map triangles to the spline index
                int triangleBaseIndex = (m_Indices.Count / 3) - 2; // Calculate the base triangle index
                triangleToSplineMap[triangleBaseIndex] = splineIndex; //first triangle
                triangleToSplineMap[triangleBaseIndex + 1] = splineIndex; // Second triangle
            }
        }

        public int GetSplineIndexFromTriangle(int triangleIndex)
        {
            if (triangleToSplineMap.TryGetValue(triangleIndex, out int splineIndex))
            {
                return splineIndex;
            }

            Debug.LogError(
                $"Triangle index {triangleIndex} not found in mapping. Total mapped triangles: {triangleToSplineMap.Count}");
            return -1;
        }
    }
}