using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.Collections.Generic;

class EdgeCookieReader
{
    private string AesGcmDecrypt(byte[] keyBytes, byte[] nonce, byte[] encryptedValue)
    {
        GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
        AeadParameters aeadParameters = new AeadParameters(
            new KeyParameter(keyBytes),
            128,
            nonce);
        gcmBlockCipher.Init(false, aeadParameters);
        byte[] plaintext = new byte[gcmBlockCipher.GetOutputSize(encryptedValue.Length)];
        int length = gcmBlockCipher.ProcessBytes(encryptedValue, 0, encryptedValue.Length, plaintext, 0);
        gcmBlockCipher.DoFinal(plaintext, length);
        return Encoding.UTF8.GetString(plaintext);
    }

    public CookieItem GetEdgeCookie(string HostName, string Name)
    {
        var userprofilePath = Environment.GetEnvironmentVariable("USERPROFILE");
        var cookieFile = $@"{userprofilePath}\AppData\Local\Microsoft\Edge\User Data\Default\Cookies";
        var localStateFile = $@"{userprofilePath}\AppData\Local\Microsoft\Edge\User Data\Local State";

        var content = File.ReadAllText(localStateFile);
        var encrypted_key = JObject.Parse(content).SelectToken(".os_crypt.encrypted_key").ToString();

        var tempCookieFile = Path.GetTempFileName();
        File.Delete(tempCookieFile);
        File.Copy(cookieFile, tempCookieFile);

        var cookies = new List<CookieItem>();

        var connection = new SQLiteConnection($@"DataSource={tempCookieFile}");

        connection.Open();
        var command = new SQLiteCommand($"select host_key,name,encrypted_value from cookies where host_key is '{HostName}'", connection);
        var dataReader = command.ExecuteReader();

        while (dataReader.Read())
        {
            var encryptedValue = (byte[])dataReader["encrypted_value"];
            var nonceLength = 96 / 8;
            var kEncryptionVersionPrefix = "v10";

            var encryptedKeyBytes = Convert.FromBase64String(encrypted_key);
            encryptedKeyBytes = encryptedKeyBytes.Skip("DPAPI".Length).Take(encryptedKeyBytes.Length - "DPAPI".Length).ToArray();

            var keyBytes = ProtectedData.Unprotect(encryptedKeyBytes, null, DataProtectionScope.CurrentUser);
            var nonce = encryptedValue.Skip(kEncryptionVersionPrefix.Length).Take(nonceLength).ToArray();
            encryptedValue = encryptedValue.Skip(kEncryptionVersionPrefix.Length + nonceLength).Take(encryptedValue.Length - (kEncryptionVersionPrefix.Length + nonceLength)).ToArray();

            var str = AesGcmDecrypt(keyBytes, nonce, encryptedValue);
            cookies.Add(new CookieItem { HostKey = dataReader["host_key"].ToString(), Name = dataReader["name"].ToString(), Value = str });
        }

        connection.Close();

        return cookies.Find(x => x.Name == Name);
    }
}