Shader "OCB/Liquid/Bottle"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
        _Albedo("Color", Color) = (1,1,1,1)
        _Tintable("Tintable", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.5
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Occlusion("Occlusion", Range(0, 1)) = 0.5
        _Opacity("Opacity", Range(0, 1)) = 0.5
        _MainTex("Albedo", 2D) = "white" { }
    }

        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+5" }
        //Blend One OneMinusSrcAlpha
        //ZWrite Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        fixed4 _Albedo;
        fixed _Tintable;
        fixed _Opacity;
        fixed _Metallic;
        fixed _Smoothness;
        fixed _Occlusion;
    // fixed4 _EmissionColor;
        // fixed _EmissionMultiply;
        // fixed _MetallicFactor;
        // fixed _MetallicInvertor;
        // fixed _SmoothnessFactor;
        // fixed _SmoothnessInvertor;
        // fixed _OcclusionFactor;
        // fixed _OcclusionInvertor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = lerp(_Albedo.rgb, _Color.rgb, _Tintable);
            // o.Emission = emissive.rgb * _EmissionColor.rgb * _EmissionMultiply;
            // o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Occlusion = _Occlusion;
            o.Alpha = 1 - (1 - albedo.a) * _Opacity;
        }
        ENDCG
    }
        FallBack "Diffuse"
}