using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField] private int targetScore = 500;
    [SerializeField] private int currentScore = 0;

    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;

    private bool isGameClear = false;

    private void Awake()
    {
        InitializeScore();
    }

    private void Start()
    {
    }

    private void InitializeScore()
    {
        isGameClear = false;

        currentScore = 0;
        // Todo: 목표점수 파일?등에서 읽어오기
        //targetScore =

        targetScoreText.text = $"Target Score : {targetScore}";
        currentScoreText.text = $"Score : {currentScore}";
    }

    public void AddScore(int score)
    {
        currentScore += score;

        currentScoreText.text = $"Score : {currentScore}";

        GameClear();
    }

    public void GameClear()
    {
        if (currentScore >= targetScore && !isGameClear)
        {
            isGameClear = true;
            Debug.Log("GameClear");
        }
    }
}
