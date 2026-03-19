using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Settings")]
    public GameObject hexCellPrefab;
    public int boardRadius = 4; // 5-hexサイズ: radius=4

    // アキシャル座標 → HexCell の辞書
    public Dictionary<Vector2Int, HexCell> cells = new();

    // 六角形の6方向
    private static readonly Vector2Int[] directions = {
        new(1, 0), new(1, -1), new(0, -1),
        new(-1, 0), new(-1, 1), new(0, 1)
    };

    [Header("Hex Size")]
    public float hexSize = 0.6f;

    void Awake() => Instance = this;

    void Start() => GenerateBoard();

    // ---- ボード生成 ----
    void GenerateBoard()
    {
        for (int q = -boardRadius; q <= boardRadius; q++)
        {
            int r1 = Mathf.Max(-boardRadius, -q - boardRadius);
            int r2 = Mathf.Min(boardRadius, -q + boardRadius);
            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = HexToWorld(q, r);
                GameObject go = Instantiate(hexCellPrefab, pos, Quaternion.identity, transform);
                HexCell cell = go.GetComponent<HexCell>();
                cell.q = q;
                cell.r = r;
                cells[new Vector2Int(q, r)] = cell;
            }
        }
    }

    // アキシャル座標 → ワールド座標
    Vector3 HexToWorld(int q, int r)
    {
        float x = hexSize * (Mathf.Sqrt(3) * q + Mathf.Sqrt(3) / 2 * r);
        float y = hexSize * (3f / 2f * r);
        return new Vector3(x, y, 0);
    }

    // ---- セルクリック処理（GameManagerへ通知）----
    public void OnCellClicked(HexCell cell)
    {
        GameManager.Instance.OnPlayerMove(cell);
    }

    // ---- 勝敗チェック ----
    // 結果: 0=なし, 1=プレイヤー勝ち, 2=CPU勝ち, -1=プレイヤー負け(3連), -2=CPU負け(3連)
    public int CheckResult(int lastQ, int lastR, int owner)
    {
        foreach (var dir in directions)
        {
            int count = 1 + CountLine(lastQ, lastR, dir.x, dir.y, owner)
                          + CountLine(lastQ, lastR, -dir.x, -dir.y, owner);

            if (count >= 4) return owner; // 4連 → 勝利
            if (count == 3) return -owner; // 3連 → 即負け
        }
        return 0;
    }

    int CountLine(int q, int r, int dq, int dr, int owner)
    {
        int count = 0;
        for (int i = 1; i <= 4; i++)
        {
            var key = new Vector2Int(q + dq * i, r + dr * i);
            if (cells.TryGetValue(key, out HexCell cell) && cell.owner == owner)
                count++;
            else
                break;
        }
        return count;
    }

    // 全セルをリセット
    public void ResetBoard()
    {
        foreach (var cell in cells.Values)
            cell.SetOwner(0);
    }

    // 空きセル一覧
    public List<HexCell> GetEmptyCells()
    {
        var list = new List<HexCell>();
        foreach (var cell in cells.Values)
            if (cell.owner == 0) list.Add(cell);
        return list;
    }
    
    // 特定のオーナーのセル一覧
    public List<HexCell> GetOwnedCells(int owner)
    {
        var list = new List<HexCell>();
        foreach (var cell in cells.Values)
            if (cell.owner == owner) list.Add(cell);
        return list;
    }
    
    // 座標からセルを取得
    public HexCell GetCell(int q, int r)
    {
        var key = new Vector2Int(q, r);
        return cells.TryGetValue(key, out HexCell cell) ? cell : null;
    }
}