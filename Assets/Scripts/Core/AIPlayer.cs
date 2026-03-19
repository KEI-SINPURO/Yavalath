using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public static AIPlayer Instance;
    void Awake() => Instance = this;

    public HexCell ChooseMove(Difficulty diff)
    {
        return diff switch
        {
            Difficulty.Weak => WeakMove(),
            Difficulty.Normal => NormalMove(),
            Difficulty.Strong => StrongMove(), // 強い専用
            Difficulty.Master => MasterMove(), // 達人専用
            _ => RandomMove()
        };
    }

    // ランダムに置く
    HexCell RandomMove()
    {
        var empty = BoardManager.Instance.GetEmptyCells();
        return empty.Count > 0 ? empty[Random.Range(0, empty.Count)] : null;
    }
    
    // 相手の2個の間を塞ぐ（最優先）
    HexCell BlockOpponentGap(BoardManager board, List<HexCell> empty)
    {
        // 6つの方向
        int[][] directions = new int[][] {
            new int[] {1, 0},   // 右
            new int[] {0, 1},   // 右上
            new int[] {-1, 1},  // 左上
        };
        
        // パターン1: ○ □ ○ （隣接）
        foreach (var cell in empty)
        {
            foreach (var dir in directions)
            {
                var prev = board.GetCell(cell.q - dir[0], cell.r - dir[1]);
                var next = board.GetCell(cell.q + dir[0], cell.r + dir[1]);
                
                // 両側がプレイヤー（owner=1）のコマか？
                if (prev != null && prev.owner == 1 && 
                    next != null && next.owner == 1)
                {
                    Debug.Log($"[AI] Blocking gap at ({cell.q}, {cell.r}) - Pattern: P _ P");
                    return cell;
                }
            }
        }
        
        // パターン2: ○ □ □ ○ （1マス空き）
        foreach (var cell in empty)
        {
            foreach (var dir in directions)
            {
                // このセルから見て、片側が空いていて、その先がプレイヤー
                var side1 = board.GetCell(cell.q + dir[0], cell.r + dir[1]);
                var side2 = board.GetCell(cell.q + dir[0] * 2, cell.r + dir[1] * 2);
                
                if (side1 != null && side1.owner == 0 &&  // 隣が空
                    side2 != null && side2.owner == 1)    // その先がプレイヤー
                {
                    // 反対側をチェック
                    var opposite = board.GetCell(cell.q - dir[0], cell.r - dir[1]);
                    if (opposite != null && opposite.owner == 1)
                    {
                        Debug.Log($"[AI] Blocking wide gap at ({cell.q}, {cell.r}) - Pattern: P _ _ P");
                        return cell;
                    }
                }
                
                // 逆方向も同様にチェック
                var side3 = board.GetCell(cell.q - dir[0], cell.r - dir[1]);
                var side4 = board.GetCell(cell.q - dir[0] * 2, cell.r - dir[1] * 2);
                
                if (side3 != null && side3.owner == 0 &&  // 隣が空
                    side4 != null && side4.owner == 1)    // その先がプレイヤー
                {
                    // 反対側をチェック
                    var opposite2 = board.GetCell(cell.q + dir[0], cell.r + dir[1]);
                    if (opposite2 != null && opposite2.owner == 1)
                    {
                        Debug.Log($"[AI] Blocking wide gap at ({cell.q}, {cell.r}) - Pattern: P _ _ P");
                        return cell;
                    }
                }
            }
        }
        
        return null; // 見つからなかった
    }

    // 弱い：ランダム（70%）+ たまに賢い手（30%）
    HexCell WeakMove()
    {
        if (Random.value < 0.3f) return NormalMove();
        return RandomMove();
    }
    
    // 強い専用：優先順位を考慮 + Minimax
    HexCell StrongMove()
    {
        var board = BoardManager.Instance;
        var empty = board.GetEmptyCells();
        
        // 1. 自分が4連で勝てる → 勝つ
        foreach (var cell in empty)
        {
            cell.owner = 2;
            int r = board.CheckResult(cell.q, cell.r, 2);
            cell.owner = 0;
            if (r == 2) return cell;
        }
        
        // 2. 自分が3連で負ける手を全て除外
        var safeMoves = empty.FindAll(cell =>
        {
            cell.owner = 2;
            int r = board.CheckResult(cell.q, cell.r, 2);
            cell.owner = 0;
            return r != -2;
        });
        
        var candidates = safeMoves.Count > 0 ? safeMoves : empty;
        
        // 3. 相手が4連で勝つ → 防ぐ
        foreach (var cell in candidates)
        {
            cell.owner = 1;
            int r = board.CheckResult(cell.q, cell.r, 1);
            cell.owner = 0;
            if (r == 1) return cell;
        }
        
        // 4. Minimaxで探索（安全な手の中から）
        return MinimaxMoveWithCandidates(3, candidates);
    }
    
    // 達人専用：優先順位を考慮した最善手
    HexCell MasterMove()
    {
        var board = BoardManager.Instance;
        var empty = board.GetEmptyCells();
        
        // 1. 自分が4連で勝てる → 勝つ
        foreach (var cell in empty)
        {
            cell.owner = 2;
            int r = board.CheckResult(cell.q, cell.r, 2);
            cell.owner = 0;
            if (r == 2)
            {
                Debug.Log("[AI Master] Winning move!");
                return cell;
            }
        }
        
        // 2. 自分が3連で負ける手を全て除外
        var safeMoves = empty.FindAll(cell =>
        {
            cell.owner = 2;
            int r = board.CheckResult(cell.q, cell.r, 2);
            cell.owner = 0;
            return r != -2; // 3連で即負けにならない手のみ
        });
        
        // 安全な手がない場合は全ての手を候補に
        var candidates = safeMoves.Count > 0 ? safeMoves : empty;
        
        // 3. 相手が4連で勝つ → 防ぐ
        foreach (var cell in candidates)
        {
            cell.owner = 1;
            int r = board.CheckResult(cell.q, cell.r, 1);
            cell.owner = 0;
            if (r == 1)
            {
                Debug.Log("[AI Master] Blocking opponent's winning move!");
                return cell;
            }
        }
        
        // 4. 相手の2個の間 → 塞ぐ
        var blockMove = BlockOpponentGap(board, candidates);
        if (blockMove != null) return blockMove;
        
        // 5. その後、Minimaxで探索（安全な手の中から）
        return MinimaxMoveWithCandidates(3, candidates);
    }

    // 普通：即勝ち・即負け回避・相手の勝ち防止
    HexCell NormalMove()
    {
        var board = BoardManager.Instance;
        var empty = board.GetEmptyCells();

        // 1. 自分が4連になるマスを探す
        foreach (var cell in empty)
        {
            cell.owner = 2;
            int r = board.CheckResult(cell.q, cell.r, 2);
            cell.owner = 0;
            if (r == 2) return cell;
        }

        // 2. 自分が3連になるマスを除外
        var safe = empty.FindAll(cell =>
        {
            cell.owner = 2;
            int r = BoardManager.Instance.CheckResult(cell.q, cell.r, 2);
            cell.owner = 0;
            return r != -2;
        });

        var candidates = safe.Count > 0 ? safe : empty;

        // 3. 相手が4連になるマスを塞ぐ（安全な手の中から）
        foreach (var cell in candidates)
        {
            cell.owner = 1;
            int r = board.CheckResult(cell.q, cell.r, 1);
            cell.owner = 0;
            if (r == 1) return cell;
        }

        // 4. ランダムに選ぶ（安全な手の中から）
        return candidates[Random.Range(0, candidates.Count)];
    }

    // 強い・達人：Minimax + αβ枝刈り
    HexCell MinimaxMove(int depth)
    {
        var empty = BoardManager.Instance.GetEmptyCells();
        return MinimaxMoveWithCandidates(depth, empty);
    }
    
    // 候補を絞った状態でMinimax実行
    HexCell MinimaxMoveWithCandidates(int depth, List<HexCell> candidates)
    {
        HexCell best = null;
        int bestScore = int.MinValue;

        foreach (var cell in candidates)
        {
            cell.owner = 2;
            int r = BoardManager.Instance.CheckResult(cell.q, cell.r, 2);

            int score;
            if (r == 2) score = 10000;
            else if (r == -2) score = -10000;
            else score = Minimax(depth - 1, false, int.MinValue, int.MaxValue);

            cell.owner = 0;

            if (score > bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }

        return best ?? RandomMove();
    }

    int Minimax(int depth, bool isMax, int alpha, int beta)
    {
        var board = BoardManager.Instance;
        var empty = board.GetEmptyCells();

        if (depth == 0 || empty.Count == 0) return EvaluateBoard();

        if (isMax) // CPUのターン
        {
            int maxScore = int.MinValue;
            foreach (var cell in empty)
            {
                cell.owner = 2;
                int r = board.CheckResult(cell.q, cell.r, 2);
                int score = (r == 2) ? 10000 : (r == -2) ? -10000 : Minimax(depth - 1, false, alpha, beta);
                cell.owner = 0;
                maxScore = Mathf.Max(maxScore, score);
                alpha = Mathf.Max(alpha, score);
                if (beta <= alpha) break;
            }
            return maxScore;
        }
        else // プレイヤーのターン
        {
            int minScore = int.MaxValue;
            foreach (var cell in empty)
            {
                cell.owner = 1;
                int r = board.CheckResult(cell.q, cell.r, 1);
                int score = (r == 1) ? -10000 : (r == -1) ? 10000 : Minimax(depth - 1, true, alpha, beta);
                cell.owner = 0;
                minScore = Mathf.Min(minScore, score);
                beta = Mathf.Min(beta, score);
                if (beta <= alpha) break;
            }
            return minScore;
        }
    }

    // 盤面評価（連続コマ数、位置、戦略的価値をカウント）
    int EvaluateBoard()
    {
        var board = BoardManager.Instance;
        int score = 0;
        
        // CPU（AI）の評価
        score += EvaluatePlayer(board, 2);
        
        // プレイヤーの評価（マイナス）
        score -= EvaluatePlayer(board, 1);
        
        return score;
    }
    
    // プレイヤーごとの評価
    int EvaluatePlayer(BoardManager board, int owner)
    {
        int score = 0;
        var cells = board.GetOwnedCells(owner);
        
        foreach (var cell in cells)
        {
            // 中央付近のボーナス（戦略的に重要）
            int distanceFromCenter = Mathf.Abs(cell.q) + Mathf.Abs(cell.r) + Mathf.Abs(-cell.q - cell.r);
            if (distanceFromCenter <= 2) score += 15; // 中央
            else if (distanceFromCenter <= 4) score += 8; // 中央寄り
            
            // 連続コマの評価
            score += EvaluateLines(board, cell, owner);
        }
        
        // フォーク（二重の脅威）の検出
        score += DetectForks(board, owner);
        
        return score;
    }
    
    // 連続コマの評価（全6方向をチェック）
    int EvaluateLines(BoardManager board, HexCell cell, int owner)
    {
        int score = 0;
        
        // 6つの方向をチェック
        int[][] directions = new int[][] {
            new int[] {1, 0},   // 右
            new int[] {0, 1},   // 右上
            new int[] {-1, 1},  // 左上
            new int[] {-1, 0},  // 左
            new int[] {0, -1},  // 左下
            new int[] {1, -1}   // 右下
        };
        
        foreach (var dir in directions)
        {
            int count = CountInLine(board, cell.q, cell.r, dir[0], dir[1], owner);
            
            if (count >= 3)
            {
                score += 300; // 3連 - 非常に強い
            }
            else if (count == 2)
            {
                // 2連の価値は「伸びしろ」で判定
                bool canExtend = CanExtendLine(board, cell.q, cell.r, dir[0], dir[1], owner, count);
                score += canExtend ? 80 : 40; // 伸ばせる2連は価値が高い
            }
        }
        
        return score;
    }
    
    // 指定方向に連続しているコマ数をカウント
    int CountInLine(BoardManager board, int q, int r, int dq, int dr, int owner)
    {
        int count = 1; // 自分自身
        
        // 正方向にカウント
        for (int i = 1; i < 4; i++)
        {
            var nextCell = board.GetCell(q + dq * i, r + dr * i);
            if (nextCell == null || nextCell.owner != owner) break;
            count++;
        }
        
        // 逆方向にカウント
        for (int i = 1; i < 4; i++)
        {
            var nextCell = board.GetCell(q - dq * i, r - dr * i);
            if (nextCell == null || nextCell.owner != owner) break;
            count++;
        }
        
        return count;
    }
    
    // ラインが伸ばせるかチェック（両端に空きがあるか）
    bool CanExtendLine(BoardManager board, int q, int r, int dq, int dr, int owner, int currentLength)
    {
        // 正方向の端
        var forwardEnd = board.GetCell(q + dq * currentLength, r + dr * currentLength);
        bool forwardEmpty = (forwardEnd != null && forwardEnd.owner == 0);
        
        // 逆方向の端
        var backwardEnd = board.GetCell(q - dq, r - dr);
        bool backwardEmpty = (backwardEnd != null && backwardEnd.owner == 0);
        
        return forwardEmpty || backwardEmpty;
    }
    
    // フォーク（二重の脅威）を検出（最適化版）
    int DetectForks(BoardManager board, int owner)
    {
        int forkScore = 0;
        var empty = board.GetEmptyCells();
        
        // 最適化: 既存のコマの近くのマスのみチェック（候補を絞る）
        var candidates = new List<HexCell>();
        var ownedCells = board.GetOwnedCells(owner);
        
        foreach (var ownedCell in ownedCells)
        {
            // 既存のコマの周囲2マス以内のみ
            foreach (var emptyCell in empty)
            {
                int dist = Mathf.Abs(emptyCell.q - ownedCell.q) + 
                          Mathf.Abs(emptyCell.r - ownedCell.r) + 
                          Mathf.Abs((-emptyCell.q - emptyCell.r) - (-ownedCell.q - ownedCell.r));
                
                if (dist <= 4 && !candidates.Contains(emptyCell))
                {
                    candidates.Add(emptyCell);
                }
            }
        }
        
        // 候補が少ない場合は全マスチェック
        if (candidates.Count == 0) candidates = empty;
        
        foreach (var cell in candidates)
        {
            // この位置に置いた場合の脅威数をカウント
            cell.owner = owner;
            int threats = CountThreatsFast(board, cell, owner);
            cell.owner = 0;
            
            if (threats >= 2)
            {
                // 二重の脅威（フォーク）= 相手は防げない
                forkScore += 1000;
            }
            else if (threats == 1)
            {
                // 単一の脅威
                forkScore += 100;
            }
        }
        
        return forkScore;
    }
    
    // このセルに置いた時、何個の「次に勝てる状態」ができるか（高速版）
    int CountThreatsFast(BoardManager board, HexCell cell, int owner)
    {
        int threatCount = 0;
        
        // 6つの方向をチェック
        int[][] directions = new int[][] {
            new int[] {1, 0},   // 右
            new int[] {0, 1},   // 右上
            new int[] {-1, 1},  // 左上
        };
        
        // 対称性を利用して3方向のみチェック（逆方向は自動的にカウントされる）
        foreach (var dir in directions)
        {
            // この方向で3連ができるか（=次に4連にできる）
            int count = CountInLine(board, cell.q, cell.r, dir[0], dir[1], owner);
            
            if (count >= 3)
            {
                // 3連以上 → 脅威としてカウント
                // 簡易版：IsSafeThreatの詳細チェックは省略（高速化）
                threatCount++;
            }
        }
        
        return threatCount;
    }
    
    // この脅威が安全か（3連で即負けにならないか）
    bool IsSafeThreat(BoardManager board, HexCell cell, int dq, int dr, int owner)
    {
        // この方向の3連が、実際には4連の一部になれるかチェック
        // つまり、両端のどちらかに空きマスがあるか
        
        int count = CountInLine(board, cell.q, cell.r, dq, dr, owner);
        
        if (count < 3) return false;
        
        // 正方向に伸ばせるか
        var forwardCell = board.GetCell(cell.q + dq * 3, cell.r + dr * 3);
        bool canExtendForward = (forwardCell != null && forwardCell.owner == 0);
        
        // 逆方向に伸ばせるか
        var backwardCell = board.GetCell(cell.q - dq, cell.r - dr);
        bool canExtendBackward = (backwardCell != null && backwardCell.owner == 0);
        
        // どちらかに伸ばせれば、4連が作れる可能性がある
        return canExtendForward || canExtendBackward;
    }
}