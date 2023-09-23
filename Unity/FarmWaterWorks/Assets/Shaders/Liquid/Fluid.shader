Shader "OCB/Liquid/Fluid"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
        _Albedo("Color", Color) = (1,1,1,1)
        _Tintable("Tintable", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.5
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Occlusion("Occlusion", Range(0, 1)) = 0.5
        _Transparency("Transparency", Range(0, 1)) = 0.5
        _MainTex("Albedo", 2D) = "white" { }
    }

        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+5" }
        //Blend One OneMinusSrcAlpha
        //ZWrite Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:premul

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float4 color : COLOR;
        };

        fixed4 _Color;
        fixed4 _Albedo;
        fixed _Tintable;
        fixed _Transparency;
        fixed _Metallic;
        fixed _Smoothness;
        fixed _Occlusion;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            IN.color = _Color;
            // fixed4 mrao = tex2D(_MRAO, IN.uv_MRAO);
            // fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);
            // fixed4 emissive = tex2D(_Emissive, IN.uv_Emissive);
            o.Albedo = lerp(IN.color.rgb, _Albedo.rgb, _Tintable); // +(_Color.rgb - albedo.rgb) * mrao.b;
            // o.Emission = emissive.rgb * _EmissionColor.rgb * _EmissionMultiply;
            // o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            // o.Metallic = 1 - (1 - mrao.r * _MetallicFactor) * _MetallicInvertor;
            // o.Smoothness = 1 - (1 - mrao.a * _SmoothnessInvertor)* _SmoothnessFactor;
            // o.Occlusion = 1 - (1 - mrao.g * _OcclusionFactor) * _OcclusionInvertor;
            o.Alpha = _Transparency;
        }
        ENDCG
    }
        FallBack "Diffuse"
}