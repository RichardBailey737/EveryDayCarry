using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Ensur.Core.Utilities.DocumentReaders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class WordDocumentReader : IDocumentReader
    {
        private readonly WordprocessingDocument _document;
        private List<Paragraph> _paragraphs;
        private int _currentIndex;
        private int _totalCharacters;
        private int _charactersProcessed;
        private int _currentSection;
        private string _lastHeading;

        public WordDocumentReader(string filePath)
        {
            _document = WordprocessingDocument.Open(filePath, false);
            Initialize();
        }

        private void Initialize()
        {
            var body = _document.MainDocumentPart.Document.Body;
            _paragraphs = body.Descendants<Paragraph>().ToList();
            _totalCharacters = _paragraphs.Sum(p => p.InnerText.Length);
            _currentIndex = 0;
            _charactersProcessed = 0;
            _currentSection = 1;
            _lastHeading = null;
        }

        public DocumentMetadata GetCurrentMetadata()
        {
            return new DocumentMetadata
            {
                SectionNumber = _currentSection,
                ElementIndex = _currentIndex,
                LastHeading = _lastHeading,
                ElementType = "Paragraph"
            };
        }

        public string ReadNext()
        {
            if (_currentIndex >= _paragraphs.Count)
            {
                return null;
            }

            var paragraph = _paragraphs[_currentIndex];

            // Check for section breaks
            var sectionProps = paragraph.ParagraphProperties?.SectionProperties;
            if (sectionProps != null && _currentIndex > 0)
            {
                _currentSection++;
            }

            // Check if this is a heading
            string headingText = GetHeadingText(paragraph);
            if (headingText != null)
            {
                _lastHeading = headingText;
            }

            string text = paragraph.InnerText;
            _charactersProcessed += text.Length;
            _currentIndex++;

            return text;
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
            _currentSection = 1;
            _lastHeading = null;
        }

        private string GetHeadingText(Paragraph paragraph)
        {
            var paragraphProperties = paragraph.ParagraphProperties;
            if (paragraphProperties == null)
                return null;

            var style = paragraphProperties.ParagraphStyleId;
            if (style == null)
                return null;

            string styleId = style.Val?.Value;
            if (styleId != null && (styleId.StartsWith("Heading") || styleId.StartsWith("Title")))
            {
                return paragraph.InnerText?.Trim();
            }

            return null;
        }

        public void Dispose()
        {
            _document?.Dispose();
        }
    }
}