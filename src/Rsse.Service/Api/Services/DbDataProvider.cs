using System.Collections.Generic;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Entities;

namespace Rsse.Api.Services;

/// <summary>
/// Компонент поставщика данных из базы, регистрировать как scoped-зависимость.
/// </summary>
// <param name="scopeFactory">Репозиторий.</param>
public sealed class DbDataProvider(IDataRepository repo) : IDataProvider<NoteEntity>
{
    /// <inheritdoc/>
    public IAsyncEnumerable<NoteEntity> GetDataAsync()
    {
        var allNotes = repo.ReadAllNotes();

        return allNotes;
    }
}
