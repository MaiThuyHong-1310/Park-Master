using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class Level : MonoBehaviour, IDisposable
{
    [SerializeField] List<Car> m_cars;

    public List<Car> Cars => m_cars;

    public Action onWin;
    public Action onLose;

    public void Dispose()
    {
        Debug.Log($"Disposing this level {gameObject.name}");
    }

    public void Init()
    {
        var parkingSpotTargets = transform.GetComponentsInChildren<ParkingSpotTarget>();
    }
}
