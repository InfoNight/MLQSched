using Ramulator.MemReq;
using Ramulator.Sim;

namespace Ramulator.MemSched
{
    public class MLQSched : MemSched
    {
        //shuffle
        private int _shuffleCyclesLeft;
        private int _max_priority;
        private int _min_priority;
        private int[] _corePriority;

        public MLQSched(MemCtrl.MemCtrl mctrl, MemCtrl.MemCtrl[] mctrls)
            : base(mctrl, mctrls)
        {
            _shuffleCyclesLeft = Config.sched.mlqsched_shuffle_cycles;
            _corePriority = new int[Config.N];
            _max_priority = Config.N;
            _min_priority = 0;
            clear_priority();
        }

        public override void Initialize()
        {
        }

        public override void enqueue_req(Req req)
        {
        }

        public override void dequeue_req(Req req)
        {
        }

        private void adjust_priority(Req req, bool hit)
        {
            int curr_priority = _corePriority[req.Pid];
            if (hit)
                _corePriority[req.Pid] = (curr_priority == _max_priority) ? _max_priority : (curr_priority + 1);
            else
                _corePriority[req.Pid] = (curr_priority == _min_priority) ? _min_priority : (curr_priority - 1);
        }

        // version 3
        public override Req better_req(Req req1, Req req2)
        {

            if (_corePriority[req1.Pid] > _corePriority[req2.Pid])
                return req1;
            else if (_corePriority[req1.Pid] < _corePriority[req2.Pid])
                return req2;

            bool hit1 = is_row_hit(req1);
            bool hit2 = is_row_hit(req2);

            adjust_priority(req1, hit1);
            adjust_priority(req2, hit2);

            if (hit1 ^ hit2)
            {
                if (hit1) return req1;
                else return req2;
            }
            if (req1.TsArrival <= req2.TsArrival) return req1;
            else return req2;
        }

        // version 2
        // public override Req better_req(Req req1, Req req2)
        // {
        //     bool hit1 = is_row_hit(req1);
        //     bool hit2 = is_row_hit(req2);

        //     adjust_priority(req1, hit1);
        //     adjust_priority(req2, hit2);
            
        //     if (hit1 ^ hit2)
        //     {
        //         if (hit1) return req1;
        //         else return req2;
        //     }

        //     if (_corePriority[req1.Pid] > _corePriority[req2.Pid])
        //         return req1;
        //     else if (_corePriority[req1.Pid] < _corePriority[req2.Pid])
        //         return req2;

        //     if (req1.TsArrival <= req2.TsArrival) return req1;
        //     else return req2;
        // }

        // version 1
        // public override Req better_req(Req req1, Req req2)
        // {
        //     bool hit1 = is_row_hit(req1);
        //     bool hit2 = is_row_hit(req2);

        //     adjust_priority(req1, hit1);
        //     adjust_priority(req2, hit2);

        //     if (_corePriority[req1.Pid] > _corePriority[req2.Pid])
        //         return req1;
        //     else if (_corePriority[req1.Pid] < _corePriority[req2.Pid])
        //         return req2;


        //     if (hit1 ^ hit2)
        //     {
        //         if (hit1) return req1;
        //         else return req2;
        //     }
        //     if (req1.TsArrival <= req2.TsArrival) return req1;
        //     else return req2;
        // }

        public override void Tick()
        {
            base.Tick();

            //shuffle
            if (_shuffleCyclesLeft > 0)
            {
                _shuffleCyclesLeft--;
            }
            else
            {
                _shuffleCyclesLeft = Config.sched.mlqsched_shuffle_cycles;
                clear_priority();
            }
        }

        public void clear_priority()
        {
            for (int p = 0; p < Config.N; p++)
                _corePriority[p] = _max_priority;
        }

        public override void issue_req(Req req)
        {
            // if (req == null) return;

            // // Channel-level bliss
            // {
            //     if (req.Pid == _lastReqPid &&
            //         _oldestStreakGlobal < Config.sched.bliss_row_hit_cap)
            //     {
            //         _oldestStreakGlobal += 1;
            //     }
            //     else if (req.Pid == _lastReqPid &&
            //              _oldestStreakGlobal == Config.sched.bliss_row_hit_cap)
            //     {
            //         _mark[req.Pid] = 1;
            //         _oldestStreakGlobal = 1;
            //     }
            //     else
            //     {
            //         _oldestStreakGlobal = 1;
            //     }
            //     _lastReqPid = req.Pid;
            // }
        }
    }
}