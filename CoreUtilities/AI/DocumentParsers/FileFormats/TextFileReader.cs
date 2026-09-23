using System;
using System.Collections.Generic;
using System.IO;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class TextFileReader : IDocumentReader
    {
        private readonly string _filePath;
        private List<string> _lines;
        private int _currentIndex;
        private int _totalCharacters;
        private int _charactersProcessed;

        public TextFileReader(string filePath)
        {
            _filePath = filePath;
            Initialize();
        }

        private void Initialize()
        {
            _lines = new List<string>(File.ReadAllLines(_filePath));
            _totalCharacters = 0;
            foreach (var line in _lines)
            {
                _totalCharacters += line.Length;
            }
            _currentIndex = 0;
            _charactersProcessed = 0;
        }

        public DocumentMetadata GetCurrentMetadata()
        {
            return new DocumentMetadata
            {
                SectionNumber = 1,
                ElementIndex = _currentIndex + 1, // Line numbers are 1-based
                LastHeading = Path.GetFileName(_filePath),
                ElementType = "Line"
            };
        }

        public string ReadNext()
        {
            if (_currentIndex >= _lines.Count)
            {
                return null;
            }

            string line = _lines[_currentIndex];
            _charactersProcessed += line.Length;
            _currentIndex++;

            return line;
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
            // Nothing to dispose for text files
        }
    }
}