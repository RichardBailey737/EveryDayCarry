using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class RichTextFileReader : IDocumentReader
    {
        private readonly string _filePath;
        private List<string> _paragraphs;
        private int _currentIndex;
        private int _totalCharacters;
        private int _charactersProcessed;

        public RichTextFileReader(string filePath)
        {
            _filePath = filePath;
            Initialize();
        }

        private void Initialize()
        {
            string rtfContent = File.ReadAllText(_filePath);
            _paragraphs = new List<string>();

            // Strip RTF formatting and extract plain text
            string plainText = StripRtfFormatting(rtfContent);

            // Split into paragraphs
            var paragraphArray = plainText.Split(new[] { "\r\n", "\r", "\n", "\\par" },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var para in paragraphArray)
            {
                string cleaned = para.Trim();
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    _paragraphs.Add(cleaned);
                }
            }

            _totalCharacters = 0;
            foreach (var para in _paragraphs)
            {
                _totalCharacters += para.Length;
            }

            _currentIndex = 0;
            _charactersProcessed = 0;
        }

        private string StripRtfFormatting(string rtf)
        {
            if (string.IsNullOrEmpty(rtf))
                return string.Empty;

            // Remove RTF control words and groups
            var sb = new StringBuilder();
            bool inControlWord = false;
            bool inGroup = false;
            int groupDepth = 0;

            for (int i = 0; i < rtf.Length; i++)
            {
                char c = rtf[i];

                if (c == '{')
                {
                    groupDepth++;
                    inGroup = true;
                }
                else if (c == '}')
                {
                    groupDepth--;
                    if (groupDepth == 0)
                        inGroup = false;
                }
                else if (c == '\\')
                {
                    inControlWord = true;
                    // Check for escaped characters
                    if (i + 1 < rtf.Length)
                    {
                        char next = rtf[i + 1];
                        if (next == '\\' || next == '{' || next == '}')
                        {
                            sb.Append(next);
                            i++;
                            inControlWord = false;
                        }
                    }
                }
                else if (inControlWord && (c == ' ' || c == '\r' || c == '\n'))
                {
                    inControlWord = false;
                }
                else if (!inControlWord && groupDepth <= 1)
                {
                    sb.Append(c);
                }
            }

            // Clean up the result
            string result = sb.ToString();
            result = Regex.Replace(result, @"\\[a-z]+\d*\s?", ""); // Remove remaining control words
            result = Regex.Replace(result, @"\s+", " "); // Normalize whitespace

            return result.Trim();
        }

        public DocumentMetadata GetCurrentMetadata()
        {
            return new DocumentMetadata
            {
                SectionNumber = 1,
                ElementIndex = _currentIndex + 1,
                LastHeading = Path.GetFileName(_filePath),
                ElementType = "Paragraph"
            };
        }

        public string ReadNext()
        {
            if (_currentIndex >= _paragraphs.Count)
            {
                return null;
            }

            string paragraph = _paragraphs[_currentIndex];
            _charactersProcessed += paragraph.Length;
            _currentIndex++;

            return paragraph;
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
            // Nothing to dispose for RTF files
        }
    }
}