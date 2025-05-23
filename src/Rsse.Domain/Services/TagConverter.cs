using System.Collections.Generic;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;

namespace SearchEngine.Services;

/// <summary>
/// Функционал для создания списка флагов из тегов.
/// </summary>
public static class TagConverter
{
    /// <summary>
    /// Конвертировать все теги из репо в список флагов.
    /// </summary>
    /// <param name="repo">Репозиторий.</param>
    /// <param name="tagsCount">Общее количество тегов в репозитории.</param>
    /// <param name="originalNoteId">Идентификатор заметки, отмеченной тегами.</param>
    /// <returns></returns>
    public static async Task<List<bool>> ConvertToFlags(IDataRepository repo, int tagsCount, int originalNoteId)
    {
        var checkboxes = new List<bool>();

        for (var i = 0; i < tagsCount; i++)
        {
            checkboxes.Add(false);
        }

        var originalNoteTags = await repo.ReadNoteTagIds(originalNoteId);

        foreach (var i in originalNoteTags)
        {
            checkboxes[i - 1] = true;
        }

        return checkboxes;
    }
}
