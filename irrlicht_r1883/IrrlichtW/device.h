#include "main.h"

extern "C"
{
    typedef bool (STDCALL EVENTCALLBACK)(const IntPtr);
 
    EXPORT IntPtr CreateDevice(E_DRIVER_TYPE type, M_DIM2DS dim, int bits, bool full, bool stencil, bool vsync, bool antialias);
    EXPORT IntPtr CreateDeviceA(E_DRIVER_TYPE type, M_DIM2DS dim, int bits, bool full, bool stencil, bool vsync, bool antialias, IntPtr handle);
    EXPORT void Device_SetWindowCaption(IntPtr device, M_STRING caption);
    EXPORT IntPtr Device_GetSceneManager(IntPtr device);
    EXPORT IntPtr Device_GetVideoDriver(IntPtr device);
    EXPORT IntPtr Device_GetGUIEnvironment(IntPtr device);
    EXPORT bool Device_Run(IntPtr device);
    EXPORT void Device_Drop(IntPtr device);
    EXPORT void Device_Close(IntPtr device);
    EXPORT IntPtr Device_GetCursorControl(IntPtr device);
    EXPORT IntPtr Device_GetFileSystem(IntPtr device);
    EXPORT IntPtr Device_GetTimer(IntPtr device);
    EXPORT IntPtr Device_GetVideoModeList(IntPtr device);
    EXPORT IntPtr Device_GetLogger(IntPtr device);
    EXPORT M_STRING Device_GetVersion(IntPtr device);
    EXPORT bool Device_IsWindowActive(IntPtr device);
    EXPORT void Device_SetResizeable(IntPtr device, bool resizeable);
    EXPORT void Device_SetCallback(IntPtr device, EVENTCALLBACK);

    EXPORT int VideoModeList_GetDesktopDepth(IntPtr videomodelist);
    EXPORT void VideoModeList_GetDesktopResolution(IntPtr videomodelist, M_DIM2DS res);
    EXPORT int VideoModeList_GetVideoModeCount(IntPtr videomodelist);
    EXPORT int VideoModeList_GetVideoModeDepth(IntPtr videomodelist, int mode);
    EXPORT void VideoModeList_GetVideoModeResolution(IntPtr videomodelist, int mode, M_DIM2DS res);

    EXPORT void FileSystem_AddFolderFileArchive(IntPtr system, M_STRING folder, bool ignoreCase, bool ignorePaths);
    EXPORT void FileSystem_AddZipFileArchive(IntPtr system,M_STRING filename, bool ignoreCase, bool ignorePaths);
    EXPORT bool FileSystem_ChangeWorkingDirectory(IntPtr system, M_STRING workingdirectory);
    EXPORT IntPtr FileSystem_GetFileList(IntPtr system);
    EXPORT bool FileSystem_ExistsFile(IntPtr system, M_STRING filename);
    EXPORT M_STRING FileSystem_GetWorkingDirectory(IntPtr system);
	EXPORT IntPtr FileSystem_CreateAndWriteFile(IntPtr system, M_STRING filename, bool append);
	EXPORT IntPtr FileSystem_CreateMemoryReadFile(IntPtr system, IntPtr memory, int len, M_STRING fileName, bool deleteMemoryWhenDropped); // note: returned IntPtr will be opaque to caller since we don't export IReadFile interface

    EXPORT void CursorControl_GetPosition(IntPtr cc, M_POS2DS pos);
    EXPORT void CursorControl_GetRelativePosition(IntPtr cc, M_POS2DF pos);
    EXPORT bool CursorControl_IsVisible(IntPtr cc);
    EXPORT void CursorControl_SetPosition(IntPtr cc, int X, int Y);
    EXPORT void CursorControl_SetPositionA(IntPtr cc, float X, float Y);
    EXPORT void CursorControl_SetVisible(IntPtr cc, bool visible);

    EXPORT unsigned int Timer_GetRealTime(IntPtr timer);
    EXPORT float Timer_GetSpeed(IntPtr timer);
    EXPORT unsigned int Timer_GetTime(IntPtr timer);
    EXPORT bool Timer_IsStopped(IntPtr timer);
    EXPORT void Timer_SetSpeed(IntPtr timer, float speed);
    EXPORT void Timer_SetTime(IntPtr timer, unsigned int time);
    EXPORT void Timer_Start(IntPtr timer);
    EXPORT void Timer_Stop(IntPtr timer);
    EXPORT void Timer_Tick(IntPtr timer);

    EXPORT int FileList_GetFileCount(IntPtr list);
    EXPORT M_STRING FileList_GetFileName(IntPtr list, int index);
    EXPORT M_STRING FileList_GetFullFileName(IntPtr list, int index);
    EXPORT bool FileList_IsDirectory(IntPtr list, int index);

    EXPORT ELOG_LEVEL Logger_GetLogLevel(IntPtr logger);
    EXPORT void Logger_Log(IntPtr logger, M_STRING text, ELOG_LEVEL lev);
    EXPORT void Logger_LogA(IntPtr logger, M_STRING text, M_STRING hint, ELOG_LEVEL lev);
    EXPORT void Logger_SetLogLevel(IntPtr logger, ELOG_LEVEL level);
}
