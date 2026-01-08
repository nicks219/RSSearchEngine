using System;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Data.Entities;
using SimpleEngine.Contracts;

namespace SimpleEngine.Tokenizer.Contracts;

/// <summary>
/// Контракт, фиксирующий функционал сервиса токенайзера (в коде не используется).
/// </summary>
public interface ITokenizerServiceCore : IDisposable
{
    /// <summary>
    /// Создать вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Флаг успешного выполнения.</returns>
    public Task<bool> CreateAsync(int id, TextRequestDto note, CancellationToken stoppingToken);

    /// <summary>
    /// Создаёт метрики релевантности.
    /// </summary>
    /// <returns>Метрики релевантности.</returns>
    public IMetricsCalculator CreateMetricsCalculator();

    /// <summary>
    /// Удаляет метрики релевантности.
    /// </summary>
    /// <param name="metricsCalculator">Метрики релевантности.</param>
    public void ReleaseMetricsCalculator(IMetricsCalculator metricsCalculator);

    /// <summary>
    /// Выполнить поиск и вычислить индексы соответствия хранимых заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Поисковый запрос.</param>
    /// <param name="metricsCalculator">Метрики релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public void ComputeComplianceIndices(string text, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);

    /// <summary>
    /// Выполнить extended-поиск и вычислить индекс соответствия хранимых заметок поисковому запросу.
    /// Используется для бенчмарков.
    /// </summary>
    /// <param name="text">Поисковый запрос.</param>
    /// <param name="metricsCalculator">Метрики релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public void ComputeComplianceIndexExtended(string text, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);

    /// <summary>
    /// Выполнить reduced-поиск и вычислить индекс соответствия хранимых заметок поисковому запросу.
    /// Используется для бенчмарков.
    /// </summary>
    /// <param name="text">Поисковый запрос.</param>
    /// <param name="metricsCalculator">Метрики релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат поискового запроса в виде идентификаторов заметок и их индексов соответствия.</returns>
    public void ComputeComplianceIndexReduced(string text, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken);

    /// <summary>
    /// Обновить вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    /// <returns>Флаг успешного выполнения.</returns>
    public Task<bool> UpdateAsync(int id, TextRequestDto note, CancellationToken stoppingToken);

    /// <summary>
    /// Удалить вектор для заметки по идентификатору.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <param name="id">Идентификатор заметки.</param>
    /// <returns>Флаг успешного выполнения.</returns>
    public Task<bool> DeleteAsync(int id, CancellationToken stoppingToken);

    /// <summary>
    /// Инициализация функционала токенизации.
    /// </summary>
    /// <param name="dataProvider">Поставщик данных с заметками.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <returns>Количество токенизированных заметок.</returns>
    public Task<int> InitializeAsync(IDataProvider<NoteEntity> dataProvider, CancellationToken stoppingToken);

    /// <summary>
    /// Дождаться освобождения блокировки токенизатора и вернуть значение флага инициализации.
    /// </summary>
    /// <param name="timeoutToken">Токен отмены.</param>
    /// <returns><b>true</b> - Инициализация завершена успешно, либо функционал отключен.</returns>
    /// <exception cref="OperationCanceledException">Запрошена отмена освобождения блокировки.</exception>
    public Task<bool> WaitWarmUpAsync(CancellationToken timeoutToken);

    /// <summary>
    /// Флаг успешной инициализации токенизатора.
    /// </summary>
    /// <returns><b>true</b> - Инициализация завершена успешно.</returns>
    public bool IsInitialized();
}
