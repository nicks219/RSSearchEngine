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
    public Task Create(int id, TextRequestDto note);

    /// <summary>
    /// Вычислить индексы соответствия хранимых заметок поисковому запросу.
    /// </summary>
    /// <param name="text">Текст для поиска соответствий.</param>
    /// <returns>Идентификаторы заметок и их индексы соответствия.</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text);

    /// <summary>
    /// Обновить вектор для заметки.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    /// <param name="note">Текстовая нагрузка заметки.</param>
    public Task Update(int id, TextRequestDto note);

    /// <summary>
    /// Удалить вектор для заметки по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    public Task Delete(int id);

    /// <summary>
    /// Инициализация функционала токенизации.
    /// </summary>
    public Task Initialize(CancellationToken ct);

    /// <summary>
    /// Дождаться освобождения блокировки токенизатора и вернуть значение флага инициализации.
    /// При отмене задачи на старте вызова метод также вернёт <b>true</b> для предсказуемого использования в цикле.
    /// Логика отмены используется для пользовательского прерывания тестов.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns><b>true</b> - Инициализация завершена, либо сразу запрошена отмена.</returns>
    /// <exception cref="OperationCanceledException">Запрошена отмена на ожидании блокировки.</exception>
    public Task<bool> WaitWarmUp(CancellationToken ct);
}
