using System.IO.Pipelines;

namespace LiteDatabase.Storage.PageData;

public class RecordData : IPageData {
    public List<Row> Rows { get; set; } = new();
    public List<(int Offset, int Size)> SlotArray { get; set; } = new(); // use slot to record length and offset of each row

    public byte[] Data { get; set; } = Array.Empty <byte>();
    public byte[] Encode() {
        SlotArray.Clear();
        using var ms = new MemoryStream();
        foreach (var row in Rows) {
            int offset = (int)ms.Position;
            byte[] rowBytes = EncodeRow(row);
            ms.Write(rowBytes, 0, rowBytes.Length);
            SlotArray.Add((offset, rowBytes.Length));
        }

        Data = ms.ToArray();

        using var result = new MemoryStream();
        WriteInt(result, SlotArray.Count);
        foreach (var (offset, size) in SlotArray)
        {
            WriteInt(result, offset);
            WriteInt(result, size);
        }
        result.Write(Data, 0, Data.Length);
        return result.ToArray();
    }

    public void Decode(Stream stream) {
        int slotCount = ReadInt(stream);
        for (int i = 0; i < slotCount; i++) {
            int offset = ReadInt(stream);
            int size = ReadInt(stream);
            SlotArray.Add((offset, size));
        }

        using (var ms = new MemoryStream()) {
            stream.CopyTo(ms);
            Data = ms.ToArray();
        }

        Rows.Clear();
        foreach (var (offset, size) in SlotArray) {
            var rowBytes = Data.Skip(offset).Take(size).ToArray();
            Rows.Add(DecodeRow(rowBytes));
        }
    }


    private static byte[] EncodeRow(Row row) {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(row.Data.Length);
        foreach (var val in row.Data) {
            switch (val) {
                case int i:
                    bw.Write((byte)1);
                    bw.Write(i);
                    break;
                case float f:
                    bw.Write((byte)2);
                    bw.Write(f);
                    break;
                case bool b:
                    bw.Write((byte)3);
                    bw.Write(b);
                    break;
                case string s:
                    bw.Write((byte)4);
                    bw.Write(s);
                    break;
                default:
                    throw new NotSupportedException("Unsupported type: " + val?.GetType());
            }
        }
        return ms.ToArray();
    }
    private static Row DecodeRow(byte[] bytes) {
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);
        int count = br.ReadInt32();
        object[] data = new object[count];
        for (int i = 0; i < count; i++) {
            byte type = br.ReadByte();
            data[i] = type switch {
                1 => br.ReadInt32(),
                2 => br.ReadSingle(),
                3 => br.ReadBoolean(),
                4 => br.ReadString(),
                _ => throw new NotSupportedException("Unsupported type: " + type)
            };
        }
        return new Row(data);
    }

    public int Size() => Encode().Length;
    private static void WriteInt(Stream s, int v) => s.Write(BitConverter.GetBytes(v), 0, 4);
    // Helper methods
    private static int ReadInt(Stream s)
    {
        Span<byte> buf = stackalloc byte[4];
        s.ReadExactly(buf);
        return BitConverter.ToInt32(buf);
    }

}
