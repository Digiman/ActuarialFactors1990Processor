'''
Special processor for Table U(1) - read CSV file and create a new file with new proper data
'''

# import needed modules
import csv
import sys
import numpy as np
import array
import math
import json

# class to store data about rows in TableU(1)
class TableU1Row:
    age = 0
    adjustedPayoutRate = 0.0
    remainderFactor = 0.0

    def __init__(self):
        self.mortalityTable = 1990

    def jdefault(self):
        return self.__dict__

# define filename to process
filename = 'i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableU(1)-1990-2.csv'
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

# calculate pages count with data from table
rowsPerPage = 38; pages = round(row_count/rowsPerPage);
print("Pages:", pages)

# get all indexes of page start (from table headers)
els = []
for i in range(0, len(data)):
    el = [x for x in data][i][0]
    els.append(el)
print(els)
indexes = [i for i, x in enumerate(els) if x == "Age"]
print(indexes)
print(len(indexes))

# result array
results = []

# process data on the each page
for page in range(0, pages):
    # start processing
    print("Processing page", (page+1), "...")
    pageStartIndex = indexes[page]
    if page!=17:
        pageEndIndex = indexes[page+1]
    else:
        pageEndIndex = len(data)
    print("StartIndex:", pageStartIndex)
    print("EndIndex:", pageEndIndex)
    # process page data - rows from file
    dataStartIndex = pageStartIndex+1
    headerIndex = pageStartIndex
    for i in range(dataStartIndex, pageEndIndex):
        for j in range(1,len(data[i])):
            x = TableU1Row()
            x.adjustedPayoutRate = data[headerIndex][j].strip('%')
            x.age = data[i][0]
            x.remainderFactor = data[i][j]
            results.append(x)
    # end processing
    print("Page", (page+1), "processed.")

'''
# print results
print("InterestRate", "Age", "dFactor", "nFactor", "dFactor")
for i in range(0,len(results)):
    print(results[i].mortalityTable, results[i].age, results[i].adjustedPayoutRate, results[i].remainderFactor)
'''

# save results to JSON file
resultFilename = 'i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableU1-1990-processed.json'
with open(resultFilename, "w") as resultFile:
    json.dump(results, resultFile, default=TableU1Row.jdefault)
