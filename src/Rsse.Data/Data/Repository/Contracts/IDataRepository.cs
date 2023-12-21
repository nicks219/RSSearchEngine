using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;

namespace SearchEngine.Data.Repository.Contracts;

public interface IDataRepository : IDisposable, IAsyncDisposable
{
    // crud:
    Task<int> CreateNote(NoteDto note);
    IQueryable<Tuple<string, string>> ReadNote(int noteId);
    Task UpdateNote(IEnumerable<int> initialTags, NoteDto note);
    Task<int> DeleteNote(int noteId);

    // common:
    Task<List<string>> ReadGeneralTagList();
    Task<int> ReadNotesCount();
    IQueryable<NoteEntity> ReadAllNotes();
    IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags);
    string ReadNoteTitle(int noteId);
    int ReadNoteId(string noteTitle);
    IQueryable<int> ReadNoteTags(int noteId);

    // additional:
    IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize);
    Task<UserEntity?> GetUser(LoginDto login);
    Task CreateTagIfNotExists(string tag);
}
