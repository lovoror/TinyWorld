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
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			sampler2D _WindField;
			uniform float4 _Color, _SubColor, _Wind;
			float _DiffusePower, _Scale, _Power, _Distortion, _Thickness, _WindFactor;
			uniform float4 _LightColor0;
	
            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 posWorld : TEXCOORD3;
				float3 normal : NORMAL;
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
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				// standard
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
				float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
				float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));
				float3 color = (ambientLighting + _DiffusePower * diffuseReflection + transAlbedo) * _Color.xyz +transAlbedo;

                // apply fog
				fixed4 col = fixed4(color, 1.0);
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

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
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
