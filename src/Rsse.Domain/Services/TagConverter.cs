using System.Collections.Generic;

namespace SearchEngine.Services;

/// <summary>
/// Функционал для создания списка флагов из тегов.
/// </summary>
public static class TagConverter
{
    /// <summary>
    /// Создать представление тегов заметки и всех тегов из репо в виде списка флагов.
    /// </summary>
    /// <param name="noteTagIds">Теги заметки.</param>
    /// <param name="totalTagsCount">Общее количество тегов в репозитории.</param>
    /// <returns></returns>
    public static List<bool> AllToFlags(List<int> noteTagIds, int totalTagsCount)
    {
        var checkboxes = new List<bool>();

        for (var i = 0; i < totalTagsCount; i++)
        {
            checkboxes.Add(false);
        }

        foreach (var tagId in noteTagIds)
        {
            checkboxes[tagId - 1] = true;
        }

        return checkboxes;
    }
}
