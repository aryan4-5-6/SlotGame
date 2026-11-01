using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns reel setup, animation, random outcomes, and the all-reels-match rule.</summary>
public class SlotMachineGame : MonoBehaviour
{
    [Header("Reels")]
    [SerializeField] private ScrollRect[] reels;
    [SerializeField] private Sprite[] symbols;
    [SerializeField, Min(3)] private int visibleRows = 3;
    [SerializeField, Min(3)] private int extraRowsBeforeStop = 18;
    [SerializeField, Min(0f)] private float symbolGap = 20f;
    [SerializeField, Min(0.1f)] private float spinDuration = 2.2f;
    [SerializeField, Min(0f)] private float staggerBetweenReels = 0.25f;

    public int[] LastResults { get; private set; }
    public bool LastSpinWon { get; private set; }

    public bool IsConfigured
    {
        get
        {
            if (reels == null || reels.Length == 0 || symbols == null || symbols.Length == 0)
                return false;
            foreach (Sprite symbol in symbols)
                if (symbol == null) return false;
            foreach (ScrollRect reel in reels)
                if (reel == null || reel.viewport == null || reel.content == null) return false;
            return true;
        }
    }

    public IEnumerator Spin()
    {
        LastResults = null;
        LastSpinWon = false;
        if (!IsConfigured)
        {
            Debug.LogError("SlotMachineGame needs reel ScrollRects and symbol sprites assigned.", this);
            yield break;
        }

        int[] results = new int[reels.Length];
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].horizontal = false;
            reels[i].vertical = false;
            reels[i].inertia = false;
            results[i] = Random.Range(0, symbols.Length);
            BuildStrip(reels[i], results[i]);
            StartCoroutine(AnimateReel(reels[i], i * staggerBetweenReels));
        }

        yield return new WaitForSeconds(spinDuration + (reels.Length - 1) * staggerBetweenReels);

        LastResults = results;
        LastSpinWon = true;
        for (int i = 1; i < results.Length; i++)
        {
            if (results[i] != results[0])
            {
                LastSpinWon = false;
                break;
            }
        }
    }

    public string SymbolNameAt(int symbolIndex)
    {
        if (symbols == null || symbolIndex < 0 || symbolIndex >= symbols.Length || symbols[symbolIndex] == null)
            return "Unknown";
        return symbols[symbolIndex].name;
    }

    private void BuildStrip(ScrollRect reel, int winningSymbol)
    {
        RectTransform content = reel.content;
        RectTransform viewport = reel.viewport;

        // The starter scene's Content has layout components that would reposition these cells.
        LayoutGroup layout = content.GetComponent<LayoutGroup>();
        if (layout != null) layout.enabled = false;
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;

        float cellHeight = (viewport.rect.height - symbolGap * (visibleRows - 1)) / visibleRows;
        float rowStep = cellHeight + symbolGap;
        float width = viewport.rect.width;
        int stopRow = extraRowsBeforeStop;
        int rowCount = stopRow + visibleRows + 2;

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(width, cellHeight * rowCount + symbolGap * (rowCount - 1));

        for (int row = 0; row < rowCount; row++)
        {
            int symbolIndex = row == stopRow + visibleRows / 2
                ? winningSymbol
                : Random.Range(0, symbols.Length);
            GameObject symbolObject = new GameObject("Symbol " + row, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            symbolObject.transform.SetParent(content, false);
            RectTransform rect = symbolObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -row * rowStep);
            rect.sizeDelta = new Vector2(width, cellHeight);

            Image image = symbolObject.GetComponent<Image>();
            image.sprite = symbols[symbolIndex];
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private IEnumerator AnimateReel(ScrollRect reel, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        RectTransform content = reel.content;
        float cellHeight = (reel.viewport.rect.height - symbolGap * (visibleRows - 1)) / visibleRows;
        float rowStep = cellHeight + symbolGap;
        float start = content.anchoredPosition.y;
        float end = rowStep * extraRowsBeforeStop;
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float eased = 1f - Mathf.Pow(1f - t, 4f);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Lerp(start, end, eased));
            yield return null;
        }

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, end);
    }
}
