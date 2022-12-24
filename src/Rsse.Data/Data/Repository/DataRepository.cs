using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RandomSongSearchEngine.Data.Dto;
using RandomSongSearchEngine.Data.Repository.Contracts;
using RandomSongSearchEngine.Data.Repository.Exceptions;

namespace RandomSongSearchEngine.Data.Repository;

public class DataRepository : IDataRepository
{
    private readonly RsseContext _context;

    public DataRepository(IServiceProvider serviceProvider)
    {
        _context = serviceProvider.GetRequiredService<RsseContext>();
    }

    public async Task CreateTagIfNotExists(string tag)
    {
        tag = tag.ToUpper();
        
        var exists = await _context.Genre!.AnyAsync(entity => entity.Genre == tag);

        if (exists)
        {
            return;
        }
        
        var maxId = await _context.Genre!.Select(entity => entity.GenreId).MaxAsync();

        var genre = new GenreEntity {Genre = tag, GenreId = ++maxId};

        await _context.Genre!.AddAsync(genre);

        await _context.SaveChangesAsync();
    }

    public IQueryable<TextEntity> ReadAllNotes()
    {
        var songs = _context.Text!
            //.Select(s => string.Concat(s.TextId, " '", s.Title!, "' '", s.Song!,"'"))
            .Select(s => s)
            .AsNoTracking();

        return songs;
    }

    public string ReadTitleByNoteId(int id)
    {
        var textEntity = _context.Text!
            .First(s => s.TextId == id);

        return textEntity.Title!;
    }

    public int FindNoteIdByTitle(string noteTitle)
    {
        var song = _context.Text!
            .First(s => s.Title == noteTitle);

        return song.TextId;
    }

    public IQueryable<int> ReadAllNotesTaggedBy(IEnumerable<int> checkedTags)
    {
        // TODO: определить какой метод лучше
        // IQueryable<int> songsCollection = database.GenreText//
        //    .Where(s => chosenOnes.Contains(s.GenreInGenreText.GenreID))
        //    .Select(s => s.TextInGenreText.TextID);

        var songsForRandomizer = _context.Text!
            .Where(a => a.GenreTextInText!.Any(c => checkedTags.Contains(c.GenreId)))
            .Select(a => a.TextId);

        return songsForRandomizer;
    }

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int lastPage, int pageSize)
    {
        var titlesAndIdsList = _context.Text!
            .OrderBy(s => s.Title)
            .Skip((lastPage - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new Tuple<string, int>(s.Title!, s.TextId))
            .AsNoTracking();

        return titlesAndIdsList;
    }

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        var titleAndText = _context.Text!
            .Where(p => p.TextId == noteId)
            .Select(s => new Tuple<string, string>(s.Song!, s.Title!))
            .AsNoTracking();

        return titleAndText;
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var songGenres = _context.GenreText!
            .Where(p => p.TextInGenreText!.TextId == noteId)
            .Select(s => s.GenreId);

        return songGenres;
    }

    public async Task<UserEntity?> GetUser(LoginDto login)
    {
        return await _context.Users!
            .FirstOrDefaultAsync(u => u.Email == login.Email && u.Password == login.Password);
    }

    public async Task<int> ReadNotesCount()
    {
        return await _context.Text!.CountAsync();
    }

    public async Task<List<string>> ReadGeneralTagList()
    {
        var genreList = await _context.Genre!
            .OrderBy(g => g.GenreId) // борьба с магией сортировки по Genre по дефолту
            .Select(g => new Tuple<string, int>(g.Genre!, g.GenreTextInGenre!.Count))
            .ToListAsync();

        return genreList.Select(genreAndAmount => genreAndAmount.Item2 > 0
                ? genreAndAmount.Item1 + ": " + genreAndAmount.Item2
                : genreAndAmount.Item1)
            .ToList();
    }

    public async Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        var forAddition = note.SongGenres!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        if (await CheckTagsExistsError(note.Id, forAddition))
        {
            // название песни остаётся неизменным (constraint)
            // Id жанров и номера кнопок с фронта совпадают
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // дешевле просто откатить или не начинать транзакцию, без механизма исключений
                var text = await _context.Text!.FindAsync(note.Id);

                if (text == null)
                {
                    throw new Exception($"[{nameof(UpdateNote)}: Null in Text]");
                }

                text.Title = note.Title;

                text.Song = note.Text;

                _context.Text.Update(text);

                _context.GenreText!
                    .RemoveRange(_context.GenreText
                        .Where(f =>
                            f.TextId == note.Id && forDelete.Contains(f.GenreId)));

                await _context.GenreText
                    .AddRangeAsync(forAddition
                        .Select(genre => new GenreTextEntity
                            {TextId = note.Id, GenreId = genre}));

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DataExistsException)
            {
                await transaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                throw new Exception($"[{nameof(UpdateNote)}: Repo]", ex);
            }
        }
    }

    public async Task<int> CreateNote(NoteDto note)
    {
        if (!await CheckTitleExistsError(note.Title!))
        {
            return note.Id;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var addition = new TextEntity {Title = note.Title, Song = note.Text};

            await _context.Text!.AddAsync(addition);

            await _context.SaveChangesAsync();

            await _context.GenreText!
                .AddRangeAsync(note.SongGenres!
                    .Select(genre => new GenreTextEntity
                        {TextId = addition.TextId, GenreId = genre}));

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            note.Id = addition.TextId;
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();

            note.Id = 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            note.Id = 0;

            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }

        return note.Id;
    }

    public async Task<int> DeleteNote(int noteId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = 0;

            var song = await _context.Text!.FindAsync(noteId);

            if (song == null)
            {
                return result;
            }

            _context.Text.Remove(song);

            result = await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            throw new Exception($"[{nameof(DeleteNote)}: Repo]", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);

        //context = null;

        GC.SuppressFinalize(this);
    }

    // на старте отрабатывает четыре раза
    void IDisposable.Dispose()
    {
        _context.Dispose();

        //context = null;

        GC.SuppressFinalize(this);
    }

    private async Task<bool> CheckTitleExistsError(string title)
    {
        return !await _context.Text!.AnyAsync(p => p.Title == title);
    }

    private async Task<bool> CheckTagsExistsError(int textId, IReadOnlyCollection<int> forAddition)
    {
        if (forAddition.Count <= 0)
        {
            return true;
        }

        if (await _context.GenreText!.AnyAsync(p => p.TextId == textId && p.GenreId == forAddition.First()))
        {
            throw new DataExistsException("[Global Error: Tags Exists Error]");
        }

        return true;
    }
}