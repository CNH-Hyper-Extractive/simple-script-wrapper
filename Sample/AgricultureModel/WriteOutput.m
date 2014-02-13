
function[] = WriteOutput(value1, value2, value3)

  % get the current time  
  simTime = sswGetCurrentTime();

  % calculate the day that the output represents
  simYear = simTime(1) + 1;

  % assume all quantities have the same number of elements
  elementCount = size(value1,2);
    
  % write the output of this step
  folderPath = sswGetScriptFolder();
  file = fopen(sprintf('%sOutput.csv', folderPath), 'a');
  fprintf(file, '%d', simYear);  
  for i=1:elementCount
    fprintf(file, ',%f,%f,%f', value1(i), value2(i), value3(i));  
  end
  fprintf(file, '\r\n');  
  fclose(file);
    
return
  