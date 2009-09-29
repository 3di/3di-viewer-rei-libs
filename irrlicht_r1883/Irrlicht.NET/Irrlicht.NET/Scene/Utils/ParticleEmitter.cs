using System;
using System.Security;

namespace IrrlichtNETCP
{	
	public class ParticleEmitter : NativeElement
	{		
		public ParticleEmitter(IntPtr raw) : base(raw)
		{
		}

        public override void Dispose()
        {
            if (Elements.ContainsKey(Raw))
                Elements.Remove(Raw);
            if (_raw != IntPtr.Zero)
                try { Pointer_SafeRelease_AEO(_raw); } catch { };
        }

        public override void Drop()
        {
            if (_raw != IntPtr.Zero)
                try { Pointer_SafeRelease_AEO(_raw); }
                catch { };
        }

        [System.Runtime.InteropServices.DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern void Pointer_SafeRelease_AEO(IntPtr pointer);
	}
}

namespace IrrlichtNETCP.Inheritable
{
    public interface IParticleEmitter
    {
        void Emit(uint now, uint timeSinceLastCall, out Particle[] Particles);
    }
}
