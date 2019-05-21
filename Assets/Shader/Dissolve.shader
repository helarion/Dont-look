
Shader "Custom/Dissolve"
{
	Properties{
		[PerRendererData] _MainTex("Main texture", 2D) = "white" {}
		_offsetX("OffsetX",Float) = 0.0
		_offsetY("OffsetY",Float) = 0.0
		_octaves("Octaves",Int) = 7
		_lacunarity("Lacunarity", Range(1.0 , 5.0)) = 2
		_gain("Gain", Range(0.0 , 1.0)) = 0.5
		_Amount("Amount", Range(-5 , 5)) = 0.0
		_amplitude("Amplitude", Range(0.0 , 5.0)) = 1.5
		_frequency("Frequency", Range(0.0 , 6.0)) = 2.0
		_power("Power", Range(0.1 , 5.0)) = 1.0
		_scale("Scale", Float) = 1.0
		_color("Color", Color) = (1.0,1.0,1.0,1.0)
		[Toggle] _monochromatic("Monochromatic", Float) = 0
		_range("Monochromatic Range", Range(0.0 , 1.0)) = 0.5
	}

	SubShader{

		Tags { "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			float _octaves, _lacunarity, _gain, _Amount, _amplitude, _frequency, _offsetX, _offsetY, _power, _scale, _monochromatic, _range;
			float4 _color;

			float fbm(float2 p)
			{
				p = p * _scale + float2(_offsetX, _offsetY);
				for (int i = 0; i < _octaves; i++)
				{
					float2 i = floor(p * _frequency);
					float2 f = frac(p * _frequency);
					float2 t = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
					float2 a = i + float2(0.0, 0.0);
					float2 b = i + float2(1.0, 0.0);
					float2 c = i + float2(0.0, 1.0);
					float2 d = i + float2(1.0, 1.0);
					a = -1.0 + 2.0 * frac(sin(float2(dot(a, float2(127.1, 311.7)), dot(a, float2(269.5, 183.3)))) * 43758.5453123);
					b = -1.0 + 2.0 * frac(sin(float2(dot(b, float2(127.1, 311.7)), dot(b, float2(269.5, 183.3)))) * 43758.5453123);
					c = -1.0 + 2.0 * frac(sin(float2(dot(c, float2(127.1, 311.7)), dot(c, float2(269.5, 183.3)))) * 43758.5453123);
					d = -1.0 + 2.0 * frac(sin(float2(dot(d, float2(127.1, 311.7)), dot(d, float2(269.5, 183.3)))) * 43758.5453123);
					float A = dot(a, f - float2(0.0, 0.0));
					float B = dot(b, f - float2(1.0, 0.0));
					float C = dot(c, f - float2(0.0, 1.0));
					float D = dot(d, f - float2(1.0, 1.0));
					float noise = (lerp(lerp(A, B, t.x), lerp(C, D, t.x), t.y));
					_Amount += _amplitude * noise;
					_frequency *= _lacunarity;
					_amplitude *= _gain;
				}
				_Amount = clamp(_Amount, 0, 1.0);
				return pow(_Amount * 0.5 + 0.5, _power);
			}

			sampler2D _DissolveTex;

			fixed4 frag(v2f i) : SV_Target {
				float4 c = tex2D(_MainTex, i.uv);
				float2 uv = i.uv.xy;
				float val = fbm(uv);
				
				c.a = val * c.a;

				return c;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
