using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Dto;

namespace SearchEngine.Service.Contracts;

/// <summary>
/// Контракт сервиса поддержки токенизации заметок.
/// </summary>
public interface ITokenizerService
{
    /// <summary>
    /// Создать вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    public Task Create(int id, TextRequestDto note, CancellationToken stoppingToken);

    /// <summary>
    /// Вычислить индексы соответствия хранимых заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска соответствий.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Идентификаторы заметок и их индексы соответствия.</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    public Task Update(int id, TextRequestDto note, CancellationToken stoppingToken);

    /// <summary>
    /// Удалить вектор для заметки по идентификатору.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены.</param>
    /// <param name="id">Идентификатор заметки.</param>
    public Task Delete(int id, CancellationToken stoppingToken);

    /// <summary>
    /// Инициализация функционала токенизации.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены.</param>
    public Task Initialize(CancellationToken stoppingToken);

    /// <summary>
    /// Дождаться освобождения блокировки токенизатора и вернуть значение флага инициализации.
    /// </summary>
    /// <param name="timeoutToken">Токен отмены.</param>
    /// <returns><b>true</b> - Инициализация завершена успешно, либо функционал отключен.</returns>
    /// <exception cref="OperationCanceledException">Запрошена отмена освобождения блокировки.</exception>
    public Task<bool> WaitWarmUp(CancellationToken timeoutToken);
}
