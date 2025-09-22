using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;
using Core.Timing;
using Nodes;
using Core.InterserverComs;
using JSON;
using Logging;
using InterserverComs;

namespace WebAPI.Controllers
{
    [Route("nodes")]
    public class NodesController : ControllerBase
    {
        [HttpGet]
        [Route("testInterserverConnections")]
        public ContentResult TestInterserverConnections()
        {
            string content = null;
            try
            {
                content = Json.Serialize(InterserverPort.Instance.GetConnectionsStates());
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
            return new ContentResult
            {
                ContentType = "text/html",
                Content = content
            };
        }
        [HttpGet]
        [Route("nodeEndpointStats")]
        public ContentResult NodeEndpointStates()
        {
            string content = null;
            try
            {
                NodeEndpointStatesHistory.AccessEntriesInLock((Action<NodeEndpointState[]>)((history) => {
                    NodeEndpointStatesResponse nodeEndpointStatesResponse = new NodeEndpointStatesResponse(
                            InterserverPort.Instance.GetNodeEndpointStates(),
                            history);
                    content = Json.Serialize<NodeEndpointStatesResponse>(nodeEndpointStatesResponse);
                }));

            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
            return new ContentResult
            {
                ContentType = "text/html",
                Content = content
            };
        }
        [HttpGet]
        [Route("stats")]
        public ContentResult Stats()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(System.Environment.MachineName);
            sb.AppendLine("OS Version:"+System.Environment.OSVersion.VersionString);
            sb.AppendLine("milliseconds UTC: "+TimeHelper.MillisecondsNow);
            GetBrowserTimeUTCScript(sb);
            sb.AppendLine((GC.GetTotalMemory(true)/1000000)+"megabyte(s) memory allocated");
            return new ContentResult
            {
                ContentType = "text/html",
                Content = sb.ToString()
            };
        }
        private void GetBrowserTimeUTCScript(StringBuilder sb) {
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("const timeElement=document.createElement('div');");
            sb.Append("document.body.appendChild(timeElement);");
            sb.Append("timeElement.innerTML='browser time: '+new Date.getTime();");
            sb.Append("</script>");
        }

    }
}