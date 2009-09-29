using System;
using System.Runtime.InteropServices;
using System.Security;

namespace IrrlichtNETCP
{
    public class WriteFile : NativeElement
    {
        public WriteFile(IntPtr raw)
            : base(raw)
        {
        }

        public void Close()
        {
            WriteFile_Close(_raw);
        }

        #region Native Code
        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern void WriteFile_Close(IntPtr writefile);
        #endregion
    }
}
