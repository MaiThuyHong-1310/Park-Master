using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; 
 
public class PathDrawer : MonoBehaviour
{
    private LayerMask groundMask;    // Only raycast hits ground
    private LineRenderer line;
    private float minPointDistance = 0.3f;
    private int maxPoints = 1000;
    public CarSelectionManager selectionCar;
    public Car car;
    public List<Vector3> path = new List<Vector3>();

    Vector3 lastPosition;


    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.useWorldSpace = true;

        // Setting the line width
        if (Mathf.Approximately(line.widthMultiplier, 0f))
            line.widthMultiplier = 0.5f;

        groundMask = LayerMask.GetMask("Ground");
    }

    bool m_isDragging;

    // Update is called once per frame
    void Update()
    {
        car = selectionCar.selectedCar;

        // if car is none -> no draw line
        if (car == null)
        {
            return;
        }

        Vector2 posMouse = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(posMouse);

        if (!m_isDragging && Mouse.current.leftButton.wasPressedThisFrame && Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            m_isDragging = true;
            lastPosition = hit.point;

            Debug.Log("Hit at: " + lastPosition);
        }

        if (m_isDragging)
        {
            if (Physics.Raycast(ray, out hit, 1000f, groundMask))
            {
                var currentPosition = hit.point;

                if (path.Count < maxPoints && Vector3.Distance(lastPosition, currentPosition) > minPointDistance)
                {
                    var index = line.positionCount++;

                    line.SetPosition(index, currentPosition);
                    path.Add(currentPosition);

                    lastPosition = hit.point;
                }
            }
        }


        if (m_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            m_isDragging = false;

            //Do something with path data

            //---------------------------

            if (path.Count >= 2)
            {
                car.SetPath(new List<Vector3>(path));
            }

            path.Clear();
            line.positionCount = 0;
            selectionCar.selectedCar = null;
        }
    }

}
