using EnsurFindExperimental.DocumentReaders;
using System;
using System.IO;

namespace Ensur.Core.Utilities.DocumentReaders
{
    public static class DocumentReaderFactory
    {
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
                case ".doc":
                    return new WordDocumentReader(filePath);

                case ".xlsx":
                case ".xls":
                    return new ExcelDocumentReader(filePath);

                case ".pptx":
                case ".ppt":
                    return new PowerPointDocumentReader(filePath);

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