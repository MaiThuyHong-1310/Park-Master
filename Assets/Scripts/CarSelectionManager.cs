using UnityEngine;
using UnityEngine.InputSystem;

public class CarSelectionManager : MonoBehaviour
{

    public Car selectedCar;
    [SerializeField] LayerMask carMask;
    [SerializeField] LayerMask StartPos;
    public Car[] arrayCar;
    float minDistance2 = 1.5f;
    int checkWin;
    int winCar = 0;
    int indexOfCarWin = 0;

    void Start()
    {
        GameObject[] objCar = GameObject.FindGameObjectsWithTag("PlayerCar");
        arrayCar = new Car[objCar.Length];
        for (int i = 0; i < objCar.Length; i++)
        {
            arrayCar[i] = objCar[i].GetComponentInParent<Car>();
        }
        Debug.Log("STARTED!");
        Debug.Log("DISTANCE OF TWO CARS: " + Vector3.Distance(arrayCar[0].GetCarBodyPos(), arrayCar[1].GetCarBodyPos()));
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            for (int i = 0; i < arrayCar.Length; i++)
            {
                for (int j = i+1; j < arrayCar.Length; j++)
                {
                    // Take position of car
                    Vector3 posCar1 = arrayCar[i].GetCarBodyPos();
                    Vector3 posCar2 = arrayCar[j].GetCarBodyPos();
                    Debug.Log("DISTANCE OF TWO CARS: " + Vector3.Distance(posCar1, posCar2));


                    // Losed condition
                    if (Vector3.Distance(posCar1, posCar2) < minDistance2)
                    {
                        StopAllCars();
                        arrayCar[i].statusCar = -1;
                        //Debug.Log("VARRRR, YOU LOSED!!!");
                        checkWin = -1;
                    }
                }
            }

            
            for (int i = indexOfCarWin; i < arrayCar.Length; i++)
            {
                //Debug.Log("DISTANCE BETWEEN CAR AND PARKINGSPOTTARGET: " + Vector3.Distance(arrayCar[i].GetCarBodyPos(), arrayCar[i].GetParkingSpotTarget().transform.position));
                if (Vector3.Distance(arrayCar[i].GetCarBodyPos(), arrayCar[i].GetParkingSpotTarget().transform.position) == 0)
                {
                    winCar++;
                    indexOfCarWin++;
                }
                //Debug.Log("NUMBER OF CAR WIN: " + winCar);
            }

            if (winCar == arrayCar.Length)
            {
                checkWin = 1;
                winCar = 0;
            }

            if (checkWin == -1)
            {
                Debug.Log("YOU LOSED");
            }
            else
            {
                Debug.Log("YOU WIN");
            }
        }
        else
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
                }

                if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "Car")
                {
                    Debug.Log("EXIST IF CONDITION!");
                }
            }
        }
    }

    public void StopAllCars()
    {
        for (int i = 0; i < arrayCar.Length; i++)
        {
            if (arrayCar[i] != null)
                arrayCar[i].StopCar();
        }
    }

}
