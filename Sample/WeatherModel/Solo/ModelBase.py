#
# This script provides a placeholder for the ModelBaseClass which is generated
# by the SSW when running the model via the SSW.
#

import sys
sys.path.append(r"C:\Python27\Lib")

class ModelBaseClass:
	def __init__(self):
		pass

        globalYear = 0

	def sswSetOutput(self,quantityName,value):
		pass

	def sswGetInput(self,quantityName):
		return 0

	def sswGetCurrentTime(self):
		import datetime
		return datetime.datetime(self.globalYear, 1, 1, 0, 0, 0)

	def sswGetScriptPath(self):
		return "./"
