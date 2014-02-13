
function[values] = sswGetInput(quantityName)
    global globalSiteCount;
    
    if(quantityName == 'Pumping')
    
        // Return a constant value for all spatial elements. This
        // function always returns a single row for the requested
        // quantity.
        values = ones(1, globalSiteCount) * 52065.0;
    end

endfunction
