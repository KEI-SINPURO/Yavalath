using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public int q, r;
    public int owner = 0;
    private SpriteRenderer spriteRenderer;

    [Header("Colors")]
    public Color emptyColor = new Color(0.08f, 0.14f, 0.25f);
    public Color hoverColor = new Color(0.16f, 0.32f, 0.51f);

    [Header("Piece Sprites")]
    public Sprite[] playerPieceSprites; // 白, 緑, 水色, 金
    public Sprite[] cpuPieceSprites;    // 黒, 赤, オレンジ, 紫

    private GameObject pieceObject;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetColor(emptyColor);
    }

    public void SetOwner(int newOwner)
    {
        owner = newOwner;
        if (pieceObject != null) Destroy(pieceObject);
        int pairIndex = PlayerPrefs.GetInt("PairIndex", 0);
        if (owner == 1 && playerPieceSprites?.Length > pairIndex)
            ShowPiece(playerPieceSprites[pairIndex]);
        else if (owner == 2 && cpuPieceSprites?.Length > pairIndex)
            ShowPiece(cpuPieceSprites[pairIndex]);
        SetColor(emptyColor);
    }

    void ShowPiece(Sprite sprite)
    {
        pieceObject = new GameObject("Piece");
        pieceObject.transform.SetParent(transform);
        pieceObject.transform.localPosition = new Vector3(0, 0, -0.1f);
        pieceObject.transform.localScale = Vector3.one * 0.8f;
        var sr = pieceObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;
    }

    public void SetColor(Color c)
    {
        if (spriteRenderer != null) spriteRenderer.color = c;
    }

    // ポーズ中かどうか確認
    bool IsPaused()
    {
        var gs = FindObjectOfType<GameScreen>();
        if (gs != null) return gs.isPaused;
        return false;
    }

    void OnMouseEnter()
    {
        if (owner == 0 && !IsPaused())
            SetColor(hoverColor);
    }

    void OnMouseExit()
    {
        if (owner == 0)
            SetColor(emptyColor);
    }

    void OnMouseDown()
    {
        if (owner == 0 && !IsPaused())
            BoardManager.Instance.OnCellClicked(this);
    }
}