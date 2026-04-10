using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RestaurantAutomation.Pages;
using RestaurantAutomation.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RestaurantAutomation.Tests
{
    [TestFixture]
    public class ChefTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private ChefPage chefPage;

        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "ChefData.json");
        private string excelPath = @"C:\bao dam chat luong phan mem _TH\TestCase_Quản lý quán ăn.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();

            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddArgument("--disable-notifications");
            options.AddArgument("--start-maximized");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            loginPage = new LoginPage(driver);
            chefPage = new ChefPage(driver);
        }

        [Test]
        public void TestChef_Flow_RealUser()
        {
            string jsonContent = File.ReadAllText(jsonPath);
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                string expected = data.ExpectedResult.ToString();
                string action = data.Action.ToString();
                string roleSheet = data.SheetName.ToString();

                try
                {
                    // ===== LOGIN =====
                    driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
                    loginPage.Login(data.Username.ToString(), data.Password.ToString());
                    Thread.Sleep(20000);

                    if (!loginPage.IsLoggedIn())
                        throw new Exception("Login fail");

                    // ===== CLICK MENU =====

                    // ===== SWITCH TAB =====
                    switch (action)
                    {
                        case "FullProcessOrder":

                            // ===== STEP 1: Pending → Start Processing =====
                            chefPage.GoToPending();
                            chefPage.StartProcessingOrder();

                            // ===== STEP 2: Qua Preparing → Start ALL dishes =====
                            chefPage.GoToPreparing();
                            chefPage.StartAllDishes();

                            // ===== STEP 3: Finish Order =====
                            chefPage.FinishOrder();

                            ExcelHelper.UpdateExcel(excelPath, testCaseID, "PASS: Hoàn thành toàn bộ flow", "Passed", roleSheet, "");
                            break;

                    }

                    Thread.Sleep(2000);

                    // ===== VERIFY =====
                    if (driver.PageSource.Contains(expected))
                    {
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "PASS", "Passed", roleSheet, "");
                    }
                    else
                    {
                        string img = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "FAIL", "Failed", roleSheet, img);
                    }

                    loginPage.Logout();
                }
                catch (Exception ex)
                {
                    string img = CaptureHelper.TakeScreenshot(driver, testCaseID);
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, ex.Message, "Failed", roleSheet, img);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}