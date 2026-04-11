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
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace RestaurantAutomation.Tests
{
    [TestFixture]
    public class CreateOrderTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private TableManagementPage tablePage;
        private OrderPage orderPage;
        private WebDriverWait wait;

        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "OrderData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\LT\Testcase.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("profile.password_manager_leak_detection", false);

            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--start-maximized");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25)); // Đợi lâu hơn cho Vercel
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            loginPage = new LoginPage(driver);
            tablePage = new TableManagementPage(driver);
            orderPage = new OrderPage(driver);

            driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
            loginPage.Login("ngochoa@gmail.com", "123456");

            wait.Until(ExpectedConditions.UrlContains("dashboard"));

            // Đợi dữ liệu sơ đồ bàn lên thực tế (tránh hiện khung rỗng)
            tablePage.WaitForDataToRender();
        }

        [Test]
        public void Test_CreateOrder_FromJSON()
        {
            string jsonContent = File.ReadAllText(jsonPath);
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

            foreach (var data in testDataList)
            {
                string tcID = data.TestCaseID.ToString();
                string tableNum = data.TableNumber.ToString();
                string expected = data.ExpectedResult.ToString().ToLower();
                string roleSheet = data.SheetName.ToString();
                string action = data.Action != null ? data.Action.ToString().ToLower() : "create";

                try
                {
                    // Trước mỗi Case, kiểm tra web có bị "đơ rỗng dữ liệu" không
                    tablePage.WaitForDataToRender();

                    // 1. Click chọn bàn
                    tablePage.ClickTableByNumber(tableNum);
                    Thread.Sleep(1500);

                    // 2. Xử lý Action
                    switch (action)
                    {
                        case "finish":
                            tablePage.ClickFinishTable();
                            ValidateSuccessModal(tcID, expected, roleSheet);
                            break;

                        case "force_clean":
                            tablePage.ClickForceClean();
                            orderPage.AcceptBrowserAlert();
                            ValidateSuccessModal(tcID, expected, roleSheet);
                            break;

                        case "call_staff":
                            tablePage.ClickCallStaff();
                            ValidateSuccessModal(tcID, expected, roleSheet);
                            break;

                        case "add_food":
                            tablePage.ClickAddMoreFood();
                            Thread.Sleep(1000);
                            ExecuteOrderingProcess(data, tcID, expected, roleSheet);
                            break;

                        case "cancel":
                            orderPage.ClickCancel();
                            Thread.Sleep(1000);
                            ExcelHelper.UpdateExcel(excelPath, tcID, "Đã đóng form (Hủy thành công)", "Passed", roleSheet, "");
                            break;

                        default:
                            ExecuteOrderingProcess(data, tcID, expected, roleSheet);
                            break;
                    }

                    // THAY VÌ REFRESH LIÊN TỤC: Kiểm tra nếu Modal còn hiện mới Refresh
                    if (driver.FindElements(By.ClassName("modal-backdrop")).Count > 0)
                    {
                        driver.Navigate().Refresh();
                        tablePage.WaitForDataToRender();
                    }
                    else
                    {
                        Thread.Sleep(1000); // Nghỉ ngắn cho API đồng bộ
                    }
                }
                catch (Exception ex)
                {
                    string ssPath = CaptureHelper.TakeScreenshot(driver, tcID);
                    ExcelHelper.UpdateExcel(excelPath, tcID, "Lỗi: " + ex.Message, "Failed", roleSheet, ssPath);
                    driver.Navigate().Refresh();
                    tablePage.WaitForDataToRender();
                }
            }
        }

        // --- CÁC HÀM PHỤ TRỢ ---

        private void ValidateSuccessModal(string tcID, string expected, string roleSheet)
        {
            Thread.Sleep(2000);
            string actualTitle = orderPage.GetSuccessTitleText();
            if (actualTitle.ToLower().Contains(expected))
            {
                ExcelHelper.UpdateExcel(excelPath, tcID, actualTitle, "Passed", roleSheet, "");
            }
            else
            {
                string ssPath = CaptureHelper.TakeScreenshot(driver, tcID);
                ExcelHelper.UpdateExcel(excelPath, tcID, "Thực tế: " + (actualTitle == "" ? "Không hiện thông báo" : actualTitle), "Failed", roleSheet, ssPath);
            }
        }

        private void ExecuteOrderingProcess(dynamic data, string tcID, string expected, string roleSheet)
        {
            List<string> menuItems = data.MenuItems != null ? data.MenuItems.ToObject<List<string>>() : new List<string>();

            if (menuItems.Count == 0)
            {
                if (!orderPage.IsSubmitButtonEnabled())
                    ExcelHelper.UpdateExcel(excelPath, tcID, "Nút Tạo Order đã bị khóa chính xác", "Passed", roleSheet, "");
                else
                {
                    string ssPath = CaptureHelper.TakeScreenshot(driver, tcID);
                    ExcelHelper.UpdateExcel(excelPath, tcID, "LỖI: Nút vẫn mở dù chưa chọn món", "Failed", roleSheet, ssPath);
                }
            }
            else
            {
                foreach (var item in menuItems)
                {
                    orderPage.SelectFood(item);
                    Thread.Sleep(500);
                }
                orderPage.EnterNotes(data.OrderNotes.ToString());

                if (orderPage.IsSubmitButtonEnabled())
                {
                    orderPage.ClickCreateOrder();
                    Thread.Sleep(1500); // Bỏ qua toast phụ

                    string msg = orderPage.GetSuccessMessage(expected);
                    if (string.IsNullOrEmpty(msg))
                        msg = driver.PageSource.Contains("thành công") ? "Order đã được tạo thành công!" : "";

                    if (!string.IsNullOrEmpty(msg) && msg.ToLower().Contains(expected))
                        ExcelHelper.UpdateExcel(excelPath, tcID, msg, "Passed", roleSheet, "");
                    else
                    {
                        string ssPath = CaptureHelper.TakeScreenshot(driver, tcID);
                        ExcelHelper.UpdateExcel(excelPath, tcID, "Thực tế: " + msg, "Failed", roleSheet, ssPath);
                    }
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null) { driver.Quit(); driver.Dispose(); }
        }
    }
}