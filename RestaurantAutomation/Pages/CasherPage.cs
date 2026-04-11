using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RestaurantAutomation.Pages
{
    public class CasherPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public CasherPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ===== MENU =====
        private By paymentMenu = By.Id("menu-payment");
        private By historyMenu = By.Id("menu-history");
        private By dashboardMenu = By.Id("menu-dashboard");

        // ===== PAYMENT =====
        private By paymentList = By.XPath("//div[contains(text(),'Hóa đơn')]");
        private By payButton = By.XPath("//button[contains(.,'Thanh toán')]");

        // ===== HISTORY =====
        private By historyList = By.XPath("//div[contains(text(),'Hóa đơn đã thanh toán')]");
        private By viewDetailBtn = By.XPath("//button[contains(.,'Chi tiết')]");

        // ===== DASHBOARD =====
        private By dashboardText = By.XPath("//*[contains(text(),'Tổng doanh thu')]");

        // ===== ACTION =====

        public void GoToPayment()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(paymentMenu)).Click();
            Thread.Sleep(2000);
        }

        public void GoToHistory()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(historyMenu)).Click();
            Thread.Sleep(2000);
        }

        public void GoToDashboard()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(dashboardMenu)).Click();
            Thread.Sleep(2000);
        }

        public void ProcessPayment()
        {
            var firstOrder = driver.FindElements(paymentList)[0];
            var btn = firstOrder.FindElement(payButton);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            Thread.Sleep(3000);
        }

        public void ViewHistoryDetail()
        {
            var first = driver.FindElements(historyList)[0];
            var btn = first.FindElement(viewDetailBtn);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            Thread.Sleep(2000);
        }
    }
}