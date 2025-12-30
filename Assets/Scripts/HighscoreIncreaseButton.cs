using UnityEngine;
using UnityEngine.UI;
using R3;

public class HighscoreIncreaseButton : MonoBehaviour
{
    [SerializeField] Button m_button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*m_button.OnClickAsObservable().Subscribe((_) =>
        {
            GameController.Instance.IncreaseHighScoreCmd.Execute(m_button);
        });*/
    }
}
