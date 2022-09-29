Shader "Custom/WindShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_WindDir ("WindDir", Vector) = (0,0,0,0)
		_UVScale ("UVScale", Float) = 1
		_WindPower ("WindPower", Float) = 2
		_WindChangeSpeed ("WindChangeSpeed", Float) = 2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard addshadow vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float4 color;
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float4 _WindDir;
		float _UVScale;
		float _WindPower;
		float _WindChangeSpeed;

		void vert (inout appdata_full v) {
			float movement = pow(v.texcoord.x,_WindPower);
			float4 noiseuv =  float4(v.vertex.xy*_UVScale + float2(_Time.x,_Time.x)*_WindChangeSpeed,0,0);

			float noiseVal = tex2Dlod (_MainTex,noiseuv).x;
			float3 localSpaceWind = mul( (float3x3)unity_WorldToObject, _WindDir.xyz);
			v.vertex.xyz = v.vertex.xyz + movement * (noiseVal * localSpaceWind);
			v.texcoord.xy = float2(movement,movement);
       	}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			o.Albedo = c.rgb;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
