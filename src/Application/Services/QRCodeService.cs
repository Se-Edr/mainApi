using QRCoder;

namespace Application.Services
{
    public class QRCodeService
    {

        public async Task<string> CreteQRCode(Guid CarId)
        {
            using (QRCodeGenerator generator = new QRCodeGenerator())
            {
                QRCodeData data = generator.CreateQrCode(CarId.ToString(),QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode=new PngByteQRCode(data))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(10);
                    string filename = $"{CarId}_car.png";

                    string photobase64=Convert.ToBase64String(qrCodeImage);

                    return photobase64;
                }
            }

        }
    }
}
