using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class Car : MonoBehaviour
{
    [SerializeField] Transform m_visualBody;
    List<Vector3> m_path;
    Vector3 nextPosOfCar;
    Vector3 curPosOfCar;
    float speedOfCar;
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

        int pathIndex = 0;

        // Copy path
        while (pathIndex < path.Count)
        {
            curPosOfCar = m_visualBody.position;

            // Waiting 1s to give new position of car
            yield return new WaitForSeconds(1f);

            // Taking new position of car and move
            m_visualBody.position = path[pathIndex];
            nextPosOfCar = m_visualBody.position;
            pathIndex++;

            
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
