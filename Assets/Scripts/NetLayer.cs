using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetLayer
{
    public string netLayerType;
    public float[] mData;
    public float centerPos;
    public float spread;
    public NetLayer(string type, float[] data, float centerPos=0.0f,float spread = 1.0f)
    {
        this.netLayerType = type;
        this.mData = data;
        this.centerPos = centerPos;
        this.spread = spread;
    }
}
