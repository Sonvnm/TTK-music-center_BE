using Microsoft.AspNetCore.Http;

namespace HMZ.Service.Helpers
{
    public static class HMZCommon
    {
        const string
            JPG = "image/jpeg",
            GIF = "image/gif",
            PNG = "image/png",
            PDF = "application/pdf",
            DOC = "application/msword",
            DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            XLS = "application/vnd.ms-excel",
            XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            PPT = "application/vnd.ms-powerpoint",
            PPTX = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ZIP = "application/zip",
            RAR = "application/x-rar-compressed",
            TXT = "text/plain",
            CSV = "text/csv";


        public static bool CheckImageFile(IFormFile file)
        {
            if (file == null)
            {
                return false;
            }
            // Check if the file is an image
            if (file.ContentType != JPG && file.ContentType != GIF && file.ContentType != PNG)
            {
                return false;
            }
            // check size > 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                return false;
            }
            return true;
        }

        public static bool CheckDocumentFile(IFormFile file, List<string> allowedExtensions = null)
        {
            if (file == null)
            {
                return false;
            }
            allowedExtensions = allowedExtensions ?? new List<string> { PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, ZIP, RAR, TXT, CSV,JPG,PNG, };
            // Check if the file is a document
            if (!allowedExtensions.Contains(file.ContentType))
            {
                return false;
            }
            // check size > 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                return false;
            }
            return true;
        }
    }
}