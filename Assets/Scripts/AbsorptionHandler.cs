using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AbsorptionHandler : MonoBehaviour
{
    [Header("Объекты и настройки")]
    [SerializeField] private GameObject eart;
    [SerializeField] private string[] absorbableTags = { "Planet", "Enemy", "TheBlackHole" };

    [Header("Масштабы")]
    public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 maxScale = new Vector3(5f, 5f, 5f);
    public float blackHoleShrinkRate = 0.3f;

    [Header("Прогресс и урон")]
    public int absorbsPerStage = 3;
    public int hitsToRegress = 2;

    [Header("Эволюция")]
    public Color[] stageColors;
    public string[] growthStageTexts;
    public Sprite finalStageSprite;
    public string finalCompletionText = "Планета собрана!";
    public Vector3 finalStageScale = new Vector3(6f, 6f, 6f);
    public float finalScaleTransitionTime = 1.5f;

    [Header("UI")]
    [SerializeField] private Image progressImage;
    [SerializeField] private Text stageText;

    [Header("Визуальные эффекты")]
    [SerializeField] private SpriteRenderer auraRenderer;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color blackHoleColor = Color.magenta;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float auraFadeSpeed = 5f;

    [Header("Звуки")]
    [SerializeField] AudioSource planetSound;
    [SerializeField] AudioSource enemySound;
    [SerializeField] AudioSource theBlackHoleSound;
    [SerializeField] AudioSource finalSound;

    private SpriteRenderer sr;
    private ObjectSpawner spawner;
    private int currentStageIndex = 0;
    private int absorbCounter = 0;
    private int damageCounter = 0;
    private int activeBlackHoles = 0;
    private Coroutine blackHoleDrainCoroutine;
    private bool evolutionComplete = false;
    private float currentFill = 0f;

    [System.Obsolete]
    private void Start()
    {
        if (eart != null)
            sr = eart.GetComponent<SpriteRenderer>();

        spawner = FindObjectOfType<ObjectSpawner>();

        ApplyStageVisuals();
        UpdateStageText();
        UpdateProgressUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (evolutionComplete) return;

        string tag = other.tag;
        if (!System.Array.Exists(absorbableTags, t => t == tag)) return;

        switch (tag)
        {
            case "Planet":
                if (planetSound != null) planetSound.PlayOneShot(planetSound.clip);
                damageCounter = 0;
                absorbCounter++;
                StartCoroutine(FlashAuraColor(healColor));
                if (absorbCounter >= absorbsPerStage)
                {
                    absorbCounter = 0;
                    if (currentStageIndex < stageColors.Length - 1)
                    {
                        currentStageIndex++;
                        ApplyStageVisuals();
                        UpdateStageText();
                    }
                    else
                    {
                        FinalizeEvolution();
                        return;
                    }
                }
                UpdateProgressUI();
                HandleObjectRecycle(other.gameObject, tag);
                break;

            case "Enemy":
                if (enemySound != null) enemySound.PlayOneShot(enemySound.clip);
                absorbCounter = 0;
                damageCounter++;
                StartCoroutine(FlashAuraColor(damageColor));
                if (damageCounter >= hitsToRegress)
                {
                    damageCounter = 0;
                    if (currentStageIndex > 0)
                    {
                        currentStageIndex--;
                        ApplyStageVisuals();
                        UpdateStageText();
                    }
                }
                UpdateProgressUI();
                HandleObjectRecycle(other.gameObject, tag);
                break;

            case "TheBlackHole":
                if (theBlackHoleSound != null) theBlackHoleSound.PlayOneShot(theBlackHoleSound.clip);
                activeBlackHoles++;
                if (blackHoleDrainCoroutine == null)
                    blackHoleDrainCoroutine = StartCoroutine(DrainAbsorptionOverTime());
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("TheBlackHole"))
        {
            activeBlackHoles = Mathf.Max(0, activeBlackHoles - 1);
            if (activeBlackHoles == 0 && blackHoleDrainCoroutine != null)
            {
                StopCoroutine(blackHoleDrainCoroutine);
                blackHoleDrainCoroutine = null;
                RestoreAuraToStageColor();
            }
        }
    }

    private IEnumerator DrainAbsorptionOverTime()
    {
        WaitForSeconds delay = new WaitForSeconds(blackHoleShrinkRate);
        while (!evolutionComplete && activeBlackHoles > 0)
        {
            StartCoroutine(FlashAuraColor(blackHoleColor));
            if (absorbCounter > 0)
            {
                absorbCounter--;
            }
            else if (currentStageIndex > 0)
            {
                currentStageIndex--;
                absorbCounter = absorbsPerStage - 1;
                ApplyStageVisuals();
                UpdateStageText();
                StartCoroutine(FlashAuraColor(blackHoleColor));
            }
            UpdateProgressUI();
            yield return delay;
        }

        RestoreAuraToStageColor();
    }

    private IEnumerator FlashAuraColor(Color flash)
    {
        if (auraRenderer == null) yield break;

        auraRenderer.color = flash;
        yield return new WaitForSeconds(flashDuration);

        float t = 0f;
        Color target = evolutionComplete ? Color.white : stageColors[Mathf.Clamp(currentStageIndex, 0, stageColors.Length - 1)];
        while (t < 1f)
        {
            t += Time.deltaTime * auraFadeSpeed;
            auraRenderer.color = Color.Lerp(flash, target, t);
            yield return null;
        }

        RestoreAuraToStageColor();
    }

    private void RestoreAuraToStageColor()
    {
        if (auraRenderer == null) return;

        Color target = evolutionComplete ? Color.white : stageColors[Mathf.Clamp(currentStageIndex, 0, stageColors.Length - 1)];
        auraRenderer.color = new Color(target.r, target.g, target.b, 1f);
    }

    private void FinalizeEvolution()
    {
        evolutionComplete = true;
        if (finalSound != null) finalSound.PlayOneShot(finalSound.clip);

        if (finalStageSprite != null)
            sr.sprite = finalStageSprite;

        sr.color = Color.white;
        if (auraRenderer != null) auraRenderer.color = Color.white;

        StartCoroutine(SmoothGrowToFinalScale());

        if (progressImage != null)
        {
            StopCoroutine(nameof(AnimateFill));
            progressImage.fillAmount = 0f;
            progressImage.enabled = false;
        }

        if (stageText != null && !string.IsNullOrEmpty(finalCompletionText))
            stageText.text = finalCompletionText;
    }

    private void ApplyStageVisuals()
    {
        if (sr == null || stageColors.Length == 0 || evolutionComplete) return;

        float ratio = Mathf.InverseLerp(0, stageColors.Length - 1, currentStageIndex);
        eart.transform.localScale = Vector3.Lerp(minScale, maxScale, ratio);

        Color stageColor = stageColors[Mathf.Clamp(currentStageIndex, 0, stageColors.Length - 1)];
        sr.color = new Color(stageColor.r, stageColor.g, stageColor.b, 1f);
        if (auraRenderer != null) auraRenderer.color = sr.color;
    }

    private void UpdateStageText()
    {
        if (stageText == null || growthStageTexts.Length == 0) return;
        int index = Mathf.Clamp(currentStageIndex, 0, growthStageTexts.Length - 1);
        stageText.text = growthStageTexts[index];
    }

    private void UpdateProgressUI()
    {
        if (progressImage == null || evolutionComplete) return;

        float baseFill = 0.25f;
        float stageProgress = Mathf.Clamp01((float)absorbCounter / absorbsPerStage);
        float targetFill = baseFill + stageProgress * (1f - baseFill);

        Color stageColor = stageColors[Mathf.Clamp(currentStageIndex, 0, stageColors.Length - 1)];
        stageColor.a = progressImage.color.a;
        progressImage.color = stageColor;

        StopCoroutine(nameof(AnimateFill));
        StartCoroutine(AnimateFill(targetFill));
    }

    private IEnumerator AnimateFill(float target)
    {
        if (evolutionComplete) yield break;

        float duration = 0.25f;
        float elapsed = 0f;
        float start = currentFill;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentFill = Mathf.Lerp(start, target, elapsed / duration);
            progressImage.fillAmount = currentFill;
            yield return null;
        }

        currentFill = target;
        progressImage.fillAmount = currentFill;
    }






    private IEnumerator SmoothGrowToFinalScale()
    {
        float duration = finalScaleTransitionTime;
        float elapsed = 0f;
        Vector3 initialScale = eart.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            eart.transform.localScale = Vector3.Lerp(initialScale, finalStageScale, smoothT);
            yield return null;
        }

        eart.transform.localScale = finalStageScale;
    }

    private void HandleObjectRecycle(GameObject obj, string tag)
    {
        obj.SetActive(false);
        if (spawner != null)
        {
            spawner.RequestRespawn(tag);
        }
    }
}