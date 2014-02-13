
function [values] = sswGetInput(quantityName)
global globalSiteCount;

values = ones(1, globalSiteCount);

if(strcmp(quantityName, 'Precipitation') == 1)
    values = values * 25.0;
end

return
