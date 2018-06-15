using System.IO;
using System.Drawing;

using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

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

        [Test]
        [TestCase("https://www.wikipedia.org/", "https://www.wikipedia.org/")]
        public void TestText(string prodUrl, string testUrl, string dirName)
        {
            string fullPath = "C:\\\\temp\\" + dirName + "\\";
            if (Directory.Exists(dirName))
            {
                Directory.Delete(fullPath);
            }
            Directory.CreateDirectory(fullPath);

            string prodText = GetPage.GetTextFromPage(_driver, prodUrl);
            string testText = GetPage.GetTextFromPage(_driver, testUrl);
            File.WriteAllText(fullPath + "prod.txt", prodText);
            File.WriteAllText(fullPath + "test.txt", testText);

            Assert.AreEqual(prodText, testText);
        }

        [Test]
        [TestCase("https://www.wikipedia.org/", "https://www.wikipedia.org/")]
        public void TestFooter(string urlProd, string urlTest)
        {
            _driver.Navigate().GoToUrl(urlProd);
            _driver.Navigate().Refresh();

            var iw = _driver.FindElement(By.Id("data-job-code"));
            string prodText = iw.FindElement(By.ClassName("desktop-view")).Text;
            string[] prodWordsReourse = prodText.Split(' ');
            List<string> prodWords = new List<string>();
            for (int i = 0; i < prodWordsReourse.Length; ++i)
            {
                if (prodWordsReourse[i].Length > 0)
                {
                    prodWords.Add(prodWordsReourse[i]);
                }
            }


            _driver.Navigate().GoToUrl(urlTest);
            _driver.Navigate().Refresh();

            string testText = _driver.FindElement(By.Id("block-prcdate")).Text;
            testText += " " + _driver.FindElement(By.Id("block-prccode")).Text;
            string[] testWordsResource = testText.Split(' ');
            List<string> testWords = new List<string>();
            for (int i = 0; i < testWordsResource.Length; ++i)
            {
                if (testWordsResource[i].Length > 0)
                {
                    testWords.Add(testWordsResource[i]);
                }
            }

            Assert.AreEqual(prodWords, testWords);

        }


        [TearDown]
        public void TearDown()
        {
            _driver.Close();
            _driver.Quit();
            _driver = null;
        }
    }
}
