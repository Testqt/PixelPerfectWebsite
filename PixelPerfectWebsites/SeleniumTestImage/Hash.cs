using System.Drawing;

namespace SeleniumTestImage
{
    class Hash
    {
        public static int CalcHash(int w, int h)
        {
            int ww = w - w % length;
            int hh = h - h % length;

            return ww * step + hh;
        }

        public static Rectangle GetCoords(int hash)
        {
            int h = hash % step;
            int w = hash / step;

            return new Rectangle(w, h, length, length);
        }

        private static int length = 10; // size of groups region
        private static int step = 1000000;
    }
}
