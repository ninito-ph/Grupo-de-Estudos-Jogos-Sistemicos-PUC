Shader "Custom Mobile/Simple Decay"
{
    Properties
    {
        _VerdantTex ("Verdant Texture", 2D) = "white" { }
        _DecayedTex ("Decayed Texture", 2D) = "black" { }
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
        
        struct Input
        {
            float2 uv_VerdantTex;
        };

        half _Blend;
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 textureVerdant = tex2D(_VerdantTex, IN.uv_VerdantTex);
            fixed4 textureDecayed = tex2D(_DecayedTex, IN.uv_VerdantTex);

            fixed4 finalColor = lerp(textureVerdant, textureDecayed, _Blend);

            o.Albedo = finalColor.rgb;
            o.Alpha = textureDecayed.a;
        }

        ENDCG
        
    }
    
    Fallback "Mobile/VertexLit"
}