// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//场景特效
Shader "Custom/ParticleAdditive_BrightnessRatio" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}

	//亮度倍数
	_BrightnessRatio("BrightnessRatio", Range(1, 4)) = 1
}
/************************************************************************************************************************/
/************************************************************************************************************************/
Category {
	Tags {
		"Queue"="Transparent+5" //特效设置为最上层初始值为3005
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
	}

	Blend SrcAlpha One
	ColorMask RGB
	Cull Off
	Lighting Off
	ZWrite Off//不记录此深度，通常用于半透明物体
	//Shader里默认有如下代码  1. ZWrite On 2. ZTestLEqual
	Fog { Color (0,0,0,0) }//关闭特效被Fog干扰

	SubShader {
		Pass {
CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
#pragma multi_compile __ UNITY_UI_CLIP_RECT

			sampler2D _MainTex;
			fixed4 _TintColor;
			float _BrightnessRatio;

#ifdef UNITY_UI_CLIP_RECT
			float4 _ClipRect;
#endif
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
#ifdef UNITY_UI_CLIP_RECT
				float4 worldPosition : TEXCOORD1;
#endif
				fixed4 color : COLOR;
			};

			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.color = v.color;
#ifdef UNITY_UI_CLIP_RECT
				o.worldPosition = o.vertex;
#endif
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord) * _BrightnessRatio;

#ifdef UNITY_UI_CLIP_RECT
				fixed clip_a = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				col.a *= clip_a;
				clip(col.a - 0.01);
#endif

				//return fixed4(i.vertex.x, i.vertex.y, i.vertex.z, 1);

				//return fixed4(_ClipRect.x, _ClipRect.y, 0, 1);
				//return fixed4(_ClipRect.z, _ClipRect.w, 0, 1);
				//return fixed4(i.worldPosition.x, i.worldPosition.y, 0, 1);
				//return fixed4(clip_a, clip_a, clip_a,1);
				return col;
			}
ENDCG
		}
	}
}
/************************************************************************************************************************/
/************************************************************************************************************************/
}