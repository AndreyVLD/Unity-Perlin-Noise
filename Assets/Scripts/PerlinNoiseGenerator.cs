using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public int scale = 20;

    public float offsetX = 100f;
    public float offsetY = 100f;

    private MeshRenderer rend;
    private Texture2D texture;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        texture = new Texture2D(width, height);
        rend.material.mainTexture = texture;

    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;

                texture.SetPixel(x, y, getNoiseColor(xCoord, yCoord));

            }
        }

        texture.Apply();
    }

    Color getNoiseColor(float xCoord, float yCoord)
    {
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }
}
