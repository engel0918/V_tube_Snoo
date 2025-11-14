using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static UnityEngine.LightProbeProxyVolume;

public class SaveData : MonoBehaviour
{

    // setting
    public float mic_minsize, mic_Sense;
    public string mic;

    // UI 숨김 상태
    public bool UI_Hide;

    //--------------------------------------------------------------------------------

    string filePath = Path.Combine(Application.streamingAssetsPath, "config.ini");

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    static extern long WritePrivateProfileString(
  string Setting, string Key, string Value, string FilePath);
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    static extern int GetPrivateProfileString(
        string Setting, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

    private void Awake()
    {

        // ini 파일 존재 확인
        if (!File.Exists(filePath))
        {
            Debug.Log("INI 파일 없음. 새로 생성합니다.");
            CreateDefaultIni();
        }
        else
        {
            //로드
            mic_minsize = float.Parse(ReadIni(filePath, "Setting", "mic_minsize"));
            mic_Sense = float.Parse(ReadIni(filePath, "Setting", "mic_Sense"));

            string boolData = ReadIni(filePath, "Setting", "ui_hide");
            bool.TryParse(boolData, out UI_Hide);


        }
    }

    private void CreateDefaultIni()
    {
        // 스트리밍에셋 폴더가 없으면 생성
        string dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // 기본값 저장

        mic_minsize = 0.5f;
        mic_Sense = 50f;

        UI_Hide = false;


        WriteIni(filePath, "Setting", "mic_minsize", "0.5");
        WriteIni(filePath, "Setting", "mic_Sense", "50");

        WriteIni(filePath, "Setting", "ui_hide", "False");
    }

    //--------------------------------------------------------------------------------

    public void Save_Data(string targetdata, string savedata)
    {  WriteIni(filePath, "Setting", targetdata, savedata); }


    //--------------------------------------------------------------------------------

    public void WriteIni(string filePath, string Section, string key, string value)
    {
        WritePrivateProfileString(Section, key, value, filePath);
    }

    public string ReadIni(string filePath, string Section, string key)
    {
        var value = new StringBuilder(255);
        GetPrivateProfileString(Section, key, "Error", value, 255, filePath);
        return value.ToString();
    }
}
