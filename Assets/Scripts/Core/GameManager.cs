using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Weak, Normal, Strong, Master }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Difficulty difficulty = Difficulty.Normal;
    public bool isPlayerTurn = true;
    public bool gameOver = false;
    private BoardManager board;

    void Awake()
    {
        // 既にInstanceが存在する場合は古いものを破棄して新しいものを使う
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (PlayerPrefs.HasKey("Difficulty"))
            difficulty = (Difficulty)PlayerPrefs.GetInt("Difficulty");

        if (!PlayerPrefs.HasKey("PairIndex"))
            PlayerPrefs.SetInt("PairIndex", 0);

        board = BoardManager.Instance;

        // ゲーム開始時にボードを必ずリセット
        if (board != null) board.ResetBoard();

        isPlayerTurn = true;
        gameOver = false;
    }

    public void OnPlayerMove(HexCell cell)
    {
        if (!isPlayerTurn || gameOver) return;
        PlacePiece(cell, 1);
        int result = board.CheckResult(cell.q, cell.r, 1);
        if (result != 0) { EndGame(result); return; }
        isPlayerTurn = false;
        Invoke(nameof(DoCPUMove), 0.5f);
    }

    void DoCPUMove()
    {
        if (gameOver) return;
        HexCell choice = AIPlayer.Instance.ChooseMove(difficulty);
        if (choice == null) return;
        PlacePiece(choice, 2);
        int result = board.CheckResult(choice.q, choice.r, 2);
        if (result != 0) { EndGame(result); return; }
        isPlayerTurn = true;
    }

    void PlacePiece(HexCell cell, int owner)
    {
        cell.SetOwner(owner);
    }

    void EndGame(int result)
    {
        gameOver = true;
        ResultData.playerWon = (result == 1 || result == -2);
        Debug.Log($"[GameManager] EndGame called. result = {result}, playerWon = {ResultData.playerWon}");
        Invoke(nameof(LoadResult), 1.0f);
    }

    void LoadResult()
    {
        // ゲームオブジェクトを破棄してからシーン遷移
        if (board != null && board.gameObject != null)
            Destroy(board.gameObject);
        
        Destroy(gameObject);
        
        SceneLoader.Load("Result");
    }
}

public static class ResultData
{
    public static bool playerWon;
}