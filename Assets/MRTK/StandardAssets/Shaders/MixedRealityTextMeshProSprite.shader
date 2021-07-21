// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// NOTE: MRTK Shaders are versioned via the MRTK.Shaders.sentinel file.
// When making changes to any shader's source file, the value in the sentinel _must_ be incremented.

// Text Mesh Pro Sprite shader with MRTK Additions

// MRTK Additions
// - Single Pass Instanced Stereo Rendering Support
// - Support for Clipping Primitives (Plane, Sphere, Box)
// - Added to MRTK namespace

Shader "Mixed Reality Toolkit/TextMeshProSprite" {

Properties {
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _Color ("Tint", Color) = (1,1,1,1)
		
    _StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255

    _CullMode ("Cull Mode", Float) = 0
	_ColorMask ("Color Mask", Float) = 15
    _ClipRect ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

    [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
}

SubShader {
    Tags
    { 
        "Queue"="Transparent" 
        "IgnoreProjector"="True" 
        "RenderType"="Transparent" 
        "PreviewType"="Plane"
        "CanUseSpriteAtlas"="True"
    }
		
    Stencil {
        Ref [_Stencil]
        Comp [_StencilComp]
        Pass [_StencilOp] 
        ReadMask [_StencilReadMask]
        WriteMask [_StencilWriteMask]
    }

    Cull [_CullMode]
    Lighting Off
    ZWrite Off
    ZTest [unity_GUIZTestMode]
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask [_ColorMask]

    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #pragma multi_compile __ UNITY_UI_CLIP_RECT
        #pragma multi_compile __ UNITY_UI_ALPHACLIP

        #pragma multi_compile __ _CLIPPING_PLANE _CLIPPING_SPHERE _CLIPPING_BOX

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"
        #include "MixedRealityShaderUtils.cginc"

#if defined(_CLIPPING_PLANE) || defined(_CLIPPING_SPHERE) || defined(_CLIPPING_BOX)
        #define _CLIPPING_PRIMITIVE
#else
        #undef _CLIPPING_PRIMITIVE
#endif

#if defined(_CLIPPING_PLANE)
        fixed _ClipPlaneSide;
        float4 _ClipPlane;
#endif

#if defined(_CLIPPING_SPHERE)
        fixed _ClipSphereSide;
        float4x4 _ClipSphereInverseTransform;
#endif

#if defined(_CLIPPING_BOX)
        fixed _ClipBoxSide;
        float4x4 _ClipBoxInverseTransform;
#endif

        uniform float        _VertexOffsetX;
        uniform float        _VertexOffsetY;

        struct appdata_t {

        float4 vertex   : POSITION;
        float4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {

        float4 vertex   : SV_POSITION;
        fixed4 color    : COLOR;
        half2 texcoord  : TEXCOORD0;				
        float3 worldPosition : TEXCOORD1;

        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO     
        };

        fixed4 _Color;
        fixed4 _TextureSampleAdd;
        float4 _ClipRect;

        v2f vert(appdata_t IN)
        {
            v2f OUT;

            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            OUT.worldPosition = IN.vertex;
            OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

            OUT.texcoord = IN.texcoord;

            float4 vertin = IN.vertex;
            vertin.x += _VertexOffsetX;
            vertin.y += _VertexOffsetY;
            float4 vPosition = UnityObjectToClipPos(vertin);

#ifdef UNITY_HALF_TEXEL_OFFSET
            OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif

            OUT.color = IN.color * _Color;
#if defined(_CLIPPING_PRIMITIVE)
            OUT.worldPosition = mul(unity_ObjectToWorld, vertin).xyz;
#endif

            return OUT;
        }

        sampler2D _MainTex; 

        fixed4 frag(v2f IN) : SV_Target
        {
            half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				
#if UNITY_UI_CLIP_RECT
            color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
#endif

    // Primitive clipping.
#if defined(_CLIPPING_PRIMITIVE)
            float primitiveDistance = 1.0;
#if defined(_CLIPPING_PLANE)
            primitiveDistance = min(primitiveDistance, PointVsPlane(IN.worldPosition, _ClipPlane) * _ClipPlaneSide);
#endif
#if defined(_CLIPPING_SPHERE)
            primitiveDistance = min(primitiveDistance, PointVsSphere(IN.worldPosition, _ClipSphereInverseTransform) * _ClipSphereSide);
#endif
#if defined(_CLIPPING_BOX)
            primitiveDistance = min(primitiveDistance, PointVsBox(IN.worldPosition, _ClipBoxInverseTransform) * _ClipBoxSide);
#endif
            color *= step(0.0, primitiveDistance);
#endif

#ifdef UNITY_UI_ALPHACLIP
            clip (color.a - 0.001);
#endif

            return color;
        }
		ENDCG
		}
	}
}