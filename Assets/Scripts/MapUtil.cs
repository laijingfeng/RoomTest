using UnityEngine;
using Jerry;

public class MapUtil
{
    /// <summary>
    /// 选中的ID
    /// </summary>
    public static int m_SelectId = 0;
    /// <summary>
    /// 选中的已经放好了
    /// </summary>
    public static bool m_SelectOK = true;
    public static bool m_SelectNew = false;

    public static float m_MapGridUnityLen;
    public static float m_AdjustZVal = 0.3f;//不要大于格子单位

    public static Map m_LeftSideWall = new Map();
    public static Map m_Wall = new Map();
    public static Map m_RightSideWall = new Map();

    public static void Init()
    {
        m_SelectId = 0;
        m_SelectOK = true;

        m_LeftSideWall.Init(Enum_Layer.LeftWall);
        m_Wall.Init(Enum_Layer.Wall);
        m_RightSideWall.Init(Enum_Layer.RightWall);
    }

    public static Map GetMap(Enum_Layer type)
    {
        switch (type)
        {
            case Enum_Layer.LeftWall:
                {
                    return m_LeftSideWall;
                }
            case Enum_Layer.Wall:
                {
                    return m_Wall;
                }
            case Enum_Layer.RightWall:
                {
                    return m_RightSideWall;
                }
        }
        return null;
    }

    public static DragInitData InitDrag(Vector3 size, bool onFloor, DragInitData oldData, Enum_Layer wall)
    {
        if (oldData == null)
        {
            oldData = new DragInitData();
        }

        oldData.m_CurWall = wall;

        GetMap(oldData.m_CurWall).GetMinMaxPos(size, onFloor, ref oldData);
        oldData.m_AdjustPar = GetMap(oldData.m_CurWall).GetAdjustPar(size);

        return oldData;
    }

    /// <summary>
    /// 默认放一个位置
    /// </summary>
    /// <returns></returns>
    public static FirstPos GetFirstPos()
    {
        FirstPos ret = new FirstPos();
        ret.pos = Vector3.zero;
        ret.wallType = Enum_Layer.Wall;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 100,
            JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                new string[]
                {
                    Enum_Layer.Wall.ToString(),
                    Enum_Layer.LeftWall.ToString(),
                    Enum_Layer.RightWall.ToString(),
                })))
        {
            if (hitInfo.collider != null
                && hitInfo.collider.gameObject != null)
            {
                ret.pos = hitInfo.point;
                ret.wallType = WallLayer2Enum(hitInfo.collider.gameObject.layer);
            }
        }
        return ret;
    }

    public static bool IsWallLayer(int layer)
    {
        if(layer == Enum_Layer.Wall.GetHashCode()
            || layer == Enum_Layer.LeftWall.GetHashCode()
            || layer == Enum_Layer.RightWall.GetHashCode())
        {
            return true;
        }
        return false;
    }

    public static Enum_Layer WallLayer2Enum(int layer)
    {
        Enum_Layer ret = Enum_Layer.None;
        if (layer == Enum_Layer.Wall.GetHashCode())
        {
            ret = Enum_Layer.Wall;
        }
        else if (layer == Enum_Layer.LeftWall.GetHashCode())
        {
            ret = Enum_Layer.LeftWall;
        }
        else if (layer == Enum_Layer.RightWall.GetHashCode())
        {
            ret = Enum_Layer.RightWall;
        }
        return ret;
    }

    public static Enum_Layer WallLayerName2Enum(string layerName)
    {
        Enum_Layer ret = Enum_Layer.None;
        if (layerName == Enum_Layer.Wall.ToString())
        {
            ret = Enum_Layer.Wall;
        }
        else if (layerName == Enum_Layer.LeftWall.ToString())
        {
            ret = Enum_Layer.LeftWall;
        }
        else if (layerName == Enum_Layer.RightWall.ToString())
        {
            ret = Enum_Layer.RightWall;
        }
        return ret;
    }
}