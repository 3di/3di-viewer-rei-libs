using System;
using System.Runtime.InteropServices;
using System.Security;

namespace IrrlichtNETCP
{	
	public class GUIProgressBar : GUIElement
	{
        public GUIProgressBar(IntPtr raw)
            : base(raw)
		{
        }

        public int Pos
        {
            get
            {
                return GUIProgressBar_GetPos(_raw);
            }
            set
            {
                GUIProgressBar_SetPos(_raw, value);
            }
        }

        public int Max
        {
            set
            {
                GUIProgressBar_SetMax(_raw, value);
            }
        }

        public Color Background
        {
            set
            {
                GUIProgressBar_SetBackground(_raw, value.ToUnmanaged());
            }
        }

        public Color Foreground
        {
            set
            {
                GUIProgressBar_SetForeground(_raw, value.ToUnmanaged());
            }
        }


        #region Native Invokes
         [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern int GUIProgressBar_GetPos(IntPtr sb);

         [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
         static extern void GUIProgressBar_SetMax(IntPtr sb, int max);

         [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
         static extern void GUIProgressBar_SetPos(IntPtr sb, int pos);

         [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
         static extern void GUIProgressBar_SetBackground(IntPtr sb, int[] color);

         [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
         static extern void GUIProgressBar_SetForeground(IntPtr sb, int[] color);
        #endregion
    }	
}
