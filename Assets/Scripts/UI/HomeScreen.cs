using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : MonoBehaviour
{
    [Header("ボタン")]
    public Button startButton;
    public Button settingsButton;

    void Start()
    {
        startButton.onClick.AddListener(() => SceneLoader.Load("Difficulty"));
        settingsButton.onClick.AddListener(() => SceneLoader.Load("Settings"));
    }
}
