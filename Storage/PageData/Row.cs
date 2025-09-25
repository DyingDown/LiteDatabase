namespace LiteDatabase.Storage.PageData;

public class Row {
    public object[] Data { get; set; }

    public Row(object[] data) {
        Data = data;
    }
}