using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class Car : MonoBehaviour
{
    [SerializeField] Transform m_visualBody;
    List<Vector3> m_path;
    [SerializeField] float speedOfCar;
    bool isSelected;
    [SerializeField]
    CarSelectionManager carSelectionManage;
    [SerializeField] ParkingSpotTarget parkingSpotTarget;

    Coroutine m_animCoroutine;

    public Vector3 GetCarBodyPos()
    {
        return m_visualBody.position;
    }

    public void SetPath(List<Vector3> path)
    {
        // if distance between last point of path and parking position less than minDistance
        int lengthOfParkingTarget = parkingSpotTarget.listParkingTarget.Length;

        float[] arrayDistance = new float[lengthOfParkingTarget];

        Vector3 minPosition = parkingSpotTarget.listParkingTarget[0].position;  //goat 

        float minDistance = 1000f;

        // need to taking the position has distance between targetParking and lastPosition is min
        for (int i = 0; i < lengthOfParkingTarget; i++)
        {
            // Taking position for loop 
            Vector3 positionParkingTarget = parkingSpotTarget.listParkingTarget[i].position;
            Debug.Log("positionParkingTarget: " + positionParkingTarget);

            // caculating distance between targetParking and lastPosition
            arrayDistance[i] = Vector3.Distance(positionParkingTarget, path[path.Count - 1]);
            Debug.Log("arrayDistance[i]: " + arrayDistance[i]);

            if (arrayDistance[i] < minDistance)
            {
                Debug.Log("minDistance: " + arrayDistance[i]);
                minDistance = arrayDistance[i];
                minPosition = positionParkingTarget;
            }
        }

        // add parkingTarget into path
        if (minDistance < 5f)
        {
            path.Add(minPosition);
        }
        
        m_path = path;
        if (m_animCoroutine != null) StopCoroutine(m_animCoroutine);
        m_animCoroutine = StartCoroutine(CarRunAnim(path));
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

                // update pos after each frame
                m_visualBody.position = Vector3.Lerp(path[pathIndex], path[pathIndex + 1], progress);
                yield return null;
            }

            remainDistance = (progress - 1) * segmentLength;

            // move segment
            pathIndex++;
        }

        m_visualBody.position = path[path.Count - 1];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

}
