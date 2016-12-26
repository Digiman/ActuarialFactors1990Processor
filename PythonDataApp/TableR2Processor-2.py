import TableR2ProcessorDef as prc

# process all files with Table U(2) parts
files = ["tabula-TableR(2)-p1-1990", "tabula-TableR(2)-p2-1990", "tabula-TableR(2)-p3-1990", "tabula-TableR(2)-p4-1990", "tabula-TableR(2)-p5-1990"]
for file in files:
    prc.TableR2ProcessorDef(file)
