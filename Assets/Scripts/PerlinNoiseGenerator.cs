using UnityEngine;

public class PerlinNoiseGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public int width = 256;
    public int height = 256;
    public float scale = 20;

    [Header("Speed Settings")]
    [Range(1, 100)]
    public float translateSpeed = 10f;

    [Range(1, 100)]
    public float zoomSpeed = 5f;


    [Header("Noise Settings")]
    [Range(1, 9)]
    public int octaves = 9;
    public float persistence = 0.5f;
    public float lacunarity = 3.0f;
    public ColorInterval[] colorIntervals;


    private float offsetX = 100f;
    private float offsetY = 100f;
    private Color[] colorCache;
    private MeshRenderer rend;
    private Texture2D texture;


    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        texture = new Texture2D(width, height);
        rend.material.mainTexture = texture;
        colorCache = new Color[width * height];
        GenerateNoise();
    }

    // Update is called once per frame
    void Update()
    {
        if (UserInput())
        {
            GenerateNoise();
        }

    }
  
    void GenerateNoise()
    {
       
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = ((float)x - width / 2) / width * scale + offsetX;
                float yCoord = ((float)y - height / 2) / height * scale + offsetY;

                float sample = FBM(xCoord, yCoord);
                Color noiseColor = GetColorFromValue(sample);
                colorCache[x + y * width] = noiseColor;

            }
        }
        texture.SetPixels(colorCache);
        texture.Apply();
    }

    bool UserInput()
    {
        bool changed = false;

        float moveX  = Input.GetAxis("Horizontal") * Time.deltaTime * translateSpeed;
        float moveY = Input.GetAxis("Vertical") * Time.deltaTime * translateSpeed;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (moveX != 0 || moveY != 0)
        {
            offsetX += moveX;
            offsetY += moveY;
            changed = true;
        }

        if (scroll != 0)
        {
            float zoomFactor = 1f - scroll * zoomSpeed * Time.deltaTime;
            scale *= zoomFactor;
            scale = Mathf.Clamp(scale, 1f, 100f);
            changed = true;
        }

        return changed;
    }

    Color GetColorFromValue(float value)
    {

        foreach(ColorInterval interval in colorIntervals)
        {
            if (value >= interval.start && value < interval.end)
                return Color.Lerp(interval.colorStart, interval.colorEnd, (value - interval.start) / (interval.end - interval.start));
        }
        return new Color(0.5f,0.5f,0.5f);
    }

    Color GetBilinearInterpolatedColor(int xCoord, int yCoord)
    {
        float x0 = (float)xCoord / width * scale + offsetX;
        float y0 = (float)yCoord / height * scale + offsetY;
        float x1 = (float)(xCoord + 1) / width * scale + offsetX;
        float y1 = (float)(yCoord + 1) / height * scale + offsetY;

        float sx = x0 - Mathf.Floor(x0);
        float sy = y0 - Mathf.Floor(y0);

        Color n00 = GetColorFromValue(Mathf.PerlinNoise(x0 , y0));
        Color n10 = GetColorFromValue(Mathf.PerlinNoise(x1, y0));
        Color n01 = GetColorFromValue(Mathf.PerlinNoise(x0, y1));
        Color n11 = GetColorFromValue(Mathf.PerlinNoise(x1, y1));

        Color ix0 = Color.Lerp(n00, n10, sx);
        Color ix1 = Color.Lerp(n01, n11, sx);
        Color value = Color.Lerp(ix0, ix1, sy);

        return value;
    }

    float FBM(float x, float y)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }
}
