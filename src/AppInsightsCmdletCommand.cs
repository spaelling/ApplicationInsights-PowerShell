using System;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;

namespace ApplicationInsights_PowerShell
{
    public class MyTelemetryClient
    {
        private static TelemetryClient _telemetryClient = new TelemetryClient();

        public static TelemetryClient getInstance()
        {
            return _telemetryClient;
        }
    }

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

        private TelemetryClient _telemetryClient;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            //WriteVerbose("Begin!");
            _telemetryClient = MyTelemetryClient.getInstance();
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

    //Send-AIException -Exception $_
    [Cmdlet(VerbsCommunications.Send,"TrackException")]
    [OutputType(typeof(Nullable))]
    public class SendTrackExceptionCmdletCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true, /* alot depends on this being mandatory */
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public ErrorRecord ErrorRecord { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 1,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public String Message { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 2,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = false)]
        public InvocationInfo MyInvocation { get; set; } /* why this warning? what is hiding what??? */

        [Parameter(
            Mandatory = false)]
        public SwitchParameter Passthru { get; set; }

        [Parameter(
            Mandatory = false)]
        public String FunctionName { get; set; }

        private TelemetryClient _telemetryClient;
        private Exception _exception;

        private string _functionName, _scriptName, _line, _customMessage, _stackTrace;
        private int _scriptLineNumber;
        private StringBuilder _stringBuilder;
        private IDictionary<string,string> _properties;

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            _telemetryClient = MyTelemetryClient.getInstance();
            _stringBuilder   = new StringBuilder();
            _properties      = new Dictionary<string,string>();

            // if we have $MyInvocation we can get the function name - it is not in the ErrorRecord somewhere?
            // user can also provide a function name as input, which takes precedence
            _functionName     = String.IsNullOrEmpty(FunctionName) ? (MyInvocation != null ? MyInvocation.MyCommand.ToString() : null) : FunctionName;

            // if function name is greater than 256 characters or contains a space then it may come from
            // a runspace (ex. Azure Function App)
            if(_functionName.Length > 256 || _functionName.Contains(" "))
            {
                WriteVerbose("_functionName invalid (too long or contains spaces), are you calling from a runspace?");
                _functionName = String.IsNullOrEmpty(FunctionName) ? null : FunctionName;
            }

            // we put these into the properties dictionary
            _scriptLineNumber = ErrorRecord.InvocationInfo.ScriptLineNumber;
            _scriptName       = ErrorRecord.InvocationInfo.ScriptName;
            _line             = ErrorRecord.InvocationInfo.Line;
            _stackTrace       = ErrorRecord.Exception.StackTrace;
            _exception        = ErrorRecord.Exception;

            _customMessage    = String.IsNullOrEmpty(Message) ? ErrorRecord.Exception.Message : Message;            
        } // end BeginProcessing

        private void PopulateProperties()
        {

            if(!String.IsNullOrEmpty(_scriptName))
            {
                _properties.Add("ScriptName", _scriptName);
            }

            if(_scriptLineNumber > 0)
            {
                _properties.Add("ScriptLineNumber", "" + _scriptLineNumber);
            }  

            if(!String.IsNullOrEmpty(_functionName))
            {
                _properties.Add("FunctionName", _functionName);
            }   

            if(!String.IsNullOrEmpty(_stackTrace))
            {
                _properties.Add("StackTrace", _stackTrace);
            }   

            _properties.Add("ExceptionType", _exception.GetType().ToString());
        } // end PopulateProperties
        private string ExceptionMessage()
        {
            // TODO: allow user to format the message
            string ExceptionType = _exception.GetType().ToString();

            if(!String.IsNullOrEmpty(_scriptName))
            {
                _stringBuilder.Append(ExceptionType + " in script: ");
                _stringBuilder.Append(_scriptName);
                _stringBuilder.Append("\n");
            }
            else
            {
                _stringBuilder.Append(ExceptionType + " raised\n");
            }

            if(!String.IsNullOrEmpty(_functionName))
            {
                _stringBuilder.Append("Function: ");
                _stringBuilder.Append(_functionName);
                _stringBuilder.Append("\n");
            }

            if(_scriptLineNumber > 0)
            {
                _stringBuilder.Append("Line number: ");
                _stringBuilder.Append(_scriptLineNumber);
                _stringBuilder.Append("\n");
            }  

            _stringBuilder.Append("Error description: ");
            _stringBuilder.Append(_customMessage);
            _stringBuilder.Append("\n");       

            return _stringBuilder.ToString();
        } // end ExceptionMessage

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            WriteVerbose("Populating properties...");          
            PopulateProperties();

            string msg = ExceptionMessage();
            WriteVerbose("\nException message:\n" + msg);
            
            // often of type RuntimeException in PowerShell?
            WriteVerbose("Exception is of type " + _exception.GetType());

            /*
            TODO: look at type of ErrorRecord.Exception and create exception of same type.
            Can maybe use some reflection to do this?
            Activator.CreateInstance ?
            maybe cast to the respective type?
            

            If we just use ErrorRecord.Exception instead of creating a new Exception
            it is always System.Exception in the AI portal
            and the message is: Exception of type 'System.Exception' was thrown
            this is why we are rolling our own here
             */
            Exception exception = new Exception(
                msg
            );

            /*
            TODO: 
            Failed method - is in the portal, how do we input that?
             */
                        
            WriteVerbose("Tracking exception");
            _telemetryClient.TrackException(
                exception,
                _properties
            );
        } // end ProcessRecord

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            //WriteObject();
            base.EndProcessing();
            if(Passthru.IsPresent)
            {
                WriteObject(ErrorRecord);
            }
        } // end EndProcessing
    } // end SendTrackExceptionCmdletCommand

    /* the stacktrace does not appear in the "call stack" in AI */
    class CustomException : Exception
    {
        private string _stackTrace;
        public CustomException(string message, string stackTrace) : base(message)
        {
            this._stackTrace = stackTrace;
        }
        public override string StackTrace
        {
            get
            {
                return this._stackTrace;
            }
        }
    } // end CustomException
} // end namespace