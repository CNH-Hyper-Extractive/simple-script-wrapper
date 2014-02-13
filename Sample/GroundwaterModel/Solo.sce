
clear all;

exec('sswInitialize.sce');
exec('sswPerformTimeStep.sce');
exec('sswFinish.sce');

// Include the folder of placeholder scripts since we're running stand-
// alone (not linked). When running via OpenMI, the ssw functions will be
// provided automatically.
exec('Solo/sswGetInput.sce');
exec('Solo/sswSetOutput.sce');
exec('Solo/sswGetScriptFolder.sce');
exec('Solo/sswGetCurrentTime.sce');

// These global variables are used to store some state of the model
// that is handled by the SSW. Note that only the placeholder functions
// (the ones in the Solo folder) access these.
global globalYear;

// Tell the groundwater model to prepare for a simulation.
sswInitialize();

// Tell the groundwater model to simulate each year.
for globalYear=2011:2040
    sswPerformTimeStep();
end;

// Tell the groundwater model the simulation is over.
sswFinish();
