using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rsse.Api.Configuration;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Data.Entities;
using Rsse.Domain.Service.Contracts;
using RsseEngine.Indexes;
using RsseEngine.Service;

namespace Rsse.Api.Services;

/// <summary>
/// Сервис, поддерживающий настройку и использование функционала токенайзера, в т.ч поиск и инициализацию.
/// </summary>
public sealed class TokenizerApiClient : ITokenizerApiClient, IDisposable
{
    private readonly TokenizerServiceCore _tokenizerServiceCore;
    private readonly ILogger<TokenizerApiClient> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Создать сервис токенайзера.
    /// </summary>
    /// <param name="options">Настройки.</param>
    /// <param name="logger">Логер.</param>
    public TokenizerApiClient(
        IOptions<CommonBaseOptions> options,
        ILogger<TokenizerApiClient> logger)
    {
        _logger = logger;
        _isEnabled = options.Value.TokenizerIsEnable;
        var extendedSearchType = options.Value.ExtendedSearchType;
        var reducedSearchType = options.Value.ReducedSearchType;
        _tokenizerServiceCore = new TokenizerServiceCore(MetricsCalculatorType.PooledMetricsCalculator,
            false, extendedSearchType, reducedSearchType);
    }

    // Используется для тестов.
    internal DirectIndex GetDirectIndex() => ((TokenizerServiceCore)_tokenizerServiceCore).GetDirectIndex();

    /// <inheritdoc/>
    public async Task Delete(int id, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var removed = await _tokenizerServiceCore.DeleteAsync(id, stoppingToken);

        if (!removed)
        {
            _logger.LogError($"[{nameof(TokenizerApiClient)}] vector deletion error");
        }
    }

    /// <inheritdoc/>
    public async Task Create(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var created = await _tokenizerServiceCore.CreateAsync(id, note, stoppingToken);

        if (!created)
        {
            _logger.LogError($"[{nameof(TokenizerApiClient)}] vector creation error");
        }
    }

    /// <inheritdoc/>
    public async Task Update(int id, TextRequestDto note, CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        var updated = await _tokenizerServiceCore.UpdateAsync(id, note, stoppingToken);

        if (!updated)
        {
            _logger.LogError($"[{nameof(TokenizerApiClient)}] vector update error");
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
            result = await _tokenizerServiceCore.InitializeAsync(dataProvider, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("[{Reporter}] initialization system error | '{Source}' | '{Message}'",
                nameof(TokenizerApiClient), ex.Source, ex.Message);
        }

        _logger.LogInformation("[{Reporter}] initialization finished | data amount '{TokenLinesCount}'",
            nameof(TokenizerApiClient), result);
    }

    /// <inheritdoc/>
    public async Task<bool> WaitWarmUp(CancellationToken timeoutToken)
    {
        if (_isEnabled == false) return true;

        var isActivated = await _tokenizerServiceCore.WaitWarmUpAsync(timeoutToken);

        return isActivated;
    }

    /// <inheritdoc/>
    public bool IsInitialized() => _tokenizerServiceCore.IsInitialized();

    public IAlgorithmConfigurable GetTokenizerServiceCore() => _tokenizerServiceCore;

    // Сценарий: основная нагрузка приходится на операции чтения, в большинстве случаев со своими данными клиент работает единолично.
    // Допустимо, если метод вернёт неактуальные данные.
    /// <inheritdoc/>
    public List<KeyValuePair<int, double>> ComputeComplianceIndices(string text, CancellationToken cancellationToken)
    {
        var metricsCalculator = _tokenizerServiceCore.CreateMetricsCalculator();

        try
        {
            _tokenizerServiceCore.ComputeComplianceIndices(text, metricsCalculator, cancellationToken);

            var indices = metricsCalculator.ComplianceMetrics
                .Select(kvp => new KeyValuePair<int, double>(kvp.Key.Value, kvp.Value))
                .ToList();

            return indices;
        }
        finally
        {
            _tokenizerServiceCore.ReleaseMetricsCalculator(metricsCalculator);
        }
    }

    public void Dispose()
    {
        _tokenizerServiceCore.Dispose();
    }
}
