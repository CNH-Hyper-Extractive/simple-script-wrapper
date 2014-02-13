
% Returns the current simulation year. This is a placeholder for running
% the model standalone (unlinked). It will be overridden when running via
% OpenMI. The current time is returned as a datevec-compatible date
% vector.
function[currentTime] = sswGetCurrentTime()
    global globalYear;
    currentTime = [globalYear,01,01,0,0,0,0];
return
