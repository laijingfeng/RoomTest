using UnityEngine;

public class MyShadow : MonoBehaviour
{
    public Renderer m_Render;
    private static MyShadow m_Inst;
    public static MyShadow Inst
    {
        get
        {
            return m_Inst;

        }
    }

    void Awake()
    {
        m_Inst = this;
        if (m_Render != null)
        {
            m_Render.enabled = false;
        }
    }

    void Update()
    {
        //if (m_Render == null)
        //{
        //    return;
        //}
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    SetColor(Color.red);
        //}
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    SetColor(Color.green);
        //}
    }

    public void SetSize(Vector3 size)
    {
        size = size * MapUtil.m_MapGridUnityLen;
        size.z = 0.001f;
        this.transform.localScale = size;
    }

    public void SetVisible(bool isShow)
    {
        if (m_Render == null)
        {
            return;
        }
        m_Render.enabled = isShow;
    }

    public void SetPos(Vector3 pos, Vector3 rotate)
    {
        this.transform.position = pos;
        this.transform.eulerAngles = rotate;
    }

    public void SetColor(Color col)
    {
        if (m_Render == null)
        {
            return;
        }
        m_Render.material.SetColor("_Color", col);
    }
}