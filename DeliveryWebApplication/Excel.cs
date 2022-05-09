using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryWebApplication
{
    public class ExcelReader : IDisposable
    {
        private XLWorkbook _workbook;
        private IXLWorksheet _worksheet;

        public ExcelReader(IFormFile fileExcel)
        {
            if (fileExcel == null)
                throw new ArgumentNullException(nameof(fileExcel));
            using var stream = new FileStream(fileExcel.FileName, FileMode.Create);
            fileExcel.CopyToAsync(stream).Wait();
            _workbook = new(stream, XLEventTracking.Disabled);
            if (_workbook.Worksheets.Count != 1)
            {
                _workbook.Dispose();
                throw new InvalidDataException("Файл Excel має містити рівно один аркуш");
            }
            _worksheet = _workbook.Worksheets.First();
        }

        public IXLRows Rows() => _worksheet.RowsUsed();

        public void Dispose()
        {
            if (_workbook is not null)
            {
                _workbook.Dispose();
                _workbook = null;
                _worksheet = null;
            }
        }
    }

    public class ExcelWriter : IDisposable
    {
        private XLWorkbook _workbook = new(XLEventTracking.Disabled);
        private IXLWorksheet _worksheet;
        private int _row = 1;

        public ExcelWriter() => _worksheet = _workbook.Worksheets.Add("Аркуш");

        public void AddRow(params IEnumerable<object>[] arr)
        {
            int col = 1;
            foreach(var cells in arr)
                foreach (object cell in cells)
                    _worksheet.Cell(_row, col++).Value = cell;
            _row++;
        }

        public FileContentResult Save(string name)
        {
            _worksheet.Row(1).Style.Font.Bold = true;
            using var stream = new MemoryStream();
            _workbook.SaveAs(stream);
            stream.Flush();

            return new(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{name}, збережено {DateTime.Now}.xlsx"
            };
        }

        public void Dispose()
        {
            if (_workbook is not null)
            {
                _workbook.Dispose();
                _workbook = null;
                _worksheet = null;
            }
        }
    }
}
