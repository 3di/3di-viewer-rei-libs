#include "main.h"
#include "../Irrlicht SDK/source/Irrlicht/CD3D9Driver.h"

extern "C"
{
	EXPORT u32 CD3D9Driver_GetAdapterVendorId(IntPtr videodriver);
	EXPORT u32 CD3D9Driver_GetAdapterDeviceId(IntPtr videodriver);
	EXPORT u32 CD3D9Driver_GetAdapterSubSysId(IntPtr videodriver);
	EXPORT u32 CD3D9Driver_GetAdapterRevision(IntPtr videodriver);
	EXPORT int CD3D9Driver_GetAdapterMaxTextureWidth(IntPtr videodriver);
	EXPORT int CD3D9Driver_GetAdapterMaxTextureHeight(IntPtr videodriver);
	EXPORT int CD3D9Driver_GetAdapterMaxActiveLights(IntPtr videodriver);
	EXPORT int CD3D9Driver_GetAdapterVertexShaderVersion(IntPtr videodriver);
	EXPORT int CD3D9Driver_GetAdapterPixelShaderVersion(IntPtr videodriver);
	EXPORT IntPtr CD3D9Driver_GetD3DDevice9(IntPtr videodriver);
};
