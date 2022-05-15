// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DistortAdd" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Main Texture", 2D) = "white" {}
	_NoiseTex ("Distort Texture (RG)", 2D) = "white" {}
	
	_HeatTime  ("Heat Time", range (-1,1)) = 0
	_ForceX  ("Strength X", range (0,1)) = 0.1
	_ForceY  ("Strength Y", range (0,1)) = 0.1

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
	Cull Off 
	Lighting Off 
	ZWrite Off//不记录此深度，通常用于半透明物体
	Fog { Color (0,0,0,0) }

	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
/************************************************************************************************************************/
/************************************************************************************************************************/
SubShader {
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile_particles
		#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord: TEXCOORD0;
		};

		struct v2f {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 uvmain : TEXCOORD1;
			float2 uvnoise : TEXCOORD2;
		};

		fixed4 _TintColor;
		fixed _ForceX;
		fixed _ForceY;
		half _HeatTime;
		float4 _MainTex_ST;
		float4 _NoiseTex_ST;
		sampler2D _NoiseTex;
		sampler2D _MainTex;
		float _BrightnessRatio;

		v2f vert (appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
			o.uvnoise = TRANSFORM_TEX(v.texcoord, _NoiseTex);
			return o;
		}

		fixed4 frag( v2f i ) : COLOR
		{
			//noise effect
			fixed4 offsetColor1 = tex2D(_NoiseTex, i.uvnoise + frac(_Time.xz*_HeatTime));
			fixed4 offsetColor2 = tex2D(_NoiseTex, i.uvnoise + frac(_Time.yx*_HeatTime));
			i.uvmain.x += ((offsetColor1.r + offsetColor2.r) - 1) * _ForceX;
			i.uvmain.y += ((offsetColor1.r + offsetColor2.r) - 1) * _ForceY;

			return 2.0f * i.color * _TintColor * tex2D( _MainTex, i.uvmain) * _BrightnessRatio;
		}
ENDCG
		}
}
/************************************************************************************************************************/
/************************************************************************************************************************/
// ------------------------------------------------------------------
// Fallback for older cards and Unity non-Pro	
SubShader {
	Blend DstColor Zero
	Pass {
		Name "BASE"
		SetTexture [_MainTex] {	combine texture }
	}
}
/************************************************************************************************************************/
/************************************************************************************************************************/
}
}
/************************************************************************************************************************/
/************************************************************************************************************************/
