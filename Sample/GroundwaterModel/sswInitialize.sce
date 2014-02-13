
//
// This function is called once at the start of the simulation.
//
function[] = sswInitialize()
    
  // initialize our global variable for storage (m^3), assuming a county area of
  // about 2000 square kilometers with a saturated thickness of 10 meters
  global S;
  S = 10.0 * 45000.0 * 45000.0;
  
  global globalSiteCount;
  globalSiteCount = 4;
  
  // create the output file
  folderPath = sswGetScriptFolder();  
  f = mopen(sprintf('%sOutput.csv', folderPath), 'w');
  mfprintf(f, 'Year');
  for i=1:globalSiteCount
  mfprintf(f, ',Storage%d(m^3)', i);
  end
  mfprintf(f, '\n');
  mclose(f);

endfunction
