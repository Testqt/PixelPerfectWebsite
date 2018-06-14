using System.IO;
using System.Drawing;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumTestImage
{
    [TestFixture]
    public class ScreenTest
    {
        IWebDriver _driver = null;

        [SetUp]
        public void SetUp()
        {
            _driver = new ChromeDriver();
        }

        Image CreatePageScreen(string url)
        {
            _driver.Navigate().GoToUrl(url);
            _driver.Navigate().Refresh();
            _driver.Manage().Window.Maximize();
            System.Threading.Thread.Sleep(1000);
            return ScreenWebElement.FullScreen(_driver);
        }

        [Test]
	[TestCase("https://www.wikipedia.org/", "https://www.wikipedia.org/", "wiki")]
        public void TestScreen(string prodUrl, string testUrl, string dirName)
        {
            string fullPath = "C:\\\\temp\\" + dirName + "\\";
            if (Directory.Exists(dirName))
            {
                Directory.Delete(fullPath);
            }
            Directory.CreateDirectory(fullPath);
            Image prodIm = CreatePageScreen(prodUrl);
            Image testIm = CreatePageScreen(testUrl);
            CompareImages compareImage = new CompareImages(prodIm, testIm, fullPath);
            Assert.IsTrue(compareImage.Compare());
        }
        

        [TearDown]
        public void TearDown()
        {
            _driver.Close();
            _driver = null;
        }
    }
}
