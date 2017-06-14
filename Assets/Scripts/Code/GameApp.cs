using UnityEngine;
using System.Collections;
using Jerry;

public class GameApp : SingletonMono<GameApp>
{
    public float m_MapGridUnityLen;
    
    public float m_OutScreenJudgeFactor = 50;
    public float m_OutScreenDragFactor = 5;

    public override void Awake()
    {
        base.Awake();
    }
}