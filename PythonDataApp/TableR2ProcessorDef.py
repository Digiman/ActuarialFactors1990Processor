'''
Special processor for Table R(2) - read CSV file and create a new file with new proper data
'''

# import needed modules
import csv
import sys
import numpy as np
import array
import math
import json

# class to store data about rows in TableR(2)
class TableR2Row:
    age1 = 0
    age2 = 0
    adjustedPayoutRate = 0.0
    remainderFactor = 0.0

    def __init__(self):
        self.mortalityTable = 1990

    def jdefault(self):
        return self.__dict__

# base directory with data files
#baseDir = "d:\\Projects\\IRS Actuarial Factors (1990) processor"
baseDir = "d:\\Temp\\ActuarialFactors1990Processor"

# definition for logic to process file with data (CSV to JSON)
def TableR2ProcessorDef(filename):
    # define filename to process
    baseFilename = filename

    filename = '{0}\\{1}.csv'.format(baseDir, baseFilename)
    print('Open file: ', filename)

    #-----------------------------------------------

    # read file to array and count lines
    with open(filename,"r") as f:
        reader = csv.reader(f, delimiter = ",")
        data = list(reader)

    # readed rows count from csv file
    row_count = len(data)
    print("Lines count:", row_count)

    # process the loaded data from file

    # get all indexes of page start (from table headers)
    els = []
    for i in range(0, len(data)):
        el = [x for x in data][i][0]
        els.append(el)
    #print(els)
    indexes = [i for i, x in enumerate(els) if x == "O"]

    # pages count calculated by indexes array
    pages = len(indexes)
    print("Pages (calculated by indexes for page start):", pages)

    # result array
    results = []

    # process data on the each page
    for page in range(0, pages):
        # start processing
        print("Processing page", (page+1), "...")
        pageStartIndex = indexes[page]
        if page!=pages-1:
            pageEndIndex = indexes[page+1]
        else:
            pageEndIndex = len(data)
        print("StartIndex:", pageStartIndex)
        print("EndIndex:", pageEndIndex)
        # process page data - rows from file
        dataStartIndex = pageStartIndex+1
        headerIndex = pageStartIndex
        for i in range(dataStartIndex, pageEndIndex):
            for j in range(2,len(data[i])):
                x = TableR2Row()
                x.adjustedPayoutRate = data[headerIndex][j].strip('%')
                x.age1 = data[i][0]
                x.age2 = data[i][1]
                x.remainderFactor = data[i][j]
                results.append(x)
        # end processing
        print("Page", (page+1), "processed.")

    '''
    # print results
    print("MortalityTable", "Age1", "Age2", "AdjustedPayoutRate", "RemainderFactor")
    for i in range(0,len(results)):
        print(results[i].mortalityTable, results[i].age1, results[i].age2, results[i].adjustedPayoutRate, results[i].remainderFactor)
    '''

    # save results to JSON file
    resultFilename = '{0}\\{1}-processed.json'.format(baseDir, baseFilename)
    with open(resultFilename, "w") as resultFile:
        json.dump(results, resultFile, default=TableR2Row.jdefault)
