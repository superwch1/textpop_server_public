using nClam;
using System.Net;

namespace textpop_server.Services.Image
{
    public class ScanImage
    {
        public async Task<bool> ContainVirus(byte[] data)
        {
            //var clam = new ClamClient(IPAddress.Parse("192.168.50.111"), 3310);
            var clam = new ClamClient("localhost", 3310); //port needed to be 3310 for scanning
            var scanResult = await clam.SendAndScanFileAsync(data);

            if (scanResult.Result == ClamScanResults.Clean)
                return false;

            else
                return true;
        }


        public bool IsNotImage(byte[] data)
        {
            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF) //.jpg format
                return false;

            else if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A) //.png format
                return false;

            else if (data[0] == 0x42 && data[1] == 0x4D) //.bmp format
                return false;

            else if (data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70) //.heif format
                return false;

            else if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50) //.webp format
                return false;

            else
                return true;
        }
    }
}
