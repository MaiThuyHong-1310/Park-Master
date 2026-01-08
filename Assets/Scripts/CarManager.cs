using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarManager : MonoBehaviour
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
    public Action<Car, Car, Vector3> varCarWithOtherCar;
    //public Action reDrawForCar;

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

    int m_increment;


    void Update()
    {
        if (IsPointerDownThisFrame())
        {
            Vector2 pointOnScreen = GetPointerPosition();
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
                    arrayCar[i].StopAndReturnToStart();
                }

                selectedCar.ClearPathVisual();

                reachedCars.Clear();
                m_beginPathHandlingForCar = true;
                m_activeCollisionCheck = false;
                m_pathDrawer.Line.sortingOrder = m_increment++;
                m_pathDrawer.ClearPoints();
                m_pathDrawer.BeginPlottingPoint();
            }
        }

        if (IsPointerUpThisFrame() && m_beginPathHandlingForCar)
        {
            selectedCar.SetPath(m_pathDrawer.path);
            selectedCar.DrawPathOnCar(m_pathDrawer.GetVisualPath(), m_pathDrawer.Line.sortingOrder);

            for (int i = 0; i < arrayCar.Length; i++)
            {
                if(arrayCar[i].HasPath()) arrayCar[i].RunPath();
            }

            m_beginPathHandlingForCar = false;
            selectedCar = null;
            m_activeCollisionCheck = true;
            m_pathDrawer.EndPlottingPoint();
            m_pathDrawer.ClearPoints();
            //reDrawForCar?.Invoke();
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
            arrayCar[i].ForceToStartPosition();
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

    bool m_activeCollisionCheck;
    void CheckLoseByDistance()
    {
        if (!m_activeCollisionCheck) return;
        //if (hasLose) return;
        for (int i = 0; i < arrayCar.Length; i++)
        {
            for (int j = i + 1; j < arrayCar.Length; j++)
            {
                if (!arrayCar[i].IsMoving && !arrayCar[j].IsMoving) continue;
                float dist = Vector3.Distance(arrayCar[i].GetCarBodyPos(), arrayCar[j].GetCarBodyPos());

                if (dist < minDistance2)
                {
                    //hasLose = true;
                    Debug.Log($"LOSE: {arrayCar[i].name} var with {arrayCar[j].name} (distance = {dist})");

                    Vector3 dir = (arrayCar[i].GetCarBodyPos() - arrayCar[j].GetCarBodyPos()).normalized;

                    float force = 10f;
                    float torque = 4f;

                    arrayCar[i].FlyAway(dir, force, torque);
                    arrayCar[j].FlyAway(-dir, force, torque);

                    //StopAllCars();
                    if (arrayCar[i].IsMoving) arrayCar[i].StopCar();
                    if (arrayCar[j].IsMoving) arrayCar[j].StopCar();

                    // Send event lose to gameController
                    varCarWithOtherCar?.Invoke(arrayCar[i], arrayCar[j], dir);
                    return;
                }
            }
        }
    }


    public bool IsPointerDownThisFrame()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;

        return false;
    }

    public bool IsPointerUpThisFrame()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;

        if (Mouse.current != null)
            return Mouse.current.leftButton.wasReleasedThisFrame;

        return false;
    }

    public Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }



}
