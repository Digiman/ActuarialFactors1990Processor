# combine JSON files
import json

# base directory with data files
baseDir = "d:\\Projects\\IRS Actuarial Factors (1990) processor"

tableU2files = ["tabula-TableU(2)-p1-1990", "tabula-TableU(2)-p2-1990", "tabula-TableU(2)-p3-1990", "tabula-TableU(2)-p4-1990", "tabula-TableU(2)-p5-1990"]
tableR2files = ["tabula-TableR(2)-p1-1990", "tabula-TableR(2)-p2-1990", "tabula-TableR(2)-p3-1990", "tabula-TableR(2)-p4-1990", "tabula-TableR(2)-p5-1990"]

# process each file for Table U2
output_list_U2 = []
for file in tableU2files:
    filename = "{0}\\{1}-processed.json".format(baseDir, file)
    print("Processing file:", filename, "...")
    with open(filename, "r") as datafile:
        fileData = json.load(datafile)
        for line in fileData:
            output_list_U2.append(line)

# save result file
resultFilename = "{0}\\tabula-TableU(2)-full-1990.json".format(baseDir)
print("Saving result file:", resultFilename)
with open(resultFilename, "w") as resultFile:
    json.dump(output_list_U2, resultFile)

# process each file for Table R2
output_list_R2 = []
for file in tableR2files:
    filename = "{0}\\{1}-processed.json".format(baseDir, file)
    print("Processing file:", filename, "...")
    with open(filename, "r") as datafile:
        fileData = json.load(datafile)
        for line in fileData:
            output_list_R2.append(line)

# save result file
resultFilename = "{0}\\tabula-TableR(2)-full-1990.json".format(baseDir)
print("Saving result file:", resultFilename)
with open(resultFilename, "w") as resultFile:
    json.dump(output_list_R2, resultFile)

