namespace SearchEngine.Tools.DevelopmentAssistant;

/// <summary>
/// Выбор контроля за процессом с dev-сервером
/// </summary>
internal enum DevServerControl
{
    /// <summary>
    /// Процесс контролируется разработчиком и будет запущен в отдельном окне
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Процесс контролируется IDE и будет запущен из её терминала
    /// </summary>
    Ide = 1
}
