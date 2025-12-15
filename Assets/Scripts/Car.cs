using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class Car : MonoBehaviour
{
    [SerializeField] Transform m_visualBody;
    List<Vector3> m_path;

    Coroutine m_animCoroutine;

    public void SetPath(List<Vector3> path)
    {
        m_path = path;
        if (m_animCoroutine != null) StopCoroutine(m_animCoroutine);
        m_animCoroutine = StartCoroutine(CarRunAnim(path));
    }

    IEnumerator carRun(List<Vector3> path)
    {

    }

    IEnumerator CarRunAnim(List<Vector3> path)
    {
        Assert.IsFalse(path == null);

        int pathIndex = 0;

        while (pathIndex < path.Count)
        {
            yield return new WaitForSeconds(1f);
            m_visualBody.position = path[pathIndex];
            pathIndex++;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
