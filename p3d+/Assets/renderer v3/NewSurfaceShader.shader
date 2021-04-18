   Shader "Instanced/InstancedShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {

        Pass {

            Tags {"LightMode"="ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
 #pragma target 4.5

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            uint id;
//RWStructuredBuffer<float3> PositionsFinal;
        #if SHADER_TARGET >= 45
            StructuredBuffer<float3> positionBuffer;
            StructuredBuffer<float4> colors;
        #endif

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float3 ambient : TEXCOORD1;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
                SHADOW_COORDS(4)
            };

          

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            {
            #if SHADER_TARGET >= 45
                float4 data = float4(positionBuffer[instanceID].x,positionBuffer[instanceID].y,positionBuffer[instanceID].z,1) ;
            #else
                float4 data = 0;
            #endif

                float3 localPosition = v.vertex.xyz * data.w;
                float3 worldPosition = data.xyz  + localPosition;
            


              float3 color =  colors[instanceID].xyz;

                v2f o;
               o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
       
               
                o.uv_MainTex = v.texcoord;
           
                o.diffuse = color;
                o.color = color;


                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 output = float4(i.color,1);
      
                return output;
            }

            ENDCG
        }
    }
}