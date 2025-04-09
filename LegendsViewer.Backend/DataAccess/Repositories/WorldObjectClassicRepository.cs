using LegendsViewer.Backend.DataAccess.Repositories.Interfaces;
using LegendsViewer.Backend.Legends;

namespace LegendsViewer.Backend.DataAccess.Repositories;

public class WorldObjectClassicRepository<T>(List<T> allElements, Func<int, T?> getById) : IWorldObjectRepository<T> where T : WorldObject
{
    protected List<T> _allElements = allElements;
    protected Func<int, T?> _getById = getById;

    public List<T> GetAllElements() => _allElements;

    public T? GetById(int id) => _getById(id);

    public int GetCount() => _allElements.Count;
}
