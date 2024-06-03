using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace HMZ.SDK.Excel
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExcelExportBase<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// 
        public Task<ExportResult> ExportToExcel(IEnumerable<T> data, string template)
        {
            string pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Resources", template);
            string pathExport = string.Empty;
            try
            {
                string filename = Path.GetFileNameWithoutExtension(template) + $"_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                var properties = typeof(T).GetProperties();


                if (!File.Exists(pathTemplate))
                {
                    return Task.FromResult(new ExportResult
                    {
                        ErrorMessage = $"Template file '{template}' not found."
                    });
                }
                // Read the template file
                using (var stream = new FileStream(pathTemplate, FileMode.Open, FileAccess.Read))
                {
                    if (stream.Length == 0)
                    {
                        return Task.FromResult(new ExportResult
                        {
                            ErrorMessage = $"Template file '{template}' is empty."
                        });
                    }
                    var workbook = new XSSFWorkbook(stream);
                    var sheet = workbook.GetSheetAt(0);

                    // Find the row that contains the property names
                    var headerRow = sheet.GetRow(1);

                    // Loop over the data and write it to the sheet
                    int rowIndex = 2; // start at row 3
                    foreach (var item in data)
                    {
                        var row = sheet.CreateRow(rowIndex++);
                        for (int i = 0; i < headerRow.LastCellNum; i++)
                        {
                            var headerCell = headerRow.GetCell(i);

                            // Find the corresponding property
                            var propName = headerCell.StringCellValue.Trim();
                            var prop = properties.FirstOrDefault(p => p.Name == propName);
                            if (prop == null) continue;

                            // Write the property value to the cell
                            var cell = row.CreateCell(i);
                            // set border
                            cell.CellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                            cell.CellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                            cell.CellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                            cell.CellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                            cell.CellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                            // auto fit column width to content
                            sheet.AutoSizeColumn(i);
                            sheet.SetAutoFilter(new CellRangeAddress(1, rowIndex, 0, headerRow.LastCellNum - 1));
                            sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + 512);

                            var value = prop.GetValue(item);
                            if (value == null)
                            {
                                cell.SetCellValue("");
                            }
                            else if (value is DateTime)
                            {
                                cell.SetCellValue(((DateTime)value).ToString("dd/MM/yyyy"));

                            }
                            else if (value is bool)
                            {
                                cell.SetCellValue((bool)value ? 1 : 0);

                            }
                            else
                            {
                                cell.SetCellValue(value.ToString());

                            }
                        }
                    }
                    // Save the workbook to a file temp 
                    pathExport = Path.Combine(Directory.GetCurrentDirectory(), "ExportData", filename);
                    if (!Directory.Exists(Path.GetDirectoryName(pathExport)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(pathExport));
                    }
                    // Write the file to disk
                    using (var fileStream = new FileStream(pathExport, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fileStream);
                    }

                }

                return Task.FromResult(new ExportResult()
                {
                    FileName = filename,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Content = File.ReadAllBytes(pathExport)
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ExportResult
                {
                    ErrorMessage = ex.Message
                });
            }
            finally
            {
                // Delete the file
                if (File.Exists(pathExport))
                    File.Delete(pathExport);
            }
        }
    }
}
