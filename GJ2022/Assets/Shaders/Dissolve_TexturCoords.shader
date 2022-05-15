// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Dissolve_TexturCoords" {
Properties {
	_DissolveMainColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)	
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}

	_DissColor1("DissColor1", Color) = (1,1,1,1)
		DissColor1Widget("DissColor1Widget", Range(0, 1)) = 0.25
	_DissColor2("DissColor2", Color) = (1,1,1,1)
		DissColor2Widget("DissColor2Widget", Range(0, 1)) = 0.5
	_DissColor3("DissColor3", Color) = (1,1,1,1)
	_DissolveSrc ("DissolveSrc", 2D) = "white" {}

	_Amount ("Amount", Range (0, 1)) = 0.5
	_StartAmount("StartAmount", float) = 0.1
	_Illuminate ("Illuminate", Range (0, 0.95)) = 0.5

	//亮度倍数
	_BrightnessRatio("BrightnessRatio", Range(1, 4)) = 1
}
/************************************************************************************************************************/
/************************************************************************************************************************/
SubShader { 
	Tags { 
		"Queue"="Transparent" //特效设置为最上层初始值为3005
		"IgnoreProjector"="True" 
		"RenderType"="Transparent" 
	}

	Pass	{
		LOD 200
	//	Cull Off 
		Lighting Off 
		ZWrite On
	ZTest On
		Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_particles
		#pragma fragmentoption ARB_precision_hint_fastest
			
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _DissolveSrc;
		float4 _DissolveSrc_ST;
		float4 _MainTex_ST;

		fixed4 _DissolveMainColor;
		half4 _DissColor1;
		fixed DissColor1Widget;
		fixed DissColor2Widget;
		half4 _DissColor2;
		half4 _DissColor3;
		half _Shininess;
		half _Amount;
		static half3 Color = float3(1,1,1);

		half _Illuminate;
		half _StartAmount;
		float _BrightnessRatio;

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
			fixed4 color : COLOR;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			fixed4 color : COLOR;
		};

		v2f vert (appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.texcoord1 = TRANSFORM_TEX(v.texcoord, _DissolveSrc);
			o.color = v.color;
			return o;
		}
			
		fixed4 frag (v2f i) : SV_Target
		{		

			float ClipTex = tex2D(_DissolveSrc, i.texcoord1).r;
			float ClipAmount = ClipTex - _Amount;
			//return fixed4(ClipTex, ClipTex, ClipTex, 1);
			clip(ClipAmount);
			//float Clip = 0;
			//return fixed4(ClipAmount, ClipAmount, ClipAmount,1);
			fixed4 result = fixed4(0, 0, 0, 0);
			if (ClipAmount > 0)
			{
				fixed4 tex = tex2D(_MainTex, i.texcoord);
				result = 2.0f * tex * _DissolveMainColor * _BrightnessRatio;
				result.a *= i.color.a;

				if (result.a > 0 && _Amount > 0)
				{

					if (ClipAmount < _StartAmount)
					{
						half clV = 1 - ClipAmount / _StartAmount;
						fixed4 col1, col2;
						fixed wi;

						if (clV < DissColor1Widget)
						{
							col1 = result;
							col2 = _DissColor1;
							wi = clV / DissColor1Widget;
						}
						else if (clV < DissColor1Widget + DissColor2Widget)
						{
							col1 = _DissColor1;
							col2 = _DissColor2;
							wi = (clV - DissColor1Widget) / DissColor2Widget;
						}
						else
						{
							col1 = _DissColor2;
							col2 = _DissColor3;
							fixed wi1andwi2 = DissColor1Widget + DissColor2Widget;
							wi = (clV - wi1andwi2) / (1 - wi1andwi2);
						}


						result = lerp(col1, col2, wi);

						//		Color.x = _DissColor.x;
						//
						//		Color.y = _DissColor.y;
						//
						//		Color.z = _DissColor.z;
						//
						//	result.rgb  = (result.rgb *((Color.x+Color.y+Color.z))* Color*((Color.x+Color.y+Color.z)))/(1 - _Illuminate);
						result.rgb /= (1 - _Illuminate);
					}

				}
			}
 

			   

			return result;
		}
ENDCG
	}
}
/************************************************************************************************************************/
/************************************************************************************************************************/
FallBack "Specular"
}
