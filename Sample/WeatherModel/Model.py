#
# This script contains the actual model code. It must extend the ModelBaseClass
# and implement the 3 essential methods: sswInitialize, sswPerformTimeStep, and
# sswFinish.
#

import sys
sys.path.append(r"C:\Python27\Lib")
from ModelBase import ModelBaseClass

# anything before the class definition is ignored by the SSW
class ModelClass(ModelBaseClass):
	def __init__(self):
		pass

        globalSiteCount = 4

	def sswInitialize(self):

		# create the output file
		filePath = self.sswGetScriptPath()
		f = open(filePath + "Output.csv", "w")
		f.write("Year");
		for index in range(self.globalSiteCount):
        		f.write(",Precip" + str(index+1) + "(mm)")
        	f.write("\r\n");
		f.close()

		pass

        def GeneratePrecip(self):

		# generate a random precipitation value (mm) between 19 and 23 inches
		P = range(self.globalSiteCount)
		import random
		for index in range(self.globalSiteCount):
                        P[index] = (random.random() * 90.0) + 500.0

                return P

	def sswPerformTimeStep(self):

                # generate some random precipitation
                P = self.GeneratePrecip()

		# give the SSW the output so that it can give it to other
		# components as necessary
		self.sswSetOutput("Precipitation", P)

		# save this step's output to a file
		self.WriteOutput(P)

		pass

	def WriteOutput(self, values):

		# get the current time  
		t = self.sswGetCurrentTime()

		# calculate the year that the output represents
		import datetime
		simYear = t.year + 1

		# write the output of this step
		filePath = self.sswGetScriptPath()
		f = open(filePath + "Output.csv", "a")
		f.write(str(simYear))
		for index in range(self.globalSiteCount):
                        f.write("," + str(values[index]))
                f.write("\n");
		f.close()

		pass

	def sswFinish(self):

		pass
