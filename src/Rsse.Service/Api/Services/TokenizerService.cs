using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rsse.Search.Indexes;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer;

namespace SearchEngine.Api.Services;

/// <summary>
/// Сервис поддержки токенайзера.
/// </summary>
public sealed class TokenizerService : ITokenizerService, IDisposable
{
    private readonly SearchEngineTokenizer _searchEngineTokenizer;
    private readonly ILogger<TokenizerService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Создать сервис токенайзера.
    /// </summary>
    /// <param name="options">Настройки.</param>
    /// <param name="logger">Логер.</param>
    public TokenizerService(
        IOptions<CommonBaseOptions> options,
        ILogger<TokenizerService> logger)
    {
        _logger = logger;
        _isEnabled = options.Value.TokenizerIsEnable;
        var extendedSearchType = options.Value.ExtendedSearchType;
        var reducedSearchType = options.Value.ReducedSearchType;
        _searchEngineTokenizer = new SearchEngineTokenizer(
            extendedSearchType, reducedSearchType);
    }

    // Используется для тестов.
    internal DirectIndex GetTokenLines() => _searchEngineTokenizer.GetTokenLines();

    /// <inheritdoc/>
    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var removed = await _searchEngineTokenizer.DeleteAsync(id, stoppingToken);

        if (!removed)
        {
            _logger.LogError($"[{nameof(TokenizerService)}] vector deletion error");
        }
    }

    /// <inheritdoc/>
    public async Task Create(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var created = await _searchEngineTokenizer.CreateAsync(id, note, stoppingToken);

        if (!created)
        {
            _logger.LogError($"[{nameof(TokenizerService)}] vector creation error");
        }
    }

    /// <inheritdoc/>
    public async Task Update(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var updated = await _searchEngineTokenizer.UpdateAsync(id, note, stoppingToken);

        if (!updated)
        {
            _logger.LogError($"[{nameof(TokenizerService)}] vector update error");
        }
    }

    // Инициализация вызывается по расписанию, раз в N часов.
    /// <inheritdoc/>
    public async Task Initialize(IDataProvider<NoteEntity> dataProvider, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var result = 0;
        try
        {
            result = await _searchEngineTokenizer.InitializeAsync(dataProvider, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("[{Reporter}] initialization system error | '{Source}' | '{Message}'",
                nameof(TokenizerService), ex.Source, ex.Message);
        }

        _logger.LogInformation("[{Reporter}] initialization finished | data amount '{TokenLinesCount}'",
            nameof(TokenizerService), result);
    }

    /// <inheritdoc/>
    public async Task<bool> WaitWarmUp(CancellationToken timeoutToken)
    {
        if (_isEnabled == false) return true;

        var isActivated = await _searchEngineTokenizer.WaitWarmUpAsync(timeoutToken);

        return isActivated;
    }

    /// <inheritdoc/>
    public bool IsInitialized() => _searchEngineTokenizer.IsInitialized();

    // Сценарий: основная нагрузка приходится на операции чтения, в большинстве случаев со своими данными клиент работает единолично.
    // Допустимо, если метод вернёт неактуальные данные.
    /// <inheritdoc/>
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        var complianceIndices = _searchEngineTokenizer.ComputeComplianceIndices(text, cancellationToken);

        return complianceIndices
            .Select(kvp => new KeyValuePair<int, double>(kvp.Key.Value, kvp.Value))
            .ToDictionary();
    }

    public void Dispose()
    {
        _searchEngineTokenizer.Dispose();
    }
}
