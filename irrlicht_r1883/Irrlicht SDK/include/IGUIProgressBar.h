// Copyright (C) 2008 Zoltan Dezso

#ifndef __I_GUI_PROGRESS_BAR_H_INCLUDED__
#define __I_GUI_PROGRESS_BAR_H_INCLUDED__

#include "IGUIElement.h"

namespace irr
{
namespace gui
{

	//! Default progress bar GUI element.
	class IGUIProgressBar : public IGUIElement
	{
	public:

		//! constructor
		IGUIProgressBar(IGUIEnvironment* environment, IGUIElement* parent, s32 id, core::rect<s32> rectangle)
			: IGUIElement(EGUIET_SCROLL_BAR, environment, parent, id, rectangle) {}

		//! gets the maximum value of the scrollbar.
		virtual s32 getMax() const = 0;

		//! sets the maximum value of the scrollbar.
		virtual void setMax(s32 max) = 0;

		//! gets the current position of the scrollbar
		virtual s32 getPos() const = 0;

		//! sets the current position of the scrollbar
		virtual void setPos(s32 pos) = 0;

		//! color information
		virtual void setForeground(video::SColor color) = 0;
		virtual void setBackground(video::SColor color) = 0;
	};


} // end namespace gui
} // end namespace irr

#endif

