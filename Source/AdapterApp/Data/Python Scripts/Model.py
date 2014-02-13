import sys
sys.path.append(r"C:\Python27\Lib")
from ModelBase import ModelBaseClass

# anything before the class definition is ignored by the SSW
class ModelClass(ModelBaseClass):
	def __init__(self):
		pass

	def sswInitialize(self):

		import random
		random.random();

		startTime = self.sswGetStartTime()
		f = open(self.sswScriptPath + '\\..\\PythonOutput.txt', 'w')
		f.write('sswInitialize: ');
		f.write('Step Length: ' + str(self.sswGetTimeStep()) + ', ')
		f.write('Start Time: ' + str(startTime.Year) + '/' + str(startTime.Month) + '/' + str(startTime.Day) + ' ' + str(startTime.Hour) + ':' + str(startTime.Minute) + ':' + str(startTime.Second) + '\r\n')
		f.close()
		
		pass

	def sswPerformTimeStep(self):
                for index in range(len(self.ExchangeIn['initem'])):
                        self.ExchangeOut['outitem'][index] = self.ExchangeIn['initem'][index] + 10
		
		currentTime = self.sswGetCurrentTime()
		f = open(self.sswScriptPath + '\\..\\PythonOutput.txt', 'a')
		f.write('sswPerformTimeStep: ' + str(currentTime.year) + '/' + str(currentTime.month) + '/' + str(currentTime.day) + ' ' + str(currentTime.hour) + ':' + str(currentTime.minute) + ':' + str(currentTime.second) + '\r\n')
		f.close()

		pass

	def sswFinish(self):
		f = open(self.sswScriptPath + '\\..\\PythonOutput.txt', 'a')

		f.write('sswFinish: ');
		for quantityName in self.ExchangeOut.keys():
                        for index in range(len(self.ExchangeOut[quantityName])):
				f.write(str(self.ExchangeOut[quantityName][index]) + ' ')
			f.write('\r\n');
			
		f.close()
		pass

