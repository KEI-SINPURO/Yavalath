using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResultScreen : MonoBehaviour
{
    [Header("バナー")]
    public Image resultBanner;
    public Sprite winBanner;
    public Sprite loseBanner;

    [Header("背景（GameObjectで切り替え）")]
    public GameObject winBackground;
    public GameObject loseBackground;
    public Image backgroundDarkOverlay; // 背景を暗くするオーバーレイ（視認性向上）
    [Range(0f, 1f)]
    public float darkOverlayAlpha = 0.5f; // オーバーレイの透明度

    [Header("演出用")]
    public Image screenOverlay; // 画面全体を覆う黒いオーバーレイ
    public ResultParticleController particleController; // パーティクル制御
    public Canvas canvas; // Canvasへの参照

    [Header("ボタン（下から登場順）")]
    public Button retryBtn;
    public Button changeDiffBtn;
    public Button homeBtn;

    void Start()
    {
        bool playerWon = ResultData.playerWon;
        Debug.Log($"[ResultScreen] playerWon = {playerWon}");

        // バナー設定
        if (playerWon)
        {
            resultBanner.sprite = winBanner;
            Debug.Log($"[ResultScreen] Setting WIN banner: {winBanner?.name}");
        }
        else
        {
            resultBanner.sprite = loseBanner;
            Debug.Log($"[ResultScreen] Setting LOSE banner: {loseBanner?.name}");
        }

        // 背景の表示切り替え
        if (winBackground != null && loseBackground != null)
        {
            winBackground.SetActive(playerWon);
            loseBackground.SetActive(!playerWon);
            Debug.Log($"[ResultScreen] Win background active: {playerWon}, Lose background active: {!playerWon}");
        }
        else
        {
            Debug.LogError("[ResultScreen] Win or Lose Background GameObject is null!");
        }

        // 背景オーバーレイを初期状態で透明に
        if (backgroundDarkOverlay != null)
        {
            backgroundDarkOverlay.color = new Color(0, 0, 0, 0);
        }

        // ボタンのリスナー
        retryBtn.onClick.AddListener(() => SceneLoader.Load("Game"));
        changeDiffBtn.onClick.AddListener(() => SceneLoader.Load("Difficulty"));
        homeBtn.onClick.AddListener(() => SceneLoader.Load("Home"));

        // 演出開始
        StartCoroutine(PlayIntro(playerWon));
    }

    IEnumerator PlayIntro(bool playerWon)
    {
        // 最初は全部非表示
        resultBanner.color = new Color(1, 1, 1, 0);
        SetButtonAlpha(retryBtn, 0);
        SetButtonAlpha(changeDiffBtn, 0);
        SetButtonAlpha(homeBtn, 0);

        // 画面全体を黒で覆う
        if (screenOverlay != null)
        {
            screenOverlay.color = Color.black;
        }

        // ① 背景を中心から広がるように表示
        GameObject activeBackground = playerWon ? winBackground : loseBackground;
        if (activeBackground != null)
        {
            // 背景は最初から表示しておく（マスクで隠す）
            Image bgImage = activeBackground.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = Color.white;
            }

            // パーティクル演出を開始
            if (particleController != null)
            {
                try
                {
                    if (playerWon)
                        particleController.PlayWinEffect();
                    else
                        particleController.PlayLoseEffect();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ResultScreen] Particle effect failed: {e.Message}");
                }
            }

            // 装飾が広がるアニメーション開始（非同期）
            StartCoroutine(CircularReveal(activeBackground, 1.2f));
            
            // 0.3秒待ってからバナーとボタンを表示
            yield return new WaitForSeconds(0.3f);
        }

        // ② バナーフェードイン
        yield return StartCoroutine(FadeImage(resultBanner, 0f, 1f, 0.7f));

        yield return new WaitForSeconds(0.3f);

        // ③ ボタンが下から順番に登場
        yield return StartCoroutine(SlideInButton(retryBtn, 0.4f));
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(SlideInButton(changeDiffBtn, 0.4f));
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(SlideInButton(homeBtn, 0.4f));
    }

    IEnumerator CircularReveal(GameObject target, float duration)
    {
        // スクリーンオーバーレイを円形にフェードアウトさせる
        if (screenOverlay != null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // イーズアウト（加速しながら終わる）
                t = t * t * (3f - 2f * t); // smoothstep
                
                Color c = screenOverlay.color;
                c.a = 1f - t;
                screenOverlay.color = c;
                
                yield return null;
            }
            screenOverlay.color = new Color(0, 0, 0, 0);
        }

        // 背景が表示された後、暗いオーバーレイをフェードイン（視認性向上）- 短縮
        if (backgroundDarkOverlay != null)
        {
            yield return StartCoroutine(FadeImage(backgroundDarkOverlay, 0f, darkOverlayAlpha, 0.3f));
        }
    }

    IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // イーズアウト
            t = 1f - (1f - t) * (1f - t);
            c.a = Mathf.Lerp(from, to, t);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }

    IEnumerator SlideInButton(Button btn, float duration)
    {
        RectTransform rt = btn.GetComponent<RectTransform>();
        Vector2 targetPos = rt.anchoredPosition;
        Vector2 startPos = targetPos + new Vector2(0, -80f);

        float elapsed = 0f;
        SetButtonAlpha(btn, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // イーズアウト
            t = 1f - (1f - t) * (1f - t);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            SetButtonAlpha(btn, t);
            yield return null;
        }

        rt.anchoredPosition = targetPos;
        SetButtonAlpha(btn, 1f);
    }

    void SetButtonAlpha(Button btn, float alpha)
    {
        // Imageコンポーネント
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
        
        // 子のTextMeshPro - 常に完全に不透明
        var tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = 1f; // テキストは常に完全に表示
            tmp.color = c;
        }
        
        // 子のすべてのImageも表示
        Image[] childImages = btn.GetComponentsInChildren<Image>();
        foreach (Image childImg in childImages)
        {
            if (childImg != img) // ボタン本体以外
            {
                Color c = childImg.color;
                c.a = alpha;
                childImg.color = c;
            }
        }
    }
}