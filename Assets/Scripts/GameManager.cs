using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Pre level UI")]
    public GameObject preLevelPanel;
    public Slider speedSlider;
    public TextMeshProUGUI speedText;

    [Header("Cars")]
    public Car[] cars;

    public bool gameStart;

    void Awake()
    {
        Instance = this;    
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStart = false;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
