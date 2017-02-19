Shader "Custom/SonarGround" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[HiddenInInspector]_PlayerPos ("Player Position", Vector) = (0,0,0,0)
		_RadiusMax ("Radius Max", Float) = 0.5
		_RadiusMin ("Radius Min", Float) = 0.2
		_SonarScale ("Sonar Scale", Float) = 1
		_SonarColor ("Sonar Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		float3 _PlayerPos;
		float _RadiusMax;
		float _RadiusMin;
		float _SonarScale;
		fixed4 _SonarColor;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			float dist = distance(_PlayerPos, IN.worldPos);
			_RadiusMax *= _SonarScale;
			_RadiusMin *= _SonarScale;
			if(dist < _RadiusMax && dist > _RadiusMin)
			{
				o.Albedo = _SonarColor.rgb;
			}
			else
			{
				o.Albedo = c.rgb;
			}

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
