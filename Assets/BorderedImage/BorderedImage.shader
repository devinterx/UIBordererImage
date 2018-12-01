Shader "UI/Bordered Image"
{
	Properties
	{
		[PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
		
		[HideInInspector]_ImageWidth("Image Width", float) = 100
		[HideInInspector]_ImageHeight("Image Height", float) = 100
		[HideInInspector]_BorderRadius("Border Radius", Vector) = (0, 0, 0, 0)
		[HideInInspector]_BorderSize("Border Size", float) = 0
		[HideInInspector]_PixelWorldScale("Pixel World Scale", float) = 1
		
		// UI.Mask
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
	}
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]
        
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			fixed4 _TextureSampleAdd;
	
			bool _UseClipRect;
			float4 _ClipRect;

			bool _UseAlphaClip;
			
			half _ImageWidth;
			half _ImageHeight;
			half4 _BorderRadius;
			half _BorderSize;
            half _PixelWorldScale;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert(appdata_t IN){
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
				#endif
				
				OUT.color = IN.color * (1 + _TextureSampleAdd);
				return OUT;
			}
			
			half visible(half2 coord, half4 r) {
			    r = half4(r.x / 100, r.y / 100, r.z / 100, r.w / 100);
			
				half4 p = half4(coord.x, coord.y, 1 - coord.x, 1 - coord.y);
				half v = min(min(min(p.x, p.y), p.z), p.w);
				bool4 b = bool4(all(p.xw < r.x), all(p.zw < r.y), all(p.zy < r.z), all(p.xy < r.w));
				half4 vis = r - half4(length(p.xw - r.x), length(p.zw - r.y), length(p.zy - r.z), length(p.xy - r.w));
				half4 dif = min(b * max(vis, 0), v) + (1 - b) * v;
				v = (any(b) * min(min(min(dif.x, dif.y), dif.z), dif.w) + v * (1 - any(b))) * 100;
    
				return v;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

				if (_UseClipRect) color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				if (_UseAlphaClip) clip (color.a - 0.001);
				
				if (_BorderSize > 0) {
					half l = (_BorderSize + 1 / _PixelWorldScale) / 2;
				 	color.a *= saturate((l - distance(visible(IN.texcoord, _BorderRadius), l)) * _PixelWorldScale);
				}
				else {
					color.a = saturate(visible(IN.texcoord, _BorderRadius) * _PixelWorldScale);
				}
				return color;
			}
			
			ENDCG
		}
	}
}

