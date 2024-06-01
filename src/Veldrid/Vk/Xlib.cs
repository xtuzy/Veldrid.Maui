using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Veldrid.Vk
{
    public class Xlib
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct Display
        {
        }

        public struct Window
        {
            public IntPtr Value;
        }
    }
}
