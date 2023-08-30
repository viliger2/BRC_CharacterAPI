Shader "Custom/Outline" {
	Properties {
		_Color ("Color", Vector) = (0,0,0,0)
		_Amount ("Extrusion Amount", Range(0, 1)) = 0.01
		_FOV ("fov", Float) = 26.82
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "VertexLit"
}