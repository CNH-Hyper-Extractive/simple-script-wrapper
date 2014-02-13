function [cropWaterUse] = CalcCropWaterUse()
    global globalSiteCount

    % use a constant water demand of 558.0 mm/year (22 inches)
    cropWaterUse = ones(1, globalSiteCount) * 558.0

    % returns a row of values, one for each spatial element
    
return
