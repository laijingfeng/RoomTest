using UnityEngine;
using Jerry;

/// <summary>
/// 家具投影
/// </summary>
public class FurnitureShadow : SingletonMono<FurnitureShadow>
{
    private Renderer m_Render;
    private Color m_Color;

    public override void Awake()
    {
        base.Awake();
        m_Render = this.transform.GetComponent<Renderer>();
        if (m_Render != null)
        {
            m_Render.enabled = false;
        }
        m_Color = Color.green;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size"></param>
    /// <param name="setType"></param>
    public void SetSize(Vector3 size, Enum_Layer wallType)
    {
        //Debug.LogWarning("size = " + MapUtil.Vector3String(size));
        size = size * GameApp.Inst.m_MapGridUnityLen;
        if (wallType == Enum_Layer.FloorWall)
        {
            size.y = 0.001f;
        }
        else if(wallType == Enum_Layer.Wall)
        {
            size.z = 0.001f;
        }
        else
        {
            size.x = 0.001f;
        }
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

    public void SetPos(Vector3 pos)
    {
        this.transform.position = pos;
    }

    public bool CanSet
    {
        get
        {
            return m_Color == Color.red ? false : true;
        }
    }

    public void SetColor(Color col)
    {
        if (m_Render == null)
        {
            return;
        }

        if (col == m_Color)
        {
            return;
        }

        m_Color = col;
        m_Render.material.SetColor("_Color", col);
    }
}