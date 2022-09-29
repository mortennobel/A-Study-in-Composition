Shader "Custom/DepthNormals" {
	Properties {
	   _MainTex ("", 2D) = "white" {}
	   _HighlightDirection ("Highlight Direction", Vector) = (1, 0,0)
	}

	SubShader {
		Tags { "RenderType"="Opaque" }

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _CameraDepthNormalsTexture;

			struct v2f {
			   float4 pos : SV_POSITION;
			   float4 scrPos: TEXCOORD1;
			};

			//Our Vertex Shader
			v2f vert (appdata_base v){
			   v2f o;
			   o.pos = UnityObjectToClipPos (v.vertex);
			   o.scrPos=ComputeScreenPos(o.pos);
			   o.scrPos.y = 1 - o.scrPos.y;
			   return o;
			}

			sampler2D _MainTex;
			float4 _HighlightDirection;

			//Our Fragment Shader
			half4 frag (v2f i) : COLOR{
				
				float3 normalValues;
				float depthValue;

				//extract depth value and normal values
				DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.scrPos.xy), depthValue, normalValues);
				half4 ret;
				ret.xyz = float4(normalValues, 1) * 0.5 + 0.5;
				ret.w = depthValue;
				return ret;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
