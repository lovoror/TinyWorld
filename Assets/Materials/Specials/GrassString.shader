// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'unity_World2Shadow' with 'unity_WorldToShadow'

Shader "Perso/GrassString"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_DiffusePower("Diffuse factor", Range(0, 1)) = 0.5

		_Thickness("Subsurface thickness", Range(0, 1)) = 0.1
		_SubColor("Subsurface color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Power("Subsurface Power", Float) = 1.0
		_Distortion("Subsurface Distortion", Float) = 0.0
		_Scale("Subsurface Scale", Float) = 0.5

		_WindFactor("Wind factor", Range(0, 1)) = 1
		_WindField("Wind force field", 2D) = "eee" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Lighting On
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fog multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "AutoLight.cginc"

			sampler2D _WindField;
			uniform float4 _Color, _SubColor, _Wind;
			float _DiffusePower, _Scale, _Power, _Distortion, _Thickness, _WindFactor;
	
            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float3 normal : NORMAL;
				LIGHTING_COORDS(2, 3)
				UNITY_FOG_COORDS(1)
				SHADOW_COORDS(4)
            };

            v2f vert (appdata_base v)
            {
                v2f o;
				float3 wind = 2 * tex2Dlod(_WindField, 1.0 / 128 * float4(v.vertex.x, v.vertex.z, 0, 0)) - float3(1, 1, 1);
				float3 dp = v.vertex.y * mul(unity_WorldToObject, wind);
				o.vertex = UnityObjectToClipPos(v.vertex + _WindFactor * float4(dp, 0));
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				UNITY_TRANSFER_FOG(o, o.vertex);
				TRANSFER_SHADOW(o)
				//TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
				// standard
				fixed atten = LIGHT_ATTENUATION(i);
				fixed shadow = SHADOW_ATTENUATION(i);
				float3 normalDirection = normalize(i.normal);
				float3 viewDirection = normalize(_WorldSpaceCameraPos - i.posWorld.xyz);
				float3 lightDirection = _WorldSpaceLightPos0.xyz - i.posWorld.xyz * _WorldSpaceLightPos0.w;
				float3 vert2LightSource = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
				float attenuation = lerp(1.0, 1.0 / length(vert2LightSource), _WorldSpaceLightPos0.w);

				// transluminecent
				half3 transLightDir = normalize(lightDirection + normalDirection);
				float transDot = pow(clamp(dot(viewDirection, -transLightDir) + _Distortion, 0, 1), _Power) * _Scale;
				fixed3 transLight = (attenuation * 2) * (transDot)* _Thickness * _SubColor.rgb;
				fixed3 transAlbedo = _Color * _LightColor0.rgb * transLight;

				// phong lightning
				float3 ambientLighting = (2-_DiffusePower) * UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
				float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));
				float3 color = (ambientLighting + _DiffusePower * diffuseReflection + transAlbedo) * _Color.xyz +transAlbedo;

				//
				//fixed3 finalColor = UNITY_LIGHTMODEL_AMBIENT.xyz + _LightColor0.rgb * atten * _Color;

                // apply fog
				fixed4 col = fixed4(shadow * float3(atten, atten, atten), 1.0);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
			}
			ENDCG
		}

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

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				float3 wind = 2 * tex2Dlod(_WindField, 1.0 / 128 * float4(v.vertex.x, v.vertex.z, 0, 0)) - float3(1, 1, 1);
				float3 dp = v.vertex.y * mul(unity_WorldToObject, wind);
				o.pos = UnityObjectToClipPos(v.vertex + _WindFactor * float4(dp, 0));
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}

		/*Pass
		{
			Name "ShadowCollector"
			Tags { "LightMode" = "ShadowCollector" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcollector

			#define SHADOW_COLLECTOR_PASS
			#include "UnityCG.cginc"

			sampler2D _WindField;
			uniform float4 _Wind;
			float  _WindFactor;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				V2F_SHADOW_COLLECTOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				float3 wind = 2 * tex2Dlod(_WindField, 1.0 / 128 * float4(v.vertex.x, v.vertex.z, 0, 0)) - float3(1, 1, 1);
				float3 dp = v.vertex.y * mul(unity_WorldToObject, wind);
				float4 p = v.vertex + _WindFactor * float4(dp, 0);
				o.pos = UnityObjectToClipPos(p);
				float4 wpos = mul(unity_ObjectToWorld, p);
				o._WorldPosViewZ.xyz = wpos;
				o._WorldPosViewZ.w = -mul(UNITY_MATRIX_MV, v.vertex).z;
				o._ShadowCoord0 = mul(unity_WorldToShadow[0], wpos).xyz;
				o._ShadowCoord1 = mul(unity_WorldToShadow[1], wpos).xyz;
				o._ShadowCoord2 = mul(unity_WorldToShadow[2], wpos).xyz;
				o._ShadowCoord3 = mul(unity_WorldToShadow[3], wpos).xyz;
				return o;
			}
			float4 frag(v2f i) : COLOR
			{
				SHADOW_COLLECTOR_FRAGMENT(i)
			}
			ENDCG
		}*/
    }
}
