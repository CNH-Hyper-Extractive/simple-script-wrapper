
folderPath = sswGetScriptFolder();
f = mopen(sprintf('%s..\\ScilabOutput.txt', folderPath), 'w');

mfprintf(f, 'sswInitialize: ');
mfprintf(f, 'Step length: %d, ', sswGetTimeStep());
dvec = sswGetStartTime();
mfprintf(f, 'Start Time: %04d/%02d/%02d %02d:%02d:%02d\r\n', dvec(1), dvec(2), dvec(3), dvec(4), dvec(5), dvec(6));

mclose(f);
