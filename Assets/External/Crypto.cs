using System.Text;
using System.Security.Cryptography;
using System.IO;

// Publically provided by StackOverflow community.
// http://goo.gl/81Jj2s

public class CryptoProvider
{
	private ICryptoTransform InitEncrypt(string a_key)
	{
		byte[] keyBytes;
        keyBytes = Encoding.Unicode.GetBytes(a_key);
        Rfc2898DeriveBytes derivedKey = new Rfc2898DeriveBytes(a_key, keyBytes);

        RijndaelManaged rijndaelCSP = new RijndaelManaged();
        rijndaelCSP.Key = derivedKey.GetBytes(rijndaelCSP.KeySize / 8);
        rijndaelCSP.IV = derivedKey.GetBytes(rijndaelCSP.BlockSize / 8);
        ICryptoTransform encryptor = rijndaelCSP.CreateEncryptor();
		rijndaelCSP.Clear();
		return encryptor;
	}
	
    public void EncryptFile(string a_inputFile, string a_outputFile, string a_key)
	{
		ICryptoTransform encryptor = InitEncrypt(a_key);

        FileStream inputFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        byte[] inputFileData = new byte[(int)inputFileStream.Length];
        inputFileStream.Read(inputFileData, 0, (int)inputFileStream.Length);
        FileStream outputFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write);

        CryptoStream encryptStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write);
        encryptStream.Write(inputFileData, 0, (int)inputFileStream.Length);
        encryptStream.FlushFinalBlock();

        encryptStream.Close();
        inputFileStream.Close();
        outputFileStream.Close();
    }
	
	private ICryptoTransform InitDecrypt(string a_key)
	{
		byte[] keyBytes = Encoding.Unicode.GetBytes(a_key);
        Rfc2898DeriveBytes derivedKey = new Rfc2898DeriveBytes(a_key, keyBytes);

        RijndaelManaged rijndaelCSP = new RijndaelManaged();
        rijndaelCSP.Key = derivedKey.GetBytes(rijndaelCSP.KeySize / 8);
        rijndaelCSP.IV = derivedKey.GetBytes(rijndaelCSP.BlockSize / 8);
        ICryptoTransform decryptor = rijndaelCSP.CreateDecryptor();
		rijndaelCSP.Clear();
		return decryptor;
	}
	
	public void DecryptFile(string a_inputFile, string a_outputFile, string a_key)
    {
        ICryptoTransform decryptor = InitDecrypt(a_key);

        FileStream inputFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        CryptoStream decryptStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read);

        byte[] inputFileData = new byte[(int)inputFileStream.Length];
		int decrypt_length = decryptStream.Read(inputFileData, 0, (int)inputFileStream.Length);
		
        FileStream outputFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write);
        outputFileStream.Write(inputFileData, 0, decrypt_length);
        outputFileStream.Flush();

        decryptStream.Close();
        inputFileStream.Close();
        outputFileStream.Close();
    }
	
    public MemoryStream DecryptFileToStream(string a_inputFile, string a_key) 
	{
        ICryptoTransform decryptor = InitDecrypt(a_key);

        FileStream inputFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        CryptoStream decryptStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read);

        byte[] inputFileData = new byte[(int)inputFileStream.Length];
        int decrypt_length = decryptStream.Read(inputFileData, 0, (int)inputFileStream.Length);
		
		MemoryStream output = new MemoryStream();
		output.Write(inputFileData, 0, decrypt_length);
		output.Seek(0, SeekOrigin.Begin);

        decryptStream.Close();
        inputFileStream.Close();
		
		return output;
    }
}