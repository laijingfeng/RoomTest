﻿using UnityEngine;

public class Util
{
    /// <summary>
    /// <para>获得点击位置</para>
    /// <para>移动设备用第一个触摸点</para>
    /// <para>返回值z轴为0</para>
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetClickPos()
    {
        Vector3 pos = Input.mousePosition;
#if UNITY_EDITOR
        pos = Input.mousePosition;
#else
#if UNITY_ANDROID || UNITY_IPHONE
            if(Input.touchCount > 0)
            {
                pos = Input.touches[0].position;
            }
            else
            {
                pos = Input.mousePosition;
            }
#else
            pos = Input.mousePosition;
#endif
#endif
        pos.z = 0;
        return pos;
    }

    private static int m_IdFactor = 0;
    public static int IDGenerator(int id)
    {
        if (id == 0)
        {
            id = ++m_IdFactor;
        }
        return id;
    }

    public static bool Vector3Equal(Vector3 a, Vector3 b)
    {
        if (Mathf.Abs(a.x - b.x) > 0.1f
            || Mathf.Abs(a.y - b.y) > 0.1f
            || Mathf.Abs(a.z - b.z) > 0.1f)
        {
            return false;
        }
        return true;
    }
}

public enum Enum_Event
{
    None = 0,
    SetOne,
    Place2Pos,
    BackOne,
    /// <summary>
    /// 放回背包
    /// </summary>
    Back2Package,
}

public enum Enum_Layer
{
    None = 0,
    Wall = 8,
    LeftWall = 9,
    RightWall = 10,
    Cube = 11,
    /// <summary>
    /// 选中的Cube
    /// </summary>
    ActiveCube = 12,
}

public class DragInitData
{
    public Vector3 m_MinPos;
    public Vector3 m_MaxPos;
    public Vector3 m_AdjustPar;

    public bool isNew = false;

    public Enum_Layer m_CurWall = Enum_Layer.None;
    public Vector3 m_CurPos = Vector3.zero;

    /// <summary>
    /// 用来回退
    /// </summary>
    public Enum_Layer m_LastWall = Enum_Layer.None;
    public Vector3 m_LastPos = Vector3.zero;
}

public class FirstPos
{
    public Vector3 pos;
    public Enum_Layer wallType;
}