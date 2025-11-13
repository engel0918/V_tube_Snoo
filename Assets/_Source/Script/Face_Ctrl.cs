using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Face_Ctrl : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer eyeRenderer;
    public SpriteRenderer mouthRenderer;

    [Header("Eye Sprites")]
    public Sprite eyeOpen;
    public Sprite eyeClosed;

    [Header("Mouth Sprites (0=닫힘, 1=열림)")]
    public Sprite[] mouthSprites;

    [Header("Blink Settings")]
    public float blinkIntervalMin = 3f;
    public float blinkIntervalMax = 6f;
    public float blinkDuration = 0.15f;

    [Header("Mic Settings")]
    public string deviceName;

    [SerializeField] TMP_InputField Inf_mic_sense;

    public float micSensitivity = 80f;   // 감도 (낮을수록 둔감)

    [SerializeField] TMP_InputField Inf_threshold;
    public float threshold = 0.15f;      // 입 열릴 기준 볼륨 (0.1~0.3 추천)
    public int minFramesToOpen = 3;      // 연속된 프레임 중 커야 입 열림 (소음 무시용)

    private AudioClip micClip;
    private bool micInitialized = false;

    private float blinkTimer;
    private float blinkInterval;
    private bool isBlinking = false;

    private int loudCount = 0;           // 연속된 큰 소리 카운터

    void Start()
    {
        // 눈 초기화
        if (eyeRenderer != null)
            eyeRenderer.sprite = eyeOpen;
        blinkInterval = Random.Range(blinkIntervalMin, blinkIntervalMax);

        // 마이크 초기화
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
            micClip = Microphone.Start(deviceName, true, 1, 44100);
            micInitialized = true;
        }
        else
        {
            Debug.LogWarning("마이크 장치를 찾을 수 없습니다.");
        }

        Inf_mic_sense.text = micSensitivity.ToString();
        Inf_threshold.text = threshold.ToString();

        Inf_mic_sense.onValueChanged.AddListener((value) => inp_func("sense"));
        Inf_threshold.onValueChanged.AddListener((value) => inp_func("threshold"));
    }

    void Update()
    {
        HandleBlink();

        if (micInitialized)
        {
            float volume = GetMicVolume();

            // 연속된 큰 소리 감지
            if (volume > threshold)
                loudCount++;
            else
                loudCount = 0;

            // 연속적으로 일정 프레임 이상일 때만 입 열림
            if (loudCount >= minFramesToOpen)
                mouthRenderer.sprite = mouthSprites[1];
            else
                mouthRenderer.sprite = mouthSprites[0];
        }
    }

    void inp_func(string str)
    {
        if (str == "sense")
        { micSensitivity = float.Parse(Inf_mic_sense.text); }
        else if (str == "threshold")
        { threshold = float.Parse(Inf_threshold.text); }
    }

    void HandleBlink()
    {
        blinkTimer += Time.deltaTime;

        if (!isBlinking && blinkTimer >= blinkInterval)
        {
            StartCoroutine(BlinkRoutine());
        }
    }

    IEnumerator BlinkRoutine()
    {
        isBlinking = true;
        eyeRenderer.sprite = eyeClosed;
        yield return new WaitForSeconds(blinkDuration);
        eyeRenderer.sprite = eyeOpen;

        blinkInterval = Random.Range(blinkIntervalMin, blinkIntervalMax);
        blinkTimer = 0f;
        isBlinking = false;
    }

    float GetMicVolume()
    {
        const int sampleSize = 128;
        float[] data = new float[sampleSize];
        int micPosition = Microphone.GetPosition(deviceName) - sampleSize + 1;
        if (micPosition < 0) return 0;

        micClip.GetData(data, micPosition);
        float sum = 0f;
        for (int i = 0; i < sampleSize; i++)
            sum += data[i] * data[i];

        float rms = Mathf.Sqrt(sum / sampleSize);
        return rms * micSensitivity;
    }
}