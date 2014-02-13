%
% This script is used to execute the model by iteself (i.e. not via the SSW).
%

clear;
clear global;

% This is used by the placeholder scripts to keep track of the simulation
% time. It is not accessed by the crop choice model directly, instead it is
% accessed via the sswGetCurrentTime function.
global globalYear;

% Include the folder of placeholder scripts since we're running stand-
% alone (not linked). When running via OpenMI, the ssw functions will be
% provided automatically.
addpath('Solo');

% Tell the crop choice model to prepare for a simulation.
sswInitialize();

% Tell the crop choice model to simulate each year.
for globalYear=2011:2040
    sswPerformTimeStep();
end;

% Tell the crop choice model the simulation is over.
sswFinish();
