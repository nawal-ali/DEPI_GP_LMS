namespace LMSProject.Areas.Admin.Helpers
{
    public class Upload
    {
        public static string UploadImage(string folder, IFormFile file)
        {

            folder += DateTime.Now.ToBinary() + "_" + file.FileName;
            string fullpath = Path.Combine("wwwroot", folder);
            var stream = new FileStream(fullpath, FileMode.Create);
            file.CopyTo(stream);
            return folder;
        }
        public static bool DeletImage(string ImageName)
        {

           
            string fullpath = Path.Combine("wwwroot", ImageName);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
                return true;
            }

            return false;
        }
    }
}
