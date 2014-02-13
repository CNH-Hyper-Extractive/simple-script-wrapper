
folderPath = sswGetScriptFolder();
file = fopen(sprintf('%s..\\MatlabOutput.txt', folderPath), 'w');

fprintf(file, 'sswInitialize: ');
fprintf(file, 'Step length: %d, ', sswGetTimeStep());
dvec = sswGetStartTime();
fprintf(file, 'Start Time: %04d/%02d/%02d %02d:%02d:%02d\r\n', dvec(1), dvec(2), dvec(3), dvec(4), dvec(5), dvec(6));

fclose(file);

