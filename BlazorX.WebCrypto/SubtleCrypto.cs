using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorX.WebCrypto
{
    public enum DigestAlgorithm
    {
        SHA1,
        SHA256,
        SHA384,
        SHA512,
    }

    [Flags]
    public enum KeyUsage
    {
        None = 0,
        Encrypt = 0x01,
        Decrypt = 0x02,
        Sign = 0x04,
        Verify = 0x08,
        DeriveKey = 0x10,
        DeriveBits = 0x20,
        WrapKey = 0x40,
        UnwrapKey = 0x80,
    }

    public class CryptoKey
    {
        public CryptoKey(KeyDetails details, KeyUsage usage, byte[] raw)
        {
            Details = details;
            Usage = usage.ToStrings(emptyValue: KeyUsage.None);
            Raw = raw;
        }

        public KeyDetails Details { get; set; }
        public string[] Usage { get; set; }
        public byte[] Raw { get; set; }
    }

    public abstract class KeyDetails
    {
        public KeyDetails(string name) => Name = name;

        public string Name { get; }
    }

    public class AesCbcKeyDetails : KeyDetails
    {
        public AesCbcKeyDetails() : base("AES-CBC")
        { }

        /// <summary>
        /// Length of the key in bits.  Valid values: 128, 192, 256.
        /// </summary>
        public int Length { get; set; } = 128;
    }

    public abstract class DeriveDetails
    {
        public DeriveDetails(string name) => Name = name;

        public string Name { get; }

    }

    public class Pbkdf2DeriveDetails : DeriveDetails
    {
        public Pbkdf2DeriveDetails() : base("PBKDF2")
        { }

        public byte[] Salt { get; set; }

        public int Iterations { get; set; } = 1000;

        public string Hash { get; set; } = "SHA-1";
    }

    public abstract class EncryptDetails
    {
        public EncryptDetails(string name) => Name = name;

        public string Name { get; }
    }

    public class AesCbcEncryptDetails : EncryptDetails
    {
        public AesCbcEncryptDetails() : base("AES-CBC")
        { }

        public byte[] IV { get; set; }
    }

    public static class SubtleCrypto
    {
        public static async Task<byte[]> Digest(DigestAlgorithm algor, byte[] buffer)
        {
            string algorStr;
            switch (algor)
            {
                case DigestAlgorithm.SHA1:
                    algorStr = "SHA-1";
                    break;
                case DigestAlgorithm.SHA256:
                    algorStr = "SHA-256";
                    break;
                case DigestAlgorithm.SHA384:
                    algorStr = "SHA-384";
                    break;
                case DigestAlgorithm.SHA512:
                    algorStr = "SHA-512";
                    break;
                default:
                    throw new ArgumentException("invalid algorithm", nameof(algor));
            }

            return await JSRuntime.Current.InvokeAsync<byte[]>(
                "_blazorWebCrypto.subtle_digest", algorStr, buffer);
        }

        public static async Task<CryptoKey> GenerateKey(KeyDetails details, KeyUsage usage)
        {
            var raw = await JSRuntime.Current.InvokeAsync<byte[]>(
                "_blazorWebCrypto.subtle_generateKey",
                details, usage.ToStrings(emptyValue: KeyUsage.None));
            return new CryptoKey(details, usage, raw);
        }

        public static async Task<CryptoKey> DeriveKey(DeriveDetails deriveDetails, byte[] keyData,
            KeyDetails keyDetails, KeyUsage usage)
        {
            switch (deriveDetails)
            {
                case Pbkdf2DeriveDetails pbkdf2:
                    var raw = await JSRuntime.Current.InvokeAsync<byte[]>(
                        "_blazorWebCrypto.subtle_derive_key_pbkdf2",
                        keyData, pbkdf2.Salt, pbkdf2.Iterations, pbkdf2.Hash,
                        keyDetails, usage.ToStrings(emptyValue: KeyUsage.None));
                    return new CryptoKey(keyDetails, usage, raw);

                default:
                    throw new NotSupportedException("unsupported key type");
            }
        }

        public static async Task<byte[]> Encrypt(EncryptDetails details, CryptoKey key, byte[] decoded)
        {
            switch (details)
            {
                case AesCbcEncryptDetails aesCbc:
                    if (!(key.Details is AesCbcKeyDetails))
                        throw new ArgumentException("invalid key type for encryption details", nameof(key));
                    if (aesCbc.IV?.Length != 16)
                        throw new ArgumentException("invalid or missing IV in encryption details (must be 16 bytes)",
                            nameof(details));
                    return await JSRuntime.Current.InvokeAsync<byte[]>(
                        "_blazorWebCrypto.subtle_encrypt_aes_cbc",
                        aesCbc.IV, key.Raw, key.Details, key.Usage, decoded);

                default:
                    throw new NotSupportedException("unsupported key type");
            }
        }

        public static async Task<byte[]> Decrypt(EncryptDetails details, CryptoKey key, byte[] encoded)
        {
            switch (details)
            {
                case AesCbcEncryptDetails aesCbc:
                    if (!(key.Details is AesCbcKeyDetails))
                        throw new ArgumentException("invalid key type for encryption details", nameof(key));
                    if (aesCbc.IV?.Length != 16)
                        throw new ArgumentException("invalid or missing IV in encryption details (must be 16 bytes)",
                            nameof(details));
                    return await JSRuntime.Current.InvokeAsync<byte[]>(
                        "_blazorWebCrypto.subtle_decrypt_aes_cbc",
                        aesCbc.IV, key.Raw, key.Details, key.Usage, encoded);

                default:
                    throw new NotSupportedException("unsupported key type");
            }
        }

        internal static string[] ToStrings(this Enum enm, object emptyValue = null)
        {
            if (emptyValue != null && enm == emptyValue)
                return Array.Empty<string>();

            char[] spliters = ", ".ToCharArray();
            return enm.ToString().Split(spliters, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => char.ToLower(x[0]) + x.Substring(1))
                .ToArray();
        }
    }
}
