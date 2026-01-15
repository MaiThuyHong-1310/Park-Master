using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PathDrawerTmp : MonoBehaviour
{
    private LayerMask groundMask;    // Only raycast hits ground
    private LayerMask StartPos;

    private float minPointDistance = 0.25f;
    private int maxPoints = 1000;

    public Car car;
    public List<Vector3> path = new List<Vector3>();

    Vector3 lastPosition;

    bool m_canPlotPoint;

    //Road Mesh Settings 
    [Header("Road Mesh")]
    [SerializeField] float roadWidth = 1.0f;
    [SerializeField] int samplesPerSegment = 8;        
    [SerializeField] float handleScale = 0.33f;        
    [SerializeField] float uvTiling = 1.0f;            

    public MeshFilter _mf;
    public Mesh _mesh;

    void Start()
    {
        _mf = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _mesh.name = "RoadMesh";
        _mf.sharedMesh = _mesh;
        groundMask = LayerMask.GetMask("Ground");
        car = GameController.Instance.GetComponentInChildren<CarManager>().selectedCar;
    }

    public void BeginPlottingPoint()
    {
        m_canPlotPoint = true;
        Vector2 posMouse = GetPointerPosition();
        Ray ray = Camera.main.ScreenPointToRay(posMouse);

        // Take positon of TargetSpot
        if (car != null)
        {
            Bounds boundsOfParkingSpotTarget = car.GetComponent<ParkingSpotTarget>().GetComponent<Collider>().bounds;
            Vector3 bLeftMax = boundsOfParkingSpotTarget.max;
            Vector3 bLeftMaxOnGroundTmp = bLeftMax;
            bLeftMaxOnGroundTmp.y = 0;
            Vector3 bLeftMaxOnGround = bLeftMaxOnGroundTmp;

            // Take bound left min on ground
            Vector3 bLeftMin = boundsOfParkingSpotTarget.min;
            Vector3 bLeftMinOnGroundTmp = bLeftMin;
            bLeftMinOnGroundTmp.y = 0;
            Vector3 bLeftMinOnGround = bLeftMinOnGroundTmp;

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            {
                lastPosition = hit.point;
                if (lastPosition.x > bLeftMinOnGround.x && lastPosition.x < bLeftMaxOnGround.x && lastPosition.z > bLeftMinOnGround.z && lastPosition.z < bLeftMaxOnGround.z)
                {
                    lastPosition = car.GetComponent<ParkingSpotTarget>().GetComponent<Collider>().transform.position;
                }
            }
            else
            {
                Debug.Log("Shouldn't begin plotting points while mouse position fail to raycast ground");
                lastPosition = default;
            }
        }

    }


    public void EndPlottingPoint()
    {
        m_canPlotPoint = false;
        RebuildRoadMesh();
    }

    public void ClearPoints()
    {
        path.Clear();
        //_mesh.Clear();
    }


    public void ClearMesh()
    {
        if (_mesh != null)
        {
            _mesh.Clear();
        }
    }

    void Update()
    {
        if (GameController.Instance.m_isGameOver)
            return;

        if (!m_canPlotPoint)
            return;

        Vector2 posMouse = GetPointerPosition();
        Ray ray = Camera.main.ScreenPointToRay(posMouse);

        //var hits = Physics.RaycastAll(ray, 1000f);
        //if (hits == null || hits.Length == 0) return;

        //System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        //int firstLayer = hits[0].collider.gameObject.layer;
        //bool firstIsGround = ((1 << firstLayer) & groundMask.value) != 0;

        if (Physics.Raycast(ray, out var hit, 1000f, groundMask))
        {
            var currentPosition = hit.point;
            currentPosition.y = 0.1f;

            if (path.Count < maxPoints && Vector3.Distance(lastPosition, currentPosition) > minPointDistance)
            {
                path.Add(currentPosition);
                lastPosition = currentPosition;

                // add more vector3 -> rebuild mesh
                RebuildRoadMesh();
            }
        }

    }

    void RebuildRoadMesh()
    {
        if (path.Count < 2)
        {
            _mesh.Clear();
            return;
        }

        //Sample centerline bằng Bezier cubic
        List<Vector3> centerLine = SampleBezierFromAnchors(path, samplesPerSegment, handleScale);

        //centerline -> build strip mesh
        BuildRoadStripMesh(centerLine, roadWidth, uvTiling);
    }

    // Create sample points to list anchor "path" by cubic bezier auto-handle
    List<Vector3> SampleBezierFromAnchors(List<Vector3> anchors, int samplesPerSeg, float hScale)
    {
        List<Vector3> outPts = new List<Vector3>(anchors.Count * samplesPerSeg);

        Vector3 GetTangent(int i)
        {
            if (i <= 0) return (anchors[1] - anchors[0]).normalized;
            if (i >= anchors.Count - 1) return (anchors[^1] - anchors[^2]).normalized;
            return (anchors[i + 1] - anchors[i - 1]).normalized;
        }

        for (int i = 0; i < anchors.Count - 1; i++)
        {
            Vector3 P0 = anchors[i];
            Vector3 P3 = anchors[i + 1];

            Vector3 t0 = GetTangent(i);
            Vector3 t1 = GetTangent(i + 1);

            float segLen = Vector3.Distance(P0, P3);
            float handleLen0 = segLen * hScale;
            float handleLen1 = segLen * hScale;

            // cubic handles
            Vector3 P1 = P0 + t0 * handleLen0;
            Vector3 P2 = P3 - t1 * handleLen1;

            // sample segment
            int startJ = (i == 0) ? 0 : 1;

            for (int j = startJ; j <= samplesPerSeg; j++)
            {
                float t = j / (float)samplesPerSeg;
                outPts.Add(EvaluateCubicBezier(P0, P1, P2, P3, t));
            }
        }

        return outPts;
    }

    Vector3 EvaluateCubicBezier(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;

        // B(t) = u^3 P0 + 3u^2 t P1 + 3u t^2 P2 + t^3 P3
        return (uu * u) * P0
             + (3f * uu * t) * P1
             + (3f * u * tt) * P2
             + (tt * t) * P3;
    }

    void BuildRoadStripMesh(List<Vector3> centerLine, float width, float uvScale)
    {
        if (centerLine.Count < 2)
        {
            _mesh.Clear();
            return;
        }

        int n = centerLine.Count;
        float halfW = width * 0.5f;

        Vector3[] verts = new Vector3[n * 2];
        Vector2[] uvs = new Vector2[n * 2];
        int[] tris = new int[(n - 1) * 6];

        float vAcc = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 pWorld = centerLine[i];
            Vector3 p = transform.InverseTransformPoint(pWorld);

            // hướng (tangent) bằng finite diff
            Vector3 dir;
            if (i == 0) dir = (centerLine[i + 1] - p);
            else if (i == n - 1) dir = (p - centerLine[i - 1]);
            else dir = (centerLine[i + 1] - centerLine[i - 1]);

            dir.y = 0f; // road phẳng XZ
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;
            dir.Normalize();

            // normal ngang (trái/phải) = up x dir
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;

            Vector3 left = p - side * halfW;
            Vector3 right = p + side * halfW;

            int vi = i * 2;
            verts[vi + 0] = left;
            verts[vi + 1] = right;

            if (i > 0)
                vAcc += Vector3.Distance(centerLine[i - 1], p) * uvScale;

            // u=0 trái, u=1 phải; v theo chiều dài
            uvs[vi + 0] = new Vector2(0f, vAcc);
            uvs[vi + 1] = new Vector2(1f, vAcc);

            if (i < n - 1)
            {
                int ti = i * 6;

                // quad: (iL, iR, nextL, nextR)
                int iL = vi + 0;
                int iR = vi + 1;
                int nL = vi + 2;
                int nR = vi + 3;

                // 2 triangles
                tris[ti + 0] = iL;
                tris[ti + 1] = nL;
                tris[ti + 2] = iR;

                tris[ti + 3] = iR;
                tris[ti + 4] = nL;
                tris[ti + 5] = nR;
            }
        }

        //_mesh.Clear();
        _mesh.vertices = verts;
        _mesh.uv = uvs;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    // extra function
    Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }
}
