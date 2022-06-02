import csv
import json
import os

f = open('output.csv', 'w')
wr = csv.writer(f)

files = os.listdir('./results')
for file in files:
    filesplit = file.split('_')
    print(filesplit)
    workload = filesplit[2]
    sched = filesplit[3]
    filepath = "./results/" + file
    jsonf = open(filepath)
    data = json.load(jsonf)
    cycles = []
    for proc in data['proc']:
        cycles.append(proc['cycle'])
    inst = data['cache'][0]['total_system_inst_executed']
    ipc = float(inst)/float((sum(cycles)/len(cycles)))
    wr.writerow([workload, sched, ipc])

f.close()

