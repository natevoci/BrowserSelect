using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrowserSelect
{
    public abstract class Keyboard
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetAsyncKeyState(int keyCode);

        public static bool IsKeyDown(Keys key)
        {
            short retVal = GetAsyncKeyState((int)key);

            if ((retVal & 0x8000) == 0x8000)
                return true;

            return false;
        }
    }
}
