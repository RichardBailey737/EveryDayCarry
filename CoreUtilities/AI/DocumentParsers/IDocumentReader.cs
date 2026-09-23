using System;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public interface IDocumentReader : IDisposable
    {
        /// <summary>
        /// Gets metadata about the current paragraph/element being processed
        /// </summary>
        DocumentMetadata GetCurrentMetadata();

        /// <summary>
        /// Moves to the next paragraph/element and returns its text content
        /// </summary>
        /// <returns>Text content or null if end of document reached</returns>
        string ReadNext();

        /// <summary>
        /// Gets the total size of the document for percentage calculations
        /// </summary>
        int GetTotalContentSize();

        /// <summary>
        /// Gets the current position in the document
        /// </summary>
        int GetCurrentPosition();

        /// <summary>
        /// Resets the reader to the beginning of the document
        /// </summary>
        void Reset();
    }

    public class DocumentMetadata
    {
        public int SectionNumber { get; set; }
        public int ElementIndex { get; set; }
        public string LastHeading { get; set; }
        public string ElementType { get; set; } // e.g., "Paragraph", "Row", "Slide", "Line"
    }
}