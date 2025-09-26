using Microsoft.AspNetCore.Mvc;
using Core.MemoryManagement;
using JSON;
using Core.Machine;
using Microsoft.AspNetCore.Cors;

namespace WebAbstract.Csontrollers
{
    [EnableCors("MachineMetricsCors")]
    [Route("machineMetrics")]
    public class MachineMetricsController : ControllerBase
    {
        static MachineMetricsController() { 
        }
        public MachineMetricsController() :base(){
        }
        [HttpGet]
        [Route("memory")]
        public ActionResult MemoryMetrics()
        {
            MemoryMetrics memoryMetrics = MemoryHelper.GetMemoryMetricsNow();
            string content = Json.Serialize(memoryMetrics);
            return new ContentResult
            {
                ContentType = "application/json",
                Content = content
            };
        }
        [HttpGet]
        [Route("processor")]
        public ActionResult ProcessorMetrics()
        {
            ProcessorMetrics processorMetrics = ProcessorMetricsSource.Instance.GetProcessorMetricsCached();
            string content = Json.Serialize(processorMetrics);
            return new ContentResult
            {
                ContentType = "application/json",
                Content = content
            };
        }
        [HttpGet]
        [Route("memoryProcessor")]
        public ActionResult Metrics()
        {
            ProcessorMetrics processorMetrics = ProcessorMetricsSource.Instance.GetProcessorMetricsCached();
            MemoryMetrics memoryMetrics = MemoryHelper.GetMemoryMetricsNow();
            MachineMetrics metrics = new MachineMetrics(memoryMetrics, processorMetrics);
            string content = Json.Serialize(metrics);
            return new ContentResult
            {
                ContentType = "application/json",
                Content = content
            };
        }
        [HttpGet]
        [Route("gc")]
        public ActionResult GarbageCOllect()
        {
            GC.Collect();
            return new ContentResult
            {
                ContentType = "text/html",
                Content = "Did Garbage Collect"
            };
        }
    }
}