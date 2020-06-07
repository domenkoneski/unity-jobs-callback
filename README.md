# unity-jobs-callback
Job Helper utillity that enables you to create callbacks for your completed jobs and safely disposes any Native Containers.

## How to use
1. Import _JobHelper.cs_ to your project.

2. Create your Job struct and implement _IJobDisposable_ interface and make sure to dispose any _NativeContainer_ in the _OnDispose_ method. Example of a simple job:
```csharp
public struct SimpleJob : IJob, IJobDisposable {
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
```
3. Create an instance of your _Job_ and schedule it with _JobHelper_.
```csharp
NativeArray<float> result = new NativeArray<float>(1, Allocator.Persistent);

SimpleJob job = new SimpleJob() {
    result = result
};

JobExecution execution = JobHelper.AddScheduledJob(job, job.Schedule(), (jobExecutor) => {
    Debug.LogFormat("Job has completed in {0}s and {1} frames!", jobExecutor.Duration, jobExecutor.FramesTaken);

    // Result is available. LateUpdate() context.
    Debug.Log(result[0]);
});
```
4. Done! If you want an extended use case check _JobHelperExamples.cs_ for more control.

