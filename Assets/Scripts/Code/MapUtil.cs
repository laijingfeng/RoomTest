﻿using UnityEngine;
using Jerry;

public class MapUtil
{
    /// <summary>
    /// 选中的ID
    /// </summary>
    public static uint m_SelectId = 0;
    /// <summary>
    /// 选中的已经放好了
    /// </summary>
    public static bool m_SelectOK = true;
    /// <summary>
    /// 是否是新的
    /// </summary>
    public static bool m_SelectNew = false;
    public static Furniture m_SelectFurn = null;

    public static Map m_LeftSideWall = new Map();
    public static Map m_Wall = new Map();
    public static Map m_RightSideWall = new Map();
    public static Map m_FloorWall = new Map();

    public static void Init()
    {
        m_SelectId = 0;
        m_SelectOK = true;
        m_SelectNew = false;
        m_SelectFurn = null;

        m_LeftSideWall.Init(Enum_Wall.Left, GameApp.Inst.m_LeftSideWallStartPos, GameApp.Inst.m_LeftSideWallSize);
        m_Wall.Init(Enum_Wall.Wall, GameApp.Inst.m_WallStartPos, GameApp.Inst.m_WallSize);
        m_RightSideWall.Init(Enum_Wall.Right, GameApp.Inst.m_RightSideWallStartPos, GameApp.Inst.m_RightSideWallSize);
        m_FloorWall.Init(Enum_Wall.Floor, GameApp.Inst.m_FloorWallStartPos, GameApp.Inst.m_FloorWallSize);
    }

    public static void ResetMapStartPosY()
    {
        m_LeftSideWall.ResetMapStartPosY();
        m_Wall.ResetMapStartPosY();
        m_RightSideWall.ResetMapStartPosY();
        m_FloorWall.ResetMapStartPosY();
    }

    public static void ResetMapFlag()
    {
        m_LeftSideWall.ResetMapFlag();
        m_Wall.ResetMapFlag();
        m_RightSideWall.ResetMapFlag();
        m_FloorWall.ResetMapFlag();
    }

    public static Map GetMap(Enum_Wall type)
    {
        switch (type)
        {
            case Enum_Wall.Left:
                {
                    return m_LeftSideWall;
                }
            case Enum_Wall.Wall:
                {
                    return m_Wall;
                }
            case Enum_Wall.Right:
                {
                    return m_RightSideWall;
                }
            case Enum_Wall.Floor:
                {
                    return m_FloorWall;
                }
        }
        return null;
    }

    public static FurnitureInitData InitFurn(IVector3 size, MapUtil.SetType setType, FurnitureInitData oldData, Enum_Wall wall)
    {
        if (oldData == null)
        {
            oldData = new FurnitureInitData();
        }

        oldData.m_CurWall = wall;

        GetMap(oldData.m_CurWall).GetMinMaxPos(size, setType, ref oldData);
        oldData.m_AdjustPar = GetMap(oldData.m_CurWall).GetAdjustPar(size);

        return oldData;
    }

    /// <summary>
    /// 默认放一个位置
    /// </summary>
    /// <returns></returns>
    public static RayClickPos GetFirstPos(MapUtil.SetType setType)
    {
        RayClickPos ret = new RayClickPos();
        ret.pos = Vector3.zero;
        ret.wallType = Enum_Wall.Wall;

        Vector3 pos = new Vector3(Screen.width / 2, Screen.height / 2, 0f);
        if (setType == SetType.Floor)
        {
            pos.y = Screen.height / 4;
        }

        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 100, JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false), GetWallLayerNames(setType))))
        {
            if (hitInfo.collider != null
                && hitInfo.collider.gameObject != null)
            {
                ret.pos = hitInfo.point;
                ret.wallType = WallLayer2WallEnum(hitInfo.collider.gameObject.layer);
            }
        }
        else 
        {
            Debug.LogError("first error");
        }

        return ret;
    }

    public static bool IsWallLayer(int layer)
    {
        if (layer == Enum_Layer.Wall.GetHashCode()
            || layer == Enum_Layer.LeftWall.GetHashCode()
            || layer == Enum_Layer.RightWall.GetHashCode()
            || layer == Enum_Layer.FloorWall.GetHashCode())
        {
            return true;
        }
        return false;
    }

    public static string[] GetWallLayerNames(SetType setType = SetType.None)
    {
        switch (setType)
        {
            case SetType.WallOnFloor:
            case SetType.Wall:
                {
                    return new string[] 
                    {
                        Enum_Layer.Wall.ToString(),
                        Enum_Layer.LeftWall.ToString(),
                        Enum_Layer.RightWall.ToString(),
                    };
                }
            case SetType.Floor:
                {
                    return new string[] 
                    {
                        Enum_Layer.FloorWall.ToString(),
                    };
                }
            default:
                {
                    return new string[] 
                    {
                        Enum_Layer.Wall.ToString(),
                        Enum_Layer.FloorWall.ToString(),
                        Enum_Layer.LeftWall.ToString(),
                        Enum_Layer.RightWall.ToString(),
                    };
                }
        }
    }

    public static Enum_Wall WallLayer2WallEnum(int layer)
    {
        Enum_Wall ret = Enum_Wall.None;
        if (layer == Enum_Layer.Wall.GetHashCode())
        {
            ret = Enum_Wall.Wall;
        }
        else if (layer == Enum_Layer.LeftWall.GetHashCode())
        {
            ret = Enum_Wall.Left;
        }
        else if (layer == Enum_Layer.RightWall.GetHashCode())
        {
            ret = Enum_Wall.Right;
        }
        else if (layer == Enum_Layer.FloorWall.GetHashCode())
        {
            ret = Enum_Wall.Floor;
        }
        return ret;
    }

    /// <summary>
    /// 放置类型
    /// </summary>
    [System.Serializable]
    public enum SetType
    {
        None = 0,
        /// <summary>
        /// 墙面
        /// </summary>
        Wall,
        /// <summary>
        /// 墙面贴地面
        /// </summary>
        WallOnFloor,
        /// <summary>
        /// 地面
        /// </summary>
        Floor,
    }

    /// <summary>
    /// 整型Vector3
    /// </summary>
    [System.Serializable]
    public struct IVector3
    {
        public int x;
        public int y;
        public int z;

        public IVector3(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public IVector3(IVector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public IVector3(Vector3 v)
        {
            x = (int)v.x;
            y = (int)v.y;
            z = (int)v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public Vector3 MulVal(float val)
        {
            return ToVector3() * val;
        }
    }

    public static string Vector3String(Vector3 v)
    {
        return string.Format("({0},{1},{2})", v.x, v.y, v.z);
    }

    public static Vector3 GetObjEulerAngles(Enum_Wall wallType)
    {
        switch (wallType)
        {
            case Enum_Wall.Left:
                {
                    return new Vector3(0, -90, 0);
                }
            case Enum_Wall.Right:
                {
                    return new Vector3(0, 90, 0);
                }
            case Enum_Wall.Wall:
            case Enum_Wall.Floor:
                {
                    return Vector3.zero;
                }
        }
        return Vector3.zero;
    }

    public static MapUtil.IVector3 ChangeObjSize(MapUtil.IVector3 size, Enum_Wall fromType, Enum_Wall toType)
    {
        MapUtil.IVector3 ret = new MapUtil.IVector3(size);
        if (((fromType == Enum_Wall.Left || fromType == Enum_Wall.Right)
            && (toType == Enum_Wall.Floor || toType == Enum_Wall.Wall)) ||
            ((fromType == Enum_Wall.Floor || fromType == Enum_Wall.Wall)
            && (toType == Enum_Wall.Left || toType == Enum_Wall.Right)))
        {
            ret.x = size.z;
            ret.z = size.x;
        }
        return ret;
    }
}