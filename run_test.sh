#!/bin/bash
outdir=results
mkdir $outdir

###########################################
# 4 cores -- run with 1 million instructions
###########################################

# ** Applications: forkset, bootup, tpcc64, libquantum
workloads=("NONE", "HHHH", "HHHL", "HHLL", "HLLL", "LLLL")
# for sched in "FRFCFS" "FCFS" "ATLAS" "BLISS" "TCM" 
for sched in "FRFCFS" 
do
    # cp configs/4core_base.cfg configs/4core_base.cfg.bak
    # cat configs/4core_base.cfg.bak \
    # | sed -E "s/sched.sched_algo = [a-zA-Z]+/sched.sched_algo = $sched/" \
    # > configs/4core_base_$sched.cfg

    for i in 5
    do
        mono bin/sim.exe -output $outdir/4core_base_${workloads[$i]}_${sched}_test.json -config configs/4core_base.cfg -N 4 -workload workloads/4core $i
    done
done

# mono bin/sim.exe -output $outdir/4core+LISA-ALL.json -config configs/LISA_RISC+VILLA+LIP_4core.cfg -N 1 -workload workloads/4core_mix 1
echo ""
