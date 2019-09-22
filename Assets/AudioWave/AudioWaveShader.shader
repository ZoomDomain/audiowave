Shader "DX11/WaveForm" {
SubShader {
Pass {

CGPROGRAM
#pragma target 5.0

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

			StructuredBuffer<float3> buf_Points;
			StructuredBuffer<float3> buf_Positions;
			StructuredBuffer<half>  buf_list;

			float offset, skipn;
			struct ps_input {
				float4 pos : SV_POSITION;
			};

			ps_input vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				float graphOriginX = 5;
				float graphOriginY = 1;
				
				ps_input o;
				
				float3 worldPos = 
				buf_Points[id]*buf_list[inst*skipn+offset] 
				+ float3( inst*0.01f - graphOriginX, graphOriginY, 0);
				
				o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
				return o;
			}

			float4 frag (ps_input i) : COLOR
			{
				return float4(0.85f,1,1.0f,1);
			}

			ENDCG

}
}

Fallback Off
}
