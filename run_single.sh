#!/bin/sh
SUFFIX=$1
mkdir ourResults
outdir=ourResults/${SUFFIX}
mkdir $outdir
mkdir ${outdir}/single-core
mkdir ${outdir}/multi-core

function generateConfigs {
    CYCLE=$3
    cp configs/${1}core_base.cfg configs/${1}core_base.cfg.bak

    cat configs/${1}core_base.cfg.bak \
    | sed -E "s/sim_cycle_max = [0-9]+/sim_cycle_max = ${CYCLE}/" \
    > configs/${1}core_base.cfg.bak1

    cat configs/${1}core_base.cfg.bak1 \
    | sed -E "s/sched.sched_algo = [a-zA-Z]+/sched.sched_algo = ${2}/" \
    > configs/${1}core_base_${2}.cfg

    rm configs/${1}core_base.cfg.bak1
}

# for sched in "FRFCFS" "FCFS" "ATLAS" "PARBS" "TCM" "BLISS" 
# for sched in "MLQSched"
# do
sched=$2
for core in 8 16 32
do  
    ### Run with 5 million instructions
    ### Uncomment the line below in order to generate configuration files
    generateConfigs $core $sched 100000000

    let "WORK_NUM=$(wc -l workloads/${core}core | cut -c1) - 1"
    for i in $(seq 1 ${WORK_NUM})
    do  
        if [ ${core} -eq 1 ];
        then
            mono bin/sim.exe -output ${outdir}/single-core/${core}core_work${i}_FRFCFS_.json -config configs/${core}core_base_$sched.cfg -N $core -workload workloads/${core}core $i
        else
            mono bin/sim.exe -output ${outdir}/multi-core/${core}core_work${i}_${sched}_.json -config configs/${core}core_base_$sched.cfg -N $core -workload workloads/${core}core $i
        fi
        sleep 5
    done
done
# done
echo ""
