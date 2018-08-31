using System;
using System.Text;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;

namespace ApplicationInsights_PowerShell
{
    public class MyTelemetryClient
    {
        private static TelemetryClient _telemetryClient = new TelemetryClient();

        public static TelemetryClient getInstance()
        {
            /*
            var gameTelemetry = new TelemetryClient();
            gameTelemetry.Context.Properties["Game"] = currentGame.Name;
            // Now all telemetry will automatically be sent with the context property:
            gameTelemetry.TrackEvent("WinGame");
            */

            return _telemetryClient;
        }

        /* TODO: overload this method */
        public static void PopulateProperties(IDictionary<string,string> properties, String scriptName, int scriptLineNumber, String functionName, String stackTrace, Exception exception)
        {

            if(!string.IsNullOrEmpty(scriptName))
            {
                properties.Add("ScriptName", scriptName);
            }

            if(scriptLineNumber > 0)
            {
                properties.Add("ScriptLineNumber", "" + scriptLineNumber);
            }  

            if(!String.IsNullOrEmpty(functionName))
            {
                properties.Add("FunctionName", functionName);
            }   

            if(!String.IsNullOrEmpty(stackTrace))
            {
                properties.Add("StackTrace", stackTrace);
            }   

            if(exception != null)
            {
                properties.Add("ExceptionType", exception.GetType().ToString());
            }
        } // end PopulateProperties 

        public static void PopulateProperties(IDictionary<string,string> properties, String functionName)
        {
            MyTelemetryClient.PopulateProperties(properties, null, -1, functionName, null, null);
        } // end PopulateProperties                
    }
}