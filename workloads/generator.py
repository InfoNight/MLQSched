# Meaning of index
# 0 : Not intensive, High RB hit rate
# 1 : Intensive,     High RB hit rate
# 2 : Not intensive, Low  RB hit rate
# 3 : Intensive,     Low  RB hit rate

workload_int = ["464.h264ref.gz", "462.libquantum.gz", "458.sjeng.gz", "429.mcf.gz"]
workload_fp = ["447.dealII.gz", "433.milc.gz", "435.gromacs.gz", "459.GemsFDTD.gz"]

core_num = [1, 4, 8, 16, 32]

def main():
    for core in core_num:
        f = open(str(core)+"core", "w")
        lines = []
        lines.append("./traces ./traces/ChargeCache_traces\n")
        if core == 1:
            for work in workload_int+workload_fp:
                lines.append(work+"\n")
        else:
            lines.append((" ".join(workload_int)+" ")*(core/4) + "\n")
            lines.append((" ".join(workload_fp)+" ")*(core/4) + "\n")
            
        for line in lines:
            f.write(line)
        f.close()

main()