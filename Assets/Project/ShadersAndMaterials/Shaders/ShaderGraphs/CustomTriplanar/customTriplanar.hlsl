//UNITY_SHADER_NO_UPGRADE
#ifndef MEGASCRIPT_INCLUDED
#define MEGASCRIPT_INCLUDED

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    
// unity-custom-func-begin
void MegaScript_float(float3 Normal, float3 MainScale, float3 MainOffset, float3 SecScale, float3 SecOffset, float3 IntScale, float3 IntOffset, float3 SecIntScale, float3 SecIntOffset, float3 Sinner, float3 Cosser, float3 SSinner, float3 SCosser, float3 Position, float B, float Int, float SecInt, float3 CPos, float3 CWidth, float3 CGrad, float Ison, float MinN, float MaxN, float IntBlend, UnityTexture2D Main, UnityTexture2D MainNormal, float Sec, float3 SecNormal, float3 IntNormal, float3 SecIntNormal, float NormalStrength, float CPPos, float CPWidth, float CPGrad, float CCGrad, float CCHeight, float4 MC, float4 SC, float4 AC, float Met, float SMet, float Glo, float SGlo, float SmoothS, float SmoothE, float SmoothSm, out float3 Color, out float3 WorldNormal, out float Smoothness, out float Metallic){
float3 triblend = (pow(Normal, 4));
triblend /=max(dot(triblend,half3(1,1,1)),0.0001);   
half m = max(triblend.x, max(triblend.y, triblend.z));   

float3 Noblend = float3(triblend.x == m, triblend.y == m, triblend.z == m); 
triblend = lerp(Noblend, triblend, B);  

//Rotation Matrixes for the interpolators

                float2x2 mat1 = float2x2(Cosser.x, -Sinner.x, Sinner.x, Cosser.x); float2x2 mat2 = float2x2(Cosser.y, Sinner.y,Sinner.y, Cosser.y); float2x2 mat3 = float2x2(Cosser.z, Sinner.z, Sinner.z, Cosser.z);

                //Warning! high rotation will result in the normals not being correct, so only rotate a little

  	      float2x2 mat21 = float2x2(SCosser.x, -SSinner.x, SSinner.x, SCosser.x); float2x2 mat22 = float2x2(SCosser.y, SSinner.y,SSinner.y, SCosser.y); float2x2 mat23 = float2x2(SCosser.z, SSinner.z, SSinner.z, SCosser.z);


half3 flipper = sign(dot(triblend,Normal));
half2 flipx = half2(flipper.x,1);
half2 flipy = half2(flipper.y,1);
half2 flipz = half2(-flipper.z,1);

float2 ColorUVx = flipx * (Position.zy * MainScale.zy + MainOffset.zy);     

float2 ColorUVy =  flipy * (Position.xz * MainScale.xz + MainOffset.xz);                   

float2 ColorUVz =  flipz * (Position.xy * MainScale.xy + MainOffset.xy);


float2 SecColorUVx = flipx* (Position.zy * SecScale.zy + SecOffset.zy);                  

float2 SecColorUVy = flipy * (Position.xz * SecScale.xz + SecOffset.xz);                   

float2 SecColorUVz = flipz * (Position.xy * SecScale.xy + SecOffset.xy);

// float2 IntUVx = mul(mat1,flipx*(Position.zy * IntScale.zy + IntOffset.zy));                  
//
// float2 IntUVy = mul(mat2,flipy*(Position.xz * IntScale.xz + IntOffset.xz));                   
//
// float2 IntUVz = mul(mat3,flipz*(Position.xy * IntScale.xy + IntOffset.xy));
//
// float2 SecIntUVx = mul(mat21,flipx*(Position.zy * SecIntScale.zy + SecIntOffset.zy));                  
//
// float2 SecIntUVy = mul(mat22,flipy*(Position.xz * SecIntScale.xz + SecIntOffset.xz));                   
//
// float2 SecIntUVz = mul(mat23,flipz*(Position.xy * SecIntScale.xy + SecIntOffset.xy));


// float c3olX = lerp(Int.x, SecInt.x, IntBlend);                      //Interpolator Bump Texture value
//
// float c3olY = lerp(Int.y, SecInt.y, IntBlend);
//
// float c3olZ = lerp(Int.z, SecInt.z, IntBlend);

float dn = lerp(Int, SecInt, IntBlend);                                    //Unclamped texture for more consistent reference points, Some n values would cause things to break lol

float3 v = abs(Position - CPos) - CWidth;



float nn = max(v.x / CGrad.x, max(v.y / CGrad.y, v.z / CGrad.z)) / 2;

nn = saturate(nn) *Ison;

dn = saturate(dn - nn);

float n = saturate(clamp(dn, MinN, MaxN) - MinN) / (MaxN - MinN);

float4 col = float4(1, 1, 1, 1);
float4 c2ol = float4(1, 1, 1, 1);

half3 B1X = half3(0.5, 0.5, 1);

half3 B1Y = half3(0.5, 0.5, 1);

half3 B1Z = half3(0.5, 0.5, 1);

half3 B2X = half3(0.5, 0.5, 1);

half3 B2Y = half3(0.5, 0.5, 1);

half3 B2Z = half3(0.5, 0.5, 1);

half3 inormalX = half3(0.5, 0.5, 1);

half3 inormalY = half3(0.5, 0.5, 1);

half3 inormalZ = half3(0.5, 0.5, 1);

if (n < 1)

{

        float4 colX = tex2D(Main, ColorUVx);                                                  

        float4 colY = tex2D(Main, ColorUVy);

        float4 colZ = tex2D(Main, ColorUVz);

        col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

        B1X = UnpackNormal(tex2D(MainNormal, ColorUVx));

        B1Y = UnpackNormal(tex2D(MainNormal, ColorUVy));

        B1Z = UnpackNormal(tex2D(MainNormal, ColorUVz));

}

if (n > 0)

{

           float4 c2olX = Sec;
           
           float4 c2olY = Sec;
           
           float4 c2olZ = Sec;
           
           c2ol = c2olX * triblend.x + c2olY * triblend.y + c2olZ * triblend.z;
    
           // c2ol = Sec;

           B2X = SecNormal.x;

           B2Y = SecNormal.y;

           B2Z = SecNormal.z;

}

half3 tnormalX = lerp(B1X, SecNormal, n);   

half3 tnormalY = lerp(B1Y, SecNormal, n);

half3 tnormalZ = lerp(B1Z, SecNormal, n);

float i = 0;
if (n - int(n) != 0)

                {

                    inormalX = lerp(IntNormal.x, SecIntNormal.x, IntBlend); 

                    inormalY = lerp(IntNormal.y, SecIntNormal.y, IntBlend); 

                    inormalZ = lerp(IntNormal.z, SecIntNormal.z, IntBlend);

                    inormalX.x *= sign(NormalStrength);

                    inormalY.x *= sign(NormalStrength);

                    inormalZ.x *= sign(NormalStrength);

                    inormalX.y *= -sign(NormalStrength);

                    inormalY.y *= -sign(NormalStrength);

                    inormalZ.y *= -sign(NormalStrength);

                    inormalX = lerp(tnormalX, inormalX, abs(NormalStrength));    

                    inormalY = lerp(tnormalY, inormalY, abs(NormalStrength));

                    inormalZ = lerp(tnormalZ, inormalZ, abs(NormalStrength));

                    i = saturate((dn > MinN && dn < MaxN) - nn);

                    tnormalX = lerp(tnormalX, inormalX, i);

                    tnormalY = lerp(tnormalY, inormalY, i);

                    tnormalZ = lerp(tnormalZ, inormalZ, i);

}

tnormalX = half3(tnormalX.xy + Normal.zy, Normal.x);

tnormalY = half3(tnormalY.xy + Normal.xz, Normal.y);

tnormalZ = half3(tnormalZ.xy + Normal.xy, Normal.z); 



WorldNormal = normalize(tnormalX.zyx * triblend.x +  tnormalY.xzy * triblend.y +  tnormalZ.xyz * triblend.z);

float colp = (abs(dn - CPPos) - CPWidth)/CPGrad;   
float4 cc = lerp(lerp(SC, MC, clamp((Position.y - CCHeight)/CCGrad, 0,1)), AC, saturate(colp));
float4 ccc = lerp(col, c2ol, n) * cc;
float sm = (abs(dn - SmoothS) -SmoothE) / SmoothSm;
Color = ccc;
Metallic= lerp(Met, SMet, saturate(sm))* ccc.a;
Smoothness  = lerp(Glo, SGlo, saturate(sm))* ccc.a;
}
// unity-custom-func-end

#endif //MEGASCRIPT_INCLUDED