#
# This script is used to execute the model by iteself (i.e. not via the SSW).
#

import sys
import os
sys.path.append(r"C:\Python27\Lib")
sys.path.append(os.path.abspath('Solo'))
from Model import ModelClass

def main():
        model = ModelClass()
        
        model.sswInitialize()

        for year in range(2011,2041):
                model.globalYear = year
                model.sswPerformTimeStep()

        model.sswFinish()
        
        pass

if __name__ == '__main__':
        main()
