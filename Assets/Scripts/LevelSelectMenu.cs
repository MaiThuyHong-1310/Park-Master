using UnityEngine;

public class LevelSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject levelSelectPanel; 
    [SerializeField] GameObject scoreUI;          

    void Start()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);

        if (scoreUI != null)
            scoreUI.SetActive(false);
    }

    public void OnClickLevelButton(int levelIndex)
    {
        // Load level
        GameController.Instance.LoadLevel(levelIndex);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);

        if (scoreUI != null)
            scoreUI.SetActive(true);
    }
}
