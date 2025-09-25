namespace LiteDatabase.Storage;

using System.IO;
using LiteDatabase.Storage.PageData;

// TODO: Rewrite this;
public class Pager {

    private readonly FileIO fileIO;

    public Page ReadPage(uint pageNo) {
        return new Page();
    }

    public void WritePage(Page page) {

    }

    public uint AllocatePage(PageType type) {
        return 0;
    }

}