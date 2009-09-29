#include "cd3d9driver.h"

CD3D9Driver* GetCD3D9DriverFromIntPtr(IntPtr videodriver)
{
    //return dynamic_cast<CD3D9Driver*>((IVideoDriver*)videodriver);
    return (CD3D9Driver*)((IVideoDriver*)videodriver);
}

u32 CD3D9Driver_GetAdapterVendorId(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : (u32)driver->getAdapterIdent().VendorId;
}

u32 CD3D9Driver_GetAdapterDeviceId(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : (u32)driver->getAdapterIdent().DeviceId;
}

u32 CD3D9Driver_GetAdapterSubSysId(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : (u32)driver->getAdapterIdent().SubSysId;
}

u32 CD3D9Driver_GetAdapterRevision(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : (u32)driver->getAdapterIdent().Revision;
}

int CD3D9Driver_GetAdapterMaxTextureWidth(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : driver->getAdapterCaps().MaxTextureWidth;
}

int CD3D9Driver_GetAdapterMaxTextureHeight(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : driver->getAdapterCaps().MaxTextureHeight;
}

int CD3D9Driver_GetAdapterMaxActiveLights(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : driver->getAdapterCaps().MaxActiveLights;
}

int CD3D9Driver_GetAdapterVertexShaderVersion(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : driver->getAdapterCaps().VertexShaderVersion;
}

int CD3D9Driver_GetAdapterPixelShaderVersion(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? 0 : driver->getAdapterCaps().PixelShaderVersion;
}

IntPtr CD3D9Driver_GetD3DDevice9(IntPtr videodriver)
{
	CD3D9Driver* driver = GetCD3D9DriverFromIntPtr(videodriver);
	return driver == 0 ? NULL : (void*)driver->getExposedVideoData().D3D9.D3DDev9;
}