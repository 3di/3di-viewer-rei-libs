// Copyright (C) 2008 Zoltan Dezso

#ifndef __C_GUI_PROGRESS_BAR_H_INCLUDED__
#define __C_GUI_PROGRESS_BAR_H_INCLUDED__

#include "IrrCompileConfig.h"
#ifdef _IRR_COMPILE_WITH_GUI_

#include "IGUIProgressBar.h"
#include "IGUIButton.h"
#include "IVideoDriver.h"

namespace irr
{
namespace gui
{

	class CGUIProgressBar : public IGUIProgressBar
	{
	public:

		//! constructor
		CGUIProgressBar(bool horizontal, IGUIEnvironment* environment,
			IGUIElement* parent, s32 id, core::rect<s32> rectangle,
			bool noclip=false);

		//! destructor
		virtual ~CGUIProgressBar();

		//! called if an event happened.
		virtual bool OnEvent(const SEvent& event);

		//! draws the element and its children
		virtual void draw();

		//! gets the maximum value of the scrollbar.
		virtual s32 getMax() const;

		//! sets the maximum value of the scrollbar.
		virtual void setMax(s32 max);

		//! gets the current position of the scrollbar
		virtual s32 getPos() const;

		//! sets the position of the scrollbar
		virtual void setPos(s32 pos);

		//! color information
		virtual void setForeground(video::SColor color);
		virtual void setBackground(video::SColor color);

		//! updates the rectangle
		virtual void updateAbsolutePosition();

		//! Writes attributes of the element.
		virtual void serializeAttributes(io::IAttributes* out, io::SAttributeReadWriteOptions* options) const;

		//! Reads attributes of the element
		virtual void deserializeAttributes(io::IAttributes* in, io::SAttributeReadWriteOptions* options);

	private:

		void refreshControls();
		s32 getPosFromMousePos(s32 x, s32 y) const;

		bool Horizontal;
		video::SColor Background;
		video::SColor Foreground;
		s32 Pos;
		s32 DrawPos;
		s32 DrawHeight;
		s32 Max;
		s32 DesiredPos;
		u32 LastChange;
	};

} // end namespace gui
} // end namespace irr

#endif // _IRR_COMPILE_WITH_GUI_

#endif

