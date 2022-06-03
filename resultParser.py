import csv
import json
import os
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('outdir')
parser.add_argument('suffix')

args = parser.parse_args()

f = open('./outputs/output_' + args.suffix + '.csv', 'w')
wr = csv.writer(f)

outdir = "./ourResults/" + args.outdir + "/"

files = os.listdir(outdir)
for file in files:
    filesplit = file.split('_')
    print(filesplit)
    core = filesplit[0]
    workload = filesplit[2]
    sched = filesplit[3]
    filepath = outdir + file
    jsonf = open(filepath)
    data = json.load(jsonf)
    cycles = []
    for proc in data['proc']:
        cycles.append(proc['cycle'])
    inst = data['cache'][0]['total_system_inst_executed']
    ipc = float(inst)/float((sum(cycles)/len(cycles)))
    wr.writerow([core, workload, sched, ipc])

f.close()

