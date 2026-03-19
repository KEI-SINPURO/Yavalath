using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyScreen : MonoBehaviour
{
    public Button weakBtn, normalBtn, strongBtn, masterBtn;

    void Start()
    {
        weakBtn.onClick.AddListener(() => Select(Difficulty.Weak));
        normalBtn.onClick.AddListener(() => Select(Difficulty.Normal));
        strongBtn.onClick.AddListener(() => Select(Difficulty.Strong));
        masterBtn.onClick.AddListener(() => Select(Difficulty.Master));
    }

    void Select(Difficulty d)
    {
        // GameManagerが存在する場合のみ設定、なければPlayerPrefsで保存
        if (GameManager.Instance != null)
            GameManager.Instance.difficulty = d;
        else
            PlayerPrefs.SetInt("Difficulty", (int)d);

        SceneLoader.Load("Game");
    }
}