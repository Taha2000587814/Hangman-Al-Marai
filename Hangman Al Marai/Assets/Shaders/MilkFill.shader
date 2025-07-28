Shader "UI/MilkFillShader"
{
	Properties
	{
		_MainTex("Milk Texture", 2D) = "white" {}
		_FillProgress("Fill Level", Range(0,1)) = 0.0
		_Color("Tint", Color) = (1,1,1,1)
	}

		SubShader
		{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" "Canvas" = "Overlay" }
			LOD 100

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _FillProgress;
				fixed4 _Color;

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// Vertical cutoff based on FillProgress
					if (i.uv.y > _FillProgress)
						discard;

					fixed4 col = tex2D(_MainTex, i.uv) * _Color;
					return col;
				}
				ENDCG
			}
		}

			Fallback "UI/Unlit/Transparent"
}
