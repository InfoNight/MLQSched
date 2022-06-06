import csv
import json
import os
import argparse
import pandas as pd

parser = argparse.ArgumentParser()
parser.add_argument('outdir')
parser.add_argument('suffix')

args = parser.parse_args()

output_filename = './outputs/output_' + args.suffix + '.csv'
total_results = []

outdir = "./ourResults/" + args.outdir + "/"

files = os.listdir(outdir)
for file in files:
    filesplit = file.split('_')
    print(filesplit)
    core = int(filesplit[0][:-4])
    workload = filesplit[2]
    sched = '_'.join([e for e in filesplit[3:] if e != '.json'])
    filepath = outdir + file
    jsonf = open(filepath)
    data = json.load(jsonf)
    cycles = []
    for proc in data['proc']:
        cycles.append(proc['cycle'])
    inst = data['cache'][0]['total_system_inst_executed']
    ipc = float(inst)/float((sum(cycles)/len(cycles)))
    total_results.append(([core, workload, sched, ipc]))

df = pd.DataFrame(total_results, columns = ['core', 'workload', 'sched', 'ipc'])
df_sort = df.sort_values(['core', 'workload', 'ipc'], ascending = [True, False, False])
df_sort["rank"] = df_sort.groupby(['core', 'workload'])["ipc"].rank("dense", ascending=False)
df_sort.to_csv(output_filename, index = False, header = False)


