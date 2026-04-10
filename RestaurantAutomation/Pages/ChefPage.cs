using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RestaurantAutomation.Pages
{
    public class ChefPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public ChefPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ===== TAB =====
        private By tabPending = By.Id("pending-tab");
        private By tabPreparing = By.Id("preparing-tab");

        // ===== ORDER =====
        private By orders = By.XPath("//div[contains(text(),'Đơn hàng #')]");

        // ===== ACTION =====
        public void GoToPending()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(tabPending)).Click();
            Thread.Sleep(2000);
        }

        public void GoToPreparing()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(tabPreparing)).Click();
            Thread.Sleep(2000);
        }

        // ===== BƯỚC 1: BẮT ĐẦU CHẾ BIẾN ĐƠN =====
        public void StartProcessingOrder()
        {
            var firstOrder = driver.FindElements(orders)[0];

            var btn = firstOrder.FindElement(By.XPath(".//button[contains(.,'Bắt đầu chế biến')]"));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            Thread.Sleep(2000);
        }

        // ===== BƯỚC 2: BẮT ĐẦU TẤT CẢ MÓN =====
        public void StartAllDishes()
        {
            var firstOrder = driver.FindElements(orders)[0];

            var startButtons = firstOrder.FindElements(By.XPath(".//button[contains(.,'Bắt đầu')]"));

            foreach (var btn in startButtons)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
                Thread.Sleep(500);
            }
        }

        // ===== BƯỚC 3: HOÀN THÀNH ĐƠN =====
        public void FinishOrder()
        {
            var firstOrder = driver.FindElements(orders)[0];

            var btn = firstOrder.FindElement(By.XPath(".//button[contains(.,'Hoàn thành đơn hàng')]"));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            Thread.Sleep(2000);
        }
    }
}