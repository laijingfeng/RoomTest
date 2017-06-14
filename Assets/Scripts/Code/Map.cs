﻿using UnityEngine;

public class Map
{
    /// <summary>
    /// 地图大小，XYZ三个方向
    /// </summary>
    private MapUtil.IVector3 m_Size;
    public Vector3 m_StartPos;

    private Enum_Layer m_Type;

    /// <summary>
    /// 地图大小，XY两个方向
    /// </summary>
    private MapUtil.IVector3 m_SizeXY;
    /// <summary>
    /// 地图标记
    /// </summary>
    private bool[, ,] m_Flag;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="type"></param>
    public void Init(Enum_Layer type, Vector3 startPos, MapUtil.IVector3 size)
    {
        m_Type = type;
        m_StartPos = startPos;
        m_Size = size;

        m_SizeXY = GridXYZ2XY(m_Size);

        m_Flag = new bool[2, m_SizeXY.x, m_SizeXY.y];
        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < m_SizeXY.x; i++)
            {
                for (int j = 0; j < m_SizeXY.y; j++)
                {
                    m_Flag[k, i, j] = false;
                }
            }
        }
    }

    public void ResetMapStartPosY()
    {
        m_StartPos.y = GameApp.Inst.GetHouseYOffset;
    }

    public void ResetMapFlag()
    {
        for (int i = 0; i < m_SizeXY.x; i++)
        {
            for (int j = 0; j < m_SizeXY.y; j++)
            {
                m_Flag[GameApp.Inst.CurNodeIdx, i, j] = false;
            }
        }
    }

    /// <summary>
    /// 粘着墙
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="interval">默认是影子的配置</param>
    /// <returns></returns>
    public Vector3 Adjust2Wall(Vector3 pos, float interval = 0.01f)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            pos.z = m_StartPos.z - interval;
        }
        else if (m_Type == Enum_Layer.LeftWall)
        {
            pos.x = m_StartPos.x + interval;
        }
        else if (m_Type == Enum_Layer.RightWall)
        {
            pos.x = m_StartPos.x - interval;
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            pos.y = m_StartPos.y + interval;
        }
        return pos;
    }

    /// <summary>
    /// 家具粘着墙
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="floating"></param>
    /// <param name="pos"></param>
    public Vector3 AdjustFurn2Wall2(MapUtil.IVector3 size, bool floating, Vector3 pos)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            pos.z = m_StartPos.z - size.z * GameApp.Inst.m_MapGridUnityLenHalf;
            if (floating)
            {
                pos.z -= GameApp.Inst.m_AdjustFurn2WallPar;
            }
        }
        else if (m_Type == Enum_Layer.LeftWall)
        {
            pos.x = m_StartPos.x + size.x * GameApp.Inst.m_MapGridUnityLenHalf;
            if (floating)
            {
                pos.x += GameApp.Inst.m_AdjustFurn2WallPar;
            }
        }
        else if (m_Type == Enum_Layer.RightWall)
        {
            pos.x = m_StartPos.x - size.x * GameApp.Inst.m_MapGridUnityLenHalf;
            if (floating)
            {
                pos.x -= GameApp.Inst.m_AdjustFurn2WallPar;
            }
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            pos.y = m_StartPos.y + size.y * GameApp.Inst.m_MapGridUnityLenHalf;
            if (floating)
            {
                pos.y += GameApp.Inst.m_AdjustFurn2WallPar;
            }
        }
        return pos;
    }

    /// <summary>
    /// 家具粘着墙
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="floating"></param>
    /// <param name="pos"></param>
    public void AdjustFurn2Wall(MapUtil.IVector3 size, bool floating, ref Vector3 pos)
    {
        pos = AdjustFurn2Wall2(size, floating, pos);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="setType"></param>
    /// <param name="data"></param>
    public void GetMinMaxPos(MapUtil.IVector3 size, MapUtil.SetType setType, ref DragInitData data)
    {
        data.m_MinPos = m_StartPos + size.MulVal(GameApp.Inst.m_MapGridUnityLenHalf);
        data.m_MaxPos = m_StartPos + m_Size.MulVal(GameApp.Inst.m_MapGridUnityLen) - size.MulVal(GameApp.Inst.m_MapGridUnityLenHalf);
        if (setType == MapUtil.SetType.WallOnFloor)
        {
            data.m_MaxPos.y = data.m_MinPos.y;
        }
        else if (setType == MapUtil.SetType.Wall)//墙上的不能贴地
        {
            data.m_MinPos.y += GameApp.Inst.m_MapGridUnityLen;
        }

        //墙面可以互跳
        if (m_Type == Enum_Layer.Wall)
        {
            data.m_MinPos.x -= GameApp.Inst.m_MapGridUnityLen;
            data.m_MaxPos.x += GameApp.Inst.m_MapGridUnityLen;
        }
        else if (m_Type == Enum_Layer.LeftWall
            || m_Type == Enum_Layer.RightWall)
        {
            data.m_MaxPos.z += GameApp.Inst.m_MapGridUnityLen;
        }

        data.m_MinPos = Adjust2Wall(data.m_MinPos, 0f);
        data.m_MaxPos = Adjust2Wall(data.m_MaxPos, 0f);

        //Debug.LogWarning("min=" + MapUtil.Vector3String(data.m_MinPos) + " max=" + MapUtil.Vector3String(data.m_MaxPos) + " size=" + size);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <returns></returns>
    public Vector3 GetAdjustPar(MapUtil.IVector3 size)
    {
        Vector3 ret = Vector3.one;

        if ((size.x & 1) == 0)
        {
            ret.x = 0;
        }
        else
        {
            ret.x = GameApp.Inst.m_MapGridUnityLen / 2;
        }

        if ((size.y & 1) == 0)
        {
            ret.y = 0;
        }
        else
        {
            ret.y = GameApp.Inst.m_MapGridUnityLen / 2;
        }

        if ((size.z & 1) == 0)
        {
            ret.z = 0;
        }
        else
        {
            ret.z = GameApp.Inst.m_MapGridUnityLen / 2;
        }

        //Debug.LogWarning("size=" + size + " ret=" + MapUtil.Vector3String(ret) + " " + m_Type);

        return ret;
    }

    /// <summary>
    /// 格子是(x,y,z)
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    public Vector3 Grid2Pos(MapUtil.IVector3 grid)
    {
        Vector3 ret = m_StartPos + Vector3.one * GameApp.Inst.m_MapGridUnityLenHalf + grid.MulVal(GameApp.Inst.m_MapGridUnityLen);
        ret = Adjust2Wall(ret, 0f);
        return ret;
    }

    /// <summary>
    /// 格子归到(x,y,z)表示
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public MapUtil.IVector3 Pos2Grid(Vector3 pos)
    {
        MapUtil.IVector3 ret = new MapUtil.IVector3(0, 0, 0);
        pos = (pos - m_StartPos) / GameApp.Inst.m_MapGridUnityLen - Vector3.one;

        ret.x = Mathf.RoundToInt(pos.x);
        ret.y = Mathf.RoundToInt(pos.y);
        ret.z = Mathf.RoundToInt(pos.z);

        return ret;
    }

    /// <summary>
    /// 格子(x,y,z)->(x,y)
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private MapUtil.IVector3 GridXYZ2XY(MapUtil.IVector3 grid)
    {
        MapUtil.IVector3 ret = new MapUtil.IVector3(0, 0, 0);
        ret.x = (m_Type == Enum_Layer.Wall || m_Type == Enum_Layer.FloorWall) ? grid.x : grid.z;
        ret.y = (m_Type == Enum_Layer.FloorWall) ? grid.z : grid.y;
        return ret;
    }

    public void CleanOne(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        DoCleanJudgeSet(pos, size, true, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="mainJudge"></param>
    /// <returns></returns>
    public bool JudgeSet(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        return DoCleanJudgeSet(pos, size, true, 1);
    }

    public bool SetOne(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        if (mainJudge)
        {
            if (!JudgeSet(pos, size, true))
            {
                return false;
            }
        }

        DoCleanJudgeSet(pos, size, true, 2);

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="mainJudge"></param>
    /// <param name="workType">012-CleanJudgeSet</param>
    /// <returns></returns>
    private bool DoCleanJudgeSet(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true, int workType = 0)
    {
        pos = AdjustFurn2Wall2(size, false, pos);

        MapUtil.IVector3 min = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, true)));
        MapUtil.IVector3 max = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, false)));

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                if (workType == 0)
                {
                    m_Flag[GameApp.Inst.CurNodeIdx, i, j] = false;
                }
                else if (workType == 1)
                {
                    if (m_Flag[GameApp.Inst.CurNodeIdx, i, j] == true)
                    {
                        return false;
                    }
                }
                else
                {
                    m_Flag[GameApp.Inst.CurNodeIdx, i, j] = true;
                }
            }
        }

        if (mainJudge)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (min.x == 0)
                {
                    if (!MapUtil.GetMap(Enum_Layer.LeftWall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }

                if (max.x + 1 == m_Size.x)
                {
                    if (!MapUtil.GetMap(Enum_Layer.RightWall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }

                if (min.y == 0)
                {
                    if (!MapUtil.GetMap(Enum_Layer.FloorWall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }
            }
            else if (m_Type == Enum_Layer.LeftWall || m_Type == Enum_Layer.RightWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    if (!MapUtil.GetMap(Enum_Layer.Wall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }

                if (min.y == 0)
                {
                    if (!MapUtil.GetMap(Enum_Layer.FloorWall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (max.y + 1 == m_Size.z)
                {
                    if (!MapUtil.GetMap(Enum_Layer.Wall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }

                if (min.x == 0)
                {
                    if (!MapUtil.GetMap(Enum_Layer.LeftWall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }

                if (max.x + 1 == m_Size.x)
                {
                    if (!MapUtil.GetMap(Enum_Layer.RightWall).DoCleanJudgeSet(pos, size, false, workType))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private float GetCornerPosX(int mulPar, float StartX, int SizeX, int sizex, float posx)
    {
        float ret = 0;
        if ((sizex & 1) == 0)
        {
            if (sizex == 0)
            {
                ret = StartX + (SizeX + 1 + mulPar) * GameApp.Inst.m_MapGridUnityLen;//MIN要大于MAX，都大于边界
            }
            else
            {
                ret = posx - mulPar * ((sizex - 1) / 2 + 0.5f) * GameApp.Inst.m_MapGridUnityLen;
            }
        }
        else
        {
            ret = posx - mulPar * (sizex / 2) * GameApp.Inst.m_MapGridUnityLen;
        }
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="min"></param>
    /// <returns></returns>
    public Vector3 GetCornerPos(Vector3 pos, MapUtil.IVector3 size, bool min)
    {
        Vector3 ret = Vector3.zero;
        int mulPar = min ? 1 : -1;

        ret.x = GetCornerPosX(mulPar, m_StartPos.x, m_Size.x, size.x, pos.x);
        ret.y = GetCornerPosX(mulPar, m_StartPos.y, m_Size.y, size.y, pos.y);
        ret.z = GetCornerPosX(mulPar, m_StartPos.z, m_Size.z, size.z, pos.z);

        ret = Adjust2Wall(ret, 0f);

        //Debug.LogWarning("corner=" + ret + " min=" + min + " pos=" + pos);
        return ret;
    }
}