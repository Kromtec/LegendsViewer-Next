
namespace LegendsViewer.Backend.Legends.Bookmarks;

public interface IBookmarkService
{
    Bookmark AddBookmark(Bookmark bookmark);
    bool DeleteBookmarkTimestamp(string filePath);
    List<Bookmark> GetAll();
    Bookmark? GetBookmark(string filePath);
}