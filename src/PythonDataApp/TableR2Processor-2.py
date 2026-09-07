import TableR2ProcessorDef as prc

# process all files with Table U(2) parts
files = ["TableR(2)-p1-90CM", "TableR(2)-p2-90CM", "TableR(2)-p3-90CM", "TableR(2)-p4-90CM", "TableR(2)-p5-90CM"]
for file in files:
    prc.TableR2ProcessorDef(file)
