namespace SearchEngine.Tools.DumpAssistant;

/// <summary/> Контракт конвертации дампа
internal interface IDbConvertor
{
    /// <summary>
    /// Конвертироать дамп, из первой схемы во вторую
    /// </summary>
    /// <param name="ddlFrom">DDL источника</param>
    /// <param name="ddlTo">DDL эталона</param>
    /// <param name="pathToDump">путь к файлу дампа</param>
    internal void Convert(object ddlFrom, object ddlTo, string pathToDump);
}
