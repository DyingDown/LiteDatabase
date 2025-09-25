using System.Text;
using System.Text.Json;
using LiteDatabase.Catalog;

namespace LiteDatabase.Storage.PageData;

public class MetaData : IPageData {
    public bool Dirty { get; set; }
    public string Version { get; set; } = "";
    public Dictionary<string, TableSchema> Tables { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public byte[] Encode() {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Version ?? "");
        bw.Write(Tables.Count);

        foreach (var kv in Tables) {
            bw.Write(kv.Key);
            string schemaJson = JsonSerializer.Serialize(kv.Value);
            bw.Write(schemaJson);
        }
        return ms.ToArray();
    }

    public void Decode(Stream stream) {
        using var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Version = br.ReadString();
        int count = br.ReadInt32();
        Tables = new Dictionary<string, TableSchema>(count, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < count; i++) {
            string key = br.ReadString();
            string schemaJson = br.ReadString();
            var schema = JsonSerializer.Deserialize<TableSchema>(schemaJson);
            Tables[key] = schema!;
        }
    }

    public int Size() => Encode().Length;
}