using OpenQA.Selenium;
using System;
using System.IO;

namespace RestaurantAutomation.Utilities
{
        public static class CaptureHelper
        {
            public static string TakeScreenshot(IWebDriver driver, string testCaseID)
            {
                try
                {
                    string folderPath = @"D:\Git\";
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // Lưu đúng tên: TC_RE_01.png
                    string filePath = Path.Combine(folderPath, testCaseID + ".png");

                    Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                    ss.SaveAsFile(filePath);

                    return filePath;
                }
                catch (Exception) { return "Error"; }
            }
        }
}