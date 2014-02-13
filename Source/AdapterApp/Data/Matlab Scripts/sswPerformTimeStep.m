
folderPath = sswGetScriptFolder();
file = fopen(sprintf('%s..\\MatlabOutput.txt', folderPath), 'a');

dvec = sswGetCurrentTime();
fprintf(file, 'sswPerformTimeStep: %04d/%02d/%02d %02d:%02d:%02d\r\n', dvec(1), dvec(2), dvec(3), dvec(4), dvec(5), dvec(6));    
fclose(file);

values = sswGetInput('initem');

values = values + 10;

sswSetOutput('outitem', values);
