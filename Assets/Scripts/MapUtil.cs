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

    public static float m_MapGridUnityLen;
    public static float m_AdjustZVal = 0.3f;//不要大于格子单位

    public static Map m_LeftSideWall = new Map();
    public static Map m_Wall = new Map();
    public static Map m_RightSideWall = new Map();

    public static void Init()
    {
        m_SelectId = 0;
        m_SelectOK = true;

        m_LeftSideWall.Init(Enum_Wall.LeftWall);
        m_Wall.Init(Enum_Wall.Wall);
        m_RightSideWall.Init(Enum_Wall.RightWall);
    }

    public static Map GetMap(Enum_Wall type)
    {
        switch (type)
        {
            case Enum_Wall.LeftWall:
                {
                    return m_LeftSideWall;
                }
            case Enum_Wall.Wall:
                {
                    return m_Wall;
                }
            case Enum_Wall.RightWall:
                {
                    return m_RightSideWall;
                }
        }
        return null;
    }

    public static DragInitData InitDrag(Vector3 size, bool onFloor, DragInitData oldData, Enum_Wall wall)
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

    public static FirstPos GetFirstPos()
    {
        FirstPos ret = new FirstPos();
        ret.pos = Vector3.zero;
        ret.wallType = Enum_Wall.Wall;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 100,
            JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                new string[]
                {
                    Enum_Wall.Wall.ToString(),
                    Enum_Wall.LeftWall.ToString(),
                    Enum_Wall.RightWall.ToString(),
                })))
        {
            if (hitInfo.collider != null
                && hitInfo.collider.gameObject != null)
            {
                ret.pos = hitInfo.point;
                string layerName = LayerMask.LayerToName(hitInfo.collider.gameObject.layer);
                switch (layerName)
                {
                    case "Wall":
                        {
                            ret.wallType = Enum_Wall.Wall;
                        }
                        break;
                    case "LeftWall":
                        {
                            ret.wallType = Enum_Wall.LeftWall;
                        }
                        break;
                    case "RightWall":
                        {
                            ret.wallType = Enum_Wall.RightWall;
                        }
                        break;
                }
            }
        }
        return ret;
    }
}