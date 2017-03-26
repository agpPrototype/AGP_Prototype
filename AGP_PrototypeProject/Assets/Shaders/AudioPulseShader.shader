// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ScanSphereShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
			};

			// built in members
			sampler2D _MainTex;
			float4 _MainTex_ST;
			// float3 _WorldSpaceCameraPos;

			// added members
			float4 _Color;
			float3 _Origin;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex); //v.vertex; //
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = mul((float4x4)unity_ObjectToWorld, v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//float3 pos = float3(i.vertex.x, i.vertex.y, i.vertex.z);
				//float3 vecOriginToCam = normalize(_WorldSpaceCameraPos - pos);
				//float dotProd = dot(vecOriginToCam, i.worldNormal);
				//fixed4 col = (1,0,0,1);
				//if (dotProd > .1f)
				//{
				fixed4 col = _Color;
				//}
				return col;
			}
			ENDCG
		}
	}
}
