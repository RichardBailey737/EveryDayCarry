using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Ensur.Core.Utilities.DocumentReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using A = DocumentFormat.OpenXml.Drawing;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class PowerPointDocumentReader : IDocumentReader
    {
        private readonly PresentationDocument _document;
        private List<SlideContent> _slideContents;
        private int _currentIndex;
        private int _totalCharacters;
        private int _charactersProcessed;

        public PowerPointDocumentReader(string filePath)
        {
            _document = PresentationDocument.Open(filePath, false);
            Initialize();
        }

        private void Initialize()
        {
            _slideContents = new List<SlideContent>();
            var presentationPart = _document.PresentationPart;
            var slideIdList = presentationPart.Presentation.SlideIdList;

            if (slideIdList == null)
                return;

            int slideNumber = 0;
            foreach (SlideId slideId in slideIdList)
            {
                slideNumber++;
                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId);
                var slide = slidePart.Slide;

                // Get slide title (usually the first shape with text)
                string slideTitle = GetSlideTitle(slide);

                // Get all text from the slide
                var texts = slide.Descendants<A.Text>().Select(t => t.Text).ToList();
                foreach (var text in texts)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        _slideContents.Add(new SlideContent
                        {
                            Text = text,
                            SlideNumber = slideNumber,
                            SlideTitle = slideTitle
                        });
                    }
                }
            }

            _totalCharacters = _slideContents.Sum(s => s.Text.Length);
            _currentIndex = 0;
            _charactersProcessed = 0;
        }

        private string GetSlideTitle(Slide slide)
        {
            var titleShape = slide.Descendants<Shape>()
                .FirstOrDefault(sp => sp.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.PlaceholderShape?.Type?.Value == PlaceholderValues.Title);

            if (titleShape != null)
            {
                var titleText = titleShape.Descendants<A.Text>().FirstOrDefault();
                if (titleText != null)
                {
                    return titleText.Text;
                }
            }

            return null;
        }

        public DocumentMetadata GetCurrentMetadata()
        {
            if (_currentIndex >= _slideContents.Count)
            {
                return new DocumentMetadata
                {
                    SectionNumber = 0,
                    ElementIndex = _currentIndex,
                    ElementType = "Text Box"
                };
            }

            var currentContent = _slideContents[_currentIndex];
            return new DocumentMetadata
            {
                SectionNumber = currentContent.SlideNumber,
                ElementIndex = _currentIndex,
                LastHeading = currentContent.SlideTitle ?? $"Slide {currentContent.SlideNumber}",
                ElementType = "Text Box"
            };
        }

        public string ReadNext()
        {
            if (_currentIndex >= _slideContents.Count)
            {
                return null;
            }

            var content = _slideContents[_currentIndex];
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

        private class SlideContent
        {
            public string Text { get; set; }
            public int SlideNumber { get; set; }
            public string SlideTitle { get; set; }
        }
    }
}