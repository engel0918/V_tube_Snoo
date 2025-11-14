using TMPro;
using UnityEngine;

public class UI_Comp : MonoBehaviour
{
    SaveData data;

    public GameObject Menu_Obj;

    public TMP_InputField Inf_mic_sense;
    public TMP_InputField Inf_threshold;

    public TMP_Dropdown Dd_MicSel;

    private void Start()
    {
        data = GetComponent<SaveData>();

        if(data.UI_Hide == true) { Menu_Obj.SetActive(false); }
        else { Menu_Obj.SetActive(true);}
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if(Menu_Obj.activeSelf.Equals(true))
            {
                data.Save_Data("ui_hide", "True");
                Menu_Obj.SetActive(false);
            }
            else
            {
                data.Save_Data("ui_hide", "False");
                Menu_Obj.SetActive(true);
            }
        }
    }
}
