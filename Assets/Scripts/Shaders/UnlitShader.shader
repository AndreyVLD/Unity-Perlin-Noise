Shader "Custom/UnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Input for the vertex shader
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Input for the fragment shader
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            /*
            * Vertex Shader - executed for every vertex
            * This is where we transform the vertices.
            * 
            * appdata V - input vertex data - contains the vertex position and uv
            * v2f - output vertex data - contains the new vertex position and uv
			*/
            v2f vert (appdata v)
            {
                v2f o;

                // Manipulating the vertex in game space before projection to camera
                v.vertex.y = sin(v.vertex.x + 2* _Time.y)*0.5;

                // Project onto camera space (CLIP = camera view)
                o.vertex = UnityObjectToClipPos(v.vertex);


                // Pass the texture
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            /* Pixel Shader - executed for every pixel (also called Fragemnt shader)
            * 
            * v2f i - input vertex data - contains the vertex position and uv
            * fixed4 - output color - the output color for that pixel
			*/
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                return float4(i.uv.x,i.uv.x,i.uv.x,1);
            }
            ENDCG
        }
    }
}
