using RandomSongSearchEngine.Service.Models;

namespace RandomSongSearchEngine.Infrastructure.Logger;

public static class FullLog
{
    /// <summary>
    /// Логгирует Id модели, треда и http-контекста
    /// </summary>
    /// <param name="logger">Логгер</param>
    /// <param name="model">Модель, из которой вызывается метод</param>
    public static void LogId(this ILogger logger, ReadModel model)
    {
        var modelId = model.GetHashCode();
        
        // костыли - у модели сейчас нет контекста
        // model.HttpContext.GetHashCode();
        const string httpContextId = "MockContext";
        
        var threadId = Environment.CurrentManagedThreadId;
        
        logger.LogInformation("[Model ID: {Model} Thread ID: {Thread} HttpContextID: {Context}]", modelId, threadId, httpContextId);
    }
}