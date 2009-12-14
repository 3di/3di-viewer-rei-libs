using System;
using System.Security;
using System.Runtime.InteropServices;

namespace IrrlichtNETCP
{	
	public class ParticleEmitter : NativeElement
	{		
		public ParticleEmitter(IntPtr raw) : base(raw)
		{
		}

        public Dimension2Df MinStartSize
        {
            get
            {
                float[] min_start_size = Emitter_GetMinStartSize(_raw);
                return (new Dimension2Df(min_start_size[0], min_start_size[1]));
            }
            set
            {
                Emitter_SetMinStartSize(_raw, value.ToUnmanaged());
            }
        }

        public Dimension2Df MaxStartSize
        {
            get
            {
                float[] max_start_size = Emitter_GetMaxStartSize(_raw);
                return (new Dimension2Df(max_start_size[0], max_start_size[1]));
            }
            set
            {
                Emitter_SetMaxStartSize(_raw, value.ToUnmanaged());
            }
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

        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern void Pointer_SafeRelease_AEO(IntPtr pointer);

        [DllImport(Native.Dll)]
        static extern void Emitter_SetMinStartSize(IntPtr emitter, float[] size);

        [DllImport(Native.Dll)]
        static extern void Emitter_SetMaxStartSize(IntPtr emitter, [MarshalAs(UnmanagedType.LPArray)] float[] size);

        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern float[] Emitter_GetMinStartSize(IntPtr emitter);

        [DllImport(Native.Dll), SuppressUnmanagedCodeSecurity]
        static extern float[] Emitter_GetMaxStartSize(IntPtr emitter);
	}
}

namespace IrrlichtNETCP.Inheritable
{
    public interface IParticleEmitter
    {
        void Emit(uint now, uint timeSinceLastCall, out Particle[] Particles);
    }
}
