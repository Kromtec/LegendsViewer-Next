using LegendsViewer.Backend.Legends;

namespace LegendsViewer.Backend.DataAccess.Repositories.Interfaces;

public interface IWorldObjectRepository<T> where T : WorldObject
{
    List<T> GetAllElements();

    T? GetById(int id);

    int GetCount();
}
