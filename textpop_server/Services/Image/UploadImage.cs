using ImageMagick;

namespace textpop_server.Services.Image
{
    public class UploadImage
    {
        public long BeyondSizeInByteForCompression { get; set; } = 2_000_000;
        public long LargeImageInByte { get; set; } = 6_000_000;
        public long MediumImageInByte { get; set; } = 4_000_000;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadImage(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }


        /// <summary>
        /// Upload the image into server folder and get its information
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Success, ImageUri, ImageSize</returns>
        public async Task<Tuple<bool, string?, long>> UploadImageAndReturnInfo(byte[] imageInByte)
        {
            try
            {
                var magickImage = new MagickImage(imageInByte);

                ConvertImageToJpg(ref magickImage);
                CompressImage(ref magickImage, imageInByte.Length);

                // Create a new MemoryStream
                using (var stream = new MemoryStream())
                {
                    await magickImage.WriteAsync(stream);

                    var imageUri = await SaveImageToMessageFolder(stream);
                    return Tuple.Create<bool, string?, long>(true, imageUri, stream.Length);
                }
            }
            catch
            {
                return Tuple.Create<bool, string?, long>(false, null, 0);
            }
        }


        /// <summary>
        /// Upload the avatar into server folder and get its information
        /// </summary>
        /// <param name="image"></param>
        /// <param name="userId"></param>
        /// <returns>Success, ImageUri, ImageSize</returns>
        public async Task<Tuple<bool, string?, long>> UploadResizeAvatarAndReturnInfo(byte[] imageInByte, string userId)
        {
            try
            {
                var magickImage = new MagickImage(imageInByte);

                ConvertImageToJpg(ref magickImage);
                CompressImage(ref magickImage, imageInByte.Length);
                ResizeAndExtentImage(ref magickImage, 300, 300);

                // Create a new MemoryStream
                using (var stream = new MemoryStream())
                {
                    await magickImage.WriteAsync(stream);

                    var imageUri = await SaveImageToAccountFolder(stream, userId);
                    return Tuple.Create<bool, string?, long>(true, imageUri, stream.Length);
                }
            }
            catch
            {
                return Tuple.Create<bool, string?, long>(false, null, 0);
            }
        }


        /// <summary>
        /// Convert the MagickImage object into jpg format
        /// </summary>
        /// <param name="magickImage"></param>
        public void ConvertImageToJpg(ref MagickImage magickImage)
        {
            magickImage.Format = MagickFormat.Jpg;
        }


        /// <summary>
        /// Compress the MagickImage object when the image exceed corresponding size
        /// </summary>
        /// <param name="magickImage"></param>
        /// <param name="imageSizeInByte"></param>
        public void CompressImage(ref MagickImage magickImage, long imageSizeInByte)
        {
            if (imageSizeInByte > BeyondSizeInByteForCompression)
            {
                if (imageSizeInByte > LargeImageInByte)
                {
                    magickImage.Quality = 20;
                }
                else if (imageSizeInByte > MediumImageInByte)
                {
                    magickImage.Quality = 40;
                }
                else
                {
                    magickImage.Quality = 60;
                }
            }
        }


        /// <summary>
        /// Resize the MagickImage object to corresponding width and height
        /// </summary>
        /// <param name="magickImage"></param>
        /// <param name="imageDesiredWidth"></param>
        /// <param name="imageDesiredHeight"></param>
        public void ResizeAndExtentImage(ref MagickImage magickImage, int imageDesiredWidth, int imageDesiredHeight)
        {
            magickImage.Resize(imageDesiredWidth, imageDesiredHeight);

            int newWidth = magickImage.Width < imageDesiredWidth ? imageDesiredWidth : magickImage.Width;
            int newHeight = magickImage.Height < imageDesiredHeight ? imageDesiredHeight : magickImage.Height;
            magickImage.Extent(newWidth, newHeight, Gravity.Center, MagickColors.Black);
        }


        /// <summary>
        /// Save the image to message folder
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>imageUri</returns>
        public async Task<string> SaveImageToMessageFolder(MemoryStream stream)
        {
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Chat", $"{DateTime.Today.ToString("dd_MM_yyyy")}");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var imageFiles = Directory.GetFiles(folderPath);
            var numberOfImage = imageFiles.Count() + 1;

            var imageUri = Path.Combine(folderPath, $"{numberOfImage}.jpg");

            using (FileStream fileStream = new FileStream(Path.Combine(folderPath, imageUri), FileMode.Create, FileAccess.Write))
            {
                stream.Position = 0;
                await stream.CopyToAsync(fileStream);
            }

            return Path.Combine($"{DateTime.Today.ToString("dd_MM_yyyy")}", $"{numberOfImage}.jpg");
        }


        /// <summary>
        /// Save the image to account folder
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="userId"></param>
        /// <returns>imageUri</returns>
        public async Task<string> SaveImageToAccountFolder(MemoryStream stream, string userId)
        {
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Account");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var imageUri = Path.Combine(folderPath, $"{userId}.jpg");

            using (FileStream fileStream = new FileStream(Path.Combine(folderPath, imageUri), FileMode.Create, FileAccess.Write))
            {
                stream.Position = 0;
                await stream.CopyToAsync(fileStream);
            }

            return Path.Combine($"{userId}.jpg");
        }


        /// <summary>
        /// Create a defaul image for the new user
        /// </summary>
        /// <param name="userId"></param>
        public void SaveDefaultImageForNewUser(string userId)
        {
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Account");
            var defaultImageUri = Path.Combine(folderPath, $"default.jpg");
            var imageUri = Path.Combine(folderPath, $"{userId}.jpg");

            File.Copy(defaultImageUri, imageUri, true);
        }
    }
}
