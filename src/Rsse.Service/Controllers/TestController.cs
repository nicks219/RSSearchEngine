using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SearchEngine.Controllers;

/// <summary>
/// Контроллер с системным функионалом для проверок
/// </summary>

[Route("test")]

public class TestController : Controller
{
    private static int _counter;
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Получить версию сервиса
    /// </summary>
    /// <returns></returns>
    [HttpGet("get/version")]
    public ActionResult GetVersion()
    {
        return Ok("v5.2.4: .NET8/React18");
    }

    // TODO последующие ручки переписать либо удалить:

    #region Get Performance Grade

    // функционал для оценки производительности/троттлинга:
    private static Thread? _thread;
    private static readonly StringBuilder Builder = new();
    private static bool _loop;
    private static int _timeCounter;

    private const int Count = 1000 * 300;
    // объект на ~2mb траффика, измерено Fiddler:
    private static readonly int[] HugeObject = Enumerable.Range(1, Count).ToArray();

    // мотивация: вернуть большой объект в ответе
    [Authorize, HttpGet("get/object")]
    public ActionResult GetHugeObject([FromQuery] bool more = false)
    {
        if (!more)
        {
            return Ok(
                new
                {
                    Result = HugeObject
                });
        }

        return Ok(
            new
            {
                Result1 = HugeObject,
                Result2 = HugeObject,
                Result3 = HugeObject
            });
    }

    // мотивация: запустить создание строки
    [Authorize, HttpGet("task/start")]
    public ActionResult StartHugeStringCreation()
    {
        _thread = new Thread(IncrementalStringBuilding);

        _thread.Start();

        return Ok("Counter start \r\n");
    }

    // мотивация: остановить создание строки и получить её в ответе
    [Authorize, HttpGet("task/stop")]
    public ActionResult StopHugeStringCreation()
    {
        _loop = false;

        var result = Builder.ToString();

        _logger.LogError("Time: {Time}", result);

        Builder.Clear();

        _timeCounter = 0;

        return Ok($"Counter stop: {result} \r\n");
    }

    #endregion

    // мотивация: запустить сборку мусора
    [Authorize, HttpGet("task/gc")]
    public ActionResult GarbageCollectorCall()
    {
        GC.Collect(2, GCCollectionMode.Forced, true);

        GC.WaitForPendingFinalizers();

        return Ok("gc \r\n");
    }

    // мотивация: получить лог и информацию о хипе по запросу
    [Authorize, HttpGet("get/logs")]
    public ActionResult LogOnEveryRequest()
    {
        var info = GC.GetGCMemoryInfo();

        _logger.LogError("[Test] " +
                         "C:{Counter} " +
                         "Time: {Time} " +
                         "Mem1+:{Memory} " +
                         "Mem2+:{Allocated} " +
                         "MemeLastGC:{MemoryLoad} " +
                         "Heap:{Heap}",
            _counter,
            DateTime.Now.Minute + ":" + DateTime.Now.Second,
            GC.GetTotalMemory(false),
            GC.GetTotalAllocatedBytes(),
            info.MemoryLoadBytes,
            info.HeapSizeBytes);


        Interlocked.Increment(ref _counter);

        return Ok(new
        {
            Heap = info.HeapSizeBytes
        });
    }

    // мотивация: получить лог по N-му запросу, и информацию о хипе
    [Authorize, HttpGet("get/log")]
    public ActionResult LogOnEveryNthRequest([FromQuery] int count = 100)
    {
        var info = GC.GetGCMemoryInfo();

        if (_counter % count == 0)
        {
            _logger.LogError("[Test100] " +
                             "C:{Counter} " +
                             "Time: {Time} " +
                             "Mem1+:{Memory} " +
                             "Mem2+:{Allocated} " +
                             "MemeLastGC:{MemoryLoad} " +
                             "Heap:{Heap}",
                _counter,
                DateTime.Now.Minute + ":" + DateTime.Now.Second,
                GC.GetTotalMemory(false),
                GC.GetTotalAllocatedBytes(),
                info.MemoryLoadBytes,
                info.HeapSizeBytes);
        }

        Interlocked.Increment(ref _counter);

        return Ok(new
        {
            Heap = info.HeapSizeBytes
        });
    }

    // мотивация: получить ответ от синхронной ручки
    [Authorize, HttpGet("live/sync")]
    public string GetLive()
    {
        return "live";
    }

    // мотивация: получить ответ от асинхронной ручки
    [Authorize, HttpGet("live/async")]
    public async Task<string> GetLiveAsync()
    {
        await Task.Delay(1);
        return "live.async";
    }

    // мотивация: получить ответ от асинхронной ручки
    [Authorize, HttpGet("live/task")]
    public Task<string> GetLiveAsTask()
    {
        return Task.FromResult<string>("live.task");
    }

    // увеличивать строку вплоть до оствновки цикла
    private static void IncrementalStringBuilding()
    {
        _loop = true;

        while (_loop)
        {
            Builder.Append(_timeCounter + " - " + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + _timeCounter++ % 10 + " \r\n");

            Thread.Sleep(100);
        }
    }
}
