using System.Text;

namespace ChatApp.Core.Client;

/// <summary>Sohbet geçmişini kullanıcının seçtiği metin dosyasına yazar.</summary>
public static class ChatHistoryWriter
{
    public static void Write(string path, IEnumerable<string> lines)
    {
        using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
        foreach (string line in lines)
        {
            writer.WriteLine(line);
        }
    }
}
