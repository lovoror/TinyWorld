// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Perso/Vegetation"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_SubColor("Subsurface color", Color) = (1.0, 1.0, 1.0, 1.0)
		_WindFactor("Wind factor", Range(0, 1)) = 1
		_WindField("Wind force field", 2D) = "eee" {}
		_Tilling("Wind tilling", Int) = 3
	}

	SubShader
	{
		Pass
		{
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

		// compile shader into multiple variants, with and without shadows
		// (we don't care about any lightmaps yet, so skip these variants)
		#pragma multi_compile_fog multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
		// shadow helper functions and macros
		#include "AutoLight.cginc"

		sampler2D _WindField;
		uniform float4 _Color, _SubColor, _Wind;
		float _WindFactor;
		int _Tilling;

		struct v2f
		{
			SHADOW_COORDS(0)
			fixed3 diff : COLOR0;
			fixed3 ambient : COLOR1;
			fixed3 trans : COLOR2;
			float4 pos : SV_POSITION;
			UNITY_FOG_COORDS(1)
		};

		float3 Wind(float u, float v, float y)
		{
			float tilling = 1.0 / 128 * _Tilling;
			float3 wind = 2 * tex2Dlod(_WindField, tilling * float4(u, v, 0, 0)) - float3(1, 1, 1);
			float3 dp = y * mul(unity_WorldToObject, wind);
			return _WindFactor * float4(dp, 0);
		}

		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex + Wind(v.vertex.x, v.vertex.z, v.vertex.y));
			
			half3 worldNormal = UnityObjectToWorldNormal(v.normal);
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			o.diff = nl * _LightColor0.rgb;
			o.ambient = ShadeSH9(half4(worldNormal,1));

			// transluminecence
			half3 transLightDir = normalize(_WorldSpaceLightPos0.xyz);
			half3 viewDirection = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
			nl = max(0, -dot(worldNormal, _WorldSpaceLightPos0.xyz)) * pow(abs(dot(viewDirection, transLightDir)), 3);
			o.trans = nl * _LightColor0.rgb * _SubColor.rgb;

			// compute shadows data
			TRANSFER_SHADOW(o);
			UNITY_TRANSFER_FOG(o, o.pos);
			return o;
		}


		fixed4 frag(v2f i) : SV_Target
		{
			// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
			fixed shadow = SHADOW_ATTENUATION(i);
			// darken light's illumination with shadow, keep ambient intact
			fixed3 lighting = i.diff * shadow + i.ambient;
			fixed3 transAlbedo = i.trans;

			fixed4 col = fixed4(_Color.rgb * lighting + transAlbedo, 1);
			UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
		}
		ENDCG
	}

		// shadow casting support
		Pass
		{
			Name "CastShadow"
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			sampler2D _WindField;
			uniform float4 _Wind;
			float  _WindFactor;
			int _Tilling;

			float3 Wind(float u, float v, float y)
			{
				float tilling = 1.0 / 128 * _Tilling;
				float3 wind = 2 * tex2Dlod(_WindField, tilling * float4(u, v, 0, 0)) - float3(1, 1, 1);
				float3 dp = y * mul(unity_WorldToObject, wind);
				return _WindFactor * float4(dp, 0);
			}

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 vec : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex + Wind(v.vertex.x, v.vertex.z, v.vertex.y));
				o.vec = mul(unity_ObjectToWorld, v.vertex + Wind(v.vertex.x, v.vertex.z, v.vertex.y)).xyz - _LightPositionRange.xyz;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
