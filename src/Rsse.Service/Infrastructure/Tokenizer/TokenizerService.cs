using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Configuration;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Infrastructure.Tokenizer;

public class TokenizerService : ITokenizerService
{
    // [TODO]: нужен ли ConcurrentDictionary при ReaderWriterLockSlim?
    // [TODO]: дополнить логгирование ошибок пересозданием линии кэша
    private readonly IServiceScopeFactory _factory;
    private readonly ConcurrentDictionary<int, List<int>> _reducedTokenLines;
    private readonly ConcurrentDictionary<int, List<int>> _extendedTokenLines;
    private readonly ReaderWriterLockSlim _lockSlim;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    public TokenizerService(IServiceScopeFactory factory, IOptions<CommonBaseOptions> options, ILogger<TokenizerService> logger)
    {
        _factory = factory;
        _reducedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _extendedTokenLines = new ConcurrentDictionary<int, List<int>>();
        _logger = logger;
        _lockSlim = new ReaderWriterLockSlim();
        _isEnabled = options.Value.TokenizerIsEnable;

        Initialize();
    }

    public ConcurrentDictionary<int, List<int>> GetReducedLines() => _reducedTokenLines;

    public ConcurrentDictionary<int, List<int>> GetExtendedLines() => _extendedTokenLines;

    public void Delete(int id)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterWriteLock();

        var isReducedRemoved = _reducedTokenLines.TryRemove(id, out _);

        var isExtendedRemoved = _extendedTokenLines.TryRemove(id, out _);

        if (!(isReducedRemoved && isExtendedRemoved))
        {
            _logger.LogError("[Cache Repository] on delete error");
        }

        _lockSlim.ExitWriteLock();
    }

    public void Create(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, note);

        if (!_extendedTokenLines.TryAdd(id, extendedLine))
        {
            _logger.LogError("[Cache Repository] extended vectors create error");
        }

        if (!_reducedTokenLines.TryAdd(id, reducedLine))
        {
            _logger.LogError("[Cache Repository] reduced vectors create error");
        }

        _lockSlim.ExitReadLock();
    }

    public void Update(int id, NoteEntity note)
    {
        if (!_isEnabled) return;

        _lockSlim.EnterReadLock();

        using var scope = _factory.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        var (extendedLine, reducedLine, _) = CreateTokensLine(processor, note);

        if (_extendedTokenLines.TryGetValue(id, out var cachedTokensLine))
        {
            if (!_extendedTokenLines.TryUpdate(id, extendedLine, cachedTokensLine))
            {
                _logger.LogError("[Cache Repository] extended vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError("[Cache Repository] extended vectors has not been updated");
        }

        if (_reducedTokenLines.TryGetValue(id, out cachedTokensLine))
        {
            if (!_reducedTokenLines.TryUpdate(id, reducedLine, cachedTokensLine))
            {
                _logger.LogError("[Cache Repository] reduced vectors concurrent update error");
            }
        }
        else
        {
            _logger.LogError("[Cache Repository] reduced vectors has not been updated");
        }

        _lockSlim.ExitReadLock();
    }

    public void Initialize()
    {
        if (!_isEnabled) return;

        _lockSlim.EnterWriteLock();

        using var scope = _factory.CreateScope();

        using var repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();

        var processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        try
        {
            _reducedTokenLines.Clear();

            _extendedTokenLines.Clear();

            // TODO: избавься от загрузки всех записей из таблицы:
            var texts = repo.ReadAllNotes();

            foreach (var text in texts)
            {
                var (extendedLine, reducedLine, id) = CreateTokensLine(processor, text);

                if (!_extendedTokenLines.TryAdd(id, extendedLine))
                {
                    throw new MethodAccessException("[Cache Repository] extended vectors initialization error");
                }

                if (!_reducedTokenLines.TryAdd(id, reducedLine))
                {
                    throw new MethodAccessException("[Cache Repository] reduced vectors initialization error");
                }
            }

            GC.Collect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache Repository] initialization system error");
        }
        finally
        {
            _lockSlim.ExitWriteLock();
        }
    }

    private static (List<int> Extended, List<int> Reduced, int Id) CreateTokensLine(ITokenizerProcessor processor, NoteEntity note)
    {
        // extended tokens chain line:
        processor.SetupChain(ConsonantChain.Extended);

        var preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var extendedTokensLine = processor.TokenizeSequence(preprocessedNote);

        // reduced tokens chain line:
        processor.SetupChain(ConsonantChain.Reduced);

        preprocessedNote = processor.PreProcessNote(note.Text + ' ' + note.Title);

        var reducedTokensLine = processor.TokenizeSequence(preprocessedNote);

        return (Extended: extendedTokensLine, Reduced: reducedTokensLine, Id: note.NoteId);
    }
}
