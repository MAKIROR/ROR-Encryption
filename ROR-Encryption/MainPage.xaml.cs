using System.Security.Cryptography;
using System.Text;

namespace ROR_Encryption;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        string key = ReadFile(FileSystem.Current.CacheDirectory + "/key");
        if (key != null)
        {
            keybox.Text = key;
        }
    }

    private void SaveKey(object sender, EventArgs e)
    {
        string Dir = FileSystem.Current.CacheDirectory;
        if (keybox.Text == "" || keybox.Text.Length != 24)
        {
            DisplayAlert("Error", "Key length should be 24", "Close");
        }
        else
        {
            if (WriteFile(Dir + "/key", keybox.Text))
            {
                DisplayAlert("OK!", "Successfully saved your key", "Close");
            }
            else
            {
                DisplayAlert("OK!", "", "Close");
            }
        }
    }
    private async void CreateKey(object sender, EventArgs e)
    {
        keybox.Text = GenerateKey();
        await Clipboard.Default.SetTextAsync(keybox.Text);
    }
    private async void CopyPlainText(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(plaintext.Text);
    }
    private async void CopyCipherText(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(ciphertext.Text);
    }
    private async void PastePlainText(object sender, EventArgs e)
    {
        if (Clipboard.Default.HasText)
        {
            plaintext.Text = await Clipboard.Default.GetTextAsync();
        }
    }
    private async void PasteCipherText(object sender, EventArgs e)
    {
        if (Clipboard.Default.HasText)
        {
            ciphertext.Text = await Clipboard.Default.GetTextAsync();
        }
    }
    private void CleanPlainText(object sender, EventArgs e)
    {
        plaintext.Text = "";
    }
    private void CleanCipherText(object sender, EventArgs e)
    {
        ciphertext.Text = "";
    }
    private void Encrypt(object sender, EventArgs e)
    {
        ciphertext.Text = Encode(plaintext.Text, keybox.Text);
    }
    private void Decrypt(object sender, EventArgs e)
    {
        plaintext.Text = Decode(ciphertext.Text, keybox.Text);
    }
    public string GenerateKey()
    {
        Random r = new Random();
        string s = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string key = "";
        for (int i = 0; i < 24; i++)
        {
            int number = r.Next(s.Length);
            key += s.Substring(number, 1);
        }
        return key;
    }
    public bool WriteFile(string path, string content)
    {
        try
        {
            using (FileStream fs = File.Create(path))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(content);
                fs.Write(info, 0, info.Length);
            }
            return true;

        }
        catch (Exception)
        {
            return false;
        }
    }
    public string ReadFile(string path)
    {
        try
        {
            string result = "";
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    result += s;
                }
            }
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
    public static string Encode(string plaintext, string key)
    {
        try
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plaintext);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
        catch (Exception)
        {
            return "Error:cannot encrypt";
        }
    }
    public static string Decode(string ciphertext, string key)
    {
        try
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(ciphertext);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        } catch(Exception)
        {
            return "Error:cannot decrypt";
        }

    }
}

