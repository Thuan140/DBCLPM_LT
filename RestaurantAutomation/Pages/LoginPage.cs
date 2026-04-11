using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace RestaurantAutomation.Pages
{
    public class LoginPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public LoginPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- Locators Đăng nhập ---
        private By txtUsername = By.XPath("//input[@type='text' or @type='email']");
        private By txtPassword = By.XPath("//input[@type='password']");
        private By btnLogin = By.CssSelector("button.login-button");

        // --- Locators Đăng xuất (Dropdown) ---
        private By btnUserMenu = By.CssSelector(".btn.btn-link.p-0.text-dark");
        private By btnLogoutItem = By.CssSelector(".dropdown-item.text-danger");
        private By btnDirectLogout = By.Id("logoutButton");
        // --- Hành động Đăng nhập ---
        public void Login(string user, string pass)
        {
            var userElem = wait.Until(ExpectedConditions.ElementIsVisible(txtUsername));
            userElem.Clear();
            userElem.SendKeys(user);

            var passElem = driver.FindElement(txtPassword);
            passElem.Clear();
            passElem.SendKeys(pass);

            // Sử dụng JavaScript Click để tránh bị chặn bởi thẻ span bên trong nút
            IWebElement button = wait.Until(ExpectedConditions.ElementToBeClickable(btnLogin));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", button);
        }

        // --- Kiểm tra trạng thái đã đăng nhập chưa ---
        public bool IsLoggedIn()
        {
            try
            {
                // Đã đăng nhập nếu: URL chứa dashboard HOẶC thấy nút menu (Phục vụ) HOẶC thấy nút logout trực tiếp (Đầu bếp)
                return driver.Url.Contains("dashboard") ||
                       driver.FindElements(btnUserMenu).Count > 0 ||
                       driver.FindElements(btnDirectLogout).Count > 0;
            }
            catch { return false; }
        }

        // --- Hành động Đăng xuất ---
        public void Logout()
        {
            try
            {
                // Kiểm tra xem có nút đăng xuất trực tiếp (Đầu bếp/Thu ngân) không
                var directLogoutElements = driver.FindElements(btnDirectLogout);

                if (directLogoutElements.Count > 0 && directLogoutElements[0].Displayed)
                {
                    // CASE 1: Role Đầu bếp/Thu ngân - Click trực tiếp
                    Console.WriteLine("Phát hiện nút đăng xuất trực tiếp (Đầu bếp/Thu ngân).");
                    IWebElement directBtn = wait.Until(ExpectedConditions.ElementToBeClickable(btnDirectLogout));
                    directBtn.Click();
                }
                else
                {
                    // CASE 2: Role Phục vụ - Click dropdown menu trước
                    Console.WriteLine("Sử dụng quy trình đăng xuất 2 bước (Phục vụ).");
                    IWebElement userMenu = wait.Until(ExpectedConditions.ElementToBeClickable(btnUserMenu));
                    userMenu.Click();

                    IWebElement logoutBtn = wait.Until(ExpectedConditions.ElementToBeClickable(btnLogoutItem));
                    logoutBtn.Click();
                }

                // Bước chung: Xử lý Alert xác nhận (nếu có)
                try
                {
                    wait.Until(ExpectedConditions.AlertIsPresent());
                    IAlert alert = driver.SwitchTo().Alert();
                    Console.WriteLine("Chấp nhận Alert: " + alert.Text);
                    alert.Accept();
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("Không có Alert xác nhận, tiếp tục...");
                }

                Thread.Sleep(1500);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi logout: " + ex.Message);
                driver.Manage().Cookies.DeleteAllCookies();
                driver.Navigate().GoToUrl("https://digisin-27mb.vercel.app/index.html");
            }
        }
        public string GetValidationMessage()
        {
            try
            {
                // Lấy thông báo lỗi trực tiếp từ thuộc tính validation của HTML5
                string emailMsg = driver.FindElement(txtUsername).GetAttribute("validationMessage");
                string passMsg = driver.FindElement(txtPassword).GetAttribute("validationMessage");

                // Trả về thông báo nào có nội dung (thường là cái đầu tiên bị trống)
                return !string.IsNullOrEmpty(emailMsg) ? emailMsg : passMsg;
            }
            catch { return ""; }
        }
        public string GetResultText()
        {
            try { return driver.PageSource; }
            catch { return ""; }
        }
    }
}