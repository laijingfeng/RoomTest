using UnityEngine;
using System.Collections;
using Jerry;

public class GameApp : SingletonMono<GameApp>
{
    public float m_MapGridUnityLen;
    public CtrObjType m_CtrType = CtrObjType.ClickAndDrag;

    public float m_OutScreenJudgeFactor = 50;
    public float m_OutScreenDragFactor = 5;

    public override void Awake()
    {
        base.Awake();
    }

    public enum CtrObjType
    {
        OnlyClick = 0,
        OnlyDrag,
        ClickAndDrag,
    }
}