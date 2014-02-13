
global globalSiteCount;
globalSiteCount = 4;

% create the output file
folderPath = sswGetScriptFolder();
file = fopen(sprintf('%sOutput.csv', folderPath), 'w');
fprintf(file, 'Year');  
for i=1:globalSiteCount
    fprintf(file, ',WaterUse%d(mm),Precip%d(mm),Pumped%d(m^3)', i, i, i);  
end
fprintf(file, '\r\n');  
fclose(file);

return
