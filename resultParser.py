import csv
import json
import os
import argparse
import pandas as pd

parser = argparse.ArgumentParser()
parser.add_argument('--outdir')
parser.add_argument('--suffix')

args = parser.parse_args()

# scheds = ["ATLAS", "BLISS", "FCFS", "FRFCFS", "PARBS", "TCM"]
scheds = ["BLISS", "FCFS", "ATLAS", "FRFCFS", "PARBS", "TCM", "TCMPlus"]
traces = ["464.h264ref.gz", "462.libquantum.gz", "458.sjeng.gz", "429.mcf.gz", "447.dealII.gz", "433.milc.gz", "435.gromacs.gz", "459.GemsFDTD.gz"]
traces_type = ["int"]*4 + ["float"]*4
traces_intensity = ["Not intensive", "Intensive"] * 4
traces_RB = (["High"]*2 + ["Low"]*2)*2
workloads = ["int", "float"]

total_result = []
single_core_result = []

for sched in scheds:
    outdir = "./ourResults/" + args.outdir + "_" + sched + "/"
    
    # Single-core informations
    if len(single_core_result) == 0:
        files = os.listdir(outdir + "single-core/")
        for file in files:
            file_info = file.split('_')
            assert(file_info[0] == '1core')
            index = int(file_info[1][-1])-1

            jsonf = open(outdir + "single-core/" + file)
            data = json.load(jsonf)
            proc = data['proc'][0]
            assert(proc["trace_fname"] == traces[index])

            # For each workload traces, extract IPC, MCPI
            cycle = proc["cycle"]
            inst = data['cache'][0]['total_system_inst_executed']
            ipc = float(inst)/float(cycle)
            # mcpi = 1.0 / (float(proc["rmpc"]) + float(proc["wmpc"]))
            mcpi = float(proc["stall_inst_wnd"])
            single_core_result.append(([traces[index], traces_type[index], traces_intensity[index], traces_RB[index], ipc, mcpi]))

        df = pd.DataFrame(single_core_result, columns = ["Trace", "Type", "Intensity", "RB hit rate", "IPC", "MCPI"])
        df_sort = df.sort_values(['IPC'], ascending = [False])

    # Multi-core informations
    files = os.listdir(outdir + "multi-core/")
    for file in files:
        file_info = file.split('_')
        core = int(file_info[0][:-4])
        index = int(file_info[1][-1])-1

        jsonf = open(outdir + "multi-core/" + file)
        data = json.load(jsonf)
        procs = data['proc']
        
        ipc_shared = []
        mcpi_shared = []
        for proc in procs:
            ipc_shared.append((proc["trace_fname"], float(proc["ipc"])))
            mcpi_i = float(proc["stall_inst_wnd"])
            mcpi_shared.append((proc["trace_fname"], mcpi_i))

        # Extract metrics: W. SpeedUp, H. SpeedUp, Sum-of-IPCs, Memory Slowdown, Maximum Slowdown, Unfairness
        assert(len(ipc_shared) == len(mcpi_shared))
        WSpeedUp = sum([ipc[1]/float(df.loc[df['Trace'] == ipc[0]]['IPC']) for ipc in ipc_shared])
        HSpeedUp = core / sum([1.0/(ipc[1]/float(df.loc[df['Trace'] == ipc[0]]['IPC']))])
        SumofIPC = sum([ipc[1] for ipc in ipc_shared])
        MaximumSlowdown = max( [float(df.loc[df['Trace'] == ipc[0]]['IPC'])/ipc[1] for ipc in ipc_shared] )
        MemSlowdown = [mcpi[1]/float(df.loc[df['Trace'] == mcpi[0]]['MCPI']) for mcpi in mcpi_shared]
        Unfairness = max(MemSlowdown) / min(MemSlowdown)

        total_result.append(([core, workloads[index], sched, WSpeedUp, HSpeedUp, SumofIPC, MaximumSlowdown, Unfairness]))

df2 = pd.DataFrame(total_result, columns=["Core", "Workload", "Sched", "WSpeedUp", "HSpeedUp", "SumofIPC", "MaximumSlowdown", "Unfairness"]).round(5)
df2_sort = df2.sort_values(["Core", "Workload", "SumofIPC"], ascending=[True, True, False])
df2_sort2 = df2.sort_values(["Core", "Workload", "Unfairness"], ascending=[True, True, True])

print(df2_sort.reset_index(drop=True))
print("-"*100)
print(df2_sort2.reset_index(drop=True))

# output_filename = './outputs/output_' + args.suffix + '_throughput.csv'
# df2_sort.to_csv(output_filename, index=False)
# output_filename = './outputs/output_' + args.suffix + '_slowdown.csv'
# df2_sort2.to_csv(output_filename, index=False)