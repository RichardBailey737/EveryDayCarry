using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ensur.Core.Utilities.DocumentReaders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class ExcelDocumentReader : IDocumentReader
    {
        private readonly SpreadsheetDocument _document;
        private List<CellData> _cells;
        private int _currentIndex;
        private int _totalCharacters;
        private int _charactersProcessed;
        private string _currentSheetName;

        public ExcelDocumentReader(string filePath)
        {
            _document = SpreadsheetDocument.Open(filePath, false);
            Initialize();
        }

        private void Initialize()
        {
            _cells = new List<CellData>();
            var workbookPart = _document.WorkbookPart;
            var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

            int sheetNumber = 0;
            foreach (var sheet in sheets)
            {
                sheetNumber++;
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                string sheetName = sheet.Name;

                foreach (var row in sheetData.Elements<Row>())
                {
                    foreach (var cell in row.Elements<Cell>())
                    {
                        string cellValue = GetCellValue(cell);
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            _cells.Add(new CellData
                            {
                                Value = cellValue,
                                SheetNumber = sheetNumber,
                                SheetName = sheetName,
                                CellReference = cell.CellReference?.Value
                            });
                        }
                    }
                }
            }

            _totalCharacters = _cells.Sum(c => c.Value.Length);
            _currentIndex = 0;
            _charactersProcessed = 0;
        }

        private string GetCellValue(Cell cell)
        {
            if (cell.CellValue == null)
                return string.Empty;

            string value = cell.CellValue.InnerText;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var stringTable = _document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                value = stringTable.ElementAt(int.Parse(value)).InnerText;
            }

            return value;
        }

        public DocumentMetadata GetCurrentMetadata()
        {
            if (_currentIndex >= _cells.Count)
            {
                return new DocumentMetadata
                {
                    SectionNumber = 0,
                    ElementIndex = _currentIndex,
                    ElementType = "Cell"
                };
            }

            var currentCell = _cells[_currentIndex];
            return new DocumentMetadata
            {
                SectionNumber = currentCell.SheetNumber,
                ElementIndex = _currentIndex,
                LastHeading = $"{currentCell.SheetName} ({currentCell.CellReference})",
                ElementType = "Cell"
            };
        }

        public string ReadNext()
        {
            if (_currentIndex >= _cells.Count)
            {
                return null;
            }

            var cell = _cells[_currentIndex];
            _charactersProcessed += cell.Value.Length;
            _currentIndex++;

            return cell.Value;
        }

        public int GetTotalContentSize()
        {
            return _totalCharacters;
        }

        public int GetCurrentPosition()
        {
            return _charactersProcessed;
        }

        public void Reset()
        {
            _currentIndex = 0;
            _charactersProcessed = 0;
        }

        public void Dispose()
        {
            _document?.Dispose();
        }

        private class CellData
        {
            public string Value { get; set; }
            public int SheetNumber { get; set; }
            public string SheetName { get; set; }
            public string CellReference { get; set; }
        }
    }
}