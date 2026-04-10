using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Drawing;

namespace RestaurantAutomation.Utilities
{
    public static class ExcelHelper
    {
        // Thêm tham số sheetName vào hàm
        public static void UpdateExcel(string filePath, string testCaseID, string actualResult, string status, string sheetName, string imagePath = "")
        {
            ExcelPackage.License.SetNonCommercialPersonal("Thuan");
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) return;

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                // Lấy Sheet dựa trên tên được truyền từ JSON, nếu không thấy thì lấy sheet đầu tiên
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetName] ?? package.Workbook.Worksheets[0];

                int totalRows = 500;
                bool isFound = false;

                for (int row = 1; row <= totalRows; row++)
                {
                    string cellValue = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";

                    if (cellValue.Equals(testCaseID.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        // CỘT J (10): Note/Link ảnh
                        var noteCell = worksheet.Cells[row, 10];
                        noteCell.Value = imagePath;
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            noteCell.Style.Font.Color.SetColor(Color.Blue);
                            noteCell.Style.Font.UnderLine = true;
                        }

                        // CỘT K (11): Status
                        var statusCell = worksheet.Cells[row, 11];
                        statusCell.Value = status;
                        statusCell.Style.Font.Bold = true;
                        statusCell.Style.Font.Color.SetColor(status.ToLower().Contains("pass") ? Color.Green : Color.Red);

                        // CỘT M (13): Kết quả thực tế
                        worksheet.Cells[row, 13].Value = actualResult;

                        var range = worksheet.Cells[row, 10, row, 13];
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.WrapText = true;

                        isFound = true;
                        Console.WriteLine($"===> Đã ghi vào Sheet [{sheetName}] cho ID: {testCaseID}");
                        break;
                    }
                }
                package.Save();
            }
        }
    }
}