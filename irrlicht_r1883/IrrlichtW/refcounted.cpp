#include "refcounted.h"
#include "conversion.h"

void ReferenceCounted_SetCallback(IntPtr obj, DROPCALLBACK callback)
{
	((IReferenceCounted*)(VIReferenceCounted*)obj)->setCallback(callback);
}
