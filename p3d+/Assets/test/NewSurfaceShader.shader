// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/NewShader 3"
{
    Properties
    {
        _BgColor ("Background Color", Color) = (0.1,0.1,0.3,0.0)
        _SampleCount ("Sample Count", Range(0,1024)) = 256
        _ShadowSampleCount ("Shadow Sample Count", Range(0,512)) = 16
        _ReflectionSampleCount ("Reflection Sample Count", Range(0,512)) = 128
        _ReflectionShadowSampleCount("Shadpow Sample Count in Reflection", Range(0,512)) = 8
        _Scene("Scene",int) = 0
        _Scale("Scene Scale",Range(1.0,200.0)) = 1
        _MandelBulbSeed("Mandelbulb Seed",Range(0.001,64)) = 4
        _MandelIterationCount("Mandelbulb Iterations",Range(1,128)) = 5
    }
    SubShader
    {
        Pass
        {
CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCg.cginc"
                       
            float4 _BgColor;
            float _Scale;
            float _MandelBulbSeed;
            int _MandelIterationCount;
            int _SampleCount;
            int _ShadowSampleCount;
            int _ReflectionSampleCount;
            int _ReflectionShadowSampleCount;
            int _Scene;
           
            struct appdata{float4 vertex : POSITION;};
            struct v2f{float4 vertex : SV_POSITION;};
           
            //WORLD STUFF###########################################################################################################################################
            //######################################################################################################################################################          
 
            float sphere( float3 _position, float _radius)
            {
                return length(_position) - _radius;
            }
            float box( float3 _position, float3 _bounding )
            {
                float3 t = abs(_position) - _bounding;
                return min(max(t.x,max(t.y,t.z)),0.0) + length(max(t,0.0));
            }
 
            float mengerSpone(float3 _position)
            {
               float f = box(_position,float3(225.0,225.0,225.0));
 
               float s = 0.5;
               for( int m=0; m<3; m++ )
               {                
                  float3 r = abs(1.0 - 3.0*abs((_position*s) - 2.0 * floor((_position*s)/2.0) - 1.0));
 
                  s *= 3.0;
 
                  float da = max(r.x,r.y);
                  float db = max(r.y,r.z);
                  float dc = max(r.z,r.x);
                  float c = (min(da,min(db,dc))-1.0)/s;
 
                  f = max(f,c);
               }
 
               return f;
            }
           
            float mandelbulb(float3 _position)
            {
                float3 z = _position;
                float dr = 1.0;
                float f = 0.0;
                for (int i = 0; i < _MandelIterationCount ; i++)
                {
                    f = length(z );
                    if (f>4) break;
                   
                    float theta = acos(z.z/f);
                    float phi = atan2(z.y,z.x);
                    dr =  pow( f, _MandelBulbSeed-1.0)*_MandelBulbSeed*dr + 1.0;
                   
                    float zr = pow( f,_MandelBulbSeed);
                    theta = theta*_MandelBulbSeed;
                    phi = phi*_MandelBulbSeed;
                   
                    z = zr*float3(sin(theta)*cos(phi), sin(phi)*sin(theta), cos(theta));
                    z+=_position;
                }
                return 0.5*log(f)*f/dr;
            }
           
 
            float fieldsCombined(float3 _position)
            {
                if(_Scene == 0)
                {
                    float f1 = box(_position / _Scale,float3(5.0,0.80,0.80)) * _Scale;
                    float f2 = box((_position + float3(4.0,3.0,0.0)) / _Scale,float3(1.0,5.0,1.0)) * _Scale;
                    float f3 = sphere(_position / _Scale,2.0) * _Scale;
                    float f4 = box((_position + float3(0.0,-1.0,0.0)) / _Scale,float3(25.0,0.10,25.0)) * _Scale;
                    return max(-f1,min(f2,min(f3,f4)));
                }
                if(_Scene == 1)
                    return mandelbulb(_position / _Scale) * _Scale;
                if(_Scene == 2)
                    return mengerSpone(_position / _Scale) * _Scale;
 
                return sphere(_position,1.0);
            }
           
            //######################################################################################################################################################          
            //######################################################################################################################################################          
 
           
            v2f vert(appdata v)
            {
                v2f    vertOut;
                vertOut.vertex = UnityObjectToClipPos(v.vertex);
                return vertOut;
            }
           
           
            float3 getNormal(float3 _origin){return normalize(float3(fieldsCombined(float3(_origin.x + 0.001, _origin.y, _origin.z)) - fieldsCombined( float3(_origin.x - 0.001, _origin.y, _origin.z)),fieldsCombined(float3(_origin.x, _origin.y + 0.001, _origin.z)) - fieldsCombined( float3(_origin.x, _origin.y - 0.001, _origin.z)),fieldsCombined(float3(_origin.x, _origin.y, _origin.z + 0.001)) - fieldsCombined( float3(_origin.x, _origin.y, _origin.z - 0.001))));}
       
            float4 castRay(float3 _origin, float3 _direction,float _offset, int _sampleCount)//xyzn
            {
                float distance = _offset;
                float3 position;
                float surfaceDistance;
                               
                for(int n = 0; n < _sampleCount; n ++)
                {
                    position = _origin + _direction * distance;
                    surfaceDistance = fieldsCombined(position);
                     
                       distance += surfaceDistance;
 
                    if(surfaceDistance < 0.001)
                    {
                        return float4(position, n);
                    }
                }
                return float4(position, n);
            }
           
            float shadow(float3 _origin,float3 _direction,int _sampleCount)
            {
                float distance = 0.1;
                float f = 1.0;
 
                for(int n = 0; n < _sampleCount; n ++)
                {
                    float3 position = _origin + _direction * distance;
                    float surfaceDistance = fieldsCombined(position);
                       distance += surfaceDistance;
 
                    if(surfaceDistance < 0.001)
                    {
                        return 0;
                    }
                   
                    f = min( f, 4 * surfaceDistance/distance);
                }
                return f;
            }
           
            float4 reflection(float3 _origin,float3 _direction)
            {
                float4 hitPoint = castRay(_origin,reflect(_direction,getNormal(_origin)),0.01,_ReflectionSampleCount);
                if(hitPoint.w == _ReflectionSampleCount)
                    return _BgColor;
                   
                float c = float(hitPoint.w) / float(_ReflectionSampleCount);
               
                return float4(c,c,c,0.0) * shadow(hitPoint.xyz,float3(1.0,-1.0,1.0),_ReflectionShadowSampleCount);
            }
           
           
            fixed4 frag(v2f _vertIn) : SV_Target
            {  
                float u = 2.0 * _vertIn.vertex.x / 1024.0 - 1.0;
                float v = 2.0 * _vertIn.vertex.y / 1024.0 - 1.0;
           
                float3 origin = _WorldSpaceCameraPos + UNITY_MATRIX_IT_MV[2] + UNITY_MATRIX_IT_MV[0].xyz * u + UNITY_MATRIX_IT_MV[1].xyz * v;
                float3 direction = -normalize(_WorldSpaceCameraPos - origin);
               
                float4 hitPoint = castRay(origin,direction,0.0,_SampleCount);
                if(hitPoint.w == _SampleCount)
                    return _BgColor;
                   
                float c = float(hitPoint.w) / float(_SampleCount);
                return float4(c,c,c,0.0) * shadow(hitPoint.xyz,float3(1.0,-1.0,1.0),_ShadowSampleCount) + (reflection(hitPoint.xyz,direction) / 2) ;
            }
ENDCG
        }  
    }
}
//Source
//
//http://blog.hvidtfeldts.net/
//http://www.iquilezles.org/index.html
//http://bugman123.com/index.html