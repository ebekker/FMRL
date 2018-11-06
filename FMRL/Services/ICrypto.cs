using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FMRL.Services
{
    public interface ICrypto
    {
        string Base64Encode(byte[] decoded);

        byte[] Base64Decode(string encoded);

        byte[] GenerateRandom(int length);

        byte[] ComputeHash(byte[] data);

        (byte[] key, byte[] iv, byte[] encoded) Encrypt(byte[] decoded);

        byte[] Decrypt(byte[] key, byte[] iv, byte[] encoded);

        (byte[] salt, byte[] iv, byte[] encoded) Encrypt(string password, byte[] decoded);

        byte[] Decrypt(string password, byte[] salt, byte[] iv, byte[] encoded);
    }

    public class BclCrypt : ICrypto
    {
        public const int SymmetricKeyLength = 128;

        public const int PbkSaltLength = 24;
        public const int PbkIterations = 1000;
        public const int PbkSymmetricKeyLength = 256;

        public string Base64Encode(byte[] decoded) =>
            Convert.ToBase64String(decoded);

        public byte[] Base64Decode(string encoded) =>
            Convert.FromBase64String(encoded);

        public byte[] GenerateRandom(int length)
        {
            var data = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }
            return data;
        }

        public byte[] ComputeHash(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                return sha.TransformFinalBlock(data, 0, data.Length);
            }
        }

        public (byte[] key, byte[] iv, byte[] encoded) Encrypt(byte[] decoded)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = SymmetricKeyLength;
                aes.GenerateIV();
                aes.GenerateKey();

                using (var enc = aes.CreateEncryptor())
                {
                    var encoded = enc.TransformFinalBlock(decoded, 0, decoded.Length);
                    return (aes.Key, aes.IV, encoded);
                }
            }
        }

        public byte[] Decrypt(byte[] key, byte[] iv, byte[] encoded)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = SymmetricKeyLength;
                aes.Key = key;
                aes.IV = iv;
                using (var dec = aes.CreateDecryptor())
                {
                    return dec.TransformFinalBlock(encoded, 0, encoded.Length);
                }
            }
        }

        public (byte[] salt, byte[] iv, byte[] encoded) Encrypt(string password, byte[] decoded)
        {
            byte[] pbk;
            byte[] pbkSalt;
            using (var pbkdf = new Rfc2898DeriveBytes(password, PbkSaltLength, PbkIterations))
            {
                pbk = pbkdf.GetBytes(PbkSymmetricKeyLength / 8);
                pbkSalt = pbkdf.Salt;
            }

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = PbkSymmetricKeyLength;
                aes.GenerateIV();
                aes.Key = pbk;

                using (var enc = aes.CreateEncryptor())
                {
                    var encoded = enc.TransformFinalBlock(decoded, 0, decoded.Length);
                    return (pbkSalt, aes.IV, encoded);
                }
            }
        }

        public byte[] Decrypt(string password, byte[] salt, byte[] iv, byte[] encoded)
        {
            byte[] pbk;
            using (var pbkdf = new Rfc2898DeriveBytes(password, salt, PbkIterations))
            {
                pbk = pbkdf.GetBytes(PbkSymmetricKeyLength / 8);
            }

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = PbkSymmetricKeyLength;
                aes.Key = pbk;
                aes.IV = iv;
                using (var dec = aes.CreateDecryptor())
                {
                    return dec.TransformFinalBlock(encoded, 0, encoded.Length);
                }
            }
        }
    }
}
