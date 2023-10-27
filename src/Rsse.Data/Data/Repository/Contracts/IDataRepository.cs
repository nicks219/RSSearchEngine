using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Dto;

namespace SearchEngine.Data.Repository.Contracts;

public interface IDataRepository : IDisposable, IAsyncDisposable
{
    // crud:
    // [TODO]: перепиши async на IQueryable<T>
    Task<int> CreateNote(NoteDto dt);
    IQueryable<Tuple<string, string>> ReadNote(int noteId);
    Task UpdateNote(IEnumerable<int> initialTags, NoteDto dt);
    Task<int> DeleteNote(int noteId);

    // common:
    Task<List<string>> ReadGeneralTagList();
    Task<int> ReadNotesCount();
    IQueryable<TextEntity> ReadAllNotes();
    IQueryable<int> ReadAllNotesTaggedBy(IEnumerable<int> checkedTags);
    string ReadTitleByNoteId(int id);
    int FindNoteIdByTitle(string noteTitle);
    IQueryable<int> ReadNoteTags(int noteId);

    // additional:
    IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize);
    Task<UserEntity?> GetUser(LoginDto dt);
    Task CreateTagIfNotExists(string tag);
}
