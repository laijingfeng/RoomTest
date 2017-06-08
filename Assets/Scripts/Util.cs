using UnityEngine;

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
}

public enum Enum_Event
{
    None = 0,
    SetOne,
    Place2Pos,
}

public enum Enum_Wall
{
    None = 0,
    LeftWall,
    Wall,
    RightWall,
}

public class DragInitData
{
    public Vector3 m_MinPos;
    public Vector3 m_MaxPos;
    public Vector3 m_AdjustPar;

    public bool isNew = false;

    public Enum_Wall m_CurWall = Enum_Wall.None;
    public Vector3 m_CurPos = Vector3.zero;

    public Enum_Wall m_LastWall = Enum_Wall.None;
    public Vector3 m_LastPos = Vector3.zero;
}

public class FirstPos
{
    public Vector3 pos;
    public Enum_Wall wallType;
}