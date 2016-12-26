'''
Special processor for Table H - read CSV file and create a new file with new proper data
'''

# import needed modules
import csv
import sys
import numpy as np
import array
import math
import json

# class to store data about rows in TableH
class TableHRow:
    interestRate = 0.0
    age = 0
    dFactor = 0
    nFactor = 0.0
    mFactor = 0.0

    def __init__(self):
        self.mortalityTable = 1990

    def jdefault(self):
        return self.__dict__

# define filename to process
filename = 'i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableH-1990.csv'
print('Open file: ', filename)

#-----------------------------------------------

'''
# register custom dialect for CSV file reader
csv.register_dialect(
    'mydialect',
    delimiter = ',',
    quotechar = '"',
    doublequote = True,
    skipinitialspace = True,
    lineterminator = '\r\n',
    quoting = csv.QUOTE_MINIMAL)
'''

# generate interest rates array with values [2.2:0.2:22.0]
interestRates = np.arange(2.2,22.1,0.2)
print("InterestReates count: ", len(interestRates))

# read file to array and count lines
with open(filename,"r") as f:
    reader = csv.reader(f,delimiter = ",")
    data = list(reader)
    row_count = len(data)

print("Lines count:", row_count)

# process the reader data from file

# calculate pages count with data from table
rowsPerPage = 57; pages = round(row_count/rowsPerPage);
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
    dataStartIndex = pageStartIndex + 2
    interestRate = interestRates[page]
    # process first part (ages from 0 to 54)
    for i in range(dataStartIndex, pageEndIndex):
        x = TableHRow()
        x.interestRate = interestRate
        x.age = data[i][0]
        x.dFactor = data[i][1]
        x.nFactor = data[i][2]
        x.mFactor = data[i][3]
        results.append(x)
    # process second part (ages from 55 to 104)
    for i in range(dataStartIndex, pageEndIndex):
        x = TableHRow()
        x.interestRate = interestRate
        x.age = data[i][4]
        x.dFactor = data[i][5]
        x.nFactor = data[i][6]
        x.mFactor = data[i][7]
        results.append(x)
    # end processing
    print("Page", (page+1), "processed.")


'''
# print results
print("InterestRate", "Age", "dFactor", "nFactor", "dFactor")
for i in range(0,104):
    print(results[i].interestRate, results[i].age, results[i].dFactor, results[i].nFactor, results[i].mFactor)
'''

# save results to JSON file
resultFilename = 'i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableH-1990-processed.json'
with open(resultFilename, "w") as resultFile:
    json.dump(results, resultFile, default=TableHRow.jdefault)
