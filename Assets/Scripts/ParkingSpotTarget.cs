using UnityEngine;

public class ParkingSpotTarget : MonoBehaviour
{

    public Transform[] listParkingTarget;

    void Start()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("ParkingSpot");
        listParkingTarget = new Transform[obj.Length];

        for (int i = 0; i < obj.Length; i++)
        {
            listParkingTarget[i] = obj[i].transform;
        }
    }

}
