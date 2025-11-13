using UnityEngine;

public class Face_Ctrl : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer eyeRenderer;
    public SpriteRenderer mouthRenderer;

    [Header("Eye Sprites")]
    public Sprite eyeOpen;
    public Sprite eyeClosed;

    [Header("Mouth Sprites (0:닫힘, 1:중간, 2:열림)")]
    public Sprite[] mouthSprites;

    [Header("Blink Settings")]
    public float blinkIntervalMin = 3f;
    public float blinkIntervalMax = 6f;
    public float blinkDuration = 0.15f;

    [Header("Mouth Settings")]
    public float mouthChangeInterval = 0.1f; // 입 프레임 변경 속도
    public bool simulateTalking = true; // 임시 랜덤 립싱크

    private float blinkTimer;
    private float blinkInterval;
    private bool isBlinking = false;

    private float mouthTimer;

    void Start()
    {
        blinkInterval = Random.Range(blinkIntervalMin, blinkIntervalMax);
        eyeRenderer.sprite = eyeOpen;
        mouthRenderer.sprite = mouthSprites[0];
    }

    void Update()
    {
        HandleBlink();
        HandleMouth();
    }

    void HandleBlink()
    {
        blinkTimer += Time.deltaTime;

        if (!isBlinking && blinkTimer >= blinkInterval)
        {
            StartCoroutine(BlinkRoutine());
        }
    }

    System.Collections.IEnumerator BlinkRoutine()
    {
        isBlinking = true;
        eyeRenderer.sprite = eyeClosed;
        yield return new WaitForSeconds(blinkDuration);
        eyeRenderer.sprite = eyeOpen;

        // 다음 깜빡임까지 대기
        blinkInterval = Random.Range(blinkIntervalMin, blinkIntervalMax);
        blinkTimer = 0f;
        isBlinking = false;
    }

    void HandleMouth()
    {
        if (!simulateTalking) return;

        mouthTimer += Time.deltaTime;
        if (mouthTimer >= mouthChangeInterval)
        {
            int rand = Random.Range(0, mouthSprites.Length);
            mouthRenderer.sprite = mouthSprites[rand];
            mouthTimer = 0f;
        }
    }

    // 외부(마이크 입력 등)에서 직접 제어할 수도 있음
    public void SetMouthFrame(int index)
    {
        if (index >= 0 && index < mouthSprites.Length)
            mouthRenderer.sprite = mouthSprites[index];
    }

    public void SetTalking(bool talking)
    {
        simulateTalking = talking;
        if (!talking)
            mouthRenderer.sprite = mouthSprites[0]; // 닫힌 입으로 복귀
    }
}