using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace RandomSongSearchEngine.Controllers;

public class TestController : Controller
{
    private static int _counter;
    private readonly ILogger<TestController> _logger;
    
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    #region Throttle Counter
    
    /// <summary> счетчик производительности </summary>
    private static Thread? _thread;
    private static readonly StringBuilder Builder = new();
    private static bool _loop;
    private static int _timeCounter;

    /// <summary> большой объект (~2mb траффика, измерено Fiddler) </summary>
    private const int Count = 1000 * 300;
    private static readonly int[] Obj = Enumerable.Range(1, Count).ToArray();

    [HttpGet($"test.traffic")]
    public ActionResult GetMega([FromQuery] bool more = false)
    {
        // TODO: можно репортать время http запроса, используй DiagnosticSource
        if (!more)
        {
            return Ok(
                new
                {
                    Result = Obj
                });
        }

        return Ok(
            new
            {
                Result1 = Obj,
                Result2 = Obj,
                Result3 = Obj
            });
    }

    [HttpGet("start")]
    public ActionResult Start()
    {
        _thread = new Thread(Method);
        
        _thread.Start();

        return Ok("Counter start \r\n");
    }

    [HttpGet("stop")]
    public ActionResult Stop()
    {
        _loop = false;

        var result = Builder.ToString();
        
        _logger.LogError("Time: {Time}", result);
        
        // TODO: можно очистить файл лога
        Builder.Clear();

        _timeCounter = 0;
        
        return Ok($"Counter stop: {result} \r\n");
    }
    
    #endregion

    [HttpGet("gc")]
    public ActionResult GcCall()
    {
        GC.Collect(2, GCCollectionMode.Forced, true);
        
        GC.WaitForPendingFinalizers();

        return Ok("gc \r\n");
    }

    [HttpGet("test")]
    public ActionResult LogEveryRequest()
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

    [HttpGet("test.every.nth")]
    public ActionResult Get([FromQuery] int count = 100)
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
    
    [HttpGet("live")]
    public string Live()
    {
        return "live";
    }
    
    [HttpGet("live.async")]
    public async Task<string> LiveAsync()
    {
        await Task.Delay(1);
        return "live.async";
    }
    
    [HttpGet("live.task")]
    public Task<string> LiveTask()
    {
        return Task.FromResult<string>("live.task");
    }
    
    private static void Method()
    {
        _loop = true;

        while (_loop)
        {
            Builder.Append(_timeCounter + " - " + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + _timeCounter++%10 + " \r\n");

            Thread.Sleep(100);
        }
    }
}