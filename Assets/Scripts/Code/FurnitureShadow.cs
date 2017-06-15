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
    /// 设置大小，选中时，切换墙面时
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

    /// <summary>
    /// <para>设置可见</para>
    /// <para>选中可见</para>
    /// </summary>
    /// <param name="isShow"></param>
    public void SetVisible(bool isShow)
    {
        if (m_Render == null)
        {
            return;
        }
        m_Render.enabled = isShow;
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="col"></param>
    public void SetPosColor(Vector3 pos, Color col)
    {
        this.transform.position = pos;

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

    /// <summary>
    /// 是否可以放置
    /// </summary>
    public bool CanSet
    {
        get
        {
            return m_Color == Color.red ? false : true;
        }
    }
}