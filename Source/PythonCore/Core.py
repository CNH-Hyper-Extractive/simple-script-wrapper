class ModelBaseClass:
	def __init__(self):
		pass

#	def sswSetOutput(self,quantityName,elementId,value):
#		self.ExchangeOut[quantityName][elementId] = value;
#		pass

#	def sswGetInput(self,quantityName,elementId):
#		return self.ExchangeIn[quantityName][elementId];

	def sswSetOutput(self,quantityName,values):
		self.ExchangeOut[quantityName] = values;
		pass

	def sswGetInput(self,quantityName):
		return self.ExchangeIn[quantityName];

	def sswGetCurrentTime(self):
		# since sswCurrentTime is a C# DateTime object, we need to convert
		# it to a Python datetime object
		import datetime
		t = datetime.datetime(self.sswCurrentTime.Year, self.sswCurrentTime.Month, self.sswCurrentTime.Day, self.sswCurrentTime.Hour, self.sswCurrentTime.Minute, self.sswCurrentTime.Second)
		return t

	def sswGetScriptPath(self):
		return self.sswScriptPath;

	def sswGetStartTime(self):
		return self.sswStartTime;
		
	def sswGetTimeStep(self):
		return self.sswTimeStep;
		
	def getdata(self, quantityName):
		return self.ExchangeOut[quantityName]
		
	def putdata(self, quantityName, values):
		self.ExchangeIn[quantityName] = values
		return 1
		
	def sswInitialize(self):
		self.Initialize();
		pass
		
	def sswPerformTimeStep(self):
		self.PerformTimeStep();
		pass

	def sswFinish(self):
		self.Finish();
		pass
