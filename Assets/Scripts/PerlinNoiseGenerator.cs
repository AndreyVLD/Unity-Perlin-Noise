using UnityEngine;

public class PerlinNoiseGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public int width = 256;
    public int height = 256;

    [SerializeField]
    private float scale = 4;

    [Header("Speed Settings")]
    [Range(1, 100)]
    public float translateSpeed = 10f;

    [Range(1, 100)]
    public float zoomSpeed = 1.00001f;


    [Header("Noise Settings")]
    [Range(1, 10)]
    public int octaves = 9;
    [Range(0, 1)]
    public float persistence = 0.5f;
    [Range(0, 10)]
    public float lacunarity = 3.0f;
    public ColorInterval[] colorIntervals;

    // Compute Shader that will generate the noise
    [Header("Shader")]
    public ComputeShader noiseShader;
    public ComputeShader postProcessingShader;

    [Header("Post Processing Settings")]
    [Range(0, 10)]
    public float stepSize = 0.1f;
    [Range(50, 10000)]
    public int maxSteps = 100;
    public Vector3 lightPos = new Vector3(1, 1, 1);

    private float offsetX = 100f;
    private float offsetY = 100f;
    private int type = 0;

    private int kernelHandleNoise;
    private int kernelHandlePostProcessing;


    // Buffer that will store the color intervals for the compute shader
    private ComputeBuffer buffer;

    private RenderTexture heightMap;
    private RenderTexture renderTextureNoiseColored;
    private RenderTexture renderTexturePostProcessed;

    private MeshRenderer rend;


    // Start is called before the first frame update
    void Start()
    {
        // Get the mesh renderer and set the render texture to it
        rend = GetComponent<MeshRenderer>();

        SetUpTexture(out renderTextureNoiseColored);
        SetUpTexture(out renderTexturePostProcessed);
        SetUpTexture(out heightMap);

        rend.material.mainTexture = renderTexturePostProcessed;

        // Find the kernel handle for the compute shaders
        kernelHandleNoise = noiseShader.FindKernel("CSMain");
        kernelHandlePostProcessing = postProcessingShader.FindKernel("CSMain");

        // Compute the buffer data - this contains the color intervals
        ComputeBufferData();

        SetUpNoiseShader();
        SetUpPostProcessShader();

        GenerateNoise();
    }

    void Update()
    {
        if (UserInput())
        {
            // If the user input has changed, generate the noise again
        }
        GenerateNoise();

    }

    // Generate the noise using the compute shader
    private void GenerateNoise()
    { 
        NoiseShader();
        SetUpPostProcessShader(); // Should not be here!
        PostProcessShader();
    }

    // Initialize the compute shader parameters
    private void SetUpNoiseShader()
    {
        noiseShader.SetInt("width", width);
        noiseShader.SetInt("height", height);

        noiseShader.SetTexture(kernelHandleNoise, "HeightMap", heightMap);
        noiseShader.SetTexture(kernelHandleNoise, "Result", renderTextureNoiseColored);
        noiseShader.SetBuffer(kernelHandleNoise, "ColorIntervals", buffer);
    }

    // Post processing will take the noise texture and apply a post processing effect to it
    private void SetUpPostProcessShader()
    {
        postProcessingShader.SetInt("width", width);
        postProcessingShader.SetInt("height", height);
        postProcessingShader.SetInt("maxSteps", maxSteps);
        postProcessingShader.SetFloat("stepSize", stepSize);
        postProcessingShader.SetVector("lightPos", new Vector4(lightPos.x,lightPos.y,lightPos.z,0));

        postProcessingShader.SetTexture(kernelHandlePostProcessing, "Result", renderTexturePostProcessed);

        // The input texture is the noise texture compute by the previous compute shader both colored and the original grayscale
        postProcessingShader.SetTexture(kernelHandlePostProcessing, "Input", renderTextureNoiseColored);
        postProcessingShader.SetTexture(kernelHandlePostProcessing, "HeightMap", heightMap);
    }

    private void NoiseShader()
    {
        // Set the input parameters for the compute shader

        noiseShader.SetFloat("scale", scale);
        noiseShader.SetFloat("offsetX", offsetX);
        noiseShader.SetFloat("offsetY", offsetY);
        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("persistence", persistence);
        noiseShader.SetFloat("lacunarity", lacunarity);
        noiseShader.SetFloat("numColors", colorIntervals.Length);
        noiseShader.SetFloat("type", type);

        // Compute the number of thread groups: width and height divided by number of workers (8 by 8 workers)
        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

        // Dispatch the compute shader
        noiseShader.Dispatch(kernelHandleNoise, threadGroupsX, threadGroupsY, 1);
    }

    private void PostProcessShader()
    {
        // Compute the number of thread groups: width and height divided by number of workers (8 by 8 workers)
        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);

        postProcessingShader.Dispatch(kernelHandlePostProcessing, threadGroupsX, threadGroupsY, 1);
    }

    private void SetUpTexture(out RenderTexture texture)
    {
        texture = new RenderTexture(width, height, 24);
        texture.enableRandomWrite = true;
        texture.Create();
    }

    private bool UserInput()
    {
        bool changed = false;

        float moveX  = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector2 move = new Vector2(moveX, moveY);

        if(move.magnitude > 1)
            move = move.normalized;

        move *= Time.deltaTime * translateSpeed * scale;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (moveX != 0 || moveY != 0)
        {
            offsetX += move.x;
            offsetY += move.y;
            changed = true;
        }

        if (scroll != 0)
        {
            if(scroll > 0)
                scale /= zoomSpeed;
            else
                scale *= zoomSpeed;
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

    private void ComputeBufferData()
    {
        int totalSize = 2 * sizeof(float) + 2 * 4 * sizeof(float);
        buffer = new ComputeBuffer(colorIntervals.Length, totalSize);
        buffer.SetData(colorIntervals);
    }


    // This code is not used but it is a good example of how to generate the noise using the CPU
    private Color GetColorFromValue(float value)
    {

        foreach(ColorInterval interval in colorIntervals)
        {
            if (value >= interval.start && value < interval.end)
                return Color.Lerp(interval.colorStart, interval.colorEnd, (value - interval.start) / (interval.end - interval.start));
        }
        return new Color(0.5f,0.5f,0.5f);
    }

    private Color GetBilinearInterpolatedColor(int xCoord, int yCoord)
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
