Shader"Custom/Background"
{
    Properties
    {
        [MainTexture] [NoScaleOffset] _MainTex("Base Texture", 2D) = "white" {}
        [NoScaleOffset] _Layer1("Shadow Layer", 2D) = "white" {}
        [NoScaleOffset] _Layer3("Lilypad Layer", 2D) = "white" {}
        [NoScaleOffset] _Layer4("Main Lilypad Shadow", 2D) = "white" {}
        _LilypadShadow1("Lilypad 1 UV", Vector) = (1, 1, 0, 0)
        _LilypadAngle1("Lilypad Angle 1", Float) = 0
        _LilypadShadow2("Lilypad 2 UV", Vector) = (1, 1, 0, 0)
        _LilypadAngle2("Lilypad Angle 2", Float) = 0
    }

    HLSLINCLUDE

    float Hash_Tchou_2_1(float2 i)
    {
        uint2 v = asuint(round(i));
        v.y ^= 1103515245U;
        v.x += v.y;
        v.x *= v.y;
        v.x ^= v.x >> 5u;
        v.x *= 0x27d4eb2du;
        return (v.x >> 8) * (1.0 / float(0x00ffffff));
    }

    float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
    {
        float x = Hash_Tchou_2_1(p);
        return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
    }

    float GradientNoise(float2 UV, float Scale)
    {
        float2 p = UV * Scale.xx;
        float2 ip = floor(p);
        float2 fp = frac(p);
        float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
        float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
        float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
        float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
        fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
        return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
    }

    inline float2 unity_voronoi_noise_randomVector (float2 UV, float offset)
    {
        float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
        UV = frac(sin(mul(UV, m)) * 46839.32);
        return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
    }

    float Voronoi(float2 UV, float AngleOffset, float CellDensity)
    {
        float2 g = floor(UV * CellDensity);
        float2 f = frac(UV * CellDensity);
        float t = 8.0;
        float3 res = float3(8.0, 0.0, 0.0);
        float Out = 0;

        for(int y=-1; y<=1; y++)
        {
            for(int x=-1; x<=1; x++)
            {
                float2 lattice = float2(x,y);
                float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
                float d = distance(lattice + offset, f);
                if(d < res.x)
                {
                    res = float3(d, offset.x, offset.y);
                    Out = res.x;
                }
            }
        }
        return Out;
    }

    float2 RotateUV(float2 UV, float2 Center, float Rotation)
    {
        Rotation = Rotation * (3.1415926f/180.0f);
        UV -= Center;
        float s = sin(Rotation);
        float c = cos(Rotation);
        float2x2 rMatrix = float2x2(c, -s, s, c);
        rMatrix *= 0.5;
        rMatrix += 0.5;
        rMatrix = rMatrix * 2 - 1;
        UV.xy = mul(UV.xy, rMatrix);
        UV += Center;
        return UV;
    }

    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Cull Back
		    Blend Off
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
                sampler2D _Layer1;
                float4 _Layer1_ST;
                sampler2D _Layer3;
                float4 _Layer3_ST;
                sampler2D _Layer4;
                float4 _Layer4_ST;
                float4 _LilypadShadow1;
                float _LilypadAngle1;
                float4 _LilypadShadow2;
                float _LilypadAngle2;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }
        
            float2 GetWaveWobble(float2 uv, float speed, float size)
            {
                float bottomLeftDisp = GradientNoise(uv + _Time.yy * speed, size) - 0.5;
                float bottomRightDisp = GradientNoise(uv + _Time.yy * speed * 1.01, size * 0.99) - 0.5;
    
                return float2(bottomRightDisp - bottomLeftDisp, bottomRightDisp + bottomLeftDisp);
            }

            float3 frag (v2f i) : SV_Target
            {
                float2 waterDistortion = GetWaveWobble(i.uv, 0.3, 2);
    
                float3 base = tex2D(_MainTex, i.uv + waterDistortion * 0.01).rgb;
    
                float2 lilypadWobble = GetWaveWobble(i.uv, 0.2, 1);
    
    
                //shadows
                float4 shadowColour = float4(0.4588235294, 0.5098039216, 0.7372549020, 0.6);
    
                //lilypads shadow
                float layer1 = tex2D(_Layer1, i.uv + lilypadWobble * 0.02 + waterDistortion * 0.007).r;
    
                //main lilypads shadow
                float2 lilypadShadow1UV = i.uv * _LilypadShadow1.xy + _LilypadShadow1.zw;
                lilypadShadow1UV = (lilypadShadow1UV + waterDistortion * 0.007) + float2(0.0213623047, 0.0087890625);
                lilypadShadow1UV = RotateUV(lilypadShadow1UV, float2((0.5 - _LilypadShadow1.z) / _LilypadShadow1.x, 0.5) * _LilypadShadow1.xy + _LilypadShadow1.zw, _LilypadAngle1);
                layer1 += tex2D(_Layer4, lilypadShadow1UV).r;
    
                float2 lilypadShadow2UV = i.uv * _LilypadShadow2.xy + _LilypadShadow2.zw;
                lilypadShadow2UV = (lilypadShadow2UV + waterDistortion * 0.007) + float2(0.0213623047, 0.0087890625);
                lilypadShadow2UV = RotateUV(lilypadShadow2UV, float2((0.5 - _LilypadShadow2.z) / _LilypadShadow2.x, 0.5) * _LilypadShadow2.xy + _LilypadShadow2.zw, _LilypadAngle2);
                layer1 += tex2D(_Layer4, lilypadShadow2UV).r;
    
                layer1 = saturate(layer1);
                layer1 *= shadowColour.a;
                base = (layer1 * shadowColour.rgb) + (1 - layer1) * base;
    
    
                //caustics
                float caustics = Voronoi(i.uv * float2(16.0 / 9.0, 1) + waterDistortion * 0.01, (_Time.y + 5) * 0.8, 20);
                caustics *= caustics; caustics *= caustics;
    
                base += float3(0.7058823529, 0.8352941176, 0.9568627451) * caustics.x * (1 - layer1) * 0.5;
    
                //lilypads
                float4 layer3 = tex2D(_Layer3, i.uv + lilypadWobble * 0.02);
                base = (layer3.a * layer3.rgb) + (1 - layer3.a) * base;
                return base;
            }

            ENDHLSL
        }
    }
}
