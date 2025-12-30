using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarSelectionManager : MonoBehaviour
{
    [SerializeField] private PathDrawer m_pathDrawer;
    public Car selectedCar;
    [SerializeField] LayerMask carMask;
    [SerializeField] LayerMask StartPos;
    public Car[] arrayCar;
    float minDistance2 = 2f;
    int checkWin;
    int winCar = 0;
    int indexOfCarWin = 0;
    bool hasLose;

    public Action<int> onCollectingAllCar;
    public Action varCarWithOtherCar;

    void Start()
    {
        GameObject[] objCar = GameObject.FindGameObjectsWithTag("PlayerCar");
        arrayCar = new Car[objCar.Length];
        for (int i = 0; i < objCar.Length; i++)
        {
            arrayCar[i] = objCar[i].GetComponentInParent<Car>();
        }
        Debug.Log("STARTED!");
    }

    public void Clear()
    {
        selectedCar = null;
    }

    bool m_beginPathHandlingForCar;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pointOnScreen = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pointOnScreen);

            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "StartPos")
                {
                    selectedCar = hits[i].collider.GetComponentInParent<Car>();
                    selectedCar.SetSelected(); // true
                    break;
                }
            }

            if(selectedCar != null)
            {
                //Stop and reset state of cars
                for(int i = 0; i < arrayCar.Length; i++)
                {
                    arrayCar[i].StopAndReset();
                }

                reachedCars.Clear();
                m_beginPathHandlingForCar = true;
                m_pathDrawer.ClearPoints();
                m_pathDrawer.BeginPlottingPoint();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && m_beginPathHandlingForCar)
        {
            selectedCar.SetPath(m_pathDrawer.path);

            for (int i = 0; i < arrayCar.Length; i++)
            {
                if(arrayCar[i].HasPath()) arrayCar[i].RunPath();
            }

            m_beginPathHandlingForCar = false;
            selectedCar = null;
            m_pathDrawer.EndPlottingPoint();
            m_pathDrawer.ClearPoints();
        }

        CheckLoseByDistance();
    }

    public void StopAllCars()
    {
        for (int i = 0; i < arrayCar.Length; i++)
        {
            if (arrayCar[i] != null)
                arrayCar[i].StopCar();
        }
    }

    public void SetCarsForLevel(Car[] cars)
    {
        if (cars != null)
        {
            arrayCar = cars;
        }
        // register listener for all car in all levels
        for (int i = 0; i < arrayCar.Length; i++)
        {
            arrayCar[i].onReachedDestination += OnCarReachedDestination;
            
        }

        checkWin = 0;
        winCar = 0;
        indexOfCarWin = 0;
    }

    private HashSet<Car> reachedCars = new HashSet<Car>();

    public void OnCarReachedDestination(Car car)
    {
        // avoid duplicates car
        if (reachedCars.Contains(car)) return;

        reachedCars.Add(car);
        Debug.Log($"CAR {car.name} REACHES DESTINATION! ({reachedCars.Count}/{arrayCar.Length})");

        // if all car reached destination
        if (reachedCars.Count == arrayCar.Length)
        {
            Debug.Log("YOU WINNNN!");
            onCollectingAllCar?.Invoke(arrayCar.Length);
        }
    }

    void CheckLoseByDistance()
    {
        if (hasLose) return;
        for (int i = 0; i < arrayCar.Length; i++)
        {
            for (int j = i + 1; j < arrayCar.Length; j++)
            {
                float dist = Vector3.Distance(arrayCar[i].GetCarBodyPos(), arrayCar[j].GetCarBodyPos());

                if (dist < minDistance2)
                {
                    hasLose = true;

                    Debug.Log($"LOSE: {arrayCar[i].name} var with {arrayCar[j].name} (distance = {dist})");

                    StopAllCars();

                    // Send event lose to gameController
                    varCarWithOtherCar?.Invoke();
                    return;
                }
            }
        }
    }


}
