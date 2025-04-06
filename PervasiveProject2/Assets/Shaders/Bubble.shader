Shader "Custom/Bubble"
{
    Properties
    {
        [MainTexture] _MainTex("Base Texture", 2D) = "white" {}
        _Grid("Grid", Vector) = (4, 2, 0, 0)
        _TimeStart("Pop at time", Float) = -1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardLit"

            Cull Back
		    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		    ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Declare all material properties in a single CBUFFER for SRP Batcher compatibility
            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Grid;
                float _TimeStart;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float index = min(floor((max(0, _Time.y - _TimeStart) / 0.0833333333)), ((_Grid.x * _Grid.y) - 1));
    
                float horizontalOffset = floor(index % _Grid.x);
                float verticalOffset = floor((index / _Grid.x) % _Grid.y);

                float4 col = tex2D(_MainTex, float2((i.uv.x + horizontalOffset) / _Grid.x, 1 - (((1 - i.uv.y) + verticalOffset) / _Grid.y)));
    
                return col;
            }

            ENDHLSL
        }
    }
}