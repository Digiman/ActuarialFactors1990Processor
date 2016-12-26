'''
Special processor for Table S - read CSV file and create a new file with new correct data.
'''

# import needed modules
import csv
import sys
import numpy as np
import array
import math
import json

# class to store data about rows in TableS
class TableSRow:
    interestRate = 0.0
    age = 0
    pvAnnuity = 0
    pvLifeEstate = 0.0
    pvReminderInterest = 0.0

    def __init__(self):
        self.mortalityTable = 1990

    def jdefault(self):
        return self.__dict__

# define filename to process
filename = 'i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableS-1990.csv'
print('Open file: ', filename)

#-----------------------------------------------

# generate interest rates array with values [2.2:0.2:22.0]
interestRates = np.arange(2.2,22.1,0.2)
print("InterestReates count: ", len(interestRates))

# read file to array and count lines
with open(filename,"r") as f:
    reader = csv.reader(f, delimiter = ",")
    data = list(reader)

# readed rows count from csv file
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
        x = TableSRow()
        x.interestRate = interestRate
        x.age = data[i][0]
        x.pvAnnuity = data[i][1]
        x.pvLifeEstate = data[i][2]
        x.pvReminderInterest = data[i][3]
        results.append(x)
    # process second part (ages from 55 to 104)
    for i in range(dataStartIndex, pageEndIndex):
        x = TableSRow()
        x.interestRate = interestRate
        x.age = data[i][4]
        x.pvAnnuity = data[i][5]
        x.pvLifeEstate = data[i][6]
        x.pvReminderInterest = data[i][7]
        results.append(x)
    # end processing
    print("Page", (page+1), "processed.")


'''
# print results
print("InterestRate", "Age", "pvAnnuity", "pvLifeEstate", "pvReminderInterest")
for i in range(0,104):
    print(results[i].interestRate, results[i].age, results[i].pvAnnuity, results[i].pvLifeEstate, results[i].pvReminderInterest)
'''

# save results to JSON file
resultFilename = 'i:\\Exadel\\WK\\FinEstCOM Replacement\\tabula-TableS-1990-processed.json'
with open(resultFilename, "w") as resultFile:
    json.dump(results, resultFile, default=TableSRow.jdefault)



