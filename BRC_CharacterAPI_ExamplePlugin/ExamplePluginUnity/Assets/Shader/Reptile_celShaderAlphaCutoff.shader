Shader "Reptile/celShaderAlphaCutoff" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _RampTex ("Ramp", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_ShadowBrightness ("Shadow Brightness", Range(0, 1)) = 0.2
		_AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}