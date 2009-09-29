// Copyright (C) 2002-2008 Nikolaus Gebhardt
// This file is part of the "Irrlicht Engine".
// For conditions of distribution and use, see copyright notice in irrlicht.h

#define STRICT_REFERENCECOUNT
//#define TRACE_REFERENCECOUNT

#ifndef __I_IREFERENCE_COUNTED_H_INCLUDED__
#define __I_IREFERENCE_COUNTED_H_INCLUDED__

#include "irrTypes.h"

#ifdef TRACE_REFERENCECOUNT
#include "StackWalker.h"
#define REFTRACE_LOG "C:\\grabdrop.txt"

class MyStackWalker: public StackWalker
{
	virtual void OnOutput(LPCSTR szText)
	{
		FILE* fp=fopen(REFTRACE_LOG,"at");
		fprintf(fp,"%s",szText);
		fclose(fp);
	}
};
#endif

// TODO: These are copied from other Irrlicht header files but are repeated here
// (instead of #included) to avoid complicating the #include file ordering. Eventually
// these should be #defined in one place and #included wherever they are used, but
// for now they are duplicated.
#ifdef WIN32
#define STDCALL __stdcall*
#define uint unsigned int
#else
#define STDCALL *
#endif
typedef void* IntPtr;

// Define a callback type to be invoked when a Drop operation is invoked
typedef void (STDCALL DROPCALLBACK)(const IntPtr);

namespace irr
{

	//! Base class of most objects of the Irrlicht Engine.
	/** This class provides reference counting through the methods grab() and drop().
	It also is able to store a debug string for every instance of an object.
	Most objects of the Irrlicht
	Engine are derived from IReferenceCounted, and so they are reference counted.

	When you create an object in the Irrlicht engine, calling a method
	which starts with 'create', an object is created, and you get a pointer
	to the new object. If you no longer need the object, you have
	to call drop(). This will destroy the object, if grab() was not called
	in another part of you program, because this part still needs the object.
	Note, that you only need to call drop() to the object, if you created it,
	and the method had a 'create' in it.

	A simple example:

	If you want to create a texture, you may want to call an imaginable method
	IDriver::createTexture. You call
	ITexture* texture = driver->createTexture(dimension2d<s32>(128, 128));
	If you no longer need the texture, call texture->drop().

	If you want to load a texture, you may want to call imaginable method
	IDriver::loadTexture. You do this like
	ITexture* texture = driver->loadTexture("example.jpg");
	You will not have to drop the pointer to the loaded texture, because
	the name of the method does not start with 'create'. The texture
	is stored somewhere by the driver.
	*/
	class IReferenceCounted
	{
	public:

		//! Constructor.
		IReferenceCounted()
			: ReferenceCounter(1), DebugName(0), IsCallbackDefined(false)
		{
#ifdef TRACE_REFERENCECOUNT
			FILE* fp;
			fp=fopen(REFTRACE_LOG,"at");
			fprintf(fp,"-----------------------------------------------------------------------\n");
			fprintf(fp,"CREATE %d %p %s\n", ReferenceCounter, this, this->DebugName);
			fclose(fp);
			MyStackWalker sw; 
			sw.ShowCallstack();
#endif
		}

		//! Destructor.
		virtual ~IReferenceCounted()
		{
		}

		//! Grabs the object. Increments the reference counter by one.
		/** Someone who calls grab() to an object, should later also
		call drop() to it. If an object never gets as much drop() as
		grab() calls, it will never be destroyed. The
		IReferenceCounted class provides a basic reference counting
		mechanism with its methods grab() and drop(). Most objects of
		the Irrlicht Engine are derived from IReferenceCounted, and so
		they are reference counted.

		When you create an object in the Irrlicht engine, calling a
		method which starts with 'create', an object is created, and
		you get a pointer to the new object. If you no longer need the
		object, you have to call drop(). This will destroy the object,
		if grab() was not called in another part of you program,
		because this part still needs the object. Note, that you only
		need to call drop() to the object, if you created it, and the
		method had a 'create' in it.

		A simple example:

		If you want to create a texture, you may want to call an
		imaginable method IDriver::createTexture. You call
		ITexture* texture = driver->createTexture(dimension2d<s32>(128, 128));
		If you no longer need the texture, call texture->drop().
		If you want to load a texture, you may want to call imaginable
		method IDriver::loadTexture. You do this like
		ITexture* texture = driver->loadTexture("example.jpg");
		You will not have to drop the pointer to the loaded texture,
		because the name of the method does not start with 'create'.
		The texture is stored somewhere by the driver. */
#ifdef STRICT_REFERENCECOUNT
		void grab() const 
		{ 
			_IRR_DEBUG_BREAK_IF(ReferenceCounter <= 0)
			_IRR_DEBUG_BREAK_IF(ReferenceCounter >= 100)
			++ReferenceCounter; 
#ifdef TRACE_REFERENCECOUNT
			FILE* fp;
			fp=fopen(REFTRACE_LOG,"at");
			fprintf(fp,"-----------------------------------------------------------------------\n");
			fprintf(fp,"GRAB %d %p %s\n", ReferenceCounter, this, this->DebugName);
			fclose(fp);
			MyStackWalker sw; 
			sw.ShowCallstack();
#endif
		}
#else
		void grab() const { ++ReferenceCounter; }
#endif

		//! Drops the object. Decrements the reference counter by one.
		/** The IReferenceCounted class provides a basic reference
		counting mechanism with its methods grab() and drop(). Most
		objects of the Irrlicht Engine are derived from
		IReferenceCounted, and so they are reference counted.

		When you create an object in the Irrlicht engine, calling a
		method which starts with 'create', an object is created, and
		you get a pointer to the new object. If you no longer need the
		object, you have to call drop(). This will destroy the object,
		if grab() was not called in another part of you program,
		because this part still needs the object. Note, that you only
		need to call drop() to the object, if you created it, and the
		method had a 'create' in it.

		A simple example:

		If you want to create a texture, you may want to call an
		imaginable method IDriver::createTexture. You call
		ITexture* texture = driver->createTexture(dimension2d<s32>(128, 128));
		If you no longer need the texture, call texture->drop().
		If you want to load a texture, you may want to call imaginable
		method IDriver::loadTexture. You do this like
		ITexture* texture = driver->loadTexture("example.jpg");
		You will not have to drop the pointer to the loaded texture,
		because the name of the method does not start with 'create'.
		The texture is stored somewhere by the driver.
		\return True, if the object was deleted. */
		bool drop() const
		{
			// someone is doing bad reference counting.
			_IRR_DEBUG_BREAK_IF(ReferenceCounter <= 0)

#ifdef STRICT_REFERENCECOUNT
			_IRR_DEBUG_BREAK_IF(ReferenceCounter >= 100)
#endif
			--ReferenceCounter;
#ifdef TRACE_REFERENCECOUNT
			FILE* fp;
			fp=fopen(REFTRACE_LOG,"at");
			fprintf(fp,"-----------------------------------------------------------------------\n");
			fprintf(fp,"DROP %d %p %s\n", ReferenceCounter, this, this->DebugName);
			fclose(fp);
			MyStackWalker sw; 
			sw.ShowCallstack();
#endif
			if (!ReferenceCounter)
			{
				if(IsCallbackDefined)
				{
					_callback((void *)this);
				}
				delete this;
				return true;
			}

			return false;
		}

		//! Get the reference count.
		/** \return Current value of the reference counter. */
		s32 getReferenceCount() const
		{
			return ReferenceCounter;
		}

		//! Returns the debug name of the object.
		/** The Debugname may only be set and changed by the object
		itself. This method should only be used in Debug mode.
		\return Returns a string, previously set by setDebugName(); */
		const c8* getDebugName() const
		{
			return DebugName;
		}

		void setCallback(DROPCALLBACK call)
		{
			IsCallbackDefined = true;
			_callback = call;
		}

	protected:

		//! Sets the debug name of the object.
		/** The Debugname may only be set and changed by the object
		itself. This method should only be used in Debug mode.
		\param newName: New debug name to set. */
		void setDebugName(const c8* newName)
		{
			DebugName = newName;
		}

	private:
		//! The reference counter. Mutable to do reference counting on const objects.
		mutable s32 ReferenceCounter;
		//! The debug name.
		const c8* DebugName;
		DROPCALLBACK _callback;
		bool IsCallbackDefined;
	};

} // end namespace irr

#endif

