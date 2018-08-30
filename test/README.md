
Important note on running tests:

Once ApplicationInsights_PowerShell.dll is loaded into the existing PS-session it cannot be unloaded again this means new builds will not be imported.
run a task (ctrl+shift+p) and enter:
Debug: Select and Start Debugging
select:
PowerShell Launch Current File in Temporary Console
Subsequent runs you can just hit F5
the temporary console is pretty limited, you cannot run single lines or enter any commands. it is a one trick pony, run the script. full stop.