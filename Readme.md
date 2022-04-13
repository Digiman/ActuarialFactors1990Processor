# IRS Actuarial Factors (1990) processor

Utilites that uses to process data from PDF files:

* [Tabula](http://tabula.technology/) - Tabula is a tool for liberating data tables locked inside PDF files.
* [Visual Studio 2015](https://www.visualstudio.com/vs/) - IDE to develop .NET applicaion.
* [Python Tools for Visual Studio](https://www.visualstudio.com/vs/python/) - Completely free Python support within Visual Studio. Add functionality to VS to work with Python and create project with this language.
* [Python](https://www.python.org/) - python language runtime and main libraries. Better to install Anaconda.
* [Anaconda](https://www.continuum.io/downloads) - data since toolbox uses Python to work with data. Contains a lot of libraries and packages to quick start. Include Puthon and many libraries.

Solution created in Visual Studio and has two types of the projects: C# application to process data and Python scripts to work with CSV files with data extracted from PDF with with Tabula.

## How to use this application

The application contains tho parts:

1. Python scripts to process CSV files generated with Tabula after extaction data from PDF documents. This scripts create JSON files with data for some table (6 tables).
2. C# console application that can read generated JSON files and do additional processing (read and save as text files or export to Excel documents).

The simple description of the way how gile processed:

**PDF Files (source)[input] -> 1) Tabula (manual processing) -> CSV files[output->input] -> 2) Python scripts (auto) -> JSON files[output->input] -> 3) C# App (auto) -> ...**

All data files already added to repository and processed and in this case no needed do processing another one. All that needed is start using JSON files.
To start work with data it need to copy all JSON files to working folder on local machine, change variable value (in class Constants (\DataProcessingApp.Core\Constants.cs)) - ```public const string BaseDatadir = "i:\\Working Data";``` and set local machine folder with files. After this application can build and run. As result in working folder will be created a set of new files (text files, json with combined data for U(2) and R(2) tables, Excel files).
Application (C#) can be extended if needed to add additional processing or file formats to export.

*Note that there are two very big tables - U(2) and R(2) with 615 pages for each. To simplify processing sourcePDF files was splited on 5 parts. But this did because all information for each interest rate can't be placed in one big table.
In other words, it was necessary in order to accommodate all the data.*

## Table details

List of tables that can be processed:

1. *Table C* - Factors for Reducing Assurances – Based on Life Table 90CM.
2. *Table R(2)* - Based on Life Table 90CM.
3. *Table U(1)* - Based on Life Table 90CM.
4. *Table U(2)* - Based on Life Table 90CM.
5. *Table H* - Commutation Factors Based on Life Table 90CM.
6. *Table S* - Single Life Factors Based on Life Table 90CM.

This tables contains data for Actuarial Factors for 1990 base year from official IRS documents.

Each table has some fields with data. This fields provided bellow wih data types (C#).

1. *Table C* - Factors for Reducing Assurances – Based on Life Table 90CM.
    * int MortalityTable
    * double Rate
    * int Age
    * double RemainderFactor
    * double RFactor
    * double DFactor
2. *Table R(2)* - Based on Life Table 90CM.
    * int MortalityTable
    * int Age1
    * int Age2
    * double AdjustedPayoutRate
    * double RemainderFactor
3. *Table U(1)* - Based on Life Table 90CM.
    * int MortalityTable
    * int Age
    * double AdjustedPayoutRate
    * double RemainderFactor
4. *Table U(2)* - Based on Life Table 90CM.
    * int MortalityTable
    * int Age1
    * int Age2
    * double AdjustedPayoutRate
    * double RemainderFactor
5. *Table H* - Commutation Factors Based on Life Table 90CM.
    * int MortalityTable
    * double InterestRate
    * int Age
    * double DFactor
    * double NFactor
    * double MFactor
6. *Table S* - Single Life Factors Based on Life Table 90CM.
    * int MortalityTable
    * double InterestRate
    * int Age
    * double PvAnnuity
    * double PvLifeEstate
    * double PvReminderInterest

---

## Data files

There are three category of the files:

* Source files (PDF).
* Files with extracted data (CSV).
* Files with processed data (JSON).

And last - all files that can be created in C# application (Excel, text files and etc.) - also processed data.

### 1 Source tables

Source files (PDF) included in repository and placed in folder - ```DataFiles```

Data files with source tables:

1. *Table C* - Factors for Reducing Assurances – Based on Life Table 90CM.
    * TableC-1990.pdf - 100 pages.
2. *Table R(2)* - Based on Life Table 90CM.
    * TableR(2)-1990.pdf - full table - 615 pages.
    * TableR(2)-p1-1990.pdf - part 1 - 123 pages.
    * TableR(2)-p2-1990.pdf - part 2 - 123 pages.
    * TableR(2)-p3-1990.pdf - part 3 - 123 pages.
    * TableR(2)-p4-1990.pdf - part 4 - 123 pages.
    * TableR(2)-p5-1990.pdf - part 4 - 123 pages.
3. *Table U(1)* - Based on Life Table 90CM.
    * TableU(1)-1990.pdf - 18 pages.
4. *Table U(2)* - Based on Life Table 90CM.
    * TableU(2)-1990.pdf - full table - 615 pages.
    * TableU(2)-p1-1990.pdf - part 1 - 123 pages.
    * TableU(2)-p2-1990.pdf - part 2 - 123 pages.
    * TableU(2)-p3-1990.pdf - part 3 - 123 pages.
    * TableU(2)-p4-1990.pdf - part 4 - 123 pages.
    * TableU(2)-p5-1990.pdf - part 5 - 123 pages.
5. *Table H* - Commutation Factors Based on Life Table 90CM.
    * TableH-1990.pdf - 100 pages.
6. *Table S* - Single Life Factors Based on Life Table 90CM.
    * TableS-1990.pdf - 100 pages.

### 2 Extracted data

All files after extraction (CSV) also included in repository and placed in folder - ```PythonDataApp\DataFiles\```. This folder also included in solution for Python project.

1. *Table C* - Factors for Reducing Assurances – Based on Life Table 90CM.
    * tabula-TableC-1990-2.csv
2. *Table R(2)* - Based on Life Table 90CM.
    * tabula-TableR(2)-p1-1990.csv
    * tabula-TableR(2)-p2-1990.csv
    * tabula-TableR(2)-p3-1990.csv
    * tabula-TableR(2)-p4-1990.csv
    * tabula-TableR(2)-p5-1990.csv
3. *Table U(1)* - Based on Life Table 90CM.
    * tabula-TableU(1)-1990-2.csv
4. *Table U(2)* - Based on Life Table 90CM.
    * tabula-TableU(2)-p1-1990.csv
    * tabula-TableU(2)-p2-1990.csv
    * tabula-TableU(2)-p3-1990.csv
    * tabula-TableU(2)-p4-1990.csv
    * tabula-TableU(2)-p5-1990.csv
5. *Table H* - Commutation Factors Based on Life Table 90CM.
    * tabula-TableH-1990.csv
6. *Table S* - Single Life Factors Based on Life Table 90CM.
    * tabula-TableS-1990.csv

### 3 Processed data

All processed file (JSON) with information extracted from CSV files. This file contains correct information from tables in PDF files and can be used in different applications.
This files placed in folder - ```JSONFiles```.

1. *Table C* - Factors for Reducing Assurances – Based on Life Table 90CM.
    * tabula-TableC-1990-processed.json
2. *Table R(2)* - Based on Life Table 90CM.
    * tabula-TableR(2)-p1-1990-processed.json
    * tabula-TableR(2)-p2-1990-processed.json
    * tabula-TableR(2)-p3-1990-processed.json
    * tabula-TableR(2)-p4-1990-processed.json
    * tabula-TableR(2)-p5-1990-processed.json
3. *Table U(1)* - Based on Life Table 90CM.
    * tabula-TableU1-1990-processed.json
4. *Table U(2)* - Based on Life Table 90CM.
    * tabula-TableU(2)-p1-1990-processed.json
    * tabula-TableU(2)-p2-1990-processed.json
    * tabula-TableU(2)-p3-1990-processed.json
    * tabula-TableU(2)-p4-1990-processed.json
    * tabula-TableU(2)-p5-1990-processed.json
5. *Table H* - Commutation Factors Based on Life Table 90CM.
    * tabula-TableH-1990-processed.json
6. *Table S* - Single Life Factors Based on Life Table 90CM.
    * tabula-TableS-1990-processed.json

---

## Notes

1. To use all logic and scrtpts need to install all the apps and libraries.
2. Section with data files contain table name and list of files with data for each table.

---

**Author: Andrey Kukharenko. 
Created on: December 2016.**
