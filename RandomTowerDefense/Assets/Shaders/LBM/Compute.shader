﻿Shader "LBM/Compute" {

	Properties {
		_Diffuse1 ("Diffuse Texture 1", 2D) = "" {}
		_Diffuse2 ("Diffuse Texture 2", 2D) = "" {}
		_Diffuse3 ("Diffuse Texture 3", 2D) = "" {}
		_Dx ("dx", float) = 0.001
		_Dy ("dy", float) = 0.001
		_T ("time", float) = 0.0
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	uniform sampler2D _Diffuse1;
	uniform sampler2D _Diffuse2;
	uniform sampler2D _Diffuse3;
	uniform float _Dx;
	uniform float _Dy;
	uniform float _T;

	float4 compute1 (v2f_img IN) : SV_Target {
	
		#include "Includes/compute.cginc"

		float feq1 = t2 * density * (1. + ux / c_squ + 0.5 * (ux / c_squ) * (ux / c_squ)- usqu / (2. * c_squ));
		float feq3 = t2 * density * (1. + uy / c_squ + 0.5 * (uy / c_squ) * (uy / c_squ) - usqu / (2. * c_squ));
		float feq2 = t3 * density * (1. + uc2 / c_squ + 0.5 * (uc2 / c_squ) * (uc2 / c_squ)- usqu / (2. * c_squ));
		
		float ff1 = clamp(feq1, 0.0, 1.0);
		float ff2 = clamp(feq2, 0.0, 1.0);
		float ff3 = clamp(feq3, 0.0, 1.0);
		
		return float4(ff1, ff2, ff3, 1.0);
	}
	
	float4 compute2(v2f_img IN) : SV_Target {
	
		#include "Includes/compute.cginc"
		
		float feq5 = t2 * density * (1. - ux / c_squ + 0.5 * (ux / c_squ) * (ux / c_squ) - usqu / (2. * c_squ));
		float feq4 = t3 * density * (1. + uc4 / c_squ + 0.5 * (uc4 / c_squ) * (uc4 / c_squ) - usqu / (2. * c_squ));
		float feq6 = t3 * density * (1. + uc6 / c_squ + 0.5 * (uc6 / c_squ) * (uc6 / c_squ) - usqu / (2. * c_squ));
		
		float ff4 = clamp(feq4, 0.0, 1.0);
		float ff5 = clamp(feq5, 0.0, 1.0);
		float ff6 = clamp(feq6, 0.0, 1.0);

		return float4(ff4, ff5, ff6, 1.);
	}
	
	float4 compute3(v2f_img IN) : SV_Target {
	
		#include "Includes/Compute.cginc"

		float feq7 = t2 * density * (1. - uy / c_squ + 0.5 * (uy / c_squ) * (uy / c_squ) - usqu / (2. * c_squ));
		float feq8 = t3 * density * (1. + uc8 / c_squ + 0.5 * (uc8 / c_squ) * (uc8 / c_squ) - usqu / (2. * c_squ));
		float feq9 = t1 * density * (1. - usqu / (2. * c_squ));

		float ff7 = clamp(feq7, 0.0, 1.0);
		float ff8 = clamp(feq8, 0.0, 1.0);
		float ff9 = clamp(feq9, 0.0, 1.0);
		
		return float4(ff7, ff8, ff9, 1.);
	}
	
	
	ENDCG
	
	SubShader {
	
		Pass {
			Fog { Mode Off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment compute1
			ENDCG
		}
		
		Pass {
			Fog { Mode Off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment compute2
			ENDCG
		}
		
		Pass {
			Fog { Mode Off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment compute3
			ENDCG
		}
		
	} 
	
	FallBack "Diffuse"
}
