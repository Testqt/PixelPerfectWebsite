using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace SeleniumTestImage
{
    class CompareImages
    {
        private Image prod = null;
        private Image test = null;
        private Image difference = null;
        private Color diffColor = Color.Red;
        int eps = 8;

        private Dictionary<int, int> pixelTolerance = null;
        private string dirPath = null;

        public CompareImages(Image prod, Image test, string dirPath)
        {
            Prod = prod;
            Test = test;
            this.dirPath = dirPath;
        }

        public Image Prod
        {
            get { return this.prod; }
            set { this.prod = value; }
        }

        public Image Test
        {
            get { return this.test; }
            set { this.test = value; }
        }

        public Image Diff
        {
            get { return this.difference; }
        }

        public static Image Crop(Image imgSrc, int width, int height)
        {
            Image img = new Bitmap(width, height);
            Rectangle cropRect = new Rectangle(0, 0, width, height);

            using (Graphics g = Graphics.FromImage(img))
            {
                g.DrawImage(imgSrc, new Rectangle(0, 0, width, height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return img;
        }

        public Image CreateFullDiffImage(Image prod_, Image test_)
        {
            Bitmap prodBmp = prod_ as Bitmap;
            Bitmap testBmp = test_ as Bitmap;

            if (prodBmp.Width == testBmp.Height && prodBmp.Height == testBmp.Height)
            {
                return CreateDiffImage(prodBmp, testBmp);
            }

            int imgWidth = Math.Max(prodBmp.Width, testBmp.Width);
            int imgHeight = Math.Max(prodBmp.Height, testBmp.Height);
            Bitmap diffImage = new Bitmap(imgWidth, imgHeight);

            int imgComWidth = Math.Min(prodBmp.Width, testBmp.Width);
            int imgComHeight = Math.Min(prodBmp.Height, testBmp.Height);
            Bitmap minImg1 = Crop(prodBmp, imgComWidth, imgComHeight) as Bitmap;
            Bitmap minImg2 = Crop(testBmp, imgComWidth, imgComHeight) as Bitmap;
            Bitmap minDiffImg = CreateDiffImage(minImg1, minImg2) as Bitmap;

            Rectangle cropRect = new Rectangle(0, 0, minDiffImg.Width, minDiffImg.Height);
            using (Graphics g = Graphics.FromImage(diffImage))
            {
                g.DrawImage(minDiffImg, new Rectangle(0, 0, minDiffImg.Width, minDiffImg.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            // if prod > test color = red
            // if test > prod color = blue
            if (prodBmp.Width > testBmp.Width)
            {
                for (int w = minImg1.Width; w < prodBmp.Width; ++w)
                {
                    for (int h = 0; h < prodBmp.Height; ++h)
                    {
                        diffImage.SetPixel(w, h, Color.Red);
                    }
                }
            }
            else
            {
                for (int w = minImg2.Width; w < testBmp.Width; ++w)
                {
                    for (int h = 0; h < testBmp.Height; ++h)
                    {
                        diffImage.SetPixel(w, h, Color.Blue);
                    }
                }
            }

            if (prodBmp.Height > testBmp.Height)
            {
                for (int h = minImg1.Height; h < prodBmp.Height; ++h)
                {
                    for (int w = 0; w < prodBmp.Width; ++w)
                    {
                        diffImage.SetPixel(w, h, Color.Red);
                    }
                }
            }
            else
            {
                for (int h = minImg2.Height; h < testBmp.Height; ++h)
                {
                    for (int w = 0; w < testBmp.Width; ++w)
                    {
                        diffImage.SetPixel(w, h, Color.Blue);
                    }
                }
            }

            return diffImage;
        }

        public Image CreateDiffImage(Image prod_, Image test_)
        {
            Bitmap prodBmp = prod_ as Bitmap;
            Bitmap testBmp = test_ as Bitmap;

            if (prodBmp.Width != testBmp.Width || prodBmp.Height != testBmp.Height)
            {
                return null;
            }

            for (int w = 0; w < prodBmp.Width; ++w)
            {
                for (int h = 0; h < prodBmp.Height; ++h)
                {
                    if ((Math.Abs(prodBmp.GetPixel(w, h).R - testBmp.GetPixel(w, h).R) > eps) ||
                        (Math.Abs(prodBmp.GetPixel(w, h).G - testBmp.GetPixel(w, h).G) > eps) ||
                        (Math.Abs(prodBmp.GetPixel(w, h).B - testBmp.GetPixel(w, h).B) > eps))
                    {
                        FillPexelsToleranceContainer(w, h);
                    }
                }
            }

            CorrectedPexelsToleranceContainer();

            Bitmap diffImage = new Bitmap(prodBmp.Width, testBmp.Height);

            diffImage = testBmp;

            foreach (int key in pixelTolerance.Keys)
            {
                Rectangle rect = Hash.GetCoords(key);
                for (int w = rect.X; w < rect.X + rect.Width && w < diffImage.Width; ++w)
                {
                    diffImage.SetPixel(w, rect.Y, diffColor);

                    if (rect.Y + rect.Height < diffImage.Height)
                        diffImage.SetPixel(w, rect.Y + rect.Height, diffColor);
                }
                for (int h = rect.Y; h < rect.Y + rect.Height && h < diffImage.Height; ++h)
                {
                    diffImage.SetPixel(rect.X, h, diffColor);

                    if (rect.X + rect.Width < diffImage.Width)
                        diffImage.SetPixel(rect.X + rect.Width, h, diffColor);
                }
            }

            return diffImage;
        }

        void CorrectedPexelsToleranceContainer()
        {
            if (pixelTolerance == null)
            {
                return;
            }
            var keys = pixelTolerance.Keys;
            List<int> removeKeys = new List<int>();
            foreach (int key in keys)
            {
                // 7 amount of tolerance pixels in rectangle
                if (pixelTolerance[key] < 4)
                {
                    removeKeys.Add(key);
                }
            }
            foreach (int key in removeKeys)
            {
                pixelTolerance.Remove(key);
            }
        }

        public void FillPexelsToleranceContainer(int w, int h)
        {
            if (pixelTolerance == null)
            {
                pixelTolerance = new Dictionary<int, int>();
            }
            if (pixelTolerance.ContainsKey(Hash.CalcHash(w, h)))
            {
                pixelTolerance[Hash.CalcHash(w, h)]++;
            }
            else
            {
                pixelTolerance.Add(Hash.CalcHash(w, h), 1);
            }
        }

        public bool Compare()
        {
            Bitmap expBmp = Prod as Bitmap;
            Bitmap actBmp = Test as Bitmap;

            // same size
            if (expBmp.Width == actBmp.Width && expBmp.Height == actBmp.Height)
            {
                for (int w = 0; w < expBmp.Width; ++w)
                {
                    for (int h = 0; h < expBmp.Height; ++h)
                    {
                        if (expBmp.GetPixel(w, h) != actBmp.GetPixel(w, h))
                        {
                            FillPexelsToleranceContainer(w, h);
                        }
                    }
                }

                CorrectedPexelsToleranceContainer();
                if (pixelTolerance == null  || pixelTolerance.Count == 0)
                {
                    return true;
                }
            }
            // different size of image
            difference = CreateFullDiffImage(expBmp, actBmp);

            Prod.Save(dirPath + "prod.png", ImageFormat.Png);
            Test.Save(dirPath + "test.png", ImageFormat.Png);
            difference.Save(dirPath + "difference.png", ImageFormat.Png);

            return false;
        }
       
    }
}
