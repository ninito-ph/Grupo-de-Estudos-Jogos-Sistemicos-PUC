Shader "Custom Mobile/Spotted Decay"
{
    Properties
    {
        _VerdantTex ("Verdant Texture", 2D) = "white" { }
        _DecayedTex ("Decayed Texture", 2D) = "black" { }
        _NoiseTex ("Decay Noise", 2D) = "white" {}
        _Blend ("Blend Value", Range (0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 150
        
        CGPROGRAM
        
        #pragma surface surf Lambert noforwardadd
        
        sampler2D _VerdantTex;
        sampler2D _DecayedTex;
        sampler2D _NoiseTex;
        
        struct Input
        {
            float2 uv_VerdantTex;
            float3 worldPos;
        };

        half _Blend;
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 textureVerdant = tex2D(_VerdantTex, IN.uv_VerdantTex);
            fixed4 textureDecayed = tex2D(_DecayedTex, IN.uv_VerdantTex);
            fixed4 textureNoise = tex2D(_NoiseTex, IN.worldPos.xyz);

            fixed4 noiseCutOff = step(textureNoise.r, _Blend); //lerp(textureVerdant, textureDecayed, _Blend);

            o.Albedo = textureVerdant + (textureDecayed * noiseCutOff);
            o.Alpha = textureDecayed.a;
        }

        ENDCG
        
    }
    
    Fallback "Mobile/VertexLit"
}