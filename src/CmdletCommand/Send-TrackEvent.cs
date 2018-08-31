using System;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;

namespace ApplicationInsights_PowerShell
{
    /* "My event" | Send-TrackEvent -Passthru | Write-Output */
    [Cmdlet(VerbsCommunications.Send,"TrackEvent")]
    public class SendTrackEventCmdletCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public String EventName { get; set; }

        [Parameter(
            Mandatory = false)]
        public SwitchParameter Passthru { get; set; }        

        [Parameter(
            Mandatory = false)]
        public String FunctionName { get; set; }

        private TelemetryClient _telemetryClient;

        private string _functionName;

        private StringBuilder _stringBuilder;
        private IDictionary<string,string> _properties;        

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            _telemetryClient = MyTelemetryClient.getInstance();
            _stringBuilder   = new StringBuilder();
            _properties      = new Dictionary<string,string>();
            _functionName    = FunctionName;
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            //WriteVerbose("Process!");
            _telemetryClient.TrackEvent(
                EventName
            );
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            //WriteObject();
            base.EndProcessing();
            if(Passthru.IsPresent)
            {
                WriteObject(EventName);
            }
        }
    }
}