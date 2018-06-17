using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeleniumTestImage
{
    class GetPage
    {

        public static string GetTextFromPage(IWebDriver driver, string pageUrl)
        {
            driver.Navigate().GoToUrl(pageUrl);
            driver.Navigate().Refresh();
            driver.Manage().Window.Maximize();
            System.Threading.Thread.Sleep(1000);

            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            try
            {
                js.ExecuteScript("document.getElementById(\"ids\").style.display = \"none\";"); // вырубаем isi
            }
            catch
            { }

            IWebElement body = driver.FindElement(By.TagName("body"));
            string pageText = body.Text;

            IList<IWebElement> hiddenElts = body.FindElements(By.ClassName("visually-hidden"));
            foreach (var elt in hiddenElts)
            {
                if (elt.Text != null && elt.Text.Length > 0)
                {
                    pageText = pageText.Replace(elt.Text, "\r\n");
                }
            }
            string temp;
            do
            {
                temp = pageText;
                pageText = Regex.Replace(pageText, @"\r\n\r\n", "\r\n");
            } while (temp != pageText);
            do
            {
                temp = pageText;
                pageText = Regex.Replace(pageText, @"\r\n ", "\r\n");
            } while (temp != pageText);

            pageText = Regex.Replace(pageText, @"February 2018\s+11US", "February 2018\r\n11US");
            return pageText;
        }
    }
}
