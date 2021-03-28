   Shader "Instanced/InstancedShader" {
    Properties {
       _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"
 

         size  ("size", Int) = 1
    }
    SubShader {

Blend SrcAlpha OneMinusSrcAlpha 
     Cull  Off 

        Pass {

          
            CGPROGRAM

            #pragma vertex vert alpha
            #pragma fragment frag alpha
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            Texture2D _MainTex;
            int size;
             uint3 id ;
             float alpha;

           #if SHADER_TARGET >= 45
            StructuredBuffer<float4> positionBuffer;
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
              static int closestInteger(int a, int b) {
              int c1 = a - (a % b);
             int c2 = (a + b) - (a % b);
          if (a - c1 > c2 - a) {
             return c2;
          } else {
             return c1;
           }
            }
            v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            {
            #if SHADER_TARGET >= 45
                float4 data = positionBuffer[instanceID];
            #else
                float4 data = 0;
            #endif
             id = uint3(instanceID % size  ,(instanceID / size)-(instanceID/40000 )*200,instanceID/40000 );
             float3 localPosition = v.vertex.xyz +  id;
        

        
                float3 worldPosition = localPosition  ;
            
                float3 worldNormal = v.normal;



                float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
                float3 diffuse = (ndotl * _LightColor0.rgb);
                float3 color = v.color;

                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv_MainTex = v.texcoord;
                o.ambient = ambient;
                o.diffuse = diffuse;
                o.color =  _MainTex[float2(id.x ,id.y)];

             

                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target 
            {  
               
            
             
            if (  i.color.r+i.color.g+i.color.b>0.1){
             alpha=1;
            }
               return half4(i.color,alpha) ;
            }

            ENDCG

        }  

        
    }
}