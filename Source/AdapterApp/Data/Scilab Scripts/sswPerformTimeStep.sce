
function[] = sswPerformTimeStep()
  
    folderPath = sswGetScriptFolder();
    f = mopen(sprintf('%s..\\ScilabOutput.txt', folderPath), 'a');

    dvec = sswGetCurrentTime();
    mfprintf(f, 'sswPerformTimeStep: %04d/%02d/%02d %02d:%02d:%02d\r\n', dvec(1), dvec(2), dvec(3), dvec(4), dvec(5), dvec(6));    
    mclose(f);

	values = sswGetInput('initem');

	values = values + 10;

	sswSetOutput('outitem', values);

endfunction
