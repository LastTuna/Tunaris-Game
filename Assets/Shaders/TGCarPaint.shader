// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

//TODO FEATURE: ADD REFLECTION PROBE SUPPORT. i read in a thread, that reflection probe support is possible
//on legacy pipeline.. i will look into that later not really interested in writing even one line of hlsl......
//THREAD: https://forum.unity.com/threads/reflection-probes-in-custom-shader.322562/

Shader "TG/TGCarPaint"
{
	//this shit is for the inspector stuff
	Properties
	{
		_MainTex("Base (RGB) AlphaTest (A)", 2D) = "white" {}

		_IllumTex("Illum (RGB)", 2D) = "black" {}
		_IllumMult("IllumStrength", Range(0,1)) = 0

		_DirtTex("Dirt (RGBA)", 2D) = "black" {}
		_Dirtiness("Dirtiness", Range(0,1)) = 0

		_Cube("Reflection Cubemap", Cube) = "_Skybox" {}
		_Metallic("Glossy(R) Metallic(G) (RGB)", 2D) = "red" {}
	}

		Category
	{
		//add shader "tags" so the compoter knows things like draw call order, and transparency property
		//queue: automatically sets the "render queue" (the last setting in the UI) to be alpha test by default.
		//render type: required for the computer to know if its transparent and what type of transparency.
		Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
		LOD 150
		//LOD is used by unity internally somewhere doesnt matter just keep it there
		SubShader
		{
		// First pass does ONLY reflection cubemap
		Pass
		{
			Name "BASE"
			Tags{ "LightMode" = "Always" }

			CGPROGRAM
			//reference necessary sources.
			//we using vertex lighting so ref vertex, fragment (surface)
			//and the shader also featured fog support ig dont touch it if it aint broke....
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			//make a struct with some parameters from foreign origins... i dont really get it either...
			struct input {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 I : TEXCOORD1;
			UNITY_FOG_COORDS(2)
			UNITY_VERTEX_OUTPUT_STEREO //dunno what this does.. some VR thing?
		};


		//this is apparently required because unity. features xy, for scale. zw for UV offset
		uniform float4 _MainTex_ST;

		//this is where you do vertex operations
		input vert(appdata_tan v)
		{
			input o;//make an instance of our data structure
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);//this is a VR property. it indexes all viewports
			//and you can do unique operations with this
			o.pos = UnityObjectToClipPos(v.vertex);//transform point from object space to cameras clip space in homoerotic cordinates 
			o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);//does something to the uv cordinate

			// calculate world space reflection vector
			//i dont understand, nor care, It Just Werks....
			float3 viewDir = WorldSpaceViewDir(v.vertex);
			float3 worldN = UnityObjectToWorldNormal(v.normal);
			o.I = reflect(-viewDir, worldN);

			UNITY_TRANSFER_FOG(o,o.pos);//some fog macro
			return o;
		}

		uniform sampler2D _MainTex;
		uniform sampler2D _Metallic;
		uniform samplerCUBE _Cube;
		uniform sampler2D _DirtTex;
		float _Dirtiness;

		fixed4 frag(input i) : SV_Target
		{
			fixed4 texcol = tex2D(_MainTex, i.uv);//this is where you call sampling
			fixed4 multimapcol = tex2D(_Metallic, i.uv);
			fixed4 dirtcol = tex2D(_DirtTex, i.uv);
			fixed4 reflcol = texCUBE(_Cube, i.I);//sample the cubemap
			fixed4 col;
			if (multimapcol.g > 0) {
				//metallic reflection. tints clearcoat with source color to add metallic effect.
				col = reflcol * texcol * (1 - dirtcol.a * _Dirtiness);
			} else {
				//darken the reflection so it isnt turbochrome. basic clearcoat.
				//can prob adjust that factor once i try this out further.. keep it at 0.6 for now
				col = reflcol * (0.6 * multimapcol.r) * (1 - dirtcol.a * _Dirtiness);
			}
			UNITY_APPLY_FOG(i.fogCoord, col);
			clip(texcol.a - 0.5);//ALPHA TEST! DISCARD PIXEL IF TRANSPARENCY IS UNDER 0.5
			return col;
		}

		ENDCG

	}
		//Base color call (Vertex Lit)
		Pass
		{
			Tags{ "LightMode" = "Vertex" }
			Blend One One
			ZWrite Off
			Lighting On

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct input
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				fixed4 diff : COLOR0;//vertexLit illum. a multiply layer.
				float4 pos : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float4 _MainTex_ST;
			uniform fixed4 _ReflectColor;

			input vert(appdata_base v)
			{
				input o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				float4 lighting = float4(ShadeVertexLightsFull(v.vertex, v.normal, 8, false),_ReflectColor.w);
				o.diff = lighting;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			uniform sampler2D _MainTex;
			uniform sampler2D _IllumTex;
			uniform sampler2D _DirtTex;
			float _IllumMult;
			float _Dirtiness;

			fixed4 frag(input i) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				fixed4 illumcol = tex2D(_IllumTex, i.uv);
				fixed4 dirtcol = tex2D(_DirtTex, i.uv);

				fixed4 output;
				output.rgb = texcol.rgb + ((dirtcol.rgb - texcol.rgb) * dirtcol.a * _Dirtiness);
				output.rgb *= i.diff.rgb;//add vertexLit
				output.a = texcol.a;//copy alpha from main texture...
				output.rgb += (illumcol.rbg * _IllumMult);//add illum
				//c.rgb = dirtcol.rgb;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, output, fixed4(0,0,0,0)); // fog towards black due to our blend mode
				
				clip(texcol.a - 0.5);//ALPHA TEST! DISCARD PIXEL IF TRANSPARENCY IS UNDER 0.5
				return output;
			}

			ENDCG
		}

		}
	}

		FallBack "Legacy Shaders/VertexLit"
}