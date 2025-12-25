using UnityEngine;

public class ParkingSpotStart : MonoBehaviour
{
    public Transform[] listParkingStart;
    //public Vector3 posOfStartSpot;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("StartPosition");
        listParkingStart = new Transform[obj.Length];

        for (int i = 0; i < obj.Length; i++)
        {
            listParkingStart[i] = obj[i].transform;
        }
    }
}
