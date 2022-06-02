#!/bin/sh
outdir=ourResults
mkdir $outdir

function generateConfigs {
    cp configs/${1}core_base.cfg configs/${1}core_base.cfg.bak
    cat configs/${1}core_base.cfg.bak \
    | sed -E "s/sched.sched_algo = [a-zA-Z]+/sched.sched_algo = ${2}/" \
    > configs/${1}core_base_${2}.cfg
}

###########################################
# 4 cores -- run with 1 million instructions
###########################################

# ** Applications: forkset, bootup, tpcc64, libquantum
workloads=("NONE" "HHHH" "HHHL" "HHLL" "HLLL" "LLLL")
for sched in "FRFCFS" "FCFS" "ATLAS" "BLISS" "TCM" "PARBS"
# for sched in "FRFCFS"
do
    for core in 16 32
    do
        # generateConfigs $core $sched
    for i in 1 2 3 4 5
    do
        mono bin/sim.exe -output ${outdir}/${core}core_base_${workloads[$i]}_${sched}_.json -config configs/${core}core_base_$sched.cfg -N $core -workload workloads/${core}core $i
    done
    done

    
done

# mono bin/sim.exe -output $outdir/4core+LISA-ALL.json -config configs/LISA_RISC+VILLA+LIP_4core.cfg -N 1 -workload workloads/4core_mix 1
echo ""
