Shader "Custom/TestShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _TextureScale ("Texture scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;
        float _TextureScale;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        UNITY_INSTANCING_BUFFER_START(Props)
   
        UNITY_INSTANCING_BUFFER_END(Props)

        float my_fmod(float a, float b)
        {
            float2 c = frac(abs(a / b)) * abs(b);
            return c; //if(a<0) c = 0-c;
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float x = IN.worldPos.x * _TextureScale;
            float y =  IN.worldPos.y * _TextureScale;
            float z = IN.worldPos.z * _TextureScale;
            float isUp = abs(IN.worldNormal.y);
            
            float2 offset = float2(my_fmod(z + x * (1 -isUp),0.1), my_fmod(y + x * isUp, 0.1));
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex + offset) * _Color;
            o.Albedo = c.rgb;
            
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
