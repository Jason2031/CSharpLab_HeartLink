using System.Text;
using PCLCrypto;

namespace CSharpLab_HeartLink
{
    public class Encryption
    {
        private static string Key = "BUPTSSEJasonWood";

        public static string Encrypt(string plainText)
        {
            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var keyMaterial = provider.CreateSymmetricKey(Encoding.UTF8.GetBytes(Key));
            byte[] cipher = WinRTCrypto.CryptographicEngine.Encrypt(keyMaterial, Encoding.UTF8.GetBytes(plainText), null);
            return Encoding.UTF8.GetString(cipher, 0, cipher.Length);
        }

        public static string Decrypt(string cipherText)
        {
            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var keyMaterial = provider.CreateSymmetricKey(Encoding.UTF8.GetBytes(Key));
            byte[] plain = WinRTCrypto.CryptographicEngine.Decrypt(keyMaterial, Encoding.UTF8.GetBytes(cipherText), null);
            return Encoding.UTF8.GetString(plain, 0, plain.Length);
        }
    }
}
