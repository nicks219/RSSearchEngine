using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Data.Repository.Exceptions;

namespace SearchEngine.Data.Repository;

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

        var exists = await _context.Genre!.AnyAsync(entity => entity.Tag == tag);

        if (exists)
        {
            return;
        }

        var maxId = await _context.Genre!.Select(entity => entity.TagId).MaxAsync();

        var genre = new GenreEntity { Tag = tag, TagId = ++maxId };

        await _context.Genre!.AddAsync(genre);

        await _context.SaveChangesAsync();
    }

    public IQueryable<TextEntity> ReadAllNotes()
    {
        var songs =
            _context.Text!
            //.Select(s => string.Concat(s.TextId, " '", s.Title!, "' '", s.Song!,"'"))
            .Select(entity => entity)
            .AsNoTracking();

        return songs;
    }

    public string ReadNoteTitle(int noteId)
    {
        // см. предупреждение Rider "21 DB commands":
        var textEntity =
            _context.Text!
            .First(entity => entity.NoteId == noteId);

        return textEntity.Title!;
    }

    public int ReadNoteId(string noteTitle)
    {
        var song =
            _context.Text!
            .First(entity => entity.Title == noteTitle);

        return song.NoteId;
    }

    public IQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags)
    {
        // TODO: определить какой метод лучше
        // IQueryable<int> songsCollection = database.GenreText//
        //    .Where(s => chosenOnes.Contains(s.GenreInGenreText.GenreID))
        //    .Select(s => s.TextInGenreText.TextID);

        var songsForRandomizer = _context.Text!
            .Where(entity => entity.GenreTextInText!.Any(c => checkedTags.Contains(c.TagId)))
            .Select(entity => entity.NoteId);

        return songsForRandomizer;
    }

    public IQueryable<Tuple<string, int>> ReadCatalogPage(int pageNumber, int pageSize)
    {
        var titlesAndIdsList = _context.Text!
            .OrderBy(s => s.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(entity => new Tuple<string, int>(entity.Title!, entity.NoteId))
            .AsNoTracking();

        return titlesAndIdsList;
    }

    public IQueryable<Tuple<string, string>> ReadNote(int noteId)
    {
        var titleAndText = _context.Text!
            .Where(entity => entity.NoteId == noteId)
            .Select(entity => new Tuple<string, string>(entity.Text!, entity.Title!))
            .AsNoTracking();

        return titleAndText;
    }

    public IQueryable<int> ReadNoteTags(int noteId)
    {
        var songGenres = _context.GenreText!
            .Where(entity => entity.TextInGenreText!.NoteId == noteId)
            .Select(entity => entity.TagId);

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
            .OrderBy(entity =>
                entity.TagId) // вместо сортировки лучше построить индекс на стороне бд (индекс Genre по дефолту)
            .Select(entity => new Tuple<string, int>(entity.Tag!, entity.GenreTextInGenre!.Count))
            .ToListAsync();

        return genreList.Select(tagAndAmount =>
                tagAndAmount.Item2 > 0
                    ? tagAndAmount.Item1 + ": " + tagAndAmount.Item2
                    : tagAndAmount.Item1)
            .ToList();
    }

    public async Task UpdateNote(IEnumerable<int> initialTags, NoteDto note)
    {
        var forAddition = note.TagsCheckedRequest!.ToHashSet();

        var forDelete = initialTags.ToHashSet();

        var except = forAddition.Intersect(forDelete).ToList();

        forAddition.ExceptWith(except);

        forDelete.ExceptWith(except);

        if (await CheckTagsExistsError(note.NoteId, forAddition))
        {
            // название песни остаётся неизменным (constraint)
            // Id жанров и номера кнопок с фронта совпадают
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // дешевле просто откатить или не начинать транзакцию, без механизма исключений
                var text = await _context.Text!.FindAsync(note.NoteId);

                if (text == null)
                {
                    throw new Exception($"[{nameof(UpdateNote)}: Null in Text]");
                }

                text.Title = note.TitleRequest;

                text.Text = note.TextRequest;

                _context.Text.Update(text);

                _context.GenreText!
                    .RemoveRange(_context.GenreText
                        .Where(f =>
                            f.NoteId == note.NoteId && forDelete.Contains(f.TagId)));

                await _context.GenreText
                    .AddRangeAsync(forAddition
                        .Select(id =>
                            new GenreTextEntity
                            {
                                NoteId = note.NoteId,
                                TagId = id
                            }));

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
        if (!await CheckTitleExistsError(note.TitleRequest!))
        {
            return note.NoteId;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var addition = new TextEntity { Title = note.TitleRequest, Text = note.TextRequest };

            await _context.Text!.AddAsync(addition);

            await _context.SaveChangesAsync();

            await _context.GenreText!
                .AddRangeAsync(note.TagsCheckedRequest!
                    .Select(id =>
                        new GenreTextEntity
                        {
                            NoteId = addition.NoteId,
                            TagId = id
                        }));

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            note.NoteId = addition.NoteId;
        }
        catch (DataExistsException)
        {
            await transaction.RollbackAsync();

            note.NoteId = 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            note.NoteId = 0;

            throw new Exception($"[{nameof(CreateNote)}: Repo]", ex);
        }

        return note.NoteId;
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

        if (await _context.GenreText!.AnyAsync(p => p.NoteId == textId && p.TagId == forAddition.First()))
        {
            throw new DataExistsException("[Global Error: Tags Exists Error]");
        }

        return true;
    }
}
