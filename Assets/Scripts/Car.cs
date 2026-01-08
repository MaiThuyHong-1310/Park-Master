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
    [SerializeField] LineRenderer carLine;
    Rigidbody rb;
    Quaternion startRotationOfCar;
    Vector3 dir;
    public int statusCar = 0;


    public bool IsMoving { get; private set; }


    Coroutine m_animCoroutine;

    public Action<Car> onReachedDestination;
    public Action<Car> varOtherCar;

    public void ForceToStartPosition()
    {
        m_visualBody.position = startPos.transform.position;
    }

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
        //DrawPathOnCar(m_path);
    }

    public void ClearPathVisual()
    {
        if (carLine == null) return;
        carLine.positionCount = 0;
    }

    public void DrawPathOnCar(Vector3[] path, int renderingOrder) 
    {
        if (carLine == null) return;
        carLine.sortingOrder = renderingOrder;
        carLine.positionCount = path.Length;
        carLine.SetPositions(path);
        carLine.enabled = true;
    }

    public void RunPath()
    {
        
        if (m_animCoroutine != null) StopCoroutine(m_animCoroutine);
        m_animCoroutine = StartCoroutine(CarRunAnim(m_path));
        IsMoving = true;
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
                BoxCollider bol = parkingSpotTarget.GetComponent<BoxCollider>();
                Bounds b = bol.bounds;

                // Take max on ground
                Vector3 maxP = b.max;
                Vector3 maxPOnGround = new Vector3(maxP.x, 0, maxP.z);

                // Take min on ground
                Vector3 minP = b.min;
                Vector3 minPOnGround = new Vector3(minP.x, 0, minP.z);

                //get x,z from car
                float xC = m_visualBody.position.x;
                float zC = m_visualBody.position.z;

                Vector3 posOfParking = parkingSpotTarget.transform.position;
                Vector3 finalPosOfCarWin = new Vector3(posOfParking.x, 0.15f, posOfParking.z);

                if (xC > minPOnGround.x && xC < maxPOnGround.x && zC > minPOnGround.z && zC < maxPOnGround.z)
                {
                    path.Add(finalPosOfCarWin);
                    //Debug.Log("Position of car: " + m_visualBody.position);
                    //Debug.Log("Position of parking: " + parkingSpotTarget.transform.position);
                    onReachedDestination?.Invoke(this);
                }
            }

            remainDistance = (progress - 1) * segmentLength;
            pathIndex++;
        }

        m_visualBody.position = path[path.Count - 1];
        m_visualBody.rotation = Quaternion.Euler(90f, -90f, 0);
        //yield return new WaitForSeconds(1f);
    }

    IEnumerator ReturnToStart()
    {
        Vector3 targetPos = startPos.transform.position;
        Vector3 origin = m_visualBody.position;
        Quaternion originRot = m_visualBody.rotation;

        float duration = 0.3f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // smooth position
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            m_visualBody.position = Vector3.Lerp(origin, targetPos, smoothT);

            // smooth rotation follow
            m_visualBody.rotation = Quaternion.Slerp(originRot, Quaternion.Euler(90f, -90f, 0), smoothT);
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

    public void FlyAway(Vector3 dir, float force, float torqueForce = 4f)
    {
        StopCar();

        if (rb == null) rb = m_visualBody.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.Log($"[{name}] Not found Rigidbody on m_visualBody!");
            return;
        }

        // Turn on physics
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // AddForce to car fly flow inertia
        rb.AddForce(dir.normalized * force, ForceMode.Impulse);

        // AddTorque 
        float randomSign = UnityEngine.Random.Range(-0.5f, 0.5f);
        rb.AddTorque(Vector3.up * torqueForce * randomSign, ForceMode.Impulse);

        Vector3 p = m_visualBody.position;
        p.y = 1f; 
        m_visualBody.position = p;
    }

    void Start()
    {
        startRotationOfCar = this.transform.rotation;
        Debug.Log("Pos of car: " + m_visualBody.position);
        //carSelectionManage.varCarWithOtherCar += ;
    }

    void Update()
    {
        //Debug.Log("pos of car: " + m_visualBody.position);
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

        IsMoving = false;
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
