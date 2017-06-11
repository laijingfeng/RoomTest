using UnityEngine;
using System.Collections;
using Jerry;

public class Flag : MonoBehaviour
{
    public Canvas m_Canvas;

    private static Flag m_Inst;
    public static Flag Inst
    {
        get
        {
            return m_Inst;
        }
    }

    public void Set2Pos(Vector3 pos)
    {
        this.transform.localPosition = JerryUtil.PosScreen2Canvas(m_Canvas, pos, this.transform);
    }

    void Awake()
    {
        m_Inst = this;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
