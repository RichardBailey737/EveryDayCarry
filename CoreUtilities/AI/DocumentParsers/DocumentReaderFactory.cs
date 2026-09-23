using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public static class DocumentReaderFactory
    {
        /// <summary>
        /// Extensions <see cref="CreateReader"/> can open (lower case, with the leading dot).
        /// </summary>
        public static IReadOnlyCollection<string> SupportedExtensions { get; } =
            new[] { ".docx", ".xlsx", ".pptx", ".pdf", ".txt", ".rtf" };

        /// <summary>
        /// True when <paramref name="filePath"/> has an extension <see cref="CreateReader"/> can open.
        /// </summary>
        public static bool IsSupported(string filePath)
        {
            return SupportedExtensions.Contains(Path.GetExtension(filePath ?? String.Empty).ToLowerInvariant());
        }

        public static IDocumentReader CreateReader(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".docx":
                    return new WordDocumentReader(filePath);

                case ".xlsx":
                    return new ExcelDocumentReader(filePath);

                case ".pptx":
                    return new PowerPointDocumentReader(filePath);

                case ".doc":
                case ".xls":
                case ".ppt":
                    // The OpenXml SDK only reads the XML-based Office formats.
                    throw new NotSupportedException(
                        $"Legacy binary Office format '{extension}' is not supported. Save the file as {extension}x.");

                case ".pdf":
                    return new PdfDocumentReader(filePath);

                case ".txt":
                    return new TextFileReader(filePath);

                case ".rtf":
                    return new RichTextFileReader(filePath);

                default:
                    throw new NotSupportedException($"File type '{extension}' is not supported.");
            }
        }
    }
}
