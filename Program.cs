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
                        // Bonus icons have a mix of yellows, but uniquely #fff16a
                        var pixel = bmp.GetPixel(x, y);

                        if (pixel.R == 0xFF && pixel.G == 0xF1 && pixel.B == 0x6A)
                        {
                            Cursor.Position = new Point(x, y);

                            Win32.mouse_event((uint)(Win32.MouseEventFlags.LEFTDOWN | Win32.MouseEventFlags.LEFTUP),
                                0, 0, 0, UIntPtr.Zero);

                            // HACK: The site doesn't register mouse movement that quickly, so
                            // slowly drag the mouse off the game board.
                            var y2 = y;
                            while (y2 > 50)
                            {
                                y2 -= 50;
                                Cursor.Position = new Point(x, y2);
                                Thread.Sleep(100);
                            }
                            
                            // HACK: Get a fresh screenshot to resolve dupe hits.
                            bmp.Dispose();
                            bmp = Desktop.GetImage();
                        }
                    }

                bmp.Dispose();

                //
                // We don't want to tax poor ol' DWM on Windows 8 here.
                // (Besides bonuses are slow generating.)
                //
                Thread.Sleep(500);
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
