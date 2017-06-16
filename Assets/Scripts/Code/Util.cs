using UnityEngine;
using UnityEngine.EventSystems;

public class Util
{
    private static uint m_IdFactor = 100;
    public static uint IDGenerator(uint id = 0)
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

    /// <summary>
    /// <para>放置家具</para>
    /// <para>参数:id(uint)</para>
    /// </summary>
    SetOneFurn,

    /// <summary>
    /// <para>设置家具到一个位置</para>
    /// <para>参数:pos(RayClickPos)</para>
    /// </summary>
    SetFurn2Pos,

    /// <summary>
    /// <para>取消家具的设置</para>
    /// <para>参数:id</para>
    /// </summary>
    CancelSetFurn,

    /// <summary>
    /// <para>放回背包</para>
    /// <para>参数:id</para>
    /// </summary>
    SetFurn2Package,

    /// <summary>
    /// <para>存档</para>
    /// <para>参数:无</para>
    /// </summary>
    SaveCurHouseData,

    /// <summary>
    /// <para>点击到了3D对象</para>
    /// <para>参数:info(RayClickInfo)</para>
    /// </summary>
    Click3DObj,

    /// <summary>
    /// <para>点下3D对象</para>
    /// <para>参数:info(RayClickInfo)</para>
    /// </summary>
    Click3DDown,
}

[System.Serializable]
public enum Enum_Layer
{
    None = 0,
    Wall = 8,
    LeftWall = 9,
    RightWall = 10,
    Furniture = 11,
    ActiveFurniture = 12,
    FloorWall = 13,
}

/// <summary>
/// 墙类型
/// </summary>
public enum Enum_Wall
{
    None = 0,
    Wall,
    Left,
    Right,
    Floor,
}

public class FurnitureInitData
{
    public Vector3 m_MinPos;
    public Vector3 m_MaxPos;
    public Vector3 m_AdjustPar;

    /// <summary>
    /// 是否是放好的
    /// </summary>
    public bool isSeted = false;

    public Enum_Wall m_CurWall = Enum_Wall.None;

    /// <summary>
    /// 上次的墙，用来回退
    /// </summary>
    public Enum_Wall m_LastWall = Enum_Wall.None;
    /// <summary>
    /// 上次的位置，用来回退
    /// </summary>
    public Vector3 m_LastPos = Vector3.zero;
}

public class RayClickPos
{
    public Vector3 pos;
    public Enum_Wall wallType;
}

public class RayClickInfo
{
    public float time;
    public Collider col;
    public Vector3 pos;
    public Vector3 screenPos;

    public RayClickInfo()
    {
        Init();
    }

    public void Init(RayClickInfo info)
    {
        time = info.time;
        col = info.col;
        pos = info.pos;
        screenPos = info.screenPos;
    }

    public void Init()
    {
        time = 0;
        col = null;
        pos = Vector3.zero;
    }

    public override string ToString()
    {
        return string.Format("time={0},col={1},pos={2},sPos={3}", time, col == null ? "无" : col.name, MapUtil.Vector3String(pos), MapUtil.Vector3String(screenPos));
    }
}