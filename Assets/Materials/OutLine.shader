Shader "Custom/OutLine" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
		_MainColor ("MainColor", color) = (0,0,0,1)
		_OutLineColor ("OutLineColor", color) = (0,0,0,1)
		_DrawOutLine ("DrawOutLine", Range(0.0,1.0)) = 0.0
    }
    SubShader
    {
        //描边
        pass
        {
            Cull front
            offset -5,-1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainColor;
			float4 _OutLineColor;
			float _DrawOutLine;
            struct v2f {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            } ;
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }
            float4 frag (v2f i) : COLOR
            {
				if(_DrawOutLine == 1.0)
				{
					return _OutLineColor;
				}
				else
				{
					float4 texCol = tex2D(_MainTex,i.uv);
					float4 outp = texCol * _MainColor;
					return outp;
				}
            }
            ENDCG
        }
        //绘制物体
        pass
        {
            offset 2,-1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainColor;
            struct v2f {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            } ;
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }
            float4 frag (v2f i) : COLOR
            {
                float4 texCol = tex2D(_MainTex,i.uv);
                float4 outp = texCol * _MainColor;
                return outp;
            }
            ENDCG
        }
    }
}