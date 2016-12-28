'''
Special processor for Table C - read CSV file and create a new file with new correct data.
'''

# import needed modules
import csv
import sys
import numpy as np
import array
import math
import json

# class to store data about rows in TableC
class TableCRow:
    rate = 0.0
    age = 0
    remainderFactor = 0
    rFactor = 0.0
    dFactor = 0.0

    def __init__(self):
        self.mortalityTable = 1990

    def jdefault(self):
        return self.__dict__

# base directory with data files
baseDir = "d:\\Projects\\IRS Actuarial Factors (1990) processor"

# define filename to process
filename = '{0}\\tabula-TableC-1990-2.csv'.format(baseDir)
print('Open file: ', filename)

#-----------------------------------------------

# generate interest rates array with values [2.2:0.2:22.0]
interestRates = np.arange(2.2,22.1,0.2)
print("InterestReates count: ", len(interestRates))

# read file to array and count lines
with open(filename,"r") as f:
    reader = csv.reader(f, delimiter = ",")
    data = list(reader)

print(data[0])

# readed rows count from csv file
row_count = len(data)
print("Lines count:", row_count)

# process the reader data from file

# calculate pages count with data from table
rowsPerPage = 55; pages = round(row_count/rowsPerPage);
print("Pages:", pages)

# result array
results = []

# process data on the each page
for page in range(0, pages):
    # start processing
    print("Processing page", (page+1), "...")
    pageStartIndex = page*rowsPerPage
    pageEndIndex = page*rowsPerPage + rowsPerPage
    print("StartIndex:", pageStartIndex)
    print("EndIndex:", pageEndIndex)
    # process page data - rows from file
    dataStartIndex = pageStartIndex
    interestRate = interestRates[page]
    # process first part (ages from 0 to 54)
    for i in range(dataStartIndex, pageEndIndex):
        x = TableCRow()
        x.rate = interestRate
        x.age = data[i][0]
        x.remainderFactor = data[i][1]
        x.rFactor = data[i][2]
        x.dFactor = data[i][3]
        results.append(x)
    # process second part (ages from 55 to 104)
    for i in range(dataStartIndex, pageEndIndex):
        x = TableCRow()
        x.rate = interestRate
        x.age = data[i][4]
        x.remainderFactor = data[i][5]
        x.rFactor = data[i][6]
        x.dFactor = data[i][7]
        results.append(x)
    # end processing
    print("Page", (page+1), "processed.")


'''
# print results
print("MortalityTable", "InterestRate", "Age", "remainderFactor", "rFactor", "dFactor")
for i in range(0,104):
    print(results[i].mortalityTable, results[i].rate, results[i].age, results[i].remainderFactor, results[i].rFactor, results[i].dFactor)
'''

# save results to JSON file
resultFilename = '{0}\\tabula-TableC-1990-processed.json'.format(baseDir)
with open(resultFilename, "w") as resultFile:
    json.dump(results, resultFile, default=TableCRow.jdefault)

