using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class PdfDocumentReader : IDocumentReader
    {
        private readonly PdfDocument _document;
        private List<PageContent> _pageContents;
        private int _currentIndex;
        private int _totalCharacters;
        private int _charactersProcessed;

        public PdfDocumentReader(string filePath)
        {
            _document = PdfDocument.Open(filePath);
            Initialize();
        }

        private void Initialize()
        {
            _pageContents = new List<PageContent>();

            foreach (var page in _document.GetPages())
            {
                string pageText = ExtractPageText(page);

                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    // Split page into paragraphs (by double line breaks or single line breaks)
                    var paragraphs = pageText.Split(new[] { "\r\n\r\n", "\n\n", "\r\n", "\n", "\r" },
                        StringSplitOptions.RemoveEmptyEntries);

                    foreach (var paragraph in paragraphs)
                    {
                        string cleanedParagraph = paragraph.Trim();
                        if (!string.IsNullOrWhiteSpace(cleanedParagraph))
                        {
                            _pageContents.Add(new PageContent
                            {
                                Text = cleanedParagraph,
                                PageNumber = page.Number,
                                TotalPages = _document.NumberOfPages
                            });
                        }
                    }
                }
            }

            _totalCharacters = _pageContents.Sum(p => p.Text.Length);
            _currentIndex = 0;
            _charactersProcessed = 0;
        }

        /// <summary>
        /// Total number of pages in the PDF, including pages without extractable text.
        /// </summary>
        public int PageCount => _document.NumberOfPages;

        // A letter, a hyphen at the end of a line, then a lower-case letter on the next line:
        // a word split across lines by typesetting ("con-" + "ditions" becomes "conditions").
        private static readonly Regex LineBreakHyphen = new Regex(@"(\p{L})-[ \t]*\r?\n[ \t]*(\p{Ll})", RegexOptions.Compiled);

        /// <summary>
        /// Extracts page text in reading order. <see cref="Page.Text"/> concatenates letters in
        /// content-stream order without line breaks, which merges columns and loses line structure.
        /// </summary>
        private static string ExtractPageText(Page page)
        {
            string text;
            try
            {
                text = ContentOrderTextExtractor.GetText(page);
            }
            catch (Exception)
            {
                text = page.Text;
            }

            return String.IsNullOrEmpty(text) ? text : LineBreakHyphen.Replace(text, "$1$2");
        }

        public DocumentMetadata GetCurrentMetadata()
        {
            // Describe the paragraph most recently returned by ReadNext (as the other readers do),
            // not the next unread one, so chunk page ranges are not shifted by a page.
            int index = Math.Min(Math.Max(_currentIndex - 1, 0), _pageContents.Count - 1);
            if (index < 0)
            {
                return new DocumentMetadata
                {
                    SectionNumber = 0,
                    ElementIndex = _currentIndex,
                    ElementType = "Paragraph"
                };
            }

            var currentContent = _pageContents[index];
            return new DocumentMetadata
            {
                SectionNumber = currentContent.PageNumber,
                ElementIndex = _currentIndex,
                LastHeading = $"Page {currentContent.PageNumber} of {currentContent.TotalPages}",
                ElementType = "Paragraph"
            };
        }

        public string ReadNext()
        {
            if (_currentIndex >= _pageContents.Count)
            {
                return null;
            }

            var content = _pageContents[_currentIndex];
            _charactersProcessed += content.Text.Length;
            _currentIndex++;

            return content.Text;
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

        private class PageContent
        {
            public string Text { get; set; }
            public int PageNumber { get; set; }
            public int TotalPages { get; set; }
        }
    }
}