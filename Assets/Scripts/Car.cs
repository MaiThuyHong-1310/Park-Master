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
    CarManager carSelectionManage;
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

    IEnumerator ReturnToStart()
    {
        Vector3 targetPos = startPos.transform.position;
        Vector3 origin = m_visualBody.position;

        float duration = 0.8f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // smooth position
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            m_visualBody.position = Vector3.Lerp(origin, targetPos, smoothT);

            // smooth rotation follow
            m_visualBody.rotation = Quaternion.Slerp(m_visualBody.rotation, Quaternion.Euler(90f, -90f, 0), 8f * Time.deltaTime);
            yield return null;
        }

        m_visualBody.position = targetPos;
        m_visualBody.rotation = Quaternion.Euler(90f, -90f, 0);
        m_animCoroutine = null;
    }


    public void StopAndReturnToStart()
    {
        if (m_animCoroutine != null)
        {
            StopCoroutine(m_animCoroutine);
            m_animCoroutine = null;
        }

        m_animCoroutine = StartCoroutine(ReturnToStart());
    }



    void Start()
    {
        startRotationOfCar = this.transform.rotation;    
    }

    void Update()
    {
        /*return;
        if (IsPointerDownThisFrame())
        {
            Vector2 mouPos = GetPointerPosition();
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
                        //Debug.Log("speed return is 1f");
                        //m_visualBody.transform.position = Vector3.Lerp(m_visualBody.transform.position, startPos.transform.position, 0.1f);
                        //m_visualBody.transform.rotation = Quaternion.Euler(90f, -90f, 0);
                        StopAndReturnToStart();

                    }

                }

                if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "Car")
                {
                    Debug.Log("");
                }
            }
        }
        else if (IsPointerUpThisFrame())
        {
            if (m_animCoroutine == null)
            {
                Debug.Log("");
            }
            else
            {
                m_animCoroutine = StartCoroutine(CarRunAnim(m_path));
            }
            
        }*/
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


    bool IsPointerDownThisFrame()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;

        return false;
    }

    bool IsPointerUpThisFrame()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

        if (Mouse.current != null)
            return Mouse.current.leftButton.wasReleasedThisFrame;

        return false;
    }

    Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }

}
