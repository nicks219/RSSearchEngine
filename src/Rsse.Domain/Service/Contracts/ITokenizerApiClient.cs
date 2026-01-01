using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Data.Entities;

namespace Rsse.Domain.Service.Contracts;

/// <summary>
/// Контракт использования функционала токенизации документов.
/// </summary>
public interface ITokenizerApiClient
{
    /// <summary>
    /// Создать вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    public Task Create(int id, TextRequestDto note, CancellationToken stoppingToken);

    /// <summary>
    /// Выполнить поиск и вычислить индексы соответствия хранимых заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Поисковый запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат поискового запроса в виде идентификаторов заметок и их индексов соответствия.</returns>
    public List<KeyValuePair<int, double>> ComputeComplianceIndices(string text, CancellationToken cancellationToken);

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
    /// <param name="dataProvider">Поставщик данных с заметками.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    public Task Initialize(IDataProvider<NoteEntity> dataProvider, CancellationToken stoppingToken);

    /// <summary>
    /// Дождаться освобождения блокировки токенизатора и вернуть значение флага инициализации.
    /// </summary>
    /// <param name="timeoutToken">Токен отмены.</param>
    /// <returns><b>true</b> - Инициализация завершена успешно, либо функционал отключен.</returns>
    /// <exception cref="OperationCanceledException">Запрошена отмена освобождения блокировки.</exception>
    public Task<bool> WaitWarmUp(CancellationToken timeoutToken);

    /// <summary>
    /// Флаг успешной инициализации токенизвтора.
    /// </summary>
    /// <returns><b>true</b> - Инициализация завершена успешно.</returns>
    public bool IsInitialized();

    /// <summary>
    /// Получить функционал для выбора алгоритмов поиска.
    /// </summary>
    /// <returns></returns>
    public IAlgorithmConfigurable GetTokenizerServiceCore();
}
