using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;


public class Car : MonoBehaviour
{
    [SerializeField] Transform m_visualBody;
    List<Vector3> m_path;
    //List<Vector3> m_historyPath = new 
    [SerializeField] float speedOfCar;
    bool isSelected;
    [SerializeField]
    CarSelectionManager carSelectionManage;
    [SerializeField] LayerMask startPosMask;
    [SerializeField] ParkingSpotTarget parkingSpotTarget;
    [SerializeField] ParkingSpotStart startPos;
    Quaternion startRotationOfCar;
    Vector3 dir;
    public int statusCar = 0;


    Coroutine m_animCoroutine;

    public Action<Car> onReachedDestination;

    public bool ReachDestination()
    {
        return false;
    }

    public bool HasPath() => m_path != null && m_path.Count > 1;

    public Vector3 GetCarBodyPos()
    {
        return m_visualBody.position;
    }

    public ParkingSpotTarget GetParkingSpotTarget()
    {
        return parkingSpotTarget;
    }

    public void SetPath(IEnumerable<Vector3> path)
    {
        m_path = new List<Vector3>(path);

        /*// if distance between last point of path and parking position less than minDistance
        int lengthOfParkingTarget = parkingSpotTarget.listParkingTarget.Length;

        float[] arrayDistance = new float[lengthOfParkingTarget];

        Vector3 minPosition = parkingSpotTarget.listParkingTarget[0].position;

        float minDistance = 1000f;

        // need to taking the position has distance between targetParking and lastPosition is min
        for (int i = 0; i < lengthOfParkingTarget; i++)
        {
            // Taking position for loop 
            Vector3 positionParkingTarget = parkingSpotTarget.listParkingTarget[i].position;
            Debug.Log("positionParkingTarget: " + positionParkingTarget);

            // caculating distance between targetParking and lastPosition
            arrayDistance[i] = Vector3.Distance(positionParkingTarget, m_path[m_path.Count - 1]);
            Debug.Log("arrayDistance[i]: " + arrayDistance[i]);

            if (arrayDistance[i] < minDistance)
            {
                Debug.Log("minDistance: " + arrayDistance[i]);
                minDistance = arrayDistance[i];
                minPosition = positionParkingTarget;
            }
        }

        // add parkingTarget into path
        if (minDistance < 2f)
        {
            m_path.Add(minPosition);
        }*/
    }

    public void RunPath()
    {
        if (m_animCoroutine != null) StopCoroutine(m_animCoroutine);
        m_animCoroutine = StartCoroutine(CarRunAnim(m_path));
    }


    IEnumerator CarRunAnim(List<Vector3> path)
    {
        Assert.IsFalse(path == null);
        Assert.IsTrue(path.Count > 1);

        int pathIndex = 0;
        //float lengthRunned = 0f;
        float remainDistance = 0;

        // go through each section of the path
        while (pathIndex < path.Count - 1)
        {
            float segmentLength = (path[pathIndex] - path[pathIndex + 1]).magnitude;
            //float lengthOfCarRunned = 0 / segmentLength; //(0-1)
            float progress = remainDistance / segmentLength;

            while (progress < 1f)
            {
                float lengthOfFrame = speedOfCar * Time.deltaTime;
                progress += lengthOfFrame / segmentLength;

                // update pos and rot after each frame
                dir = (path[pathIndex + 1] - path[pathIndex]).normalized;
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0,0,90);

                m_visualBody.position = Vector3.Lerp(path[pathIndex], path[pathIndex + 1], progress);
                m_visualBody.rotation = Quaternion.Slerp(m_visualBody.rotation, targetRot, speedOfCar * Time.deltaTime);
                yield return null;
            }

            remainDistance = (progress - 1) * segmentLength;

            // move segment
            pathIndex++;
        }

        m_visualBody.position = path[path.Count - 1];

        // Win event
        float distance = Vector3.Distance(GetCarBodyPos(), GetParkingSpotTarget().transform.position);
        if (distance < 1f)
        {
            onReachedDestination?.Invoke(this);
        }
    }


    IEnumerator returnStartSpot()
    {
        Vector3 startPos = m_path[0];
        while (Vector3.Distance(this.m_visualBody.position, startPos) < 0.1f)
        {
            this.m_visualBody.position = Vector3.Lerp(this.m_visualBody.position, startPos, 0.5f);
        }
        yield return null;

        m_visualBody.position = startPos;
        m_visualBody.transform.rotation = Quaternion.Euler(90f, -90f, 0);
        m_animCoroutine = null;
    }

    public void StopAndStartReturn()
    {
        if (m_animCoroutine != null)
        {
            StopCoroutine(m_animCoroutine);
            m_animCoroutine = null;
        }
        m_animCoroutine = StartCoroutine(returnStartSpot());
    }

    void Start()
    {
        startRotationOfCar = this.transform.rotation;    
    }

    void Update()
    {
        return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouPos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mouPos);

            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "StartPos")
                {
                    if (m_animCoroutine == null)
                    {
                        Debug.Log("");
                    }
                    else
                    {
                        //StopCoroutine(m_animCoroutine);
                        ////m_visualBody.transform.position = startPos.transform.position;
                        //StartCoroutine();
                        //m_visualBody.transform.rotation = Quaternion.Euler(90f, -90f, 0);
                        StopAndStartReturn();
                    }

                }

                if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "Car")
                {
                    Debug.Log("");
                }
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (m_animCoroutine == null)
            {
                Debug.Log("");
            }
            else
            {
                m_animCoroutine = StartCoroutine(CarRunAnim(m_path));
            }
            
        }
    }

    // extra function
    public void SetSelected()
    {
        isSelected = true;

        // Add any event when click on car
        Debug.Log("This car is selecting");
    }

    public void StopCar()
    {
        if (m_animCoroutine != null)
        {
            StopCoroutine(m_animCoroutine);
            m_animCoroutine = null;
        }
    }

    public void StopAndReset()
    {
        if (m_animCoroutine != null)
        {
            StopCoroutine(m_animCoroutine);
            m_animCoroutine = null;
        }

        m_visualBody.transform.position = startPos.transform.position;
        m_visualBody.transform.rotation = Quaternion.Euler(90f, -90f, 0);
    }

}
