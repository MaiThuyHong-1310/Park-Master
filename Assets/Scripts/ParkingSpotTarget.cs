using System.Collections;
using UnityEngine;

public class ParkingSpotTarget : MonoBehaviour
{
    public Transform[] listParkingTarget;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        GameObject[] obj = GameObject.FindGameObjectsWithTag("ParkingSpot");
        listParkingTarget = new Transform[obj.Length];

        for (int i = 0; i < obj.Length; i++)
        {
            listParkingTarget[i] = obj[i].transform;
        }
    }
}
