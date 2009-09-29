using System;
using System.Runtime.InteropServices;
using System.Security;

namespace IrrlichtNETCP
{	
	public class GUIToolBar : GUIElement
	{		
		public GUIToolBar(IntPtr raw) : base(raw)
		{
        }

        public GUIButton AddButton(int id, string text, string tooltip, Texture img, Texture pressedimg, bool isPushButton, bool useAlphaChannel)
        {
            return (GUIButton)NativeElement.GetObject(
                GUIToolBar_AddButton(_raw, id, text, tooltip, img == null ? IntPtr.Zero : img.Raw, pressedimg == null ? IntPtr.Zero : pressedimg.Raw, isPushButton, useAlphaChannel),
                typeof(GUIButton));
        }

        #region Native Invokes (you must be tired now... Actually I am... This must be the... hundreth time I write "region Native Invokes")
         [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern IntPtr GUIToolBar_AddButton(IntPtr toolbar, int id, string text, string tooltip, IntPtr img, IntPtr pressedimg, bool isPushButton, bool useAlphaChannel);
        #endregion
    }	
}
