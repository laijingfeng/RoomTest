using UnityEngine;
using UnityEngine.EventSystems;

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

    private static int m_IdFactor = 100;
    public static int IDGenerator(int id = 0)
    {
        if (id == 0)
        {
            id = ++m_IdFactor;
        }
        return id;
    }

    public static bool Vector3Equal(Vector3 a, Vector3 b, float val = 0.1f)
    {
        if (Mathf.Abs(a.x - b.x) > val
            || Mathf.Abs(a.y - b.y) > val
            || Mathf.Abs(a.z - b.z) > val)
        {
            return false;
        }
        return true;
    }

    public static bool ClickUI()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
			if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
            if (EventSystem.current.IsPointerOverGameObject())
#endif
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}

public enum Enum_Event
{
    None = 0,
    SetOne,
    
    /// <summary>
    /// 点击放到一个位置
    /// </summary>
    Place2Pos,

    /// <summary>
    /// <para>撤回原位置</para>
    /// <para>参数:id</para>
    /// </summary>
    BackOne,
    
    /// <summary>
    /// <para>放回背包</para>
    /// <para>参数:id</para>
    /// </summary>
    Back2Package,

    /// <summary>
    /// <para>存档</para>
    /// <para>参数:无</para>
    /// </summary>
    SaveCurHouseData,
}

[System.Serializable]
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
    FloorWall = 13,
}

public class DragInitData
{
    public Vector3 m_MinPos;
    public Vector3 m_MaxPos;
    public Vector3 m_AdjustPar;

    /// <summary>
    /// 是否是放好的
    /// </summary>
    public bool isSeted = false;

    public Enum_Layer m_CurWall = Enum_Layer.None;
    
    /// <summary>
    /// 上次的墙，用来回退
    /// </summary>
    public Enum_Layer m_LastWall = Enum_Layer.None;
    /// <summary>
    /// 上次的位置，用来回退
    /// </summary>
    public Vector3 m_LastPos = Vector3.zero;
}

public class FirstPos
{
    public Vector3 pos;
    public Enum_Layer wallType;
}