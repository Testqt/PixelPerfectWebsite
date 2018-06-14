using System.IO;
using System.Drawing;

using OpenQA.Selenium;

namespace SeleniumTestImage
{
    class ScreenWebElement
    {
        /*public static Image CropWebElement(IWebDriver driver, IWebElement element)
        {
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            Rectangle cropRect = new Rectangle(element.Location.X, element.Location.Y, element.Size.Width, element.Size.Height);
            Bitmap srcImage = new Bitmap(CompareImages.byteArrayToImage(ss.AsByteArray));
            Bitmap cropImage = new Bitmap(element.Size.Width, element.Size.Height);

            using (Graphics g = Graphics.FromImage(cropImage))
            {
                g.DrawImage(srcImage, new Rectangle(0, 0, cropImage.Width, cropImage.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return cropImage;
        }*/

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static Image FullScreen(IWebDriver driver)
        {
            IWebElement element = driver.FindElement(By.TagName("html"));
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            Bitmap srcImage = new Bitmap(byteArrayToImage(ss.AsByteArray));

            if (srcImage.Width <= element.Size.Width && srcImage.Height <= element.Size.Height)
            {
                return srcImage;    // когда всё влезло
            }

            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            try
            {
                js.ExecuteScript("document.getElementById(\"ids\").style.display = \"none\";"); // вырубаем isi
            }
            catch
            { }

            // result page
            Bitmap fullScreenImage = new Bitmap(element.Size.Width, element.Size.Height);

            for (int xOff = 0; xOff < element.Size.Width; xOff += ((xOff + srcImage.Width) >= element.Size.Width || (xOff + 2 * srcImage.Width) < element.Size.Width) ? srcImage.Width : element.Size.Width - srcImage.Width - xOff)
            {
                if (xOff > 0)
                {
                    js.ExecuteScript("window.scrollBy(" + srcImage.Width + ",0);");
                }
                else
                {
                    js.ExecuteScript("window.scrollTo(0,0)");
                }

                for (int yOff = 0; yOff < element.Size.Height; yOff += ((yOff + srcImage.Height) >= element.Size.Height || (yOff + 2 * srcImage.Height) < element.Size.Height) ? srcImage.Height : element.Size.Height - srcImage.Height - yOff)
                {
                    if (yOff > 0)
                    {
                        js.ExecuteScript("window.scrollBy(0," + srcImage.Height + ");");
                    }

                    ss = ((ITakesScreenshot)driver).GetScreenshot();
                    srcImage = new Bitmap(byteArrayToImage(ss.AsByteArray));

                    Rectangle cropRect = new Rectangle(0, 0, srcImage.Width, srcImage.Height);

                    using (Graphics g = Graphics.FromImage(fullScreenImage))
                    {
                        g.DrawImage(srcImage, new Rectangle(xOff, yOff, srcImage.Width, srcImage.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                    }
                }
            }

            return fullScreenImage;
        }
    }
}
