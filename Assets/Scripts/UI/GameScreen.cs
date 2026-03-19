using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : MonoBehaviour
{
    [Header("ターン表示")]
    public Image turnLabel;
    public Sprite yourTurnSprite;
    public Sprite cpuTurnSprite;

    [Header("ポーズ")]
    public Button pauseButton;
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button retireButton;

    public bool isPaused = false;

    void Start()
    {
        pauseMenu.SetActive(false);

        pauseButton.onClick.AddListener(OpenPauseMenu);
        resumeButton.onClick.AddListener(ClosePauseMenu);
        retireButton.onClick.AddListener(Retire);
    }

    void Update()
    {
        // ポーズ中はターン表示を更新しない
        if (isPaused) return;

        if (GameManager.Instance != null && turnLabel != null)
        {
            bool isPlayerTurn = GameManager.Instance.isPlayerTurn;
            turnLabel.sprite = isPlayerTurn ? yourTurnSprite : cpuTurnSprite;
        }
    }

    void OpenPauseMenu()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.gameOver = true;
    }

    void ClosePauseMenu()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.gameOver = false;
    }

    void Retire()
    {
        isPaused = false;
        ResultData.playerWon = false;
        Debug.Log("[GameScreen] Retire button pressed. playerWon set to false");
        
        // GameManagerとBoardManagerを破棄
        if (GameManager.Instance != null)
        {
            var board = BoardManager.Instance;
            if (board != null && board.gameObject != null)
                Destroy(board.gameObject);
            
            Destroy(GameManager.Instance.gameObject);
        }
        
        SceneLoader.Load("Result");
    }
}