H = "462.libquantum.gz "
L = "tpcc64.gz "

core_num = [16, 32]
workload = [0, 25, 50, 75, 100]

for core in core_num:
    f = open(str(core)+"core", "w")
    f.write("./traces /home/kevincha/traces/hybrid\n")
    for work in workload:
        line = core*work/100*L + core*(100-work)/100*H + "\n"
        f.write(line)
    f.close()
