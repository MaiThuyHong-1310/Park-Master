using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : Singleton<GameController>
{
    [SerializeField] CarSelectionManager m_carSelectionManager;
    [SerializeField] PathDrawer m_pathDrawer;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI m_scoreText;
    int m_score = 0;

    [Header("Level Settings")]
    [SerializeField] int m_startLevelId = 0;

    [SerializeField] GameObject replayButton;

    Level m_currentLevel;

    int m_currentLevelId;

    bool m_changingLevel;

    private void Start()
    {
        m_currentLevelId = 0;

        //LoadLevel(m_currentLevelId);

        m_carSelectionManager.onCollectingAllCar += OnWinLevel;
        m_carSelectionManager.varCarWithOtherCar += OnLoseLevel;

        m_score = 0;
        updateScoreUI();

    }

    public bool m_isGameOver;

    private void Update()
    {
        CheckGameOver();
    }

    void CheckGameOver()
    {
        if (m_isGameOver) return;

        //
    }

    public void LoadLevel(int id)
    {
        //ScoreManager.Instance.MarkScoreAtLevel();
        int totalLevel = 2;

        m_carSelectionManager.Clear();

        if (m_currentLevel != null)
        {
            m_currentLevel.Dispose();
            Destroy(m_currentLevel.gameObject);
            //m_currentLevel = null;
        }

        m_currentLevel = Instantiate(Resources.Load<GameObject>($"Level{id % totalLevel}")).GetComponent<Level>();
        m_currentLevel.Init();
        m_carSelectionManager.SetCarsForLevel(m_currentLevel.Cars.ToArray());
    }

    void OnWinLevel(int count)
    {
        addScore(100);

        Debug.Log($"WIN COLLECTING {count} cars");

        NextLevel();
    }

    void OnLoseLevel()
    {
        m_isGameOver = true;
        Debug.Log("VARR! YOU LOSED!");
        if (replayButton != null)
        {
            replayButton.SetActive(true);
        }
        //ReplayLevel();
        //replayButton.SetActive(false);
    }

    public void ReplayLevel()
    {
        //ScoreManager.Instance.ResetScoreAtLevel(m_currentLevelId);
        Debug.Log("REPLAY THIS LEVEL!");
        LoadLevel(m_currentLevelId);
        replayButton.SetActive(false);
        m_isGameOver = false;
    }

    public void NextLevel()
    {
        //LoadLevel(m_currentLevel+1);
        if (m_changingLevel) return;
        m_changingLevel = true;

        m_currentLevelId++;

        LoadLevel(m_currentLevelId);

        m_changingLevel = false;
    }

    public void updateScoreUI()
    {
        if (m_scoreText != null)
        {
            m_scoreText.text = $"Score: {m_score}";
        }
    }

    public void addScore(int amount)
    {
        m_score += amount;
        if (m_score < 0) m_score = 0;
        updateScoreUI();
    }
}
