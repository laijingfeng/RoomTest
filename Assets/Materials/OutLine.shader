Shader "Custom/OutLine" {
	Properties {
		[Header(Textures)]
		_MainTexture("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_TextureColor("Color", Color) = (1,1,1,1)

		[Header(Outline Settings)]
		_Scale("OutlineSize", Range(1,1.1)) = 1.05
		_OutlineColor("Color", Color) = (0,0,0,1)
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTexture;
	float4 _OutlineColor;
	float4 _TextureColor;
	float _Scale;

	struct appdata {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	ENDCG		

	SubShader {
		Tags{
			"RenderType" = "Opaque"
			"Queue" = "Transparent"
		}

		Pass{
			Zwrite off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert (appdata IN)
			{
				v2f OUT;
				float scale = _Scale;
				float4x4 scaleMat = float4x4(scale, 0.0, 0.0, 0.0,
											 0.0, scale, 0.0, 0.0,
											 0.0, 0.0, scale, 0.0,
											 0.0, 0.0, 0.0, 1.0);

				OUT.pos = mul(scaleMat, IN.vertex);
				OUT.pos = mul(UNITY_MATRIX_MVP, OUT.pos);
				return OUT;
			}

			fixed4 frag (v2f IN) : COLOR
			{
				fixed4 col = _OutlineColor;
				return col;
			}
			ENDCG
		}

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert (appdata IN)
			{
				v2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}

			fixed4 frag (v2f IN) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTexture, IN.uv);
				return col * _TextureColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}