#include "main.h"
extern "C"
{
	EXPORT IntPtr Event_Create() { return new SEvent(); }
    EXPORT EEVENT_TYPE Event_GetType(IntPtr event);
    EXPORT void Event_SetType (IntPtr event, int evtype);
    EXPORT EMOUSE_INPUT_EVENT Event_GetMouseInputEvent(IntPtr event);
    EXPORT EGUI_EVENT_TYPE Event_GetGUIEventType(IntPtr event);
    EXPORT float Event_GetMouseWheelDelta(IntPtr event);
    EXPORT void Event_GetMousePosition(IntPtr event, M_POS2DS pos);
    EXPORT EKEY_CODE Event_GetKey(IntPtr event);
    EXPORT bool Event_GetKeyPressedDown(IntPtr event);
    EXPORT bool Event_GetKeyShift(IntPtr event);
    EXPORT bool Event_GetKeyControl(IntPtr event);
    EXPORT char Event_GetKeyChar(IntPtr event);
    EXPORT void Event_GetLogString(IntPtr event, char* str);
    EXPORT IntPtr Event_GetCaller(IntPtr event);
    
    EXPORT int Event_GetUserDataI (IntPtr event, unsigned int num);
    EXPORT float Event_GetUserDataF (IntPtr event);
    EXPORT void Event_SetUserDataI (IntPtr event, char num, int data);
    EXPORT void Event_SetUserDataF (IntPtr event, float data);
	EXPORT void Event_Release(IntPtr event);
}
