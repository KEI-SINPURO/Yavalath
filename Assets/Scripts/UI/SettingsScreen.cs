using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [Header("ボタン")]
    public Button backButton;
    public Button[] pairButtons;

    [Header("プレビュー")]
    public Image playerPreview;
    public Image cpuPreview;

    [Header("コマスプライト（ペア順）")]
    public Sprite[] playerSprites; // 白, 緑, 水色, 金
    public Sprite[] cpuSprites;    // 黒, 赤, オレンジ, 紫

    private int selectedPairIndex = 0;

    void Start()
    {
        selectedPairIndex = PlayerPrefs.GetInt("PairIndex", 0);
        UpdatePreview();
        UpdateButtonHighlight();

        for (int i = 0; i < pairButtons.Length; i++)
        {
            int index = i;
            pairButtons[i].onClick.AddListener(() => SelectPair(index));
        }

        backButton.onClick.AddListener(() => SceneLoader.Load("Home"));
    }

    void SelectPair(int index)
    {
        selectedPairIndex = index;
        PlayerPrefs.SetInt("PairIndex", index);
        UpdatePreview();
        UpdateButtonHighlight();
    }

    void UpdatePreview()
    {
        if (playerPreview != null && playerSprites.Length > selectedPairIndex)
            playerPreview.sprite = playerSprites[selectedPairIndex];
        if (cpuPreview != null && cpuSprites.Length > selectedPairIndex)
            cpuPreview.sprite = cpuSprites[selectedPairIndex];
    }

    void UpdateButtonHighlight()
    {
        for (int i = 0; i < pairButtons.Length; i++)
        {
            var colors = pairButtons[i].colors;
            colors.normalColor = (i == selectedPairIndex)
                ? new Color(0.3f, 0.6f, 1.0f)
                : new Color(0.1f, 0.2f, 0.4f);
            pairButtons[i].colors = colors;
        }
    }
}
