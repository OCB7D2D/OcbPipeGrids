Shader "OCB/MRAO"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _EmissionColor("Emissive Color", Color) = (0,0,0,1)
        _EmissionMultiply("Emissive Multiplier", Range(1, 100)) = 1
        _MainTex("Albedo", 2D) = "white" { }
        _Normal("Normal", 2D) = "white" { }
        _Emissive("Emissive", 2D) = "black" { }
        _MRAO("MRAO", 2D) = "white" { }
        _MetallicFactor("Metallic Multiplier", Range(0, 2)) = 1
        _MetallicInvertor("Metallic Invertor", Range(0, 2)) = 1
        _SmoothnessFactor("Smoothness Multiplier", Range(0, 2)) = 1
        _SmoothnessInvertor("Smoothness Invertor", Range(0, 2)) = 1
        _OcclusionFactor("Occlusion Multiplier", Range(0, 2)) = 1
        _OcclusionInvertor("Occlusion Invertor", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normal;
        sampler2D _Emissive;
        sampler2D _MRAO;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float2 uv_Emissive;
            float2 uv_MRAO;
        };

        fixed4 _Color;
        fixed4 _EmissionColor;
        fixed _EmissionMultiply;
        fixed _MetallicFactor;
        fixed _MetallicInvertor;
        fixed _SmoothnessFactor;
        fixed _SmoothnessInvertor;
        fixed _OcclusionFactor;
        fixed _OcclusionInvertor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mrao = tex2D(_MRAO, IN.uv_MRAO);
            fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 emissive = tex2D(_Emissive, IN.uv_Emissive);
            o.Albedo = albedo.rgb; // +(_Color.rgb - albedo.rgb) * mrao.b;
            o.Emission = emissive.rgb * _EmissionColor.rgb * _EmissionMultiply;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            o.Metallic = 1 - (1 - mrao.r * _MetallicFactor) * _MetallicInvertor;
            o.Smoothness = 1 - (1 - mrao.a * _SmoothnessInvertor)* _SmoothnessFactor;
            o.Occlusion = 1 - (1 - mrao.g * _OcclusionFactor) * _OcclusionInvertor;
            o.Alpha = albedo.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
