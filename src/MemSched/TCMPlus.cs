using Ramulator.MemReq;
using Ramulator.Sim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ramulator.MemSched
{
    public class TCMPlus : MemSched
    {
        //priority
        private int[] _priority;

        //rank
        private int[] _rank_mem_intensity;
        private int[] _rank_rbl;

        //mpki
        private double[] _mpki;

        private ulong[] _prevCacheMiss;
        private ulong[] _prevInstCnt;

        //rbl
        private double[] _rbl;

        private ulong[] _shadowRowHits;

        //quantum
        private int _quantumCnt;
        private int _quantumCyclesLeft;

        public TCMPlus(MemCtrl.MemCtrl mctrl, MemCtrl.MemCtrl[] mctrls)
            : base(mctrl, mctrls)
        {
            _priority = new int[Config.N];

            _rank_mem_intensity = new int[Config.N];
            _rank_rbl = new int[Config.N];

            _mpki = new double[Config.N];
            _prevCacheMiss = new ulong[Config.N];
            _prevInstCnt = new ulong[Config.N];

            _rbl = new double[Config.N];
            _shadowRowHits = new ulong[Config.N];

            _quantumCyclesLeft = Config.sched.quantum_cycles;
        }

        public override void enqueue_req(Req req)
        {
        }

        public override void dequeue_req(Req req)
        {
        }

        public override Req better_req(Req req1, Req req2)
        {
            int rank1 = _priority[req1.Pid];
            int rank2 = _priority[req2.Pid];
            if (rank1 != rank2)
            {
                if (rank1 > rank2) return req1;
                else return req2;
            }

            bool hit1 = is_row_hit(req1);
            bool hit2 = is_row_hit(req2);
            if (hit1 ^ hit2)
            {
                if (hit1) return req1;
                else return req2;
            }

            if (req1.TsArrival <= req2.TsArrival) return req1;
            else return req2;
        }

        public override void Tick()
        {
            base.Tick();

            //quantum
            if (_quantumCyclesLeft > 0)
            {
                _quantumCyclesLeft--;
                return;
            }

            //new quantum
            decay_stats();
            rank_mem_intensity();
            rank_rbl();
            set_priority();

            _quantumCnt++;
            _quantumCyclesLeft = Config.sched.quantum_cycles;
            
        }

        private void set_priority()
        {
            for (int p = 0; p < Config.N; p++)
                _priority[p] = 0;

            int mem_intensity_standard = Math.Max((int)(0.1 * Config.N), 1);
            int rbl_standard = Math.Max((int)(0.1 * Config.N), 1);

            for (int p = 0; p < Config.N; p++)
            {
                if (_rank_mem_intensity[p] < mem_intensity_standard)
                    _priority[p] += 1;
                if (_rank_rbl[p] < rbl_standard)
                    _priority[p] += 2;
            }
        }

        private void rank_mem_intensity()
        {
            int[] tids = new int[Config.N];
            for (int p = 0; p < Config.N; p++)
                tids[p] = p;

            Array.Sort(tids, sort_mpki);
            for (int p = 0; p < Config.N; p++)
            {
                _rank_mem_intensity[p] = Array.IndexOf(tids, p);
            }
        }

        private void rank_rbl()
        {
            int[] tids = new int[Config.N];
            for (int p = 0; p < Config.N; p++)
                tids[p] = p;

            Array.Sort(tids, sort_rbl);
            for (int p = 0; p < Config.N; p++)
            {
                _rank_rbl[p] = Array.IndexOf(tids, p);
            }
        }

        private void decay_stats()
        {
            for (int p = 0; p < Config.N; p++)
            {
                ulong cacheMiss;
                cacheMiss = Config.sched.tcm_only_rmpki ? Sim.Sim.Procs[p].mem_rd_req_count : Sim.Sim.Procs[p].mem_req_count;

                ulong deltaCacheMiss = cacheMiss - _prevCacheMiss[p];
                _prevCacheMiss[p] = cacheMiss;

                ulong instCnt = Sim.Sim.Procs[p].inst_count;
                ulong deltaInstCnt = instCnt - _prevInstCnt[p];
                _prevInstCnt[p] = instCnt;
                
                //mpki
                double currMpki = 1000 * ((double)deltaCacheMiss) / deltaInstCnt;
                _mpki[p] = Config.sched.history_weight * _mpki[p] + (1 - Config.sched.history_weight) * currMpki;

                //rbl
                double currRbl = ((double)_shadowRowHits[p]) / deltaCacheMiss;
                _rbl[p] = Config.sched.history_weight * _rbl[p] + (1 - Config.sched.history_weight) * currRbl;
                _shadowRowHits[p] = 0;
            }
        }

        private int sort_mpki(int pid1, int pid2)
        {
            //return 1 if first argument is "greater" (higher rank)
            //modified return 1 if first argument is "smaller" (higher rank)
            if (pid1 == pid2) return 0;

            double mpki1 = _mpki[pid1];
            double mpki2 = _mpki[pid2];

            if (mpki1 > mpki2) return 1;
            else return -1;
        }

        private int sort_rbl(int pid1, int pid2)
        {
            //return 1 if first argument is "greater" (higher rank)
            if (pid1 == pid2) return 0;

            double rbl1 = _rbl[pid1];
            double rbl2 = _rbl[pid2];

            if (rbl1 < rbl2) return 1;
            else return -1;
        }

        public override void issue_req(Req req)
        {
            if (req == null) return;

            ulong shadowRowid = Mctrl.ShadowRowidPerProcrankbank[req.Pid, req.Addr.rid, req.Addr.bid];
            if (shadowRowid == req.Addr.rowid)
            {
                _shadowRowHits[req.Pid]++;
            }
        }
    }
}