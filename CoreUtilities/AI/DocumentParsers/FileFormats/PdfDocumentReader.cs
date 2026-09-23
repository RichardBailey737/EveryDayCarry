using Ensur.Core.Utilities.DocumentReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace EnsurFindExperimental.DocumentReaders
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
                // Get text from the page
                string pageText = page.Text;

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

        public DocumentMetadata GetCurrentMetadata()
        {
            if (_currentIndex >= _pageContents.Count)
            {
                return new DocumentMetadata
                {
                    SectionNumber = 0,
                    ElementIndex = _currentIndex,
                    ElementType = "Paragraph"
                };
            }

            var currentContent = _pageContents[_currentIndex];
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