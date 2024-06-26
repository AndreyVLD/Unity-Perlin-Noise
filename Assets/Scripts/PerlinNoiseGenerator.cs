using UnityEngine;

public class PerlinNoiseGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;

    public float scale = 20;
    public float translateSpeed = 10f;
    public float zoomSpeed = 5f;


    public float offsetX = 100f;
    public float offsetY = 100f;

    public ColorInterval[] colorIntervals;

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

        userInput();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = ((float)x - width/2) / width * scale + offsetX;
                float yCoord = ((float)y - height/2) / height * scale + offsetY;

                texture.SetPixel(x, y, GetNoiseColor(xCoord, yCoord));

            }
        }

        texture.Apply();
    }

    void userInput()
    {
        offsetX += Input.GetAxis("Horizontal") * Time.deltaTime * translateSpeed;
        offsetY += Input.GetAxis("Vertical") * Time.deltaTime * translateSpeed;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float zoomFactor = 1f - scroll * zoomSpeed * Time.deltaTime;
            scale *= zoomFactor;
            scale = Mathf.Clamp(scale * zoomFactor, 1f, 100f);
        }
    }

    Color GetNoiseColor(float xCoord, float yCoord)
    {
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        foreach(ColorInterval interval in colorIntervals)
        {
            if (sample >= interval.start && sample < interval.end)
                return Color.Lerp(interval.colorStart, interval.colorEnd, (sample - interval.start) / (interval.end - interval.start));
        }
        return new Color(0.5f,0.5f,0.5f);
    }
}
