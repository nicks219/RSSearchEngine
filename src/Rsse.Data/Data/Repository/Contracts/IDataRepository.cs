using RandomSongSearchEngine.Data.DTO;

namespace RandomSongSearchEngine.Data.Repository.Contracts
{
    public interface IDataRepository : IAsyncDisposable, IDisposable
    {
        IQueryable<string> ReadAllSongs();
        string ReadSongTitleById(int id);

        int FindIdByName(string name);
        
        // [TODO]: перепиши async на IQueryable<T>
        Task<int> CreateSongAsync(SongDto dt);
        IQueryable<Tuple<string, string>> ReadSong(int textId);
        Task UpdateSongAsync(IEnumerable<int> originalCheckboxes, SongDto dt);
        Task<int> DeleteSongAsync(int songId);
        Task<UserEntity?> GetUser(LoginDto dt);
        IQueryable<Tuple<string, int>> ReadCatalogPage(int lastPage, int pageSize);
        Task<List<string>> ReadGenreListAsync();
        Task<int> ReadTextsCountAsync();
        IQueryable<int> ReadSongGenres(int textId);
        IQueryable<int> SelectAllSongsInGenres(IEnumerable<int> checkedGenres);
    }
}