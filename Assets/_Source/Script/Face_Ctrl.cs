using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Face_Ctrl : MonoBehaviour
{
    SaveData data;
    UI_Comp ui_comp;

    [Header("Sprite Renderers")]
    public SpriteRenderer eyeRenderer;
    public SpriteRenderer mouthRenderer;

    [Header("Equip")]
    [SerializeField] SpriteRenderer Glasses_renderer;
    public List<string> GlassesTit_list;
    public List<Sprite> GlassesSpr_list;


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
    public List<string> mic_List;
    public string sel_mic;

    public float micSensitivity = 80f;   // 감도 (낮을수록 둔감)

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
        // 데이터 받아오기
        data = GetComponent<SaveData>();
        ui_comp = GetComponent<UI_Comp>();


        threshold = data.mic_minsize;
        ui_comp.Inf_threshold.text = data.mic_minsize.ToString();

        micSensitivity = data.mic_Sense;
        ui_comp.Inf_mic_sense.text = data.mic_Sense.ToString();

        //--------------------------------------------------------------------------------

        // 눈 초기화
        if (eyeRenderer != null)
            eyeRenderer.sprite = eyeOpen;
        blinkInterval = Random.Range(blinkIntervalMin, blinkIntervalMax);

        // 마이크 초기화
        if (Microphone.devices.Length > 0)
        {
            sel_mic = Microphone.devices[0];
            micClip = Microphone.Start(sel_mic, true, 1, 44100);
            micInitialized = true;

            mic_List.Clear();
            mic_List = new List<string>(Microphone.devices);

            ui_comp.Dd_MicSel.ClearOptions();
            ui_comp.Dd_MicSel.AddOptions(new List<string>(Microphone.devices));
            ui_comp.Dd_MicSel.value = 0;
        }
        else
        {
            Debug.LogWarning("마이크 장치를 찾을 수 없습니다.");
        }

        ui_comp.Inf_mic_sense.text = micSensitivity.ToString();
        ui_comp.Inf_threshold.text = threshold.ToString();

        ui_comp.Dd_MicSel.onValueChanged.AddListener((value) => inp_func("mic"));
        ui_comp.Inf_mic_sense.onValueChanged.AddListener((value) => inp_func("sense"));
        ui_comp.Inf_threshold.onValueChanged.AddListener((value) => inp_func("threshold"));

        Dd_funcInput("Glasses", ui_comp.Dd_Glasses, GlassesTit_list);
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

    void Dd_funcInput(string Tit, TMP_Dropdown dd, List<string> str)
    {
        dd.ClearOptions();
        dd.AddOptions(new List<string>(str));
        dd.onValueChanged.AddListener((value) => Clothes_func(Tit));
    }

    void Clothes_func(string str)
    {
        if (str == "Glasses")
        {
            int i = ui_comp.Dd_Glasses.value;
            Glasses_renderer.sprite = GlassesSpr_list[i];
        }
    }

    void inp_func(string str)
    {
        if (str == "sense")
        { 
            micSensitivity = float.Parse(ui_comp.Inf_mic_sense.text);
            data.Save_Data("mic_Sense", ui_comp.Inf_mic_sense.text);
        }
        else if (str == "threshold")
        {
            threshold = float.Parse(ui_comp.Inf_threshold.text);
            data.Save_Data("mic_minsize", ui_comp.Inf_threshold.text);
        }
        else if (str == "mic")
        {
            sel_mic = mic_List[ui_comp.Dd_MicSel.value];
        }
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
        int micPosition = Microphone.GetPosition(sel_mic) - sampleSize + 1;
        if (micPosition < 0) return 0;

        micClip.GetData(data, micPosition);
        float sum = 0f;
        for (int i = 0; i < sampleSize; i++)
            sum += data[i] * data[i];

        float rms = Mathf.Sqrt(sum / sampleSize);
        return rms * micSensitivity;
    }
}