
folderPath = sswGetScriptFolder();
file = fopen(sprintf('%s..\\MatlabOutput.txt', folderPath), 'a');

fprintf(file, 'sswFinish: ');
values = sswGetOutput('outitem');
for i=1:size(values,2)
	fprintf(file, '%0.2f ', values(i));
end
fprintf(file, '\n');
    
fclose(file);
