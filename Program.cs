using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BonusBooster
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var bmp = Desktop.GetImage();

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        // Bonus icons have a mix of yellows, but uniquely #ffb812
                        var pixel = bmp.GetPixel(x, y);

                        if (pixel.R == 0xFF && pixel.G == 0xB8 && pixel.B == 0x12)
                        {
                            Cursor.Position = new Point(x, y);

                            Win32.mouse_event((uint)(Win32.MouseEventFlags.LEFTDOWN | Win32.MouseEventFlags.LEFTUP),
                                0, 0, 0, UIntPtr.Zero);

                            Cursor.Position = new Point(20, 20); // Not 0,0 as Windows 8 may show a conflicting back stack item

                            // HACK: Get a fresh screenshot to resolve dupe hits.
                            bmp = Desktop.GetImage();
                        }
                    }


                //
                // We don't want to tax poor ol' DWM on Windows 8 here.
                // (Besides bonuses are slow generating.)
                //
                Thread.Sleep(30000);
            }
        }
    }

    static class Desktop
    {
        public static Bitmap GetImage()
        {
            //
            // NOTE: This will only work on the primary monitor (where 0,0 exists).
            // Not a big deal given we're intended to be used while asleep.
            //

            var bounds = Screen.GetBounds(Point.Empty);
            var bmp = new Bitmap(bounds.Width, bounds.Height);

            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

            return bmp;
        }
    }

    static class Win32
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
        }
    }
}
