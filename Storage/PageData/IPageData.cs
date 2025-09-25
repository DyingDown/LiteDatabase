namespace LiteDatabase.Storage.PageData;

public interface IPageData {
    byte[] Encode();
    void Decode(Stream stream);
    int Size();
}