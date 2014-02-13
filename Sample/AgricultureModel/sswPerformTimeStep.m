
% Get the inputs from the groundwater model for the current time step only.
% When running standalone, the sswGetInput placeholder script calculates
% these. When running in OpenMI, the SSW collects the data from the other
% linked model and provides it here. The sseGetInput function always
% returns a row.
P = sswGetInput('Precipitation');

% calculate the crop water demand for the time step (1 year)
C = CalcCropWaterUse();

% calculate the amount of water that must be pumped from the groundwater
% to meet the crop water demand
Q = C - P;

% if there is sufficient precipitation, then there is no pumping
for i=1:size(P,2)
    if(Q(i) < 0.0)
        Q(i) = 0.0;
    end
end
  
% convert pumping from mm to m^3 assuming a county area of about 2000
% square kilometers
Q = (Q / 1000.0) * 45000.0 * 45000.0;
 
% Save the outputs from this time step (i.e. the column that corresponds
% to the current year). When running standalone, the values are saved in
% in a global variable. When running in OpenMI, the values are stored
% inside the SSW. Note that the sswSetOutput function always expects a
% single row.
sswSetOutput('Pumping', Q);

% save this step's output to a file
WriteOutput(C,P,Q);
