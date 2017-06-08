using UnityEngine;

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

    public static Map m_LeftSideWall = new Map();
    public static Map m_Wall = new Map();
    public static Map m_RightSideWall = new Map();

    public static void Init()
    {
        m_SelectId = 0;
        m_SelectOK = true;

        m_LeftSideWall.Init(Enum_Wall.Left);
        m_Wall.Init(Enum_Wall.Wall);
        m_RightSideWall.Init(Enum_Wall.Right);
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
        }
        return null;
    }

    public static DragInitData InitDrag(Vector3 size, bool onFloor, DragInitData oldData)
    {
        if (oldData == null)
        {
            oldData = new DragInitData();
        }
        oldData.m_LastWall = oldData.m_CurWall;
        oldData.m_LastPos = oldData.m_CurPos;

        oldData.m_CurWall = Wall.Inst.m_DefaulfWall;
        oldData.m_CurPos = Vector3.zero;//TODO

        GetMap(oldData.m_CurWall).GetMinMaxPos(size, onFloor, ref oldData);
        oldData.m_AdjustPar = GetMap(oldData.m_CurWall).GetAdjustPar(size);

        return oldData;
    }
}