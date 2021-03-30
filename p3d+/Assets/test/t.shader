Shader "Custom/t"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
   
        Tags { "RenderType"="Opaque" }
        LOD 200

           Pass
         {
             CGPROGRAM
             #pragma target 3.0
             #pragma vertex vert
             #pragma fragment frag
 
             #include "UnityCG.cginc"
 
             struct appdata{
                 float4 position : POSITION;
                 float2 uv : TEXCOORD0;
             };
 
             struct v2f {
                 float4 position : SV_POSITION;
                 float2 uv : TEXCOORD0;
                 float4 worldSpacePos : TEXCOORD1;
             };
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
             float _Transparency;
             float _Slice;
 
             v2f vert(appdata vertex) 
             {
                 v2f output;
                 output.position = UnityObjectToClipPos(vertex.position);
                 output.worldSpacePos = mul(unity_ObjectToWorld, vertex.position);
                 output.uv = TRANSFORM_TEX(vertex.uv, _MainTex);
                 return output;
             }
 
             fixed4 frag(v2f input) : SV_TARGET
             {
                 
                 fixed4 col = tex2D(_MainTex, input.uv);
                 col.a = _Transparency;
                 
                 //clip(_Slice - input.position.y);
                 if(_Slice < input.worldSpacePos.y){
                     col.a = 0;
                 }
                 
                 return col;
             }
             ENDCG
         }
        
    }
    FallBack "Diffuse"
}
