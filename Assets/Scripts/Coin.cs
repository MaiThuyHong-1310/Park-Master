using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] Car[] cars;
    //Transform m_coinBody;
    Vector3[] carBodyPos;
    float minDistanceCarAndCoin = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cars != null)
        {
            carBodyPos = new Vector3[cars.Length];
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cars.Length; i++)
        {
            carBodyPos[i] = cars[i].GetCarBodyPos();

            if (Vector3.Distance(this.transform.position, carBodyPos[i]) < minDistanceCarAndCoin)
            {
                GameController.Instance.addScore(1);
                //Debug.Log("DESTROY COIN");
                Destroy(gameObject);
            }
        }
    }
}
