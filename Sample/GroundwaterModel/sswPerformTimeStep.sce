
//
// This function is called whenever the SSW needs the model to advance forward
// in simulation time.
//
function[] = sswPerformTimeStep()

    // declare our global storage variable
    global S;

    // Get the inputs from the other components for the current time step only.
    // When running standalone, the sswGetInput placeholder script calculates
    // these. When running in OpenMI, the SSW collects the data from the other
    // linked model and provides it here. Input/output arrays are stored as a
    // single row per quantity.

    Q = sswGetInput('Pumping');

    R = ones(1, size(Q,'c')) * 24.5;

    // convert recharge from mm to m^3 assuming a county area of about 2000
    // square kilometers
    R = (R / 1000.0) * 45000.0 * 45000.0;

    // update the global storage variable
    S = S - Q + R;

    // give the SSW the output so that it can give it to other
    // components as necessary (note the transpose again)
    sswSetOutput('Storage', S);

    // save this step's output to a file
    WriteOutput(S);  

endfunction


//
// Save the output of this time step to a file.
//
function [] = WriteOutput(value)
    
  // get the current time  
  t = sswGetCurrentTime();
  
  // calculate the year that this output represents (i.e. the next year)
  year = t(1) + 1;
  
  // assume all quantities have the same number of elements
  elementCount = size(value, 'c');

  // write the output of this step
  folderPath = sswGetScriptFolder();  
  f = mopen(sprintf('%sOutput.csv', folderPath), 'a');
  mfprintf(f, '%d', year);
  for(i=1:elementCount)
    mfprintf(f, ',%f', value(i));
  end
  mfprintf(f, '\n');
  mclose(f);

endfunction
