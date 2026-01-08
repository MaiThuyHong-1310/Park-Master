using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; 
 
public class PathDrawer : MonoBehaviour
{
    private LayerMask groundMask;    // Only raycast hits ground
    private LayerMask lineMask;
    private LineRenderer line;
    private float minPointDistance = 0.25f;
    private int maxPoints = 1000;
    //public CarSelectionManager selectionCar;
    public Car car;
    public List<Vector3> path = new List<Vector3>();
    Vector3 lastPosition;
    [SerializeField] float baseOffsetY = 0f;
    [SerializeField] float stepOffsetY = 0.02f;
    //int strokeIndex = 0;
    //float currentStrokeOffsetY;

    public LineRenderer Line => line;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.useWorldSpace = true;

        // Setting the line width
        if (Mathf.Approximately(line.widthMultiplier, 0f))
            line.widthMultiplier = 0.5f;

        groundMask = LayerMask.GetMask("Ground");
        //lineMask = LayerMask.GetMask("Line");
    }

    public Vector3[] GetVisualPath()
    {
        Vector3[] points = new Vector3[line.positionCount];
        line.GetPositions(points);
        return points;
    }

    bool m_isDragging;

    bool m_canPlotPoint;

    public void BeginPlottingPoint()
    {
        if (car != null) car.StopCar();
        m_canPlotPoint = true;

        // each line, increase a little
        //currentStrokeOffsetY = baseOffsetY + strokeIndex * stepOffsetY;
        //strokeIndex++;

        path.Clear();
        line.positionCount = 0;

        Vector2 posMouse = GetPointerPosition();
        Ray ray = Camera.main.ScreenPointToRay(posMouse);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            lastPosition = hit.point;
    }


    public void EndPlottingPoint()
    {
        m_canPlotPoint = false;
    }

    public void ClearPoints()
    {
        path.Clear();
        line.positionCount = 0;
    }

    Vector3 CatmullRomSplineInterp(Vector3 p_mi1, Vector3 p_0, Vector3 p_1, Vector3 p_2, float t)
    {
        Vector3 a4 = p_0;
        Vector3 a3 = (p_1 - p_mi1) / 2.0f;
        Vector3 a1 = (p_2 - p_0) / 2.0f - 2.0f * p_1 + a3 + 2.0f * a4;
        Vector3 a2 = 3.0f * p_1 - (p_2 - p_0) / 2.0f - 2.0f * a3 - 3.0f * a4;

        return a1 * t * t * t + a2 * t * t + a3 * t + a4;
    }

    public List<Vector3> Smoothtify(List<Vector3> inputPoints, int division = 4)
    {
        if (inputPoints.Count < 3)
        {
            return new List<Vector3>(inputPoints);
        }

        List<Vector3> samplings = new List<Vector3>(inputPoints);
        List<Vector3> outputPoints = new List<Vector3>(samplings.Count * division);

        Vector3 firstSegment = inputPoints[1] - inputPoints[0];
        Vector3 lastSegment = inputPoints[^1] - inputPoints[^2];

        samplings.Insert(0, inputPoints[0] - firstSegment);
        samplings.Add(inputPoints[^1] + lastSegment);

        for (int i = 0; i < samplings.Count - 3; i++)
        {
            Vector3 prevPos = samplings[i + 1];
            outputPoints.Add(prevPos);
            for (int j = 0; j <= division; j++)
            {
                float t = j * 1.0f / division;
                Vector3 pos = CatmullRomSplineInterp(samplings[i], samplings[i + 1], samplings[i + 2], samplings[i + 3], t);
                prevPos = pos;
                outputPoints.Add(prevPos);
            }
        }

        return outputPoints;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.m_isGameOver)
        {
            return;
        }
        if (m_canPlotPoint)
        {
            Vector2 posMouse = GetPointerPosition();
            Ray ray = Camera.main.ScreenPointToRay(posMouse);

            if (Physics.Raycast(ray, out var hit, 1000f, groundMask))
            {
                var currentPosition = hit.point;
                currentPosition.y = 0.5f;
                //currentPosition = currentPosition + Vector3.up * path.Count * 0.01f;


                if (path.Count < maxPoints && Vector3.Distance(lastPosition, currentPosition) > minPointDistance)
                {
                    //var index = line.positionCount++;

                    //line.SetPosition(index, currentPosition);
                    path.Add(currentPosition);

                    lastPosition = currentPosition;
                }
            }


            List<Vector3> smoothPoints = Smoothtify(path);
            line.positionCount = smoothPoints.Count;
            line.SetPositions(smoothPoints.ToArray());
        }
        return;
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
