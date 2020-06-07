
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static JobHelper;

public class SimpleJobExample : MonoBehaviour {
    
    void ExtendedExample() {
        NativeArray<float> result = new NativeArray<float>(1, Allocator.Persistent);

        SimpleJob job = new SimpleJob() {
            result = result
        };

        JobExecution execution = JobHelper.AddScheduledJob(job, job.Schedule(), (jobExecutor) => {
            /// OnJobComplete delegate returns 'jobExecutor' with usefull data.
            if (jobExecutor.FramesTaken == -1) {
                Debug.Log("Job has completed immediatelly!");
            } else {
                Debug.LogFormat("Job has completed in {0}s and {1} frames!", jobExecutor.Duration, jobExecutor.FramesTaken);
            }

            // Result is available. LateUpdate() context.
            Debug.Log(result[0]);

            // You can dispose NativeContainers container within the jobexecution immediately after completion.
            // This is required if your set your container allocator as Temp or TempJob, dont Dispose if you use Persistent allocation.
            // Otherwise no need to call Dispose, the JobHelper will execute Dispose() on all completed jobs on OnDestroy()
            jobExecutor.Dispose();
        }, 
        // If you want to complete this job immediately in the LateUpdate() check this to true.
        // But make sure to schedule the job before, to leave some room for workers to do its work.
        completeImmediatelly: false);

        // There is also an option to complete the job whenever you want.
        Debug.Log(execution.Complete());
    }

    void SimpleExample() {
        NativeArray<float> result = new NativeArray<float>(1, Allocator.Persistent);

        SimpleJob job = new SimpleJob() {
            result = result
        };

        JobExecution execution = JobHelper.AddScheduledJob(job, job.Schedule(), (jobExecutor) => {

            Debug.LogFormat("Job has completed in {0}s and {1} frames!", jobExecutor.Duration, jobExecutor.FramesTaken);
      
            // Result is available. LateUpdate() context.
            Debug.Log(result[0]);
        });
    }

    public struct SimpleJob : IJob, IJobDisposable  {
        public NativeArray<float> result;

        public void Execute() {
            for (int i = 0; i < 1000000; i++) {
                var s = Mathf.Sin(10 * i);
            }
            result[0] = 5;
        }

        public void OnDispose() {
            result.Dispose();
        }
    }
}
