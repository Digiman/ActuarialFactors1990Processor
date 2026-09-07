import TableU2ProcessorDef as prc

# process all files with Table U(2) parts
files = ["TableU(2)-p1-90CM", "TableU(2)-p2-90CM", "TableU(2)-p3-90CM", "TableU(2)-p4-90CM", "TableU(2)-p5-90CM"]
for file in files:
    prc.TableU2ProcessorDef(file)
