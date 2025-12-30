using System.Collections;
using UnityEngine;

public class ParkingSpotStart : MonoBehaviour
{
    public ParkingSpotStart[] listParkingStart;
    //public Vector3 posOfStartSpot;

    public void Init(ParkingSpotStart[] parkingStartBuffer)
    {

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        GameObject[] obj = GameObject.FindGameObjectsWithTag("StartPosition");
        listParkingStart = new Transform[obj.Length];

        for (int i = 0; i < obj.Length; i++)
        {
            listParkingStart[i] = obj[i].transform;
        }
    }*/
}
