using Ramulator.MemReq;
using Ramulator.Sim;

namespace Ramulator.MemSched
{
    public class MLQSched : MemSched
    {
        //shuffle
        private int _shuffleCyclesLeft;

        private int[] _corePriority;

        public MLQSched(MemCtrl.MemCtrl mctrl, MemCtrl.MemCtrl[] mctrls)
            : base(mctrl, mctrls)
        {
            _shuffleCyclesLeft = Config.sched.mlqsched_shuffle_cycles;
            _corePriority = new int[Config.N];
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

        public override Req better_req(Req req1, Req req2)
        {
            if (_corePriority[req1.Pid] > _corePriority[req2.Pid])
                return req1;
            else if (_corePriority[req1.Pid] < _corePriority[req2.Pid])
                return req2;

            if (!is_row_hit(req1))
                _corePriority[req1.Pid] -= 1;
            if (!is_row_hit(req2))
                _corePriority[req2.Pid] -= 1;

            if (req1.TsArrival <= req2.TsArrival) return req1;
            else return req2;
        }

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
                _corePriority[p] = Config.sched.max_priority;
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