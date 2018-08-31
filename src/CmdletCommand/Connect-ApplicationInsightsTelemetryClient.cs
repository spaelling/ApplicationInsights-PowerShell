using System;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.ApplicationInsights;

namespace ApplicationInsights_PowerShell
{
    /// <summary>
    /// <para type="synopsis">This is the cmdlet synopsis.</para>
    /// <para type="description">This is part of the longer cmdlet description.</para>
    /// <para type="description">Also part of the longer cmdlet description.</para>
    /// </summary>    
    /// <example>
    ///   <para>This is part of the first example's introduction.</para>
    ///   <para>This is also part of the first example's introduction.</para>
    ///   <code>Connect-ApplicationInsightsTelemetryClient -InstrumentationKey 'KEY'</code>
    ///   <para>This is part of the first example's remarks.</para>
    ///   <para>This is also part of the first example's remarks.</para>
    /// </example>  
    [Cmdlet(VerbsCommunications.Connect,"ApplicationInsightsTelemetryClient")]
    //[OutputType(typeof(TelemetryClient))]
    public class ConnectApplicationInsightsTelemetryClientCmdletCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="inputType">String in the form of a guid</para>
        /// <para type="description">Instrumentation key from Application Insights resource</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Enter instrumentation key, e.g. f9a8f28c-3f5b-4e23-9ae1-934a52432647")]
        [Alias("key")]
        public String InstrumentationKey { get; set; }

        private TelemetryClient _telemetryClient;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            //WriteVerbose("Begin!");
            //StaticStuff.InstrumentationKey = InstrumentationKey;
            // create new telemetry client and add the instrumentation key
            _telemetryClient = MyTelemetryClient.getInstance();
            _telemetryClient.InstrumentationKey = InstrumentationKey;
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            //WriteVerbose("Process!");
            // need this when calling other cmdlets
            //MyTelemetryClient._telemetryClient = _telemetryClient;
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            //WriteVerbose("End!");
            // will expose the instrumentation key
            //WriteObject(_telemetryClient);
            base.EndProcessing();
        }
    }
}