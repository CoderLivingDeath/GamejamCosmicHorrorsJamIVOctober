Shader "Shader Graphs/MegaScript (Custom Function)"
{
Properties
{
[NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
[Normal][NoScaleOffset]_BumpMap("BumpMap", 2D) = "bump" {}
_MainScaleFactor("MainScaleFactor", Vector, 3) = (1, 1, 1, 0)
_MainOffsetFactor("MainOffsetFactor", Vector, 3) = (0, 0, 0, 0)
[NoScaleOffset]_UnderTex("UnderTex", 2D) = "white" {}
[Normal][NoScaleOffset]_BumpMapUnder("BumpMapUnder", 2D) = "bump" {}
_SecScaleFactor("SecScaleFactor", Vector, 3) = (1, 1, 1, 0)
_SecOffsetFactor("SecOffsetFactor", Vector, 3) = (0, 0, 0, 0)
_Blending("Blending", Range(0, 1)) = 0
_MainColor("MainColor", Color) = (1, 1, 1, 0)
_SecColor("SecColor", Color) = (1, 1, 1, 0)
_CChangeHeight("CChangeHeight", Float) = 0
_CChangeGrad("CChangeGrad", Float) = 0
_CChangePenPos("CChangePenPos", Range(0, 1)) = 0
_CChangePenWidth("CChangePenWidth", Range(0, 1)) = 0
_CChangePenGrad("CChangePenGrad", Range(0, 1)) = 0
_CChangeAmbCol("CChangeAmbCol", Color) = (1, 1, 1, 0)
_Glossiness("Glossiness", Range(0, 1)) = 0.5
_Metallic("Metallic", Range(0, 1)) = 0.5
_SmoothStart("SmoothStart", Range(0, 1)) = 0.5
_SmoothEnd("SmoothEnd", Range(0, 1)) = 0.5
_SmoothSmooth("SmoothSmooth", Range(0, 1)) = 0
_SecGlossiness("SecGlossiness", Range(0, 1)) = 0.5
_SecMetallic("SecMetallic", Range(0, 1)) = 0.5
[NoScaleOffset]_InterpBump("InterpBump", 2D) = "white" {}
[Normal][NoScaleOffset]_InterpNormal("InterpNormal", 2D) = "bump" {}
_IntScaleFactor("IntScaleFactor", Vector, 3) = (1, 1, 1, 0)
_IntOffsetFactor("IntOffsetFactor", Vector, 3) = (0, 0, 0, 0)
_Sin("Sin", Vector, 3) = (0, 0, 0, 0)
_Cos("Cos", Vector, 3) = (1, 1, 1, 0)
[NoScaleOffset]_SecInterpBump("SecInterpBump", 2D) = "white" {}
[Normal][NoScaleOffset]_SecInterpNormal("SecInterpNormal", 2D) = "bump" {}
_SecIntScaleFactor("SecIntScaleFactor", Vector, 3) = (1, 1, 1, 0)
_SecIntOffsetFactor("SecIntOffsetFactor", Vector, 3) = (0, 0, 0, 0)
_SecSin("SecSin", Vector, 3) = (0, 0, 0, 0)
_SecCos("SecCos", Vector, 3) = (1, 1, 1, 0)
_IntBlend("IntBlend", Range(0, 1)) = 0.5
_MinNoise("MinNoise", Range(0, 0.99)) = 0
_MaxNoise("MaxNoise", Range(0, 1)) = 1
[ToggleUI]_IntPosInf("IntPosInf", Float) = 0
_IntClampPos("IntClampPos", Vector, 3) = (0, 0, 0, 0)
_IntClampWidth("IntClampWidth", Vector, 3) = (1, 1, 1, 0)
_IntPosGrad("IntPosGrad", Vector, 3) = (1, 1, 1, 0)
_NoiseBumpStrength("NoiseBumpStrength", Float) = 1
}
SubShader
{
Tags
{
// RenderPipeline: <None>
"RenderType"="Opaque"
"Queue"="Geometry"
// DisableBatching: <None>
"ShaderGraphShader"="true"
}
Pass
{
    // Name: <None>
    Tags
    {
        // LightMode: <None>
    }

    // Render State
    // RenderState: <None>

    // Debug
    // <None>

    // --------------------------------------------------
    // Pass

    HLSLPROGRAM

    // Pragmas
    #pragma vertex vert
#pragma fragment frag

    // Keywords
    // PassKeywords: <None>
    // GraphKeywords: <None>

    // Defines
    #define ATTRIBUTES_NEED_NORMAL
    #define VARYINGS_NEED_POSITION_WS
    #define VARYINGS_NEED_NORMAL_WS
    /* WARNING: $splice Could not find named fragment 'PassInstancing' */
    #define SHADERPASS SHADERPASS_PREVIEW
#define SHADERGRAPH_PREVIEW 1

    // Includes
    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreInclude' */

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

    // --------------------------------------------------
    // Structs and Packing

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

    struct Attributes
{
 float3 positionOS : POSITION;
 float3 normalOS : NORMAL;
#if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
 uint instanceID : INSTANCEID_SEMANTIC;
#endif
};
struct Varyings
{
 float4 positionCS : SV_POSITION;
 float3 positionWS;
 float3 normalWS;
#if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
 uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
 FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
 float3 WorldSpaceNormal;
 float3 WorldSpacePosition;
};
struct VertexDescriptionInputs
{
};
struct PackedVaryings
{
 float4 positionCS : SV_POSITION;
 float3 positionWS : INTERP0;
 float3 normalWS : INTERP1;
#if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
 uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
 FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};

    PackedVaryings PackVaryings (Varyings input)
{
PackedVaryings output;
ZERO_INITIALIZE(PackedVaryings, output);
output.positionCS = input.positionCS;
output.positionWS.xyz = input.positionWS;
output.normalWS.xyz = input.normalWS;
#if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
output.instanceID = input.instanceID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
output.cullFace = input.cullFace;
#endif
return output;
}

Varyings UnpackVaryings (PackedVaryings input)
{
Varyings output;
output.positionCS = input.positionCS;
output.positionWS = input.positionWS.xyz;
output.normalWS = input.normalWS.xyz;
#if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
output.instanceID = input.instanceID;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
output.cullFace = input.cullFace;
#endif
return output;
}


    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 _MainTex_TexelSize;
float4 _UnderTex_TexelSize;
float4 _BumpMap_TexelSize;
float4 _BumpMapUnder_TexelSize;
float3 _MainScaleFactor;
float3 _MainOffsetFactor;
float3 _SecScaleFactor;
float3 _SecOffsetFactor;
float4 _MainColor;
float4 _SecColor;
float _CChangeHeight;
float _CChangeGrad;
float _CChangePenPos;
float _CChangePenWidth;
float _CChangePenGrad;
float4 _CChangeAmbCol;
float _Glossiness;
float _Metallic;
float _SmoothStart;
float _SmoothEnd;
float _SmoothSmooth;
float _SecGlossiness;
float _SecMetallic;
float4 _InterpBump_TexelSize;
float4 _InterpNormal_TexelSize;
float3 _IntScaleFactor;
float3 _IntOffsetFactor;
float3 _Sin;
float3 _Cos;
float4 _SecInterpBump_TexelSize;
float4 _SecInterpNormal_TexelSize;
float3 _SecIntScaleFactor;
float3 _SecIntOffsetFactor;
float3 _SecSin;
float3 _SecCos;
float _IntBlend;
float _MinNoise;
float _MaxNoise;
float _IntPosInf;
float3 _IntClampPos;
float3 _IntClampWidth;
float3 _IntPosGrad;
float _NoiseBumpStrength;
float _Blending;
CBUFFER_END


// Object and Global properties
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_UnderTex);
SAMPLER(sampler_UnderTex);
TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);
TEXTURE2D(_BumpMapUnder);
SAMPLER(sampler_BumpMapUnder);
TEXTURE2D(_InterpBump);
SAMPLER(sampler_InterpBump);
TEXTURE2D(_InterpNormal);
SAMPLER(sampler_InterpNormal);
TEXTURE2D(_SecInterpBump);
SAMPLER(sampler_SecInterpBump);
TEXTURE2D(_SecInterpNormal);
SAMPLER(sampler_SecInterpNormal);

    // Graph Includes
    // GraphIncludes: <None>

    // -- Property used by ScenePickingPass
    #ifdef SCENEPICKINGPASS
    float4 _SelectionID;
    #endif

    // -- Properties used by SceneSelectionPass
    #ifdef SCENESELECTIONPASS
    int _ObjectId;
    int _PassValue;
    #endif

    // Graph Functions
    
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

// dn = saturate(dn - nn);

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

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

    // Graph Vertex
    // GraphVertex: <None>

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreSurface' */

    // Graph Pixel
    struct SurfaceDescription
{
float4 Out;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
SurfaceDescription surface = (SurfaceDescription)0;
float3 _Property_331a53629d2d42beb28bc23d0ed21d79_Out_0_Vector3 = _MainScaleFactor;
float3 _Property_3ec67918498a481ea84c122a454e4f71_Out_0_Vector3 = _MainOffsetFactor;
float3 _Property_ee719e58a2934eb89d581a4f7370ca7b_Out_0_Vector3 = _SecScaleFactor;
float3 _Property_35f553f987e440cbbd5f9687e9b8b769_Out_0_Vector3 = _SecOffsetFactor;
float3 _Property_abed56341b14400f9e9e8d2b2f8a1657_Out_0_Vector3 = _IntScaleFactor;
float3 _Property_478b7043db69470b84d2ca711775a8dc_Out_0_Vector3 = _IntOffsetFactor;
float3 _Property_7355bc52ed0d4e6b9146f9b52c3ce1c6_Out_0_Vector3 = _SecIntScaleFactor;
float3 _Property_cf22cde7563a44fcb85a9d0c158dcdc0_Out_0_Vector3 = _SecIntOffsetFactor;
float3 _Property_3e266dc3fff64eeca78847c91d41bebc_Out_0_Vector3 = _Sin;
float3 _Property_a742824678f44c8792ba4eeda85aaec9_Out_0_Vector3 = _Cos;
float3 _Property_27e30baf83a64649a0947fa91bb68d61_Out_0_Vector3 = _SecSin;
float3 _Property_51fa07d0a4b14a64ae5d550ee9908c94_Out_0_Vector3 = _SecCos;
float _Property_1e982ffed5f54ec89ae63bc27b015c47_Out_0_Float = _Blending;
UnityTexture2D _Property_57b5a7ed02f947dab68916880397afb2_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_InterpBump);
UnityTexture2D _Property_9417fd7c087b4ef8858d17cc6ffbf2c6_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_SecInterpBump);
float3 _Property_84fa2577fbe144478103ec29e366525b_Out_0_Vector3 = _IntClampPos;
float3 _Property_89f9e37a27da476daf47cad6142608a9_Out_0_Vector3 = _IntClampWidth;
float3 _Property_c2c274a2ab0d4b2d851ea40c2a7e50ea_Out_0_Vector3 = _IntPosGrad;
float _Property_7dba5c38287642ad95cbebf15726b2c8_Out_0_Boolean = _IntPosInf;
float _Property_432360cd9c4a44098eddaf4a554adc87_Out_0_Float = _MinNoise;
float _Property_a84644d36165475f9f4f96d13cbcd257_Out_0_Float = _MaxNoise;
float _Property_8f04a5aaa0b04cd081c7127cbe139a5a_Out_0_Float = _IntBlend;
UnityTexture2D _Property_b2b2b19ef2dd4af49c6d03e2313b68b8_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
UnityTexture2D _Property_aaeb6fb8e0344125ac8e3f69c1edde30_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BumpMap);
UnityTexture2D _Property_f2878fe145db4812aff43fd9acb35313_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_UnderTex);
UnityTexture2D _Property_f639aac1a0df48e9bb6f305e00eca992_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BumpMapUnder);
UnityTexture2D _Property_fc9eba13ada64457a95a2f23c3ff60fc_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_InterpNormal);
UnityTexture2D _Property_d7933a99c0364595bf91a0d8eaa47310_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_SecInterpNormal);
float _Property_357a799d85e7448e92fbbe20e3acd542_Out_0_Float = _NoiseBumpStrength;
float _Property_44889a03372044b396bc1a51648efd6b_Out_0_Float = _CChangePenPos;
float _Property_1c78bb4568a549d4918dea249d5849fe_Out_0_Float = _CChangePenWidth;
float _Property_43a9992964aa4a30890e7b79d30e2048_Out_0_Float = _CChangePenGrad;
float _Property_27cc7e5e139243adaf1a6effe7aaa2ff_Out_0_Float = _CChangeGrad;
float _Property_da5ade1a1f684962af6e6256bee8cad5_Out_0_Float = _CChangeHeight;
float4 _Property_a8dc7877e3af46bc967df7d91376a024_Out_0_Vector4 = _MainColor;
float4 _Property_b4c326b5e00145959c4bb88c3431b7e4_Out_0_Vector4 = _SecColor;
float4 _Property_3dec348555c74c9d800ec2884eee3ef9_Out_0_Vector4 = _CChangeAmbCol;
float _Property_26482ed461764c5d8b59d16aee3418b4_Out_0_Float = _Metallic;
float _Property_b54ab714eef6489e9c65eedc52883dfd_Out_0_Float = _SecMetallic;
float _Property_0213efc365c54d30a9ed86ce7a6750c4_Out_0_Float = _Glossiness;
float _Property_f4829cc23dc24ecd801471c0d4d537cf_Out_0_Float = _SecGlossiness;
float _Property_ae43e7a7d3ed427ba749899791a9fbbf_Out_0_Float = _SmoothStart;
float _Property_0200f0a6c18e4cd69c2459d1053909a9_Out_0_Float = _SmoothEnd;
float _Property_aeec87807d7d4e44bf85449a50e55ca8_Out_0_Float = _SmoothSmooth;
float3 _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Color_0_Vector3;
float3 _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_WorldNormal_40_Vector3;
float _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Smoothness_41_Float;
float _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Metallic_42_Float;
MegaScript_float(IN.WorldSpaceNormal, _Property_331a53629d2d42beb28bc23d0ed21d79_Out_0_Vector3, _Property_3ec67918498a481ea84c122a454e4f71_Out_0_Vector3, _Property_ee719e58a2934eb89d581a4f7370ca7b_Out_0_Vector3, _Property_35f553f987e440cbbd5f9687e9b8b769_Out_0_Vector3, _Property_abed56341b14400f9e9e8d2b2f8a1657_Out_0_Vector3, _Property_478b7043db69470b84d2ca711775a8dc_Out_0_Vector3, _Property_7355bc52ed0d4e6b9146f9b52c3ce1c6_Out_0_Vector3, _Property_cf22cde7563a44fcb85a9d0c158dcdc0_Out_0_Vector3, _Property_3e266dc3fff64eeca78847c91d41bebc_Out_0_Vector3, _Property_a742824678f44c8792ba4eeda85aaec9_Out_0_Vector3, _Property_27e30baf83a64649a0947fa91bb68d61_Out_0_Vector3, _Property_51fa07d0a4b14a64ae5d550ee9908c94_Out_0_Vector3, IN.WorldSpacePosition, _Property_1e982ffed5f54ec89ae63bc27b015c47_Out_0_Float, _Property_57b5a7ed02f947dab68916880397afb2_Out_0_Texture2D, _Property_9417fd7c087b4ef8858d17cc6ffbf2c6_Out_0_Texture2D, _Property_84fa2577fbe144478103ec29e366525b_Out_0_Vector3, _Property_89f9e37a27da476daf47cad6142608a9_Out_0_Vector3, _Property_c2c274a2ab0d4b2d851ea40c2a7e50ea_Out_0_Vector3, ((float) _Property_7dba5c38287642ad95cbebf15726b2c8_Out_0_Boolean), _Property_432360cd9c4a44098eddaf4a554adc87_Out_0_Float, _Property_a84644d36165475f9f4f96d13cbcd257_Out_0_Float, _Property_8f04a5aaa0b04cd081c7127cbe139a5a_Out_0_Float, _Property_b2b2b19ef2dd4af49c6d03e2313b68b8_Out_0_Texture2D, _Property_aaeb6fb8e0344125ac8e3f69c1edde30_Out_0_Texture2D, _Property_f2878fe145db4812aff43fd9acb35313_Out_0_Texture2D, _Property_f639aac1a0df48e9bb6f305e00eca992_Out_0_Texture2D, _Property_fc9eba13ada64457a95a2f23c3ff60fc_Out_0_Texture2D, _Property_d7933a99c0364595bf91a0d8eaa47310_Out_0_Texture2D, _Property_357a799d85e7448e92fbbe20e3acd542_Out_0_Float, _Property_44889a03372044b396bc1a51648efd6b_Out_0_Float, _Property_1c78bb4568a549d4918dea249d5849fe_Out_0_Float, _Property_43a9992964aa4a30890e7b79d30e2048_Out_0_Float, _Property_27cc7e5e139243adaf1a6effe7aaa2ff_Out_0_Float, _Property_da5ade1a1f684962af6e6256bee8cad5_Out_0_Float, _Property_a8dc7877e3af46bc967df7d91376a024_Out_0_Vector4, _Property_b4c326b5e00145959c4bb88c3431b7e4_Out_0_Vector4, _Property_3dec348555c74c9d800ec2884eee3ef9_Out_0_Vector4, _Property_26482ed461764c5d8b59d16aee3418b4_Out_0_Float, _Property_b54ab714eef6489e9c65eedc52883dfd_Out_0_Float, _Property_0213efc365c54d30a9ed86ce7a6750c4_Out_0_Float, _Property_f4829cc23dc24ecd801471c0d4d537cf_Out_0_Float, _Property_ae43e7a7d3ed427ba749899791a9fbbf_Out_0_Float, _Property_0200f0a6c18e4cd69c2459d1053909a9_Out_0_Float, _Property_aeec87807d7d4e44bf85449a50e55ca8_Out_0_Float, _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Color_0_Vector3, _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_WorldNormal_40_Vector3, _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Smoothness_41_Float, _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Metallic_42_Float);
surface.Out = all(isfinite(_MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Color_0_Vector3)) ? half4(_MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Color_0_Vector3.x, _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Color_0_Vector3.y, _MegaScriptCustomFunction_25e289b18c5a4daead4460159d61a6d8_Color_0_Vector3.z, 1.0) : float4(1.0f, 0.0f, 1.0f, 1.0f);
return surface;
}

    // --------------------------------------------------
    // Build Graph Inputs

    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

    /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */

    // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
    float3 unnormalizedNormalWS =                       input.normalWS;
    const float renormFactor =                          1.0 / length(unnormalizedNormalWS);


    output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph


    output.WorldSpacePosition =                         input.positionWS;

    #if UNITY_UV_STARTS_AT_TOP
    #else
    #endif


#if UNITY_ANY_INSTANCING_ENABLED
#else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/PreviewVaryings.hlsl"
#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/PreviewPass.hlsl"

    ENDHLSL
}
}
CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
FallBack "Hidden/Shader Graph/FallbackError"
}