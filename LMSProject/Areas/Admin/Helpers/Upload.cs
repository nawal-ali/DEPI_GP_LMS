namespace LMSProject.Areas.Admin.Helpers
{
    public class Upload
    {
        public static string UploadImage(string folder, IFormFile file)
        {
            folder += DateTime.Now.ToBinary() + "_" + file.FileName;
            string fullPath = Path.Combine("wwwroot", folder);

            // using statement ensures the stream is always disposed,
            // which closes the file handle and prevents the "file in use" IOException
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return folder;
        }

        public static bool DeletImage(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) return false;

            string fullPath = Path.Combine("wwwroot", imageName);

            if (!File.Exists(fullPath)) return false;

            try
            {
                File.Delete(fullPath);
                return true;
            }
            catch (IOException)
            {
                // File still in use — skip silently rather than crashing
                return false;
            }
        }
    }
}