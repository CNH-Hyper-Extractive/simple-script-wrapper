function[] = sswFinish()

    folderPath = sswGetScriptFolder();
    f = mopen(sprintf('%s..\\ScilabOutput.txt', folderPath), 'a');

	mfprintf(f, 'sswFinish: ');
	values = sswGetOutput('outitem');
	for i=1:size(values,2)
		mfprintf(f, '%0.2f ', values(i));
	end
    mfprintf(f, '\n');
    
    mclose(f);

endfunction
