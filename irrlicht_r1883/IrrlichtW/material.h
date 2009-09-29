#include "main.h"

extern "C"
{
	EXPORT IntPtr Material_Create() { return new SMaterial(); }
	EXPORT void Material_GetAmbientColor(IntPtr material, M_SCOLOR color);
	EXPORT void Material_GetDiffuseColor(IntPtr material, M_SCOLOR color);
	EXPORT void Material_GetEmissiveColor(IntPtr material, M_SCOLOR color);
	EXPORT E_MATERIAL_TYPE Material_GetMaterialType(IntPtr material);
	EXPORT float Material_GetMaterialTypeParam(IntPtr material);
	EXPORT float Material_GetShininess(IntPtr material);
	EXPORT void Material_GetSpecularColor(IntPtr material, M_SCOLOR color);
	EXPORT IntPtr Material_GetTexture(IntPtr material,int numtex);
	EXPORT bool Material_GetBackfaceCulling(IntPtr material);
	EXPORT bool Material_GetFogEnable(IntPtr material);
	EXPORT bool Material_GetGouraudShading(IntPtr material);
	EXPORT bool Material_GetLighting(IntPtr material);
	EXPORT bool Material_GetNormalizeNormals(IntPtr material);
	EXPORT bool Material_GetWireframe(IntPtr material);
	EXPORT unsigned int Material_GetZBuffer(IntPtr material);
	EXPORT bool Material_GetZWriteEnable(IntPtr material);
	EXPORT void Material_SetAmbientColor(IntPtr material, M_SCOLOR color);
	EXPORT void Material_SetDiffuseColor(IntPtr material, M_SCOLOR color);
	EXPORT void Material_SetEmissiveColor(IntPtr material, M_SCOLOR color);
	EXPORT void Material_SetMaterialType(IntPtr material, E_MATERIAL_TYPE val);
	EXPORT void Material_SetMaterialTypeParam(IntPtr material, float val);
	EXPORT void Material_SetShininess(IntPtr material, float val);
	EXPORT void Material_SetSpecularColor(IntPtr material, M_SCOLOR color);
	EXPORT void Material_SetTexture(IntPtr material, int num, IntPtr text);
	EXPORT void Material_SetBackfaceCulling(IntPtr material, bool val);
	EXPORT void Material_SetFogEnable(IntPtr material, bool val);
	EXPORT void Material_SetGouraudShading(IntPtr material, bool val);
	EXPORT void Material_SetLighting(IntPtr material, bool val);
	EXPORT void Material_SetNormalizeNormals(IntPtr material, bool val);
	EXPORT void Material_SetWireframe(IntPtr material, bool val);
	EXPORT void Material_SetZBuffer(IntPtr material, unsigned int val);
	EXPORT void Material_SetZWriteEnable(IntPtr material, bool val);
	EXPORT IntPtr Material_GetMaterialLayer(IntPtr material, u32 nr);
	EXPORT void Material_SetMaterialLayer (IntPtr material, u32 nr, SMaterialLayer* layer);
	//EXPORT void Material_Release (IntPtr material);
	
	/*
	 * MaterialLayer
	 */
	//EXPORT IntPtr MaterialLayer_Create() { return new SMaterialLayer(); }	
	EXPORT bool MaterialLayer_GetAnisotropicFilter(IntPtr material);
	EXPORT void MaterialLayer_SetAnisotropicFilter(IntPtr material, bool val);	
	EXPORT bool MaterialLayer_GetBilinearFilter(IntPtr material);
	EXPORT void MaterialLayer_SetBilinearFilter(IntPtr material, bool val);	
	EXPORT bool MaterialLayer_GetTrilinearFilter(IntPtr material);
	EXPORT void MaterialLayer_SetTrilinearFilter(IntPtr material, bool val);
	EXPORT void MaterialLayer_SetTexture(IntPtr material, IntPtr texture);
	EXPORT IntPtr MaterialLayer_GetTexture(IntPtr material);
	EXPORT void MaterialLayer_GetTransform(IntPtr lyr, M_MAT4 TxT);
	EXPORT void MaterialLayer_SetTransform(IntPtr lyr, M_MAT4 TxT);	
	//EXPORT void MaterialLayer_Release(IntPtr lyr);	
	
	
}
