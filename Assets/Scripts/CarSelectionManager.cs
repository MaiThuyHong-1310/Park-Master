using UnityEngine;
using UnityEngine.InputSystem;

public class CarSelectionManager : MonoBehaviour
{

    public Car selectedCar;
    [SerializeField] LayerMask carMask;
    public Car[] arrayCar;
    float minDistance = 1.5f;

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
                    //Debug.Log("START CHECKING!!!");
                    // Take position of car
                    Vector3 posCar1 = arrayCar[i].GetCarBodyPos();
                    Vector3 posCar2 = arrayCar[j].GetCarBodyPos();
                    Debug.Log("DISTANCE OF TWO CARS: " + Vector3.Distance(posCar1, posCar2));

                    if (Vector3.Distance(posCar1, posCar2) < minDistance)
                    {
                        Debug.Log("VARRRR, YOU LOSED!!!");
                        StopAllCars();
                    }
                }
            }
            return;
        }
        else
        {
            Vector2 pointOnScreen = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pointOnScreen);

            if (Physics.Raycast(ray, out RaycastHit hit,1000f, carMask))
            {
                selectedCar = hit.collider.GetComponentInParent <Car>();
                selectedCar.SetSelected(); // true
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
