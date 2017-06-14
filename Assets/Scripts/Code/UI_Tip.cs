using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Jerry;

public class UI_Tip : SingletonMono<UI_Tip>
{
    private Text m_Text;

    public override void Awake()
    {
        base.Awake();
        m_Text = this.GetComponent<Text>();
        ShowTip("");
    }

    public void ShowTip(string str)
    {
        m_Text.text = str;
        if (!string.IsNullOrEmpty(str))
        {
            this.StopCoroutine("IE_WaitClean");
            this.StartCoroutine("IE_WaitClean");
        }
    }

    private IEnumerator IE_WaitClean()
    {
        yield return new WaitForSeconds(0.5f);
        ShowTip("");
    }
}