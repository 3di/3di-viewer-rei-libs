#include "main.h"

extern "C"
{
	EXPORT bool GUIButton_GetUseAlphaChannel(IntPtr button);
	EXPORT bool GUIButton_IsPressed(IntPtr button);
	EXPORT void GUIButton_SetImage(IntPtr button, IntPtr image, M_RECT pos);
	EXPORT void GUIButton_SetImageA(IntPtr button, IntPtr image);
	EXPORT void GUIButton_SetIsPushButton(IntPtr button, bool ispush);
	EXPORT void GUIButton_SetOverrideFont(IntPtr button, IntPtr font);
	EXPORT void GUIButton_SetPressed(IntPtr button, bool pressed);
	EXPORT void GUIButton_SetPressedImage(IntPtr button, IntPtr image, M_RECT pos);
	EXPORT void GUIButton_SetPressedImageA(IntPtr button, IntPtr image);
	EXPORT void GUIButton_SetUseAlphaChannel(IntPtr button, bool use);

	EXPORT bool GUICheckBox_IsChecked(IntPtr checkbox);
	EXPORT void GUICheckBox_SetChecked(IntPtr checkbox, bool ck);
	
	EXPORT int GUIComboBox_AddItem(IntPtr combo, M_STRING text);
	EXPORT void GUIComboBox_Clear(IntPtr combo);
	EXPORT M_STRING GUIComboBox_GetItem(IntPtr combo, int index);
	EXPORT int GUIComboBox_GetItemCount(IntPtr combo);
	EXPORT int GUIComboBox_GetSelected(IntPtr combo);
	EXPORT void GUIComboBox_SetSelected(IntPtr combo, int index);

	EXPORT int GUIContextMenu_AddItem(IntPtr menu, M_STRING text, int commandID, bool enabled, bool hasSubMenu);
	EXPORT void GUIContextMenu_AddSeparator(IntPtr menu);
	EXPORT int GUIContextMenu_GetItemCommandID(IntPtr menu, int id);
	EXPORT int GUIContextMenu_GetItemCount(IntPtr menu);
	EXPORT M_STRING GUIContextMenu_GetItemText(IntPtr menu, int index);
	EXPORT int GUIContextMenu_GetSelectedItem(IntPtr menu);
	EXPORT IntPtr GUIContextMenu_GetSubMenu(IntPtr menu, int index);
	EXPORT bool GUIContextMenu_IsItemEnabled(IntPtr menu, int id);
	EXPORT void GUIContextMenu_RemoveAllItems(IntPtr menu);
	EXPORT void GUIContextMenu_RemoveItem(IntPtr menu, int item);
	EXPORT void GUIContextMenu_SetItemCommandID(IntPtr menu, int index, int id);
	EXPORT void GUIContextMenu_SetItemEnabled(IntPtr menu, int index, bool enabled);
	EXPORT void GUIContextMenu_SetItemText(IntPtr menu, int index, M_STRING text);

	EXPORT void GUIEditBox_EnableOverrideColor(IntPtr edit, bool enabled);
	EXPORT int GUIEditBox_GetMax(IntPtr edit);
	EXPORT void GUIEditBox_SetMax(IntPtr edit, int max);
	EXPORT void GUIEditBox_SetOverrideColor(IntPtr edit, M_SCOLOR color);
	EXPORT void GUIEditBox_SetOverrideFont(IntPtr edit, IntPtr font);
	EXPORT void GUIEditBox_SetPassword(IntPtr edit, bool on_off);
	EXPORT bool GUIEditBox_GetPassword(IntPtr edit);

	EXPORT M_STRING GUIFileOpenDialog_GetFilename(IntPtr dialog);

	EXPORT void GUIFont_Draw(IntPtr font, M_STRING text, M_RECT pos, M_SCOLOR color, bool hcenter, bool vcenter, M_RECT clip);
	EXPORT int GUIFont_GetCharacterFromPos(IntPtr font, M_STRING text, int pixel_x);
	EXPORT void GUIFont_GetDimension(IntPtr font, M_STRING text, M_DIM2DS dim);

	EXPORT void GUIImage_SetImage(IntPtr image, IntPtr texture);
	EXPORT void GUIImage_SetUseAlphaChannel(IntPtr image, bool use);

	EXPORT void GUIFader_FadeIn(IntPtr fader, unsigned int time);
	EXPORT void GUIFader_FadeOut(IntPtr fader, unsigned int time);
	EXPORT void GUIFader_GetColor(IntPtr fader, M_SCOLOR color);
	EXPORT bool GUIFader_IsReady(IntPtr fader);
	EXPORT void GUIFader_SetColor(IntPtr fader, M_SCOLOR color);

	EXPORT int GUIListBox_AddItem(IntPtr listb, M_STRING text, int icon);
	EXPORT int GUIListBox_AddItemA(IntPtr listb, M_STRING text);
	EXPORT void GUIListBox_Clear(IntPtr listb);
	EXPORT int GUIListBox_GetItemCount(IntPtr listb);
	EXPORT M_STRING GUIListBox_GetListItem(IntPtr listb, int id);
	EXPORT int GUIListBox_GetSelected(IntPtr listb);
	EXPORT void GUIListBox_SetSelected(IntPtr listb, int sel);
	
	EXPORT float GUISpinBox_GetMax (IntPtr spin);
	EXPORT float GUISpinBox_GetMin(IntPtr spin);
	EXPORT float GUISpinBox_GetStepSize (IntPtr spin);
	EXPORT float GUISpinBox_GetValue (IntPtr spin);
	EXPORT IntPtr GUISpinBox_GetEditBox (IntPtr spin);
	EXPORT void GUISpinBox_SetRange (IntPtr spin, int min, int max);
	EXPORT void GUISpinBox_SetStepSize (IntPtr spin, float step);
	EXPORT void GUISpinBox_SetValue (IntPtr spin, float value);
	EXPORT void GUISpinBox_SetDecimalPlaces (IntPtr spin, int places);
	

	EXPORT IntPtr GUIMeshViewer_GetMaterial(IntPtr meshv);
	EXPORT void GUIMeshViewer_SetMaterial(IntPtr meshv, IntPtr mat);
	EXPORT void GUIMeshViewer_SetMesh(IntPtr meshv, IntPtr animatedmesh);

	EXPORT IntPtr GUIWindow_GetCloseButton(IntPtr window);
	EXPORT IntPtr GUIWindow_GetMaximizeButton(IntPtr window);
	EXPORT IntPtr GUIWindow_GetMinimizeButton(IntPtr window);

	EXPORT int GUIScrollBar_GetPos(IntPtr sb);
	EXPORT void GUIScrollBar_SetMax(IntPtr sb, int max);
	EXPORT void GUIScrollBar_SetPos(IntPtr sb, int pos);

	EXPORT int GUIProgressBar_GetPos(IntPtr sb);
	EXPORT void GUIProgressBar_SetMax(IntPtr sb, int max);
	EXPORT void GUIProgressBar_SetPos(IntPtr sb, int pos);
	EXPORT void GUIProgressBar_SetBackground(IntPtr sb, M_SCOLOR color);
	EXPORT void GUIProgressBar_SetForeground(IntPtr sb, M_SCOLOR color);

	EXPORT void GUIStaticText_EnableOverrideColor(IntPtr st, bool enabled);
	EXPORT int GUIStaticText_GetTextHeight(IntPtr st);
	EXPORT void GUIStaticText_SetOverrideColor(IntPtr st, M_SCOLOR color);
	EXPORT void GUIStaticText_SetOverrideFont(IntPtr st, IntPtr font);
	EXPORT void GUIStaticText_SetWordWrap(IntPtr st, bool enabled);

	EXPORT int GUITab_GetNumber(IntPtr tab);
	EXPORT void GUITab_SetBackgroundColor(IntPtr tab, M_SCOLOR color);
	EXPORT void GUITab_SetDrawBackground(IntPtr tab, bool draw);

	EXPORT IntPtr GUITabControl_AddTab(IntPtr tabc, M_STRING caption, int id);
	EXPORT int GUITabControl_GetActiveTab(IntPtr tabc);
	EXPORT IntPtr GUITabControl_GetTab(IntPtr tabc, int index);
	EXPORT int GUITabControl_GetTabCount(IntPtr tabc);
	EXPORT bool GUITabControl_SetActiveTab(IntPtr tabc, int index);

	EXPORT IntPtr GUIToolBar_AddButton(IntPtr toolbar, int id, M_STRING text, M_STRING tooltip, IntPtr img, IntPtr pressedimg, bool isPushButton, bool useAlphaChannel);
}
