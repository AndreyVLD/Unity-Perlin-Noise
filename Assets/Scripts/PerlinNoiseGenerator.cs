using UnityEngine;

public class PerlinNoiseGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public int width = 256;
    public int height = 256;

    [Header("Speed Settings")]
    [Range(1, 100)]
    public float translateSpeed = 10f;

    [Range(1, 100)]
    public float zoomSpeed = 5f;


    [Header("Noise Settings")]
    [Range(1, 10)]
    public int octaves = 9;
    [Range(0, 1)]
    public float persistence = 0.5f;
    [Range(0, 10)]
    public float lacunarity = 3.0f;
    public ColorInterval[] colorIntervals;

    [Header("Shader")]
    public ComputeShader computeShader;

    private float offsetX = 100f;
    private float offsetY = 100f;
    private ComputeBuffer buffer;
    private int type = 0;
    private int kernelHandle;
    private float scale = 4;


    public RenderTexture renderTexture;
    private MeshRenderer rend;


    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();

        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        rend.material.mainTexture = renderTexture;
        kernelHandle = computeShader.FindKernel("CSMain");

        ComputeBufferData();
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
  
    // Generate the noise using the compute shader
    void GenerateNoise()
    {

        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        computeShader.SetFloat("scale", scale);
        computeShader.SetFloat("offsetX", offsetX);
        computeShader.SetFloat("offsetY", offsetY);
        computeShader.SetInt("octaves", octaves);
        computeShader.SetFloat("persistence", persistence);
        computeShader.SetFloat("lacunarity", lacunarity);
        computeShader.SetFloat("numColors", colorIntervals.Length);
        computeShader.SetFloat("type", type);

        computeShader.SetTexture(kernelHandle, "Result", renderTexture);
        computeShader.SetBuffer(kernelHandle, "ColorIntervals", buffer);

        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
        computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);
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
            scale = Mathf.Clamp(scale, 0.1f, 500f);
            changed = true;
        }

        return changed;
    }

    public void SetType(int type)
    {
        this.type = type;
        GenerateNoise();
    }

    void ComputeBufferData()
    {
        int totalSize = 2 * sizeof(float) + 2 * 4 * sizeof(float);
        buffer = new ComputeBuffer(colorIntervals.Length, totalSize);
        buffer.SetData(colorIntervals);
    }


    // This code is not used but it is a good example of how to generate the noise using the CPU
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

        Color n00 = GetColorFromValue(Mathf.PerlinNoise(x0, y0));
        Color n10 = GetColorFromValue(Mathf.PerlinNoise(x1, y0));
        Color n01 = GetColorFromValue(Mathf.PerlinNoise(x0, y1));
        Color n11 = GetColorFromValue(Mathf.PerlinNoise(x1, y1));

        Color ix0 = Color.Lerp(n00, n10, sx);
        Color ix1 = Color.Lerp(n01, n11, sx);
        Color value = Color.Lerp(ix0, ix1, sy);

        return value;
    }

}
