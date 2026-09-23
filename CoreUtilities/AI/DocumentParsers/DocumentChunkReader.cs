using System;
using System.Collections.Generic;
using System.Linq;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public class DocumentChunkReader : IDisposable
    {
        private readonly IDocumentReader _documentReader;
        private readonly int _overlapWordCount;
        private readonly Queue<string> _pendingWords = new Queue<string>();
        private List<string> _overlapWords = new List<string>();
        private DocumentLocation _pendingStartLocation;
        private bool _endOfDocument;
        private int _totalContentSize;

        public DocumentChunkReader(IDocumentReader documentReader)
            : this(documentReader, 15)
        {
        }

        public DocumentChunkReader(IDocumentReader documentReader, int overlapWordCount)
        {
            if (documentReader == null)
            {
                throw new ArgumentNullException(nameof(documentReader));
            }

            if (overlapWordCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(overlapWordCount), "Overlap word count cannot be negative.");
            }

            _documentReader = documentReader;
            _overlapWordCount = overlapWordCount;
            _totalContentSize = _documentReader.GetTotalContentSize();
        }

        public DocumentChunk ReadNextChunk(int wordCount)
        {
            if (wordCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(wordCount), "Word count must be greater than zero.");
            }

            if (_overlapWordCount >= wordCount)
            {
                throw new ArgumentException("Overlap word count must be smaller than the chunk word count.", nameof(wordCount));
            }

            if (_endOfDocument && _pendingWords.Count == 0)
            {
                return null;
            }

            var currentWords = new List<string>(wordCount);
            DocumentLocation startLocation = null;

            if (_overlapWords.Count > 0)
            {
                currentWords.AddRange(_overlapWords);
                startLocation = CloneLocation(_pendingStartLocation);
            }

            while (currentWords.Count < wordCount)
            {
                if (_pendingWords.Count > 0)
                {
                    if (startLocation == null)
                    {
                        startLocation = CloneLocation(_pendingStartLocation);
                    }

                    currentWords.Add(_pendingWords.Dequeue());
                    continue;
                }

                if (_endOfDocument)
                {
                    break;
                }

                string content = _documentReader.ReadNext();
                if (content == null)
                {
                    _endOfDocument = true;
                    break;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                var metadata = _documentReader.GetCurrentMetadata();
                var contentLocation = CreateLocation(metadata);
                var words = content.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length == 0)
                {
                    continue;
                }

                if (_pendingStartLocation == null)
                {
                    _pendingStartLocation = CloneLocation(contentLocation);
                }

                foreach (var word in words)
                {
                    _pendingWords.Enqueue(word);
                }
            }

            if (currentWords.Count == 0 || (_endOfDocument && currentWords.Count == _overlapWords.Count && _pendingWords.Count == 0))
            {
                return null;
            }

            if (startLocation == null)
            {
                startLocation = CloneLocation(_pendingStartLocation);
            }

            var endLocation = CreateLocation(_documentReader.GetCurrentMetadata());
            _overlapWords = currentWords
                .Skip(Math.Max(0, currentWords.Count - _overlapWordCount))
                .ToList();
            _pendingStartLocation = CloneLocation(endLocation);

            return new DocumentChunk
            {
                Words = currentWords.ToArray(),
                StartLocation = startLocation,
                EndLocation = endLocation
            };
        }

        private DocumentLocation CreateLocation(DocumentMetadata metadata)
        {
            return new DocumentLocation
            {
                SectionNumber = metadata.SectionNumber,
                PercentComplete = CalculatePercentage(_documentReader.GetCurrentPosition(), _totalContentSize),
                ElementIndex = metadata.ElementIndex,
                LastHeading = metadata.LastHeading,
                ElementType = metadata.ElementType
            };
        }

        private static DocumentLocation CloneLocation(DocumentLocation location)
        {
            if (location == null)
            {
                return null;
            }

            return new DocumentLocation
            {
                SectionNumber = location.SectionNumber,
                PercentComplete = location.PercentComplete,
                ElementIndex = location.ElementIndex,
                LastHeading = location.LastHeading,
                ElementType = location.ElementType
            };
        }

        private double CalculatePercentage(int current, int total)
        {
            if (total == 0)
                return 0;

            return Math.Round((double)current / total * 100, 1);
        }

        public void Dispose()
        {
            _documentReader?.Dispose();
        }
    }

    public class DocumentChunk
    {
        public string[] Words { get; set; }
        public DocumentLocation StartLocation { get; set; }
        public DocumentLocation EndLocation { get; set; }

        public string GetText()
        {
            return string.Join(" ", Words);
        }

        public string GetLocationDescription()
        {
            var parts = new List<string>();

            if (StartLocation.LastHeading != null)
            {
                parts.Add($"Section: \"{StartLocation.LastHeading}\"");
            }
            else if (StartLocation.SectionNumber > 0)
            {
                parts.Add($"Section {StartLocation.SectionNumber}");
            }

            if (Math.Abs(StartLocation.PercentComplete - EndLocation.PercentComplete) < 0.1)
            {
                parts.Add($"{StartLocation.PercentComplete}% through document");
            }
            else
            {
                parts.Add($"{StartLocation.PercentComplete}%-{EndLocation.PercentComplete}% through document");
            }

            string elementLabel = StartLocation.ElementType ?? "Elements";
            parts.Add($"{elementLabel} {StartLocation.ElementIndex}-{EndLocation.ElementIndex}");

            return string.Join(", ", parts);
        }
    }

    public class DocumentLocation
    {
        public int SectionNumber { get; set; }
        public double PercentComplete { get; set; }
        public int ElementIndex { get; set; }
        public string LastHeading { get; set; }
        public string ElementType { get; set; }
    }
}