using System;
using UnityEngine;

[Serializable]
public class ColorInterval
{
    public float start;
    public float end;
    public Color colorStart;
    public Color colorEnd;

    public ColorInterval(float start, float end, Color colorStart, Color colorEnd)
    {
        this.start = start;
        this.end = end;
        this.colorStart = colorStart;
        this.colorEnd = colorEnd;
    }
}

