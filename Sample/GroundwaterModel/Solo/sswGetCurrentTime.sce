function [currentTime] = sswGetCurrentTime()
    global globalYear;
    
    currentTime = [globalYear,1,1,0,0,0];
endfunction