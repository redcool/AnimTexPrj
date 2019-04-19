Shader "Unlit/AnimTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AnimTex("Anim Tex",2d) = ""{}
		_AnimSampleRate("Anim Sample Rate",float) = 30
		_StartFrame("Start Frame",float) = 0
		_EndFrame("End Frame",float) = 1
		_Loop("Loop[0:Loop,1:Clamp]",range(0,1)) = 1
		_PlayTime("Play Time",float) = 0
		_OffsetPlayTime("Offset Play Time",float) = 0

		_NextStartFrame("Next Anim Start Frame",float) = 0
		_NextEndFrame("Next Anim End Frame",float) = 0
		_CrossLerp("Cross Lerp",range(0,1)) = 0
    }

CGINCLUDE
		sampler2D _AnimTex;
		half4 _AnimTex_TexelSize;

		struct AnimInfo {
			uint frameRate;
			uint startFrame;
			uint endFrame;
			half loop;
			half playTime;
			uint offsetPlayTime;
		};

		inline half GetY(AnimInfo info) {
			// length = fps/sampleRatio
			half totalLen = _AnimTex_TexelSize.w / info.frameRate;
			half start = info.startFrame / _AnimTex_TexelSize.w;
			half end = info.endFrame / _AnimTex_TexelSize.w;
			half len = end - start;
			half y = start + (info.playTime + info.offsetPlayTime) / totalLen % len;
			y = lerp(y, end, info.loop);
			return y;
		}

		inline half4 GetAnimPos(uint vertexId, AnimInfo info) {
			half y = GetY(info);
			half x = (vertexId + 0.5) * _AnimTex_TexelSize.x;

			half4 animPos = tex2Dlod(_AnimTex, half4(x, y, 0, 0));
			return animPos;
		}
ENDCG
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
			NAME "AnimTexture"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			// enable gpu instancing
			#pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                half2 uv : TEXCOORD0;
				uint vertexId:SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                half4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            half4 _MainTex_ST;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(uint, _StartFrame)
				UNITY_DEFINE_INSTANCED_PROP(uint, _EndFrame)
				UNITY_DEFINE_INSTANCED_PROP(uint, _AnimSampleRate)
				UNITY_DEFINE_INSTANCED_PROP(half, _Loop)
				UNITY_DEFINE_INSTANCED_PROP(uint, _NextStartFrame)
				UNITY_DEFINE_INSTANCED_PROP(uint, _NextEndFrame)
				UNITY_DEFINE_INSTANCED_PROP(half, _CrossLerp)
				UNITY_DEFINE_INSTANCED_PROP(half, _PlayTime)
				UNITY_DEFINE_INSTANCED_PROP(half, _OffsetPlayTime)
			UNITY_INSTANCING_BUFFER_END(Props)

			half4 GetBlendAnimPos(appdata v) {
				AnimInfo info ;
				UNITY_INITIALIZE_OUTPUT(AnimInfo,info);

				info.frameRate = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimSampleRate);
				info.startFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _StartFrame);
				info.endFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _EndFrame);
				info.loop = UNITY_ACCESS_INSTANCED_PROP(Props, _Loop);
				info.playTime = UNITY_ACCESS_INSTANCED_PROP(Props, _PlayTime);
				info.offsetPlayTime = UNITY_ACCESS_INSTANCED_PROP(Props,_OffsetPlayTime);
				half crossLerp = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossLerp);
				half4 curPos = GetAnimPos(v.vertexId, info);

				info.startFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _NextStartFrame);
				info.endFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _NextEndFrame);
				half4 nextPos = GetAnimPos(v.vertexId, info);

				return lerp(curPos, nextPos, crossLerp);
			}
			

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				half4 pos = GetBlendAnimPos(v);

				o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }

		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			// enable gpu instancing
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata
			{
				half2 uv : TEXCOORD0;
				uint vertexId:SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			half4 _MainTex_ST;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(uint, _StartFrame)
				UNITY_DEFINE_INSTANCED_PROP(uint, _EndFrame)
				UNITY_DEFINE_INSTANCED_PROP(uint, _AnimSampleRate)
				UNITY_DEFINE_INSTANCED_PROP(half, _Loop)
				UNITY_DEFINE_INSTANCED_PROP(uint, _NextStartFrame)
				UNITY_DEFINE_INSTANCED_PROP(uint, _NextEndFrame)
				UNITY_DEFINE_INSTANCED_PROP(half, _CrossLerp)
				UNITY_DEFINE_INSTANCED_PROP(half, _PlayTime)
				UNITY_DEFINE_INSTANCED_PROP(half, _OffsetPlayTime)
			UNITY_INSTANCING_BUFFER_END(Props)

			half4 GetBlendAnimPos(appdata v) {
				AnimInfo info;
				UNITY_INITIALIZE_OUTPUT(AnimInfo,info);

				info.frameRate = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimSampleRate);
				info.startFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _StartFrame);
				info.endFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _EndFrame);
				info.loop = UNITY_ACCESS_INSTANCED_PROP(Props, _Loop);
				info.playTime = UNITY_ACCESS_INSTANCED_PROP(Props, _PlayTime);
				info.offsetPlayTime = UNITY_ACCESS_INSTANCED_PROP(Props,_OffsetPlayTime);
				half crossLerp = UNITY_ACCESS_INSTANCED_PROP(Props, _CrossLerp);
				half4 curPos = GetAnimPos(v.vertexId, info);

				info.startFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _NextStartFrame);
				info.endFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _NextEndFrame);
				half4 nextPos = GetAnimPos(v.vertexId, info);

				return lerp(curPos, nextPos, crossLerp);
			}


			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				half4 pos = GetBlendAnimPos(v);

				o.vertex = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
    }
	
}
