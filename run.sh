#!/bin/sh
SUFFIX=$1
outdir=ourResults/${SUFFIX}
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
for sched in "FRFCFS" "FCFS" "ATLAS" "BLISS" "TCM" "PARBS" "MLQSched"
# for sched in "MLQSched"
do
    for core in 1
    do
        # generateConfigs $core $sched
        if [ ${core} -eq 1 ];
        then
            mono bin/sim.exe -output ${outdir}/${core}core_base_L_${sched}_.json -config configs/${core}core_base_$sched.cfg -N $core -workload workloads/${core}core 1
            mono bin/sim.exe -output ${outdir}/${core}core_base_H_${sched}_.json -config configs/${core}core_base_$sched.cfg -N $core -workload workloads/${core}core 2
            sleep 10
        else
            for i in 1 2 3 4 5
            do
                mono bin/sim.exe -output ${outdir}/${core}core_base_${workloads[$i]}_${sched}_.json -config configs/${core}core_base_$sched.cfg -N $core -workload workloads/${core}core $i
                sleep 10
            done
        fi
    done
done

# mono bin/sim.exe -output $outdir/4core+LISA-ALL.json -config configs/LISA_RISC+VILLA+LIP_4core.cfg -N 1 -workload workloads/4core_mix 1
echo ""
