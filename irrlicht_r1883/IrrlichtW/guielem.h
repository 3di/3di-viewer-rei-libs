#include "main.h"

extern "C"
{
	EXPORT void GuiElem_AddChild(IntPtr elem, IntPtr child);
	EXPORT bool GuiElem_BringToFront(IntPtr elem, IntPtr element);
	EXPORT void GuiElem_Draw(IntPtr elem);
	EXPORT void GuiElem_GetAbsolutePosition(IntPtr elem, M_RECT pos);
	EXPORT void GuiElem_GetChildren(IntPtr elem, IntPtr *list);
	EXPORT unsigned int GuiElem_GetChildrenCount(IntPtr elem); //Only used by GUIElement.Children on C#
	EXPORT IntPtr GuiElem_GetElementFromID(IntPtr elem, int id, bool searchchildren);
	EXPORT IntPtr GuiElem_GetElementFromPoint(IntPtr elem, M_POS2DS point);
	EXPORT int GuiElem_GetID(IntPtr elem);
	EXPORT IntPtr GuiElem_GetParent(IntPtr elem);
	EXPORT void GuiElem_GetRelativePosition(IntPtr elem, M_RECT pos);
	EXPORT IntPtr GuiElem_GetText(IntPtr elem);
	EXPORT EGUI_ELEMENT_TYPE GuiElem_GetType(IntPtr elem);
	EXPORT bool GuiElem_IsEnabled(IntPtr elem);
	EXPORT bool GuiElem_IsVisible(IntPtr elem);
	EXPORT void GuiElem_Move(IntPtr elem, M_POS2DS absolutemovement);
	EXPORT bool GuiElem_OnEvent(IntPtr elem, IntPtr ev);
	EXPORT void GuiElem_Remove(IntPtr elem);
	EXPORT void GuiElem_RemoveChild(IntPtr elem, IntPtr child);
	EXPORT void GuiElem_SetEnabled(IntPtr elem, bool enabled);	
	EXPORT M_STRING GuiElem_GetToolTipText(IntPtr elem);
	EXPORT void GuiElem_SetToolTipText( IntPtr elem, M_STRING text );
	EXPORT void GuiElem_SetID(IntPtr elem, int id);
	EXPORT void GuiElem_SetRelativePosition(IntPtr elem, M_RECT pos);
	EXPORT void GuiElem_SetText(IntPtr elem, M_STRING text);
	EXPORT void GuiElem_SetVisible(IntPtr elem, bool visible);
	EXPORT void GuiElem_UpdateAbsolutePosition(IntPtr elem);
	EXPORT void GuiElem_OnPostRender (IntPtr elem, u32 timeMs);
	EXPORT void GuiElem_SetAlignment (IntPtr elem, int *align);
	EXPORT void GuiElem_SetMaxSize (IntPtr elem, M_DIM2DS size);
	EXPORT void GuiElem_SetMinSize (IntPtr elem, M_DIM2DS size);
	EXPORT void GuiElem_SetNotClipped (IntPtr elem, bool noClip);
	EXPORT bool GuiElem_GetNotClipped (IntPtr elem);
}
