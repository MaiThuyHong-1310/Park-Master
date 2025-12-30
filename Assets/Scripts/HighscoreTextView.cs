using UnityEngine;
using R3;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HighscoreTextView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_text;
    CompositeDisposable m_bindings = new();

    void Start()
    {
        ScoreManager.Instance.Score
            .Subscribe(score =>
            {
                m_text.text = $"Score: {score}";
            })
            .AddTo(m_bindings);
    }

    private void OnDestroy()
    {
        m_bindings.Clear();
    }
}
