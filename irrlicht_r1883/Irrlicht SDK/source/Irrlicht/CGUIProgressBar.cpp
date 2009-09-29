// Copyright (C) 2008 Zoltan Dezso

#include "CGUIProgressBar.h"
#ifdef _IRR_COMPILE_WITH_GUI_

#include "IGUISkin.h"
#include "IGUIEnvironment.h"
#include "IVideoDriver.h"
#include "CGUIButton.h"
#include "IGUIFont.h"
#include "IGUIFontBitmap.h"
#include "os.h"

namespace irr
{
namespace gui
{


//! constructor
	CGUIProgressBar::CGUIProgressBar(bool horizontal, IGUIEnvironment* environment,
				IGUIElement* parent, s32 id,
				core::rect<s32> rectangle, bool noclip)
	: IGUIProgressBar(environment, parent, id, rectangle), 
	Horizontal(horizontal),
	Background(video::SColor(50,100,100,100)), Foreground(video::SColor(50,0,0,255)), 
	Pos(0), DrawPos(0),
	DrawHeight(0), Max(100), DesiredPos(0),
	LastChange(0)
{
	#ifdef _DEBUG
	setDebugName("CGUIProgressBar");
	#endif

	setPos(0);
}


//! destructor
CGUIProgressBar::~CGUIProgressBar()
{
}


//! called if an event happened.
bool CGUIProgressBar::OnEvent(const SEvent& event)
{
	return IGUIElement::OnEvent(event);
}

//! draws the element and its children
void CGUIProgressBar::draw()
{
  if(!IsVisible) return;
  IGUISkin* skin = Environment->getSkin();
  if(!skin) return;
  irr::video::IVideoDriver* driver = Environment->getVideoDriver();
  if(!driver) return;

  core::rect<s32> FrameRect(AbsoluteRect);
  driver->draw2DRectangle(Background, FrameRect, &AbsoluteClippingRect);

  skin->draw3DSunkenPane(this, Background, true, false, FrameRect, &AbsoluteClippingRect);
  FrameRect.UpperLeftCorner.X += 1;
  FrameRect.UpperLeftCorner.Y += 1;
  FrameRect.LowerRightCorner.X -= 1;
  FrameRect.LowerRightCorner.Y -= 1;

  core::rect<s32> ProgressRect(FrameRect);
  if(Horizontal)
    ProgressRect.LowerRightCorner.X = ProgressRect.UpperLeftCorner.X + (Pos * (FrameRect.LowerRightCorner.X - FrameRect.UpperLeftCorner.X) / Max);
  else
    ProgressRect.UpperLeftCorner.Y = ProgressRect.LowerRightCorner.Y - (Pos * (FrameRect.LowerRightCorner.Y - FrameRect.UpperLeftCorner.Y) / Max);
  driver->draw2DRectangle(Foreground, ProgressRect, &AbsoluteClippingRect);
  IGUIElement::draw();
}

void CGUIProgressBar::updateAbsolutePosition()
{
	IGUIElement::updateAbsolutePosition();
	// todo: properly resize
	if (Horizontal)
	{
		const f32 f = (RelativeRect.getWidth() - ((f32)RelativeRect.getHeight()*3.0f)) / (f32)Max;
		DrawPos = (s32)((Pos * f) + ((f32)RelativeRect.getHeight() * 0.5f));
		DrawHeight = RelativeRect.getHeight();
	}
	else
	{
		f32 f = 0.0f;
		if (Max != 0)
			f = (RelativeRect.getHeight() - ((f32)RelativeRect.getWidth()*3.0f)) / (f32)Max;

		DrawPos = (s32)((Pos * f) + ((f32)RelativeRect.getWidth() * 0.5f));
		DrawHeight = RelativeRect.getWidth();
	}
}

//! sets the position of the scrollbar
void CGUIProgressBar::setPos(s32 pos)
{
	if (pos < 0)
		Pos = 0;
	else if (pos > Max)
		Pos = Max;
	else
		Pos = pos;

	if (Horizontal)
	{
		const f32 f = (RelativeRect.getWidth() - ((f32)RelativeRect.getHeight()*3.0f)) / (f32)Max;
		DrawPos = (s32)((Pos * f) + ((f32)RelativeRect.getHeight() * 0.5f));
		DrawHeight = RelativeRect.getHeight();
	}
	else
	{
		f32 f = 0.0f;
		if (Max != 0)
			f = (RelativeRect.getHeight() - ((f32)RelativeRect.getWidth()*3.0f)) / (f32)Max;

		DrawPos = (s32)((Pos * f) + ((f32)RelativeRect.getWidth() * 0.5f));
		DrawHeight = RelativeRect.getWidth();
	}
}

//! gets the maximum value of the scrollbar.
s32 CGUIProgressBar::getMax() const
{
	return Max;
}

//! sets the maximum value of the scrollbar.
void CGUIProgressBar::setMax(s32 max)
{
	if (max > 0)
		Max = max;
	else
		Max = 0;

	bool enable = (Max != 0);
	setPos(Pos);
}

void CGUIProgressBar::setBackground(video::SColor background)
{
	Background = background;
}

void CGUIProgressBar::setForeground(video::SColor foreground)
{
	Foreground = foreground;
}


//! gets the current position of the scrollbar
s32 CGUIProgressBar::getPos() const
{
	return Pos;
}

//! Writes attributes of the element.
void CGUIProgressBar::serializeAttributes(io::IAttributes* out, io::SAttributeReadWriteOptions* options=0) const
{
	IGUIProgressBar::serializeAttributes(out,options);

	out->addBool("Horizontal",	Horizontal);
	out->addInt ("Value",		Pos);
	out->addInt ("Max",		Max);
	out->addColor ("Background", Background);
	out->addColor ("Foreground", Foreground);
}


//! Reads attributes of the element
void CGUIProgressBar::deserializeAttributes(io::IAttributes* in, io::SAttributeReadWriteOptions* options=0)
{
	IGUIProgressBar::deserializeAttributes(in,options);

	Horizontal = in->getAttributeAsBool("Horizontal");
	setMax(in->getAttributeAsInt("Max"));
	setPos(in->getAttributeAsInt("Value"));
	setBackground(in->getAttributeAsColor("Background"));
	setForeground(in->getAttributeAsColor("Foreground"));
}


} // end namespace gui
} // end namespace irr

#endif // _IRR_COMPILE_WITH_GUI_

