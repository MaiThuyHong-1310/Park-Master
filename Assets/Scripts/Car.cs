using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class Car : MonoBehaviour
{
    [SerializeField] Transform m_visualBody;
    List<Vector3> m_path;
    Vector3 nextPosOfCar;
    //Vector3 curPosOfCar;
    [SerializeField] float speedOfCar;
    bool statusRunOfCar;

    Coroutine m_animCoroutine;

    public void SetPath(List<Vector3> path)
    {
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
}
